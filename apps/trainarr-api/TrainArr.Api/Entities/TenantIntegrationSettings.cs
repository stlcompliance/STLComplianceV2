using STLCompliance.Shared.Data;

namespace TrainArr.Api.Entities;

public sealed class TenantIntegrationSettings : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public bool StaffArrIntegrationEnabled { get; set; } = true;

    public bool StaffArrIncidentIntakeEnabled { get; set; } = true;

    public bool StaffArrPublicationDeliveryEnabled { get; set; } = true;

    public bool ComplianceCoreIntegrationEnabled { get; set; } = true;

    public bool ComplianceCoreQualificationChecksEnabled { get; set; } = true;

    public bool RoutarrIntegrationEnabled { get; set; } = true;

    public bool RoutarrQualificationDispatchEnabled { get; set; } = true;

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
