using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Services;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using SupplyArr.Api.Services;

namespace STLCompliance.SupplyArr.Auth.Tests;

public sealed class SupplyArrNotificationTests : IAsyncLifetime
{
    private readonly List<HttpRequestMessage> _webhookRequests = [];
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::SupplyArr.Api.Program> _supplyarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _supplyarrClient = null!;
    private string _sharedWorkerToSupplyArrToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"SupplyArrNotificationNexArr-{Guid.NewGuid():N}";
        var supplyArrDbName = $"SupplyArrNotificationSupplyArr-{Guid.NewGuid():N}";

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
        _sharedWorkerToSupplyArrToken = await IssueServiceTokenAsync(
            adminToken,
            "shared-worker",
            ["supplyarr"],
            ProcurementNotificationDispatchService.ProcessNotificationsActionScope);

        _supplyarrFactory = new WebApplicationFactory<global::SupplyArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<SupplyArrDbContext>(services);
                services.AddDbContext<SupplyArrDbContext>(options => options.UseInMemoryDatabase(supplyArrDbName));
                services.AddHttpClient(ProcurementNotificationDispatchService.WebhookHttpClientName)
                    .ConfigurePrimaryHttpMessageHandler(() => new WebhookCaptureHandler(_webhookRequests));
            });
        });

        _supplyarrClient = _supplyarrFactory.CreateClient();

        using var scope = _supplyarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        _supplyarrClient.Dispose();
        _nexarrClient.Dispose();
        await _supplyarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Purchase_request_submit_enqueues_dispatch_and_worker_posts_webhook()
    {
        const string webhookUrl = "https://hooks.example.test/supplyarr-procurement";
        await UpsertNotificationSettingsAsync(webhookUrl);

        var token = CreateSupplyArrAccessToken(["supplyarr"], "supplyarr_admin");

        var createSupplierRequest = Authorized(HttpMethod.Post, "/api/suppliers", token);
        createSupplierRequest.Content = JsonContent.Create(new CreateSupplierRequest(
            $"notify-supplier-{Guid.NewGuid():N}"[..20],
            null,
            null,
            "Notify Supplier",
            "Notify Supplier LLC",
            null,
            string.Empty,
            ["parts"],
            null,
            null,
            null,
            null,
            null,
            null));
        var supplier = (await (await _supplyarrClient.SendAsync(createSupplierRequest)).Content
            .ReadFromJsonAsync<SupplierResponse>())!;

        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", token);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            $"notify-part-{Guid.NewGuid():N}"[..20],
            null,
            "Notify Part",
            string.Empty,
            "general",
            "each",
            string.Empty,
            string.Empty));
        var part = (await (await _supplyarrClient.SendAsync(createPartRequest)).Content
            .ReadFromJsonAsync<PartResponse>())!;

        var createPrRequest = Authorized(HttpMethod.Post, "/api/purchase-requests", token);
        createPrRequest.Content = JsonContent.Create(new CreatePurchaseRequestRequest(
            $"notify-pr-{Guid.NewGuid():N}"[..20],
            "Notify PR",
            "Webhook test",
            supplier.SupplierId,
            [new CreatePurchaseRequestLineRequest(part.PartId, 2m, "line")]));
        var purchaseRequest = (await (await _supplyarrClient.SendAsync(createPrRequest)).Content
            .ReadFromJsonAsync<PurchaseRequestResponse>())!;

        var submitRequest = Authorized(
            HttpMethod.Post,
            $"/api/purchase-requests/{purchaseRequest.PurchaseRequestId}/submit",
            token);
        (await _supplyarrClient.SendAsync(submitRequest)).EnsureSuccessStatusCode();

        using (var scope = _supplyarrFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
            var pending = await db.ProcurementNotificationDispatches.SingleAsync(x =>
                x.TenantId == PlatformSeeder.DemoTenantId
                && x.EventKind == ProcurementNotificationEventKinds.PurchaseRequestSubmitted
                && x.RelatedEntityId == purchaseRequest.PurchaseRequestId);
            Assert.Equal(ProcurementNotificationDispatchStatuses.Pending, pending.DispatchStatus);
        }

        var processRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/procurement-notifications/process-batch",
            _sharedWorkerToSupplyArrToken);
        processRequest.Content = JsonContent.Create(new ProcessProcurementNotificationsRequest(
            PlatformSeeder.DemoTenantId,
            null,
            10));
        (await _supplyarrClient.SendAsync(processRequest)).EnsureSuccessStatusCode();

        Assert.Single(_webhookRequests);
        Assert.Equal(webhookUrl, _webhookRequests[0].RequestUri?.ToString());
        var payload = JsonDocument.Parse(await _webhookRequests[0].Content!.ReadAsStringAsync());
        Assert.Equal("supplyarr.purchase_request.submitted", payload.RootElement.GetProperty("event").GetString());
        Assert.Equal(supplier.SupplierId, payload.RootElement.GetProperty("supplierId").GetGuid());
        Assert.False(payload.RootElement.TryGetProperty("vendorPartyId", out _));

        using var verifyScope = _supplyarrFactory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var dispatched = await verifyDb.ProcurementNotificationDispatches.SingleAsync(x =>
            x.RelatedEntityId == purchaseRequest.PurchaseRequestId);
        Assert.Equal(ProcurementNotificationDispatchStatuses.Sent, dispatched.DispatchStatus);
        Assert.Equal(200, dispatched.HttpStatusCode);
    }

    [Fact]
    public async Task Notification_settings_requires_admin()
    {
        var buyerToken = CreateSupplyArrAccessToken(["supplyarr"], "supplyarr_buyer");
        var response = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/notification-settings", buyerToken));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Settings_manifest_v1_requires_admin_and_lists_setting_groups()
    {
        var buyerToken = CreateSupplyArrAccessToken(["supplyarr"], "supplyarr_buyer");
        var forbiddenResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/settings", buyerToken));
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenResponse.StatusCode);

        var adminToken = CreateSupplyArrAccessToken(["supplyarr"], "supplyarr_admin");
        var manifestResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/settings", adminToken));
        manifestResponse.EnsureSuccessStatusCode();
        var manifest = (await manifestResponse.Content.ReadFromJsonAsync<SupplyArrSettingsManifestResponse>())!;
        Assert.Contains(manifest.Items, x => x.SettingKey == "notification_settings");
        Assert.Contains(manifest.Items, x => x.SettingKey == "demand_processing_settings");
        Assert.Contains(manifest.Items, x => x.SettingKey == "integration_event_settings");
    }

    [Fact]
    public async Task Config_manifest_v1_requires_admin_and_matches_settings_manifest()
    {
        var buyerToken = CreateSupplyArrAccessToken(["supplyarr"], "supplyarr_buyer");
        var forbiddenResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/config", buyerToken));
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenResponse.StatusCode);

        var adminToken = CreateSupplyArrAccessToken(["supplyarr"], "supplyarr_admin");
        var configResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/config", adminToken));
        configResponse.EnsureSuccessStatusCode();
        var configManifest = (await configResponse.Content.ReadFromJsonAsync<SupplyArrSettingsManifestResponse>())!;

        var settingsResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/settings", adminToken));
        settingsResponse.EnsureSuccessStatusCode();
        var settingsManifest = (await settingsResponse.Content.ReadFromJsonAsync<SupplyArrSettingsManifestResponse>())!;

        Assert.Equal(settingsManifest.Items.Count, configManifest.Items.Count);
        foreach (var item in settingsManifest.Items)
        {
            Assert.Contains(configManifest.Items, x => x.SettingKey == item.SettingKey);
        }
    }

    [Fact]
    public async Task Process_batch_rejects_missing_service_token()
    {
        var response = await _supplyarrClient.PostAsJsonAsync(
            "/api/internal/procurement-notifications/process-batch",
            new ProcessProcurementNotificationsRequest(PlatformSeeder.DemoTenantId, null, 10));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task UpsertNotificationSettingsAsync(string webhookUrl)
    {
        var token = CreateSupplyArrAccessToken(["supplyarr"], "supplyarr_admin");
        var request = Authorized(HttpMethod.Put, "/api/notification-settings", token);
        request.Content = JsonContent.Create(new UpsertProcurementNotificationSettingsRequest(
            true,
            webhookUrl,
            true,
            true,
            true,
            true));
        (await _supplyarrClient.SendAsync(request)).EnsureSuccessStatusCode();
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

    private string CreateSupplyArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_admin")
    {
        using var scope = _supplyarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<SupplyArrTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "Test Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements,
            isPlatformAdmin: false);
        return accessToken;
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string path, string token)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
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
