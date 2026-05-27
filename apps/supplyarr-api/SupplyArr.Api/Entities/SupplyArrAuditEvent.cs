namespace SupplyArr.Api.Entities;

public sealed class SupplyArrAuditEvent
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid? ActorUserId { get; set; }

    public string Action { get; set; } = string.Empty;

    public string TargetType { get; set; } = string.Empty;

    public string? TargetId { get; set; }

    public string Result { get; set; } = string.Empty;

    public string? ReasonCode { get; set; }

    public Guid CorrelationId { get; set; }

    public DateTimeOffset OccurredAt { get; set; }
}
