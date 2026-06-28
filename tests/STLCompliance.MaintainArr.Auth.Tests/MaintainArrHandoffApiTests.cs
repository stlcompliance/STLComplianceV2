using STLCompliance.Shared.Integration;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MaintainArrRedeemRequest = MaintainArr.Api.Contracts.RedeemHandoffRequest;
using MaintainArrHandoffSessionResponse = MaintainArr.Api.Contracts.HandoffSessionResponse;
using MaintainArrMeResponse = MaintainArr.Api.Contracts.MaintainArrMeResponse;
using AssetClassResponse = MaintainArr.Api.Contracts.AssetClassResponse;
using CreateAssetClassRequest = MaintainArr.Api.Contracts.CreateAssetClassRequest;
using AssetTypeResponse = MaintainArr.Api.Contracts.AssetTypeResponse;
using CreateAssetTypeRequest = MaintainArr.Api.Contracts.CreateAssetTypeRequest;
using AssetResponse = MaintainArr.Api.Contracts.AssetResponse;
using CreateAssetRequest = MaintainArr.Api.Contracts.CreateAssetRequest;
using AssetUpsertV1Request = MaintainArr.Api.Contracts.AssetUpsertV1Request;
using FieldsetResponse = MaintainArr.Api.Contracts.FieldsetResponse;
using MaintainArr.Api.Data;
using MaintainArr.Api.Services;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using NexArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.MaintainArr.Auth.Tests;

public sealed class MaintainArrHandoffApiTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::MaintainArr.Api.Program> _maintainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _maintainarrClient = null!;
    private string _serviceToken = null!;
    private readonly Guid _staffarrSiteOrgUnitId = Guid.Parse("5f0b49a9-7c67-4ce1-a0e9-3e7e226d3992");
    private RecordingStaffArrSiteLookupHandler _staffarrSiteLookupHandler = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"MaintainArrHandoffNexArrTests-{Guid.NewGuid():N}";
        var maintainArrDbName = $"MaintainArrHandoffTests-{Guid.NewGuid():N}";

        _nexarrFactory = new WebApplicationFactory<global::NexArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
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
                    options.UseInMemoryDatabase(nexArrDbName));
            });
        });

        _nexarrClient = _nexarrFactory.CreateClient();
        await SeedNexArrAsync();

        var adminToken = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        _serviceToken = await IssueServiceTokenAsync(adminToken, "maintainarr");
        _staffarrSiteLookupHandler = new RecordingStaffArrSiteLookupHandler(_staffarrSiteOrgUnitId);
        ClearTenantSeedReady(PlatformSeeder.DemoTenantId);

        _maintainarrFactory = new WebApplicationFactory<global::MaintainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", _serviceToken);
            builder.UseSetting("StaffArr:BaseUrl", "http://staffarr.test");
            builder.UseSetting("StaffArr:ServiceToken", "maintainarr-to-staffarr-sites");
            builder.ConfigureServices(services =>
            {
                var descriptors = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions<MaintainArrDbContext>)
                        || d.ServiceType == typeof(MaintainArrDbContext))
                    .ToList();
                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<MaintainArrDbContext>(options =>
                    options.UseInMemoryDatabase(maintainArrDbName));

                services.AddHttpClient<StlNexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarrFactory.Server.CreateHandler());
                services.AddHttpClient<StlNexArrLaunchClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarrFactory.Server.CreateHandler());
                services.AddHttpClient<StaffArrSiteLookupClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _staffarrSiteLookupHandler);
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
    public async Task Handoff_redeem_happy_path_returns_session_and_me_works()
    {
        var handoffCode = await CreateHandoffAsync();
        var redeemResponse = await _maintainarrClient.PostAsJsonAsync(
            "/api/auth/handoff/redeem",
            new MaintainArrRedeemRequest(handoffCode));

        redeemResponse.EnsureSuccessStatusCode();
        var session = (await redeemResponse.Content.ReadFromJsonAsync<MaintainArrHandoffSessionResponse>())!;
        Assert.False(string.IsNullOrWhiteSpace(session.AccessToken));
        Assert.Equal(PlatformSeeder.DemoAdminUserId, session.UserId);
        Assert.Equal(session.UserId, session.PersonId);
        Assert.Contains("maintainarr", session.LaunchableProductKeys);
        Assert.Contains("ledgarr", session.LaunchableProductKeys);
        Assert.DoesNotContain("compliancecore", session.LaunchableProductKeys);

        var meRequest = Authorized(HttpMethod.Get, "/api/me", session.AccessToken);
        var meResponse = await _maintainarrClient.SendAsync(meRequest);
        meResponse.EnsureSuccessStatusCode();
        var me = await meResponse.Content.ReadFromJsonAsync<MaintainArrMeResponse>();
        Assert.NotNull(me);
        Assert.Contains("maintainarr", me.LaunchableProductKeys);
    }

    [Fact]
    public async Task Handoff_redeem_nexarr_alias_happy_path_returns_session()
    {
        var handoffCode = await CreateHandoffAsync();
        var redeemResponse = await _maintainarrClient.PostAsJsonAsync(
            "/api/auth/nexarr/redeem",
            new MaintainArrRedeemRequest(handoffCode));

        redeemResponse.EnsureSuccessStatusCode();
        var session = (await redeemResponse.Content.ReadFromJsonAsync<MaintainArrHandoffSessionResponse>())!;
        Assert.False(string.IsNullOrWhiteSpace(session.AccessToken));
        Assert.Contains("maintainarr", session.LaunchableProductKeys);
    }

    [Fact]
    public async Task V1_handoff_session_and_me_aliases_work()
    {
        var handoffCode = await CreateHandoffAsync();
        var redeemResponse = await _maintainarrClient.PostAsJsonAsync(
            "/api/v1/auth/handoff/redeem",
            new MaintainArrRedeemRequest(handoffCode));

        redeemResponse.EnsureSuccessStatusCode();
        var session = (await redeemResponse.Content.ReadFromJsonAsync<MaintainArrHandoffSessionResponse>())!;
        Assert.False(string.IsNullOrWhiteSpace(session.AccessToken));

        var meResponse = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/me", session.AccessToken));
        meResponse.EnsureSuccessStatusCode();
        var me = (await meResponse.Content.ReadFromJsonAsync<MaintainArrMeResponse>())!;
        Assert.Contains("maintainarr", me.LaunchableProductKeys);

        var sessionResponse = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/session", session.AccessToken));
        sessionResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task V1_launch_handoff_proxy_returns_handoff_code()
    {
        var nexarrToken = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        var request = Authorized(HttpMethod.Post, "/api/v1/launch/handoff", nexarrToken);
        request.Content = JsonContent.Create(new CreateHandoffRequest("maintainarr", "http://localhost:5178/launch"));
        var response = await _maintainarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var handoff = (await response.Content.ReadFromJsonAsync<HandoffCreatedResponse>())!;
        Assert.False(string.IsNullOrWhiteSpace(handoff.HandoffCode));
    }

    [Fact]
    public async Task Asset_create_fieldset_v1_seeds_defaults_for_fresh_tenant()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var response = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/fieldsets/assets/create", token));

        await AssertSuccessAsync(response);
        var fieldset = (await response.Content.ReadFromJsonAsync<FieldsetResponse>())!;
        Assert.Equal("assets", fieldset.Key);
        Assert.Equal("create", fieldset.Purpose);
        Assert.Contains(fieldset.Fields, x => x.Key == "assetType");

        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        Assert.True(await db.FieldsetDefinitions.AnyAsync(x =>
            x.TenantId == PlatformSeeder.DemoTenantId
            && x.Key == "assets"
            && x.Purpose == "create"
            && x.IsActive));
    }

    [Fact]
    public async Task Asset_registry_crud_happy_path()
    {
        var token = await RedeemMaintainArrTokenAsync();

        var createClassRequest = Authorized(HttpMethod.Post, "/api/asset-classes", token);
        createClassRequest.Content = JsonContent.Create(new CreateAssetClassRequest(
            "heavy-equipment",
            "Heavy Equipment",
            "Tracked and wheeled heavy assets"));
        var createClassResponse = await _maintainarrClient.SendAsync(createClassRequest);
        await AssertSuccessAsync(createClassResponse);
        var assetClass = (await createClassResponse.Content.ReadFromJsonAsync<AssetClassResponse>())!;

        var createTypeRequest = Authorized(HttpMethod.Post, "/api/asset-types", token);
        createTypeRequest.Content = JsonContent.Create(new CreateAssetTypeRequest(
            assetClass.AssetClassId,
            "excavator",
            "Excavator",
            "Hydraulic excavators"));
        var createTypeResponse = await _maintainarrClient.SendAsync(createTypeRequest);
        createTypeResponse.EnsureSuccessStatusCode();
        var assetType = (await createTypeResponse.Content.ReadFromJsonAsync<AssetTypeResponse>())!;

        var createAssetRequest = Authorized(HttpMethod.Post, "/api/assets", token);
        createAssetRequest.Content = JsonContent.Create(new CreateAssetRequest(
            assetType.AssetTypeId,
            "EX-1001",
            "Excavator 1001",
            "Primary yard excavator",
            _staffarrSiteOrgUnitId.ToString("D")));
        var createAssetResponse = await _maintainarrClient.SendAsync(createAssetRequest);
        createAssetResponse.EnsureSuccessStatusCode();
        var asset = (await createAssetResponse.Content.ReadFromJsonAsync<AssetResponse>())!;
        Assert.Equal("EX-1001", asset.AssetTag);
        Assert.Equal("active", asset.LifecycleStatus);
        Assert.Equal(_staffarrSiteOrgUnitId, asset.StaffarrSiteOrgUnitId);
        Assert.Equal("Central Maintenance Site", asset.StaffarrSiteNameSnapshot);
        Assert.Equal(_staffarrSiteOrgUnitId.ToString("D"), asset.SiteRef);

        var listAssetsRequest = Authorized(HttpMethod.Get, "/api/assets", token);
        var listAssetsResponse = await _maintainarrClient.SendAsync(listAssetsRequest);
        listAssetsResponse.EnsureSuccessStatusCode();
        var assets = (await listAssetsResponse.Content.ReadFromJsonAsync<List<AssetResponse>>())!;
        Assert.Contains(assets, x => x.AssetId == asset.AssetId);

        var getAssetRequest = Authorized(HttpMethod.Get, $"/api/assets/{asset.AssetId}", token);
        var getAssetResponse = await _maintainarrClient.SendAsync(getAssetRequest);
        getAssetResponse.EnsureSuccessStatusCode();
        var fetched = (await getAssetResponse.Content.ReadFromJsonAsync<AssetResponse>())!;
        Assert.Equal("excavator", fetched.TypeKey);
        Assert.Equal("heavy-equipment", fetched.ClassKey);
    }

    [Fact]
    public async Task Asset_registry_crud_v1_alias_happy_path()
    {
        var token = await RedeemMaintainArrTokenAsync();

        var createClassRequest = Authorized(HttpMethod.Post, "/api/v1/asset-classes", token);
        createClassRequest.Content = JsonContent.Create(new CreateAssetClassRequest(
            "yard-equipment",
            "Yard Equipment",
            "Assets for yard operations"));
        var createClassResponse = await _maintainarrClient.SendAsync(createClassRequest);
        await AssertSuccessAsync(createClassResponse);
        var assetClass = (await createClassResponse.Content.ReadFromJsonAsync<AssetClassResponse>())!;

        var createTypeRequest = Authorized(HttpMethod.Post, "/api/v1/asset-types", token);
        createTypeRequest.Content = JsonContent.Create(new CreateAssetTypeRequest(
            assetClass.AssetClassId,
            "loader",
            "Loader",
            "Wheel loaders"));
        var createTypeResponse = await _maintainarrClient.SendAsync(createTypeRequest);
        createTypeResponse.EnsureSuccessStatusCode();
        var assetType = (await createTypeResponse.Content.ReadFromJsonAsync<AssetTypeResponse>())!;

        var createAssetRequest = Authorized(HttpMethod.Post, "/api/v1/assets", token);
        createAssetRequest.Content = JsonContent.Create(new AssetUpsertV1Request(
            "LD-2001",
            "Loader 2001",
            "Primary loader",
            new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
            {
                ["assetClass"] = "vehicle",
                ["assetType"] = "pickup",
                ["siteId"] = _staffarrSiteOrgUnitId.ToString("D"),
                ["lifecycleStatus"] = "active",
            }));
        var createAssetResponse = await _maintainarrClient.SendAsync(createAssetRequest);
        await AssertSuccessAsync(createAssetResponse);
        var asset = (await createAssetResponse.Content.ReadFromJsonAsync<AssetResponse>())!;
        Assert.Equal(_staffarrSiteOrgUnitId, asset.StaffarrSiteOrgUnitId);

        var listAssetsRequest = Authorized(HttpMethod.Get, "/api/v1/assets", token);
        var listAssetsResponse = await _maintainarrClient.SendAsync(listAssetsRequest);
        listAssetsResponse.EnsureSuccessStatusCode();
        var assets = (await listAssetsResponse.Content.ReadFromJsonAsync<List<AssetResponse>>())!;
        Assert.Contains(assets, x => x.AssetId == asset.AssetId);

        var getAssetRequest = Authorized(HttpMethod.Get, $"/api/v1/assets/{asset.AssetId}", token);
        var getAssetResponse = await _maintainarrClient.SendAsync(getAssetRequest);
        getAssetResponse.EnsureSuccessStatusCode();
        var fetched = (await getAssetResponse.Content.ReadFromJsonAsync<AssetResponse>())!;
        Assert.Equal("pickup", fetched.TypeKey);
        Assert.Equal("vehicle", fetched.ClassKey);
    }

    [Fact]
    public async Task Asset_create_rejects_free_text_internal_site_alias()
    {
        var token = await RedeemMaintainArrTokenAsync();

        var createClassRequest = Authorized(HttpMethod.Post, "/api/asset-classes", token);
        createClassRequest.Content = JsonContent.Create(new CreateAssetClassRequest(
            "site-validation",
            "Site Validation",
            "Assets used for StaffArr site validation"));
        var createClassResponse = await _maintainarrClient.SendAsync(createClassRequest);
        await AssertSuccessAsync(createClassResponse);
        var assetClass = (await createClassResponse.Content.ReadFromJsonAsync<AssetClassResponse>())!;

        var createTypeRequest = Authorized(HttpMethod.Post, "/api/asset-types", token);
        createTypeRequest.Content = JsonContent.Create(new CreateAssetTypeRequest(
            assetClass.AssetClassId,
            "test-asset",
            "Test asset",
            "Validation asset type"));
        var createTypeResponse = await _maintainarrClient.SendAsync(createTypeRequest);
        createTypeResponse.EnsureSuccessStatusCode();
        var assetType = (await createTypeResponse.Content.ReadFromJsonAsync<AssetTypeResponse>())!;

        var createAssetRequest = Authorized(HttpMethod.Post, "/api/assets", token);
        createAssetRequest.Content = JsonContent.Create(new CreateAssetRequest(
            assetType.AssetTypeId,
            "SITE-TEXT-1",
            "Free Text Site Asset",
            "Should be rejected",
            "yard-a"));

        var createAssetResponse = await _maintainarrClient.SendAsync(createAssetRequest);
        Assert.Equal(HttpStatusCode.BadRequest, createAssetResponse.StatusCode);
    }

    [Fact]
    public async Task Asset_create_denied_without_manage_role()
    {
        var token = CreateMaintainArrAccessToken(["maintainarr"], "maintainarr_technician");
        var request = Authorized(HttpMethod.Post, "/api/assets", token);
        request.Content = JsonContent.Create(new CreateAssetRequest(
            Guid.NewGuid(),
            "EX-9999",
            "Denied Asset",
            string.Empty,
            null));

        var response = await _maintainarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Me_allows_users_after_non_maintainarr_launch_context()
    {
        var token = CreateMaintainArrAccessToken(["nexarr"]);
        var request = Authorized(HttpMethod.Get, "/api/me", token);
        var response = await _maintainarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var me = (await response.Content.ReadFromJsonAsync<MaintainArrMeResponse>())!;
        Assert.Contains("maintainarr", me.LaunchableProductKeys);
        Assert.Contains("ledgarr", me.LaunchableProductKeys);
        Assert.DoesNotContain("compliancecore", me.LaunchableProductKeys);
    }

    private async Task<string> RedeemMaintainArrTokenAsync()
    {
        var handoffCode = await CreateHandoffAsync();
        var redeemResponse = await _maintainarrClient.PostAsJsonAsync(
            "/api/auth/handoff/redeem",
            new MaintainArrRedeemRequest(handoffCode));
        redeemResponse.EnsureSuccessStatusCode();
        var session = (await redeemResponse.Content.ReadFromJsonAsync<MaintainArrHandoffSessionResponse>())!;
        return session.AccessToken;
    }

    private async Task<string> CreateHandoffAsync()
    {
        var token = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        var request = Authorized(HttpMethod.Post, "/api/v1/launch/handoff", token);
        request.Content = JsonContent.Create(new CreateHandoffRequest("maintainarr", "http://localhost:5178/launch"));
        var response = await _nexarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var handoff = (await response.Content.ReadFromJsonAsync<HandoffCreatedResponse>())!;
        return handoff.HandoffCode;
    }

    private async Task<string> IssueServiceTokenAsync(string adminToken, string productKey)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{productKey}-maintainarr-handoff-test",
            $"{productKey} MaintainArr Handoff Test",
            productKey,
            [productKey]));
        var registerResponse = await _nexarrClient.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/service-tokens", adminToken);
        issueRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            null,
            "launch.redeem",
            30));
        var issueResponse = await _nexarrClient.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        var issued = (await issueResponse.Content.ReadFromJsonAsync<ServiceTokenIssueResponse>())!;
        return issued.AccessToken;
    }

    private string CreateMaintainArrAccessToken(
        IReadOnlyList<string> launchableProductKeys,
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
            launchableProductKeys,
            isPlatformAdmin: false);
        return token;
    }

    private async Task<string> LoginNexArrAsync(string email)
    {
        var response = await _nexarrClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        var login = (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!;
        return login.AccessToken;
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
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private static async Task AssertSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync();
        Assert.Fail($"Expected success but received {(int)response.StatusCode} {response.StatusCode}: {body}");
    }

    private static void ClearTenantSeedReady(Guid tenantId)
    {
        var readyField = typeof(CatalogSeedService).GetField("TenantSeedReady", BindingFlags.NonPublic | BindingFlags.Static);
        var ready = Assert.IsType<ConcurrentDictionary<Guid, byte>>(readyField?.GetValue(null));
        ready.TryRemove(tenantId, out _);
    }

    private sealed class RecordingStaffArrSiteLookupHandler(Guid siteOrgUnitId) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var path = request.RequestUri?.AbsolutePath ?? string.Empty;
            if (!path.Contains("/api/v1/integrations/sites", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            }

            var response = new StaffArrSiteLookupResponse(
                siteOrgUnitId,
                "Central Maintenance Site",
                null,
                null,
                "active",
                DateTimeOffset.UnixEpoch);

            if (path.EndsWith("/sites", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(new[] { response })
                });
            }

            if (path.EndsWith($"/{siteOrgUnitId:D}", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(response)
                });
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }
    }
}


