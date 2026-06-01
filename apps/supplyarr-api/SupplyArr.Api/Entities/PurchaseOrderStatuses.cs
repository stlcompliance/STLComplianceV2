namespace SupplyArr.Api.Entities;

public static class PurchaseOrderStatuses
{
    public const string Draft = "draft";
    public const string Approved = "approved";
    public const string Issued = "issued";
    public const string Cancelled = "cancelled";

    public static readonly HashSet<string> All = new(StringComparer.OrdinalIgnoreCase)
    {
        Draft,
        Approved,
        Issued,
        Cancelled
    };

    public static readonly HashSet<string> Editable = new(StringComparer.OrdinalIgnoreCase)
    {
        Draft
    };

    public static readonly string[] Open =
    [
        Draft,
        Approved
    ];
}
