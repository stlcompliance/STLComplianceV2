using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
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
    public async Task Platform_admin_can_diagnose_handoff_redeem_after_entitlement_revoked()
    {
        await SeedDatabaseAsync();
        var platformAdminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var tenantAdminToken = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var handoffRequest = Authorized(HttpMethod.Post, "/api/v1/launch/handoff", tenantAdminToken);
        handoffRequest.Content = JsonContent.Create(new CreateHandoffRequest(
            "staffarr",
            "http://localhost:5173/app/staffarr"));
        var handoffResponse = await _client.SendAsync(handoffRequest);
        handoffResponse.EnsureSuccessStatusCode();
        var handoff = (await handoffResponse.Content.ReadFromJsonAsync<HandoffCreatedResponse>())!;
        var serviceToken = await IssueServiceTokenAsync(platformAdminToken, "staffarr");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
            var entitlement = await db.Entitlements.SingleAsync(
                x => x.TenantId == PlatformSeeder.DemoTenantId && x.ProductKey == "staffarr");
            entitlement.Status = EntitlementStatuses.Revoked;
            await db.SaveChangesAsync();
        }

        var redeemRequest = Authorized(HttpMethod.Post, "/api/v1/handoff/redeem", platformAdminToken);
        redeemRequest.Content = JsonContent.Create(new RedeemHandoffRequest(handoff.HandoffCode, serviceToken));
        var redeemResponse = await _client.SendAsync(redeemRequest);
        Assert.Equal(HttpStatusCode.Unauthorized, redeemResponse.StatusCode);

        var attemptsResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Get,
                "/api/platform-admin/launch-attempts?productKey=staffarr&result=Denied",
                platformAdminToken));
        attemptsResponse.EnsureSuccessStatusCode();
        var attempts = (await attemptsResponse.Content.ReadFromJsonAsync<PagedResult<LaunchAttemptTimelineItemResponse>>())!;
        var redeemAttempt = Assert.Single(attempts.Items, x => x.Action == "launch.handoff.redeem");

        Assert.Equal("entitlement_revoked", redeemAttempt.ReasonCode);
        Assert.Equal("staffarr", redeemAttempt.ProductKey);
        Assert.Equal(PlatformSeeder.DemoTenantId, redeemAttempt.TenantId);
        Assert.Equal(PlatformSeeder.DemoTenantAdminEmail, redeemAttempt.ActorEmail);
        Assert.Contains("entitlement", redeemAttempt.RemediationHint, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Platform_admin_can_diagnose_handoff_redeem_without_service_token()
    {
        await SeedDatabaseAsync();
        var platformAdminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var tenantAdminToken = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var handoffRequest = Authorized(HttpMethod.Post, "/api/v1/launch/handoff", tenantAdminToken);
        handoffRequest.Content = JsonContent.Create(new CreateHandoffRequest(
            "staffarr",
            "http://localhost:5173/app/staffarr"));
        var handoffResponse = await _client.SendAsync(handoffRequest);
        handoffResponse.EnsureSuccessStatusCode();
        var handoff = (await handoffResponse.Content.ReadFromJsonAsync<HandoffCreatedResponse>())!;

        var redeemRequest = Authorized(HttpMethod.Post, "/api/v1/handoff/redeem", tenantAdminToken);
        redeemRequest.Content = JsonContent.Create(new RedeemHandoffRequest(handoff.HandoffCode, null));
        var redeemResponse = await _client.SendAsync(redeemRequest);
        Assert.Equal(HttpStatusCode.Forbidden, redeemResponse.StatusCode);

        var attemptsResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Get,
                "/api/platform-admin/launch-attempts?productKey=staffarr&result=Denied",
                platformAdminToken));
        attemptsResponse.EnsureSuccessStatusCode();
        var attempts = (await attemptsResponse.Content.ReadFromJsonAsync<PagedResult<LaunchAttemptTimelineItemResponse>>())!;
        var redeemAttempt = Assert.Single(attempts.Items, x => x.Action == "launch.handoff.redeem");

        Assert.Equal("auth.forbidden", redeemAttempt.ReasonCode);
        Assert.Equal("staffarr", redeemAttempt.ProductKey);
        Assert.Equal(PlatformSeeder.DemoTenantAdminEmail, redeemAttempt.ActorEmail);
        Assert.Contains("service token", redeemAttempt.RemediationHint, StringComparison.OrdinalIgnoreCase);
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
    public async Task Platform_admin_can_create_and_update_user_with_outbox_events()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var createRequest = Authorized(HttpMethod.Post, "/api/v1/platform-admin/users", token);
        createRequest.Content = JsonContent.Create(new CreatePlatformUserRequest(
            "Ops-Lead@Example.test",
            "Ops Lead",
            "StrongPass1234",
            IsPlatformAdmin: false));

        var createResponse = await _client.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = (await createResponse.Content.ReadFromJsonAsync<PlatformUserDetailResponse>())!;

        Assert.Equal("ops-lead@example.test", created.Email);
        Assert.Equal("Ops Lead", created.DisplayName);
        Assert.True(created.IsActive);
        Assert.False(created.IsPlatformAdmin);

        var updateRequest = Authorized(HttpMethod.Patch, $"/api/v1/platform-admin/users/{created.UserId}", token);
        updateRequest.Content = JsonContent.Create(new UpdatePlatformUserRequest(
            "ops-admin@example.test",
            "Ops Admin",
            IsPlatformAdmin: true));

        var updateResponse = await _client.SendAsync(updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = (await updateResponse.Content.ReadFromJsonAsync<PlatformUserDetailResponse>())!;

        Assert.Equal(created.UserId, updated.UserId);
        Assert.Equal("ops-admin@example.test", updated.Email);
        Assert.Equal("Ops Admin", updated.DisplayName);
        Assert.True(updated.IsPlatformAdmin);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var user = await db.Users.AsNoTracking().SingleAsync(x => x.Id == created.UserId);
        Assert.Equal("ops-admin@example.test", user.Email);
        Assert.True(user.IsPlatformAdmin);

        var outboxEvents = await db.PlatformOutboxEvents
            .AsNoTracking()
            .Where(x => x.PayloadJson.Contains(created.UserId.ToString()))
            .ToListAsync();

        var createdEvent = Assert.Single(outboxEvents, x => x.EventType == PlatformOutboxEventKinds.UserCreated);
        Assert.Contains("ops-lead@example.test", createdEvent.PayloadJson);

        var updatedEvent = Assert.Single(outboxEvents, x => x.EventType == PlatformOutboxEventKinds.UserUpdated);
        Assert.Contains("ops-admin@example.test", updatedEvent.PayloadJson);
        Assert.Contains("Ops Lead", updatedEvent.PayloadJson);
    }

    [Fact]
    public async Task Tenant_admin_cannot_create_platform_user()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var request = Authorized(HttpMethod.Post, "/api/v1/platform-admin/users", token);
        request.Content = JsonContent.Create(new CreatePlatformUserRequest(
            "tenant-user@example.test",
            "Tenant User",
            "StrongPass1234"));

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Platform_admin_can_lock_and_unlock_user_with_outbox_events()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var lockResponse = await _client.SendAsync(
            Authorized(HttpMethod.Post, $"/api/v1/platform-admin/users/{PlatformSeeder.DemoTenantAdminUserId}/lock", token));

        Assert.Equal(HttpStatusCode.OK, lockResponse.StatusCode);
        var locked = (await lockResponse.Content.ReadFromJsonAsync<PlatformUserLockResponse>())!;
        Assert.False(locked.WasAlreadyLocked);
        Assert.True(locked.LockedUntil > DateTimeOffset.UtcNow);

        var blockedLoginResponse = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(
                PlatformSeeder.DemoTenantAdminEmail,
                PlatformSeeder.DemoAdminPassword,
                PlatformSeeder.DemoTenantId));

        Assert.Equal(HttpStatusCode.Locked, blockedLoginResponse.StatusCode);

        var unlockResponse = await _client.SendAsync(
            Authorized(HttpMethod.Post, $"/api/v1/platform-admin/users/{PlatformSeeder.DemoTenantAdminUserId}/unlock", token));

        Assert.Equal(HttpStatusCode.OK, unlockResponse.StatusCode);
        var unlocked = (await unlockResponse.Content.ReadFromJsonAsync<PlatformUserUnlockResponse>())!;
        Assert.False(unlocked.WasAlreadyUnlocked);

        var loginResponse = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(
                PlatformSeeder.DemoTenantAdminEmail,
                PlatformSeeder.DemoAdminPassword,
                PlatformSeeder.DemoTenantId));

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var user = await db.Users
            .Include(x => x.Credential)
            .SingleAsync(x => x.Id == PlatformSeeder.DemoTenantAdminUserId);

        Assert.Null(user.Credential!.LockedUntil);
        Assert.Equal(0, user.Credential.FailedLoginCount);

        var outboxEvents = await db.PlatformOutboxEvents
            .AsNoTracking()
            .Where(x => x.PayloadJson.Contains(PlatformSeeder.DemoTenantAdminUserId.ToString()))
            .ToListAsync();

        Assert.Single(outboxEvents, x => x.EventType == PlatformOutboxEventKinds.UserLocked);
        Assert.Single(outboxEvents, x => x.EventType == PlatformOutboxEventKinds.UserUnlocked);
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

    private async Task<string> IssueServiceTokenAsync(string adminToken, string productKey)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{productKey}-platform-admin-test",
            $"{productKey} Platform Admin Test",
            productKey,
            [productKey]));
        var registerResponse = await _client.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/service-tokens", adminToken);
        issueRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            [productKey],
            "launch.redeem",
            30));
        var issueResponse = await _client.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        return (await issueResponse.Content.ReadFromJsonAsync<ServiceTokenIssueResponse>())!.AccessToken;
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
