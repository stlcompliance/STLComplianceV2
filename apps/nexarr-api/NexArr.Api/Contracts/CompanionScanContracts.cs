namespace NexArr.Api.Contracts;

public sealed record CompanionScanResolveRequest(
    string ScannedValue,
    string? Symbology = null);

public sealed record CompanionScanResolveResponse(
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

public static class CompanionScanOutcomes
{
    public const string Resolved = "resolved";
    public const string Denied = "denied";
}

public static class CompanionScanReasonCodes
{
    public const string InvalidPayload = "scan.invalid_payload";
    public const string NotEntitled = "scan.not_entitled";
    public const string NotInInbox = "scan.not_in_inbox";
}
