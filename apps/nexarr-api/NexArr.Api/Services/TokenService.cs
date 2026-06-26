using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;

namespace NexArr.Api.Services;

public sealed class TokenService(IOptions<StlJwtOptions> options, IConfiguration configuration) : ITokenService
{
    private readonly StlJwtOptions _options = options.Value;

    public (string AccessToken, DateTimeOffset ExpiresAt) CreateAccessToken(
        PlatformUser user,
        Guid tenantId,
        Guid sessionId,
        IReadOnlyList<string> launchableProductKeys,
        int? accessTokenMinutes = null) =>
        CreateSessionAccessToken(user, tenantId, sessionId, launchableProductKeys, string.Empty, user.Id, accessTokenMinutes);

    public (string AccessToken, DateTimeOffset ExpiresAt) CreateSessionAccessToken(
        PlatformUser user,
        Guid tenantId,
        Guid sessionId,
        IReadOnlyList<string> launchableProductKeys,
        string tenantRoleKey,
        Guid personId,
        int? accessTokenMinutes = null)
    {
        var signingKey = configuration["AUTH_SIGNING_KEY"] ?? _options.SigningKey;
        var lifetimeMinutes = accessTokenMinutes is > 0
            ? accessTokenMinutes.Value
            : _options.AccessTokenMinutes;
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(lifetimeMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Name, user.DisplayName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(StlClaimTypes.TenantId, tenantId.ToString()),
            new(StlClaimTypes.SessionId, sessionId.ToString()),
            new(StlClaimTypes.LaunchableProductKeys, string.Join(',', launchableProductKeys)),
            new(StlClaimTypes.PlatformAdmin, user.IsPlatformAdmin.ToString().ToLowerInvariant()),
            new(StlClaimTypes.PersonId, personId.ToString()),
        };

        if (!string.IsNullOrWhiteSpace(tenantRoleKey))
        {
            claims.Add(new Claim(StlClaimTypes.TenantRoleKey, tenantRoleKey));
        }

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    public string HashRefreshToken(string refreshToken)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToHexString(hash);
    }
}

