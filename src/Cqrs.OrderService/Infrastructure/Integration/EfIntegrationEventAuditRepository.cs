using Cqrs.OrderService.Application.Abstractions.Data;
using Cqrs.OrderService.Infrastructure.Persistence;

namespace Cqrs.OrderService.Infrastructure.Integration;

public sealed class EfIntegrationEventAuditRepository(OrdersDbContext dbContext) : IIntegrationEventAuditRepository
{
    public async Task SaveAsync(IntegrationEventAuditRecord record, CancellationToken cancellationToken)
    {
        dbContext.IntegrationEventAudits.Add(new IntegrationEventAuditEntity
        {
            AuditId = record.AuditId,
            MessageId = record.MessageId,
            EventType = record.EventType,
            RoutingKey = record.RoutingKey,
            AggregateId = record.AggregateId,
            Description = record.Description,
            OccurredAt = record.OccurredAt,
            ProcessedAt = record.ProcessedAt
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
