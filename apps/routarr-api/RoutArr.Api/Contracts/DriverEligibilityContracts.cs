namespace RoutArr.Api.Contracts;

public sealed record DriverEligibilityCheckRequest(
    string PersonId,
    string? QualificationKey = null,
    string? RulePackKey = null);

public sealed record DriverEligibilityTrainArrSummary(
    string Outcome,
    string ReasonCode,
    string Message,
    string? QualificationKey);

public sealed record DriverEligibilityStaffArrSummary(
    string ReadinessStatus,
    string ReadinessBasis,
    int BlockerCount,
    string? PrimaryBlockerMessage);

public sealed record DriverEligibilityCheckResponse(
    string PersonId,
    string Outcome,
    string ReasonCode,
    string Message,
    bool IsBlocking,
    DriverEligibilityTrainArrSummary? TrainArr,
    DriverEligibilityStaffArrSummary? StaffArr);

public sealed record DispatchAssignmentEligibilitySummary(
    string Outcome,
    string ReasonCode,
    string Message,
    bool IsBlocking,
    DriverEligibilityTrainArrSummary? TrainArr,
    DriverEligibilityStaffArrSummary? StaffArr);

public static class DriverEligibilityOutcomes
{
    public const string Allow = "allow";

    public const string Warn = "warn";

    public const string Block = "block";
}
