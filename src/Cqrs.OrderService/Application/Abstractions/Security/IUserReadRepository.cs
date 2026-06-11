namespace Cqrs.OrderService.Application.Abstractions.Security;

public sealed record AppUser(string Username, string PasswordHash, bool Enabled, IReadOnlySet<string> Authorities);

public interface IUserReadRepository
{
    Task<AppUser?> FindByUsername(string username, CancellationToken cancellationToken);
}
