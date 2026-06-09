namespace MaintainArr.Api.Contracts;

public sealed record ExternalIntelligenceProviderSummaryResponse(
    string ProviderKey,
    string DisplayName,
    string Description,
    string SourceOfTruth,
    string Status,
    bool SupportsVinDecode,
    bool SupportsRecallLookup,
    bool SupportsComplaintLookup,
    bool SupportsReferenceLookups,
    bool SupportsEquipmentReferences,
    DateTimeOffset? LastCheckedAt,
    DateTimeOffset? LastSuccessfulAt,
    string? LastError);

public sealed record ExternalProviderHealthResponse(
    string ProviderKey,
    string Status,
    string Message,
    DateTimeOffset CheckedAt,
    int? LatencyMs);

public sealed record ExternalAssetIdentifierResponse(
    Guid IdentifierId,
    Guid AssetId,
    string SourceSystem,
    string IdentifierType,
    string IdentifierValue,
    string NormalizedValue,
    bool IsPrimary,
    bool IsVerified,
    IReadOnlyDictionary<string, string?>? Metadata,
    DateTimeOffset ObservedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record AssetEnrichmentSnapshotResponse(
    Guid SnapshotId,
    Guid AssetId,
    string ProviderKey,
    string SnapshotType,
    string? SourceObjectRef,
    string Summary,
    IReadOnlyDictionary<string, string?>? Details,
    DateTimeOffset CapturedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record AssetEnrichmentSuggestionResponse(
    Guid SuggestionId,
    Guid AssetId,
    Guid? SnapshotId,
    string ProviderKey,
    string FieldKey,
    string FieldLabel,
    string? CurrentValue,
    string? ProposedValue,
    string Reason,
    double Confidence,
    string Status,
    string? ReviewedByPersonId,
    DateTimeOffset? ReviewedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record AssetRecallSnapshotResponse(
    Guid RecallId,
    Guid AssetId,
    string ProviderKey,
    string CampaignNumber,
    string? ActionNumber,
    string Manufacturer,
    string Component,
    string Summary,
    string Consequence,
    string Remedy,
    string Notes,
    string? ModelYear,
    string? Make,
    string? Model,
    string? ReportReceivedDate,
    string Status,
    Guid? QualityHoldId,
    DateTimeOffset CapturedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record AssetComplaintSignalResponse(
    string OdiNumber,
    string? Manufacturer,
    bool Crash,
    bool Fire,
    int? NumberOfInjuries,
    int? NumberOfDeaths,
    string? DateOfIncident,
    string? DateComplaintFiled,
    string? Vin,
    IReadOnlyList<string> Components,
    string Summary);

public sealed record AssetExternalIntelligenceSummaryResponse(
    int IdentifierCount,
    int SnapshotCount,
    int SuggestionCount,
    int ActiveRecallCount,
    int ComplaintCount,
    DateTimeOffset? LastRefreshedAt);

public sealed record AssetExternalIntelligenceOverviewResponse(
    Guid AssetId,
    string? Vin,
    ExternalIntelligenceProviderSummaryResponse[] Providers,
    AssetExternalIntelligenceSummaryResponse Summary,
    IReadOnlyList<ExternalAssetIdentifierResponse> Identifiers,
    IReadOnlyList<AssetEnrichmentSnapshotResponse> Snapshots,
    IReadOnlyList<AssetEnrichmentSuggestionResponse> Suggestions,
    IReadOnlyList<AssetRecallSnapshotResponse> Recalls,
    IReadOnlyList<AssetComplaintSignalResponse> Complaints);

public sealed record ExternalVinDecodeRequest(
    string Vin,
    int? ModelYear = null);

public sealed record ExternalVinDecodeBatchItemRequest(
    string Vin,
    int? ModelYear = null);

public sealed record ExternalVinDecodeBatchRequest(
    IReadOnlyList<ExternalVinDecodeBatchItemRequest> Items);

public sealed record ExternalVinDecodeResponse(
    string ProviderKey,
    string Vin,
    string NormalizedVin,
    int? ModelYear,
    bool IsPartial,
    string? SearchCriteria,
    string? Message,
    string? ErrorCode,
    string? ErrorText,
    string? AdditionalErrorText,
    IReadOnlyDictionary<string, string?> DecodedFields,
    IReadOnlyList<AssetEnrichmentSuggestionResponse> Suggestions,
    IReadOnlyList<ExternalAssetIdentifierResponse> Identifiers,
    Guid? SnapshotId,
    DateTimeOffset? CapturedAt);

public sealed record ExternalVinDecodeBatchItemResponse(
    string Vin,
    int? ModelYear,
    ExternalVinDecodeResponse? Result,
    string? Error);
