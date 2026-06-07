using Cqrs.OrderService.Bus.Command;

namespace Cqrs.OrderService.Application.Command;

public sealed record ConfirmOrderCommand(string OrderId) : ICommand<Unit>;
