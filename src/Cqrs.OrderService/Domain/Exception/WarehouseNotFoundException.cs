namespace Cqrs.OrderService.Domain.Exception;

public sealed class WarehouseNotFoundException(string id) : DomainException($"Warehouse not found: {id}");
