using ComplianceCore.Api.Entities;

namespace ComplianceCore.Api.Services;

public static class MissingEvidenceWarningRules
{
    public const int MaxPacksPerEvaluate = 25;

    public const int MaxListLimit = 100;

    public static string DetermineSeverity(
        bool isRequiredInRule,
        bool isRequiredInCatalog,
        bool hasMirrorAtScope,
        bool isUnresolved,
        bool hasDefinition)
    {
        if (!hasDefinition)
        {
            return isRequiredInRule
                ? MissingEvidenceWarningSeverities.Critical
                : MissingEvidenceWarningSeverities.High;
        }

        if (isRequiredInRule && isUnresolved && !hasMirrorAtScope)
        {
            return MissingEvidenceWarningSeverities.Critical;
        }

        if (isRequiredInRule && (!hasMirrorAtScope || isUnresolved))
        {
            return MissingEvidenceWarningSeverities.High;
        }

        if (isRequiredInCatalog && (!hasMirrorAtScope || isUnresolved))
        {
            return MissingEvidenceWarningSeverities.Medium;
        }

        if (!hasMirrorAtScope || isUnresolved)
        {
            return MissingEvidenceWarningSeverities.Low;
        }

        return MissingEvidenceWarningSeverities.Low;
    }

    public static string DetermineReasonCode(
        bool hasDefinition,
        bool hasMirrorAtScope,
        bool isUnresolved)
    {
        if (!hasDefinition)
        {
            return MissingEvidenceReasonCodes.NoFactDefinition;
        }

        if (isUnresolved)
        {
            return MissingEvidenceReasonCodes.UnresolvedFact;
        }

        return MissingEvidenceReasonCodes.MissingMirror;
    }

    public static string DetermineWarningType(bool isRequiredInRule, bool isInCatalog) =>
        isRequiredInRule && !isInCatalog
            ? MissingEvidenceWarningTypes.RulePackFact
            : isInCatalog
                ? MissingEvidenceWarningTypes.CatalogRequirement
                : MissingEvidenceWarningTypes.RulePackFact;

    public static bool ShouldEmitWarning(
        bool isInRule,
        bool isInCatalog,
        bool isRequiredInCatalog,
        bool hasMirrorAtScope,
        bool isUnresolved,
        bool hasDefinition)
    {
        if (isInRule && (!hasDefinition || isUnresolved || !hasMirrorAtScope))
        {
            return true;
        }

        if (isInCatalog && isRequiredInCatalog && !hasMirrorAtScope)
        {
            return true;
        }

        if (isInCatalog && !hasMirrorAtScope && (isUnresolved || !hasDefinition))
        {
            return true;
        }

        return false;
    }

    public static string BuildSummary(
        string packKey,
        string factKey,
        string severity,
        string reasonCode,
        bool hasMirrorAtScope) =>
        $"Predicted missing evidence for {packKey}/{factKey}: {reasonCode} ({severity})" +
        (hasMirrorAtScope ? " — mirror present but fact unresolved." : " — no mirror at scope.");
}
