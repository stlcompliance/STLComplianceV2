namespace STLCompliance.Shared.Health;

public sealed record HealthResponse(
    string Status,
    string Product,
    string Version,
    DateTimeOffset TimestampUtc,
    IReadOnlyDictionary<string, object>? Checks = null);
