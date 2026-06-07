using Cqrs.OrderService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cqrs.OrderService.Infrastructure.Persistence;

public sealed class OrdersDbContext(DbContextOptions<OrdersDbContext> options) : DbContext(options)
{
    public DbSet<OrderEntity> Orders => Set<OrderEntity>();
    public DbSet<OrderItemEntity> OrderItems => Set<OrderItemEntity>();
    public DbSet<InventoryEntity> Inventory => Set<InventoryEntity>();
    public DbSet<InventoryTransactionEntity> InventoryTransactions => Set<InventoryTransactionEntity>();
    public DbSet<ProductEntity> Products => Set<ProductEntity>();
    public DbSet<ProductCategoryEntity> ProductCategories => Set<ProductCategoryEntity>();
    public DbSet<WarehouseEntity> Warehouses => Set<WarehouseEntity>();
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<RoleEntity> Roles => Set<RoleEntity>();
    public DbSet<PermissionEntity> Permissions => Set<PermissionEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureOrders(modelBuilder);
        ConfigureInventory(modelBuilder);
        ConfigureUsers(modelBuilder);
    }

    private static void ConfigureOrders(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderEntity>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(e => e.OrderId);
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.TotalAmount).HasColumnName("total_amount");
            entity.Property(e => e.Currency).HasColumnName("currency");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.HasMany(e => e.Items).WithOne(e => e.Order).HasForeignKey(e => e.OrderId);
        });

        modelBuilder.Entity<OrderItemEntity>(entity =>
        {
            entity.ToTable("order_items");
            entity.HasKey(e => e.ItemId);
            entity.Property(e => e.ItemId).HasColumnName("item_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.ProductName).HasColumnName("product_name");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.UnitPrice).HasColumnName("unit_price");
            entity.Property(e => e.Currency).HasColumnName("currency");
        });
    }

    private static void ConfigureInventory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductCategoryEntity>(entity =>
        {
            entity.ToTable("product_categories");
            entity.HasKey(e => e.CategoryId);
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.ParentCategoryId).HasColumnName("parent_category_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.HasOne(e => e.Parent).WithMany().HasForeignKey(e => e.ParentCategoryId);
        });

        modelBuilder.Entity<ProductEntity>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(e => e.ProductId);
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Sku).HasColumnName("sku");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.UnitPrice).HasColumnName("unit_price");
            entity.Property(e => e.Currency).HasColumnName("currency");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(e => e.Category).WithMany().HasForeignKey(e => e.CategoryId);
        });

        modelBuilder.Entity<WarehouseEntity>(entity =>
        {
            entity.ToTable("warehouses");
            entity.HasKey(e => e.WarehouseId);
            entity.Property(e => e.WarehouseId).HasColumnName("warehouse_id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.LocationCode).HasColumnName("location_code");
            entity.Property(e => e.Region).HasColumnName("region");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<InventoryEntity>(entity =>
        {
            entity.ToTable("inventory");
            entity.HasKey(e => e.InventoryId);
            entity.Property(e => e.InventoryId).HasColumnName("inventory_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.WarehouseId).HasColumnName("warehouse_id");
            entity.Property(e => e.QuantityAvailable).HasColumnName("quantity_available");
            entity.Property(e => e.QuantityReserved).HasColumnName("quantity_reserved");
            entity.Property(e => e.LastUpdated).HasColumnName("last_updated");
            entity.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId);
            entity.HasOne(e => e.Warehouse).WithMany().HasForeignKey(e => e.WarehouseId);
        });

        modelBuilder.Entity<InventoryTransactionEntity>(entity =>
        {
            entity.ToTable("inventory_transactions");
            entity.HasKey(e => e.TransactionId);
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.WarehouseId).HasColumnName("warehouse_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.TransactionType).HasColumnName("transaction_type");
            entity.Property(e => e.QuantityDelta).HasColumnName("quantity_delta");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });
    }

    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Username).HasColumnName("username");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Enabled).HasColumnName("enabled");
            entity.HasMany(e => e.Roles)
                .WithMany(e => e.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "user_roles",
                    r => r.HasOne<RoleEntity>().WithMany().HasForeignKey("role_id"),
                    l => l.HasOne<UserEntity>().WithMany().HasForeignKey("user_id"));
        });

        modelBuilder.Entity<RoleEntity>(entity =>
        {
            entity.ToTable("roles");
            entity.HasKey(e => e.RoleId);
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.HasMany(e => e.Permissions)
                .WithMany(e => e.Roles)
                .UsingEntity<Dictionary<string, object>>(
                    "role_permissions",
                    r => r.HasOne<PermissionEntity>().WithMany().HasForeignKey("permission_id"),
                    l => l.HasOne<RoleEntity>().WithMany().HasForeignKey("role_id"));
        });

        modelBuilder.Entity<PermissionEntity>(entity =>
        {
            entity.ToTable("permissions");
            entity.HasKey(e => e.PermissionId);
            entity.Property(e => e.PermissionId).HasColumnName("permission_id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Description).HasColumnName("description");
        });
    }
}
