namespace TrainArr.Api.Services;

public static class EvidenceRetentionRules
{
    public const int DefaultRetentionDays = 365;

    public static int NormalizeBatchSize(int? batchSize) =>
        Math.Clamp(batchSize ?? 50, 1, 200);

    public static int NormalizeRetentionDays(int? retentionDays) =>
        Math.Clamp(retentionDays ?? DefaultRetentionDays, 30, 3650);

    public static int NormalizeRunListLimit(int? limit) =>
        Math.Clamp(limit ?? 20, 1, 100);

    public static DateTimeOffset? GetAssignmentClosedAt(string status, DateTimeOffset? completedAt, DateTimeOffset updatedAt) =>
        status switch
        {
            "completed" => completedAt ?? updatedAt,
            "cancelled" => updatedAt,
            _ => null
        };

    public static bool IsExpired(
        DateTimeOffset assignmentClosedAt,
        DateTimeOffset asOfUtc,
        int retentionDays)
    {
        var cutoff = asOfUtc.AddDays(-retentionDays);
        return assignmentClosedAt < cutoff;
    }

    public static bool IsClosedAssignmentStatus(string status) =>
        status is "completed" or "cancelled";
}
