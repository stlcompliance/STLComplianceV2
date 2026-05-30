using ComplianceCore.Api.Entities;

namespace ComplianceCore.Api.Services;

public sealed record FactRequirementContractInput(
    string RequirementKey,
    string FactKey,
    string ApplicabilityKey,
    string SourceProduct,
    string SourceEntity,
    string SourceFieldOrRecordType,
    string ValueType,
    string Operator,
    string ExpectedValue,
    string EvidenceKind,
    string RequiredDocumentType,
    string RetentionPeriod,
    string AuditQuestion,
    string FailureSeverity,
    bool AutomaticFailureFlag,
    bool OverrideAllowed,
    string OverridePermission,
    bool RemediationRequired,
    bool IsRequired,
    bool ExternallyAssertable = false);

public static class FactRequirementContractRules
{
    public static IReadOnlyList<string> Validate(FactRequirementContractInput input, bool strictAuditMetadata)
    {
        var issues = new List<string>();

        ValidateRequired(input.RequirementKey, "requirement_key", issues);
        ValidateRequired(input.FactKey, "fact_key", issues);
        ValidateRequired(input.ValueType, "value_type", issues);
        ValidateRequired(input.Operator, "operator", issues);
        ValidateRequired(input.EvidenceKind, "evidence_kind", issues);
        ValidateRequired(input.FailureSeverity, "failure_severity", issues);

        if (!FactValueTypes.All.Contains(input.ValueType))
        {
            issues.Add($"Unsupported value_type '{input.ValueType}'.");
        }

        if (!FactRequirementOperators.All.Contains(input.Operator))
        {
            issues.Add($"Unsupported operator '{input.Operator}'.");
        }

        if (!FactRequirementEvidenceKinds.All.Contains(input.EvidenceKind))
        {
            issues.Add($"Unsupported evidence_kind '{input.EvidenceKind}'.");
        }

        if (!FactRequirementFailureSeverities.All.Contains(input.FailureSeverity))
        {
            issues.Add($"Unsupported failure_severity '{input.FailureSeverity}'.");
        }

        foreach (var product in SplitCsv(input.SourceProduct))
        {
            if (!ComplianceCoreProductKeys.Canonical.ContainsKey(product))
            {
                issues.Add($"Unknown source_product '{product}'.");
            }
        }

        var isDerived = string.Equals(input.EvidenceKind, FactRequirementEvidenceKinds.DerivedFact, StringComparison.OrdinalIgnoreCase);
        if (isDerived)
        {
            if (!string.Equals(input.Operator, FactRequirementOperators.AllTrue, StringComparison.OrdinalIgnoreCase))
            {
                issues.Add("Derived facts must use operator all_true.");
            }

            if (SplitCsv(input.ExpectedValue).Count == 0)
            {
                issues.Add("Derived facts must list component fact keys in expected_value.");
            }
        }
        else
        {
            if (string.Equals(input.Operator, FactRequirementOperators.AllTrue, StringComparison.OrdinalIgnoreCase))
            {
                issues.Add("Non-derived facts cannot use operator all_true.");
            }

            if (strictAuditMetadata || input.IsRequired)
            {
                ValidateRequired(input.SourceProduct, "source_product", issues);
                ValidateRequired(input.SourceEntity, "source_entity", issues);
                ValidateRequired(input.SourceFieldOrRecordType, "source_field_or_record_type", issues);
                ValidateRequired(input.AuditQuestion, "audit_question", issues);
                ValidateRequired(input.RetentionPeriod, "retention_period", issues);
                ValidateRequired(input.FailureSeverity, "failure_severity", issues);
            }
        }

        return issues;
    }

    public static IReadOnlyList<string> SplitCsv(string? raw) =>
        (raw ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToList();

    public static string NormalizeProducts(string raw) =>
        string.Join(
            ',',
            SplitCsv(raw).Select(product =>
                ComplianceCoreProductKeys.Canonical.TryGetValue(product, out var canonical)
                    ? canonical
                    : product.Trim()));

    public static bool TryParseBool(string raw) =>
        bool.TryParse(raw, out var parsed) && parsed;

    private static void ValidateRequired(string? value, string column, List<string> issues)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            issues.Add($"Column '{column}' is required.");
        }
    }
}
