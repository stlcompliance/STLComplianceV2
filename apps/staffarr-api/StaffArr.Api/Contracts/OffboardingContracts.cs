namespace StaffArr.Api.Contracts;

public sealed record StartPersonOffboardingRequest(
    Guid PersonId,
    DateTimeOffset SeparationDate,
    string? SeparationReason,
    string TargetEmploymentStatus,
    bool DisableLoginRequested,
    Guid? NewManagerPersonIdForReports);

public sealed record ExecutePersonOffboardingRequest(
    Guid? NewManagerPersonIdForReports);

public sealed record PersonOffboardingStepResponse(
    string StepKey,
    string Title,
    string Detail,
    string Status,
    string? BlockerDetail,
    int SortOrder,
    DateTimeOffset? CompletedAt);

public sealed record PersonOffboardingResponse(
    Guid OffboardingId,
    Guid PersonId,
    string Status,
    DateTimeOffset SeparationDate,
    string? SeparationReason,
    string TargetEmploymentStatus,
    bool DisableLoginRequested,
    Guid? NewManagerPersonIdForReports,
    DateTimeOffset StartedAt,
    Guid StartedByUserId,
    DateTimeOffset? CompletedAt,
    Guid? CompletedByUserId,
    IReadOnlyList<PersonOffboardingStepResponse> Steps,
    int ActiveDirectReportCount,
    int OpenIncidentCount,
    int ActiveRoleAssignmentCount,
    int ActiveOrgAssignmentCount);
