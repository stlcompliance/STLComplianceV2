using System.Security.Claims;
using NexArr.Api.Entities;

namespace NexArr.Api.Services;

public interface ITokenService
{
    (string AccessToken, DateTimeOffset ExpiresAt) CreateAccessToken(
        PlatformUser user,
        Guid tenantId,
        Guid sessionId,
        IReadOnlyList<string> launchableProductKeys,
        int? accessTokenMinutes = null);

    (string AccessToken, DateTimeOffset ExpiresAt) CreateSessionAccessToken(
        PlatformUser user,
        Guid tenantId,
        Guid sessionId,
        IReadOnlyList<string> launchableProductKeys,
        string tenantRoleKey,
        Guid personId,
        int? accessTokenMinutes = null);

    string GenerateRefreshToken();

    string HashRefreshToken(string refreshToken);
}
