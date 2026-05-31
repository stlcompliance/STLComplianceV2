namespace SupplyArr.Api.Contracts;

public sealed record ExternalReferenceResolutionResponse(
    string EntityType,
    string Key,
    Guid EntityId,
    string DisplayCode,
    string? DisplayLabel,
    DateTimeOffset ResolvedAt);

public sealed record ExternalReferenceEntityContract(
    string EntityType,
    string Description,
    string ResolvePathTemplate);

public sealed record ExternalReferenceContractIndexResponse(
    IReadOnlyList<ExternalReferenceEntityContract> Items,
    DateTimeOffset GeneratedAt);

public sealed record ExternalReferenceResolveRequest(
    string EntityType,
    string Key);

public sealed record ExternalReferenceBatchResolveRequest(
    IReadOnlyList<ExternalReferenceResolveRequest> Items);

public sealed record ExternalReferenceBatchResolveItemResponse(
    int Index,
    string EntityType,
    string Key,
    bool Found,
    ExternalReferenceResolutionResponse? Resolution);

public sealed record ExternalReferenceBatchResolveResponse(
    IReadOnlyList<ExternalReferenceBatchResolveItemResponse> Items,
    DateTimeOffset ResolvedAt);
