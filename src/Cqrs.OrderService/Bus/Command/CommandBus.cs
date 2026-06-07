using Cqrs.OrderService.Infrastructure.Persistence;

namespace Cqrs.OrderService.Bus.Command;

public sealed class CommandBus(IServiceProvider services, OrdersDbContext dbContext)
{
    public Task<TResult> Dispatch<TResult>(ICommand<TResult> command, CancellationToken cancellationToken)
    {
        return DispatchInternal<TResult>((dynamic)command, cancellationToken);
    }

    private async Task<TResult> DispatchInternal<TResult>(ICommand<TResult> command, CancellationToken cancellationToken)
    {
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResult));
        dynamic handler = services.GetRequiredService(handlerType);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            TResult result = await handler.Handle((dynamic)command, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
