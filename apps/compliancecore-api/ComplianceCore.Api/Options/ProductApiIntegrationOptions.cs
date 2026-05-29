namespace ComplianceCore.Api.Options;

public sealed class ProductApiIntegrationOptions
{
    public const string SectionName = "ProductApis";

    public Dictionary<string, ProductApiConnectionOptions> Products { get; set; } =
        new(StringComparer.OrdinalIgnoreCase);
}

public sealed class ProductApiConnectionOptions
{
    public string BaseUrl { get; set; } = string.Empty;

    public string ServiceToken { get; set; } = string.Empty;
}
