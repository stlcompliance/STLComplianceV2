namespace ComplianceCore.Api.Contracts;

public sealed record ProductFactPublicationItemRequest(
    string FactKey,
    string ValueType,
    string ScopeKey,
    string? StringValue,
    bool? BooleanValue,
    decimal? NumberValue,
    string? DateValue,
    string SourceEntityType,
    Guid? SourceEntityId,
    string SourceEventKind,
    string IdempotencyKey);

public sealed record IngestProductFactsRequest(
    Guid TenantId,
    Guid PublicationId,
    string SourceProduct,
    DateTimeOffset PublishedAt,
    IReadOnlyList<ProductFactPublicationItemRequest> Facts);

public sealed record IngestProductFactsResponse(
    Guid TenantId,
    Guid PublicationId,
    int AcceptedCount,
    int SkippedDuplicateCount);

public sealed record ProductFactMirrorResponse(
    Guid MirrorId,
    string SourceProduct,
    string FactKey,
    string ScopeKey,
    string ValueType,
    string? StringValue,
    bool? BooleanValue,
    decimal? NumberValue,
    string? DateValue,
    string SourceEntityType,
    Guid? SourceEntityId,
    string SourceEventKind,
    DateTimeOffset PublishedAt);
