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

    public string? Notes { get; set; }

    public Guid LoggedByUserId { get; set; }

    public DateTimeOffset LoggedAt { get; set; }

    public WorkOrder WorkOrder { get; set; } = null!;

    public WorkOrderTaskLine? WorkOrderTaskLine { get; set; }
}

public static class WorkOrderLaborTypes
{
    public const string Regular = "regular";

    public const string Overtime = "overtime";

    public const string Travel = "travel";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Regular,
        Overtime,
        Travel,
    };
}
