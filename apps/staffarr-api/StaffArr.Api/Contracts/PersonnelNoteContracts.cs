namespace StaffArr.Api.Contracts;

public sealed record CreatePersonnelNoteRequest(
    string CategoryKey,
    string VisibilityKey,
    string Subject,
    string Body);

public sealed record PersonnelNoteSummaryResponse(
    Guid NoteId,
    Guid PersonId,
    string CategoryKey,
    string VisibilityKey,
    string Subject,
    string Status,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record PersonnelNoteDetailResponse(
    Guid NoteId,
    Guid PersonId,
    string CategoryKey,
    string VisibilityKey,
    string Subject,
    string Body,
    string Status,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
