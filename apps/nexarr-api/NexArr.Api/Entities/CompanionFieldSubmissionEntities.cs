namespace NexArr.Api.Entities;

public sealed class CompanionFieldSubmission
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public string TaskKey { get; set; } = string.Empty;
    public string ProductKey { get; set; } = string.Empty;
    public string SubmissionKind { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? DetailMessage { get; set; }
    public DateTimeOffset ClientSubmittedAt { get; set; }
    public DateTimeOffset RecordedAt { get; set; }
}

public static class CompanionFieldSubmissionKinds
{
    public const string Acknowledge = "acknowledge";
    public const string Evidence = "evidence";
    public const string Dvir = "dvir";
    public const string Inspection = "inspection";
    public const string WorkOrder = "work-order";
    public const string Receiving = "receiving";
}

public static class CompanionFieldSubmissionStatuses
{
    public const string Synced = "synced";
    public const string Failed = "failed";
}
