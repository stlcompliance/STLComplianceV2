namespace NexArr.Api.Contracts;

public sealed record DataPlaneProfileResponse(
    Guid ProfileId,
    Guid TenantId,
    string TenantSlug,
    string TenantDisplayName,
    string ProductKey,
    string ProductDisplayName,
    string DeploymentMode,
    string? DataEndpointUrl,
    string TrustStatus,
    string? Notes,
    DateTimeOffset ModifiedAt);

public sealed record UpsertDataPlaneProfileRequest(
    Guid TenantId,
    string ProductKey,
    string DeploymentMode,
    string? DataEndpointUrl,
    string TrustStatus,
    string? Notes);

public sealed record DataPlaneDefaultProfileResponse(
    Guid TenantId,
    string ProductKey,
    string ProductDisplayName,
    string DeploymentMode,
    string TrustStatus);
