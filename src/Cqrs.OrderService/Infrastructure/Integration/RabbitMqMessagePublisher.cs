using System.Text;
using RabbitMQ.Client;

namespace Cqrs.OrderService.Infrastructure.Integration;

public sealed class RabbitMqMessagePublisher : IDisposable
{
    private readonly object _sync = new();
    private readonly RabbitMqOptions _options;
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMqMessagePublisher(RabbitMqOptions options)
    {
        _options = options;
    }

    public Task PublishAsync(string messageId, string eventType, string routingKey, string payload, CancellationToken cancellationToken)
    {
        EnsureConnected();

        var properties = _channel!.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";
        properties.MessageId = messageId;
        properties.Type = eventType;
        properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        var body = Encoding.UTF8.GetBytes(payload);
        _channel.BasicPublish(_options.ExchangeName, routingKey, properties, body);
        return Task.CompletedTask;
    }

    private void EnsureConnected()
    {
        if (_connection is { IsOpen: true } && _channel is { IsOpen: true })
        {
            return;
        }

        lock (_sync)
        {
            if (_connection is { IsOpen: true } && _channel is { IsOpen: true })
            {
                return;
            }

            DisposeCore();

            var factory = new RabbitMqConnectionFactory(_options);
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(_options.ExchangeName, ExchangeType.Topic, durable: true, autoDelete: false);
            _channel.QueueDeclare(_options.QueueName, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(_options.QueueName, _options.ExchangeName, "#");
        }
    }

    public void Dispose()
    {
        DisposeCore();
        GC.SuppressFinalize(this);
    }

    private void DisposeCore()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        _channel = null;
        _connection = null;
    }
}
