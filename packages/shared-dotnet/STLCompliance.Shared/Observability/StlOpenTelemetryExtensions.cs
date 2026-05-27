using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using STLCompliance.Shared.Hosting;

namespace STLCompliance.Shared.Observability;

public static class StlOpenTelemetryExtensions
{
    public static void AddStlOpenTelemetry(this WebApplicationBuilder builder, ProductDescriptor product)
    {
        var options = StlOpenTelemetryOptions.FromConfiguration(builder.Configuration, product.ProductKey);
        if (!options.Enabled)
        {
            return;
        }

        builder.Services.AddSingleton(options);
        builder.Services.AddSingleton<StlPlatformMetrics>();

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(options.ServiceName))
            .WithTracing(tracing => ConfigureTracing(tracing, builder.Configuration, builder.Environment))
            .WithMetrics(metrics => ConfigureMetrics(metrics, builder.Configuration, builder.Environment));
    }

    public static void AddStlOpenTelemetry(this HostApplicationBuilder builder, ProductDescriptor product)
    {
        var options = StlOpenTelemetryOptions.FromConfiguration(builder.Configuration, product.ProductKey);
        if (!options.Enabled)
        {
            return;
        }

        builder.Services.AddSingleton(options);
        builder.Services.AddSingleton<StlPlatformMetrics>();

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(options.ServiceName))
            .WithTracing(tracing => ConfigureTracing(tracing, builder.Configuration, builder.Environment))
            .WithMetrics(metrics => ConfigureWorkerMetrics(metrics, builder.Configuration, builder.Environment));
    }

    public static StlObservabilityStatus BuildStatus(
        IConfiguration configuration,
        ProductDescriptor product,
        bool includeAspNetCoreInstrumentation)
    {
        var options = StlOpenTelemetryOptions.FromConfiguration(configuration, product.ProductKey);
        if (!options.Enabled)
        {
            return new StlObservabilityStatus(
                OtelEnabled: false,
                ServiceName: product.ProductKey,
                Exporter: "none",
                OtlpEndpointConfigured: false,
                Meters: [],
                ActivitySources: []);
        }

        var exporter = ResolveExporterLabel(options);
        var meters = new List<string> { StlPlatformMetrics.MeterName };
        if (includeAspNetCoreInstrumentation)
        {
            meters.Add("Microsoft.AspNetCore.Hosting");
        }

        var sources = new List<string> { "STLCompliance.Platform" };
        if (includeAspNetCoreInstrumentation)
        {
            sources.Add("Microsoft.AspNetCore");
        }

        return new StlObservabilityStatus(
            OtelEnabled: true,
            ServiceName: options.ServiceName,
            Exporter: exporter,
            OtlpEndpointConfigured: !string.IsNullOrWhiteSpace(options.OtlpEndpoint),
            Meters: meters,
            ActivitySources: sources);
    }

    private static void ConfigureTracing(
        TracerProviderBuilder tracing,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSource("STLCompliance.Platform");

        AddExporters(tracing, configuration, environment);
    }

    private static void ConfigureMetrics(
        MeterProviderBuilder metrics,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddMeter(StlPlatformMetrics.MeterName);

        AddExporters(metrics, configuration, environment);
    }

    private static void ConfigureWorkerMetrics(
        MeterProviderBuilder metrics,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        metrics
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddMeter(StlPlatformMetrics.MeterName);

        AddExporters(metrics, configuration, environment);
    }

    private static void AddExporters(
        TracerProviderBuilder tracing,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var endpoint = configuration[StlOpenTelemetryOptions.OtlpEndpointKey];
        if (!string.IsNullOrWhiteSpace(endpoint))
        {
            tracing.AddOtlpExporter(options => options.Endpoint = new Uri(endpoint));
            return;
        }

        if (environment.IsDevelopment() || environment.IsEnvironment("Testing"))
        {
            tracing.AddConsoleExporter();
        }
    }

    private static void AddExporters(
        MeterProviderBuilder metrics,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var endpoint = configuration[StlOpenTelemetryOptions.OtlpEndpointKey];
        if (!string.IsNullOrWhiteSpace(endpoint))
        {
            metrics.AddOtlpExporter(options => options.Endpoint = new Uri(endpoint));
            return;
        }

        if (environment.IsDevelopment() || environment.IsEnvironment("Testing"))
        {
            metrics.AddConsoleExporter();
        }
    }

    private static string ResolveExporterLabel(StlOpenTelemetryOptions options) =>
        !string.IsNullOrWhiteSpace(options.OtlpEndpoint) ? "otlp" : "console";
}
