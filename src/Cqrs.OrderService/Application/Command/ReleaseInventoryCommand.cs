using Cqrs.OrderService.Bus.Command;

namespace Cqrs.OrderService.Application.Command;

public sealed record ReleaseInventoryCommand(
    string ProductId,
    string WarehouseId,
    int Quantity,
    string OrderId) : ICommand<Unit>;
