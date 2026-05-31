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

namespace STLCompliance.NexArr.Auth.Tests;

public class NexArrPlatformIdentityIntegrationTests : IClassFixture<WebApplicationFactory<global::NexArr.Api.Program>>
{
    private readonly WebApplicationFactory<global::NexArr.Api.Program> _factory;
    private readonly HttpClient _client;

    public NexArrPlatformIdentityIntegrationTests(WebApplicationFactory<global::NexArr.Api.Program> factory)
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
                    options.UseInMemoryDatabase("NexArrPlatformIdentityIntegrationTests"));
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task StaffArr_can_create_and_products_can_resolve_minimal_platform_identity()
    {
        await SeedDatabaseAsync();
        var adminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var staffarrCreateToken = await IssueServiceTokenAsync(
            adminToken,
            "staffarr",
            PlatformIdentityIntegrationService.CreateIdentityActionScope,
            PlatformSeeder.DemoTenantId);

        var createRequest = Authorized(HttpMethod.Post, "/api/internal/platform-identities", staffarrCreateToken);
        createRequest.Content = JsonContent.Create(new CreatePlatformIdentityRequest(
            PlatformSeeder.DemoTenantId,
            "new.worker@example.com",
            "New Worker",
            "driver"));
        var createResponse = await _client.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = (await createResponse.Content.ReadFromJsonAsync<CreatePlatformIdentityResponse>())!;
        Assert.True(created.WasCreated);
        Assert.True(created.MembershipWasCreated);
        Assert.False(created.Identity.CanLogin);
        Assert.Equal("invited", created.Identity.Status);
        Assert.Contains(created.Identity.TenantMemberships, m =>
            m.TenantId == PlatformSeeder.DemoTenantId && m.RoleKey == "driver" && m.IsActive);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
            var user = await db.Users.Include(u => u.Credential).FirstAsync(u => u.Id == created.Identity.PersonId);
            Assert.Null(user.Credential);

            var outboxEvent = await db.PlatformOutboxEvents
                .FirstOrDefaultAsync(e => e.EventType == PlatformOutboxEventKinds.UserCreated
                    && e.TenantId == PlatformSeeder.DemoTenantId);
            Assert.NotNull(outboxEvent);
        }

        var maintainarrReadToken = await IssueServiceTokenAsync(
            adminToken,
            "maintainarr",
            PlatformIdentityIntegrationService.ReadIdentityActionScope,
            PlatformSeeder.DemoTenantId);
        var resolveRequest = Authorized(
            HttpMethod.Get,
            $"/api/internal/platform-identities/{created.Identity.PersonId}?tenantId={PlatformSeeder.DemoTenantId}",
            maintainarrReadToken);
        var resolveResponse = await _client.SendAsync(resolveRequest);
        resolveResponse.EnsureSuccessStatusCode();
        var resolved = (await resolveResponse.Content.ReadFromJsonAsync<PlatformIdentityResponse>())!;

        Assert.Equal(created.Identity.PersonId, resolved.PersonId);
        Assert.Equal("new.worker@example.com", resolved.Email);
        Assert.False(resolved.CanLogin);
        Assert.True(resolved.LaunchEligible);
        Assert.Equal("invited", resolved.Status);
    }

    [Fact]
    public async Task Platform_identity_create_rejects_non_staffarr_source()
    {
        await SeedDatabaseAsync();
        var adminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var maintainarrToken = await IssueServiceTokenAsync(
            adminToken,
            "maintainarr",
            PlatformIdentityIntegrationService.CreateIdentityActionScope,
            PlatformSeeder.DemoTenantId);

        var request = Authorized(HttpMethod.Post, "/api/internal/platform-identities", maintainarrToken);
        request.Content = JsonContent.Create(new CreatePlatformIdentityRequest(
            PlatformSeeder.DemoTenantId,
            "bad-create@example.com",
            "Bad Create"));
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Platform_identity_resolve_requires_read_action_scope()
    {
        await SeedDatabaseAsync();
        var adminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var wrongScopeToken = await IssueServiceTokenAsync(
            adminToken,
            "maintainarr",
            PlatformIdentityIntegrationService.CreateIdentityActionScope,
            PlatformSeeder.DemoTenantId);

        var request = Authorized(
            HttpMethod.Get,
            $"/api/internal/platform-identities/{PlatformSeeder.DemoTenantAdminUserId}?tenantId={PlatformSeeder.DemoTenantId}",
            wrongScopeToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Platform_identity_resolve_returns_active_status_for_login_enabled_user()
    {
        await SeedDatabaseAsync();
        var adminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var tenantAdminToken = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);
        var readToken = await IssueServiceTokenAsync(
            adminToken,
            "maintainarr",
            PlatformIdentityIntegrationService.ReadIdentityActionScope,
            PlatformSeeder.DemoTenantId);

        var handoffRequest = Authorized(HttpMethod.Post, "/api/v1/launch/handoff", tenantAdminToken);
        handoffRequest.Content = JsonContent.Create(new CreateHandoffRequest(
            "staffarr",
            "http://localhost:5173/app/staffarr"));
        var handoffResponse = await _client.SendAsync(handoffRequest);
        handoffResponse.EnsureSuccessStatusCode();

        var request = Authorized(
            HttpMethod.Get,
            $"/api/internal/platform-identities/{PlatformSeeder.DemoTenantAdminUserId}?tenantId={PlatformSeeder.DemoTenantId}",
            readToken);
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var identity = (await response.Content.ReadFromJsonAsync<PlatformIdentityResponse>())!;

        Assert.True(identity.CanLogin);
        Assert.True(identity.LaunchEligible);
        Assert.Equal("active", identity.Status);
        Assert.Null(identity.SecondaryEmail);
        Assert.Null(identity.PhoneNumber);
        Assert.Null(identity.AvatarUrl);
        Assert.NotNull(identity.LastProductLaunchAt);
    }

    [Fact]
    public async Task StaffArr_can_sync_existing_platform_identity_by_person_id()
    {
        await SeedDatabaseAsync();
        var adminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var staffarrSyncToken = await IssueServiceTokenAsync(
            adminToken,
            "staffarr",
            PlatformIdentityIntegrationService.CreateIdentityActionScope,
            PlatformSeeder.DemoTenantId);

        var request = Authorized(
            HttpMethod.Put,
            $"/api/internal/platform-identities/{PlatformSeeder.DemoTenantAdminUserId}",
            staffarrSyncToken);
        request.Content = JsonContent.Create(new SyncPlatformIdentityRequest(
            PlatformSeeder.DemoTenantId,
            "Updated Tenant Admin",
            "driver"));
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var identity = (await response.Content.ReadFromJsonAsync<PlatformIdentityResponse>())!;

        Assert.Equal(PlatformSeeder.DemoTenantAdminUserId, identity.PersonId);
        Assert.Equal("Updated Tenant Admin", identity.DisplayName);
        Assert.True(identity.LaunchEligible);
        Assert.Contains(identity.TenantMemberships, x =>
            x.TenantId == PlatformSeeder.DemoTenantId
            && x.RoleKey == "driver"
            && x.IsActive);
    }

    [Fact]
    public async Task Platform_identity_resolve_returns_not_launch_eligible_when_tenant_has_no_entitlements()
    {
        await SeedDatabaseAsync();
        var adminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var readToken = await IssueServiceTokenAsync(
            adminToken,
            "maintainarr",
            PlatformIdentityIntegrationService.ReadIdentityActionScope,
            PlatformSeeder.DemoTenantId);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
            var entitlements = await db.Entitlements
                .Where(x => x.TenantId == PlatformSeeder.DemoTenantId)
                .ToListAsync();
            foreach (var entitlement in entitlements)
            {
                entitlement.Status = EntitlementStatuses.Revoked;
            }
            await db.SaveChangesAsync();
        }

        var request = Authorized(
            HttpMethod.Get,
            $"/api/internal/platform-identities/{PlatformSeeder.DemoTenantAdminUserId}?tenantId={PlatformSeeder.DemoTenantId}",
            readToken);
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var identity = (await response.Content.ReadFromJsonAsync<PlatformIdentityResponse>())!;

        Assert.False(identity.LaunchEligible);
    }

    private async Task<string> IssueServiceTokenAsync(
        string adminToken,
        string sourceProduct,
        string actionScope,
        Guid tenantId)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"identity-{sourceProduct}-{Guid.NewGuid():N}",
            $"{sourceProduct} identity integration test",
            sourceProduct,
            ["nexarr"]));
        var registerResponse = await _client.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/service-tokens", adminToken);
        issueRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            tenantId,
            ["nexarr"],
            actionScope,
            30));
        var issueResponse = await _client.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        return (await issueResponse.Content.ReadFromJsonAsync<ServiceTokenIssueResponse>())!.AccessToken;
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
        return (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!.AccessToken;
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
