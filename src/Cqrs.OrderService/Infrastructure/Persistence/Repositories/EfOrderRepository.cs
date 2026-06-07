using Cqrs.OrderService.Domain.Model;
using Cqrs.OrderService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cqrs.OrderService.Infrastructure.Persistence.Repositories;

public sealed class EfOrderRepository(OrdersDbContext dbContext) : IOrderRepository
{
    public async Task<Order> Save(Order order, CancellationToken cancellationToken)
    {
        var existing = await dbContext.Orders
            .Include(o => o.Items)
            .SingleOrDefaultAsync(o => o.OrderId == order.OrderId, cancellationToken);

        if (existing is null)
        {
            dbContext.Orders.Add(ToEntity(order));
        }
        else
        {
            existing.Status = order.Status.ToString();
            existing.TotalAmount = order.TotalAmount.Amount;
            existing.Currency = order.TotalAmount.Currency;
            existing.UpdatedAt = order.UpdatedAt;
            dbContext.OrderItems.RemoveRange(existing.Items);
            existing.Items = order.Items.Select(i => ToEntity(i, order.OrderId)).ToList();
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return order;
    }

    public async Task<Order?> FindById(string orderId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .SingleOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);
        return entity is null ? null : ToDomain(entity);
    }

    public async Task<IReadOnlyList<Order>> FindByCustomerId(
        string customerId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => ToDomain(o))
            .ToListAsync(cancellationToken);
    }

    private static OrderEntity ToEntity(Order order) =>
        new()
        {
            OrderId = order.OrderId,
            CustomerId = order.CustomerId,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount.Amount,
            Currency = order.TotalAmount.Currency,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            Items = order.Items.Select(i => ToEntity(i, order.OrderId)).ToList()
        };

    private static OrderItemEntity ToEntity(OrderItem item, string orderId) =>
        new()
        {
            ItemId = item.ItemId,
            OrderId = orderId,
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice.Amount,
            Currency = item.UnitPrice.Currency
        };

    private static Order ToDomain(OrderEntity entity) =>
        new(
            entity.OrderId,
            entity.CustomerId,
            entity.Items
                .Select(i => new OrderItem(
                    i.ItemId,
                    i.ProductId,
                    i.ProductName,
                    i.Quantity,
                    new Money(i.UnitPrice, i.Currency)))
                .ToList(),
            Enum.Parse<OrderStatus>(entity.Status),
            new Money(entity.TotalAmount, entity.Currency),
            entity.CreatedAt,
            entity.UpdatedAt);
}
