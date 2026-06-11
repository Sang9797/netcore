using Cqrs.OrderService.Application.Abstractions.Caching;

namespace Cqrs.OrderService.Infrastructure.Caching;

public sealed class NoOpQueryCache : IQueryCache
{
    public Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan ttl,
        CancellationToken cancellationToken) =>
        factory(cancellationToken);
}
