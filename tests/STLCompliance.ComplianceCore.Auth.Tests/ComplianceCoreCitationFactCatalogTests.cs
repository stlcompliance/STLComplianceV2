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
using NexArr.Api.Services;

namespace STLCompliance.ComplianceCore.Auth.Tests;

public class ComplianceCoreCitationFactCatalogTests : IAsyncLifetime
{
    private WebApplicationFactory<global::ComplianceCore.Api.Program> _complianceCoreFactory = null!;
    private HttpClient _complianceCoreClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"ComplianceCoreCitationFact-{Guid.NewGuid():N}";

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
    public async Task Citation_create_list_and_versioning()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var (programId, rulePackId) = await CreateSampleRulePackAsync(adminToken);

        var createRequest = Authorized(HttpMethod.Post, "/api/citations", adminToken);
        createRequest.Content = JsonContent.Create(new CreateRegulatoryCitationRequest(
            programId,
            rulePackId,
            "cfr_391_11",
            "General qualifications of drivers",
            "49 CFR 391.11",
            "General driver qualification requirements.",
            null));
        var createResponse = await _complianceCoreClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<RegulatoryCitationResponse>())!;
        Assert.Equal(1, created.VersionNumber);
        Assert.Equal("fmcsa_safety", created.RegulatoryProgramKey);
        Assert.Equal("driver_qualification", created.RulePackKey);

        var revisedRequest = Authorized(HttpMethod.Post, "/api/citations", adminToken);
        revisedRequest.Content = JsonContent.Create(new CreateRegulatoryCitationRequest(
            programId,
            rulePackId,
            "cfr_391_11",
            "General qualifications of drivers (revised)",
            "49 CFR 391.11(b)",
            "Revised general driver qualification requirements.",
            created.CitationId));
        var revisedResponse = await _complianceCoreClient.SendAsync(revisedRequest);
        revisedResponse.EnsureSuccessStatusCode();
        var revised = (await revisedResponse.Content.ReadFromJsonAsync<RegulatoryCitationResponse>())!;
        Assert.Equal(2, revised.VersionNumber);
        Assert.Equal(created.CitationId, revised.SupersedesCitationId);

        var listResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/citations?rulePackId={rulePackId}", adminToken));
        listResponse.EnsureSuccessStatusCode();
        var citations = (await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<RegulatoryCitationResponse>>())!;
        Assert.Equal(2, citations.Count);
    }

    [Fact]
    public async Task Fact_catalog_create_and_link_to_rule_pack()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var (_, rulePackId) = await CreateSampleRulePackAsync(adminToken);

        var factRequest = Authorized(HttpMethod.Post, "/api/fact-definitions", adminToken);
        factRequest.Content = JsonContent.Create(new CreateFactDefinitionRequest(
            "driver_license_valid",
            "Valid driver license",
            "Driver holds a valid commercial driver license.",
            "boolean"));
        var factResponse = await _complianceCoreClient.SendAsync(factRequest);
        factResponse.EnsureSuccessStatusCode();
        var fact = (await factResponse.Content.ReadFromJsonAsync<FactDefinitionResponse>())!;

        var requirementRequest = Authorized(HttpMethod.Post, "/api/fact-requirements", adminToken);
        requirementRequest.Content = JsonContent.Create(new CreateFactRequirementRequest(
            fact.FactDefinitionId,
            rulePackId,
            null,
            "dq_license_check",
            "License validity check",
            "Driver license must be valid for driver qualification.",
            true));
        var requirementResponse = await _complianceCoreClient.SendAsync(requirementRequest);
        requirementResponse.EnsureSuccessStatusCode();
        var requirement = (await requirementResponse.Content.ReadFromJsonAsync<FactRequirementResponse>())!;
        Assert.Equal("driver_license_valid", requirement.FactKey);
        Assert.Equal("driver_qualification", requirement.RulePackKey);

        var listResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/fact-requirements?rulePackId={rulePackId}", adminToken));
        listResponse.EnsureSuccessStatusCode();
        var requirements = (await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<FactRequirementResponse>>())!;
        Assert.Single(requirements);
    }

    [Fact]
    public async Task Citation_create_denies_member_role()
    {
        var memberToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "tenant_member");
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var (programId, rulePackId) = await CreateSampleRulePackAsync(adminToken);

        var request = Authorized(HttpMethod.Post, "/api/citations", memberToken);
        request.Content = JsonContent.Create(new CreateRegulatoryCitationRequest(
            programId,
            rulePackId,
            "cfr_391_11",
            "General qualifications of drivers",
            "49 CFR 391.11",
            "General driver qualification requirements.",
            null));
        var response = await _complianceCoreClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Fact_requirement_create_requires_rule_pack_or_citation()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");

        var factRequest = Authorized(HttpMethod.Post, "/api/fact-definitions", adminToken);
        factRequest.Content = JsonContent.Create(new CreateFactDefinitionRequest(
            "driver_license_valid",
            "Valid driver license",
            "Driver holds a valid commercial driver license.",
            "boolean"));
        var fact = (await (await _complianceCoreClient.SendAsync(factRequest)).Content.ReadFromJsonAsync<FactDefinitionResponse>())!;

        var requirementRequest = Authorized(HttpMethod.Post, "/api/fact-requirements", adminToken);
        requirementRequest.Content = JsonContent.Create(new CreateFactRequirementRequest(
            fact.FactDefinitionId,
            null,
            null,
            "orphan_requirement",
            "Orphan requirement",
            "Requirement without linkage.",
            true));
        var response = await _complianceCoreClient.SendAsync(requirementRequest);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Citation_read_requires_compliancecore_entitlement()
    {
        var token = CreateComplianceCoreAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/citations", token));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task V1_citations_and_facts_aliases_match_primary_endpoints()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var (programId, rulePackId) = await CreateSampleRulePackAsync(adminToken);

        var createCitationRequest = Authorized(HttpMethod.Post, "/api/v1/citations", adminToken);
        createCitationRequest.Content = JsonContent.Create(new CreateRegulatoryCitationRequest(
            programId,
            rulePackId,
            "cfr_v1_alias",
            "V1 alias citation",
            "49 CFR V1",
            "Created through v1 citations alias.",
            null));
        var createCitationResponse = await _complianceCoreClient.SendAsync(createCitationRequest);
        createCitationResponse.EnsureSuccessStatusCode();

        var createFactRequest = Authorized(HttpMethod.Post, "/api/v1/facts", adminToken);
        createFactRequest.Content = JsonContent.Create(new CreateFactDefinitionRequest(
            "v1_alias_fact",
            "V1 alias fact",
            "Created through v1 facts alias.",
            "boolean"));
        var createFactResponse = await _complianceCoreClient.SendAsync(createFactRequest);
        createFactResponse.EnsureSuccessStatusCode();

        var legacyCitationsResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/citations?rulePackId={rulePackId}", adminToken));
        legacyCitationsResponse.EnsureSuccessStatusCode();
        var legacyCitations = (await legacyCitationsResponse.Content.ReadFromJsonAsync<IReadOnlyList<RegulatoryCitationResponse>>())!;

        var v1CitationsResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/citations?rulePackId={rulePackId}", adminToken));
        v1CitationsResponse.EnsureSuccessStatusCode();
        var v1Citations = (await v1CitationsResponse.Content.ReadFromJsonAsync<IReadOnlyList<RegulatoryCitationResponse>>())!;
        Assert.Equal(legacyCitations.Count, v1Citations.Count);

        var legacyFactsResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/fact-definitions", adminToken));
        legacyFactsResponse.EnsureSuccessStatusCode();
        var legacyFacts = (await legacyFactsResponse.Content.ReadFromJsonAsync<IReadOnlyList<FactDefinitionResponse>>())!;

        var v1FactsResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/facts", adminToken));
        v1FactsResponse.EnsureSuccessStatusCode();
        var v1Facts = (await v1FactsResponse.Content.ReadFromJsonAsync<IReadOnlyList<FactDefinitionResponse>>())!;
        Assert.Equal(legacyFacts.Count, v1Facts.Count);
    }

    [Fact]
    public async Task V1_citation_and_fact_detail_routes_support_read_update_history_usage_and_validation()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var (programId, rulePackId) = await CreateSampleRulePackAsync(adminToken);

        var createCitationRequest = Authorized(HttpMethod.Post, "/api/v1/citations", adminToken);
        createCitationRequest.Content = JsonContent.Create(new CreateRegulatoryCitationRequest(
            programId,
            rulePackId,
            "cfr_detail_routes",
            "Detail route citation",
            "49 CFR detail",
            "Detail route description",
            null));
        var createdCitation = (await (await _complianceCoreClient.SendAsync(createCitationRequest))
            .Content.ReadFromJsonAsync<RegulatoryCitationResponse>())!;

        var createCitationRevisionRequest = Authorized(HttpMethod.Post, "/api/v1/citations", adminToken);
        createCitationRevisionRequest.Content = JsonContent.Create(new CreateRegulatoryCitationRequest(
            programId,
            rulePackId,
            "cfr_detail_routes",
            "Detail route citation rev2",
            "49 CFR detail rev2",
            "Detail route description rev2",
            createdCitation.CitationId));
        await _complianceCoreClient.SendAsync(createCitationRevisionRequest);

        var getCitationResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/citations/{createdCitation.CitationId}", adminToken));
        getCitationResponse.EnsureSuccessStatusCode();
        var fetchedCitation = (await getCitationResponse.Content.ReadFromJsonAsync<RegulatoryCitationResponse>())!;
        Assert.Equal("cfr_detail_routes", fetchedCitation.CitationKey);

        var updateCitationRequest = Authorized(HttpMethod.Patch, $"/api/v1/citations/{createdCitation.CitationId}", adminToken);
        updateCitationRequest.Content = JsonContent.Create(new UpdateRegulatoryCitationRequest(
            "Updated detail citation",
            "49 CFR detail updated",
            "Updated detail route description",
            true));
        var updateCitationResponse = await _complianceCoreClient.SendAsync(updateCitationRequest);
        updateCitationResponse.EnsureSuccessStatusCode();
        var updatedCitation = (await updateCitationResponse.Content.ReadFromJsonAsync<RegulatoryCitationResponse>())!;
        Assert.Equal("Updated detail citation", updatedCitation.Label);

        var citationHistoryResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/citations/{createdCitation.CitationId}/history", adminToken));
        citationHistoryResponse.EnsureSuccessStatusCode();
        var citationHistory = (await citationHistoryResponse.Content.ReadFromJsonAsync<IReadOnlyList<RegulatoryCitationResponse>>())!;
        Assert.True(citationHistory.Count >= 2);

        var citationRulesResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/citations/{createdCitation.CitationId}/rules", adminToken));
        citationRulesResponse.EnsureSuccessStatusCode();
        var citationRules = (await citationRulesResponse.Content.ReadFromJsonAsync<IReadOnlyList<CitationRuleLinkResponse>>())!;
        Assert.Contains(citationRules, x => x.RulePackId == rulePackId);

        var createFactRequest = Authorized(HttpMethod.Post, "/api/v1/facts", adminToken);
        createFactRequest.Content = JsonContent.Create(new CreateFactDefinitionRequest(
            "fact_detail_routes",
            "Fact detail routes",
            "Fact detail description",
            "string"));
        var createFactResponse = await _complianceCoreClient.SendAsync(createFactRequest);
        createFactResponse.EnsureSuccessStatusCode();

        var getFactResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/facts/fact_detail_routes", adminToken));
        getFactResponse.EnsureSuccessStatusCode();
        var fetchedFact = (await getFactResponse.Content.ReadFromJsonAsync<FactDefinitionResponse>())!;
        Assert.Equal("fact_detail_routes", fetchedFact.FactKey);

        var updateFactRequest = Authorized(HttpMethod.Patch, "/api/v1/facts/fact_detail_routes", adminToken);
        updateFactRequest.Content = JsonContent.Create(new UpdateFactDefinitionRequest(
            "Fact detail routes updated",
            "Fact detail description updated",
            "boolean",
            true));
        var updateFactResponse = await _complianceCoreClient.SendAsync(updateFactRequest);
        updateFactResponse.EnsureSuccessStatusCode();
        var updatedFact = (await updateFactResponse.Content.ReadFromJsonAsync<FactDefinitionResponse>())!;
        Assert.Equal("boolean", updatedFact.ValueType);

        var createRequirementRequest = Authorized(HttpMethod.Post, "/api/fact-requirements", adminToken);
        createRequirementRequest.Content = JsonContent.Create(new CreateFactRequirementRequest(
            updatedFact.FactDefinitionId,
            rulePackId,
            createdCitation.CitationId,
            "fact_detail_requirement",
            "Fact detail requirement",
            "Fact detail requirement description",
            true));
        var createRequirementResponse = await _complianceCoreClient.SendAsync(createRequirementRequest);
        createRequirementResponse.EnsureSuccessStatusCode();

        var usageResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/facts/fact_detail_routes/usage", adminToken));
        usageResponse.EnsureSuccessStatusCode();
        var usage = (await usageResponse.Content.ReadFromJsonAsync<FactDefinitionUsageResponse>())!;
        Assert.True(usage.FactRequirementCount >= 1);
        Assert.True(usage.RulePackCount >= 1);
        Assert.True(usage.CitationCount >= 1);

        var validateRequest = Authorized(HttpMethod.Post, "/api/v1/facts/validate-payload", adminToken);
        validateRequest.Content = JsonContent.Create(new ValidateFactPayloadRequest(
        [
            new ValidateFactPayloadItemRequest("fact_detail_routes", "true"),
            new ValidateFactPayloadItemRequest("unknown_fact_key", "anything")
        ]));
        var validateResponse = await _complianceCoreClient.SendAsync(validateRequest);
        validateResponse.EnsureSuccessStatusCode();
        var validation = (await validateResponse.Content.ReadFromJsonAsync<ValidateFactPayloadResponse>())!;
        Assert.Equal(2, validation.Results.Count);
        Assert.True(validation.Results.First(x => x.FactKey == "fact_detail_routes").IsValid);
        Assert.False(validation.Results.First(x => x.FactKey == "unknown_fact_key").IsValid);
    }

    private async Task<(Guid ProgramId, Guid RulePackId)> CreateSampleRulePackAsync(string adminToken)
    {
        var bodyRequest = Authorized(HttpMethod.Post, "/api/governing-bodies", adminToken);
        bodyRequest.Content = JsonContent.Create(new CreateGoverningBodyRequest(
            "dot",
            "U.S. Department of Transportation",
            "Federal transportation safety and compliance authority."));
        var body = (await (await _complianceCoreClient.SendAsync(bodyRequest)).Content.ReadFromJsonAsync<GoverningBodyResponse>())!;

        var jurisdictionRequest = Authorized(HttpMethod.Post, "/api/jurisdictions", adminToken);
        jurisdictionRequest.Content = JsonContent.Create(new CreateJurisdictionRequest(
            body.GoverningBodyId,
            "us_federal",
            "United States Federal",
            "Federal jurisdiction for interstate transportation rules."));
        var jurisdiction = (await (await _complianceCoreClient.SendAsync(jurisdictionRequest)).Content.ReadFromJsonAsync<JurisdictionResponse>())!;

        var programRequest = Authorized(HttpMethod.Post, "/api/regulatory-programs", adminToken);
        programRequest.Content = JsonContent.Create(new CreateRegulatoryProgramRequest(
            jurisdiction.JurisdictionId,
            "fmcsa_safety",
            "FMCSA Safety Compliance",
            "Federal motor carrier safety compliance program."));
        var program = (await (await _complianceCoreClient.SendAsync(programRequest)).Content.ReadFromJsonAsync<RegulatoryProgramResponse>())!;

        var packRequest = Authorized(HttpMethod.Post, "/api/rule-packs", adminToken);
        packRequest.Content = JsonContent.Create(new CreateRulePackRequest(
            program.RegulatoryProgramId,
            "driver_qualification",
            "Driver Qualification Rules",
            "Baseline driver qualification rule pack."));
        var pack = (await (await _complianceCoreClient.SendAsync(packRequest)).Content.ReadFromJsonAsync<RulePackResponse>())!;

        return (program.RegulatoryProgramId, pack.RulePackId);
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
