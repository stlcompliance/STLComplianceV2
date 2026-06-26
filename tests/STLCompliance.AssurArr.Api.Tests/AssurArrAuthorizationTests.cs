using System.Net;
using System.Net.Http.Json;
using AssurArr.Api.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace STLCompliance.AssurArr.Api.Tests;

public sealed class AssurArrAuthorizationTests : IClassFixture<WebApplicationFactory<global::AssurArr.Api.Program>>
{
    private readonly HttpClient _client;

    public AssurArrAuthorizationTests(WebApplicationFactory<global::AssurArr.Api.Program> factory)
    {
        _client = factory
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.UseSetting("ConnectionStrings:Database", string.Empty);
                builder.UseSetting("DATABASE_URL", string.Empty);
            })
            .CreateClient();
    }

    [Fact]
    public async Task Dashboard_rejects_plain_tenant_member()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/dashboard");
        request.Headers.Add("X-STL-Test-TenantRoleKey", "tenant_member");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Integration_nonconformances_allow_quality_manager_after_non_assurarr_launch_context()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/integrations/nonconformances");
        request.Headers.Add("X-STL-Test-TenantRoleKey", "quality_manager");
        request.Headers.Add("X-STL-Test-LaunchableProductKeys", "staffarr");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Integration_nonconformances_accept_canonical_launchable_products_test_header()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/integrations/nonconformances");
        request.Headers.Add("X-STL-Test-TenantRoleKey", "quality_manager");
        request.Headers.Add("X-STL-Test-LaunchableProductKeys", "staffarr");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Integration_nonconformance_create_rejects_plain_tenant_member()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/integrations/nonconformances");
        request.Headers.Add("X-STL-Test-TenantRoleKey", "tenant_member");
        request.Content = JsonContent.Create(new CreateAssurArrNonconformanceRequest(
            $"Authorization test {Guid.NewGuid():N}",
            "Verifies AssurArr manage authorization.",
            "high",
            "receiving",
            "failed_inspection",
            "loadarr",
            "loadarr:receiving:auth-test",
            ["loadarr:inventory:auth-test"],
            null,
            null,
            null,
            null,
            null,
            false,
            null,
            null,
            [],
            DateTimeOffset.UtcNow.AddDays(1)));

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Dashboard_rejects_platform_admin_without_quality_role()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/dashboard");
        request.Headers.Add("X-STL-Test-TenantRoleKey", "tenant_member");
        request.Headers.Add("X-STL-Test-PlatformAdmin", "true");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
