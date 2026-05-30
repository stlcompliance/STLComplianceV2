using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Services;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.NexArr.Auth.Tests;

public class NexArrPlatformAdminApiTests : IClassFixture<WebApplicationFactory<global::NexArr.Api.Program>>
{
    private readonly WebApplicationFactory<global::NexArr.Api.Program> _factory;
    private readonly HttpClient _client;

    public NexArrPlatformAdminApiTests(WebApplicationFactory<global::NexArr.Api.Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", "test-signing-key-at-least-32-chars-long");
            builder.UseSetting("ServiceToken:SigningKey", "test-signing-key-at-least-32-chars-long");
            builder.ConfigureServices(services =>
            {
                var descriptors = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions<NexArrDbContext>)
                        || d.ServiceType == typeof(NexArrDbContext))
                    .ToList();
                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<NexArrDbContext>(options =>
                    options.UseInMemoryDatabase("NexArrPlatformAdminTests"));
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Platform_admin_dashboard_requires_authentication()
    {
        var response = await _client.GetAsync("/api/platform-admin/dashboard");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Platform_admin_can_read_dashboard()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/platform-admin/dashboard", token));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dashboard = await response.Content.ReadFromJsonAsync<PlatformAdminDashboardResponse>();
        Assert.NotNull(dashboard);
        Assert.True(dashboard.TenantCount >= 1);
        Assert.True(dashboard.ProductCount >= 7);
        Assert.True(dashboard.LaunchProfileCount >= 1);
    }

    [Fact]
    public async Task Tenant_admin_cannot_read_platform_admin_dashboard()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/platform-admin/dashboard", token));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Platform_admin_can_read_launch_diagnostics()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/platform-admin/launch-diagnostics", token));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var diagnostics = await response.Content.ReadFromJsonAsync<LaunchDiagnosticsResponse>();
        Assert.NotNull(diagnostics);
        Assert.NotEmpty(diagnostics.Rows);

        var v1Response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/platform-admin/launch-diagnostics", token));
        Assert.Equal(HttpStatusCode.OK, v1Response.StatusCode);
        var v1Diagnostics = await v1Response.Content.ReadFromJsonAsync<LaunchDiagnosticsResponse>();
        Assert.NotNull(v1Diagnostics);
        Assert.NotEmpty(v1Diagnostics.Rows);
    }

    [Fact]
    public async Task Tenant_admin_cannot_read_launch_diagnostics()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/platform-admin/launch-diagnostics", token));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        var v1Response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/platform-admin/launch-diagnostics", token));
        Assert.Equal(HttpStatusCode.Forbidden, v1Response.StatusCode);
    }

    [Fact]
    public async Task Platform_admin_can_lookup_launch_attempts_by_product_and_result()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var handoffRequest = Authorized(HttpMethod.Post, "/api/v1/launch/handoff", token);
        handoffRequest.Content = JsonContent.Create(new CreateHandoffRequest(
            "staffarr",
            "https://evil.example/callback"));
        var handoffResponse = await _client.SendAsync(handoffRequest);
        Assert.Equal(HttpStatusCode.Forbidden, handoffResponse.StatusCode);

        var response = await _client.SendAsync(
            Authorized(
                HttpMethod.Get,
                "/api/platform-admin/launch-attempts?productKey=staffarr&result=Denied",
                token));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var attempts = await response.Content.ReadFromJsonAsync<PagedResult<LaunchAttemptTimelineItemResponse>>();
        Assert.NotNull(attempts);
        var attempt = Assert.Single(attempts.Items);
        Assert.Equal("launch.handoff.create", attempt.Action);
        Assert.Equal("Denied", attempt.Result);
        Assert.Equal("callback_not_allowed", attempt.ReasonCode);
        Assert.Equal("staffarr", attempt.ProductKey);
        Assert.Equal("StaffArr", attempt.ProductDisplayName);
        Assert.Equal(PlatformSeeder.DemoTenantId, attempt.TenantId);
        Assert.Equal(PlatformSeeder.DemoAdminEmail, attempt.ActorEmail);
        Assert.Contains("callback allowlist", attempt.RemediationHint);

        var v1Response = await _client.SendAsync(
            Authorized(
                HttpMethod.Get,
                "/api/v1/platform-admin/launch-attempts?productKey=staffarr&result=Denied",
                token));
        Assert.Equal(HttpStatusCode.OK, v1Response.StatusCode);
        var v1Attempts = await v1Response.Content.ReadFromJsonAsync<PagedResult<LaunchAttemptTimelineItemResponse>>();
        Assert.NotNull(v1Attempts);
        Assert.NotEmpty(v1Attempts.Items);
    }

    [Fact]
    public async Task Tenant_admin_cannot_read_launch_attempts()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/platform-admin/launch-attempts", token));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Platform_admin_can_read_tenant_overview()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/platform-admin/overview/tenants", token));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var overview = await response.Content.ReadFromJsonAsync<PagedResult<TenantOverviewRowResponse>>();
        Assert.NotNull(overview);
        Assert.NotEmpty(overview.Items);
    }

    [Fact]
    public async Task Platform_admin_can_read_product_overview()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/platform-admin/overview/products", token));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var products = await response.Content.ReadFromJsonAsync<IReadOnlyList<ProductOverviewRowResponse>>();
        Assert.NotNull(products);
        Assert.Contains(products, p => p.ProductKey == "staffarr");
    }

    [Fact]
    public async Task Tenant_admin_cannot_read_tenant_overview()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/platform-admin/overview/tenants", token));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }

    private async Task<string> LoginAsync(string email)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        var payload = (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!;
        return payload.AccessToken;
    }

    private async Task SeedDatabaseAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await PlatformSeeder.SeedAsync(db, hasher);
    }
}
