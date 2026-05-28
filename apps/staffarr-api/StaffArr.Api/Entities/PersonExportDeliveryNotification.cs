using STLCompliance.Shared.Data;

namespace StaffArr.Api.Entities;

public sealed class PersonExportDeliveryNotification : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid? DeliveryRunId { get; set; }

    public string EventKind { get; set; } = string.Empty;

    public string DeliveryStatus { get; set; } = string.Empty;

    public string? WebhookHost { get; set; }

    public int? HttpStatusCode { get; set; }

    public string? ErrorMessage { get; set; }

    public Guid? ExportId { get; set; }

    public int? PersonCount { get; set; }

    public DateTimeOffset AttemptedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

public static class PersonExportDeliveryNotificationEventKinds
{
    public const string Success = "success";

    public const string Failure = "failure";
}

public static class PersonExportDeliveryNotificationStatuses
{
    public const string Sent = "sent";

    public const string Failed = "failed";

    public const string Skipped = "skipped";
}
