namespace NexArr.Api.Contracts;

public static class CompanionFieldEvidenceCaptureKinds
{
    public const string Photo = "photo";
    public const string Document = "document";
    public const string Signature = "signature";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Photo,
        Document,
        Signature,
    };
}

public sealed record SubmitCompanionFieldEvidenceRequest(
    string TaskKey,
    string CaptureKind,
    string FileName,
    string ContentType,
    string ContentBase64,
    string? Notes);

public sealed record CompanionFieldEvidenceResponse(
    string TaskKey,
    string ProductKey,
    Guid EvidenceId,
    string EvidenceTypeKey,
    string FileName,
    string ContentType,
    long SizeBytes,
    string? Notes,
    DateTimeOffset CreatedAt);
