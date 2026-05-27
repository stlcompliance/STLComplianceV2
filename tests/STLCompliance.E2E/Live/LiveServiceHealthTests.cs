using System.Net;
using System.Net.Http.Json;
using NexArr.Api.Contracts;
using STLCompliance.E2E.Support;
using STLCompliance.Shared.Health;

namespace STLCompliance.E2E.Live;

/// <summary>
/// Optional live-stack smoke tests against docker-compose URLs.
/// Skipped unless E2E_LIVE=1 and services respond on /health.
/// </summary>
[Trait("Category", "Live")]
public sealed class LiveServiceHealthTests
{
    [SkippableFact]
    public async Task All_product_apis_report_healthy_when_stack_is_running()
    {
        Skip.IfNot(LiveServiceProbe.LiveModeEnabled, "Set E2E_LIVE=1 to run live E2E probes.");

        var endpoints = LiveServiceEndpoints.FromEnvironment();
        Skip.IfNot(
            await LiveServiceProbe.AreAllProductApisAvailableAsync(endpoints),
            "One or more product APIs are unreachable. Start docker-compose or set E2E_*_URL overrides.");

        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        foreach (var (product, baseUrl) in endpoints.AllProducts)
        {
            var response = await client.GetAsync(new Uri(baseUrl, "/health"));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var payload = await response.Content.ReadFromJsonAsync<HealthResponse>();
            Assert.NotNull(payload);
            Assert.Equal("Healthy", payload.Status);
            Assert.Equal(product, payload.Product);
        }
    }

    [SkippableFact]
    public async Task NexArr_demo_login_succeeds_against_live_stack()
    {
        Skip.IfNot(LiveServiceProbe.LiveModeEnabled, "Set E2E_LIVE=1 to run live E2E probes.");

        var endpoints = LiveServiceEndpoints.FromEnvironment();
        Skip.IfNot(
            await LiveServiceProbe.IsNexArrAvailableAsync(endpoints),
            "NexArr API is unreachable at " + endpoints.NexArr);

        using var client = new HttpClient { BaseAddress = endpoints.NexArr, Timeout = TimeSpan.FromSeconds(10) };
        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(
                NexArr.Api.Services.PlatformSeeder.DemoAdminEmail,
                NexArr.Api.Services.PlatformSeeder.DemoAdminPassword,
                NexArr.Api.Services.PlatformSeeder.DemoTenantId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var tokens = await response.Content.ReadFromJsonAsync<AuthTokenResponse>();
        Assert.NotNull(tokens);
        Assert.False(string.IsNullOrWhiteSpace(tokens.AccessToken));
    }
}
