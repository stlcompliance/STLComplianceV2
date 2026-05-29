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

public class NexArrHybridDataPlaneTests : IClassFixture<WebApplicationFactory<global::NexArr.Api.Program>>
{
    private readonly WebApplicationFactory<global::NexArr.Api.Program> _factory;
    private readonly HttpClient _client;

    public NexArrHybridDataPlaneTests(WebApplicationFactory<global::NexArr.Api.Program> factory)
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
                    options.UseInMemoryDatabase("NexArrHybridDataPlaneTests"));
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Platform_admin_can_upsert_and_list_data_plane_profile()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var upsertRequest = Authorized(HttpMethod.Put, "/api/platform-admin/data-plane", token);
        upsertRequest.Content = JsonContent.Create(new UpsertDataPlaneProfileRequest(
            PlatformSeeder.DemoTenantId,
            "staffarr",
            "customer_hosted",
            "https://customer.example/staffarr",
            "untrusted",
            "Pilot customer deployment"));
        var upsertResponse = await _client.SendAsync(upsertRequest);
        Assert.Equal(HttpStatusCode.OK, upsertResponse.StatusCode);
        var profile = await upsertResponse.Content.ReadFromJsonAsync<DataPlaneProfileResponse>();
        Assert.NotNull(profile);
        Assert.Equal("customer_hosted", profile.DeploymentMode);
        Assert.Equal("untrusted", profile.TrustStatus);

        var listResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/platform-admin/data-plane?tenantId={PlatformSeeder.DemoTenantId}",
                token));
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var page = await listResponse.Content.ReadFromJsonAsync<PagedResult<DataPlaneProfileResponse>>();
        Assert.NotNull(page);
        Assert.Single(page.Items);
    }

    [Fact]
    public async Task Customer_hosted_cannot_be_marked_trusted_without_validation()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var upsertRequest = Authorized(HttpMethod.Put, "/api/platform-admin/data-plane", token);
        upsertRequest.Content = JsonContent.Create(new UpsertDataPlaneProfileRequest(
            PlatformSeeder.DemoTenantId,
            "staffarr",
            "customer_hosted",
            "https://customer.example/staffarr",
            "trusted",
            null));
        var response = await _client.SendAsync(upsertRequest);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Tenant_admin_cannot_manage_data_plane_profiles()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/platform-admin/data-plane", token));
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
