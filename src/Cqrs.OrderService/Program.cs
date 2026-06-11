using System.Text.Json;
using System.Text.Json.Serialization;
using Cqrs.OrderService.Application.DependencyInjection;
using Cqrs.OrderService.Infrastructure;
using Cqrs.OrderService.Infrastructure.DependencyInjection;
using Cqrs.OrderService.Infrastructure.Jobs;
using Cqrs.OrderService.Infrastructure.Persistence;
using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("ConnectionStrings:Default is required");

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration, connectionString);

builder.Services.AddAuthentication("Bearer")
    .AddScheme<AuthenticationSchemeOptions, HmacJwtAuthenticationHandler>("Bearer", null);
builder.Services.AddAuthorization();

var app = builder.Build();
var hangfireOptions = app.Services.GetRequiredService<HangfireOptions>();

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().Initialize(CancellationToken.None);
}

app.UseAuthentication();
app.UseAuthorization();

if (hangfireOptions.Enabled)
{
    app.UseHangfireDashboard(
        hangfireOptions.DashboardPath,
        new DashboardOptions
        {
            Authorization = [app.Services.GetRequiredService<HangfireDashboardAuthorizationFilter>()]
        });

    RecurringJob.AddOrUpdate<OutboxBacklogMonitorJob>(
        "outbox-backlog-monitor",
        job => job.CheckBacklogAsync(CancellationToken.None),
        hangfireOptions.BacklogMonitorCron);

    RecurringJob.AddOrUpdate<AutoCancelStaleOrdersJob>(
        "auto-cancel-stale-orders",
        job => job.RunAsync(CancellationToken.None),
        hangfireOptions.AutoCancelStaleOrdersCron);

    RecurringJob.AddOrUpdate<DailyOperationsReportJob>(
        "daily-operations-report",
        job => job.RunAsync(CancellationToken.None),
        hangfireOptions.DailyOperationsReportCron);

    RecurringJob.AddOrUpdate<RetryFailedOutboxMessagesJob>(
        "retry-failed-outbox-messages",
        job => job.RunAsync(CancellationToken.None),
        hangfireOptions.RetryFailedOutboxCron);

    RecurringJob.AddOrUpdate<CleanupOldAuditRecordsJob>(
        "cleanup-old-audit-records",
        job => job.RunAsync(CancellationToken.None),
        hangfireOptions.CleanupAuditCron);
}

app.MapControllers();
app.MapOpenApi();
app.MapGet("/swagger-ui.html", () => Results.Redirect("/openapi/v1.json")).AllowAnonymous();
app.MapHealthChecks("/actuator/health");
app.MapHealthChecks("/actuator/health/liveness", new HealthCheckOptions());
app.MapGet("/actuator/prometheus", () => Results.Text("""
    # HELP cqrs_order_service_info Application info
    # TYPE cqrs_order_service_info gauge
    cqrs_order_service_info{runtime=".NET"} 1
    """, "text/plain")).AllowAnonymous();
app.MapGet("/actuator/info", () => new { application = "cqrs-order-service", runtime = ".NET 10" }).AllowAnonymous();

app.Run();

public partial class Program;
