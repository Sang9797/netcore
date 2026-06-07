using Cqrs.OrderService.Domain.Exception;

namespace Cqrs.OrderService.Presentation;

public static class GlobalExceptionHandler
{
    public static void UseGlobalExceptionHandler(this WebApplication app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var exception = context.Features
                    .Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()
                    ?.Error;

                var (status, payload) = exception switch
                {
                    OrderNotFoundException ex => (StatusCodes.Status404NotFound, ErrorResponse.Of("ORDER_NOT_FOUND", ex.Message)),
                    ProductNotFoundException ex => (StatusCodes.Status404NotFound, ErrorResponse.Of("PRODUCT_NOT_FOUND", ex.Message)),
                    WarehouseNotFoundException ex => (StatusCodes.Status404NotFound, ErrorResponse.Of("WAREHOUSE_NOT_FOUND", ex.Message)),
                    InvalidOrderStateException ex => (StatusCodes.Status409Conflict, ErrorResponse.Of("INVALID_STATE", ex.Message)),
                    InsufficientInventoryException ex => (StatusCodes.Status409Conflict, ErrorResponse.Of("INSUFFICIENT_INVENTORY", ex.Message)),
                    DomainException ex => (StatusCodes.Status400BadRequest, ErrorResponse.Of("DOMAIN_ERROR", ex.Message)),
                    ArgumentException ex => (StatusCodes.Status400BadRequest, ErrorResponse.Of("VALIDATION_ERROR", ex.Message)),
                    _ => (StatusCodes.Status500InternalServerError, ErrorResponse.Of("INTERNAL_ERROR", "An unexpected error occurred"))
                };

                context.Response.StatusCode = status;
                await context.Response.WriteAsJsonAsync(payload);
            });
        });
    }
}
