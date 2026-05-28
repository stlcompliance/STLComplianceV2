using StaffArr.Api.Contracts;

namespace StaffArr.Api.Services;

public static class PersonnelHistoryRules
{
    public const int DefaultReadStalenessHours = 1;

    public static int NormalizeBatchSize(int? batchSize) =>
        Math.Clamp(batchSize ?? 100, 1, 500);

    public static int NormalizeStalenessHours(int? stalenessHours) =>
        Math.Clamp(stalenessHours ?? DefaultReadStalenessHours, 1, 168);

    public static bool IsStale(DateTimeOffset? computedAt, DateTimeOffset asOfUtc, int stalenessHours)
    {
        if (computedAt is null)
        {
            return true;
        }

        var threshold = asOfUtc.AddHours(-stalenessHours);
        return computedAt < threshold;
    }

    public static PersonnelHistoryCategoryCounts AggregateCategoryCounts(
        IReadOnlyList<PersonTimelineEntryResponse> entries)
    {
        var incident = 0;
        var certification = 0;
        var permission = 0;
        var readiness = 0;
        var trainingBlocker = 0;
        var personnelNote = 0;
        var personnelDocument = 0;

        foreach (var entry in entries)
        {
            switch (entry.Category)
            {
                case "incident":
                case "incident_routing":
                    incident++;
                    break;
                case "certification":
                    certification++;
                    break;
                case "permission":
                    permission++;
                    break;
                case "readiness":
                    readiness++;
                    break;
                case "training_blocker":
                    trainingBlocker++;
                    break;
                case "personnel_note":
                    personnelNote++;
                    break;
                case "personnel_document":
                    personnelDocument++;
                    break;
            }
        }

        return new PersonnelHistoryCategoryCounts(
            incident,
            certification,
            permission,
            readiness,
            trainingBlocker,
            personnelNote,
            personnelDocument);
    }
}

public sealed record PersonnelHistoryCategoryCounts(
    int IncidentCount,
    int CertificationCount,
    int PermissionCount,
    int ReadinessCount,
    int TrainingBlockerCount,
    int PersonnelNoteCount,
    int PersonnelDocumentCount);
