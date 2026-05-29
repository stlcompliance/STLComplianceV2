namespace RoutArr.Api.Contracts;

public sealed record DispatchExceptionSummaryResponse(
    Guid ExceptionId,
    string ExceptionKey,
    string Title,
    string Description,
    string Category,
    string Status,
    Guid? TripId,
    string? TripNumber,
    string? TripTitle,
    Guid? AssignedToUserId,
    DateTimeOffset? SlaDueAt,
    bool IsSlaBreached,
    string ResolutionTemplateKey,
    string ResolutionNotes,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? AssignedAt,
    DateTimeOffset? ResolvedAt);

public sealed record DispatchExceptionListResponse(
    int TotalCount,
    int OpenCount,
    int OverdueCount,
    IReadOnlyList<DispatchExceptionSummaryResponse> Items);

public sealed record CreateDispatchExceptionRequest(
    string Title,
    string Description,
    string? Category,
    Guid? TripId,
    Guid? AssignedToUserId,
    DateTimeOffset? SlaDueAt);

public sealed record AssignDispatchExceptionRequest(
    Guid AssignedToUserId,
    DateTimeOffset? SlaDueAt);

public sealed record ResolveDispatchExceptionRequest(
    string? ResolutionNotes,
    string? ResolutionTemplateKey);

public sealed record LinkDispatchExceptionTripRequest(Guid TripId);
