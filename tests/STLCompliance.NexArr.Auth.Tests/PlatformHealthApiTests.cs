using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NexArr.Api.Contracts;
using NexArr.Api.Options;

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
        Assert.Equal(7, payload.Products.Count);
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
        Assert.Equal(7, payload.Products.Count);
    }

    [Fact]
    public async Task Ready_shortcut_is_available()
    {
        var response = await _client.GetAsync("/ready");
        Assert.True(
            response.StatusCode is HttpStatusCode.OK or HttpStatusCode.ServiceUnavailable,
            $"Unexpected status: {response.StatusCode}");
    }

    [Fact]
    public void Product_url_options_read_hierarchical_render_keys()
    {
        using var factory = new WebApplicationFactory<global::NexArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", "test-signing-key-at-least-32-chars-long");
            builder.UseSetting("StaffArr:BaseUrl", "https://staffarr-api.example.test");
            builder.UseSetting("TrainArr:BaseUrl", "https://trainarr-api.example.test");
            builder.UseSetting("ComplianceCore:BaseUrl", "https://compliancecore-api.example.test");
        });

        using var scope = factory.Services.CreateScope();
        var platformOptions = scope.ServiceProvider.GetRequiredService<IOptions<PlatformProductUrlsOptions>>().Value;
        var companionOptions = scope.ServiceProvider.GetRequiredService<IOptions<CompanionProductUrlsOptions>>().Value;

        Assert.Equal("https://staffarr-api.example.test", platformOptions.StaffArrBaseUrl);
        Assert.Equal("https://trainarr-api.example.test", platformOptions.TrainArrBaseUrl);
        Assert.Equal("https://compliancecore-api.example.test", platformOptions.ComplianceCoreBaseUrl);
        Assert.Equal("https://staffarr-api.example.test", companionOptions.StaffArrBaseUrl);
        Assert.Equal("https://trainarr-api.example.test", companionOptions.TrainArrBaseUrl);
    }
}
