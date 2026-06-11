namespace Cqrs.OrderService.Application.Abstractions.Data;

public interface IIntegrationEventAuditRepository
{
    Task SaveAsync(IntegrationEventAuditRecord record, CancellationToken cancellationToken);
}

public sealed record IntegrationEventAuditRecord(
    string AuditId,
    string MessageId,
    string EventType,
    string RoutingKey,
    string AggregateId,
    string Description,
    DateTimeOffset OccurredAt,
    DateTimeOffset ProcessedAt);
