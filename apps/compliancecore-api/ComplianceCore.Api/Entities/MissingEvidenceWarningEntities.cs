using STLCompliance.Shared.Data;

namespace ComplianceCore.Api.Entities;

public sealed class MissingEvidenceWarningRun : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string ScopeKey { get; set; } = "tenant";

    public Guid? ActorUserId { get; set; }

    public int PacksAnalyzedCount { get; set; }

    public int WarningsEmittedCount { get; set; }

    public string HighestSeverity { get; set; } = MissingEvidenceWarningSeverities.Low;

    public DateTimeOffset EvaluatedAt { get; set; }
}

public sealed class MissingEvidenceWarning : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid RunId { get; set; }

    public string ScopeKey { get; set; } = "tenant";

    public Guid RulePackId { get; set; }

    public string PackKey { get; set; } = string.Empty;

    public string FactKey { get; set; } = string.Empty;

    public Guid? FactDefinitionId { get; set; }

    public string WarningType { get; set; } = MissingEvidenceWarningTypes.RulePackFact;

    public string Severity { get; set; } = MissingEvidenceWarningSeverities.Medium;

    public string ReasonCode { get; set; } = MissingEvidenceReasonCodes.MissingMirror;

    public bool HasMirrorAtScope { get; set; }

    public bool IsRequiredInRule { get; set; }

    public bool IsRequiredInCatalog { get; set; }

    public string Summary { get; set; } = string.Empty;

    public DateTimeOffset EvaluatedAt { get; set; }

    public MissingEvidenceWarningRun? Run { get; set; }
}

public static class MissingEvidenceWarningTypes
{
    public const string RulePackFact = "rule_pack_fact";

    public const string CatalogRequirement = "catalog_requirement";
}

public static class MissingEvidenceWarningSeverities
{
    public const string Low = "low";

    public const string Medium = "medium";

    public const string High = "high";

    public const string Critical = "critical";

    public static int Rank(string severity) =>
        severity switch
        {
            Critical => 4,
            High => 3,
            Medium => 2,
            _ => 1,
        };

    public static string Max(string left, string right) =>
        Rank(left) >= Rank(right) ? left : right;
}

public static class MissingEvidenceReasonCodes
{
    public const string MissingMirror = "missing_mirror";

    public const string UnresolvedFact = "unresolved_fact";

    public const string NoFactDefinition = "no_fact_definition";
}
