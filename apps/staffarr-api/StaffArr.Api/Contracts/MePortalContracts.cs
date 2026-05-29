namespace StaffArr.Api.Contracts;

public sealed record MePortalPermissionSummaryResponse(
    int PermissionCount,
    IReadOnlyList<string> PermissionSummaries);

public sealed record MePortalCertificationSummaryResponse(
    int ActiveCount,
    int ExpiringSoonCount,
    int MissingRequirementCount,
    IReadOnlyList<PersonCertificationResponse> Highlights);

public sealed record MePortalReadinessSummaryResponse(
    string ReadinessStatus,
    string ReadinessBasis,
    IReadOnlyList<string> BlockerMessages);

public sealed record MePortalOnboardingSummaryResponse(
    string OverallStatus,
    int CompletedSteps,
    int TotalSteps,
    int BlockedSteps);

public sealed record MePortalSummaryResponse(
    StaffArrMeResponse Session,
    PersonLookupResponse Profile,
    MePortalReadinessSummaryResponse Readiness,
    MePortalCertificationSummaryResponse Certifications,
    MePortalPermissionSummaryResponse Permissions,
    MePortalOnboardingSummaryResponse? Onboarding,
    int DirectReportCount,
    IReadOnlyList<SubordinateSummaryResponse> DirectReportsPreview,
    IReadOnlyList<string> ProductAccess);

public sealed record PersonnelUpdateRequestResponse(
    Guid RequestId,
    Guid PersonId,
    string RequestType,
    string Status,
    string FieldKey,
    string? CurrentValue,
    string RequestedValue,
    string? Details,
    Guid SubmittedByUserId,
    DateTimeOffset SubmittedAt,
    Guid? ReviewedByUserId,
    DateTimeOffset? ReviewedAt,
    string? ReviewNotes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record SubmitPersonnelUpdateRequest(
    string RequestType,
    string FieldKey,
    string? CurrentValue,
    string RequestedValue,
    string? Details);

public sealed record ReviewPersonnelUpdateRequest(
    string Decision,
    string? ReviewNotes,
    bool ApplyToProfile = false);

public sealed record PersonnelUpdateRequestReviewResponse(
    PersonnelUpdateRequestResponse Request,
    bool AppliedToProfile);
