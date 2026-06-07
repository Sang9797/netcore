using Cqrs.OrderService.Bus.Command;

namespace Cqrs.OrderService.Application.Command;

public sealed record CancelOrderCommand(string OrderId, string Reason) : ICommand<Unit>;
