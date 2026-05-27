using STLCompliance.Shared.Data;

namespace MaintainArr.Api.Entities;

public sealed class InspectionTemplate : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string TemplateKey { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Version { get; set; } = 1;

    public string Status { get; set; } = InspectionTemplateStatuses.Draft;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<InspectionTemplateCategory> Categories { get; set; } = [];

    public ICollection<InspectionChecklistItem> ChecklistItems { get; set; } = [];

    public ICollection<InspectionTemplateAssetType> AssetTypeLinks { get; set; } = [];
}

public static class InspectionTemplateStatuses
{
    public const string Draft = "draft";
    public const string Active = "active";
    public const string Inactive = "inactive";
}

public static class InspectionChecklistItemTypes
{
    public const string PassFail = "pass_fail";
    public const string Numeric = "numeric";
    public const string Text = "text";
}
