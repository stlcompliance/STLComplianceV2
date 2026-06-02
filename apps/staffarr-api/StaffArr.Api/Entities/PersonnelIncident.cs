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

    public DateTimeOffset? DiscoveredAt { get; set; }

    public DateTimeOffset ReportedAt { get; set; }

    public Guid ReportedByUserId { get; set; }

    public Guid? ReporterPersonId { get; set; }

    public Guid? ManagerPersonId { get; set; }

    public string? IncidentSource { get; set; }

    public string? IncidentType { get; set; }

    public Guid? SiteOrgUnitId { get; set; }

    public Guid? DepartmentOrgUnitId { get; set; }

    public string? LocationDetail { get; set; }

    public string? WitnessPersonIdsJson { get; set; }

    public string? AdditionalInvolvedPersonIdsJson { get; set; }

    public bool EmployeeSelfReport { get; set; }

    public string? ImmediateActionsTaken { get; set; }

    public string? RootCause { get; set; }

    public string? CategoryKeysJson { get; set; }

    public string? ReadinessDecision { get; set; }

    public string? WorkRestriction { get; set; }

    public string? ReturnToWorkNeeded { get; set; }

    public string? PpeConcern { get; set; }

    public string? MedicalAttention { get; set; }

    public string? OutOfServiceRemoveFromDuty { get; set; }

    public string? FollowUpRequired { get; set; }

    public bool TrainingReviewRequired { get; set; }

    public string? TrainingReviewReason { get; set; }

    public string? RelatedAssetReference { get; set; }

    public string? RelatedWorkOrderReference { get; set; }

    public string? RelatedRouteReference { get; set; }

    public string? RelatedSupplierReference { get; set; }

    public string? RelatedDocumentReference { get; set; }

    public string? RelatedPolicyReference { get; set; }

    public bool EvidencePackageRequested { get; set; }

    public bool NotifyManager { get; set; }

    public bool NotifySafetyCompliance { get; set; }

    public bool NotifyHr { get; set; }

    public bool CreateFollowUpTask { get; set; }

    public DateTimeOffset? FollowUpDueAt { get; set; }

    public string? SourceProduct { get; set; }

    public Guid? SourceIncidentId { get; set; }

    public string? SourceEventKind { get; set; }

    public string? SourceReferenceKey { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
