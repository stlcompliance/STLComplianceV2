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
    string ResolutionNotes,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? AssignedAt,
    DateTimeOffset? ResolvedAt);

public sealed record DispatchExceptionListResponse(
    int TotalCount,
    int OpenCount,
    IReadOnlyList<DispatchExceptionSummaryResponse> Items);

public sealed record CreateDispatchExceptionRequest(
    string Title,
    string Description,
    string? Category,
    Guid? TripId);

public sealed record AssignDispatchExceptionRequest(Guid AssignedToUserId);

public sealed record ResolveDispatchExceptionRequest(string? ResolutionNotes);

public sealed record LinkDispatchExceptionTripRequest(Guid TripId);
