namespace Cqrs.OrderService.Domain.Exception;

public sealed class OrderNotFoundException(string id) : DomainException($"Order not found: {id}");
