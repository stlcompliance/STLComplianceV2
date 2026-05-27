using STLCompliance.Shared.Data;

namespace ComplianceCore.Api.Entities;

public sealed class RegulatoryMapping : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string MappingKey { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string TargetKind { get; set; } = string.Empty;

    public Guid RegulatoryProgramId { get; set; }

    public Guid? RulePackId { get; set; }

    public Guid? CitationId { get; set; }

    public Guid? FactDefinitionId { get; set; }

    public Guid? ComplianceKeyId { get; set; }

    public Guid? MaterialKeyId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public RegulatoryProgram? RegulatoryProgram { get; set; }

    public RulePack? RulePack { get; set; }

    public RegulatoryCitation? Citation { get; set; }

    public FactDefinition? FactDefinition { get; set; }

    public ComplianceKey? ComplianceKey { get; set; }

    public MaterialKey? MaterialKey { get; set; }
}
