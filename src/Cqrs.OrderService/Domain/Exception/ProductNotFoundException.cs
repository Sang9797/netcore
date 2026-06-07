namespace Cqrs.OrderService.Domain.Exception;

public sealed class ProductNotFoundException(string id) : DomainException($"Product not found: {id}");
