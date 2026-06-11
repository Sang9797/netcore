using Cqrs.OrderService.Application.Abstractions.Caching;
using Cqrs.OrderService.Application.Query;
using Cqrs.OrderService.Application.Abstractions.Data;
using Cqrs.OrderService.Application.Common.Caching;
using FluentResults;
using MediatR;

namespace Cqrs.OrderService.Application.Handler.Query;

public sealed class GetProductInventoryQueryHandler(IInventoryReadRepository repository, IQueryCache cache, ICacheVersionService cacheVersionService)
    : IRequestHandler<GetProductInventoryQuery, Result<IReadOnlyList<ProductStockItem>>>
{
    public async Task<Result<IReadOnlyList<ProductStockItem>>> Handle(GetProductInventoryQuery query, CancellationToken cancellationToken)
    {
        var version = await cacheVersionService.GetVersionAsync(CacheKeys.InventoryScope, cancellationToken);
        var key = CacheKeys.ProductInventory(query.ProductId, version);
        var result = await cache.GetOrCreateAsync(
            key,
            token => repository.FindProductStock(query, token),
            TimeSpan.FromMinutes(5),
            cancellationToken);
        return Result.Ok(result);
    }
}
