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
using STLCompliance.Shared.Integration;
using STLCompliance.Shared.Middleware;
using STLCompliance.Shared.Observability;

namespace STLCompliance.Shared.Hosting;

public static class StlApiHost
{
    public static async Task RunAsync<TContext>(
        ProductDescriptor product,
        string[] args,
        Action<WebApplicationBuilder>? configure = null,
        Action<WebApplication>? configurePipeline = null,
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

            var connectionString = StlDatabaseConnection.Resolve(builder.Configuration);

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
            builder.Services.Configure<StlServiceTokenOptions>(builder.Configuration.GetSection(StlServiceTokenOptions.SectionName));
            builder.Services.AddSingleton<StlServiceTokenValidator>();
            builder.AddStlIntegrationTokenProvisioning();
            builder.AddStlOpenTelemetry(product);
            configure?.Invoke(builder);

            var signingKey = builder.Configuration["AUTH_SIGNING_KEY"]
                ?? builder.Configuration[$"{StlJwtOptions.SectionName}:SigningKey"];
            if (builder.Environment.IsProduction() && (string.IsNullOrWhiteSpace(signingKey) || signingKey.Length < 32))
            {
                throw new InvalidOperationException(
                    "AUTH_SIGNING_KEY must be configured with at least 32 characters before starting in Production.");
            }
            var jwtEnabled = !string.IsNullOrWhiteSpace(signingKey) && signingKey.Length >= 32;

            var app = builder.Build();
            var bundledFrontendIndexPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "index.html");
            var hasBundledFrontend = File.Exists(bundledFrontendIndexPath);

            if (hasBundledFrontend)
            {
                var productFrontendIndexPaths = Directory
                    .EnumerateDirectories(Path.Combine(app.Environment.ContentRootPath, "wwwroot"))
                    .Select(path => new
                    {
                        Segment = Path.GetFileName(path),
                        IndexPath = Path.Combine(path, "index.html")
                    })
                    .Where(item => File.Exists(item.IndexPath))
                    .ToDictionary(
                        item => item.Segment,
                        item => item.IndexPath,
                        StringComparer.OrdinalIgnoreCase);

                app.MapWhen(
                    context =>
                        !context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase)
                        && !context.Request.Path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase)
                        && !context.Request.Path.StartsWithSegments("/openapi", StringComparison.OrdinalIgnoreCase),
                    spaApp =>
                    {
                        spaApp.UseDefaultFiles();
                        spaApp.UseStaticFiles();
                        spaApp.Run(async context =>
                        {
                            var firstSegment = context.Request.Path.Value?
                                .TrimStart('/')
                                .Split('/', 2, StringSplitOptions.RemoveEmptyEntries)
                                .FirstOrDefault();
                            var indexPath = firstSegment is not null
                                && productFrontendIndexPaths.TryGetValue(firstSegment, out var productIndexPath)
                                    ? productIndexPath
                                    : bundledFrontendIndexPath;

                            context.Response.ContentType = "text/html; charset=utf-8";
                            await context.Response.SendFileAsync(indexPath);
                        });
                    });
            }

            app.UseStlCorrelationId();
            app.UseStlSecurityHeaders();
            configurePipeline?.Invoke(app);
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

            if (app.Environment.IsDevelopment()
                || app.Environment.IsProduction()
                || app.Environment.IsEnvironment("Testing"))
            {
                await ApplyMigrationsAsync<TContext>(app);
            }

            static IResult BuildLivenessResponse(ProductDescriptor descriptor, IServiceProvider services)
            {
                RecordHealthMetric(services, descriptor, "liveness");
                var response = new HealthResponse(
                    Status: "Healthy",
                    Product: descriptor.ProductKey,
                    Version: GetVersion(),
                    TimestampUtc: DateTimeOffset.UtcNow);
                return Results.Ok(response);
            }

            app.MapGet("/health", (ProductDescriptor descriptor, IServiceProvider services) =>
                BuildLivenessResponse(descriptor, services))
            .WithName("GetHealth")
            .WithTags("Health")
            .AllowAnonymous();

            app.MapGet("/api/v1/health", (ProductDescriptor descriptor, IServiceProvider services) =>
                BuildLivenessResponse(descriptor, services))
            .WithName("GetHealthV1")
            .WithTags("Health")
            .AllowAnonymous();

            var readyOptions = new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    RecordHealthMetric(context.RequestServices, product, "ready");
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
            };

            app.MapHealthChecks("/health/ready", readyOptions).AllowAnonymous();
            app.MapHealthChecks("/api/v1/health/ready", readyOptions).AllowAnonymous();

            app.MapGet("/health/observability", (ProductDescriptor descriptor, IConfiguration configuration) =>
            {
                var status = StlOpenTelemetryExtensions.BuildStatus(
                    configuration,
                    descriptor,
                    includeAspNetCoreInstrumentation: true);
                return Results.Ok(status);
            })
            .WithName("GetObservabilityHealth")
            .WithTags("Health")
            .AllowAnonymous();

            if (!hasBundledFrontend)
            {
                app.MapGet("/", (ProductDescriptor descriptor) => Results.Ok(new
                {
                    product = descriptor.DisplayName,
                    key = descriptor.ProductKey,
                    health = "/health",
                    ready = "/health/ready",
                    openapi = exposeOpenApi ? "/openapi/v1.json" : null
                })).AllowAnonymous();
            }

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

    private static void RecordHealthMetric(IServiceProvider services, ProductDescriptor descriptor, string endpoint)
    {
        var metrics = services.GetService<StlPlatformMetrics>();
        metrics?.RecordHealthRequest(endpoint, descriptor.ProductKey);
    }

    private static async Task ApplyMigrationsAsync<TContext>(WebApplication app)
        where TContext : PlatformDbContext
    {
        var connectionString = StlDatabaseConnection.Resolve(app.Configuration);

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
