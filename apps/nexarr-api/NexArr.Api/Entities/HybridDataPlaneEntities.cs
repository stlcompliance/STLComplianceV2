using STLCompliance.Shared.Data;

namespace NexArr.Api.Entities;

public sealed class TenantProductDataPlaneProfile : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string ProductKey { get; set; } = string.Empty;

    public string DeploymentMode { get; set; } = DataPlaneDeploymentModes.Hosted;

    public string? DataEndpointUrl { get; set; }

    public string TrustStatus { get; set; } = DataPlaneTrustStatuses.Trusted;

    public string? Notes { get; set; }

    public Guid? ModifiedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset ModifiedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;

    public ProductCatalogItem Product { get; set; } = null!;
}

public static class DataPlaneDeploymentModes
{
    public const string Hosted = "hosted";
    public const string CustomerHosted = "customer_hosted";
    public const string Hybrid = "hybrid";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        Hosted,
        CustomerHosted,
        Hybrid,
    };
}

public static class DataPlaneTrustStatuses
{
    public const string Trusted = "trusted";
    public const string Untrusted = "untrusted";
    public const string PendingValidation = "pending_validation";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        Trusted,
        Untrusted,
        PendingValidation,
    };
}
