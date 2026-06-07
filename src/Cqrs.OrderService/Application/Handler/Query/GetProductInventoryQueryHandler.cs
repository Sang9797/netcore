using Cqrs.OrderService.Application.Query;
using Cqrs.OrderService.Bus.Query;
using Cqrs.OrderService.Infrastructure.Persistence;

namespace Cqrs.OrderService.Application.Handler.Query;

public sealed class GetProductInventoryQueryHandler(IInventoryReadRepository repository)
    : IQueryHandler<GetProductInventoryQuery, IReadOnlyList<ProductStockItem>>
{
    public Task<IReadOnlyList<ProductStockItem>> Handle(GetProductInventoryQuery query, CancellationToken cancellationToken) =>
        repository.FindProductStock(query, cancellationToken);
}
