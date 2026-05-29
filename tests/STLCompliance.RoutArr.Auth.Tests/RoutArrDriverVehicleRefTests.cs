using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Services;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Services;

namespace STLCompliance.RoutArr.Auth.Tests;

public sealed class RoutArrDriverVehicleRefTests : IAsyncLifetime
{
    private WebApplicationFactory<global::RoutArr.Api.Program> _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"RoutArrDriverVehicle-{Guid.NewGuid():N}";

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
    public async Task Drivers_upsert_and_list_via_api_drivers()
    {
        var managerToken = CreateAccessToken(["routarr"], "routarr_manager");
        var upsertRequest = Authorized(HttpMethod.Put, "/api/drivers", managerToken);
        upsertRequest.Content = JsonContent.Create(new UpsertDriverRequest(
            "person-001",
            "Alex Driver",
            DateTimeOffset.UtcNow));

        var upsertResponse = await _client.SendAsync(upsertRequest);
        upsertResponse.EnsureSuccessStatusCode();

        var listResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/drivers", managerToken));
        listResponse.EnsureSuccessStatusCode();
        var payload = (await listResponse.Content.ReadFromJsonAsync<DriverListResponse>())!;
        Assert.Contains(payload.Items, x => x.PersonId == "person-001" && x.DisplayName == "Alex Driver");
    }

    [Fact]
    public async Task Vehicle_refs_upsert_and_list()
    {
        var managerToken = CreateAccessToken(["routarr"], "routarr_manager");
        var upsertRequest = Authorized(HttpMethod.Put, "/api/vehicle-refs", managerToken);
        upsertRequest.Content = JsonContent.Create(new UpsertVehicleRefRequest(
            "unit-42",
            "Unit 42",
            "TAG-42",
            DateTimeOffset.UtcNow));

        var upsertResponse = await _client.SendAsync(upsertRequest);
        upsertResponse.EnsureSuccessStatusCode();

        var listResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/vehicle-refs", managerToken));
        listResponse.EnsureSuccessStatusCode();
        var payload = (await listResponse.Content.ReadFromJsonAsync<VehicleRefListResponse>())!;
        Assert.Contains(payload.Items, x => x.VehicleRefKey == "unit-42" && x.FromMirror);
    }

    private string CreateAccessToken(IReadOnlyList<string> entitlements, string tenantRoleKey)
    {
        using var scope = _factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<RoutArrTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "Demo Admin",
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
