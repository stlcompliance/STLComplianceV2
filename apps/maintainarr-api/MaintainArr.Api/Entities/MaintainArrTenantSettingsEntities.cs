using STLCompliance.Shared.Data;

namespace MaintainArr.Api.Entities;

public sealed class MaintainArrTenantSettings : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public int SchemaVersion { get; set; }

    public string SettingsJson { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public string? CreatedByPersonId { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }

    public string? UpdatedByPersonId { get; set; }
}

public sealed class MaintainArrTenantSettingsAudit : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid SettingsId { get; set; }

    public int SchemaVersion { get; set; }

    public DateTimeOffset ChangedAtUtc { get; set; }

    public string? ChangedByPersonId { get; set; }

    public string? ChangeReason { get; set; }

    public string BeforeJson { get; set; } = string.Empty;

    public string AfterJson { get; set; } = string.Empty;

    public string DiffJson { get; set; } = string.Empty;
}
