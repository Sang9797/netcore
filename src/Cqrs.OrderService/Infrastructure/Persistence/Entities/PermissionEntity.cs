namespace Cqrs.OrderService.Infrastructure.Persistence.Entities;

public sealed class PermissionEntity
{
    public string PermissionId { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public List<RoleEntity> Roles { get; set; } = [];
}
