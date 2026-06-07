namespace Cqrs.OrderService.Domain.Exception;

public sealed class InsufficientInventoryException(string message) : DomainException(message);
