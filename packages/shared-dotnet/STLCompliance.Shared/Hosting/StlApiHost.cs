using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Data;
using STLCompliance.Shared.Health;
using STLCompliance.Shared.Middleware;

namespace STLCompliance.Shared.Hosting;

public static class StlApiHost
{
    public static async Task RunAsync<TContext>(
        ProductDescriptor product,
        string[] args,
        Action<WebApplicationBuilder>? configure = null,
        Func<WebApplication, Task>? mapEndpoints = null)
        where TContext : PlatformDbContext
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("product", product.ProductKey)
            .CreateLogger();

        try
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseSerilog();

            var connectionString = builder.Configuration.GetConnectionString("Database")
                ?? builder.Configuration["DATABASE_URL"];

            builder.Services.AddSingleton(product);
            builder.Services.AddDbContext<TContext>(options =>
            {
                if (!string.IsNullOrWhiteSpace(connectionString))
                {
                    options.UseNpgsql(connectionString);
                }
            });

            var healthBuilder = builder.Services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy("API process is running."));

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                healthBuilder.AddNpgSql(connectionString, name: "database");
            }

            builder.Services.AddOpenApi();
            builder.Services.AddStlCorrelationId();
            builder.Services.AddStlJwtAuthentication(builder.Configuration);
            configure?.Invoke(builder);

            var signingKey = builder.Configuration["AUTH_SIGNING_KEY"]
                ?? builder.Configuration[$"{StlJwtOptions.SectionName}:SigningKey"];
            var jwtEnabled = !string.IsNullOrWhiteSpace(signingKey) && signingKey.Length >= 32;

            var app = builder.Build();

            app.UseStlCorrelationId();
            app.UseMiddleware<ApiExceptionMiddleware>();
            if (jwtEnabled)
            {
                app.UseAuthentication();
            }

            app.UseAuthorization();

            var exposeOpenApi = app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing");
            if (exposeOpenApi)
            {
                app.MapOpenApi();
            }

            if (app.Environment.IsDevelopment())
            {
                await ApplyMigrationsAsync<TContext>(app);
            }

            app.MapGet("/health", (ProductDescriptor descriptor) =>
            {
                var response = new HealthResponse(
                    Status: "Healthy",
                    Product: descriptor.ProductKey,
                    Version: GetVersion(),
                    TimestampUtc: DateTimeOffset.UtcNow);
                return Results.Ok(response);
            })
            .WithName("GetHealth")
            .WithTags("Health")
            .AllowAnonymous();

            app.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    var payload = new HealthResponse(
                        Status: report.Status.ToString(),
                        Product: product.ProductKey,
                        Version: GetVersion(),
                        TimestampUtc: DateTimeOffset.UtcNow,
                        Checks: report.Entries.ToDictionary(
                            e => e.Key,
                            e => (object)new
                            {
                                status = e.Value.Status.ToString(),
                                description = e.Value.Description,
                                durationMs = e.Value.Duration.TotalMilliseconds
                            }));
                    await context.Response.WriteAsJsonAsync(payload);
                }
            }).AllowAnonymous();

            app.MapGet("/", (ProductDescriptor descriptor) => Results.Ok(new
            {
                product = descriptor.DisplayName,
                key = descriptor.ProductKey,
                health = "/health",
                ready = "/health/ready",
                openapi = exposeOpenApi ? "/openapi/v1.json" : null
            })).AllowAnonymous();

            if (mapEndpoints is not null)
            {
                await mapEndpoints(app);
            }

            var urls = builder.Configuration["ASPNETCORE_URLS"];
            if (string.IsNullOrWhiteSpace(urls) && app.Environment.IsDevelopment())
            {
                app.Urls.Add($"http://localhost:{product.LocalDevPort}");
            }

            Log.Information(
                "Starting {Product} API ({Key}) on {Urls}",
                product.DisplayName,
                product.ProductKey,
                urls ?? $"http://localhost:{product.LocalDevPort}");

            await app.RunAsync();
        }
        catch (Exception ex) when (ex is not HostAbortedException)
        {
            Log.Fatal(ex, "{Product} API terminated unexpectedly", product.DisplayName);
            throw;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    private static string GetVersion() =>
        typeof(StlApiHost).Assembly.GetName().Version?.ToString() ?? "0.0.0";

    private static async Task ApplyMigrationsAsync<TContext>(WebApplication app)
        where TContext : PlatformDbContext
    {
        var connectionString = app.Configuration.GetConnectionString("Database")
            ?? app.Configuration["DATABASE_URL"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            app.Logger.LogWarning(
                "Skipping EF migrations for {Product}: no database connection configured.",
                typeof(TContext).Name);
            return;
        }

        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Migrations");

        try
        {
            logger.LogInformation("Applying EF migrations for {Context}", typeof(TContext).Name);
            await db.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "EF migration failed for {Context}", typeof(TContext).Name);
            throw;
        }
    }
}
