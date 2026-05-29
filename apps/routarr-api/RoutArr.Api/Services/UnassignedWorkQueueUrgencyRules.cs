using RoutArr.Api.Contracts;

namespace RoutArr.Api.Services;

public static class UnassignedWorkQueueUrgencyRules
{
    public static int ComputeMinutesUntilStart(DateTimeOffset? scheduledStartAt, DateTimeOffset asOf)
    {
        if (!scheduledStartAt.HasValue)
        {
            return int.MaxValue;
        }

        return (int)Math.Floor((scheduledStartAt.Value - asOf).TotalMinutes);
    }

    public static int ComputeUrgencyRank(bool isLate, bool isAtRisk) =>
        isLate ? 0 : isAtRisk ? 1 : 2;

    public static IReadOnlyList<UnassignedWorkQueueTripRow> SortByUrgency(
        IEnumerable<UnassignedWorkQueueTripRow> items) =>
        items
            .OrderBy(x => ComputeUrgencyRank(x.IsLate, x.IsAtRisk))
            .ThenBy(x => x.ScheduledStartAt ?? DateTimeOffset.MaxValue)
            .ThenBy(x => x.TripNumber)
            .ToList();

    public static bool MatchesAttentionFilter(bool isLate, bool isAtRisk, bool attentionOnly) =>
        !attentionOnly || isLate || isAtRisk;
}
