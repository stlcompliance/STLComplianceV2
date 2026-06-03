namespace NexArr.Api.Entities;

public sealed class Tenant
{
    public Guid Id { get; set; }

    public string Slug { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Status { get; set; } = TenantStatuses.Active;

    public string SubscriptionTier { get; set; } = TenantSubscriptionTiers.Standard;

    public string? BillingCustomerId { get; set; }

    public string? BillingSubscriptionId { get; set; }

    public int? BillingGraceDays { get; set; }

    public bool IsTrial { get; set; }

    public bool IsInternalTenant { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset ModifiedAt { get; set; }

    public ICollection<TenantMembership> Memberships { get; set; } = [];

    public ICollection<TenantProductEntitlement> Entitlements { get; set; } = [];
}

public static class TenantStatuses
{
    public const string Active = "Active";
    public const string Trial = "Trial";
    public const string Suspended = "Suspended";
    public const string Archived = "Archived";
}

public static class TenantSubscriptionTiers
{
    public const string Standard = "standard";
    public const string Trial = "trial";
    public const string Enterprise = "enterprise";
    public const string Internal = "internal";
}
