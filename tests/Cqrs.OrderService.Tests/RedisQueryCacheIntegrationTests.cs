using System.Diagnostics;
using Cqrs.OrderService.Application.Abstractions.Data;
using Cqrs.OrderService.Application.Common.Caching;
using Cqrs.OrderService.Application.Handler.Query;
using Cqrs.OrderService.Application.Query;
using Cqrs.OrderService.Infrastructure.Caching;
using StackExchange.Redis;

namespace Cqrs.OrderService.Tests;

public sealed class RedisQueryCacheIntegrationTests : IAsyncLifetime
{
    private string? _containerId;
    private ConnectionMultiplexer? _connectionMultiplexer;

    [Fact]
    public async Task InventoryReportQuery_RefreshesAfterCacheVersionIncrement()
    {
        var connectionMultiplexer = _connectionMultiplexer ?? throw new InvalidOperationException("Redis connection not initialized");
        var options = new RedisOptions
        {
            Enabled = true,
            ConnectionString = connectionMultiplexer.Configuration,
            InstanceName = $"test:{Guid.NewGuid():N}:",
            DefaultTtlSeconds = 60
        };

        var cache = new RedisQueryCache(connectionMultiplexer, options);
        var versionService = new RedisCacheVersionService(connectionMultiplexer, options);
        var repository = new FakeInventoryReadRepository();
        var handler = new GetInventoryReportQueryHandler(repository, cache, versionService);
        var query = GetInventoryReportQuery.All(null, null, 0, 0, 100);

        repository.Report = [CreateItem("SKU-1")];

        var first = await handler.Handle(query, CancellationToken.None);
        repository.Report = [CreateItem("SKU-2")];
        var second = await handler.Handle(query, CancellationToken.None);

        await versionService.IncrementVersionAsync(CacheKeys.InventoryScope, CancellationToken.None);
        var third = await handler.Handle(query, CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.True(third.IsSuccess);
        Assert.Equal("SKU-1", first.Value[0].Sku);
        Assert.Equal("SKU-1", second.Value[0].Sku);
        Assert.Equal("SKU-2", third.Value[0].Sku);
        Assert.Equal(2, repository.ReportCallCount);
    }

    public async Task InitializeAsync()
    {
        _containerId = await RunDockerAsync("run", "--rm", "-d", "-P", "redis:7-alpine");
        var port = await RunDockerAsync("inspect", _containerId, "--format", "{{(index (index .NetworkSettings.Ports \"6379/tcp\") 0).HostPort}}");
        var connectionString = $"127.0.0.1:{port.Trim()},abortConnect=false";

        _connectionMultiplexer = await WaitForRedis(connectionString);
    }

    public async Task DisposeAsync()
    {
        if (_connectionMultiplexer is not null)
        {
            await _connectionMultiplexer.CloseAsync();
            await _connectionMultiplexer.DisposeAsync();
        }

        if (!string.IsNullOrWhiteSpace(_containerId))
        {
            _ = await RunDockerAsync("rm", "-f", _containerId);
        }
    }

    private static InventoryReportItem CreateItem(string sku) =>
        new(
            "Root",
            "Category",
            "product-1",
            sku,
            "Product Name",
            10m,
            "USD",
            "warehouse-1",
            "Warehouse",
            "Region",
            100,
            10,
            90,
            100,
            10,
            1,
            DateTimeOffset.UtcNow);

    private static async Task<ConnectionMultiplexer> WaitForRedis(string connectionString)
    {
        Exception? lastError = null;

        for (var attempt = 0; attempt < 20; attempt++)
        {
            try
            {
                var mux = await ConnectionMultiplexer.ConnectAsync(connectionString);
                await mux.GetDatabase().PingAsync();
                return mux;
            }
            catch (Exception exception)
            {
                lastError = exception;
                await Task.Delay(500);
            }
        }

        throw new InvalidOperationException("Unable to connect to Redis test container", lastError);
    }

    private static async Task<string> RunDockerAsync(params string[] arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "docker",
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start docker process");
        var stdout = await process.StandardOutput.ReadToEndAsync();
        var stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"docker {string.Join(' ', arguments)} failed: {stderr}");
        }

        return stdout.Trim();
    }

    private sealed class FakeInventoryReadRepository : IInventoryReadRepository
    {
        public IReadOnlyList<InventoryReportItem> Report { get; set; } = [];
        public int ReportCallCount { get; private set; }

        public Task<IReadOnlyList<InventoryReportItem>> FindInventoryReport(
            GetInventoryReportQuery query,
            CancellationToken cancellationToken)
        {
            ReportCallCount += 1;
            return Task.FromResult(Report);
        }

        public Task<IReadOnlyList<ProductStockItem>> FindProductStock(
            GetProductInventoryQuery query,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<LowStockItem>> FindLowStock(
            ListLowStockQuery query,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }
}
