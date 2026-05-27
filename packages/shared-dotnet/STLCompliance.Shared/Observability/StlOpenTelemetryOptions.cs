using Microsoft.Extensions.Configuration;

namespace STLCompliance.Shared.Observability;

public sealed class StlOpenTelemetryOptions
{
    public const string EnabledKey = "OTEL_ENABLED";
    public const string ServiceNameKey = "OTEL_SERVICE_NAME";
    public const string OtlpEndpointKey = "OTEL_EXPORTER_OTLP_ENDPOINT";

    public bool Enabled { get; init; }
    public string ServiceName { get; init; } = string.Empty;
    public string? OtlpEndpoint { get; init; }

    public static StlOpenTelemetryOptions FromConfiguration(IConfiguration configuration, string defaultServiceName)
    {
        var enabled = ParseEnabled(configuration[EnabledKey]);
        var serviceName = configuration[ServiceNameKey];
        if (string.IsNullOrWhiteSpace(serviceName))
        {
            serviceName = defaultServiceName;
        }

        var otlpEndpoint = configuration[OtlpEndpointKey];
        if (string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            otlpEndpoint = null;
        }

        return new StlOpenTelemetryOptions
        {
            Enabled = enabled,
            ServiceName = serviceName,
            OtlpEndpoint = otlpEndpoint
        };
    }

    public static bool IsEnabled(IConfiguration configuration) =>
        ParseEnabled(configuration[EnabledKey]);

    private static bool ParseEnabled(string? raw) =>
        string.Equals(raw, "true", StringComparison.OrdinalIgnoreCase)
        || string.Equals(raw, "1", StringComparison.OrdinalIgnoreCase)
        || string.Equals(raw, "yes", StringComparison.OrdinalIgnoreCase);
}
