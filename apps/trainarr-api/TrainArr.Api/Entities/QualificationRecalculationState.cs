using STLCompliance.Shared.Data;

namespace TrainArr.Api.Entities;

public sealed class QualificationRecalculationState : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid QualificationIssueId { get; set; }

    public Guid StaffarrPersonId { get; set; }

    public string QualificationKey { get; set; } = string.Empty;

    public string Outcome { get; set; } = string.Empty;

    public string ReasonCode { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string? RulePackKey { get; set; }

    public string? PreviousOutcome { get; set; }

    public DateTimeOffset ComputedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
