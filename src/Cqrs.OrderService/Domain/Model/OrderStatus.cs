namespace Cqrs.OrderService.Domain.Model;

public enum OrderStatus
{
    PENDING,
    CONFIRMED,
    SHIPPED,
    DELIVERED,
    CANCELLED
}
