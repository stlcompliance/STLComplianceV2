using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrdArr.Api.Data;
using OrdArr.Api.Services;

namespace STLCompliance.OrdArr.Auth.Tests;

public sealed class OrdArrAuthEndpointsTests : IAsyncLifetime
{
    private static readonly Guid DemoTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid DemoUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid DemoPersonId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    private WebApplicationFactory<global::OrdArr.Api.Program> _factory = null!;
    private HttpClient _client = null!;

    public Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"OrdArrAuth-{Guid.NewGuid():N}";

        _factory = new WebApplicationFactory<global::OrdArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<OrdArrDbContext>(services);
                services.AddDbContext<OrdArrDbContext>(options => options.UseInMemoryDatabase(dbName));
            });
        });

        _client = _factory.CreateClient();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Session_bootstrap_allows_users_after_non_ordarr_launch_context()
    {
        var token = CreateAccessToken(["nexarr"]);

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/session", token));
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = document.RootElement;
        Assert.Equal("ordarr", root.GetProperty("productKey").GetString());
        Assert.True(root.GetProperty("hasOrdArrAccess").GetBoolean());
        Assert.Contains(
            root.GetProperty("launchableProductKeys").EnumerateArray().Select(item => item.GetString()),
            value => string.Equals(value, "nexarr", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Workspace_summary_rejects_plain_tenant_member()
    {
        var token = CreateAccessToken(["nexarr"], tenantRoleKey: "tenant_member");

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/workspace/summary", token));

        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Workspace_summary_allows_ordarr_ops_after_non_ordarr_launch_context()
    {
        var token = CreateAccessToken(["nexarr"], tenantRoleKey: "ordarr-ops");

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/workspace/summary", token));

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Create_order_rejects_platform_admin_without_ordarr_role()
    {
        var token = CreateAccessToken(["nexarr"], tenantRoleKey: "tenant_member", isPlatformAdmin: true);
        using var request = Authorized(HttpMethod.Post, "/api/v1/orders/", token);
        request.Headers.Add("Idempotency-Key", $"auth-test-{Guid.NewGuid():N}");
        request.Content = JsonContent.Create(new OrdArrCreateOrderRequest(
            new STLCompliance.Shared.Integration.StlProductObjectReference("customarr", "customer", "cust-auth", "CUST-AUTH"),
            "Auth Test Customer",
            "customer_order",
            "person-ordarr-owner",
            "Verifies OrdArr authorization."));

        var response = await _client.SendAsync(request);

        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    private string CreateAccessToken(IReadOnlyList<string> entitlements, string tenantRoleKey = "tenant_member", bool isPlatformAdmin = false)
    {
        using var scope = _factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<OrdArrTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            DemoUserId,
            DemoPersonId,
            "ordarr.user@demo.stl",
            "OrdArr User",
            DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements,
            isPlatformAdmin);
        return accessToken;
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string path, string token)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private static void RemoveDbContext<TContext>(IServiceCollection services)
        where TContext : DbContext
    {
        var descriptors = services
            .Where(d => d.ServiceType == typeof(DbContextOptions<TContext>) || d.ServiceType == typeof(TContext))
            .ToList();
        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }
}
