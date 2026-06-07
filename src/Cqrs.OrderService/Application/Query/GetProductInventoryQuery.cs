using Cqrs.OrderService.Bus.Query;

namespace Cqrs.OrderService.Application.Query;

public sealed record GetProductInventoryQuery(
    string ProductId,
    IReadOnlySet<string> Fields) : IQuery<IReadOnlyList<ProductStockItem>>
{
    public static GetProductInventoryQuery All(string productId) => new(productId, new HashSet<string>());
}
