using Cqrs.OrderService.Application.Query;
using Cqrs.OrderService.Bus.Query;
using Cqrs.OrderService.Infrastructure.Persistence;

namespace Cqrs.OrderService.Application.Handler.Query;

public sealed class ListLowStockQueryHandler(IInventoryReadRepository repository)
    : IQueryHandler<ListLowStockQuery, IReadOnlyList<LowStockItem>>
{
    public Task<IReadOnlyList<LowStockItem>> Handle(ListLowStockQuery query, CancellationToken cancellationToken) =>
        repository.FindLowStock(query, cancellationToken);
}
