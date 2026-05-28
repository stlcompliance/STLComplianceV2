using STLCompliance.Shared.Data;

namespace TrainArr.Api.Entities;

public sealed class RulePackImpactState : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string RulePackKey { get; set; } = string.Empty;

    public bool RequiresAttention { get; set; }

    public bool HasDrift { get; set; }

    public string Triggers { get; set; } = string.Empty;

    public int? BaselineVersionNumber { get; set; }

    public int? CurrentVersionNumber { get; set; }

    public string? BaselineStatus { get; set; }

    public string? CurrentStatus { get; set; }

    public int RequirementCount { get; set; }

    public int DefinitionCount { get; set; }

    public int ProgramCount { get; set; }

    public int ActiveAssignmentCount { get; set; }

    public int ActiveQualificationCount { get; set; }

    public Guid LastAssessmentId { get; set; }

    public DateTimeOffset ComputedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
