namespace NexArr.Api.Contracts;

public sealed record SubmitFieldCompanionFieldDvirRequest(
    string TaskKey,
    string Phase,
    string Result,
    long? OdometerReading,
    string? DefectNotes,
    string? VehicleRefKey);

public sealed record FieldCompanionFieldDvirResponse(
    string TaskKey,
    string ProductKey,
    Guid DvirId,
    Guid TripId,
    string Phase,
    string Result,
    long? OdometerReading,
    string DefectNotes,
    DateTimeOffset SubmittedAt);
