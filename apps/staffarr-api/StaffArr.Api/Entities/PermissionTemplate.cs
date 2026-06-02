using STLCompliance.Shared.Data;

namespace StaffArr.Api.Entities;

public sealed class PermissionTemplate : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string PermissionKey { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Status { get; set; } = "active";

    public string ProductKey { get; set; } = "staffarr";

    public string PermissionScope { get; set; } = "tenant";

    public string Sensitivity { get; set; } = "standard";

    public DateTimeOffset? LastSyncedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
