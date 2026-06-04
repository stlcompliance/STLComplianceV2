using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Endpoints;

public static class ComplianceCoreReportEndpoints
{
    public static void MapComplianceCoreReportIndexEndpoints(this WebApplication app)
    {
        var routes = new[]
        {
            (Route: "/api/reports", Suffix: string.Empty),
            (Route: "/api/v1/reports", Suffix: "V1")
        };

        foreach (var (route, suffix) in routes)
        {
            var group = app.MapGroup(route)
                .WithTags("Reports")
                .RequireAuthorization();

            group.MapGet("/", (
                ComplianceCoreAuthorizationService authorization,
                HttpContext context) =>
            {
                authorization.RequireFindingsReportRead(context.User);
                return Results.Ok(new
                {
                    Reports = new[]
                    {
                        new { Key = "findings", Path = "/api/v1/reports/findings" },
                    new { Key = "operator", Path = "/api/v1/reports/operator" },
                    new { Key = "missing_evidence", Path = "/api/v1/reports/evidence/missing" },
                    new { Key = "evidence_completeness", Path = "/api/v1/reports/evidence/completeness" },
                    new { Key = "waivers", Path = "/api/v1/reports/waivers" },
                    new { Key = "exception_exemptions", Path = "/api/v1/reports/exception-exemptions" },
                    new { Key = "integration_health", Path = "/api/v1/reports/integration-health" },
                    new { Key = "audit_readiness", Path = "/api/v1/reports/audit-readiness" },
                    new { Key = "remediation_queue", Path = "/api/v1/reports/remediation-queue" },
                    new { Key = "regulatory_domain_coverage", Path = "/api/v1/reports/regulatory-domains" },
                    new { Key = "hazmat_table_coverage", Path = "/api/v1/reports/hazmat-table" },
                    new { Key = "title49_coverage_explorer", Path = "/api/v1/reports/title49" },
                    new { Key = "title49_citation_coverage", Path = "/api/v1/reports/title49/citations" },
                    new { Key = "citation_review", Path = "/api/v1/reports/citation-review" },
                    new { Key = "rule_change_impact", Path = "/api/v1/reports/rule-changes/impact" },
                    new { Key = "evaluation_history_explorer", Path = "/api/v1/reports/evaluations" }
                }
            });
        })
        .WithName($"ListComplianceCoreReportGroups{suffix}");
        }
    }

    public static void MapComplianceCoreFindingsReportEndpoints(this WebApplication app)
    {
        MapGroup(app, "/api/reports/findings", string.Empty);
        MapGroup(app, "/api/v1/reports/findings", "V1");
    }

