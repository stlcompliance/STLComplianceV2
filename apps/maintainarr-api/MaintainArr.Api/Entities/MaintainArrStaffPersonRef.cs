using STLCompliance.Shared.Data;

namespace MaintainArr.Api.Entities;

/// <summary>
/// Rebuildable mirror of StaffArr person identifiers for technician assignment and labor capture.
/// </summary>
public sealed class MaintainArrStaffPersonRef : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string StaffarrPersonId { get; set; } = string.Empty;

    public string DisplayNameSnapshot { get; set; } = string.Empty;

    public string? ActiveStatusSnapshot { get; set; }

    public string? PrimarySiteSnapshot { get; set; }

    public DateTimeOffset LastSeenAt { get; set; }

    public string? SourceCorrelationId { get; set; }
}
