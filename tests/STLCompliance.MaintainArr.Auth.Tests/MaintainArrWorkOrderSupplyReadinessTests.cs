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
using MaintainArr.Api.Services;
using MaintainArrIntegration = MaintainArr.Api.Endpoints.IntegrationEndpoints;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Services;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using SupplyArr.Api.Services;
using SupplyArrIntegration = SupplyArr.Api.Endpoints.IntegrationEndpoints;
using MaintainArrRedeemRequest = MaintainArr.Api.Contracts.RedeemHandoffRequest;
using MaintainArrHandoffSessionResponse = MaintainArr.Api.Contracts.HandoffSessionResponse;
using AssetResponse = MaintainArr.Api.Contracts.AssetResponse;
using CreateAssetRequest = MaintainArr.Api.Contracts.CreateAssetRequest;
using AssetClassResponse = MaintainArr.Api.Contracts.AssetClassResponse;
using CreateAssetClassRequest = MaintainArr.Api.Contracts.CreateAssetClassRequest;
using AssetTypeResponse = MaintainArr.Api.Contracts.AssetTypeResponse;
using CreateAssetTypeRequest = MaintainArr.Api.Contracts.CreateAssetTypeRequest;
using WorkOrderDetailResponse = MaintainArr.Api.Contracts.WorkOrderDetailResponse;
using SupplyArrRedeemRequest = SupplyArr.Api.Contracts.RedeemHandoffRequest;
using SupplyArrHandoffSessionResponse = SupplyArr.Api.Contracts.HandoffSessionResponse;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Integration;

namespace STLCompliance.MaintainArr.Auth.Tests;

