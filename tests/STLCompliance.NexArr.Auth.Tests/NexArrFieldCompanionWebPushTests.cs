using System.Net;
using System.Net.Http.Headers;
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
using WebPush;

namespace STLCompliance.NexArr.Auth.Tests;

public sealed class NexArrFieldCompanionWebPushTests : IAsyncLifetime
{
    private static readonly VapidDetails TestVapid = VapidHelper.GenerateVapidKeys();

    private readonly List<FieldCompanionWebPushSendCall> _pushCalls = [];
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private string _sharedWorkerToNexArrToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"NexArrFieldCompanionWebPush-{Guid.NewGuid():N}";

        _nexarrFactory = new WebApplicationFactory<global::NexArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.UseSetting("FieldCompanionWebPush:Subject", "mailto:fieldcompanion-push@test.local");
            builder.UseSetting("FieldCompanionWebPush:PublicKey", TestVapid.PublicKey);
            builder.UseSetting("FieldCompanionWebPush:PrivateKey", TestVapid.PrivateKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<NexArrDbContext>(services);
                services.AddDbContext<NexArrDbContext>(options => options.UseInMemoryDatabase(nexArrDbName));
                services.AddSingleton<IFieldCompanionWebPushSender>(new RecordingWebPushSender(_pushCalls));
            });
        });

        _nexarrClient = _nexarrFactory.CreateClient();
        await SeedNexArrAsync();
        await EnsureFieldCompanionEntitlementAsync();

        var adminToken = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        _sharedWorkerToNexArrToken = await IssueServiceTokenAsync(
            adminToken,
            "shared-worker",
            ["nexarr"],
            FieldCompanionNotificationDispatchService.ProcessNotificationsActionScope);
    }

    public async Task DisposeAsync()
    {
        _nexarrClient.Dispose();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Subscribe_persists_push_subscription_for_fieldcompanion_user()
    {
        var session = await RedeemFieldCompanionSessionAsync();
        var subscribeResponse = await _nexarrClient.SendAsync(Authorized(
            HttpMethod.Post,
            "/api/fieldcompanion/push/subscribe",
            session.AccessToken,
            new UpsertFieldCompanionPushSubscriptionRequest(
                "https://push.example.test/device-1",
                new FieldCompanionPushSubscriptionKeysRequest("p256dh-test-key", "auth-test-key"),
                "vitest")));
        subscribeResponse.EnsureSuccessStatusCode();

        await using var scope = _nexarrFactory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var row = await db.FieldCompanionPushSubscriptions.SingleAsync(x =>
            x.TenantId == PlatformSeeder.DemoTenantId && x.UserId == session.UserId);
        Assert.Equal("https://push.example.test/device-1", row.Endpoint);
    }

    [Fact]
    public async Task Field_inbox_refresh_dispatches_web_push_without_webhook()
    {
        await UpsertNotificationSettingsAsync(webhookUrl: null);
        var session = await RedeemFieldCompanionSessionAsync();

        var subscribeResponse = await _nexarrClient.SendAsync(Authorized(
            HttpMethod.Post,
            "/api/fieldcompanion/push/subscribe",
            session.AccessToken,
            new UpsertFieldCompanionPushSubscriptionRequest(
                "https://push.example.test/device-2",
                new FieldCompanionPushSubscriptionKeysRequest("p256dh-test-key-2", "auth-test-key-2"),
                null)));
        subscribeResponse.EnsureSuccessStatusCode();

        var inboxResponse = await _nexarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/fieldcompanion/field-inbox", session.AccessToken));
        inboxResponse.EnsureSuccessStatusCode();

        var processRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/fieldcompanion-notifications/process-batch",
            _sharedWorkerToNexArrToken);
        processRequest.Content = JsonContent.Create(new ProcessFieldCompanionNotificationsRequest(
            PlatformSeeder.DemoTenantId,
            null,
            10));
        var processResponse = await _nexarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var batch = (await processResponse.Content.ReadFromJsonAsync<ProcessFieldCompanionNotificationsResponse>())!;
        Assert.Equal(1, batch.DispatchedCount);
        Assert.NotEmpty(_pushCalls);

        await using var scope = _nexarrFactory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var dispatched = await db.FieldCompanionNotificationDispatches.SingleAsync(x =>
            x.TenantId == PlatformSeeder.DemoTenantId
            && x.EventKind == FieldCompanionNotificationEventKinds.FieldInboxRefreshed);
        Assert.Equal(FieldCompanionNotificationDispatchStatuses.Sent, dispatched.DispatchStatus);
        Assert.Equal(1, dispatched.PushDeliveredCount);
    }

    [Fact]
    public async Task Unsubscribe_removes_push_subscription()
    {
        var session = await RedeemFieldCompanionSessionAsync();
        const string endpoint = "https://push.example.test/device-3";

        var subscribeResponse = await _nexarrClient.SendAsync(Authorized(
            HttpMethod.Post,
            "/api/fieldcompanion/push/subscribe",
            session.AccessToken,
            new UpsertFieldCompanionPushSubscriptionRequest(
                endpoint,
                new FieldCompanionPushSubscriptionKeysRequest("p256dh-test-key-3", "auth-test-key-3"),
                null)));
        subscribeResponse.EnsureSuccessStatusCode();

        var unsubscribeRequest = Authorized(
            HttpMethod.Delete,
            "/api/fieldcompanion/push/subscribe",
            session.AccessToken);
        unsubscribeRequest.Content = JsonContent.Create(new UnsubscribeFieldCompanionPushRequest(endpoint));
        var unsubscribeResponse = await _nexarrClient.SendAsync(unsubscribeRequest);
        Assert.Equal(HttpStatusCode.NoContent, unsubscribeResponse.StatusCode);

        await using var scope = _nexarrFactory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        Assert.False(await db.FieldCompanionPushSubscriptions.AnyAsync(x =>
            x.TenantId == PlatformSeeder.DemoTenantId && x.UserId == session.UserId));
    }

    private async Task UpsertNotificationSettingsAsync(string? webhookUrl)
    {
        var session = await RedeemFieldCompanionSessionAsync();
        var request = Authorized(
            HttpMethod.Put,
            "/api/fieldcompanion/notification-settings",
            session.AccessToken);
        request.Content = JsonContent.Create(new UpsertFieldCompanionNotificationSettingsRequest(
            true,
            webhookUrl,
            true,
            true));
        (await _nexarrClient.SendAsync(request)).EnsureSuccessStatusCode();
    }

    private async Task<FieldCompanionSessionResponse> RedeemFieldCompanionSessionAsync()
    {
        var adminToken = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        var handoffResponse = await _nexarrClient.SendAsync(Authorized(
            HttpMethod.Post,
            "/api/v1/launch/handoff",
            adminToken,
            new CreateHandoffRequest("fieldcompanion", null)));
        handoffResponse.EnsureSuccessStatusCode();
        var handoff = (await handoffResponse.Content.ReadFromJsonAsync<HandoffCreatedResponse>())!;

        var redeemResponse = await _nexarrClient.PostAsJsonAsync(
            "/api/fieldcompanion/auth/handoff/redeem",
            new FieldCompanionRedeemHandoffRequest(handoff.HandoffCode));
        redeemResponse.EnsureSuccessStatusCode();
        return (await redeemResponse.Content.ReadFromJsonAsync<FieldCompanionSessionResponse>())!;
    }

    private async Task EnsureFieldCompanionEntitlementAsync()
    {
        await using var scope = _nexarrFactory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var exists = await db.Entitlements.AnyAsync(
            e => e.TenantId == PlatformSeeder.DemoTenantId && e.ProductKey == "fieldcompanion");
        if (exists)
        {
            return;
        }

        db.ProductCatalog.Add(new ProductCatalogItem
        {
            ProductKey = "fieldcompanion",
            DisplayName = "fieldcompanion App",
            SortOrder = 80,
            IsActive = true,
        });
        db.Entitlements.Add(new TenantProductEntitlement
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            ProductKey = "fieldcompanion",
            Status = EntitlementStatuses.Active,
            GrantedAt = DateTimeOffset.UtcNow,
        });
        db.LaunchProfiles.Add(new ProductLaunchProfile
        {
            ProductKey = "fieldcompanion",
            BaseUrl = "http://localhost:5181",
            LaunchPath = "/launch",
            IsActive = true,
            ModifiedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync();
    }

    private async Task SeedNexArrAsync()
    {
        await using var scope = _nexarrFactory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await PlatformSeeder.SeedAsync(db, passwordHasher);
    }

    private async Task<string> LoginNexArrAsync(string email)
    {
        var response = await _nexarrClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        var auth = (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!;
        return auth.AccessToken;
    }

    private async Task<string> IssueServiceTokenAsync(
        string adminToken,
        string sourceProduct,
        string[] allowedProducts,
        string actionScope)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{sourceProduct}-webpush-{Guid.NewGuid():N}",
            $"{sourceProduct} web push test",
            sourceProduct,
            allowedProducts));
        var registerResponse = await _nexarrClient.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/service-tokens", adminToken);
        issueRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            allowedProducts,
            actionScope,
            30));
        var issueResponse = await _nexarrClient.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        var issued = (await issueResponse.Content.ReadFromJsonAsync<ServiceTokenIssueResponse>())!;
        return issued.AccessToken;
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string path, string token, object? body = null)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        return request;
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

    private sealed record FieldCompanionWebPushSendCall(Guid SubscriptionId, string PayloadJson);

    private sealed class RecordingWebPushSender(List<FieldCompanionWebPushSendCall> calls) : IFieldCompanionWebPushSender
    {
        public Task<FieldCompanionWebPushSendResult> SendAsync(
            FieldCompanionPushSubscription subscription,
            string payloadJson,
            CancellationToken cancellationToken = default)
        {
            calls.Add(new FieldCompanionWebPushSendCall(subscription.Id, payloadJson));
            return Task.FromResult(new FieldCompanionWebPushSendResult(true, 201, null));
        }
    }
}
