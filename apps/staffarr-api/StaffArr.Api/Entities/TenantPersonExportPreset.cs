using STLCompliance.Shared.Data;

namespace StaffArr.Api.Entities;

public sealed class TenantPersonExportPreset : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string? EmploymentStatus { get; set; }

    public Guid? OrgUnitId { get; set; }

    public string? PresetKey { get; set; }

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public OrgUnit? OrgUnit { get; set; }
}
