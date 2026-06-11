using Cqrs.OrderService.Application.Abstractions.Caching;
using Cqrs.OrderService.Application.Query;
using Cqrs.OrderService.Application.Abstractions.Data;
using Cqrs.OrderService.Application.Common.Caching;
using FluentResults;
using MediatR;

namespace Cqrs.OrderService.Application.Handler.Query;

public sealed class GetInventoryReportQueryHandler(IInventoryReadRepository repository, IQueryCache cache, ICacheVersionService cacheVersionService)
    : IRequestHandler<GetInventoryReportQuery, Result<IReadOnlyList<InventoryReportItem>>>
{
    public async Task<Result<IReadOnlyList<InventoryReportItem>>> Handle(GetInventoryReportQuery query, CancellationToken cancellationToken)
    {
        var version = await cacheVersionService.GetVersionAsync(CacheKeys.InventoryScope, cancellationToken);
        var key = CacheKeys.InventoryReport(query.CategoryId, query.WarehouseId, query.MinStock, query.Page, query.PageSize, version);
        var result = await cache.GetOrCreateAsync(
            key,
            token => repository.FindInventoryReport(query, token),
            TimeSpan.FromMinutes(5),
            cancellationToken);
        return Result.Ok(result);
    }
}
