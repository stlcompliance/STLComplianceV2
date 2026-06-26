using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using STLCompliance.Shared.Auth;

namespace AssurArr.Api.Auth;

internal sealed class AssurArrTestingAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "AssurArrTesting";
    private static readonly Guid DefaultUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid DefaultPersonId = Guid.Parse("11111111-1111-1111-1111-111111111112");
    private static readonly Guid DefaultTenantId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid DefaultSessionId = Guid.Parse("11111111-1111-1111-1111-111111111113");

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (TryGetHeaderValue("X-STL-Test-Auth", out var authMode) &&
            string.Equals(authMode, "anonymous", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var userId = ReadGuidHeader("X-STL-Test-UserId") ?? DefaultUserId;
        var personId = ReadGuidHeader("X-STL-Test-PersonId") ?? DefaultPersonId;
        var tenantId = ReadGuidHeader("X-STL-Test-TenantId") ?? DefaultTenantId;
        var sessionId = ReadGuidHeader("X-STL-Test-SessionId") ?? DefaultSessionId;
        var tenantRoleKey = TryGetHeaderValue("X-STL-Test-TenantRoleKey", out var tenantRole)
            ? tenantRole
            : "assurarr_manager";
        var isPlatformAdmin = ReadBoolHeader("X-STL-Test-PlatformAdmin") ?? false;
        var launchableProductKeys = TryGetHeaderValue("X-STL-Test-LaunchableProductKeys", out var rawLaunchableProductKeys)
            ? rawLaunchableProductKeys
            : "assurarr";

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString("D")),
            new(ClaimTypes.NameIdentifier, userId.ToString("D")),
            new(StlClaimTypes.PersonId, personId.ToString("D")),
            new(StlClaimTypes.TenantId, tenantId.ToString("D")),
            new(StlClaimTypes.SessionId, sessionId.ToString("D")),
            new(StlClaimTypes.TenantRoleKey, tenantRoleKey),
            new(StlClaimTypes.PlatformAdmin, isPlatformAdmin.ToString()),
            new(StlClaimTypes.LaunchableProductKeys, launchableProductKeys),
        };

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name)));
    }

    private bool TryGetHeaderValue(string headerName, out string value)
    {
        if (Request.Headers.TryGetValue(headerName, out var headerValues))
        {
            value = headerValues.ToString();
            return !string.IsNullOrWhiteSpace(value);
        }

        value = string.Empty;
        return false;
    }

    private Guid? ReadGuidHeader(string headerName)
    {
        return TryGetHeaderValue(headerName, out var value) && Guid.TryParse(value, out var parsed)
            ? parsed
            : null;
    }

    private bool? ReadBoolHeader(string headerName)
    {
        return TryGetHeaderValue(headerName, out var value) && bool.TryParse(value, out var parsed)
            ? parsed
            : null;
    }
}

