namespace SupplyArr.Api.Options;

public sealed class ComplianceCoreClientOptions
{
    public const string SectionName = "ComplianceCore";

    public string BaseUrl { get; set; } = "http://localhost:5107";

    public string ServiceToken { get; set; } = string.Empty;

    public string VendorUseActionKey { get; set; } = "can-use-vendor";

    public string? VendorUseWorkflowKey { get; set; }

    public string VendorUseActivityContextKey { get; set; } = "purchase_order_issue";

    public bool EmitVendorUseFindings { get; set; }
}
