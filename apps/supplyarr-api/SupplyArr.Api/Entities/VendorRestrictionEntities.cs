using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class VendorRestriction : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid ExternalPartyId { get; set; }

    public string RestrictionKey { get; set; } = string.Empty;

    public string ScopesJson { get; set; } = "[]";

    public string Reason { get; set; } = string.Empty;

    public string Status { get; set; } = VendorRestrictionStatuses.Active;

    public DateTimeOffset EffectiveFrom { get; set; }

    public DateTimeOffset? EffectiveUntil { get; set; }

    public Guid CreatedByUserId { get; set; }

    public Guid? LiftedByUserId { get; set; }

    public DateTimeOffset? LiftedAt { get; set; }

    public string? LiftNotes { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ExternalParty ExternalParty { get; set; } = null!;
}

public static class VendorRestrictionStatuses
{
    public const string Active = "active";

    public const string Lifted = "lifted";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Active,
        Lifted,
    };
}

public static class VendorRestrictionScopes
{
    public const string PurchaseRequests = "purchase_requests";

    public const string PurchaseOrders = "purchase_orders";

    public const string RfqInvitations = "rfq_invitations";

    public const string Receiving = "receiving";

    public const string AllProcurement = "all_procurement";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        PurchaseRequests,
        PurchaseOrders,
        RfqInvitations,
        Receiving,
        AllProcurement,
    };
}

public static class VendorRestrictionPartyTypes
{
    public static readonly IReadOnlySet<string> Allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "vendor",
        "supplier",
    };
}
