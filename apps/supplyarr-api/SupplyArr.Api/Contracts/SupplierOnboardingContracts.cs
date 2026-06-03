namespace SupplyArr.Api.Contracts;

public sealed record SupplierOnboardingResponse(
    Guid OnboardingId,
    Guid ExternalPartyId,
    string PartyKey,
    string PartyType,
    string DisplayName,
    string OnboardingStatus,
    string Notes,
    DateTimeOffset? SubmittedAt,
    DateTimeOffset? ReviewedAt,
    string RejectionReason,
    IReadOnlyList<OnboardingDocumentRequirementStatus> DocumentRequirements,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record OnboardingDocumentRequirementStatus(
    string DocumentTypeKey,
    string Label,
    bool IsRequired,
    bool IsSatisfied,
    Guid? SatisfyingDocumentId,
    string? SatisfyingReviewStatus);

public sealed record StartSupplierOnboardingRequest(Guid ExternalPartyId, string? Notes);

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
    string? ContentBase64 = null);

public sealed record PartyComplianceDocumentResponse(
    Guid DocumentId,
    Guid ExternalPartyId,
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
    DateTimeOffset UpdatedAt);

public sealed record RejectPartyComplianceDocumentRequest(string Reason);
