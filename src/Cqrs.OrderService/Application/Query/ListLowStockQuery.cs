using Cqrs.OrderService.Application.Abstractions.Messaging;

namespace Cqrs.OrderService.Application.Query;

public sealed record ListLowStockQuery(
    int Threshold,
    int Limit,
    IReadOnlySet<string> Fields) : IQuery<IReadOnlyList<LowStockItem>>
{
    public static ListLowStockQuery All(int threshold, int limit) => new(threshold, limit, new HashSet<string>());
}
