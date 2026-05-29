using STLCompliance.Shared.Data;

namespace TrainArr.Api.Entities;

public sealed class TrainingDefinitionStepBranch : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid TrainingDefinitionStepId { get; set; }

    public TrainingDefinitionStep TrainingDefinitionStep { get; set; } = null!;

    public string BranchKey { get; set; } = string.Empty;

    public string BranchType { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string ConfigJson { get; set; } = "{}";

    public int SortOrder { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
