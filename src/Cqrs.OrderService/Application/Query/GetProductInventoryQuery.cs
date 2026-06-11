using Cqrs.OrderService.Application.Abstractions.Messaging;

namespace Cqrs.OrderService.Application.Query;

public sealed record GetProductInventoryQuery(
    string ProductId,
    IReadOnlySet<string> Fields) : IQuery<IReadOnlyList<ProductStockItem>>
{
    public static GetProductInventoryQuery All(string productId) => new(productId, new HashSet<string>());
}
