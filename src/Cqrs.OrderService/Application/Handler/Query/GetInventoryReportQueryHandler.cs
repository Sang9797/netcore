using Cqrs.OrderService.Application.Query;
using Cqrs.OrderService.Bus.Query;
using Cqrs.OrderService.Infrastructure.Persistence;

namespace Cqrs.OrderService.Application.Handler.Query;

public sealed class GetInventoryReportQueryHandler(IInventoryReadRepository repository)
    : IQueryHandler<GetInventoryReportQuery, IReadOnlyList<InventoryReportItem>>
{
    public Task<IReadOnlyList<InventoryReportItem>> Handle(GetInventoryReportQuery query, CancellationToken cancellationToken) =>
        repository.FindInventoryReport(query, cancellationToken);
}
