namespace Cqrs.OrderService.Domain.Exception;

public class DomainException(string message) : System.Exception(message);
