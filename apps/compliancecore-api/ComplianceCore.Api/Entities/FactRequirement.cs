using STLCompliance.Shared.Data;

namespace ComplianceCore.Api.Entities;

public sealed class FactRequirement : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid FactDefinitionId { get; set; }

    public Guid? RulePackId { get; set; }

    public Guid? CitationId { get; set; }

    public string RequirementKey { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsRequired { get; set; } = true;

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public FactDefinition? FactDefinition { get; set; }

    public RulePack? RulePack { get; set; }

    public RegulatoryCitation? Citation { get; set; }
}
