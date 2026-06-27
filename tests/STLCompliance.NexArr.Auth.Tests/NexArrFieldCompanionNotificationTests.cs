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

namespace STLCompliance.NexArr.Auth.Tests;

public sealed class NexArrFieldCompanionNotificationTests : IAsyncLifetime
{
    private readonly List<HttpRequestMessage> _webhookRequests = [];
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private string _sharedWorkerToNexArrToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"NexArrFieldCompanionNotification-{Guid.NewGuid():N}";

        _nexarrFactory = new WebApplicationFactory<global::NexArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<NexArrDbContext>(services);
                services.AddDbContext<NexArrDbContext>(options => options.UseInMemoryDatabase(nexArrDbName));
                services.AddHttpClient(FieldCompanionNotificationDispatchService.WebhookHttpClientName)
                    .ConfigurePrimaryHttpMessageHandler(() => new WebhookCaptureHandler(_webhookRequests));
            });
        });

        _nexarrClient = _nexarrFactory.CreateClient();
        await SeedNexArrAsync();
        await EnsureFieldCompanionLaunchDestinationCompatibilityAsync();

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
    public async Task Handoff_redeem_enqueues_dispatch_and_worker_posts_webhook()
    {
        const string webhookUrl = "https://hooks.example.test/fieldcompanion-ops";
        await UpsertNotificationSettingsAsync(webhookUrl);

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

        await using (var scope = _nexarrFactory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
            var pending = await db.FieldCompanionNotificationDispatches.SingleAsync(x =>
                x.TenantId == PlatformSeeder.DemoTenantId
                && x.EventKind == FieldCompanionNotificationEventKinds.HandoffRedeemed);
            Assert.Equal(FieldCompanionNotificationDispatchStatuses.Pending, pending.DispatchStatus);
        }

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

        await using (var scope = _nexarrFactory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
            var dispatched = await db.FieldCompanionNotificationDispatches.SingleAsync(x =>
                x.TenantId == PlatformSeeder.DemoTenantId
                && x.EventKind == FieldCompanionNotificationEventKinds.HandoffRedeemed);
            Assert.Equal(FieldCompanionNotificationDispatchStatuses.Sent, dispatched.DispatchStatus);
        }

        Assert.NotEmpty(_webhookRequests);
    }

    [Fact]
    public async Task Field_inbox_refresh_enqueues_when_enabled()
    {
        const string webhookUrl = "https://hooks.example.test/fieldcompanion-inbox";
        await UpsertNotificationSettingsAsync(webhookUrl);

        var session = await RedeemFieldCompanionSessionAsync();
        var inboxResponse = await _nexarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/fieldcompanion/field-inbox", session.AccessToken));
        inboxResponse.EnsureSuccessStatusCode();

        await using var scope = _nexarrFactory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        Assert.True(await db.FieldCompanionNotificationDispatches.AnyAsync(x =>
            x.TenantId == PlatformSeeder.DemoTenantId
            && x.EventKind == FieldCompanionNotificationEventKinds.FieldInboxRefreshed
            && x.ActorUserId == session.UserId));
    }

    [Fact]
    public async Task Notification_settings_test_endpoint_dispatches_notification()
    {
        const string webhookUrl = "https://hooks.example.test/fieldcompanion-test";
        await UpsertNotificationSettingsAsync(webhookUrl);

        var session = await RedeemFieldCompanionSessionAsync();
        var webhookCountBefore = _webhookRequests.Count;
        var request = Authorized(
            HttpMethod.Post,
            "/api/fieldcompanion/notification-settings/test",
            session.AccessToken);
        var response = await _nexarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var dispatch = (await response.Content.ReadFromJsonAsync<FieldCompanionNotificationDispatchItem>())!;
        Assert.Equal(FieldCompanionNotificationEventKinds.TestNotification, dispatch.EventKind);
        Assert.Equal(FieldCompanionNotificationDispatchStatuses.Sent, dispatch.DispatchStatus);
        Assert.Equal("fieldcompanion_notification_test", dispatch.RelatedEntityType);
        Assert.Equal("hooks.example.test", dispatch.WebhookHost);
        Assert.Equal(200, dispatch.HttpStatusCode);
        Assert.NotNull(dispatch.DispatchedAt);
        Assert.NotNull(dispatch.PushDeliveredCount);
        Assert.True(_webhookRequests.Count > webhookCountBefore);
    }

    [Fact]
    public async Task Process_batch_rejects_missing_service_token()
    {
        var response = await _nexarrClient.PostAsJsonAsync(
            "/api/internal/fieldcompanion-notifications/process-batch",
            new ProcessFieldCompanionNotificationsRequest(PlatformSeeder.DemoTenantId, null, 10));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task UpsertNotificationSettingsAsync(string webhookUrl)
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

    private async Task EnsureFieldCompanionLaunchDestinationCompatibilityAsync()
    {
        await using var scope = _nexarrFactory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        if (!db.ProductCatalog.Local.Any(x => x.ProductKey == "fieldcompanion")
            && !await db.ProductCatalog.AnyAsync(x => x.ProductKey == "fieldcompanion"))
        {
            db.ProductCatalog.Add(new ProductCatalogItem
            {
                ProductKey = "fieldcompanion",
                DisplayName = "fieldcompanion App",
                SortOrder = 80,
                IsActive = true,
            });
        }

        if (!db.Entitlements.Local.Any(x => x.TenantId == PlatformSeeder.DemoTenantId && x.ProductKey == "fieldcompanion")
            && !await db.Entitlements.AnyAsync(x => x.TenantId == PlatformSeeder.DemoTenantId && x.ProductKey == "fieldcompanion"))
        {
            db.Entitlements.Add(new TenantProductEntitlement
            {
                Id = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                ProductKey = "fieldcompanion",
                Status = EntitlementStatuses.Active,
                GrantedAt = DateTimeOffset.UtcNow,
            });
        }

        if (!db.LaunchProfiles.Local.Any(x => x.ProductKey == "fieldcompanion")
            && !await db.LaunchProfiles.AnyAsync(x => x.ProductKey == "fieldcompanion"))
        {
            db.LaunchProfiles.Add(new ProductLaunchProfile
            {
                ProductKey = "fieldcompanion",
                BaseUrl = "http://localhost:5181",
                LaunchPath = "/launch",
                IsActive = true,
                ModifiedAt = DateTimeOffset.UtcNow,
            });
        }
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
            $"{sourceProduct}-notifications-{Guid.NewGuid():N}",
            $"{sourceProduct} notification test",
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

    private sealed class WebhookCaptureHandler(List<HttpRequestMessage> captured) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            captured.Add(request);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
