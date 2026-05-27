using STLCompliance.Shared.Data;

namespace MaintainArr.Api.Entities;

public sealed class InspectionTemplateCategory : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid InspectionTemplateId { get; set; }

    public string CategoryKey { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public InspectionTemplate InspectionTemplate { get; set; } = null!;
}
