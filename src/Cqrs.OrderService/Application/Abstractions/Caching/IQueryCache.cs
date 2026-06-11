namespace Cqrs.OrderService.Application.Abstractions.Caching;

public interface IQueryCache
{
    Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan ttl,
        CancellationToken cancellationToken);
}
