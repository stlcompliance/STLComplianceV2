using STLCompliance.Shared.Data;

namespace NexArr.Api.Entities;

public sealed class TenantProductLicense : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string ProductKey { get; set; } = string.Empty;

    public string Status { get; set; } = LicenseStatuses.Active;

    public DateTimeOffset ValidFrom { get; set; }

    public DateTimeOffset? ValidTo { get; set; }

    public string? ExternalReference { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset ModifiedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;

    public ProductCatalogItem Product { get; set; } = null!;
}

public static class LicenseStatuses
{
    public const string Active = "Active";
    public const string Expired = "Expired";
    public const string Revoked = "Revoked";
}

public sealed class PlatformEntitlementReconciliationSettings
{
    public static readonly Guid SingletonId = Guid.Parse("00000000-0000-0000-0000-000000000002");

    public Guid Id { get; set; } = SingletonId;

    public bool IsEnabled { get; set; }

    public bool AutoGrantFromLicense { get; set; } = true;

    public bool AutoRevokeStaleEntitlements { get; set; } = true;

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class EntitlementReconciliationRun
{
    public Guid Id { get; set; }

    public string Outcome { get; set; } = string.Empty;

    public int DriftFoundCount { get; set; }

    public int GrantedCount { get; set; }

    public int RevokedCount { get; set; }

    public int SkippedCount { get; set; }

    public string? SkipReason { get; set; }

    public DateTimeOffset ProcessedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
