namespace Cqrs.OrderService.Domain.Exception;

public sealed class InvalidOrderStateException(string message) : DomainException(message);
