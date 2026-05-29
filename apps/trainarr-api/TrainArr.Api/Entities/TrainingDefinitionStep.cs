using STLCompliance.Shared.Data;

namespace TrainArr.Api.Entities;

public sealed class TrainingDefinitionStep : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid TrainingDefinitionId { get; set; }

    public TrainingDefinition TrainingDefinition { get; set; } = null!;

    public string StepKey { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string StepType { get; set; } = "content";

    public string ConfigJson { get; set; } = "{}";

    public int SortOrder { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
