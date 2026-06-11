using Cqrs.OrderService.Application.Abstractions.Caching;
using Cqrs.OrderService.Application.Query;
using Cqrs.OrderService.Application.Abstractions.Data;
using Cqrs.OrderService.Application.Common.Caching;
using Cqrs.OrderService.Application.Common.Errors;
using Cqrs.OrderService.Domain.Model;
using FluentResults;
using MediatR;

namespace Cqrs.OrderService.Application.Handler.Query;

public sealed class GetOrderByIdQueryHandler(IOrderRepository repository, IQueryCache cache, ICacheVersionService cacheVersionService)
    : IRequestHandler<GetOrderByIdQuery, Result<Order>>
{
    public async Task<Result<Order>> Handle(GetOrderByIdQuery query, CancellationToken cancellationToken)
    {
        var version = await cacheVersionService.GetVersionAsync(CacheKeys.OrderScope(query.OrderId), cancellationToken);
        var key = CacheKeys.OrderById(query.OrderId, version);
        var order = await cache.GetOrCreateAsync(
            key,
            token => repository.FindById(query.OrderId, token),
            TimeSpan.FromMinutes(5),
            cancellationToken);
        return order is null
            ? Result.Fail<Order>(ApplicationErrors.NotFound("ORDER_NOT_FOUND", $"Order '{query.OrderId}' was not found"))
            : Result.Ok(order);
    }
}
