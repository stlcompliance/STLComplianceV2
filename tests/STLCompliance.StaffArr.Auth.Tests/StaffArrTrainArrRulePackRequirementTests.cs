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

public class StaffArrTrainArrRulePackRequirementTests : IAsyncLifetime
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
        var nexArrDbName = $"RulePackNexArr-{Guid.NewGuid():N}";
        var complianceDbName = $"RulePackCompliance-{Guid.NewGuid():N}";
        var trainArrDbName = $"RulePackTrainArr-{Guid.NewGuid():N}";

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
    public async Task Training_definition_rule_pack_upsert_list_remove_with_metadata()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var definitionId = await CreateTrainingDefinitionAsync(adminToken);
        await CreateComplianceRulePackAsync("driver_qualification");

        var upsertRequest = Authorized(
            HttpMethod.Put,
            $"/api/training-definitions/{definitionId}/rule-pack-requirements?validateWithComplianceCore=true",
            adminToken);
        upsertRequest.Content = JsonContent.Create(new UpsertTrainingRulePackRequirementRequest("driver_qualification"));
        var upsertResponse = await _trainarrClient.SendAsync(upsertRequest);
        upsertResponse.EnsureSuccessStatusCode();
        var upserted = (await upsertResponse.Content.ReadFromJsonAsync<TrainingRulePackRequirementResponse>())!;
        Assert.Equal("driver_qualification", upserted.RulePackKey);
        Assert.NotNull(upserted.Metadata);
        Assert.Equal("Driver Qualification Rules", upserted.Metadata!.Label);

        var listRequest = Authorized(
            HttpMethod.Get,
            $"/api/training-definitions/{definitionId}/rule-pack-requirements?includeMetadata=true",
            adminToken);
        var listResponse = await _trainarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var list = (await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<TrainingRulePackRequirementResponse>>())!;
        Assert.Single(list);

        var removeRequest = Authorized(
            HttpMethod.Delete,
            $"/api/training-definitions/{definitionId}/rule-pack-requirements/{upserted.RequirementId}",
            adminToken);
        var removeResponse = await _trainarrClient.SendAsync(removeRequest);
        Assert.Equal(HttpStatusCode.NoContent, removeResponse.StatusCode);
    }

    [Fact]
    public async Task Training_program_rule_pack_requirement_persists_reference_only()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var definitionId = await CreateTrainingDefinitionAsync(adminToken);
        var programId = await CreateTrainingProgramAsync(adminToken, definitionId);

        var upsertRequest = Authorized(
            HttpMethod.Put,
            $"/api/training-programs/{programId}/rule-pack-requirements",
            adminToken);
        upsertRequest.Content = JsonContent.Create(new UpsertTrainingRulePackRequirementRequest("external_rule_pack"));
        var upsertResponse = await _trainarrClient.SendAsync(upsertRequest);
        upsertResponse.EnsureSuccessStatusCode();
        var upserted = (await upsertResponse.Content.ReadFromJsonAsync<TrainingRulePackRequirementResponse>())!;
        Assert.Equal("external_rule_pack", upserted.RulePackKey);
        Assert.Null(upserted.Metadata);
    }

    [Fact]
    public async Task Training_definition_rule_pack_denies_member_role()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var definitionId = await CreateTrainingDefinitionAsync(adminToken);
        var memberToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "tenant_member");

        var upsertRequest = Authorized(
            HttpMethod.Put,
            $"/api/training-definitions/{definitionId}/rule-pack-requirements",
            memberToken);
        upsertRequest.Content = JsonContent.Create(new UpsertTrainingRulePackRequirementRequest("driver_qualification"));
        var upsertResponse = await _trainarrClient.SendAsync(upsertRequest);
        Assert.Equal(HttpStatusCode.Forbidden, upsertResponse.StatusCode);
    }

    [Fact]
    public async Task Training_definition_rule_pack_rejects_unknown_pack_when_validating()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var definitionId = await CreateTrainingDefinitionAsync(adminToken);

        var upsertRequest = Authorized(
            HttpMethod.Put,
            $"/api/training-definitions/{definitionId}/rule-pack-requirements?validateWithComplianceCore=true",
            adminToken);
        upsertRequest.Content = JsonContent.Create(new UpsertTrainingRulePackRequirementRequest("missing_rule_pack"));
        var upsertResponse = await _trainarrClient.SendAsync(upsertRequest);
        Assert.Equal(HttpStatusCode.NotFound, upsertResponse.StatusCode);
    }

    [Fact]
    public async Task Rule_pack_requirement_writes_audit_event()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var definitionId = await CreateTrainingDefinitionAsync(adminToken);

        var upsertRequest = Authorized(
            HttpMethod.Put,
            $"/api/training-definitions/{definitionId}/rule-pack-requirements",
            adminToken);
        upsertRequest.Content = JsonContent.Create(new UpsertTrainingRulePackRequirementRequest("driver_qualification"));
        (await _trainarrClient.SendAsync(upsertRequest)).EnsureSuccessStatusCode();

        using var scope = _trainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        var audit = await db.AuditEvents
            .Where(x => x.Action == "rule_pack_requirement.create")
            .ToListAsync();
        Assert.NotEmpty(audit);
    }

    [Fact]
    public async Task Qualification_check_resolves_rule_pack_from_definition_requirement()
    {
        var complianceAdminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        await SeedDriverQualificationRulePackAsync(complianceAdminToken, booleanValue: true);

        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var definitionId = await CreateTrainingDefinitionAsync(adminToken);

        var upsertRequest = Authorized(
            HttpMethod.Put,
            $"/api/training-definitions/{definitionId}/rule-pack-requirements?validateWithComplianceCore=true",
            adminToken);
        upsertRequest.Content = JsonContent.Create(new UpsertTrainingRulePackRequirementRequest("driver_qualification"));
        (await _trainarrClient.SendAsync(upsertRequest)).EnsureSuccessStatusCode();

        var personId = Guid.NewGuid();
        var checkRequest = Authorized(HttpMethod.Post, "/api/qualification-checks", adminToken);
        checkRequest.Content = JsonContent.Create(new CreateQualificationCheckRequest(
            personId,
            "hazmat_endorsement",
            null,
            null,
            definitionId,
            null));
        var checkResponse = await _trainarrClient.SendAsync(checkRequest);
        checkResponse.EnsureSuccessStatusCode();
        var check = (await checkResponse.Content.ReadFromJsonAsync<QualificationCheckResponse>())!;

        Assert.Equal(QualificationCheckOutcomes.Warn, check.Outcome);
        Assert.NotNull(check.ComplianceCore);
        Assert.Equal("driver_qualification", check.ComplianceCore!.RulePackKey);
        Assert.Equal(QualificationCheckOutcomes.Allow, check.ComplianceCore.Outcome);
    }

    private async Task CreateComplianceRulePackAsync(string packKey)
    {
        var complianceAdminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var programId = await CreateSampleProgramAsync(complianceAdminToken);
        var createPackRequest = Authorized(HttpMethod.Post, "/api/rule-packs", complianceAdminToken);
        createPackRequest.Content = JsonContent.Create(new CreateRulePackRequest(
            programId,
            packKey,
            "Driver Qualification Rules",
            "TrainArr rule pack requirement test."));
        (await _complianceCoreClient.SendAsync(createPackRequest)).EnsureSuccessStatusCode();
    }

    private async Task SeedDriverQualificationRulePackAsync(string adminToken, bool booleanValue)
    {
        var programId = await CreateSampleProgramAsync(adminToken);
        var createPackRequest = Authorized(HttpMethod.Post, "/api/rule-packs", adminToken);
        createPackRequest.Content = JsonContent.Create(new CreateRulePackRequest(
            programId,
            "driver_qualification",
            "Driver Qualification Rules",
            "TrainArr authorization check rule pack."));
        var createPackResponse = await _complianceCoreClient.SendAsync(createPackRequest);
        createPackResponse.EnsureSuccessStatusCode();
        var pack = (await createPackResponse.Content.ReadFromJsonAsync<RulePackResponse>())!;

        var factKey = "driver_license_valid";
        var factDefinitionId = await CreateBooleanFactDefinitionAsync(adminToken, factKey);
        var createSourceRequest = Authorized(HttpMethod.Post, "/api/fact-sources", adminToken);
        createSourceRequest.Content = JsonContent.Create(new CreateFactSourceRequest(
            factDefinitionId,
            "default_license_flag",
            "static_config",
            "Default license valid",
            "Static default for driver license validity checks.",
            null,
            null,
            booleanValue ? """{"booleanValue":true}""" : """{"booleanValue":false}""",
            0));
        (await _complianceCoreClient.SendAsync(createSourceRequest)).EnsureSuccessStatusCode();

        var content = new RulePackContentBody(
            1,
            "all",
            [
                new RuleDefinitionDto(
                    "license_valid",
                    "Valid driver license",
                    "fact_boolean",
                    factKey,
                    true),
            ]);

        var updateContentRequest = Authorized(HttpMethod.Put, $"/api/rule-packs/{pack.RulePackId}/content", adminToken);
        updateContentRequest.Content = JsonContent.Create(new UpdateRulePackContentRequest(content));
        (await _complianceCoreClient.SendAsync(updateContentRequest)).EnsureSuccessStatusCode();
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

    private async Task<Guid> CreateBooleanFactDefinitionAsync(string adminToken, string factKey)
    {
        var listRequest = Authorized(HttpMethod.Get, "/api/fact-definitions", adminToken);
        var listResponse = await _complianceCoreClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var existing = (await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<FactDefinitionResponse>>())!;
        var match = existing.FirstOrDefault(item =>
            string.Equals(item.FactKey, factKey, StringComparison.OrdinalIgnoreCase));
        if (match is not null)
        {
            return match.FactDefinitionId;
        }

        var request = Authorized(HttpMethod.Post, "/api/fact-definitions", adminToken);
        request.Content = JsonContent.Create(new CreateFactDefinitionRequest(
            factKey,
            factKey.Replace('_', ' '),
            "Test fact for qualification checks.",
            "boolean"));
        var response = await _complianceCoreClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var created = (await response.Content.ReadFromJsonAsync<FactDefinitionResponse>())!;
        return created.FactDefinitionId;
    }

    private async Task<Guid> CreateTrainingDefinitionAsync(string adminToken)
    {
        var definitionKey = $"rulepack_def_{Guid.NewGuid():N}"[..20];
        var request = Authorized(HttpMethod.Post, "/api/training-definitions", adminToken);
        request.Content = JsonContent.Create(new CreateTrainingDefinitionRequest(
            definitionKey,
            "Rule pack definition",
            "Training definition for rule pack requirement tests.",
            "hazmat_endorsement",
            "Hazmat Endorsement"));
        var response = await _trainarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var created = (await response.Content.ReadFromJsonAsync<TrainingDefinitionResponse>())!;
        return created.TrainingDefinitionId;
    }

    private async Task<Guid> CreateTrainingProgramAsync(string adminToken, Guid definitionId)
    {
        var programKey = $"rulepack_prog_{Guid.NewGuid():N}"[..20];
        var request = Authorized(HttpMethod.Post, "/api/training-programs", adminToken);
        request.Content = JsonContent.Create(new CreateTrainingProgramRequest(
            programKey,
            "Rule pack program",
            "Program for rule pack requirement tests.",
            [definitionId]));
        var response = await _trainarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var created = (await response.Content.ReadFromJsonAsync<TrainingProgramDetailResponse>())!;
        return created.ProgramId;
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
            $"{sourceProduct}-rulepack-{Guid.NewGuid():N}",
            $"{sourceProduct} rule pack test",
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
