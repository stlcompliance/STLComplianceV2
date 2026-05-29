using STLCompliance.Shared.Data;

namespace ComplianceCore.Api.Entities;

public sealed class ComplianceWaiver : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string WaiverKey { get; set; } = string.Empty;

    public Guid RulePackId { get; set; }

    public string PackKey { get; set; } = string.Empty;

    public string? RuleKey { get; set; }

    public string? GateKey { get; set; }

    public string SubjectScopeKey { get; set; } = "tenant";

    public string ReasonCode { get; set; } = string.Empty;

    public string Explanation { get; set; } = string.Empty;

    public string Status { get; set; } = WaiverStatuses.Pending;

    public DateTimeOffset EffectiveAt { get; set; }

    public DateTimeOffset? ExpiresAt { get; set; }

    public Guid? CreatedByUserId { get; set; }

    public Guid? ApprovedByUserId { get; set; }

    public DateTimeOffset? ApprovedAt { get; set; }

    public Guid? RevokedByUserId { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public RulePack? RulePack { get; set; }
}

public static class WaiverStatuses
{
    public const string Pending = "pending";

    public const string Approved = "approved";

    public const string Rejected = "rejected";

    public const string Revoked = "revoked";

    public const string Expired = "expired";

    public static readonly IReadOnlySet<string> Terminal = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Rejected,
        Revoked,
        Expired,
    };
}
