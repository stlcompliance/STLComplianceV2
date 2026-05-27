using STLCompliance.Shared.Data;

namespace StaffArr.Api.Entities;

public sealed class PersonnelIncident : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid PersonId { get; set; }

    public string ReasonCategoryKey { get; set; } = string.Empty;

    public string Severity { get; set; } = "medium";

    public string Status { get; set; } = "open";

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTimeOffset OccurredAt { get; set; }

    public DateTimeOffset ReportedAt { get; set; }

    public Guid ReportedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
