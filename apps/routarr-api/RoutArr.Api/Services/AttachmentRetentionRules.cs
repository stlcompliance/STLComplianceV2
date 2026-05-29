using RoutArr.Api.Entities;

namespace RoutArr.Api.Services;

public static class AttachmentRetentionRules
{
    public const int DefaultRetentionDays = AttachmentRetentionDefaults.RetentionDaysAfterTripClose;

    public static int NormalizeBatchSize(int? batchSize) =>
        Math.Clamp(batchSize ?? 50, 1, 200);

    public static int NormalizeRetentionDays(int? retentionDays) =>
        Math.Clamp(retentionDays ?? DefaultRetentionDays, 30, 3650);

    public static int NormalizeRunListLimit(int? limit) =>
        Math.Clamp(limit ?? 20, 1, 100);

    public static DateTimeOffset? GetTripClosedAt(
        string dispatchStatus,
        DateTimeOffset? closedAt,
        DateTimeOffset? completedAt,
        DateTimeOffset? cancelledAt,
        DateTimeOffset updatedAt) =>
        dispatchStatus switch
        {
            TripDispatchStatuses.Completed => closedAt ?? completedAt ?? updatedAt,
            TripDispatchStatuses.Cancelled => cancelledAt ?? updatedAt,
            _ => null
        };

    public static bool IsExpired(
        DateTimeOffset tripClosedAt,
        DateTimeOffset asOfUtc,
        int retentionDays)
    {
        var cutoff = asOfUtc.AddDays(-retentionDays);
        return tripClosedAt < cutoff;
    }

    public static bool IsClosedTripStatus(string status) =>
        status is TripDispatchStatuses.Completed or TripDispatchStatuses.Cancelled;
}
