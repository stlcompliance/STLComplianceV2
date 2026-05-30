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

public sealed class MaintainArrAssetStatusRollupWorkerTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::MaintainArr.Api.Program> _maintainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _maintainarrClient = null!;
    private string _sharedWorkerToMaintainArrToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"AssetStatusRollupNexArr-{Guid.NewGuid():N}";
        var maintainArrDbName = $"AssetStatusRollupMaintainArr-{Guid.NewGuid():N}";

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
            AssetStatusRollupWorkerService.ProcessAssetStatusRollupsActionScope);

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
            "/api/internal/asset-status-rollups/process-batch",
            new ProcessAssetStatusRollupsRequest(PlatformSeeder.DemoTenantId, DateTimeOffset.UtcNow, 25, 1));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Process_batch_refreshes_asset_and_fleet_rollups()
    {
        var assetId = await SeedAssetWithCriticalDefectAsync();
        await UpsertRollupSettingsAsync();

        var processRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/asset-status-rollups/process-batch",
            _sharedWorkerToMaintainArrToken);
        processRequest.Content = JsonContent.Create(new ProcessAssetStatusRollupsRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            25,
            1));
        var processResponse = await _maintainarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var body = (await processResponse.Content.ReadFromJsonAsync<ProcessAssetStatusRollupsResponse>())!;
        Assert.Equal(1, body.RefreshedCount);
        Assert.Contains(body.Refreshed, x => x.AssetId == assetId && x.ReadinessStatus == "not_ready");

        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        var rollup = await db.AssetStatusRollups.SingleAsync(x => x.AssetId == assetId);
        Assert.Equal("not_ready", rollup.ReadinessStatus);
        Assert.True(rollup.BlockerCount > 0);

        var fleetRollup = await db.AssetStatusScopeRollups.SingleAsync(
            x => x.TenantId == PlatformSeeder.DemoTenantId
                && x.ScopeType == AssetStatusRollupScopeTypes.Fleet);
        Assert.Equal(0, fleetRollup.ReadyCount);
        Assert.Equal(1, fleetRollup.NotReadyCount);

        var siteRollup = await db.AssetStatusScopeRollups.SingleAsync(
            x => x.TenantId == PlatformSeeder.DemoTenantId
                && x.ScopeType == AssetStatusRollupScopeTypes.Site
                && x.ScopeEntityKey == "Plant-A");
        Assert.Equal(1, siteRollup.TotalAssets);
        Assert.Equal(0, siteRollup.ReadyCount);

        var assetTypeRollup = await db.AssetStatusScopeRollups.SingleAsync(
            x => x.TenantId == PlatformSeeder.DemoTenantId
                && x.ScopeType == AssetStatusRollupScopeTypes.AssetType);
        Assert.Equal(1, assetTypeRollup.TotalAssets);
    }

    [Fact]
    public async Task Pending_preview_uses_tenant_configured_staleness_hours()
    {
        var assetId = await SeedAssetWithCriticalDefectAsync();
        await UpsertRollupSettingsAsync(stalenessHours: 24);

        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        var now = DateTimeOffset.UtcNow;
        db.AssetStatusRollups.Add(new AssetStatusRollup
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            AssetId = assetId,
            AssetTag = "ROLL-001",
            AssetName = "Rollup Asset",
            LifecycleStatus = "active",
            ReadinessStatus = "not_ready",
            ReadinessBasis = "live",
            BlockerCount = 1,
            ComputedAt = now.AddHours(-2),
            CreatedAt = now,
            UpdatedAt = now,
        });
        await db.SaveChangesAsync();

        var listRequest = Authorized(
            HttpMethod.Get,
            $"/api/asset-status-rollup-settings/pending",
            CreateMaintainArrAccessToken(["maintainarr"], "maintainarr_admin"));
        var listResponse = await _maintainarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var pending = (await listResponse.Content.ReadFromJsonAsync<PendingAssetStatusRollupsResponse>())!;
        Assert.Equal(24, pending.StalenessHours);
        Assert.DoesNotContain(pending.Items, x => x.AssetId == assetId);
    }

    [Fact]
    public async Task Pending_preview_lists_stale_assets_before_processing()
    {
        var assetId = await SeedAssetWithCriticalDefectAsync();
        await UpsertRollupSettingsAsync();

        var listRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/internal/asset-status-rollups/pending?tenantId={PlatformSeeder.DemoTenantId}&batchSize=10&stalenessHours=1");
        listRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _sharedWorkerToMaintainArrToken);
        var listResponse = await _maintainarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var pending = (await listResponse.Content.ReadFromJsonAsync<PendingAssetStatusRollupsResponse>())!;
        Assert.Contains(pending.Items, x => x.AssetId == assetId);
    }

    [Fact]
    public async Task Asset_status_rollup_v1_aliases_fleet_and_pending_work()
    {
        await SeedAssetWithCriticalDefectAsync();
        await UpsertRollupSettingsAsync();

        var processRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/asset-status-rollups/process-batch",
            _sharedWorkerToMaintainArrToken);
        processRequest.Content = JsonContent.Create(new ProcessAssetStatusRollupsRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            25,
            1));
        var processResponse = await _maintainarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();

        var adminToken = CreateMaintainArrAccessToken(["maintainarr"], "maintainarr_admin");
        var fleetResponse = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/asset-status-rollups/fleet", adminToken));
        fleetResponse.EnsureSuccessStatusCode();

        var pendingResponse = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/asset-status-rollup-settings/pending", adminToken));
        pendingResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Fleet_rollup_read_requires_authentication()
    {
        var response = await _maintainarrClient.GetAsync("/api/asset-status-rollups/fleet");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Settings_put_requires_admin()
    {
        var memberToken = CreateMaintainArrAccessToken(["maintainarr"], "maintainarr_manager");
        var request = Authorized(HttpMethod.Put, "/api/asset-status-rollup-settings", memberToken);
        request.Content = JsonContent.Create(new UpsertAssetStatusRollupSettingsRequest(true, 1));
        var response = await _maintainarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task UpsertRollupSettingsAsync(int stalenessHours = 1)
    {
        var token = CreateMaintainArrAccessToken(["maintainarr"], "maintainarr_admin");
        var request = Authorized(HttpMethod.Put, "/api/asset-status-rollup-settings", token);
        request.Content = JsonContent.Create(new UpsertAssetStatusRollupSettingsRequest(true, stalenessHours));
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
            ClassKey = "rollup-class",
            Name = "Rollup Class",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };
        var assetType = new AssetType
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            AssetClassId = assetClass.Id,
            TypeKey = "rollup-type",
            Name = "Rollup Type",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };
        var asset = new Asset
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            AssetTypeId = assetType.Id,
            AssetTag = "ROLL-001",
            Name = "Rollup Asset",
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
            $"{sourceProduct}-asset-status-rollup-{Guid.NewGuid():N}",
            $"{sourceProduct} asset status rollup test",
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
}
