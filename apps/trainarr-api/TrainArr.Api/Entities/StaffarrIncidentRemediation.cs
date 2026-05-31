using STLCompliance.Shared.Data;

namespace TrainArr.Api.Entities;

public sealed class StaffarrIncidentRemediation : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid StaffarrIncidentId { get; set; }

    public Guid StaffarrPersonId { get; set; }

    public string SourceProduct { get; set; } = "staffarr";

    public Guid SourceIncidentId { get; set; }

    public string? SourceEventKind { get; set; }

    public string ReasonCategoryKey { get; set; } = string.Empty;

    public string Severity { get; set; } = "medium";

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTimeOffset OccurredAt { get; set; }

    public DateTimeOffset ReportedAt { get; set; }

    public string Status { get; set; } = "intake_received";

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
