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

public sealed class ComplianceCoreAuditReadyFactRequirementTests : IAsyncLifetime
{
    private const string SigningKey = "test-signing-key-at-least-32-chars-long";
    private static readonly Guid SubjectId = Guid.NewGuid();

    private WebApplicationFactory<global::ComplianceCore.Api.Program> _complianceCoreFactory = null!;
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private HttpClient _complianceCoreClient = null!;
    private HttpClient _nexarrClient = null!;
    private string _staffArrToken = null!;

    public async Task InitializeAsync()
    {
        var complianceDbName = $"ComplianceCoreAuditReadyFacts-{Guid.NewGuid():N}";
        var nexarrDbName = $"NexArrAuditReadyFacts-{Guid.NewGuid():N}";

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
        await SeedComplianceCoreAsync();

        var adminToken = await LoginNexArrAdminAsync();
        _staffArrToken = await IssueServiceTokenAsync(
            adminToken,
            "StaffArr",
            ["compliancecore"],
            ProductFactIngestionService.IngestFactsActionScope);
    }

    public async Task DisposeAsync()
    {
        _complianceCoreClient.Dispose();
        _nexarrClient.Dispose();
        await _complianceCoreFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Dq_atomic_facts_roll_up_and_create_audit_traces()
    {
        var userToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");

        var evidenceRequest = ServiceAuthorized(HttpMethod.Post, "/api/v1/evidence-references", _staffArrToken);
        evidenceRequest.Content = JsonContent.Create(new EvidenceReferenceCreateRequest(
            PlatformSeeder.DemoTenantId,
            "ev_dq_application",
            "t49_dq_application_present",
            "StaffArr",
            "driver",
            SubjectId.ToString(),
            "driver_qualification_application",
            "driver_qualification_application",
            "https://example.test/dq/application.pdf",
            null,
            "sha256:test",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            null,
            PlatformSeeder.DemoAdminUserId,
            null,
            "pending",
            "Application uploaded by StaffArr."));
        var evidenceResponse = await _complianceCoreClient.SendAsync(evidenceRequest);
        evidenceResponse.EnsureSuccessStatusCode();
        var evidence = (await evidenceResponse.Content.ReadFromJsonAsync<EvidenceReferenceResponse>())!;
        Assert.Equal("ev_dq_application", evidence.EvidenceId);

        var assertedAt = DateTimeOffset.UtcNow;
        foreach (var factKey in DqAtomicFactKeys)
        {
            await PublishFactAssertionAsync(
                factKey,
                "true",
                assertedAt,
                factKey == "t49_dq_application_present" ? evidence.EvidenceId : null);
        }

        var pass = await EvaluateDqAsync(userToken);
        Assert.Equal("pass", pass.OverallResult);
        Assert.Contains(pass.Traces, trace =>
            trace.FactKey == "t49_driver_dq_file_complete" && trace.Result == "pass");

        await PublishFactAssertionAsync("t49_dq_mvr_annual_current", "false", assertedAt.AddMinutes(1), null);
        var fail = await EvaluateDqAsync(userToken);
        Assert.Equal("fail", fail.OverallResult);
        Assert.Contains(fail.Traces, trace =>
            trace.FactKey == "t49_driver_dq_file_complete" && trace.Result == "fail");

        await PublishFactAssertionAsync("t49_dq_mvr_annual_current", "true", assertedAt.AddMinutes(2), null);
        await PublishFactAssertionAsync("t49_dq_medical_certificate_current", "false", assertedAt.AddMinutes(3), null);
        var block = await EvaluateDqAsync(
            userToken,
            new Dictionary<string, string>
            {
                ["t49_dq_medical_certificate_current"] = "Manager tried to waive medical certificate."
            });
        Assert.Equal("block", block.OverallResult);
        var medicalTrace = Assert.Single(block.Traces, trace => trace.FactKey == "t49_dq_medical_certificate_current");
        Assert.Equal("automatic_failure", medicalTrace.Result);
        Assert.Equal("critical", medicalTrace.FailureSeverity);
        Assert.False(medicalTrace.OverrideUsed);

        using var scope = _complianceCoreFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        Assert.True(await db.AuditTraces.AnyAsync(x => x.TenantId == PlatformSeeder.DemoTenantId && x.PackKey == "title49.driver.qualification_file"));
        Assert.True(await db.FactAssertions.AnyAsync(x => x.EvidenceId == "ev_dq_application"));
    }

    private async Task<AuditRequirementEvaluationResponse> EvaluateDqAsync(
        string userToken,
        IReadOnlyDictionary<string, string>? overrideReasons = null)
    {
        var request = Authorized(HttpMethod.Post, "/api/v1/audit-requirements/evaluate", userToken);
        request.Content = JsonContent.Create(new AuditRequirementEvaluationRequest(
            "title49.driver.qualification_file",
            "driver",
            SubjectId.ToString(),
            overrideReasons,
            PlatformSeeder.DemoAdminUserId));
        var response = await _complianceCoreClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuditRequirementEvaluationResponse>())!;
    }

