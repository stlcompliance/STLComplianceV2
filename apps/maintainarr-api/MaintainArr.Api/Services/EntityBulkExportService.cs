using System.Text;
using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
namespace MaintainArr.Api.Services;

public sealed class EntityBulkExportService(
    MaintainArrDbContext db,
    AssetService assetService,
    IMaintainArrAuditService auditService)
{
    public const string AssetsCsvHeader =
        "assetClassKey,assetTypeKey,assetTag,name,description,siteRef,lifecycleStatus,assetId,createdAt,updatedAt";

    public const string WorkOrdersCsvHeader =
        "workOrderNumber,assetTag,title,description,priority,status,source,assignedTechnicianPersonId,createdAt,updatedAt,startedAt,completedAt,cancelledAt,workOrderId,assetId";

    public const string InspectionRunsCsvHeader =
        "assetTag,templateKey,templateVersion,status,result,startedAt,completedAt,inspectionRunId,assetId";

    private static readonly EntityExportFormatDescriptor CsvFormat = new(
        "csv",
        "text/csv",
        "maintainarr-{entity}-export-{timestamp}.csv",
        "Comma-separated values for spreadsheets and bulk import round-trips.");

    public EntityExportManifestResponse GetManifest() =>
        new(
            PackageVersion: "1",
            Entities:
            [
                new(
                    "assets",
                    "/api/exports/assets",
                    "Assets",
                    AssetsCsvHeader,
                    "Full tenant asset registry with class and type keys.",
                    [CsvFormat]),
                new(
                    "work_orders",
                    "/api/exports/work-orders",
                    "Work orders",
                    WorkOrdersCsvHeader,
                    "Maintenance work orders with asset tags for cross-reference.",
                    [CsvFormat]),
                new(
                    "inspection_runs",
                    "/api/exports/inspection-runs",
                    "Inspection runs",
                    InspectionRunsCsvHeader,
                    "Inspection execution history with template keys.",
                    [CsvFormat]),
            ],
            ReportExports:
            [
                new(
                    "maintenance",
                    "/api/reports/maintenance/summary/export",
                    "Maintenance report CSV",
                    "Aggregated fleet maintenance KPIs per asset (Worker 203)."),
                new(
                    "executive",
                    "/api/reports/executive/summary/export",
                    "Executive report CSV",
                    "Fleet readiness and scope rollups (Worker 204)."),
                new(
                    "compliance",
                    "/api/reports/compliance/summary/export",
                    "Compliance report CSV",
                    "Regulatory key coverage on inspection templates (Worker 205)."),
            ],
            AuditPackageFormats:
            [
                new("zip", "application/zip", "maintainarr-audit-package-{timestamp}.zip", "ZIP bundle with JSON sections."),
                new("json", "application/json", "maintainarr-audit-package.json", "Structured audit package JSON."),
            ]);

    public async Task<(string ContentType, string FileName, byte[] Content)> ExportAssetsCsvAsync(
        Guid tenantId,
        Guid? actorUserId,
        string? lifecycleStatus,
        CancellationToken cancellationToken = default)
    {
        var assets = await assetService.ListAsync(tenantId, cancellationToken);
        if (!string.IsNullOrWhiteSpace(lifecycleStatus))
        {
            var normalized = lifecycleStatus.Trim().ToLowerInvariant();
            assets = assets
                .Where(x => string.Equals(x.LifecycleStatus, normalized, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var builder = new StringBuilder();
        builder.AppendLine(AssetsCsvHeader);

        foreach (var asset in assets)
        {
            builder.Append(CsvEscape(asset.ClassKey));
            builder.Append(',');
            builder.Append(CsvEscape(asset.TypeKey));
            builder.Append(',');
            builder.Append(CsvEscape(asset.AssetTag));
            builder.Append(',');
            builder.Append(CsvEscape(asset.Name));
            builder.Append(',');
            builder.Append(CsvEscape(asset.Description));
            builder.Append(',');
            builder.Append(CsvEscape(asset.SiteRef ?? string.Empty));
            builder.Append(',');
            builder.Append(CsvEscape(asset.LifecycleStatus));
            builder.Append(',');
            builder.Append(asset.AssetId);
            builder.Append(',');
            builder.Append(asset.CreatedAt.ToString("O"));
            builder.Append(',');
            builder.AppendLine(asset.UpdatedAt.ToString("O"));
        }

        await WriteExportAuditAsync(
            "maintainarr.exports.assets",
            tenantId,
            actorUserId,
            assets.Count,
            cancellationToken);

        var fileName = $"maintainarr-assets-export-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.csv";
        return ("text/csv", fileName, Encoding.UTF8.GetBytes(builder.ToString()));
    }

    public async Task<(string ContentType, string FileName, byte[] Content)> ExportWorkOrdersCsvAsync(
        Guid tenantId,
        Guid? actorUserId,
        string? status,
        Guid? assetId,
        CancellationToken cancellationToken = default)
    {
        var workOrdersQuery =
            from workOrder in db.WorkOrders.AsNoTracking()
            join asset in db.Assets.AsNoTracking() on workOrder.AssetId equals asset.Id
            where workOrder.TenantId == tenantId && asset.TenantId == tenantId
            select new { workOrder, asset };

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalized = status.Trim().ToLowerInvariant();
            workOrdersQuery = workOrdersQuery.Where(x => x.workOrder.Status == normalized);
        }

        if (assetId is not null)
        {
            workOrdersQuery = workOrdersQuery.Where(x => x.workOrder.AssetId == assetId);
        }

        var workOrders = await workOrdersQuery.OrderBy(x => x.workOrder.WorkOrderNumber).ToListAsync(cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine(WorkOrdersCsvHeader);

        foreach (var row in workOrders)
        {
            var workOrder = row.workOrder;
            builder.Append(CsvEscape(workOrder.WorkOrderNumber));
            builder.Append(',');
            builder.Append(CsvEscape(row.asset.AssetTag));
            builder.Append(',');
            builder.Append(CsvEscape(workOrder.Title));
            builder.Append(',');
            builder.Append(CsvEscape(workOrder.Description));
            builder.Append(',');
            builder.Append(CsvEscape(workOrder.Priority));
            builder.Append(',');
            builder.Append(CsvEscape(workOrder.Status));
            builder.Append(',');
            builder.Append(CsvEscape(workOrder.Source));
            builder.Append(',');
            builder.Append(CsvEscape(workOrder.AssignedTechnicianPersonId ?? string.Empty));
            builder.Append(',');
            builder.Append(workOrder.CreatedAt.ToString("O"));
            builder.Append(',');
            builder.Append(workOrder.UpdatedAt.ToString("O"));
            builder.Append(',');
            builder.Append(workOrder.StartedAt?.ToString("O") ?? string.Empty);
            builder.Append(',');
            builder.Append(workOrder.CompletedAt?.ToString("O") ?? string.Empty);
            builder.Append(',');
            builder.Append(workOrder.CancelledAt?.ToString("O") ?? string.Empty);
            builder.Append(',');
            builder.Append(workOrder.Id);
            builder.Append(',');
            builder.AppendLine(workOrder.AssetId.ToString());
        }

        await WriteExportAuditAsync(
            "maintainarr.exports.work_orders",
            tenantId,
            actorUserId,
            workOrders.Count,
            cancellationToken);

        var fileName = $"maintainarr-work-orders-export-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.csv";
        return ("text/csv", fileName, Encoding.UTF8.GetBytes(builder.ToString()));
    }

    public async Task<(string ContentType, string FileName, byte[] Content)> ExportInspectionRunsCsvAsync(
        Guid tenantId,
        Guid? actorUserId,
        string? status,
        Guid? assetId,
        CancellationToken cancellationToken = default)
    {
        var runsQuery =
            from run in db.InspectionRuns.AsNoTracking()
            join asset in db.Assets.AsNoTracking() on run.AssetId equals asset.Id
            join template in db.InspectionTemplates.AsNoTracking() on run.InspectionTemplateId equals template.Id
            where run.TenantId == tenantId && asset.TenantId == tenantId && template.TenantId == tenantId
            select new { run, asset, template };

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalized = status.Trim().ToLowerInvariant();
            runsQuery = runsQuery.Where(x => x.run.Status == normalized);
        }

        if (assetId is not null)
        {
            runsQuery = runsQuery.Where(x => x.run.AssetId == assetId);
        }

        var runs = await runsQuery.OrderByDescending(x => x.run.StartedAt).ToListAsync(cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine(InspectionRunsCsvHeader);

        foreach (var row in runs)
        {
            builder.Append(CsvEscape(row.asset.AssetTag));
            builder.Append(',');
            builder.Append(CsvEscape(row.template.TemplateKey));
            builder.Append(',');
            builder.Append(row.run.TemplateVersion);
            builder.Append(',');
            builder.Append(CsvEscape(row.run.Status));
            builder.Append(',');
            builder.Append(CsvEscape(row.run.Result ?? string.Empty));
            builder.Append(',');
            builder.Append(row.run.StartedAt.ToString("O"));
            builder.Append(',');
            builder.Append(row.run.CompletedAt?.ToString("O") ?? string.Empty);
            builder.Append(',');
            builder.Append(row.run.Id);
            builder.Append(',');
            builder.AppendLine(row.run.AssetId.ToString());
        }

        await WriteExportAuditAsync(
            "maintainarr.exports.inspection_runs",
            tenantId,
            actorUserId,
            runs.Count,
            cancellationToken);

        var fileName = $"maintainarr-inspection-runs-export-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.csv";
        return ("text/csv", fileName, Encoding.UTF8.GetBytes(builder.ToString()));
    }

    private async Task WriteExportAuditAsync(
        string action,
        Guid tenantId,
        Guid? actorUserId,
        int rowCount,
        CancellationToken cancellationToken)
    {
        await auditService.WriteAsync(
            action,
            tenantId,
            actorUserId,
            "entity_export",
            null,
            "success",
            reasonCode: rowCount.ToString(),
            cancellationToken: cancellationToken);
    }

    private static string CsvEscape(string value)
    {
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        }

        return value;
    }
}
