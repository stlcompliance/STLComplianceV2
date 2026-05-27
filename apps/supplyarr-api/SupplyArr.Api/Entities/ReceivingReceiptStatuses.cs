namespace SupplyArr.Api.Entities;

public static class ReceivingReceiptStatuses
{
    public const string Draft = "draft";

    public const string Posted = "posted";

    public static readonly IReadOnlySet<string> Editable = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Draft
    };
}
