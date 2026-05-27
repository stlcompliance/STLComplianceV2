namespace SupplyArr.Api.Entities;

public static class BackorderSourceTypes
{
    public const string ReceiptPost = "receipt_post";

    public const string PurchaseOrderLine = "purchase_order_line";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ReceiptPost,
        PurchaseOrderLine
    };
}
