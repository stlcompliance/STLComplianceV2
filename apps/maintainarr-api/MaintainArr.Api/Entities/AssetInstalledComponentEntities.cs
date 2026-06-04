using STLCompliance.Shared.Data;

namespace MaintainArr.Api.Entities;

public sealed class AssetInstalledComponent : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string ComponentNumber { get; set; } = string.Empty;

    public Guid ParentAssetId { get; set; }

    public Guid? ParentComponentId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string ComponentType { get; set; } = "other";

    public string Status { get; set; } = "installed";

    public string? Make { get; set; }

    public string? Model { get; set; }

    public string? SerialNumber { get; set; }

    public string? PartNumberSnapshot { get; set; }

    public string? InstalledPartUsageRef { get; set; }

    public DateTimeOffset? InstallDate { get; set; }

    public string? InstalledByPersonId { get; set; }

    public decimal? InstalledMeterReading { get; set; }

    public DateTimeOffset? RemovedDate { get; set; }

    public string? RemovedByPersonId { get; set; }

    public decimal? RemovedMeterReading { get; set; }

    public string? RemovalReason { get; set; }

    public DateTimeOffset? WarrantyStartDate { get; set; }

    public DateTimeOffset? WarrantyEndDate { get; set; }

    public decimal? ExpectedLifeHours { get; set; }

    public decimal? ExpectedLifeMiles { get; set; }

    public int? ExpectedLifeCycles { get; set; }

    public string Condition { get; set; } = "unknown";

    public string ReplacementPartRefsJson { get; set; } = "[]";

    public string DocumentRefsJson { get; set; } = "[]";

    public string DefectRefsJson { get; set; } = "[]";

    public string WorkOrderRefsJson { get; set; } = "[]";

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Asset ParentAsset { get; set; } = null!;
}
