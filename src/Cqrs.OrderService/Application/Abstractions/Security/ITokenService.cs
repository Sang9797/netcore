namespace Cqrs.OrderService.Application.Abstractions.Security;

public interface ITokenService
{
    string GenerateToken(AppUser user);
}

public interface IPasswordVerifier
{
    bool Verify(string username, string password, string passwordHash);
}
