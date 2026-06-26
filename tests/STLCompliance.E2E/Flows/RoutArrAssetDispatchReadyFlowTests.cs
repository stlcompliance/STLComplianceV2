using STLCompliance.Shared.Integration;
using System.Net.Http.Json;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using MaintainArr.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Data;
using NexArr.Api.Services;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Services;
using MaintainArrIntegration = MaintainArr.Api.Endpoints.IntegrationEndpoints;
using RoutArrRedeemRequest = RoutArr.Api.Contracts.RedeemHandoffRequest;
using RoutArrHandoffSessionResponse = RoutArr.Api.Contracts.HandoffSessionResponse;
using MaintainArrRedeemRequest = MaintainArr.Api.Contracts.RedeemHandoffRequest;
using MaintainArrHandoffSessionResponse = MaintainArr.Api.Contracts.HandoffSessionResponse;

namespace STLCompliance.E2E.Flows;

/// <summary>
/// MaintainArr ready asset → RoutArr dispatchability allow and vehicle assign (docs/23 workflow 2).
/// </summary>
[Trait("Category", "Integration")]
public sealed class RoutArrAssetDispatchReadyFlowTests : IAsyncLifetime
{
    private const string SigningKey = "test-signing-key-at-least-32-chars-long";

    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::MaintainArr.Api.Program> _maintainarrFactory = null!;
    private WebApplicationFactory<global::RoutArr.Api.Program> _routarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _maintainarrClient = null!;
    private HttpClient _routarrClient = null!;
    private string _routarrToMaintainarrToken = null!;
    private readonly Guid _staffarrSiteOrgUnitId = Guid.Parse("5f0b49a9-7c67-4ce1-a0e9-3e7e226d3992");

