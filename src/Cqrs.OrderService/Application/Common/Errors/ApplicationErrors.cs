using Cqrs.OrderService.Domain.Exception;
using FluentResults;

namespace Cqrs.OrderService.Application.Common.Errors;

public static class ApplicationErrors
{
    public static Error Validation(string message) => Create("VALIDATION_ERROR", StatusCodes.Status400BadRequest, message);

    public static Error NotFound(string code, string message) => Create(code, StatusCodes.Status404NotFound, message);

    public static Error Conflict(string code, string message) => Create(code, StatusCodes.Status409Conflict, message);

    public static Error Unauthorized(string message) => Create("UNAUTHORIZED", StatusCodes.Status401Unauthorized, message);

    public static Error Unexpected(string message) => Create("INTERNAL_ERROR", StatusCodes.Status500InternalServerError, message);

    public static Error FromException(Exception exception) =>
        exception switch
        {
            InvalidOrderStateException ex => Conflict("INVALID_STATE", ex.Message),
            InsufficientInventoryException ex => Conflict("INSUFFICIENT_INVENTORY", ex.Message),
            DomainException ex => Create("DOMAIN_ERROR", StatusCodes.Status400BadRequest, ex.Message),
            ArgumentException ex => Validation(ex.Message),
            _ => Unexpected("An unexpected error occurred")
        };

    private static Error Create(string code, int statusCode, string message) =>
        new Error(message)
            .WithMetadata(ErrorMetadata.ErrorCode, code)
            .WithMetadata(ErrorMetadata.StatusCode, statusCode);
}
