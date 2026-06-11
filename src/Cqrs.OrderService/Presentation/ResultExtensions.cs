using Cqrs.OrderService.Application.Common.Errors;
using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace Cqrs.OrderService.Presentation;

public static class ResultExtensions
{
    public static ActionResult ToActionResult<T>(
        this ControllerBase controller,
        Result<T> result,
        Func<T, ActionResult>? onSuccess = null)
    {
        if (result.IsSuccess)
        {
            return onSuccess is null ? controller.Ok(result.Value) : onSuccess(result.Value);
        }

        var primaryError = result.Errors.FirstOrDefault();
        var statusCode = primaryError?.Metadata.TryGetValue(ErrorMetadata.StatusCode, out var status)
            == true && status is int typedStatus
            ? typedStatus
            : StatusCodes.Status400BadRequest;

        var errorCode = primaryError?.Metadata.TryGetValue(ErrorMetadata.ErrorCode, out var code)
            == true && code is string typedCode
            ? typedCode
            : "REQUEST_FAILED";

        var payload = ErrorResponse.Of(
            errorCode,
            primaryError?.Message ?? "Request failed",
            result.Errors.Skip(1).Select(error => error.Message).ToList());

        return new ObjectResult(payload) { StatusCode = statusCode };
    }
}
