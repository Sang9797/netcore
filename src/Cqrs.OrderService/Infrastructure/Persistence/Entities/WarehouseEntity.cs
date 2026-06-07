namespace Cqrs.OrderService.Infrastructure.Persistence.Entities;

public sealed class WarehouseEntity
{
    public string WarehouseId { get; set; } = "";
    public string Name { get; set; } = "";
    public string LocationCode { get; set; } = "";
    public string Region { get; set; } = "";
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
