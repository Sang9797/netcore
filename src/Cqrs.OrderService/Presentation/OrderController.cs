using Cqrs.OrderService.Application.Command;
using Cqrs.OrderService.Application.Query;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cqrs.OrderService.Presentation;

[ApiController]
[Authorize]
[Route("api/v1/orders")]
public sealed class OrderController(ISender sender) : ControllerBase
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

        var result = await sender.Send(command, cancellationToken);
        return this.ToActionResult(result, order => Created($"/api/v1/orders/{order.OrderId}", OrderResponse.From(order)));
    }

    [HttpGet("{orderId}")]
    public async Task<ActionResult<OrderResponse>> GetOrder(
        string orderId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOrderByIdQuery(orderId), cancellationToken);
        return this.ToActionResult(result, order => Ok(OrderResponse.From(order)));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderResponse>>> ListOrders(
        [FromQuery] string customerId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListOrdersByCustomerQuery(customerId), cancellationToken);
        return this.ToActionResult(result, orders => Ok(orders.Select(OrderResponse.From).ToList()));
    }

    [HttpPost("{orderId}/confirm")]
    public async Task<IActionResult> ConfirmOrder(string orderId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ConfirmOrderCommand(orderId), cancellationToken);
        return this.ToActionResult(result, _ => NoContent());
    }

    [HttpDelete("{orderId}")]
    public async Task<IActionResult> CancelOrder(
        string orderId,
        [FromBody] CancelRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CancelOrderCommand(orderId, request.Reason), cancellationToken);
        return this.ToActionResult(result, _ => NoContent());
    }
}
