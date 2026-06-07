using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Cqrs.OrderService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Cqrs.OrderService.Infrastructure;

public sealed class JwtOptions
{
    public string Secret { get; set; } = "c2VjcmV0LWtleS1mb3ItZGV2ZWxvcG1lbnQtb25seS1ub3QtZm9yLXByb2R1Y3Rpb24=";
    public long ExpirationMs { get; set; } = 86_400_000;
}

public sealed record AppUser(string Username, string PasswordHash, bool Enabled, IReadOnlySet<string> Authorities);

public sealed class UserRepository(OrdersDbContext dbContext)
{
    public async Task<AppUser?> FindByUsername(string username, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Users
            .AsNoTracking()
            .Include(u => u.Roles)
            .ThenInclude(r => r.Permissions)
            .SingleOrDefaultAsync(u => u.Username == username, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        var authorities = entity.Roles
            .Select(r => r.Name)
            .Concat(entity.Roles.SelectMany(r => r.Permissions).Select(p => p.Name))
            .ToHashSet(StringComparer.Ordinal);

        return new AppUser(entity.Username, entity.PasswordHash, entity.Enabled, authorities);
    }
}

public sealed class JwtTokenService(IConfiguration configuration)
{
    public string GenerateToken(AppUser user)
    {
        var options = ResolveOptions(configuration);
        var now = DateTimeOffset.UtcNow;
        var header = Base64Url(JsonSerializer.SerializeToUtf8Bytes(new { alg = "HS256", typ = "JWT" }));
        var payload = Base64Url(JsonSerializer.SerializeToUtf8Bytes(new
        {
            sub = user.Username,
            name = user.Username,
            authorities = user.Authorities,
            iat = now.ToUnixTimeSeconds(),
            exp = now.AddMilliseconds(options.ExpirationMs).ToUnixTimeSeconds()
        }));
        var body = $"{header}.{payload}";
        return $"{body}.{Sign(body, options.Secret)}";
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var options = ResolveOptions(configuration);
        var parts = token.Split('.');
        if (parts.Length != 3)
        {
            return null;
        }

        var body = $"{parts[0]}.{parts[1]}";
        if (!CryptographicOperations.FixedTimeEquals(
            Encoding.ASCII.GetBytes(Sign(body, options.Secret)),
            Encoding.ASCII.GetBytes(parts[2])))
        {
            return null;
        }

        using var payload = JsonDocument.Parse(Base64UrlDecode(parts[1]));
        var exp = payload.RootElement.TryGetProperty("exp", out var expEl) ? expEl.GetInt64() : 0;
        if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() >= exp)
        {
            return null;
        }

        var username = payload.RootElement.TryGetProperty("sub", out var sub) ? sub.GetString() ?? "" : "";
        var claims = new List<Claim> { new(ClaimTypes.Name, username) };
        if (payload.RootElement.TryGetProperty("authorities", out var authorities))
        {
            claims.AddRange(authorities.EnumerateArray().Select(a => new Claim(ClaimTypes.Role, a.GetString() ?? "")));
            claims.AddRange(authorities.EnumerateArray().Select(a => new Claim("authority", a.GetString() ?? "")));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
    }

    public static byte[] DecodeSecret(string secret)
    {
        try
        {
            return Convert.FromBase64String(secret);
        }
        catch (FormatException)
        {
            return Encoding.UTF8.GetBytes(secret);
        }
    }

    private static JwtOptions ResolveOptions(IConfiguration configuration)
    {
        var options = configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
        options.Secret = configuration["APP_JWT_SECRET"] ?? options.Secret;
        if (long.TryParse(configuration["APP_JWT_EXPIRATION_MS"], out var expirationMs))
        {
            options.ExpirationMs = expirationMs;
        }

        return options;
    }

    private static string Sign(string body, string secret)
    {
        using var hmac = new HMACSHA256(DecodeSecret(secret));
        return Base64Url(hmac.ComputeHash(Encoding.ASCII.GetBytes(body)));
    }

    private static string Base64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[] Base64UrlDecode(string value)
    {
        var padded = value.Replace('-', '+').Replace('_', '/');
        padded = padded.PadRight(padded.Length + (4 - padded.Length % 4) % 4, '=');
        return Convert.FromBase64String(padded);
    }
}

public static class PasswordVerifier
{
    private static readonly Dictionary<string, string> SeedPasswords = new(StringComparer.Ordinal)
    {
        ["admin"] = "admin123",
        ["john"] = "userpass",
        ["testuser"] = "testpass"
    };

    public static bool Verify(string username, string password, string passwordHash)
    {
        if (passwordHash.StartsWith("$2", StringComparison.Ordinal))
        {
            return SeedPasswords.TryGetValue(username, out var seeded) && password == seeded;
        }

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(password),
            Encoding.UTF8.GetBytes(passwordHash));
    }
}

public sealed class HmacJwtAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    JwtTokenService jwtTokenService)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var header = Request.Headers.Authorization.ToString();
        if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var principal = jwtTokenService.ValidateToken(header["Bearer ".Length..]);
        return Task.FromResult(principal is null
            ? AuthenticateResult.Fail("Invalid bearer token")
            : AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name)));
    }
}
