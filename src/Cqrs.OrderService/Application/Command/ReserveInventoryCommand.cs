using Cqrs.OrderService.Application.Abstractions.Messaging;

namespace Cqrs.OrderService.Application.Command;

public sealed record ReserveInventoryCommand(
    string ProductId,
    string WarehouseId,
    int Quantity,
    string OrderId) : ICommand<MediatR.Unit>;
