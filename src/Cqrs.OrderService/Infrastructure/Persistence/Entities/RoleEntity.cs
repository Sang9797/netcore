namespace Cqrs.OrderService.Infrastructure.Persistence.Entities;

public sealed class RoleEntity
{
    public string RoleId { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public List<PermissionEntity> Permissions { get; set; } = [];
    public List<UserEntity> Users { get; set; } = [];
}
