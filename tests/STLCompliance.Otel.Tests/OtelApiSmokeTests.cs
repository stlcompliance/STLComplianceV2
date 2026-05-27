using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using STLCompliance.Shared.Observability;

namespace STLCompliance.Otel.Tests;

[Trait("Category", "Otel")]
public sealed class NexArrOtelSmokeTests(WebApplicationFactory<NexArr.Api.Program> factory)
    : OtelApiSmokeTests<NexArr.Api.Program>(factory, "nexarr");

[Trait("Category", "Otel")]
public sealed class StaffArrOtelSmokeTests(WebApplicationFactory<StaffArr.Api.Program> factory)
    : OtelApiSmokeTests<StaffArr.Api.Program>(factory, "staffarr");

[Trait("Category", "Otel")]
public sealed class TrainArrOtelSmokeTests(WebApplicationFactory<TrainArr.Api.Program> factory)
    : OtelApiSmokeTests<TrainArr.Api.Program>(factory, "trainarr");

[Trait("Category", "Otel")]
public sealed class MaintainArrOtelSmokeTests(WebApplicationFactory<MaintainArr.Api.Program> factory)
    : OtelApiSmokeTests<MaintainArr.Api.Program>(factory, "maintainarr");

[Trait("Category", "Otel")]
public sealed class RoutArrOtelSmokeTests(WebApplicationFactory<RoutArr.Api.Program> factory)
    : OtelApiSmokeTests<RoutArr.Api.Program>(factory, "routarr");

[Trait("Category", "Otel")]
public sealed class SupplyArrOtelSmokeTests(WebApplicationFactory<SupplyArr.Api.Program> factory)
    : OtelApiSmokeTests<SupplyArr.Api.Program>(factory, "supplyarr");

[Trait("Category", "Otel")]
public sealed class ComplianceCoreOtelSmokeTests(WebApplicationFactory<ComplianceCore.Api.Program> factory)
    : OtelApiSmokeTests<ComplianceCore.Api.Program>(factory, "compliancecore");

public abstract class OtelApiSmokeTests<TProgram>(
    WebApplicationFactory<TProgram> factory,
    string expectedProductKey) : IClassFixture<WebApplicationFactory<TProgram>>
    where TProgram : class
{
    [Fact]
    public async Task Observability_endpoint_reports_disabled_when_otel_off()
    {
        await using var host = CreateHost(otelEnabled: false);
        var response = await host.Client.GetAsync("/health/observability");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var status = await response.Content.ReadFromJsonAsync<StlObservabilityStatus>();
        Assert.NotNull(status);
        Assert.False(status.OtelEnabled);
        Assert.Equal(expectedProductKey, status.ServiceName);
        Assert.Equal("none", status.Exporter);
    }

    [Fact]
    public async Task Observability_endpoint_reports_otlp_when_enabled()
    {
        await using var host = CreateHost(
            otelEnabled: true,
            otlpEndpoint: "http://127.0.0.1:4317",
            serviceName: $"stl-{expectedProductKey}-api");

        var response = await host.Client.GetAsync("/health/observability");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var status = await response.Content.ReadFromJsonAsync<StlObservabilityStatus>();
        Assert.NotNull(status);
        Assert.True(status.OtelEnabled);
        Assert.Equal($"stl-{expectedProductKey}-api", status.ServiceName);
        Assert.Equal("otlp", status.Exporter);
        Assert.True(status.OtlpEndpointConfigured);
        Assert.Contains(StlPlatformMetrics.MeterName, status.Meters);
    }

    [Fact]
    public async Task Health_liveness_emits_platform_metric_when_otel_enabled()
    {
        using var listener = new MeterListener();
        long observed = 0;

        listener.InstrumentPublished = (instrument, meterListener) =>
        {
            if (instrument.Meter.Name == StlPlatformMetrics.MeterName
                && instrument.Name == "stl.health.requests")
            {
                meterListener.EnableMeasurementEvents(instrument);
            }
        };

        listener.SetMeasurementEventCallback<long>((instrument, measurement, _, _) =>
        {
            if (instrument.Name == "stl.health.requests")
            {
                Interlocked.Add(ref observed, measurement);
            }
        });

        listener.Start();

        await using var host = CreateHost(otelEnabled: true, otlpEndpoint: "http://127.0.0.1:4317");

        var response = await host.Client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(observed >= 1, "Expected stl.health.requests counter increment after /health probe.");
    }

    [Fact]
    public async Task Health_liveness_succeeds_when_otel_enabled()
    {
        await using var host = CreateHost(otelEnabled: true, otlpEndpoint: "http://127.0.0.1:4317");
        var response = await host.Client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private OtelTestHost CreateHost(bool otelEnabled, string? otlpEndpoint = null, string? serviceName = null)
    {
        var settings = new Dictionary<string, string?>
        {
            ["ConnectionStrings:Database"] = string.Empty,
            ["DATABASE_URL"] = string.Empty,
            ["Auth:SigningKey"] = "test-signing-key-at-least-32-chars-long",
            [StlOpenTelemetryOptions.EnabledKey] = otelEnabled ? "true" : "false"
        };

        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            settings[StlOpenTelemetryOptions.OtlpEndpointKey] = otlpEndpoint;
        }

        if (!string.IsNullOrWhiteSpace(serviceName))
        {
            settings[StlOpenTelemetryOptions.ServiceNameKey] = serviceName;
        }

        return new OtelTestHost(factory, settings);
    }

    private sealed class OtelTestHost : IAsyncDisposable
    {
        private readonly WebApplicationFactory<TProgram> _factory;

        public OtelTestHost(WebApplicationFactory<TProgram> factory, IReadOnlyDictionary<string, string?> settings)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                foreach (var (key, value) in settings)
                {
                    builder.UseSetting(key, value);
                }
            });
            Client = _factory.CreateClient();
        }

        public HttpClient Client { get; }

        public ValueTask DisposeAsync() => _factory.DisposeAsync();
    }
}
