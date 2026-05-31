namespace SupplyArr.Api.Entities;

public static class ReceivingReceiptStatuses
{
    public const string Draft = "draft";

    public const string Posted = "posted";

    public const string Received = "received";

    public const string PartiallyReceived = "partially_received";

    public const string Overreceived = "overreceived";

    public const string Underreceived = "underreceived";

    public const string Damaged = "damaged";

    public const string WrongItem = "wrong_item";

    public const string PendingInspection = "pending_inspection";

    public const string Quarantined = "quarantined";

    public const string Returned = "returned";

    public const string Closed = "closed";

    public static readonly IReadOnlySet<string> Editable = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Draft
    };

    public static readonly IReadOnlySet<string> PostedLike = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Posted,
        Received,
        PartiallyReceived,
        Overreceived,
        Underreceived,
        Damaged,
        WrongItem,
        PendingInspection,
        Quarantined,
        Returned,
        Closed
    };

    public static bool IsPostedLike(string status) => PostedLike.Contains(status ?? string.Empty);
}
