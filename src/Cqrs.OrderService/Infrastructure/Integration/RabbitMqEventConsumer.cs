using System.Text;
using System.Text.Json;
using Cqrs.OrderService.Application.Abstractions.Messaging;
using Cqrs.OrderService.Application.Abstractions.Data;
using Cqrs.OrderService.Application.IntegrationEvents;
using Cqrs.OrderService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Cqrs.OrderService.Infrastructure.Integration;

public sealed class RabbitMqEventConsumer(
    RabbitMqConnectionFactory connectionFactory,
    RabbitMqOptions options,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<RabbitMqEventConsumer> logger)
    : BackgroundService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private IConnection? _connection;
    private IModel? _channel;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _connection = connectionFactory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(options.ExchangeName, ExchangeType.Topic, durable: true, autoDelete: false);
        _channel.QueueDeclare(options.QueueName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(options.QueueName, options.ExchangeName, "#");
        _channel.BasicQos(0, 10, false);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += OnReceivedAsync;
        _channel.BasicConsume(options.QueueName, autoAck: false, consumer);
        return Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task OnReceivedAsync(object sender, BasicDeliverEventArgs eventArgs)
    {
        var result = await ProcessMessage(eventArgs, CancellationToken.None);

        switch (result)
        {
            case RabbitMqConsumerResult.Success:
            case RabbitMqConsumerResult.PermanentFailure:
                _channel?.BasicAck(eventArgs.DeliveryTag, multiple: false);
                break;
            default:
                _channel?.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: true);
                break;
        }
    }

    private async Task<RabbitMqConsumerResult> ProcessMessage(BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
    {
        var messageId = eventArgs.BasicProperties.MessageId;
        var eventType = eventArgs.BasicProperties.Type;
        var routingKey = eventArgs.RoutingKey;

        if (string.IsNullOrWhiteSpace(messageId) || string.IsNullOrWhiteSpace(eventType))
        {
            logger.LogWarning("Dropping RabbitMQ message with missing metadata. RoutingKey={RoutingKey}", routingKey);
            return RabbitMqConsumerResult.PermanentFailure;
        }

        try
        {
            using var scope = serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();

            if (await dbContext.InboxMessages.AnyAsync(x => x.InboxMessageId == messageId, cancellationToken))
            {
                return RabbitMqConsumerResult.Success;
            }

            var payload = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
            var processed = eventType switch
            {
                nameof(OrderPlacedIntegrationEvent) =>
                    await Dispatch<OrderPlacedIntegrationEvent>(scope.ServiceProvider, payload,
                        static (sp, evt, token) => sp.GetRequiredService<IIntegrationEventHandler<OrderPlacedIntegrationEvent>>().HandleAsync(evt, token),
                        cancellationToken),
                nameof(OrderConfirmedIntegrationEvent) =>
                    await Dispatch<OrderConfirmedIntegrationEvent>(scope.ServiceProvider, payload,
                        static (sp, evt, token) => sp.GetRequiredService<IIntegrationEventHandler<OrderConfirmedIntegrationEvent>>().HandleAsync(evt, token),
                        cancellationToken),
                nameof(OrderCancelledIntegrationEvent) =>
                    await Dispatch<OrderCancelledIntegrationEvent>(scope.ServiceProvider, payload,
                        static (sp, evt, token) => sp.GetRequiredService<IIntegrationEventHandler<OrderCancelledIntegrationEvent>>().HandleAsync(evt, token),
                        cancellationToken),
                nameof(InventoryReservedIntegrationEvent) =>
                    await Dispatch<InventoryReservedIntegrationEvent>(scope.ServiceProvider, payload,
                        static (sp, evt, token) => sp.GetRequiredService<IIntegrationEventHandler<InventoryReservedIntegrationEvent>>().HandleAsync(evt, token),
                        cancellationToken),
                nameof(InventoryReleasedIntegrationEvent) =>
                    await Dispatch<InventoryReleasedIntegrationEvent>(scope.ServiceProvider, payload,
                        static (sp, evt, token) => sp.GetRequiredService<IIntegrationEventHandler<InventoryReleasedIntegrationEvent>>().HandleAsync(evt, token),
                        cancellationToken),
                nameof(InventoryAdjustedIntegrationEvent) =>
                    await Dispatch<InventoryAdjustedIntegrationEvent>(scope.ServiceProvider, payload,
                        static (sp, evt, token) => sp.GetRequiredService<IIntegrationEventHandler<InventoryAdjustedIntegrationEvent>>().HandleAsync(evt, token),
                        cancellationToken),
                _ => false
            };

            if (!processed)
            {
                logger.LogWarning("Dropping unsupported RabbitMQ event type {EventType}", eventType);
                return RabbitMqConsumerResult.PermanentFailure;
            }

            dbContext.InboxMessages.Add(new InboxMessageEntity
            {
                InboxMessageId = messageId,
                EventType = eventType,
                RoutingKey = routingKey,
                ReceivedAt = DateTimeOffset.UtcNow,
                ProcessedAt = DateTimeOffset.UtcNow
            });
            await dbContext.SaveChangesAsync(cancellationToken);

            return RabbitMqConsumerResult.Success;
        }
        catch (JsonException exception)
        {
            logger.LogWarning(exception, "Dropping invalid RabbitMQ payload for event type {EventType}", eventType);
            return RabbitMqConsumerResult.PermanentFailure;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Transient RabbitMQ consumer failure for event type {EventType}", eventType);
            return RabbitMqConsumerResult.TransientFailure;
        }
    }

    private static async Task<bool> Dispatch<TEvent>(
        IServiceProvider serviceProvider,
        string payload,
        Func<IServiceProvider, TEvent, CancellationToken, Task> dispatch,
        CancellationToken cancellationToken)
        where TEvent : IntegrationEvent
    {
        var integrationEvent = JsonSerializer.Deserialize<TEvent>(payload, SerializerOptions);
        if (integrationEvent is null)
        {
            return false;
        }

        await dispatch(serviceProvider, integrationEvent, cancellationToken);
        return true;
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
