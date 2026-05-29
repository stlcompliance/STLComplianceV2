namespace RoutArr.Api.Contracts;

public sealed record DispatchExceptionResolutionTemplateResponse(
    string TemplateKey,
    string Label,
    string DefaultResolutionNotes);

public sealed record BulkAssignDispatchExceptionsRequest(
    IReadOnlyList<Guid> ExceptionIds,
    Guid AssignedToUserId,
    DateTimeOffset? SlaDueAt);

public sealed record BulkResolveDispatchExceptionsRequest(
    IReadOnlyList<Guid> ExceptionIds,
    string? ResolutionNotes,
    string? ResolutionTemplateKey);

public sealed record BulkDispatchExceptionActionResult(
    Guid ExceptionId,
    bool Success,
    string? ErrorCode,
    string? ErrorMessage,
    DispatchExceptionSummaryResponse? Exception);

public sealed record BulkDispatchExceptionActionResponse(
    int TotalCount,
    int SuccessCount,
    int FailureCount,
    IReadOnlyList<BulkDispatchExceptionActionResult> Results);
