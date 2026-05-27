using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using STLCompliance.Shared.Hosting;
using STLCompliance.Shared.Integration;

namespace STLCompliance.Shared.Workers;

public static class StlWorkerHost
{
    public static async Task RunAsync(
        ProductDescriptor product,
        string[] args,
        Action<HostApplicationBuilder>? configure = null)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("product", product.ProductKey)
            .CreateLogger();

        try
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddSingleton(product);
            builder.Services.AddHostedService<HeartbeatWorker>();
            builder.AddStlIntegrationTokenProvisioning();
            configure?.Invoke(builder);

            builder.Services.AddSerilog();

            var host = builder.Build();
            Log.Information("Starting {Product} worker ({Key})", product.DisplayName, product.ProductKey);
            await host.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "{Product} worker terminated unexpectedly", product.DisplayName);
            throw;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}

internal sealed class HeartbeatWorker(
    ProductDescriptor product,
    ILogger<HeartbeatWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation(
                "{Product} worker heartbeat at {TimestampUtc}",
                product.ProductKey,
                DateTimeOffset.UtcNow);
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
