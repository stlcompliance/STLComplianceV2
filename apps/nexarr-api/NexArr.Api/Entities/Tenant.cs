namespace NexArr.Api.Entities;

public sealed class Tenant
{
    public Guid Id { get; set; }

    public string Slug { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Status { get; set; } = TenantStatuses.Active;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset ModifiedAt { get; set; }

    public ICollection<TenantMembership> Memberships { get; set; } = [];

    public ICollection<TenantProductEntitlement> Entitlements { get; set; } = [];
}

public static class TenantStatuses
{
    public const string Active = "Active";
    public const string Suspended = "Suspended";
}
