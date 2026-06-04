using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
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

public sealed class ComplianceCoreReportTests : IAsyncLifetime
{
    private WebApplicationFactory<global::ComplianceCore.Api.Program> _complianceCoreFactory = null!;
    private HttpClient _complianceCoreClient = null!;
    private string _adminToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"ComplianceCoreReports-{Guid.NewGuid():N}";

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
        _adminToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "compliance_admin");

        using var scope = _complianceCoreFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        await db.Database.EnsureCreatedAsync();
        var vocabularyService = scope.ServiceProvider.GetRequiredService<VocabularyService>();
        await vocabularyService.EnsureVocabularyTypesSeededAsync();

        await SeedFindingViaEvaluationAsync();
        await SeedExceptionExemptionsAsync();
        await SeedProductIntegrationHealthAsync();
        await SeedAuditReadinessForecastAsync();
        await SeedRegulatoryDomainCoverageAsync();
        await SeedTitle49CoverageExplorerAsync();
        await SeedCitationReviewAsync();
    }

    public async Task DisposeAsync()
    {
        _complianceCoreClient.Dispose();
        await _complianceCoreFactory.DisposeAsync();
    }

    [Fact]
    public async Task Findings_report_summary_returns_aggregates()
    {
        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/findings/summary", _adminToken));
        response.EnsureSuccessStatusCode();

        var summary = (await response.Content.ReadFromJsonAsync<FindingsReportSummaryResponse>())!;
        Assert.True(summary.TotalFindings >= 1);
        Assert.True(summary.OpenCount >= 1);
    }

    [Fact]
    public async Task Operator_report_summary_returns_aggregates()
    {
        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/operator/summary", _adminToken));
        response.EnsureSuccessStatusCode();

        var summary = (await response.Content.ReadFromJsonAsync<OperatorReportSummaryResponse>())!;
        Assert.True(summary.EvaluationTotalCount >= 1);
        Assert.True(summary.EvaluationFailCount >= 1);
    }

    [Fact]
    public async Task Operator_report_alerts_include_missing_evidence_and_expiring_waivers()
    {
        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/operator/alerts", _adminToken));
        response.EnsureSuccessStatusCode();

        var alerts = await response.Content.ReadFromJsonAsync<IReadOnlyList<OperatorReportAlertResponse>>();
        Assert.NotNull(alerts);
        Assert.Contains(alerts!, x => x.AlertType == "evidence_missing");
        Assert.Contains(alerts, x => x.AlertType == "waiver_expiring");
    }

    [Fact]
    public async Task Missing_evidence_report_summary_returns_aggregates()
    {
        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/evidence/missing/summary", _adminToken));
        response.EnsureSuccessStatusCode();

        var summary = (await response.Content.ReadFromJsonAsync<MissingEvidenceReportSummaryResponse>())!;
        Assert.True(summary.TotalWarnings >= 1);
        Assert.True(summary.HighCount >= 1);
        Assert.True(summary.MissingMirrorCount >= 1);
    }

    [Fact]
    public async Task Evidence_completeness_report_summary_returns_pack_rollups()
    {
        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/evidence/completeness/summary?scopeKey=tenant&severity=high", _adminToken));
        response.EnsureSuccessStatusCode();

        var summary = (await response.Content.ReadFromJsonAsync<EvidenceCompletenessReportSummaryResponse>())!;
        Assert.True(summary.TotalRulePacks >= 1);
        Assert.True(summary.TotalWarnings >= 1);
        Assert.True(summary.CompletenessScore < 100);
        Assert.Equal(1, summary.HighWarningCount);
        Assert.Single(summary.RulePacks);
        Assert.Equal("driver_qualification", summary.RulePacks[0].PackKey);
        Assert.Equal("partial", summary.RulePacks[0].CompletenessLevel);

        var exportResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/evidence/completeness/summary/export?scopeKey=tenant&severity=high", _adminToken));
        exportResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", exportResponse.Content.Headers.ContentType?.MediaType);
        var csv = await exportResponse.Content.ReadAsStringAsync();
        Assert.Contains("driver_qualification", csv, StringComparison.Ordinal);
        Assert.Contains("partial", csv, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Remediation_queue_report_summary_returns_queue_items()
    {
        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/remediation-queue/summary", _adminToken));
        response.EnsureSuccessStatusCode();

        var summary = (await response.Content.ReadFromJsonAsync<RemediationQueueReportSummaryResponse>())!;
        Assert.True(summary.TotalWarnings >= 1);
        Assert.True(summary.QueuedCount >= 1);
        Assert.NotEmpty(summary.QueueItems);
    }

    [Fact]
    public async Task Waiver_report_summary_returns_aggregates()
    {
        var createRequest = Authorized(HttpMethod.Post, "/api/waivers", _adminToken);
        createRequest.Content = JsonContent.Create(new CreateComplianceWaiverRequest(
            "pending-waiver-report-seed",
            await GetSeedRulePackIdAsync(),
            "tenant",
            "operations_override",
            "Pending waiver included in waiver report coverage.",
            DateTimeOffset.UtcNow.AddHours(-1),
            DateTimeOffset.UtcNow.AddDays(5),
            RuleKey: "license_valid"));
        (await _complianceCoreClient.SendAsync(createRequest)).EnsureSuccessStatusCode();

        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/waivers/summary", _adminToken));
        response.EnsureSuccessStatusCode();

        var summary = (await response.Content.ReadFromJsonAsync<WaiverReportSummaryResponse>())!;
        Assert.True(summary.TotalWaivers >= 2);
        Assert.True(summary.PendingCount >= 1);
        Assert.True(summary.ApprovedCount >= 1);
        Assert.True(summary.ExpiringSoonCount >= 1);
    }

    [Fact]
    public async Task Exception_exemption_report_summary_returns_aggregates()
    {
        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/exception-exemptions/summary", _adminToken));
        response.EnsureSuccessStatusCode();

        var summary = (await response.Content.ReadFromJsonAsync<ExceptionExemptionReportSummaryResponse>())!;
        Assert.True(summary.TotalExceptionExemptions >= 2);
        Assert.True(summary.ActiveCount >= 1);
        Assert.True(summary.WaiverTypeCount >= 1);
        Assert.True(summary.VarianceTypeCount >= 1);
    }

    [Fact]
    public async Task Product_integration_health_report_summary_returns_aggregates()
    {
        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/integration-health/summary", _adminToken));
        response.EnsureSuccessStatusCode();

        var summary = (await response.Content.ReadFromJsonAsync<ProductIntegrationHealthReportSummaryResponse>())!;
        Assert.Equal(PlatformSeeder.DemoTenantId, summary.TenantId);
        Assert.True(summary.ProductApiSourceCount >= 1);
        Assert.True(summary.HealthyCount >= 1);
        Assert.True(summary.WorkerEnabled);
    }

    [Fact]
    public async Task Audit_readiness_report_summary_returns_forecasts()
    {
        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/audit-readiness/summary", _adminToken));
        response.EnsureSuccessStatusCode();

        var summary = (await response.Content.ReadFromJsonAsync<AuditReadinessReportSummaryResponse>())!;
        Assert.True(summary.TotalForecasts >= 1);
        Assert.True(summary.ReadinessScore >= 0);
        Assert.NotEmpty(summary.Forecasts);
    }

    [Fact]
    public async Task Regulatory_domain_coverage_report_summary_returns_program_coverage()
    {
        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/regulatory-domains/summary", _adminToken));
        response.EnsureSuccessStatusCode();

        var summary = (await response.Content.ReadFromJsonAsync<RegulatoryDomainCoverageReportSummaryResponse>())!;
        Assert.True(summary.TotalPrograms >= 1);
        Assert.True(summary.TotalRulePacks >= 2);
        Assert.True(summary.OperationalRulePackCount >= 1);
        Assert.True(summary.ReferenceRulePackCount >= 1);
        Assert.Contains(summary.Programs, item => item.ProgramKey == "phmsa_hmr");
        Assert.Contains(summary.Programs, item => item.ProgramKey == "phmsa_hmr" && item.CoverageMode == "mixed");

        var exportResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/regulatory-domains/summary/export", _adminToken));
        exportResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", exportResponse.Content.Headers.ContentType?.MediaType);
        var csv = await exportResponse.Content.ReadAsStringAsync();
        Assert.Contains("phmsa_hmr", csv, StringComparison.Ordinal);
        Assert.Contains("mixed", csv, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Hazmat_table_coverage_report_summary_enumerates_material_keys()
    {
        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/hazmat-table/summary", _adminToken));
        response.EnsureSuccessStatusCode();

        var summary = (await response.Content.ReadFromJsonAsync<HazmatTableCoverageReportSummaryResponse>())!;
        Assert.True(summary.TotalMaterialKeys >= 2);
        Assert.True(summary.LookupControlledCount >= 1);
        Assert.True(summary.UnmappedCount >= 1);
        Assert.Contains(summary.MaterialKeys, item => item.Key == "flammable_liquid" && item.CoverageMode == "lookup_and_citation");
        Assert.Contains(summary.MaterialKeys, item => item.Key == "non_regulated_inert" && item.CoverageMode == "unmapped");

        var exportResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/hazmat-table/summary/export", _adminToken));
        exportResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", exportResponse.Content.Headers.ContentType?.MediaType);
        var csv = await exportResponse.Content.ReadAsStringAsync();
        Assert.Contains("flammable_liquid", csv, StringComparison.Ordinal);
        Assert.Contains("lookup_and_citation", csv, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Title49_coverage_explorer_summary_enumerates_coverage_types()
    {
        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/title49/summary", _adminToken));
        response.EnsureSuccessStatusCode();

        var summary = (await response.Content.ReadFromJsonAsync<Title49CoverageExplorerResponse>())!;
        Assert.Equal(3, summary.TotalRulePacks);
        Assert.True(summary.OperationalRulePackCount >= 1);
        Assert.True(summary.ReferenceRulePackCount >= 1);
        Assert.True(summary.MetadataRulePackCount >= 1);
        Assert.Equal(2, summary.TotalCitations);
        Assert.Equal(1, summary.TotalFacts);
        Assert.Contains(summary.RulePacks, item => item.PackKey == "title49_hmr_operational" && item.CoverageKind == "operational");
        Assert.Contains(summary.RulePacks, item => item.PackKey == "title49_hmr_reference" && item.CoverageKind == "reference");
        Assert.Contains(summary.RulePacks, item => item.PackKey == "title49_hmr_metadata_metadata" && item.CoverageKind == "metadata");

        var exportResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/title49/summary/export", _adminToken));
        exportResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", exportResponse.Content.Headers.ContentType?.MediaType);
        var csv = await exportResponse.Content.ReadAsStringAsync();
        Assert.Contains("title49_hmr_operational", csv, StringComparison.Ordinal);
        Assert.Contains("metadata", csv, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Title49_citation_coverage_report_summary_enumerates_legal_states()
    {
        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/title49/citations/summary", _adminToken));
        response.EnsureSuccessStatusCode();

        var summary = (await response.Content.ReadFromJsonAsync<Title49CitationCoverageReportResponse>())!;
        Assert.Equal(4, summary.TotalCitations);
        Assert.Equal(4, summary.ActiveCitationCount);
        Assert.Equal(2, summary.OperationalCitationCount);
        Assert.Equal(1, summary.ReferenceCitationCount);
        Assert.Equal(1, summary.UnmappedCitationCount);
        Assert.Equal(3, summary.TotalRulePacks);
        Assert.Equal(1, summary.TotalFactRequirements);
        Assert.Equal(3, summary.TotalMappings);
        Assert.Contains(summary.Citations, item => item.CitationKey == "cfr_172_101_title49" && item.CoverageMode == "operational");
        Assert.Contains(summary.Citations, item => item.CitationKey == "cfr_172_102_title49" && item.CoverageMode == "reference");
        Assert.Contains(summary.Citations, item => item.CitationKey == "cfr_172_103_title49" && item.CoverageMode == "unmapped");

        var exportResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/title49/citations/summary/export", _adminToken));
        exportResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", exportResponse.Content.Headers.ContentType?.MediaType);
        var csv = await exportResponse.Content.ReadAsStringAsync();
        Assert.Contains("cfr_172_101_title49", csv, StringComparison.Ordinal);
        Assert.Contains("unmapped", csv, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Rule_change_impact_report_summarizes_downstream_effects()
    {
        var adminToken = _adminToken;
        var programId = await CreateImpactProgramAsync(adminToken);
        var packId = await CreateImpactRulePackAsync(adminToken, programId, "impact_monitor_pack");

        var contentRequest = Authorized(HttpMethod.Put, $"/api/rule-packs/{packId}/content", adminToken);
        contentRequest.Content = JsonContent.Create(new UpdateRulePackContentRequest(
            new RulePackContentBody(
                1,
                "all",
                [new RuleDefinitionDto("impact_rule", "Impact rule", "fact_boolean", "impact_fact", true)])));
        (await _complianceCoreClient.SendAsync(contentRequest)).EnsureSuccessStatusCode();

        var reviewRequest = Authorized(HttpMethod.Patch, $"/api/rule-packs/{packId}/status", adminToken);
        reviewRequest.Content = JsonContent.Create(new UpdateRulePackStatusRequest(RulePackStatuses.Review));
        (await _complianceCoreClient.SendAsync(reviewRequest)).EnsureSuccessStatusCode();

        var publishRequest = Authorized(HttpMethod.Patch, $"/api/rule-packs/{packId}/status", adminToken);
        publishRequest.Content = JsonContent.Create(new UpdateRulePackStatusRequest(RulePackStatuses.Published));
        (await _complianceCoreClient.SendAsync(publishRequest)).EnsureSuccessStatusCode();

        using (var scope = _complianceCoreFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
            var now = DateTimeOffset.UtcNow;
            db.RuleEvaluationRuns.Add(new RuleEvaluationRun
            {
                Id = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                RulePackId = packId,
                ActorUserId = PlatformSeeder.DemoAdminUserId,
                Status = RuleEvaluationRunStatuses.Completed,
                OverallResult = RuleEvaluationResults.Fail,
                FactInputsJson = "{}",
                RuleResultsJson = "[]",
                CreatedAt = now,
            });

            db.ComplianceFindings.Add(new ComplianceFinding
            {
                Id = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                RulePackId = packId,
                RuleEvaluationRunId = null,
                FindingKey = "impact_finding",
                Severity = FindingSeverities.Block,
                Status = FindingStatuses.Open,
                RuleKey = "impact_rule",
                FactKey = "impact_fact",
                Title = "Impact finding",
                Message = "Impact report seed finding.",
                ReasonCode = "impact_seed",
                CreatedAt = now,
            });

            db.ComplianceWaivers.Add(new ComplianceWaiver
            {
                Id = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                WaiverKey = "impact_waiver",
                RulePackId = packId,
                PackKey = "impact_monitor_pack",
                RuleKey = "impact_rule",
                GateKey = null,
                SubjectScopeKey = "tenant",
                ReasonCode = "impact_seed",
                Explanation = "Impact report seed waiver.",
                Status = WaiverStatuses.Approved,
                EffectiveAt = now.AddHours(-1),
                ExpiresAt = now.AddDays(7),
                ApprovedByUserId = PlatformSeeder.DemoAdminUserId,
                ApprovedAt = now.AddMinutes(-5),
                CreatedByUserId = PlatformSeeder.DemoAdminUserId,
                CreatedAt = now.AddHours(-2),
                UpdatedAt = now.AddMinutes(-5),
            });

        await db.SaveChangesAsync();
        }

        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/rule-changes/impact/summary?packKey=impact_monitor_pack", adminToken));
        response.EnsureSuccessStatusCode();

        var summary = (await response.Content.ReadFromJsonAsync<RuleChangeImpactReportResponse>())!;
        Assert.Equal(1, summary.TotalImpactedRulePacks);
        Assert.Equal(4, summary.TotalChangeEvents);
        Assert.Equal(1, summary.TotalEvaluationRuns);
        Assert.Equal(1, summary.TotalFindings);
        Assert.Equal(1, summary.TotalWaivers);
        Assert.Single(summary.RulePacks);
        Assert.Equal("impact_monitor_pack", summary.RulePacks[0].PackKey);
        Assert.True(summary.RulePacks[0].ChangeEventCount >= 4);
        Assert.True(summary.RulePacks[0].EvaluationRunCount >= 1);
        Assert.True(summary.RulePacks[0].FindingCount >= 1);
        Assert.True(summary.RulePacks[0].WaiverCount >= 1);
        Assert.Equal("status_changed", summary.RulePacks[0].LatestChangeType);

        var exportResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/rule-changes/impact/summary/export?packKey=impact_monitor_pack", adminToken));
        exportResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", exportResponse.Content.Headers.ContentType?.MediaType);
        var csv = await exportResponse.Content.ReadAsStringAsync();
        Assert.Contains("impact_monitor_pack", csv, StringComparison.Ordinal);
        Assert.Contains("status_changed", csv, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Evaluation_history_explorer_paginates_runs_and_exports_csv()
    {
        var rulePackId = await GetSeedRulePackIdAsync();

        var passRequest = Authorized(HttpMethod.Post, $"/api/rule-packs/{rulePackId}/evaluate", _adminToken);
        passRequest.Content = JsonContent.Create(new EvaluateRulePackRequest(new Dictionary<string, bool>
        {
            ["driver_license_valid"] = true,
        }));
        (await _complianceCoreClient.SendAsync(passRequest)).EnsureSuccessStatusCode();

        var failRequest = Authorized(HttpMethod.Post, $"/api/rule-packs/{rulePackId}/evaluate", _adminToken);
        failRequest.Content = JsonContent.Create(new EvaluateRulePackRequest(new Dictionary<string, bool>
        {
            ["driver_license_valid"] = false,
        }));
        (await _complianceCoreClient.SendAsync(failRequest)).EnsureSuccessStatusCode();

        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/reports/evaluations/summary?rulePackId={rulePackId}&limit=1&offset=0", _adminToken));
        response.EnsureSuccessStatusCode();

        var summary = (await response.Content.ReadFromJsonAsync<EvaluationHistoryExplorerResponse>())!;
        Assert.Equal(rulePackId, summary.RulePackId);
        Assert.Equal(2, summary.TotalRuns);
        Assert.Equal(1, summary.PassedCount);
        Assert.Equal(1, summary.FailedCount);
        Assert.Equal(1, summary.Limit);
        Assert.Equal(0, summary.Offset);
        Assert.True(summary.HasMore);
        Assert.Single(summary.Runs);
        Assert.Equal("fail", summary.Runs[0].OverallResult);

        var exportResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/reports/evaluations/summary/export?rulePackId={rulePackId}", _adminToken));
        exportResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", exportResponse.Content.Headers.ContentType?.MediaType);
        var csv = await exportResponse.Content.ReadAsStringAsync();
        Assert.Contains(summary.RulePackId.ToString(), csv, StringComparison.Ordinal);
        Assert.Contains("fail", csv, StringComparison.Ordinal);
    }

    [Fact]
    public async Task V1_reports_index_summaries_and_export_are_available()
    {
        var indexResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports", _adminToken));
        indexResponse.EnsureSuccessStatusCode();
        var indexJson = await indexResponse.Content.ReadAsStringAsync();
        Assert.Contains("/api/v1/reports/findings", indexJson, StringComparison.Ordinal);
        Assert.Contains("/api/v1/reports/operator", indexJson, StringComparison.Ordinal);
        Assert.Contains("/api/v1/reports/evidence/missing", indexJson, StringComparison.Ordinal);
        Assert.Contains("/api/v1/reports/evidence/completeness", indexJson, StringComparison.Ordinal);
        Assert.Contains("/api/v1/reports/waivers", indexJson, StringComparison.Ordinal);
        Assert.Contains("/api/v1/reports/exception-exemptions", indexJson, StringComparison.Ordinal);
        Assert.Contains("/api/v1/reports/integration-health", indexJson, StringComparison.Ordinal);
        Assert.Contains("/api/v1/reports/audit-readiness", indexJson, StringComparison.Ordinal);
        Assert.Contains("/api/v1/reports/remediation-queue", indexJson, StringComparison.Ordinal);
        Assert.Contains("/api/v1/reports/regulatory-domains", indexJson, StringComparison.Ordinal);
        Assert.Contains("/api/v1/reports/hazmat-table", indexJson, StringComparison.Ordinal);
        Assert.Contains("/api/v1/reports/title49", indexJson, StringComparison.Ordinal);
        Assert.Contains("/api/v1/reports/title49/citations", indexJson, StringComparison.Ordinal);
        Assert.Contains("/api/v1/reports/citation-review", indexJson, StringComparison.Ordinal);
        Assert.Contains("/api/v1/reports/rule-changes/impact", indexJson, StringComparison.Ordinal);
        Assert.Contains("/api/v1/reports/evaluations", indexJson, StringComparison.Ordinal);

        var findingsResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/findings/summary?openOnly=true", _adminToken));
        findingsResponse.EnsureSuccessStatusCode();
        var findings = (await findingsResponse.Content.ReadFromJsonAsync<FindingsReportSummaryResponse>())!;
        Assert.True(findings.OpenCount >= 1);

        var operatorResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/operator/summary?attentionOnly=true", _adminToken));
        operatorResponse.EnsureSuccessStatusCode();
        var operatorSummary = (await operatorResponse.Content.ReadFromJsonAsync<OperatorReportSummaryResponse>())!;
        Assert.True(operatorSummary.EvaluationFailCount >= 1);

        var exportResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/findings/summary/export?openOnly=true", _adminToken));
        exportResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", exportResponse.Content.Headers.ContentType?.MediaType);

        var missingEvidenceResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/evidence/missing/summary?severity=high", _adminToken));
        missingEvidenceResponse.EnsureSuccessStatusCode();
        var missingEvidence = (await missingEvidenceResponse.Content.ReadFromJsonAsync<MissingEvidenceReportSummaryResponse>())!;
        Assert.True(missingEvidence.TotalWarnings >= 1);

        var missingEvidenceExportResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/evidence/missing/summary/export?severity=high", _adminToken));
        missingEvidenceExportResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", missingEvidenceExportResponse.Content.Headers.ContentType?.MediaType);

        var waiverResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/waivers/summary?status=all", _adminToken));
        waiverResponse.EnsureSuccessStatusCode();
        var waiverSummary = (await waiverResponse.Content.ReadFromJsonAsync<WaiverReportSummaryResponse>())!;
        Assert.True(waiverSummary.TotalWaivers >= 1);

        var waiverExportResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/waivers/summary/export?status=all", _adminToken));
        waiverExportResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", waiverExportResponse.Content.Headers.ContentType?.MediaType);

        var exceptionResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/exception-exemptions/summary?activeOnly=true", _adminToken));
        exceptionResponse.EnsureSuccessStatusCode();
        var exceptionSummary = (await exceptionResponse.Content.ReadFromJsonAsync<ExceptionExemptionReportSummaryResponse>())!;
        Assert.True(exceptionSummary.ActiveCount >= 1);

        var exceptionExportResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/exception-exemptions/summary/export?activeOnly=true", _adminToken));
        exceptionExportResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", exceptionExportResponse.Content.Headers.ContentType?.MediaType);

        var integrationHealthResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/integration-health/summary", _adminToken));
        integrationHealthResponse.EnsureSuccessStatusCode();
        var integrationHealth = (await integrationHealthResponse.Content.ReadFromJsonAsync<ProductIntegrationHealthReportSummaryResponse>())!;
        Assert.True(integrationHealth.ProductApiSourceCount >= 1);

        var integrationHealthExportResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/integration-health/summary/export", _adminToken));
        integrationHealthExportResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", integrationHealthExportResponse.Content.Headers.ContentType?.MediaType);

        var auditReadinessResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/audit-readiness/summary?limit=5", _adminToken));
        auditReadinessResponse.EnsureSuccessStatusCode();
        var auditReadiness = (await auditReadinessResponse.Content.ReadFromJsonAsync<AuditReadinessReportSummaryResponse>())!;
        Assert.True(auditReadiness.TotalForecasts >= 1);

        var auditReadinessExportResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/audit-readiness/summary/export", _adminToken));
        auditReadinessExportResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", auditReadinessExportResponse.Content.Headers.ContentType?.MediaType);

        var remediationQueueResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/remediation-queue/summary?queueOnly=true", _adminToken));
        remediationQueueResponse.EnsureSuccessStatusCode();
        var remediationQueue = (await remediationQueueResponse.Content.ReadFromJsonAsync<RemediationQueueReportSummaryResponse>())!;
        Assert.True(remediationQueue.TotalWarnings >= 1);

        var remediationQueueExportResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/remediation-queue/summary/export?queueOnly=true", _adminToken));
        remediationQueueExportResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", remediationQueueExportResponse.Content.Headers.ContentType?.MediaType);

        var regulatoryDomainsResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/regulatory-domains/summary", _adminToken));
        regulatoryDomainsResponse.EnsureSuccessStatusCode();
        var regulatoryDomains = (await regulatoryDomainsResponse.Content.ReadFromJsonAsync<RegulatoryDomainCoverageReportSummaryResponse>())!;
        Assert.True(regulatoryDomains.TotalPrograms >= 1);

        var regulatoryDomainsExportResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/regulatory-domains/summary/export", _adminToken));
        regulatoryDomainsExportResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", regulatoryDomainsExportResponse.Content.Headers.ContentType?.MediaType);

        var hazmatTableResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/hazmat-table/summary", _adminToken));
        hazmatTableResponse.EnsureSuccessStatusCode();
        var hazmatTable = (await hazmatTableResponse.Content.ReadFromJsonAsync<HazmatTableCoverageReportSummaryResponse>())!;
        Assert.True(hazmatTable.TotalMaterialKeys >= 2);

        var hazmatTableExportResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/hazmat-table/summary/export", _adminToken));
        hazmatTableExportResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", hazmatTableExportResponse.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Entity_export_manifest_lists_current_entities()
    {
        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/exports/manifest", _adminToken));
        response.EnsureSuccessStatusCode();

        var manifest = (await response.Content.ReadFromJsonAsync<EntityExportManifestResponse>())!;
        Assert.Equal(4, manifest.Entities.Count);
        Assert.Contains(manifest.Entities, entity => entity.EntityKey == "findings");
        Assert.Contains(manifest.Entities, entity => entity.EntityKey == "rule_packs");
        Assert.Contains(manifest.ReportExports, report =>
            report.ReportKey == "missing_evidence"
            && report.ExportPath == "/api/reports/evidence/missing/summary/export");
        Assert.Contains(manifest.ReportExports, report =>
            report.ReportKey == "evidence_completeness"
            && report.ExportPath == "/api/reports/evidence/completeness/summary/export");
        Assert.Contains(manifest.ReportExports, report =>
            report.ReportKey == "waivers"
            && report.ExportPath == "/api/reports/waivers/summary/export");
        Assert.Contains(manifest.ReportExports, report =>
            report.ReportKey == "exception_exemptions"
            && report.ExportPath == "/api/reports/exception-exemptions/summary/export");
        Assert.Contains(manifest.ReportExports, report =>
            report.ReportKey == "integration_health"
            && report.ExportPath == "/api/reports/integration-health/summary/export");
        Assert.Contains(manifest.ReportExports, report =>
            report.ReportKey == "audit_readiness"
            && report.ExportPath == "/api/reports/audit-readiness/summary/export");
        Assert.Contains(manifest.ReportExports, report =>
            report.ReportKey == "remediation_queue"
            && report.ExportPath == "/api/reports/remediation-queue/summary/export");
        Assert.Contains(manifest.ReportExports, report =>
            report.ReportKey == "citation_review"
            && report.ExportPath == "/api/reports/citation-review/summary/export");
    }

    [Fact]
    public async Task Citation_review_report_summary_tracks_review_states()
    {
        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/citation-review/summary?programKey=citation_review_program&limit=5", _adminToken));
        response.EnsureSuccessStatusCode();

        var summary = (await response.Content.ReadFromJsonAsync<CitationReviewReportSummaryResponse>())!;
        Assert.Equal(5, summary.TotalCitations);
        Assert.Equal(3, summary.ActiveCitationCount);
        Assert.Equal(2, summary.ReviewedCitationCount);
        Assert.Equal(1, summary.NeedsReviewCitationCount);
        Assert.Equal(1, summary.InactiveCitationCount);
        Assert.Equal(1, summary.SupersededCitationCount);
        Assert.Equal(2, summary.LinkedRulePackCount);
        Assert.Equal(2, summary.TotalFactRequirementCount);
        Assert.Equal(2, summary.TotalMappingCount);
        Assert.Equal(5, summary.Citations.Count);
        Assert.Contains(summary.Citations, item => item.CitationKey == "cfr_172_200_title49" && item.ReviewState == "reviewed");
        Assert.Contains(summary.Citations, item => item.CitationKey == "cfr_172_201_title49" && item.VersionNumber == 2 && item.ReviewState == "reviewed");
        Assert.Contains(summary.Citations, item => item.CitationKey == "cfr_172_202_title49" && item.ReviewState == "needs_review");
        Assert.Contains(summary.Citations, item => item.CitationKey == "cfr_172_201_title49" && item.VersionNumber == 1 && item.ReviewState == "superseded");

        var exportResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/citation-review/summary/export?programKey=citation_review_program", _adminToken));
        exportResponse.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", exportResponse.Content.Headers.ContentType?.MediaType);
        var csv = await exportResponse.Content.ReadAsStringAsync();
        Assert.Contains("cfr_172_200_title49", csv, StringComparison.Ordinal);
        Assert.Contains("cfr_172_201_title49", csv, StringComparison.Ordinal);
        Assert.Contains("needs_review", csv, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Tenant_member_can_read_findings_report_but_cannot_export_manifest()
    {
        var memberToken = CreateComplianceCoreAccessToken(["compliancecore"], tenantRoleKey: "tenant_member");

        var readResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/findings/summary", memberToken));
        readResponse.EnsureSuccessStatusCode();

        var exportResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/exports/manifest", memberToken));
        Assert.Equal(HttpStatusCode.Forbidden, exportResponse.StatusCode);
    }

    private async Task SeedFindingViaEvaluationAsync()
    {
        var programId = await CreateRegulatoryProgramAsync(_adminToken);
        var createPackRequest = Authorized(HttpMethod.Post, "/api/rule-packs", _adminToken);
        createPackRequest.Content = JsonContent.Create(new CreateRulePackRequest(
            programId,
            "driver_qualification",
            "Driver Qualification Rules",
            "Baseline driver qualification rule pack."));
        var pack = (await (await _complianceCoreClient.SendAsync(createPackRequest)).Content.ReadFromJsonAsync<RulePackResponse>())!;

        var contentRequest = Authorized(HttpMethod.Put, $"/api/rule-packs/{pack.RulePackId}/content", _adminToken);
        contentRequest.Content = JsonContent.Create(new UpdateRulePackContentRequest(new RulePackContentBody(
            1,
            "all",
            [
                new RuleDefinitionDto(
                    "license_valid",
                    "Valid driver license",
                    "fact_boolean",
                    "driver_license_valid",
                    true),
            ])));
        (await _complianceCoreClient.SendAsync(contentRequest)).EnsureSuccessStatusCode();

        var evaluateRequest = Authorized(HttpMethod.Post, $"/api/rule-packs/{pack.RulePackId}/evaluate", _adminToken);
        evaluateRequest.Content = JsonContent.Create(new EvaluateRulePackRequest(
            new Dictionary<string, bool> { ["driver_license_valid"] = false },
            EmitFindings: true));
        (await _complianceCoreClient.SendAsync(evaluateRequest)).EnsureSuccessStatusCode();

        using var scope = _complianceCoreFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        var now = DateTimeOffset.UtcNow;

        var warningRunId = Guid.NewGuid();
        db.MissingEvidenceWarningRuns.Add(new MissingEvidenceWarningRun
        {
            Id = warningRunId,
            TenantId = PlatformSeeder.DemoTenantId,
            ScopeKey = "tenant",
            PacksAnalyzedCount = 1,
            WarningsEmittedCount = 1,
            HighestSeverity = MissingEvidenceWarningSeverities.High,
            EvaluatedAt = now
        });

        db.MissingEvidenceWarnings.Add(new MissingEvidenceWarning
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            RunId = warningRunId,
            ScopeKey = "tenant",
            RulePackId = pack.RulePackId,
            PackKey = pack.PackKey,
            FactKey = "driver_license_valid",
            WarningType = MissingEvidenceWarningTypes.RulePackFact,
            Severity = MissingEvidenceWarningSeverities.High,
            ReasonCode = MissingEvidenceReasonCodes.MissingMirror,
            HasMirrorAtScope = false,
            IsRequiredInRule = true,
            IsRequiredInCatalog = true,
            Summary = "Missing evidence for driver license validation fact.",
            EvaluatedAt = now
        });

        db.ComplianceWaivers.Add(new ComplianceWaiver
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            WaiverKey = "expiring-waiver-seed",
            RulePackId = pack.RulePackId,
            PackKey = pack.PackKey,
            SubjectScopeKey = "tenant",
            ReasonCode = "operations_override",
            Explanation = "Temporary override while vendor evidence is in transit.",
            Status = WaiverStatuses.Approved,
            EffectiveAt = now.AddDays(-1),
            ExpiresAt = now.AddDays(3),
            CreatedByUserId = PlatformSeeder.DemoAdminUserId,
            ApprovedByUserId = PlatformSeeder.DemoAdminUserId,
            ApprovedAt = now.AddDays(-1),
            CreatedAt = now.AddDays(-1),
            UpdatedAt = now
        });

        await db.SaveChangesAsync();
    }

    private async Task SeedExceptionExemptionsAsync()
    {
        using var scope = _complianceCoreFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        var now = DateTimeOffset.UtcNow;

        db.ComplianceExceptionExemptions.AddRange(
            new ComplianceExceptionExemption
            {
                ExceptionExemptionId = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                Key = "variance-driver-hours",
                Label = "Driver hours variance",
                Type = ComplianceExceptionExemptionTypes.Variance,
                GoverningBody = "dot",
                ProgramKey = "fmcsa_safety",
                PackKey = "driver_qualification",
                CitationKey = "cfr_395_3",
                ApplicabilityKey = "hours_of_service",
                AppliesToSubjectKind = "person",
                AppliesToSourceProduct = "staffarr",
                AppliesToSourceEntity = "people",
                EffectType = ComplianceExceptionExemptionEffectTypes.ExtendsDeadline,
                ConditionLogicJson = "{\"all\":[{\"fact\":\"hours_remaining\",\"gte\":8}]}",
                IssuingAuthority = "FMCSA",
                AuthorizationNumber = "VA-001",
                EffectiveAt = now.AddDays(-5),
                ExpiresAt = now.AddDays(5),
                Active = true,
                Description = "Variance for controlled hours-of-service operations.",
                CreatedAt = now.AddDays(-5),
                UpdatedAt = now,
            },
            new ComplianceExceptionExemption
            {
                ExceptionExemptionId = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                Key = "waiver-medical-docs",
                Label = "Medical documentation waiver",
                Type = ComplianceExceptionExemptionTypes.Waiver,
                GoverningBody = "dot",
                ProgramKey = "fmcsa_safety",
                PackKey = "driver_qualification",
                CitationKey = "cfr_391_41",
                ApplicabilityKey = "medical_certification",
                AppliesToSubjectKind = "person",
                AppliesToSourceProduct = "staffarr",
                AppliesToSourceEntity = "people",
                EffectType = ComplianceExceptionExemptionEffectTypes.MakesRequirementNotApplicable,
                ConditionLogicJson = "{\"any\":[{\"fact\":\"temporary_clearance\",\"eq\":true}]}",
                IssuingAuthority = "FMCSA",
                AuthorizationNumber = "WA-002",
                EffectiveAt = now.AddDays(-20),
                ExpiresAt = now.AddDays(-2),
                Active = false,
                Description = "Waiver for temporary medical documentation relief.",
                CreatedAt = now.AddDays(-20),
                UpdatedAt = now.AddDays(-2),
            });

        await db.SaveChangesAsync();
    }

    private async Task SeedProductIntegrationHealthAsync()
    {
        using var scope = _complianceCoreFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        var now = DateTimeOffset.UtcNow;

        var factDefinitionId = Guid.NewGuid();
        var factSourceId = Guid.NewGuid();

        db.FactDefinitions.Add(new FactDefinition
        {
            Id = factDefinitionId,
            TenantId = PlatformSeeder.DemoTenantId,
            FactKey = "medical_cert_on_file",
            Label = "Medical certification on file",
            Description = "Seeded source for product integration health coverage.",
            ValueType = FactValueTypes.String,
            IsActive = true,
            CreatedAt = now.AddDays(-2),
            UpdatedAt = now.AddDays(-2),
        });

        db.FactSources.Add(new FactSource
        {
            Id = factSourceId,
            TenantId = PlatformSeeder.DemoTenantId,
            FactDefinitionId = factDefinitionId,
            SourceKey = "staffarr_med_cert",
            SourceType = FactSourceTypes.ProductApi,
            Label = "StaffArr medical certification",
            Description = "Seeded product API source used by the integration health report.",
            ProductKey = "staffarr",
            ProductReference = "people.medical_cert_on_file",
            ConfigJson = "{\"scopeKey\":\"tenant\",\"stringValue\":\"active\"}",
            Priority = 1,
            IsActive = true,
            CreatedAt = now.AddDays(-2),
            UpdatedAt = now.AddDays(-2),
        });

        db.FactSourceSyncStatuses.Add(new FactSourceSyncStatus
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            FactSourceId = factSourceId,
            ScopeKey = "tenant",
            HealthStatus = FactSourceSyncStatuses.Healthy,
            LastAttemptAt = now.AddMinutes(-5),
            LastSuccessAt = now.AddMinutes(-5),
            LastFailureAt = null,
            LastErrorMessage = null,
            ConsecutiveFailureCount = 0,
            CreatedAt = now.AddMinutes(-5),
            UpdatedAt = now,
        });

        db.TenantFactSourceSyncWorkerSettings.Add(new TenantFactSourceSyncWorkerSettings
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            IsEnabled = true,
            DefaultScopeKey = "tenant",
            IntervalMinutes = 60,
            LastBatchRunAt = now.AddMinutes(-10),
            CreatedAt = now.AddHours(-1),
            UpdatedAt = now.AddMinutes(-10),
        });

        await db.SaveChangesAsync();
    }

    private async Task SeedAuditReadinessForecastAsync()
    {
        using var scope = _complianceCoreFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        var now = DateTimeOffset.UtcNow;
        var packId = Guid.NewGuid();
        var programId = Guid.NewGuid();
        var runId = Guid.NewGuid();

        db.RegulatoryPrograms.Add(new RegulatoryProgram
        {
            Id = programId,
            TenantId = PlatformSeeder.DemoTenantId,
            JurisdictionId = Guid.NewGuid(),
            ProgramKey = "audit_readiness_program",
            Label = "Audit Readiness Program",
            Description = "Seeded program for audit readiness report coverage.",
            IsActive = true,
            CreatedAt = now.AddDays(-2),
            UpdatedAt = now.AddDays(-2),
        });

        db.RulePacks.Add(new RulePack
        {
            Id = packId,
            TenantId = PlatformSeeder.DemoTenantId,
            RegulatoryProgramId = programId,
            PackKey = "audit_readiness_pack",
            Label = "Audit Readiness Pack",
            Description = "Seeded pack for audit readiness report coverage.",
            VersionNumber = 1,
            Status = RulePackStatuses.Published,
            IsActive = true,
            CreatedAt = now.AddDays(-2),
            UpdatedAt = now.AddDays(-1),
            RuleContentJson = "{}",
        });

        db.ReadinessForecastRuns.Add(new ReadinessForecastRun
        {
            Id = runId,
            TenantId = PlatformSeeder.DemoTenantId,
            ScopeKey = "tenant",
            ActorUserId = PlatformSeeder.DemoAdminUserId,
            PacksForecastCount = 1,
            ReadinessScore = 88,
            ReadinessLevel = ReadinessForecastLevels.Ready,
            LowestReadinessScore = 88,
            AverageReadinessScore = 88,
            HighestRiskScore = 10,
            MissingEvidenceWarningCount = 0,
            AverageEffectivenessScore = 90,
            RiskScoreRunId = Guid.NewGuid(),
            MissingEvidenceWarningRunId = Guid.NewGuid(),
            ControlEffectivenessRunId = Guid.NewGuid(),
            ForecastedAt = now.AddMinutes(-15),
        });

        db.ReadinessForecasts.Add(new ReadinessForecast
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            RunId = runId,
            ScopeKey = "tenant",
            RulePackId = packId,
            PackKey = "audit_readiness_pack",
            ReadinessScore = 88,
            ReadinessLevel = ReadinessForecastLevels.Ready,
            RiskScore = 10,
            RiskLevel = "low",
            EffectivenessScore = 90,
            EffectivenessLevel = "high",
            MissingEvidenceWarningCount = 0,
            HighestMissingEvidenceSeverity = MissingEvidenceWarningSeverities.Low,
            Summary = "Readiness forecast for audit_readiness_pack: 88 (ready) from risk 10, effectiveness 90, 0 missing-evidence warning(s).",
            ForecastedAt = now.AddMinutes(-15),
        });

        await db.SaveChangesAsync();
    }

    private async Task SeedTitle49CoverageExplorerAsync()
    {
        using var scope = _complianceCoreFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        var now = DateTimeOffset.UtcNow;

        var governingBodyId = Guid.NewGuid();
        var jurisdictionId = Guid.NewGuid();
        var programId = Guid.NewGuid();
        var operationalPackId = Guid.NewGuid();
        var referencePackId = Guid.NewGuid();
        var metadataPackId = Guid.NewGuid();
        var citationId = Guid.NewGuid();
        var referenceCitationId = Guid.NewGuid();
        var unmappedCitationId = Guid.NewGuid();
        var complianceKeyId = Guid.NewGuid();
        var factDefinitionId = Guid.NewGuid();

        db.GoverningBodies.Add(new GoverningBody
        {
            Id = governingBodyId,
            TenantId = PlatformSeeder.DemoTenantId,
            BodyKey = "dot",
            Label = "U.S. Department of Transportation",
            Description = "Federal transportation safety and compliance authority.",
            IsActive = true,
            CreatedAt = now.AddDays(-2),
            UpdatedAt = now.AddDays(-2),
        });

        db.Jurisdictions.Add(new Jurisdiction
        {
            Id = jurisdictionId,
            TenantId = PlatformSeeder.DemoTenantId,
            GoverningBodyId = governingBodyId,
            JurisdictionKey = "us_federal",
            Label = "United States Federal",
            Description = "Federal jurisdiction for Title 49 coverage reporting.",
            IsActive = true,
            CreatedAt = now.AddDays(-2),
            UpdatedAt = now.AddDays(-2),
        });

        db.RegulatoryPrograms.Add(new RegulatoryProgram
        {
            Id = programId,
            TenantId = PlatformSeeder.DemoTenantId,
            JurisdictionId = jurisdictionId,
            ProgramKey = "title49_hmr",
            Label = "Title 49 HMR",
            Description = "Title 49 coverage explorer verification program.",
            IsActive = true,
            CreatedAt = now.AddDays(-2),
            UpdatedAt = now.AddDays(-1),
        });

        db.RulePacks.AddRange(
            new RulePack
            {
                Id = operationalPackId,
                TenantId = PlatformSeeder.DemoTenantId,
                RegulatoryProgramId = programId,
                PackKey = "title49_hmr_operational",
                Label = "Title 49 HMR Operational Pack",
                Description = "Operational Title 49 coverage pack.",
                VersionNumber = 1,
                Status = RulePackStatuses.Published,
                IsActive = true,
                CreatedAt = now.AddDays(-1),
                UpdatedAt = now,
                RuleContentJson = """{"schemaVersion":1,"logic":"all","rules":[{"ruleKey":"title49_operational","label":"Operational rule","type":"fact_boolean","factKey":"title49_operational_fact","expectedValue":true}]}""",
            },
            new RulePack
            {
                Id = referencePackId,
                TenantId = PlatformSeeder.DemoTenantId,
                RegulatoryProgramId = programId,
                PackKey = "title49_hmr_reference",
                Label = "Title 49 HMR Reference Pack",
                Description = "Reference Title 49 coverage pack.",
                VersionNumber = 1,
                Status = RulePackStatuses.Published,
                IsActive = true,
                CreatedAt = now.AddDays(-1),
                UpdatedAt = now,
                RuleContentJson = string.Empty,
            },
            new RulePack
            {
                Id = metadataPackId,
                TenantId = PlatformSeeder.DemoTenantId,
                RegulatoryProgramId = programId,
                PackKey = "title49_hmr_metadata_metadata",
                Label = "Title 49 HMR Metadata Pack",
                Description = "Metadata-only Title 49 coverage pack.",
                VersionNumber = 1,
                Status = RulePackStatuses.Published,
                IsActive = true,
                CreatedAt = now.AddDays(-1),
                UpdatedAt = now,
                RuleContentJson = """{"schemaVersion":1,"logic":"all","rules":[{"ruleKey":"title49_metadata","label":"Metadata rule","type":"fact_boolean","factKey":"title49_metadata_fact","expectedValue":true}]}""",
            });

        db.RegulatoryCitations.Add(new RegulatoryCitation
        {
            Id = citationId,
            TenantId = PlatformSeeder.DemoTenantId,
            RegulatoryProgramId = programId,
            RulePackId = operationalPackId,
            CitationKey = "cfr_172_101_title49",
            Label = "Hazardous materials table",
            SourceReference = "49 CFR 172.101",
            Description = "Operational title49 citation.",
            VersionNumber = 1,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.RegulatoryCitations.AddRange(
            new RegulatoryCitation
            {
                Id = referenceCitationId,
                TenantId = PlatformSeeder.DemoTenantId,
                RegulatoryProgramId = programId,
                RulePackId = referencePackId,
                CitationKey = "cfr_172_102_title49",
                Label = "Reference hazmat citation",
                SourceReference = "49 CFR 172.102",
                Description = "Reference citation for the Title 49 coverage explorer.",
                VersionNumber = 1,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
            },
            new RegulatoryCitation
            {
                Id = unmappedCitationId,
                TenantId = PlatformSeeder.DemoTenantId,
                RegulatoryProgramId = programId,
                CitationKey = "cfr_172_103_title49",
                Label = "Unmapped hazmat citation",
                SourceReference = "49 CFR 172.103",
                Description = "Unmapped citation used to demonstrate legal coverage gaps.",
                VersionNumber = 1,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
            });

        db.ComplianceKeys.Add(new ComplianceKey
        {
            Id = complianceKeyId,
            TenantId = PlatformSeeder.DemoTenantId,
            Key = "title49_coverage_key",
            Label = "Title 49 coverage key",
            Category = "title49",
            Description = "Coverage key used for the Title 49 citation report.",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.RegulatoryMappings.Add(new RegulatoryMapping
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            MappingKey = "title49_operational_citation_mapping",
            Label = "Title 49 operational citation mapping",
            Description = "Mapping used by the citation coverage report.",
            TargetKind = "citation",
            RegulatoryProgramId = programId,
            RulePackId = operationalPackId,
            CitationId = citationId,
            ComplianceKeyId = complianceKeyId,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.FactDefinitions.Add(new FactDefinition
        {
            Id = factDefinitionId,
            TenantId = PlatformSeeder.DemoTenantId,
            FactKey = "title49_operational_fact",
            Label = "Title 49 operational fact",
            Description = "Fact used by the Title 49 operational pack.",
            ValueType = FactValueTypes.Boolean,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.FactRequirements.Add(new FactRequirement
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            FactDefinitionId = factDefinitionId,
            RulePackId = operationalPackId,
            CitationId = citationId,
            RequirementKey = "title49_operational_requirement",
            Label = "Title 49 operational requirement",
            Description = "Requirement used for the Title 49 explorer.",
            ApplicabilityKey = "hazmat",
            SourceProduct = "ComplianceCore",
            SourceEntity = "rule_packs",
            SourceFieldOrRecordType = "rule_content",
            ValueType = FactValueTypes.Boolean,
            Operator = FactRequirementOperators.Equal,
            ExpectedValue = "true",
            EvidenceKind = FactRequirementEvidenceKinds.SystemFact,
            RequiredDocumentType = string.Empty,
            RetentionPeriod = string.Empty,
            AuditQuestion = "Is the operational coverage requirement satisfied?",
            FailureSeverity = FactRequirementFailureSeverities.Major,
            AutomaticFailureFlag = false,
            OverrideAllowed = true,
            OverridePermission = "compliance.override.title49",
            RemediationRequired = true,
            ExternallyAssertable = false,
            IsRequired = true,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        });

        await db.SaveChangesAsync();
    }

    private async Task SeedCitationReviewAsync()
    {
        using var scope = _complianceCoreFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        var now = DateTimeOffset.UtcNow;

        var governingBodyId = Guid.NewGuid();
        var jurisdictionId = Guid.NewGuid();
        var programId = Guid.NewGuid();
        var reviewPackId = Guid.NewGuid();
        var reviewedCitationId = Guid.NewGuid();
        var reviewedCitationVersion2Id = Guid.NewGuid();
        var needsReviewCitationId = Guid.NewGuid();
        var supersededCitationId = Guid.NewGuid();
        var inactiveCitationId = Guid.NewGuid();
        var complianceKeyId = Guid.NewGuid();
        var factDefinitionId = Guid.NewGuid();

        db.GoverningBodies.Add(new GoverningBody
        {
            Id = governingBodyId,
            TenantId = PlatformSeeder.DemoTenantId,
            BodyKey = "dot",
            Label = "U.S. Department of Transportation",
            Description = "Federal transportation safety and compliance authority.",
            IsActive = true,
            CreatedAt = now.AddDays(-3),
            UpdatedAt = now.AddDays(-3),
        });

        db.Jurisdictions.Add(new Jurisdiction
        {
            Id = jurisdictionId,
            TenantId = PlatformSeeder.DemoTenantId,
            GoverningBodyId = governingBodyId,
            JurisdictionKey = "us_federal",
            Label = "United States Federal",
            Description = "Federal jurisdiction for citation review reporting.",
            IsActive = true,
            CreatedAt = now.AddDays(-3),
            UpdatedAt = now.AddDays(-3),
        });

        db.RegulatoryPrograms.Add(new RegulatoryProgram
        {
            Id = programId,
            TenantId = PlatformSeeder.DemoTenantId,
            JurisdictionId = jurisdictionId,
            ProgramKey = "citation_review_program",
            Label = "Citation Review Program",
            Description = "Program used to exercise citation review reporting.",
            IsActive = true,
            CreatedAt = now.AddDays(-3),
            UpdatedAt = now.AddDays(-1),
        });

        db.RulePacks.Add(new RulePack
        {
            Id = reviewPackId,
            TenantId = PlatformSeeder.DemoTenantId,
            RegulatoryProgramId = programId,
            PackKey = "citation_review_operational",
            Label = "Citation Review Operational Pack",
            Description = "Operational citation review coverage pack.",
            VersionNumber = 1,
            Status = RulePackStatuses.Published,
            IsActive = true,
            CreatedAt = now.AddDays(-2),
            UpdatedAt = now.AddDays(-1),
            RuleContentJson = """{"schemaVersion":1,"logic":"all","rules":[{"ruleKey":"citation_review_rule","label":"Citation review rule","type":"fact_boolean","factKey":"citation_review_fact","expectedValue":true}]}""",
        });

        db.RegulatoryCitations.AddRange(
            new RegulatoryCitation
            {
                Id = reviewedCitationId,
                TenantId = PlatformSeeder.DemoTenantId,
                RegulatoryProgramId = programId,
                RulePackId = reviewPackId,
                CitationKey = "cfr_172_200_title49",
                Label = "Citation review active citation",
                SourceReference = "49 CFR 172.200",
                Description = "Reviewed citation with rule-pack and downstream traceability.",
                VersionNumber = 1,
                IsActive = true,
                CreatedAt = now.AddHours(-4),
                UpdatedAt = now.AddHours(-2),
            },
            new RegulatoryCitation
            {
                Id = reviewedCitationVersion2Id,
                TenantId = PlatformSeeder.DemoTenantId,
                RegulatoryProgramId = programId,
                RulePackId = reviewPackId,
                CitationKey = "cfr_172_201_title49",
                Label = "Citation review active citation v2",
                SourceReference = "49 CFR 172.201",
                Description = "Latest reviewed citation version.",
                VersionNumber = 2,
                SupersedesCitationId = supersededCitationId,
                IsActive = true,
                CreatedAt = now.AddHours(-1),
                UpdatedAt = now,
            },
            new RegulatoryCitation
            {
                Id = needsReviewCitationId,
                TenantId = PlatformSeeder.DemoTenantId,
                RegulatoryProgramId = programId,
                CitationKey = "cfr_172_202_title49",
                Label = "Citation review needs review",
                SourceReference = "49 CFR 172.202",
                Description = "Active citation that still needs review assignment.",
                VersionNumber = 1,
                IsActive = true,
                CreatedAt = now.AddHours(-3),
                UpdatedAt = now.AddHours(-3),
            },
            new RegulatoryCitation
            {
                Id = supersededCitationId,
                TenantId = PlatformSeeder.DemoTenantId,
                RegulatoryProgramId = programId,
                CitationKey = "cfr_172_201_title49",
                Label = "Citation review superseded old version",
                SourceReference = "49 CFR 172.201",
                Description = "Older citation version kept for review history.",
                VersionNumber = 1,
                IsActive = false,
                CreatedAt = now.AddHours(-5),
                UpdatedAt = now.AddHours(-1),
            },
            new RegulatoryCitation
            {
                Id = inactiveCitationId,
                TenantId = PlatformSeeder.DemoTenantId,
                RegulatoryProgramId = programId,
                CitationKey = "cfr_172_204_title49",
                Label = "Citation review inactive citation",
                SourceReference = "49 CFR 172.204",
                Description = "Inactive citation preserved for historical review.",
                VersionNumber = 1,
                IsActive = false,
                CreatedAt = now.AddHours(-6),
                UpdatedAt = now.AddHours(-6),
            });

        db.ComplianceKeys.Add(new ComplianceKey
        {
            Id = complianceKeyId,
            TenantId = PlatformSeeder.DemoTenantId,
            Key = "citation_review_compliance_key",
            Label = "Citation review compliance key",
            Category = "citation_review",
            Description = "Compliance key used by the citation review report.",
            IsActive = true,
            CreatedAt = now.AddHours(-4),
            UpdatedAt = now.AddHours(-2),
        });

        db.FactDefinitions.Add(new FactDefinition
        {
            Id = factDefinitionId,
            TenantId = PlatformSeeder.DemoTenantId,
            FactKey = "citation_review_fact",
            Label = "Citation review fact",
            Description = "Fact definition used by the citation review report.",
            ValueType = FactValueTypes.Boolean,
            IsActive = true,
            CreatedAt = now.AddHours(-4),
            UpdatedAt = now.AddHours(-2),
        });

        db.FactRequirements.AddRange(
            new FactRequirement
            {
                Id = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                FactDefinitionId = factDefinitionId,
                RulePackId = reviewPackId,
                CitationId = reviewedCitationId,
                RequirementKey = "citation_review_requirement",
                Label = "Citation review requirement",
                Description = "Requirement used to demonstrate reviewed citation linkage.",
                ApplicabilityKey = "citation_review",
                SourceProduct = "ComplianceCore",
                SourceEntity = "regulatory_citations",
                SourceFieldOrRecordType = "citation_review",
                ValueType = FactValueTypes.Boolean,
                Operator = FactRequirementOperators.Equal,
                ExpectedValue = "true",
                EvidenceKind = FactRequirementEvidenceKinds.SystemFact,
                RequiredDocumentType = string.Empty,
                RetentionPeriod = string.Empty,
                AuditQuestion = "Is the citation review requirement satisfied?",
                FailureSeverity = FactRequirementFailureSeverities.Major,
                AutomaticFailureFlag = false,
                OverrideAllowed = true,
                OverridePermission = "compliance.override.citation_review",
                RemediationRequired = true,
                ExternallyAssertable = false,
                IsRequired = true,
                IsActive = true,
                CreatedAt = now.AddHours(-2),
                UpdatedAt = now.AddHours(-2),
            },
            new FactRequirement
            {
                Id = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                FactDefinitionId = factDefinitionId,
                RulePackId = reviewPackId,
                CitationId = reviewedCitationVersion2Id,
                RequirementKey = "citation_review_requirement_v2",
                Label = "Citation review requirement v2",
                Description = "Requirement used to demonstrate the latest citation version linkage.",
                ApplicabilityKey = "citation_review",
                SourceProduct = "ComplianceCore",
                SourceEntity = "regulatory_citations",
                SourceFieldOrRecordType = "citation_review",
                ValueType = FactValueTypes.Boolean,
                Operator = FactRequirementOperators.Equal,
                ExpectedValue = "true",
                EvidenceKind = FactRequirementEvidenceKinds.SystemFact,
                RequiredDocumentType = string.Empty,
                RetentionPeriod = string.Empty,
                AuditQuestion = "Is the citation review v2 requirement satisfied?",
                FailureSeverity = FactRequirementFailureSeverities.Major,
                AutomaticFailureFlag = false,
                OverrideAllowed = true,
                OverridePermission = "compliance.override.citation_review",
                RemediationRequired = true,
                ExternallyAssertable = false,
                IsRequired = true,
                IsActive = true,
                CreatedAt = now.AddHours(-1),
                UpdatedAt = now,
            });

        db.RegulatoryMappings.AddRange(
            new RegulatoryMapping
            {
                Id = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                MappingKey = "citation_review_mapping",
                Label = "Citation review mapping",
                Description = "Mapping used to demonstrate citation review traceability.",
                TargetKind = "citation",
                RegulatoryProgramId = programId,
                RulePackId = reviewPackId,
                CitationId = reviewedCitationId,
                ComplianceKeyId = complianceKeyId,
                IsActive = true,
                CreatedAt = now.AddHours(-2),
                UpdatedAt = now.AddHours(-2),
            },
            new RegulatoryMapping
            {
                Id = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                MappingKey = "citation_review_mapping_v2",
                Label = "Citation review mapping v2",
                Description = "Mapping used to demonstrate the latest citation review traceability.",
                TargetKind = "citation",
                RegulatoryProgramId = programId,
                RulePackId = reviewPackId,
                CitationId = reviewedCitationVersion2Id,
                ComplianceKeyId = complianceKeyId,
                IsActive = true,
                CreatedAt = now.AddHours(-1),
                UpdatedAt = now,
            });

        await db.SaveChangesAsync();
    }

    private async Task SeedRegulatoryDomainCoverageAsync()
    {
        using var scope = _complianceCoreFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        var now = DateTimeOffset.UtcNow;

        var governingBodyId = Guid.NewGuid();
        var jurisdictionId = Guid.NewGuid();
        var programId = Guid.NewGuid();
        var operationalPackId = Guid.NewGuid();
        var referencePackId = Guid.NewGuid();
        var citationId = Guid.NewGuid();
        var complianceKeyId = Guid.NewGuid();
        var flammableLiquidKeyId = Guid.NewGuid();
        var inertKeyId = Guid.NewGuid();

        db.GoverningBodies.Add(new GoverningBody
        {
            Id = governingBodyId,
            TenantId = PlatformSeeder.DemoTenantId,
            BodyKey = "dot",
            Label = "U.S. Department of Transportation",
            Description = "Federal transportation safety and compliance authority.",
            IsActive = true,
            CreatedAt = now.AddDays(-2),
            UpdatedAt = now.AddDays(-2),
        });

        db.Jurisdictions.Add(new Jurisdiction
        {
            Id = jurisdictionId,
            TenantId = PlatformSeeder.DemoTenantId,
            GoverningBodyId = governingBodyId,
            JurisdictionKey = "us_federal",
            Label = "United States Federal",
            Description = "Federal jurisdiction for interstate transportation rules.",
            IsActive = true,
            CreatedAt = now.AddDays(-2),
            UpdatedAt = now.AddDays(-2),
        });

        db.RegulatoryPrograms.Add(new RegulatoryProgram
        {
            Id = programId,
            TenantId = PlatformSeeder.DemoTenantId,
            JurisdictionId = jurisdictionId,
            ProgramKey = "phmsa_hmr",
            Label = "PHMSA Hazardous Materials Regulations",
            Description = "Hazmat rule coverage used by the regulatory-domain report.",
            IsActive = true,
            CreatedAt = now.AddDays(-2),
            UpdatedAt = now.AddDays(-1),
        });

        db.RulePacks.AddRange(
            new RulePack
            {
                Id = operationalPackId,
                TenantId = PlatformSeeder.DemoTenantId,
                RegulatoryProgramId = programId,
                PackKey = "phmsa_hmr_operational",
                Label = "PHMSA HMR Operational Pack",
                Description = "Operational hazmat coverage for report testing.",
                VersionNumber = 1,
                Status = RulePackStatuses.Published,
                IsActive = true,
                CreatedAt = now.AddDays(-1),
                UpdatedAt = now,
                RuleContentJson = JsonSerializer.Serialize(
                    new RulePackContentBody(
                        1,
                        "all",
                        [new RuleDefinitionDto("license_valid", "Valid driver license", "fact_boolean", "driver_license_valid", true)]),
                    new JsonSerializerOptions(JsonSerializerDefaults.Web)),
            },
            new RulePack
            {
                Id = referencePackId,
                TenantId = PlatformSeeder.DemoTenantId,
                RegulatoryProgramId = programId,
                PackKey = "phmsa_hmr_reference",
                Label = "PHMSA HMR Reference Pack",
                Description = "Reference-only coverage for report testing.",
                VersionNumber = 1,
                Status = RulePackStatuses.Published,
                IsActive = true,
                CreatedAt = now.AddDays(-1),
                UpdatedAt = now.AddDays(-1),
            });

        db.RegulatoryCitations.Add(new RegulatoryCitation
        {
            Id = citationId,
            TenantId = PlatformSeeder.DemoTenantId,
            RegulatoryProgramId = programId,
            RulePackId = operationalPackId,
            CitationKey = "cfr_172_101",
            Label = "Hazardous materials table",
            SourceReference = "49 CFR 172.101",
            Description = "Lookup-verification coverage for the hazardous materials table.",
            VersionNumber = 1,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.ComplianceKeys.Add(new ComplianceKey
        {
            Id = complianceKeyId,
            TenantId = PlatformSeeder.DemoTenantId,
            Key = "hazmat_lookup",
            Label = "Hazmat lookup",
            Category = "hazmat_domain",
            Description = "Lookup key for hazmat table verification.",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.MaterialKeys.AddRange(
            new MaterialKey
            {
                Id = flammableLiquidKeyId,
                TenantId = PlatformSeeder.DemoTenantId,
                Key = "flammable_liquid",
                Label = "Flammable liquid",
                Category = "hazmat",
                Description = "Material key used to enumerate hazmat table coverage.",
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
            },
            new MaterialKey
            {
                Id = inertKeyId,
                TenantId = PlatformSeeder.DemoTenantId,
                Key = "non_regulated_inert",
                Label = "Non-regulated inert",
                Category = "hazmat",
                Description = "Unmapped material key used for coverage gap reporting.",
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
            });

        db.RegulatoryMappings.Add(new RegulatoryMapping
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            MappingKey = "phmsa_hmr_lookup",
            Label = "PHMSA HMR lookup mapping",
            Description = "Maps hazmat lookup coverage into the PHMSA regulatory program.",
            TargetKind = "compliance_key",
            RegulatoryProgramId = programId,
            RulePackId = operationalPackId,
            CitationId = citationId,
            ComplianceKeyId = complianceKeyId,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.RegulatoryMappings.Add(new RegulatoryMapping
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            MappingKey = "phmsa_hmr_flammable_liquid",
            Label = "Flammable liquid hazmat mapping",
            Description = "Maps a hazmat material key into the PHMSA lookup table coverage.",
            TargetKind = "material_key",
            RegulatoryProgramId = programId,
            RulePackId = operationalPackId,
            CitationId = citationId,
            MaterialKeyId = flammableLiquidKeyId,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        });

        await db.SaveChangesAsync();
    }

    private async Task<Guid> CreateImpactProgramAsync(string adminToken)
    {
        var bodyRequest = Authorized(HttpMethod.Post, "/api/governing-bodies", adminToken);
        bodyRequest.Content = JsonContent.Create(new CreateGoverningBodyRequest(
            "impact_body",
            "Impact Body",
            "Rule change impact test body."));
        var bodyResponse = await _complianceCoreClient.SendAsync(bodyRequest);
        bodyResponse.EnsureSuccessStatusCode();
        var body = (await bodyResponse.Content.ReadFromJsonAsync<GoverningBodyResponse>())!;

        var jurisdictionRequest = Authorized(HttpMethod.Post, "/api/jurisdictions", adminToken);
        jurisdictionRequest.Content = JsonContent.Create(new CreateJurisdictionRequest(
            body.GoverningBodyId,
            "impact_jurisdiction",
            "Impact Jurisdiction",
            "Rule change impact test jurisdiction."));
        var jurisdictionResponse = await _complianceCoreClient.SendAsync(jurisdictionRequest);
        jurisdictionResponse.EnsureSuccessStatusCode();
        var jurisdiction = (await jurisdictionResponse.Content.ReadFromJsonAsync<JurisdictionResponse>())!;

        var programRequest = Authorized(HttpMethod.Post, "/api/regulatory-programs", adminToken);
        programRequest.Content = JsonContent.Create(new CreateRegulatoryProgramRequest(
            jurisdiction.JurisdictionId,
            "impact_program",
            "Impact Program",
            "Rule change impact test program."));
        var programResponse = await _complianceCoreClient.SendAsync(programRequest);
        programResponse.EnsureSuccessStatusCode();
        return (await programResponse.Content.ReadFromJsonAsync<RegulatoryProgramResponse>())!.RegulatoryProgramId;
    }

    private async Task<Guid> CreateImpactRulePackAsync(string adminToken, Guid programId, string packKey)
    {
        var request = Authorized(HttpMethod.Post, "/api/rule-packs", adminToken);
        request.Content = JsonContent.Create(new CreateRulePackRequest(
            programId,
            packKey,
            $"Label {packKey}",
            "Rule change impact test pack."));
        var response = await _complianceCoreClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RulePackResponse>())!.RulePackId;
    }

    private async Task<Guid> CreateReadinessFactAsync(string adminToken, string factKey)
    {
        var request = Authorized(HttpMethod.Post, "/api/fact-definitions", adminToken);
        request.Content = JsonContent.Create(new CreateFactDefinitionRequest(
            factKey,
            $"Label {factKey}",
            "Readiness report test fact.",
            "boolean"));
        var response = await _complianceCoreClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<FactDefinitionResponse>())!.FactDefinitionId;
    }

    private async Task<Guid> CreateReadinessPackAsync(string adminToken, Guid programId, string packKey)
    {
        var request = Authorized(HttpMethod.Post, "/api/rule-packs", adminToken);
        request.Content = JsonContent.Create(new CreateRulePackRequest(
            programId,
            packKey,
            $"Label {packKey}",
            "Readiness report test pack."));
        var response = await _complianceCoreClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RulePackResponse>())!.RulePackId;
    }

    private async Task SetReadinessPackContentAsync(string adminToken, Guid packId, string factKey)
    {
        var content = new RulePackContentBody(
            1,
            "all",
            [new RuleDefinitionDto("license_valid", "Valid license", "fact_boolean", factKey, true)]);

        var request = Authorized(HttpMethod.Put, $"/api/rule-packs/{packId}/content", adminToken);
        request.Content = JsonContent.Create(new UpdateRulePackContentRequest(content));
        (await _complianceCoreClient.SendAsync(request)).EnsureSuccessStatusCode();
    }

    private async Task CreateReadinessStaticFactSourceAsync(string adminToken, Guid factId, string sourceKey)
    {
        var request = Authorized(HttpMethod.Post, "/api/fact-sources", adminToken);
        request.Content = JsonContent.Create(new CreateFactSourceRequest(
            factId,
            sourceKey,
            FactSourceTypes.StaticConfig,
            "Readiness report source",
            "Static true for readiness report coverage.",
            null,
            null,
            """{"booleanValue":true}""",
            0));
        (await _complianceCoreClient.SendAsync(request)).EnsureSuccessStatusCode();
    }

    private async Task PublishReadinessPackAsync(string adminToken, Guid packId)
    {
        var reviewRequest = Authorized(HttpMethod.Patch, $"/api/rule-packs/{packId}/status", adminToken);
        reviewRequest.Content = JsonContent.Create(new UpdateRulePackStatusRequest(RulePackStatuses.Review));
        (await _complianceCoreClient.SendAsync(reviewRequest)).EnsureSuccessStatusCode();

        var publishRequest = Authorized(HttpMethod.Patch, $"/api/rule-packs/{packId}/status", adminToken);
        publishRequest.Content = JsonContent.Create(new UpdateRulePackStatusRequest(RulePackStatuses.Published));
        (await _complianceCoreClient.SendAsync(publishRequest)).EnsureSuccessStatusCode();
    }

    private async Task<Guid> CreateRegulatoryProgramAsync(string adminToken)
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

    private async Task<Guid> GetSeedRulePackIdAsync()
    {
        var response = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/rule-packs", _adminToken));
        response.EnsureSuccessStatusCode();

        var packs = await response.Content.ReadFromJsonAsync<IReadOnlyList<RulePackResponse>>();
        return packs?.FirstOrDefault(x => x.PackKey == "phmsa_hmr_operational")?.RulePackId
            ?? packs?.FirstOrDefault(x => x.PackKey == "title49_hmr_operational")?.RulePackId
            ?? packs?.FirstOrDefault()?.RulePackId
            ?? throw new InvalidOperationException("No rule packs seeded.");
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
