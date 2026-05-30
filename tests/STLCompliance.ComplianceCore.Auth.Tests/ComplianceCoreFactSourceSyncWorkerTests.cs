using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using ComplianceCore.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Services;

namespace STLCompliance.ComplianceCore.Auth.Tests;

public sealed class ComplianceCoreFactSourceSyncWorkerTests : IAsyncLifetime
{
    private const string SigningKey = "test-signing-key-at-least-32-chars-long";

    private WebApplicationFactory<global::ComplianceCore.Api.Program> _complianceCoreFactory = null!;
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private HttpClient _complianceCoreClient = null!;
    private HttpClient _nexarrClient = null!;
    private string _workerToken = null!;

    public async Task InitializeAsync()
    {
        var complianceDbName = $"ComplianceCoreFactSync-{Guid.NewGuid():N}";
        var nexarrDbName = $"NexArrFactSync-{Guid.NewGuid():N}";

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
                services.AddDbContext<NexArrDbContext>(options => options.UseInMemoryDatabase(nexarrDbName));
            });
        });

        _complianceCoreFactory = new WebApplicationFactory<global::ComplianceCore.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", SigningKey);
            builder.UseSetting("ServiceToken:SigningKey", SigningKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<ComplianceCoreDbContext>(services);
                services.AddDbContext<ComplianceCoreDbContext>(options => options.UseInMemoryDatabase(complianceDbName));
            });
        });

        _nexarrClient = _nexarrFactory.CreateClient();
        _complianceCoreClient = _complianceCoreFactory.CreateClient();

        await SeedNexArrAsync();

        using var scope = _complianceCoreFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        await db.Database.EnsureCreatedAsync();
        var vocabularyService = scope.ServiceProvider.GetRequiredService<VocabularyService>();
        await vocabularyService.EnsureVocabularyTypesSeededAsync();

        var adminToken = await LoginNexArrAdminAsync();
        _workerToken = await IssueServiceTokenAsync(
            adminToken,
            FactSourceSyncWorkerService.ProcessBatchActionScope);
    }

    public async Task DisposeAsync()
    {
        _complianceCoreClient.Dispose();
        _nexarrClient.Dispose();
        await _complianceCoreFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Process_batch_rejects_missing_service_token()
    {
        var response = await _complianceCoreClient.PostAsJsonAsync(
            "/api/internal/fact-source-sync/process-batch",
            new ProcessFactSourceSyncsRequest(PlatformSeeder.DemoTenantId, null, null, null));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Product_api_source_requires_sync_config()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var factId = await CreateBooleanFactAsync(adminToken, "sync_config_required");

        var createRequest = Authorized(HttpMethod.Post, "/api/fact-sources", adminToken);
        createRequest.Content = JsonContent.Create(new CreateFactSourceRequest(
            factId,
            "missing_sync_config",
            FactSourceTypes.ProductApi,
            "Missing config",
            "Should fail validation.",
            "staffarr",
            null,
            "{}",
            0));
        var response = await _complianceCoreClient.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Settings_manifest_v1_requires_admin_and_lists_setting_groups()
    {
        var reviewerToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_reviewer");
        var forbiddenResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/settings", reviewerToken));
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenResponse.StatusCode);

        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var manifestResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/settings", adminToken));
        manifestResponse.EnsureSuccessStatusCode();
        var manifest = (await manifestResponse.Content.ReadFromJsonAsync<ComplianceCoreSettingsManifestResponse>())!;
        Assert.Contains(manifest.Items, x => x.SettingKey == "fact_source_sync_worker_settings");
        Assert.Contains(manifest.Items, x => x.SettingKey == "m12_analytics_worker_settings");
    }

    [Fact]
    public async Task Config_manifest_v1_requires_admin_and_matches_settings_manifest()
    {
        var reviewerToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_reviewer");
        var forbiddenResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/config", reviewerToken));
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenResponse.StatusCode);

        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var configResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/config", adminToken));
        configResponse.EnsureSuccessStatusCode();
        var configManifest = (await configResponse.Content.ReadFromJsonAsync<ComplianceCoreSettingsManifestResponse>())!;

        var settingsResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/settings", adminToken));
        settingsResponse.EnsureSuccessStatusCode();
        var settingsManifest = (await settingsResponse.Content.ReadFromJsonAsync<ComplianceCoreSettingsManifestResponse>())!;

        Assert.Equal(settingsManifest.Items.Count, configManifest.Items.Count);
        foreach (var item in settingsManifest.Items)
        {
            Assert.Contains(configManifest.Items, x => x.SettingKey == item.SettingKey);
        }
    }

    [Fact]
    public async Task Snapshot_sync_caches_product_api_fact_and_internal_resolve_uses_cache()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var factId = await CreateBooleanFactAsync(adminToken, "sync_cached_cert");
        await CreateProductApiSnapshotSourceAsync(adminToken, factId, "sync_cached_cert_source");

        var putRequest = Authorized(HttpMethod.Put, "/api/fact-source-sync-worker-settings", adminToken);
        putRequest.Content = JsonContent.Create(new UpsertFactSourceSyncWorkerSettingsRequest(
            true,
            "tenant",
            60));
        (await _complianceCoreClient.SendAsync(putRequest)).EnsureSuccessStatusCode();

        var processRequest = ServiceAuthorized(
            HttpMethod.Post,
            "/api/internal/fact-source-sync/process-batch",
            _workerToken);
        processRequest.Content = JsonContent.Create(new ProcessFactSourceSyncsRequest(
            PlatformSeeder.DemoTenantId,
            null,
            60,
            10));
        var processResponse = await _complianceCoreClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var batch = (await processResponse.Content.ReadFromJsonAsync<ProcessFactSourceSyncsResponse>())!;
        Assert.Equal(1, batch.SucceededCount);
        Assert.Equal("succeeded", batch.Results[0].Status);

        var healthResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/fact-source-sync-health", adminToken));
        healthResponse.EnsureSuccessStatusCode();
        var health = (await healthResponse.Content.ReadFromJsonAsync<FactSourceSyncHealthResponse>())!;
        Assert.Equal(1, health.ProductApiSourceCount);
        Assert.Equal(1, health.HealthyCount);

        var nexarrAdminToken = await LoginNexArrAdminAsync();
        var resolveToken = await IssueResolveTokenAsync(nexarrAdminToken);
        var resolveRequest = ServiceAuthorized(HttpMethod.Post, "/api/internal/resolve", resolveToken);
        resolveRequest.Content = JsonContent.Create(new InternalResolveFactsRequest(
            PlatformSeeder.DemoTenantId,
            ["sync_cached_cert"],
            null));
        var resolveResponse = await _complianceCoreClient.SendAsync(resolveRequest);
        resolveResponse.EnsureSuccessStatusCode();
        var resolved = (await resolveResponse.Content.ReadFromJsonAsync<InternalResolveFactsResponse>())!;
        Assert.Empty(resolved.UnresolvedFactKeys);
        Assert.False(resolved.Resolved[0].FromContext);
        Assert.True(resolved.Resolved[0].Value!.Value.GetBoolean());
    }

    private async Task<string> IssueResolveTokenAsync(string adminToken)
    {
        var registerRequest = NexArrAuthorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"staffarr-sync-{Guid.NewGuid():N}",
            "Fact sync resolve test",
            "staffarr",
            ["compliancecore"]));
        var registerResponse = await _nexarrClient.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var issueRequest = NexArrAuthorized(HttpMethod.Post, "/api/service-tokens", adminToken);
        issueRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            ["compliancecore"],
            $"{FactResolveService.ResolveActionScope},{FactResolveService.ValidateActionScope}",
            30));
        var issueResponse = await _nexarrClient.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        var issued = (await issueResponse.Content.ReadFromJsonAsync<ServiceTokenIssueResponse>())!;
        return issued.AccessToken;
    }

    private async Task CreateProductApiSnapshotSourceAsync(string adminToken, Guid factId, string sourceKey)
    {
        var request = Authorized(HttpMethod.Post, "/api/fact-sources", adminToken);
        request.Content = JsonContent.Create(new CreateFactSourceRequest(
            factId,
            sourceKey,
            FactSourceTypes.ProductApi,
            "Snapshot sync source",
            "Background sync writes mirror cache from configured snapshot value.",
            "staffarr",
            "/api/internal/compliance-facts/{factKey}",
            """{"scopeKey":"tenant","booleanValue":true}""",
            0));
        (await _complianceCoreClient.SendAsync(request)).EnsureSuccessStatusCode();
    }

    private async Task<Guid> CreateBooleanFactAsync(string adminToken, string factKey)
    {
        var request = Authorized(HttpMethod.Post, "/api/fact-definitions", adminToken);
        request.Content = JsonContent.Create(new CreateFactDefinitionRequest(
            factKey,
            $"Label {factKey}",
            "Fact source sync test fact.",
            "boolean"));
        var response = await _complianceCoreClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<FactDefinitionResponse>())!.FactDefinitionId;
    }

    private static HttpRequestMessage ServiceAuthorized(HttpMethod method, string url, string serviceToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);
        return request;
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }

    private string CreateComplianceCoreAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_member")
    {
        using var scope = _complianceCoreFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<ComplianceCoreTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "Test Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements,
            isPlatformAdmin: false);

        return accessToken;
    }

    private async Task<string> LoginNexArrAdminAsync()
    {
        var response = await _nexarrClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(PlatformSeeder.DemoAdminEmail, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        var payload = (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!;
        return payload.AccessToken;
    }

    private async Task<string> IssueServiceTokenAsync(string adminToken, string actionScope)
    {
        var registerRequest = NexArrAuthorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"shared-worker-fact-sync-{Guid.NewGuid():N}",
            "Fact source sync worker test",
            "shared-worker",
            ["compliancecore"]));
        var registerResponse = await _nexarrClient.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var issueRequest = NexArrAuthorized(HttpMethod.Post, "/api/service-tokens", adminToken);
        issueRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            ["compliancecore"],
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

    private static HttpRequestMessage NexArrAuthorized(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
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
