using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace STLCompliance.Shared.Auth;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var raw = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (raw is null || !Guid.TryParse(raw, out var userId))
        {
            throw new InvalidOperationException("Authenticated principal is missing a user id claim.");
        }

        return userId;
    }

    public static Guid GetTenantId(this ClaimsPrincipal principal)
    {
        var raw = principal.FindFirstValue(StlClaimTypes.TenantId);
        if (raw is null || !Guid.TryParse(raw, out var tenantId))
        {
            throw new InvalidOperationException("Authenticated principal is missing a tenant id claim.");
        }

        return tenantId;
    }
}
