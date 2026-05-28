using STLCompliance.Shared.Data;

namespace StaffArr.Api.Entities;

public sealed class PersonExportDeliveryRun : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid ExportId { get; set; }

    public int PersonCount { get; set; }

    public string Status { get; set; } = "success";

    public int IntervalHours { get; set; }

    public string? EmploymentStatus { get; set; }

    public Guid? OrgUnitId { get; set; }

    public string? SkipReason { get; set; }

    public DateTimeOffset StartedAt { get; set; }

    public DateTimeOffset CompletedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
