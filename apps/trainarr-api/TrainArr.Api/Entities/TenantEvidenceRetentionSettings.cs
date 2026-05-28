using STLCompliance.Shared.Data;

namespace TrainArr.Api.Entities;

public sealed class TenantEvidenceRetentionSettings : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public bool IsEnabled { get; set; }

    public int RetentionDaysAfterAssignmentClose { get; set; } = 365;

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
