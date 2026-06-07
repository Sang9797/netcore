namespace Cqrs.OrderService.Bus.Query;

public sealed class QueryBus(IServiceProvider services)
{
    public Task<TResult> Dispatch<TResult>(IQuery<TResult> query, CancellationToken cancellationToken)
    {
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
        dynamic handler = services.GetRequiredService(handlerType);
        return handler.Handle((dynamic)query, cancellationToken);
    }
}
