namespace StaffArr.Api.Contracts;

public sealed record MyTeamMemberResponse(
    SubordinateSummaryResponse Summary,
    string ReadinessStatus,
    int BlockerCount,
    int MissingCertificationCount,
    int ExpiringCertificationCount,
    int OpenIncidentCount,
    int PendingUpdateRequestCount,
    int PendingTrainingBlockerCount);

public sealed record MyTeamDashboardResponse(
    int DirectReportCount,
    int NotReadyCount,
    int MissingCertificationCount,
    int ExpiringCertificationCount,
    int OpenIncidentCount,
    int PendingUpdateRequestCount,
    int OnboardingInProgressCount,
    int PendingTrainingBlockerCount,
    IReadOnlyList<MyTeamMemberResponse> Members,
    IReadOnlyList<PersonnelUpdateRequestResponse> PendingUpdateRequests);
