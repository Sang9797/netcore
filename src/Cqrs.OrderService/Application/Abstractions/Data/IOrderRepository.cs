using Cqrs.OrderService.Domain.Model;

namespace Cqrs.OrderService.Application.Abstractions.Data;

public interface IOrderRepository
{
    Task<Order> Save(Order order, CancellationToken cancellationToken);

    Task<Order?> FindById(string orderId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Order>> FindByCustomerId(string customerId, CancellationToken cancellationToken);
}
