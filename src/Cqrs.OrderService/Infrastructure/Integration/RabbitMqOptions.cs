namespace Cqrs.OrderService.Infrastructure.Integration;

public sealed class RabbitMqOptions
{
    public bool Enabled { get; set; }
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string ExchangeName { get; set; } = "cqrs.order.events";
    public string QueueName { get; set; } = "cqrs.order.events.audit";
    public bool ConsumerEnabled { get; set; } = true;
    public int PollIntervalSeconds { get; set; } = 5;
    public int BatchSize { get; set; } = 50;
}
