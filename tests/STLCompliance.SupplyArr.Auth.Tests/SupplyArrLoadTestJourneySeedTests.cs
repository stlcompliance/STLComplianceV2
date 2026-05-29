using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Services;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Operations.LoadTesting;

namespace STLCompliance.SupplyArr.Auth.Tests;

public sealed class SupplyArrLoadTestJourneySeedTests : IAsyncLifetime
{
    private WebApplicationFactory<global::SupplyArr.Api.Program> _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"SupplyArrJourneySeed-{Guid.NewGuid():N}";

        _factory = new WebApplicationFactory<global::SupplyArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<SupplyArrDbContext>(services);
                services.AddDbContext<SupplyArrDbContext>(options => options.UseInMemoryDatabase(dbName));
            });
        });

        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Load_test_journey_seed_is_idempotent_and_creates_maintainarr_demand_ref()
    {
        var adminToken = CreateSupplyArrAccessToken(["supplyarr"], tenantRoleKey: "supplyarr_admin");

        var firstResponse = await _client.SendAsync(
            Authorized(HttpMethod.Post, StlSupplyArrLoadTestJourneySeedCatalog.SeedEndpointPath, adminToken));
        firstResponse.EnsureSuccessStatusCode();
        var first = (await firstResponse.Content.ReadFromJsonAsync<LoadTestJourneySeedResponse>())!;
        Assert.Equal(DemandRefSources.MaintainArr, first.DemandRefSource);
        Assert.Equal(StlSupplyArrLoadTestJourneySeedCatalog.JourneyWorkOrderNumber, first.SourceRefKey);
        Assert.Equal(StlSupplyArrLoadTestJourneySeedCatalog.JourneyDemandRefTitle, first.Title);
        Assert.True(first.DemandRefCreated);
        Assert.True(first.SettingsEnsured);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
            var demandRef = await db.MaintainArrDemandRefs.SingleAsync(x =>
                x.TenantId == PlatformSeeder.DemoTenantId
                && x.Title == StlSupplyArrLoadTestJourneySeedCatalog.JourneyDemandRefTitle);
            Assert.Equal(first.DemandRefId, demandRef.Id);
            Assert.Equal(1, await db.MaintainArrDemandRefLines.CountAsync(x => x.DemandRefId == demandRef.Id));
            Assert.True(await db.TenantDemandProcessingSettings.AnyAsync(x =>
                x.TenantId == PlatformSeeder.DemoTenantId
                && x.IsEnabled
                && x.ProcessMaintainarrDemandRefs));
            Assert.Equal(1, await db.AuditEvents.CountAsync(x =>
                x.TenantId == PlatformSeeder.DemoTenantId && x.Action == "load_test_journey.seed"));
        }

        var secondResponse = await _client.SendAsync(
            Authorized(HttpMethod.Post, StlSupplyArrLoadTestJourneySeedCatalog.SeedEndpointPath, adminToken));
        secondResponse.EnsureSuccessStatusCode();
        var second = (await secondResponse.Content.ReadFromJsonAsync<LoadTestJourneySeedResponse>())!;
        Assert.False(second.DemandRefCreated);
        Assert.Equal(first.DemandRefId, second.DemandRefId);
    }

    [Fact]
    public async Task Load_test_journey_seed_denied_for_read_only_role()
    {
        var token = CreateSupplyArrAccessToken(["supplyarr"], tenantRoleKey: "supplyarr_buyer");
        var response = await _client.SendAsync(
            Authorized(HttpMethod.Post, StlSupplyArrLoadTestJourneySeedCatalog.SeedEndpointPath, token));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private string CreateSupplyArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_admin")
    {
        using var scope = _factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<global::SupplyArr.Api.Services.SupplyArrTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "Test Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements,
            isPlatformAdmin: false);

        return accessToken;
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
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
