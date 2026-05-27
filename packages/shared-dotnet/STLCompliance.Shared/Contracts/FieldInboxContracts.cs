namespace STLCompliance.Shared.Contracts;

public sealed record FieldInboxTaskItem(
    string TaskKey,
    string ProductKey,
    string TaskType,
    string Title,
    string? Subtitle,
    string Status,
    string? Priority,
    DateTimeOffset? DueAt,
    DateTimeOffset? SortAt,
    string DeepLinkPath,
    string? BlockedReason = null);

public sealed record FieldInboxSummary(
    int TotalCount,
    int BlockedCount,
    IReadOnlyDictionary<string, int> CountByProduct);

public sealed record FieldInboxResponse(
    FieldInboxSummary Summary,
    IReadOnlyList<FieldInboxTaskItem> Items);

public sealed record FieldInboxProductSlice(
    string ProductKey,
    bool Entitled,
    bool Fetched,
    string? ErrorCode,
    string? ErrorMessage,
    IReadOnlyList<FieldInboxTaskItem> Items);

public sealed record AggregatedFieldInboxResponse(
    FieldInboxSummary Summary,
    IReadOnlyList<FieldInboxTaskItem> Items,
    IReadOnlyList<FieldInboxProductSlice> Sources);

public static class FieldInboxRules
{
    private static readonly HashSet<string> FieldInboxProducts =
    [
        "staffarr",
        "trainarr",
        "maintainarr",
        "routarr",
        "supplyarr",
    ];

    public static IReadOnlyList<string> FieldProductKeys => FieldInboxProducts.ToList();

    public static FieldInboxResponse BuildProductResponse(IReadOnlyList<FieldInboxTaskItem> items)
    {
        var ordered = OrderItems(items);
        return new FieldInboxResponse(BuildSummary(ordered), ordered);
    }

    public static AggregatedFieldInboxResponse BuildAggregatedResponse(
        IReadOnlyList<FieldInboxProductSlice> sources)
    {
        var merged = OrderItems(sources.SelectMany(x => x.Items).ToList());
        return new AggregatedFieldInboxResponse(BuildSummary(merged), merged, sources);
    }

    private static FieldInboxSummary BuildSummary(IReadOnlyList<FieldInboxTaskItem> items)
    {
        var countByProduct = items
            .GroupBy(x => x.ProductKey, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

        return new FieldInboxSummary(
            items.Count,
            items.Count(x => !string.IsNullOrWhiteSpace(x.BlockedReason)),
            countByProduct);
    }

    private static IReadOnlyList<FieldInboxTaskItem> OrderItems(IReadOnlyList<FieldInboxTaskItem> items) =>
        items
            .OrderBy(x => x.BlockedReason is null ? 0 : 1)
            .ThenByDescending(x => x.SortAt ?? x.DueAt ?? DateTimeOffset.MinValue)
            .ThenBy(x => x.Title, StringComparer.OrdinalIgnoreCase)
            .ToList();
}
