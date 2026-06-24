using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ReportArr.Api.Data;
using ReportArr.Api.Models;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Print;

namespace ReportArr.Api.Services;

public sealed class ReportArrPrintableProvider(
    ReportArrStore store,
    IPdfRenderer pdfRenderer,
    IRecordArchiveClient archiveClient) : IPrintableProvider
{
    private static readonly HashSet<string> SupportedTemplates = new(StringComparer.OrdinalIgnoreCase)
    {
        "reportarr.report.print",
        "reportarr.report.pdf_export",
        "reportarr.dashboard.snapshot",
        "reportarr.scheduled_report.output",
        "reportarr.audit.packet",
        "reportarr.compliance_readiness.packet",
        "reportarr.management.summary",
    };

    public bool CanHandle(
        StlPrintProviderContext context,
        StlPrintDocumentRequest request,
        StlPrintTemplateDescriptor template)
    {
        if (!string.Equals(context.Product.ProductKey, "reportarr", StringComparison.OrdinalIgnoreCase)
            || !string.Equals(template.ProductKey, "reportarr", StringComparison.OrdinalIgnoreCase)
            || !SupportedTemplates.Contains(template.TemplateKey))
        {
            return false;
        }

        return template.TemplateKey switch
        {
            "reportarr.dashboard.snapshot" =>
                string.Equals(request.SourceEntityType, "dashboard", StringComparison.OrdinalIgnoreCase),
            "reportarr.report.print" or "reportarr.report.pdf_export" =>
                string.Equals(request.SourceEntityType, "report_run", StringComparison.OrdinalIgnoreCase),
            "reportarr.scheduled_report.output" =>
                string.Equals(request.SourceEntityType, "report_schedule", StringComparison.OrdinalIgnoreCase),
            "reportarr.audit.packet" or "reportarr.compliance_readiness.packet" or "reportarr.management.summary" =>
                string.Equals(request.SourceEntityType, "audit_package", StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }

    public Task<StlPrintPreviewResult> BuildPreviewAsync(
        StlPrintProviderContext context,
        StlPrintDocumentRequest request,
        StlPrintTemplateDescriptor template,
        CancellationToken cancellationToken)
    {
        var model = BuildModel(context, request, template);
        var html = RenderHtml(model);

        return Task.FromResult(
            new StlPrintPreviewResult(
                model.DocumentTitle,
                model.SourceDisplayRef,
                template.TemplateKey,
                request.TemplateVersion ?? template.Version,
                html,
                PreviewRoute: null,
                model.Warnings,
                model.MissingRequirements));
    }

    public async Task<StlGeneratedPrintFile> GeneratePdfAsync(
        StlPrintProviderContext context,
        StlPrintDocumentRequest request,
        StlPrintTemplateDescriptor template,
        string action,
        CancellationToken cancellationToken)
    {
        var model = BuildModel(context, request, template);
        return await BuildGeneratedFileAsync(request, template, model, cancellationToken);
    }

    public async Task<StlPrintArchiveResult> ArchiveOfficialAsync(
        StlPrintProviderContext context,
        StlPrintDocumentRequest request,
        StlPrintTemplateDescriptor template,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(template.TemplateKey, "reportarr.audit.packet", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "print.archive_not_supported",
                $"Template '{template.TemplateKey}' does not support official RecordArr archive in ReportArr.",
                409);
        }

        var model = BuildModel(context, request, template);
        var file = await BuildGeneratedFileAsync(request, template, model, cancellationToken);

        var archiveRequest = new StlRecordArchiveRequest(
            context.TenantId,
            context.Product.ProductKey,
            request.SourceEntityType ?? "audit_package",
            request.SourceEntityId ?? string.Empty,
            model.SourceDisplayRef,
            model.DocumentClass,
            model.DocumentType,
            model.DocumentSubtype,
            model.DocumentTitle,
            template.TemplateKey,
            request.TemplateVersion ?? template.Version,
            DateTimeOffset.UtcNow,
            context.RequestedByPersonId,
            template.RetentionClass,
            file.ContentHash ?? string.Empty,
            file.FileName,
            file.Content);

        var receipt = await archiveClient.ArchiveAsync(archiveRequest, cancellationToken);

        return new StlPrintArchiveResult(
            model.DocumentTitle,
            model.SourceDisplayRef,
            template.TemplateKey,
            request.TemplateVersion ?? template.Version,
            receipt.RecordArrDocumentId,
            receipt.FileName ?? file.FileName,
            receipt.ContentHash ?? file.ContentHash,
            model.Warnings,
            model.MissingRequirements);
    }

    private async Task<StlGeneratedPrintFile> BuildGeneratedFileAsync(
        StlPrintDocumentRequest request,
        StlPrintTemplateDescriptor template,
        ReportArrPrintModel model,
        CancellationToken cancellationToken)
    {
        var renderable = new StlRenderablePrintDocument(
            model.DocumentTitle,
            model.SourceDisplayRef,
            template.TemplateKey,
            request.TemplateVersion ?? template.Version,
            RenderHtml(model),
            BuildFileName(model.SourceDisplayRef, template.TemplateKey),
            "application/pdf",
            model.Warnings,
            model.MissingRequirements);

        return await pdfRenderer.RenderPdfAsync(renderable, cancellationToken);
    }

    private ReportArrPrintModel BuildModel(
        StlPrintProviderContext context,
        StlPrintDocumentRequest request,
        StlPrintTemplateDescriptor template)
    {
        var options = ParseOptions(request.OptionsJson);
        var tenantDisplayName = ResolveOptionValue(options, "tenantDisplayName") ?? "Current tenant workspace";
        var generatedByDisplayName = ResolveGeneratedByDisplayName(context.Principal, options);
        var templateVersion = request.TemplateVersion ?? template.Version;

        return template.TemplateKey switch
        {
            "reportarr.dashboard.snapshot" => BuildDashboardModel(
                context,
                request,
                templateVersion,
                tenantDisplayName,
                generatedByDisplayName),
            "reportarr.report.print" or "reportarr.report.pdf_export" => BuildReportRunModel(
                context,
                request,
                template.TemplateKey,
                templateVersion,
                tenantDisplayName,
                generatedByDisplayName),
            "reportarr.scheduled_report.output" => BuildScheduleModel(
                context,
                request,
                templateVersion,
                tenantDisplayName,
                generatedByDisplayName),
            "reportarr.audit.packet" or "reportarr.compliance_readiness.packet" or "reportarr.management.summary" => BuildAuditPackageModel(
                context,
                request,
                template.TemplateKey,
                templateVersion,
                tenantDisplayName,
                generatedByDisplayName),
            _ => throw new StlApiException(
                "print.template_not_supported",
                $"ReportArr template '{template.TemplateKey}' is not supported.",
                400)
        };
    }

    private ReportArrPrintModel BuildDashboardModel(
        StlPrintProviderContext context,
        StlPrintDocumentRequest request,
        string templateVersion,
        string tenantDisplayName,
        string generatedByDisplayName)
    {
        var dashboardId = RequireSourceEntityId(request, "dashboard");
        var dashboard = store.GetDashboard(context.Principal, dashboardId);
        if (dashboard is null)
        {
            throw new StlApiException(
                "print.source_not_found",
                $"Dashboard '{dashboardId}' was not found or is not available for printing.",
                404);
        }

        var filters = store.GetDashboardFilters(context.Principal)
            .Where(filter => string.Equals(filter.DashboardId, dashboard.DashboardId, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        var drilldowns = store.GetDrilldowns(context.Principal)
            .Where(drilldown => string.Equals(drilldown.DashboardId, dashboard.DashboardId, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        var widgets = store.GetWidgets(context.Principal)
            .Where(widget => string.Equals(widget.DashboardId, dashboard.DashboardId, StringComparison.OrdinalIgnoreCase))
            .OrderBy(widget => widget.SortOrder)
            .ToArray();
        var policy = store.GetDashboardAccessPolicies(context.Principal)
            .FirstOrDefault(candidate => string.Equals(candidate.DashboardId, dashboard.DashboardId, StringComparison.OrdinalIgnoreCase));
        var sourceDatasetCount = widgets
            .Select(widget => widget.DatasetRef)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
        var sourceReadModelCount = widgets
            .Select(widget => widget.ReadModelRef)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();

        var warnings = new List<string>();
        var missingRequirements = new List<string>();

        if (!string.Equals(dashboard.FreshnessStatus, "fresh", StringComparison.OrdinalIgnoreCase))
        {
            warnings.Add($"Dashboard freshness is {HumanizeToken(dashboard.FreshnessStatus)}.");
        }

        if (widgets.Length == 0)
        {
            missingRequirements.Add("No widgets are configured for this dashboard.");
        }

        var sections = new List<ReportArrPrintSection>();
        AddSingleLineSection(sections, "Dashboard description", dashboard.Description);
        AddSection(
            sections,
            "Widget summary",
            widgets.Select(widget =>
                $"{widget.Title} · {HumanizeToken(widget.WidgetType)} · {HumanizeToken(widget.Status)} · freshness {HumanizeToken(widget.FreshnessStatus)}"));
        AddSection(
            sections,
            "Filter summary",
            filters.Select(filter =>
                $"{filter.Label} · {HumanizeToken(filter.FilterType)} · required {(filter.Required ? "yes" : "no")} · default {DefaultText(filter.DefaultValue)}"));
        AddSection(
            sections,
            "Drilldown summary",
            drilldowns.Select(drilldown =>
                $"{drilldown.Title} · {HumanizeToken(drilldown.TargetType)} · {HumanizeToken(drilldown.Status)}"));
        if (policy is not null)
        {
            AddSection(
                sections,
                "Access policy",
                [
                    $"Visibility: {HumanizeToken(policy.Visibility)}",
                    $"Export allowed: {(policy.ExportAllowed ? "yes" : "no")}",
                    $"Source product restrictions: {(policy.SourceProductRestrictions.Count == 0 ? "none" : string.Join(", ", policy.SourceProductRestrictions.Select(HumanizeToken)))}",
                ]);
        }

        AddSection(
            sections,
            "Ownership note",
            [
                "ReportArr owns this dashboard snapshot and export surface.",
                "Underlying operational records and identifiers remain owned by the source products referenced by the dashboard datasets.",
            ]);

        return new ReportArrPrintModel(
            TenantDisplayName: tenantDisplayName,
            ProductDisplayName: context.Product.DisplayName,
            SourceDisplayRef: dashboard.DashboardNumber,
            DocumentTitle: $"{dashboard.Title} dashboard snapshot",
            TemplateKey: "reportarr.dashboard.snapshot",
            TemplateVersion: templateVersion,
            DocumentStatus: request.DocumentStatus ?? StlPrintDocumentStatuses.WorkingCopy,
            GeneratedAtUtc: DateTimeOffset.UtcNow,
            GeneratedByDisplayName: generatedByDisplayName,
            DocumentClass: "analytics",
            DocumentType: "dashboard",
            DocumentSubtype: "snapshot",
            SummaryLines:
            [
                $"Dashboard number: {dashboard.DashboardNumber}",
                $"Dashboard key: {dashboard.DashboardKey}",
                $"Type: {HumanizeToken(dashboard.DashboardType)}",
                $"Status: {HumanizeToken(dashboard.Status)}",
                $"Freshness: {HumanizeToken(dashboard.FreshnessStatus)}",
                $"Default date range: {dashboard.DefaultDateRange}",
                $"Widgets: {widgets.Length}",
                $"Filters: {filters.Length}",
                $"Drilldowns: {drilldowns.Length}",
                $"Source traces: {sourceDatasetCount} dataset source(s), {sourceReadModelCount} read model(s)",
                $"Last viewed: {FormatDateTime(dashboard.LastViewedAt)}",
            ],
            Sections: sections,
            Warnings: warnings.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            MissingRequirements: missingRequirements.Distinct(StringComparer.OrdinalIgnoreCase).ToArray());
    }

    private ReportArrPrintModel BuildReportRunModel(
        StlPrintProviderContext context,
        StlPrintDocumentRequest request,
        string templateKey,
        string templateVersion,
        string tenantDisplayName,
        string generatedByDisplayName)
    {
        var reportRunId = RequireSourceEntityId(request, "report run");
        var run = store.GetReportRun(context.Principal, reportRunId);
        if (run is null)
        {
            throw new StlApiException(
                "print.source_not_found",
                $"Report run '{reportRunId}' was not found or is not available for printing.",
                404);
        }

        var definition = store.GetReportDefinition(context.Principal, run.ReportDefinitionId);
        var reportParameters = store.GetReportParameters(context.Principal)
            .Where(parameter => string.Equals(parameter.ReportDefinitionId, run.ReportDefinitionId, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        var reportSections = store.GetReportSections(context.Principal)
            .Where(section => string.Equals(section.ReportDefinitionId, run.ReportDefinitionId, StringComparison.OrdinalIgnoreCase))
            .OrderBy(section => section.Sequence)
            .ToArray();
        var exportJobs = store.GetExportJobs(context.Principal)
            .Where(job => string.Equals(job.ReportRunId, run.ReportRunId, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(job => job.GeneratedAt)
            .ToArray();

        var warnings = new List<string>();
        var missingRequirements = new List<string>();

        if (run.WarningCount > 0)
        {
            warnings.Add($"The report run completed with {run.WarningCount} warning(s).");
        }

        if (!string.Equals(run.FreshnessStatus, "fresh", StringComparison.OrdinalIgnoreCase))
        {
            warnings.Add($"Report freshness is {HumanizeToken(run.FreshnessStatus)}.");
        }

        if (run.ErrorCount > 0)
        {
            missingRequirements.Add($"The report run recorded {run.ErrorCount} error(s).");
        }

        if (reportSections.Length == 0)
        {
            missingRequirements.Add("No report sections are configured for the source definition.");
        }

        var sections = new List<ReportArrPrintSection>();
        AddSingleLineSection(sections, "Report description", definition?.Description);
        AddSection(
            sections,
            "Execution summary",
            [
                run.FreshnessSummary,
                string.IsNullOrWhiteSpace(run.ErrorMessage) ? null : $"Error message: {run.ErrorMessage}",
                $"Dataset traces included: {definition?.DatasetRefs.Count ?? 0}",
                $"Read models included: {definition?.ReadModelRefs.Count ?? 0}",
            ]);
        AddSection(
            sections,
            "Parameters used",
            run.ParametersUsed.Select(parameter => parameter));
        AddSection(
            sections,
            "Filters used",
            run.FiltersUsed.Select(filter => filter));
        AddSection(
            sections,
            "Configured sections",
            reportSections.Select(section =>
                $"{section.Sequence}. {section.Title} · {HumanizeToken(section.SectionType)}"));
        AddSection(
            sections,
            "Export history",
            exportJobs.Select(job =>
                $"{HumanizeToken(job.ExportType)} · {HumanizeToken(job.ExportFormat)} · {HumanizeToken(job.Status)} · generated {FormatDateTime(job.GeneratedAt)}"));
        AddSection(
            sections,
            "Ownership note",
            [
                "ReportArr owns report generation, rendered output, and snapshot history.",
                "Underlying source records remain owned by the products that feed the report datasets and read models.",
            ]);

        return new ReportArrPrintModel(
            TenantDisplayName: tenantDisplayName,
            ProductDisplayName: context.Product.DisplayName,
            SourceDisplayRef: run.ReportRunNumber,
            DocumentTitle: templateKey switch
            {
                "reportarr.report.pdf_export" => $"{run.Title} PDF export",
                _ => $"{run.Title} report print view",
            },
            TemplateKey: templateKey,
            TemplateVersion: templateVersion,
            DocumentStatus: request.DocumentStatus ?? StlPrintDocumentStatuses.Copy,
            GeneratedAtUtc: DateTimeOffset.UtcNow,
            GeneratedByDisplayName: generatedByDisplayName,
            DocumentClass: "analytics",
            DocumentType: "report",
            DocumentSubtype: string.Equals(templateKey, "reportarr.report.pdf_export", StringComparison.OrdinalIgnoreCase)
                ? "pdf_export"
                : "print_view",
            SummaryLines:
            [
                $"Report number: {definition?.ReportNumber ?? "Not available"}",
                $"Run number: {run.ReportRunNumber}",
                $"Report type: {HumanizeToken(definition?.ReportType)}",
                $"Run status: {HumanizeToken(run.Status)}",
                $"Output format: {HumanizeToken(run.OutputFormat)}",
                $"Requested: {FormatDateTime(run.RequestedAt)}",
                $"Completed: {FormatDateTime(run.CompletedAt)}",
                $"Rows: {run.RowCount}",
                $"Warnings: {run.WarningCount}",
                $"Errors: {run.ErrorCount}",
                $"Parameters applied: {run.ParametersUsed.Count}",
                $"Filters applied: {run.FiltersUsed.Count}",
                $"Configured parameters: {reportParameters.Length}",
            ],
            Sections: sections,
            Warnings: warnings.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            MissingRequirements: missingRequirements.Distinct(StringComparer.OrdinalIgnoreCase).ToArray());
    }

    private ReportArrPrintModel BuildScheduleModel(
        StlPrintProviderContext context,
        StlPrintDocumentRequest request,
        string templateVersion,
        string tenantDisplayName,
        string generatedByDisplayName)
    {
        var scheduleId = RequireSourceEntityId(request, "report schedule");
        var schedule = store.GetReportSchedules(context.Principal)
            .FirstOrDefault(item => string.Equals(item.ScheduleId, scheduleId, StringComparison.OrdinalIgnoreCase));
        if (schedule is null)
        {
            throw new StlApiException(
                "print.source_not_found",
                $"Report schedule '{scheduleId}' was not found or is not available for printing.",
                404);
        }

        var definition = store.GetReportDefinition(context.Principal, schedule.ReportDefinitionId);
        var recipients = store.GetReportRecipients(context.Principal)
            .Where(recipient => string.Equals(recipient.ScheduleId, schedule.ScheduleId, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        var recipientTypes = recipients
            .GroupBy(recipient => recipient.RecipientType, StringComparer.OrdinalIgnoreCase)
            .Select(group => $"{group.Count()} {HumanizeToken(group.Key)}")
            .ToArray();
        var recipientFormats = recipients
            .GroupBy(recipient => recipient.DeliveryFormat, StringComparer.OrdinalIgnoreCase)
            .Select(group => $"{group.Count()} {HumanizeToken(group.Key)}")
            .ToArray();

        var warnings = new List<string>();
        var missingRequirements = new List<string>();

        if (!string.Equals(schedule.Status, "active", StringComparison.OrdinalIgnoreCase))
        {
            warnings.Add($"The schedule is currently {HumanizeToken(schedule.Status)}.");
        }

        if (recipients.Length == 0)
        {
            missingRequirements.Add("No recipients are configured for this scheduled output.");
        }

        var sections = new List<ReportArrPrintSection>();
        AddSection(
            sections,
            "Delivery rules",
            [
                $"Cadence: {HumanizeToken(schedule.Cadence)}",
                $"Timezone: {schedule.Timezone}",
                $"Delivery method: {HumanizeToken(schedule.DeliveryMethod)}",
                $"Cron expression: {DefaultText(schedule.CronExpression)}",
                $"Starts at: {FormatDateTime(schedule.StartsAt)}",
                $"Ends at: {FormatDateTime(schedule.EndsAt)}",
            ]);
        AddSection(sections, "Parameter set", schedule.Parameters);
        AddSection(
            sections,
            "Recipient summary",
            [
                $"Recipient total: {recipients.Length}",
                $"Recipient types: {(recipientTypes.Length == 0 ? "none" : string.Join(", ", recipientTypes))}",
                $"Delivery formats: {(recipientFormats.Length == 0 ? "none" : string.Join(", ", recipientFormats))}",
            ]);
        AddSection(
            sections,
            "Ownership note",
            [
                "ReportArr owns the scheduled output definition and delivery snapshot.",
                "Recipient-specific access filters still depend on the report access policy and source-product ownership boundaries.",
            ]);

        return new ReportArrPrintModel(
            TenantDisplayName: tenantDisplayName,
            ProductDisplayName: context.Product.DisplayName,
            SourceDisplayRef: schedule.ScheduleNumber,
            DocumentTitle: $"{schedule.Title} scheduled report output",
            TemplateKey: "reportarr.scheduled_report.output",
            TemplateVersion: templateVersion,
            DocumentStatus: request.DocumentStatus ?? StlPrintDocumentStatuses.Copy,
            GeneratedAtUtc: DateTimeOffset.UtcNow,
            GeneratedByDisplayName: generatedByDisplayName,
            DocumentClass: "analytics",
            DocumentType: "report",
            DocumentSubtype: "scheduled_output",
            SummaryLines:
            [
                $"Report number: {definition?.ReportNumber ?? "Not available"}",
                $"Schedule number: {schedule.ScheduleNumber}",
                $"Schedule status: {HumanizeToken(schedule.Status)}",
                $"Cadence: {HumanizeToken(schedule.Cadence)}",
                $"Delivery method: {HumanizeToken(schedule.DeliveryMethod)}",
                $"Timezone: {schedule.Timezone}",
                $"Next run: {FormatDateTime(schedule.NextRunAt)}",
                $"Last run: {FormatDateTime(schedule.LastRunAt)}",
                $"Parameters configured: {schedule.Parameters.Count}",
                $"Recipients configured: {recipients.Length}",
            ],
            Sections: sections,
            Warnings: warnings.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            MissingRequirements: missingRequirements.Distinct(StringComparer.OrdinalIgnoreCase).ToArray());
    }

    private ReportArrPrintModel BuildAuditPackageModel(
        StlPrintProviderContext context,
        StlPrintDocumentRequest request,
        string templateKey,
        string templateVersion,
        string tenantDisplayName,
        string generatedByDisplayName)
    {
        var auditPackageId = RequireSourceEntityId(request, "audit package");
        var auditPackage = store.GetAuditPackage(context.Principal, auditPackageId);
        if (auditPackage is null)
        {
            throw new StlApiException(
                "print.source_not_found",
                $"Audit package '{auditPackageId}' was not found or is not available for printing.",
                404);
        }

        var linkedRuns = auditPackage.ReportRunRefs
            .Select(reportRunId => store.GetReportRun(context.Principal, reportRunId))
            .Where(run => run is not null)
            .Cast<ReportArrReportRunResponse>()
            .OrderByDescending(run => run.RequestedAt)
            .ToArray();
        var warnings = new List<string>();
        var missingRequirements = new List<string>();

        if (auditPackage.ReadinessScore < 90m)
        {
            warnings.Add($"Audit readiness is {auditPackage.ReadinessScore:0.#}%.");
        }

        if (!IsClearSummary(auditPackage.InvalidEvidenceSummary))
        {
            warnings.Add(auditPackage.InvalidEvidenceSummary);
        }

        if (!IsClearSummary(auditPackage.MissingEvidenceSummary))
        {
            missingRequirements.Add(auditPackage.MissingEvidenceSummary);
        }

        if (linkedRuns.Length == 0 && string.Equals(templateKey, "reportarr.audit.packet", StringComparison.OrdinalIgnoreCase))
        {
            missingRequirements.Add("No linked report runs are available for this audit packet.");
        }

        var sections = templateKey switch
        {
            "reportarr.management.summary" => BuildManagementSummarySections(context, auditPackage, linkedRuns),
            "reportarr.compliance_readiness.packet" => BuildComplianceReadinessSections(auditPackage, linkedRuns),
            _ => BuildAuditPacketSections(context, auditPackage, linkedRuns),
        };

        var (documentType, documentSubtype) = templateKey switch
        {
            "reportarr.management.summary" => ("report_output", "management_summary"),
            "reportarr.compliance_readiness.packet" => ("audit_packet", "readiness"),
            _ => ("audit_packet", "cross_product"),
        };

        return new ReportArrPrintModel(
            TenantDisplayName: tenantDisplayName,
            ProductDisplayName: context.Product.DisplayName,
            SourceDisplayRef: auditPackage.PackageNumber,
            DocumentTitle: templateKey switch
            {
                "reportarr.management.summary" => $"{auditPackage.Title} management summary",
                "reportarr.compliance_readiness.packet" => $"{auditPackage.Title} compliance readiness packet",
                _ => $"{auditPackage.Title} audit packet",
            },
            TemplateKey: templateKey,
            TemplateVersion: templateVersion,
            DocumentStatus: request.DocumentStatus ?? StlPrintDocumentStatuses.Official,
            GeneratedAtUtc: DateTimeOffset.UtcNow,
            GeneratedByDisplayName: generatedByDisplayName,
            DocumentClass: "compliance",
            DocumentType: documentType,
            DocumentSubtype: documentSubtype,
            SummaryLines:
            [
                $"Package number: {auditPackage.PackageNumber}",
                $"Status: {HumanizeToken(auditPackage.Status)}",
                $"Readiness score: {auditPackage.ReadinessScore:0.#}%",
                $"Requested by: {ResolveActorLabel(context, auditPackage.RequestedByPersonId)}",
                $"Generated at: {FormatDateTime(auditPackage.GeneratedAt)}",
                $"Locked at: {FormatDateTime(auditPackage.LockedAt)}",
                $"Source products: {auditPackage.SourceProductRefs.Count}",
                $"Linked report runs: {linkedRuns.Length}",
                $"Compliance evaluations: {auditPackage.ComplianceEvaluationRefs.Count}",
            ],
            Sections: sections,
            Warnings: warnings.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            MissingRequirements: missingRequirements.Distinct(StringComparer.OrdinalIgnoreCase).ToArray());
    }

    private static IReadOnlyList<ReportArrPrintSection> BuildAuditPacketSections(
        StlPrintProviderContext context,
        ReportArrAuditPackageResponse auditPackage,
        IReadOnlyList<ReportArrReportRunResponse> linkedRuns)
    {
        var sections = new List<ReportArrPrintSection>();
        AddSingleLineSection(sections, "Package description", auditPackage.Description);
        AddAuditScopeSection(sections, auditPackage.AuditScope);
        AddSection(
            sections,
            "Evidence readiness",
            [
                $"Missing evidence: {DefaultText(auditPackage.MissingEvidenceSummary)}",
                $"Invalid evidence: {DefaultText(auditPackage.InvalidEvidenceSummary)}",
                $"RecordArr package linked: {(string.IsNullOrWhiteSpace(auditPackage.RecordArrPackageRef) ? "no" : "yes")}",
                $"Source object count: {auditPackage.SourceObjectRefs.Count}",
            ]);
        AddSection(
            sections,
            "Source products",
            auditPackage.SourceProductRefs.Select(HumanizeToken));
        AddSection(
            sections,
            "Included report runs",
            linkedRuns.Select(run =>
                $"{run.ReportRunNumber} · {run.Title} · {HumanizeToken(run.Status)} · {run.RowCount} rows"));
        AddSection(
            sections,
            "Ownership note",
            [
                "ReportArr owns audit packet assembly, report snapshots, and package readiness summaries.",
                "Source products retain ownership of the operational data referenced by the packet.",
                "RecordArr owns the official archived copy once the packet is issued.",
            ]);
        return sections;
    }

    private static IReadOnlyList<ReportArrPrintSection> BuildComplianceReadinessSections(
        ReportArrAuditPackageResponse auditPackage,
        IReadOnlyList<ReportArrReportRunResponse> linkedRuns)
    {
        var sections = new List<ReportArrPrintSection>();
        AddSingleLineSection(sections, "Package description", auditPackage.Description);
        AddAuditScopeSection(sections, auditPackage.AuditScope);
        AddSection(
            sections,
            "Readiness summary",
            [
                $"Readiness score: {auditPackage.ReadinessScore:0.#}%",
                $"Missing evidence: {DefaultText(auditPackage.MissingEvidenceSummary)}",
                $"Invalid evidence: {DefaultText(auditPackage.InvalidEvidenceSummary)}",
                $"Linked report runs: {linkedRuns.Count}",
                $"Source products in scope: {(auditPackage.SourceProductRefs.Count == 0 ? "none" : string.Join(", ", auditPackage.SourceProductRefs.Select(HumanizeToken)))}",
            ]);
        AddSection(
            sections,
            "Compliance note",
            [
                "This readiness packet summarizes ReportArr-owned evidence coverage and report snapshots.",
                "Underlying compliance facts and source records still remain owned by Compliance Core and the contributing source products.",
            ]);
        return sections;
    }

    private static IReadOnlyList<ReportArrPrintSection> BuildManagementSummarySections(
        StlPrintProviderContext context,
        ReportArrAuditPackageResponse auditPackage,
        IReadOnlyList<ReportArrReportRunResponse> linkedRuns)
    {
        var sections = new List<ReportArrPrintSection>();
        AddSection(
            sections,
            "Executive summary",
            [
                $"Audit status: {HumanizeToken(auditPackage.Status)}",
                $"Readiness score: {auditPackage.ReadinessScore:0.#}%",
                $"Missing evidence: {DefaultText(auditPackage.MissingEvidenceSummary)}",
                $"Invalid evidence: {DefaultText(auditPackage.InvalidEvidenceSummary)}",
                $"Requested by: {ResolveActorLabel(context, auditPackage.RequestedByPersonId)}",
            ]);
        AddSection(
            sections,
            "Coverage snapshot",
            [
                $"Source products in scope: {(auditPackage.SourceProductRefs.Count == 0 ? "none" : string.Join(", ", auditPackage.SourceProductRefs.Select(HumanizeToken)))}",
                $"Compliance evaluations linked: {auditPackage.ComplianceEvaluationRefs.Count}",
                $"Report runs linked: {linkedRuns.Count}",
                $"Audit window: {DescribeDateRange(auditPackage.AuditScope.DateRangeStart, auditPackage.AuditScope.DateRangeEnd)}",
            ]);
        AddSection(
            sections,
            "Ownership note",
            [
                "This management summary is a ReportArr-owned executive snapshot.",
                "Source systems continue to own the operational records behind the summarized readiness signals.",
            ]);
        return sections;
    }

    private static void AddAuditScopeSection(
        ICollection<ReportArrPrintSection> sections,
        ReportArrAuditScopeResponse auditScope)
    {
        AddSection(
            sections,
            "Audit scope",
            [
                $"Scope type: {HumanizeToken(auditScope.ScopeType)}",
                $"Date range: {DescribeDateRange(auditScope.DateRangeStart, auditScope.DateRangeEnd)}",
                $"Product filters: {(auditScope.ProductFilters.Count == 0 ? "none" : string.Join(", ", auditScope.ProductFilters.Select(HumanizeToken)))}",
                $"Rulepack references: {auditScope.RulepackRefs.Count}",
                $"Site filters: {auditScope.SiteRefs.Count}",
                $"Department filters: {auditScope.DepartmentRefs.Count}",
                $"Object references in scope: {auditScope.ObjectRefs.Count}",
                $"Include evidence: {(auditScope.IncludeEvidence ? "yes" : "no")}",
                $"Include source trace: {(auditScope.IncludeSourceTrace ? "yes" : "no")}",
            ]);
    }

    private static void AddSection(
        ICollection<ReportArrPrintSection> sections,
        string title,
        IEnumerable<string?> lines)
    {
        var normalizedLines = lines
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => line!.Trim())
            .ToArray();
        if (normalizedLines.Length == 0)
        {
            return;
        }

        sections.Add(new ReportArrPrintSection(title, normalizedLines));
    }

    private static void AddSingleLineSection(
        ICollection<ReportArrPrintSection> sections,
        string title,
        string? line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return;
        }

        sections.Add(new ReportArrPrintSection(title, [line.Trim()]));
    }

    private static string RenderHtml(ReportArrPrintModel model)
    {
        var builder = new StringBuilder();
        builder.AppendLine("<article class=\"reportarr-print-document\">");
        builder.AppendLine($"<header><h1>{Encode(model.DocumentTitle)}</h1>");
        builder.AppendLine($"<p>{Encode(model.TenantDisplayName)} · {Encode(model.ProductDisplayName)} · {Encode(model.SourceDisplayRef)}</p>");
        builder.AppendLine(
            $"<p>{Encode(ResolveStatusBanner(model.DocumentStatus))} · Template {Encode(model.TemplateKey)} v{Encode(model.TemplateVersion)} · Generated {Encode(FormatDateTime(model.GeneratedAtUtc))} by {Encode(model.GeneratedByDisplayName)}</p></header>");

        AppendSection(builder, "Print summary", model.SummaryLines);
        foreach (var section in model.Sections)
        {
            AppendSection(builder, section.Title, section.Lines);
        }

        AppendSection(builder, "Warnings", model.Warnings);
        AppendSection(builder, "Missing requirements", model.MissingRequirements);
        builder.AppendLine("</article>");
        return builder.ToString();
    }

    private static void AppendSection(
        StringBuilder builder,
        string title,
        IEnumerable<string> lines)
    {
        var normalized = lines
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();
        if (normalized.Length == 0)
        {
            return;
        }

        builder.AppendLine($"<section><h2>{Encode(title)}</h2><ul>");
        foreach (var line in normalized)
        {
            builder.AppendLine($"<li>{Encode(line)}</li>");
        }

        builder.AppendLine("</ul></section>");
    }

    private static string RequireSourceEntityId(StlPrintDocumentRequest request, string label)
    {
        if (!string.IsNullOrWhiteSpace(request.SourceEntityId))
        {
            return request.SourceEntityId.Trim();
        }

        throw new StlApiException(
            "print.invalid_request",
            $"ReportArr {label} print requests require a sourceEntityId.",
            400);
    }

    private static Dictionary<string, JsonElement>? ParseOptions(string? optionsJson)
    {
        if (string.IsNullOrWhiteSpace(optionsJson))
        {
            return null;
        }

        using var document = JsonDocument.Parse(optionsJson);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return document.RootElement.EnumerateObject()
            .ToDictionary(property => property.Name, property => property.Value.Clone(), StringComparer.OrdinalIgnoreCase);
    }

    private static string? ResolveOptionValue(Dictionary<string, JsonElement>? options, string key)
    {
        if (options is null || !options.TryGetValue(key, out var value))
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
    }

    private static string ResolveGeneratedByDisplayName(
        ClaimsPrincipal principal,
        Dictionary<string, JsonElement>? options)
    {
        var explicitDisplayName = ResolveOptionValue(options, "actorDisplayName");
        if (!string.IsNullOrWhiteSpace(explicitDisplayName))
        {
            return explicitDisplayName.Trim();
        }

        var displayName = principal.Identity?.Name
            ?? principal.FindFirstValue(JwtRegisteredClaimNames.Name)
            ?? principal.FindFirstValue(ClaimTypes.Name);
        return string.IsNullOrWhiteSpace(displayName) ? "Authorized user" : displayName.Trim();
    }

    private static string ResolveActorLabel(StlPrintProviderContext context, string? actorPersonId)
    {
        if (!string.IsNullOrWhiteSpace(actorPersonId)
            && Guid.TryParse(actorPersonId, out var actorGuid)
            && actorGuid == context.RequestedByPersonId)
        {
            return ResolveGeneratedByDisplayName(context.Principal, options: null);
        }

        return string.IsNullOrWhiteSpace(actorPersonId) ? "System" : "Authorized user";
    }

    private static string ResolveStatusBanner(string documentStatus) =>
        documentStatus switch
        {
            StlPrintDocumentStatuses.Official => "Official copy",
            StlPrintDocumentStatuses.Copy => "Copy",
            StlPrintDocumentStatuses.Redacted => "Redacted copy",
            StlPrintDocumentStatuses.Draft => "Draft",
            _ => "Working copy",
        };

    private static string BuildFileName(string sourceDisplayRef, string templateKey)
    {
        var stem = Regex.Replace(sourceDisplayRef.Trim().ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
        var suffix = templateKey[(templateKey.LastIndexOf('.') + 1)..].Replace('_', '-');
        return $"{stem}-{suffix}.pdf";
    }

    private static string HumanizeToken(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "n/a";
        }

        var normalized = value.Trim().Replace('_', ' ').Replace('-', ' ');
        normalized = Regex.Replace(normalized, "\\s+", " ");
        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(normalized.ToLowerInvariant());
    }

    private static string FormatDateTime(DateTimeOffset? value) =>
        value.HasValue ? value.Value.ToString("yyyy-MM-dd HH:mm 'UTC'", CultureInfo.InvariantCulture) : "Not set";

    private static string DescribeDateRange(DateTimeOffset? start, DateTimeOffset? end)
    {
        if (!start.HasValue && !end.HasValue)
        {
            return "Not set";
        }

        return $"{FormatDateTime(start)} to {FormatDateTime(end)}";
    }

    private static string DefaultText(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "none" : value.Trim();

    private static bool IsClearSummary(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var normalized = value.Trim();
        return normalized.Equals("No missing evidence.", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("No invalid evidence.", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("none", StringComparison.OrdinalIgnoreCase);
    }

    private static string Encode(string value) =>
        System.Net.WebUtility.HtmlEncode(value);

    private sealed record ReportArrPrintModel(
        string TenantDisplayName,
        string ProductDisplayName,
        string SourceDisplayRef,
        string DocumentTitle,
        string TemplateKey,
        string TemplateVersion,
        string DocumentStatus,
        DateTimeOffset GeneratedAtUtc,
        string GeneratedByDisplayName,
        string DocumentClass,
        string DocumentType,
        string DocumentSubtype,
        IReadOnlyList<string> SummaryLines,
        IReadOnlyList<ReportArrPrintSection> Sections,
        IReadOnlyList<string> Warnings,
        IReadOnlyList<string> MissingRequirements);

    private sealed record ReportArrPrintSection(
        string Title,
        IReadOnlyList<string> Lines);
}
