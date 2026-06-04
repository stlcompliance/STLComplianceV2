using Microsoft.Extensions.Configuration;
using STLCompliance.Shared.Observability;

namespace STLCompliance.Otel.Tests;

[Trait("Category", "Otel")]
public sealed class StlOpenTelemetryOptionsTests
{
    [Theory]
    [InlineData("true", true)]
    [InlineData("TRUE", true)]
    [InlineData("1", true)]
    [InlineData("yes", true)]
    [InlineData("false", false)]
    [InlineData(null, false)]
    public void IsEnabled_parses_common_truthy_values(string? raw, bool expected)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(raw is null
                ? null
                : new Dictionary<string, string?> { [StlOpenTelemetryOptions.EnabledKey] = raw })
            .Build();

        Assert.Equal(expected, StlOpenTelemetryOptions.IsEnabled(configuration));
    }

    [Fact]
    public void FromConfiguration_uses_default_service_name_when_unset()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [StlOpenTelemetryOptions.EnabledKey] = "true",
                [StlOpenTelemetryOptions.OtlpEndpointKey] = "http://otel-collector:4317"
            })
            .Build();

        var options = StlOpenTelemetryOptions.FromConfiguration(configuration, "staffarr");

        Assert.True(options.Enabled);
        Assert.Equal("staffarr", options.ServiceName);
        Assert.Equal("http://otel-collector:4317", options.OtlpEndpoint);
    }

    [Fact]
    public void FromConfiguration_honors_explicit_service_name()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [StlOpenTelemetryOptions.EnabledKey] = "true",
                [StlOpenTelemetryOptions.ServiceNameKey] = "stl-staffarr-api"
            })
            .Build();

        var options = StlOpenTelemetryOptions.FromConfiguration(configuration, "staffarr");

        Assert.Equal("stl-staffarr-api", options.ServiceName);
    }

    [Fact]
    public void FromConfiguration_honors_assurarr_service_name()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [StlOpenTelemetryOptions.EnabledKey] = "true",
                [StlOpenTelemetryOptions.ServiceNameKey] = "stl-assurarr-api"
            })
            .Build();

        var options = StlOpenTelemetryOptions.FromConfiguration(configuration, "assurarr");

        Assert.Equal("stl-assurarr-api", options.ServiceName);
    }
}
