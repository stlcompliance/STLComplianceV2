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
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Services;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;

namespace STLCompliance.StaffArr.Auth.Tests;

public class StaffArrTrainArrRulePackImpactTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::ComplianceCore.Api.Program> _complianceCoreFactory = null!;
    private WebApplicationFactory<global::TrainArr.Api.Program> _trainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _complianceCoreClient = null!;
    private HttpClient _trainarrClient = null!;
    private string _trainarrToComplianceCoreToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"ImpactNexArr-{Guid.NewGuid():N}";
        var complianceDbName = $"ImpactCompliance-{Guid.NewGuid():N}";
        var trainArrDbName = $"ImpactTrainArr-{Guid.NewGuid():N}";

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

                services.AddHttpClient<TrainArr.Api.Services.ComplianceCoreRuleEvaluationClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _complianceCoreFactory.Server.CreateHandler());
                services.AddHttpClient<TrainArr.Api.Services.ComplianceCoreRulePackClient>()
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
    public async Task Rule_pack_impact_get_lists_affected_entities_with_version_drift()
    {
        var complianceAdminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var programId = await CreateSampleProgramAsync(complianceAdminToken);
        await CreatePublishedRulePackAsync(complianceAdminToken, programId, "driver_qualification", 1);

        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var definitionId = await CreateTrainingDefinitionAsync(adminToken);
        await LinkRulePackRequirementAsync(adminToken, definitionId, "driver_qualification");

        var personId = Guid.NewGuid();
        await SeedActiveAssignmentAsync(definitionId, personId);

        await CreatePublishedRulePackAsync(complianceAdminToken, programId, "driver_qualification", 2);

        var impactRequest = Authorized(
            HttpMethod.Get,
            "/api/rule-pack-impact?rulePackKey=driver_qualification",
            adminToken);
        var impactResponse = await _trainarrClient.SendAsync(impactRequest);
        impactResponse.EnsureSuccessStatusCode();
        var impact = (await impactResponse.Content.ReadFromJsonAsync<RulePackImpactAssessmentResponse>())!;

        Assert.Equal("driver_qualification", impact.RulePackKey);
        Assert.Contains(RulePackImpactTriggers.VersionDrift, impact.Triggers);
        Assert.True(impact.Summary.RequiresAttention);
        Assert.Single(impact.AffectedDefinitions);
        Assert.Single(impact.AffectedAssignments);
        Assert.True(impact.Drift!.HasVersionDrift);
        Assert.Equal(1, impact.Drift.BaselineVersionNumber);
        Assert.Equal(2, impact.Drift.CurrentVersionNumber);
        Assert.Contains(
            impact.RecommendedActions,
            action => action.ActionType == RulePackImpactRecommendedActionTypes.ReviewActiveAssignments);
    }

    [Fact]
    public async Task Rule_pack_impact_post_assess_accepts_expected_version_override()
    {
        var complianceAdminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var programId = await CreateSampleProgramAsync(complianceAdminToken);
        await CreatePublishedRulePackAsync(complianceAdminToken, programId, "hazmat_rules", 1);

        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var definitionId = await CreateTrainingDefinitionAsync(adminToken);
        await LinkRulePackRequirementAsync(adminToken, definitionId, "hazmat_rules", validate: false);

        var assessRequest = Authorized(HttpMethod.Post, "/api/rule-pack-impact/assess", adminToken);
        assessRequest.Content = JsonContent.Create(new AssessRulePackImpactRequest(
            "hazmat_rules",
            ExpectedVersionNumber: 1,
            ExpectedStatus: "published"));
        var assessResponse = await _trainarrClient.SendAsync(assessRequest);
        assessResponse.EnsureSuccessStatusCode();
        var impact = (await assessResponse.Content.ReadFromJsonAsync<RulePackImpactAssessmentResponse>())!;

        Assert.Contains(RulePackImpactTriggers.ManualAssessment, impact.Triggers);
        Assert.False(impact.Drift!.HasVersionDrift);
        Assert.Single(impact.AffectedDefinitions);
    }

    [Fact]
    public async Task Rule_pack_impact_denies_member_role()
    {
        var memberToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "tenant_member");
        var request = Authorized(HttpMethod.Get, "/api/rule-pack-impact?rulePackKey=driver_qualification", memberToken);
        var response = await _trainarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Rule_pack_impact_assessment_writes_audit_event()
    {
        var complianceAdminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var programId = await CreateSampleProgramAsync(complianceAdminToken);
        await CreatePublishedRulePackAsync(complianceAdminToken, programId, "driver_qualification", 1);

        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var definitionId = await CreateTrainingDefinitionAsync(adminToken);
        await LinkRulePackRequirementAsync(adminToken, definitionId, "driver_qualification");

        var assessRequest = Authorized(HttpMethod.Post, "/api/rule-pack-impact/assess", adminToken);
        assessRequest.Content = JsonContent.Create(new AssessRulePackImpactRequest("driver_qualification"));
        (await _trainarrClient.SendAsync(assessRequest)).EnsureSuccessStatusCode();

        using var scope = _trainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        var audit = await db.AuditEvents
            .Where(x => x.Action == "rule_pack_impact.assess")
            .ToListAsync();
        Assert.NotEmpty(audit);
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

    private async Task SeedActiveAssignmentAsync(Guid definitionId, Guid personId)
    {
        using var scope = _trainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        var now = DateTimeOffset.UtcNow;
        db.TrainingAssignments.Add(new TrainingAssignment
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            StaffarrPersonId = personId,
            TrainingDefinitionId = definitionId,
            AssignmentReason = "manual",
            Status = "in_progress",
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
        var definitionKey = $"impact_def_{Guid.NewGuid():N}"[..20];
        var request = Authorized(HttpMethod.Post, "/api/training-definitions", adminToken);
        request.Content = JsonContent.Create(new CreateTrainingDefinitionRequest(
            definitionKey,
            "Impact definition",
            "Training definition for rule pack impact tests.",
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
        var tokenService = scope.ServiceProvider.GetRequiredService<TrainArr.Api.Services.TrainArrTokenService>();
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
            $"{sourceProduct}-impact-{Guid.NewGuid():N}",
            $"{sourceProduct} impact test",
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
