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

public sealed class ComplianceCoreRiskScoringTests : IAsyncLifetime
{
    private WebApplicationFactory<global::ComplianceCore.Api.Program> _complianceCoreFactory = null!;
    private HttpClient _complianceCoreClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"ComplianceCoreRiskScoring-{Guid.NewGuid():N}";

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
    public async Task Risk_score_evaluate_list_and_summary_for_published_pack()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var packKey = await SeedPublishedPackWithStaticFactAsync(adminToken);

        var evaluateRequest = Authorized(HttpMethod.Post, "/api/risk-scores/evaluate", adminToken);
        evaluateRequest.Content = JsonContent.Create(new EvaluateRiskScoresRequest(
            "tenant",
            packKey,
            null));
        var evaluateResponse = await _complianceCoreClient.SendAsync(evaluateRequest);
        evaluateResponse.EnsureSuccessStatusCode();
        var evaluateResult = (await evaluateResponse.Content.ReadFromJsonAsync<EvaluateRiskScoresResponse>())!;
        Assert.Equal(1, evaluateResult.PacksEvaluatedCount);
        Assert.Single(evaluateResult.Scores);
        Assert.True(evaluateResult.Scores[0].RiskScore >= 0);
        Assert.True(evaluateResult.Scores[0].RiskScore <= 100);
        Assert.False(string.IsNullOrWhiteSpace(evaluateResult.Scores[0].RiskLevel));

        var listResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/risk-scores?rulePackKey={packKey}", adminToken));
        listResponse.EnsureSuccessStatusCode();
        var listed = (await listResponse.Content.ReadFromJsonAsync<List<RiskScoreResponse>>())!;
        Assert.Single(listed);

        var summaryResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/risk-scores/summary", adminToken));
        summaryResponse.EnsureSuccessStatusCode();
        var summary = (await summaryResponse.Content.ReadFromJsonAsync<RiskScoreSummaryResponse>())!;
        Assert.Equal(1, summary.TotalScores);
        Assert.Equal(1, summary.ScopesTracked);
    }

    [Fact]
    public async Task Risk_score_evaluate_without_fact_source_yields_higher_risk()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var packKey = await SeedPublishedPackWithoutFactSourceAsync(adminToken);

        var evaluateRequest = Authorized(HttpMethod.Post, "/api/risk-scores/evaluate", adminToken);
        evaluateRequest.Content = JsonContent.Create(new EvaluateRiskScoresRequest("tenant", packKey, null));
        var evaluateResponse = await _complianceCoreClient.SendAsync(evaluateRequest);
        evaluateResponse.EnsureSuccessStatusCode();
        var result = (await evaluateResponse.Content.ReadFromJsonAsync<EvaluateRiskScoresResponse>())!;
        Assert.True(result.Scores[0].UnresolvedFactCount > 0);
        Assert.True(result.Scores[0].RiskScore >= 40);
    }

    [Fact]
    public async Task Risk_score_evaluate_denies_tenant_member()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var memberToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "tenant_member");
        var packKey = await SeedPublishedPackWithStaticFactAsync(adminToken);

        var request = Authorized(HttpMethod.Post, "/api/risk-scores/evaluate", memberToken);
        request.Content = JsonContent.Create(new EvaluateRiskScoresRequest("tenant", packKey, null));
        var response = await _complianceCoreClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Risk_score_list_allowed_for_tenant_member()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var memberToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "tenant_member");
        var packKey = await SeedPublishedPackWithStaticFactAsync(adminToken);

        var evaluateRequest = Authorized(HttpMethod.Post, "/api/risk-scores/evaluate", adminToken);
        evaluateRequest.Content = JsonContent.Create(new EvaluateRiskScoresRequest("tenant", packKey, null));
        (await _complianceCoreClient.SendAsync(evaluateRequest)).EnsureSuccessStatusCode();

        var listResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/risk-scores", memberToken));
        listResponse.EnsureSuccessStatusCode();
    }

    private async Task<string> SeedPublishedPackWithStaticFactAsync(string adminToken)
    {
        var factId = await CreateBooleanFactAsync(adminToken, "risk_score_license_valid");
        var packKey = "risk_score_pack";

        var programId = await CreateProgramAsync(adminToken);
        var packId = await CreatePackAsync(adminToken, programId, packKey);
        await SetPackContentAsync(adminToken, packId);
        await CreateStaticFactSourceAsync(adminToken, factId, "risk_score_license_source");
        await PublishPackAsync(adminToken, packId);
        return packKey;
    }

    private async Task<string> SeedPublishedPackWithoutFactSourceAsync(string adminToken)
    {
        var programId = await CreateProgramAsync(adminToken);
        var packKey = "risk_score_unresolved_pack";
        var packId = await CreatePackAsync(adminToken, programId, packKey);
        await SetPackContentAsync(adminToken, packId);
        await PublishPackAsync(adminToken, packId);
        return packKey;
    }

    private async Task<Guid> CreateBooleanFactAsync(string adminToken, string factKey)
    {
        var request = Authorized(HttpMethod.Post, "/api/fact-definitions", adminToken);
        request.Content = JsonContent.Create(new CreateFactDefinitionRequest(
            factKey,
            $"Label {factKey}",
            "Risk scoring test fact.",
            "boolean"));
        var response = await _complianceCoreClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<FactDefinitionResponse>())!.FactDefinitionId;
    }

    private async Task CreateStaticFactSourceAsync(string adminToken, Guid factId, string sourceKey)
    {
        var request = Authorized(HttpMethod.Post, "/api/fact-sources", adminToken);
        request.Content = JsonContent.Create(new CreateFactSourceRequest(
            factId,
            sourceKey,
            FactSourceTypes.StaticConfig,
            "Risk score license source",
            "Static true for risk scoring tests.",
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
            [new RuleDefinitionDto("license_valid", "Valid license", "fact_boolean", "risk_score_license_valid", true)]);

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
            "Risk scoring test pack."));
        var response = await _complianceCoreClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RulePackResponse>())!.RulePackId;
    }

    private async Task<Guid> CreateProgramAsync(string adminToken)
    {
        var bodyRequest = Authorized(HttpMethod.Post, "/api/governing-bodies", adminToken);
        bodyRequest.Content = JsonContent.Create(new CreateGoverningBodyRequest(
            "risk_dot",
            "Risk DOT",
            "Risk scoring governing body."));
        var body = (await (await _complianceCoreClient.SendAsync(bodyRequest)).Content.ReadFromJsonAsync<GoverningBodyResponse>())!;

        var jurisdictionRequest = Authorized(HttpMethod.Post, "/api/jurisdictions", adminToken);
        jurisdictionRequest.Content = JsonContent.Create(new CreateJurisdictionRequest(
            body.GoverningBodyId,
            "risk_us",
            "Risk US",
            "Risk jurisdiction."));
        var jurisdiction = (await (await _complianceCoreClient.SendAsync(jurisdictionRequest)).Content.ReadFromJsonAsync<JurisdictionResponse>())!;

        var programRequest = Authorized(HttpMethod.Post, "/api/regulatory-programs", adminToken);
        programRequest.Content = JsonContent.Create(new CreateRegulatoryProgramRequest(
            jurisdiction.JurisdictionId,
            "risk_fmcsa",
            "Risk FMCSA",
            "Risk program."));
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
