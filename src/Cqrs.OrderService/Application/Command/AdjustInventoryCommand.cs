using Cqrs.OrderService.Application.Abstractions.Messaging;

namespace Cqrs.OrderService.Application.Command;

public sealed record AdjustInventoryCommand(
    string ProductId,
    string WarehouseId,
    int Delta,
    string Reason) : ICommand<MediatR.Unit>;
