namespace StaffArr.Api.Contracts;

public sealed record GrantReadinessOverrideRequest(
    string Reason,
    DateTimeOffset? ExpiresAt);

public sealed record ReadinessOverrideResponse(
    Guid OverrideId,
    Guid PersonId,
    string Status,
    string Reason,
    DateTimeOffset GrantedAt,
    DateTimeOffset? ExpiresAt,
    Guid GrantedByUserId,
    DateTimeOffset? ClearedAt,
    Guid? ClearedByUserId);

public sealed record ReadinessOverrideSummaryResponse(
    Guid OverrideId,
    string Reason,
    DateTimeOffset GrantedAt,
    DateTimeOffset? ExpiresAt,
    Guid GrantedByUserId);
