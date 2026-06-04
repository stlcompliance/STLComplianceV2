using STLCompliance.Shared.Data;

namespace MaintainArr.Api.Entities;

public sealed class WorkOrderComment : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid WorkOrderId { get; set; }

    public string Body { get; set; } = string.Empty;

    public string Visibility { get; set; } = WorkOrderCommentVisibility.Internal;

    public DateTimeOffset CreatedAt { get; set; }

    public string? CreatedByPersonId { get; set; }

    public DateTimeOffset? EditedAt { get; set; }

    public string? EditedByPersonId { get; set; }

    public bool Pinned { get; set; }
}

public sealed class WorkOrderTimelineEvent : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid WorkOrderId { get; set; }

    public string EventType { get; set; } = string.Empty;

    public DateTimeOffset OccurredAt { get; set; }

    public string? ActorPersonId { get; set; }

    public string? ActorServiceClientId { get; set; }

    public string Summary { get; set; } = string.Empty;

    public string? SourceProduct { get; set; }

    public string? SourceObjectRef { get; set; }

    public string? BeforeSnapshot { get; set; }

    public string? AfterSnapshot { get; set; }
}

public static class WorkOrderCommentVisibility
{
    public const string Internal = "internal";

    public const string SupervisorOnly = "supervisor_only";

    public const string AuditorVisible = "auditor_visible";

    public const string VendorVisible = "vendor_visible";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Internal,
        SupervisorOnly,
        AuditorVisible,
        VendorVisible,
    };
}
