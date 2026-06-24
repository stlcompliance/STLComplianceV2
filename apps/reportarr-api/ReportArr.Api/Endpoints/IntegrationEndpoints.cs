using ReportArr.Api.Data;
using STLCompliance.Shared.Auth;

namespace ReportArr.Api.Endpoints;

public static class IntegrationEndpoints
{
    public static void MapReportArrIntegrationEndpoints(this WebApplication app)
    {
        MapRoutes(app.MapGroup("/api/integrations"), "/api/integrations");
        MapRoutes(app.MapGroup("/api/v1/integrations"), "/api/v1/integrations");
    }

    private static void MapRoutes(RouteGroupBuilder group, string routePrefix)
    {
        group.WithTags("Integrations").RequireAuthorization();

        group.MapGet("/datasets", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetDatasets(context.User)))
            .WithName($"ListReportArrIntegrationDatasets{routePrefix}");
        group.MapGet("/dataset-fields", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetDatasetFields(context.User)))
            .WithName($"ListReportArrIntegrationDatasetFields{routePrefix}");
        group.MapGet("/datasets/{datasetId}", (HttpContext context, string datasetId, ReportArrStore store) =>
        {
            var item = store.GetDataset(context.User, datasetId);
            return item is null ? Results.NotFound() : Results.Ok(item);
        }).WithName($"GetReportArrIntegrationDataset{routePrefix}");
        group.MapPost("/datasets", (HttpContext context, CreateDatasetRequest request, ReportArrStore store) =>
        {
            var created = store.CreateDataset(context.User, request);
            return Results.Created($"{routePrefix}/datasets/{created.DatasetId}", created);
        })
            .WithName($"CreateReportArrIntegrationDataset{routePrefix}");
        group.MapPost("/datasets/{datasetId}/refresh", (HttpContext context, string datasetId, RefreshDatasetRequest request, ReportArrStore store) =>
            Results.Ok(store.RefreshDataset(context.User, datasetId, request)))
            .WithName($"RefreshReportArrIntegrationDataset{routePrefix}");

        group.MapGet("/read-models", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetReadModels(context.User)))
            .WithName($"ListReportArrIntegrationReadModels{routePrefix}");
        group.MapGet("/read-model-records", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetReadModelRecords(context.User)))
            .WithName($"ListReportArrIntegrationReadModelRecords{routePrefix}");
        group.MapGet("/dataset-lineage", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetDatasetLineage(context.User)))
            .WithName($"ListReportArrIntegrationDatasetLineage{routePrefix}");
        group.MapGet("/read-models/{readModelId}", (HttpContext context, string readModelId, ReportArrStore store) =>
        {
            var item = store.GetReadModel(context.User, readModelId);
            return item is null ? Results.NotFound() : Results.Ok(item);
        }).WithName($"GetReportArrIntegrationReadModel{routePrefix}");
        group.MapPost("/read-models/{readModelId}/rebuild", (HttpContext context, string readModelId, RebuildReadModelRequest request, ReportArrStore store) =>
            Results.Ok(store.RebuildReadModel(context.User, readModelId, request)))
            .WithName($"RebuildReportArrIntegrationReadModel{routePrefix}");

        group.MapPost("/events", (HttpContext context, SourceEventRequest request, ReportArrStore store) =>
            Results.Ok(store.ReceiveEvent(context.User, request)))
            .WithName($"ReceiveReportArrIntegrationEvent{routePrefix}");
        group.MapPost("/events/batch", (HttpContext context, SourceEventBatchRequest request, ReportArrStore store) =>
            Results.Ok(store.ReceiveEvents(context.User, request)))
            .WithName($"ReceiveReportArrIntegrationEvents{routePrefix}");

        group.MapGet("/dashboards", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetDashboards(context.User)))
            .WithName($"ListReportArrIntegrationDashboards{routePrefix}");
        group.MapGet("/dashboards/{dashboardId}", (HttpContext context, string dashboardId, ReportArrStore store) =>
        {
            var item = store.GetDashboard(context.User, dashboardId);
            return item is null ? Results.NotFound() : Results.Ok(item);
        }).WithName($"GetReportArrIntegrationDashboard{routePrefix}");
        group.MapGet("/dashboard-access-policies", (HttpContext context, ReportArrStore store) =>
            context.User.IsPlatformAdmin() ? Results.Ok(store.GetDashboardAccessPolicies(context.User)) : Results.Forbid())
            .WithName($"ListReportArrIntegrationDashboardAccessPolicies{routePrefix}");
        group.MapGet("/dashboard-filters", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetDashboardFilters(context.User)))
            .WithName($"ListReportArrIntegrationDashboardFilters{routePrefix}");
        group.MapGet("/drilldowns", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetDrilldowns(context.User)))
            .WithName($"ListReportArrIntegrationDrilldowns{routePrefix}");
        group.MapPost("/dashboards", (HttpContext context, CreateDashboardRequest request, ReportArrStore store) =>
        {
            var created = store.CreateDashboard(context.User, request);
            return Results.Created($"{routePrefix}/dashboards/{created.DashboardId}", created);
        })
            .WithName($"CreateReportArrIntegrationDashboard{routePrefix}");
        group.MapPatch("/dashboards/{dashboardId}", (HttpContext context, string dashboardId, UpdateDashboardRequest request, ReportArrStore store) =>
            Results.Ok(store.UpdateDashboard(context.User, dashboardId, request)))
            .WithName($"UpdateReportArrIntegrationDashboard{routePrefix}");
        group.MapGet("/widgets/{widgetId}/render", (HttpContext context, string widgetId, ReportArrStore store) => Results.Ok(store.RenderWidget(context.User, widgetId)))
            .WithName($"RenderReportArrIntegrationWidget{routePrefix}");

        group.MapGet("/report-definitions", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetReportDefinitions(context.User)))
            .WithName($"ListReportArrIntegrationReportDefinitions{routePrefix}");
        group.MapGet("/report-definitions/{reportDefinitionId}", (HttpContext context, string reportDefinitionId, ReportArrStore store) =>
        {
            var item = store.GetReportDefinition(context.User, reportDefinitionId);
            return item is null ? Results.NotFound() : Results.Ok(item);
        }).WithName($"GetReportArrIntegrationReportDefinition{routePrefix}");
        group.MapGet("/report-access-policies", (HttpContext context, ReportArrStore store) =>
            context.User.IsPlatformAdmin() ? Results.Ok(store.GetReportAccessPolicies(context.User)) : Results.Forbid())
            .WithName($"ListReportArrIntegrationReportAccessPolicies{routePrefix}");
        group.MapPost("/report-definitions", (HttpContext context, CreateReportDefinitionRequest request, ReportArrStore store) =>
        {
            var created = store.CreateReportDefinition(context.User, request);
            return Results.Created($"{routePrefix}/report-definitions/{created.ReportDefinitionId}", created);
        })
            .WithName($"CreateReportArrIntegrationReportDefinition{routePrefix}");
        group.MapPatch("/report-definitions/{reportDefinitionId}", (HttpContext context, string reportDefinitionId, UpdateReportDefinitionRequest request, ReportArrStore store) =>
            Results.Ok(store.UpdateReportDefinition(context.User, reportDefinitionId, request)))
            .WithName($"UpdateReportArrIntegrationReportDefinition{routePrefix}");

        group.MapPost("/report-runs", (HttpContext context, CreateReportRunRequest request, ReportArrStore store) =>
        {
            var created = store.CreateReportRun(context.User, request);
            return Results.Created($"{routePrefix}/report-runs/{created.ReportRunId}", created);
        })
            .WithName($"CreateReportArrIntegrationReportRun{routePrefix}");
        group.MapGet("/report-runs", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetReportRuns(context.User)))
            .WithName($"ListReportArrIntegrationReportRuns{routePrefix}");
        group.MapGet("/report-runs/{reportRunId}", (HttpContext context, string reportRunId, ReportArrStore store) =>
        {
            var item = store.GetReportRun(context.User, reportRunId);
            return item is null ? Results.NotFound() : Results.Ok(item);
        }).WithName($"GetReportArrIntegrationReportRun{routePrefix}");
        group.MapPost("/report-runs/{reportRunId}/cancel", (HttpContext context, string reportRunId, CancelReportRunRequest request, ReportArrStore store) =>
            Results.Ok(store.CancelReportRun(context.User, reportRunId, request)))
            .WithName($"CancelReportArrIntegrationReportRun{routePrefix}");

        group.MapGet("/report-schedules", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetReportSchedules(context.User)))
            .WithName($"ListReportArrIntegrationReportSchedules{routePrefix}");
        group.MapGet("/report-recipients", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetReportRecipients(context.User)))
            .WithName($"ListReportArrIntegrationReportRecipients{routePrefix}");
        group.MapPost("/report-schedules", (HttpContext context, CreateReportScheduleRequest request, ReportArrStore store) =>
        {
            var created = store.CreateReportSchedule(context.User, request);
            return Results.Created($"{routePrefix}/report-schedules/{created.ScheduleId}", created);
        })
            .WithName($"CreateReportArrIntegrationReportSchedule{routePrefix}");
        group.MapPatch("/report-schedules/{scheduleId}", (HttpContext context, string scheduleId, UpdateReportScheduleRequest request, ReportArrStore store) =>
            Results.Ok(store.UpdateReportSchedule(context.User, scheduleId, request)))
            .WithName($"UpdateReportArrIntegrationReportSchedule{routePrefix}");

        group.MapPost("/exports", (HttpContext context, CreateExportRequest request, ReportArrStore store) =>
        {
            var created = store.CreateExport(context.User, request);
            return Results.Created($"{routePrefix}/exports/{created.ExportJobId}", created);
        })
            .WithName($"CreateReportArrIntegrationExport{routePrefix}");
        group.MapGet("/exports", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetExportJobs(context.User)))
            .WithName($"ListReportArrIntegrationExports{routePrefix}");
        group.MapGet("/exports/{exportJobId}", (HttpContext context, string exportJobId, ReportArrStore store) =>
        {
            var item = store.GetExportJob(context.User, exportJobId);
            return item is null ? Results.NotFound() : Results.Ok(item);
        }).WithName($"GetReportArrIntegrationExport{routePrefix}");

        group.MapGet("/kpis", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetKpiDefinitions(context.User)))
            .WithName($"ListReportArrIntegrationKpis{routePrefix}");
        group.MapGet("/kpi-values", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetKpiValues(context.User)))
            .WithName($"ListReportArrIntegrationKpiValues{routePrefix}");
        group.MapGet("/kpis/{kpiId}", (HttpContext context, string kpiId, ReportArrStore store) =>
        {
            var item = store.GetKpiDefinition(context.User, kpiId);
            return item is null ? Results.NotFound() : Results.Ok(item);
        }).WithName($"GetReportArrIntegrationKpi{routePrefix}");
        group.MapPost("/kpis/{kpiId}/calculate", (HttpContext context, string kpiId, CalculateKpiRequest request, ReportArrStore store) =>
            Results.Ok(store.CalculateKpi(context.User, kpiId, request)))
            .WithName($"CalculateReportArrIntegrationKpi{routePrefix}");
        group.MapGet("/metric-values", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetMetricValues(context.User)))
            .WithName($"ListReportArrIntegrationMetricValues{routePrefix}");
        group.MapGet("/analytics-snapshots", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetAnalyticsSnapshots(context.User)))
            .WithName($"ListReportArrIntegrationAnalyticsSnapshots{routePrefix}");
        group.MapGet("/trend-analyses", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetTrendAnalyses(context.User)))
            .WithName($"ListReportArrIntegrationTrendAnalyses{routePrefix}");
        group.MapGet("/exception-queries", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetExceptionQueries(context.User)))
            .WithName($"ListReportArrIntegrationExceptionQueries{routePrefix}");
        group.MapGet("/exception-results", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetExceptionResults(context.User)))
            .WithName($"ListReportArrIntegrationExceptionResults{routePrefix}");

        group.MapPost("/audit-packages", (HttpContext context, CreateAuditPackageRequest request, ReportArrStore store) =>
        {
            var created = store.CreateAuditPackage(context.User, request);
            return Results.Created($"{routePrefix}/audit-packages/{created.AuditReportPackageId}", created);
        })
            .WithName($"CreateReportArrIntegrationAuditPackage{routePrefix}");
        group.MapGet("/audit-scopes", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetAuditScopes(context.User)))
            .WithName($"ListReportArrIntegrationAuditScopes{routePrefix}");
        group.MapGet("/audit-packages/{auditReportPackageId}", (HttpContext context, string auditReportPackageId, ReportArrStore store) =>
        {
            var item = store.GetAuditPackage(context.User, auditReportPackageId);
            return item is null ? Results.NotFound() : Results.Ok(item);
        }).WithName($"GetReportArrIntegrationAuditPackage{routePrefix}");
        group.MapPost("/audit-packages/{auditReportPackageId}/lock", (HttpContext context, string auditReportPackageId, LockAuditPackageRequest request, ReportArrStore store) =>
            Results.Ok(store.LockAuditPackage(context.User, auditReportPackageId, request)))
            .WithName($"LockReportArrIntegrationAuditPackage{routePrefix}");

        group.MapGet("/alerts", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetAlerts(context.User)))
            .WithName($"ListReportArrIntegrationAlerts{routePrefix}");
        group.MapPost("/alerts/{alertId}/acknowledge", (HttpContext context, string alertId, AcknowledgeAlertRequest request, ReportArrStore store) =>
            Results.Ok(store.AcknowledgeAlert(context.User, alertId, request)))
            .WithName($"AcknowledgeReportArrIntegrationAlert{routePrefix}");
        group.MapPost("/alerts/{alertId}/resolve", (HttpContext context, string alertId, ResolveAlertRequest request, ReportArrStore store) =>
            Results.Ok(store.ResolveAlert(context.User, alertId, request)))
            .WithName($"ResolveReportArrIntegrationAlert{routePrefix}");

        group.MapGet("/widgets", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetWidgets(context.User)))
            .WithName($"ListReportArrIntegrationWidgets{routePrefix}");
        group.MapGet("/widget-visualizations", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetWidgetVisualizations(context.User)))
            .WithName($"ListReportArrIntegrationWidgetVisualizations{routePrefix}");
        group.MapGet("/source-events", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetSourceEvents(context.User)))
            .WithName($"ListReportArrIntegrationSourceEvents{routePrefix}");
        group.MapGet("/metrics", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetMetricDefinitions(context.User)))
            .WithName($"ListReportArrIntegrationMetrics{routePrefix}");
        group.MapGet("/ingestion-cursors", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetIngestionCursors(context.User)))
            .WithName($"ListReportArrIntegrationIngestionCursors{routePrefix}");
        group.MapGet("/report-parameters", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetReportParameters(context.User)))
            .WithName($"ListReportArrIntegrationReportParameters{routePrefix}");
        group.MapGet("/report-sections", (HttpContext context, ReportArrStore store) => Results.Ok(store.GetReportSections(context.User)))
            .WithName($"ListReportArrIntegrationReportSections{routePrefix}");
    }

    public sealed record CreateDatasetRequest(
        string DatasetKey,
        string Title,
        string Description,
        string DatasetType,
        IReadOnlyList<string> SourceProducts,
        string OwnerPersonId);

    public sealed record RefreshDatasetRequest(string RequestedByPersonId);
    public sealed record RebuildReadModelRequest(string RequestedByPersonId);
    public sealed record SourceEventRequest(string SourceProduct, string SourceEventId, string EventType, string? SourceObjectRef, string? CorrelationId);
    public sealed record SourceEventBatchRequest(IReadOnlyList<SourceEventRequest> Events);
    public sealed record CreateDashboardRequest(string DashboardKey, string Title, string Description, string DashboardType, string DefaultDateRange, string OwnerPersonId);
    public sealed record UpdateDashboardRequest(string Title, string Description, string Status, string DefaultDateRange);
    public sealed record CreateReportDefinitionRequest(string ReportKey, string Title, string Description, string ReportType, string LayoutDefinition, IReadOnlyList<string> ExportFormats, string OwnerPersonId);
    public sealed record UpdateReportDefinitionRequest(string Status, string RequestedByPersonId);
    public sealed record CreateReportRunRequest(string ReportDefinitionId, string RequestedByPersonId, string? ExportFormat, IReadOnlyList<string> ParametersUsed, IReadOnlyList<string> FiltersUsed);
    public sealed record CancelReportRunRequest(string RequestedByPersonId, string? Reason);
    public sealed record CreateReportScheduleRequest(string ReportDefinitionId, string Title, string Cadence, string Timezone, string? CronExpression, string DeliveryMethod, IReadOnlyList<string> Recipients, IReadOnlyList<string> Parameters, string RequestedByPersonId);
    public sealed record UpdateReportScheduleRequest(string Status, string Cadence, DateTimeOffset? NextRunAt, string RequestedByPersonId);
    public sealed record CreateExportRequest(string? ReportRunId, string? ExportType, string? SourceRef, string ExportFormat, string RequestedByPersonId);
    public sealed record CalculateKpiRequest(DateTimeOffset PeriodStart, DateTimeOffset PeriodEnd, string RequestedByPersonId);
    public sealed record CreateAuditPackageRequest(string AuditScopeId, string Title, string Description, string RequestedByPersonId);
    public sealed record LockAuditPackageRequest(string RequestedByPersonId);
    public sealed record AcknowledgeAlertRequest(string RequestedByPersonId);
    public sealed record ResolveAlertRequest(string RequestedByPersonId);
}
