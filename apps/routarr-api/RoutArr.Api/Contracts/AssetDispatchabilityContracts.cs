namespace RoutArr.Api.Contracts;

public sealed record AssetDispatchabilityCheckRequest(
    string? VehicleRefKey = null,
    string? AssetTag = null);

public sealed record AssetDispatchabilityMaintainArrSummary(
    Guid AssetId,
    string AssetTag,
    string ReadinessStatus,
    string ReadinessBasis,
    int BlockerCount,
    string? PrimaryBlockerMessage);

public sealed record AssetDispatchabilityCheckResponse(
    string? VehicleRefKey,
    string? AssetTag,
    string Outcome,
    string ReasonCode,
    string Message,
    bool IsBlocking,
    AssetDispatchabilityMaintainArrSummary? MaintainArr);

public sealed record DispatchAssignmentDispatchabilitySummary(
    string Outcome,
    string ReasonCode,
    string Message,
    bool IsBlocking,
    AssetDispatchabilityMaintainArrSummary? MaintainArr);

public static class AssetDispatchabilityOutcomes
{
    public const string Allow = "allow";

    public const string Warn = "warn";

    public const string Block = "block";
}
