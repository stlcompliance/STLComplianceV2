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

public class ComplianceCoreRuleEvaluationTests : IAsyncLifetime
{
    private WebApplicationFactory<global::ComplianceCore.Api.Program> _complianceCoreFactory = null!;
    private HttpClient _complianceCoreClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"ComplianceCoreRuleEval-{Guid.NewGuid():N}";

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
    public async Task Rule_content_update_get_and_evaluate_pass_and_fail()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var rulePackId = await CreateSampleRulePackAsync(adminToken);

        var content = new RulePackContentBody(
            1,
            "all",
            [
                new RuleDefinitionDto(
                    "license_valid",
                    "Valid driver license",
                    "fact_boolean",
                    "driver_license_valid",
                    true),
                new RuleDefinitionDto(
                    "med_cert",
                    "Medical certificate on file",
                    "fact_boolean",
                    "medical_cert_on_file",
                    true),
            ]);

        var updateRequest = Authorized(HttpMethod.Put, $"/api/rule-packs/{rulePackId}/content", adminToken);
        updateRequest.Content = JsonContent.Create(new UpdateRulePackContentRequest(content));
        var updateResponse = await _complianceCoreClient.SendAsync(updateRequest);
        updateResponse.EnsureSuccessStatusCode();
        var updatedContent = (await updateResponse.Content.ReadFromJsonAsync<RulePackContentResponse>())!;
        Assert.True(updatedContent.HasContent);
        Assert.Equal("all", updatedContent.Content!.Logic);
        Assert.Equal(2, updatedContent.Content.Rules.Count);

        var getResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/rule-packs/{rulePackId}/content", adminToken));
        getResponse.EnsureSuccessStatusCode();
        var loadedContent = (await getResponse.Content.ReadFromJsonAsync<RulePackContentResponse>())!;
        Assert.True(loadedContent.HasContent);

        var passEvaluateRequest = Authorized(HttpMethod.Post, $"/api/rule-packs/{rulePackId}/evaluate", adminToken);
        passEvaluateRequest.Content = JsonContent.Create(new EvaluateRulePackRequest(new Dictionary<string, bool>
        {
            ["driver_license_valid"] = true,
            ["medical_cert_on_file"] = true,
        }));
        var passEvaluateResponse = await _complianceCoreClient.SendAsync(passEvaluateRequest);
        passEvaluateResponse.EnsureSuccessStatusCode();
        var passResult = (await passEvaluateResponse.Content.ReadFromJsonAsync<RuleEvaluationRunResponse>())!;
        Assert.Equal("pass", passResult.OverallResult);
        Assert.Equal(2, passResult.RuleResults.Count);
        Assert.All(passResult.RuleResults, item => Assert.Equal("pass", item.Result));

        var failEvaluateRequest = Authorized(HttpMethod.Post, $"/api/rule-packs/{rulePackId}/evaluate", adminToken);
        failEvaluateRequest.Content = JsonContent.Create(new EvaluateRulePackRequest(new Dictionary<string, bool>
        {
            ["driver_license_valid"] = true,
            ["medical_cert_on_file"] = false,
        }));
        var failEvaluateResponse = await _complianceCoreClient.SendAsync(failEvaluateRequest);
        failEvaluateResponse.EnsureSuccessStatusCode();
        var failResult = (await failEvaluateResponse.Content.ReadFromJsonAsync<RuleEvaluationRunResponse>())!;
        Assert.Equal("fail", failResult.OverallResult);
        Assert.Contains(failResult.RuleResults, item => item.RuleKey == "med_cert" && item.Result == "fail");

        var listResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/rule-evaluations?rulePackId={rulePackId}", adminToken));
        listResponse.EnsureSuccessStatusCode();
        var runs = (await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<RuleEvaluationRunResponse>>())!;
        Assert.Equal(2, runs.Count);

