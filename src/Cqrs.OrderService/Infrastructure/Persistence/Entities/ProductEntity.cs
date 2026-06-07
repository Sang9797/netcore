namespace Cqrs.OrderService.Infrastructure.Persistence.Entities;

public sealed class ProductEntity
{
    public string ProductId { get; set; } = "";
    public string Sku { get; set; } = "";
    public string Name { get; set; } = "";
    public string CategoryId { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public string Currency { get; set; } = "";
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public ProductCategoryEntity? Category { get; set; }
}
