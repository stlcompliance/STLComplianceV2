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
using NexArr.Api.Services;

namespace STLCompliance.ComplianceCore.Auth.Tests;

public sealed class ComplianceCoreMissingEvidenceWarningTests : IAsyncLifetime
{
    private WebApplicationFactory<global::ComplianceCore.Api.Program> _complianceCoreFactory = null!;
    private HttpClient _complianceCoreClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"ComplianceCoreMissingEvidence-{Guid.NewGuid():N}";

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
    public async Task Missing_evidence_evaluate_list_and_summary_for_pack_without_mirror()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var packKey = await SeedPublishedPackWithUnresolvedFactAsync(adminToken);

        var evaluateRequest = Authorized(HttpMethod.Post, "/api/missing-evidence-warnings/evaluate", adminToken);
        evaluateRequest.Content = JsonContent.Create(new EvaluateMissingEvidenceWarningsRequest(
            "tenant",
            packKey,
            null));
        var evaluateResponse = await _complianceCoreClient.SendAsync(evaluateRequest);
        evaluateResponse.EnsureSuccessStatusCode();
        var evaluateResult = (await evaluateResponse.Content.ReadFromJsonAsync<EvaluateMissingEvidenceWarningsResponse>())!;
        Assert.Equal(1, evaluateResult.PacksAnalyzedCount);
        Assert.True(evaluateResult.WarningsEmittedCount >= 1);
        Assert.Contains(
            evaluateResult.Warnings,
            warning => warning.FactKey == "missing_evidence_license_valid");

        var listResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/missing-evidence-warnings?rulePackKey={packKey}", adminToken));
        listResponse.EnsureSuccessStatusCode();
        var listed = (await listResponse.Content.ReadFromJsonAsync<List<MissingEvidenceWarningResponse>>())!;
        Assert.NotEmpty(listed);

