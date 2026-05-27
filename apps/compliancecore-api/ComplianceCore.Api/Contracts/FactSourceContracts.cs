using System.Text.Json;

namespace ComplianceCore.Api.Contracts;

public sealed record FactSourceResponse(
    Guid FactSourceId,
    Guid FactDefinitionId,
    string FactKey,
    string FactLabel,
    string SourceKey,
    string SourceType,
    string Label,
    string Description,
    string? ProductKey,
    string? ProductReference,
    string ConfigJson,
    int Priority,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateFactSourceRequest(
    Guid FactDefinitionId,
    string SourceKey,
    string SourceType,
    string Label,
    string Description,
    string? ProductKey,
    string? ProductReference,
    string ConfigJson,
    int Priority);

public sealed record InternalResolveFactsRequest(
    Guid TenantId,
    IReadOnlyList<string> FactKeys,
    IReadOnlyDictionary<string, string>? Context);

public sealed record InternalValidateFactsRequest(
    Guid TenantId,
    IReadOnlyList<string> FactKeys);

public sealed record ResolvedFactValue(
    string FactKey,
    string ValueType,
    JsonElement? Value,
    string SourceType,
    string SourceKey,
    bool FromContext);

public sealed record InternalResolveFactsResponse(
    Guid TenantId,
    IReadOnlyList<ResolvedFactValue> Resolved,
    IReadOnlyList<string> UnresolvedFactKeys);

public sealed record FactValidationItem(
    string FactKey,
    bool CanResolve,
    string? Message);

public sealed record InternalValidateFactsResponse(
    Guid TenantId,
    bool IsValid,
    IReadOnlyList<FactValidationItem> Results);
