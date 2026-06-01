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
using MaintainArr.Api.Entities;
using MaintainArr.Api.Services;
using MaintainArrRedeemRequest = MaintainArr.Api.Contracts.RedeemHandoffRequest;
using MaintainArrHandoffSessionResponse = MaintainArr.Api.Contracts.HandoffSessionResponse;
using NexArr.Api.Data;
using NexArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace STLCompliance.MaintainArr.Auth.Tests;

public sealed class MaintainArrAssetBulkImportTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::MaintainArr.Api.Program> _maintainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _maintainarrClient = null!;
    private string _managerToken = null!;
    private const string _classKey = "powered_industrial_truck";
    private const string _typeKey = "forklift";

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"AssetImportNexArr-{Guid.NewGuid():N}";
        var maintainArrDbName = $"AssetImportMaintainArr-{Guid.NewGuid():N}";

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
    }

    public async Task DisposeAsync()
    {
        _maintainarrClient.Dispose();
        _nexarrClient.Dispose();
        await _maintainarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Asset_import_validate_does_not_persist()
    {
        var tag = $"IMP-{Guid.NewGuid():N}".Substring(0, 12);
        var request = Authorized(HttpMethod.Post, "/api/imports/assets/validate", _managerToken);
        request.Content = JsonContent.Create(new AssetBulkImportRequest(
        [
            new AssetImportRowRequest(_classKey, _typeKey, tag, "Import Test Asset"),
        ]));

        var response = await _maintainarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var result = (await response.Content.ReadFromJsonAsync<AssetBulkImportResponse>())!;
        Assert.True(result.DryRun);
        Assert.Equal(1, result.SuccessCount);
        Assert.Equal("validated", result.Results[0].Status);

        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        Assert.False(await db.Assets.AnyAsync(x => x.AssetTag == tag));
        Assert.True(await db.MaintainArrImportBatches.AnyAsync(x => x.Id == result.ImportBatchId));
    }

    [Fact]
    public async Task Asset_import_commit_creates_assets()
    {
        var tag = $"IMP-{Guid.NewGuid():N}".Substring(0, 12);
        var request = Authorized(HttpMethod.Post, "/api/imports/assets/commit", _managerToken);
        request.Content = JsonContent.Create(new AssetBulkImportRequest(
        [
            new AssetImportRowRequest(_classKey, _typeKey, tag, "Committed Import Asset", "desc", "site_a", "active"),
        ]));

        var response = await _maintainarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var result = (await response.Content.ReadFromJsonAsync<AssetBulkImportResponse>())!;
        Assert.False(result.DryRun);
        Assert.Equal(1, result.SuccessCount);
        Assert.Equal("created", result.Results[0].Status);

        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        Assert.True(await db.Assets.AnyAsync(x => x.AssetTag == tag.ToUpperInvariant()));
    }

    [Fact]
    public async Task Asset_import_v1_validate_does_not_persist()
    {
        var tag = $"IMPV1-{Guid.NewGuid():N}".Substring(0, 12);
        var request = Authorized(HttpMethod.Post, "/api/v1/imports/assets/validate", _managerToken);
        request.Content = JsonContent.Create(new AssetBulkImportRequest(
        [
            new AssetImportRowRequest(_classKey, _typeKey, tag, "Import V1 Test Asset"),
        ]));

        var response = await _maintainarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var result = (await response.Content.ReadFromJsonAsync<AssetBulkImportResponse>())!;
        Assert.True(result.DryRun);
        Assert.Equal(1, result.SuccessCount);
        Assert.Equal("validated", result.Results[0].Status);

        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        Assert.False(await db.Assets.AnyAsync(x => x.AssetTag == tag));
    }

    [Fact]
    public async Task Imports_v1_index_lists_validate_and_commit_paths()
    {
        var response = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/imports", _managerToken));
        response.EnsureSuccessStatusCode();

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var paths = json.RootElement.GetProperty("items")
            .EnumerateArray()
            .Select(x => x.GetProperty("path").GetString())
            .ToList();

        Assert.Contains("/api/v1/imports/assets/validate", paths);
        Assert.Contains("/api/v1/imports/assets/commit", paths);
    }

    [Fact]
    public async Task Asset_import_reports_duplicate_tag_in_batch()
    {
        var tag = $"DUP-{Guid.NewGuid():N}".Substring(0, 12);
        var request = Authorized(HttpMethod.Post, "/api/imports/assets/validate", _managerToken);
        request.Content = JsonContent.Create(new AssetBulkImportRequest(
        [
            new AssetImportRowRequest(_classKey, _typeKey, tag, "First"),
            new AssetImportRowRequest(_classKey, _typeKey, tag, "Second"),
        ]));

        var response = await _maintainarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var result = (await response.Content.ReadFromJsonAsync<AssetBulkImportResponse>())!;
        Assert.Equal(1, result.ErrorCount);
        Assert.Contains(result.Results, x => x.ErrorCode == "assets.duplicate_tag");
    }

    [Fact]
    public async Task Asset_import_denies_unauthenticated()
    {
        var response = await _maintainarrClient.SendAsync(
            new HttpRequestMessage(HttpMethod.Post, "/api/imports/assets/validate"));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Asset_import_normalizes_alias_values_on_commit()
    {
        var tag = $"ALIAS-{Guid.NewGuid():N}".Substring(0, 12);
        var request = Authorized(HttpMethod.Post, "/api/imports/assets/commit", _managerToken);
        request.Content = JsonContent.Create(new AssetBulkImportRequest(
        [
            new AssetImportRowRequest
            {
                AssetTag = tag,
                Name = "Alias Normalized Asset",
                Description = "alias normalization coverage",
                Values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["assetClass"] = "vehicle",
                    ["assetType"] = "semi_tractor",
                    ["make"] = "Frtlnr",
                    ["model"] = "Freightliner Cascadia",
                    ["brakeType"] = "air disc",
                    ["tireConfiguration"] = "wide-base singles",
                    ["fuelType"] = "compressed natural gas",
                    ["trailerType"] = "refrigerated trailer",
                    ["assetStatus"] = "active",
                    ["criticality"] = "medium",
                    ["lifecycleStatus"] = "in_service",
                },
            },
        ]));

        var response = await _maintainarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var result = (await response.Content.ReadFromJsonAsync<AssetBulkImportResponse>())!;
        Assert.Equal(1, result.SuccessCount);
        Assert.Equal("created", result.Results[0].Status);

        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        var asset = await db.Assets.SingleAsync(x => x.AssetTag == tag.ToUpperInvariant());
        var fields = await db.AssetCustomFieldValues
            .Where(x => x.AssetId == asset.Id)
            .ToDictionaryAsync(x => x.FieldKey, x => x.ValueJson);

        Assert.Equal("\"freightliner\"", fields["make"]);
        Assert.Equal("\"cascadia\"", fields["model"]);
        Assert.Equal("\"disc\"", fields["brakeType"]);
        Assert.Equal("\"super_single\"", fields["tireConfiguration"]);
        Assert.Equal("\"cng\"", fields["fuelType"]);
        Assert.Equal("\"reefer\"", fields["trailerType"]);
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
            $"{productKey}-asset-import-test",
            $"{productKey} asset import test",
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
