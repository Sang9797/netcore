using Cqrs.OrderService.Application.Query;
using Cqrs.OrderService.Bus.Query;
using Cqrs.OrderService.Domain.Model;
using Cqrs.OrderService.Infrastructure.Persistence;

namespace Cqrs.OrderService.Application.Handler.Query;

public sealed class GetOrderByIdQueryHandler(IOrderRepository repository)
    : IQueryHandler<GetOrderByIdQuery, Order?>
{
    public Task<Order?> Handle(GetOrderByIdQuery query, CancellationToken cancellationToken) =>
        repository.FindById(query.OrderId, cancellationToken);
}
