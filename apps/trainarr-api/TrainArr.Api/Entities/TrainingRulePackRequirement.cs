namespace TrainArr.Api.Entities;

public sealed class TrainingRulePackRequirement
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    /// <summary>training_definition | training_program</summary>
    public string EntityType { get; set; } = string.Empty;

    public Guid EntityId { get; set; }

    public string RulePackKey { get; set; } = string.Empty;

    /// <summary>Compliance Core version captured when requirement was last validated.</summary>
    public int? KnownVersionNumber { get; set; }

    /// <summary>Compliance Core status captured when requirement was last validated.</summary>
    public string? KnownStatus { get; set; }

    public Guid? AttachedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

}
