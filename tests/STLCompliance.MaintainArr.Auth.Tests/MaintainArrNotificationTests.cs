using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using MaintainArr.Api.Services;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Services;

namespace STLCompliance.MaintainArr.Auth.Tests;

public sealed class MaintainArrNotificationTests : IAsyncLifetime
{
    private readonly List<HttpRequestMessage> _webhookRequests = [];
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::MaintainArr.Api.Program> _maintainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _maintainarrClient = null!;
    private string _sharedWorkerToMaintainArrToken = null!;
    private Guid _assetId;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"MaintainArrNotificationNexArr-{Guid.NewGuid():N}";
        var maintainArrDbName = $"MaintainArrNotificationMaintainArr-{Guid.NewGuid():N}";

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
        _sharedWorkerToMaintainArrToken = await IssueServiceTokenAsync(
            adminToken,
            "shared-worker",
            ["maintainarr"],
            MaintenanceNotificationDispatchService.ProcessNotificationsActionScope);

        _maintainarrFactory = new WebApplicationFactory<global::MaintainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<MaintainArrDbContext>(services);
                services.AddDbContext<MaintainArrDbContext>(options => options.UseInMemoryDatabase(maintainArrDbName));
                services.AddHttpClient(MaintenanceNotificationDispatchService.WebhookHttpClientName)
                    .ConfigurePrimaryHttpMessageHandler(() => new WebhookCaptureHandler(_webhookRequests));
            });
        });

        _maintainarrClient = _maintainarrFactory.CreateClient();

        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        await db.Database.EnsureCreatedAsync();
        _assetId = await SeedAssetAsync(db);
    }

    public async Task DisposeAsync()
    {
        _maintainarrClient.Dispose();
        _nexarrClient.Dispose();
        await _maintainarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Work_order_create_enqueues_dispatch_and_worker_posts_webhook()
    {
        const string webhookUrl = "https://hooks.example.test/maintainarr-work-orders";
        await UpsertNotificationSettingsAsync(webhookUrl);

        var adminToken = CreateMaintainArrAccessToken(["maintainarr"], "maintainarr_admin");
        var createRequest = Authorized(HttpMethod.Post, "/api/work-orders", adminToken);
        createRequest.Content = JsonContent.Create(new CreateWorkOrderRequest(
            _assetId,
            "Notify WO",
            "Notification test work order",
            "medium",
            null,
            null));
        var createResponse = await _maintainarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var workOrder = (await createResponse.Content.ReadFromJsonAsync<WorkOrderDetailResponse>())!;

        using (var scope = _maintainarrFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
            var pending = await db.MaintenanceNotificationDispatches.SingleAsync(x =>
                x.TenantId == PlatformSeeder.DemoTenantId
                && x.EventKind == MaintenanceNotificationEventKinds.WorkOrderCreated
                && x.RelatedEntityId == workOrder.WorkOrderId);
            Assert.Equal(MaintenanceNotificationDispatchStatuses.Pending, pending.DispatchStatus);
        }

        var processRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/maintenance-notifications/process-batch",
            _sharedWorkerToMaintainArrToken);
        processRequest.Content = JsonContent.Create(new ProcessMaintenanceNotificationsRequest(
            PlatformSeeder.DemoTenantId,
            null,
            10));
        var processResponse = await _maintainarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();

        Assert.Single(_webhookRequests);
        Assert.Equal(webhookUrl, _webhookRequests[0].RequestUri?.ToString());

        using var verifyScope = _maintainarrFactory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        var dispatched = await verifyDb.MaintenanceNotificationDispatches.SingleAsync(x =>
            x.RelatedEntityId == workOrder.WorkOrderId);
        Assert.Equal(MaintenanceNotificationDispatchStatuses.Sent, dispatched.DispatchStatus);
        Assert.Equal(200, dispatched.HttpStatusCode);
    }

    [Fact]
    public async Task Notification_settings_put_rejects_invalid_webhook()
    {
        var token = CreateMaintainArrAccessToken(["maintainarr"], "maintainarr_admin");
        var request = Authorized(HttpMethod.Put, "/api/notification-settings", token);
        request.Content = JsonContent.Create(new UpsertMaintenanceNotificationSettingsRequest(
            true,
            "not-a-url",
            true,
            true,
            true));

        var response = await _maintainarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Notification_settings_requires_admin()
    {
        var memberToken = CreateMaintainArrAccessToken(["maintainarr"], "maintainarr_manager");
        var response = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/notification-settings", memberToken));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Process_batch_rejects_missing_service_token()
    {
        var response = await _maintainarrClient.PostAsJsonAsync(
            "/api/internal/maintenance-notifications/process-batch",
            new ProcessMaintenanceNotificationsRequest(PlatformSeeder.DemoTenantId, null, 10));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task UpsertNotificationSettingsAsync(string webhookUrl)
    {
        var token = CreateMaintainArrAccessToken(["maintainarr"], "maintainarr_admin");
        var request = Authorized(HttpMethod.Put, "/api/notification-settings", token);
        request.Content = JsonContent.Create(new UpsertMaintenanceNotificationSettingsRequest(
            true,
            webhookUrl,
            true,
            true,
            true));
        var response = await _maintainarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    private static async Task<Guid> SeedAssetAsync(MaintainArrDbContext db)
    {
        var now = DateTimeOffset.UtcNow;
        var assetClass = new AssetClass
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            ClassKey = "notify-class",
            Name = "Notify Class",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };
        var assetType = new AssetType
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            AssetClassId = assetClass.Id,
            TypeKey = "notify-type",
            Name = "Notify Type",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };
        var asset = new Asset
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            AssetTypeId = assetType.Id,
            AssetTag = "NOTIFY-001",
            Name = "Notify Asset",
            LifecycleStatus = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };
        db.AssetClasses.Add(assetClass);
        db.AssetTypes.Add(assetType);
        db.Assets.Add(asset);
        await db.SaveChangesAsync();
        return asset.Id;
    }

    private string CreateMaintainArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_admin")
    {
        using var scope = _maintainarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<MaintainArrTokenService>();
        var (token, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "Demo Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements,
            isPlatformAdmin: false);
        return token;
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
