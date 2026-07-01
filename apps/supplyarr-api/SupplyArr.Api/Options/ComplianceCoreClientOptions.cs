namespace SupplyArr.Api.Options;

public sealed class ComplianceCoreClientOptions
{
    public const string SectionName = "ComplianceCore";

    public string BaseUrl { get; set; } = "http://localhost:5107";

    public string ServiceToken { get; set; } = string.Empty;

    public string SupplierUseActionKey { get; set; } = "can-use-supplier";

    public string? SupplierUseWorkflowKey { get; set; }

    public string SupplierUseActivityContextKey { get; set; } = "purchase_order_issue";

    public bool EmitSupplierUseFindings { get; set; }
}
