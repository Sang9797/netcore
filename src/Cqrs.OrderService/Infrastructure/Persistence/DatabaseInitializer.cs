namespace Cqrs.OrderService.Infrastructure.Persistence;

public sealed class DatabaseInitializer(
    IConfiguration config,
    ILogger<DatabaseInitializer> logger)
{
    public async Task Initialize(CancellationToken cancellationToken)
    {
        if (!config.GetValue("Database:ApplyMigrations", true))
        {
            return;
        }

        logger.LogWarning(
            "Database:ApplyMigrations is enabled, but in-app migrations are disabled. Run Liquibase via `make migrate` or the docker compose liquibase service.");
        await Task.CompletedTask;
    }
}
