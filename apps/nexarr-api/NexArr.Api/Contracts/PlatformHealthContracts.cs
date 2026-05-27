using STLCompliance.Shared.Health;

namespace NexArr.Api.Contracts;

public sealed record PlatformHealthResponse(
    string Status,
    DateTimeOffset TimestampUtc,
    IReadOnlyList<ProductHealthProbeResult> Products);

public sealed record ProductHealthProbeResult(
    string ProductKey,
    string Status,
    string? ReadyUrl,
    double? LatencyMs,
    string? ErrorCode,
    string? ErrorMessage,
    HealthResponse? Detail);