    private static void MapGroup(WebApplication app, string routePrefix, string routeNameSuffix)
    {
        var group = app.MapGroup(routePrefix)
            .WithTags("FindingsReports")
            .RequireAuthorization();

        group.MapGet("/summary", async (
            string? status,
            string? severity,
            bool? openOnly,
            ComplianceCoreAuthorizationService authorization,
            FindingsReportService reportService,
            IComplianceCoreAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireFindingsReportRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var summary = await reportService.GetSummaryAsync(
                tenantId,
                status,
                severity,
                openOnly ?? false,
                cancellationToken);
            await audit.WriteAsync(
                "compliancecore.reports.findings.summary",
                tenantId,
                actorUserId,
                "findings_report",
                null,
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(summary);
        })
        .WithName($"GetComplianceCoreFindingsReportSummary{routeNameSuffix}");

        group.MapGet("/summary/export", async (
            string? status,
            string? severity,
            bool? openOnly,
            ComplianceCoreAuthorizationService authorization,
            FindingsReportService reportService,
            IComplianceCoreAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireFindingsReportExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await reportService.ExportSummaryCsvAsync(
                tenantId,
                status,
                severity,
                openOnly ?? false,
                cancellationToken);
            await audit.WriteAsync(
                "compliancecore.reports.findings.export",
                tenantId,
                actorUserId,
                "findings_report",
                "summary",
                "success",
                cancellationToken: cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName($"ExportComplianceCoreFindingsReportSummary{routeNameSuffix}");
    }

    public static void MapComplianceCoreOperatorReportEndpoints(this WebApplication app)
    {
        MapOperatorGroup(app, "/api/reports/operator", string.Empty);
        MapOperatorGroup(app, "/api/v1/reports/operator", "V1");
    }

    public static void MapComplianceCoreMissingEvidenceReportEndpoints(this WebApplication app)
    {
        MapMissingEvidenceGroup(app, "/api/reports/evidence/missing", string.Empty);
        MapMissingEvidenceGroup(app, "/api/v1/reports/evidence/missing", "V1");
    }

    public static void MapComplianceCoreEvidenceCompletenessReportEndpoints(this WebApplication app)
    {
        MapEvidenceCompletenessGroup(app, "/api/reports/evidence/completeness", string.Empty);
        MapEvidenceCompletenessGroup(app, "/api/v1/reports/evidence/completeness", "V1");
    }

    public static void MapComplianceCoreWaiverReportEndpoints(this WebApplication app)
    {
        MapWaiverGroup(app, "/api/reports/waivers", string.Empty);
        MapWaiverGroup(app, "/api/v1/reports/waivers", "V1");
    }

    public static void MapComplianceCoreExceptionExemptionReportEndpoints(this WebApplication app)
    {
        MapExceptionExemptionGroup(app, "/api/reports/exception-exemptions", string.Empty);
        MapExceptionExemptionGroup(app, "/api/v1/reports/exception-exemptions", "V1");
    }

    public static void MapComplianceCoreProductIntegrationHealthReportEndpoints(this WebApplication app)
    {
        MapProductIntegrationHealthGroup(app, "/api/reports/integration-health", string.Empty);
        MapProductIntegrationHealthGroup(app, "/api/v1/reports/integration-health", "V1");
    }

    public static void MapComplianceCoreAuditReadinessReportEndpoints(this WebApplication app)
    {
        MapAuditReadinessGroup(app, "/api/reports/audit-readiness", string.Empty);
        MapAuditReadinessGroup(app, "/api/v1/reports/audit-readiness", "V1");
    }

    public static void MapComplianceCoreRemediationQueueReportEndpoints(this WebApplication app)
    {
        MapRemediationQueueGroup(app, "/api/reports/remediation-queue", string.Empty);
        MapRemediationQueueGroup(app, "/api/v1/reports/remediation-queue", "V1");
    }

    public static void MapComplianceCoreRegulatoryDomainCoverageReportEndpoints(this WebApplication app)
    {
        MapRegulatoryDomainCoverageGroup(app, "/api/reports/regulatory-domains", string.Empty);
        MapRegulatoryDomainCoverageGroup(app, "/api/v1/reports/regulatory-domains", "V1");
    }

    public static void MapComplianceCoreHazmatTableCoverageReportEndpoints(this WebApplication app)
    {
        MapHazmatTableCoverageGroup(app, "/api/reports/hazmat-table", string.Empty);
        MapHazmatTableCoverageGroup(app, "/api/v1/reports/hazmat-table", "V1");
    }

    public static void MapComplianceCoreTitle49CoverageExplorerEndpoints(this WebApplication app)
    {
        MapTitle49CoverageExplorerGroup(app, "/api/reports/title49", string.Empty);
        MapTitle49CoverageExplorerGroup(app, "/api/v1/reports/title49", "V1");
    }

    public static void MapComplianceCoreTitle49CitationCoverageReportEndpoints(this WebApplication app)
    {
        MapTitle49CitationCoverageGroup(app, "/api/reports/title49/citations", string.Empty);
        MapTitle49CitationCoverageGroup(app, "/api/v1/reports/title49/citations", "V1");
    }

    public static void MapComplianceCoreCitationReviewReportEndpoints(this WebApplication app)
    {
        MapCitationReviewGroup(app, "/api/reports/citation-review", string.Empty);
        MapCitationReviewGroup(app, "/api/v1/reports/citation-review", "V1");
    }

    public static void MapComplianceCoreRuleChangeImpactReportEndpoints(this WebApplication app)
    {
        MapRuleChangeImpactGroup(app, "/api/reports/rule-changes/impact", string.Empty);
        MapRuleChangeImpactGroup(app, "/api/v1/reports/rule-changes/impact", "V1");
    }

    public static void MapComplianceCoreEvaluationHistoryExplorerEndpoints(this WebApplication app)
    {
        MapEvaluationHistoryGroup(app, "/api/reports/evaluations", string.Empty);
        MapEvaluationHistoryGroup(app, "/api/v1/reports/evaluations", "V1");
    }

    private static void MapMissingEvidenceGroup(WebApplication app, string routePrefix, string routeNameSuffix)
    {
        var group = app.MapGroup(routePrefix)
            .WithTags("MissingEvidenceReports")
            .RequireAuthorization();

        group.MapGet("/summary", async (
            string? severity,
            string? reasonCode,
            ComplianceCoreAuthorizationService authorization,
            MissingEvidenceReportService reportService,
            IComplianceCoreAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireMissingEvidenceWarningRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var summary = await reportService.GetSummaryAsync(
                tenantId,
                severity,
                reasonCode,
                cancellationToken);
            await audit.WriteAsync(
                "compliancecore.reports.missing_evidence.summary",
                tenantId,
                actorUserId,
                "missing_evidence_report",
                null,
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(summary);
        })
        .WithName($"GetComplianceCoreMissingEvidenceReportSummary{routeNameSuffix}");

        group.MapGet("/summary/export", async (
            string? severity,
            string? reasonCode,
            ComplianceCoreAuthorizationService authorization,
            MissingEvidenceReportService reportService,
            IComplianceCoreAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireFindingsReportExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await reportService.ExportSummaryCsvAsync(
                tenantId,
                severity,
                reasonCode,
                cancellationToken);
            await audit.WriteAsync(
                "compliancecore.reports.missing_evidence.export",
                tenantId,
                actorUserId,
                "missing_evidence_report",
                "summary",
                "success",
                cancellationToken: cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName($"ExportComplianceCoreMissingEvidenceReportSummary{routeNameSuffix}");
    }

    private static void MapEvidenceCompletenessGroup(WebApplication app, string routePrefix, string routeNameSuffix)
    {
        var group = app.MapGroup(routePrefix)
            .WithTags("EvidenceCompletenessReports")
            .RequireAuthorization();

        group.MapGet("/summary", async (
            string? scopeKey,
            string? rulePackKey,
            string? severity,
            int? limit,
            ComplianceCoreAuthorizationService authorization,
            EvidenceCompletenessReportService reportService,
            IComplianceCoreAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireMissingEvidenceWarningRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var summary = await reportService.GetSummaryAsync(
                tenantId,
                scopeKey,
                rulePackKey,
                severity,
                limit,
                cancellationToken);
            await audit.WriteAsync(
                "compliancecore.reports.evidence_completeness.summary",
                tenantId,
                actorUserId,
                "evidence_completeness_report",
                null,
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(summary);
        })
        .WithName($"GetComplianceCoreEvidenceCompletenessReportSummary{routeNameSuffix}");

        group.MapGet("/summary/export", async (
            string? scopeKey,
            string? rulePackKey,
            string? severity,
            ComplianceCoreAuthorizationService authorization,
            EvidenceCompletenessReportService reportService,
            IComplianceCoreAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireFindingsReportExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await reportService.ExportSummaryCsvAsync(
                tenantId,
                scopeKey,
                rulePackKey,
                severity,
                cancellationToken);
            await audit.WriteAsync(
                "compliancecore.reports.evidence_completeness.export",
                tenantId,
                actorUserId,
                "evidence_completeness_report",
                "summary",
                "success",
                cancellationToken: cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName($"ExportComplianceCoreEvidenceCompletenessReportSummary{routeNameSuffix}");
    }

    private static void MapOperatorGroup(WebApplication app, string routePrefix, string routeNameSuffix)
    {
        var group = app.MapGroup(routePrefix)
            .WithTags("OperatorReports")
            .RequireAuthorization();

        group.MapGet("/summary", async (
            bool? attentionOnly,
            ComplianceCoreAuthorizationService authorization,
            OperatorReportService reportService,
            IComplianceCoreAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireOperatorReportRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var summary = await reportService.GetSummaryAsync(
                tenantId,
                attentionOnly ?? false,
                cancellationToken);
            await audit.WriteAsync(
                "compliancecore.reports.operator.summary",
                tenantId,
                actorUserId,
                "operator_report",
                null,
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(summary);
        })
        .WithName($"GetComplianceCoreOperatorReportSummary{routeNameSuffix}");

        group.MapGet("/summary/export", async (
            bool? attentionOnly,
            ComplianceCoreAuthorizationService authorization,
            OperatorReportService reportService,
            IComplianceCoreAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireOperatorReportExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await reportService.ExportSummaryCsvAsync(
                tenantId,
                attentionOnly ?? false,
                cancellationToken);
            await audit.WriteAsync(
                "compliancecore.reports.operator.export",
                tenantId,
                actorUserId,
                "operator_report",
                "summary",
                "success",
                cancellationToken: cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName($"ExportComplianceCoreOperatorReportSummary{routeNameSuffix}");

        group.MapGet("/alerts", async (
            ComplianceCoreAuthorizationService authorization,
            OperatorReportService reportService,
            IComplianceCoreAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireOperatorReportRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var alerts = await reportService.ListAlertsAsync(tenantId, cancellationToken);
            await audit.WriteAsync(
                "compliancecore.reports.operator.alerts",
                tenantId,
                actorUserId,
                "operator_report",
                null,
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(alerts);
        })
        .WithName($"GetComplianceCoreOperatorReportAlerts{routeNameSuffix}");
    }

    private static void MapWaiverGroup(WebApplication app, string routePrefix, string routeNameSuffix)
    {
        var group = app.MapGroup(routePrefix)
            .WithTags("WaiverReports")
            .RequireAuthorization();

        group.MapGet("/summary", async (
            string? status,
            string? packKey,
            string? scopeKey,
            ComplianceCoreAuthorizationService authorization,
            WaiverReportService reportService,
            IComplianceCoreAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWaiverRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var summary = await reportService.GetSummaryAsync(
                tenantId,
                status,
                packKey,
                scopeKey,
                cancellationToken);
            await audit.WriteAsync(
                "compliancecore.reports.waivers.summary",
                tenantId,
                actorUserId,
                "waiver_report",
                null,
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(summary);
        })
        .WithName($"GetComplianceCoreWaiverReportSummary{routeNameSuffix}");

        group.MapGet("/summary/export", async (
            string? status,
            string? packKey,
            string? scopeKey,
            ComplianceCoreAuthorizationService authorization,
            WaiverReportService reportService,
            IComplianceCoreAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireFindingsReportExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await reportService.ExportSummaryCsvAsync(
                tenantId,
                status,
                packKey,
                scopeKey,
                cancellationToken);
            await audit.WriteAsync(
                "compliancecore.reports.waivers.export",
                tenantId,
                actorUserId,
                "waiver_report",
                "summary",
                "success",
                cancellationToken: cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName($"ExportComplianceCoreWaiverReportSummary{routeNameSuffix}");
    }

    private static void MapExceptionExemptionGroup(WebApplication app, string routePrefix, string routeNameSuffix)
    {
        var group = app.MapGroup(routePrefix)
            .WithTags("ExceptionExemptionReports")
            .RequireAuthorization();

        group.MapGet("/summary", async (
            string? type,
            string? effectType,
            bool? activeOnly,
            ComplianceCoreAuthorizationService authorization,
            ExceptionExemptionReportService reportService,
            IComplianceCoreAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var summary = await reportService.GetSummaryAsync(
                tenantId,
                type,
                effectType,
                activeOnly,
                cancellationToken);
            await audit.WriteAsync(
                "compliancecore.reports.exception_exemptions.summary",
                tenantId,
                actorUserId,
                "exception_exemption_report",
                null,
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(summary);
        })
        .WithName($"GetComplianceCoreExceptionExemptionReportSummary{routeNameSuffix}");

        group.MapGet("/summary/export", async (
            string? type,
            string? effectType,
            bool? activeOnly,
            ComplianceCoreAuthorizationService authorization,
            ExceptionExemptionReportService reportService,
            IComplianceCoreAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireFindingsReportExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await reportService.ExportSummaryCsvAsync(
                tenantId,
                type,
                effectType,
                activeOnly,
                cancellationToken);
            await audit.WriteAsync(
                "compliancecore.reports.exception_exemptions.export",
                tenantId,
                actorUserId,
                "exception_exemption_report",
                "summary",
                "success",
                cancellationToken: cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName($"ExportComplianceCoreExceptionExemptionReportSummary{routeNameSuffix}");
    }

    private static void MapProductIntegrationHealthGroup(WebApplication app, string routePrefix, string routeNameSuffix)
    {
        var group = app.MapGroup(routePrefix)
            .WithTags("IntegrationHealthReports")
            .RequireAuthorization();

        group.MapGet("/summary", async (
            ComplianceCoreAuthorizationService authorization,
            ProductIntegrationHealthReportService reportService,
            IComplianceCoreAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireFactSourceSyncHealthRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var summary = await reportService.GetSummaryAsync(tenantId, cancellationToken);
            await audit.WriteAsync(
                "compliancecore.reports.integration_health.summary",
                tenantId,
                actorUserId,
                "integration_health_report",
                null,
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(summary);
        })
        .WithName($"GetComplianceCoreProductIntegrationHealthReportSummary{routeNameSuffix}");

        group.MapGet("/summary/export", async (
            ComplianceCoreAuthorizationService authorization,
            ProductIntegrationHealthReportService reportService,
            IComplianceCoreAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireFindingsReportExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await reportService.ExportSummaryCsvAsync(tenantId, cancellationToken);
            await audit.WriteAsync(
                "compliancecore.reports.integration_health.export",
                tenantId,
                actorUserId,
                "integration_health_report",
                "summary",
                "success",
                cancellationToken: cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName($"ExportComplianceCoreProductIntegrationHealthReportSummary{routeNameSuffix}");
    }

    private static void MapAuditReadinessGroup(WebApplication app, string routePrefix, string routeNameSuffix)
    {
        var group = app.MapGroup(routePrefix)
            .WithTags("AuditReadinessReports")
            .RequireAuthorization();

        group.MapGet("/summary", async (
            string? scopeKey,
            string? rulePackKey,
            string? readinessLevel,
            int? limit,
            ComplianceCoreAuthorizationService authorization,
            AuditReadinessReportService reportService,
            IComplianceCoreAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireFindingsReportRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var summary = await reportService.GetSummaryAsync(
                tenantId,
                scopeKey,
                rulePackKey,
                readinessLevel,
                limit,
                cancellationToken);
            await audit.WriteAsync(
                "compliancecore.reports.audit_readiness.summary",
                tenantId,
                actorUserId,
                "audit_readiness_report",
                null,
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(summary);
        })
        .WithName($"GetComplianceCoreAuditReadinessReportSummary{routeNameSuffix}");

        group.MapGet("/summary/export", async (
            string? scopeKey,
            string? rulePackKey,
            string? readinessLevel,
            ComplianceCoreAuthorizationService authorization,
            AuditReadinessReportService reportService,
            IComplianceCoreAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireFindingsReportExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await reportService.ExportSummaryCsvAsync(
                tenantId,
                scopeKey,
                rulePackKey,
                readinessLevel,
                cancellationToken);
            await audit.WriteAsync(
                "compliancecore.reports.audit_readiness.export",
                tenantId,
                actorUserId,
                "audit_readiness_report",
                "summary",
                "success",
                cancellationToken: cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName($"ExportComplianceCoreAuditReadinessReportSummary{routeNameSuffix}");
    }

    private static void MapRemediationQueueGroup(WebApplication app, string routePrefix, string routeNameSuffix)
    {
        var group = app.MapGroup(routePrefix)
            .WithTags("RemediationQueueReports")
            .RequireAuthorization();

        group.MapGet("/summary", async (
            bool? queueOnly,
            string? scopeKey,
            string? rulePackKey,
            string? severity,
            int? limit,
            ComplianceCoreAuthorizationService authorization,
            RemediationQueueReportService reportService,
            IComplianceCoreAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireFindingsReportRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var summary = await reportService.GetSummaryAsync(
                tenantId,
                queueOnly ?? true,
                scopeKey,
                rulePackKey,
                severity,
                limit,
                cancellationToken);
            await audit.WriteAsync(
                "compliancecore.reports.remediation_queue.summary",
                tenantId,
                actorUserId,
                "remediation_queue_report",
                null,
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(summary);
        })
        .WithName($"GetComplianceCoreRemediationQueueReportSummary{routeNameSuffix}");

        group.MapGet("/summary/export", async (
            bool? queueOnly,
            string? scopeKey,
            string? rulePackKey,
            string? severity,
            ComplianceCoreAuthorizationService authorization,
            RemediationQueueReportService reportService,
            IComplianceCoreAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireFindingsReportExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await reportService.ExportSummaryCsvAsync(
                tenantId,
                queueOnly ?? true,
                scopeKey,
                rulePackKey,
                severity,
                cancellationToken);
            await audit.WriteAsync(
                "compliancecore.reports.remediation_queue.export",
                tenantId,
                actorUserId,
                "remediation_queue_report",
                "summary",
                "success",
                cancellationToken: cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName($"ExportComplianceCoreRemediationQueueReportSummary{routeNameSuffix}");
    }

    private static void MapRegulatoryDomainCoverageGroup(WebApplication app, string routePrefix, string routeNameSuffix)
    {
        var group = app.MapGroup(routePrefix)
            .WithTags("RegulatoryDomainCoverageReports")
            .RequireAuthorization();

        group.MapGet("/summary", async (
            ComplianceCoreAuthorizationService authorization,
            RegulatoryDomainCoverageReportService reportService,
            IComplianceCoreAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var summary = await reportService.GetSummaryAsync(tenantId, cancellationToken);
            await audit.WriteAsync(
                "compliancecore.reports.regulatory_domain_coverage.summary",
                tenantId,
                actorUserId,
                "regulatory_domain_coverage_report",
                null,
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(summary);
        })
        .WithName($"GetComplianceCoreRegulatoryDomainCoverageReportSummary{routeNameSuffix}");

        group.MapGet("/summary/export", async (
            ComplianceCoreAuthorizationService authorization,
            RegulatoryDomainCoverageReportService reportService,
            IComplianceCoreAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await reportService.ExportSummaryCsvAsync(tenantId, cancellationToken);
            await audit.WriteAsync(
                "compliancecore.reports.regulatory_domain_coverage.export",
                tenantId,
                actorUserId,
                "regulatory_domain_coverage_report",
                "summary",
                "success",
                cancellationToken: cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName($"ExportComplianceCoreRegulatoryDomainCoverageReportSummary{routeNameSuffix}");
    }

    private static void MapHazmatTableCoverageGroup(WebApplication app, string routePrefix, string routeNameSuffix)
    {
        var group = app.MapGroup(routePrefix)
            .WithTags("HazmatTableCoverageReports")
            .RequireAuthorization();

        group.MapGet("/summary", async (
            ComplianceCoreAuthorizationService authorization,
            HazmatTableCoverageReportService reportService,
            IComplianceCoreAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var summary = await reportService.GetSummaryAsync(tenantId, cancellationToken);
            await audit.WriteAsync(
                "compliancecore.reports.hazmat_table_coverage.summary",
                tenantId,
                actorUserId,
                "hazmat_table_coverage_report",
                null,
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(summary);
        })
        .WithName($"GetComplianceCoreHazmatTableCoverageReportSummary{routeNameSuffix}");

        group.MapGet("/summary/export", async (
            ComplianceCoreAuthorizationService authorization,
            HazmatTableCoverageReportService reportService,
            IComplianceCoreAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await reportService.ExportSummaryCsvAsync(tenantId, cancellationToken);
            await audit.WriteAsync(
                "compliancecore.reports.hazmat_table_coverage.export",
                tenantId,
                actorUserId,
                "hazmat_table_coverage_report",
                "summary",
                "success",
                cancellationToken: cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName($"ExportComplianceCoreHazmatTableCoverageReportSummary{routeNameSuffix}");
    }

    private static void MapTitle49CoverageExplorerGroup(WebApplication app, string routePrefix, string routeNameSuffix)
    {
        var group = app.MapGroup(routePrefix)
            .WithTags("Title49CoverageReports")
            .RequireAuthorization();

        group.MapGet("/summary", async (
            ComplianceCoreAuthorizationService authorization,
            Title49CoverageExplorerService reportService,
            IComplianceCoreAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var summary = await reportService.GetSummaryAsync(tenantId, cancellationToken);
            await audit.WriteAsync(
                "compliancecore.reports.title49_coverage.summary",
                tenantId,
                actorUserId,
                "title49_coverage_report",
                null,
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(summary);
        })
        .WithName($"GetComplianceCoreTitle49CoverageExplorerSummary{routeNameSuffix}");

        group.MapGet("/summary/export", async (
            ComplianceCoreAuthorizationService authorization,
            Title49CoverageExplorerService reportService,
            IComplianceCoreAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await reportService.ExportSummaryCsvAsync(tenantId, cancellationToken);
            await audit.WriteAsync(
                "compliancecore.reports.title49_coverage.export",
                tenantId,
                actorUserId,
                "title49_coverage_report",
                "summary",
                "success",
                cancellationToken: cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName($"ExportComplianceCoreTitle49CoverageExplorerSummary{routeNameSuffix}");
    }

    private static void MapTitle49CitationCoverageGroup(WebApplication app, string routePrefix, string routeNameSuffix)
    {
        var group = app.MapGroup(routePrefix)
            .WithTags("Title49CitationCoverageReports")
            .RequireAuthorization();

        group.MapGet("/summary", async (
            ComplianceCoreAuthorizationService authorization,
            Title49CitationCoverageReportService reportService,
            IComplianceCoreAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var summary = await reportService.GetSummaryAsync(tenantId, cancellationToken);
            await audit.WriteAsync(
                "compliancecore.reports.title49_citation_coverage.summary",
                tenantId,
                actorUserId,
                "title49_citation_coverage_report",
                null,
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(summary);
        })
        .WithName($"GetComplianceCoreTitle49CitationCoverageReportSummary{routeNameSuffix}");

        group.MapGet("/summary/export", async (
            ComplianceCoreAuthorizationService authorization,
            Title49CitationCoverageReportService reportService,
            IComplianceCoreAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await reportService.ExportSummaryCsvAsync(tenantId, cancellationToken);
            await audit.WriteAsync(
                "compliancecore.reports.title49_citation_coverage.export",
                tenantId,
                actorUserId,
                "title49_citation_coverage_report",
                "summary",
                "success",
                cancellationToken: cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName($"ExportComplianceCoreTitle49CitationCoverageReportSummary{routeNameSuffix}");
    }

    private static void MapCitationReviewGroup(WebApplication app, string routePrefix, string routeNameSuffix)
    {
        var group = app.MapGroup(routePrefix)
            .WithTags("CitationReviewReports")
            .RequireAuthorization();

        group.MapGet("/summary", async (
            string? reviewState,
            string? programKey,
            string? rulePackKey,
            int? limit,
            ComplianceCoreAuthorizationService authorization,
            CitationReviewReportService reportService,
            IComplianceCoreAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var summary = await reportService.GetSummaryAsync(
                tenantId,
                reviewState,
                programKey,
                rulePackKey,
                limit,
                cancellationToken);
            await audit.WriteAsync(
                "compliancecore.reports.citation_review.summary",
                tenantId,
                actorUserId,
                "citation_review_report",
                null,
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(summary);
        })
        .WithName($"GetComplianceCoreCitationReviewReportSummary{routeNameSuffix}");

        group.MapGet("/summary/export", async (
            string? reviewState,
            string? programKey,
            string? rulePackKey,
            ComplianceCoreAuthorizationService authorization,
            CitationReviewReportService reportService,
            IComplianceCoreAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await reportService.ExportSummaryCsvAsync(
                tenantId,
                reviewState,
                programKey,
                rulePackKey,
                cancellationToken);
            await audit.WriteAsync(
                "compliancecore.reports.citation_review.export",
                tenantId,
                actorUserId,
                "citation_review_report",
                "summary",
                "success",
                cancellationToken: cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName($"ExportComplianceCoreCitationReviewReportSummary{routeNameSuffix}");
    }

    private static void MapRuleChangeImpactGroup(WebApplication app, string routePrefix, string routeNameSuffix)
    {
        var group = app.MapGroup(routePrefix)
            .WithTags("RuleChangeImpactReports")
            .RequireAuthorization();

        group.MapGet("/summary", async (
            string? packKey,
            ComplianceCoreAuthorizationService authorization,
            RuleChangeImpactReportService reportService,
            IComplianceCoreAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRuleChangeMonitoringRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var summary = await reportService.GetSummaryAsync(tenantId, packKey, cancellationToken);
            await audit.WriteAsync(
                "compliancecore.reports.rule_change_impact.summary",
                tenantId,
                actorUserId,
                "rule_change_impact_report",
                null,
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(summary);
        })
        .WithName($"GetComplianceCoreRuleChangeImpactReportSummary{routeNameSuffix}");

        group.MapGet("/summary/export", async (
            string? packKey,
            ComplianceCoreAuthorizationService authorization,
            RuleChangeImpactReportService reportService,
            IComplianceCoreAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRuleChangeMonitoringRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await reportService.ExportSummaryCsvAsync(tenantId, packKey, cancellationToken);
            await audit.WriteAsync(
                "compliancecore.reports.rule_change_impact.export",
                tenantId,
                actorUserId,
                "rule_change_impact_report",
                "summary",
                "success",
                cancellationToken: cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName($"ExportComplianceCoreRuleChangeImpactReportSummary{routeNameSuffix}");
    }

    private static void MapEvaluationHistoryGroup(WebApplication app, string routePrefix, string routeNameSuffix)
    {
        var group = app.MapGroup(routePrefix)
            .WithTags("EvaluationHistoryReports")
            .RequireAuthorization();

        group.MapGet("/summary", async (
            Guid? rulePackId,
            string? overallResult,
            string? status,
            int? limit,
            int? offset,
            ComplianceCoreAuthorizationService authorization,
            EvaluationHistoryExplorerService reportService,
            IComplianceCoreAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePacksRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var summary = await reportService.GetSummaryAsync(
                tenantId,
                rulePackId,
                overallResult,
                status,
                limit,
                offset,
                cancellationToken);
            await audit.WriteAsync(
                "compliancecore.reports.evaluation_history.summary",
                tenantId,
                actorUserId,
                "evaluation_history_report",
                null,
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(summary);
        })
        .WithName($"GetComplianceCoreEvaluationHistoryReportSummary{routeNameSuffix}");

        group.MapGet("/summary/export", async (
            Guid? rulePackId,
            string? overallResult,
            string? status,
            ComplianceCoreAuthorizationService authorization,
            EvaluationHistoryExplorerService reportService,
            IComplianceCoreAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePacksRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await reportService.ExportSummaryCsvAsync(
                tenantId,
                rulePackId,
                overallResult,
                status,
                cancellationToken);
            await audit.WriteAsync(
                "compliancecore.reports.evaluation_history.export",
                tenantId,
                actorUserId,
                "evaluation_history_report",
                "summary",
                "success",
                cancellationToken: cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName($"ExportComplianceCoreEvaluationHistoryReportSummary{routeNameSuffix}");
    }

    public static void MapComplianceCoreEntityExportEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/exports")
            .WithTags("EntityExports")
            .RequireAuthorization();

        group.MapGet("/manifest", (
            ComplianceCoreAuthorizationService authorization,
            ComplianceCoreEntityBulkExportService exportService,
            HttpContext context) =>
        {
            authorization.RequireFindingsReportExport(context.User);
            return Results.Ok(exportService.GetManifest());
        })
        .WithName("GetComplianceCoreEntityExportManifest");

        group.MapGet("/findings", async (
            string? status,
            bool? openOnly,
            ComplianceCoreAuthorizationService authorization,
            ComplianceCoreEntityBulkExportService exportService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireFindingsReportExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await exportService.ExportFindingsCsvAsync(
                tenantId,
                actorUserId,
                status,
                openOnly,
                cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName("ExportComplianceCoreFindingsCsv");

        group.MapGet("/evaluations", async (
            ComplianceCoreAuthorizationService authorization,
            ComplianceCoreEntityBulkExportService exportService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireFindingsReportExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await exportService.ExportEvaluationsCsvAsync(
                tenantId,
                actorUserId,
                cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName("ExportComplianceCoreEvaluationsCsv");

        group.MapGet("/workflow-gate-checks", async (
            ComplianceCoreAuthorizationService authorization,
            ComplianceCoreEntityBulkExportService exportService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireFindingsReportExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await exportService.ExportWorkflowGateChecksCsvAsync(
                tenantId,
                actorUserId,
                cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName("ExportComplianceCoreWorkflowGateChecksCsv");

        group.MapGet("/rule-packs", async (
            string? status,
            ComplianceCoreAuthorizationService authorization,
            ComplianceCoreEntityBulkExportService exportService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireFindingsReportExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var export = await exportService.ExportRulePacksCsvAsync(
                tenantId,
                actorUserId,
                status,
                cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName("ExportComplianceCoreRulePacksCsv");
    }
}
