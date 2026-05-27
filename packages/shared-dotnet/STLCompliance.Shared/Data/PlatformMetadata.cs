namespace STLCompliance.Shared.Data;

/// <summary>
/// Tenant-scoped platform metadata row used by M1 foundation migrations.
/// Product-specific tables are added in later milestones.
/// </summary>
public sealed class PlatformMetadata : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset ModifiedAt { get; set; }

    public string? CreatedBy { get; set; }

    public string? ModifiedBy { get; set; }
}
