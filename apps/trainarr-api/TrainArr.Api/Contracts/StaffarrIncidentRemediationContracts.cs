namespace TrainArr.Api.Contracts;

public sealed record IngestStaffarrIncidentRemediationRequest(
    Guid TenantId,
    Guid StaffarrIncidentId,
    Guid StaffarrPersonId,
    string ReasonCategoryKey,
    string Severity,
    string Title,
    string Description,
    DateTimeOffset OccurredAt,
    DateTimeOffset ReportedAt);

public sealed record StaffarrIncidentRemediationResponse(
    Guid RemediationId,
    Guid TenantId,
    Guid StaffarrIncidentId,
    Guid StaffarrPersonId,
    string ReasonCategoryKey,
    string Status,
    DateTimeOffset CreatedAt);

public sealed record StaffarrIncidentRemediationDetailResponse(
    Guid RemediationId,
    Guid TenantId,
    Guid StaffarrIncidentId,
    Guid StaffarrPersonId,
    string ReasonCategoryKey,
    string Severity,
    string Title,
    string Description,
    string Status,
    DateTimeOffset OccurredAt,
    DateTimeOffset ReportedAt,
    DateTimeOffset CreatedAt);
