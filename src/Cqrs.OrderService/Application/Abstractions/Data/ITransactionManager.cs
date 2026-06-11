namespace Cqrs.OrderService.Application.Abstractions.Data;

public interface ITransactionManager
{
    Task<T> ExecuteAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken);
}
