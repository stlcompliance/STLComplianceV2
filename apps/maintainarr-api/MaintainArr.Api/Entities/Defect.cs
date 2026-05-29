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



    public string Status { get; set; } = DefectStatuses.Open;



    public string Source { get; set; } = DefectSources.Manual;



    public Guid ReportedByUserId { get; set; }



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

    public const string Open = "open";



    public const string Acknowledged = "acknowledged";



    public const string InRepair = "in_repair";



    public const string Resolved = "resolved";



    public const string Closed = "closed";



    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)

    {

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

}