    private async Task PublishFactAssertionAsync(
        string factKey,
        string value,
        DateTimeOffset assertedAt,
        string? evidenceId)
    {
        var request = ServiceAuthorized(HttpMethod.Post, "/api/v1/fact-assertions", _staffArrToken);
        request.Content = JsonContent.Create(new FactAssertionCreateRequest(
            PlatformSeeder.DemoTenantId,
            factKey,
            "driver",
            SubjectId.ToString(),
            value,
            "boolean",
            "StaffArr",
            $"{SubjectId:D}:{factKey}",
            evidenceId,
            assertedAt,
            assertedAt,
            null));
        var response = await _complianceCoreClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var assertion = (await response.Content.ReadFromJsonAsync<FactAssertionResponse>())!;
        Assert.Equal(evidenceId, assertion.EvidenceId);
    }

    private async Task SeedComplianceCoreAsync()
    {
        using var scope = _complianceCoreFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        await db.Database.EnsureCreatedAsync();
        var vocabularyService = scope.ServiceProvider.GetRequiredService<VocabularyService>();
        await vocabularyService.EnsureVocabularyTypesSeededAsync();

        var now = DateTimeOffset.UtcNow;
        var body = new GoverningBody
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            BodyKey = "dot",
            Label = "U.S. Department of Transportation",
            Description = "Federal transportation safety and compliance authority.",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
        var jurisdiction = new Jurisdiction
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            GoverningBodyId = body.Id,
            JurisdictionKey = "us_federal",
            Label = "United States Federal",
            Description = "Federal jurisdiction.",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
        var program = new RegulatoryProgram
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            JurisdictionId = jurisdiction.Id,
            ProgramKey = "fmcsa_fmcsr",
            Label = "FMCSA Federal Motor Carrier Safety Regulations",
            Description = "FMCSA safety compliance program.",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
        var pack = new RulePack
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            RegulatoryProgramId = program.Id,
            PackKey = "title49.driver.qualification_file",
            Label = "Driver qualification file",
            Description = "Audit-ready DQ file requirements.",
            VersionNumber = 1,
            Status = RulePackStatuses.Published,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
        var citation = new RegulatoryCitation
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            RegulatoryProgramId = program.Id,
            RulePackId = pack.Id,
            CitationKey = "t49_391_51",
            Label = "Driver qualification files",
            SourceReference = "49 CFR 391.51",
            Description = "Driver qualification file records.",
            VersionNumber = 1,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.GoverningBodies.Add(body);
        db.Jurisdictions.Add(jurisdiction);
        db.RegulatoryPrograms.Add(program);
        db.RulePacks.Add(pack);
        db.RegulatoryCitations.Add(citation);

        foreach (var factKey in DqAtomicFactKeys)
        {
            var definition = new FactDefinition
            {
                Id = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                FactKey = factKey,
                Label = factKey.Replace('_', ' '),
                Description = $"Atomic DQ audit fact {factKey}.",
                ValueType = FactValueTypes.Boolean,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };
            db.FactDefinitions.Add(definition);
            db.FactRequirements.Add(CreateRequirement(definition, pack, citation, now));
        }

        var rollup = new FactDefinition
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            FactKey = "t49_driver_dq_file_complete",
            Label = "Driver DQ file complete",
            Description = "Derived rollup over atomic DQ facts.",
            ValueType = FactValueTypes.Boolean,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
        db.FactDefinitions.Add(rollup);
        db.FactRequirements.Add(new FactRequirement
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            FactDefinitionId = rollup.Id,
            RulePackId = pack.Id,
            CitationId = citation.Id,
            RequirementKey = "req_t49_driver_dq_file_complete",
            Label = rollup.Label,
            Description = rollup.Description,
            ApplicabilityKey = "motor_carrier_driver",
            SourceProduct = "ComplianceCore",
            SourceEntity = "driver",
            SourceFieldOrRecordType = "derived_dq_file_rollup",
            ValueType = FactValueTypes.Boolean,
            Operator = FactRequirementOperators.AllTrue,
            ExpectedValue = string.Join(',', DqAtomicFactKeys),
            EvidenceKind = FactRequirementEvidenceKinds.DerivedFact,
            RequiredDocumentType = string.Empty,
            RetentionPeriod = "49_cfr_391_51",
            AuditQuestion = "Are all atomic DQ file facts satisfied?",
            FailureSeverity = FactRequirementFailureSeverities.Major,
            AutomaticFailureFlag = false,
            OverrideAllowed = false,
            OverridePermission = string.Empty,
            RemediationRequired = false,
            IsRequired = true,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        await db.SaveChangesAsync();
    }

    private static FactRequirement CreateRequirement(
        FactDefinition definition,
        RulePack pack,
        RegulatoryCitation citation,
        DateTimeOffset now)
    {
        var nonWaivable = definition.FactKey == "t49_dq_medical_certificate_current";
        return new FactRequirement
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            FactDefinitionId = definition.Id,
            RulePackId = pack.Id,
            CitationId = citation.Id,
            RequirementKey = $"req_{definition.FactKey}",
            Label = definition.Label,
            Description = definition.Description,
            ApplicabilityKey = "motor_carrier_driver",
            SourceProduct = "StaffArr",
            SourceEntity = "driver",
            SourceFieldOrRecordType = definition.FactKey.Replace("t49_dq_", string.Empty),
            ValueType = FactValueTypes.Boolean,
            Operator = FactRequirementOperators.Equal,
            ExpectedValue = "true",
            EvidenceKind = FactRequirementEvidenceKinds.ProductRecord,
            RequiredDocumentType = "driver_qualification_file_record",
            RetentionPeriod = "49_cfr_391_51",
            AuditQuestion = $"Is {definition.Label}?",
            FailureSeverity = nonWaivable
                ? FactRequirementFailureSeverities.Critical
                : FactRequirementFailureSeverities.Major,
            AutomaticFailureFlag = nonWaivable,
            OverrideAllowed = !nonWaivable,
            OverridePermission = nonWaivable ? string.Empty : "compliance.override.title49",
            RemediationRequired = true,
            IsRequired = true,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private static readonly IReadOnlyList<string> DqAtomicFactKeys =
    [
        "t49_dq_application_present",
        "t49_dq_mvr_initial_present",
        "t49_dq_mvr_annual_current",
        "t49_dq_medical_certificate_current",
        "t49_dq_road_test_or_equivalent_present",
        "t49_dq_prior_employer_inquiry_complete",
        "t49_dq_annual_violation_review_complete"
    ];

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

    private async Task<string> IssueServiceTokenAsync(
        string adminToken,
        string sourceProduct,
        IReadOnlyList<string> allowedProducts,
        string actionScope)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{sourceProduct}-audit-ready-facts-{Guid.NewGuid():N}",
            $"{sourceProduct} audit-ready facts test",
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

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }

    private static HttpRequestMessage ServiceAuthorized(HttpMethod method, string url, string serviceToken) =>
        Authorized(method, url, serviceToken);

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
