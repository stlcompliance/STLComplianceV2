namespace SupplyArr.Api.Contracts;

public sealed record SupplierOnboardingResponse(
    Guid OnboardingId,
    Guid SupplierId,
    string SupplierKey,
    string SupplierUnitKind,
    Guid? ParentSupplierId,
    string? ParentSupplierDisplayName,
    string DisplayName,
    string OnboardingStatus,
    string Notes,
    DateTimeOffset? SubmittedAt,
    DateTimeOffset? ReviewedAt,
    string RejectionReason,
    IReadOnlyList<OnboardingDocumentRequirementStatus> DocumentRequirements,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public Guid ExternalPartyId => SupplierId;

    public string PartyKey => SupplierKey;
}

public sealed record OnboardingDocumentRequirementStatus(
    string DocumentTypeKey,
    string Label,
    bool IsRequired,
    bool IsSatisfied,
    Guid? SatisfyingDocumentId,
    string? SatisfyingReviewStatus);

public sealed record StartSupplierOnboardingRequest(
    Guid SupplierId,
    string? Notes,
    Guid? ExternalPartyId = null);

public sealed record UpdateSupplierOnboardingNotesRequest(string Notes);

public sealed record SubmitSupplierOnboardingForReviewRequest(string? Notes);

public sealed record RejectSupplierOnboardingRequest(string Reason);

public sealed record SuspendSupplierOnboardingRequest(string? Reason);

public sealed record SupplierOnboardingDocumentRequirementsResponse(
    IReadOnlyList<OnboardingDocumentRequirementDefinition> Requirements);

public sealed record OnboardingDocumentRequirementDefinition(
    string DocumentTypeKey,
    string Label,
    bool IsRequired);

public sealed record UpsertSupplierOnboardingDocumentRequirementsRequest(
    IReadOnlyList<string> RequiredDocumentTypeKeys);

public record SupplierComplianceDocumentRegistrationRequest(
    string DocumentKey,
    string DocumentTypeKey,
    string Title,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset? EffectiveAt,
    string FileName,
    string ContentType,
    long SizeBytes,
    string Notes,
    string? ContentBase64 = null);

public sealed record RegisterPartyComplianceDocumentRequest(
    string DocumentKey,
    string DocumentTypeKey,
    string Title,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset? EffectiveAt,
    string FileName,
    string ContentType,
    long SizeBytes,
    string Notes,
    string? ContentBase64 = null)
    : SupplierComplianceDocumentRegistrationRequest(
        DocumentKey,
        DocumentTypeKey,
        Title,
        ExpiresAt,
        EffectiveAt,
        FileName,
        ContentType,
        SizeBytes,
        Notes,
        ContentBase64);

public record SupplierComplianceDocumentResponse(
    Guid DocumentId,
    Guid SupplierId,
    string SupplierKey,
    string SupplierDisplayName,
    string DocumentKey,
    string DocumentTypeKey,
    string Title,
    int Version,
    string ReviewStatus,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset? EffectiveAt,
    string FileName,
    string ContentType,
    long SizeBytes,
    string Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public Guid ExternalPartyId => SupplierId;
}

public sealed record PartyComplianceDocumentResponse(
    Guid DocumentId,
    Guid SupplierId,
    string SupplierKey,
    string SupplierDisplayName,
    string DocumentKey,
    string DocumentTypeKey,
    string Title,
    int Version,
    string ReviewStatus,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset? EffectiveAt,
    string FileName,
    string ContentType,
    long SizeBytes,
    string Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
    : SupplierComplianceDocumentResponse(
        DocumentId,
        SupplierId,
        SupplierKey,
        SupplierDisplayName,
        DocumentKey,
        DocumentTypeKey,
        Title,
        Version,
        ReviewStatus,
        ExpiresAt,
        EffectiveAt,
        FileName,
        ContentType,
        SizeBytes,
        Notes,
        CreatedAt,
        UpdatedAt);

public record RejectSupplierComplianceDocumentRequest(string Reason);

public sealed record RejectPartyComplianceDocumentRequest(string Reason)
    : RejectSupplierComplianceDocumentRequest(Reason);
