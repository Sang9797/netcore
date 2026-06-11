using Cqrs.OrderService.Application.Command;
using Cqrs.OrderService.Application.Query;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cqrs.OrderService.Presentation;

[ApiController]
[Authorize]
[Route("api/v1/inventory")]
public sealed class InventoryController(ISender sender) : ControllerBase
{
    [HttpGet("report")]
    public async Task<ActionResult<IReadOnlyList<InventoryReportItem>>> GetReport(
        [FromQuery] string? categoryId,
        [FromQuery] string? warehouseId,
        [FromQuery] int minStock = 0,
        [FromQuery] int page = 0,
        [FromQuery] int pageSize = 100,
        CancellationToken cancellationToken = default)
    {
        var query = GetInventoryReportQuery.All(
            categoryId,
            warehouseId,
            Math.Max(0, minStock),
            Math.Max(0, page),
            Math.Clamp(pageSize, 1, 500));
        var result = await sender.Send(query, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("products/{productId}/stock")]
    public async Task<ActionResult<IReadOnlyList<ProductStockItem>>> GetProductStock(
        string productId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(GetProductInventoryQuery.All(productId), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("low-stock")]
    public async Task<ActionResult<IReadOnlyList<LowStockItem>>> GetLowStock(
        [FromQuery] int threshold = 10,
        [FromQuery] int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            ListLowStockQuery.All(Math.Max(0, threshold), Math.Clamp(limit, 1, 500)),
            cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("reserve")]
    public async Task<IActionResult> Reserve(
        [FromBody] ReserveRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ReserveInventoryCommand(request.ProductId, request.WarehouseId, request.Quantity, request.OrderId),
            cancellationToken);
        return this.ToActionResult(result, _ => NoContent());
    }

    [HttpPost("release")]
    public async Task<IActionResult> Release(
        [FromBody] ReleaseRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ReleaseInventoryCommand(request.ProductId, request.WarehouseId, request.Quantity, request.OrderId),
            cancellationToken);
        return this.ToActionResult(result, _ => NoContent());
    }

    [HttpPost("adjust")]
    public async Task<IActionResult> Adjust(
        [FromBody] AdjustRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new AdjustInventoryCommand(request.ProductId, request.WarehouseId, request.Delta, request.Reason),
            cancellationToken);
        return this.ToActionResult(result, _ => NoContent());
    }
}
