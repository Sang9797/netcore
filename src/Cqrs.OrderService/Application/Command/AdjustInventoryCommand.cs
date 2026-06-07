using Cqrs.OrderService.Bus.Command;

namespace Cqrs.OrderService.Application.Command;

public sealed record AdjustInventoryCommand(
    string ProductId,
    string WarehouseId,
    int Delta,
    string Reason) : ICommand<Unit>;
