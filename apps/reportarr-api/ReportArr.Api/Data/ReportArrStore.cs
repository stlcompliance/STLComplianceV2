using System.Security.Claims;
using ReportArr.Api.Endpoints;
using ReportArr.Api.Models;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace ReportArr.Api.Data;

public sealed class ReportArrStore
{
    private static readonly HashSet<string> DatasetTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "source_product",
        "cross_product",
        "compliance",
        "operational",
        "audit",
        "executive",
        "custom"
    };

    private static readonly HashSet<string> DashboardTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "executive",
        "product",
        "compliance",
        "maintenance",
        "training",
        "workforce",
        "inventory",
        "procurement",
        "transportation",
        "customer",
        "order",
        "quality",
        "mobile",
        "audit",
        "custom"
    };

    private static readonly HashSet<string> ReportTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "operational",
        "compliance",
        "audit",
        "executive",
        "exception",
        "scheduled",
        "management_review",
        "customer",
        "supplier",
        "product",
        "custom"
    };

    private static readonly HashSet<string> ReportStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "draft",
        "active",
        "paused",
        "archived"
    };

    private static readonly HashSet<string> ScheduleStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "active",
        "paused",
        "canceled",
        "expired"
    };

    private static readonly HashSet<string> ScheduleCadences = new(StringComparer.OrdinalIgnoreCase)
    {
        "hourly",
        "daily",
        "weekly",
        "monthly",
        "quarterly",
        "annually",
        "custom_cron"
    };

    private static readonly HashSet<string> DeliveryMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "email",
        "recordarr_package",
        "dashboard_notification",
        "webhook",
        "download_only"
    };

    private static readonly HashSet<string> ExportTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "report",
        "dashboard",
        "table",
        "dataset",
        "audit_package",
        "chart",
        "custom"
    };

    private static readonly HashSet<string> ExportFormats = new(StringComparer.OrdinalIgnoreCase)
    {
        "pdf",
        "csv",
        "xlsx",
        "json",
        "html",
        "png",
        "zip"
    };

    private static readonly HashSet<string> AlertStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "active",
        "triggered",
        "acknowledged",
        "resolved",
        "muted",
        "archived"
    };

    private readonly object _gate = new();
    private readonly List<ReportArrDatasetResponse> _datasets;
    private readonly List<ReportArrDatasetFieldResponse> _datasetFields;
    private readonly List<ReportArrSourceConnectorResponse> _sourceConnectors;
    private readonly List<ReportArrIngestionCursorResponse> _ingestionCursors;
    private readonly List<ReportArrSourceEventReceiptResponse> _sourceEvents;
    private readonly List<ReportArrReadModelResponse> _readModels;
    private readonly List<ReportArrReadModelRecordResponse> _readModelRecords;
    private readonly List<ReportArrDatasetLineageResponse> _datasetLineage;
    private readonly List<ReportArrDashboardResponse> _dashboards;
    private readonly List<ReportArrDashboardAccessPolicyResponse> _dashboardAccessPolicies;
    private readonly List<ReportArrDashboardFilterResponse> _dashboardFilters;
    private readonly List<ReportArrDrilldownDefinitionResponse> _drilldowns;
    private readonly List<ReportArrDashboardWidgetResponse> _widgets;
    private readonly List<ReportArrWidgetVisualizationSettingsResponse> _widgetVisualizations;
    private readonly List<ReportArrReportDefinitionResponse> _reportDefinitions;
    private readonly List<ReportArrReportAccessPolicyResponse> _reportAccessPolicies;
    private readonly List<ReportArrReportParameterResponse> _reportParameters;
    private readonly List<ReportArrReportSectionResponse> _reportSections;
    private readonly List<ReportArrReportRunResponse> _reportRuns;
    private readonly List<ReportArrReportScheduleResponse> _reportSchedules;
    private readonly List<ReportArrReportRecipientResponse> _reportRecipients;
    private readonly List<ReportArrExportJobResponse> _exportJobs;
    private readonly List<ReportArrMetricDefinitionResponse> _metrics;
    private readonly List<ReportArrMetricValueResponse> _metricValues;
    private readonly List<ReportArrAnalyticsSnapshotResponse> _analyticsSnapshots;
    private readonly List<ReportArrTrendAnalysisResponse> _trendAnalyses;
    private readonly List<ReportArrExceptionQueryResponse> _exceptionQueries;
    private readonly List<ReportArrExceptionResultResponse> _exceptionResults;
    private readonly List<ReportArrKpiDefinitionResponse> _kpis;
    private readonly List<ReportArrKpiValueResponse> _kpiValues;
    private readonly List<ReportArrAlertResponse> _alerts;
    private readonly List<ReportArrAuditScopeResponse> _auditScopes;
    private readonly List<ReportArrAuditPackageResponse> _auditPackages;
    private readonly List<ReportArrRefreshJobResponse> _refreshJobs;

    public ReportArrStore()
    {
        var now = DateTimeOffset.UtcNow;
        (_datasets, _datasetFields, _sourceConnectors, _ingestionCursors, _sourceEvents, _readModels, _readModelRecords, _datasetLineage, _dashboards, _dashboardAccessPolicies, _dashboardFilters, _drilldowns, _widgets, _widgetVisualizations, _reportDefinitions, _reportAccessPolicies, _reportParameters, _reportSections, _reportRuns, _reportSchedules, _reportRecipients, _exportJobs, _metrics, _metricValues, _analyticsSnapshots, _trendAnalyses, _exceptionQueries, _exceptionResults, _kpis, _kpiValues, _alerts, _auditScopes, _auditPackages, _refreshJobs)
            = ([], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], []);

        if (ShouldSeedDemoData())
        {

        _datasets =
        [
            new("ds-001", "DS-260604-001", "ops-executive",
                "Executive operations snapshot",
                "Cross-suite operational summary for executive review.",
                "executive",
                "active",
                "hybrid",
                "daily",
                "fresh",
                ["staffarr", "maintainarr", "loadarr", "routarr", "supplyarr", "trainarr", "compliancecore", "assurarr"],
                ["nexarr-manifest", "reportarr-refresh"],
                now.AddMinutes(-18),
                now.AddMinutes(-14),
                null,
                "v1",
                ["openCriticalBlockers", "freshnessStatus"],
                "source-traceability-required",
                "retain-7-years",
                "person-analytics-lead",
                now.AddDays(-8),
                now.AddMinutes(-4)),
            new("ds-002", "DS-260604-002", "compliance-posture",
                "Compliance posture dataset",
                "Combines evidence, rulepack, and source-product facts.",
                "compliance",
                "active",
                "event_driven",
                "hourly",
                "slightly_stale",
                ["compliancecore", "recordarr", "assurarr"],
                ["compliancecore-events"],
                now.AddHours(-2),
                now.AddHours(-2),
                null,
                "v2",
                ["missingEvidenceCount", "rulepackStatus"],
                "source-traceability-required",
                "retain-7-years",
                "person-compliance-reporter",
                now.AddDays(-15),
                now.AddHours(-2)),
            new("ds-003", "DS-260604-003", "transportation-performance",
                "Transportation performance dataset",
                "Dispatch, ETA, proof, and exception analytics.",
                "operational",
                "active",
                "scheduled",
                "daily",
                "fresh",
                ["routarr", "loadarr", "staffarr"],
                ["routarr-events"],
                now.AddMinutes(-52),
                now.AddMinutes(-52),
                null,
                "v1",
                ["onTimeArrivalRate", "exceptionCount"],
                "source-traceability-required",
                "retain-3-years",
                "person-ops-analyst",
                now.AddDays(-11),
                now.AddMinutes(-49)),
        ];

        _datasetFields =
        [
            new("dsf-001", "ds-001", "openCriticalBlockers", "Open critical blockers", "Open critical actions across products", "number", "compliancecore", "evaluations.blockers.critical", true, true, true, true, false, false, true),
            new("dsf-002", "ds-001", "freshnessStatus", "Freshness status", "Latest freshness status", "enum", "reportarr", "dataset.freshnessStatus", false, true, true, true, false, false, true),
            new("dsf-003", "ds-002", "missingEvidenceCount", "Missing evidence count", "Evidence items missing from packages", "number", "compliancecore", "evaluations.missingEvidence", true, true, true, true, false, false, true),
            new("dsf-004", "ds-003", "onTimeArrivalRate", "On-time arrival rate", "Arrival performance percentage", "percentage", "routarr", "trips.onTimeArrivalRate", true, true, true, true, false, false, false),
        ];

        _sourceConnectors =
        [
            new("src-001", "compliancecore", "event_stream", "active", "svc-compliancecore-events", now.AddMinutes(-13), null, null, ["rule_evaluation.created", "rule_evaluation.updated"], ["ds-002"]),
            new("src-002", "routarr", "api_poll", "active", "svc-routarr-read", now.AddMinutes(-30), null, null, ["trip.completed", "trip.exception"], ["ds-003"]),
            new("src-003", "loadarr", "batch_import", "paused", "svc-loadarr-batch", now.AddHours(-4), now.AddHours(-3), "Temporary upstream maintenance.", ["inventory.balance.updated"], ["ds-001"]),
        ];

        _ingestionCursors =
        [
            new("ing-001", "src-001", "compliancecore", "event_offset", "182455", "evt-9981", now.AddMinutes(-12), now.AddMinutes(-12), "active"),
            new("ing-002", "src-002", "routarr", "timestamp", now.AddMinutes(-25).ToString("O"), "evt-7781", now.AddMinutes(-25), now.AddMinutes(-24), "active"),
            new("ing-003", "src-003", "loadarr", "page_token", "page-11", null, null, now.AddHours(-4), "paused"),
        ];

        _sourceEvents =
        [
            new("evt-1001", "compliancecore", "evt-1001", "evaluation.updated", "eval-7781", now.AddMinutes(-16), now.AddMinutes(-15), "processed", null, "corr-1001"),
            new("evt-1002", "routarr", "evt-1002", "trip.exception", "trip-7781", now.AddMinutes(-31), now.AddMinutes(-30), "processed", null, "corr-1002"),
            new("evt-1003", "loadarr", "evt-1003", "inventory.balance.updated", "bal-551", now.AddHours(-4), null, "failed", "Schema mismatch on field 'quantityAvailable'.", "corr-1003"),
        ];

        _readModels =
        [
            new("rm-001", "RM-260604-001", "executive-operational-posture", "Executive operational posture", "Executive scorecard of cross-product posture.", "scorecard", "active", "Executive", "reportarr", ["ds-001", "ds-002"], ["dataset-freshness", "kpi-scorecard"], ["ref-001", "ref-002"], now.AddMinutes(-14), now.AddMinutes(-4), now.AddMinutes(-4)),
            new("rm-002", "RM-260604-002", "dispatch-performance", "Dispatch performance", "Transportation performance timeline.", "timeline", "active", "Trip", "routarr", ["ds-003"], ["trip-status", "trip-exceptions"], ["ref-003"], now.AddMinutes(-53), now.AddMinutes(-49), now.AddMinutes(-49)),
            new("rm-003", "RM-260604-003", "compliance-audit-matrix", "Compliance audit matrix", "Audit matrix of evidence status.", "audit_matrix", "stale", "Compliance Item", "compliancecore", ["ds-002"], ["evidence-matrix", "rulepack-status"], ["ref-004"], now.AddHours(-2), now.AddHours(-2), now.AddHours(-2)),
        ];

        _readModelRecords =
        [
            new("rmr-001", "rm-001", "obj-exec-001", """{"readinessScore":94.3,"openBlockers":2}""", ["ds-001@evt-1001", "ds-002@evt-1002"], "fresh", now.AddMinutes(-20), now.AddMinutes(-16), now.AddMinutes(-18), now.AddMinutes(-15)),
            new("rmr-002", "rm-002", "trip-7781", """{"onTime":true,"etaDeltaMinutes":-3}""", ["ds-003@evt-1002"], "current", now.AddMinutes(-50), now.AddMinutes(-48), now.AddMinutes(-49), now.AddMinutes(-47)),
            new("rmr-003", "rm-003", "eval-7781", """{"missingEvidenceCount":1,"invalidEvidenceCount":0}""", ["ds-002@evt-1001"], "stale", now.AddHours(-2), now.AddHours(-2), now.AddHours(-2), now.AddHours(-1)),
        ];

        _datasetLineage =
        [
            new("lin-001", "ds-001", "compliancecore", "evaluation", "evaluations.blockers.critical", "openCriticalBlockers", "Aggregate critical blocker counts across evaluation records.", "high"),
            new("lin-002", "ds-002", "compliancecore", "evaluation", "evaluations.missingEvidence", "missingEvidenceCount", "Summarize evidence gaps from compliance evaluations.", "high"),
            new("lin-003", "ds-003", "routarr", "trip", "trips.onTimeArrivalRate", "onTimeArrivalRate", "Calculate arrival performance from trip outcomes.", "high"),
        ];

        _dashboards =
        [
            new("dash-001", "DASH-260604-001", "executive-command", "Executive command", "Overview for leadership.", "executive", "active", "person-exec-lead", "last_30_days", "fresh", ["wid-001", "wid-002", "wid-003"], ["filt-001", "filt-002"], ["drl-001"], "dash-pol-001", now.AddMinutes(-10), now.AddDays(-8), "person-exec-lead", now, "person-exec-lead"),
            new("dash-002", "DASH-260604-002", "compliance-watch", "Compliance watch", "Rulepack posture and missing evidence.", "compliance", "active", "person-compliance-reporter", "last_14_days", "slightly_stale", ["wid-004", "wid-005"], ["filt-003"], ["drl-002"], "dash-pol-002", now.AddHours(-1), now.AddDays(-12), "person-compliance-reporter", now.AddMinutes(-2), "person-compliance-reporter"),
            new("dash-003", "DASH-260604-003", "transportation-board", "Transportation board", "ETA and exception performance.", "transportation", "active", "person-dispatch-lead", "last_7_days", "fresh", ["wid-006"], ["filt-004"], ["drl-003"], "dash-pol-003", null, now.AddDays(-9), "person-dispatch-lead", now.AddMinutes(-15), "person-dispatch-lead"),
        ];

        _dashboardAccessPolicies =
        [
            new("dash-pol-001", "dash-001", "tenant_wide", ["person-exec-lead"], ["executive"], ["reportarr.dashboards.read"], ["reportarr"], true, now.AddDays(-8), now),
            new("dash-pol-002", "dash-002", "role_based", ["person-compliance-reporter"], ["compliance"], ["reportarr.dashboards.read", "reportarr.dashboards.update"], ["reportarr", "compliancecore"], true, now.AddDays(-12), now.AddMinutes(-2)),
            new("dash-pol-003", "dash-003", "product_based", ["person-dispatch-lead"], ["transportation"], ["reportarr.dashboards.read"], ["reportarr", "routarr"], true, now.AddDays(-9), now.AddMinutes(-15)),
        ];

        _dashboardFilters =
        [
            new("filt-001", "dash-001", "dateRange", "Date range", "date_range", "dataset.freshnessStatus", "last_30_days", "dashboard.timeRange", true, true),
            new("filt-002", "dash-001", "product", "Product", "product", "sourceProducts", "all", "productCatalog", false, true),
            new("filt-003", "dash-002", "severity", "Severity", "severity", "alert.severity", "high", "alert.severityCatalog", true, true),
            new("filt-004", "dash-003", "routeStatus", "Route status", "status", "trip.status", "active", "trip.statusCatalog", false, true),
        ];

        _drilldowns =
        [
            new("drl-001", "dash-001", "Open executive details", "Drill into the executive scorecard source report.", "wid-001", "report", "rpt-001", ["period=selectedRange"], ["reportarr.reports.read"], "active"),
            new("drl-002", "dash-002", "Review compliance package", "Open the audit-ready compliance report.", "wid-004", "report", "rpt-002", ["scope=compliance"], ["reportarr.reports.read", "reportarr.reports.update"], "active"),
            new("drl-003", "dash-003", "Inspect route performance", "Jump to the dispatch performance pack.", "wid-006", "report", "rpt-003", ["routeId=selectedRoute"], ["reportarr.reports.read"], "active"),
        ];

        _widgets =
        [
            new("wid-001", "dash-001", "w-open-critical", "Open critical blockers", "Critical blockers across products.", "metric", "active", "ds-001", "rm-001", "count(open blockers)", ["status != closed"], "rpt-001", 1, "grid:1x1", "number:openCriticalBlockers", "fresh", now.AddMinutes(-8)),
            new("wid-002", "dash-001", "w-compliance-trend", "Compliance posture trend", "Trend over the last 30 days.", "trend", "active", "ds-002", "rm-003", "trend(missing evidence)", ["severity = high"], "rpt-002", 2, "grid:2x1", "line:missingEvidence", "slightly_stale", now.AddMinutes(-4)),
            new("wid-003", "dash-001", "w-audit-ready", "Audit readiness", "Readiness score by source product.", "gauge", "active", "ds-002", "rm-003", "score(readiness)", ["rulepack = compliance"], "rpt-002", 3, "grid:1x1", "gauge:readiness", "fresh", now.AddMinutes(-3)),
            new("wid-004", "dash-002", "w-missing-evidence", "Missing evidence", "Open evidence gaps.", "exception_list", "active", "ds-002", "rm-003", "exceptions(missing evidence)", ["severity = critical"], "rpt-002", 1, "grid:2x1", "table:missingEvidence", "slightly_stale", now.AddHours(-1)),
            new("wid-005", "dash-002", "w-audit-matrix", "Audit matrix", "Evidence matrix by rule.", "table", "active", "ds-002", "rm-003", "matrix(rulepack,evidence)", ["rulepack = active"], "rpt-002", 2, "grid:2x2", "table:auditMatrix", "stale", now.AddHours(-1)),
            new("wid-006", "dash-003", "w-arrivals", "On-time arrivals", "Arrival rate and variance.", "trend", "active", "ds-003", "rm-002", "trend(on-time arrivals)", ["tripStatus = active"], "rpt-003", 1, "grid:2x1", "area:onTimeArrivals", "fresh", now.AddMinutes(-14)),
        ];

        _widgetVisualizations =
        [
            new("wid-001", "number", null, null, null, null, "value", "label", null, ["rule-green"], ["threshold-open-critical"], "percentage", false, false, 5),
            new("wid-002", "line", "date", "value", "series", null, "value", "label", "date", ["rule-blue"], ["threshold-freshness"], "percentage", true, false, 30),
            new("wid-003", "gauge", null, null, null, null, "value", "label", null, ["rule-cyan"], ["threshold-readiness"], "score", true, true, 5),
            new("wid-004", "table", null, null, null, "group", "value", "label", null, ["rule-red"], ["threshold-evidence"], "number", true, false, 20),
            new("wid-005", "heatmap", "date", "value", null, "group", "value", "label", "date", ["rule-amber"], ["threshold-audit"], "number", true, false, 25),
            new("wid-006", "area", "date", "value", "series", null, "value", "label", "date", ["rule-green"], ["threshold-arrival"], "percentage", true, false, 20),
        ];

        _reportDefinitions =
        [
            new("rpt-001", "RPT-260604-001", "executive-summary", "Executive summary", "Operational summary for leadership.", "executive", "active", ["ds-001"], ["rm-001"], ["param-001"], ["date_range:last_30_days"], "layout:grid:2x2", ["sec-001", "sec-002"], ["pdf", "xlsx", "html"], "rpt-pol-001", "person-exec-lead", now.AddDays(-7), "person-exec-lead", now, "person-exec-lead"),
            new("rpt-002", "RPT-260604-002", "compliance-audit-readiness", "Compliance audit readiness", "Audit-ready posture and gaps.", "audit", "active", ["ds-002"], ["rm-003"], ["param-002"], ["date_range:last_14_days"], "layout:grid:1x3", ["sec-003"], ["pdf", "xlsx", "json"], "rpt-pol-002", "person-compliance-reporter", now.AddDays(-12), "person-compliance-reporter", now.AddMinutes(-4), "person-compliance-reporter"),
            new("rpt-003", "RPT-260604-003", "dispatch-performance-pack", "Dispatch performance pack", "Transportation KPIs and exceptions.", "operational", "active", ["ds-003"], ["rm-002"], ["param-003"], ["date_range:last_7_days"], "layout:split:detail", ["sec-004"], ["pdf", "csv", "html"], "rpt-pol-003", "person-dispatch-lead", now.AddDays(-9), "person-dispatch-lead", now.AddMinutes(-10), "person-dispatch-lead"),
        ];

        _reportAccessPolicies =
        [
            new("rpt-pol-001", "rpt-001", "tenant_wide", ["person-exec-lead"], ["executive"], ["reportarr.reports.read"], ["reportarr"], true, true, false, now.AddDays(-7), now),
            new("rpt-pol-002", "rpt-002", "role_based", ["person-compliance-reporter"], ["compliance"], ["reportarr.reports.read", "reportarr.reports.update"], ["reportarr", "compliancecore"], true, true, true, now.AddDays(-12), now.AddMinutes(-4)),
            new("rpt-pol-003", "rpt-003", "product_based", ["person-dispatch-lead"], ["transportation"], ["reportarr.reports.read"], ["reportarr", "routarr"], true, true, false, now.AddDays(-9), now.AddMinutes(-10)),
        ];

        _reportParameters =
        [
            new("param-001", "rpt-001", "period", "Period", "date_range", true, "last_30_days", "dateRange", "must be a supported range"),
            new("param-002", "rpt-002", "scope", "Scope", "single_select", true, "site", "compliance scope", "must be a valid scope"),
            new("param-003", "rpt-003", "includeExceptions", "Include exceptions", "boolean", false, "true", "report flags", "boolean only"),
        ];

        _reportSections =
        [
            new("sec-001", "rpt-001", 1, "Summary", "Top-level health indicators.", "summary", "ds-001", "aggregate latest metrics", "layout:grid:2x2"),
            new("sec-002", "rpt-001", 2, "Notes", "Executive commentary.", "narrative", "ds-001", "render narrative summary", "layout:full-width:narrative"),
            new("sec-003", "rpt-002", 1, "Evidence gaps", "Missing and invalid evidence.", "evidence_matrix", "ds-002", "group by rulepack", "layout:grid:1x3"),
            new("sec-004", "rpt-003", 1, "Route performance", "On-time and exceptions.", "chart", "ds-003", "group by route", "layout:split:detail"),
        ];

        _reportRuns =
        [
            new("run-001", "RUN-260604-001", "rpt-001", "Executive summary", "completed", "person-exec-lead", now.AddMinutes(-42), now.AddMinutes(-42), now.AddMinutes(-39), ["period=last_30_days"], ["dateRange:last_30_days"], "pdf", "recordarr://pkg-reportarr-001", "pkg-reportarr-001", 181, 0, "exp-001", 0, null, "fresh", "dataset ds-001 and ds-002", "All source traces aligned."),
            new("run-002", "RUN-260604-002", "rpt-002", "Compliance audit readiness", "completed_with_warnings", "person-compliance-reporter", now.AddMinutes(-18), now.AddMinutes(-18), now.AddMinutes(-15), ["scope=site"], ["dateRange:last_14_days"], "xlsx", null, "pkg-reportarr-002", 88, 2, "exp-002", 1, "One source feed was slightly stale.", "slightly_stale", "dataset ds-002 contains stale widget refs", "One source trace is stale."),
            new("run-003", "RUN-260604-003", "rpt-003", "Dispatch performance pack", "running", "person-dispatch-lead", now.AddMinutes(-4), now.AddMinutes(-4), null, ["includeExceptions=true"], ["dateRange:last_7_days"], "html", null, null, 0, 0, null, 0, null, "fresh", "dataset ds-003", "Source trace is current."),
        ];

        _reportSchedules =
        [
            new("sch-001", "SCH-260604-001", "rpt-001", "Executive summary weekly", "active", "weekly", "America/Chicago", null, now.AddDays(3), now.AddDays(-4), now.AddDays(-15), null, ["period=last_30_days"], ["person-exec-lead", "person-cfo"], "email", "person-exec-lead", now.AddDays(-15), now),
            new("sch-002", "SCH-260604-002", "rpt-002", "Compliance audit daily", "paused", "daily", "America/Chicago", null, now.AddHours(20), now.AddHours(-2), now.AddDays(-18), null, ["scope=site"], ["person-compliance-reporter"], "webhook", "person-compliance-reporter", now.AddDays(-18), now.AddMinutes(-1)),
        ];

        _reportRecipients =
        [
            new("rec-001", "sch-001", "person", "person-exec-lead", "exec@example.test", "pdf", "active"),
            new("rec-002", "sch-001", "person", "person-cfo", "cfo@example.test", "xlsx", "active"),
            new("rec-003", "sch-002", "person", "person-compliance-reporter", "reporter@example.test", "pdf", "active"),
        ];

        _exportJobs =
        [
            new("exp-001", "EXP-260604-001", "run-001", "Executive summary PDF", "completed", "report", "pdf", "person-exec-lead", now.AddMinutes(-42), now.AddMinutes(-42), now.AddMinutes(-39), "report-run:run-001", "recordarr://pkg-reportarr-001", 181, 23168, now.AddDays(30), null, now.AddMinutes(-39), now.AddMinutes(-39), "pkg-001"),
            new("exp-002", "EXP-260604-002", "run-002", "Compliance audit XLSX", "completed", "report", "xlsx", "person-compliance-reporter", now.AddMinutes(-18), now.AddMinutes(-18), now.AddMinutes(-15), "report-run:run-002", "recordarr://pkg-reportarr-002", 88, 11264, now.AddDays(30), null, now.AddMinutes(-15), now.AddMinutes(-15), "pkg-002"),
        ];

        _metrics =
        [
            new("met-001", "active-blockers", "Active blockers", "Open blockers across the suite.", "count", "ds-001", ["openCriticalBlockers"], "count(open blockers)", "status != closed", "site, product", "timestamp", "active"),
            new("met-002", "missing-evidence", "Missing evidence", "Missing evidence count.", "count", "ds-002", ["missingEvidenceCount"], "count(gaps)", "severity = high", "site, rulepack", "timestamp", "active"),
        ];

        _metricValues =
        [
            new("mv-001", "met-001", now.AddDays(-1), now, 14, null, null, "dataset ds-001 via routarr/loadarr", now.AddMinutes(-8)),
            new("mv-002", "met-002", now.AddDays(-1), now, 7, null, null, "dataset ds-002 via compliancecore", now.AddMinutes(-7)),
        ];

        _analyticsSnapshots =
        [
            new("snap-001", "AN-260604-001", "daily", "completed", now.AddDays(-1), now, ["ds-001", "ds-002"], ["kpi-001"], ["mv-001", "mv-002"], now.AddMinutes(-7), "system"),
            new("snap-002", "AN-260604-002", "manual", "created", now.AddDays(-7), now.AddDays(-1), ["ds-003"], ["kpi-002"], ["mv-002"], now.AddHours(-5), "person"),
        ];

        _trendAnalyses =
        [
            new("trend-001", "met-001", "kpi-002", now.AddDays(-7), now, "improving", 2.4m, 0.025m, "high", "Dispatch performance is trending upward.", now.AddMinutes(-5)),
            new("trend-002", "met-002", "kpi-001", now.AddDays(-7), now, "stable", 0.2m, 0.003m, "medium", "Compliance posture remains steady.", now.AddMinutes(-4)),
        ];

        _exceptionQueries =
        [
            new("exq-001", "overdue_potential", "Potential overdue items", "Find records approaching overdue status.", "ds-001", "status = pending and dueDate < now()+7d", "high", "active", "person-analytics-lead"),
            new("exq-002", "missing_evidence", "Missing evidence", "Find records missing required evidence.", "ds-002", "evidenceCount = 0", "critical", "active", "person-compliance-reporter"),
        ];

        _exceptionResults =
        [
            new("exr-001", "exq-001", "trip-7781", "Route proof overdue", "Trip proof items are approaching overdue.", "high", "open", now.AddHours(-5), null, null, null, "dataset ds-001 indicates proof gap"),
            new("exr-002", "exq-002", "eval-7781", "Evidence missing", "Compliance evidence is missing for the record.", "critical", "acknowledged", now.AddHours(-3), "person-compliance-reporter", now.AddHours(-2), null, "dataset ds-002 evidence count is zero"),
        ];

        _kpis =
        [
            new("kpi-001", "KPI-260604-001", "audit-readiness-score", "Audit readiness score", "Weighted average of evidence and posture.", "compliance", "active", "weighted_mean(readiness)", ["ds-002"], ["met-002"], 92m, 85m, 70m, true, "percentage", "person-compliance-reporter", now.AddDays(-20), now),
            new("kpi-002", "KPI-260604-002", "dispatch-on-time-rate", "Dispatch on-time rate", "On-time transportation performance.", "transportation", "active", "avg(on_time)", ["ds-003"], ["met-001"], 96m, 90m, 80m, true, "percentage", "person-dispatch-lead", now.AddDays(-18), now),
        ];

        _kpiValues =
        [
            new("kval-001", "kpi-001", now.AddDays(-1), now, 91.4m, 92m, 85m, 70m, "warning", "stable", "dataset ds-002 via compliancecore and reportarr", now.AddMinutes(-7)),
            new("kval-002", "kpi-002", now.AddDays(-1), now, 97.2m, 96m, 90m, 80m, "good", "improving", "dataset ds-003 via routarr", now.AddMinutes(-6)),
        ];

        _alerts =
        [
            new("alrt-001", "ALRT-260604-001", "Stale compliance dataset", "Compliance posture dataset is slightly stale.", "stale_data", "triggered", "ds-002", "met-002", "freshness <= threshold", "moderate", now.AddHours(-1), null, null, null, ["notif-001"]),
            new("alrt-002", "ALRT-260604-002", "Failed inventory refresh", "LoadArr import failed during nightly sync.", "failed_refresh", "acknowledged", "ds-001", "met-001", "refresh status = failed", "high", now.AddHours(-4), "person-analytics-lead", now.AddHours(-3), null, ["notif-002"]),
        ];

        _auditScopes =
        [
            new("scope-001", "rulepack", now.AddDays(-30), now, ["reportarr", "compliancecore"], ["obj-001", "obj-002"], ["rp-001"], ["site-01"], ["dept-01"], true, true),
            new("scope-002", "date_range", now.AddDays(-14), now, ["routarr", "loadarr"], ["trip-7781"], [], [], [], true, true),
            new("scope-003", "custom", null, null, ["reportarr"], ["obj-201"], ["rp-015"], ["site-04"], ["dept-09"], false, true),
        ];

        _auditPackages =
        [
            new("aud-001", "AUD-260604-001", "Quarterly executive audit", "Executive audit readiness across the suite.", "complete", "person-compliance-reporter", _auditScopes[0], ["eval-001", "eval-002"], ["reportarr", "compliancecore", "routarr"], ["obj-001", "obj-002"], "pkg-001", ["run-001"], "Only one minor gap remains.", "No invalid evidence.", 94.3m, now.AddHours(-6), null),
            new("aud-002", "AUD-260604-002", "Transportation readiness review", "Audit pack for dispatch readiness.", "locked", "person-dispatch-lead", _auditScopes[1], ["eval-003"], ["routarr", "loadarr"], ["trip-7781"], "pkg-002", ["run-003"], "No missing evidence.", "No invalid evidence.", 98.5m, now.AddHours(-3), now.AddHours(-1)),
        ];

        _refreshJobs =
        [
            new("ref-001", "ds-001", "rm-001", "full", "completed", "person-analytics-lead", now.AddMinutes(-21), now.AddMinutes(-21), now.AddMinutes(-18), 92, 88, 4, 0, 0, null),
            new("ref-002", "ds-002", "rm-003", "incremental", "running", "person-compliance-reporter", now.AddMinutes(-6), now.AddMinutes(-6), null, 0, 0, 0, 0, 0, null),
        ];
        }
    }

    private static bool ShouldSeedDemoData()
    {
        var explicitFlag = Environment.GetEnvironmentVariable("REPORTARR_ENABLE_DEMO_DATA");
        return bool.TryParse(explicitFlag, out var enabled) && enabled;
    }

    private static string RequireTrimmed(string? value, string fieldName)
    {
        var trimmed = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            throw new StlApiException($"reportarr.{ToLowerCamel(fieldName)}_required", $"{fieldName} is required.", 400);
        }

        return trimmed;
    }

    private static string NormalizeEnumValue(string? value, string fieldName, HashSet<string> allowedValues)
    {
        var normalized = RequireTrimmed(value, fieldName).ToLowerInvariant();
        if (!allowedValues.Contains(normalized))
        {
            throw new StlApiException(
                $"reportarr.{ToLowerCamel(fieldName)}_invalid",
                $"{fieldName} must be one of: {string.Join(", ", allowedValues.OrderBy(item => item, StringComparer.OrdinalIgnoreCase))}.",
                400);
        }

        return normalized;
    }

    private static IReadOnlyList<string> NormalizeSelectionList(IReadOnlyList<string>? values, string fieldName)
    {
        if (values is null || values.Count == 0)
        {
            throw new StlApiException($"reportarr.{ToLowerCamel(fieldName)}_required", $"{fieldName} must contain at least one value.", 400);
        }

        return values
            .Select(item => RequireTrimmed(item, fieldName))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static IReadOnlyList<string> NormalizeStrings(IReadOnlyList<string>? values)
    {
        if (values is null)
        {
            return [];
        }

        return values
            .Select(item => item.Trim())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static void RequireCondition(bool condition, string code, string message)
    {
        if (!condition)
        {
            throw new StlApiException(code, message, 400);
        }
    }

    private static void RequireCondition(bool condition, string code, string message, int statusCode)
    {
        if (!condition)
        {
            throw new StlApiException(code, message, statusCode);
        }
    }

    private static string ToLowerCamel(string fieldName) =>
        string.IsNullOrEmpty(fieldName)
            ? fieldName
            : char.ToLowerInvariant(fieldName[0]) + fieldName[1..];

    public ReportArrSessionBootstrapResponse BuildSession(
        string userId,
        string personId,
        string tenantId,
        string tenantRoleKey,
        bool isPlatformAdmin,
        IReadOnlyList<string> entitlements)
    {
        return new ReportArrSessionBootstrapResponse(
            userId,
            personId,
            tenantId,
            $"session-{tenantId[..8]}",
            tenantRoleKey,
            isPlatformAdmin,
            "reportarr",
            entitlements.Contains("reportarr", StringComparer.OrdinalIgnoreCase),
            entitlements);
    }

    public object GetMe(ClaimsPrincipal principal) => new
    {
        userId = principal.GetUserId().ToString(),
        personId = principal.GetPersonId().ToString(),
        tenantId = principal.GetTenantId().ToString(),
        tenantRoleKey = principal.GetTenantRoleKey(),
        isPlatformAdmin = principal.IsPlatformAdmin(),
        entitlements = principal.GetEntitlements(),
        productKey = "reportarr"
    };

    public ReportArrSummaryResponse GetSummary(ClaimsPrincipal principal)
    {
        lock (_gate)
        {
            var accessibleDashboards = GetAccessibleDashboards(principal);
            var accessibleReports = GetAccessibleReports(principal);
            return new ReportArrSummaryResponse(
                DateTimeOffset.UtcNow,
                ResolveFreshnessStatus(),
                _datasets.Count,
                accessibleDashboards.Count,
                accessibleReports.Count,
                _reportRuns.Count,
                _kpis.Count,
                _alerts.Count,
                _auditPackages.Count,
                _datasets.Take(3).ToList(),
                accessibleDashboards.Take(3).ToList(),
                accessibleReports.Take(3).ToList(),
                _alerts.Take(3).ToList(),
                _auditPackages.Take(3).ToList());
        }
    }

    public IReadOnlyList<ReportArrDatasetResponse> GetDatasets(ClaimsPrincipal principal)
    {
        lock (_gate)
        {
            return _datasets
                .Where(dataset => CanAccessSourceProducts(principal, dataset.SourceProducts))
                .OrderByDescending(dataset => dataset.UpdatedAt)
                .ToList();
        }
    }

    public IReadOnlyList<ReportArrDatasetFieldResponse> GetDatasetFields(ClaimsPrincipal principal)
    {
        lock (_gate)
        {
            return _datasetFields
                .Where(field =>
                {
                    var dataset = _datasets.FirstOrDefault(item => item.DatasetId == field.DatasetId);
                    return dataset is not null && CanAccessSourceProducts(principal, dataset.SourceProducts);
                })
                .ToList();
        }
    }

    public ReportArrDatasetResponse? GetDataset(ClaimsPrincipal principal, string datasetId)
    {
        lock (_gate)
        {
            var dataset = _datasets.FirstOrDefault(item => item.DatasetId == datasetId);
            return dataset is not null && CanAccessSourceProducts(principal, dataset.SourceProducts) ? dataset : null;
        }
    }

    public ReportArrDatasetResponse CreateDataset(ClaimsPrincipal principal, IntegrationEndpoints.CreateDatasetRequest request)
    {
        lock (_gate)
        {
            RequireCanManageDatasets(principal);
            var now = DateTimeOffset.UtcNow;
            var datasetKey = RequireTrimmed(request.DatasetKey, nameof(request.DatasetKey)).ToLowerInvariant();
            var title = RequireTrimmed(request.Title, nameof(request.Title));
            var description = RequireTrimmed(request.Description, nameof(request.Description));
            var datasetType = NormalizeEnumValue(request.DatasetType, nameof(request.DatasetType), DatasetTypes);
            var sourceProducts = NormalizeSelectionList(request.SourceProducts, nameof(request.SourceProducts));
            RequireCondition(sourceProducts.All(product => !string.Equals(product, "reportarr", StringComparison.OrdinalIgnoreCase)),
                "reportarr.dataset_source_product_invalid",
                "ReportArr datasets must reference source products, not ReportArr itself.");

            var dataset = new ReportArrDatasetResponse(
                NextId("ds"),
                NextNumber("DS"),
                datasetKey,
                title,
                description,
                datasetType,
                "draft",
                "manual",
                "manual",
                "unknown",
                sourceProducts,
                ["manual-import"],
                null,
                null,
                null,
                "v1",
                ["openCriticalBlockers", "freshnessStatus"],
                "source-traceability-required",
                "retain-7-years",
                request.OwnerPersonId,
                now,
                now);
            _datasets.Add(dataset);
            return dataset;
        }
    }

    public ReportArrRefreshJobResponse RefreshDataset(ClaimsPrincipal principal, string datasetId, IntegrationEndpoints.RefreshDatasetRequest request)
    {
        lock (_gate)
        {
            RequireCanRefreshDatasets(principal);
            var now = DateTimeOffset.UtcNow;
            var dataset = _datasets.FirstOrDefault(item => item.DatasetId == datasetId)
                ?? throw new InvalidOperationException("Dataset not found.");
            var requestedByPersonId = RequireTrimmed(request.RequestedByPersonId, nameof(request.RequestedByPersonId));

            dataset = dataset with
            {
                FreshnessStatus = "fresh",
                LastRefreshedAt = now,
                LastSuccessfulRefreshAt = now,
                LastFailedRefreshAt = null,
                UpdatedAt = now
            };

            ReplaceDataset(dataset);

            var job = new ReportArrRefreshJobResponse(
                NextId("ref"),
                datasetId,
                null,
                "manual",
                "completed",
                requestedByPersonId,
                now,
                now,
                now,
                100,
                98,
                2,
                0,
                0,
                null);
            _refreshJobs.Add(job);
            return job;
        }
    }

    public IReadOnlyList<ReportArrReadModelResponse> GetReadModels(ClaimsPrincipal principal)
    {
        lock (_gate)
        {
            return _readModels
                .Where(model => CanAccessReadModel(principal, model))
                .OrderByDescending(model => model.UpdatedAt)
                .ToList();
        }
    }

    public ReportArrReadModelResponse? GetReadModel(ClaimsPrincipal principal, string readModelId)
    {
        lock (_gate)
        {
            var model = _readModels.FirstOrDefault(item => item.ReadModelId == readModelId);
            return model is not null && CanAccessReadModel(principal, model) ? model : null;
        }
    }

    public IReadOnlyList<ReportArrReadModelRecordResponse> GetReadModelRecords(ClaimsPrincipal principal)
    {
        lock (_gate)
        {
            return _readModelRecords
                .Where(record =>
                {
                    var model = _readModels.FirstOrDefault(item => item.ReadModelId == record.ReadModelId);
                    return model is not null && CanAccessReadModel(principal, model);
                })
                .OrderByDescending(item => item.UpdatedAt)
                .ToList();
        }
    }

    public IReadOnlyList<ReportArrDatasetLineageResponse> GetDatasetLineage(ClaimsPrincipal principal)
    {
        lock (_gate)
        {
            return _datasetLineage
                .Where(item =>
                {
                    var dataset = _datasets.FirstOrDefault(data => data.DatasetId == item.DatasetId);
                    return dataset is not null && CanAccessSourceProducts(principal, dataset.SourceProducts);
                })
                .ToList();
        }
    }

    public ReportArrRefreshJobResponse RebuildReadModel(ClaimsPrincipal principal, string readModelId, IntegrationEndpoints.RebuildReadModelRequest request)
    {
        lock (_gate)
        {
            RequireCanRebuildReadModels(principal);
            var now = DateTimeOffset.UtcNow;
            var model = _readModels.FirstOrDefault(item => item.ReadModelId == readModelId)
                ?? throw new InvalidOperationException("Read model not found.");
            var requestedByPersonId = RequireTrimmed(request.RequestedByPersonId, nameof(request.RequestedByPersonId));

            model = model with
            {
                Status = "active",
                LastRebuiltAt = now,
                LastUpdatedAt = now
            };

            ReplaceReadModel(model);

            var job = new ReportArrRefreshJobResponse(
                NextId("ref"),
                model.DatasetRefs.FirstOrDefault() ?? "ds-001",
                readModelId,
                "rebuild",
                "completed",
                requestedByPersonId,
                now,
                now,
                now,
                120,
                120,
                0,
                0,
                0,
                null);
            _refreshJobs.Add(job);
            return job;
        }
    }

    public IReadOnlyList<ReportArrDashboardResponse> GetDashboards(ClaimsPrincipal principal)
    {
        lock (_gate)
        {
            return GetAccessibleDashboards(principal)
                .OrderByDescending(item => item.UpdatedAt)
                .ToList();
        }
    }

    public ReportArrDashboardResponse? GetDashboard(ClaimsPrincipal principal, string dashboardId)
    {
        lock (_gate)
        {
            var dashboard = _dashboards.FirstOrDefault(item => item.DashboardId == dashboardId);
            if (dashboard is null || !CanAccessDashboard(principal, dashboard))
            {
                return null;
            }

            var viewed = dashboard with
            {
                LastViewedAt = DateTimeOffset.UtcNow
            };
            ReplaceDashboard(viewed);
            return viewed;
        }
    }

    public IReadOnlyList<ReportArrDashboardAccessPolicyResponse> GetDashboardAccessPolicies()
    {
        lock (_gate)
        {
            return _dashboardAccessPolicies.OrderByDescending(item => item.UpdatedAt).ToList();
        }
    }

    public IReadOnlyList<ReportArrDashboardFilterResponse> GetDashboardFilters(ClaimsPrincipal principal)
    {
        lock (_gate)
        {
            return _dashboardFilters
                .Where(filter => CanAccessDashboardById(principal, filter.DashboardId))
                .ToList();
        }
    }

    public IReadOnlyList<ReportArrDrilldownDefinitionResponse> GetDrilldowns(ClaimsPrincipal principal)
    {
        lock (_gate)
        {
            return _drilldowns
                .Where(drilldown => CanAccessDashboardById(principal, drilldown.DashboardId))
                .ToList();
        }
    }

    public ReportArrDashboardResponse CreateDashboard(ClaimsPrincipal principal, IntegrationEndpoints.CreateDashboardRequest request)
    {
        lock (_gate)
        {
            RequireCanBuildReports(principal);
            var now = DateTimeOffset.UtcNow;
            var dashboardKey = RequireTrimmed(request.DashboardKey, nameof(request.DashboardKey)).ToLowerInvariant();
            var title = RequireTrimmed(request.Title, nameof(request.Title));
            var description = RequireTrimmed(request.Description, nameof(request.Description));
            var dashboardType = NormalizeEnumValue(request.DashboardType, nameof(request.DashboardType), DashboardTypes);
            var defaultDateRange = RequireTrimmed(request.DefaultDateRange, nameof(request.DefaultDateRange));
            var ownerPersonId = RequireTrimmed(request.OwnerPersonId, nameof(request.OwnerPersonId));
            var dashboard = new ReportArrDashboardResponse(
                NextId("dash"),
                NextNumber("DASH"),
                dashboardKey,
                title,
                description,
                dashboardType,
                "draft",
                ownerPersonId,
                defaultDateRange,
                "unknown",
                [],
                [],
                [],
                "",
                null,
                now,
                ownerPersonId,
                now,
                ownerPersonId);
            var policyId = NextId("dash-pol");
            _dashboardAccessPolicies.Add(new ReportArrDashboardAccessPolicyResponse(
                policyId,
                dashboard.DashboardId,
                "private",
                [ownerPersonId],
                ["owner"],
                ["reportarr.dashboards.read", "reportarr.dashboards.update"],
                ["reportarr"],
                true,
                now,
                now));
            dashboard = dashboard with { AccessPolicyRef = policyId };
            _dashboards.Add(dashboard);
            return dashboard;
        }
    }

    public ReportArrDashboardResponse UpdateDashboard(ClaimsPrincipal principal, string dashboardId, IntegrationEndpoints.UpdateDashboardRequest request)
    {
        lock (_gate)
        {
            var existing = _dashboards.First(item => item.DashboardId == dashboardId);
            EnsureCanManageDashboard(principal, existing);
            var title = RequireTrimmed(request.Title, nameof(request.Title));
            var description = RequireTrimmed(request.Description, nameof(request.Description));
            var status = NormalizeEnumValue(request.Status, nameof(request.Status), new HashSet<string>(["draft", "active", "paused", "archived"], StringComparer.OrdinalIgnoreCase));
            var defaultDateRange = RequireTrimmed(request.DefaultDateRange, nameof(request.DefaultDateRange));
            var updated = existing with
            {
                Title = title,
                Description = description,
                Status = status,
                DefaultDateRange = defaultDateRange,
                UpdatedAt = DateTimeOffset.UtcNow,
                UpdatedByPersonId = principal.GetPersonId().ToString()
            };
            ReplaceDashboard(updated);
            return updated;
        }
    }

    public ReportArrDashboardWidgetResponse RenderWidget(ClaimsPrincipal principal, string widgetId)
    {
        lock (_gate)
        {
            var widget = _widgets.First(item => item.WidgetId == widgetId);
            var dashboard = _dashboards.FirstOrDefault(item => item.DashboardId == widget.DashboardId)
                ?? throw new InvalidOperationException("Dashboard not found.");
            EnsureCanViewDashboard(principal, dashboard);
            ReplaceDashboard(dashboard with
            {
                LastViewedAt = DateTimeOffset.UtcNow
            });
            var updated = widget with
            {
                LastRenderedAt = DateTimeOffset.UtcNow
            };
            ReplaceWidget(updated);
            return updated;
        }
    }

    public IReadOnlyList<ReportArrReportDefinitionResponse> GetReportDefinitions(ClaimsPrincipal principal)
    {
        lock (_gate)
        {
            return GetAccessibleReports(principal)
                .OrderByDescending(item => item.UpdatedAt)
                .ToList();
        }
    }

    public ReportArrReportDefinitionResponse? GetReportDefinition(ClaimsPrincipal principal, string reportDefinitionId)
    {
        lock (_gate)
        {
            var report = _reportDefinitions.FirstOrDefault(item => item.ReportDefinitionId == reportDefinitionId);
            return report is not null && CanAccessReport(principal, report) ? report : null;
        }
    }

    public IReadOnlyList<ReportArrReportAccessPolicyResponse> GetReportAccessPolicies()
    {
        lock (_gate)
        {
            return _reportAccessPolicies.OrderByDescending(item => item.UpdatedAt).ToList();
        }
    }

    public ReportArrReportDefinitionResponse CreateReportDefinition(ClaimsPrincipal principal, IntegrationEndpoints.CreateReportDefinitionRequest request)
    {
        lock (_gate)
        {
            RequireCanBuildReports(principal);
            var now = DateTimeOffset.UtcNow;
            var reportKey = RequireTrimmed(request.ReportKey, nameof(request.ReportKey)).ToLowerInvariant();
            var title = RequireTrimmed(request.Title, nameof(request.Title));
            var description = RequireTrimmed(request.Description, nameof(request.Description));
            var reportType = NormalizeEnumValue(request.ReportType, nameof(request.ReportType), ReportTypes);
            var layoutDefinition = RequireTrimmed(request.LayoutDefinition, nameof(request.LayoutDefinition));
            var exportFormats = NormalizeSelectionList(request.ExportFormats, nameof(request.ExportFormats))
                .Select(item => NormalizeEnumValue(item, nameof(request.ExportFormats), ExportFormats))
                .ToList();
            var ownerPersonId = RequireTrimmed(request.OwnerPersonId, nameof(request.OwnerPersonId));
            var definition = new ReportArrReportDefinitionResponse(
                NextId("rpt"),
                NextNumber("RPT"),
                reportKey,
                title,
                description,
                reportType,
                "draft",
                [],
                [],
                [],
                [],
                layoutDefinition,
                [],
                exportFormats,
                "",
                ownerPersonId,
                now,
                ownerPersonId,
                now,
                ownerPersonId);
            var policyId = NextId("rpt-pol");
            _reportAccessPolicies.Add(new ReportArrReportAccessPolicyResponse(
                policyId,
                definition.ReportDefinitionId,
                "private",
                [ownerPersonId],
                ["owner"],
                ["reportarr.reports.read", "reportarr.reports.update"],
                ["reportarr"],
                true,
                true,
                false,
                now,
                now));
            definition = definition with { AccessPolicyRef = policyId };
            _reportDefinitions.Add(definition);
            return definition;
        }
    }

    public ReportArrReportDefinitionResponse UpdateReportDefinition(ClaimsPrincipal principal, string reportDefinitionId, IntegrationEndpoints.UpdateReportDefinitionRequest request)
    {
        lock (_gate)
        {
            var existing = _reportDefinitions.First(item => item.ReportDefinitionId == reportDefinitionId);
            EnsureCanManageReport(principal, existing);
            var now = DateTimeOffset.UtcNow;
            var status = NormalizeEnumValue(request.Status, nameof(request.Status), ReportStatuses);
            var requestedByPersonId = RequireTrimmed(request.RequestedByPersonId, nameof(request.RequestedByPersonId));
            var updated = existing with
            {
                Status = status,
                UpdatedAt = now,
                UpdatedByPersonId = requestedByPersonId
            };
            ReplaceReportDefinition(updated);
            return updated;
        }
    }

    public ReportArrReportRunResponse CreateReportRun(ClaimsPrincipal principal, IntegrationEndpoints.CreateReportRunRequest request)
    {
        lock (_gate)
        {
            RequireCanRunReports(principal);
            var now = DateTimeOffset.UtcNow;
            var reportDefinitionId = RequireTrimmed(request.ReportDefinitionId, nameof(request.ReportDefinitionId));
            var definition = _reportDefinitions.First(item => item.ReportDefinitionId == reportDefinitionId);
            EnsureCanViewReport(principal, definition);
            var requestedByPersonId = RequireTrimmed(request.RequestedByPersonId, nameof(request.RequestedByPersonId));
            var exportFormat = string.IsNullOrWhiteSpace(request.ExportFormat)
                ? null
                : NormalizeEnumValue(request.ExportFormat, nameof(request.ExportFormat), ExportFormats);
            if (exportFormat is not null)
            {
                EnsureCanExportReport(principal, definition);
            }
            var parametersUsed = NormalizeStrings(request.ParametersUsed);
            var filtersUsed = NormalizeStrings(request.FiltersUsed);
            var warnings = GetFreshnessWarnings();
            var exportJobId = exportFormat is null ? null : NextId("exp");
            var outputFormat = exportFormat is null ? "html" : exportFormat;
            var run = new ReportArrReportRunResponse(
                NextId("run"),
                NextNumber("RUN"),
                definition.ReportDefinitionId,
                definition.Title,
                warnings > 0 ? "completed_with_warnings" : "completed",
                requestedByPersonId,
                now,
                now,
                now,
                parametersUsed,
                filtersUsed,
                outputFormat,
                exportFormat is null ? null : "recordarr://pkg-reportarr",
                exportFormat is null ? null : "pkg-reportarr",
                100 + warnings,
                warnings,
                exportJobId,
                0,
                null,
                ResolveFreshnessStatus(),
                $"datasets={string.Join(',', definition.DatasetRefs)}",
                warnings > 0 ? "freshness warnings were observed." : "all source traces were current.");
            _reportRuns.Add(run);

            if (request.ExportFormat is not null)
            {
                _exportJobs.Add(new ReportArrExportJobResponse(
                    exportJobId!,
                    NextNumber("EXP"),
                    run.ReportRunId,
                    $"{definition.Title} export",
                    "completed",
                    "report",
                    exportFormat!,
                    requestedByPersonId,
                    now,
                    now,
                    now,
                    $"report-run:{run.ReportRunId}",
                    run.OutputRecordRef,
                    run.RowCount,
                    run.RowCount * 128L,
                    now.AddDays(30),
                    null,
                    now,
                    now,
                    run.OutputPackageRef));
            }

            return run;
        }
    }

    public ReportArrReportRunResponse? GetReportRun(ClaimsPrincipal principal, string reportRunId)
    {
        lock (_gate)
        {
            var run = _reportRuns.FirstOrDefault(item => item.ReportRunId == reportRunId);
            return run is not null && CanAccessReport(principal, run.ReportDefinitionId) ? run : null;
        }
    }

    public IReadOnlyList<ReportArrReportRunResponse> GetReportRuns(ClaimsPrincipal principal)
    {
        lock (_gate)
        {
            return _reportRuns
                .Where(item => CanAccessReport(principal, item.ReportDefinitionId))
                .OrderByDescending(item => item.RequestedAt)
                .ToList();
        }
    }

    public ReportArrReportRunResponse CancelReportRun(ClaimsPrincipal principal, string reportRunId, IntegrationEndpoints.CancelReportRunRequest request)
    {
        lock (_gate)
        {
            var existing = _reportRuns.First(item => item.ReportRunId == reportRunId);
            EnsureCanViewReport(principal, RequireAccessibleReport(principal, existing.ReportDefinitionId));
            var updated = existing with { Status = "canceled" };
            ReplaceReportRun(updated);
            return updated;
        }
    }

    public IReadOnlyList<ReportArrReportScheduleResponse> GetReportSchedules(ClaimsPrincipal principal)
    {
        lock (_gate)
        {
            return _reportSchedules
                .Where(item => CanAccessReport(principal, item.ReportDefinitionId))
                .OrderByDescending(item => item.UpdatedAt)
                .ToList();
        }
    }

    public IReadOnlyList<ReportArrReportRecipientResponse> GetReportRecipients(ClaimsPrincipal principal)
    {
        lock (_gate)
        {
            var accessibleScheduleIds = _reportSchedules
                .Where(schedule => CanAccessReport(principal, schedule.ReportDefinitionId))
                .Select(schedule => schedule.ScheduleId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return _reportRecipients
                .Where(item => accessibleScheduleIds.Contains(item.ScheduleId))
                .ToList();
        }
    }

    public ReportArrReportScheduleResponse CreateReportSchedule(ClaimsPrincipal principal, IntegrationEndpoints.CreateReportScheduleRequest request)
    {
        lock (_gate)
        {
            RequireCanScheduleReports(principal);
            var now = DateTimeOffset.UtcNow;
            var reportDefinitionId = RequireTrimmed(request.ReportDefinitionId, nameof(request.ReportDefinitionId));
            var title = RequireTrimmed(request.Title, nameof(request.Title));
            var cadence = NormalizeEnumValue(request.Cadence, nameof(request.Cadence), ScheduleCadences);
            var timezone = RequireTrimmed(request.Timezone, nameof(request.Timezone));
            var deliveryMethod = NormalizeEnumValue(request.DeliveryMethod, nameof(request.DeliveryMethod), DeliveryMethods);
            var definition = _reportDefinitions.First(item => item.ReportDefinitionId == reportDefinitionId);
            EnsureCanViewReport(principal, definition);
            EnsureCanScheduleReport(principal, definition);
            var policy = _reportAccessPolicies.FirstOrDefault(item => item.AccessPolicyId == definition.AccessPolicyRef);
            RequireCondition(
                !string.Equals(deliveryMethod, "webhook", StringComparison.OrdinalIgnoreCase) || policy?.ExternalDeliveryAllowed == true,
                "reportarr.report_delivery_forbidden",
                "Webhook delivery is not allowed for this report.",
                403);
            var requestedByPersonId = RequireTrimmed(request.RequestedByPersonId, nameof(request.RequestedByPersonId));
            var parameters = NormalizeStrings(request.Parameters);
            var recipients = NormalizeStrings(request.Recipients);
            var schedule = new ReportArrReportScheduleResponse(
                NextId("sch"),
                NextNumber("SCH"),
                reportDefinitionId,
                title,
                "active",
                cadence,
                timezone,
                string.IsNullOrWhiteSpace(request.CronExpression) ? null : request.CronExpression.Trim(),
                now.AddDays(1),
                null,
                null,
                null,
                parameters,
                recipients,
                deliveryMethod,
                requestedByPersonId,
                now,
                now);
            _reportSchedules.Add(schedule);
            foreach (var recipient in recipients)
            {
                var isEmail = recipient.Contains('@');
                _reportRecipients.Add(new ReportArrReportRecipientResponse(
                    NextId("rec"),
                    schedule.ScheduleId,
                    isEmail ? "external" : "person",
                    isEmail ? recipient : recipient,
                    isEmail ? recipient : null,
                    "pdf",
                    "active"));
            }
            return schedule;
        }
    }

    public ReportArrReportScheduleResponse UpdateReportSchedule(ClaimsPrincipal principal, string scheduleId, IntegrationEndpoints.UpdateReportScheduleRequest request)
    {
        lock (_gate)
        {
            var existing = _reportSchedules.First(item => item.ScheduleId == scheduleId);
            var report = RequireAccessibleReport(principal, existing.ReportDefinitionId);
            EnsureCanScheduleReport(principal, report);
            var status = NormalizeEnumValue(request.Status, nameof(request.Status), ScheduleStatuses);
            var cadence = NormalizeEnumValue(request.Cadence, nameof(request.Cadence), ScheduleCadences);
            RequireTrimmed(request.RequestedByPersonId, nameof(request.RequestedByPersonId));
            var updated = existing with
            {
                Status = status,
                Cadence = cadence,
                NextRunAt = request.NextRunAt,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            ReplaceReportSchedule(updated);
            return updated;
        }
    }

    public ReportArrExportJobResponse CreateExport(ClaimsPrincipal principal, IntegrationEndpoints.CreateExportRequest request)
    {
        lock (_gate)
        {
            RequireCanRunReports(principal);
            var now = DateTimeOffset.UtcNow;
            var reportRunId = string.IsNullOrWhiteSpace(request.ReportRunId) ? null : RequireTrimmed(request.ReportRunId, nameof(request.ReportRunId));
            var exportType = string.IsNullOrWhiteSpace(request.ExportType)
                ? "report"
                : NormalizeEnumValue(request.ExportType, nameof(request.ExportType), ExportTypes);
            var sourceRef = string.IsNullOrWhiteSpace(request.SourceRef) ? null : request.SourceRef.Trim();
            var exportFormat = NormalizeEnumValue(request.ExportFormat, nameof(request.ExportFormat), ExportFormats);
            var requestedByPersonId = RequireTrimmed(request.RequestedByPersonId, nameof(request.RequestedByPersonId));
            ReportArrReportRunResponse? run = null;
            ReportArrDashboardResponse? dashboard = null;
            ReportArrDatasetResponse? dataset = null;
            ReportArrAuditPackageResponse? auditPackage = null;
            ReportArrReportDefinitionResponse? report = null;

            if (!string.IsNullOrWhiteSpace(reportRunId))
            {
                run = _reportRuns.First(item => item.ReportRunId == reportRunId);
                var definition = _reportDefinitions.First(item => item.ReportDefinitionId == run.ReportDefinitionId);
                EnsureCanViewReport(principal, definition);
                EnsureCanExportReport(principal, definition);
                sourceRef ??= $"report-run:{run.ReportRunId}";
            }
            else if (string.Equals(exportType, "dashboard", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(sourceRef))
                {
                    throw new StlApiException("reportarr.export_source_required", "Dashboard exports require a dashboard source ref.", 400);
                }

                dashboard = _dashboards.FirstOrDefault(item => item.DashboardId == sourceRef);
                RequireCondition(
                    dashboard is not null && CanAccessDashboard(principal, dashboard),
                    "reportarr.forbidden",
                    "You do not have access to this dashboard.",
                    403);
                EnsureCanExportDashboard(principal, dashboard!);
            }
            else if (string.Equals(exportType, "dataset", StringComparison.OrdinalIgnoreCase) || string.Equals(exportType, "table", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(sourceRef))
                {
                    throw new StlApiException("reportarr.export_source_required", "Dataset exports require a dataset source ref.", 400);
                }

                dataset = _datasets.FirstOrDefault(item => item.DatasetId == sourceRef);
                RequireCondition(
                    dataset is not null && CanAccessSourceProducts(principal, dataset.SourceProducts),
                    "reportarr.forbidden",
                    "You do not have access to this dataset.",
                    403);
            }
            else if (string.Equals(exportType, "audit_package", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(sourceRef))
                {
                    throw new StlApiException("reportarr.export_source_required", "Audit package exports require an audit package source ref.", 400);
                }

                auditPackage = _auditPackages.FirstOrDefault(item => item.AuditReportPackageId == sourceRef);
                RequireCondition(
                    auditPackage is not null && CanAccessAuditPackage(principal, auditPackage),
                    "reportarr.forbidden",
                    "You do not have access to this audit package.",
                    403);
            }
            else if (string.Equals(exportType, "chart", StringComparison.OrdinalIgnoreCase) || string.Equals(exportType, "custom", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(sourceRef))
                {
                    throw new StlApiException("reportarr.export_source_required", "Custom exports require a source ref.", 400);
                }

                report = _reportDefinitions.FirstOrDefault(item => item.ReportDefinitionId == sourceRef);
                dashboard = _dashboards.FirstOrDefault(item => item.DashboardId == sourceRef);
                dataset = _datasets.FirstOrDefault(item => item.DatasetId == sourceRef);
                auditPackage = _auditPackages.FirstOrDefault(item => item.AuditReportPackageId == sourceRef);

                var canAccessReport = report is not null && CanAccessReport(principal, report);
                var canAccessDashboard = dashboard is not null && CanAccessDashboard(principal, dashboard);
                var canAccessDataset = dataset is not null && CanAccessSourceProducts(principal, dataset.SourceProducts);
                var canAccessAuditPackage = auditPackage is not null && CanAccessAuditPackage(principal, auditPackage);

                RequireCondition(
                    canAccessReport || canAccessDashboard || canAccessDataset || canAccessAuditPackage,
                    "reportarr.forbidden",
                    "You do not have access to this export source.",
                    403);
            }

            if (run is null && string.IsNullOrWhiteSpace(sourceRef))
            {
                throw new InvalidOperationException("Export requests must include either a report run id or a source ref.");
            }

            var title = run is not null
                ? $"{run.Title} export"
                : $"{exportType.Replace('_', ' ')} export";
            var export = new ReportArrExportJobResponse(
                NextId("exp"),
                NextNumber("EXP"),
                reportRunId ?? string.Empty,
                title,
                "completed",
                exportType,
                exportFormat,
                requestedByPersonId,
                now,
                now,
                now,
                sourceRef,
                run?.OutputRecordRef,
                run?.RowCount ?? 0,
                (run?.RowCount ?? 0) * 128L,
                now.AddDays(30),
                null,
                now,
                now,
                run?.OutputPackageRef);
            _exportJobs.Add(export);
            return export;
        }
    }

    public ReportArrExportJobResponse? GetExportJob(ClaimsPrincipal principal, string exportJobId)
    {
        lock (_gate)
        {
            var export = _exportJobs.FirstOrDefault(item => item.ExportJobId == exportJobId);
            return export is not null && CanAccessExportJob(principal, export) ? export : null;
        }
    }

    public IReadOnlyList<ReportArrExportJobResponse> GetExportJobs(ClaimsPrincipal principal)
    {
        lock (_gate)
        {
            return _exportJobs
                .Where(job => CanAccessExportJob(principal, job))
                .OrderByDescending(item => item.GeneratedAt)
                .ToList();
        }
    }

    public IReadOnlyList<ReportArrMetricDefinitionResponse> GetMetricDefinitions(ClaimsPrincipal principal)
    {
        lock (_gate)
        {
            return _metrics
                .Where(item =>
                {
                    var dataset = _datasets.FirstOrDefault(data => data.DatasetId == item.SourceDatasetRef);
                    return dataset is not null && CanAccessSourceProducts(principal, dataset.SourceProducts);
                })
                .ToList();
        }
    }

    public IReadOnlyList<ReportArrKpiValueResponse> GetKpiValues(ClaimsPrincipal principal)
    {
        lock (_gate)
        {
            return _kpiValues
                .Where(item => CanAccessKpiValue(principal, item))
                .OrderByDescending(item => item.CalculatedAt)
                .ToList();
        }
    }

    public IReadOnlyList<ReportArrMetricValueResponse> GetMetricValues(ClaimsPrincipal principal)
    {
        lock (_gate)
        {
            return _metricValues
                .Where(item => CanAccessMetricValue(principal, item))
                .OrderByDescending(item => item.CalculatedAt)
                .ToList();
        }
    }

    public IReadOnlyList<ReportArrAnalyticsSnapshotResponse> GetAnalyticsSnapshots(ClaimsPrincipal principal)
    {
        lock (_gate)
        {
            return _analyticsSnapshots
                .Where(item => CanAccessAnalyticsSnapshot(principal, item))
                .OrderByDescending(item => item.GeneratedAt)
                .ToList();
        }
    }

    public IReadOnlyList<ReportArrTrendAnalysisResponse> GetTrendAnalyses(ClaimsPrincipal principal)
    {
        lock (_gate)
        {
            return _trendAnalyses
                .Where(item => CanAccessTrendAnalysis(principal, item))
                .OrderByDescending(item => item.GeneratedAt)
                .ToList();
        }
    }

    public IReadOnlyList<ReportArrExceptionQueryResponse> GetExceptionQueries(ClaimsPrincipal principal)
    {
        lock (_gate)
        {
            return _exceptionQueries
                .Where(item => CanAccessExceptionQuery(principal, item))
                .ToList();
        }
    }

    public IReadOnlyList<ReportArrExceptionResultResponse> GetExceptionResults(ClaimsPrincipal principal)
    {
        lock (_gate)
        {
            return _exceptionResults
                .Where(item => CanAccessExceptionResult(principal, item))
                .OrderByDescending(item => item.DetectedAt)
                .ToList();
        }
    }

    public IReadOnlyList<ReportArrReportParameterResponse> GetReportParameters(ClaimsPrincipal principal)
    {
        lock (_gate)
        {
            return _reportParameters.Where(item => CanAccessReport(principal, item.ReportDefinitionId)).ToList();
        }
    }

    public IReadOnlyList<ReportArrReportSectionResponse> GetReportSections(ClaimsPrincipal principal)
    {
        lock (_gate)
        {
            return _reportSections
                .Where(item => CanAccessReport(principal, item.ReportDefinitionId))
                .OrderBy(item => item.Sequence)
                .ToList();
        }
    }

    public ReportArrKpiValueResponse CalculateKpi(ClaimsPrincipal principal, string kpiId, IntegrationEndpoints.CalculateKpiRequest request)
    {
        lock (_gate)
        {
            var kpi = _kpis.First(item => item.KpiId == kpiId);
            EnsureCanViewKpi(principal, kpi);
            RequireCondition(
                request.PeriodStart <= request.PeriodEnd,
                "reportarr.kpi_period_invalid",
                "Period start must be before or equal to period end.");
            RequireTrimmed(request.RequestedByPersonId, nameof(request.RequestedByPersonId));
            var freshnessStatus = ResolveFreshnessStatus();
            var status = freshnessStatus is "fresh" ? "good" : "warning";
            var trend = freshnessStatus is "fresh" ? "improving" : "stable";
            var value = new ReportArrKpiValueResponse(
                NextId("kval"),
                kpi.KpiId,
                request.PeriodStart,
                request.PeriodEnd,
                kpi.TargetValue ?? 0m,
                kpi.TargetValue,
                kpi.WarningThreshold,
                kpi.CriticalThreshold,
                status,
                trend,
                $"dataset {kpi.SourceDatasetRefs.FirstOrDefault() ?? "unknown"} via ReportArr",
                DateTimeOffset.UtcNow);
            _kpiValues.Add(value);
            return value;
        }
    }

    public IReadOnlyList<ReportArrKpiDefinitionResponse> GetKpiDefinitions(ClaimsPrincipal principal)
    {
        lock (_gate)
        {
            return _kpis
                .Where(item => CanAccessKpi(principal, item))
                .OrderByDescending(item => item.UpdatedAt)
                .ToList();
        }
    }

    public ReportArrKpiDefinitionResponse? GetKpiDefinition(ClaimsPrincipal principal, string kpiId)
    {
        lock (_gate)
        {
            var kpi = _kpis.FirstOrDefault(item => item.KpiId == kpiId);
            return kpi is not null && CanAccessKpi(principal, kpi) ? kpi : null;
        }
    }

    public IReadOnlyList<ReportArrAlertResponse> GetAlerts(ClaimsPrincipal principal)
    {
        lock (_gate)
        {
            return _alerts
                .Where(item => CanAccessAlert(principal, item))
                .OrderByDescending(item => item.TriggeredAt)
                .ToList();
        }
    }

    public ReportArrAlertResponse AcknowledgeAlert(ClaimsPrincipal principal, string alertId, IntegrationEndpoints.AcknowledgeAlertRequest request)
    {
        lock (_gate)
        {
            var existing = _alerts.First(item => item.AlertId == alertId);
            EnsureCanViewAlert(principal, existing);
            var requestedByPersonId = RequireTrimmed(request.RequestedByPersonId, nameof(request.RequestedByPersonId));
            var updated = existing with
            {
                Status = "acknowledged",
                AcknowledgedByPersonId = requestedByPersonId,
                AcknowledgedAt = DateTimeOffset.UtcNow
            };
            ReplaceAlert(updated);
            return updated;
        }
    }

    public ReportArrAlertResponse ResolveAlert(ClaimsPrincipal principal, string alertId, IntegrationEndpoints.ResolveAlertRequest request)
    {
        lock (_gate)
        {
            var existing = _alerts.First(item => item.AlertId == alertId);
            EnsureCanViewAlert(principal, existing);
            RequireTrimmed(request.RequestedByPersonId, nameof(request.RequestedByPersonId));
            var updated = existing with
            {
                Status = "resolved",
                ResolvedAt = DateTimeOffset.UtcNow
            };
            ReplaceAlert(updated);
            return updated;
        }
    }

    public ReportArrAuditPackageResponse CreateAuditPackage(ClaimsPrincipal principal, IntegrationEndpoints.CreateAuditPackageRequest request)
    {
        lock (_gate)
        {
            RequireCanCreateAuditPackages(principal);
            var now = DateTimeOffset.UtcNow;
            var title = RequireTrimmed(request.Title, nameof(request.Title));
            var description = RequireTrimmed(request.Description, nameof(request.Description));
            var requestedByPersonId = RequireTrimmed(request.RequestedByPersonId, nameof(request.RequestedByPersonId));
            var auditScopeId = RequireTrimmed(request.AuditScopeId, nameof(request.AuditScopeId));
            var auditScope = _auditScopes.FirstOrDefault(scope => scope.AuditScopeId == auditScopeId)
                ?? throw new InvalidOperationException("Audit scope not found.");
            EnsureCanAccessProducts(principal, auditScope.ProductFilters, "reportarr.audit_package_forbidden", "You do not have access to this audit scope.", 403);
            var package = new ReportArrAuditPackageResponse(
                NextId("aud"),
                NextNumber("AUD"),
                title,
                description,
                "assembling",
                requestedByPersonId,
                auditScope,
                ["eval-001", "eval-002"],
                ["reportarr", "compliancecore"],
                ["obj-001", "obj-002"],
                "pkg-reportarr",
                _reportRuns.Take(2).Select(item => item.ReportRunId).ToList(),
                "No missing evidence.",
                "No invalid evidence.",
                95m,
                now,
                null);
            _auditPackages.Add(package);
            return package;
        }
    }

    public ReportArrAuditPackageResponse? GetAuditPackage(ClaimsPrincipal principal, string auditReportPackageId)
    {
        lock (_gate)
        {
            var auditPackage = _auditPackages.FirstOrDefault(item => item.AuditReportPackageId == auditReportPackageId);
            return auditPackage is not null && CanAccessSourceProducts(principal, auditPackage.SourceProductRefs) ? auditPackage : null;
        }
    }

    public ReportArrAuditPackageResponse LockAuditPackage(ClaimsPrincipal principal, string auditReportPackageId, IntegrationEndpoints.LockAuditPackageRequest request)
    {
        lock (_gate)
        {
            var existing = _auditPackages.First(item => item.AuditReportPackageId == auditReportPackageId);
            EnsureCanViewAuditPackage(principal, existing);
            RequireTrimmed(request.RequestedByPersonId, nameof(request.RequestedByPersonId));
            var updated = existing with
            {
                Status = "locked",
                LockedAt = DateTimeOffset.UtcNow
            };
            ReplaceAuditPackage(updated);
            return updated;
        }
    }

    public IReadOnlyList<ReportArrAuditPackageResponse> GetAuditPackages(ClaimsPrincipal principal)
    {
        lock (_gate)
        {
            return _auditPackages
                .Where(item => CanAccessSourceProducts(principal, item.SourceProductRefs))
                .OrderByDescending(item => item.GeneratedAt)
                .ToList();
        }
    }

    public IReadOnlyList<ReportArrAuditScopeResponse> GetAuditScopes(ClaimsPrincipal principal)
    {
        lock (_gate)
        {
            return principal.IsPlatformAdmin() ? _auditScopes.ToList() : [];
        }
    }

    public IReadOnlyList<ReportArrDashboardWidgetResponse> GetWidgets(ClaimsPrincipal principal)
    {
        lock (_gate)
        {
            return _widgets
                .Where(widget =>
                {
                    var dashboard = _dashboards.FirstOrDefault(item => item.DashboardId == widget.DashboardId);
                    return dashboard is not null && CanAccessDashboard(principal, dashboard);
                })
                .ToList();
        }
    }

    public IReadOnlyList<ReportArrWidgetVisualizationSettingsResponse> GetWidgetVisualizations(ClaimsPrincipal principal)
    {
        lock (_gate)
        {
            return _widgetVisualizations
                .Where(item =>
                {
                    var widget = _widgets.FirstOrDefault(candidate => candidate.WidgetId == item.WidgetId);
                    if (widget is null)
                    {
                        return false;
                    }

                    var dashboard = _dashboards.FirstOrDefault(candidate => candidate.DashboardId == widget.DashboardId);
                    return dashboard is not null && CanAccessDashboard(principal, dashboard);
                })
                .ToList();
        }
    }

    public IReadOnlyList<ReportArrSourceConnectorResponse> GetSourceConnectors(ClaimsPrincipal principal)
    {
        lock (_gate)
        {
            return _sourceConnectors
                .Where(item => CanAccessSourceProducts(principal, [item.SourceProduct]))
                .ToList();
        }
    }

    public IReadOnlyList<ReportArrIngestionCursorResponse> GetIngestionCursors(ClaimsPrincipal principal)
    {
        lock (_gate)
        {
            return _ingestionCursors
                .Where(item => CanAccessSourceProducts(principal, [item.SourceProduct]))
                .ToList();
        }
    }

    public IReadOnlyList<ReportArrRefreshJobResponse> GetRefreshJobs(ClaimsPrincipal principal)
    {
        lock (_gate)
        {
            return _refreshJobs
                .Where(item =>
                {
                    var dataset = _datasets.FirstOrDefault(dataset => dataset.DatasetId == item.DatasetId);
                    return dataset is null || CanAccessSourceProducts(principal, dataset.SourceProducts);
                })
                .ToList();
        }
    }

    private IReadOnlyList<ReportArrDashboardResponse> GetAccessibleDashboards(ClaimsPrincipal principal)
    {
        if (principal.IsPlatformAdmin())
        {
            return _dashboards.ToList();
        }

        return _dashboards
            .Where(dashboard =>
                CanAccessPolicy(
                    principal,
                    _dashboardAccessPolicies.FirstOrDefault(policy => policy.AccessPolicyId == dashboard.AccessPolicyRef),
                    dashboard.OwnerPersonId))
            .ToList();
    }

    private IReadOnlyList<ReportArrReportDefinitionResponse> GetAccessibleReports(ClaimsPrincipal principal)
    {
        if (principal.IsPlatformAdmin())
        {
            return _reportDefinitions.ToList();
        }

        return _reportDefinitions
            .Where(report =>
                CanAccessPolicy(
                    principal,
                    _reportAccessPolicies.FirstOrDefault(policy => policy.AccessPolicyId == report.AccessPolicyRef),
                    report.OwnerPersonId))
            .ToList();
    }

    private static bool CanAccessPolicy(
        ClaimsPrincipal principal,
        ReportArrDashboardAccessPolicyResponse? policy,
        string ownerPersonId)
    {
        if (policy is null)
        {
            return false;
        }

        return CanAccessPolicy(
            principal,
            policy.Visibility,
            policy.AllowedPersonRefs,
            policy.AllowedRoleRefs,
            policy.AllowedPermissionRefs,
            policy.SourceProductRestrictions,
            ownerPersonId);
    }

    private static bool CanAccessPolicy(
        ClaimsPrincipal principal,
        ReportArrReportAccessPolicyResponse? policy,
        string ownerPersonId)
    {
        if (policy is null)
        {
            return false;
        }

        return CanAccessPolicy(
            principal,
            policy.Visibility,
            policy.AllowedPersonRefs,
            policy.AllowedRoleRefs,
            policy.AllowedPermissionRefs,
            policy.SourceProductRestrictions,
            ownerPersonId);
    }

    private static bool CanAccessPolicy(
        ClaimsPrincipal principal,
        string visibility,
        IReadOnlyList<string> allowedPersonRefs,
        IReadOnlyList<string> allowedRoleRefs,
        IReadOnlyList<string> allowedPermissionRefs,
        IReadOnlyList<string> sourceProductRestrictions,
        string ownerPersonId)
    {
        if (principal.IsPlatformAdmin())
        {
            return true;
        }

        var personId = principal.GetPersonId().ToString();
        var roleKey = principal.GetTenantRoleKey();
        var entitlements = principal.GetEntitlements();

        if (sourceProductRestrictions.Count > 0 &&
            !sourceProductRestrictions.Any(restriction => entitlements.Any(entitlement =>
                string.Equals(
                    ProductKeyAliases.Normalize(entitlement),
                    ProductKeyAliases.Normalize(restriction),
                    StringComparison.OrdinalIgnoreCase))))
        {
            return false;
        }

        if (allowedPersonRefs.Any(person =>
                string.Equals(person, personId, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        if (allowedRoleRefs.Any(role => string.Equals(role, roleKey, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        if (allowedPermissionRefs.Any(permission =>
                entitlements.Any(entitlement => string.Equals(permission, entitlement, StringComparison.OrdinalIgnoreCase))))
        {
            return true;
        }

        return string.Equals(visibility, "tenant_wide", StringComparison.OrdinalIgnoreCase);
    }

    private bool CanAccessDashboard(ClaimsPrincipal principal, ReportArrDashboardResponse dashboard)
    {
        var policy = _dashboardAccessPolicies.FirstOrDefault(item => item.AccessPolicyId == dashboard.AccessPolicyRef);
        return CanAccessPolicy(principal, policy, dashboard.OwnerPersonId);
    }

    private bool CanAccessDashboardById(ClaimsPrincipal principal, string dashboardId)
    {
        var dashboard = _dashboards.FirstOrDefault(item => item.DashboardId == dashboardId);
        return dashboard is not null && CanAccessDashboard(principal, dashboard);
    }

    private bool CanAccessReport(ClaimsPrincipal principal, ReportArrReportDefinitionResponse report)
    {
        var policy = _reportAccessPolicies.FirstOrDefault(item => item.AccessPolicyId == report.AccessPolicyRef);
        return CanAccessPolicy(principal, policy, report.OwnerPersonId);
    }

    private bool CanAccessReport(ClaimsPrincipal principal, string reportDefinitionId)
    {
        var report = _reportDefinitions.FirstOrDefault(item => item.ReportDefinitionId == reportDefinitionId);
        return report is not null && CanAccessReport(principal, report);
    }

    private bool CanAccessReadModel(ClaimsPrincipal principal, ReportArrReadModelResponse readModel)
    {
        if (principal.IsPlatformAdmin())
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(readModel.PrimarySourceProduct))
        {
            return CanAccessSourceProducts(principal, [readModel.PrimarySourceProduct]);
        }

        return readModel.DatasetRefs
            .Select(datasetId => _datasets.FirstOrDefault(dataset => dataset.DatasetId == datasetId))
            .Where(dataset => dataset is not null)
            .All(dataset => CanAccessSourceProducts(principal, dataset!.SourceProducts));
    }

    private bool CanAccessKpi(ClaimsPrincipal principal, ReportArrKpiDefinitionResponse kpi)
    {
        if (principal.IsPlatformAdmin())
        {
            return true;
        }

        var sourceProducts = kpi.SourceDatasetRefs
            .SelectMany(datasetId => _datasets.Where(dataset => dataset.DatasetId == datasetId))
            .SelectMany(dataset => dataset.SourceProducts)
            .ToList();
        return CanAccessSourceProducts(principal, sourceProducts);
    }

    private bool CanAccessAlert(ClaimsPrincipal principal, ReportArrAlertResponse alert)
    {
        if (principal.IsPlatformAdmin())
        {
            return true;
        }

        var sourceProducts = _datasets
            .Where(dataset => string.Equals(dataset.DatasetId, alert.DatasetRef, StringComparison.OrdinalIgnoreCase))
            .SelectMany(dataset => dataset.SourceProducts)
            .Concat(_metrics
                .Where(metric => string.Equals(metric.MetricId, alert.MetricRef, StringComparison.OrdinalIgnoreCase))
                .SelectMany(metric => _datasets.Where(dataset => dataset.DatasetId == metric.SourceDatasetRef))
                .SelectMany(dataset => dataset.SourceProducts))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return CanAccessSourceProducts(principal, sourceProducts);
    }

    private bool CanAccessAuditPackage(ClaimsPrincipal principal, ReportArrAuditPackageResponse auditPackage)
    {
        if (principal.IsPlatformAdmin())
        {
            return true;
        }

        return CanAccessSourceProducts(principal, auditPackage.SourceProductRefs);
    }

    private static bool CanAccessSourceProducts(ClaimsPrincipal principal, IReadOnlyList<string> sourceProducts)
    {
        if (principal.IsPlatformAdmin())
        {
            return true;
        }

        if (sourceProducts.Count == 0)
        {
            return true;
        }

        var entitlements = principal.GetEntitlements();
        return sourceProducts.Any(sourceProduct =>
            entitlements.Any(entitlement =>
                string.Equals(
                    ProductKeyAliases.Normalize(entitlement),
                    ProductKeyAliases.Normalize(sourceProduct),
                    StringComparison.OrdinalIgnoreCase)));
    }

    private bool CanAccessKpiValue(ClaimsPrincipal principal, ReportArrKpiValueResponse kpiValue)
    {
        var kpi = _kpis.FirstOrDefault(item => item.KpiId == kpiValue.KpiId);
        return kpi is not null && CanAccessKpi(principal, kpi);
    }

    private bool CanAccessMetricValue(ClaimsPrincipal principal, ReportArrMetricValueResponse metricValue)
    {
        var metric = _metrics.FirstOrDefault(item => item.MetricId == metricValue.MetricId);
        if (metric is null)
        {
            return false;
        }

        var dataset = _datasets.FirstOrDefault(item => item.DatasetId == metric.SourceDatasetRef);
        return dataset is null || CanAccessSourceProducts(principal, dataset.SourceProducts);
    }

    private bool CanAccessAnalyticsSnapshot(ClaimsPrincipal principal, ReportArrAnalyticsSnapshotResponse snapshot)
    {
        return snapshot.DatasetRefs.All(datasetId =>
        {
            var dataset = _datasets.FirstOrDefault(item => item.DatasetId == datasetId);
            return dataset is null || CanAccessSourceProducts(principal, dataset.SourceProducts);
        });
    }

    private bool CanAccessTrendAnalysis(ClaimsPrincipal principal, ReportArrTrendAnalysisResponse trend)
    {
        var metric = _metrics.FirstOrDefault(item => item.MetricId == trend.MetricRef);
        if (metric is not null)
        {
            var dataset = _datasets.FirstOrDefault(item => item.DatasetId == metric.SourceDatasetRef);
            return dataset is null || CanAccessSourceProducts(principal, dataset.SourceProducts);
        }

        var kpi = _kpis.FirstOrDefault(item => item.KpiId == trend.KpiRef);
        return kpi is not null && CanAccessKpi(principal, kpi);
    }

    private bool CanAccessExceptionQuery(ClaimsPrincipal principal, ReportArrExceptionQueryResponse query)
    {
        var dataset = _datasets.FirstOrDefault(item => item.DatasetId == query.SourceDatasetRef);
        return dataset is null || CanAccessSourceProducts(principal, dataset.SourceProducts);
    }

    private bool CanAccessExceptionResult(ClaimsPrincipal principal, ReportArrExceptionResultResponse result)
    {
        var query = _exceptionQueries.FirstOrDefault(item => item.ExceptionQueryId == result.ExceptionQueryId);
        return query is not null && CanAccessExceptionQuery(principal, query);
    }

    private void RequireCanManageDatasets(ClaimsPrincipal principal)
    {
        RequireCondition(
            principal.IsPlatformAdmin() || (principal.HasProductEntitlement("reportarr") && MatchesRole(principal.GetTenantRoleKey(), "analytics_admin", "reportarr_admin")),
            "reportarr.dataset_manage_forbidden",
            "Managing datasets is not allowed.",
            403);
    }

    private void RequireCanRefreshDatasets(ClaimsPrincipal principal)
    {
        RequireCondition(
            principal.IsPlatformAdmin() || (principal.HasProductEntitlement("reportarr") && MatchesRole(principal.GetTenantRoleKey(), "analytics_admin", "reportarr_admin")),
            "reportarr.dataset_refresh_forbidden",
            "Refreshing datasets is not allowed.",
            403);
    }

    private void RequireCanRebuildReadModels(ClaimsPrincipal principal)
    {
        RequireCondition(
            principal.IsPlatformAdmin() || (principal.HasProductEntitlement("reportarr") && MatchesRole(principal.GetTenantRoleKey(), "analytics_admin", "reportarr_admin")),
            "reportarr.read_model_rebuild_forbidden",
            "Rebuilding read models is not allowed.",
            403);
    }

    private void RequireCanBuildReports(ClaimsPrincipal principal)
    {
        RequireCondition(
            principal.IsPlatformAdmin() || (principal.HasProductEntitlement("reportarr") && MatchesRole(principal.GetTenantRoleKey(), "report_builder", "reportarr_builder", "tenant_admin", "reportarr_admin")),
            "reportarr.report_build_forbidden",
            "Building dashboards and reports is not allowed.",
            403);
    }

    private void RequireCanCreateAuditPackages(ClaimsPrincipal principal)
    {
        RequireCondition(
            principal.IsPlatformAdmin() || (principal.HasProductEntitlement("reportarr") && MatchesRole(principal.GetTenantRoleKey(), "compliance_reporter", "reportarr_admin", "tenant_admin")),
            "reportarr.audit_package_forbidden",
            "Creating audit packages is not allowed.",
            403);
    }

    private void RequireCanRunReports(ClaimsPrincipal principal)
    {
        RequireCondition(
            principal.IsPlatformAdmin() || (principal.HasProductEntitlement("reportarr") && MatchesRole(principal.GetTenantRoleKey(), "report_runner", "reportarr_runner", "report_builder", "reportarr_admin", "tenant_admin")),
            "reportarr.report_run_forbidden",
            "Running and exporting reports is not allowed.",
            403);
    }

    private void RequireCanScheduleReports(ClaimsPrincipal principal)
    {
        RequireCondition(
            principal.IsPlatformAdmin() || (principal.HasProductEntitlement("reportarr") && MatchesRole(principal.GetTenantRoleKey(), "report_scheduler", "reportarr_scheduler", "report_builder", "reportarr_admin", "tenant_admin")),
            "reportarr.report_schedule_forbidden",
            "Scheduling reports is not allowed.",
            403);
    }

    private void RequireCanReceiveSourceEvents(ClaimsPrincipal principal)
    {
        RequireCondition(
            principal.IsPlatformAdmin() || (principal.HasProductEntitlement("reportarr") && MatchesRole(principal.GetTenantRoleKey(), "analytics_admin", "reportarr_admin", "tenant_admin")),
            "reportarr.source_event_receive_forbidden",
            "Receiving source events is not allowed.",
            403);
    }

    private static bool MatchesRole(string roleKey, params string[] expectedRoleKeys) =>
        expectedRoleKeys.Any(expected =>
            string.Equals(roleKey, expected, StringComparison.OrdinalIgnoreCase));

    private void EnsureCanViewKpi(ClaimsPrincipal principal, ReportArrKpiDefinitionResponse kpi)
    {
        RequireCondition(
            CanAccessKpi(principal, kpi),
            "reportarr.forbidden",
            "You do not have access to this KPI.",
            403);
    }

    private void EnsureCanViewAlert(ClaimsPrincipal principal, ReportArrAlertResponse alert)
    {
        RequireCondition(
            CanAccessAlert(principal, alert),
            "reportarr.forbidden",
            "You do not have access to this alert.",
            403);
    }

    private void EnsureCanViewAuditPackage(ClaimsPrincipal principal, ReportArrAuditPackageResponse auditPackage)
    {
        RequireCondition(
            CanAccessAuditPackage(principal, auditPackage),
            "reportarr.forbidden",
            "You do not have access to this audit package.",
            403);
    }

    private void EnsureCanAccessProducts(ClaimsPrincipal principal, IReadOnlyList<string> sourceProducts, string code, string message, int statusCode)
    {
        RequireCondition(
            CanAccessSourceProducts(principal, sourceProducts),
            code,
            message,
            statusCode);
    }

    private ReportArrReportDefinitionResponse RequireAccessibleReport(ClaimsPrincipal principal, string reportDefinitionId)
    {
        var definition = _reportDefinitions.First(item => item.ReportDefinitionId == reportDefinitionId);
        EnsureCanViewReport(principal, definition);
        return definition;
    }

    private void EnsureCanViewReport(ClaimsPrincipal principal, ReportArrReportDefinitionResponse definition)
    {
        var policy = _reportAccessPolicies.FirstOrDefault(item => item.AccessPolicyId == definition.AccessPolicyRef);
        RequireCondition(
            principal.IsPlatformAdmin() || CanAccessPolicy(principal, policy, definition.OwnerPersonId),
            "reportarr.forbidden",
            "You do not have access to this report.",
            403);
    }

    private void EnsureCanViewDashboard(ClaimsPrincipal principal, ReportArrDashboardResponse dashboard)
    {
        var policy = _dashboardAccessPolicies.FirstOrDefault(item => item.AccessPolicyId == dashboard.AccessPolicyRef);
        RequireCondition(
            principal.IsPlatformAdmin() || CanAccessPolicy(principal, policy, dashboard.OwnerPersonId),
            "reportarr.forbidden",
            "You do not have access to this dashboard.",
            403);
    }

    private void EnsureCanScheduleReport(ClaimsPrincipal principal, ReportArrReportDefinitionResponse definition)
    {
        var policy = _reportAccessPolicies.FirstOrDefault(item => item.AccessPolicyId == definition.AccessPolicyRef);
        RequireCondition(
            principal.IsPlatformAdmin() || (policy is not null && policy.ScheduleAllowed),
            "reportarr.report_schedule_forbidden",
            "Scheduling is not allowed for this report.",
            403);
    }

    private void EnsureCanExportReport(ClaimsPrincipal principal, ReportArrReportDefinitionResponse definition)
    {
        var policy = _reportAccessPolicies.FirstOrDefault(item => item.AccessPolicyId == definition.AccessPolicyRef);
        RequireCondition(
            principal.IsPlatformAdmin() || (policy is not null && policy.ExportAllowed),
            "reportarr.report_export_forbidden",
            "Exporting is not allowed for this report.",
            403);
    }

    private void EnsureCanExportDashboard(ClaimsPrincipal principal, ReportArrDashboardResponse dashboard)
    {
        var policy = _dashboardAccessPolicies.FirstOrDefault(item => item.AccessPolicyId == dashboard.AccessPolicyRef);
        RequireCondition(
            principal.IsPlatformAdmin() || (policy is not null && policy.ExportAllowed),
            "reportarr.dashboard_export_forbidden",
            "Exporting is not allowed for this dashboard.",
            403);
    }

    private void EnsureCanManageDashboard(ClaimsPrincipal principal, ReportArrDashboardResponse dashboard)
    {
        var policy = _dashboardAccessPolicies.FirstOrDefault(item => item.AccessPolicyId == dashboard.AccessPolicyRef);
        RequireCondition(
            principal.IsPlatformAdmin() || CanAccessPolicy(principal, policy, dashboard.OwnerPersonId) && policy is not null && policy.AllowedPermissionRefs.Any(permission => string.Equals(permission, "reportarr.dashboards.update", StringComparison.OrdinalIgnoreCase)),
            "reportarr.dashboard_update_forbidden",
            "Updating this dashboard is not allowed.",
            403);
    }

    private void EnsureCanManageReport(ClaimsPrincipal principal, ReportArrReportDefinitionResponse definition)
    {
        var policy = _reportAccessPolicies.FirstOrDefault(item => item.AccessPolicyId == definition.AccessPolicyRef);
        RequireCondition(
            principal.IsPlatformAdmin() || CanAccessPolicy(principal, policy, definition.OwnerPersonId) && policy is not null && policy.AllowedPermissionRefs.Any(permission => string.Equals(permission, "reportarr.reports.update", StringComparison.OrdinalIgnoreCase)),
            "reportarr.report_update_forbidden",
            "Updating this report is not allowed.",
            403);
    }

    public IReadOnlyList<ReportArrSourceEventReceiptResponse> GetSourceEvents(ClaimsPrincipal principal)
    {
        lock (_gate)
        {
            return principal.IsPlatformAdmin() ? _sourceEvents.ToList() : [];
        }
    }

    public ReportArrSourceEventReceiptResponse ReceiveEvent(ClaimsPrincipal principal, IntegrationEndpoints.SourceEventRequest request)
    {
        lock (_gate)
        {
            RequireCanReceiveSourceEvents(principal);
            var now = DateTimeOffset.UtcNow;
            var sourceProduct = RequireTrimmed(request.SourceProduct, nameof(request.SourceProduct)).ToLowerInvariant();
            var sourceEventId = RequireTrimmed(request.SourceEventId, nameof(request.SourceEventId));
            var eventType = RequireTrimmed(request.EventType, nameof(request.EventType));
            var sourceObjectRef = string.IsNullOrWhiteSpace(request.SourceObjectRef) ? null : request.SourceObjectRef.Trim();
            var correlationId = string.IsNullOrWhiteSpace(request.CorrelationId) ? null : request.CorrelationId.Trim();
            var connector = _sourceConnectors.FirstOrDefault(item => string.Equals(item.SourceProduct, sourceProduct, StringComparison.OrdinalIgnoreCase));
            var isActiveConnector = connector is not null && string.Equals(connector.Status, "active", StringComparison.OrdinalIgnoreCase);
            var supportsEventType = connector?.SupportedEventTypes.Any(item => string.Equals(item, eventType, StringComparison.OrdinalIgnoreCase)) ?? false;
            var status = isActiveConnector && supportsEventType ? "processed" : "failed";
            var failureReason = isActiveConnector
                ? (supportsEventType ? null : $"Event type '{eventType}' is not supported by source connector '{sourceProduct}'.")
                : $"No active source connector registered for '{sourceProduct}'.";
            var receipt = new ReportArrSourceEventReceiptResponse(
                NextId("ing"),
                sourceProduct,
                sourceEventId,
                eventType,
                sourceObjectRef,
                now,
                status == "processed" ? now : null,
                status,
                failureReason,
                correlationId);
            _sourceEvents.Add(receipt);
            ApplySourceEventOutcome(sourceProduct, status, now);
            return receipt;
        }
    }

    public object ReceiveEvents(ClaimsPrincipal principal, IntegrationEndpoints.SourceEventBatchRequest request)
    {
        if (request.Events.Count == 0)
        {
            throw new StlApiException("reportarr.source_events_required", "At least one source event is required.", 400);
        }

        var receipts = request.Events.Select(eventRequest => ReceiveEvent(principal, eventRequest)).ToList();
        return new { received = receipts.Count, receipts };
    }

    private string ResolveFreshnessStatus()
    {
        if (_datasets.Any(dataset => dataset.FreshnessStatus == "failed"))
        {
            return "failed";
        }

        if (_datasets.Any(dataset => dataset.FreshnessStatus == "stale"))
        {
            return "stale";
        }

        if (_datasets.Any(dataset => dataset.FreshnessStatus == "slightly_stale"))
        {
            return "slightly_stale";
        }

        return "fresh";
    }

    private int GetFreshnessWarnings() =>
        _datasets.Count(dataset => dataset.FreshnessStatus is "stale" or "failed" or "slightly_stale");

    private void ReplaceDataset(ReportArrDatasetResponse dataset)
    {
        var index = _datasets.FindIndex(item => item.DatasetId == dataset.DatasetId);
        if (index >= 0)
        {
            _datasets[index] = dataset;
        }
    }

    private void ReplaceReadModel(ReportArrReadModelResponse model)
    {
        var index = _readModels.FindIndex(item => item.ReadModelId == model.ReadModelId);
        if (index >= 0)
        {
            _readModels[index] = model;
        }
    }

    private void ReplaceDashboard(ReportArrDashboardResponse dashboard)
    {
        var index = _dashboards.FindIndex(item => item.DashboardId == dashboard.DashboardId);
        if (index >= 0)
        {
            _dashboards[index] = dashboard;
        }
    }

    private void ReplaceWidget(ReportArrDashboardWidgetResponse widget)
    {
        var index = _widgets.FindIndex(item => item.WidgetId == widget.WidgetId);
        if (index >= 0)
        {
            _widgets[index] = widget;
        }
    }

    private void ReplaceReportRun(ReportArrReportRunResponse run)
    {
        var index = _reportRuns.FindIndex(item => item.ReportRunId == run.ReportRunId);
        if (index >= 0)
        {
            _reportRuns[index] = run;
        }
    }

    private void ReplaceReportDefinition(ReportArrReportDefinitionResponse definition)
    {
        var index = _reportDefinitions.FindIndex(item => item.ReportDefinitionId == definition.ReportDefinitionId);
        if (index >= 0)
        {
            _reportDefinitions[index] = definition;
        }
    }

    private static IReadOnlyList<string> NormalizeFormats(IReadOnlyList<string> exportFormats)
    {
        var normalized = exportFormats
            .Select(format => format.Trim().ToLowerInvariant())
            .Where(format => !string.IsNullOrWhiteSpace(format))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return normalized.Count == 0 ? ["pdf", "csv"] : normalized;
    }

    private void ReplaceReportSchedule(ReportArrReportScheduleResponse schedule)
    {
        var index = _reportSchedules.FindIndex(item => item.ScheduleId == schedule.ScheduleId);
        if (index >= 0)
        {
            _reportSchedules[index] = schedule;
        }
    }

    private void ReplaceAlert(ReportArrAlertResponse alert)
    {
        var index = _alerts.FindIndex(item => item.AlertId == alert.AlertId);
        if (index >= 0)
        {
            _alerts[index] = alert;
        }
    }

    private void ReplaceAuditPackage(ReportArrAuditPackageResponse auditPackage)
    {
        var index = _auditPackages.FindIndex(item => item.AuditReportPackageId == auditPackage.AuditReportPackageId);
        if (index >= 0)
        {
            _auditPackages[index] = auditPackage;
        }
    }

    private bool CanAccessExportJob(ClaimsPrincipal principal, ReportArrExportJobResponse export)
    {
        if (principal.IsPlatformAdmin())
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(export.ReportRunId))
        {
            var run = _reportRuns.FirstOrDefault(item => item.ReportRunId == export.ReportRunId);
            return run is not null && CanAccessReport(principal, run.ReportDefinitionId);
        }

        if (string.Equals(export.ExportType, "dashboard", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(export.SourceRef))
        {
            var dashboard = _dashboards.FirstOrDefault(item => item.DashboardId == export.SourceRef);
            return dashboard is not null && CanAccessDashboard(principal, dashboard);
        }

        if ((string.Equals(export.ExportType, "dataset", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(export.ExportType, "table", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(export.ExportType, "chart", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(export.ExportType, "custom", StringComparison.OrdinalIgnoreCase)) &&
            !string.IsNullOrWhiteSpace(export.SourceRef))
        {
            var dataset = _datasets.FirstOrDefault(item => item.DatasetId == export.SourceRef);
            if (dataset is not null)
            {
                return CanAccessSourceProducts(principal, dataset.SourceProducts);
            }

            var auditPackage = _auditPackages.FirstOrDefault(item => item.AuditReportPackageId == export.SourceRef);
            if (auditPackage is not null)
            {
                return CanAccessAuditPackage(principal, auditPackage);
            }

            var report = _reportDefinitions.FirstOrDefault(item => item.ReportDefinitionId == export.SourceRef);
            if (report is not null)
            {
                return CanAccessReport(principal, report);
            }

            var dashboard = _dashboards.FirstOrDefault(item => item.DashboardId == export.SourceRef);
            if (dashboard is not null)
            {
                return CanAccessDashboard(principal, dashboard);
            }
        }

        return string.Equals(export.RequestedByPersonId, principal.GetPersonId().ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private void ApplySourceEventOutcome(string sourceProduct, string status, DateTimeOffset now)
    {
        var impactedDatasets = _datasets
            .Where(dataset => dataset.SourceProducts.Any(product => string.Equals(product, sourceProduct, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        if (impactedDatasets.Count == 0)
        {
            return;
        }

        var impactedDatasetIds = impactedDatasets
            .Select(dataset => dataset.DatasetId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var processed = string.Equals(status, "processed", StringComparison.OrdinalIgnoreCase);

        foreach (var dataset in impactedDatasets)
        {
            var updatedDataset = dataset with
            {
                FreshnessStatus = processed ? "fresh" : "stale",
                LastRefreshedAt = now,
                LastSuccessfulRefreshAt = processed ? now : dataset.LastSuccessfulRefreshAt,
                LastFailedRefreshAt = processed ? dataset.LastFailedRefreshAt : now,
                UpdatedAt = now
            };
            ReplaceDataset(updatedDataset);
        }

        foreach (var readModel in _readModels.Where(model => model.DatasetRefs.Any(datasetId => impactedDatasetIds.Contains(datasetId))).ToList())
        {
            var updatedReadModel = readModel with
            {
                Status = processed ? "active" : "stale",
                LastUpdatedAt = now,
                UpdatedAt = now
            };
            ReplaceReadModel(updatedReadModel);
        }

        foreach (var dashboard in _dashboards
                     .Where(item => _widgets.Any(widget =>
                         string.Equals(widget.DashboardId, item.DashboardId, StringComparison.OrdinalIgnoreCase) &&
                         impactedDatasetIds.Contains(widget.DatasetRef)))
                     .ToList())
        {
            var updatedDashboard = dashboard with
            {
                FreshnessStatus = ResolveDashboardFreshness(dashboard.DashboardId),
                UpdatedAt = now,
                UpdatedByPersonId = "reportarr-system"
            };
            ReplaceDashboard(updatedDashboard);
        }
    }

    private string ResolveDashboardFreshness(string dashboardId)
    {
        var widgetDatasetIds = _widgets
            .Where(widget => string.Equals(widget.DashboardId, dashboardId, StringComparison.OrdinalIgnoreCase) &&
                             !string.IsNullOrWhiteSpace(widget.DatasetRef))
            .Select(widget => widget.DatasetRef)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (widgetDatasetIds.Count == 0)
        {
            return "unknown";
        }

        var freshnessStatuses = widgetDatasetIds
            .Select(datasetId => _datasets.FirstOrDefault(dataset => string.Equals(dataset.DatasetId, datasetId, StringComparison.OrdinalIgnoreCase))?.FreshnessStatus)
            .Where(status => !string.IsNullOrWhiteSpace(status))
            .Select(status => status!)
            .ToList();

        if (freshnessStatuses.Count == 0)
        {
            return "unknown";
        }

        if (freshnessStatuses.Any(status => string.Equals(status, "failed", StringComparison.OrdinalIgnoreCase)))
        {
            return "failed";
        }

        if (freshnessStatuses.Any(status => string.Equals(status, "stale", StringComparison.OrdinalIgnoreCase)))
        {
            return "stale";
        }

        if (freshnessStatuses.Any(status => string.Equals(status, "slightly_stale", StringComparison.OrdinalIgnoreCase)))
        {
            return "slightly_stale";
        }

        return "fresh";
    }

    private string NextId(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

    private static string NextNumber(string prefix) => $"{prefix}-{DateTimeOffset.UtcNow:yyMMdd}-{Random.Shared.Next(1, 999):000}";
}
