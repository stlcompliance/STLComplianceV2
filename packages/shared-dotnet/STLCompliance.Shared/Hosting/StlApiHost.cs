using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Serilog;
using System.Net.Sockets;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Data;
using STLCompliance.Shared.Endpoints;
using STLCompliance.Shared.Health;
using STLCompliance.Shared.Integration;
using STLCompliance.Shared.Middleware;
using STLCompliance.Shared.Observability;
using STLCompliance.Shared.Print;

namespace STLCompliance.Shared.Hosting;

public static class StlApiHost
{
    internal const int MigrationStartupMaxAttempts = 8;

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
            builder.Services.AddScoped<PlatformDbContext>(sp => sp.GetRequiredService<TContext>());

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
            builder.Services.AddStlPrintRuntime();
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
                        spaApp.UseStlDocumentHeaders();
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
                await EnsurePrintExportLogStorageAsync<TContext>(app);
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

            app.MapGet("/api/v1/integration/product-response-framework", (ProductDescriptor descriptor) =>
                Results.Ok(StlProductResponseFrameworkContracts.Describe(
                    descriptor.ProductKey,
                    descriptor.DisplayName)))
            .WithName("GetProductResponseFrameworkContract")
            .WithTags("Integration")
            .AllowAnonymous();

            app.MapStlPrintEndpoints();

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

        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Migrations");

        for (var attempt = 1; attempt <= MigrationStartupMaxAttempts; attempt++)
        {
            await using var scope = app.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<TContext>();

            try
            {
                logger.LogInformation(
                    "Applying EF migrations for {Context} (attempt {Attempt}/{MaxAttempts})",
                    typeof(TContext).Name,
                    attempt,
                    MigrationStartupMaxAttempts);
                await db.Database.MigrateAsync();
                return;
            }
            catch (Exception ex) when (attempt < MigrationStartupMaxAttempts && IsTransientMigrationStartupException(ex))
            {
                var retryDelay = ComputeMigrationStartupRetryDelay(attempt);
                logger.LogWarning(
                    ex,
                    "EF migration attempt {Attempt}/{MaxAttempts} failed for {Context}. Retrying in {RetryDelaySeconds}s.",
                    attempt,
                    MigrationStartupMaxAttempts,
                    typeof(TContext).Name,
                    retryDelay.TotalSeconds);
                await Task.Delay(retryDelay);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "EF migration failed for {Context}", typeof(TContext).Name);
                throw;
            }
        }
    }

    private static async Task EnsurePrintExportLogStorageAsync<TContext>(WebApplication app)
        where TContext : PlatformDbContext
    {
        var connectionString = StlDatabaseConnection.Resolve(app.Configuration);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TContext>();
        if (!db.Database.IsRelational())
        {
            return;
        }

        const string sql = """
            CREATE TABLE IF NOT EXISTS print_export_logs (
                "Id" uuid NOT NULL,
                "TenantId" uuid NOT NULL,
                "ProductKey" character varying(64) NOT NULL,
                "SourceEntityType" character varying(128) NOT NULL,
                "SourceEntityId" character varying(256) NOT NULL,
                "SourceDisplayRef" character varying(256) NOT NULL,
                "TemplateKey" character varying(160) NOT NULL,
                "TemplateVersion" character varying(64) NOT NULL,
                "Action" character varying(32) NOT NULL,
                "DocumentStatus" character varying(32) NOT NULL,
                "RequestedByPersonId" uuid NOT NULL,
                "RequestedAtUtc" timestamp with time zone NOT NULL,
                "CompletedAtUtc" timestamp with time zone NULL,
                "RecordArrDocumentId" character varying(128) NULL,
                "FileName" character varying(256) NULL,
                "ContentHash" character varying(128) NULL,
                "ReprintReason" character varying(1024) NULL,
                "FailureReason" character varying(1024) NULL,
                "MetadataJson" jsonb NULL,
                CONSTRAINT "PK_print_export_logs" PRIMARY KEY ("Id")
            );

            CREATE INDEX IF NOT EXISTS "IX_print_export_logs_TenantId"
                ON print_export_logs ("TenantId");

            CREATE INDEX IF NOT EXISTS "IX_print_export_logs_lookup"
                ON print_export_logs ("TenantId", "ProductKey", "SourceEntityType", "SourceEntityId", "RequestedAtUtc");

            CREATE INDEX IF NOT EXISTS "IX_print_export_logs_action_lookup"
                ON print_export_logs ("TenantId", "ProductKey", "Action", "RequestedAtUtc");
            """;

        await db.Database.ExecuteSqlRawAsync(sql);
    }

    internal static TimeSpan ComputeMigrationStartupRetryDelay(int failedAttempt)
    {
        var seconds = Math.Min(2 * Math.Pow(2, Math.Max(0, failedAttempt - 1)), 30);
        return TimeSpan.FromSeconds(seconds);
    }

    internal static bool IsTransientMigrationStartupException(Exception exception)
    {
        return exception switch
        {
            TimeoutException => true,
            SocketException socketException => IsTransientStartupSocketError(socketException.SocketErrorCode),
            NpgsqlException npgsqlException => npgsqlException.IsTransient
                                               || IsTransientStartupMessage(npgsqlException.Message)
                                               || (npgsqlException.InnerException is not null
                                                   && IsTransientMigrationStartupException(npgsqlException.InnerException)),
            _ when exception.InnerException is not null => IsTransientMigrationStartupException(exception.InnerException),
            _ => false
        };
    }

    private static bool IsTransientStartupSocketError(SocketError socketErrorCode)
    {
        return socketErrorCode is SocketError.HostNotFound
            or SocketError.TryAgain
            or SocketError.TimedOut
            or SocketError.NetworkUnreachable
            or SocketError.HostUnreachable;
    }

    private static bool IsTransientStartupMessage(string? message)
    {
        return !string.IsNullOrWhiteSpace(message)
               && (message.Contains("Name or service not known", StringComparison.OrdinalIgnoreCase)
                   || message.Contains("Temporary failure in name resolution", StringComparison.OrdinalIgnoreCase)
                   || message.Contains("No such host is known", StringComparison.OrdinalIgnoreCase)
                   || message.Contains("nodename nor servname provided", StringComparison.OrdinalIgnoreCase));
    }
}
