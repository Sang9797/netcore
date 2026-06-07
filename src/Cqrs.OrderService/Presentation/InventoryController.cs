using Cqrs.OrderService.Application.Command;
using Cqrs.OrderService.Application.Query;
using Cqrs.OrderService.Bus.Command;
using Cqrs.OrderService.Bus.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cqrs.OrderService.Presentation;

[ApiController]
[Authorize]
[Route("api/v1/inventory")]
public sealed class InventoryController(CommandBus commandBus, QueryBus queryBus) : ControllerBase
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
        return Ok(await queryBus.Dispatch(query, cancellationToken));
    }

    [HttpGet("products/{productId}/stock")]
    public async Task<ActionResult<IReadOnlyList<ProductStockItem>>> GetProductStock(
        string productId,
        CancellationToken cancellationToken)
    {
        return Ok(await queryBus.Dispatch(GetProductInventoryQuery.All(productId), cancellationToken));
    }

    [HttpGet("low-stock")]
    public async Task<ActionResult<IReadOnlyList<LowStockItem>>> GetLowStock(
        [FromQuery] int threshold = 10,
        [FromQuery] int limit = 100,
        CancellationToken cancellationToken = default)
    {
        return Ok(await queryBus.Dispatch(
            ListLowStockQuery.All(Math.Max(0, threshold), Math.Clamp(limit, 1, 500)),
            cancellationToken));
    }

    [HttpPost("reserve")]
    public async Task<IActionResult> Reserve(
        [FromBody] ReserveRequest request,
        CancellationToken cancellationToken)
    {
        await commandBus.Dispatch(
            new ReserveInventoryCommand(request.ProductId, request.WarehouseId, request.Quantity, request.OrderId),
            cancellationToken);
        return NoContent();
    }

    [HttpPost("release")]
    public async Task<IActionResult> Release(
        [FromBody] ReleaseRequest request,
        CancellationToken cancellationToken)
    {
        await commandBus.Dispatch(
            new ReleaseInventoryCommand(request.ProductId, request.WarehouseId, request.Quantity, request.OrderId),
            cancellationToken);
        return NoContent();
    }

    [HttpPost("adjust")]
    public async Task<IActionResult> Adjust(
        [FromBody] AdjustRequest request,
        CancellationToken cancellationToken)
    {
        await commandBus.Dispatch(
            new AdjustInventoryCommand(request.ProductId, request.WarehouseId, request.Delta, request.Reason),
            cancellationToken);
        return NoContent();
    }
}
