using Cqrs.OrderService.Application.Abstractions.Caching;
using Cqrs.OrderService.Application.Query;
using Cqrs.OrderService.Application.Abstractions.Data;
using Cqrs.OrderService.Application.Common.Caching;
using FluentResults;
using MediatR;

namespace Cqrs.OrderService.Application.Handler.Query;

public sealed class ListLowStockQueryHandler(IInventoryReadRepository repository, IQueryCache cache, ICacheVersionService cacheVersionService)
    : IRequestHandler<ListLowStockQuery, Result<IReadOnlyList<LowStockItem>>>
{
    public async Task<Result<IReadOnlyList<LowStockItem>>> Handle(ListLowStockQuery query, CancellationToken cancellationToken)
    {
        var version = await cacheVersionService.GetVersionAsync(CacheKeys.InventoryScope, cancellationToken);
        var key = CacheKeys.LowStock(query.Threshold, query.Limit, version);
        var result = await cache.GetOrCreateAsync(
            key,
            token => repository.FindLowStock(query, token),
            TimeSpan.FromMinutes(2),
            cancellationToken);
        return Result.Ok(result);
    }
}
