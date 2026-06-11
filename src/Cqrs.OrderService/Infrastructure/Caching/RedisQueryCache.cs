using System.Text.Json;
using Cqrs.OrderService.Application.Abstractions.Caching;
using StackExchange.Redis;

namespace Cqrs.OrderService.Infrastructure.Caching;

public sealed class RedisQueryCache(IConnectionMultiplexer connectionMultiplexer, RedisOptions options)
    : IQueryCache
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly IDatabase _database = connectionMultiplexer.GetDatabase();

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan ttl,
        CancellationToken cancellationToken)
    {
        if (ttl <= TimeSpan.Zero)
        {
            ttl = TimeSpan.FromSeconds(options.DefaultTtlSeconds);
        }

        var cacheKey = ToKey(key);
        var cached = await _database.StringGetAsync(cacheKey);
        if (cached.HasValue)
        {
            var restored = JsonSerializer.Deserialize<T>(cached.ToString(), SerializerOptions);
            if (restored is not null)
            {
                return restored;
            }
        }

        var created = await factory(cancellationToken);
        await _database.StringSetAsync(cacheKey, JsonSerializer.Serialize(created, SerializerOptions), ttl);
        return created;
    }

    private string ToKey(string key) => $"{options.InstanceName}{key}";
}
