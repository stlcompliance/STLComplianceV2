using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
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
    public async Task Remember_device_login_uses_remembered_refresh_lifetime_and_marks_session()
    {
        var rememberFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Auth:RememberedRefreshTokenDays", "30");
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
                    options.UseInMemoryDatabase("NexArrAuthTests-RememberDevice"));
            });
        });

        await SeedDatabaseAsync(rememberFactory);
        var client = rememberFactory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(
                PlatformSeeder.DemoAdminEmail,
                PlatformSeeder.DemoAdminPassword,
                PlatformSeeder.DemoTenantId,
                RememberDevice: true));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var tokens = (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!;
        Assert.True(tokens.RefreshTokenExpiresAt > DateTimeOffset.UtcNow.AddDays(20));

        var sessionsRequest = new HttpRequestMessage(HttpMethod.Get, "/api/me/sessions");
        sessionsRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        var sessionsResponse = await client.SendAsync(sessionsRequest);
        sessionsResponse.EnsureSuccessStatusCode();
        var sessions = (await sessionsResponse.Content.ReadFromJsonAsync<UserSessionsResponse>())!;
        var current = Assert.Single(sessions.Sessions, s => s.IsCurrent);
        Assert.True(current.IsRemembered);
    }

    [Fact]
    public async Task Login_from_new_user_agent_emits_suspicious_login_audit_event()
    {
        await SeedDatabaseAsync();

        var firstRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login");
        firstRequest.Headers.UserAgent.ParseAdd("stl-agent/1.0");
        firstRequest.Content = JsonContent.Create(new LoginRequest(
            PlatformSeeder.DemoAdminEmail,
            PlatformSeeder.DemoAdminPassword,
            PlatformSeeder.DemoTenantId));
        var firstResponse = await _client.SendAsync(firstRequest);
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        var secondRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login");
        secondRequest.Headers.UserAgent.ParseAdd("stl-agent/2.0");
        secondRequest.Content = JsonContent.Create(new LoginRequest(
            PlatformSeeder.DemoAdminEmail,
            PlatformSeeder.DemoAdminPassword,
            PlatformSeeder.DemoTenantId));
        var secondResponse = await _client.SendAsync(secondRequest);
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var suspiciousEvents = await db.AuditEvents
            .AsNoTracking()
            .Where(x => x.Action == "auth.suspicious_login" && x.ActorUserId == PlatformSeeder.DemoAdminUserId)
            .ToListAsync();

        var suspicious = Assert.Single(suspiciousEvents);
        Assert.Equal("Warning", suspicious.Result);
        Assert.Equal("new_device_or_ip", suspicious.ReasonCode);
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
    public async Task Login_requests_are_rate_limited_by_ip()
    {
        var limitedFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Auth:LoginRateLimitPermitLimit", "1");
            builder.UseSetting("Auth:LoginRateLimitWindowSeconds", "60");
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
                    options.UseInMemoryDatabase("NexArrAuthTests-LoginRateLimit"));
            });
        });

        await SeedDatabaseAsync(limitedFactory);
        var client = limitedFactory.CreateClient();

        var firstResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(PlatformSeeder.DemoAdminEmail, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        var secondResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(PlatformSeeder.DemoAdminEmail, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        Assert.Equal(HttpStatusCode.TooManyRequests, secondResponse.StatusCode);
    }

    [Fact]
    public async Task Platform_admin_login_requires_mfa_when_enforced()
    {
        var mfaFactory = CreateMfaRequiredFactory("NexArrAuthTests-MfaRequired-Denied");
        var client = mfaFactory.CreateClient();
        await SeedDatabaseAsync(mfaFactory);

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(PlatformSeeder.DemoAdminEmail, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Platform_admin_login_succeeds_when_mfa_is_enabled_and_enforced()
    {
        var mfaFactory = CreateMfaRequiredFactory("NexArrAuthTests-MfaRequired-Allowed");
        var client = mfaFactory.CreateClient();
        await SeedDatabaseAsync(mfaFactory);

        string mfaCode;
        using (var scope = mfaFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
            var mfaService = scope.ServiceProvider.GetRequiredService<MfaService>();
            var credential = await db.UserCredentials.SingleAsync(x => x.UserId == PlatformSeeder.DemoAdminUserId);
            credential.IsMfaEnabled = true;
            credential.MfaSecret = mfaService.GenerateSecret();
            credential.MfaRecoveryCodeHashesJson = JsonSerializer.Serialize(
                mfaService.HashRecoveryCodes(mfaService.GenerateRecoveryCodes()));
            await db.SaveChangesAsync();
            mfaCode = mfaService.GenerateTotpCode(credential.MfaSecret, DateTimeOffset.UtcNow);
        }

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(
                PlatformSeeder.DemoAdminEmail,
                PlatformSeeder.DemoAdminPassword,
                PlatformSeeder.DemoTenantId,
                MfaCode: mfaCode));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Platform_admin_login_with_recovery_code_consumes_it()
    {
        var mfaFactory = CreateMfaRequiredFactory("NexArrAuthTests-MfaRequired-Recovery");
        var client = mfaFactory.CreateClient();
        await SeedDatabaseAsync(mfaFactory);

        string recoveryCode;
        using (var scope = mfaFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
            var mfaService = scope.ServiceProvider.GetRequiredService<MfaService>();
            var credential = await db.UserCredentials.SingleAsync(x => x.UserId == PlatformSeeder.DemoAdminUserId);
            credential.IsMfaEnabled = true;
            credential.MfaSecret = mfaService.GenerateSecret();
            var recoveryCodes = mfaService.GenerateRecoveryCodes();
            recoveryCode = recoveryCodes[0];
            credential.MfaRecoveryCodeHashesJson = JsonSerializer.Serialize(mfaService.HashRecoveryCodes(recoveryCodes));
            await db.SaveChangesAsync();
        }

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(
                PlatformSeeder.DemoAdminEmail,
                PlatformSeeder.DemoAdminPassword,
                PlatformSeeder.DemoTenantId,
                RecoveryCode: recoveryCode));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var secondResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(
                PlatformSeeder.DemoAdminEmail,
                PlatformSeeder.DemoAdminPassword,
                PlatformSeeder.DemoTenantId,
                RecoveryCode: recoveryCode));

        Assert.Equal(HttpStatusCode.Forbidden, secondResponse.StatusCode);
    }

    [Fact]
    public async Task Repeated_failed_login_locks_user_and_emits_outbox()
    {
        await SeedDatabaseAsync();

        for (var attempt = 0; attempt < AuthService.FailedLoginLockoutThreshold; attempt++)
        {
            var response = await _client.PostAsJsonAsync(
                "/api/auth/login",
                new LoginRequest(
                    PlatformSeeder.DemoTenantAdminEmail,
                    "wrong-password",
                    PlatformSeeder.DemoTenantId));

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        var lockedResponse = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(
                PlatformSeeder.DemoTenantAdminEmail,
                PlatformSeeder.DemoAdminPassword,
                PlatformSeeder.DemoTenantId));

        Assert.Equal(HttpStatusCode.Locked, lockedResponse.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var user = await db.Users
            .Include(x => x.Credential)
            .SingleAsync(x => x.Id == PlatformSeeder.DemoTenantAdminUserId);

        Assert.Equal(AuthService.FailedLoginLockoutThreshold, user.Credential!.FailedLoginCount);
        Assert.True(user.Credential.LockedUntil > DateTimeOffset.UtcNow);

        var outboxEvent = await db.PlatformOutboxEvents
            .AsNoTracking()
            .SingleAsync(x => x.EventType == PlatformOutboxEventKinds.UserLocked
                && x.PayloadJson.Contains(PlatformSeeder.DemoTenantAdminUserId.ToString()));

        Assert.Contains("failed_login_threshold", outboxEvent.PayloadJson);
    }

    [Fact]
    public async Task Login_v1_with_demo_credentials_returns_tokens()
    {
        await SeedDatabaseAsync();
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest(PlatformSeeder.DemoAdminEmail, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<AuthTokenResponse>();
        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(payload.RefreshToken));
    }

    [Fact]
    public async Task Me_requires_authentication()
    {
        var response = await _client.GetAsync("/api/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_v1_requires_authentication()
    {
        var response = await _client.GetAsync("/api/v1/auth/me");
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
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/me/navigation?currentProductKey=staffarr");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var navigation = await response.Content.ReadFromJsonAsync<NavigationResponse>();
        Assert.NotNull(navigation);
        Assert.True(navigation.Products.Count >= 10);
        var staffarr = navigation.Products.First(p => p.ProductKey.Equals("staffarr", StringComparison.OrdinalIgnoreCase));
        Assert.Equal("workforce", staffarr.ProductCategory);
        Assert.Equal("available", staffarr.ProductStatus);
        Assert.Equal("/app/staffarr/launch", staffarr.LaunchUrl);
        Assert.True(staffarr.IsCurrent);
        Assert.NotEmpty(staffarr.Surfaces);
        Assert.Contains(staffarr.Surfaces, s => s.SurfaceKey == "overview" && s.IsEnabled);
        Assert.Contains(navigation.Products, p => p.ProductKey.Equals("loadarr", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(navigation.Products, p => p.ProductKey.Equals("reportarr", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(navigation.Products, p => p.ProductKey.Equals("assurarr", StringComparison.OrdinalIgnoreCase));

        var fieldcompanion = navigation.Products.First(p =>
            p.ProductKey.Equals("fieldcompanion", StringComparison.OrdinalIgnoreCase));
        Assert.Equal("/app/field-companion", fieldcompanion.RoutePath);
        Assert.Equal("/app/field-companion/launch", fieldcompanion.LaunchUrl);
        Assert.Contains(fieldcompanion.Surfaces, s => s.SurfaceKey == "inbox" && s.IsEnabled);

        var sharedWorker = navigation.Products.First(p =>
            p.ProductKey.Equals("shared-worker", StringComparison.OrdinalIgnoreCase));
        Assert.Equal("worker", sharedWorker.ProductStatus);
        Assert.Contains(sharedWorker.Surfaces, s => s.SurfaceKey == "overview" && s.IsEnabled);
        Assert.DoesNotContain(sharedWorker.Surfaces, s => s.SurfaceKey == "launch");
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
    public async Task Revoke_session_v1_invalidates_refresh_token()
    {
        await SeedDatabaseAsync();
        var tokens = await LoginAsync();

        var revokeRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/auth/sessions/{tokens.SessionId}");
        revokeRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        var revokeResponse = await _client.SendAsync(revokeRequest);
        Assert.Equal(HttpStatusCode.NoContent, revokeResponse.StatusCode);

        var renewResponse = await _client.PostAsJsonAsync(
            "/api/v1/auth/refresh",
            new RenewSessionRequest(tokens.RefreshToken));
        Assert.Equal(HttpStatusCode.Unauthorized, renewResponse.StatusCode);
    }

    [Fact]
    public async Task Sessions_requires_authentication()
    {
        var response = await _client.GetAsync("/api/me/sessions");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Sessions_v1_lists_active_session_after_login()
    {
        await SeedDatabaseAsync();
        var tokens = await LoginAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/sessions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<UserSessionsResponse>();
        Assert.NotNull(payload);
        Assert.NotEmpty(payload.Sessions);
        Assert.Contains(payload.Sessions, s => s.SessionId == tokens.SessionId);
    }

    [Fact]
    public async Task Refresh_v1_renews_tokens()
    {
        await SeedDatabaseAsync();
        var tokens = await LoginAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/refresh",
            new RenewSessionRequest(tokens.RefreshToken));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<AuthTokenResponse>();
        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload.AccessToken));
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

    private WebApplicationFactory<global::NexArr.Api.Program> CreateMfaRequiredFactory(string databaseName)
    {
        return _factory.WithWebHostBuilder(builder =>
            {
                builder.UseSetting("Auth:RequirePlatformAdminMfa", "true");
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
                        options.UseInMemoryDatabase(databaseName));
                });
            });
    }

    private async Task SeedDatabaseAsync(WebApplicationFactory<global::NexArr.Api.Program> factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await PlatformSeeder.SeedAsync(db, hasher);
    }
}
