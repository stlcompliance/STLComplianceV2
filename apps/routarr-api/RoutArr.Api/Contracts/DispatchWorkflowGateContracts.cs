namespace RoutArr.Api.Contracts;

public sealed record DispatchWorkflowGateCheckRequest(
    Guid TripId,
    string? DriverPersonId = null,
    string? VehicleRefKey = null,
    string? AssignmentKind = null);

public sealed record DispatchWorkflowGateResultSummary(
    string GateKey,
    string Outcome,
    string ReasonCode,
    string Message,
    bool IsBlocking);

public sealed record DispatchWorkflowGateCheckResponse(
    Guid TripId,
    string Outcome,
    string ReasonCode,
    string Message,
    bool IsBlocking,
    IReadOnlyList<DispatchWorkflowGateResultSummary> Gates);

public sealed record DispatchAssignmentWorkflowGateSummary(
    string Outcome,
    string ReasonCode,
    string Message,
    bool IsBlocking,
    IReadOnlyList<DispatchWorkflowGateResultSummary> Gates);

public static class DispatchWorkflowGateOutcomes
{
    public const string Allow = "allow";

    public const string Warn = "warn";

    public const string Block = "block";
}
