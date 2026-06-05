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
using STLCompliance.Shared.Operations;

namespace STLCompliance.NexArr.Auth.Tests;

public sealed class NexArrPlatformWorkerHealthOrchestrationTests
    : IClassFixture<WebApplicationFactory<global::NexArr.Api.Program>>
{
    private readonly WebApplicationFactory<global::NexArr.Api.Program> _factory;
    private readonly HttpClient _client;

    public NexArrPlatformWorkerHealthOrchestrationTests(WebApplicationFactory<global::NexArr.Api.Program> factory)
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
                foreach (var descriptor in services
                             .Where(d => d.ServiceType == typeof(DbContextOptions<NexArrDbContext>)
                                 || d.ServiceType == typeof(NexArrDbContext))
                             .ToList())
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<NexArrDbContext>(options =>
                    options.UseInMemoryDatabase("NexArrPlatformWorkerHealthOrchestrationTests"));
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Orchestration_requires_platform_admin()
    {
        await SeedDatabaseAsync();
        var tenantAdminToken = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/platform-admin/worker-health-orchestration", tenantAdminToken));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Orchestration_status_includes_health_tokens_and_workers()
    {
        await SeedDatabaseAsync();
        var platformAdminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var expectedProductCount = StlProductDatabaseCatalog.All.Count - 1;

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/platform-admin/worker-health-orchestration", platformAdminToken));
        response.EnsureSuccessStatusCode();

        var status = (await response.Content.ReadFromJsonAsync<PlatformWorkerHealthOrchestrationStatusResponse>())!;
        Assert.NotEqual(default, status.GeneratedAt);
        Assert.False(string.IsNullOrWhiteSpace(status.PlatformHealthStatus));
        Assert.Equal(expectedProductCount, status.ProductHealth.Count);
        Assert.True(status.ServiceTokens.ActiveCount >= 0);
        Assert.Equal(4, status.Workers.Count);
        Assert.Contains(status.Workers, x => x.WorkerKey == "service_token_cleanup");
        Assert.Contains(status.Workers, x => x.WorkerKey == "entitlement_reconciliation");
        Assert.Contains(status.Workers, x => x.WorkerKey == "tenant_lifecycle");
        Assert.Contains(status.Workers, x => x.WorkerKey == "platform_outbox_publisher");
    }

    [Fact]
    public async Task Trigger_service_token_cleanup_when_disabled_returns_conflict()
    {
        await SeedDatabaseAsync();
        var platformAdminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var response = await _client.SendAsync(
            Authorized(
                HttpMethod.Post,
                "/api/platform-admin/worker-health-orchestration/trigger-service-token-cleanup",
                platformAdminToken));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
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

    private async Task<string> LoginAsync(string email)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        var login = (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!;
        return login.AccessToken;
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string path, string token)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }
}
