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

        await UpsertSettingsAsync(
            platformAdmin.AccessToken,
            accessTokenMinutes: 60,
            refreshTokenDays: 14,
            rememberedRefreshTokenDays: 90,
            requirePlatformAdminMfa: false,
            passwordMinLength: 12,
            requirePasswordComplexity: true);

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
    public async Task Login_requires_platform_admin_mfa_when_enabled()
    {
        await SeedDatabaseAsync();
        var platformAdmin = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        await UpsertSettingsAsync(
            platformAdmin.AccessToken,
            accessTokenMinutes: 45,
            refreshTokenDays: 14,
            rememberedRefreshTokenDays: 90,
            requirePlatformAdminMfa: true,
            passwordMinLength: 12,
            requirePasswordComplexity: true);

        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(
                PlatformSeeder.DemoAdminEmail,
                PlatformSeeder.DemoAdminPassword,
                PlatformSeeder.DemoTenantId));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Handoff_redeem_includes_configured_access_lifetime()
    {
        await SeedDatabaseAsync();
        var platformAdmin = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        await UpsertSettingsAsync(
            platformAdmin.AccessToken,
            accessTokenMinutes: 75,
            refreshTokenDays: 14,
            rememberedRefreshTokenDays: 90,
            requirePlatformAdminMfa: false,
            passwordMinLength: 12,
            requirePasswordComplexity: true);

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

    [Fact]
    public async Task Password_policy_configuration_is_persisted_and_enforced()
    {
        await SeedDatabaseAsync();
        var platformAdmin = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        await UpsertSettingsAsync(
            platformAdmin.AccessToken,
            accessTokenMinutes: 45,
            refreshTokenDays: 14,
            rememberedRefreshTokenDays: 90,
            requirePlatformAdminMfa: false,
            passwordMinLength: 16,
            requirePasswordComplexity: true);

        var settingsResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/platform-admin/session-settings", platformAdmin.AccessToken));
        settingsResponse.EnsureSuccessStatusCode();
        var settings = (await settingsResponse.Content.ReadFromJsonAsync<PlatformSessionSettingsResponse>())!;
        Assert.Equal(16, settings.PasswordMinLength);
        Assert.True(settings.RequirePasswordComplexity);

        var weakCreateRequest = Authorized(HttpMethod.Post, "/api/v1/platform-admin/users", platformAdmin.AccessToken);
        weakCreateRequest.Content = JsonContent.Create(new CreatePlatformUserRequest(
            "policy-weak@example.test",
            "Policy Weak",
            "WeakPass123"));
        var weakCreateResponse = await _client.SendAsync(weakCreateRequest);
        Assert.Equal(HttpStatusCode.BadRequest, weakCreateResponse.StatusCode);
        var weakCreateBody = await weakCreateResponse.Content.ReadAsStringAsync();
        Assert.Contains("auth.password_policy", weakCreateBody);

        var strongCreateRequest = Authorized(HttpMethod.Post, "/api/v1/platform-admin/users", platformAdmin.AccessToken);
        strongCreateRequest.Content = JsonContent.Create(new CreatePlatformUserRequest(
            "policy-strong@example.test",
            "Policy Strong",
            "StrongPassword123!",
            IsPlatformAdmin: true));
        var strongCreateResponse = await _client.SendAsync(strongCreateRequest);
        strongCreateResponse.EnsureSuccessStatusCode();
        var created = (await strongCreateResponse.Content.ReadFromJsonAsync<PlatformUserDetailResponse>())!;

        var forgotResponse = await _client.PostAsJsonAsync(
            "/api/auth/password/forgot",
            new ForgotPasswordRequest(created.Email));
        forgotResponse.EnsureSuccessStatusCode();
        var forgot = (await forgotResponse.Content.ReadFromJsonAsync<ForgotPasswordResponse>())!;
        Assert.False(string.IsNullOrWhiteSpace(forgot.DevResetToken));

        var weakResetResponse = await _client.PostAsJsonAsync(
            "/api/auth/password/reset",
            new ResetPasswordRequest(forgot.DevResetToken!, "weakpass123"));
        Assert.Equal(HttpStatusCode.BadRequest, weakResetResponse.StatusCode);
        var weakResetBody = await weakResetResponse.Content.ReadAsStringAsync();
        Assert.Contains("auth.password_policy", weakResetBody);

        var strongResetResponse = await _client.PostAsJsonAsync(
            "/api/auth/password/reset",
            new ResetPasswordRequest(forgot.DevResetToken!, "AnotherStrongPassword123!"));
        strongResetResponse.EnsureSuccessStatusCode();

        var loginAfterResetResponse = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(
                created.Email,
                "AnotherStrongPassword123!",
                PlatformSeeder.DemoTenantId));
        loginAfterResetResponse.EnsureSuccessStatusCode();
    }

    private async Task UpsertSettingsAsync(
        string accessToken,
        int accessTokenMinutes,
        int refreshTokenDays,
        int rememberedRefreshTokenDays,
        bool requirePlatformAdminMfa,
        int passwordMinLength,
        bool requirePasswordComplexity)
    {
        var request = Authorized(HttpMethod.Put, "/api/platform-admin/session-settings", accessToken);
        request.Content = JsonContent.Create(new UpsertPlatformSessionSettingsRequest(
            accessTokenMinutes,
            refreshTokenDays,
            rememberedRefreshTokenDays,
            requirePlatformAdminMfa,
            passwordMinLength,
            requirePasswordComplexity));
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
