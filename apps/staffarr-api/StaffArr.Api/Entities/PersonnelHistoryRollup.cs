using STLCompliance.Shared.Data;

namespace StaffArr.Api.Entities;

public sealed class PersonnelHistoryRollup : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid PersonId { get; set; }

    public int EventCount { get; set; }

    public int IncidentCount { get; set; }

    public int CertificationCount { get; set; }

    public int PermissionCount { get; set; }

    public int ReadinessCount { get; set; }

    public int TrainingBlockerCount { get; set; }

    public int PersonnelNoteCount { get; set; }

    public int PersonnelDocumentCount { get; set; }

    public DateTimeOffset? LastEventAt { get; set; }

    public DateTimeOffset ComputedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<PersonnelHistoryEvent> Events { get; set; } = [];
}

public sealed class PersonnelHistoryEvent : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid PersonId { get; set; }

    public Guid RollupId { get; set; }

    public string EntryId { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string EventType { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Detail { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    public Guid? ActorUserId { get; set; }

    public string SourceEntityType { get; set; } = string.Empty;

    public string SourceEntityId { get; set; } = string.Empty;

    public string? ExternalReferenceId { get; set; }

    public PersonnelHistoryRollup Rollup { get; set; } = null!;
}
