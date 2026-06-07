using Cqrs.OrderService.Bus.Command;

namespace Cqrs.OrderService.Application.Command;

public sealed record ReserveInventoryCommand(
    string ProductId,
    string WarehouseId,
    int Quantity,
    string OrderId) : ICommand<Unit>;
