using System.Reflection;
using Cqrs.OrderService.Application.Common.Errors;
using FluentResults;
using FluentValidation;
using MediatR;

namespace Cqrs.OrderService.Application.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var failures = validators
            .Select(v => v.Validate(request))
            .SelectMany(result => result.Errors)
            .Where(error => error is not null)
            .Select(error => ApplicationErrors.Validation(error.ErrorMessage))
            .Cast<IError>()
            .ToList();

        if (failures.Count == 0)
        {
            return await next();
        }

        var failMethod = typeof(Result)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(method =>
                method.Name == nameof(Result.Fail) &&
                method.IsGenericMethodDefinition &&
                method.GetParameters() is [{ ParameterType: var parameterType }] &&
                parameterType == typeof(IEnumerable<IError>));

        return (TResponse)failMethod.MakeGenericMethod(typeof(TResponse).GenericTypeArguments[0])
            .Invoke(null, [failures])!;
    }
}