    public async Task InitializeAsync()
    {
        var nexArrDbName = $"E2E-RoutDisp-NexArr-{Guid.NewGuid():N}";
        var maintainArrDbName = $"E2E-RoutDisp-MaintainArr-{Guid.NewGuid():N}";
        var routArrDbName = $"E2E-RoutDisp-RoutArr-{Guid.NewGuid():N}";

        _nexarrFactory = new WebApplicationFactory<global::NexArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", SigningKey);
            builder.UseSetting("ServiceToken:SigningKey", SigningKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<NexArrDbContext>(services);
                services.AddDbContext<NexArrDbContext>(options => options.UseInMemoryDatabase(nexArrDbName));
            });
        });

        _nexarrClient = _nexarrFactory.CreateClient();
        await SeedNexArrAsync();

        var adminToken = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        var routarrHandoffToken = await IssueServiceTokenAsync(adminToken, "routarr", ["routarr"], "launch.redeem");
        _routarrToMaintainarrToken = await IssueServiceTokenAsync(
            adminToken,
            "routarr",
            ["maintainarr"],
            MaintainArrIntegration.RoutarrAssetReadinessDispatchActionScope);
        var maintainarrHandoffToken = await IssueServiceTokenAsync(adminToken, "maintainarr", ["maintainarr"], "launch.redeem");

        _maintainarrFactory = new WebApplicationFactory<global::MaintainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", SigningKey);
            builder.UseSetting("ServiceToken:SigningKey", SigningKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", maintainarrHandoffToken);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<MaintainArrDbContext>(services);
                services.AddDbContext<MaintainArrDbContext>(options => options.UseInMemoryDatabase(maintainArrDbName));

                services.AddHttpClient<StlNexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarrFactory.Server.CreateHandler());
            });
        });

        _maintainarrClient = _maintainarrFactory.CreateClient();
        using (var scope = _maintainarrFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
            await db.Database.EnsureCreatedAsync();
        }
        await SeedCachedStaffArrSiteAsync();

        _routarrFactory = new WebApplicationFactory<global::RoutArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", SigningKey);
            builder.UseSetting("ServiceToken:SigningKey", SigningKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", routarrHandoffToken);
            builder.UseSetting("MaintainArr:BaseUrl", _maintainarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("MaintainArr:ServiceToken", _routarrToMaintainarrToken);
            builder.UseSetting("DriverEligibility:CheckStaffArrReadiness", "false");
            builder.UseSetting("DriverEligibility:CheckTrainArrQualification", "false");
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<RoutArrDbContext>(services);
                services.AddDbContext<RoutArrDbContext>(options => options.UseInMemoryDatabase(routArrDbName));

                services.AddHttpClient<StlNexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarrFactory.Server.CreateHandler());
                services.AddHttpClient<MaintainArrAssetReadinessClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _maintainarrFactory.Server.CreateHandler());
            });
        });

        _routarrClient = _routarrFactory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _routarrClient.Dispose();
        _maintainarrClient.Dispose();
        _nexarrClient.Dispose();
        await _routarrFactory.DisposeAsync();
        await _maintainarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Ready_maintainarr_asset_allows_routarr_dispatchability_and_vehicle_assign()
    {
        var maintainarrToken = CreateMaintainArrAccessToken(["maintainarr"], "tenant_admin");
        var asset = await SeedReadyAssetAsync(maintainarrToken);

        var readinessRequest = Authorized(HttpMethod.Get, $"/api/asset-readiness?assetId={asset.AssetId}", maintainarrToken);
        var readinessResponse = await _maintainarrClient.SendAsync(readinessRequest);
        readinessResponse.EnsureSuccessStatusCode();
        var readiness = (await readinessResponse.Content.ReadFromJsonAsync<AssetReadinessResponse>())!;
        Assert.Equal("ready", readiness.ReadinessStatus);

        var routarrToken = CreateRoutArrAccessToken(["routarr"], "tenant_admin");
        var checkRequest = Authorized(HttpMethod.Post, "/api/asset-dispatchability/check", routarrToken);
        checkRequest.Content = JsonContent.Create(new AssetDispatchabilityCheckRequest(asset.AssetTag, null));
        var checkResponse = await _routarrClient.SendAsync(checkRequest);
        checkResponse.EnsureSuccessStatusCode();
        var check = (await checkResponse.Content.ReadFromJsonAsync<AssetDispatchabilityCheckResponse>())!;
        Assert.Equal(AssetDispatchabilityOutcomes.Allow, check.Outcome);

        var now = DateTimeOffset.UtcNow;
        var createTripRequest = Authorized(HttpMethod.Post, "/api/trips", routarrToken);
        createTripRequest.Content = JsonContent.Create(new CreateTripRequest(
            "Dispatch-ready asset trip",
            "Cross-product asset → dispatch",
            null,
            now.AddHours(2),
            now.AddHours(6),
            null));
        var createTripResponse = await _routarrClient.SendAsync(createTripRequest);
        createTripResponse.EnsureSuccessStatusCode();
        var trip = (await createTripResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;

        var assignRequest = Authorized(HttpMethod.Patch, $"/api/trips/{trip.TripId}/assign-vehicle", routarrToken);
        assignRequest.Content = JsonContent.Create(new AssignTripVehicleRequest(asset.AssetTag));
        var assignResponse = await _routarrClient.SendAsync(assignRequest);
        assignResponse.EnsureSuccessStatusCode();
        var assigned = (await assignResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;
        Assert.Equal(asset.AssetTag, assigned.VehicleRefKey);
    }

    private async Task<AssetResponse> SeedReadyAssetAsync(string token)
    {
        var createClassRequest = Authorized(HttpMethod.Post, "/api/asset-classes", token);
        createClassRequest.Content = JsonContent.Create(new CreateAssetClassRequest(
            $"disp-vehicles-{Guid.NewGuid():N}".Substring(0, 12),
            "Dispatch Vehicles",
            string.Empty));
        var createClassResponse = await _maintainarrClient.SendAsync(createClassRequest);
        createClassResponse.EnsureSuccessStatusCode();
        var assetClass = (await createClassResponse.Content.ReadFromJsonAsync<AssetClassResponse>())!;

        var createTypeRequest = Authorized(HttpMethod.Post, "/api/asset-types", token);
        createTypeRequest.Content = JsonContent.Create(new CreateAssetTypeRequest(
            assetClass.AssetClassId,
            $"disp-truck-{Guid.NewGuid():N}".Substring(0, 12),
            "Dispatch Truck",
            string.Empty));
        var createTypeResponse = await _maintainarrClient.SendAsync(createTypeRequest);
        createTypeResponse.EnsureSuccessStatusCode();
        var assetType = (await createTypeResponse.Content.ReadFromJsonAsync<AssetTypeResponse>())!;

        var assetTag = $"DISP-{Guid.NewGuid():N}".Substring(0, 12);
        var createAssetRequest = Authorized(HttpMethod.Post, "/api/assets", token);
        createAssetRequest.Content = JsonContent.Create(new CreateAssetRequest(
            assetType.AssetTypeId,
            assetTag,
            "Dispatch-Ready Asset",
            string.Empty,
            _staffarrSiteOrgUnitId.ToString("D")));
        var createAssetResponse = await _maintainarrClient.SendAsync(createAssetRequest);
        createAssetResponse.EnsureSuccessStatusCode();
        return (await createAssetResponse.Content.ReadFromJsonAsync<AssetResponse>())!;
    }

    private string CreateMaintainArrAccessToken(
        IReadOnlyList<string> launchableProductKeys,
        string tenantRoleKey = "tenant_admin",
        Guid? userIdOverride = null)
    {
        using var scope = _maintainarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<MaintainArrTokenService>();
        var userId = userIdOverride ?? PlatformSeeder.DemoAdminUserId;
        var (token, _) = tokenService.CreateAccessToken(
            userId,
            userId,
            PlatformSeeder.DemoAdminEmail,
            "Demo Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            launchableProductKeys,
            isPlatformAdmin: false);
        return token;
    }

    private string CreateRoutArrAccessToken(
        IReadOnlyList<string> launchableProductKeys,
        string tenantRoleKey = "tenant_admin",
        Guid? userIdOverride = null)
    {
        using var scope = _routarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<RoutArrTokenService>();
        var userId = userIdOverride ?? PlatformSeeder.DemoAdminUserId;
        var (token, _) = tokenService.CreateAccessToken(
            userId,
            userId,
            PlatformSeeder.DemoAdminEmail,
            "Demo Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            launchableProductKeys,
            isPlatformAdmin: false);
        return token;
    }

    private async Task SeedCachedStaffArrSiteAsync()
    {
        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();

        db.ReferenceCacheEntries.Add(new ReferenceCacheEntry
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            SourceOfTruth = "StaffArr",
            ReferenceKey = "sites",
            ExternalKey = _staffarrSiteOrgUnitId.ToString("D"),
            ExternalId = _staffarrSiteOrgUnitId.ToString("D"),
            Label = "Central Maintenance Site",
            Description = null,
            MetadataJson = "{}",
            IsActive = true,
            LastSyncedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        });

        await db.SaveChangesAsync();
    }

    private async Task<string> RedeemMaintainArrTokenAsync()
    {
        var handoffCode = await CreateHandoffAsync("maintainarr", "http://localhost:5178/launch");
        var redeemResponse = await _maintainarrClient.PostAsJsonAsync(
            "/api/auth/handoff/redeem",
            new MaintainArrRedeemRequest(handoffCode));
        redeemResponse.EnsureSuccessStatusCode();
        var session = (await redeemResponse.Content.ReadFromJsonAsync<MaintainArrHandoffSessionResponse>())!;
        return session.AccessToken;
    }

    private async Task<string> RedeemRoutArrTokenAsync()
    {
        var handoffCode = await CreateHandoffAsync("routarr", "http://localhost:5180/launch");
        var redeemResponse = await _routarrClient.PostAsJsonAsync(
            "/api/auth/handoff/redeem",
            new RoutArrRedeemRequest(handoffCode));
        redeemResponse.EnsureSuccessStatusCode();
        var session = (await redeemResponse.Content.ReadFromJsonAsync<RoutArrHandoffSessionResponse>())!;
        return session.AccessToken;
    }

    private async Task<string> CreateHandoffAsync(string productKey, string callbackUrl)
    {
        var token = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        var request = Authorized(HttpMethod.Post, "/api/v1/launch/handoff", token);
        request.Content = JsonContent.Create(new NexArr.Api.Contracts.CreateHandoffRequest(productKey, callbackUrl));
        var response = await _nexarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var handoff = (await response.Content.ReadFromJsonAsync<NexArr.Api.Contracts.HandoffCreatedResponse>())!;
        return handoff.HandoffCode;
    }

    private async Task<string> LoginNexArrAsync(string email)
    {
        var response = await _nexarrClient.PostAsJsonAsync(
            "/api/auth/login",
            new NexArr.Api.Contracts.LoginRequest(email, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        var login = (await response.Content.ReadFromJsonAsync<NexArr.Api.Contracts.AuthTokenResponse>())!;
        return login.AccessToken;
    }

    private async Task<string> IssueServiceTokenAsync(
        string adminToken,
        string productKey,
        IReadOnlyList<string> targetProducts,
        string actionScope)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new NexArr.Api.Contracts.RegisterServiceClientRequest(
            $"{productKey}-e2e-{Guid.NewGuid():N}",
            $"{productKey} E2E",
            productKey,
            targetProducts));
        var registerResponse = await _nexarrClient.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<NexArr.Api.Contracts.ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/service-tokens", adminToken);
        issueRequest.Content = JsonContent.Create(new NexArr.Api.Contracts.IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            targetProducts,
            actionScope,
            30));
        var issueResponse = await _nexarrClient.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        var issued = (await issueResponse.Content.ReadFromJsonAsync<NexArr.Api.Contracts.ServiceTokenIssueResponse>())!;
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

    private static HttpRequestMessage Authorized(HttpMethod method, string path, string token)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
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
}
