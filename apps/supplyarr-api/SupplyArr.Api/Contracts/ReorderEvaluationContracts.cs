namespace SupplyArr.Api.Contracts;

public sealed record PartReorderPolicyResponse(
    Guid PartId,
    string PartKey,
    string DisplayName,
    decimal? ReorderPoint,
    decimal? ReorderQuantity,
    DateTimeOffset UpdatedAt);

public sealed record UpsertPartReorderPolicyRequest(
    decimal? ReorderPoint,
    decimal? ReorderQuantity);

public sealed record ReorderSuggestionResponse(
    Guid PartId,
    string PartKey,
    string DisplayName,
    string UnitOfMeasure,
    decimal ReorderPoint,
    decimal? ReorderQuantity,
    decimal QuantityOnHand,
    decimal QuantityReserved,
    decimal QuantityAvailable,
    decimal SuggestedOrderQuantity,
    Guid? PreferredVendorPartyId,
    string? PreferredVendorPartyKey,
    string? PreferredVendorDisplayName,
    bool HasOpenPurchaseRequest,
    string? SkipReason);

public sealed record ReorderEvaluationResponse(
    DateTimeOffset EvaluatedAt,
    IReadOnlyList<ReorderSuggestionResponse> Suggestions);

public sealed record CreatePurchaseRequestFromReorderRequest(
    string RequestKey,
    string Title,
    string Notes,
    IReadOnlyList<Guid> PartIds);

public sealed record PendingReorderEvaluationItem(
    Guid PartId,
    string PartKey,
    decimal ReorderPoint,
    decimal QuantityAvailable,
    decimal SuggestedOrderQuantity,
    bool HasOpenPurchaseRequest);

public sealed record PendingReorderEvaluationResponse(
    DateTimeOffset EvaluatedAt,
    int BatchSize,
    IReadOnlyList<PendingReorderEvaluationItem> Items);

public sealed record ProcessReorderEvaluationRequest(
    Guid? TenantId,
    int? BatchSize,
    bool CreateDraftPurchaseRequests);

public sealed record ProcessReorderEvaluationResponse(
    DateTimeOffset EvaluatedAt,
    int BatchSize,
    int CandidatesFound,
    int SuggestionsCount,
    int SkippedOpenPurchaseRequestCount,
    int DraftPurchaseRequestsCreated,
    IReadOnlyList<Guid> CreatedPurchaseRequestIds,
    IReadOnlyList<ReorderSuggestionResponse> Suggestions);
