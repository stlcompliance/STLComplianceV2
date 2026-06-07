using STLCompliance.Shared.Data;

namespace MaintainArr.Api.Entities;

public sealed class PmProgram : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string ProgramKey { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string ScopeType { get; set; } = PmProgramScopeTypes.AssetType;

    public Guid? AssetTypeId { get; set; }

    public Guid? AssetId { get; set; }

    public string Status { get; set; } = PmProgramStatuses.Draft;

    public string? CategoryKey { get; set; }

    public string? WorkTypeKey { get; set; }

    public string? PriorityKey { get; set; }

    public string? OwningSiteRef { get; set; }

    public string? OwningTeamRef { get; set; }

    public string? OwningDepartmentRef { get; set; }

    public string? OwnerPersonId { get; set; }

    public string? OwnerRoleKey { get; set; }

    public string TagsJson { get; set; } = "[]";

    public string ScopeDefinitionJson { get; set; } = "{}";

    public string DueTriggerDefinitionJson { get; set; } = "{}";

    public string WorkPackageDefinitionJson { get; set; } = "{}";

    public string InspectionDefinitionJson { get; set; } = "{}";

    public string ComplianceDefinitionJson { get; set; } = "{}";

    public string AutomationDefinitionJson { get; set; } = "{}";

    public bool AutoGenerateWorkOrder { get; set; } = true;

    public string? DefaultWorkOrderTemplateRef { get; set; }

    public bool AutoGenerateInspection { get; set; }

    public Guid? InspectionTemplateId { get; set; }

    public string? CreatedByPersonId { get; set; }

    public string? UpdatedByPersonId { get; set; }

    public string? ActivatedByPersonId { get; set; }

    public DateTimeOffset? ActivatedAt { get; set; }

    public string? PausedByPersonId { get; set; }

    public DateTimeOffset? PausedAt { get; set; }

    public string? RetiredByPersonId { get; set; }

    public DateTimeOffset? RetiredAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public AssetType? AssetType { get; set; }

    public Asset? Asset { get; set; }

    public InspectionTemplate? InspectionTemplate { get; set; }

    public ICollection<PmProgramSchedule> ProgramSchedules { get; set; } = [];
}

public sealed class PmProgramSchedule
{
    public Guid PmProgramId { get; set; }

    public PmProgram PmProgram { get; set; } = null!;

    public Guid PmScheduleId { get; set; }

    public PmSchedule PmSchedule { get; set; } = null!;

    public int SortOrder { get; set; }
}

public static class PmProgramScopeTypes
{
    public const string AssetType = "asset_type";
    public const string Asset = "asset";
    public const string Custom = "custom";
}

public static class PmProgramStatuses
{
    public const string Draft = "draft";
    public const string Active = "active";
    public const string Paused = "paused";
    public const string Retired = "retired";
    public const string Inactive = "inactive";
}
