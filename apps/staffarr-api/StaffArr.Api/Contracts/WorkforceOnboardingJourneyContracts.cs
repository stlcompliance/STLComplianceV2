namespace StaffArr.Api.Contracts;

public sealed record WorkforceOnboardingJourneyStepResponse(
    string StepKey,
    string Title,
    string Detail,
    string Status,
    string? StatusReason);

public sealed record WorkforceOnboardingJourneyResponse(
    Guid PersonId,
    string JourneyKey,
    string OverallStatus,
    string OverallSummary,
    IReadOnlyList<WorkforceOnboardingJourneyStepResponse> Steps,
    string? TrainarrIntegrationNote);
