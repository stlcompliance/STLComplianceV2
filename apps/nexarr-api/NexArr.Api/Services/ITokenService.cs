using System.Security.Claims;
using NexArr.Api.Entities;

namespace NexArr.Api.Services;

public interface ITokenService
{
    (string AccessToken, DateTimeOffset ExpiresAt) CreateAccessToken(
        PlatformUser user,
        Guid tenantId,
        Guid sessionId,
        IReadOnlyList<string> entitlements);

    string GenerateRefreshToken();

    string HashRefreshToken(string refreshToken);
}
