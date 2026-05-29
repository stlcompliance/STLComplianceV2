using STLCompliance.Shared.Data;

namespace StaffArr.Api.Entities;

public sealed class PersonOffboardingStep : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid OffboardingRecordId { get; set; }

    public string StepKey { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Detail { get; set; } = string.Empty;

    public string Status { get; set; } = OffboardingStepStatuses.Pending;

    public string? BlockerDetail { get; set; }

    public int SortOrder { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public Guid? CompletedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public PersonOffboardingRecord? OffboardingRecord { get; set; }
}
