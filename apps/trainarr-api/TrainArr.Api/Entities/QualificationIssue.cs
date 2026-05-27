using STLCompliance.Shared.Data;

namespace TrainArr.Api.Entities;

public sealed class QualificationIssue : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid TrainingAssignmentId { get; set; }

    public TrainingAssignment TrainingAssignment { get; set; } = null!;

    public Guid StaffarrPersonId { get; set; }

    public string QualificationKey { get; set; } = string.Empty;

    public string QualificationName { get; set; } = string.Empty;

    public Guid GrantPublicationId { get; set; }

    public string Status { get; set; } = "issued";

    public DateTimeOffset IssuedAt { get; set; }

    public DateTimeOffset? ExpiresAt { get; set; }

    public DateTimeOffset? StatusChangedAt { get; set; }

    public string? LifecycleReason { get; set; }

    public Guid? LifecyclePublicationId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
