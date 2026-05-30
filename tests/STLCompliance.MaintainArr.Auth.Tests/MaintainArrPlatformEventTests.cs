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

public sealed class MaintainArrPlatformEventTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::MaintainArr.Api.Program> _maintainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _maintainarrClient = null!;
    private string _sharedWorkerRollupToken = null!;
    private string _sharedWorkerPlatformEventToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"PlatformEventNexArr-{Guid.NewGuid():N}";
        var maintainArrDbName = $"PlatformEventMaintainArr-{Guid.NewGuid():N}";

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
        _sharedWorkerRollupToken = await IssueServiceTokenAsync(
            adminToken,
            "shared-worker",
            ["maintainarr"],
            AssetStatusRollupWorkerService.ProcessAssetStatusRollupsActionScope);
        _sharedWorkerPlatformEventToken = await IssueServiceTokenAsync(
            adminToken,
            "shared-worker",
            ["maintainarr"],
            MaintenancePlatformEventProcessingService.ProcessEventsActionScope);

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
            });
        });

        _maintainarrClient = _maintainarrFactory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _maintainarrClient.Dispose();
        _nexarrClient.Dispose();
        await _maintainarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Process_batch_rejects_missing_service_token()
    {
        var response = await _maintainarrClient.PostAsJsonAsync(
            "/api/internal/platform-events/process-batch",
            new ProcessMaintenancePlatformEventsRequest(PlatformSeeder.DemoTenantId));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Rollup_refresh_enqueues_and_processes_asset_readiness_changed()
    {
        var assetId = await SeedAssetWithCriticalDefectAsync();
        await UpsertRollupSettingsAsync();
        await UpsertPlatformEventSettingsAsync();

        var rollupRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/asset-status-rollups/process-batch",
            _sharedWorkerRollupToken);
        rollupRequest.Content = JsonContent.Create(new ProcessAssetStatusRollupsRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            25,
            1));
        var rollupResponse = await _maintainarrClient.SendAsync(rollupRequest);
        rollupResponse.EnsureSuccessStatusCode();

        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        var outbox = await db.MaintenancePlatformOutboxEvents
            .Where(x => x.TenantId == PlatformSeeder.DemoTenantId && x.RelatedEntityId == assetId)
            .ToListAsync();

        Assert.Contains(outbox, x => x.EventKind == MaintenancePlatformOutboxEventKinds.AssetReadinessChanged);
        Assert.All(outbox, x => Assert.Equal(MaintenancePlatformEventStatuses.Processed, x.ProcessingStatus));

        var duplicateRollupRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/asset-status-rollups/process-batch",
            _sharedWorkerRollupToken);
        duplicateRollupRequest.Content = JsonContent.Create(new ProcessAssetStatusRollupsRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            25,
            1));
        var duplicateRollupResponse = await _maintainarrClient.SendAsync(duplicateRollupRequest);
        duplicateRollupResponse.EnsureSuccessStatusCode();
        var outboxCount = await db.MaintenancePlatformOutboxEvents
            .CountAsync(x => x.TenantId == PlatformSeeder.DemoTenantId && x.RelatedEntityId == assetId);
        Assert.Equal(outbox.Count, outboxCount);
    }

    [Fact]
    public async Task Platform_event_settings_v1_put_get_outbox_and_runs_work_for_admin()
    {
        await SeedAssetWithCriticalDefectAsync();
        var token = CreateMaintainArrAccessToken(["maintainarr"], "maintainarr_admin");

        var putRequest = Authorized(HttpMethod.Put, "/api/v1/platform-event-settings", token);
        putRequest.Content = JsonContent.Create(new UpsertMaintenancePlatformEventSettingsRequest(true));
        (await _maintainarrClient.SendAsync(putRequest)).EnsureSuccessStatusCode();

        var getRequest = Authorized(HttpMethod.Get, "/api/v1/platform-event-settings", token);
        var getResponse = await _maintainarrClient.SendAsync(getRequest);
        getResponse.EnsureSuccessStatusCode();
        var settings = (await getResponse.Content.ReadFromJsonAsync<MaintenancePlatformEventSettingsResponse>())!;
        Assert.True(settings.IsEnabled);

        var outboxRequest = Authorized(HttpMethod.Get, "/api/v1/platform-event-settings/outbox?limit=5", token);
        var outboxResponse = await _maintainarrClient.SendAsync(outboxRequest);
        outboxResponse.EnsureSuccessStatusCode();
        var outbox = (await outboxResponse.Content.ReadFromJsonAsync<MaintenancePlatformOutboxEventsResponse>())!;
        Assert.NotNull(outbox.Items);

        var runsRequest = Authorized(HttpMethod.Get, "/api/v1/platform-event-settings/runs?limit=5", token);
        var runsResponse = await _maintainarrClient.SendAsync(runsRequest);
        runsResponse.EnsureSuccessStatusCode();
        var runs = (await runsResponse.Content.ReadFromJsonAsync<MaintenancePlatformEventProcessingRunsResponse>())!;
        Assert.NotNull(runs.Items);
    }

    [Fact]
    public async Task Events_v1_alias_matches_platform_event_outbox_endpoint()
    {
        await SeedAssetWithCriticalDefectAsync();
        var token = CreateMaintainArrAccessToken(["maintainarr"], "maintainarr_admin");

        var putRequest = Authorized(HttpMethod.Put, "/api/v1/platform-event-settings", token);
        putRequest.Content = JsonContent.Create(new UpsertMaintenancePlatformEventSettingsRequest(true));
        (await _maintainarrClient.SendAsync(putRequest)).EnsureSuccessStatusCode();

        var outboxResponse = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/platform-event-settings/outbox?limit=5", token));
        outboxResponse.EnsureSuccessStatusCode();
        var outbox = (await outboxResponse.Content.ReadFromJsonAsync<MaintenancePlatformOutboxEventsResponse>())!;

        var eventsResponse = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/events?limit=5", token));
        eventsResponse.EnsureSuccessStatusCode();
        var eventsAlias = (await eventsResponse.Content.ReadFromJsonAsync<MaintenancePlatformOutboxEventsResponse>())!;

        Assert.Equal(outbox.Items.Count, eventsAlias.Items.Count);
    }

    private async Task UpsertRollupSettingsAsync()
    {
        var token = CreateMaintainArrAccessToken(["maintainarr"], "maintainarr_admin");
        var request = Authorized(HttpMethod.Put, "/api/asset-status-rollup-settings", token);
        request.Content = JsonContent.Create(new UpsertAssetStatusRollupSettingsRequest(true, 1));
        var response = await _maintainarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    private async Task UpsertPlatformEventSettingsAsync()
    {
        var token = CreateMaintainArrAccessToken(["maintainarr"], "maintainarr_admin");
        var request = Authorized(HttpMethod.Put, "/api/platform-event-settings", token);
        request.Content = JsonContent.Create(new UpsertMaintenancePlatformEventSettingsRequest(true));
        var response = await _maintainarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    private async Task<Guid> SeedAssetWithCriticalDefectAsync()
    {
        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        var now = DateTimeOffset.UtcNow;

        var assetClass = new AssetClass
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            ClassKey = "platform-event-class",
            Name = "Platform Event Class",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };
        var assetType = new AssetType
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            AssetClassId = assetClass.Id,
            TypeKey = "platform-event-type",
            Name = "Platform Event Type",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };
        var asset = new Asset
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            AssetTypeId = assetType.Id,
            AssetTag = "PE-001",
            Name = "Platform Event Asset",
            SiteRef = "Plant-A",
            LifecycleStatus = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };
        var defect = new Defect
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            AssetId = asset.Id,
            Title = "Critical brake failure",
            Description = "Blocks readiness",
            Severity = DefectSeverities.Critical,
            Status = DefectStatuses.Open,
            Source = DefectSources.Manual,
            ReportedByUserId = PlatformSeeder.DemoAdminUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.AssetClasses.Add(assetClass);
        db.AssetTypes.Add(assetType);
        db.Assets.Add(asset);
        db.Defects.Add(defect);
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

    private static HttpRequestMessage Authorized(HttpMethod method, string path, string token)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
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
        IReadOnlyList<string> allowedProducts,
        string actionScope)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{sourceProduct}-platform-events-{Guid.NewGuid():N}",
            $"{sourceProduct} platform events test",
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
}
