using STLCompliance.Shared.Data;

namespace NexArr.Api.Entities;

public sealed class TenantMembership : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid UserId { get; set; }

    public string RoleKey { get; set; } = "tenant_admin";

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;

    public PlatformUser User { get; set; } = null!;
}
