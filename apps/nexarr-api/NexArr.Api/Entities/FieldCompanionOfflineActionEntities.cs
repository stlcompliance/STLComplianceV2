namespace NexArr.Api.Entities;

public sealed class FieldCompanionOfflineAction
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public string ActionKind { get; set; } = string.Empty;
    public string TaskKey { get; set; } = string.Empty;
    public string ProductKey { get; set; } = string.Empty;
    public DateTimeOffset ClientCreatedAt { get; set; }
    public DateTimeOffset SyncedAt { get; set; }
    public string? PayloadJson { get; set; }
}

public static class FieldCompanionOfflineActionKinds
{
    public const string FieldInboxAcknowledge = "field_inbox.acknowledge";
    public const string StaffArrClockPunch = "staffarr.clock.punch";
}
