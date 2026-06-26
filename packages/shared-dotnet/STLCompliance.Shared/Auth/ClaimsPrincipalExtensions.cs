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

    public static Guid GetSessionId(this ClaimsPrincipal principal)
    {
        var raw = principal.FindFirstValue(StlClaimTypes.SessionId);
        if (raw is null || !Guid.TryParse(raw, out var sessionId))
        {
            throw new InvalidOperationException("Authenticated principal is missing a session id claim.");
        }

        return sessionId;
    }

    public static string GetTenantRoleKey(this ClaimsPrincipal principal) =>
        principal.FindFirstValue(StlClaimTypes.TenantRoleKey) ?? string.Empty;

    public static bool IsPlatformAdmin(this ClaimsPrincipal principal) =>
        bool.TryParse(principal.FindFirstValue(StlClaimTypes.PlatformAdmin), out var isAdmin) && isAdmin;

    public static IReadOnlyList<string> GetLaunchableProductKeys(this ClaimsPrincipal principal)
    {
        var raw = principal.FindFirstValue(StlClaimTypes.LaunchableProductKeys);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return [];
        }

        return raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    public static bool HasLaunchableProductAccess(this ClaimsPrincipal principal, string productKey) =>
        principal.IsPlatformAdmin()
        || principal.GetLaunchableProductKeys().Any(product =>
            string.Equals(
                ProductKeyAliases.Normalize(product),
                ProductKeyAliases.Normalize(productKey),
                StringComparison.OrdinalIgnoreCase));

    public static bool IsServicePrincipal(this ClaimsPrincipal principal) =>
        string.Equals(
            principal.FindFirstValue(StlServiceTokenClaimTypes.TokenType),
            StlServiceTokenClaimTypes.ServiceTokenTypeValue,
            StringComparison.Ordinal);

    public static string? GetSourceProductKey(this ClaimsPrincipal principal)
    {
        var raw = principal.FindFirstValue(StlServiceTokenClaimTypes.SourceProduct);
        return string.IsNullOrWhiteSpace(raw)
            ? null
            : ProductKeyAliases.Normalize(raw);
    }

    public static Guid GetPersonId(this ClaimsPrincipal principal)
    {
        var raw = principal.FindFirstValue(StlClaimTypes.PersonId);
        if (raw is not null && Guid.TryParse(raw, out var personId))
        {
            return personId;
        }

        return principal.GetUserId();
    }
}

