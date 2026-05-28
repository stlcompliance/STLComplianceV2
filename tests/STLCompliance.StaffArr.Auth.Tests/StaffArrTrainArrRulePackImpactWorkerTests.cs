using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Services;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using TrainArr.Api.Services;

namespace STLCompliance.StaffArr.Auth.Tests;

public class StaffArrTrainArrRulePackImpactWorkerTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::ComplianceCore.Api.Program> _complianceCoreFactory = null!;
    private WebApplicationFactory<global::TrainArr.Api.Program> _trainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _complianceCoreClient = null!;
    private HttpClient _trainarrClient = null!;
    private string _sharedWorkerToTrainarrToken = null!;
    private string _trainarrToComplianceCoreToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"ImpactWorkerNexArr-{Guid.NewGuid():N}";
        var complianceDbName = $"ImpactWorkerCompliance-{Guid.NewGuid():N}";
        var trainArrDbName = $"ImpactWorkerTrainArr-{Guid.NewGuid():N}";

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

        _complianceCoreFactory = new WebApplicationFactory<global::ComplianceCore.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<ComplianceCoreDbContext>(services);
                services.AddDbContext<ComplianceCoreDbContext>(options => options.UseInMemoryDatabase(complianceDbName));
            });
        });

        _nexarrClient = _nexarrFactory.CreateClient();
        _complianceCoreClient = _complianceCoreFactory.CreateClient();
        await SeedNexArrAsync();

        var adminToken = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        _sharedWorkerToTrainarrToken = await IssueServiceTokenAsync(
            adminToken,
            "shared-worker",
            ["trainarr"],
            RulePackImpactWorkerService.ProcessImpactScansActionScope);
        _trainarrToComplianceCoreToken = await IssueServiceTokenAsync(
            adminToken,
            "trainarr",
            ["compliancecore"],
            $"{InternalRuleEvaluationService.EvaluateActionScope},{InternalRulePackLookupService.ReadActionScope}");

        _trainarrFactory = new WebApplicationFactory<global::TrainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.UseSetting("ComplianceCore:BaseUrl", _complianceCoreClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("ComplianceCore:ServiceToken", _trainarrToComplianceCoreToken);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<TrainArrDbContext>(services);
                services.AddDbContext<TrainArrDbContext>(options => options.UseInMemoryDatabase(trainArrDbName));

                services.AddHttpClient<ComplianceCoreRuleEvaluationClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _complianceCoreFactory.Server.CreateHandler());
                services.AddHttpClient<ComplianceCoreRulePackClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _complianceCoreFactory.Server.CreateHandler());
            });
        });

        _trainarrClient = _trainarrFactory.CreateClient();

        using var scope = _complianceCoreFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        await db.Database.EnsureCreatedAsync();
        var vocabularyService = scope.ServiceProvider.GetRequiredService<VocabularyService>();
        await vocabularyService.EnsureVocabularyTypesSeededAsync();
    }

    public async Task DisposeAsync()
    {
        _trainarrClient.Dispose();
        _complianceCoreClient.Dispose();
        _nexarrClient.Dispose();
        await _trainarrFactory.DisposeAsync();
        await _complianceCoreFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Process_rule_pack_impact_batch_rejects_missing_service_token()
    {
        var response = await _trainarrClient.PostAsJsonAsync(
            "/api/internal/rule-pack-impact/process-batch",
            new ProcessRulePackImpactScansRequest(null, DateTimeOffset.UtcNow, 50, 24));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task List_pending_rule_pack_impact_returns_linked_rule_pack_keys()
    {
        var complianceAdminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var programId = await CreateSampleProgramAsync(complianceAdminToken);
        await CreatePublishedRulePackAsync(complianceAdminToken, programId, "driver_qualification", 1);

        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var definitionId = await CreateTrainingDefinitionAsync(adminToken);
        await LinkRulePackRequirementAsync(adminToken, definitionId, "driver_qualification");
        await SeedImpactSettingsAsync(PlatformSeeder.DemoTenantId);

        var listRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/internal/rule-pack-impact/pending?tenantId={PlatformSeeder.DemoTenantId}&batchSize=10&stalenessHours=24");
        listRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _sharedWorkerToTrainarrToken);

        var listResponse = await _trainarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var pending = (await listResponse.Content.ReadFromJsonAsync<PendingRulePackImpactScansResponse>())!;
        Assert.Contains(pending.Items, x => x.RulePackKey == "driver_qualification");
    }

    [Fact]
    public async Task Process_rule_pack_impact_batch_persists_materialized_state_on_version_drift()
    {
        var complianceAdminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var programId = await CreateSampleProgramAsync(complianceAdminToken);
        await CreatePublishedRulePackAsync(complianceAdminToken, programId, "driver_qualification", 1);

        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var definitionId = await CreateTrainingDefinitionAsync(adminToken);
        await LinkRulePackRequirementAsync(adminToken, definitionId, "driver_qualification");
        await SeedImpactSettingsAsync(PlatformSeeder.DemoTenantId);

        await CreatePublishedRulePackAsync(complianceAdminToken, programId, "driver_qualification", 2);

        var processRequest = new HttpRequestMessage(HttpMethod.Post, "/api/internal/rule-pack-impact/process-batch");
        processRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _sharedWorkerToTrainarrToken);
        processRequest.Content = JsonContent.Create(new ProcessRulePackImpactScansRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            50,
            24));

        var processResponse = await _trainarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var body = (await processResponse.Content.ReadFromJsonAsync<ProcessRulePackImpactScansResponse>())!;
        Assert.Equal(1, body.AssessedCount);
        Assert.Contains("driver_qualification", body.AttentionRequiredRulePackKeys);

        using var scope = _trainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        var state = await db.RulePackImpactStates.SingleAsync(x => x.RulePackKey == "driver_qualification");
        Assert.True(state.RequiresAttention);
        Assert.True(state.HasDrift);
        Assert.Equal(2, state.CurrentVersionNumber);

        var run = await db.RulePackImpactRuns.SingleAsync(x => x.RulePackKey == "driver_qualification");
        Assert.Equal("attention_required", run.Outcome);
    }

    private async Task LinkRulePackRequirementAsync(
        string adminToken,
        Guid definitionId,
        string rulePackKey,
        bool validate = true)
    {
        var upsertRequest = Authorized(
            HttpMethod.Put,
            $"/api/training-definitions/{definitionId}/rule-pack-requirements?validateWithComplianceCore={validate.ToString().ToLowerInvariant()}",
            adminToken);
        upsertRequest.Content = JsonContent.Create(new UpsertTrainingRulePackRequirementRequest(rulePackKey));
        (await _trainarrClient.SendAsync(upsertRequest)).EnsureSuccessStatusCode();
    }

    private async Task SeedImpactSettingsAsync(Guid tenantId)
    {
        using var scope = _trainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        var now = DateTimeOffset.UtcNow;
        db.TenantRulePackImpactSettings.Add(new TenantRulePackImpactSettings
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            IsEnabled = true,
            StalenessHours = 24,
            AutoUpdateRequirementBaselines = false,
            CreatedAt = now,
            UpdatedAt = now,
        });
        await db.SaveChangesAsync();
    }

    private async Task CreatePublishedRulePackAsync(
        string adminToken,
        Guid programId,
        string packKey,
        int expectedVersion)
    {
        var createRequest = Authorized(HttpMethod.Post, "/api/rule-packs", adminToken);
        createRequest.Content = JsonContent.Create(new CreateRulePackRequest(
            programId,
            packKey,
            $"Rules {packKey} v{expectedVersion}",
            $"Version {expectedVersion} rule pack."));
        var createResponse = await _complianceCoreClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<RulePackResponse>())!;
        Assert.Equal(expectedVersion, created.VersionNumber);

        var reviewRequest = Authorized(HttpMethod.Patch, $"/api/rule-packs/{created.RulePackId}/status", adminToken);
        reviewRequest.Content = JsonContent.Create(new UpdateRulePackStatusRequest("review"));
        (await _complianceCoreClient.SendAsync(reviewRequest)).EnsureSuccessStatusCode();

        var publishRequest = Authorized(HttpMethod.Patch, $"/api/rule-packs/{created.RulePackId}/status", adminToken);
        publishRequest.Content = JsonContent.Create(new UpdateRulePackStatusRequest("published"));
        (await _complianceCoreClient.SendAsync(publishRequest)).EnsureSuccessStatusCode();
    }

    private async Task<Guid> CreateTrainingDefinitionAsync(string adminToken)
    {
        var definitionKey = $"impact_worker_{Guid.NewGuid():N}"[..20];
        var request = Authorized(HttpMethod.Post, "/api/training-definitions", adminToken);
        request.Content = JsonContent.Create(new CreateTrainingDefinitionRequest(
            definitionKey,
            "Impact worker definition",
            "Training definition for rule pack impact worker tests.",
            "hazmat_endorsement",
            "Hazmat Endorsement"));
        var response = await _trainarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var created = (await response.Content.ReadFromJsonAsync<TrainingDefinitionResponse>())!;
        return created.TrainingDefinitionId;
    }

    private async Task<Guid> CreateSampleProgramAsync(string adminToken)
    {
        var bodyRequest = Authorized(HttpMethod.Post, "/api/governing-bodies", adminToken);
        bodyRequest.Content = JsonContent.Create(new CreateGoverningBodyRequest(
            "dot",
            "Department of Transportation",
            "US DOT"));
        var bodyResponse = await _complianceCoreClient.SendAsync(bodyRequest);
        if (bodyResponse.IsSuccessStatusCode)
        {
            var body = (await bodyResponse.Content.ReadFromJsonAsync<GoverningBodyResponse>())!;
            var jurisdictionRequest = Authorized(HttpMethod.Post, "/api/jurisdictions", adminToken);
            jurisdictionRequest.Content = JsonContent.Create(new CreateJurisdictionRequest(
                body.GoverningBodyId,
                "us_federal",
                "US Federal",
                "United States federal jurisdiction."));
            var jurisdictionResponse = await _complianceCoreClient.SendAsync(jurisdictionRequest);
            jurisdictionResponse.EnsureSuccessStatusCode();
            var jurisdiction = (await jurisdictionResponse.Content.ReadFromJsonAsync<JurisdictionResponse>())!;

            var programRequest = Authorized(HttpMethod.Post, "/api/regulatory-programs", adminToken);
            programRequest.Content = JsonContent.Create(new CreateRegulatoryProgramRequest(
                jurisdiction.JurisdictionId,
                "driver_compliance",
                "Driver Compliance",
                "Driver qualification program."));
            var programResponse = await _complianceCoreClient.SendAsync(programRequest);
            programResponse.EnsureSuccessStatusCode();
            var program = (await programResponse.Content.ReadFromJsonAsync<RegulatoryProgramResponse>())!;
            return program.RegulatoryProgramId;
        }

        var listPrograms = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/regulatory-programs", adminToken));
        listPrograms.EnsureSuccessStatusCode();
        var programs = (await listPrograms.Content.ReadFromJsonAsync<IReadOnlyList<RegulatoryProgramResponse>>())!;
        return programs[0].RegulatoryProgramId;
    }

    private string CreateTrainArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_member",
        Guid? personId = null)
    {
        using var scope = _trainarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<TrainArrTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            personId ?? PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "Test Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements,
            isPlatformAdmin: false);

        return accessToken;
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

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
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
            $"{sourceProduct}-impact-worker-{Guid.NewGuid():N}",
            $"{sourceProduct} impact worker test",
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
