namespace SupplyArr.Api.Entities;

public static class BackorderStatuses
{
    public const string Open = "open";

    public const string Fulfilled = "fulfilled";

    public const string Cancelled = "cancelled";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Open,
        Fulfilled,
        Cancelled
    };
}
