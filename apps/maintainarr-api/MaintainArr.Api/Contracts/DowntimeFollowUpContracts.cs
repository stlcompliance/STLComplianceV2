namespace MaintainArr.Api.Contracts;

public sealed record DowntimeFollowUpResponse(
    Guid EventId,
    Guid AssetId,
    string DeepLinkPath,
    string Reason,
    string Trigger);