        var getRunResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/rule-evaluations/{passResult.EvaluationRunId}", adminToken));
        getRunResponse.EnsureSuccessStatusCode();
        var runDetail = (await getRunResponse.Content.ReadFromJsonAsync<RuleEvaluationRunResponse>())!;
        Assert.Equal("pass", runDetail.OverallResult);
        Assert.True(runDetail.FactInputs["driver_license_valid"]);
    }

    [Fact]
    public async Task Rule_content_update_denies_member_role()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var memberToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "tenant_member");
        var rulePackId = await CreateSampleRulePackAsync(adminToken);

        var content = new RulePackContentBody(
            1,
            "all",
            [new RuleDefinitionDto("license_valid", "Valid license", "fact_boolean", "driver_license_valid", true)]);

        var request = Authorized(HttpMethod.Put, $"/api/rule-packs/{rulePackId}/content", memberToken);
        request.Content = JsonContent.Create(new UpdateRulePackContentRequest(content));
        var response = await _complianceCoreClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Rule_evaluation_member_can_run_and_read()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var memberToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "tenant_member");
        var rulePackId = await CreateSampleRulePackWithContentAsync(adminToken);

        var evaluateRequest = Authorized(HttpMethod.Post, $"/api/rule-packs/{rulePackId}/evaluate", memberToken);
        evaluateRequest.Content = JsonContent.Create(new EvaluateRulePackRequest(new Dictionary<string, bool>
        {
            ["driver_license_valid"] = false,
        }));
        var evaluateResponse = await _complianceCoreClient.SendAsync(evaluateRequest);
        evaluateResponse.EnsureSuccessStatusCode();
        var result = (await evaluateResponse.Content.ReadFromJsonAsync<RuleEvaluationRunResponse>())!;
        Assert.Equal("fail", result.OverallResult);

        var listResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/rule-evaluations?rulePackId={rulePackId}", memberToken));
        listResponse.EnsureSuccessStatusCode();
        var runs = (await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<RuleEvaluationRunResponse>>())!;
        Assert.Single(runs);
    }

    [Fact]
    public async Task Rule_evaluation_requires_compliancecore_entitlement()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var staffArrToken = CreateComplianceCoreAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var rulePackId = await CreateSampleRulePackWithContentAsync(adminToken);

        var request = Authorized(HttpMethod.Post, $"/api/rule-packs/{rulePackId}/evaluate", staffArrToken);
        request.Content = JsonContent.Create(new EvaluateRulePackRequest(new Dictionary<string, bool>
        {
            ["driver_license_valid"] = true,
        }));
        var response = await _complianceCoreClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Rule_evaluation_denies_missing_fact()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var rulePackId = await CreateSampleRulePackWithContentAsync(adminToken);

        var request = Authorized(HttpMethod.Post, $"/api/rule-packs/{rulePackId}/evaluate", adminToken);
        request.Content = JsonContent.Create(new EvaluateRulePackRequest(new Dictionary<string, bool>()));
        var response = await _complianceCoreClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var result = (await response.Content.ReadFromJsonAsync<RuleEvaluationRunResponse>())!;
        Assert.Equal("fail", result.OverallResult);
        Assert.Contains(result.RuleResults, item => item.Message.Contains("was not provided", StringComparison.OrdinalIgnoreCase));

        using var scope = _complianceCoreFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        var events = await db.AuditEvents
            .AsNoTracking()
            .Where(x => x.TenantId == PlatformSeeder.DemoTenantId
                && x.TargetId == result.EvaluationRunId.ToString())
            .ToListAsync();

        Assert.Single(events, x => x.Action == RuleEvaluationService.EvaluationCompletedEventAction);
        Assert.Single(events, x => x.Action == RuleEvaluationService.EvaluationBlockedEventAction);
        var missingEvent = Assert.Single(events, x => x.Action == RuleEvaluationService.EvidenceMissingEventAction);
        Assert.Equal("license_valid", missingEvent.ReasonCode);
    }

    [Fact]
    public async Task Rule_evaluation_failed_remediation_rule_emits_canonical_event()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var rulePackId = await CreateSampleRulePackAsync(adminToken);

        var content = new RulePackContentBody(
            1,
            "all",
            [
                new RuleDefinitionDto(
                    "license_valid",
                    "Valid license",
                    "fact_boolean",
                    "driver_license_valid",
                    true,
                    RemediationRequired: true),
            ]);

        var updateRequest = Authorized(HttpMethod.Put, $"/api/rule-packs/{rulePackId}/content", adminToken);
        updateRequest.Content = JsonContent.Create(new UpdateRulePackContentRequest(content));
        (await _complianceCoreClient.SendAsync(updateRequest)).EnsureSuccessStatusCode();

        var request = Authorized(HttpMethod.Post, $"/api/rule-packs/{rulePackId}/evaluate", adminToken);
        request.Content = JsonContent.Create(new EvaluateRulePackRequest(new Dictionary<string, bool>
        {
            ["driver_license_valid"] = false,
        }));
        var response = await _complianceCoreClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var result = (await response.Content.ReadFromJsonAsync<RuleEvaluationRunResponse>())!;
        Assert.Equal("fail", result.OverallResult);
        var failedRule = Assert.Single(result.RuleResults);
        Assert.True(failedRule.RemediationRequired);

        using var scope = _complianceCoreFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        var remediationEvent = await db.AuditEvents.SingleAsync(
            x => x.TenantId == PlatformSeeder.DemoTenantId
                && x.Action == RuleEvaluationService.RemediationRequiredEventAction
                && x.TargetId == result.EvaluationRunId.ToString());
        Assert.Equal("rule_evaluation_run", remediationEvent.TargetType);
        Assert.Equal(ComplianceEvaluationOutcomes.NeedsRemediation, remediationEvent.Result);
        Assert.Equal("license_valid", remediationEvent.ReasonCode);
    }

    [Fact]
    public async Task Rule_pack_batch_evaluate_review_required_rule_returns_review_and_emits_event()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var rulePackId = await CreateSampleRulePackAsync(adminToken);

        var content = new RulePackContentBody(
            1,
            "all",
            [
                new RuleDefinitionDto(
                    "license_review",
                    "License requires review",
                    "fact_boolean",
                    "driver_license_valid",
                    true,
                    ReviewRequired: true),
            ]);

        var updateRequest = Authorized(HttpMethod.Put, $"/api/rule-packs/{rulePackId}/content", adminToken);
        updateRequest.Content = JsonContent.Create(new UpdateRulePackContentRequest(content));
        (await _complianceCoreClient.SendAsync(updateRequest)).EnsureSuccessStatusCode();

        var request = Authorized(HttpMethod.Post, "/api/rule-packs/evaluate/batch", adminToken);
        request.Content = JsonContent.Create(new EvaluateRulePackBatchRequest(
            [new EvaluateRulePackBatchItem("driver_qualification")],
            new Dictionary<string, bool>
            {
                ["driver_license_valid"] = false,
            }));
        var response = await _complianceCoreClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var batch = (await response.Content.ReadFromJsonAsync<EvaluateRulePackBatchResponse>())!;
        var item = Assert.Single(batch.Results);
        Assert.Equal(ComplianceEvaluationOutcomes.Review, item.Outcome);
        Assert.Equal("review_required", item.ReasonCode);
        Assert.Equal(1, batch.Summary.BlockCount);
        var rule = Assert.Single(item.RuleResults);
        Assert.True(rule.ReviewRequired);

        using var scope = _complianceCoreFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        var reviewEvent = await db.AuditEvents.SingleAsync(
            x => x.TenantId == PlatformSeeder.DemoTenantId
                && x.Action == RuleEvaluationService.ReviewRequiredEventAction
                && x.TargetId == item.EvaluationRunId!.Value.ToString());
        Assert.Equal("rule_evaluation_run", reviewEvent.TargetType);
        Assert.Equal(ComplianceEvaluationOutcomes.Review, reviewEvent.Result);
        Assert.Equal("license_review", reviewEvent.ReasonCode);
    }

    [Fact]
    public async Task Rule_pack_batch_evaluate_returns_per_item_results_and_summary()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        await CreateSampleRulePackWithContentAsync(adminToken);

        var request = Authorized(HttpMethod.Post, "/api/rule-packs/evaluate/batch", adminToken);
        request.Content = JsonContent.Create(new EvaluateRulePackBatchRequest(
            [
                new EvaluateRulePackBatchItem("driver_qualification"),
                new EvaluateRulePackBatchItem("driver_qualification"),
            ],
            new Dictionary<string, bool>
            {
                ["driver_license_valid"] = true,
            }));
        var response = await _complianceCoreClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var batch = (await response.Content.ReadFromJsonAsync<EvaluateRulePackBatchResponse>())!;
        Assert.NotEqual(Guid.Empty, batch.BatchId);
        Assert.Single(batch.Results);
        Assert.Equal(1, batch.Summary.Total);
        Assert.Equal(1, batch.Summary.AllowCount);
        Assert.Equal(0, batch.Summary.WarnCount);
        Assert.Equal(0, batch.Summary.BlockCount);
        Assert.All(batch.Results, result => Assert.Equal(ComplianceEvaluationOutcomes.Allow, result.Outcome));
    }

    [Fact]
    public async Task Rule_pack_batch_evaluate_blocks_when_shared_facts_fail()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        await CreateSampleRulePackWithContentAsync(adminToken);

        var request = Authorized(HttpMethod.Post, "/api/rule-packs/evaluate/batch", adminToken);
        request.Content = JsonContent.Create(new EvaluateRulePackBatchRequest(
            [new EvaluateRulePackBatchItem("driver_qualification")],
            new Dictionary<string, bool>
            {
                ["driver_license_valid"] = false,
            }));
        var response = await _complianceCoreClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var batch = (await response.Content.ReadFromJsonAsync<EvaluateRulePackBatchResponse>())!;
        Assert.Equal(1, batch.Summary.BlockCount);
        Assert.Equal(ComplianceEvaluationOutcomes.Block, batch.Results[0].Outcome);
    }

    [Fact]
    public async Task Rule_pack_batch_evaluate_rejects_empty_items()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var request = Authorized(HttpMethod.Post, "/api/rule-packs/evaluate/batch", adminToken);
        request.Content = JsonContent.Create(new EvaluateRulePackBatchRequest([]));
        var response = await _complianceCoreClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Rule_pack_batch_evaluate_requires_compliancecore_entitlement()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var staffArrToken = CreateComplianceCoreAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        await CreateSampleRulePackWithContentAsync(adminToken);

        var request = Authorized(HttpMethod.Post, "/api/rule-packs/evaluate/batch", staffArrToken);
        request.Content = JsonContent.Create(new EvaluateRulePackBatchRequest(
            [new EvaluateRulePackBatchItem("driver_qualification")],
            new Dictionary<string, bool> { ["driver_license_valid"] = true }));
        var response = await _complianceCoreClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Rule_pack_batch_evaluate_member_can_run()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var memberToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "tenant_member");
        await CreateSampleRulePackWithContentAsync(adminToken);

        var request = Authorized(HttpMethod.Post, "/api/rule-packs/evaluate/batch", memberToken);
        request.Content = JsonContent.Create(new EvaluateRulePackBatchRequest(
            [new EvaluateRulePackBatchItem("driver_qualification")],
            new Dictionary<string, bool> { ["driver_license_valid"] = true }));
        var response = await _complianceCoreClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var batch = (await response.Content.ReadFromJsonAsync<EvaluateRulePackBatchResponse>())!;
        Assert.Equal(1, batch.Summary.AllowCount);
    }

    [Fact]
    public async Task V1_evaluations_alias_routes_run_list_get_and_export()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var rulePackId = await CreateSampleRulePackWithContentAsync(adminToken);

        var runRequest = Authorized(HttpMethod.Post, "/api/v1/evaluations/run", adminToken);
        runRequest.Content = JsonContent.Create(new EvaluateRulePackRunRequest(
            rulePackId,
            new Dictionary<string, bool> { ["driver_license_valid"] = true }));
        var runResponse = await _complianceCoreClient.SendAsync(runRequest);
        runResponse.EnsureSuccessStatusCode();
        var run = (await runResponse.Content.ReadFromJsonAsync<RuleEvaluationRunResponse>())!;
        Assert.Equal("pass", run.OverallResult);

        var listResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/evaluations?rulePackId={rulePackId}", adminToken));
        listResponse.EnsureSuccessStatusCode();
        var runs = (await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<RuleEvaluationRunResponse>>())!;
        var listed = Assert.Single(runs);
        Assert.Equal(run.EvaluationRunId, listed.EvaluationRunId);

        var getResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/evaluations/{run.EvaluationRunId}", adminToken));
        getResponse.EnsureSuccessStatusCode();
        var loaded = (await getResponse.Content.ReadFromJsonAsync<RuleEvaluationRunResponse>())!;
        Assert.Equal(run.EvaluationRunId, loaded.EvaluationRunId);

        var exportResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/evaluations/{run.EvaluationRunId}/audit-export", adminToken));
        exportResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task V1_evaluations_support_simulate_re_evaluate_and_explanation()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var rulePackId = await CreateSampleRulePackWithContentAsync(adminToken);

        var runRequest = Authorized(HttpMethod.Post, "/api/v1/evaluations/run", adminToken);
        runRequest.Content = JsonContent.Create(new EvaluateRulePackRunRequest(
            rulePackId,
            new Dictionary<string, bool> { ["driver_license_valid"] = false }));
        var runResponse = await _complianceCoreClient.SendAsync(runRequest);
        runResponse.EnsureSuccessStatusCode();
        var run = (await runResponse.Content.ReadFromJsonAsync<RuleEvaluationRunResponse>())!;
        Assert.Equal("fail", run.OverallResult);

        var explainResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/evaluations/{run.EvaluationRunId}/explanation", adminToken));
        explainResponse.EnsureSuccessStatusCode();
        var explanation = (await explainResponse.Content.ReadFromJsonAsync<RuleEvaluationExplanationResponse>())!;
        Assert.Equal(run.EvaluationRunId, explanation.EvaluationRunId);
        Assert.Equal("fail", explanation.OverallResult);
        Assert.NotEmpty(explanation.FailedRuleKeys);

        var reEvaluateRequest = Authorized(
            HttpMethod.Post,
            $"/api/v1/evaluations/{run.EvaluationRunId}/re-evaluate",
            adminToken);
        reEvaluateRequest.Content = JsonContent.Create(new ReEvaluateRuleEvaluationRequest());
        var reEvaluateResponse = await _complianceCoreClient.SendAsync(reEvaluateRequest);
        reEvaluateResponse.EnsureSuccessStatusCode();
        var rerun = (await reEvaluateResponse.Content.ReadFromJsonAsync<RuleEvaluationRunResponse>())!;
        Assert.NotEqual(run.EvaluationRunId, rerun.EvaluationRunId);
        Assert.Equal(run.OverallResult, rerun.OverallResult);
        Assert.Equal(run.FactInputs["driver_license_valid"], rerun.FactInputs["driver_license_valid"]);

        var listBeforeSimResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/evaluations?rulePackId={rulePackId}", adminToken));
        listBeforeSimResponse.EnsureSuccessStatusCode();
        var runsBeforeSim = (await listBeforeSimResponse.Content.ReadFromJsonAsync<IReadOnlyList<RuleEvaluationRunResponse>>())!;

        var simulateRequest = Authorized(HttpMethod.Post, "/api/v1/evaluations/simulate", adminToken);
        simulateRequest.Content = JsonContent.Create(new EvaluateRulePackSimulationRequest(
            rulePackId,
            new Dictionary<string, bool> { ["driver_license_valid"] = true }));
        var simulateResponse = await _complianceCoreClient.SendAsync(simulateRequest);
        simulateResponse.EnsureSuccessStatusCode();
        var simulation = (await simulateResponse.Content.ReadFromJsonAsync<RuleEvaluationSimulationResponse>())!;
        Assert.Equal("pass", simulation.OverallResult);

        var listAfterSimResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/evaluations?rulePackId={rulePackId}", adminToken));
        listAfterSimResponse.EnsureSuccessStatusCode();
        var runsAfterSim = (await listAfterSimResponse.Content.ReadFromJsonAsync<IReadOnlyList<RuleEvaluationRunResponse>>())!;
        Assert.Equal(runsBeforeSim.Count, runsAfterSim.Count);
    }

    [Fact]
    public async Task V1_rule_pack_evaluation_aliases_support_content_and_evaluate_routes()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var rulePackId = await CreateSampleRulePackAsync(adminToken);

        var content = new RulePackContentBody(
            1,
            "all",
            [new RuleDefinitionDto("license_valid", "Valid license", "fact_boolean", "driver_license_valid", true)]);

        var updateRequest = Authorized(HttpMethod.Put, $"/api/v1/rule-packs/{rulePackId}/content", adminToken);
        updateRequest.Content = JsonContent.Create(new UpdateRulePackContentRequest(content));
        var updateResponse = await _complianceCoreClient.SendAsync(updateRequest);
        updateResponse.EnsureSuccessStatusCode();

        var getResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/rule-packs/{rulePackId}/content", adminToken));
        getResponse.EnsureSuccessStatusCode();
        var loadedContent = (await getResponse.Content.ReadFromJsonAsync<RulePackContentResponse>())!;
        Assert.True(loadedContent.HasContent);

        var evaluateRequest = Authorized(HttpMethod.Post, $"/api/v1/rule-packs/{rulePackId}/evaluate", adminToken);
        evaluateRequest.Content = JsonContent.Create(new EvaluateRulePackRequest(new Dictionary<string, bool>
        {
            ["driver_license_valid"] = true,
        }));
        var evaluateResponse = await _complianceCoreClient.SendAsync(evaluateRequest);
        evaluateResponse.EnsureSuccessStatusCode();
        var run = (await evaluateResponse.Content.ReadFromJsonAsync<RuleEvaluationRunResponse>())!;
        Assert.Equal("pass", run.OverallResult);

        var batchRequest = Authorized(HttpMethod.Post, "/api/v1/rule-packs/evaluate/batch", adminToken);
        batchRequest.Content = JsonContent.Create(new EvaluateRulePackBatchRequest(
            [new EvaluateRulePackBatchItem("driver_qualification")],
            new Dictionary<string, bool> { ["driver_license_valid"] = true }));
        var batchResponse = await _complianceCoreClient.SendAsync(batchRequest);
        batchResponse.EnsureSuccessStatusCode();
        var batch = (await batchResponse.Content.ReadFromJsonAsync<EvaluateRulePackBatchResponse>())!;
        Assert.Equal(1, batch.Summary.AllowCount);
    }

    [Fact]
    public async Task V1_rules_catalog_routes_list_get_create_patch_validate_test_usage_and_history_work()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var rulePackId = await CreateSampleRulePackWithContentAsync(adminToken);

        var listResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/rules", adminToken));
        listResponse.EnsureSuccessStatusCode();
        var listedRules = (await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<RuleCatalogItemResponse>>())!;
        var existingRule = Assert.Single(listedRules.Where(x => x.RulePackId == rulePackId));

        var getResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/rules/{Uri.EscapeDataString(existingRule.RuleId)}", adminToken));
        getResponse.EnsureSuccessStatusCode();
        var loadedRule = (await getResponse.Content.ReadFromJsonAsync<RuleCatalogItemResponse>())!;
        Assert.Equal(existingRule.RuleId, loadedRule.RuleId);

        var createRequest = Authorized(HttpMethod.Post, "/api/v1/rules", adminToken);
        createRequest.Content = JsonContent.Create(new CreateRuleCatalogRequest(
            rulePackId,
            "medical_cert_rule",
            "Medical certificate required",
            "fact_boolean",
            "medical_cert_on_file",
            true));
        var createResponse = await _complianceCoreClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<RuleCatalogItemResponse>())!;
        Assert.Equal("medical_cert_rule", created.RuleKey);

        var patchRequest = Authorized(HttpMethod.Patch, $"/api/v1/rules/{Uri.EscapeDataString(created.RuleId)}", adminToken);
        patchRequest.Content = JsonContent.Create(new PatchRuleCatalogRequest(
            "Medical certificate still required",
            null,
            "medical_cert_on_file",
            false,
            true));
        var patchResponse = await _complianceCoreClient.SendAsync(patchRequest);
        patchResponse.EnsureSuccessStatusCode();
        var patched = (await patchResponse.Content.ReadFromJsonAsync<RuleCatalogItemResponse>())!;
        Assert.Equal("Medical certificate still required", patched.Label);
        Assert.False(patched.ExpectedValue);
        Assert.True(patched.NonWaivable);

        var validateResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Post, $"/api/v1/rules/{Uri.EscapeDataString(patched.RuleId)}/validate", adminToken));
        validateResponse.EnsureSuccessStatusCode();
        var validation = (await validateResponse.Content.ReadFromJsonAsync<RuleCatalogValidateResponse>())!;
        Assert.True(validation.IsValid);

        var testRequest = Authorized(HttpMethod.Post, $"/api/v1/rules/{Uri.EscapeDataString(patched.RuleId)}/test", adminToken);
        testRequest.Content = JsonContent.Create(new RuleCatalogTestRequest(new Dictionary<string, bool>
        {
            ["medical_cert_on_file"] = false
        }));
        var testResponse = await _complianceCoreClient.SendAsync(testRequest);
        testResponse.EnsureSuccessStatusCode();
        var testResult = (await testResponse.Content.ReadFromJsonAsync<RuleCatalogTestResponse>())!;
        Assert.Equal("pass", testResult.Result);

        var evaluateRequest = Authorized(HttpMethod.Post, $"/api/rule-packs/{rulePackId}/evaluate", adminToken);
        evaluateRequest.Content = JsonContent.Create(new EvaluateRulePackRequest(new Dictionary<string, bool>
        {
            ["driver_license_valid"] = true,
            ["medical_cert_on_file"] = false,
        }));
        var evaluateResponse = await _complianceCoreClient.SendAsync(evaluateRequest);
        evaluateResponse.EnsureSuccessStatusCode();

        var usageResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/rules/{Uri.EscapeDataString(patched.RuleId)}/usage", adminToken));
        usageResponse.EnsureSuccessStatusCode();
        var usage = (await usageResponse.Content.ReadFromJsonAsync<RuleCatalogUsageResponse>())!;
        Assert.True(usage.EvaluationRunCount >= 1);

        var historyResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/rules/{Uri.EscapeDataString(patched.RuleId)}/history", adminToken));
        historyResponse.EnsureSuccessStatusCode();
        var history = (await historyResponse.Content.ReadFromJsonAsync<RuleCatalogHistoryResponse>())!;
        Assert.Equal("medical_cert_rule", history.RuleKey);
        Assert.True(history.History.Count >= 1);
        Assert.Contains(history.History, item => item.ExistsInVersion);

        using var scope = _complianceCoreFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        var changedEvents = await db.AuditEvents
            .AsNoTracking()
            .Where(x => x.TenantId == PlatformSeeder.DemoTenantId
                && x.Action == RuleCatalogService.RuleChangedEventAction
                && x.TargetId == patched.RuleId)
            .ToListAsync();

        Assert.Single(changedEvents, x => x.Result == "created");
        Assert.Single(changedEvents, x => x.Result == "updated");
        Assert.All(changedEvents, changedEvent => Assert.Equal("medical_cert_rule", changedEvent.ReasonCode));
    }

    [Fact]
    public async Task V1_rules_catalog_create_denies_member_role()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var memberToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "tenant_member");
        var rulePackId = await CreateSampleRulePackWithContentAsync(adminToken);

        var createRequest = Authorized(HttpMethod.Post, "/api/v1/rules", memberToken);
        createRequest.Content = JsonContent.Create(new CreateRuleCatalogRequest(
            rulePackId,
            "member_denied_rule",
            "Member denied rule",
            "fact_boolean",
            "driver_license_valid",
            true));
        var createResponse = await _complianceCoreClient.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Forbidden, createResponse.StatusCode);
    }

    [Fact]
    public async Task V1_rules_catalog_requires_compliancecore_entitlement()
    {
        var adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");
        var staffArrToken = CreateComplianceCoreAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        await CreateSampleRulePackWithContentAsync(adminToken);

        var listResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/rules", staffArrToken));
        Assert.Equal(HttpStatusCode.Forbidden, listResponse.StatusCode);
    }

    private async Task<Guid> CreateSampleRulePackAsync(string adminToken)
    {
        var programId = await CreateSampleProgramAsync(adminToken);

        var createRequest = Authorized(HttpMethod.Post, "/api/rule-packs", adminToken);
        createRequest.Content = JsonContent.Create(new CreateRulePackRequest(
            programId,
            "driver_qualification",
            "Driver Qualification Rules",
            "Baseline driver qualification rule pack."));
        var createResponse = await _complianceCoreClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<RulePackResponse>())!;
        return created.RulePackId;
    }

    private async Task<Guid> CreateSampleRulePackWithContentAsync(string adminToken)
    {
        var rulePackId = await CreateSampleRulePackAsync(adminToken);
        var content = new RulePackContentBody(
            1,
            "all",
            [new RuleDefinitionDto("license_valid", "Valid license", "fact_boolean", "driver_license_valid", true)]);

        var updateRequest = Authorized(HttpMethod.Put, $"/api/rule-packs/{rulePackId}/content", adminToken);
        updateRequest.Content = JsonContent.Create(new UpdateRulePackContentRequest(content));
        (await _complianceCoreClient.SendAsync(updateRequest)).EnsureSuccessStatusCode();
        return rulePackId;
    }

    private async Task<Guid> CreateSampleProgramAsync(string adminToken)
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
