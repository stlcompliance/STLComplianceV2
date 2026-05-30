namespace ComplianceCore.Api.Contracts;

public sealed record ComplianceWaiverResponse(
    Guid WaiverId,
    string WaiverKey,
    Guid RulePackId,
    string PackKey,
    string? RuleKey,
    string? GateKey,
    string SubjectScopeKey,
    string ReasonCode,
    string Explanation,
    string Status,
    DateTimeOffset EffectiveAt,
    DateTimeOffset? ExpiresAt,
    Guid? CreatedByUserId,
    Guid? ApprovedByUserId,
    DateTimeOffset? ApprovedAt,
    Guid? RevokedByUserId,
    DateTimeOffset? RevokedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateComplianceWaiverRequest(
    string WaiverKey,
    Guid RulePackId,
    string SubjectScopeKey,
    string ReasonCode,
    string Explanation,
    DateTimeOffset EffectiveAt,
    DateTimeOffset? ExpiresAt,
    string? RuleKey = null,
    string? GateKey = null);

public sealed record RejectComplianceWaiverRequest(string? Notes = null);

public sealed record RevokeComplianceWaiverRequest(string? Notes = null);

public sealed record RenewComplianceWaiverRequest(
    DateTimeOffset EffectiveAt,
    DateTimeOffset? ExpiresAt,
    string? Notes = null);

public sealed record UpdateComplianceWaiverRequest(
    string Status,
    DateTimeOffset? EffectiveAt = null,
    DateTimeOffset? ExpiresAt = null,
    string? Notes = null);

public sealed record ProcessExpiredWaiversRequest(
    Guid? TenantId = null,
    DateTimeOffset? AsOfUtc = null,
    int? BatchSize = null);

public sealed record ProcessExpiredWaiversResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int ExpiredCount,
    IReadOnlyList<string> ExpiredWaiverKeys);
