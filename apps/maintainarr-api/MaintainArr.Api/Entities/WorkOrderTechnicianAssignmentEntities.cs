using STLCompliance.Shared.Data;

namespace MaintainArr.Api.Entities;

public sealed class WorkOrderTechnicianAssignment : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid WorkOrderId { get; set; }

    public string PersonId { get; set; } = string.Empty;

    public string AssignmentRole { get; set; } = WorkOrderTechnicianAssignmentRoles.Primary;

    public string Status { get; set; } = WorkOrderTechnicianAssignmentStatuses.Assigned;

    public DateTimeOffset AssignedAt { get; set; }

    public string? AssignedByPersonId { get; set; }

    public DateTimeOffset? AcceptedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public string RequiredQualificationRefsJson { get; set; } = "[]";

    public string QualificationCheckSnapshotJson { get; set; } = "[]";

    public WorkOrder WorkOrder { get; set; } = null!;
}

public static class WorkOrderTechnicianAssignmentRoles
{
    public const string Primary = "primary";
    public const string Helper = "helper";
    public const string Supervisor = "supervisor";
    public const string Inspector = "inspector";
    public const string Specialist = "specialist";
    public const string VendorContact = "vendor_contact";
}

public static class WorkOrderTechnicianAssignmentStatuses
{
    public const string Assigned = "assigned";
    public const string Accepted = "accepted";
    public const string Declined = "declined";
    public const string InProgress = "in_progress";
    public const string Completed = "completed";
    public const string Removed = "removed";
}
