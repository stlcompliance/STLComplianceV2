namespace NexArr.Api.Contracts;

public sealed record FieldCompanionScanResolveRequest(
    string ScannedValue,
    string? Symbology = null);

public sealed record FieldCompanionScanResolveResponse(
    string Outcome,
    string? ReasonCode,
    string? ReasonMessage,
    string? TaskKey,
    string? ProductKey,
    string? TaskType,
    string? Title,
    string? Subtitle,
    string? Status,
    string? DeepLinkPath,
    string? DeepLinkUrl,
    string? BlockedReason);

public static class FieldCompanionScanOutcomes
{
    public const string Resolved = "resolved";
    public const string Denied = "denied";
}

public static class FieldCompanionScanReasonCodes
{
    public const string InvalidPayload = "scan.invalid_payload";
    public const string AccessUnavailable = "scan.not_available";
    public const string NotInInbox = "scan.not_in_inbox";
}
