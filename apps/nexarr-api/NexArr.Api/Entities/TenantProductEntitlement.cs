using STLCompliance.Shared.Data;

namespace NexArr.Api.Entities;

public sealed class TenantProductEntitlement : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string ProductKey { get; set; } = string.Empty;

    public string Status { get; set; } = EntitlementStatuses.Active;

    public DateTimeOffset GrantedAt { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;

    public ProductCatalogItem Product { get; set; } = null!;
}

public static class EntitlementStatuses
{
    public const string Active = "Active";
    public const string Revoked = "Revoked";
}
