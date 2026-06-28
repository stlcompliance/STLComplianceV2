using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReportArr.Api.Endpoints;
using ReportArr.Api.Models;
using ReportArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace ReportArr.Api.Data;

public sealed class ReportArrStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly FieldInfo[] SnapshotListFields = typeof(ReportArrStore)
        .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
        .Where(field => field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(List<>))
        .ToArray();
    private const string SnapshotTableName = "reportarr_store_snapshots";
    private const string SnapshotRowKey = "reportarr";

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
    private readonly IServiceScopeFactory? _serviceScopeFactory;
    private bool _snapshotLoaded;

    private object Gate
    {
        get
        {
            EnsureSnapshotLoaded();
            return _gate;
        }
    }

    public ReportArrStore()
    {
        var now = DateTimeOffset.UtcNow;
        (_datasets, _datasetFields, _sourceConnectors, _ingestionCursors, _sourceEvents, _readModels, _readModelRecords, _datasetLineage, _dashboards, _dashboardAccessPolicies, _dashboardFilters, _drilldowns, _widgets, _widgetVisualizations, _reportDefinitions, _reportAccessPolicies, _reportParameters, _reportSections, _reportRuns, _reportSchedules, _reportRecipients, _exportJobs, _metrics, _metricValues, _analyticsSnapshots, _trendAnalyses, _exceptionQueries, _exceptionResults, _kpis, _kpiValues, _alerts, _auditScopes, _auditPackages, _refreshJobs)
            = ([], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], []);

    }

    public ReportArrStore(IServiceScopeFactory serviceScopeFactory) : this()
    {
        _serviceScopeFactory = serviceScopeFactory;
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

    private string CurrentTenantId(ClaimsPrincipal principal) => principal.GetTenantId().ToString("D");

    private bool IsCurrentTenant(ClaimsPrincipal principal, string tenantId) =>
        string.Equals(tenantId, CurrentTenantId(principal), StringComparison.OrdinalIgnoreCase);

    private IEnumerable<T> CurrentTenantItems<T>(
        ClaimsPrincipal principal,
        IEnumerable<T> items,
        Func<T, string> tenantSelector) =>
        items.Where(item => IsCurrentTenant(principal, tenantSelector(item)));

    private void EnsureSnapshotLoaded()
    {
        if (_snapshotLoaded)
        {
            return;
        }

        _snapshotLoaded = true;
        if (_serviceScopeFactory is null)
        {
            return;
        }

        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetService<ReportArrDbContext>();
            if (db is null || !db.Database.IsRelational())
            {
                return;
            }

            var payload = ReadSnapshotPayload(db);
            if (!string.IsNullOrWhiteSpace(payload))
            {
                ApplySnapshot(payload);
            }
        }
        catch
        {
            // The store remains usable even when persistence is unavailable.
        }
    }

    private string? ReadSnapshotPayload(ReportArrDbContext db)
    {
        EnsureSnapshotTableExists(db);

        var connection = db.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;
        if (shouldClose)
        {
            db.Database.OpenConnection();
        }

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"""
                SELECT "PayloadJson"::text
                FROM {SnapshotTableName}
                WHERE "SnapshotKey" = @snapshotKey
                LIMIT 1
                """;
            AddParameter(command, "@snapshotKey", SnapshotRowKey);
            return command.ExecuteScalar() as string;
        }
        finally
        {
            if (shouldClose)
            {
                db.Database.CloseConnection();
            }
        }
    }

    private void PersistSnapshot()
    {
        if (_serviceScopeFactory is null)
        {
            return;
        }

        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetService<ReportArrDbContext>();
            if (db is null || !db.Database.IsRelational())
            {
                return;
            }

            EnsureSnapshotTableExists(db);
            var payload = SerializeSnapshot();
            var connection = db.Database.GetDbConnection();
            var shouldClose = connection.State != ConnectionState.Open;
            if (shouldClose)
            {
                db.Database.OpenConnection();
            }

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = $"""
                    INSERT INTO {SnapshotTableName} ("SnapshotKey", "PayloadJson", "UpdatedAtUtc")
                    VALUES (@snapshotKey, (@payload)::jsonb, @updatedAtUtc)
                    ON CONFLICT ("SnapshotKey")
                    DO UPDATE SET
                        "PayloadJson" = EXCLUDED."PayloadJson",
                        "UpdatedAtUtc" = EXCLUDED."UpdatedAtUtc"
                    """;
                AddParameter(command, "@snapshotKey", SnapshotRowKey);
                AddParameter(command, "@payload", payload);
                AddParameter(command, "@updatedAtUtc", DateTimeOffset.UtcNow);
                command.ExecuteNonQuery();
            }
            finally
            {
                if (shouldClose)
                {
                    db.Database.CloseConnection();
                }
            }
        }
        catch
        {
            // Persistence is best-effort so the API can continue serving requests.
        }
    }

    private static void EnsureSnapshotTableExists(ReportArrDbContext db)
    {
        var connection = db.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;
        if (shouldClose)
        {
            db.Database.OpenConnection();
        }

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"""
                CREATE TABLE IF NOT EXISTS {SnapshotTableName} (
                    "SnapshotKey" text NOT NULL PRIMARY KEY,
                    "PayloadJson" jsonb NOT NULL,
                    "UpdatedAtUtc" timestamptz NOT NULL
                )
                """;
            command.ExecuteNonQuery();
        }
        finally
        {
            if (shouldClose)
            {
                db.Database.CloseConnection();
            }
        }
    }

    private string SerializeSnapshot()
    {
        var snapshot = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var field in SnapshotListFields)
        {
            snapshot[field.Name] = field.GetValue(this);
        }

        return JsonSerializer.Serialize(snapshot, JsonOptions);
    }

    private void ApplySnapshot(string json)
    {
        var payload = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json, JsonOptions);
        if (payload is null)
        {
            return;
        }

        foreach (var field in SnapshotListFields)
        {
            if (!payload.TryGetValue(field.Name, out var element) ||
                element.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            {
                continue;
            }

            var value = JsonSerializer.Deserialize(element.GetRawText(), field.FieldType, JsonOptions);
            if (value is not null)
            {
                field.SetValue(this, value);
            }
        }
    }

    private static void AddParameter(DbCommand command, string name, object? value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value ?? DBNull.Value;
        command.Parameters.Add(parameter);
    }

    public ReportArrSessionBootstrapResponse BuildSession(
        string userId,
        string personId,
        string tenantId,
        string tenantRoleKey,
        bool isPlatformAdmin)
    {
        return new ReportArrSessionBootstrapResponse(
            userId,
            personId,
            tenantId,
            $"session-{tenantId[..8]}",
            tenantRoleKey,
            isPlatformAdmin,
            "reportarr",
            ReportArrSuiteLaunchCatalog.OrdinaryProductKeys);
    }

    public ReportArrMeResponse GetMe(ClaimsPrincipal principal) => new(
        principal.GetUserId().ToString(),
        principal.GetPersonId().ToString(),
        principal.GetTenantId().ToString(),
        principal.GetTenantRoleKey(),
        principal.IsPlatformAdmin(),
        "reportarr",
        ReportArrSuiteLaunchCatalog.OrdinaryProductKeys);

    public ReportArrSummaryResponse GetSummary(ClaimsPrincipal principal)
    {
        lock (Gate)
        {
            var datasets = GetDatasets(principal);
            var dashboards = GetDashboards(principal);
            var reports = GetReportDefinitions(principal);
            var runs = GetReportRuns(principal);
            var kpis = GetKpiDefinitions(principal);
            var alerts = GetAlerts(principal);
            var auditPackages = GetAuditPackages(principal);
            return new ReportArrSummaryResponse(
                DateTimeOffset.UtcNow,
                ResolveFreshnessStatus(principal),
                datasets.Count,
                dashboards.Count,
                reports.Count,
                runs.Count,
                kpis.Count,
                alerts.Count,
                auditPackages.Count,
                datasets.Take(3).ToList(),
                dashboards.Take(3).ToList(),
                reports.Take(3).ToList(),
                alerts.Take(3).ToList(),
                auditPackages.Take(3).ToList());
        }
    }

    public IReadOnlyList<ReportArrDatasetResponse> GetDatasets(ClaimsPrincipal principal)
    {
        lock (Gate)
        {
            return CurrentTenantItems(principal, _datasets, dataset => dataset.TenantId)
                .Where(dataset => CanAccessSourceProducts(principal, dataset.SourceProducts))
                .OrderByDescending(dataset => dataset.UpdatedAt)
                .ToList();
        }
    }

    public IReadOnlyList<ReportArrDatasetFieldResponse> GetDatasetFields(ClaimsPrincipal principal)
    {
        lock (Gate)
        {
            return CurrentTenantItems(principal, _datasetFields, field => field.TenantId)
                .Where(field =>
                {
                    var dataset = _datasets.FirstOrDefault(item => item.DatasetId == field.DatasetId && IsCurrentTenant(principal, item.TenantId));
                    return dataset is not null && CanAccessSourceProducts(principal, dataset.SourceProducts);
                })
                .ToList();
        }
    }

    public ReportArrDatasetResponse? GetDataset(ClaimsPrincipal principal, string datasetId)
    {
        lock (Gate)
        {
            var dataset = _datasets.FirstOrDefault(item => item.DatasetId == datasetId && IsCurrentTenant(principal, item.TenantId));
            return dataset is not null && CanAccessSourceProducts(principal, dataset.SourceProducts) ? dataset : null;
        }
    }

    public ReportArrDatasetResponse CreateDataset(ClaimsPrincipal principal, IntegrationEndpoints.CreateDatasetRequest request)
    {
        lock (Gate)
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
                now,
                CurrentTenantId(principal));
            _datasets.Add(dataset);
            PersistSnapshot();
            return dataset;
        }
    }

    public ReportArrRefreshJobResponse RefreshDataset(ClaimsPrincipal principal, string datasetId, IntegrationEndpoints.RefreshDatasetRequest request)
    {
        lock (Gate)
        {
            RequireCanRefreshDatasets(principal);
            var now = DateTimeOffset.UtcNow;
            var dataset = _datasets.FirstOrDefault(item => item.DatasetId == datasetId && IsCurrentTenant(principal, item.TenantId))
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
            PersistSnapshot();
            return job;
        }
    }

    public IReadOnlyList<ReportArrReadModelResponse> GetReadModels(ClaimsPrincipal principal)
    {
        lock (Gate)
        {
            return CurrentTenantItems(principal, _readModels, model => model.TenantId)
                .Where(model => CanAccessReadModel(principal, model))
                .OrderByDescending(model => model.UpdatedAt)
                .ToList();
        }
    }

    public ReportArrReadModelResponse? GetReadModel(ClaimsPrincipal principal, string readModelId)
    {
        lock (Gate)
        {
            var model = _readModels.FirstOrDefault(item => item.ReadModelId == readModelId && IsCurrentTenant(principal, item.TenantId));
            return model is not null && CanAccessReadModel(principal, model) ? model : null;
        }
    }

    public IReadOnlyList<ReportArrReadModelRecordResponse> GetReadModelRecords(ClaimsPrincipal principal)
    {
        lock (Gate)
        {
            return CurrentTenantItems(principal, _readModelRecords, record => record.TenantId)
                .Where(record =>
                {
                    var model = _readModels.FirstOrDefault(item => item.ReadModelId == record.ReadModelId && IsCurrentTenant(principal, item.TenantId));
                    return model is not null && CanAccessReadModel(principal, model);
                })
                .OrderByDescending(item => item.UpdatedAt)
                .ToList();
        }
    }

    public IReadOnlyList<ReportArrDatasetLineageResponse> GetDatasetLineage(ClaimsPrincipal principal)
    {
        lock (Gate)
        {
            return CurrentTenantItems(principal, _datasetLineage, lineage => lineage.TenantId)
                .Where(item =>
                {
                    var dataset = _datasets.FirstOrDefault(data => data.DatasetId == item.DatasetId && IsCurrentTenant(principal, data.TenantId));
                    return dataset is not null && CanAccessSourceProducts(principal, dataset.SourceProducts);
                })
                .ToList();
        }
    }

    public ReportArrRefreshJobResponse RebuildReadModel(ClaimsPrincipal principal, string readModelId, IntegrationEndpoints.RebuildReadModelRequest request)
    {
        lock (Gate)
        {
            RequireCanRebuildReadModels(principal);
            var now = DateTimeOffset.UtcNow;
            var model = _readModels.FirstOrDefault(item => item.ReadModelId == readModelId && IsCurrentTenant(principal, item.TenantId))
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
            PersistSnapshot();
            return job;
        }
    }

    public IReadOnlyList<ReportArrDashboardResponse> GetDashboards(ClaimsPrincipal principal)
    {
        lock (Gate)
        {
            return GetAccessibleDashboards(principal)
                .OrderByDescending(item => item.UpdatedAt)
                .ToList();
        }
    }

    public ReportArrDashboardResponse? GetDashboard(ClaimsPrincipal principal, string dashboardId)
    {
        lock (Gate)
        {
            var dashboard = _dashboards.FirstOrDefault(item => item.DashboardId == dashboardId && IsCurrentTenant(principal, item.TenantId));
            if (dashboard is null || !CanAccessDashboard(principal, dashboard))
            {
                return null;
            }

            var viewed = dashboard with
            {
                LastViewedAt = DateTimeOffset.UtcNow
            };
            ReplaceDashboard(viewed);
            PersistSnapshot();
            return viewed;
        }
    }

    public IReadOnlyList<ReportArrDashboardAccessPolicyResponse> GetDashboardAccessPolicies(ClaimsPrincipal principal)
    {
        lock (Gate)
        {
            RequireCanBuildReports(principal);
            return CurrentTenantItems(principal, _dashboardAccessPolicies, item => item.TenantId)
                .OrderByDescending(item => item.UpdatedAt)
                .ToList();
        }
    }

    public IReadOnlyList<ReportArrDashboardFilterResponse> GetDashboardFilters(ClaimsPrincipal principal)
    {
        lock (Gate)
        {
            return CurrentTenantItems(principal, _dashboardFilters, item => item.TenantId)
                .Where(filter => CanAccessDashboardById(principal, filter.DashboardId))
                .ToList();
        }
    }

    public IReadOnlyList<ReportArrDrilldownDefinitionResponse> GetDrilldowns(ClaimsPrincipal principal)
    {
        lock (Gate)
        {
            return CurrentTenantItems(principal, _drilldowns, item => item.TenantId)
                .Where(drilldown => CanAccessDashboardById(principal, drilldown.DashboardId))
                .ToList();
        }
    }

    public ReportArrDashboardResponse CreateDashboard(ClaimsPrincipal principal, IntegrationEndpoints.CreateDashboardRequest request)
    {
        lock (Gate)
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
                ownerPersonId,
                CurrentTenantId(principal));
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
                now,
                CurrentTenantId(principal)));
            dashboard = dashboard with { AccessPolicyRef = policyId };
            _dashboards.Add(dashboard);
            PersistSnapshot();
            return dashboard;
        }
    }

    public ReportArrDashboardResponse UpdateDashboard(ClaimsPrincipal principal, string dashboardId, IntegrationEndpoints.UpdateDashboardRequest request)
    {
        lock (Gate)
        {
            var existing = _dashboards.First(item => item.DashboardId == dashboardId && IsCurrentTenant(principal, item.TenantId));
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
            PersistSnapshot();
            return updated;
        }
    }

    public ReportArrDashboardWidgetResponse RenderWidget(ClaimsPrincipal principal, string widgetId)
    {
        lock (Gate)
        {
            var widget = _widgets.First(item => item.WidgetId == widgetId && IsCurrentTenant(principal, item.TenantId));
            var dashboard = _dashboards.FirstOrDefault(item => item.DashboardId == widget.DashboardId && IsCurrentTenant(principal, item.TenantId))
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
            PersistSnapshot();
            return updated;
        }
    }

    public IReadOnlyList<ReportArrReportDefinitionResponse> GetReportDefinitions(ClaimsPrincipal principal)
    {
        lock (Gate)
        {
            return GetAccessibleReports(principal)
                .OrderByDescending(item => item.UpdatedAt)
                .ToList();
        }
    }

    public ReportArrReportDefinitionResponse? GetReportDefinition(ClaimsPrincipal principal, string reportDefinitionId)
    {
        lock (Gate)
        {
            var report = _reportDefinitions.FirstOrDefault(item => item.ReportDefinitionId == reportDefinitionId && IsCurrentTenant(principal, item.TenantId));
            return report is not null && CanAccessReport(principal, report) ? report : null;
        }
    }

    public IReadOnlyList<ReportArrReportAccessPolicyResponse> GetReportAccessPolicies(ClaimsPrincipal principal)
    {
        lock (Gate)
        {
            RequireCanBuildReports(principal);
            return CurrentTenantItems(principal, _reportAccessPolicies, item => item.TenantId)
                .OrderByDescending(item => item.UpdatedAt)
                .ToList();
        }
    }

    public ReportArrReportDefinitionResponse CreateReportDefinition(ClaimsPrincipal principal, IntegrationEndpoints.CreateReportDefinitionRequest request)
    {
        lock (Gate)
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
                ownerPersonId,
                CurrentTenantId(principal));
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
                now,
                CurrentTenantId(principal)));
            definition = definition with { AccessPolicyRef = policyId };
            _reportDefinitions.Add(definition);
            PersistSnapshot();
            return definition;
        }
    }

    public ReportArrReportDefinitionResponse UpdateReportDefinition(ClaimsPrincipal principal, string reportDefinitionId, IntegrationEndpoints.UpdateReportDefinitionRequest request)
    {
        lock (Gate)
        {
            var existing = _reportDefinitions.First(item => item.ReportDefinitionId == reportDefinitionId && IsCurrentTenant(principal, item.TenantId));
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
            PersistSnapshot();
            return updated;
        }
    }

    public ReportArrReportRunResponse CreateReportRun(ClaimsPrincipal principal, IntegrationEndpoints.CreateReportRunRequest request)
    {
        lock (Gate)
        {
            RequireCanRunReports(principal);
            var now = DateTimeOffset.UtcNow;
            var reportDefinitionId = RequireTrimmed(request.ReportDefinitionId, nameof(request.ReportDefinitionId));
            var definition = _reportDefinitions.First(item => item.ReportDefinitionId == reportDefinitionId && IsCurrentTenant(principal, item.TenantId));
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
            var warnings = GetFreshnessWarnings(principal);
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
                ResolveFreshnessStatus(principal),
                $"datasets={string.Join(',', definition.DatasetRefs)}",
                warnings > 0 ? "freshness warnings were observed." : "all source traces were current.",
                CurrentTenantId(principal));
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
                    run.OutputPackageRef,
                    CurrentTenantId(principal)));
            }

            PersistSnapshot();
            return run;
        }
    }

    public ReportArrReportRunResponse? GetReportRun(ClaimsPrincipal principal, string reportRunId)
    {
        lock (Gate)
        {
            var run = _reportRuns.FirstOrDefault(item => item.ReportRunId == reportRunId && IsCurrentTenant(principal, item.TenantId));
            return run is not null && CanAccessReport(principal, run.ReportDefinitionId) ? run : null;
        }
    }

    public IReadOnlyList<ReportArrReportRunResponse> GetReportRuns(ClaimsPrincipal principal)
    {
        lock (Gate)
        {
            return CurrentTenantItems(principal, _reportRuns, run => run.TenantId)
                .Where(item => CanAccessReport(principal, item.ReportDefinitionId))
                .OrderByDescending(item => item.RequestedAt)
                .ToList();
        }
    }

    public ReportArrReportRunResponse CancelReportRun(ClaimsPrincipal principal, string reportRunId, IntegrationEndpoints.CancelReportRunRequest request)
    {
        lock (Gate)
        {
            var existing = _reportRuns.First(item => item.ReportRunId == reportRunId && IsCurrentTenant(principal, item.TenantId));
            EnsureCanViewReport(principal, RequireAccessibleReport(principal, existing.ReportDefinitionId));
            var updated = existing with { Status = "canceled" };
            ReplaceReportRun(updated);
            PersistSnapshot();
            return updated;
        }
    }

    public IReadOnlyList<ReportArrReportScheduleResponse> GetReportSchedules(ClaimsPrincipal principal)
    {
        lock (Gate)
        {
            return CurrentTenantItems(principal, _reportSchedules, schedule => schedule.TenantId)
                .Where(item => CanAccessReport(principal, item.ReportDefinitionId))
                .OrderByDescending(item => item.UpdatedAt)
                .ToList();
        }
    }

    public IReadOnlyList<ReportArrReportRecipientResponse> GetReportRecipients(ClaimsPrincipal principal)
    {
        lock (Gate)
        {
            var accessibleScheduleIds = _reportSchedules
                .Where(schedule => IsCurrentTenant(principal, schedule.TenantId) && CanAccessReport(principal, schedule.ReportDefinitionId))
                .Select(schedule => schedule.ScheduleId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return CurrentTenantItems(principal, _reportRecipients, recipient => recipient.TenantId)
                .Where(item => accessibleScheduleIds.Contains(item.ScheduleId))
                .ToList();
        }
    }

    public ReportArrReportScheduleResponse CreateReportSchedule(ClaimsPrincipal principal, IntegrationEndpoints.CreateReportScheduleRequest request)
    {
        lock (Gate)
        {
            RequireCanScheduleReports(principal);
            var now = DateTimeOffset.UtcNow;
            var reportDefinitionId = RequireTrimmed(request.ReportDefinitionId, nameof(request.ReportDefinitionId));
            var title = RequireTrimmed(request.Title, nameof(request.Title));
            var cadence = NormalizeEnumValue(request.Cadence, nameof(request.Cadence), ScheduleCadences);
            var timezone = RequireTrimmed(request.Timezone, nameof(request.Timezone));
            var deliveryMethod = NormalizeEnumValue(request.DeliveryMethod, nameof(request.DeliveryMethod), DeliveryMethods);
            var definition = _reportDefinitions.First(item => item.ReportDefinitionId == reportDefinitionId && IsCurrentTenant(principal, item.TenantId));
            EnsureCanViewReport(principal, definition);
            EnsureCanScheduleReport(principal, definition);
            var policy = _reportAccessPolicies.FirstOrDefault(item => item.AccessPolicyId == definition.AccessPolicyRef && IsCurrentTenant(principal, item.TenantId));
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
                now,
                CurrentTenantId(principal));
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
                    "active",
                    CurrentTenantId(principal)));
            }
            PersistSnapshot();
            return schedule;
        }
    }

    public ReportArrReportScheduleResponse UpdateReportSchedule(ClaimsPrincipal principal, string scheduleId, IntegrationEndpoints.UpdateReportScheduleRequest request)
    {
        lock (Gate)
        {
            var existing = _reportSchedules.First(item => item.ScheduleId == scheduleId && IsCurrentTenant(principal, item.TenantId));
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
            PersistSnapshot();
            return updated;
        }
    }

    public ReportArrExportJobResponse CreateExport(ClaimsPrincipal principal, IntegrationEndpoints.CreateExportRequest request)
    {
        lock (Gate)
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
                run = _reportRuns.First(item => item.ReportRunId == reportRunId && IsCurrentTenant(principal, item.TenantId));
                var definition = _reportDefinitions.First(item => item.ReportDefinitionId == run.ReportDefinitionId && IsCurrentTenant(principal, item.TenantId));
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

                dashboard = _dashboards.FirstOrDefault(item => item.DashboardId == sourceRef && IsCurrentTenant(principal, item.TenantId));
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

                dataset = _datasets.FirstOrDefault(item => item.DatasetId == sourceRef && IsCurrentTenant(principal, item.TenantId));
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

                auditPackage = _auditPackages.FirstOrDefault(item => item.AuditReportPackageId == sourceRef && IsCurrentTenant(principal, item.TenantId));
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

                report = _reportDefinitions.FirstOrDefault(item => item.ReportDefinitionId == sourceRef && IsCurrentTenant(principal, item.TenantId));
                dashboard = _dashboards.FirstOrDefault(item => item.DashboardId == sourceRef && IsCurrentTenant(principal, item.TenantId));
                dataset = _datasets.FirstOrDefault(item => item.DatasetId == sourceRef && IsCurrentTenant(principal, item.TenantId));
                auditPackage = _auditPackages.FirstOrDefault(item => item.AuditReportPackageId == sourceRef && IsCurrentTenant(principal, item.TenantId));

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
                run?.OutputPackageRef,
                CurrentTenantId(principal));
            _exportJobs.Add(export);
            PersistSnapshot();
            return export;
        }
    }

    public ReportArrExportJobResponse? GetExportJob(ClaimsPrincipal principal, string exportJobId)
    {
        lock (Gate)
        {
            var export = _exportJobs.FirstOrDefault(item => item.ExportJobId == exportJobId && IsCurrentTenant(principal, item.TenantId));
            return export is not null && CanAccessExportJob(principal, export) ? export : null;
        }
    }

    public IReadOnlyList<ReportArrExportJobResponse> GetExportJobs(ClaimsPrincipal principal)
    {
        lock (Gate)
        {
            return CurrentTenantItems(principal, _exportJobs, item => item.TenantId)
                .Where(job => CanAccessExportJob(principal, job))
                .OrderByDescending(item => item.GeneratedAt)
                .ToList();
        }
    }

    public IReadOnlyList<ReportArrMetricDefinitionResponse> GetMetricDefinitions(ClaimsPrincipal principal)
    {
        lock (Gate)
        {
            return CurrentTenantItems(principal, _metrics, item => item.TenantId)
                .Where(item =>
                {
                    var dataset = _datasets.FirstOrDefault(data => data.DatasetId == item.SourceDatasetRef && IsCurrentTenant(principal, data.TenantId));
                    return dataset is not null && CanAccessSourceProducts(principal, dataset.SourceProducts);
                })
                .ToList();
        }
    }

    public IReadOnlyList<ReportArrKpiValueResponse> GetKpiValues(ClaimsPrincipal principal)
    {
        lock (Gate)
        {
            return CurrentTenantItems(principal, _kpiValues, item => item.TenantId)
                .Where(item => CanAccessKpiValue(principal, item))
                .OrderByDescending(item => item.CalculatedAt)
                .ToList();
        }
    }

    public IReadOnlyList<ReportArrMetricValueResponse> GetMetricValues(ClaimsPrincipal principal)
    {
        lock (Gate)
        {
            return CurrentTenantItems(principal, _metricValues, item => item.TenantId)
                .Where(item => CanAccessMetricValue(principal, item))
                .OrderByDescending(item => item.CalculatedAt)
                .ToList();
        }
    }

    public IReadOnlyList<ReportArrAnalyticsSnapshotResponse> GetAnalyticsSnapshots(ClaimsPrincipal principal)
    {
        lock (Gate)
        {
            return CurrentTenantItems(principal, _analyticsSnapshots, item => item.TenantId)
                .Where(item => CanAccessAnalyticsSnapshot(principal, item))
                .OrderByDescending(item => item.GeneratedAt)
                .ToList();
        }
    }

    public IReadOnlyList<ReportArrTrendAnalysisResponse> GetTrendAnalyses(ClaimsPrincipal principal)
    {
        lock (Gate)
        {
            return CurrentTenantItems(principal, _trendAnalyses, item => item.TenantId)
                .Where(item => CanAccessTrendAnalysis(principal, item))
                .OrderByDescending(item => item.GeneratedAt)
                .ToList();
        }
    }

    public IReadOnlyList<ReportArrExceptionQueryResponse> GetExceptionQueries(ClaimsPrincipal principal)
    {
        lock (Gate)
        {
            return CurrentTenantItems(principal, _exceptionQueries, item => item.TenantId)
                .Where(item => CanAccessExceptionQuery(principal, item))
                .ToList();
        }
    }

    public IReadOnlyList<ReportArrExceptionResultResponse> GetExceptionResults(ClaimsPrincipal principal)
    {
        lock (Gate)
        {
            return CurrentTenantItems(principal, _exceptionResults, item => item.TenantId)
                .Where(item => CanAccessExceptionResult(principal, item))
                .OrderByDescending(item => item.DetectedAt)
                .ToList();
        }
    }

    public IReadOnlyList<ReportArrReportParameterResponse> GetReportParameters(ClaimsPrincipal principal)
    {
        lock (Gate)
        {
            return CurrentTenantItems(principal, _reportParameters, item => item.TenantId)
                .Where(item => CanAccessReport(principal, item.ReportDefinitionId))
                .ToList();
        }
    }

    public IReadOnlyList<ReportArrReportSectionResponse> GetReportSections(ClaimsPrincipal principal)
    {
        lock (Gate)
        {
            return CurrentTenantItems(principal, _reportSections, item => item.TenantId)
                .Where(item => CanAccessReport(principal, item.ReportDefinitionId))
                .OrderBy(item => item.Sequence)
                .ToList();
        }
    }

    public ReportArrKpiValueResponse CalculateKpi(ClaimsPrincipal principal, string kpiId, IntegrationEndpoints.CalculateKpiRequest request)
    {
        lock (Gate)
        {
            var kpi = _kpis.First(item => item.KpiId == kpiId && IsCurrentTenant(principal, item.TenantId));
            EnsureCanViewKpi(principal, kpi);
            RequireCondition(
                request.PeriodStart <= request.PeriodEnd,
                "reportarr.kpi_period_invalid",
                "Period start must be before or equal to period end.");
            RequireTrimmed(request.RequestedByPersonId, nameof(request.RequestedByPersonId));
            var freshnessStatus = ResolveFreshnessStatus(principal);
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
                DateTimeOffset.UtcNow,
                CurrentTenantId(principal));
            _kpiValues.Add(value);
            PersistSnapshot();
            return value;
        }
    }

    public IReadOnlyList<ReportArrKpiDefinitionResponse> GetKpiDefinitions(ClaimsPrincipal principal)
    {
        lock (Gate)
        {
            return CurrentTenantItems(principal, _kpis, item => item.TenantId)
                .Where(item => CanAccessKpi(principal, item))
                .OrderByDescending(item => item.UpdatedAt)
                .ToList();
        }
    }

    public ReportArrKpiDefinitionResponse? GetKpiDefinition(ClaimsPrincipal principal, string kpiId)
    {
        lock (Gate)
        {
            var kpi = _kpis.FirstOrDefault(item => item.KpiId == kpiId && IsCurrentTenant(principal, item.TenantId));
            return kpi is not null && CanAccessKpi(principal, kpi) ? kpi : null;
        }
    }

    public IReadOnlyList<ReportArrAlertResponse> GetAlerts(ClaimsPrincipal principal)
    {
        lock (Gate)
        {
            return CurrentTenantItems(principal, _alerts, item => item.TenantId)
                .Where(item => CanAccessAlert(principal, item))
                .OrderByDescending(item => item.TriggeredAt)
                .ToList();
        }
    }

    public ReportArrAlertResponse AcknowledgeAlert(ClaimsPrincipal principal, string alertId, IntegrationEndpoints.AcknowledgeAlertRequest request)
    {
        lock (Gate)
        {
            var existing = _alerts.First(item => item.AlertId == alertId && IsCurrentTenant(principal, item.TenantId));
            EnsureCanViewAlert(principal, existing);
            var requestedByPersonId = RequireTrimmed(request.RequestedByPersonId, nameof(request.RequestedByPersonId));
            var updated = existing with
            {
                Status = "acknowledged",
                AcknowledgedByPersonId = requestedByPersonId,
                AcknowledgedAt = DateTimeOffset.UtcNow
            };
            ReplaceAlert(updated);
            PersistSnapshot();
            return updated;
        }
    }

    public ReportArrAlertResponse ResolveAlert(ClaimsPrincipal principal, string alertId, IntegrationEndpoints.ResolveAlertRequest request)
    {
        lock (Gate)
        {
            var existing = _alerts.First(item => item.AlertId == alertId && IsCurrentTenant(principal, item.TenantId));
            EnsureCanViewAlert(principal, existing);
            RequireTrimmed(request.RequestedByPersonId, nameof(request.RequestedByPersonId));
            var updated = existing with
            {
                Status = "resolved",
                ResolvedAt = DateTimeOffset.UtcNow
            };
            ReplaceAlert(updated);
            PersistSnapshot();
            return updated;
        }
    }

    public ReportArrAuditPackageResponse CreateAuditPackage(ClaimsPrincipal principal, IntegrationEndpoints.CreateAuditPackageRequest request)
    {
        lock (Gate)
        {
            RequireCanCreateAuditPackages(principal);
            var now = DateTimeOffset.UtcNow;
            var title = RequireTrimmed(request.Title, nameof(request.Title));
            var description = RequireTrimmed(request.Description, nameof(request.Description));
            var requestedByPersonId = RequireTrimmed(request.RequestedByPersonId, nameof(request.RequestedByPersonId));
            var auditScopeId = RequireTrimmed(request.AuditScopeId, nameof(request.AuditScopeId));
            var auditScope = _auditScopes.FirstOrDefault(scope => scope.AuditScopeId == auditScopeId && IsCurrentTenant(principal, scope.TenantId))
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
                null,
                CurrentTenantId(principal));
            _auditPackages.Add(package);
            PersistSnapshot();
            return package;
        }
    }

    public ReportArrAuditPackageResponse? GetAuditPackage(ClaimsPrincipal principal, string auditReportPackageId)
    {
        lock (Gate)
        {
            var auditPackage = _auditPackages.FirstOrDefault(item => item.AuditReportPackageId == auditReportPackageId && IsCurrentTenant(principal, item.TenantId));
            return auditPackage is not null && CanAccessSourceProducts(principal, auditPackage.SourceProductRefs) ? auditPackage : null;
        }
    }

    public ReportArrAuditPackageResponse LockAuditPackage(ClaimsPrincipal principal, string auditReportPackageId, IntegrationEndpoints.LockAuditPackageRequest request)
    {
        lock (Gate)
        {
            var existing = _auditPackages.First(item => item.AuditReportPackageId == auditReportPackageId && IsCurrentTenant(principal, item.TenantId));
            EnsureCanViewAuditPackage(principal, existing);
            RequireTrimmed(request.RequestedByPersonId, nameof(request.RequestedByPersonId));
            var updated = existing with
            {
                Status = "locked",
                LockedAt = DateTimeOffset.UtcNow
            };
            ReplaceAuditPackage(updated);
            PersistSnapshot();
            return updated;
        }
    }

    public IReadOnlyList<ReportArrAuditPackageResponse> GetAuditPackages(ClaimsPrincipal principal)
    {
        lock (Gate)
        {
            return CurrentTenantItems(principal, _auditPackages, item => item.TenantId)
                .Where(item => CanAccessSourceProducts(principal, item.SourceProductRefs))
                .OrderByDescending(item => item.GeneratedAt)
                .ToList();
        }
    }

    public IReadOnlyList<ReportArrAuditScopeResponse> GetAuditScopes(ClaimsPrincipal principal)
    {
        lock (Gate)
        {
            RequireCanCreateAuditPackages(principal);
            return CurrentTenantItems(principal, _auditScopes, item => item.TenantId)
                .Where(item => CanAccessSourceProducts(principal, item.ProductFilters))
                .ToList();
        }
    }

    public IReadOnlyList<ReportArrDashboardWidgetResponse> GetWidgets(ClaimsPrincipal principal)
    {
        lock (Gate)
        {
            return CurrentTenantItems(principal, _widgets, item => item.TenantId)
                .Where(widget =>
                {
                    var dashboard = _dashboards.FirstOrDefault(item => item.DashboardId == widget.DashboardId && IsCurrentTenant(principal, item.TenantId));
                    return dashboard is not null && CanAccessDashboard(principal, dashboard);
                })
                .ToList();
        }
    }

    public IReadOnlyList<ReportArrWidgetVisualizationSettingsResponse> GetWidgetVisualizations(ClaimsPrincipal principal)
    {
        lock (Gate)
        {
            return CurrentTenantItems(principal, _widgetVisualizations, item => item.TenantId)
                .Where(item =>
                {
                    var widget = _widgets.FirstOrDefault(candidate => candidate.WidgetId == item.WidgetId && IsCurrentTenant(principal, candidate.TenantId));
                    if (widget is null)
                    {
                        return false;
                    }

                    var dashboard = _dashboards.FirstOrDefault(candidate => candidate.DashboardId == widget.DashboardId && IsCurrentTenant(principal, candidate.TenantId));
                    return dashboard is not null && CanAccessDashboard(principal, dashboard);
                })
                .ToList();
        }
    }

    public IReadOnlyList<ReportArrSourceConnectorResponse> GetSourceConnectors(ClaimsPrincipal principal)
    {
        lock (Gate)
        {
            return CurrentTenantItems(principal, _sourceConnectors, item => item.TenantId)
                .Where(item => CanAccessSourceProducts(principal, [item.SourceProduct]))
                .ToList();
        }
    }

    public IReadOnlyList<ReportArrIngestionCursorResponse> GetIngestionCursors(ClaimsPrincipal principal)
    {
        lock (Gate)
        {
            return CurrentTenantItems(principal, _ingestionCursors, item => item.TenantId)
                .Where(item => CanAccessSourceProducts(principal, [item.SourceProduct]))
                .ToList();
        }
    }

    public IReadOnlyList<ReportArrRefreshJobResponse> GetRefreshJobs(ClaimsPrincipal principal)
    {
        lock (Gate)
        {
            return CurrentTenantItems(principal, _refreshJobs, item => item.TenantId)
                .Where(item =>
                {
                    var dataset = _datasets.FirstOrDefault(dataset => dataset.DatasetId == item.DatasetId && IsCurrentTenant(principal, dataset.TenantId));
                    return dataset is null || CanAccessSourceProducts(principal, dataset.SourceProducts);
                })
                .ToList();
        }
    }

    private IReadOnlyList<ReportArrDashboardResponse> GetAccessibleDashboards(ClaimsPrincipal principal)
    {
        var dashboards = CurrentTenantItems(principal, _dashboards, item => item.TenantId);
        return dashboards
            .Where(dashboard =>
                CanAccessPolicy(
                    principal,
                    _dashboardAccessPolicies.FirstOrDefault(policy => policy.AccessPolicyId == dashboard.AccessPolicyRef && IsCurrentTenant(principal, policy.TenantId)),
                    dashboard.OwnerPersonId))
            .ToList();
    }

    private IReadOnlyList<ReportArrReportDefinitionResponse> GetAccessibleReports(ClaimsPrincipal principal)
    {
        var reports = CurrentTenantItems(principal, _reportDefinitions, item => item.TenantId);
        return reports
            .Where(report =>
                CanAccessPolicy(
                    principal,
                    _reportAccessPolicies.FirstOrDefault(policy => policy.AccessPolicyId == report.AccessPolicyRef && IsCurrentTenant(principal, policy.TenantId)),
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
        var personId = principal.GetPersonId().ToString();
        var roleKey = principal.GetTenantRoleKey();
        if (sourceProductRestrictions.Count > 0 &&
            !CanAccessSourceProducts(principal, sourceProductRestrictions))
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

        if (allowedPermissionRefs.Any(permission => MatchesReportArrPermission(principal, permission)))
        {
            return true;
        }

        return string.Equals(visibility, "tenant_wide", StringComparison.OrdinalIgnoreCase);
    }

    private bool CanAccessDashboard(ClaimsPrincipal principal, ReportArrDashboardResponse dashboard)
    {
        if (!IsCurrentTenant(principal, dashboard.TenantId))
        {
            return false;
        }

        var policy = _dashboardAccessPolicies.FirstOrDefault(item => item.AccessPolicyId == dashboard.AccessPolicyRef && IsCurrentTenant(principal, item.TenantId));
        return CanAccessPolicy(principal, policy, dashboard.OwnerPersonId);
    }

    private bool CanAccessDashboardById(ClaimsPrincipal principal, string dashboardId)
    {
        var dashboard = _dashboards.FirstOrDefault(item => item.DashboardId == dashboardId && IsCurrentTenant(principal, item.TenantId));
        return dashboard is not null && CanAccessDashboard(principal, dashboard);
    }

    private bool CanAccessReport(ClaimsPrincipal principal, ReportArrReportDefinitionResponse report)
    {
        if (!IsCurrentTenant(principal, report.TenantId))
        {
            return false;
        }

        var policy = _reportAccessPolicies.FirstOrDefault(item => item.AccessPolicyId == report.AccessPolicyRef && IsCurrentTenant(principal, item.TenantId));
        return CanAccessPolicy(principal, policy, report.OwnerPersonId);
    }

    private bool CanAccessReport(ClaimsPrincipal principal, string reportDefinitionId)
    {
        var report = _reportDefinitions.FirstOrDefault(item => item.ReportDefinitionId == reportDefinitionId && IsCurrentTenant(principal, item.TenantId));
        return report is not null && CanAccessReport(principal, report);
    }

    private bool CanAccessReadModel(ClaimsPrincipal principal, ReportArrReadModelResponse readModel)
    {
        if (!IsCurrentTenant(principal, readModel.TenantId))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(readModel.PrimarySourceProduct))
        {
            return CanAccessSourceProducts(principal, [readModel.PrimarySourceProduct]);
        }

        return readModel.DatasetRefs
            .Select(datasetId => _datasets.FirstOrDefault(dataset => dataset.DatasetId == datasetId && IsCurrentTenant(principal, dataset.TenantId)))
            .Where(dataset => dataset is not null)
            .All(dataset => CanAccessSourceProducts(principal, dataset!.SourceProducts));
    }

    private bool CanAccessKpi(ClaimsPrincipal principal, ReportArrKpiDefinitionResponse kpi)
    {
        if (!IsCurrentTenant(principal, kpi.TenantId))
        {
            return false;
        }

        var sourceProducts = kpi.SourceDatasetRefs
            .SelectMany(datasetId => _datasets.Where(dataset => dataset.DatasetId == datasetId && IsCurrentTenant(principal, dataset.TenantId)))
            .SelectMany(dataset => dataset.SourceProducts)
            .ToList();
        return CanAccessSourceProducts(principal, sourceProducts);
    }

    private bool CanAccessAlert(ClaimsPrincipal principal, ReportArrAlertResponse alert)
    {
        if (!IsCurrentTenant(principal, alert.TenantId))
        {
            return false;
        }

        var sourceProducts = _datasets
            .Where(dataset => string.Equals(dataset.DatasetId, alert.DatasetRef, StringComparison.OrdinalIgnoreCase) && IsCurrentTenant(principal, dataset.TenantId))
            .SelectMany(dataset => dataset.SourceProducts)
            .Concat(_metrics
                .Where(metric => string.Equals(metric.MetricId, alert.MetricRef, StringComparison.OrdinalIgnoreCase) && IsCurrentTenant(principal, metric.TenantId))
                .SelectMany(metric => _datasets.Where(dataset => dataset.DatasetId == metric.SourceDatasetRef && IsCurrentTenant(principal, dataset.TenantId)))
                .SelectMany(dataset => dataset.SourceProducts))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return CanAccessSourceProducts(principal, sourceProducts);
    }

    private bool CanAccessAuditPackage(ClaimsPrincipal principal, ReportArrAuditPackageResponse auditPackage)
    {
        if (!IsCurrentTenant(principal, auditPackage.TenantId))
        {
            return false;
        }

        return CanAccessSourceProducts(principal, auditPackage.SourceProductRefs);
    }

    private static bool CanAccessSourceProducts(ClaimsPrincipal principal, IReadOnlyList<string> sourceProducts)
    {
        if (sourceProducts.Count == 0)
        {
            return true;
        }

        return HasSourceReportingRole(principal);
    }

    private static bool MatchesReportArrPermission(ClaimsPrincipal principal, string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
        {
            return false;
        }

        var normalizedPermission = permission.Trim();
        var roleKey = principal.GetTenantRoleKey();
        return normalizedPermission switch
        {
            "reportarr.datasets.manage" => MatchesRole(roleKey, "analytics_admin", "reportarr_admin"),
            "reportarr.datasets.refresh" => MatchesRole(roleKey, "analytics_admin", "reportarr_admin"),
            "reportarr.read_models.rebuild" => MatchesRole(roleKey, "analytics_admin", "reportarr_admin"),
            "reportarr.reports.build" => MatchesRole(roleKey, "report_builder", "reportarr_builder", "tenant_admin", "reportarr_admin"),
            "reportarr.reports.run" => MatchesRole(roleKey, "report_runner", "reportarr_runner", "report_builder", "reportarr_admin", "tenant_admin"),
            "reportarr.reports.schedule" => MatchesRole(roleKey, "report_scheduler", "reportarr_scheduler", "report_builder", "reportarr_admin", "tenant_admin"),
            "reportarr.reports.update" => MatchesRole(roleKey, "report_builder", "reportarr_builder", "tenant_admin", "reportarr_admin"),
            "reportarr.dashboards.update" => MatchesRole(roleKey, "report_builder", "reportarr_builder", "tenant_admin", "reportarr_admin"),
            "reportarr.audit_packages.create" => MatchesRole(roleKey, "compliance_reporter", "reportarr_admin", "tenant_admin"),
            "reportarr.source_events.receive" => MatchesRole(roleKey, "analytics_admin", "reportarr_admin", "tenant_admin"),
            "reportarr.viewer" => HasSourceReportingRole(principal),
            "reportarr.view" => HasSourceReportingRole(principal),
            _ => string.Equals(normalizedPermission, roleKey, StringComparison.OrdinalIgnoreCase),
        };
    }

    private static bool HasSourceReportingRole(ClaimsPrincipal principal) =>
        MatchesRole(
            principal.GetTenantRoleKey(),
            "analytics_admin",
            "reportarr_admin",
            "reportarr_viewer",
            "report_viewer",
            "reportarr_runner",
            "report_runner",
            "reportarr_builder",
            "report_builder",
            "reportarr_scheduler",
            "report_scheduler",
            "compliance_reporter",
            "tenant_admin");

    private bool CanAccessKpiValue(ClaimsPrincipal principal, ReportArrKpiValueResponse kpiValue)
    {
        if (!IsCurrentTenant(principal, kpiValue.TenantId))
        {
            return false;
        }

        var kpi = _kpis.FirstOrDefault(item => item.KpiId == kpiValue.KpiId && IsCurrentTenant(principal, item.TenantId));
        return kpi is not null && CanAccessKpi(principal, kpi);
    }

    private bool CanAccessMetricValue(ClaimsPrincipal principal, ReportArrMetricValueResponse metricValue)
    {
        if (!IsCurrentTenant(principal, metricValue.TenantId))
        {
            return false;
        }

        var metric = _metrics.FirstOrDefault(item => item.MetricId == metricValue.MetricId && IsCurrentTenant(principal, item.TenantId));
        if (metric is null)
        {
            return false;
        }

        var dataset = _datasets.FirstOrDefault(item => item.DatasetId == metric.SourceDatasetRef && IsCurrentTenant(principal, item.TenantId));
        return dataset is null || CanAccessSourceProducts(principal, dataset.SourceProducts);
    }

    private bool CanAccessAnalyticsSnapshot(ClaimsPrincipal principal, ReportArrAnalyticsSnapshotResponse snapshot)
    {
        if (!IsCurrentTenant(principal, snapshot.TenantId))
        {
            return false;
        }

        return snapshot.DatasetRefs.All(datasetId =>
        {
            var dataset = _datasets.FirstOrDefault(item => item.DatasetId == datasetId && IsCurrentTenant(principal, item.TenantId));
            return dataset is null || CanAccessSourceProducts(principal, dataset.SourceProducts);
        });
    }

    private bool CanAccessTrendAnalysis(ClaimsPrincipal principal, ReportArrTrendAnalysisResponse trend)
    {
        if (!IsCurrentTenant(principal, trend.TenantId))
        {
            return false;
        }

        var metric = _metrics.FirstOrDefault(item => item.MetricId == trend.MetricRef && IsCurrentTenant(principal, item.TenantId));
        if (metric is not null)
        {
            var dataset = _datasets.FirstOrDefault(item => item.DatasetId == metric.SourceDatasetRef && IsCurrentTenant(principal, item.TenantId));
            return dataset is null || CanAccessSourceProducts(principal, dataset.SourceProducts);
        }

        var kpi = _kpis.FirstOrDefault(item => item.KpiId == trend.KpiRef && IsCurrentTenant(principal, item.TenantId));
        return kpi is not null && CanAccessKpi(principal, kpi);
    }

    private bool CanAccessExceptionQuery(ClaimsPrincipal principal, ReportArrExceptionQueryResponse query)
    {
        if (!IsCurrentTenant(principal, query.TenantId))
        {
            return false;
        }

        var dataset = _datasets.FirstOrDefault(item => item.DatasetId == query.SourceDatasetRef && IsCurrentTenant(principal, item.TenantId));
        return dataset is null || CanAccessSourceProducts(principal, dataset.SourceProducts);
    }

    private bool CanAccessExceptionResult(ClaimsPrincipal principal, ReportArrExceptionResultResponse result)
    {
        if (!IsCurrentTenant(principal, result.TenantId))
        {
            return false;
        }

        var query = _exceptionQueries.FirstOrDefault(item => item.ExceptionQueryId == result.ExceptionQueryId && IsCurrentTenant(principal, item.TenantId));
        return query is not null && CanAccessExceptionQuery(principal, query);
    }

    private void RequireCanManageDatasets(ClaimsPrincipal principal)
    {
        RequireCondition(
            MatchesRole(principal.GetTenantRoleKey(), "analytics_admin", "reportarr_admin"),
            "reportarr.dataset_manage_forbidden",
            "Managing datasets is not allowed.",
            403);
    }

    private void RequireCanRefreshDatasets(ClaimsPrincipal principal)
    {
        RequireCondition(
            MatchesRole(principal.GetTenantRoleKey(), "analytics_admin", "reportarr_admin"),
            "reportarr.dataset_refresh_forbidden",
            "Refreshing datasets is not allowed.",
            403);
    }

    private void RequireCanRebuildReadModels(ClaimsPrincipal principal)
    {
        RequireCondition(
            MatchesRole(principal.GetTenantRoleKey(), "analytics_admin", "reportarr_admin"),
            "reportarr.read_model_rebuild_forbidden",
            "Rebuilding read models is not allowed.",
            403);
    }

    private void RequireCanBuildReports(ClaimsPrincipal principal)
    {
        RequireCondition(
            MatchesRole(principal.GetTenantRoleKey(), "report_builder", "reportarr_builder", "tenant_admin", "reportarr_admin"),
            "reportarr.report_build_forbidden",
            "Building dashboards and reports is not allowed.",
            403);
    }

    private void RequireCanCreateAuditPackages(ClaimsPrincipal principal)
    {
        RequireCondition(
            MatchesRole(principal.GetTenantRoleKey(), "compliance_reporter", "reportarr_admin", "tenant_admin"),
            "reportarr.audit_package_forbidden",
            "Creating audit packages is not allowed.",
            403);
    }

    private void RequireCanRunReports(ClaimsPrincipal principal)
    {
        RequireCondition(
            MatchesRole(principal.GetTenantRoleKey(), "report_runner", "reportarr_runner", "report_builder", "reportarr_admin", "tenant_admin"),
            "reportarr.report_run_forbidden",
            "Running and exporting reports is not allowed.",
            403);
    }

    private void RequireCanScheduleReports(ClaimsPrincipal principal)
    {
        RequireCondition(
            MatchesRole(principal.GetTenantRoleKey(), "report_scheduler", "reportarr_scheduler", "report_builder", "reportarr_admin", "tenant_admin"),
            "reportarr.report_schedule_forbidden",
            "Scheduling reports is not allowed.",
            403);
    }

    private void RequireCanReceiveSourceEvents(ClaimsPrincipal principal)
    {
        RequireCondition(
            MatchesRole(principal.GetTenantRoleKey(), "analytics_admin", "reportarr_admin", "tenant_admin"),
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
        var definition = _reportDefinitions.First(item => item.ReportDefinitionId == reportDefinitionId && IsCurrentTenant(principal, item.TenantId));
        EnsureCanViewReport(principal, definition);
        return definition;
    }

    private void EnsureCanViewReport(ClaimsPrincipal principal, ReportArrReportDefinitionResponse definition)
    {
        var policy = _reportAccessPolicies.FirstOrDefault(item => item.AccessPolicyId == definition.AccessPolicyRef && IsCurrentTenant(principal, item.TenantId));
        RequireCondition(
            CanAccessPolicy(principal, policy, definition.OwnerPersonId),
            "reportarr.forbidden",
            "You do not have access to this report.",
            403);
    }

    private void EnsureCanViewDashboard(ClaimsPrincipal principal, ReportArrDashboardResponse dashboard)
    {
        var policy = _dashboardAccessPolicies.FirstOrDefault(item => item.AccessPolicyId == dashboard.AccessPolicyRef && IsCurrentTenant(principal, item.TenantId));
        RequireCondition(
            CanAccessPolicy(principal, policy, dashboard.OwnerPersonId),
            "reportarr.forbidden",
            "You do not have access to this dashboard.",
            403);
    }

    private void EnsureCanScheduleReport(ClaimsPrincipal principal, ReportArrReportDefinitionResponse definition)
    {
        var policy = _reportAccessPolicies.FirstOrDefault(item => item.AccessPolicyId == definition.AccessPolicyRef && IsCurrentTenant(principal, item.TenantId));
        RequireCondition(
            CanAccessPolicy(principal, policy, definition.OwnerPersonId) && policy is not null && policy.ScheduleAllowed,
            "reportarr.report_schedule_forbidden",
            "Scheduling is not allowed for this report.",
            403);
    }

    private void EnsureCanExportReport(ClaimsPrincipal principal, ReportArrReportDefinitionResponse definition)
    {
        var policy = _reportAccessPolicies.FirstOrDefault(item => item.AccessPolicyId == definition.AccessPolicyRef && IsCurrentTenant(principal, item.TenantId));
        RequireCondition(
            CanAccessPolicy(principal, policy, definition.OwnerPersonId) && policy is not null && policy.ExportAllowed,
            "reportarr.report_export_forbidden",
            "Exporting is not allowed for this report.",
            403);
    }

    private void EnsureCanExportDashboard(ClaimsPrincipal principal, ReportArrDashboardResponse dashboard)
    {
        var policy = _dashboardAccessPolicies.FirstOrDefault(item => item.AccessPolicyId == dashboard.AccessPolicyRef && IsCurrentTenant(principal, item.TenantId));
        RequireCondition(
            CanAccessPolicy(principal, policy, dashboard.OwnerPersonId) && policy is not null && policy.ExportAllowed,
            "reportarr.dashboard_export_forbidden",
            "Exporting is not allowed for this dashboard.",
            403);
    }

    private void EnsureCanManageDashboard(ClaimsPrincipal principal, ReportArrDashboardResponse dashboard)
    {
        var policy = _dashboardAccessPolicies.FirstOrDefault(item => item.AccessPolicyId == dashboard.AccessPolicyRef && IsCurrentTenant(principal, item.TenantId));
        RequireCondition(
            CanAccessPolicy(principal, policy, dashboard.OwnerPersonId) && policy is not null && policy.AllowedPermissionRefs.Any(permission => string.Equals(permission, "reportarr.dashboards.update", StringComparison.OrdinalIgnoreCase)),
            "reportarr.dashboard_update_forbidden",
            "Updating this dashboard is not allowed.",
            403);
    }

    private void EnsureCanManageReport(ClaimsPrincipal principal, ReportArrReportDefinitionResponse definition)
    {
        var policy = _reportAccessPolicies.FirstOrDefault(item => item.AccessPolicyId == definition.AccessPolicyRef && IsCurrentTenant(principal, item.TenantId));
        RequireCondition(
            CanAccessPolicy(principal, policy, definition.OwnerPersonId) && policy is not null && policy.AllowedPermissionRefs.Any(permission => string.Equals(permission, "reportarr.reports.update", StringComparison.OrdinalIgnoreCase)),
            "reportarr.report_update_forbidden",
            "Updating this report is not allowed.",
            403);
    }

    public IReadOnlyList<ReportArrSourceEventReceiptResponse> GetSourceEvents(ClaimsPrincipal principal)
    {
        lock (Gate)
        {
            return CurrentTenantItems(principal, _sourceEvents, item => item.TenantId).ToList();
        }
    }

    public ReportArrSourceEventReceiptResponse ReceiveEvent(ClaimsPrincipal principal, IntegrationEndpoints.SourceEventRequest request)
    {
        lock (Gate)
        {
            RequireCanReceiveSourceEvents(principal);
            var now = DateTimeOffset.UtcNow;
            var sourceProduct = RequireTrimmed(request.SourceProduct, nameof(request.SourceProduct)).ToLowerInvariant();
            var sourceEventId = RequireTrimmed(request.SourceEventId, nameof(request.SourceEventId));
            var eventType = RequireTrimmed(request.EventType, nameof(request.EventType));
            var sourceObjectRef = string.IsNullOrWhiteSpace(request.SourceObjectRef) ? null : request.SourceObjectRef.Trim();
            var correlationId = string.IsNullOrWhiteSpace(request.CorrelationId) ? null : request.CorrelationId.Trim();
            var connector = _sourceConnectors.FirstOrDefault(item => string.Equals(item.SourceProduct, sourceProduct, StringComparison.OrdinalIgnoreCase) && IsCurrentTenant(principal, item.TenantId));
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
                correlationId,
                CurrentTenantId(principal));
            _sourceEvents.Add(receipt);
            ApplySourceEventOutcome(principal, sourceProduct, status, now);
            PersistSnapshot();
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

    private string ResolveFreshnessStatus(ClaimsPrincipal principal)
    {
        if (CurrentTenantItems(principal, _datasets, dataset => dataset.TenantId).Any(dataset => dataset.FreshnessStatus == "failed"))
        {
            return "failed";
        }

        if (CurrentTenantItems(principal, _datasets, dataset => dataset.TenantId).Any(dataset => dataset.FreshnessStatus == "stale"))
        {
            return "stale";
        }

        if (CurrentTenantItems(principal, _datasets, dataset => dataset.TenantId).Any(dataset => dataset.FreshnessStatus == "slightly_stale"))
        {
            return "slightly_stale";
        }

        return "fresh";
    }

    private int GetFreshnessWarnings(ClaimsPrincipal principal) =>
        CurrentTenantItems(principal, _datasets, dataset => dataset.TenantId)
            .Count(dataset => dataset.FreshnessStatus is "stale" or "failed" or "slightly_stale");

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
        if (!IsCurrentTenant(principal, export.TenantId))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(export.ReportRunId))
        {
            var run = _reportRuns.FirstOrDefault(item => item.ReportRunId == export.ReportRunId && IsCurrentTenant(principal, item.TenantId));
            return run is not null && CanAccessReport(principal, run.ReportDefinitionId);
        }

        if (string.Equals(export.ExportType, "dashboard", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(export.SourceRef))
        {
            var dashboard = _dashboards.FirstOrDefault(item => item.DashboardId == export.SourceRef && IsCurrentTenant(principal, item.TenantId));
            return dashboard is not null && CanAccessDashboard(principal, dashboard);
        }

        if ((string.Equals(export.ExportType, "dataset", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(export.ExportType, "table", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(export.ExportType, "chart", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(export.ExportType, "custom", StringComparison.OrdinalIgnoreCase)) &&
            !string.IsNullOrWhiteSpace(export.SourceRef))
        {
            var dataset = _datasets.FirstOrDefault(item => item.DatasetId == export.SourceRef && IsCurrentTenant(principal, item.TenantId));
            if (dataset is not null)
            {
                return CanAccessSourceProducts(principal, dataset.SourceProducts);
            }

            var auditPackage = _auditPackages.FirstOrDefault(item => item.AuditReportPackageId == export.SourceRef && IsCurrentTenant(principal, item.TenantId));
            if (auditPackage is not null)
            {
                return CanAccessAuditPackage(principal, auditPackage);
            }

            var report = _reportDefinitions.FirstOrDefault(item => item.ReportDefinitionId == export.SourceRef && IsCurrentTenant(principal, item.TenantId));
            if (report is not null)
            {
                return CanAccessReport(principal, report);
            }

            var dashboard = _dashboards.FirstOrDefault(item => item.DashboardId == export.SourceRef && IsCurrentTenant(principal, item.TenantId));
            if (dashboard is not null)
            {
                return CanAccessDashboard(principal, dashboard);
            }
        }

        return string.Equals(export.RequestedByPersonId, principal.GetPersonId().ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private void ApplySourceEventOutcome(ClaimsPrincipal principal, string sourceProduct, string status, DateTimeOffset now)
    {
        var impactedDatasets = CurrentTenantItems(principal, _datasets, dataset => dataset.TenantId)
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

        foreach (var readModel in CurrentTenantItems(principal, _readModels, model => model.TenantId).Where(model => model.DatasetRefs.Any(datasetId => impactedDatasetIds.Contains(datasetId))).ToList())
        {
            var updatedReadModel = readModel with
            {
                Status = processed ? "active" : "stale",
                LastUpdatedAt = now,
                UpdatedAt = now
            };
            ReplaceReadModel(updatedReadModel);
        }

        foreach (var dashboard in CurrentTenantItems(principal, _dashboards, item => item.TenantId)
                     .Where(item => _widgets.Any(widget =>
                         IsCurrentTenant(principal, widget.TenantId) &&
                         string.Equals(widget.DashboardId, item.DashboardId, StringComparison.OrdinalIgnoreCase) &&
                         impactedDatasetIds.Contains(widget.DatasetRef)))
                     .ToList())
        {
            var updatedDashboard = dashboard with
            {
                FreshnessStatus = ResolveDashboardFreshness(principal, dashboard.DashboardId),
                UpdatedAt = now,
                UpdatedByPersonId = "reportarr-system"
            };
            ReplaceDashboard(updatedDashboard);
        }
    }

    private string ResolveDashboardFreshness(ClaimsPrincipal principal, string dashboardId)
    {
        var widgetDatasetIds = CurrentTenantItems(principal, _widgets, widget => widget.TenantId)
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
            .Select(datasetId => _datasets.FirstOrDefault(dataset => string.Equals(dataset.DatasetId, datasetId, StringComparison.OrdinalIgnoreCase) && IsCurrentTenant(principal, dataset.TenantId))?.FreshnessStatus)
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


