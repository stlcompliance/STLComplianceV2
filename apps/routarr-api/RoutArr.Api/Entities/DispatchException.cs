using STLCompliance.Shared.Data;

namespace RoutArr.Api.Entities;

public sealed class DispatchException : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string ExceptionKey { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = DispatchExceptionCategories.Other;

    public string Status { get; set; } = DispatchExceptionStatuses.Open;

    public string IncidentType { get; set; } = DispatchIncidentTypes.OperationalException;

    public string IncidentSeverity { get; set; } = DispatchIncidentSeverities.Medium;

    public string IncidentReviewStatus { get; set; } = DispatchIncidentReviewStatuses.Open;

    public string IncidentRoutedProduct { get; set; } = DispatchIncidentRoutedProducts.RoutArr;

    public Guid? StaffarrPersonnelIncidentId { get; set; }

    public DateTimeOffset? StaffarrIncidentRoutedAt { get; set; }

    public string StaffarrIncidentRouteStatus { get; set; } = string.Empty;

    public Guid? TrainarrIncidentRemediationId { get; set; }

    public DateTimeOffset? TrainarrIncidentRoutedAt { get; set; }

    public string TrainarrIncidentRouteStatus { get; set; } = string.Empty;

    public Guid? MaintainarrInboundEventId { get; set; }

    public Guid? MaintainarrDefectId { get; set; }

    public DateTimeOffset? MaintainarrIncidentRoutedAt { get; set; }

    public string MaintainarrIncidentRouteStatus { get; set; } = string.Empty;

    public Guid? CompliancecoreFactPublicationId { get; set; }

    public DateTimeOffset? CompliancecoreIncidentRoutedAt { get; set; }

    public string CompliancecoreIncidentRouteStatus { get; set; } = string.Empty;

    public Guid? TripId { get; set; }

    public Guid? AssignedToUserId { get; set; }

    public DateTimeOffset? SlaDueAt { get; set; }

    public string ResolutionTemplateKey { get; set; } = string.Empty;

    public string ResolutionNotes { get; set; } = string.Empty;

    public Guid CreatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? AssignedAt { get; set; }

    public Guid? ResolvedByUserId { get; set; }

    public DateTimeOffset? ResolvedAt { get; set; }
}

public static class DispatchExceptionStatuses
{
    public const string Open = "open";

    public const string Assigned = "assigned";

    public const string Resolved = "resolved";

    public const string Cancelled = "cancelled";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Open,
        Assigned,
        Resolved,
        Cancelled,
    };

    public static readonly IReadOnlySet<string> OpenQueue = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Open,
        Assigned,
    };

    public static bool IsTerminal(string status) =>
        string.Equals(status, Resolved, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, Cancelled, StringComparison.OrdinalIgnoreCase);
}

public static class DispatchExceptionCategories
{
    public const string Delay = "delay";

    public const string Driver = "driver";

    public const string Vehicle = "vehicle";

    public const string Route = "route";

    public const string Stop = "stop";

    public const string Compliance = "compliance";

    public const string Other = "other";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Delay,
        Driver,
        Vehicle,
        Route,
        Stop,
        Compliance,
        Other,
    };
}

public static class DispatchIncidentTypes
{
    public const string OperationalException = "operational_exception";

    public const string Accident = "accident";

    public const string NearMiss = "near_miss";

    public const string Injury = "injury";

    public const string PropertyDamage = "property_damage";

    public const string CargoDamage = "cargo_damage";

    public const string EquipmentAbuse = "equipment_abuse";

    public const string SafetyConcern = "safety_concern";

    public const string CustomerComplaint = "customer_complaint";

    public const string TrainingRelated = "training_related";

    public const string ComplianceRelated = "compliance_related";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        OperationalException,
        Accident,
        NearMiss,
        Injury,
        PropertyDamage,
        CargoDamage,
        EquipmentAbuse,
        SafetyConcern,
        CustomerComplaint,
        TrainingRelated,
        ComplianceRelated,
    };
}

public static class DispatchIncidentSeverities
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
}

public static class DispatchIncidentReviewStatuses
{
    public const string Open = "open";

    public const string UnderReview = "under_review";

    public const string Routed = "routed";

    public const string Closed = "closed";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Open,
        UnderReview,
        Routed,
        Closed,
    };
}

public static class DispatchIncidentRoutedProducts
{
    public const string RoutArr = "routarr";

    public const string MaintainArr = "maintainarr";

    public const string StaffArr = "staffarr";

    public const string TrainArr = "trainarr";

    public const string ComplianceCore = "compliancecore";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        RoutArr,
        MaintainArr,
        StaffArr,
        TrainArr,
        ComplianceCore,
    };
}

public static class DispatchExceptionResolutionTemplates
{
    public const string ReassignDriver = "reassign_driver";

    public const string RescheduleDeparture = "reschedule_departure";

    public const string SwapVehicle = "swap_vehicle";

    public const string RouteReplan = "route_replan";

    public const string EscalateLeadDispatcher = "escalate_lead_dispatcher";

    public static readonly IReadOnlyList<DispatchExceptionResolutionTemplateDefinition> All =
    [
        new(ReassignDriver, "Reassign driver", "Assign a qualified replacement driver and notify the field team."),
        new(RescheduleDeparture, "Reschedule departure", "Adjust the planned departure window and update dispatch board."),
        new(SwapVehicle, "Swap vehicle", "Move the trip to an alternate dispatchable vehicle."),
        new(RouteReplan, "Route replan", "Revise route/stop sequence to recover service level."),
        new(EscalateLeadDispatcher, "Escalate to lead dispatcher", "Escalate to lead dispatcher for decision within SLA."),
    ];

    public static readonly IReadOnlySet<string> Keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ReassignDriver,
        RescheduleDeparture,
        SwapVehicle,
        RouteReplan,
        EscalateLeadDispatcher,
    };
}

public sealed record DispatchExceptionResolutionTemplateDefinition(
    string TemplateKey,
    string Label,
    string DefaultResolutionNotes);
