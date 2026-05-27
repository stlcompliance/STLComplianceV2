using Microsoft.Extensions.Configuration;
using STLCompliance.Shared.Hosting;
using STLCompliance.Shared.Observability;

namespace STLCompliance.Otel.Tests;

[Trait("Category", "Otel")]
public sealed class OtelWorkerStatusTests
{
    [Fact]
    public void Worker_observability_status_reports_runtime_meters_when_enabled()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [StlOpenTelemetryOptions.EnabledKey] = "true",
                [StlOpenTelemetryOptions.ServiceNameKey] = "stl-shared-worker"
            })
            .Build();

        var product = new ProductDescriptor("shared-worker", "STL Shared Worker", 0);
        var status = StlOpenTelemetryExtensions.BuildStatus(
            configuration,
            product,
            includeAspNetCoreInstrumentation: false);

        Assert.True(status.OtelEnabled);
        Assert.Equal("stl-shared-worker", status.ServiceName);
        Assert.Equal("console", status.Exporter);
        Assert.Contains(StlPlatformMetrics.MeterName, status.Meters);
        Assert.DoesNotContain("Microsoft.AspNetCore.Hosting", status.Meters);
    }
}
