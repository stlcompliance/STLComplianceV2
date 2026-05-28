using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class PartyComplianceDocument : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid ExternalPartyId { get; set; }

    public string DocumentKey { get; set; } = string.Empty;

    public string DocumentTypeKey { get; set; } = "other";

    public string Title { get; set; } = string.Empty;

    public int Version { get; set; } = 1;

    public string ReviewStatus { get; set; } = PartyComplianceDocumentReviewStatuses.Pending;

    public DateTimeOffset? ExpiresAt { get; set; }

    public DateTimeOffset? EffectiveAt { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = "application/octet-stream";

    public long SizeBytes { get; set; }

    public string? StorageKey { get; set; }

    public string Notes { get; set; } = string.Empty;

    public Guid? UploadedByUserId { get; set; }

    public Guid? ReviewedByUserId { get; set; }

    public DateTimeOffset? ReviewedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ExternalParty ExternalParty { get; set; } = null!;
}
