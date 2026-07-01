using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class SupplyContract : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string ContractKey { get; set; } = string.Empty;

    public string ContractType { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public Guid SupplierId { get; set; }

    public DateTimeOffset EffectiveAt { get; set; }

    public DateTimeOffset? ExpiresAt { get; set; }

    public DateTimeOffset? RenewalAt { get; set; }

    public string PaymentTerms { get; set; } = string.Empty;

    public string FreightTerms { get; set; } = string.Empty;

    public string WarrantyTerms { get; set; } = string.Empty;

    public decimal? MinimumSpend { get; set; }

    public string ServiceLevelAgreement { get; set; } = string.Empty;

    public string ApprovalStatus { get; set; } = SupplyContractApprovalStatuses.Draft;

    public string Status { get; set; } = SupplyContractStatuses.Draft;

    public string Notes { get; set; } = string.Empty;

    public Guid CreatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Supplier Supplier { get; set; } = null!;
}

public static class SupplyContractStatuses
{
    public const string Draft = "draft";
    public const string PendingReview = "pending_review";
    public const string Active = "active";
    public const string ExpiringSoon = "expiring_soon";
    public const string Expired = "expired";
    public const string Superseded = "superseded";
    public const string Cancelled = "cancelled";
    public const string Archived = "archived";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Draft,
        PendingReview,
        Active,
        ExpiringSoon,
        Expired,
        Superseded,
        Cancelled,
        Archived
    };
}

public static class SupplyContractApprovalStatuses
{
    public const string Draft = "draft";
    public const string PendingReview = "pending_review";
    public const string Approved = "approved";
    public const string Rejected = "rejected";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Draft,
        PendingReview,
        Approved,
        Rejected
    };
}

