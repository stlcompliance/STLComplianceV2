using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Services;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using STLCompliance.Shared.Operations.LoadTesting;

namespace STLCompliance.RoutArr.Auth.Tests;

public sealed class RoutArrLoadTestJourneySeedTests : IAsyncLifetime
{
    private WebApplicationFactory<global::RoutArr.Api.Program> _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"RoutArrJourneySeed-{Guid.NewGuid():N}";

        _factory = new WebApplicationFactory<global::RoutArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<RoutArrDbContext>(services);
                services.AddDbContext<RoutArrDbContext>(options => options.UseInMemoryDatabase(dbName));
            });
        });

        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RoutArrDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Load_test_journey_seed_endpoint_is_removed()
    {
        var adminToken = CreateRoutArrAccessToken(["routarr"], tenantRoleKey: "routarr_admin");

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Post, StlRoutArrLoadTestJourneySeedCatalog.SeedEndpointPath, adminToken));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Load_test_journey_seed_endpoint_is_removed_for_read_only_role()
    {
        var token = CreateRoutArrAccessToken(["routarr"], tenantRoleKey: "routarr_driver");
        var response = await _client.SendAsync(
            Authorized(HttpMethod.Post, StlRoutArrLoadTestJourneySeedCatalog.SeedEndpointPath, token));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Load_test_journey_trip_get_endpoint_is_removed()
    {
        var adminToken = CreateRoutArrAccessToken(["routarr"], tenantRoleKey: "routarr_admin");
        var getResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/load-test-journey/trip", adminToken));
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Load_test_journey_trip_get_endpoint_is_removed_before_seed()
    {
        var adminToken = CreateRoutArrAccessToken(["routarr"], tenantRoleKey: "routarr_admin");
        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/load-test-journey/trip", adminToken));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private string CreateRoutArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_admin",
        Guid? userIdOverride = null)
    {
        using var scope = _factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<global::RoutArr.Api.Services.RoutArrTokenService>();
        var userId = userIdOverride ?? PlatformSeeder.DemoAdminUserId;
        var (accessToken, _) = tokenService.CreateAccessToken(
            userId,
            userId,
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
