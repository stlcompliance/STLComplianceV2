using System.Net;
using System.Net.Http.Json;
using NexArr.Api.Contracts;
using NexArr.Api.Services;
using STLCompliance.E2E.Support;
using STLCompliance.Shared.Operations;

namespace STLCompliance.E2E.Live;

/// <summary>
/// Live Render staging ship-gate probes against deployed public API URLs.
/// Skipped unless <c>SHIP_GATE_RENDER_STAGING_LIVE=1</c> and all <c>RENDER_STAGING_*_API_URL</c> values are set.
/// </summary>
[Trait("Category", "Live")]
[Trait("Area", "RenderStagingShipGate")]
public sealed class RenderStagingShipGateLiveTests
{
    [SkippableFact]
    public async Task Staging_all_product_apis_report_liveness()
    {
        Skip.IfNot(RenderStagingShipGateLiveProbe.LiveModeEnabled, "Set SHIP_GATE_RENDER_STAGING_LIVE=1.");
        Skip.IfNot(RenderStagingShipGateLiveProbe.AreStagingApiUrlsConfigured(), "Set all RENDER_STAGING_*_API_URL values.");

        var targets = RenderStagingShipGateLiveProbe.ResolveApiTargets();
        var errors = await StlRenderStagingShipGateSupport.GetUnhealthyApiMessagesAsync(targets, "/health");
        Assert.True(errors.Count == 0, string.Join("; ", errors));
    }

    [SkippableFact]
    public async Task Staging_all_product_apis_report_ready()
    {
        Skip.IfNot(RenderStagingShipGateLiveProbe.LiveModeEnabled, "Set SHIP_GATE_RENDER_STAGING_LIVE=1.");
        Skip.IfNot(RenderStagingShipGateLiveProbe.AreStagingApiUrlsConfigured(), "Set all RENDER_STAGING_*_API_URL values.");

        var targets = RenderStagingShipGateLiveProbe.ResolveApiTargets();
        var errors = await StlRenderStagingShipGateSupport.GetUnhealthyApiMessagesAsync(
            targets,
            StlRenderBlueprintCatalog.ApiHealthCheckPath);
        Assert.True(errors.Count == 0, string.Join("; ", errors));
    }

    [SkippableFact]
    public async Task Staging_nexarr_demo_login_succeeds()
    {
        Skip.IfNot(RenderStagingShipGateLiveProbe.LiveModeEnabled, "Set SHIP_GATE_RENDER_STAGING_LIVE=1.");
        Skip.IfNot(RenderStagingShipGateLiveProbe.AreStagingApiUrlsConfigured(), "Set all RENDER_STAGING_*_API_URL values.");

        var nexarr = RenderStagingShipGateLiveProbe.ResolveApiTargets()
            .Single(target => target.ProductKey.Equals("nexarr", StringComparison.OrdinalIgnoreCase));
        var (email, password, tenantId) = StlRenderStagingShipGateSupport.ResolveDemoCredentials();

        using var client = new HttpClient { BaseAddress = nexarr.BaseUrl, Timeout = TimeSpan.FromSeconds(20) };
        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, password, tenantId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var tokens = await response.Content.ReadFromJsonAsync<AuthTokenResponse>();
        Assert.NotNull(tokens);
        Assert.False(string.IsNullOrWhiteSpace(tokens.AccessToken));
    }

    [SkippableFact]
    public async Task Staging_nexarr_launch_context_denied_for_unknown_product()
    {
        Skip.IfNot(RenderStagingShipGateLiveProbe.LiveModeEnabled, "Set SHIP_GATE_RENDER_STAGING_LIVE=1.");
        Skip.IfNot(RenderStagingShipGateLiveProbe.AreStagingApiUrlsConfigured(), "Set all RENDER_STAGING_*_API_URL values.");

        var nexarr = RenderStagingShipGateLiveProbe.ResolveApiTargets()
            .Single(target => target.ProductKey.Equals("nexarr", StringComparison.OrdinalIgnoreCase));
        var (email, password, tenantId) = StlRenderStagingShipGateSupport.ResolveDemoCredentials();

        using var client = new HttpClient { BaseAddress = nexarr.BaseUrl, Timeout = TimeSpan.FromSeconds(20) };

        var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, password, tenantId));
        loginResponse.EnsureSuccessStatusCode();
        var login = (await loginResponse.Content.ReadFromJsonAsync<AuthTokenResponse>())!;

        var response = await client.SendAsync(
            HttpTestClient.Authorized(
                HttpMethod.Get,
                $"{StlM13ShipGateCatalog.NexArrLaunchContextPath}?productKey={StlM13ShipGateCatalog.NexArrDeniedLaunchProductKey}",
                login.AccessToken));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [SkippableFact]
    public async Task Staging_nexarr_platform_health_aggregation_is_healthy()
    {
        Skip.IfNot(RenderStagingShipGateLiveProbe.LiveModeEnabled, "Set SHIP_GATE_RENDER_STAGING_LIVE=1.");
        Skip.IfNot(RenderStagingShipGateLiveProbe.AreStagingApiUrlsConfigured(), "Set all RENDER_STAGING_*_API_URL values.");

        var nexarr = RenderStagingShipGateLiveProbe.ResolveApiTargets()
            .Single(target => target.ProductKey.Equals("nexarr", StringComparison.OrdinalIgnoreCase));
        var (email, password, tenantId) = StlRenderStagingShipGateSupport.ResolveDemoCredentials();

        using var client = new HttpClient { BaseAddress = nexarr.BaseUrl, Timeout = TimeSpan.FromSeconds(30) };

        var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, password, tenantId));
        loginResponse.EnsureSuccessStatusCode();
        var login = (await loginResponse.Content.ReadFromJsonAsync<AuthTokenResponse>())!;

        var response = await client.SendAsync(
            HttpTestClient.Authorized(HttpMethod.Get, "/api/platform/health", login.AccessToken));
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<PlatformHealthResponse>();
        Assert.NotNull(payload);
        Assert.Equal("Healthy", payload.Status);
        Assert.NotEmpty(payload.Products);
        Assert.All(
            payload.Products,
            probe => Assert.Equal("Healthy", probe.Status, StringComparer.OrdinalIgnoreCase));
    }

    [SkippableFact]
    public async Task Staging_optional_static_sites_respond_when_configured()
    {
        Skip.IfNot(RenderStagingShipGateLiveProbe.LiveModeEnabled, "Set SHIP_GATE_RENDER_STAGING_LIVE=1.");

        var staticSites = StlRenderStagingShipGateSupport.ResolveConfiguredStaticSiteTargetsFromEnvironment();
        Skip.If(staticSites.Count == 0, "No RENDER_STAGING_*_FRONTEND_URL values configured.");

        var errors = await StlRenderStagingShipGateSupport.GetUnreachableStaticSiteMessagesAsync(staticSites);
        Assert.True(errors.Count == 0, string.Join("; ", errors));
    }
}

internal static class RenderStagingShipGateLiveProbe
{
    public static bool LiveModeEnabled => StlRenderStagingShipGateSupport.LiveModeEnabled;

    public static bool AreStagingApiUrlsConfigured() => StlRenderStagingShipGateSupport.AreStagingApiUrlsConfigured();

    public static IReadOnlyList<StlRenderStagingShipGateApiTarget> ResolveApiTargets() =>
        StlRenderStagingShipGateSupport.ResolveApiTargetsFromEnvironment();
}
