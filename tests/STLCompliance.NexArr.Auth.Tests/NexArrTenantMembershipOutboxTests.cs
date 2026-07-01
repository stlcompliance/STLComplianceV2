using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using NexArr.Api.Services;

namespace STLCompliance.NexArr.Auth.Tests;

public class NexArrTenantMembershipOutboxTests : IClassFixture<WebApplicationFactory<global::NexArr.Api.Program>>
{
    private readonly WebApplicationFactory<global::NexArr.Api.Program> _factory;
    private readonly HttpClient _client;
    private readonly RecordingStaffArrProvisioningClient _staffArrProvisioning = new();

    public NexArrTenantMembershipOutboxTests(WebApplicationFactory<global::NexArr.Api.Program> factory)
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
                    options.UseInMemoryDatabase("NexArrTenantMembershipOutboxTests"));
                services.RemoveAll<IStaffArrPersonProvisioningClient>();
                services.AddSingleton<IStaffArrPersonProvisioningClient>(_staffArrProvisioning);
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Add_member_enqueues_membership_added_outbox_event()
    {
        await SeedDatabaseAsync();
        var adminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var createTenant = Authorized(HttpMethod.Post, "/api/tenants", adminToken);
        createTenant.Content = JsonContent.Create(new CreateTenantRequest(
            $"membership-{Guid.NewGuid():N}",
            "Membership Outbox Tenant"));
        var createResponse = await _client.SendAsync(createTenant);
        createResponse.EnsureSuccessStatusCode();
        var tenant = (await createResponse.Content.ReadFromJsonAsync<TenantDetailResponse>())!;

        var addRequest = Authorized(HttpMethod.Post, $"/api/tenants/{tenant.TenantId}/members", adminToken);
        addRequest.Content = JsonContent.Create(new AddTenantMemberRequest(
            PlatformSeeder.DemoTenantAdminUserId,
            "tenant_user"));
        var addResponse = await _client.SendAsync(addRequest);
        addResponse.EnsureSuccessStatusCode();
        Assert.Contains(_staffArrProvisioning.Requests, call =>
            call.TenantId == tenant.TenantId
            && call.ExternalUserId == PlatformSeeder.DemoTenantAdminUserId);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var outbox = await db.PlatformOutboxEvents
            .Where(x => x.EventType == PlatformOutboxEventKinds.TenantMembershipAdded
                && x.TenantId == tenant.TenantId)
            .ToListAsync();
        Assert.Single(outbox);
    }

    [Fact]
    public async Task Remove_member_enqueues_membership_removed_outbox_event()
    {
        await SeedDatabaseAsync();
        var adminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var removeRequest = Authorized(
            HttpMethod.Delete,
            $"/api/tenants/{PlatformSeeder.DemoTenantId}/members/{PlatformSeeder.DemoTenantAdminUserId}",
            adminToken);
        var removeResponse = await _client.SendAsync(removeRequest);
        removeResponse.EnsureSuccessStatusCode();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var outbox = await db.PlatformOutboxEvents
            .FirstOrDefaultAsync(x => x.EventType == PlatformOutboxEventKinds.TenantMembershipRemoved
                && x.TenantId == PlatformSeeder.DemoTenantId);
        Assert.NotNull(outbox);
    }

    [Fact]
    public async Task Remove_member_is_idempotent_for_outbox()
    {
        await SeedDatabaseAsync();
        var adminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var url =
            $"/api/tenants/{PlatformSeeder.DemoTenantId}/members/{PlatformSeeder.DemoTenantAdminUserId}";
        var first = await _client.SendAsync(Authorized(HttpMethod.Delete, url, adminToken));
        first.EnsureSuccessStatusCode();
        var second = await _client.SendAsync(Authorized(HttpMethod.Delete, url, adminToken));
        second.EnsureSuccessStatusCode();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var removedEvents = await db.PlatformOutboxEvents
            .CountAsync(x => x.EventType == PlatformOutboxEventKinds.TenantMembershipRemoved
                && x.TenantId == PlatformSeeder.DemoTenantId);
        Assert.Equal(1, removedEvents);
    }

    [Fact]
    public async Task Tenant_admin_can_list_members_for_own_tenant()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/tenants/{PlatformSeeder.DemoTenantId}/members", token));
        response.EnsureSuccessStatusCode();
        var payload = (await response.Content.ReadFromJsonAsync<TenantMembersListResponse>())!;
        Assert.Equal(PlatformSeeder.DemoTenantId, payload.TenantId);
        Assert.NotEmpty(payload.Members);
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
        _staffArrProvisioning.Requests.Clear();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await PlatformSeeder.SeedAsync(db, hasher);
    }

    private sealed class RecordingStaffArrProvisioningClient : IStaffArrPersonProvisioningClient
    {
        public List<ProvisioningCall> Requests { get; } = [];

        public Task EnsurePersonAsync(
            Guid tenantId,
            Guid externalUserId,
            string email,
            string displayName,
            Guid? requestedByUserId,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(new ProvisioningCall(tenantId, externalUserId, email, displayName, requestedByUserId));
            return Task.CompletedTask;
        }
    }

    private sealed record ProvisioningCall(
        Guid TenantId,
        Guid ExternalUserId,
        string Email,
        string DisplayName,
        Guid? RequestedByUserId);
}
