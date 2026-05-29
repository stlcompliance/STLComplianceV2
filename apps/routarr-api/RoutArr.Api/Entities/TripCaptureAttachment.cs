using STLCompliance.Shared.Data;

namespace RoutArr.Api.Entities;

public sealed class TripCaptureAttachment : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid TripId { get; set; }

    public string SubjectType { get; set; } = TripCaptureAttachmentSubjects.Proof;

    public Guid SubjectId { get; set; }

    public string AttachmentKind { get; set; } = TripCaptureAttachmentKinds.Photo;

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public string StorageKey { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public string CapturedByPersonId { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public Trip Trip { get; set; } = null!;
}

public static class TripCaptureAttachmentSubjects
{
    public const string Proof = "proof";

    public const string Dvir = "dvir";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Proof,
        Dvir,
    };
}

public static class TripCaptureAttachmentKinds
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
