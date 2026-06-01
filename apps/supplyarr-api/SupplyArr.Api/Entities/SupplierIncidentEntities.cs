using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class SupplierIncident : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid ExternalPartyId { get; set; }

    public string IncidentKey { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string IncidentType { get; set; } = SupplierIncidentTypes.Quality;

    public string Severity { get; set; } = SupplierIncidentSeverities.Medium;

    public string Status { get; set; } = SupplierIncidentStatuses.Open;

    public Guid? PurchaseRequestId { get; set; }

    public Guid? PurchaseOrderId { get; set; }

    public Guid? ReceivingReceiptId { get; set; }

    public Guid? ReceivingExceptionId { get; set; }

    public Guid? VendorRestrictionId { get; set; }

    public Guid ReportedByUserId { get; set; }

    public Guid? AssignedToUserId { get; set; }

    public Guid? InvolvedStaffarrPersonId { get; set; }

    public Guid? StaffarrPersonnelIncidentId { get; set; }

    public DateTimeOffset? StaffarrIncidentRoutedAt { get; set; }

    public string StaffarrIncidentRouteStatus { get; set; } = string.Empty;

    public Guid? TrainarrIncidentRemediationId { get; set; }

    public DateTimeOffset? TrainarrIncidentRoutedAt { get; set; }

    public string TrainarrIncidentRouteStatus { get; set; } = string.Empty;

    public string ResolutionNotes { get; set; } = string.Empty;

    public Guid? ResolvedByUserId { get; set; }

    public DateTimeOffset? ResolvedAt { get; set; }

    public Guid? ClosedByUserId { get; set; }

    public DateTimeOffset? ClosedAt { get; set; }

    public Guid? CancelledByUserId { get; set; }

    public DateTimeOffset? CancelledAt { get; set; }

    public string CancellationReason { get; set; } = string.Empty;

    public Guid? ReopenedByUserId { get; set; }

    public DateTimeOffset? ReopenedAt { get; set; }

    public string LastReopenReason { get; set; } = string.Empty;

    public int ReopenCount { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ExternalParty ExternalParty { get; set; } = null!;

    public VendorRestriction? VendorRestriction { get; set; }
}

public static class SupplierIncidentStatuses
{
    public const string Open = "open";

    public const string Investigating = "investigating";

    public const string Resolved = "resolved";

    public const string Closed = "closed";

    public const string Cancelled = "cancelled";

    public static readonly string[] Active =
    [
        Open,
        Investigating,
        Resolved,
    ];

    public static readonly IReadOnlySet<string> Editable = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Open,
        Investigating,
    };
}

public static class SupplierIncidentTypes
{
    public const string Quality = "quality";

    public const string Delivery = "delivery";

    public const string Compliance = "compliance";

    public const string Safety = "safety";

    public const string Other = "other";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Quality,
        Delivery,
        Compliance,
        Safety,
        Other,
    };
}

public static class SupplierIncidentSeverities
{
    public const string Low = "low";

    public const string Medium = "medium";

    public const string High = "high";

    public const string Critical = "critical";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Low,
        Medium,
        High,
        Critical,
    };

    public static readonly IReadOnlySet<string> RestrictionRecommended = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        High,
        Critical,
    };
}
