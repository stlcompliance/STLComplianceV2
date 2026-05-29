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

namespace STLCompliance.NexArr.Auth.Tests;

public class NexArrAuthApiTests : IClassFixture<WebApplicationFactory<global::NexArr.Api.Program>>
{
    private readonly WebApplicationFactory<global::NexArr.Api.Program> _factory;
    private readonly HttpClient _client;

    public NexArrAuthApiTests(WebApplicationFactory<global::NexArr.Api.Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", "test-signing-key-at-least-32-chars-long");
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
                    options.UseInMemoryDatabase("NexArrAuthTests"));
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Login_with_demo_credentials_returns_tokens()
    {
        await SeedDatabaseAsync();
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(PlatformSeeder.DemoAdminEmail, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<AuthTokenResponse>();
        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(payload.RefreshToken));
        Assert.Equal(PlatformSeeder.DemoTenantId, payload.TenantId);
    }

    [Fact]
    public async Task Login_with_wrong_password_returns_unauthorized()
    {
        await SeedDatabaseAsync();
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(PlatformSeeder.DemoAdminEmail, "wrong-password", PlatformSeeder.DemoTenantId));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_requires_authentication()
    {
        var response = await _client.GetAsync("/api/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_returns_profile_after_login()
    {
        await SeedDatabaseAsync();
        var tokens = await LoginAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var me = await response.Content.ReadFromJsonAsync<MeResponse>();
        Assert.NotNull(me);
        Assert.Equal(PlatformSeeder.DemoAdminEmail, me.Email);
        Assert.Contains("staffarr", me.Entitlements);
    }

    [Fact]
    public async Task Navigation_returns_entitled_products()
    {
        await SeedDatabaseAsync();
        var tokens = await LoginAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/me/navigation");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var navigation = await response.Content.ReadFromJsonAsync<NavigationResponse>();
        Assert.NotNull(navigation);
        Assert.True(navigation.Products.Count >= 7);
        var staffarr = navigation.Products.First(p => p.ProductKey.Equals("staffarr", StringComparison.OrdinalIgnoreCase));
        Assert.NotEmpty(staffarr.Surfaces);
        Assert.Contains(staffarr.Surfaces, s => s.SurfaceKey == "overview" && s.IsEnabled);
    }

    [Fact]
    public async Task Sessions_lists_active_session_after_login()
    {
        await SeedDatabaseAsync();
        var tokens = await LoginAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/me/sessions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<UserSessionsResponse>();
        Assert.NotNull(payload);
        Assert.NotEmpty(payload.Sessions);
        var current = Assert.Single(payload.Sessions, s => s.IsCurrent);
        Assert.Equal(tokens.SessionId, current.SessionId);
        Assert.True(current.IsActive);
    }

    [Fact]
    public async Task Revoke_session_invalidates_refresh_token()
    {
        await SeedDatabaseAsync();
        var tokens = await LoginAsync();

        var revokeRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/me/sessions/{tokens.SessionId}");
        revokeRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        var revokeResponse = await _client.SendAsync(revokeRequest);
        Assert.Equal(HttpStatusCode.NoContent, revokeResponse.StatusCode);

        var renewResponse = await _client.PostAsJsonAsync(
            "/api/auth/renew",
            new RenewSessionRequest(tokens.RefreshToken));
        Assert.Equal(HttpStatusCode.Unauthorized, renewResponse.StatusCode);
    }

    [Fact]
    public async Task Revoke_other_users_session_returns_not_found()
    {
        await SeedDatabaseAsync();
        var tokens = await LoginAsync();

        var revokeRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/me/sessions/{Guid.NewGuid()}");
        revokeRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        var revokeResponse = await _client.SendAsync(revokeRequest);
        Assert.Equal(HttpStatusCode.NotFound, revokeResponse.StatusCode);
    }

    [Fact]
    public async Task Sessions_requires_authentication()
    {
        var response = await _client.GetAsync("/api/me/sessions");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<AuthTokenResponse> LoginAsync()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(PlatformSeeder.DemoAdminEmail, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!;
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
