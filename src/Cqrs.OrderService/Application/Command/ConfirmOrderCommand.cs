using Cqrs.OrderService.Application.Abstractions.Messaging;

namespace Cqrs.OrderService.Application.Command;

public sealed record ConfirmOrderCommand(string OrderId) : ICommand<MediatR.Unit>;
