namespace StaffArr.Api.Contracts;

public sealed record ProductPermissionCatalogItemRequest(
    string PermissionKey,
    string Label,
    string? Description,
    string Scope,
    string Sensitivity,
    string Status = "active");

public sealed record SyncProductPermissionCatalogRequest(
    Guid TenantId,
    string ProductKey,
    IReadOnlyList<ProductPermissionCatalogItemRequest> Permissions);

public sealed record ProductPermissionCatalogItemResponse(
    Guid PermissionTemplateId,
    string ProductKey,
    string PermissionKey,
    string Label,
    string? Description,
    string Scope,
    string Sensitivity,
    string Status,
    DateTimeOffset LastSyncedAt);

public sealed record SyncProductPermissionCatalogResponse(
    Guid TenantId,
    string ProductKey,
    int UpsertedCount,
    DateTimeOffset SyncedAt,
    IReadOnlyList<ProductPermissionCatalogItemResponse> Permissions);
