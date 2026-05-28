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

public sealed class ComplianceCoreM12AnalyticsBatchWorkerTests : IAsyncLifetime
{
    private const string SigningKey = "test-signing-key-at-least-32-chars-long";

    private WebApplicationFactory<global::ComplianceCore.Api.Program> _complianceCoreFactory = null!;
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private HttpClient _complianceCoreClient = null!;
    private HttpClient _nexarrClient = null!;
    private string _workerToken = null!;

    public async Task InitializeAsync()
    {
        var complianceDbName = $"ComplianceCoreM12Batch-{Guid.NewGuid():N}";
        var nexarrDbName = $"NexArrM12Batch-{Guid.NewGuid():N}";

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
            M12AnalyticsBatchWorkerService.ProcessBatchActionScope);
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
            "/api/internal/m12-analytics-batches/process-batch",
            new ProcessM12AnalyticsBatchesRequest(PlatformSeeder.DemoTenantId, null, null, null));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Worker_settings_upsert_and_get_round_trip()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");

        var putRequest = Authorized(HttpMethod.Put, "/api/m12-analytics-worker-settings", adminToken);
        putRequest.Content = JsonContent.Create(new UpsertM12AnalyticsWorkerSettingsRequest(
            true,
            "tenant",
            12,
            true,
            true,
            true,
            true,
            false));
        var putResponse = await _complianceCoreClient.SendAsync(putRequest);
        putResponse.EnsureSuccessStatusCode();
        var saved = (await putResponse.Content.ReadFromJsonAsync<M12AnalyticsWorkerSettingsResponse>())!;
        Assert.True(saved.IsEnabled);
        Assert.Equal(12, saved.IntervalHours);

        var getResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/m12-analytics-worker-settings", adminToken));
        getResponse.EnsureSuccessStatusCode();
        var loaded = (await getResponse.Content.ReadFromJsonAsync<M12AnalyticsWorkerSettingsResponse>())!;
        Assert.True(loaded.IsEnabled);
        Assert.Equal("tenant", loaded.DefaultScopeKey);
    }

    [Fact]
    public async Task Process_batch_runs_readiness_forecast_for_enabled_tenant()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        await SeedPublishedPackWithStaticFactAsync(adminToken);

        var putRequest = Authorized(HttpMethod.Put, "/api/m12-analytics-worker-settings", adminToken);
        putRequest.Content = JsonContent.Create(new UpsertM12AnalyticsWorkerSettingsRequest(
            true,
            "tenant",
            24,
            RiskScoringEnabled: false,
            MissingEvidenceEnabled: false,
            ControlEffectivenessEnabled: false,
            ReadinessForecastEnabled: true,
            AuditDeliveryEnabled: false));
        (await _complianceCoreClient.SendAsync(putRequest)).EnsureSuccessStatusCode();

        var pendingRequest = ServiceAuthorized(
            HttpMethod.Get,
            "/api/internal/m12-analytics-batches/pending?tenantId=" + PlatformSeeder.DemoTenantId,
            _workerToken);
        var pendingResponse = await _complianceCoreClient.SendAsync(pendingRequest);
        pendingResponse.EnsureSuccessStatusCode();
        var pending = (await pendingResponse.Content.ReadFromJsonAsync<PendingM12AnalyticsBatchesResponse>())!;
        Assert.Contains(pending.Items, item => item.TenantId == PlatformSeeder.DemoTenantId);

        var processRequest = ServiceAuthorized(
            HttpMethod.Post,
            "/api/internal/m12-analytics-batches/process-batch",
            _workerToken);
        processRequest.Content = JsonContent.Create(new ProcessM12AnalyticsBatchesRequest(
            PlatformSeeder.DemoTenantId,
            AsOfUtc: null,
            IntervalHours: 24,
            BatchSize: 10));
        var processResponse = await _complianceCoreClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var batch = (await processResponse.Content.ReadFromJsonAsync<ProcessM12AnalyticsBatchesResponse>())!;
        Assert.Equal(1, batch.ProcessedCount);
        Assert.Equal(M12AnalyticsBatchRunStatuses.Completed, batch.Results[0].Status);
        Assert.True(batch.Results[0].ReadinessForecastRan);

        using var scope = _complianceCoreFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        Assert.True(await db.ReadinessForecastRuns.AnyAsync(x => x.TenantId == PlatformSeeder.DemoTenantId));
        Assert.True(await db.M12AnalyticsBatchRuns.AnyAsync(x => x.TenantId == PlatformSeeder.DemoTenantId));
    }

    [Fact]
    public async Task Process_batch_queues_audit_delivery_when_enabled()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "tenant_admin");
        await SeedPublishedPackWithStaticFactAsync(adminToken);

        var putRequest = Authorized(HttpMethod.Put, "/api/m12-analytics-worker-settings", adminToken);
        putRequest.Content = JsonContent.Create(new UpsertM12AnalyticsWorkerSettingsRequest(
            true,
            "tenant",
            24,
            false,
            false,
            false,
            false,
            true));
        (await _complianceCoreClient.SendAsync(putRequest)).EnsureSuccessStatusCode();

        var processRequest = ServiceAuthorized(
            HttpMethod.Post,
            "/api/internal/m12-analytics-batches/process-batch",
            _workerToken);
        processRequest.Content = JsonContent.Create(
            new ProcessM12AnalyticsBatchesRequest(
                TenantId: PlatformSeeder.DemoTenantId,
                AsOfUtc: null,
                IntervalHours: null,
                BatchSize: null));
        var processResponse = await _complianceCoreClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var batch = (await processResponse.Content.ReadFromJsonAsync<ProcessM12AnalyticsBatchesResponse>())!;
        Assert.NotEmpty(batch.Results);
        Assert.True(batch.Results[0].AuditDeliveryQueued);
        Assert.NotNull(batch.Results[0].AuditPackageJobId);

        using var scope = _complianceCoreFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        Assert.True(await db.AuditPackageGenerationJobs.AnyAsync(x =>
            x.TenantId == PlatformSeeder.DemoTenantId && x.Status == AuditPackageGenerationJobStatuses.Pending));
    }

    private async Task SeedPublishedPackWithStaticFactAsync(string adminToken)
    {
        var factId = await CreateBooleanFactAsync(adminToken, "m12_batch_license_valid");
        var programId = await CreateProgramAsync(adminToken);
        var packId = await CreatePackAsync(adminToken, programId, "m12_batch_pack");
        await SetPackContentAsync(adminToken, packId);
        await CreateStaticFactSourceAsync(adminToken, factId);
        await PublishPackAsync(adminToken, packId);
    }

    private async Task<Guid> CreateBooleanFactAsync(string adminToken, string factKey)
    {
        var request = Authorized(HttpMethod.Post, "/api/fact-definitions", adminToken);
        request.Content = JsonContent.Create(new CreateFactDefinitionRequest(
            factKey,
            $"Label {factKey}",
            "M12 batch test fact.",
            "boolean"));
        var response = await _complianceCoreClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<FactDefinitionResponse>())!.FactDefinitionId;
    }

    private async Task CreateStaticFactSourceAsync(string adminToken, Guid factId)
    {
        var request = Authorized(HttpMethod.Post, "/api/fact-sources", adminToken);
        request.Content = JsonContent.Create(new CreateFactSourceRequest(
            factId,
            "m12_batch_license_source",
            FactSourceTypes.StaticConfig,
            "M12 batch license source",
            "Static true for M12 batch tests.",
            null,
            null,
            """{"booleanValue":true}""",
            0));
        (await _complianceCoreClient.SendAsync(request)).EnsureSuccessStatusCode();
    }

    private async Task SetPackContentAsync(string adminToken, Guid packId)
    {
        var content = new RulePackContentBody(
            1,
            "all",
            [new RuleDefinitionDto("license_valid", "Valid license", "fact_boolean", "m12_batch_license_valid", true)]);

        var request = Authorized(HttpMethod.Put, $"/api/rule-packs/{packId}/content", adminToken);
        request.Content = JsonContent.Create(new UpdateRulePackContentRequest(content));
        (await _complianceCoreClient.SendAsync(request)).EnsureSuccessStatusCode();
    }

    private async Task PublishPackAsync(string adminToken, Guid packId)
    {
        var reviewRequest = Authorized(HttpMethod.Patch, $"/api/rule-packs/{packId}/status", adminToken);
        reviewRequest.Content = JsonContent.Create(new UpdateRulePackStatusRequest(RulePackStatuses.Review));
        (await _complianceCoreClient.SendAsync(reviewRequest)).EnsureSuccessStatusCode();

        var publishRequest = Authorized(HttpMethod.Patch, $"/api/rule-packs/{packId}/status", adminToken);
        publishRequest.Content = JsonContent.Create(new UpdateRulePackStatusRequest(RulePackStatuses.Published));
        (await _complianceCoreClient.SendAsync(publishRequest)).EnsureSuccessStatusCode();
    }

    private async Task<Guid> CreatePackAsync(string adminToken, Guid programId, string packKey)
    {
        var request = Authorized(HttpMethod.Post, "/api/rule-packs", adminToken);
        request.Content = JsonContent.Create(new CreateRulePackRequest(
            programId,
            packKey,
            $"Label {packKey}",
            "M12 batch test pack."));
        var response = await _complianceCoreClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RulePackResponse>())!.RulePackId;
    }

    private async Task<Guid> CreateProgramAsync(string adminToken)
    {
        var bodyRequest = Authorized(HttpMethod.Post, "/api/governing-bodies", adminToken);
        bodyRequest.Content = JsonContent.Create(new CreateGoverningBodyRequest(
            "m12_dot",
            "M12 DOT",
            "M12 batch governing body."));
        var body = (await (await _complianceCoreClient.SendAsync(bodyRequest)).Content.ReadFromJsonAsync<GoverningBodyResponse>())!;

        var jurisdictionRequest = Authorized(HttpMethod.Post, "/api/jurisdictions", adminToken);
        jurisdictionRequest.Content = JsonContent.Create(new CreateJurisdictionRequest(
            body.GoverningBodyId,
            "m12_us",
            "M12 US",
            "M12 jurisdiction."));
        var jurisdiction = (await (await _complianceCoreClient.SendAsync(jurisdictionRequest)).Content.ReadFromJsonAsync<JurisdictionResponse>())!;

        var programRequest = Authorized(HttpMethod.Post, "/api/regulatory-programs", adminToken);
        programRequest.Content = JsonContent.Create(new CreateRegulatoryProgramRequest(
            jurisdiction.JurisdictionId,
            "m12_fmcsa",
            "M12 FMCSA",
            "M12 program."));
        var program = (await (await _complianceCoreClient.SendAsync(programRequest)).Content.ReadFromJsonAsync<RegulatoryProgramResponse>())!;
        return program.RegulatoryProgramId;
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
            $"shared-worker-m12-{Guid.NewGuid():N}",
            "M12 analytics batch worker test",
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
