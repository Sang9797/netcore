using Cqrs.OrderService.Application.Command;
using Cqrs.OrderService.Application.Query;
using Cqrs.OrderService.Bus.Command;
using Cqrs.OrderService.Bus.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cqrs.OrderService.Presentation;

[ApiController]
[Authorize]
[Route("api/v1/orders")]
public sealed class OrderController(CommandBus commandBus, QueryBus queryBus) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<OrderResponse>> PlaceOrder(
        [FromBody] PlaceOrderRequest request,
        CancellationToken cancellationToken)
    {
        var command = new PlaceOrderCommand(
            request.CustomerId,
            request.Items
                .Select(i => new OrderItemCommand(
                    i.ProductId,
                    i.ProductName,
                    i.Quantity,
                    i.UnitPrice,
                    i.Currency))
                .ToList());

        var order = await commandBus.Dispatch(command, cancellationToken);
        return Created($"/api/v1/orders/{order.OrderId}", OrderResponse.From(order));
    }

    [HttpGet("{orderId}")]
    public async Task<ActionResult<OrderResponse>> GetOrder(
        string orderId,
        CancellationToken cancellationToken)
    {
        var order = await queryBus.Dispatch(new GetOrderByIdQuery(orderId), cancellationToken);
        return order is null ? NotFound() : Ok(OrderResponse.From(order));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderResponse>>> ListOrders(
        [FromQuery] string customerId,
        CancellationToken cancellationToken)
    {
        var orders = await queryBus.Dispatch(new ListOrdersByCustomerQuery(customerId), cancellationToken);
        return Ok(orders.Select(OrderResponse.From).ToList());
    }

    [HttpPost("{orderId}/confirm")]
    public async Task<IActionResult> ConfirmOrder(string orderId, CancellationToken cancellationToken)
    {
        await commandBus.Dispatch(new ConfirmOrderCommand(orderId), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{orderId}")]
    public async Task<IActionResult> CancelOrder(
        string orderId,
        [FromBody] CancelRequest request,
        CancellationToken cancellationToken)
    {
        await commandBus.Dispatch(new CancelOrderCommand(orderId, request.Reason), cancellationToken);
        return NoContent();
    }
}
