namespace RoutArr.Api.Contracts;

public sealed record ProofDvirReportCountItem(string Key, int Count);

public sealed record ProofDvirReportTripSummaryItem(
    Guid TripId,
    string TripNumber,
    string Title,
    string DispatchStatus,
    string? AssignedDriverPersonId,
    string? VehicleRefKey,
    int ProofCount,
    bool HasPreTripDvir,
    bool HasPostTripDvir,
    int MissingRequiredProofCount,
    int FailOrConditionalDvirCount);

public sealed record ProofDvirReportProofRow(
    Guid ProofId,
    Guid TripId,
    string TripNumber,
    string ProofType,
    string CapturedByPersonId,
    string? VehicleRefKey,
    string ReferenceKey,
    DateTimeOffset CapturedAt);

public sealed record ProofDvirReportDvirRow(
    Guid DvirId,
    Guid TripId,
    string TripNumber,
    string Phase,
    string Result,
    string VehicleRefKey,
    string SubmittedByPersonId,
    DateTimeOffset SubmittedAt);

public sealed record ProofDvirReportSummaryResponse(
    DateTimeOffset GeneratedAt,
    string Scope,
    DateTimeOffset WindowStart,
    DateTimeOffset WindowEnd,
    int TotalProofCount,
    int TotalDvirCount,
    int TripWithProofOrDvirCount,
    int MissingProofTripCount,
    int PreTripDvirCount,
    int PostTripDvirCount,
    int FailOrConditionalDvirCount,
    IReadOnlyList<ProofDvirReportCountItem> ProofTypeCounts,
    IReadOnlyList<ProofDvirReportCountItem> DvirPhaseCounts,
    IReadOnlyList<ProofDvirReportCountItem> DvirResultCounts,
    IReadOnlyList<ProofDvirReportTripSummaryItem> Trips,
    IReadOnlyList<ProofDvirReportProofRow> RecentProofs,
    IReadOnlyList<ProofDvirReportDvirRow> RecentDvirInspections);

public sealed record ProofDvirReportTripDetailResponse(
    Guid TripId,
    string TripNumber,
    string Title,
    string DispatchStatus,
    string? AssignedDriverPersonId,
    string? VehicleRefKey,
    DateTimeOffset? ScheduledStartAt,
    DateTimeOffset? ScheduledEndAt,
    int ProofCount,
    bool HasPreTripDvir,
    bool HasPostTripDvir,
    int MissingRequiredProofCount,
    int FailOrConditionalDvirCount,
    IReadOnlyList<ProofDvirReportProofRow> Proofs,
    IReadOnlyList<ProofDvirReportDvirRow> DvirInspections);

public sealed record ProofDvirReportProofDetailResponse(
    Guid ProofId,
    Guid TripId,
    string TripNumber,
    string TripTitle,
    string ProofType,
    string CapturedByPersonId,
    string? VehicleRefKey,
    string ReferenceKey,
    string Notes,
    DateTimeOffset CapturedAt,
    DateTimeOffset CreatedAt);

public sealed record ProofDvirReportDvirDetailResponse(
    Guid DvirId,
    Guid TripId,
    string TripNumber,
    string TripTitle,
    string Phase,
    string VehicleRefKey,
    string Result,
    long? OdometerReading,
    string DefectNotes,
    string SubmittedByPersonId,
    DateTimeOffset SubmittedAt,
    DateTimeOffset CreatedAt);
