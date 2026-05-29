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

public sealed record IncidentTrainarrRoutingResponse(
    string RoutingStatus,
    Guid TrainarrRemediationId,
    DateTimeOffset RoutedAt,
    Guid RoutedByUserId);

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
    IncidentTrainarrRoutingResponse? TrainarrRouting);

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
    IncidentTrainarrRoutingResponse? TrainarrRouting);

public sealed record RouteIncidentToTrainarrResponse(
    Guid IncidentId,
    Guid PersonId,
    string ReasonCategoryKey,
    string Status,
    IncidentTrainarrRoutingResponse TrainarrRouting);
