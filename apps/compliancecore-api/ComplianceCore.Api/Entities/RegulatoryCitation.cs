using STLCompliance.Shared.Data;

namespace ComplianceCore.Api.Entities;

public sealed class RegulatoryCitation : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid RegulatoryProgramId { get; set; }

    public Guid? RulePackId { get; set; }

    public string CitationKey { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string SourceReference { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int VersionNumber { get; set; } = 1;

    public Guid? SupersedesCitationId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public RegulatoryProgram? RegulatoryProgram { get; set; }

    public RulePack? RulePack { get; set; }

    public RegulatoryCitation? SupersedesCitation { get; set; }
}
