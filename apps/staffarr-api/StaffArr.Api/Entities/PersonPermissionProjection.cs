using STLCompliance.Shared.Data;

namespace StaffArr.Api.Entities;

public sealed class PersonPermissionProjection : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid PersonId { get; set; }

    public int PermissionCount { get; set; }

    public DateTimeOffset ComputedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<PersonPermissionProjectionEntry> Entries { get; set; } = [];
}

public sealed class PersonPermissionProjectionEntry : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid PersonId { get; set; }

    public Guid ProjectionId { get; set; }

    public string PermissionKey { get; set; } = string.Empty;

    public string PermissionName { get; set; } = string.Empty;

    public string ScopeType { get; set; } = string.Empty;

    public string? ScopeValue { get; set; }

    public PersonPermissionProjection Projection { get; set; } = null!;
}
