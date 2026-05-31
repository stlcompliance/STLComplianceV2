namespace StaffArr.Api.Contracts;

public sealed record CreatePersonnelIncidentRequest(
    Guid PersonId,
    string ReasonCategoryKey,
    string Severity,
    string Title,
    string Description,
    DateTimeOffset OccurredAt);

public sealed record SubmitSelfReportedPersonnelIncidentRequest(
    string ReasonCategoryKey,
    string Severity,
    string Title,
    string Description,
    DateTimeOffset OccurredAt);

public sealed record IngestProductIncidentRequest(
    Guid TenantId,
    string SourceProduct,
    Guid SourceIncidentId,
    string? SourceEventKind,
    Guid PersonId,
    string ReasonCategoryKey,
    string Severity,
    string Title,
    string Description,
    DateTimeOffset OccurredAt,
    string? SourceReferenceKey = null);

public sealed record IngestProductIncidentResponse(
    Guid IncidentId,
    Guid PersonId,
    string SourceProduct,
    Guid SourceIncidentId,
    string Status,
    bool IdempotentReplay);

public sealed record IncidentTrainarrRoutingResponse(
    string RoutingStatus,
    Guid TrainarrRemediationId,
    DateTimeOffset RoutedAt,
    Guid RoutedByUserId,
    IncidentTrainarrRemediationResultResponse? RemediationResult = null);

public sealed record IncidentTrainarrRemediationResultResponse(
    Guid RemediationId,
    string Status,
    string ReasonCategoryKey,
    string Severity,
    string Title,
    DateTimeOffset CreatedAt);

public sealed record PersonnelIncidentSummaryResponse(
    Guid IncidentId,
    Guid PersonId,
    string ReasonCategoryKey,
    string Severity,
    string Status,
    string Title,
    DateTimeOffset OccurredAt,
    DateTimeOffset ReportedAt,
    Guid ReportedByUserId,
    IncidentTrainarrRoutingResponse? TrainarrRouting,
    string? SourceProduct = null,
    Guid? SourceIncidentId = null,
    string? SourceEventKind = null,
    string? SourceReferenceKey = null);

public sealed record PersonnelIncidentDetailResponse(
    Guid IncidentId,
    Guid PersonId,
    string ReasonCategoryKey,
    string Severity,
    string Status,
    string Title,
    string Description,
    DateTimeOffset OccurredAt,
    DateTimeOffset ReportedAt,
    Guid ReportedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IncidentTrainarrRoutingResponse? TrainarrRouting,
    string? SourceProduct = null,
    Guid? SourceIncidentId = null,
    string? SourceEventKind = null,
    string? SourceReferenceKey = null);

public sealed record RouteIncidentToTrainarrResponse(
    Guid IncidentId,
    Guid PersonId,
    string ReasonCategoryKey,
    string Status,
    IncidentTrainarrRoutingResponse TrainarrRouting);
