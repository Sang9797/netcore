using RabbitMQ.Client;

namespace Cqrs.OrderService.Infrastructure.Integration;

public sealed class RabbitMqConnectionFactory(RabbitMqOptions options)
{
    public IConnection CreateConnection()
    {
        var factory = new ConnectionFactory
        {
            HostName = options.HostName,
            Port = options.Port,
            UserName = options.UserName,
            Password = options.Password,
            VirtualHost = options.VirtualHost,
            DispatchConsumersAsync = true,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };

        return factory.CreateConnection();
    }
}
