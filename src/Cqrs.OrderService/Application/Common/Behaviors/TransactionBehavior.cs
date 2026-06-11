using Cqrs.OrderService.Application.Abstractions.Messaging;
using Cqrs.OrderService.Application.Abstractions.Data;
using MediatR;

namespace Cqrs.OrderService.Application.Common.Behaviors;

public sealed class TransactionBehavior<TRequest, TResponse>(ITransactionManager transactionManager)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        return IsCommand() ? transactionManager.ExecuteAsync<TResponse>(() => next(), cancellationToken) : next();

        static bool IsCommand() =>
            typeof(TRequest).GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>));
    }
}
