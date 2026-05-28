using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using NexArr.Api.Services;

namespace STLCompliance.NexArr.Auth.Tests;

public sealed class NexArrCompanionOfflineSyncTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        var dbName = $"NexArrCompanionOffline-{Guid.NewGuid():N}";
        _factory = new WebApplicationFactory<global::NexArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", "test-signing-key-at-least-32-chars-long");
            builder.UseSetting("ServiceToken:SigningKey", "test-signing-key-at-least-32-chars-long");
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<NexArrDbContext>(services);
                services.AddDbContext<NexArrDbContext>(options => options.UseInMemoryDatabase(dbName));
            });
        });

        _client = _factory.CreateClient();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await PlatformSeeder.SeedAsync(db, passwordHasher);
        await EnsureCompanionEntitlementAsync(db);
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Sync_accepts_field_inbox_acknowledge_with_idempotency()
    {
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var idempotencyKey = $"e2e-offline-{Guid.NewGuid():N}";

        var syncRequest = Authorized(HttpMethod.Post, "/api/companion/offline-actions/sync", token);
        syncRequest.Content = JsonContent.Create(new SyncCompanionOfflineActionsRequest(
        [
            new CompanionOfflineActionItem(
                idempotencyKey,
                CompanionOfflineActionKinds.FieldInboxAcknowledge,
                "trainarr:assignment:abc",
                "trainarr",
                DateTimeOffset.UtcNow),
        ]));

        var syncResponse = await _client.SendAsync(syncRequest);
        syncResponse.EnsureSuccessStatusCode();
        var synced = (await syncResponse.Content.ReadFromJsonAsync<SyncCompanionOfflineActionsResponse>())!;
        Assert.Equal(1, synced.Accepted);
        Assert.Equal(0, synced.Duplicates);

        var duplicateRequest = Authorized(HttpMethod.Post, "/api/companion/offline-actions/sync", token);
        duplicateRequest.Content = JsonContent.Create(new SyncCompanionOfflineActionsRequest(
        [
            new CompanionOfflineActionItem(
                idempotencyKey,
                CompanionOfflineActionKinds.FieldInboxAcknowledge,
                "trainarr:assignment:abc",
                "trainarr",
                DateTimeOffset.UtcNow),
        ]));
        var duplicateResponse = await _client.SendAsync(duplicateRequest);
        duplicateResponse.EnsureSuccessStatusCode();
        var duplicate = (await duplicateResponse.Content.ReadFromJsonAsync<SyncCompanionOfflineActionsResponse>())!;
        Assert.Equal(0, duplicate.Accepted);
        Assert.Equal(1, duplicate.Duplicates);

        var listResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/companion/offline-actions?limit=5", token));
        listResponse.EnsureSuccessStatusCode();
        var list = (await listResponse.Content.ReadFromJsonAsync<CompanionOfflineActionsListResponse>())!;
        Assert.Contains(list.Items, item => item.IdempotencyKey == idempotencyKey);
    }

    [Fact]
    public async Task Sync_rejects_unsupported_action_kind()
    {
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var syncRequest = Authorized(HttpMethod.Post, "/api/companion/offline-actions/sync", token);
        syncRequest.Content = JsonContent.Create(new SyncCompanionOfflineActionsRequest(
        [
            new CompanionOfflineActionItem(
                Guid.NewGuid().ToString("N"),
                "unsupported.action",
                "task-1",
                "trainarr",
                DateTimeOffset.UtcNow),
        ]));

        var response = await _client.SendAsync(syncRequest);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string token)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private static async Task EnsureCompanionEntitlementAsync(NexArrDbContext db)
    {
        if (await db.Entitlements.AnyAsync(e =>
                e.TenantId == PlatformSeeder.DemoTenantId && e.ProductKey == "companion"))
        {
            return;
        }

        db.Entitlements.Add(new TenantProductEntitlement
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            ProductKey = "companion",
            Status = EntitlementStatuses.Active,
            GrantedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync();
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
