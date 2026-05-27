namespace SupplyArr.Api.Entities;

public static class ReceivingExceptionStatuses
{
    public const string Open = "open";

    public const string Resolved = "resolved";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Open,
        Resolved
    };
}
