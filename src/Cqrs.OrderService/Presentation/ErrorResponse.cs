namespace Cqrs.OrderService.Presentation;

public sealed record ErrorResponse(string Error, string Message, IReadOnlyList<string>? Details = null)
{
    public static ErrorResponse Of(string error, string message, IReadOnlyList<string>? details = null) =>
        new(error, message, details);
}
