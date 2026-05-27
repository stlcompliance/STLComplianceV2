namespace STLCompliance.Shared.Observability;

public sealed record StlObservabilityStatus(
    bool OtelEnabled,
    string ServiceName,
    string Exporter,
    bool OtlpEndpointConfigured,
    IReadOnlyList<string> Meters,
    IReadOnlyList<string> ActivitySources);
