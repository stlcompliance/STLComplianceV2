using STLCompliance.Shared.Data;

namespace TrainArr.Api.Entities;

public sealed class TrainingSignoff : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid TrainingAssignmentId { get; set; }

    public TrainingAssignment TrainingAssignment { get; set; } = null!;

    public string SignoffRole { get; set; } = string.Empty;

    public Guid SignedByUserId { get; set; }

    public string? Notes { get; set; }

    public DateTimeOffset SignedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