public sealed class MaintainArrWorkOrderSupplyReadinessTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::MaintainArr.Api.Program> _maintainarrFactory = null!;
    private WebApplicationFactory<global::SupplyArr.Api.Program> _supplyarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _maintainarrClient = null!;
    private HttpClient _supplyarrClient = null!;
    private string _supplyarrIntegrationToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"MaintainWoReadinessNexArr-{Guid.NewGuid():N}";
        var maintainArrDbName = $"MaintainWoReadinessMaintainArr-{Guid.NewGuid():N}";
        var supplyArrDbName = $"MaintainWoReadinessSupplyArr-{Guid.NewGuid():N}";

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
        _supplyarrIntegrationToken = await IssueServiceTokenAsync(
            adminToken,
            "maintainarr",
            ["supplyarr"],
            $"{SupplyArrIntegration.MaintainarrDemandIngestActionScope},{SupplyArrIntegration.SupplyReadinessReadActionScope}");

        var maintainarrHandoffToken = await IssueServiceTokenAsync(adminToken, "maintainarr", ["maintainarr"], "launch.redeem");
        var supplyarrHandoffToken = await IssueServiceTokenAsync(adminToken, "supplyarr", ["supplyarr"], "launch.redeem");
        var maintainarrStatusCallbackToken = await IssueServiceTokenAsync(
            adminToken,
            "supplyarr",
            ["maintainarr"],
            MaintainArrIntegration.SupplyarrDemandStatusIngestActionScope);

        WebApplicationFactory<global::SupplyArr.Api.Program>? supplyarrFactoryRef = null;

        _maintainarrFactory = new WebApplicationFactory<global::MaintainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", maintainarrHandoffToken);
            builder.UseSetting("SupplyArr:BaseUrl", "http://localhost");
            builder.UseSetting("SupplyArr:ServiceToken", _supplyarrIntegrationToken);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<MaintainArrDbContext>(services);
                services.AddDbContext<MaintainArrDbContext>(options => options.UseInMemoryDatabase(maintainArrDbName));

                services.AddHttpClient<StlNexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarrFactory.Server.CreateHandler());
                services.AddHttpClient<SupplyArrDemandClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => supplyarrFactoryRef!.Server.CreateHandler());
                services.AddHttpClient<SupplyArrSupplyReadinessClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => supplyarrFactoryRef!.Server.CreateHandler());
            });
        });

        _supplyarrFactory = new WebApplicationFactory<global::SupplyArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", supplyarrHandoffToken);
            builder.UseSetting("MaintainArr:BaseUrl", _maintainarrFactory.Server.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("MaintainArr:ServiceToken", maintainarrStatusCallbackToken);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<SupplyArrDbContext>(services);
                services.AddDbContext<SupplyArrDbContext>(options => options.UseInMemoryDatabase(supplyArrDbName));

                services.AddHttpClient<StlNexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarrFactory.Server.CreateHandler());
            });
        });

        supplyarrFactoryRef = _supplyarrFactory;
        _maintainarrClient = _maintainarrFactory.CreateClient();
        _supplyarrClient = _supplyarrFactory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _maintainarrClient.Dispose();
        _supplyarrClient.Dispose();
        _nexarrClient.Dispose();
        await _maintainarrFactory.DisposeAsync();
        await _supplyarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public void ResolveOverallStatus_returns_expected_values()
    {
        Assert.Equal("no_demand", WorkOrderSupplyReadinessService.ResolveOverallStatus(0, 0, 0, 0, 0));
        Assert.Equal("unknown", WorkOrderSupplyReadinessService.ResolveOverallStatus(2, 0, 0, 0, 2));
        Assert.Equal("not_ready", WorkOrderSupplyReadinessService.ResolveOverallStatus(2, 2, 1, 1, 0));
        Assert.Equal("ready", WorkOrderSupplyReadinessService.ResolveOverallStatus(2, 2, 2, 0, 0));
    }

    [Fact]
    public async Task Work_order_supply_readiness_returns_not_ready_for_stockout_part()
    {
        var maintainarrToken = await RedeemMaintainArrTokenAsync();
        var supplyarrToken = await RedeemSupplyArrTokenAsync();
        var partId = await SeedSupplyArrPartWithStockAsync(supplyarrToken, quantityOnHand: 0m, reorderPoint: 10m);
        var assetId = await SeedAssetOnlyAsync(maintainarrToken);
        var workOrderId = await CreateOpenWorkOrderAsync(maintainarrToken, assetId);

        var createLineRequest = Authorized(HttpMethod.Post, $"/api/work-orders/{workOrderId}/parts-demand", maintainarrToken);
        createLineRequest.Content = JsonContent.Create(new CreateWorkOrderPartsDemandLineRequest(
            partId,
            "BRK-001",
            "Front brake pads",
            2m,
            "each",
            null));
        (await _maintainarrClient.SendAsync(createLineRequest)).EnsureSuccessStatusCode();

        var readinessRequest = Authorized(
            HttpMethod.Get,
            $"/api/work-orders/{workOrderId}/supply-readiness",
            maintainarrToken);
        var readinessResponse = await _maintainarrClient.SendAsync(readinessRequest);
        readinessResponse.EnsureSuccessStatusCode();

        var readiness = (await readinessResponse.Content.ReadFromJsonAsync<WorkOrderSupplyReadinessResponse>())!;
        Assert.Equal("not_ready", readiness.OverallReadinessStatus);
        Assert.Equal(1, readiness.LinesChecked);
        Assert.Equal(0, readiness.LinesReady);
        Assert.Equal(1, readiness.LinesBlocked);
        var line = Assert.Single(readiness.Lines);
        Assert.Equal("not_ready", line.ReadinessStatus);
        Assert.Contains(line.Blockers, x => x.ReasonCode == SupplyReadinessReasonCodes.PartStockout);
    }

    [Fact]
    public async Task Work_order_supply_readiness_skips_lines_without_supplyarr_part_id()
    {
        var maintainarrToken = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetOnlyAsync(maintainarrToken);
        var workOrderId = await CreateOpenWorkOrderAsync(maintainarrToken, assetId);

        var createLineRequest = Authorized(HttpMethod.Post, $"/api/work-orders/{workOrderId}/parts-demand", maintainarrToken);
        createLineRequest.Content = JsonContent.Create(new CreateWorkOrderPartsDemandLineRequest(
            null,
            "FREE-TEXT",
            "Free text part",
            1m,
            "each",
            null));
        (await _maintainarrClient.SendAsync(createLineRequest)).EnsureSuccessStatusCode();

        var readinessRequest = Authorized(
            HttpMethod.Get,
            $"/api/work-orders/{workOrderId}/supply-readiness",
            maintainarrToken);
        var readinessResponse = await _maintainarrClient.SendAsync(readinessRequest);
        readinessResponse.EnsureSuccessStatusCode();

        var readiness = (await readinessResponse.Content.ReadFromJsonAsync<WorkOrderSupplyReadinessResponse>())!;
        Assert.Equal("unknown", readiness.OverallReadinessStatus);
        Assert.Equal(0, readiness.LinesChecked);
        Assert.Equal(1, readiness.LinesSkipped);
        var line = Assert.Single(readiness.Lines);
        Assert.Equal("missing_supplyarr_part_id", line.SkipReason);
    }

    [Fact]
    public async Task Work_order_supply_readiness_requires_authorization()
    {
        var response = await _maintainarrClient.GetAsync($"/api/work-orders/{Guid.NewGuid()}/supply-readiness");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<Guid> SeedSupplyArrPartWithStockAsync(
        string token,
        decimal quantityOnHand,
        decimal reorderPoint)
    {
        var createPartRequest = Authorized(HttpMethod.Post, "/api/parts", token);
        createPartRequest.Content = JsonContent.Create(new CreatePartRequest(
            $"part-{Guid.NewGuid():N}".Substring(0, 12),
            null,
            "Readiness Test Part",
            string.Empty,
            "general",
            "each",
            "Acme",
            "AC-100"));
        var createPartResponse = await _supplyarrClient.SendAsync(createPartRequest);
        createPartResponse.EnsureSuccessStatusCode();
        var part = (await createPartResponse.Content.ReadFromJsonAsync<PartResponse>())!;

        using var scope = _supplyarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var entity = await db.Parts.FirstAsync(x => x.Id == part.PartId);
        entity.ReorderPoint = reorderPoint;
        db.PartStockLevels.Add(new PartStockLevel
        {
            Id = Guid.NewGuid(),
            TenantId = entity.TenantId,
            PartId = entity.Id,
            InventoryBinId = Guid.NewGuid(),
            QuantityOnHand = quantityOnHand,
            QuantityReserved = 0m,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync();

        return part.PartId;
    }

    private async Task<Guid> SeedAssetOnlyAsync(string token)
    {
        var assetTypeId = await SeedAssetTypeAsync(token);

        var createAssetRequest = Authorized(HttpMethod.Post, "/api/assets", token);
        createAssetRequest.Content = JsonContent.Create(new CreateAssetRequest(
            assetTypeId,
            $"READINESS-ASSET-{Guid.NewGuid():N}".Substring(0, 12),
            "Readiness Test Asset",
            string.Empty,
            null));
        var createAssetResponse = await _maintainarrClient.SendAsync(createAssetRequest);
        createAssetResponse.EnsureSuccessStatusCode();
        var asset = (await createAssetResponse.Content.ReadFromJsonAsync<AssetResponse>())!;
        return asset.AssetId;
    }

    private async Task<Guid> SeedAssetTypeAsync(string token)
    {
        var createClassRequest = Authorized(HttpMethod.Post, "/api/asset-classes", token);
        createClassRequest.Content = JsonContent.Create(new CreateAssetClassRequest(
            $"class-{Guid.NewGuid():N}".Substring(0, 12),
            "Equipment",
            string.Empty));
        var createClassResponse = await _maintainarrClient.SendAsync(createClassRequest);
        createClassResponse.EnsureSuccessStatusCode();
        var assetClass = (await createClassResponse.Content.ReadFromJsonAsync<AssetClassResponse>())!;

        var createTypeRequest = Authorized(HttpMethod.Post, "/api/asset-types", token);
        createTypeRequest.Content = JsonContent.Create(new CreateAssetTypeRequest(
            assetClass.AssetClassId,
            $"type-{Guid.NewGuid():N}".Substring(0, 12),
            "Machine",
            string.Empty));
        var createTypeResponse = await _maintainarrClient.SendAsync(createTypeRequest);
        createTypeResponse.EnsureSuccessStatusCode();
        var assetType = (await createTypeResponse.Content.ReadFromJsonAsync<AssetTypeResponse>())!;
        return assetType.AssetTypeId;
    }

    private async Task<Guid> CreateOpenWorkOrderAsync(string token, Guid assetId)
    {
        var createRequest = Authorized(HttpMethod.Post, "/api/work-orders", token);
        createRequest.Content = JsonContent.Create(new CreateWorkOrderRequest(
            assetId,
            "Supply readiness WO",
            string.Empty,
            "medium",
            null,
            null));
        var createResponse = await _maintainarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<WorkOrderDetailResponse>())!;
        return created.WorkOrderId;
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

    private async Task<string> RedeemSupplyArrTokenAsync()
    {
        var handoffCode = await CreateHandoffAsync("supplyarr", "http://localhost:5179/launch");
        var redeemResponse = await _supplyarrClient.PostAsJsonAsync(
            "/api/auth/handoff/redeem",
            new SupplyArrRedeemRequest(handoffCode));
        redeemResponse.EnsureSuccessStatusCode();
        var session = (await redeemResponse.Content.ReadFromJsonAsync<SupplyArrHandoffSessionResponse>())!;
        return session.AccessToken;
    }

    private async Task<string> CreateHandoffAsync(string productKey, string callbackUrl)
    {
        var token = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        var request = Authorized(HttpMethod.Post, "/api/launch/handoff", token);
        request.Content = JsonContent.Create(new CreateHandoffRequest(productKey, callbackUrl));
        var response = await _nexarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var handoff = (await response.Content.ReadFromJsonAsync<HandoffCreatedResponse>())!;
        return handoff.HandoffCode;
    }

    private async Task<string> IssueServiceTokenAsync(
        string adminToken,
        string sourceProduct,
        IReadOnlyList<string> allowedProducts,
        string actionScope)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{sourceProduct}-readiness-test-{Guid.NewGuid():N}",
            $"{sourceProduct} readiness test",
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

    private static void RemoveDbContext<TContext>(IServiceCollection services) where TContext : DbContext
    {
        var descriptors = services
            .Where(d => d.ServiceType == typeof(DbContextOptions<TContext>) || d.ServiceType == typeof(TContext))
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
