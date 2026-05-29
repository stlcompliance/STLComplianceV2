using STLCompliance.Shared.Data;

namespace StaffArr.Api.Entities;

public sealed class PersonOffboardingRecord : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid PersonId { get; set; }

    public string Status { get; set; } = OffboardingStatuses.InProgress;

    public DateTimeOffset SeparationDate { get; set; }

    public string? SeparationReason { get; set; }

    public string TargetEmploymentStatus { get; set; } = "inactive";

    public bool DisableLoginRequested { get; set; }

    public Guid? NewManagerPersonIdForReports { get; set; }

    public Guid StartedByUserId { get; set; }

    public DateTimeOffset StartedAt { get; set; }

    public Guid? CompletedByUserId { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public StaffPerson? Person { get; set; }

    public ICollection<PersonOffboardingStep> Steps { get; set; } = [];
}

public static class OffboardingStatuses
{
    public const string InProgress = "in_progress";

    public const string Completed = "completed";

    public const string Cancelled = "cancelled";
}

public static class OffboardingStepKeys
{
    public const string ReviewAccess = "review_access";

    public const string ReassignDirectReports = "reassign_direct_reports";

    public const string EndOrgAssignments = "end_org_assignments";

    public const string RevokePermissions = "revoke_permissions";

    public const string DisableLogin = "disable_login";

    public const string MarkInactive = "mark_inactive";

    public const string PreserveAudit = "preserve_audit";
}

public static class OffboardingStepStatuses
{
    public const string Pending = "pending";

    public const string Complete = "complete";

    public const string Blocked = "blocked";

    public const string Skipped = "skipped";
}
