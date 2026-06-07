using STLCompliance.Shared.Data;

namespace MaintainArr.Api.Entities;

public sealed class WorkOrderLaborEntry : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid WorkOrderId { get; set; }

    public Guid? WorkOrderTaskLineId { get; set; }

    public string PersonId { get; set; } = string.Empty;

    public decimal HoursWorked { get; set; }

    public string LaborTypeKey { get; set; } = WorkOrderLaborTypes.Regular;

    public string Status { get; set; } = WorkOrderLaborStatuses.Submitted;

    public string? Notes { get; set; }

    public DateTimeOffset? SubmittedAt { get; set; }

    public string? ApprovedByPersonId { get; set; }

    public DateTimeOffset? ApprovedAt { get; set; }

    public string? RejectionReason { get; set; }

    public Guid LoggedByUserId { get; set; }

    public DateTimeOffset LoggedAt { get; set; }

    public WorkOrder WorkOrder { get; set; } = null!;

    public WorkOrderTaskLine? WorkOrderTaskLine { get; set; }
}

public static class WorkOrderLaborTypes
{
    public const string Regular = "regular";

    public const string Overtime = "overtime";

    public const string Diagnostic = "diagnostic";

    public const string Repair = "repair";

    public const string Inspection = "inspection";

    public const string Testing = "testing";

    public const string Calibration = "calibration";

    public const string Cleanup = "cleanup";

    public const string Admin = "admin";

    public const string Travel = "travel";

    public const string VendorCoordination = "vendor_coordination";

    public const string Waiting = "waiting";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Regular,
        Overtime,
        Diagnostic,
        Repair,
        Inspection,
        Testing,
        Calibration,
        Cleanup,
        Admin,
        Travel,
        VendorCoordination,
        Waiting,
    };
}

public static class WorkOrderLaborStatuses
{
    public const string Draft = "draft";

    public const string Submitted = "submitted";

    public const string Approved = "approved";

    public const string Rejected = "rejected";

    public const string Corrected = "corrected";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Draft,
        Submitted,
        Approved,
        Rejected,
        Corrected,
    };
}
