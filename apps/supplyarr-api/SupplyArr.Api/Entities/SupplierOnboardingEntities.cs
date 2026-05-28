using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class PartySupplierOnboarding : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid ExternalPartyId { get; set; }

    public string OnboardingStatus { get; set; } = SupplierOnboardingStatuses.Draft;

    public string Notes { get; set; } = string.Empty;

    public DateTimeOffset? SubmittedAt { get; set; }

    public Guid? SubmittedByUserId { get; set; }

    public DateTimeOffset? ReviewedAt { get; set; }

    public Guid? ReviewedByUserId { get; set; }

    public string RejectionReason { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ExternalParty ExternalParty { get; set; } = null!;
}

public sealed class TenantSupplierOnboardingSettings : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string RequiredDocumentTypeKeysJson { get; set; } = "[]";

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public static class SupplierOnboardingStatuses
{
    public const string Draft = "draft";

    public const string PendingReview = "pending_review";

    public const string Approved = "approved";

    public const string Rejected = "rejected";

    public const string Suspended = "suspended";

    public static readonly HashSet<string> Editable = new(StringComparer.OrdinalIgnoreCase)
    {
        Draft,
        Rejected,
    };

    public static readonly HashSet<string> Reviewable = new(StringComparer.OrdinalIgnoreCase)
    {
        PendingReview,
    };
}

public static class SupplierOnboardingPartyTypes
{
    public static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase)
    {
        "vendor",
        "supplier",
    };
}
