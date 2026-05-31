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

public class NexArrPlatformOutboxTests : IClassFixture<WebApplicationFactory<global::NexArr.Api.Program>>
{
    private readonly WebApplicationFactory<global::NexArr.Api.Program> _factory;
    private readonly HttpClient _client;
    private string _workerToken = null!;

    public NexArrPlatformOutboxTests(WebApplicationFactory<global::NexArr.Api.Program> factory)
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
                    options.UseInMemoryDatabase("NexArrPlatformOutboxTests"));
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Settings_requires_platform_admin()
    {
        await SeedDatabaseAsync();
        var tenantAdminToken = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/platform-admin/platform-outbox/settings", tenantAdminToken));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Process_batch_rejects_missing_service_token()
    {
        await SeedDatabaseAsync();
        var response = await _client.PostAsJsonAsync(
            "/api/internal/platform-outbox/process-batch",
            new ProcessPlatformOutboxPublisherRequest(null, 50));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Tenant_create_enqueues_outbox_event()
    {
        await SeedDatabaseAsync();
        var adminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var createRequest = Authorized(HttpMethod.Post, "/api/tenants", adminToken);
        createRequest.Content = JsonContent.Create(new CreateTenantRequest(
            $"outbox-{Guid.NewGuid():N}",
            "Outbox Test Tenant"));
        var createResponse = await _client.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var outbox = await db.PlatformOutboxEvents
            .Where(x => x.EventType == PlatformOutboxEventKinds.TenantCreated)
            .ToListAsync();
        Assert.NotEmpty(outbox);
    }

    [Fact]
    public async Task Process_batch_publishes_pending_events_and_records_run()
    {
        await SeedDatabaseAsync();
        var adminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        _workerToken = await IssueServiceTokenAsync(
            adminToken,
            PlatformOutboxPublisherWorkerService.ProcessPublishActionScope);

        await EnablePublisherAsync(adminToken);

        var insertedEventId = Guid.Empty;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
            var now = DateTimeOffset.UtcNow;
            insertedEventId = Guid.NewGuid();
            db.PlatformOutboxEvents.Add(new PlatformOutboxEvent
            {
                Id = insertedEventId,
                EventType = PlatformOutboxEventKinds.TenantUpdated,
                IdempotencyKey = $"tenant.updated:tenant:{PlatformSeeder.DemoTenantId}:{now.ToUnixTimeMilliseconds()}",
                SchemaVersion = 1,
                PayloadJson = "{\"schemaVersion\":1,\"targetType\":\"tenant\",\"targetId\":\"" + PlatformSeeder.DemoTenantId + "\",\"summary\":\"Test\"}",
                TenantId = PlatformSeeder.DemoTenantId,
                ProcessingStatus = PlatformOutboxEventStatuses.Pending,
                OccurredAt = now,
                CreatedAt = now,
                UpdatedAt = now,
            });
            await db.SaveChangesAsync();
        }

        var processRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/platform-outbox/process-batch",
            _workerToken);
        processRequest.Content = JsonContent.Create(new ProcessPlatformOutboxPublisherRequest(null, 50));
        var processResponse = await _client.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var batch = (await processResponse.Content.ReadFromJsonAsync<ProcessPlatformOutboxPublisherResponse>())!;

        Assert.True(batch.PublishedCount >= 1);
        Assert.Contains(insertedEventId, batch.PublishedEventIds);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
            var published = await db.PlatformOutboxEvents.FirstAsync(x => x.Id == insertedEventId);
            Assert.Equal(PlatformOutboxEventStatuses.Published, published.ProcessingStatus);
            Assert.NotNull(published.PublishedAt);
            Assert.True(await db.PlatformOutboxPublisherRuns.AnyAsync());
        }
    }

    [Fact]
    public async Task Outbox_enqueue_is_idempotent()
    {
        await SeedDatabaseAsync();
        using var scope = _factory.Services.CreateScope();
        var enqueue = scope.ServiceProvider.GetRequiredService<PlatformOutboxEnqueueService>();
        var tenantId = PlatformSeeder.DemoTenantId;
        var payload = new PlatformOutboxPayload(
            1,
            tenantId,
            PlatformSeeder.DemoAdminUserId,
            "tenant",
            tenantId.ToString(),
            "Test");

        await enqueue.TryEnqueueAsync(
            PlatformOutboxEventKinds.TenantUpdated,
            "tenant",
            tenantId.ToString(),
            "change-1",
            payload);
        await enqueue.TryEnqueueAsync(
            PlatformOutboxEventKinds.TenantUpdated,
            "tenant",
            tenantId.ToString(),
            "change-1",
            payload);

        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var count = await db.PlatformOutboxEvents.CountAsync(
            x => x.EventType == PlatformOutboxEventKinds.TenantUpdated && x.TenantId == tenantId);
        Assert.Equal(1, count);
    }

    private async Task EnablePublisherAsync(string adminToken)
    {
        var request = Authorized(
            HttpMethod.Put,
            "/api/platform-admin/platform-outbox/settings",
            adminToken);
        request.Content = JsonContent.Create(new UpsertPlatformOutboxPublisherSettingsRequest(
            true,
            5,
            5));
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    private async Task<string> IssueServiceTokenAsync(string adminToken, string actionScope)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"nexarr-worker-outbox-{Guid.NewGuid():N}",
            "nexarr-worker outbox test",
            "nexarr-worker",
            ["nexarr"]));
        var registerResponse = await _client.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/service-tokens", adminToken);
        issueRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            null,
            ["nexarr"],
            actionScope,
            30));
        var issueResponse = await _client.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        var issued = (await issueResponse.Content.ReadFromJsonAsync<ServiceTokenIssueResponse>())!;
        return issued.AccessToken;
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
