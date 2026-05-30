namespace StaffArr.Api.Contracts;

public sealed record IntegrationPermissionCheckGrantResponse(
    string PermissionKey,
    string PermissionName,
    string ScopeType,
    string? ScopeValue,
    string RoleKey,
    string RoleName);

public sealed record IntegrationPermissionCheckItemResponse(
    string PermissionKey,
    bool Granted,
    IReadOnlyList<IntegrationPermissionCheckGrantResponse> Grants);

public sealed record IntegrationPermissionCheckResponse(
    Guid PersonId,
    Guid? ExternalUserId,
    bool IsPersonActive,
    DateTimeOffset ComputedAt,
    bool IsAuthorizedAll,
    bool IsAuthorizedAny,
    IReadOnlyList<IntegrationPermissionCheckItemResponse> Checks);
