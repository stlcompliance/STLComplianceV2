namespace MaintainArr.Api.Contracts;

public sealed record TechnicianRefResponse(
    string PersonId,
    string DisplayName,
    string? ActiveStatus,
    string? PrimarySite,
    DateTimeOffset LastSeenAt);

public sealed record TechnicianRefListResponse(
    IReadOnlyList<TechnicianRefResponse> Items);

public sealed record UpsertTechnicianRefRequest(
    string PersonId,
    string DisplayName,
    string? ActiveStatus,
    string? PrimarySite,
    DateTimeOffset? SourceUpdatedAt,
    string? SourceCorrelationId);
