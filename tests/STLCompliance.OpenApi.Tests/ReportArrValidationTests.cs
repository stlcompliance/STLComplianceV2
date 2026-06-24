using ReportArr.Api.Data;
using ReportArr.Api.Endpoints;
using ReportArr.Api.Models;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;
using System.Security.Claims;
using System.Reflection;

namespace STLCompliance.OpenApi.Tests;

public sealed class ReportArrValidationTests
{
    [Fact]
    public void CreateReportDefinition_rejects_unknown_report_type()
    {
        var store = new ReportArrStore();

        var ex = Assert.Throws<StlApiException>(() =>
            store.CreateReportDefinition(
                CreatePrincipal(
                    personId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    roleKey: "report_builder",
                    entitlements: ["reportarr"]),
                new IntegrationEndpoints.CreateReportDefinitionRequest(
                    "quality-summary",
                    "Quality summary",
                    "Cross-suite quality summary.",
                    "unsupported",
                    "layout:grid:1x2",
                    ["pdf"],
                    "person-ops-analyst")));

        Assert.Equal("reportarr.reportType_invalid", ex.Code);
        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public void CreateExport_rejects_unknown_export_format()
    {
        var store = new ReportArrStore();

        var ex = Assert.Throws<StlApiException>(() =>
            store.CreateExport(
                CreatePrincipal(
                    personId: Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    roleKey: "report_runner",
                    entitlements: ["reportarr"]),
                new IntegrationEndpoints.CreateExportRequest(
                    "run-001",
                    "report",
                    null,
                    "docx",
                    "person-ops-analyst")));

        Assert.Equal("reportarr.exportFormat_invalid", ex.Code);
        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public void GetReportDefinition_returns_null_when_principal_cannot_access_report()
    {
        var store = new ReportArrStore();

        var report = store.GetReportDefinition(
            CreatePrincipal(
                personId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
                roleKey: "operations",
                entitlements: ["reportarr", "compliancecore"]),
            "rpt-002");

        Assert.Null(report);
    }

    [Fact]
    public void GetExportJob_returns_null_when_principal_cannot_access_source_run()
    {
        var store = new ReportArrStore();

        var export = store.GetExportJob(
            CreatePrincipal(
                personId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
                roleKey: "operations",
                entitlements: ["reportarr", "compliancecore"]),
            "exp-002");

        Assert.Null(export);
    }

    [Fact]
    public void GetKpiDefinitions_filters_out_inaccessible_source_products()
    {
        var store = new ReportArrStore();

        var kpis = store.GetKpiDefinitions(
            CreatePrincipal(
                personId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
                roleKey: "operations",
                entitlements: ["customarr"]));

        Assert.Empty(kpis);
    }

    [Fact]
    public void GetAlerts_filters_out_inaccessible_source_products()
    {
        var store = new ReportArrStore();

        var alerts = store.GetAlerts(
            CreatePrincipal(
                personId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
                roleKey: "operations",
                entitlements: ["customarr"]));

        Assert.Empty(alerts);
    }

    [Fact]
    public void GetReadModels_filters_out_inaccessible_source_products()
    {
        var store = new ReportArrStore();

        var models = store.GetReadModels(
            CreatePrincipal(
                personId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
                roleKey: "operations",
                entitlements: ["customarr"]));

        Assert.Empty(models);
    }

    [Fact]
    public void GetSourceConnectors_filters_out_inaccessible_source_products()
    {
        var store = new ReportArrStore();

        var connectors = store.GetSourceConnectors(
            CreatePrincipal(
                personId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
                roleKey: "operations",
                entitlements: ["customarr"]));

        Assert.Empty(connectors);
    }

    [Fact]
    public void GetDatasets_filters_out_inaccessible_source_products()
    {
        var store = new ReportArrStore();

        var datasets = store.GetDatasets(
            CreatePrincipal(
                personId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
                roleKey: "operations",
                entitlements: ["customarr"]));

        Assert.Empty(datasets);
    }

    [Fact]
    public void GetDatasetFields_filters_out_inaccessible_source_products()
    {
        var store = new ReportArrStore();

        var fields = store.GetDatasetFields(
            CreatePrincipal(
                personId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
                roleKey: "operations",
                entitlements: ["customarr"]));

        Assert.Empty(fields);
    }

    [Fact]
    public void GetDashboardFilters_filters_out_inaccessible_dashboards()
    {
        var store = new ReportArrStore();

        var filters = store.GetDashboardFilters(
            CreatePrincipal(
                personId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
                roleKey: "operations",
                entitlements: ["customarr"]));

        Assert.Empty(filters);
    }

    [Fact]
    public void GetDrilldowns_filters_out_inaccessible_dashboards()
    {
        var store = new ReportArrStore();

        var drilldowns = store.GetDrilldowns(
            CreatePrincipal(
                personId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
                roleKey: "operations",
                entitlements: ["customarr"]));

        Assert.Empty(drilldowns);
    }

    [Fact]
    public void GetSourceEvents_requires_platform_admin()
    {
        var store = new ReportArrStore();

        var events = store.GetSourceEvents(
            CreatePrincipal(
                personId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
                roleKey: "operations",
                entitlements: ["reportarr", "compliancecore"]));

        Assert.Empty(events);
    }

    [Fact]
    public void CreateDataset_rejects_principal_without_analytics_admin_role()
    {
        var store = new ReportArrStore();

        var ex = Assert.Throws<StlApiException>(() =>
            store.CreateDataset(
                CreatePrincipal(
                    personId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    roleKey: "operations",
                    entitlements: ["reportarr"]),
                new IntegrationEndpoints.CreateDatasetRequest(
                    "fleet-scorecard",
                    "Fleet scorecard",
                    "Operational scorecard dataset.",
                    "operational",
                    ["routarr"],
                    "person-ops-analyst")));

        Assert.Equal("reportarr.dataset_manage_forbidden", ex.Code);
        Assert.Equal(403, ex.StatusCode);
    }

    [Fact]
    public void RefreshDataset_rejects_principal_without_analytics_admin_role()
    {
        var store = new ReportArrStore();

        var ex = Assert.Throws<StlApiException>(() =>
            store.RefreshDataset(
                CreatePrincipal(
                    personId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    roleKey: "operations",
                    entitlements: ["reportarr"]),
                "ds-001",
                new IntegrationEndpoints.RefreshDatasetRequest("person-ops-analyst")));

        Assert.Equal("reportarr.dataset_refresh_forbidden", ex.Code);
        Assert.Equal(403, ex.StatusCode);
    }

    [Fact]
    public void RebuildReadModel_rejects_principal_without_analytics_admin_role()
    {
        var store = new ReportArrStore();

        var ex = Assert.Throws<StlApiException>(() =>
            store.RebuildReadModel(
                CreatePrincipal(
                    personId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    roleKey: "operations",
                    entitlements: ["reportarr"]),
                "rm-001",
                new IntegrationEndpoints.RebuildReadModelRequest("person-ops-analyst")));

        Assert.Equal("reportarr.read_model_rebuild_forbidden", ex.Code);
        Assert.Equal(403, ex.StatusCode);
    }

    [Fact]
    public void CreateDashboard_rejects_principal_without_report_builder_role()
    {
        var store = new ReportArrStore();

        var ex = Assert.Throws<StlApiException>(() =>
            store.CreateDashboard(
                CreatePrincipal(
                    personId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    roleKey: "operations",
                    entitlements: ["reportarr"]),
                new IntegrationEndpoints.CreateDashboardRequest(
                    "ops-summary",
                    "Ops summary",
                    "Operational dashboard.",
                    "operational",
                    "last_7_days",
                    "person-ops-analyst")));

        Assert.Equal("reportarr.report_build_forbidden", ex.Code);
        Assert.Equal(403, ex.StatusCode);
    }

    [Fact]
    public void CreateReportDefinition_rejects_principal_without_report_builder_role()
    {
        var store = new ReportArrStore();

        var ex = Assert.Throws<StlApiException>(() =>
            store.CreateReportDefinition(
                CreatePrincipal(
                    personId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    roleKey: "operations",
                    entitlements: ["reportarr"]),
                new IntegrationEndpoints.CreateReportDefinitionRequest(
                    "ops-summary",
                    "Ops summary",
                    "Operational report.",
                    "operational",
                    "layout:grid:2x2",
                    ["pdf"],
                    "person-ops-analyst")));

        Assert.Equal("reportarr.report_build_forbidden", ex.Code);
        Assert.Equal(403, ex.StatusCode);
    }

    [Fact]
    public void CreateAuditPackage_rejects_principal_without_compliance_reporter_role()
    {
        var store = new ReportArrStore();

        var ex = Assert.Throws<StlApiException>(() =>
            store.CreateAuditPackage(
                CreatePrincipal(
                    personId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    roleKey: "operations",
                    entitlements: ["reportarr", "compliancecore"]),
                new IntegrationEndpoints.CreateAuditPackageRequest(
                    "aud-scope-001",
                    "Audit package",
                    "Audit evidence package.",
                    "person-auditor")));

        Assert.Equal("reportarr.audit_package_forbidden", ex.Code);
        Assert.Equal(403, ex.StatusCode);
    }

    [Fact]
    public void CreateReportRun_rejects_principal_without_report_runner_role()
    {
        var store = new ReportArrStore();

        var ex = Assert.Throws<StlApiException>(() =>
            store.CreateReportRun(
                CreatePrincipal(
                    personId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    roleKey: "operations",
                    entitlements: ["reportarr"]),
                new IntegrationEndpoints.CreateReportRunRequest(
                    "rpt-001",
                    "person-ops-analyst",
                    null,
                    ["period=last_30_days"],
                    ["dateRange:last_30_days"])));

        Assert.Equal("reportarr.report_run_forbidden", ex.Code);
        Assert.Equal(403, ex.StatusCode);
    }

    [Fact]
    public void CreateReportSchedule_rejects_principal_without_report_scheduler_role()
    {
        var store = new ReportArrStore();

        var ex = Assert.Throws<StlApiException>(() =>
            store.CreateReportSchedule(
                CreatePrincipal(
                    personId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    roleKey: "operations",
                    entitlements: ["reportarr"]),
                new IntegrationEndpoints.CreateReportScheduleRequest(
                    "rpt-001",
                    "Weekly executive summary",
                    "weekly",
                    "America/Chicago",
                    null,
                    "email",
                    ["person-exec-lead"],
                    ["period=last_30_days"],
                    "person-ops-analyst")));

        Assert.Equal("reportarr.report_schedule_forbidden", ex.Code);
        Assert.Equal(403, ex.StatusCode);
    }

    [Fact]
    public void CreateReportSchedule_rejects_webhook_delivery_when_policy_disallows_external_delivery()
    {
        var store = new ReportArrStore();
        var ownerPrincipal = CreatePrincipal(
            personId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
            roleKey: "report_builder",
            entitlements: ["reportarr"]);
        var report = store.CreateReportDefinition(
            ownerPrincipal,
            new IntegrationEndpoints.CreateReportDefinitionRequest(
                "weekly-executive-summary",
                "Weekly executive summary",
                "Weekly leadership report.",
                "executive",
                "layout:grid:1x2",
                ["pdf"],
                ownerPrincipal.GetPersonId().ToString()));

        var ex = Assert.Throws<StlApiException>(() =>
            store.CreateReportSchedule(
                CreatePrincipal(
                    personId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    roleKey: "report_scheduler",
                    entitlements: ["reportarr"]),
                new IntegrationEndpoints.CreateReportScheduleRequest(
                    report.ReportDefinitionId,
                    "Weekly executive summary",
                    "weekly",
                    "America/Chicago",
                    null,
                    "webhook",
                    ["person-exec-lead"],
                    ["period=last_30_days"],
                    "person-ops-analyst")));

        Assert.Equal("reportarr.report_delivery_forbidden", ex.Code);
        Assert.Equal(403, ex.StatusCode);
    }

    [Fact]
    public void CreateReportSchedule_materializes_recipients_for_the_new_schedule()
    {
        var store = new ReportArrStore();
        var ownerPersonId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var ownerPrincipal = CreatePrincipal(
            personId: ownerPersonId,
            roleKey: "report_builder",
            entitlements: ["reportarr", "routarr", "reportarr.reports.read"]);
        var report = store.CreateReportDefinition(
            ownerPrincipal,
            new IntegrationEndpoints.CreateReportDefinitionRequest(
                "dispatch-performance",
                "Dispatch performance",
                "Dispatch performance report.",
                "operational",
                "layout:grid:1x3",
                ["pdf"],
                ownerPrincipal.GetPersonId().ToString()));
        var principal = CreatePrincipal(
            personId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
            roleKey: "report_scheduler",
            entitlements: ["reportarr", "routarr", "reportarr.reports.read"]);

        var schedule = store.CreateReportSchedule(
            principal,
            new IntegrationEndpoints.CreateReportScheduleRequest(
                report.ReportDefinitionId,
                "Daily dispatch performance",
                "daily",
                "America/Chicago",
                null,
                "email",
                ["person-compliance-reporter", "audit@example.test"],
                ["scope=site"],
                "person-compliance-reporter"));

        var adminRecipients = store.GetReportRecipients(
            CreatePrincipal(
                personId: Guid.Parse("99999999-9999-9999-9999-999999999999"),
                roleKey: "reportarr_admin",
                entitlements: ["reportarr"],
                isPlatformAdmin: true)).Where(recipient => recipient.ScheduleId == schedule.ScheduleId).ToList();

        Assert.Equal(2, adminRecipients.Count);
        Assert.Contains(adminRecipients, recipient => recipient.RecipientType == "person" && recipient.RecipientRef == "person-compliance-reporter");
        Assert.Contains(adminRecipients, recipient => recipient.RecipientType == "external" && recipient.Email == "audit@example.test");
    }

    [Fact]
    public void CreateExport_rejects_principal_without_report_runner_role()
    {
        var store = new ReportArrStore();

        var ex = Assert.Throws<StlApiException>(() =>
            store.CreateExport(
                CreatePrincipal(
                    personId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    roleKey: "operations",
                    entitlements: ["reportarr"]),
                new IntegrationEndpoints.CreateExportRequest(
                    "run-001",
                    "report",
                    null,
                    "pdf",
                    "person-ops-analyst")));

        Assert.Equal("reportarr.report_run_forbidden", ex.Code);
        Assert.Equal(403, ex.StatusCode);
    }

    [Fact]
    public void CreateDashboardExport_rejects_principal_without_dashboard_access()
    {
        var store = new ReportArrStore();

        var ex = Assert.Throws<StlApiException>(() =>
            store.CreateExport(
                CreatePrincipal(
                    personId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    roleKey: "report_runner",
                    entitlements: ["reportarr"]),
                new IntegrationEndpoints.CreateExportRequest(
                    null,
                    "dashboard",
                    "dash-003",
                    "pdf",
                    "person-ops-analyst")));

        Assert.Equal("reportarr.forbidden", ex.Code);
        Assert.Equal(403, ex.StatusCode);
    }

    [Fact]
    public void Dashboard_export_jobs_require_dashboard_access()
    {
        var store = new ReportArrStore();
        var creator = CreatePrincipal(
            personId: Guid.Parse("55555555-5555-5555-5555-555555555555"),
            roleKey: "report_builder",
            entitlements: ["reportarr"]);

        var dashboard = store.CreateDashboard(
            creator,
            new IntegrationEndpoints.CreateDashboardRequest(
                "finance-summary",
                "Finance summary",
                "Finance dashboard.",
                "executive",
                "last_30_days",
                creator.GetPersonId().ToString()));

        var export = store.CreateExport(
            creator,
            new IntegrationEndpoints.CreateExportRequest(
                null,
                "dashboard",
                dashboard.DashboardId,
                "pdf",
                creator.GetPersonId().ToString()));

        var unauthorized = CreatePrincipal(
            personId: Guid.Parse("66666666-6666-6666-6666-666666666666"),
            roleKey: "operations",
            entitlements: ["reportarr"]);

        Assert.Null(store.GetExportJob(unauthorized, export.ExportJobId));
        Assert.DoesNotContain(store.GetExportJobs(unauthorized), item => item.ExportJobId == export.ExportJobId);
    }

    [Fact]
    public void Dataset_export_jobs_require_dataset_access()
    {
        var store = new ReportArrStore();
        var creator = CreatePrincipal(
            personId: Guid.Parse("77777777-7777-7777-7777-777777777777"),
            roleKey: "analytics_admin",
            entitlements: ["reportarr", "routarr"]);

        var dataset = store.CreateDataset(
            creator,
            new IntegrationEndpoints.CreateDatasetRequest(
                "route-exceptions",
                "Route exceptions",
                "Transportation operational dataset.",
                "operational",
                ["routarr"],
                creator.GetPersonId().ToString()));

        var exportPrincipal = CreatePrincipal(
            personId: Guid.Parse("77777777-7777-7777-7777-777777777777"),
            roleKey: "report_runner",
            entitlements: ["reportarr", "routarr"]);

        var export = store.CreateExport(
            exportPrincipal,
            new IntegrationEndpoints.CreateExportRequest(
                null,
                "dataset",
                dataset.DatasetId,
                "csv",
                exportPrincipal.GetPersonId().ToString()));

        var unauthorized = CreatePrincipal(
            personId: Guid.Parse("88888888-8888-8888-8888-888888888888"),
            roleKey: "operations",
            entitlements: ["reportarr"]);

        Assert.Null(store.GetExportJob(unauthorized, export.ExportJobId));
        Assert.DoesNotContain(store.GetExportJobs(unauthorized), item => item.ExportJobId == export.ExportJobId);
    }

    [Fact]
    public void GetKpiValues_filters_out_inaccessible_source_products()
    {
        var store = new ReportArrStore();

        var values = store.GetKpiValues(
            CreatePrincipal(
                personId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
                roleKey: "operations",
                entitlements: ["customarr"]));

        Assert.Empty(values);
    }

    [Fact]
    public void GetExceptionResults_filters_out_inaccessible_source_products()
    {
        var store = new ReportArrStore();

        var results = store.GetExceptionResults(
            CreatePrincipal(
                personId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
                roleKey: "operations",
                entitlements: ["customarr"]));

        Assert.Empty(results);
    }

    [Fact]
    public void GetAuditPackages_filters_out_inaccessible_source_products()
    {
        var store = new ReportArrStore();

        var packages = store.GetAuditPackages(
            CreatePrincipal(
                personId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
                roleKey: "operations",
                entitlements: ["customarr"]));

        Assert.Empty(packages);
    }

    [Fact]
    public void AcknowledgeAlert_rejects_principal_without_source_product_access()
    {
        var store = new ReportArrStore();
        AddDataset(store, new ReportArrDatasetResponse(
            "ds-test-001",
            "DS-TEST-001",
            "routarr-test",
            "Route data issue dataset",
            "Transportation dataset for access control testing.",
            "operational",
            "active",
            "scheduled",
            "daily",
            "fresh",
            ["routarr"],
            ["src-test-001"],
            DateTimeOffset.UtcNow.AddMinutes(-15),
            DateTimeOffset.UtcNow.AddMinutes(-15),
            null,
            "v1",
            [],
            "source-traceability-required",
            "retain-90-days",
            "person-analytics-lead",
            DateTimeOffset.UtcNow.AddHours(-1),
            DateTimeOffset.UtcNow.AddMinutes(-15),
            TenantId: Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa").ToString()));
        AddAlert(store, new ReportArrAlertResponse(
            "alrt-test-001",
            "ALRT-TEST-001",
            "Route data issue",
            "Transportation alert for access control testing.",
            "stale_data",
            "triggered",
            "ds-test-001",
            "met-test-001",
            "freshness <= threshold",
            "high",
            DateTimeOffset.UtcNow.AddMinutes(-10),
            null,
            null,
            null,
            ["notif-test-001"],
            TenantId: Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa").ToString()));

        var ex = Assert.Throws<StlApiException>(() =>
            store.AcknowledgeAlert(
                CreatePrincipal(
                    personId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    roleKey: "operations",
                    entitlements: ["customarr"]),
                "alrt-test-001",
                new IntegrationEndpoints.AcknowledgeAlertRequest("person-ops-analyst")));

        Assert.Equal("reportarr.forbidden", ex.Code);
        Assert.Equal(403, ex.StatusCode);
    }

    [Fact]
    public void UpdateDashboard_rejects_principal_without_dashboard_update_permission()
    {
        var store = new ReportArrStore();
        var ownerPrincipal = CreatePrincipal(
            personId: Guid.Parse("22222222-2222-2222-2222-222222222222"),
            roleKey: "report_builder",
            entitlements: ["reportarr", "routarr"]);
        var dashboard = store.CreateDashboard(
            ownerPrincipal,
            new IntegrationEndpoints.CreateDashboardRequest(
                "transport-summary",
                "Transport summary",
                "Transportation dashboard.",
                "transportation",
                "last_7_days",
                ownerPrincipal.GetPersonId().ToString()));

        var ex = Assert.Throws<StlApiException>(() =>
            store.UpdateDashboard(
                CreatePrincipal(
                    personId: Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    roleKey: "operations",
                    entitlements: ["reportarr", "routarr"]),
                dashboard.DashboardId,
                new IntegrationEndpoints.UpdateDashboardRequest(
                    "Transportation board",
                    "Transportation dashboard.",
                    "active",
                    "last_7_days")));

        Assert.Equal("reportarr.dashboard_update_forbidden", ex.Code);
        Assert.Equal(403, ex.StatusCode);
    }

    [Fact]
    public void CreateDashboard_grants_creator_dashboard_update_access()
    {
        var store = new ReportArrStore();
        var principal = CreatePrincipal(
            personId: Guid.Parse("22222222-2222-2222-2222-222222222222"),
            roleKey: "report_builder",
            entitlements: ["reportarr"]);

        var dashboard = store.CreateDashboard(
            principal,
            new IntegrationEndpoints.CreateDashboardRequest(
                "transport-summary",
                "Transport summary",
                "Transportation dashboard.",
                "transportation",
                "last_7_days",
                principal.GetPersonId().ToString()));

        var updated = store.UpdateDashboard(
            principal,
            dashboard.DashboardId,
            new IntegrationEndpoints.UpdateDashboardRequest(
                "Transport summary refreshed",
                "Transportation dashboard refreshed.",
                "active",
                "last_30_days"));

        Assert.Equal("active", updated.Status);
        Assert.Equal("Transport summary refreshed", updated.Title);
        Assert.Equal(principal.GetPersonId().ToString(), updated.UpdatedByPersonId);
    }

    [Fact]
    public void UpdateReportDefinition_rejects_principal_without_report_update_permission()
    {
        var store = new ReportArrStore();
        var ownerPrincipal = CreatePrincipal(
            personId: Guid.Parse("33333333-3333-3333-3333-333333333333"),
            roleKey: "report_builder",
            entitlements: ["reportarr", "routarr"]);
        var report = store.CreateReportDefinition(
            ownerPrincipal,
            new IntegrationEndpoints.CreateReportDefinitionRequest(
                "transport-summary",
                "Transport summary",
                "Transportation report.",
                "operational",
                "layout:grid:2x2",
                ["pdf"],
                ownerPrincipal.GetPersonId().ToString()));

        var ex = Assert.Throws<StlApiException>(() =>
            store.UpdateReportDefinition(
                CreatePrincipal(
                    personId: Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    roleKey: "operations",
                    entitlements: ["reportarr", "routarr"]),
                report.ReportDefinitionId,
                new IntegrationEndpoints.UpdateReportDefinitionRequest(
                    "active",
                    "person-dispatch-lead")));

        Assert.Equal("reportarr.report_update_forbidden", ex.Code);
        Assert.Equal(403, ex.StatusCode);
    }

    [Fact]
    public void CreateReportDefinition_grants_creator_report_update_access()
    {
        var store = new ReportArrStore();
        var principal = CreatePrincipal(
            personId: Guid.Parse("33333333-3333-3333-3333-333333333333"),
            roleKey: "report_builder",
            entitlements: ["reportarr"]);

        var report = store.CreateReportDefinition(
            principal,
            new IntegrationEndpoints.CreateReportDefinitionRequest(
                "transport-summary",
                "Transport summary",
                "Transportation report.",
                "operational",
                "layout:grid:2x2",
                ["pdf"],
                principal.GetPersonId().ToString()));

        var updated = store.UpdateReportDefinition(
            principal,
            report.ReportDefinitionId,
            new IntegrationEndpoints.UpdateReportDefinitionRequest(
                "active",
                "person-transport-lead"));

        Assert.Equal("active", updated.Status);
        Assert.Equal("person-transport-lead", updated.UpdatedByPersonId);
    }

    [Fact]
    public void Tenant_scoping_hides_reportarr_entities_from_other_tenants()
    {
        var store = new ReportArrStore();
        var tenantAPrincipal = CreatePrincipal(
            personId: Guid.Parse("55555555-5555-5555-5555-555555555555"),
            roleKey: "report_builder",
            entitlements: ["reportarr"],
            tenantId: "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var tenantBPrincipal = CreatePrincipal(
            personId: Guid.Parse("66666666-6666-6666-6666-666666666666"),
            roleKey: "report_builder",
            entitlements: ["reportarr"],
            tenantId: "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

        var dashboard = store.CreateDashboard(
            tenantAPrincipal,
            new IntegrationEndpoints.CreateDashboardRequest(
                "tenant-a-dashboard",
                "Tenant A dashboard",
                "Tenant A only dashboard.",
                "transportation",
                "last_7_days",
                tenantAPrincipal.GetPersonId().ToString()));
        var report = store.CreateReportDefinition(
            tenantAPrincipal,
            new IntegrationEndpoints.CreateReportDefinitionRequest(
                "tenant-a-report",
                "Tenant A report",
                "Tenant A only report.",
                "operational",
                "layout:grid:1x1",
                ["pdf"],
                tenantAPrincipal.GetPersonId().ToString()));

        Assert.Null(store.GetDashboard(tenantBPrincipal, dashboard.DashboardId));
        Assert.Null(store.GetReportDefinition(tenantBPrincipal, report.ReportDefinitionId));
        Assert.DoesNotContain(store.GetDashboards(tenantBPrincipal), item => item.DashboardId == dashboard.DashboardId);
        Assert.DoesNotContain(store.GetReportDefinitions(tenantBPrincipal), item => item.ReportDefinitionId == report.ReportDefinitionId);
    }

    [Fact]
    public void GetDashboard_records_dashboard_view_timestamp()
    {
        var store = new ReportArrStore();
        var principal = CreatePrincipal(
            personId: Guid.Parse("44444444-4444-4444-4444-444444444444"),
            roleKey: "report_builder",
            entitlements: ["reportarr"]);

        var dashboard = store.CreateDashboard(
            principal,
            new IntegrationEndpoints.CreateDashboardRequest(
                "ops-summary",
                "Ops summary",
                "Operational dashboard.",
                "transportation",
                "last_7_days",
                principal.GetPersonId().ToString()));

        Assert.Null(dashboard.LastViewedAt);

        var viewed = store.GetDashboard(principal, dashboard.DashboardId);

        Assert.NotNull(viewed);
        Assert.NotNull(viewed!.LastViewedAt);
    }

    [Fact]
    public void RenderWidget_rejects_principal_without_dashboard_access()
    {
        var store = new ReportArrStore();
        var ownerPrincipal = CreatePrincipal(
            personId: Guid.Parse("22222222-2222-2222-2222-222222222222"),
            roleKey: "report_builder",
            entitlements: ["reportarr", "routarr"]);
        var dashboard = store.CreateDashboard(
            ownerPrincipal,
            new IntegrationEndpoints.CreateDashboardRequest(
                "transport-summary",
                "Transport summary",
                "Transportation dashboard.",
                "transportation",
                "last_7_days",
                ownerPrincipal.GetPersonId().ToString()));
        AddWidget(store, new ReportArrDashboardWidgetResponse(
            "wid-test-001",
            dashboard.DashboardId,
            "w-transport-summary",
            "Transport summary",
            "Access-control test widget.",
            "metric",
            "active",
            "ds-test-001",
            "rm-test-001",
            "count(open blockers)",
            ["status != closed"],
            "rpt-test-001",
            1,
            "grid:1x1",
            "number:transport-summary",
            "fresh",
            null,
            TenantId: Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa").ToString()));

        var ex = Assert.Throws<StlApiException>(() =>
            store.RenderWidget(
                CreatePrincipal(
                    personId: Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    roleKey: "operations",
                    entitlements: ["reportarr"]),
                "wid-test-001"));

        Assert.Equal("reportarr.forbidden", ex.Code);
        Assert.Equal(403, ex.StatusCode);
    }

    [Fact]
    public void ReceiveEvents_requires_at_least_one_event()
    {
        var store = new ReportArrStore();

        var ex = Assert.Throws<StlApiException>(() =>
            store.ReceiveEvents(
                CreatePrincipal(
                    personId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    roleKey: "analytics_admin",
                    entitlements: ["reportarr"]),
                new IntegrationEndpoints.SourceEventBatchRequest([])));

        Assert.Equal("reportarr.source_events_required", ex.Code);
        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public void ReceiveEvent_records_failed_receipt_for_unsupported_event_type()
    {
        var store = new ReportArrStore();
        AddSourceConnector(store, new ReportArrSourceConnectorResponse(
            "src-test-001",
            "routarr",
            "api_poll",
            "active",
            "svc-routarr-test",
            DateTimeOffset.UtcNow.AddMinutes(-5),
            null,
            null,
            ["trip.completed", "trip.exception"],
            ["ds-test-001"],
            TenantId: Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa").ToString()));

        var receipt = store.ReceiveEvent(
            CreatePrincipal(
                personId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
                roleKey: "analytics_admin",
                entitlements: ["reportarr"]),
            new IntegrationEndpoints.SourceEventRequest(
                "routarr",
                "evt-9999",
                "trip.delayed",
                "trip-9999",
                "corr-9999"));

        Assert.Equal("failed", receipt.Status);
        Assert.Contains("not supported", receipt.FailureReason ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Null(receipt.ProcessedAt);
    }

    [Fact]
    public void ReceiveEvent_rejects_principal_without_integration_access()
    {
        var store = new ReportArrStore();

        var ex = Assert.Throws<StlApiException>(() =>
            store.ReceiveEvent(
                CreatePrincipal(
                    personId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    roleKey: "operations",
                    entitlements: ["reportarr"]),
                new IntegrationEndpoints.SourceEventRequest(
                    "routarr",
                    "evt-9998",
                    "trip.completed",
                    "trip-9998",
                    "corr-9998")));

        Assert.Equal("reportarr.source_event_receive_forbidden", ex.Code);
        Assert.Equal(403, ex.StatusCode);
    }

    private static void AddAlert(ReportArrStore store, ReportArrAlertResponse alert)
    {
        GetPrivateList<ReportArrAlertResponse>(store, "_alerts").Add(alert);
    }

    private static void AddDataset(ReportArrStore store, ReportArrDatasetResponse dataset)
    {
        GetPrivateList<ReportArrDatasetResponse>(store, "_datasets").Add(dataset);
    }

    private static void AddSourceConnector(ReportArrStore store, ReportArrSourceConnectorResponse connector)
    {
        GetPrivateList<ReportArrSourceConnectorResponse>(store, "_sourceConnectors").Add(connector);
    }

    private static void AddWidget(ReportArrStore store, ReportArrDashboardWidgetResponse widget)
    {
        GetPrivateList<ReportArrDashboardWidgetResponse>(store, "_widgets").Add(widget);
    }

    private static List<T> GetPrivateList<T>(ReportArrStore store, string fieldName)
    {
        var field = typeof(ReportArrStore).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Missing field {fieldName}.");
        return (List<T>)field.GetValue(store)!;
    }

    private static ClaimsPrincipal CreatePrincipal(
        Guid personId,
        string roleKey,
        IReadOnlyList<string> entitlements,
        bool isPlatformAdmin = false,
        string? tenantId = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, personId.ToString()),
            new(StlClaimTypes.PersonId, personId.ToString()),
            new(StlClaimTypes.TenantId, (tenantId is null ? Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa") : Guid.Parse(tenantId)).ToString()),
            new(StlClaimTypes.SessionId, Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb").ToString()),
            new(StlClaimTypes.TenantRoleKey, roleKey),
            new(StlClaimTypes.PlatformAdmin, isPlatformAdmin.ToString()),
            new(StlClaimTypes.Entitlements, string.Join(',', entitlements)),
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }
}
