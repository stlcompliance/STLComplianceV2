using STLCompliance.Shared.Data;

namespace MaintainArr.Api.Entities;

public sealed class Defect : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid AssetId { get; set; }

    public Guid? InspectionRunId { get; set; }

    public Guid? ChecklistItemId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Severity { get; set; } = DefectSeverities.Medium;

    public string Priority { get; set; } = "medium";

    public string? DefectType { get; set; }

    public string? ReportSource { get; set; }

    public string? SourceType { get; set; }

    public string? SourceReferenceId { get; set; }

    public string? IncidentReferenceId { get; set; }

    public string Status { get; set; } = DefectStatuses.Open;

    public string Source { get; set; } = DefectSources.Manual;

    public Guid ReportedByUserId { get; set; }

    public string? ReportedByPersonId { get; set; }

    public string? DiscoveredByPersonId { get; set; }

    public string? CreatedByPersonId { get; set; }

    public string? UpdatedByPersonId { get; set; }

    public DateTimeOffset? ReportedAt { get; set; }

    public DateTimeOffset? DiscoveredAt { get; set; }

    public bool IsSafetyCritical { get; set; }

    public bool IsComplianceImpacting { get; set; }

    public bool IsOperabilityImpacting { get; set; }

    public string? FailureMode { get; set; }

    public string? SystemKey { get; set; }

    public string? ComponentKey { get; set; }

    public string? Symptom { get; set; }

    public string? SidePosition { get; set; }

    public string? OperatingCondition { get; set; }

    public string? DeferralCode { get; set; }

    public string? ReadinessNotes { get; set; }

    public string? CorrectiveAction { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? ResolvedAt { get; set; }

    public DateTimeOffset? LastEscalatedAt { get; set; }

    public int EscalationCount { get; set; }

    public Asset Asset { get; set; } = null!;

    public InspectionRun? InspectionRun { get; set; }

    public InspectionChecklistItem? ChecklistItem { get; set; }

    public ICollection<DefectEvidence> Evidence { get; set; } = [];
}

public static class DefectStatuses
{
    public const string Draft = "draft";

    public const string Open = "open";

    public const string Acknowledged = "acknowledged";

    public const string InRepair = "in_repair";

    public const string Resolved = "resolved";

    public const string Closed = "closed";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Draft,
        Open,
        Acknowledged,
        InRepair,
        Resolved,
        Closed,
    };
}

public static class DefectSeverities
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

public static class DefectSources
{
    public const string Manual = "manual";

    public const string InspectionAuto = "inspection_auto";

    public const string InspectionManual = "inspection_manual";

    public const string RoutArr = "routarr";
}
