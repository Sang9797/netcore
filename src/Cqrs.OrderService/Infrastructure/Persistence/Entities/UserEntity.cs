namespace Cqrs.OrderService.Infrastructure.Persistence.Entities;

public sealed class UserEntity
{
    public string UserId { get; set; } = "";
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string? Email { get; set; }
    public bool Enabled { get; set; }
    public List<RoleEntity> Roles { get; set; } = [];
}
