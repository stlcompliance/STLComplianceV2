using ReportArr.Api.Data;
using STLCompliance.Shared.Auth;

namespace ReportArr.Api.Endpoints;

public static class WorkspaceEndpoints
{
    public static void MapReportArrWorkspaceEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/workspace").WithTags("Workspace").RequireAuthorization();

        group.MapGet("/summary", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetSummary(context.User)))
            .WithName("GetReportArrWorkspaceSummary");

        group.MapGet("/datasets", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetDatasets(context.User)))
            .WithName("ListReportArrWorkspaceDatasets");
        group.MapGet("/dataset-fields", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetDatasetFields(context.User)))
            .WithName("ListReportArrWorkspaceDatasetFields");

        group.MapGet("/dashboards", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetDashboards(context.User)))
            .WithName("ListReportArrWorkspaceDashboards");
        group.MapGet("/dashboard-access-policies", (HttpContext context, ReportArrStore store) =>
            Results.Ok(store.GetDashboardAccessPolicies(context.User)))
            .WithName("ListReportArrWorkspaceDashboardAccessPolicies");
        group.MapGet("/dashboard-filters", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetDashboardFilters(context.User)))
            .WithName("ListReportArrWorkspaceDashboardFilters");
        group.MapGet("/drilldowns", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetDrilldowns(context.User)))
            .WithName("ListReportArrWorkspaceDrilldowns");

        group.MapGet("/reports", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetReportDefinitions(context.User)))
            .WithName("ListReportArrWorkspaceReports");
        group.MapGet("/report-access-policies", (HttpContext context, ReportArrStore store) =>
            Results.Ok(store.GetReportAccessPolicies(context.User)))
            .WithName("ListReportArrWorkspaceReportAccessPolicies");

        group.MapGet("/kpis", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetKpiDefinitions(context.User)))
            .WithName("ListReportArrWorkspaceKpis");
        group.MapGet("/kpi-values", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetKpiValues(context.User)))
            .WithName("ListReportArrWorkspaceKpiValues");
        group.MapGet("/metric-values", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetMetricValues(context.User)))
            .WithName("ListReportArrWorkspaceMetricValues");
        group.MapGet("/analytics-snapshots", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetAnalyticsSnapshots(context.User)))
            .WithName("ListReportArrWorkspaceAnalyticsSnapshots");
        group.MapGet("/trend-analyses", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetTrendAnalyses(context.User)))
            .WithName("ListReportArrWorkspaceTrendAnalyses");
        group.MapGet("/exception-queries", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetExceptionQueries(context.User)))
            .WithName("ListReportArrWorkspaceExceptionQueries");
        group.MapGet("/exception-results", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetExceptionResults(context.User)))
            .WithName("ListReportArrWorkspaceExceptionResults");

        group.MapGet("/alerts", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetAlerts(context.User)))
            .WithName("ListReportArrWorkspaceAlerts");

        group.MapGet("/audit-packages", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetAuditPackages(context.User)))
            .WithName("ListReportArrWorkspaceAuditPackages");
        group.MapGet("/audit-scopes", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetAuditScopes(context.User)))
            .WithName("ListReportArrWorkspaceAuditScopes");

        group.MapGet("/source-connectors", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetSourceConnectors(context.User)))
            .WithName("ListReportArrWorkspaceSourceConnectors");
        group.MapGet("/ingestion-cursors", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetIngestionCursors(context.User)))
            .WithName("ListReportArrWorkspaceIngestionCursors");

        group.MapGet("/read-models", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetReadModels(context.User)))
            .WithName("ListReportArrWorkspaceReadModels");
        group.MapGet("/read-model-records", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetReadModelRecords(context.User)))
            .WithName("ListReportArrWorkspaceReadModelRecords");
        group.MapGet("/dataset-lineage", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetDatasetLineage(context.User)))
            .WithName("ListReportArrWorkspaceDatasetLineage");

        group.MapGet("/report-parameters", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetReportParameters(context.User)))
            .WithName("ListReportArrWorkspaceReportParameters");
        group.MapGet("/report-sections", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetReportSections(context.User)))
            .WithName("ListReportArrWorkspaceReportSections");
        group.MapGet("/report-recipients", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetReportRecipients(context.User)))
            .WithName("ListReportArrWorkspaceReportRecipients");

        group.MapGet("/refresh-jobs", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetRefreshJobs(context.User)))
            .WithName("ListReportArrWorkspaceRefreshJobs");
    }
}
