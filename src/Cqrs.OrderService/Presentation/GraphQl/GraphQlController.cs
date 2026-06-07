using System.Security.Claims;
using System.Text.Json;
using Cqrs.OrderService.Application.Command;
using Cqrs.OrderService.Application.Query;
using Cqrs.OrderService.Bus.Command;
using Cqrs.OrderService.Bus.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cqrs.OrderService.Presentation.GraphQl;

[ApiController]
[Authorize]
[Route("graphql")]
public sealed class GraphQlController(CommandBus commandBus, QueryBus queryBus) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Execute(CancellationToken cancellationToken)
    {
        using var document = await JsonDocument.ParseAsync(Request.Body, cancellationToken: cancellationToken);
        var root = document.RootElement;
        var query = root.TryGetProperty("query", out var queryElement) ? queryElement.GetString() ?? "" : "";
        var variables = root.TryGetProperty("variables", out var vars) ? vars : default;
        var hasPrice = User.HasClaim("authority", "INVENTORY_PRICE") || User.IsInRole("INVENTORY_PRICE");

        if (query.Contains("reserveInventory", StringComparison.Ordinal))
        {
            var input = GraphInput(variables, root);
            await commandBus.Dispatch(new ReserveInventoryCommand(S(input, "productId"), S(input, "warehouseId"), I(input, "quantity"), S(input, "orderId")), cancellationToken);
            return Ok(new { data = new { reserveInventory = true } });
        }

        if (query.Contains("releaseInventory", StringComparison.Ordinal))
        {
            var input = GraphInput(variables, root);
            await commandBus.Dispatch(new ReleaseInventoryCommand(S(input, "productId"), S(input, "warehouseId"), I(input, "quantity"), S(input, "orderId")), cancellationToken);
            return Ok(new { data = new { releaseInventory = true } });
        }

        if (query.Contains("adjustInventory", StringComparison.Ordinal))
        {
            var input = GraphInput(variables, root);
            await commandBus.Dispatch(new AdjustInventoryCommand(S(input, "productId"), S(input, "warehouseId"), I(input, "delta"), S(input, "reason")), cancellationToken);
            return Ok(new { data = new { adjustInventory = true } });
        }

        if (query.Contains("productStock", StringComparison.Ordinal))
        {
            var productId = variables.ValueKind == JsonValueKind.Object && variables.TryGetProperty("productId", out var p)
                ? p.GetString()!
                : ExtractArg(query, "productId");
            var result = await queryBus.Dispatch(new GetProductInventoryQuery(productId, RequestedFields(query)), cancellationToken);
            return Ok(new { data = new { productStock = result.Select(i => ProductStockGraph(i, hasPrice)) } });
        }

        if (query.Contains("lowStock", StringComparison.Ordinal))
        {
            var threshold = variables.ValueKind == JsonValueKind.Object && variables.TryGetProperty("threshold", out var t) ? t.GetInt32() : 10;
            var limit = variables.ValueKind == JsonValueKind.Object && variables.TryGetProperty("limit", out var l) ? l.GetInt32() : 100;
            var result = await queryBus.Dispatch(new ListLowStockQuery(threshold, limit, RequestedFields(query)), cancellationToken);
            return Ok(new { data = new { lowStock = result } });
        }

        if (query.Contains("inventoryReport", StringComparison.Ordinal))
        {
            var result = await queryBus.Dispatch(new GetInventoryReportQuery(null, null, 0, 0, 100, RequestedFields(query)), cancellationToken);
            return Ok(new { data = new { inventoryReport = result.Select(i => InventoryReportGraph(i, hasPrice)) } });
        }

        return Ok(new { errors = new[] { new { message = "Unsupported GraphQL operation" } } });
    }

    private static JsonElement GraphInput(JsonElement variables, JsonElement root)
    {
        if (variables.ValueKind == JsonValueKind.Object && variables.TryGetProperty("input", out var input))
        {
            return input;
        }

        return root.TryGetProperty("input", out var rootInput) ? rootInput : variables;
    }

    private static string S(JsonElement element, string property) =>
        element.TryGetProperty(property, out var value) ? value.GetString() ?? "" : "";

    private static int I(JsonElement element, string property) =>
        element.TryGetProperty(property, out var value) ? value.GetInt32() : 0;

    private static HashSet<string> RequestedFields(string query) =>
        new HashSet<string>(StringComparer.Ordinal)
        {
            "parentCategoryName", "categoryName", "productId", "sku", "productName", "unitPrice",
            "currency", "warehouseId", "warehouseName", "region", "quantityAvailable",
            "quantityReserved", "quantityFree", "totalReceived", "totalShipped",
            "transactionCount", "lastMovement", "lastUpdated"
        }.Where(query.Contains).ToHashSet(StringComparer.Ordinal);

    private static string ExtractArg(string query, string name)
    {
        var marker = $"{name}:";
        var start = query.IndexOf(marker, StringComparison.Ordinal);
        if (start < 0) return "";
        start += marker.Length;
        while (start < query.Length && char.IsWhiteSpace(query[start])) start++;
        if (start < query.Length && query[start] == '"') start++;
        var end = start;
        while (end < query.Length && query[end] != '"' && query[end] != ')' && query[end] != ',') end++;
        return query[start..end].Trim();
    }

    private static object ProductStockGraph(ProductStockItem item, bool hasPrice) => new
    {
        item.ProductId,
        item.Sku,
        item.ProductName,
        UnitPrice = hasPrice ? item.UnitPrice : null,
        item.Currency,
        item.CategoryName,
        item.WarehouseId,
        item.WarehouseName,
        item.Region,
        item.QuantityAvailable,
        item.QuantityReserved,
        item.QuantityFree,
        item.LastUpdated
    };

    private static object InventoryReportGraph(InventoryReportItem item, bool hasPrice) => new
    {
        item.ParentCategoryName,
        item.CategoryName,
        item.ProductId,
        item.Sku,
        item.ProductName,
        UnitPrice = hasPrice ? item.UnitPrice : null,
        item.Currency,
        item.WarehouseId,
        item.WarehouseName,
        item.Region,
        item.QuantityAvailable,
        item.QuantityReserved,
        item.QuantityFree,
        TotalReceived = hasPrice ? item.TotalReceived : (long?)null,
        TotalShipped = hasPrice ? item.TotalShipped : (long?)null,
        TransactionCount = hasPrice ? item.TransactionCount : (long?)null,
        item.LastMovement
    };
}
