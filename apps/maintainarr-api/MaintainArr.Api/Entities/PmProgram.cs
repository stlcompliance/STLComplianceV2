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

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public AssetType? AssetType { get; set; }

    public Asset? Asset { get; set; }

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
}

public static class PmProgramStatuses
{
    public const string Draft = "draft";
    public const string Active = "active";
    public const string Inactive = "inactive";
}
