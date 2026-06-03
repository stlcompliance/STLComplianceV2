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
                    new { Key = "waivers", Path = "/api/v1/reports/waivers" },
                    new { Key = "exception_exemptions", Path = "/api/v1/reports/exception-exemptions" },
                    new { Key = "integration_health", Path = "/api/v1/reports/integration-health" },
                    new { Key = "audit_readiness", Path = "/api/v1/reports/audit-readiness" },
                    new { Key = "remediation_queue", Path = "/api/v1/reports/remediation-queue" }
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
