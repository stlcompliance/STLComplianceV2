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
using STLCompliance.Shared.Contracts;

namespace STLCompliance.NexArr.Auth.Tests;

public sealed class NexArrCompanionOfflineSyncTests : IAsyncLifetime
{
    private static readonly Guid AssignmentId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
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
            builder.UseSetting("TrainArr__BaseUrl", "http://trainarr.test");
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<NexArrDbContext>(services);
                services.AddDbContext<NexArrDbContext>(options => options.UseInMemoryDatabase(dbName));
                services.AddHttpClient(nameof(CompanionProductClient))
                    .ConfigurePrimaryHttpMessageHandler(() => new FieldInboxStubHandler());
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
                $"trainarr:assignment:{AssignmentId:D}",
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
                $"trainarr:assignment:{AssignmentId:D}",
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
        response.EnsureSuccessStatusCode();
        var payload = (await response.Content.ReadFromJsonAsync<SyncCompanionOfflineActionsResponse>())!;
        Assert.Equal(0, payload.Accepted);
        Assert.Equal(1, payload.Rejected);
        Assert.Equal("companion.offline_actions.unsupported_kind", payload.RejectedItems[0].ReasonCode);
    }

    [Fact]
    public async Task Sync_accepts_valid_actions_and_rejects_invalid_in_same_batch()
    {
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var validKey = $"offline-mixed-valid-{Guid.NewGuid():N}";
        var invalidKey = $"offline-mixed-invalid-{Guid.NewGuid():N}";
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post,
            "/api/companion/offline-actions/sync",
            token,
            new SyncCompanionOfflineActionsRequest(
            [
                new CompanionOfflineActionItem(
                    validKey,
                    CompanionOfflineActionKinds.FieldInboxAcknowledge,
                    $"trainarr:assignment:{AssignmentId:D}",
                    "trainarr",
                    DateTimeOffset.UtcNow),
                new CompanionOfflineActionItem(
                    invalidKey,
                    CompanionOfflineActionKinds.FieldInboxAcknowledge,
                    "trainarr:assignment:abc",
                    "trainarr",
                    DateTimeOffset.UtcNow),
            ])));

        response.EnsureSuccessStatusCode();
        var payload = (await response.Content.ReadFromJsonAsync<SyncCompanionOfflineActionsResponse>())!;
        Assert.Equal(1, payload.Accepted);
        Assert.Equal(1, payload.Rejected);
        Assert.Contains(payload.Synced, item => item.IdempotencyKey == validKey);
        Assert.Contains(payload.RejectedItems, item => item.IdempotencyKey == invalidKey);
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

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string token, object? body = null)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

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

    private sealed class FieldInboxStubHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request.RequestUri?.AbsolutePath.EndsWith("/api/field-inbox", StringComparison.OrdinalIgnoreCase) == true
                && request.RequestUri.Host.Contains("trainarr", StringComparison.OrdinalIgnoreCase))
            {
                var inbox = new FieldInboxResponse(
                    new FieldInboxSummary(1, 0, new Dictionary<string, int> { ["trainarr"] = 1 }),
                    [
                        new FieldInboxTaskItem(
                            $"trainarr:assignment:{AssignmentId:D}",
                            "trainarr",
                            "training_assignment",
                            "Offline sync assignment",
                            null,
                            "assigned",
                            null,
                            null,
                            DateTimeOffset.UtcNow,
                            $"/assignments/{AssignmentId:D}",
                            null,
                            $"http://trainarr.test/assignments/{AssignmentId:D}"),
                    ]);

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(inbox),
                });
            }

            var empty = new FieldInboxResponse(
                FieldInboxRules.BuildProductResponse([]).Summary,
                []);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(empty),
            });
        }
    }
}
