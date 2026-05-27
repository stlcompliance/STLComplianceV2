using STLCompliance.Shared.Data;

namespace StaffArr.Api.Entities;

public sealed class PersonReadinessOverride : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid PersonId { get; set; }

    public string Status { get; set; } = "active";

    public string Reason { get; set; } = string.Empty;

    public DateTimeOffset GrantedAt { get; set; }

    public DateTimeOffset? ExpiresAt { get; set; }

    public Guid GrantedByUserId { get; set; }

    public DateTimeOffset? ClearedAt { get; set; }

    public Guid? ClearedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
