using STLCompliance.Shared.Data;

namespace StaffArr.Api.Entities;

public sealed class PersonTrainingAcknowledgement : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid PersonId { get; set; }

    public Guid TrainarrAcknowledgementRequestId { get; set; }

    public Guid TrainarrAssignmentId { get; set; }

    public string TrainingTitle { get; set; } = string.Empty;

    public string AssignmentReason { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string Status { get; set; } = TrainingAcknowledgementStatuses.Pending;

    public DateTimeOffset? DueAt { get; set; }

    public DateTimeOffset RequestedAt { get; set; }

    public DateTimeOffset? AcknowledgedAt { get; set; }

    public Guid? AcknowledgedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public static class TrainingAcknowledgementStatuses
{
    public const string Pending = "pending";

    public const string Acknowledged = "acknowledged";

    public const string Superseded = "superseded";

    public const string Cancelled = "cancelled";

    public static readonly IReadOnlySet<string> Open = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Pending,
    };
}
