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
    public async Task V1_reports_index_summaries_and_export_are_available()
    {
        var indexResponse = await _complianceCoreClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports", _adminToken));
        indexResponse.EnsureSuccessStatusCode();
        var indexJson = await indexResponse.Content.ReadAsStringAsync();
        Assert.Contains("/api/v1/reports/findings", indexJson, StringComparison.Ordinal);
        Assert.Contains("/api/v1/reports/operator", indexJson, StringComparison.Ordinal);
        Assert.Contains("/api/v1/reports/evidence/missing", indexJson, StringComparison.Ordinal);
        Assert.Contains("/api/v1/reports/waivers", indexJson, StringComparison.Ordinal);
        Assert.Contains("/api/v1/reports/exception-exemptions", indexJson, StringComparison.Ordinal);
        Assert.Contains("/api/v1/reports/integration-health", indexJson, StringComparison.Ordinal);
        Assert.Contains("/api/v1/reports/audit-readiness", indexJson, StringComparison.Ordinal);
        Assert.Contains("/api/v1/reports/remediation-queue", indexJson, StringComparison.Ordinal);

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
        return packs?.FirstOrDefault()?.RulePackId ?? throw new InvalidOperationException("No rule packs seeded.");
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
