namespace RoutArr.Api.Services;

public static class DispatchOverrideReportRules
{
    public const string OverrideMarker = "(override:";

    public static readonly IReadOnlySet<string> AssignmentActions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "trip.assign_driver",
        "trip.assign_vehicle",
    };

    public static bool IsOverrideAuditEntry(string action, string result) =>
        AssignmentActions.Contains(action)
        && result.Contains(OverrideMarker, StringComparison.OrdinalIgnoreCase);

    public static IReadOnlyList<string> ParseOverrideKinds(string result)
    {
        var start = result.IndexOf(OverrideMarker, StringComparison.OrdinalIgnoreCase);
        if (start < 0)
        {
            return [];
        }

        var openParen = result.IndexOf('(', start);
        var closeParen = result.IndexOf(')', openParen + 1);
        if (openParen < 0 || closeParen <= openParen)
        {
            return [];
        }

        var inner = result[(openParen + 1)..closeParen];
        const string prefix = "override:";
        if (!inner.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }

        return inner[prefix.Length..]
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.ToLowerInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static string NormalizeScope(string? scope) =>
        string.Equals(scope, DispatchBoardService.ScopeWeekly, StringComparison.OrdinalIgnoreCase)
            ? DispatchBoardService.ScopeWeekly
            : DispatchBoardService.ScopeDaily;

    public static (DateTimeOffset WindowStart, DateTimeOffset WindowEnd) GetWindow(string scope, DateTimeOffset now)
    {
        var dayStart = new DateTimeOffset(now.UtcDateTime.Date, TimeSpan.Zero);
        return scope == DispatchBoardService.ScopeWeekly
            ? (dayStart, dayStart.AddDays(7))
            : (dayStart, dayStart.AddDays(1));
    }
}
