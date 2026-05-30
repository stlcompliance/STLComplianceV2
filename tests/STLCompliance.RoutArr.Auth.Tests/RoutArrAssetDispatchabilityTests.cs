using STLCompliance.Shared.Integration;
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
using MaintainArr.Api.Endpoints;
using MaintainArr.Api.Entities;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Services;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Services;
using RoutArrRedeemRequest = RoutArr.Api.Contracts.RedeemHandoffRequest;
using RoutArrHandoffSessionResponse = RoutArr.Api.Contracts.HandoffSessionResponse;
using CreateAssetClassRequest = MaintainArr.Api.Contracts.CreateAssetClassRequest;
using CreateAssetTypeRequest = MaintainArr.Api.Contracts.CreateAssetTypeRequest;
using CreateAssetRequest = MaintainArr.Api.Contracts.CreateAssetRequest;
using AssetClassResponse = MaintainArr.Api.Contracts.AssetClassResponse;
using AssetTypeResponse = MaintainArr.Api.Contracts.AssetTypeResponse;
using AssetResponse = MaintainArr.Api.Contracts.AssetResponse;
using MaintainArrRedeemRequest = MaintainArr.Api.Contracts.RedeemHandoffRequest;
using MaintainArrHandoffSessionResponse = MaintainArr.Api.Contracts.HandoffSessionResponse;

namespace STLCompliance.RoutArr.Auth.Tests;

public sealed class RoutArrAssetDispatchabilityTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::MaintainArr.Api.Program> _maintainarrFactory = null!;
    private WebApplicationFactory<global::RoutArr.Api.Program> _routarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _maintainarrClient = null!;
    private HttpClient _routarrClient = null!;
    private string _routarrToMaintainarrToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"RoutArrDispNexArr-{Guid.NewGuid():N}";
        var maintainArrDbName = $"RoutArrDispMaintainArr-{Guid.NewGuid():N}";
        var routArrDbName = $"RoutArrDisp-{Guid.NewGuid():N}";

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
        var routarrHandoffToken = await IssueServiceTokenAsync(adminToken, "routarr", "launch.redeem");
        _routarrToMaintainarrToken = await IssueServiceTokenAsync(
            adminToken,
            "routarr",
            IntegrationEndpoints.RoutarrAssetReadinessDispatchActionScope,
            ["maintainarr"]);

        var maintainarrHandoffToken = await IssueServiceTokenAsync(adminToken, "maintainarr", "launch.redeem", ["maintainarr"]);

        _maintainarrFactory = new WebApplicationFactory<global::MaintainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
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

        _routarrFactory = new WebApplicationFactory<global::RoutArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", routarrHandoffToken);
            builder.UseSetting("MaintainArr:BaseUrl", _maintainarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("MaintainArr:ServiceToken", _routarrToMaintainarrToken);
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
    public async Task Asset_dispatchability_check_reports_maintainarr_not_ready()
    {
        var assetTag = await SeedNotReadyAssetAsync();
        var dispatcherToken = await RedeemRoutArrTokenAsync();

        var checkRequest = Authorized(HttpMethod.Post, "/api/asset-dispatchability/check", dispatcherToken);
        checkRequest.Content = JsonContent.Create(new AssetDispatchabilityCheckRequest(assetTag, null));
        var checkResponse = await _routarrClient.SendAsync(checkRequest);
        checkResponse.EnsureSuccessStatusCode();
        var check = (await checkResponse.Content.ReadFromJsonAsync<AssetDispatchabilityCheckResponse>())!;

        Assert.Equal(AssetDispatchabilityOutcomes.Block, check.Outcome);
        Assert.Equal("maintainarr_not_ready", check.ReasonCode);
        Assert.True(check.IsBlocking);
        Assert.NotNull(check.MaintainArr);
        Assert.Equal("not_ready", check.MaintainArr!.ReadinessStatus);
    }

    [Fact]
    public async Task Assign_vehicle_blocked_when_maintainarr_not_ready_and_override_succeeds()
    {
        var assetTag = await SeedNotReadyAssetAsync();
        var dispatcherToken = await RedeemRoutArrTokenAsync();
        var now = DateTimeOffset.UtcNow;
        var trip = await CreateTripAsync(dispatcherToken, now.AddHours(2), now.AddHours(6));

        var assignRequest = Authorized(HttpMethod.Patch, $"/api/trips/{trip.TripId}/assign-vehicle", dispatcherToken);
        assignRequest.Content = JsonContent.Create(new AssignTripVehicleRequest(assetTag));
        var blocked = await _routarrClient.SendAsync(assignRequest);
        Assert.Equal(HttpStatusCode.Conflict, blocked.StatusCode);

        var previewRequest = Authorized(HttpMethod.Post, "/api/dispatch/assignments/preview", dispatcherToken);
        previewRequest.Content = JsonContent.Create(new DispatchAssignmentPreviewRequest(
            trip.TripId,
            "vehicle",
            null,
            assetTag));
        var previewResponse = await _routarrClient.SendAsync(previewRequest);
        previewResponse.EnsureSuccessStatusCode();
        var preview = (await previewResponse.Content.ReadFromJsonAsync<DispatchAssignmentPreviewResponse>())!;
        Assert.True(preview.HasBlockingConflicts);
        Assert.NotNull(preview.AssetDispatchability);
        Assert.Equal(AssetDispatchabilityOutcomes.Block, preview.AssetDispatchability!.Outcome);

        assignRequest = Authorized(HttpMethod.Patch, $"/api/trips/{trip.TripId}/assign-vehicle", dispatcherToken);
        assignRequest.Content = JsonContent.Create(new AssignTripVehicleRequest(
            assetTag,
            IgnoreAvailabilityConflicts: false,
            IgnoreDispatchabilityBlocks: true));
        (await _routarrClient.SendAsync(assignRequest)).EnsureSuccessStatusCode();
    }

    private async Task<string> SeedNotReadyAssetAsync()
    {
        var maintainarrToken = await RedeemMaintainArrTokenAsync();
        var asset = await SeedAssetAsync(maintainarrToken);

        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        db.Defects.Add(new Defect
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            AssetId = asset.AssetId,
            Title = "Critical brake fault",
            Description = "Dispatch gate test",
            Severity = DefectSeverities.Critical,
            Status = DefectStatuses.Open,
            Source = DefectSources.Manual,
            ReportedByUserId = PlatformSeeder.DemoAdminUserId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync();

        return asset.AssetTag;
    }

    private async Task<AssetResponse> SeedAssetAsync(string token)
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
            "Dispatch Test Asset",
            string.Empty,
            null));
        var createAssetResponse = await _maintainarrClient.SendAsync(createAssetRequest);
        createAssetResponse.EnsureSuccessStatusCode();
        return (await createAssetResponse.Content.ReadFromJsonAsync<AssetResponse>())!;
    }

    private async Task<TripDetailResponse> CreateTripAsync(
        string dispatcherToken,
        DateTimeOffset tripStart,
        DateTimeOffset tripEnd)
    {
        var createTripRequest = Authorized(HttpMethod.Post, "/api/trips", dispatcherToken);
        createTripRequest.Content = JsonContent.Create(new CreateTripRequest(
            "Dispatchability trip",
            "Asset dispatchability test",
            null,
            tripStart,
            tripEnd,
            null));
        var createTripResponse = await _routarrClient.SendAsync(createTripRequest);
        createTripResponse.EnsureSuccessStatusCode();
        return (await createTripResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;
    }

    private async Task<string> RedeemRoutArrTokenAsync()
    {
        var handoffCode = await CreateRoutArrHandoffAsync();
        var redeemRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/handoff/redeem")
        {
            Content = JsonContent.Create(new RoutArrRedeemRequest(handoffCode)),
        };
        var redeemResponse = await _routarrClient.SendAsync(redeemRequest);
        redeemResponse.EnsureSuccessStatusCode();
        var session = (await redeemResponse.Content.ReadFromJsonAsync<RoutArrHandoffSessionResponse>())!;
        return session.AccessToken;
    }

    private async Task<string> RedeemMaintainArrTokenAsync()
    {
        var handoffCode = await CreateMaintainArrHandoffAsync();
        var redeemResponse = await _maintainarrClient.PostAsJsonAsync(
            "/api/auth/handoff/redeem",
            new MaintainArrRedeemRequest(handoffCode));
        redeemResponse.EnsureSuccessStatusCode();
        var session = (await redeemResponse.Content.ReadFromJsonAsync<MaintainArrHandoffSessionResponse>())!;
        return session.AccessToken;
    }

    private async Task<string> CreateRoutArrHandoffAsync()
    {
        var token = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        var request = Authorized(HttpMethod.Post, "/api/v1/launch/handoff", token);
        request.Content = JsonContent.Create(new CreateHandoffRequest("routarr", "http://localhost:5180/launch"));
        var response = await _nexarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var handoff = (await response.Content.ReadFromJsonAsync<HandoffCreatedResponse>())!;
        return handoff.HandoffCode;
    }

    private async Task<string> CreateMaintainArrHandoffAsync()
    {
        var token = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        var request = Authorized(HttpMethod.Post, "/api/v1/launch/handoff", token);
        request.Content = JsonContent.Create(new CreateHandoffRequest("maintainarr", "http://localhost:5178/launch"));
        var response = await _nexarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var handoff = (await response.Content.ReadFromJsonAsync<HandoffCreatedResponse>())!;
        return handoff.HandoffCode;
    }

    private async Task<string> IssueServiceTokenAsync(
        string adminToken,
        string productKey,
        string actionScope,
        string[]? targetProducts = null)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{productKey}-dispatchability-{Guid.NewGuid():N}",
            $"{productKey} Dispatchability Test",
            productKey,
            targetProducts ?? [productKey]));
        var registerResponse = await _nexarrClient.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/service-tokens", adminToken);
        issueRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            null,
            actionScope,
            30));
        var issueResponse = await _nexarrClient.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        var issued = (await issueResponse.Content.ReadFromJsonAsync<ServiceTokenIssueResponse>())!;
        return issued.AccessToken;
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

    private static void RemoveDbContext<TContext>(IServiceCollection services)
        where TContext : DbContext
    {
        var descriptors = services
            .Where(d => d.ServiceType == typeof(DbContextOptions<TContext>)
                || d.ServiceType == typeof(TContext))
            .ToList();
        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string path, string token)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }
}
