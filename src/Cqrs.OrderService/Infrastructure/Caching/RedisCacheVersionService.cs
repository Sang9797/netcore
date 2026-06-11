using System.Globalization;
using Cqrs.OrderService.Application.Abstractions.Caching;
using StackExchange.Redis;

namespace Cqrs.OrderService.Infrastructure.Caching;

public sealed class RedisCacheVersionService(IConnectionMultiplexer connectionMultiplexer, RedisOptions options)
    : ICacheVersionService
{
    private readonly IDatabase _database = connectionMultiplexer.GetDatabase();

    public async Task<long> GetVersionAsync(string scope, CancellationToken cancellationToken)
    {
        var value = await _database.StringGetAsync(ToKey(scope));
        return value.HasValue && long.TryParse(value.ToString(), NumberStyles.None, CultureInfo.InvariantCulture, out var version)
            ? version
            : 0L;
    }

    public async Task IncrementVersionAsync(string scope, CancellationToken cancellationToken)
    {
        await _database.StringIncrementAsync(ToKey(scope));
    }

    private string ToKey(string scope) => $"{options.InstanceName}versions:{scope}";
}
