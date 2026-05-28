using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using NexArr.Api.Contracts;
using NexArr.Api.Services;
using STLCompliance.E2E.Support;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Operations;

namespace STLCompliance.E2E.Live;

/// <summary>
/// Live-stack entitlement denial probe: JWT without StaffArr entitlement must not bootstrap <c>/api/me</c>.
/// </summary>
[Trait("Category", "Live")]
[Trait("Area", "EntitlementDenial")]
public sealed class EntitlementDenialLiveTests
{
    private const string DefaultDevSigningKey = "local-dev-only-change-in-production-min-32";

    [SkippableFact]
    public async Task Live_StaffArr_me_forbidden_without_staffarr_entitlement()
    {
        Skip.IfNot(LiveServiceProbe.LiveModeEnabled, "Set E2E_LIVE=1 to run live entitlement denial probes.");

        var endpoints = LiveServiceEndpoints.FromEnvironment();
        Skip.IfNot(
            await LiveServiceProbe.IsReachableAsync(endpoints.StaffArr),
            "StaffArr API must be reachable for live entitlement denial.");

        using var staffarrClient = new HttpClient { BaseAddress = endpoints.StaffArr, Timeout = TimeSpan.FromSeconds(15) };

        var token = MintUserJwt(
            E2ETenants.TenantAId,
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminUserId,
            ["nexarr"],
            "tenant_admin");

        var response = await staffarrClient.SendAsync(
            HttpTestClient.Authorized(HttpMethod.Get, "/api/me", token));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [SkippableFact]
    public async Task Live_NexArr_launch_context_forbidden_for_unknown_product()
    {
        Skip.IfNot(LiveServiceProbe.LiveModeEnabled, "Set E2E_LIVE=1 to run live entitlement denial probes.");

        var endpoints = LiveServiceEndpoints.FromEnvironment();
        Skip.IfNot(
            await LiveServiceProbe.IsNexArrAvailableAsync(endpoints),
            "NexArr API must be reachable for live launch denial.");

        using var nexarrClient = new HttpClient { BaseAddress = endpoints.NexArr, Timeout = TimeSpan.FromSeconds(15) };

        var loginResponse = await nexarrClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(
                PlatformSeeder.DemoTenantAdminEmail,
                PlatformSeeder.DemoAdminPassword,
                PlatformSeeder.DemoTenantId));
        loginResponse.EnsureSuccessStatusCode();
        var login = (await loginResponse.Content.ReadFromJsonAsync<AuthTokenResponse>())!;

        var response = await nexarrClient.SendAsync(
            HttpTestClient.Authorized(
                HttpMethod.Get,
                $"{StlM13ShipGateCatalog.NexArrLaunchContextPath}?productKey={StlM13ShipGateCatalog.NexArrDeniedLaunchProductKey}",
                login.AccessToken));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static string MintUserJwt(
        Guid tenantId,
        Guid userId,
        Guid personId,
        IReadOnlyList<string> entitlements,
        string tenantRoleKey)
    {
        var signingKey = Environment.GetEnvironmentVariable("E2E_DEV_SIGNING_KEY") ?? DefaultDevSigningKey;
        var issuer = Environment.GetEnvironmentVariable("E2E_JWT_ISSUER") ?? "stl-compliance-nexarr";
        var audience = Environment.GetEnvironmentVariable("E2E_JWT_AUDIENCE") ?? "stl-compliance-suite";

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, E2ETenants.TenantAAdminEmail),
            new(JwtRegisteredClaimNames.Name, "Live Tenant A Admin"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(StlClaimTypes.TenantId, tenantId.ToString()),
            new(StlClaimTypes.SessionId, Guid.NewGuid().ToString()),
            new(StlClaimTypes.TenantRoleKey, tenantRoleKey),
            new(StlClaimTypes.PersonId, personId.ToString()),
            new(StlClaimTypes.Entitlements, string.Join(',', entitlements)),
            new(StlClaimTypes.PlatformAdmin, "false"),
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
