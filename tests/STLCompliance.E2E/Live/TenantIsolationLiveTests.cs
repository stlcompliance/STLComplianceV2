using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using NexArr.Api.Contracts;
using NexArr.Api.Services;
using StaffArr.Api.Contracts;
using STLCompliance.E2E.Support;
using STLCompliance.Shared.Auth;
using SupplyArr.Api.Contracts;

namespace STLCompliance.E2E.Live;

/// <summary>
/// Live-stack tenant isolation probes against docker-compose APIs.
/// Requires E2E_LIVE=1 and the dev signing key used by local/docker APIs.
/// </summary>
[Trait("Category", "Live")]
[Trait("Area", "TenantIsolation")]
public sealed class TenantIsolationLiveTests
{
    private const string DefaultDevSigningKey = "local-dev-only-change-in-production-min-32";

    [SkippableFact]
    public async Task Live_StaffArr_cross_tenant_person_get_returns_not_found()
    {
        Skip.IfNot(LiveServiceProbe.LiveModeEnabled, "Set E2E_LIVE=1 to run live tenant isolation probes.");

        var endpoints = LiveServiceEndpoints.FromEnvironment();
        Skip.IfNot(
            await LiveServiceProbe.IsNexArrAvailableAsync(endpoints)
                && await LiveServiceProbe.IsReachableAsync(endpoints.StaffArr),
            "NexArr and StaffArr APIs must be reachable for live tenant isolation.");

        using var nexarrClient = new HttpClient { BaseAddress = endpoints.NexArr, Timeout = TimeSpan.FromSeconds(15) };
        using var staffarrClient = new HttpClient { BaseAddress = endpoints.StaffArr, Timeout = TimeSpan.FromSeconds(15) };

        var loginResponse = await nexarrClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(
                PlatformSeeder.DemoAdminEmail,
                PlatformSeeder.DemoAdminPassword,
                PlatformSeeder.DemoTenantId));
        loginResponse.EnsureSuccessStatusCode();

        var tenantAToken = MintUserJwt(
            E2ETenants.TenantAId,
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminUserId,
            ["staffarr"],
            "tenant_admin");

        var createPersonRequest = HttpTestClient.Authorized(HttpMethod.Post, "/api/people", tenantAToken);
        createPersonRequest.Content = JsonContent.Create(new CreateStaffPersonRequest(
            "Live",
            "Isolation",
            $"live-isolation-{Guid.NewGuid():N}@e2e.stl",
            "active",
            null,
            null,
            null));
        var createPersonResponse = await staffarrClient.SendAsync(createPersonRequest);
        createPersonResponse.EnsureSuccessStatusCode();
        var created = (await createPersonResponse.Content.ReadFromJsonAsync<StaffPersonDetailResponse>())!;

        var tenantBToken = MintUserJwt(
            E2ETenants.TenantBId,
            E2ETenants.TenantBUserId,
            E2ETenants.TenantBPersonId,
            ["staffarr"],
            "tenant_admin");

        var crossTenantResponse = await staffarrClient.SendAsync(
            HttpTestClient.Authorized(HttpMethod.Get, $"/api/people/{created.PersonId}", tenantBToken));

        Assert.Equal(HttpStatusCode.NotFound, crossTenantResponse.StatusCode);
    }

    [SkippableFact]
    public async Task Live_SupplyArr_cross_tenant_vendor_get_returns_not_found()
    {
        Skip.IfNot(LiveServiceProbe.LiveModeEnabled, "Set E2E_LIVE=1 to run live tenant isolation probes.");

        var endpoints = LiveServiceEndpoints.FromEnvironment();
        Skip.IfNot(
            await LiveServiceProbe.IsNexArrAvailableAsync(endpoints)
                && await LiveServiceProbe.IsReachableAsync(endpoints.SupplyArr),
            "NexArr and SupplyArr APIs must be reachable for live tenant isolation.");

        using var nexarrClient = new HttpClient { BaseAddress = endpoints.NexArr, Timeout = TimeSpan.FromSeconds(15) };
        using var supplyarrClient = new HttpClient { BaseAddress = endpoints.SupplyArr, Timeout = TimeSpan.FromSeconds(15) };

        var loginResponse = await nexarrClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(
                PlatformSeeder.DemoAdminEmail,
                PlatformSeeder.DemoAdminPassword,
                PlatformSeeder.DemoTenantId));
        loginResponse.EnsureSuccessStatusCode();

        var tenantAToken = MintUserJwt(
            E2ETenants.TenantAId,
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminUserId,
            ["supplyarr"],
            "tenant_admin");

        var createVendorRequest = HttpTestClient.Authorized(HttpMethod.Post, "/api/vendors", tenantAToken);
        createVendorRequest.Content = JsonContent.Create(new CreateTypedExternalPartyRequest(
            $"live-iso-{Guid.NewGuid():N}".Substring(0, 12),
            "Live Isolation Vendor",
            "Live Isolation Vendor LLC",
            null,
            "Live tenant isolation probe"));
        var createVendorResponse = await supplyarrClient.SendAsync(createVendorRequest);
        createVendorResponse.EnsureSuccessStatusCode();
        var created = (await createVendorResponse.Content.ReadFromJsonAsync<ExternalPartyResponse>())!;

        var tenantBToken = MintUserJwt(
            E2ETenants.TenantBId,
            E2ETenants.TenantBUserId,
            E2ETenants.TenantBPersonId,
            ["supplyarr"],
            "tenant_admin");

        var crossTenantResponse = await supplyarrClient.SendAsync(
            HttpTestClient.Authorized(HttpMethod.Get, $"/api/vendors/{created.PartyId}", tenantBToken));

        Assert.Equal(HttpStatusCode.NotFound, crossTenantResponse.StatusCode);
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
            new(JwtRegisteredClaimNames.Email, E2ETenants.TenantBAdminEmail),
            new(JwtRegisteredClaimNames.Name, "Live Tenant B Admin"),
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
