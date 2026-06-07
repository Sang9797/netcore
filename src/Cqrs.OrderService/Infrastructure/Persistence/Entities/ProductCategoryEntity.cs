namespace Cqrs.OrderService.Infrastructure.Persistence.Entities;

public sealed class ProductCategoryEntity
{
    public string CategoryId { get; set; } = "";
    public string Name { get; set; } = "";
    public string? ParentCategoryId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public ProductCategoryEntity? Parent { get; set; }
}
