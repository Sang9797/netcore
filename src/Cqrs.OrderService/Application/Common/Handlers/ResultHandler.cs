using Cqrs.OrderService.Application.Common.Errors;
using FluentResults;

namespace Cqrs.OrderService.Application.Common.Handlers;

public static class ResultHandler
{
    public static async Task<Result<T>> Execute<T>(Func<Task<T>> action)
    {
        try
        {
            return Result.Ok(await action());
        }
        catch (Exception exception)
        {
            return Result.Fail<T>(ApplicationErrors.FromException(exception));
        }
    }

    public static async Task<Result<MediatR.Unit>> Execute(Func<Task> action)
    {
        try
        {
            await action();
            return Result.Ok(MediatR.Unit.Value);
        }
        catch (Exception exception)
        {
            return Result.Fail<MediatR.Unit>(ApplicationErrors.FromException(exception));
        }
    }
}
