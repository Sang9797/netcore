using Cqrs.OrderService.Application.Abstractions.Caching;
using Cqrs.OrderService.Application.Query;
using Cqrs.OrderService.Application.Abstractions.Data;
using Cqrs.OrderService.Application.Common.Caching;
using Cqrs.OrderService.Domain.Model;
using FluentResults;
using MediatR;

namespace Cqrs.OrderService.Application.Handler.Query;

public sealed class ListOrdersByCustomerQueryHandler(IOrderRepository repository, IQueryCache cache, ICacheVersionService cacheVersionService)
    : IRequestHandler<ListOrdersByCustomerQuery, Result<IReadOnlyList<Order>>>
{
    public async Task<Result<IReadOnlyList<Order>>> Handle(ListOrdersByCustomerQuery query, CancellationToken cancellationToken)
    {
        var version = await cacheVersionService.GetVersionAsync(CacheKeys.CustomerOrdersScope(query.CustomerId), cancellationToken);
        var key = CacheKeys.OrdersByCustomer(query.CustomerId, version);
        var result = await cache.GetOrCreateAsync(
            key,
            token => repository.FindByCustomerId(query.CustomerId, token),
            TimeSpan.FromMinutes(5),
            cancellationToken);
        return Result.Ok(result);
    }
}
