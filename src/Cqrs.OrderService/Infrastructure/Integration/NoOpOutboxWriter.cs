using Cqrs.OrderService.Application.Abstractions.Messaging;
using Cqrs.OrderService.Application.IntegrationEvents;

namespace Cqrs.OrderService.Infrastructure.Integration;

public sealed class NoOpOutboxWriter : IOutboxWriter
{
    public Task EnqueueAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken) => Task.CompletedTask;
}
