namespace Cqrs.OrderService.Application.Abstractions.Caching;

public interface ICacheVersionService
{
    Task<long> GetVersionAsync(string scope, CancellationToken cancellationToken);

    Task IncrementVersionAsync(string scope, CancellationToken cancellationToken);
}
