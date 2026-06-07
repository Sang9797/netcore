using System.Text.Json;
using System.Text.Json.Serialization;
using Cqrs.OrderService.Application;
using Cqrs.OrderService.Application.Command;
using Cqrs.OrderService.Application.Handler.Command;
using Cqrs.OrderService.Application.Handler.Query;
using Cqrs.OrderService.Application.Query;
using Cqrs.OrderService.Bus.Command;
using Cqrs.OrderService.Bus.Query;
using Cqrs.OrderService.Domain.Model;
using Cqrs.OrderService.Infrastructure;
using Cqrs.OrderService.Infrastructure.DependencyInjection;
using Cqrs.OrderService.Infrastructure.Persistence;
using Cqrs.OrderService.Infrastructure.Persistence.Repositories;
using Cqrs.OrderService.Presentation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();
var connectionString = BuildConnectionString(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

builder.Services.AddDbContext<OrdersDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<DatabaseInitializer>();
builder.Services.AddScoped<CommandBus>();
builder.Services.AddScoped<QueryBus>();
builder.Services.AddApplicationServices();

builder.Services.AddAuthentication("Bearer")
    .AddScheme<AuthenticationSchemeOptions, HmacJwtAuthenticationHandler>("Bearer", null);
builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().Initialize(CancellationToken.None);
}

app.UseGlobalExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

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
app.MapGet("/graphiql", () => Results.Content("""
    <!doctype html><html><body><h1>GraphQL</h1><form method="post" action="/graphql">
    <textarea name="query" rows="20" cols="100">{ lowStock { productId sku quantityFree } }</textarea>
    <br><button type="submit">Run</button></form></body></html>
    """, "text/html")).AllowAnonymous();
app.MapGet("/graphql/schema.graphqls", async () =>
{
    var path = Path.Combine(AppContext.BaseDirectory, "graphql", "inventory.graphqls");
    return Results.Text(await File.ReadAllTextAsync(path), "text/plain");
}).AllowAnonymous();

app.Run();

static string BuildConnectionString(IConfiguration config)
{
    var configured = config.GetConnectionString("Default");
    if (!string.IsNullOrWhiteSpace(configured))
    {
        return configured;
    }

    var host = config["DB_HOST"] ?? "localhost";
    var port = config["DB_PORT"] ?? "5432";
    var db = config["DB_NAME"] ?? "orders_db";
    var user = config["DB_USERNAME"] ?? config["DB_USER"] ?? "orders_user";
    var password = config["DB_PASSWORD"] ?? "orders_pass";
    return $"Host={host};Port={port};Database={db};Username={user};Password={password};Pooling=true;Maximum Pool Size=20";
}

public partial class Program;
