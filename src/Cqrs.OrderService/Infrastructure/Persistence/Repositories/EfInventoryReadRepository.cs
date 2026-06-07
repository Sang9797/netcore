using Cqrs.OrderService.Application.Query;
using Microsoft.EntityFrameworkCore;

namespace Cqrs.OrderService.Infrastructure.Persistence.Repositories;

public sealed class EfInventoryReadRepository(OrdersDbContext dbContext) : IInventoryReadRepository
{
    public async Task<IReadOnlyList<InventoryReportItem>> FindInventoryReport(
        GetInventoryReportQuery query,
        CancellationToken cancellationToken)
    {
        var baseQuery =
            from inventory in dbContext.Inventory.AsNoTracking()
            join product in dbContext.Products.AsNoTracking() on inventory.ProductId equals product.ProductId
            join category in dbContext.ProductCategories.AsNoTracking() on product.CategoryId equals category.CategoryId
            join parentCategory in dbContext.ProductCategories.AsNoTracking()
                on category.ParentCategoryId equals parentCategory.CategoryId into parents
            from parentCategory in parents.DefaultIfEmpty()
            join warehouse in dbContext.Warehouses.AsNoTracking() on inventory.WarehouseId equals warehouse.WarehouseId
            where product.IsActive
                && inventory.QuantityAvailable >= query.MinStock
                && (query.CategoryId == null || category.CategoryId == query.CategoryId)
                && (query.WarehouseId == null || warehouse.WarehouseId == query.WarehouseId)
            orderby parentCategory == null ? "Root" : parentCategory.Name, category.Name, product.Name, warehouse.Name
            select new
            {
                Inventory = inventory,
                Product = product,
                Category = category,
                ParentCategoryName = parentCategory == null ? "Root" : parentCategory.Name,
                Warehouse = warehouse
            };

        var page = await baseQuery
            .Skip(query.Page * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var result = new List<InventoryReportItem>(page.Count);
        foreach (var row in page)
        {
            var transactions = dbContext.InventoryTransactions.AsNoTracking()
                .Where(tx => tx.ProductId == row.Product.ProductId && tx.WarehouseId == row.Inventory.WarehouseId);

            result.Add(new InventoryReportItem(
                row.ParentCategoryName,
                row.Category.Name,
                row.Product.ProductId,
                row.Product.Sku,
                row.Product.Name,
                row.Product.UnitPrice,
                row.Product.Currency,
                row.Warehouse.WarehouseId,
                row.Warehouse.Name,
                row.Warehouse.Region,
                row.Inventory.QuantityAvailable,
                row.Inventory.QuantityReserved,
                row.Inventory.QuantityAvailable - row.Inventory.QuantityReserved,
                await transactions.Where(tx => tx.QuantityDelta > 0).SumAsync(tx => (long?)tx.QuantityDelta, cancellationToken) ?? 0,
                await transactions.Where(tx => tx.QuantityDelta < 0).SumAsync(tx => (long?)-tx.QuantityDelta, cancellationToken) ?? 0,
                await transactions.LongCountAsync(cancellationToken),
                await transactions.MaxAsync(tx => (DateTimeOffset?)tx.CreatedAt, cancellationToken) ?? row.Inventory.LastUpdated));
        }

        return result;
    }

    public async Task<IReadOnlyList<ProductStockItem>> FindProductStock(
        GetProductInventoryQuery query,
        CancellationToken cancellationToken)
    {
        return await (
            from inventory in dbContext.Inventory.AsNoTracking()
            join product in dbContext.Products.AsNoTracking() on inventory.ProductId equals product.ProductId
            join category in dbContext.ProductCategories.AsNoTracking() on product.CategoryId equals category.CategoryId
            join warehouse in dbContext.Warehouses.AsNoTracking() on inventory.WarehouseId equals warehouse.WarehouseId
            where product.ProductId == query.ProductId
            orderby warehouse.Name
            select new ProductStockItem(
                product.ProductId,
                product.Sku,
                product.Name,
                product.UnitPrice,
                product.Currency,
                category.Name,
                warehouse.WarehouseId,
                warehouse.Name,
                warehouse.Region,
                inventory.QuantityAvailable,
                inventory.QuantityReserved,
                inventory.QuantityAvailable - inventory.QuantityReserved,
                inventory.LastUpdated))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LowStockItem>> FindLowStock(
        ListLowStockQuery query,
        CancellationToken cancellationToken)
    {
        return await (
            from inventory in dbContext.Inventory.AsNoTracking()
            join product in dbContext.Products.AsNoTracking() on inventory.ProductId equals product.ProductId
            join warehouse in dbContext.Warehouses.AsNoTracking() on inventory.WarehouseId equals warehouse.WarehouseId
            let quantityFree = inventory.QuantityAvailable - inventory.QuantityReserved
            where product.IsActive && quantityFree <= query.Threshold
            orderby quantityFree, product.Name
            select new LowStockItem(
                product.ProductId,
                product.Sku,
                product.Name,
                warehouse.WarehouseId,
                warehouse.Name,
                warehouse.Region,
                inventory.QuantityAvailable,
                inventory.QuantityReserved,
                quantityFree))
            .Take(query.Limit)
            .ToListAsync(cancellationToken);
    }
}
