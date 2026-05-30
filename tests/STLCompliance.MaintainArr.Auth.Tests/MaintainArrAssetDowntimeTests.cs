using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
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

public sealed class MaintainArrAssetDowntimeTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::MaintainArr.Api.Program> _maintainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _maintainarrClient = null!;
    private string _sharedWorkerToMaintainArrToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"DowntimeNexArr-{Guid.NewGuid():N}";
        var maintainArrDbName = $"DowntimeMaintainArr-{Guid.NewGuid():N}";

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
            AssetDowntimeSyncWorkerService.ProcessAssetDowntimeSyncActionScope);

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
    public async Task Process_batch_opens_automatic_downtime_for_not_ready_asset()
    {
        var assetId = await SeedAssetWithCriticalDefectAsync();
        await UpsertDowntimeSettingsAsync();

        var processRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/downtime-sync/process-batch",
            _sharedWorkerToMaintainArrToken);
        processRequest.Content = JsonContent.Create(new ProcessAssetDowntimeSyncRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            25));
        var processResponse = await _maintainarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var body = (await processResponse.Content.ReadFromJsonAsync<ProcessAssetDowntimeSyncResponse>())!;
        Assert.Equal(1, body.EventsOpened);
        Assert.Equal(1, body.AssetsScanned);

        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        var openEvent = await db.AssetDowntimeEvents.SingleAsync(
            x => x.AssetId == assetId && x.Source == AssetDowntimeSources.AutomaticStatus && x.EndedAt == null);
        Assert.Equal(AssetDowntimeReasons.RestrictedUse, openEvent.Reason);

        var fleetSnapshot = await db.FleetAvailabilitySnapshots.SingleAsync(
            x => x.TenantId == PlatformSeeder.DemoTenantId);
        Assert.True(fleetSnapshot.DowntimeHours >= 0);
    }

    [Fact]
    public async Task Manual_downtime_create_and_close_round_trip()
    {
        var assetId = await SeedActiveAssetAsync();
        var managerToken = CreateMaintainArrAccessToken(["maintainarr"], "maintainarr_manager");

        var createRequest = Authorized(HttpMethod.Post, "/api/downtime/events", managerToken);
        createRequest.Content = JsonContent.Create(new CreateManualDowntimeEventRequest(
            assetId,
            AssetDowntimeReasons.InRepair,
            IsPlanned: true,
            DateTimeOffset.UtcNow.AddHours(-2),
            "Shop bay maintenance",
            null,
            null));
        var createResponse = await _maintainarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<AssetDowntimeEventResponse>())!;
        Assert.Equal(AssetDowntimeSources.Manual, created.Source);
        Assert.True(created.IsActive);

        var closeRequest = Authorized(
            HttpMethod.Post,
            $"/api/downtime/events/{created.EventId}/close",
            managerToken);
        closeRequest.Content = JsonContent.Create(new CloseDowntimeEventRequest(DateTimeOffset.UtcNow, "Returned to service"));
        var closeResponse = await _maintainarrClient.SendAsync(closeRequest);
        closeResponse.EnsureSuccessStatusCode();
        var closed = (await closeResponse.Content.ReadFromJsonAsync<AssetDowntimeEventResponse>())!;
        Assert.False(closed.IsActive);
        Assert.NotNull(closed.EndedAt);
    }

    [Fact]
    public async Task Manual_downtime_v1_create_and_close_round_trip()
    {
        var assetId = await SeedActiveAssetAsync();
        var managerToken = CreateMaintainArrAccessToken(["maintainarr"], "maintainarr_manager");

        var createRequest = Authorized(HttpMethod.Post, "/api/v1/downtime/events", managerToken);
        createRequest.Content = JsonContent.Create(new CreateManualDowntimeEventRequest(
            assetId,
            AssetDowntimeReasons.InRepair,
            IsPlanned: true,
            DateTimeOffset.UtcNow.AddHours(-2),
            "Shop bay maintenance v1",
            null,
            null));
        var createResponse = await _maintainarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<AssetDowntimeEventResponse>())!;
        Assert.Equal(AssetDowntimeSources.Manual, created.Source);
        Assert.True(created.IsActive);

        var closeRequest = Authorized(
            HttpMethod.Post,
            $"/api/v1/downtime/events/{created.EventId}/close",
            managerToken);
        closeRequest.Content = JsonContent.Create(new CloseDowntimeEventRequest(DateTimeOffset.UtcNow, "Returned to service v1"));
        var closeResponse = await _maintainarrClient.SendAsync(closeRequest);
        closeResponse.EnsureSuccessStatusCode();
        var closed = (await closeResponse.Content.ReadFromJsonAsync<AssetDowntimeEventResponse>())!;
        Assert.False(closed.IsActive);
        Assert.NotNull(closed.EndedAt);
    }

    [Fact]
    public async Task Fleet_availability_requires_authenticated_user()
    {
        var response = await _maintainarrClient.GetAsync("/api/downtime/availability/fleet");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Settings_put_requires_admin()
    {
        var memberToken = CreateMaintainArrAccessToken(["maintainarr"], "maintainarr_manager");
        var request = Authorized(HttpMethod.Put, "/api/downtime-tracking-settings", memberToken);
        request.Content = JsonContent.Create(new UpsertDowntimeTrackingSettingsRequest(
            true,
            true,
            true,
            30));
        var response = await _maintainarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Downtime_tracking_settings_v1_put_get_pending_and_runs_work_for_admin()
    {
        var adminToken = CreateMaintainArrAccessToken(["maintainarr"], "maintainarr_admin");

        var putRequest = Authorized(HttpMethod.Put, "/api/v1/downtime-tracking-settings", adminToken);
        putRequest.Content = JsonContent.Create(new UpsertDowntimeTrackingSettingsRequest(
            true,
            true,
            true,
            45));
        var putResponse = await _maintainarrClient.SendAsync(putRequest);
        putResponse.EnsureSuccessStatusCode();

        var getResponse = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/downtime-tracking-settings", adminToken));
        getResponse.EnsureSuccessStatusCode();

        var pendingResponse = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/downtime-tracking-settings/pending", adminToken));
        pendingResponse.EnsureSuccessStatusCode();

        var runsResponse = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/downtime-tracking-settings/runs?limit=5", adminToken));
        runsResponse.EnsureSuccessStatusCode();
    }

    private async Task UpsertDowntimeSettingsAsync()
    {
        var token = CreateMaintainArrAccessToken(["maintainarr"], "maintainarr_admin");
        var request = Authorized(HttpMethod.Put, "/api/downtime-tracking-settings", token);
        request.Content = JsonContent.Create(new UpsertDowntimeTrackingSettingsRequest(
            true,
            true,
            true,
            30));
        var response = await _maintainarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    private async Task<Guid> SeedAssetWithCriticalDefectAsync()
    {
        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        var now = DateTimeOffset.UtcNow;
        var asset = await SeedAssetGraphAsync(db, now, "DT-001", "active");
        db.Defects.Add(new Defect
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
        });
        await db.SaveChangesAsync();
        return asset.Id;
    }

    private async Task<Guid> SeedActiveAssetAsync()
    {
        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        var now = DateTimeOffset.UtcNow;
        var asset = await SeedAssetGraphAsync(db, now, "DT-002", "active");
        await db.SaveChangesAsync();
        return asset.Id;
    }

    private static async Task<Asset> SeedAssetGraphAsync(
        MaintainArrDbContext db,
        DateTimeOffset now,
        string assetTag,
        string lifecycleStatus)
    {
        var assetClass = new AssetClass
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            ClassKey = $"class-{assetTag}",
            Name = "Downtime Class",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };
        var assetType = new AssetType
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            AssetClassId = assetClass.Id,
            TypeKey = $"type-{assetTag}",
            Name = "Downtime Type",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };
        var asset = new Asset
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            AssetTypeId = assetType.Id,
            AssetTag = assetTag,
            Name = $"Asset {assetTag}",
            LifecycleStatus = lifecycleStatus,
            CreatedAt = now,
            UpdatedAt = now,
        };
        db.AssetClasses.Add(assetClass);
        db.AssetTypes.Add(assetType);
        db.Assets.Add(asset);
        await db.SaveChangesAsync();
        return asset;
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
            $"{sourceProduct}-downtime-sync-{Guid.NewGuid():N}",
            $"{sourceProduct} downtime sync test",
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

    private static HttpRequestMessage Authorized(HttpMethod method, string path, string token)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private static void RemoveDbContext<TContext>(IServiceCollection services) where TContext : DbContext
    {
        var descriptors = services.Where(d =>
            d.ServiceType == typeof(DbContextOptions<TContext>)
            || d.ServiceType == typeof(TContext)).ToList();
        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
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
}
