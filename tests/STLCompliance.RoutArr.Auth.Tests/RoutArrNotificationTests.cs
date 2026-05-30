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
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using RoutArr.Api.Services;

namespace STLCompliance.RoutArr.Auth.Tests;

public sealed class RoutArrNotificationTests : IAsyncLifetime
{
    private readonly List<HttpRequestMessage> _webhookRequests = [];
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::RoutArr.Api.Program> _routarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _routarrClient = null!;
    private string _sharedWorkerToRoutarrToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"RoutArrNotificationNexArr-{Guid.NewGuid():N}";
        var routArrDbName = $"RoutArrNotificationRoutArr-{Guid.NewGuid():N}";

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
            });
        });

        _nexarrClient = _nexarrFactory.CreateClient();
        await SeedNexArrAsync();

        var adminToken = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        _sharedWorkerToRoutarrToken = await IssueServiceTokenAsync(
            adminToken,
            "shared-worker",
            ["routarr"],
            DispatchNotificationDispatchService.ProcessNotificationsActionScope);

        _routarrFactory = new WebApplicationFactory<global::RoutArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<RoutArrDbContext>(services);
                services.AddDbContext<RoutArrDbContext>(options => options.UseInMemoryDatabase(routArrDbName));
                services.AddHttpClient(DispatchNotificationDispatchService.WebhookHttpClientName)
                    .ConfigurePrimaryHttpMessageHandler(() => new WebhookCaptureHandler(_webhookRequests));
            });
        });

        _routarrClient = _routarrFactory.CreateClient();

        using var scope = _routarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RoutArrDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        _routarrClient.Dispose();
        _nexarrClient.Dispose();
        await _routarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Trip_assign_enqueues_dispatch_and_worker_posts_webhook()
    {
        const string webhookUrl = "https://hooks.example.test/routarr-dispatch";
        await UpsertNotificationSettingsAsync(webhookUrl);

        var adminToken = CreateRoutArrAccessToken(["routarr"], "routarr_admin");
        var driverPersonId = Guid.NewGuid().ToString();

        var createRequest = Authorized(HttpMethod.Post, "/api/trips", adminToken);
        createRequest.Content = JsonContent.Create(new CreateTripRequest(
            "Webhook test trip",
            string.Empty,
            null,
            null,
            null,
            null));
        var createResponse = await _routarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;

        var assignRequest = Authorized(HttpMethod.Patch, $"/api/trips/{created.TripId}/assign-driver", adminToken);
        assignRequest.Content = JsonContent.Create(new AssignTripDriverRequest(
            driverPersonId,
            IgnoreAvailabilityConflicts: false,
            IgnoreEligibilityBlocks: false,
            IgnoreWorkflowGateBlocks: false));
        var assignResponse = await _routarrClient.SendAsync(assignRequest);
        assignResponse.EnsureSuccessStatusCode();

        using (var scope = _routarrFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<RoutArrDbContext>();
            var pending = await db.DispatchNotificationDispatches.SingleAsync(x =>
                x.TenantId == PlatformSeeder.DemoTenantId
                && x.EventKind == DispatchNotificationEventKinds.TripAssigned
                && x.TripId == created.TripId);
            Assert.Equal(DispatchNotificationDispatchStatuses.Pending, pending.DispatchStatus);
        }

        var processRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/dispatch-notifications/process-batch",
            _sharedWorkerToRoutarrToken);
        processRequest.Content = JsonContent.Create(new ProcessDispatchNotificationsRequest(
            PlatformSeeder.DemoTenantId,
            null,
            10));
        var processResponse = await _routarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();

        Assert.Single(_webhookRequests);
        Assert.Equal(webhookUrl, _webhookRequests[0].RequestUri?.ToString());

        using var verifyScope = _routarrFactory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<RoutArrDbContext>();
        var dispatched = await verifyDb.DispatchNotificationDispatches.SingleAsync(x => x.TripId == created.TripId);
        Assert.Equal(DispatchNotificationDispatchStatuses.Sent, dispatched.DispatchStatus);
        Assert.Equal(200, dispatched.HttpStatusCode);
    }

    [Fact]
    public async Task Notification_settings_put_rejects_invalid_webhook()
    {
        var token = CreateRoutArrAccessToken(["routarr"], "routarr_admin");
        var request = Authorized(HttpMethod.Put, "/api/notification-settings", token);
        request.Content = JsonContent.Create(new UpsertDispatchNotificationSettingsRequest(
            true,
            "not-a-url",
            true,
            true,
            true,
            true,
            true));

        var response = await _routarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Notification_settings_put_rejects_enabled_without_webhook()
    {
        var token = CreateRoutArrAccessToken(["routarr"], "routarr_admin");
        var request = Authorized(HttpMethod.Put, "/api/notification-settings", token);
        request.Content = JsonContent.Create(new UpsertDispatchNotificationSettingsRequest(
            true,
            null,
            true,
            true,
            true,
            true,
            true));

        var response = await _routarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Notification_settings_disable_preserves_webhook_url()
    {
        const string webhookUrl = "https://hooks.example.test/routarr-disable-preserve";
        await UpsertNotificationSettingsAsync(webhookUrl);

        var token = CreateRoutArrAccessToken(["routarr"], "routarr_admin");
        var request = Authorized(HttpMethod.Put, "/api/notification-settings", token);
        request.Content = JsonContent.Create(new UpsertDispatchNotificationSettingsRequest(
            false,
            null,
            true,
            true,
            true,
            true,
            true));

        var response = await _routarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var payload = (await response.Content.ReadFromJsonAsync<DispatchNotificationSettingsResponse>())!;

        Assert.False(payload.IsEnabled);
        Assert.Equal(webhookUrl, payload.NotificationWebhookUrl);
    }

    [Fact]
    public async Task Notification_settings_disable_explicit_clear_webhook_url()
    {
        const string webhookUrl = "https://hooks.example.test/routarr-disable-explicit-clear";
        await UpsertNotificationSettingsAsync(webhookUrl);

        var token = CreateRoutArrAccessToken(["routarr"], "routarr_admin");
        var request = Authorized(HttpMethod.Put, "/api/notification-settings", token);
        request.Content = JsonContent.Create(new UpsertDispatchNotificationSettingsRequest(
            false,
            null,
            true,
            true,
            true,
            true,
            true,
            ClearNotificationWebhookOnDisable: true));

        var response = await _routarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var payload = (await response.Content.ReadFromJsonAsync<DispatchNotificationSettingsResponse>())!;

        Assert.False(payload.IsEnabled);
        Assert.Null(payload.NotificationWebhookUrl);
    }

    [Fact]
    public async Task Notification_settings_requires_admin()
    {
        var dispatcherToken = CreateRoutArrAccessToken(["routarr"], "routarr_dispatcher");
        var response = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/notification-settings", dispatcherToken));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Settings_manifest_v1_requires_admin_and_lists_setting_groups()
    {
        var dispatcherToken = CreateRoutArrAccessToken(["routarr"], "routarr_dispatcher");
        var forbiddenResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/settings", dispatcherToken));
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenResponse.StatusCode);

        var adminToken = CreateRoutArrAccessToken(["routarr"], "routarr_admin");
        var manifestResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/settings", adminToken));
        manifestResponse.EnsureSuccessStatusCode();
        var manifest = (await manifestResponse.Content.ReadFromJsonAsync<RoutArrSettingsManifestResponse>())!;
        Assert.Contains(manifest.Items, x => x.SettingKey == "notification_settings");
        Assert.Contains(manifest.Items, x => x.SettingKey == "integration_event_settings");
        Assert.Contains(manifest.Items, x => x.SettingKey == "trip_completion_rollup_settings");
    }

    [Fact]
    public async Task Config_manifest_v1_requires_admin_and_matches_settings_manifest()
    {
        var dispatcherToken = CreateRoutArrAccessToken(["routarr"], "routarr_dispatcher");
        var forbiddenResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/config", dispatcherToken));
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenResponse.StatusCode);

        var adminToken = CreateRoutArrAccessToken(["routarr"], "routarr_admin");
        var configResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/config", adminToken));
        configResponse.EnsureSuccessStatusCode();
        var configManifest = (await configResponse.Content.ReadFromJsonAsync<RoutArrSettingsManifestResponse>())!;

        var settingsResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/settings", adminToken));
        settingsResponse.EnsureSuccessStatusCode();
        var settingsManifest = (await settingsResponse.Content.ReadFromJsonAsync<RoutArrSettingsManifestResponse>())!;

        Assert.Equal(settingsManifest.Items.Count, configManifest.Items.Count);
        foreach (var item in settingsManifest.Items)
        {
            Assert.Contains(configManifest.Items, x => x.SettingKey == item.SettingKey);
        }
    }

    [Fact]
    public async Task Process_batch_rejects_missing_service_token()
    {
        var response = await _routarrClient.PostAsJsonAsync(
            "/api/internal/dispatch-notifications/process-batch",
            new ProcessDispatchNotificationsRequest(PlatformSeeder.DemoTenantId, null, 10));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task UpsertNotificationSettingsAsync(string webhookUrl)
    {
        var token = CreateRoutArrAccessToken(["routarr"], "routarr_admin");
        var request = Authorized(HttpMethod.Put, "/api/notification-settings", token);
        request.Content = JsonContent.Create(new UpsertDispatchNotificationSettingsRequest(
            true,
            webhookUrl,
            true,
            true,
            true,
            true,
            true));
        var response = await _routarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    private string CreateRoutArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_member",
        Guid? personId = null)
    {
        using var scope = _routarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<RoutArrTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            personId ?? PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "Test Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements,
            isPlatformAdmin: false);
        return accessToken;
    }

    private async Task<string> LoginNexArrAsync(string email)
    {
        var response = await _nexarrClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        var payload = (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!;
        return payload.AccessToken;
    }

    private async Task<string> IssueServiceTokenAsync(
        string adminToken,
        string sourceProduct,
        IReadOnlyList<string> allowedProducts,
        string actionScope)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{sourceProduct}-notification-{Guid.NewGuid():N}",
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

    private async Task SeedNexArrAsync()
    {
        using var scope = _nexarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await PlatformSeeder.SeedAsync(db, hasher);
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }

    private static void RemoveDbContext<TContext>(IServiceCollection services)
        where TContext : DbContext
    {
        var descriptors = services.Where(d =>
            d.ServiceType == typeof(DbContextOptions<TContext>)
            || d.ServiceType == typeof(TContext)).ToList();
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
