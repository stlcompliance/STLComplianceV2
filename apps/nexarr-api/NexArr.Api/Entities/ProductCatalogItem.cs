namespace NexArr.Api.Entities;

public sealed class ProductCatalogItem
{
    public string ProductKey { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string ProductCategory { get; set; } = "operations";

    public string ProductOwner { get; set; } = "STL Compliance";

    public string ProductStatus { get; set; } = "available";

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public string CanonicalCallbackPath { get; set; } = "/auth/nexarr/callback";

    public string ApiBaseUrl { get; set; } = string.Empty;

    public string HealthUrl { get; set; } = string.Empty;

    public string ServiceAudience { get; set; } = string.Empty;

    public string MarketingUrl { get; set; } = string.Empty;

    public string DocumentationUrl { get; set; } = string.Empty;

    public string SupportUrl { get; set; } = string.Empty;

    public string EnvironmentKey { get; set; } = "local";

    public string AvailabilityDependencyRules { get; set; } = string.Empty;

    public string ProductDependencyMetadata { get; set; } = string.Empty;
}
