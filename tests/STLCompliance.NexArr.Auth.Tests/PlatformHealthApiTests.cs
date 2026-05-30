using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using NexArr.Api.Contracts;

namespace STLCompliance.NexArr.Auth.Tests;

public class PlatformHealthApiTests : IClassFixture<WebApplicationFactory<global::NexArr.Api.Program>>
{
    private readonly HttpClient _client;

    public PlatformHealthApiTests(WebApplicationFactory<global::NexArr.Api.Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", "test-signing-key-at-least-32-chars-long");
        }).CreateClient();
    }

    [Fact]
    public async Task Platform_health_is_anonymous_and_returns_product_probes()
    {
        var response = await _client.GetAsync("/api/platform/health");

        Assert.True(
            response.StatusCode is HttpStatusCode.OK or HttpStatusCode.ServiceUnavailable,
            $"Unexpected status: {response.StatusCode}");

        var payload = await response.Content.ReadFromJsonAsync<PlatformHealthResponse>();
        Assert.NotNull(payload);
        Assert.Contains(payload.Status, new[] { "Healthy", "Degraded", "Unhealthy" });
        Assert.Equal(6, payload.Products.Count);
        Assert.Contains(payload.Products, p => p.ProductKey == "staffarr");
        Assert.Contains(payload.Products, p => p.ProductKey == "compliancecore");
    }

    [Fact]
    public async Task System_status_v1_alias_returns_platform_health_payload()
    {
        var response = await _client.GetAsync("/api/v1/system/status");

        Assert.True(
            response.StatusCode is HttpStatusCode.OK or HttpStatusCode.ServiceUnavailable,
            $"Unexpected status: {response.StatusCode}");

        var payload = await response.Content.ReadFromJsonAsync<PlatformHealthResponse>();
        Assert.NotNull(payload);
        Assert.Equal(6, payload.Products.Count);
    }

    [Fact]
    public async Task Ready_shortcut_is_available()
    {
        var response = await _client.GetAsync("/ready");
        Assert.True(
            response.StatusCode is HttpStatusCode.OK or HttpStatusCode.ServiceUnavailable,
            $"Unexpected status: {response.StatusCode}");
    }
}
