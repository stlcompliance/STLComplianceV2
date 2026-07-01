namespace SupplyArr.Api.Contracts;

public sealed record SupplyReferenceResolutionResponse(
    Guid TenantId,
    string ReferenceType,
    Guid SupplyArrReferenceId,
    string DisplayCode,
    string DisplayName,
    string Status,
    string SourceProduct,
    string ApiPath,
    string DeepLinkPath,
    DateTimeOffset UpdatedAt,
    IReadOnlyDictionary<string, string> Metadata);

public static class SupplyReferenceTypes
{
    public const string Supplier = "supplier";

    public const string Part = "part";

    public const string PurchaseRequest = "purchase_request";

    public const string PurchaseOrder = "purchase_order";

    public const string ReceivingReceipt = "receiving_receipt";

    public const string WarrantyClaim = "warranty_claim";

    public const string SupplierReturn = "supplier_return";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Supplier,
        Part,
        PurchaseRequest,
        PurchaseOrder,
        ReceivingReceipt,
        WarrantyClaim,
        SupplierReturn,
    };
}
