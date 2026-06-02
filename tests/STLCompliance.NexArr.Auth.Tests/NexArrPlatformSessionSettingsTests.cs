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

public class NexArrPlatformSessionSettingsTests : IClassFixture<WebApplicationFactory<global::NexArr.Api.Program>>
{
    private readonly WebApplicationFactory<global::NexArr.Api.Program> _factory;
    private readonly HttpClient _client;

    public NexArrPlatformSessionSettingsTests(WebApplicationFactory<global::NexArr.Api.Program> factory)
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
                    options.UseInMemoryDatabase("NexArrPlatformSessionSettingsTests"));
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Settings_requires_platform_admin()
    {
        await SeedDatabaseAsync();
        var tenantAdmin = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/platform-admin/session-settings", tenantAdmin.AccessToken));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Login_uses_configured_session_lifetimes()
    {
        await SeedDatabaseAsync();
        var platformAdmin = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        await UpsertSettingsAsync(platformAdmin.AccessToken, accessTokenMinutes: 60, refreshTokenDays: 14, rememberedRefreshTokenDays: 90);

        var issuedAt = DateTimeOffset.UtcNow;
        var standard = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        Assert.InRange(
            standard.AccessTokenExpiresAt,
            issuedAt.AddMinutes(55),
            issuedAt.AddMinutes(65));
        Assert.InRange(
            standard.RefreshTokenExpiresAt,
            issuedAt.AddDays(13),
            issuedAt.AddDays(15));

        var remembered = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail, rememberDevice: true);

        Assert.InRange(
            remembered.RefreshTokenExpiresAt,
            issuedAt.AddDays(89),
            issuedAt.AddDays(91));
    }

    [Fact]
    public async Task Handoff_redeem_includes_configured_access_lifetime()
    {
        await SeedDatabaseAsync();
        var platformAdmin = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        await UpsertSettingsAsync(platformAdmin.AccessToken, accessTokenMinutes: 75, refreshTokenDays: 14, rememberedRefreshTokenDays: 90);

        var handoffRequest = Authorized(HttpMethod.Post, "/api/v1/launch/handoff", platformAdmin.AccessToken);
        handoffRequest.Content = JsonContent.Create(new CreateHandoffRequest(
            "staffarr",
            "http://localhost:5173/app/staffarr"));
        var handoffResponse = await _client.SendAsync(handoffRequest);
        handoffResponse.EnsureSuccessStatusCode();
        var handoff = (await handoffResponse.Content.ReadFromJsonAsync<HandoffCreatedResponse>())!;

        var redeemRequest = Authorized(HttpMethod.Post, "/api/v1/handoff/redeem", platformAdmin.AccessToken);
        redeemRequest.Content = JsonContent.Create(new RedeemHandoffRequest(handoff.HandoffCode, null));
        var redeemResponse = await _client.SendAsync(redeemRequest);
        redeemResponse.EnsureSuccessStatusCode();
        var redeemed = (await redeemResponse.Content.ReadFromJsonAsync<HandoffRedeemedResponse>())!;

        Assert.Equal(75, redeemed.AccessTokenMinutes);
    }

    private async Task UpsertSettingsAsync(
        string accessToken,
        int accessTokenMinutes,
        int refreshTokenDays,
        int rememberedRefreshTokenDays)
    {
        var request = Authorized(HttpMethod.Put, "/api/platform-admin/session-settings", accessToken);
        request.Content = JsonContent.Create(new UpsertPlatformSessionSettingsRequest(
            accessTokenMinutes,
            refreshTokenDays,
            rememberedRefreshTokenDays));
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }

    private async Task<AuthTokenResponse> LoginAsync(string email, bool rememberDevice = false)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(
                email,
                PlatformSeeder.DemoAdminPassword,
                PlatformSeeder.DemoTenantId,
                rememberDevice));
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
