namespace Cqrs.OrderService.Infrastructure.Caching;

public sealed class RedisOptions
{
    public bool Enabled { get; set; }
    public string ConnectionString { get; set; } = "localhost:6379";
    public string InstanceName { get; set; } = "cqrs-order-service:";
    public int DefaultTtlSeconds { get; set; } = 300;
}
