using STLCompliance.Shared.Integration;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Services;
using MaintainArrRedeemRequest = MaintainArr.Api.Contracts.RedeemHandoffRequest;
using MaintainArrHandoffSessionResponse = MaintainArr.Api.Contracts.HandoffSessionResponse;
using CreateAssetClassRequest = MaintainArr.Api.Contracts.CreateAssetClassRequest;
using CreateAssetRequest = MaintainArr.Api.Contracts.CreateAssetRequest;
using CreateAssetTypeRequest = MaintainArr.Api.Contracts.CreateAssetTypeRequest;
using CreateWorkOrderRequest = MaintainArr.Api.Contracts.CreateWorkOrderRequest;
using NexArr.Api.Data;
using NexArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace STLCompliance.MaintainArr.Auth.Tests;

public sealed class MaintainArrEntityBulkExportTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::MaintainArr.Api.Program> _maintainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _maintainarrClient = null!;
    private string _managerToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"EntityExportNexArr-{Guid.NewGuid():N}";
        var maintainArrDbName = $"EntityExportMaintainArr-{Guid.NewGuid():N}";

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
        var serviceToken = await IssueServiceTokenAsync(adminToken, "maintainarr");

        _maintainarrFactory = new WebApplicationFactory<global::MaintainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", serviceToken);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<MaintainArrDbContext>(services);
                services.AddDbContext<MaintainArrDbContext>(options => options.UseInMemoryDatabase(maintainArrDbName));
                services.AddHttpClient<StlNexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarrFactory.Server.CreateHandler());
            });
        });

        _maintainarrClient = _maintainarrFactory.CreateClient();
        _managerToken = await RedeemMaintainArrTokenAsync();
        await SeedAssetAndWorkOrderAsync(_managerToken);
    }

    public async Task DisposeAsync()
    {
        _maintainarrClient.Dispose();
        _nexarrClient.Dispose();
        await _maintainarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Entity_export_manifest_lists_entities_and_reports()
    {
        var response = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/exports/manifest", _managerToken));
        response.EnsureSuccessStatusCode();

        var manifest = (await response.Content.ReadFromJsonAsync<EntityExportManifestResponse>())!;
        Assert.Contains(manifest.Entities, x => x.EntityKey == "assets");
        Assert.Contains(manifest.Entities, x => x.EntityKey == "work_orders");
        Assert.Contains(manifest.Entities, x => x.EntityKey == "inspection_runs");
        Assert.Contains(manifest.ReportExports, x => x.ReportKey == "maintenance");
        Assert.Contains(manifest.ReportExports, x => x.ReportKey == "executive");
        Assert.Contains(manifest.ReportExports, x => x.ReportKey == "compliance");
    }

    [Fact]
    public async Task Entity_export_assets_csv_includes_seeded_asset()
    {
        var response = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/exports/assets", _managerToken));
        response.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);

        var csv = await response.Content.ReadAsStringAsync();
        Assert.Contains(EntityBulkExportService.AssetsCsvHeader, csv, StringComparison.Ordinal);
        Assert.Contains("Export Test Asset", csv, StringComparison.Ordinal);
        var dataLines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        Assert.True(dataLines.Length >= 2, "Expected header plus at least one asset row.");
    }

    [Fact]
    public async Task Entity_export_work_orders_csv_includes_seeded_work_order()
    {
        var response = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/exports/work-orders", _managerToken));
        response.EnsureSuccessStatusCode();

        var csv = await response.Content.ReadAsStringAsync();
        Assert.Contains(EntityBulkExportService.WorkOrdersCsvHeader, csv, StringComparison.Ordinal);
        Assert.Contains("Export test work order", csv, StringComparison.Ordinal);
        var dataLines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        Assert.True(dataLines.Length >= 2, "Expected header plus at least one work order row.");
    }

    [Fact]
    public async Task Entity_export_inspection_runs_csv_returns_header_when_empty()
    {
        var response = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/exports/inspection-runs", _managerToken));
        response.EnsureSuccessStatusCode();

        var csv = await response.Content.ReadAsStringAsync();
        Assert.Contains(EntityBulkExportService.InspectionRunsCsvHeader, csv, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Entity_export_v1_alias_manifest_and_csv_endpoints_work()
    {
        var manifestResponse = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/exports/manifest", _managerToken));
        manifestResponse.EnsureSuccessStatusCode();
        var manifest = (await manifestResponse.Content.ReadFromJsonAsync<EntityExportManifestResponse>())!;
        Assert.Contains(manifest.Entities, x => x.EntityKey == "assets" && x.Route == "/api/v1/exports/assets");
        Assert.Contains(manifest.ReportExports, x => x.ReportKey == "maintenance" && x.Route == "/api/v1/reports/maintenance/summary/export");

        var assetsResponse = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/exports/assets", _managerToken));
        assetsResponse.EnsureSuccessStatusCode();
        var assetsCsv = await assetsResponse.Content.ReadAsStringAsync();
        Assert.Contains(EntityBulkExportService.AssetsCsvHeader, assetsCsv, StringComparison.Ordinal);

        var workOrdersResponse = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/exports/work-orders", _managerToken));
        workOrdersResponse.EnsureSuccessStatusCode();
        var workOrdersCsv = await workOrdersResponse.Content.ReadAsStringAsync();
        Assert.Contains(EntityBulkExportService.WorkOrdersCsvHeader, workOrdersCsv, StringComparison.Ordinal);

        var inspectionRunsResponse = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/exports/inspection-runs", _managerToken));
        inspectionRunsResponse.EnsureSuccessStatusCode();
        var inspectionRunsCsv = await inspectionRunsResponse.Content.ReadAsStringAsync();
        Assert.Contains(EntityBulkExportService.InspectionRunsCsvHeader, inspectionRunsCsv, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Exports_v1_index_lists_manifest_and_entity_export_paths()
    {
        var response = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/exports", _managerToken));
        response.EnsureSuccessStatusCode();

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var paths = json.RootElement.GetProperty("items")
            .EnumerateArray()
            .Select(x => x.GetProperty("path").GetString())
            .ToList();

        Assert.Contains("/api/v1/exports/manifest", paths);
        Assert.Contains("/api/v1/exports/assets", paths);
        Assert.Contains("/api/v1/exports/work-orders", paths);
        Assert.Contains("/api/v1/exports/inspection-runs", paths);
    }

    [Fact]
    public async Task Entity_export_denies_unauthenticated()
    {
        var response = await _maintainarrClient.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, "/api/exports/assets"));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task SeedAssetAndWorkOrderAsync(string token)
    {
        var assetTypeId = await SeedAssetTypeAsync(token);
        var assetTag = $"EXP-{Guid.NewGuid():N}".Substring(0, 10);

        var createAssetRequest = Authorized(HttpMethod.Post, "/api/assets", token);
        createAssetRequest.Content = JsonContent.Create(new CreateAssetRequest(
            assetTypeId,
            assetTag,
            "Export Test Asset",
            string.Empty,
            null));
        var createAssetResponse = await _maintainarrClient.SendAsync(createAssetRequest);
        createAssetResponse.EnsureSuccessStatusCode();
        var asset = (await createAssetResponse.Content.ReadFromJsonAsync<AssetResponse>())!;

        var createWorkOrderRequest = Authorized(HttpMethod.Post, "/api/work-orders", token);
        createWorkOrderRequest.Content = JsonContent.Create(new CreateWorkOrderRequest(
            asset.AssetId,
            "Export test work order",
            "Used by entity export tests",
            "medium",
            null,
            null));
        (await _maintainarrClient.SendAsync(createWorkOrderRequest)).EnsureSuccessStatusCode();
    }

    private async Task<Guid> SeedAssetTypeAsync(string token)
    {
        var createClassRequest = Authorized(HttpMethod.Post, "/api/asset-classes", token);
        createClassRequest.Content = JsonContent.Create(new CreateAssetClassRequest(
            $"class-{Guid.NewGuid():N}".Substring(0, 10),
            "Export Class",
            string.Empty));
        var createClassResponse = await _maintainarrClient.SendAsync(createClassRequest);
        createClassResponse.EnsureSuccessStatusCode();
        var assetClass = (await createClassResponse.Content.ReadFromJsonAsync<AssetClassResponse>())!;

        var createTypeRequest = Authorized(HttpMethod.Post, "/api/asset-types", token);
        createTypeRequest.Content = JsonContent.Create(new CreateAssetTypeRequest(
            assetClass.AssetClassId,
            $"type-{Guid.NewGuid():N}".Substring(0, 10),
            "Export Type",
            string.Empty));
        var createTypeResponse = await _maintainarrClient.SendAsync(createTypeRequest);
        createTypeResponse.EnsureSuccessStatusCode();
        var assetType = (await createTypeResponse.Content.ReadFromJsonAsync<AssetTypeResponse>())!;
        return assetType.AssetTypeId;
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
        request.Content = JsonContent.Create(new NexArr.Api.Contracts.CreateHandoffRequest(
            "maintainarr",
            "http://localhost:5178/launch"));
        var response = await _nexarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var handoff = (await response.Content.ReadFromJsonAsync<NexArr.Api.Contracts.HandoffCreatedResponse>())!;
        return handoff.HandoffCode;
    }

    private async Task<string> IssueServiceTokenAsync(string adminToken, string productKey)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new NexArr.Api.Contracts.RegisterServiceClientRequest(
            $"{productKey}-entity-export-test",
            $"{productKey} entity export test",
            productKey,
            [productKey]));
        var registerResponse = await _nexarrClient.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<NexArr.Api.Contracts.ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/service-tokens", adminToken);
        issueRequest.Content = JsonContent.Create(new NexArr.Api.Contracts.IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            null,
            "launch.redeem",
            30));
        var issueResponse = await _nexarrClient.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        var issued = (await issueResponse.Content.ReadFromJsonAsync<NexArr.Api.Contracts.ServiceTokenIssueResponse>())!;
        return issued.AccessToken;
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
        foreach (var descriptor in services
                     .Where(d => d.ServiceType == typeof(DbContextOptions<TContext>) || d.ServiceType == typeof(TContext))
                     .ToList())
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
