using Cqrs.OrderService.Application.Abstractions.Data;
using Cqrs.OrderService.Application.Abstractions.Security;
using Cqrs.OrderService.Application.Abstractions.Caching;
using Cqrs.OrderService.Application.Abstractions.Messaging;
using Cqrs.OrderService.Infrastructure.Caching;
using Cqrs.OrderService.Infrastructure.Integration;
using Cqrs.OrderService.Infrastructure.Jobs;
using Cqrs.OrderService.Infrastructure.Persistence;
using Cqrs.OrderService.Infrastructure.Persistence.Repositories;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace Cqrs.OrderService.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionString)
    {
        var redisOptions = configuration.GetSection("Redis").Get<RedisOptions>() ?? new RedisOptions();
        var rabbitMqOptions = configuration.GetSection("RabbitMq").Get<RabbitMqOptions>() ?? new RabbitMqOptions();
        var hangfireOptions = configuration.GetSection("Hangfire").Get<HangfireOptions>() ?? new HangfireOptions();

        services.AddSingleton(redisOptions);
        services.AddSingleton(rabbitMqOptions);
        services.AddSingleton(hangfireOptions);
        services.AddDbContext<OrdersDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<DatabaseInitializer>();
        services.AddScoped<ITransactionManager, EfCoreTransactionManager>();
        services.AddScoped<IOrderRepository, EfOrderRepository>();
        services.AddScoped<IInventoryRepository, EfInventoryRepository>();
        services.AddScoped<IInventoryReadRepository, EfInventoryReadRepository>();
        services.AddScoped<IIntegrationEventAuditRepository, EfIntegrationEventAuditRepository>();
        services.AddScoped<IUserReadRepository, UserRepository>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IPasswordVerifier, PasswordVerifier>();
        services.AddScoped<UserRepository>();
        services.AddScoped<JwtTokenService>();
        services.AddScoped<PasswordVerifier>();
        services.AddScoped<OutboxDispatchProcessor>();
        services.AddScoped<OutboxDispatchHangfireJob>();
        services.AddScoped<OutboxBacklogMonitorJob>();
        services.AddScoped<AutoCancelStaleOrdersJob>();
        services.AddScoped<DailyOperationsReportJob>();
        services.AddScoped<RetryFailedOutboxMessagesJob>();
        services.AddScoped<CleanupOldAuditRecordsJob>();
        services.AddSingleton<HangfireDashboardAuthorizationFilter>();

        if (redisOptions.Enabled)
        {
            services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisOptions.ConnectionString));
            services.AddSingleton<IQueryCache, RedisQueryCache>();
            services.AddSingleton<ICacheVersionService, RedisCacheVersionService>();
        }
        else
        {
            services.AddSingleton<IQueryCache, NoOpQueryCache>();
            services.AddSingleton<ICacheVersionService, NoOpCacheVersionService>();
        }

        if (rabbitMqOptions.Enabled)
        {
            services.AddScoped<IOutboxWriter, OutboxWriter>();
            services.AddSingleton<RabbitMqConnectionFactory>();
            services.AddSingleton<RabbitMqMessagePublisher>();
            services.AddHostedService<RabbitMqOutboxDispatcher>();
            if (rabbitMqOptions.ConsumerEnabled)
            {
                services.AddHostedService<RabbitMqEventConsumer>();
            }
        }
        else
        {
            services.AddScoped<IOutboxWriter, NoOpOutboxWriter>();
        }

        if (hangfireOptions.Enabled)
        {
            services.AddHangfire((_, globalConfiguration) => globalConfiguration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString)));

            services.AddHangfireServer(serverOptions =>
            {
                serverOptions.WorkerCount = hangfireOptions.WorkerCount;
            });
        }

        return services;
    }
}
