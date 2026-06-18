using STLCompliance.Shared.Data;

namespace TrainArr.Api.Entities;

public sealed class TrainArrTenantSettings : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string SettingsJson { get; set; } = string.Empty;

    public int SchemaVersion { get; set; } = 1;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Guid? UpdatedByPersonId { get; set; }

    public long RowVersion { get; set; } = 1;
}
