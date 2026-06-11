namespace Cqrs.OrderService.Infrastructure.Integration;

public enum RabbitMqConsumerResult
{
    Success,
    PermanentFailure,
    TransientFailure
}
