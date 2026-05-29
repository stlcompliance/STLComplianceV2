namespace NexArr.Api.Contracts;

public sealed record SubmitCompanionFieldDvirRequest(
    string TaskKey,
    string Phase,
    string Result,
    long? OdometerReading,
    string? DefectNotes,
    string? VehicleRefKey);

public sealed record CompanionFieldDvirResponse(
    string TaskKey,
    string ProductKey,
    Guid DvirId,
    Guid TripId,
    string Phase,
    string Result,
    long? OdometerReading,
    string DefectNotes,
    DateTimeOffset SubmittedAt);
