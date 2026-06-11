using Cqrs.OrderService.Application.Abstractions.Messaging;

namespace Cqrs.OrderService.Application.Command;

public sealed record CancelOrderCommand(string OrderId, string Reason) : ICommand<MediatR.Unit>;
