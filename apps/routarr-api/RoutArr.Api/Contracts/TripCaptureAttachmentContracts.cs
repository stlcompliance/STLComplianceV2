namespace RoutArr.Api.Contracts;

public sealed record TripCaptureAttachmentResponse(
    Guid AttachmentId,
    Guid TripId,
    string SubjectType,
    Guid SubjectId,
    string AttachmentKind,
    string FileName,
    string ContentType,
    long SizeBytes,
    string? Notes,
    string CapturedByPersonId,
    DateTimeOffset CreatedAt);

public sealed record UploadTripCaptureAttachmentRequest(
    string AttachmentKind,
    string FileName,
    string ContentType,
    string ContentBase64,
    string? Notes);

public sealed record TripCaptureAttachmentListResponse(
    Guid TripId,
    string SubjectType,
    Guid SubjectId,
    IReadOnlyList<TripCaptureAttachmentResponse> Items);
