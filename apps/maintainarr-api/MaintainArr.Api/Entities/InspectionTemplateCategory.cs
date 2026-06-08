using STLCompliance.Shared.Data;

namespace MaintainArr.Api.Entities;

public sealed class InspectionTemplateCategory : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid InspectionTemplateId { get; set; }

    public string CategoryKey { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsRequired { get; set; }

    public bool CanBeSkipped { get; set; }

    public bool SkipReasonRequired { get; set; }

    public bool TimingTracked { get; set; }

    public string SettingsJson { get; set; } = "{}";

    public int SortOrder { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public InspectionTemplate InspectionTemplate { get; set; } = null!;
}
