using STLCompliance.Shared.Data;

namespace TrainArr.Api.Entities;

/// <summary>
/// Maps a training requirement to a program or definition with optional structured applicability.
/// </summary>
public sealed class TrainingRequirement : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string RequirementKey { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string RequirementSource { get; set; } = "internal";

    public string? SourceKey { get; set; }

    public Guid? TrainingProgramId { get; set; }

    public TrainingProgram? TrainingProgram { get; set; }

    public Guid? TrainingDefinitionId { get; set; }

    public TrainingDefinition? TrainingDefinition { get; set; }

    public Guid? ApplicabilityProfileId { get; set; }

    public TrainingApplicabilityProfile? ApplicabilityProfile { get; set; }

    public string RequirementLevel { get; set; } = "required";

    public int SortOrder { get; set; }

    public string Status { get; set; } = "active";

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public static class TrainingRequirementSources
{
    public const string Internal = "internal";
    public const string RulePack = "rule_pack";
    public const string Citation = "citation";
}
