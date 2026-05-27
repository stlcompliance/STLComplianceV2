namespace TrainArr.Api.Entities;

public sealed class TrainingCitationAttachment
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    /// <summary>training_definition | training_program | training_assignment</summary>
    public string EntityType { get; set; } = string.Empty;

    public Guid EntityId { get; set; }

    public Guid ComplianceCoreCitationId { get; set; }

    public string CitationKey { get; set; } = string.Empty;

    public int CitationVersion { get; set; }

    public Guid? AttachedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