        var summaryResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/missing-evidence-warnings/summary", adminToken));
        summaryResponse.EnsureSuccessStatusCode();
        var summary = (await summaryResponse.Content.ReadFromJsonAsync<MissingEvidenceWarningSummaryResponse>())!;
        Assert.True(summary.TotalWarnings >= 1);
    }

    [Fact]
    public async Task Missing_evidence_catalog_requirement_without_mirror_emits_warning()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var packKey = await SeedPublishedPackWithCatalogRequirementAsync(adminToken);

        var evaluateRequest = Authorized(HttpMethod.Post, "/api/missing-evidence-warnings/evaluate", adminToken);
        evaluateRequest.Content = JsonContent.Create(new EvaluateMissingEvidenceWarningsRequest("tenant", packKey, null));
        var evaluateResponse = await _complianceCoreClient.SendAsync(evaluateRequest);
        evaluateResponse.EnsureSuccessStatusCode();
        var result = (await evaluateResponse.Content.ReadFromJsonAsync<EvaluateMissingEvidenceWarningsResponse>())!;
        Assert.Contains(
            result.Warnings,
            warning => warning.WarningType == MissingEvidenceWarningTypes.CatalogRequirement);
    }

    [Fact]
    public async Task Missing_evidence_evaluate_denies_tenant_member()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var memberToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "tenant_member");
        var packKey = await SeedPublishedPackWithUnresolvedFactAsync(adminToken);

        var request = Authorized(HttpMethod.Post, "/api/missing-evidence-warnings/evaluate", memberToken);
        request.Content = JsonContent.Create(new EvaluateMissingEvidenceWarningsRequest("tenant", packKey, null));
        var response = await _complianceCoreClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Missing_evidence_list_allowed_for_tenant_member()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var memberToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "tenant_member");
        var packKey = await SeedPublishedPackWithUnresolvedFactAsync(adminToken);

        var evaluateRequest = Authorized(HttpMethod.Post, "/api/missing-evidence-warnings/evaluate", adminToken);
        evaluateRequest.Content = JsonContent.Create(new EvaluateMissingEvidenceWarningsRequest("tenant", packKey, null));
        (await _complianceCoreClient.SendAsync(evaluateRequest)).EnsureSuccessStatusCode();

        var listResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/missing-evidence-warnings", memberToken));
        listResponse.EnsureSuccessStatusCode();
    }

    private async Task<string> SeedPublishedPackWithUnresolvedFactAsync(string adminToken)
    {
        await CreateBooleanFactAsync(adminToken, "missing_evidence_license_valid");
        var programId = await CreateProgramAsync(adminToken);
        var packKey = "missing_evidence_pack";
        var packId = await CreatePackAsync(adminToken, programId, packKey);
        await SetPackContentAsync(adminToken, packId, "missing_evidence_license_valid");
        await PublishPackAsync(adminToken, packId);
        return packKey;
    }

    private async Task<string> SeedPublishedPackWithCatalogRequirementAsync(string adminToken)
    {
        var factId = await CreateBooleanFactAsync(adminToken, "missing_evidence_catalog_fact");
        var programId = await CreateProgramAsync(adminToken);
        var packKey = "missing_evidence_catalog_pack";
        var packId = await CreatePackAsync(adminToken, programId, packKey);
        await SetPackContentAsync(adminToken, packId, "missing_evidence_catalog_fact");
        await CreateFactRequirementAsync(adminToken, factId, packId);
        await PublishPackAsync(adminToken, packId);
        return packKey;
    }

    private async Task CreateFactRequirementAsync(string adminToken, Guid factId, Guid packId)
    {
        var request = Authorized(HttpMethod.Post, "/api/fact-requirements", adminToken);
        request.Content = JsonContent.Create(new CreateFactRequirementRequest(
            factId,
            packId,
            null,
            "missing_evidence_req",
            "Missing evidence catalog requirement",
            "Required for predictive warning test.",
            true));
        (await _complianceCoreClient.SendAsync(request)).EnsureSuccessStatusCode();
    }

    private async Task<Guid> CreateBooleanFactAsync(string adminToken, string factKey)
    {
        var request = Authorized(HttpMethod.Post, "/api/fact-definitions", adminToken);
        request.Content = JsonContent.Create(new CreateFactDefinitionRequest(
            factKey,
            $"Label {factKey}",
            "Missing evidence test fact.",
            "boolean"));
        var response = await _complianceCoreClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<FactDefinitionResponse>())!.FactDefinitionId;
    }

    private async Task SetPackContentAsync(string adminToken, Guid packId, string factKey)
    {
        var content = new RulePackContentBody(
            1,
            "all",
            [new RuleDefinitionDto("license_rule", "License rule", "fact_boolean", factKey, true)]);

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
            "Missing evidence test pack."));
        var response = await _complianceCoreClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RulePackResponse>())!.RulePackId;
    }

    private async Task<Guid> CreateProgramAsync(string adminToken)
    {
        var bodyRequest = Authorized(HttpMethod.Post, "/api/governing-bodies", adminToken);
        bodyRequest.Content = JsonContent.Create(new CreateGoverningBodyRequest(
            "missing_dot",
            "Missing DOT",
            "Missing evidence governing body."));
        var body = (await (await _complianceCoreClient.SendAsync(bodyRequest)).Content.ReadFromJsonAsync<GoverningBodyResponse>())!;

        var jurisdictionRequest = Authorized(HttpMethod.Post, "/api/jurisdictions", adminToken);
        jurisdictionRequest.Content = JsonContent.Create(new CreateJurisdictionRequest(
            body.GoverningBodyId,
            "missing_us",
            "Missing US",
            "Missing jurisdiction."));
        var jurisdiction = (await (await _complianceCoreClient.SendAsync(jurisdictionRequest)).Content.ReadFromJsonAsync<JurisdictionResponse>())!;

        var programRequest = Authorized(HttpMethod.Post, "/api/regulatory-programs", adminToken);
        programRequest.Content = JsonContent.Create(new CreateRegulatoryProgramRequest(
            jurisdiction.JurisdictionId,
            "missing_fmcsa",
            "Missing FMCSA",
            "Missing program."));
        var program = (await (await _complianceCoreClient.SendAsync(programRequest)).Content.ReadFromJsonAsync<RegulatoryProgramResponse>())!;
        return program.RegulatoryProgramId;
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
