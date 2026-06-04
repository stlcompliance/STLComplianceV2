using STLCompliance.Shared.Data;

namespace ComplianceCore.Api.Entities;

public sealed class RuleTestCase : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid RulePackId { get; set; }

    public string RuleKey { get; set; } = string.Empty;

    public string TestKey { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string ExpectedResult { get; set; } = RuleEvaluationResults.Pass;

    public string FactsJson { get; set; } = "{}";

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public RulePack? RulePack { get; set; }
}
