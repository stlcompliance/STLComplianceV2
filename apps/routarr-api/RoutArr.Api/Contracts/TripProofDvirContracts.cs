namespace RoutArr.Api.Contracts;

public sealed record TripProofRecordResponse(
    Guid ProofId,
    Guid TripId,
    string ProofType,
    string CapturedByPersonId,
    string? VehicleRefKey,
    string ReferenceKey,
    string Notes,
    string ReviewStatus,
    string? ReviewedByPersonId,
    DateTimeOffset? ReviewedAt,
    string ReviewNotes,
    DateTimeOffset CapturedAt,
    DateTimeOffset CreatedAt,
    IReadOnlyList<TripCaptureAttachmentResponse> Attachments);

public sealed record CreateTripProofRequest(
    string ProofType,
    string? VehicleRefKey,
    string? ReferenceKey,
    string? Notes,
    DateTimeOffset? CapturedAt);

public sealed record CreateProofEventRequest(
    Guid TripId,
    string ProofType,
    string? VehicleRefKey,
    string? ReferenceKey,
    string? Notes,
    DateTimeOffset? CapturedAt);

public sealed record RejectTripProofRequest(string Reason);

public sealed record CorrectTripProofRequest(
    string? VehicleRefKey,
    string? ReferenceKey,
    string? Notes,
    DateTimeOffset? CapturedAt,
    string Reason);

public sealed record TripDvirInspectionResponse(
    Guid DvirId,
    Guid TripId,
    string Phase,
    string VehicleRefKey,
    string Result,
    long? OdometerReading,
    string DefectNotes,
    Guid? MaintainarrInboundEventId,
    Guid? MaintainarrDefectId,
    DateTimeOffset? MaintainarrEventRoutedAt,
    string MaintainarrEventRouteStatus,
    string SubmittedByPersonId,
    DateTimeOffset SubmittedAt,
    IReadOnlyList<TripCaptureAttachmentResponse> Attachments);

public sealed record SubmitTripDvirRequest(
    string Phase,
    string? VehicleRefKey,
    string Result,
    long? OdometerReading,
    string? DefectNotes);

public sealed record TripProofListResponse(
    Guid TripId,
    IReadOnlyList<TripProofRecordResponse> Items);

public sealed record TripDvirListResponse(
    Guid TripId,
    IReadOnlyList<TripDvirInspectionResponse> Items);

public sealed record TripExecutionSummaryResponse(
    Guid TripId,
    string TripNumber,
    string DispatchStatus,
    string? AssignedDriverPersonId,
    DateTimeOffset? ClosedAt,
    IReadOnlyList<TripProofRecordResponse> Proofs,
    IReadOnlyList<TripDvirInspectionResponse> DvirInspections,
    bool HasPreTripDvir,
    bool HasPostTripDvir);
