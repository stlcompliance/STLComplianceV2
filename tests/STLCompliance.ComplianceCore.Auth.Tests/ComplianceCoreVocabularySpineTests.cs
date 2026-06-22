using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using ComplianceCore.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Data;
using NexArr.Api.Services;

namespace STLCompliance.ComplianceCore.Auth.Tests;

public class ComplianceCoreVocabularySpineTests : IAsyncLifetime
{
    private WebApplicationFactory<global::ComplianceCore.Api.Program> _complianceCoreFactory = null!;
    private HttpClient _complianceCoreClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"ComplianceCoreVocab-{Guid.NewGuid():N}";

        _complianceCoreFactory = new WebApplicationFactory<global::ComplianceCore.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<ComplianceCoreDbContext>(services);
                services.AddDbContext<ComplianceCoreDbContext>(options => options.UseInMemoryDatabase(dbName));
            });
        });

        _complianceCoreClient = _complianceCoreFactory.CreateClient();

        using var scope = _complianceCoreFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        await db.Database.EnsureCreatedAsync();
        var vocabularyService = scope.ServiceProvider.GetRequiredService<VocabularyService>();
        await vocabularyService.EnsureVocabularyTypesSeededAsync();
    }

    public async Task DisposeAsync()
    {
        _complianceCoreClient.Dispose();
        await _complianceCoreFactory.DisposeAsync();
    }

    [Fact]
    public async Task Vocabulary_types_returns_seventeen_controlled_keys()
    {
        var token = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "tenant_admin");
        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/vocabulary/types", token));

        response.EnsureSuccessStatusCode();
        var types = (await response.Content.ReadFromJsonAsync<IReadOnlyList<VocabularyTypeResponse>>())!;
        Assert.Equal(17, types.Count);
        Assert.Contains(types, t => t.TypeKey == "material_hazard");
        Assert.Contains(types, t => t.TypeKey == "incident_reason");
        Assert.Contains(types, t => t.TypeKey == "evidence_type");
    }

    [Fact]
    public async Task Core_vocabulary_key_registry_returns_documented_fourteen_keys()
    {
        var token = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "tenant_admin");
        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/vocabulary/core-keys", token));

        response.EnsureSuccessStatusCode();
        var registry = (await response.Content.ReadFromJsonAsync<CoreVocabularyKeyRegistryResponse>())!;
        Assert.Equal(14, registry.Keys.Count);
        Assert.Equal(
            [
                "governing_body_key",
                "regulatory_program_key",
                "regulated_context_key",
                "subject_type_key",
                "activity_context_key",
                "material_key",
                "hazard_class_key",
                "equipment_class_key",
                "training_requirement_key",
                "inspection_type_key",
                "permit_type_key",
                "incident_report_type_key",
                "evidence_type_key",
                "record_retention_key"
            ],
            registry.Keys.Select(key => key.Key).ToArray());
    }

    [Fact]
    public async Task Core_vocabulary_key_validation_reports_unknown_keys()
    {
        var token = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "tenant_admin");
        var request = Authorized(HttpMethod.Post, "/api/v1/vocabulary/core-keys/validate", token);
        request.Content = JsonContent.Create(new ValidateCoreVocabularyKeysRequest([
            "material_key",
            "dispatch_category",
            ""
        ]));

        var response = await _complianceCoreClient.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var result = (await response.Content.ReadFromJsonAsync<ValidateCoreVocabularyKeysResponse>())!;
        Assert.Collection(
            result.Items,
            item =>
            {
                Assert.Equal("material_key", item.Key);
                Assert.True(item.IsKnown);
                Assert.Null(item.ReasonCode);
            },
            item =>
            {
                Assert.Equal("dispatch_category", item.Key);
                Assert.False(item.IsKnown);
                Assert.Equal("unknown_core_key", item.ReasonCode);
            },
            item =>
            {
                Assert.Equal(string.Empty, item.Key);
                Assert.False(item.IsKnown);
                Assert.Equal("empty_key", item.ReasonCode);
            });
    }

    [Fact]
    public async Task Vocabulary_term_create_and_list_with_alias()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");

        var createTermRequest = Authorized(HttpMethod.Post, "/api/vocabulary", adminToken);
        createTermRequest.Content = JsonContent.Create(new CreateVocabularyTermRequest(
            "flammable",
            "Flammable",
            "material_hazard",
            "Can ignite under defined conditions."));
        var createTermResponse = await _complianceCoreClient.SendAsync(createTermRequest);
        createTermResponse.EnsureSuccessStatusCode();
        var term = (await createTermResponse.Content.ReadFromJsonAsync<VocabularyTermResponse>())!;

        var createAliasRequest = Authorized(HttpMethod.Post, "/api/vocabulary/aliases", adminToken);
        createAliasRequest.Content = JsonContent.Create(new CreateVocabularyAliasRequest(
            term.TermId,
            "Fire hazard"));
        var createAliasResponse = await _complianceCoreClient.SendAsync(createAliasRequest);
        createAliasResponse.EnsureSuccessStatusCode();

        var listResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/vocabulary?vocabularyTypeKey=material_hazard", adminToken));
        listResponse.EnsureSuccessStatusCode();
        var terms = (await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<VocabularyTermResponse>>())!;
        Assert.Single(terms);
        Assert.Equal("flammable", terms[0].TermKey);
        Assert.Contains("Fire hazard", terms[0].Aliases);
    }

    [Fact]
    public async Task Compliance_key_create_denies_member_role()
    {
        var memberToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "tenant_member");
        var request = Authorized(HttpMethod.Post, "/api/compliance-keys", memberToken);
        request.Content = JsonContent.Create(new CreateComplianceKeyRequest(
            "driver_qualification",
            "Driver Qualification",
            "compliance_domain",
            "Driver qualification requirement domain."));
        var response = await _complianceCoreClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Material_key_create_and_list_for_compliance_admin()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");

        var createRequest = Authorized(HttpMethod.Post, "/api/material-keys", adminToken);
        createRequest.Content = JsonContent.Create(new CreateMaterialKeyRequest(
            "gas",
            "Gas",
            "physical_state",
            "Material exists as gas under defined conditions."));
        var createResponse = await _complianceCoreClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();

        var listResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/material-keys", adminToken));
        listResponse.EnsureSuccessStatusCode();
        var keys = (await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<MaterialKeyResponse>>())!;
        Assert.Single(keys);
        Assert.Equal("gas", keys[0].Key);
    }

    [Fact]
    public async Task Vocabulary_read_requires_compliancecore_entitlement()
    {
        var token = CreateComplianceCoreAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/vocabulary/types", token));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task V1_vocabulary_aliases_match_primary_endpoints()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");

        var createTermRequest = Authorized(HttpMethod.Post, "/api/v1/vocabulary", adminToken);
        createTermRequest.Content = JsonContent.Create(new CreateVocabularyTermRequest(
            "oxidizer",
            "Oxidizer",
            "material_hazard",
            "Supports combustion."));
        var createTermResponse = await _complianceCoreClient.SendAsync(createTermRequest);
        createTermResponse.EnsureSuccessStatusCode();
        var term = (await createTermResponse.Content.ReadFromJsonAsync<VocabularyTermResponse>())!;

        var createAliasRequest = Authorized(HttpMethod.Post, "/api/v1/vocabulary/aliases", adminToken);
        createAliasRequest.Content = JsonContent.Create(new CreateVocabularyAliasRequest(
            term.TermId,
            "Oxidizing agent"));
        var createAliasResponse = await _complianceCoreClient.SendAsync(createAliasRequest);
        createAliasResponse.EnsureSuccessStatusCode();

        var legacyTypesResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/vocabulary/types", adminToken));
        legacyTypesResponse.EnsureSuccessStatusCode();
        var legacyTypes = (await legacyTypesResponse.Content.ReadFromJsonAsync<IReadOnlyList<VocabularyTypeResponse>>())!;

        var v1TypesResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/vocabulary/types", adminToken));
        v1TypesResponse.EnsureSuccessStatusCode();
        var v1Types = (await v1TypesResponse.Content.ReadFromJsonAsync<IReadOnlyList<VocabularyTypeResponse>>())!;
        Assert.Equal(legacyTypes.Count, v1Types.Count);

        var legacyTermsResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/vocabulary?vocabularyTypeKey=material_hazard", adminToken));
        legacyTermsResponse.EnsureSuccessStatusCode();
        var legacyTerms = (await legacyTermsResponse.Content.ReadFromJsonAsync<IReadOnlyList<VocabularyTermResponse>>())!;

        var v1TermsResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/vocabulary?vocabularyTypeKey=material_hazard", adminToken));
        v1TermsResponse.EnsureSuccessStatusCode();
        var v1Terms = (await v1TermsResponse.Content.ReadFromJsonAsync<IReadOnlyList<VocabularyTermResponse>>())!;
        Assert.Equal(legacyTerms.Count, v1Terms.Count);
    }

    [Fact]
    public async Task V1_vocabulary_family_routes_create_list_and_validate_family()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");

        var createRequest = Authorized(HttpMethod.Post, "/api/v1/vocabulary/material_hazard", adminToken);
        createRequest.Content = JsonContent.Create(new CreateVocabularyTermRequest(
            "corrosive",
            "Corrosive",
            "material_hazard",
            "Can damage materials or tissue on contact."));
        var createResponse = await _complianceCoreClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<VocabularyTermResponse>())!;
        Assert.Equal("material_hazard", created.VocabularyTypeKey);

        var listResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/vocabulary/material_hazard", adminToken));
        listResponse.EnsureSuccessStatusCode();
        var terms = (await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<VocabularyTermResponse>>())!;
        Assert.Contains(terms, term => term.TermKey == "corrosive");

        var mismatchRequest = Authorized(HttpMethod.Post, "/api/v1/vocabulary/evidence_type", adminToken);
        mismatchRequest.Content = JsonContent.Create(new CreateVocabularyTermRequest(
            "not_evidence",
            "Not Evidence",
            "material_hazard",
            "This intentionally mismatches the route family."));
        var mismatchResponse = await _complianceCoreClient.SendAsync(mismatchRequest);
        Assert.Equal(HttpStatusCode.BadRequest, mismatchResponse.StatusCode);
    }

    [Fact]
    public async Task V1_vocabulary_key_routes_patch_and_validate_keys()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");

        var createRequest = Authorized(HttpMethod.Post, "/api/v1/vocabulary/material_hazard", adminToken);
        createRequest.Content = JsonContent.Create(new CreateVocabularyTermRequest(
            "corrosive",
            "Corrosive",
            "material_hazard",
            "Can damage materials or tissue on contact."));
        var createResponse = await _complianceCoreClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();

        var updateRequest = Authorized(HttpMethod.Patch, "/api/v1/vocabulary/material_hazard/corrosive", adminToken);
        updateRequest.Content = JsonContent.Create(new UpdateVocabularyTermRequest(
            "Corrosive material",
            "Can damage materials, equipment, or tissue on contact.",
            IsActive: true));
        var updateResponse = await _complianceCoreClient.SendAsync(updateRequest);
        updateResponse.EnsureSuccessStatusCode();
        var updated = (await updateResponse.Content.ReadFromJsonAsync<VocabularyTermResponse>())!;
        Assert.Equal("corrosive", updated.TermKey);
        Assert.Equal("Corrosive material", updated.Label);
        Assert.Equal("Can damage materials, equipment, or tissue on contact.", updated.Description);

        var validateRequest = Authorized(HttpMethod.Post, "/api/v1/vocabulary/validate-keys", adminToken);
        validateRequest.Content = JsonContent.Create(new ValidateVocabularyKeysRequest([
            new ValidateVocabularyKeyItem("material_hazard", "corrosive"),
            new ValidateVocabularyKeyItem("material_hazard", "unknown"),
            new ValidateVocabularyKeyItem("unknown_family", "corrosive")
        ]));
        var validateResponse = await _complianceCoreClient.SendAsync(validateRequest);
        validateResponse.EnsureSuccessStatusCode();
        var validation = (await validateResponse.Content.ReadFromJsonAsync<ValidateVocabularyKeysResponse>())!;
        Assert.Collection(
            validation.Items,
            item =>
            {
                Assert.Equal("material_hazard", item.Family);
                Assert.Equal("corrosive", item.Key);
                Assert.True(item.IsValid);
                Assert.Null(item.ReasonCode);
                Assert.Equal(updated.TermId, item.TermId);
            },
            item =>
            {
                Assert.Equal("material_hazard", item.Family);
                Assert.Equal("unknown", item.Key);
                Assert.False(item.IsValid);
                Assert.Equal("unknown_key", item.ReasonCode);
                Assert.Null(item.TermId);
            },
            item =>
            {
                Assert.Equal("unknown_family", item.Family);
                Assert.Equal("corrosive", item.Key);
                Assert.False(item.IsValid);
                Assert.Equal("unknown_family", item.ReasonCode);
                Assert.Null(item.TermId);
            });

        var createAliasRequest = Authorized(HttpMethod.Post, "/api/v1/vocabulary/aliases", adminToken);
        createAliasRequest.Content = JsonContent.Create(new CreateVocabularyAliasRequest(
            updated.TermId,
            "Caustic material"));
        var createAliasResponse = await _complianceCoreClient.SendAsync(createAliasRequest);
        createAliasResponse.EnsureSuccessStatusCode();

        await CreateFactRequirementForVocabularyTermAsync(updated.TermKey);

        var usageResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/vocabulary/material_hazard/corrosive/usage", adminToken));
        usageResponse.EnsureSuccessStatusCode();
        var usage = (await usageResponse.Content.ReadFromJsonAsync<VocabularyTermUsageResponse>())!;
        Assert.Equal("material_hazard", usage.Family);
        Assert.Equal("corrosive", usage.Key);
        Assert.Equal(1, usage.AliasCount);
        Assert.Equal(1, usage.FactRequirementCount);

        var historyResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/vocabulary/material_hazard/corrosive/history", adminToken));
        historyResponse.EnsureSuccessStatusCode();
        var history = (await historyResponse.Content.ReadFromJsonAsync<IReadOnlyList<VocabularyTermHistoryItemResponse>>())!;
        Assert.Contains(history, item => item.Action == "vocabulary.term.create" && item.TermId == updated.TermId);
        Assert.Contains(history, item => item.Action == "vocabulary.term.update" && item.TermId == updated.TermId);
    }

    private async Task CreateFactRequirementForVocabularyTermAsync(string termKey)
    {
        using var scope = _complianceCoreFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        var now = DateTimeOffset.UtcNow;
        var factDefinitionId = Guid.NewGuid();
        db.FactDefinitions.Add(new FactDefinition
        {
            Id = factDefinitionId,
            TenantId = PlatformSeeder.DemoTenantId,
            FactKey = "material_hazard_key",
            Label = "Material hazard key",
            Description = "Material hazard controlled vocabulary value.",
            ValueType = FactValueTypes.String,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        });
        db.FactRequirements.Add(new FactRequirement
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            FactDefinitionId = factDefinitionId,
            RequirementKey = "material_hazard_matches",
            Label = "Material hazard matches",
            Description = "Requires the material hazard controlled vocabulary value.",
            ApplicabilityKey = "material_hazard",
            SourceProduct = "supplyarr",
            SourceEntity = "material",
            SourceFieldOrRecordType = "hazard_key",
            ValueType = FactValueTypes.String,
            Operator = FactRequirementOperators.Equal,
            ExpectedValue = termKey,
            EvidenceKind = FactRequirementEvidenceKinds.SystemFact,
            RequiredDocumentType = "none",
            RetentionPeriod = "P1Y",
            AuditQuestion = "Is the material hazard key approved?",
            FailureSeverity = FactRequirementFailureSeverities.Major,
            IsRequired = true,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        });
        await db.SaveChangesAsync();
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
