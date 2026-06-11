using Cqrs.OrderService.Application.Abstractions.Caching;

namespace Cqrs.OrderService.Infrastructure.Caching;

public sealed class NoOpCacheVersionService : ICacheVersionService
{
    public Task<long> GetVersionAsync(string scope, CancellationToken cancellationToken) => Task.FromResult(0L);

    public Task IncrementVersionAsync(string scope, CancellationToken cancellationToken) => Task.CompletedTask;
}
