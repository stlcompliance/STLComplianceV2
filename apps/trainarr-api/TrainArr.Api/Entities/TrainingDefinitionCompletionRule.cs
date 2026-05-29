using STLCompliance.Shared.Data;

namespace TrainArr.Api.Entities;

public sealed class TrainingDefinitionCompletionRule : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid TrainingDefinitionId { get; set; }

    public TrainingDefinition TrainingDefinition { get; set; } = null!;

    public string RuleKey { get; set; } = string.Empty;

    public string RuleType { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string ConfigJson { get; set; } = "{}";

    public int SortOrder { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
