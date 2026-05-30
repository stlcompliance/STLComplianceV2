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

namespace STLCompliance.StaffArr.Auth.Tests;

public class StaffArrTrainArrQualificationCheckTests : IAsyncLifetime
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
        var nexArrDbName = $"QualCheckNexArr-{Guid.NewGuid():N}";
        var complianceDbName = $"QualCheckCompliance-{Guid.NewGuid():N}";
        var trainArrDbName = $"QualCheckTrainArr-{Guid.NewGuid():N}";

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
            InternalRuleEvaluationService.EvaluateActionScope);

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
                RemoveDbContext<TrainArr.Api.Data.TrainArrDbContext>(services);
                services.AddDbContext<TrainArr.Api.Data.TrainArrDbContext>(options =>
                    options.UseInMemoryDatabase(trainArrDbName));

                services.AddHttpClient<TrainArr.Api.Services.ComplianceCoreRuleEvaluationClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _complianceCoreFactory.Server.CreateHandler());
            });
        });

        _trainarrClient = _trainarrFactory.CreateClient();

        using var scope = _complianceCoreFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        await db.Database.EnsureCreatedAsync();
        var vocabularyService = scope.ServiceProvider.GetRequiredService<VocabularyService>();
        await vocabularyService.EnsureVocabularyTypesSeededAsync();

        var complianceAdminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        await SeedDriverQualificationRulePackAsync(complianceAdminToken, booleanValue: true);
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
    public async Task Qualification_check_warns_without_local_qualification_when_rules_pass()
    {
        var personId = Guid.NewGuid();
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");

        var check = await RunQualificationCheckAsync(
            adminToken,
            personId,
            "hazmat_endorsement",
            "driver_qualification");

        Assert.Equal(QualificationCheckOutcomes.Warn, check.Outcome);
        Assert.Equal("local_no_qualification", check.ReasonCode);
        Assert.NotNull(check.ComplianceCore);
        Assert.Equal(QualificationCheckOutcomes.Allow, check.ComplianceCore!.Outcome);
    }

    [Fact]
    public async Task Qualification_check_blocks_when_compliance_rules_fail()
    {
        var complianceAdminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        await SeedDriverQualificationRulePackAsync(
            complianceAdminToken,
            packKey: "driver_qualification_fail",
            booleanValue: false);

        var personId = Guid.NewGuid();
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var check = await RunQualificationCheckAsync(
            adminToken,
            personId,
            "hazmat_endorsement",
            "driver_qualification_fail");

        Assert.Equal(QualificationCheckOutcomes.Block, check.Outcome);
        Assert.Equal("rule_evaluation_failed", check.ReasonCode);
        Assert.NotNull(check.ComplianceCore);
        Assert.Equal(QualificationCheckOutcomes.Block, check.ComplianceCore!.Outcome);
    }

    [Fact]
    public async Task Qualification_check_blocks_suspended_local_qualification()
    {
        var personId = Guid.NewGuid();
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        await SeedSuspendedQualificationIssueAsync(personId, "hazmat_endorsement");

        var check = await RunQualificationCheckAsync(
            adminToken,
            personId,
            "hazmat_endorsement",
            "driver_qualification");

        Assert.Equal(QualificationCheckOutcomes.Block, check.Outcome);
        Assert.Equal("local_suspended", check.ReasonCode);
        Assert.Equal("suspended", check.LocalQualification!.Status);
    }

    [Fact]
    public async Task Qualification_check_honors_effective_time_for_expiration_state()
    {
        var personId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        await SeedIssuedQualificationIssueAsync(
            personId,
            "hazmat_endorsement",
            issuedAt: now.AddDays(-5),
            expiresAt: now.AddDays(-1));

        var current = await RunQualificationCheckAsync(
            adminToken,
            personId,
            "hazmat_endorsement",
            "driver_qualification");
        Assert.Equal(QualificationCheckOutcomes.Block, current.Outcome);
        Assert.Equal("expired", current.LocalQualification!.Status);

        var pointInTime = await RunQualificationCheckAsync(
            adminToken,
            personId,
            "hazmat_endorsement",
            "driver_qualification",
            effectiveAt: now.AddDays(-2));
        Assert.Equal(QualificationCheckOutcomes.Allow, pointInTime.Outcome);
        Assert.Equal("issued", pointInTime.LocalQualification!.Status);
    }

    [Fact]
    public async Task Qualification_check_denies_unrelated_member_for_other_person()
    {
        var subjectPersonId = Guid.NewGuid();
        var memberToken = CreateTrainArrAccessToken(
            ["trainarr"],
            tenantRoleKey: "tenant_member",
            personId: PlatformSeeder.DemoAdminUserId);

        var request = Authorized(HttpMethod.Post, "/api/qualification-checks", memberToken);
        request.Content = JsonContent.Create(new CreateQualificationCheckRequest(
            subjectPersonId,
            "hazmat_endorsement",
            "driver_qualification",
            null));
        var response = await _trainarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Qualification_check_v1_alias_matches_primary_endpoint()
    {
        var personId = Guid.NewGuid();
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");

        var primary = await RunQualificationCheckAsync(
            adminToken,
            personId,
            "hazmat_endorsement",
            "driver_qualification");
        var v1 = await RunQualificationCheckAsync(
            adminToken,
            personId,
            "hazmat_endorsement",
            "driver_qualification",
            "/api/v1/qualification-checks");

        Assert.Equal(primary.Outcome, v1.Outcome);
        Assert.Equal(primary.ReasonCode, v1.ReasonCode);
        Assert.Equal(primary.ComplianceCore?.Outcome, v1.ComplianceCore?.Outcome);
    }

    [Fact]
    public async Task Authorization_check_v1_alias_matches_qualification_check_endpoint()
    {
        var personId = Guid.NewGuid();
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");

        var qualificationCheck = await RunQualificationCheckAsync(
            adminToken,
            personId,
            "hazmat_endorsement",
            "driver_qualification",
            "/api/v1/qualification-checks");
        var authorizationCheck = await RunQualificationCheckAsync(
            adminToken,
            personId,
            "hazmat_endorsement",
            "driver_qualification",
            "/api/v1/authorization-checks");

        Assert.Equal(qualificationCheck.Outcome, authorizationCheck.Outcome);
        Assert.Equal(qualificationCheck.ReasonCode, authorizationCheck.ReasonCode);
        Assert.Equal(qualificationCheck.ComplianceCore?.Outcome, authorizationCheck.ComplianceCore?.Outcome);
    }

    private async Task<QualificationCheckResponse> RunQualificationCheckAsync(
        string trainarrToken,
        Guid personId,
        string qualificationKey,
        string rulePackKey,
        string endpoint = "/api/qualification-checks",
        DateTimeOffset? effectiveAt = null)
    {
        var request = Authorized(HttpMethod.Post, endpoint, trainarrToken);
        request.Content = JsonContent.Create(new CreateQualificationCheckRequest(
            personId,
            qualificationKey,
            rulePackKey,
            null,
            effectiveAt));
        var response = await _trainarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<QualificationCheckResponse>())!;
    }

    private async Task SeedIssuedQualificationIssueAsync(
        Guid personId,
        string qualificationKey,
        DateTimeOffset issuedAt,
        DateTimeOffset? expiresAt)
    {
        using var scope = _trainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArr.Api.Data.TrainArrDbContext>();
        var now = DateTimeOffset.UtcNow;
        var assignmentId = Guid.NewGuid();

        db.TrainingAssignments.Add(new TrainArr.Api.Entities.TrainingAssignment
        {
            Id = assignmentId,
            TenantId = PlatformSeeder.DemoTenantId,
            StaffarrPersonId = personId,
            TrainingDefinitionId = Guid.NewGuid(),
            AssignmentReason = "manual",
            Status = "completed",
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.QualificationIssues.Add(new TrainArr.Api.Entities.QualificationIssue
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            TrainingAssignmentId = assignmentId,
            StaffarrPersonId = personId,
            QualificationKey = qualificationKey,
            QualificationName = "Hazmat Endorsement",
            GrantPublicationId = Guid.NewGuid(),
            Status = "issued",
            IssuedAt = issuedAt,
            ExpiresAt = expiresAt,
            CreatedAt = now,
            UpdatedAt = now,
        });

        await db.SaveChangesAsync();
    }

    private async Task SeedSuspendedQualificationIssueAsync(Guid personId, string qualificationKey)
    {
        using var scope = _trainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArr.Api.Data.TrainArrDbContext>();
        var now = DateTimeOffset.UtcNow;
        var assignmentId = Guid.NewGuid();

        db.TrainingAssignments.Add(new TrainArr.Api.Entities.TrainingAssignment
        {
            Id = assignmentId,
            TenantId = PlatformSeeder.DemoTenantId,
            StaffarrPersonId = personId,
            TrainingDefinitionId = Guid.NewGuid(),
            AssignmentReason = "manual",
            Status = "completed",
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.QualificationIssues.Add(new TrainArr.Api.Entities.QualificationIssue
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            TrainingAssignmentId = assignmentId,
            StaffarrPersonId = personId,
            QualificationKey = qualificationKey,
            QualificationName = "Hazmat Endorsement",
            GrantPublicationId = Guid.NewGuid(),
            Status = "suspended",
            IssuedAt = now.AddDays(-30),
            StatusChangedAt = now,
            LifecycleReason = "Suspended for test.",
            CreatedAt = now,
            UpdatedAt = now,
        });

        await db.SaveChangesAsync();
    }

    private async Task SeedDriverQualificationRulePackAsync(
        string adminToken,
        bool booleanValue,
        string packKey = "driver_qualification")
    {
        var programId = await CreateSampleProgramAsync(adminToken);
        var createPackRequest = Authorized(HttpMethod.Post, "/api/rule-packs", adminToken);
        createPackRequest.Content = JsonContent.Create(new CreateRulePackRequest(
            programId,
            packKey,
            "Driver Qualification Rules",
            "TrainArr authorization check rule pack."));
        var createPackResponse = await _complianceCoreClient.SendAsync(createPackRequest);
        createPackResponse.EnsureSuccessStatusCode();
        var pack = (await createPackResponse.Content.ReadFromJsonAsync<RulePackResponse>())!;

        var factKey = booleanValue ? "driver_license_valid" : "driver_license_strict";
        var factDefinitionId = await CreateBooleanFactDefinitionAsync(adminToken, factKey);
        var createSourceRequest = Authorized(HttpMethod.Post, "/api/fact-sources", adminToken);
        createSourceRequest.Content = JsonContent.Create(new CreateFactSourceRequest(
            factDefinitionId,
            $"default_license_flag_{packKey}",
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
            $"{sourceProduct}-qual-check-{Guid.NewGuid():N}",
            $"{sourceProduct} qualification check test",
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
