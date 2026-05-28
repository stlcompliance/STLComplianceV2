namespace NexArr.Api.Contracts;

public sealed record FieldTaskSubmissionStatusItem(
    string TaskKey,
    string SubmissionKind,
    string Status,
    string? DetailMessage,
    DateTimeOffset RecordedAt);

public sealed record FieldTaskSubmissionStatusResponse(
    IReadOnlyList<FieldTaskSubmissionStatusItem> Items);
