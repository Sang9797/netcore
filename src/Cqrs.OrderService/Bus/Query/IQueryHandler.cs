namespace Cqrs.OrderService.Bus.Query;

public interface IQueryHandler<in TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    Task<TResult> Handle(TQuery query, CancellationToken cancellationToken);
}
