using ComplianceCore.Api.Entities;

namespace ComplianceCore.Api.Services;

public static class ReadinessForecastRules
{
    public const int MaxListLimit = 100;

    public static int ComputePackReadinessScore(
        int riskScore,
        int effectivenessScore,
        int missingEvidenceWarningCount,
        string highestMissingEvidenceSeverity)
    {
        var riskComponent = 100 - Math.Clamp(riskScore, 0, 100);
        var effectivenessComponent = Math.Clamp(effectivenessScore, 0, 100);
        var severityPenalty = MissingEvidenceWarningSeverities.Rank(highestMissingEvidenceSeverity) * 6;
        var countPenalty = Math.Min(18, missingEvidenceWarningCount * 4);

        var blended = (int)Math.Round(
            riskComponent * 0.35
            + effectivenessComponent * 0.40
            + Math.Max(0, 100 - severityPenalty - countPenalty) * 0.25);

        return Math.Clamp(blended - countPenalty, 0, 100);
    }

    public static string BuildSummary(
        string packKey,
        int readinessScore,
        string readinessLevel,
        int riskScore,
        int effectivenessScore,
        int missingEvidenceWarningCount) =>
        $"Readiness forecast for {packKey}: {readinessScore} ({readinessLevel}) from risk {riskScore}, " +
        $"effectiveness {effectivenessScore}, {missingEvidenceWarningCount} missing-evidence warning(s).";
}
