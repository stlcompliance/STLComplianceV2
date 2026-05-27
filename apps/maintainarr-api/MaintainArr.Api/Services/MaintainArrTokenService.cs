using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Services;

public sealed class MaintainArrTokenService(IOptions<StlJwtOptions> options, IConfiguration configuration)
{
    public (string AccessToken, DateTimeOffset ExpiresAt) CreateAccessToken(
        Guid userId,
        Guid personId,
        string email,
        string displayName,
        Guid tenantId,
        Guid sessionId,
        string tenantRoleKey,
        IReadOnlyList<string> entitlements,
        bool isPlatformAdmin)
    {
        var signingKey = configuration["AUTH_SIGNING_KEY"] ?? options.Value.SigningKey;
        var jwtOptions = options.Value;
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(jwtOptions.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Name, displayName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(StlClaimTypes.TenantId, tenantId.ToString()),
            new(StlClaimTypes.SessionId, sessionId.ToString()),
            new(StlClaimTypes.TenantRoleKey, tenantRoleKey),
            new(StlClaimTypes.PersonId, personId.ToString()),
            new(StlClaimTypes.Entitlements, string.Join(',', entitlements)),
            new(StlClaimTypes.PlatformAdmin, isPlatformAdmin.ToString().ToLowerInvariant())
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
