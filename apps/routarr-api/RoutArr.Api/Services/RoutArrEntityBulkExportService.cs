using System.Text;
using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;

namespace RoutArr.Api.Services;

public sealed class RoutArrEntityBulkExportService(
    RoutArrDbContext db,
    IRoutArrAuditService auditService)
{
    public const string TripsCsvHeader =
        "tripNumber,title,description,dispatchStatus,assignedDriverPersonId,vehicleRefKey,scheduledStartAt,scheduledEndAt,dispatchedAt,startedAt,completedAt,cancelledAt,tripId,createdAt,updatedAt";

    public const string RoutesCsvHeader =
        "routeNumber,title,description,routeStatus,tripNumber,stopCount,routeId,tripId,createdAt,updatedAt,activatedAt,completedAt";

    public const string DispatchExceptionsCsvHeader =
        "exceptionKey,title,description,category,status,tripNumber,tripId,assignedToUserId,createdAt,updatedAt,resolvedAt,exceptionId";

    private static readonly EntityExportFormatDescriptor CsvFormat = new(
        "csv",
        "text/csv",
        "routarr-{entity}-export-{timestamp}.csv",
        "Comma-separated values for spreadsheets and operational analysis.");

    public EntityExportManifestResponse GetManifest() =>
        new(
            PackageVersion: "1",
            Entities:
            [
                new(
                    "trips",
                    "/api/exports/trips",
                    "Trips",
                    TripsCsvHeader,
                    "Tenant trip registry with dispatch status and schedule timestamps.",
                    [CsvFormat]),
                new(
                    "routes",
                    "/api/exports/routes",
                    "Routes",
                    RoutesCsvHeader,
                    "Route definitions with linked trip numbers and stop counts.",
                    [CsvFormat]),
                new(
                    "dispatch_exceptions",
                    "/api/exports/dispatch-exceptions",
                    "Dispatch exceptions",
                    DispatchExceptionsCsvHeader,
                    "Dispatch exception queue including delay-category rows.",
                    [CsvFormat]),
            ],
            ReportExports:
            [
                new(
                    "dispatch",
                    "/api/reports/dispatch/summary/export",
                    "Dispatch report CSV",
                    "Scoped trip rollups with late/at-risk flags (Worker 214)."),
                new(
                    "routes_report",
                    "/api/reports/routes/summary/export",
                    "Route execution report CSV",
                    "Scoped route and stop completion metrics (Worker 215)."),
                new(
                    "proof_dvir_report",
                    "/api/reports/proof-dvir/summary/export",
                    "Proof & DVIR report CSV",
                    "Scoped pickup/delivery proof and pre/post-trip DVIR rows (Worker 218)."),
            ],
            AuditPackageFormats: []);

    public async Task<(string ContentType, string FileName, byte[] Content)> ExportTripsCsvAsync(
        Guid tenantId,
        Guid actorUserId,
        string? dispatchStatus,
        CancellationToken cancellationToken = default)
    {
        var query = db.Trips.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(dispatchStatus))
        {
            var normalized = dispatchStatus.Trim().ToLowerInvariant();
            query = query.Where(x => x.DispatchStatus == normalized);
        }

        var trips = await query.OrderBy(x => x.TripNumber).ToListAsync(cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine(TripsCsvHeader);

        foreach (var trip in trips)
        {
            builder.Append(CsvEscape(trip.TripNumber));
            builder.Append(',');
            builder.Append(CsvEscape(trip.Title));
            builder.Append(',');
            builder.Append(CsvEscape(trip.Description));
            builder.Append(',');
            builder.Append(CsvEscape(trip.DispatchStatus));
            builder.Append(',');
            builder.Append(CsvEscape(trip.AssignedDriverPersonId ?? string.Empty));
            builder.Append(',');
            builder.Append(CsvEscape(trip.VehicleRefKey ?? string.Empty));
            builder.Append(',');
            builder.Append(trip.ScheduledStartAt?.ToString("O") ?? string.Empty);
            builder.Append(',');
            builder.Append(trip.ScheduledEndAt?.ToString("O") ?? string.Empty);
            builder.Append(',');
            builder.Append(trip.DispatchedAt?.ToString("O") ?? string.Empty);
            builder.Append(',');
            builder.Append(trip.StartedAt?.ToString("O") ?? string.Empty);
            builder.Append(',');
            builder.Append(trip.CompletedAt?.ToString("O") ?? string.Empty);
            builder.Append(',');
            builder.Append(trip.CancelledAt?.ToString("O") ?? string.Empty);
            builder.Append(',');
            builder.Append(trip.Id);
            builder.Append(',');
            builder.Append(trip.CreatedAt.ToString("O"));
            builder.Append(',');
            builder.AppendLine(trip.UpdatedAt.ToString("O"));
        }

        await WriteExportAuditAsync("routarr.exports.trips", tenantId, actorUserId, trips.Count, cancellationToken);
        return ("text/csv", $"routarr-trips-export-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.csv", Encoding.UTF8.GetBytes(builder.ToString()));
    }

    public async Task<(string ContentType, string FileName, byte[] Content)> ExportRoutesCsvAsync(
        Guid tenantId,
        Guid actorUserId,
        string? routeStatus,
        CancellationToken cancellationToken = default)
    {
        var routesQuery = db.Routes.AsNoTracking().Include(x => x.Trip).Where(x => x.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(routeStatus))
        {
            var normalized = routeStatus.Trim().ToLowerInvariant();
            routesQuery = routesQuery.Where(x => x.RouteStatus == normalized);
        }

        var routes = await routesQuery.OrderBy(x => x.RouteNumber).ToListAsync(cancellationToken);
        var routeIds = routes.Select(x => x.Id).ToList();
        var stopCounts = await db.RouteStops
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && routeIds.Contains(x.RouteId))
            .GroupBy(x => x.RouteId)
            .Select(g => new { RouteId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.RouteId, x => x.Count, cancellationToken);

        var builder = new StringBuilder();
        builder.AppendLine(RoutesCsvHeader);

        foreach (var route in routes)
        {
            builder.Append(CsvEscape(route.RouteNumber));
            builder.Append(',');
            builder.Append(CsvEscape(route.Title));
            builder.Append(',');
            builder.Append(CsvEscape(route.Description));
            builder.Append(',');
            builder.Append(CsvEscape(route.RouteStatus));
            builder.Append(',');
            builder.Append(CsvEscape(route.Trip?.TripNumber ?? string.Empty));
            builder.Append(',');
            builder.Append(stopCounts.GetValueOrDefault(route.Id));
            builder.Append(',');
            builder.Append(route.Id);
            builder.Append(',');
            builder.Append(route.TripId?.ToString() ?? string.Empty);
            builder.Append(',');
            builder.Append(route.CreatedAt.ToString("O"));
            builder.Append(',');
            builder.Append(route.UpdatedAt.ToString("O"));
            builder.Append(',');
            builder.Append(route.ActivatedAt?.ToString("O") ?? string.Empty);
            builder.Append(',');
            builder.AppendLine(route.CompletedAt?.ToString("O") ?? string.Empty);
        }

        await WriteExportAuditAsync("routarr.exports.routes", tenantId, actorUserId, routes.Count, cancellationToken);
        return ("text/csv", $"routarr-routes-export-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.csv", Encoding.UTF8.GetBytes(builder.ToString()));
    }

    public async Task<(string ContentType, string FileName, byte[] Content)> ExportDispatchExceptionsCsvAsync(
        Guid tenantId,
        Guid actorUserId,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var query =
            from exception in db.DispatchExceptions.AsNoTracking()
            join trip in db.Trips.AsNoTracking() on exception.TripId equals trip.Id into tripJoin
            from trip in tripJoin.DefaultIfEmpty()
            where exception.TenantId == tenantId
            select new { exception, trip };

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalized = status.Trim().ToLowerInvariant();
            query = query.Where(x => x.exception.Status == normalized);
        }

        var rows = await query.OrderByDescending(x => x.exception.UpdatedAt).ToListAsync(cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine(DispatchExceptionsCsvHeader);

        foreach (var row in rows)
        {
            var entity = row.exception;
            builder.Append(CsvEscape(entity.ExceptionKey));
            builder.Append(',');
            builder.Append(CsvEscape(entity.Title));
            builder.Append(',');
            builder.Append(CsvEscape(entity.Description));
            builder.Append(',');
            builder.Append(CsvEscape(entity.Category));
            builder.Append(',');
            builder.Append(CsvEscape(entity.Status));
            builder.Append(',');
            builder.Append(CsvEscape(row.trip?.TripNumber ?? string.Empty));
            builder.Append(',');
            builder.Append(entity.TripId?.ToString() ?? string.Empty);
            builder.Append(',');
            builder.Append(entity.AssignedToUserId?.ToString() ?? string.Empty);
            builder.Append(',');
            builder.Append(entity.CreatedAt.ToString("O"));
            builder.Append(',');
            builder.Append(entity.UpdatedAt.ToString("O"));
            builder.Append(',');
            builder.Append(entity.ResolvedAt?.ToString("O") ?? string.Empty);
            builder.Append(',');
            builder.AppendLine(entity.Id.ToString());
        }

        await WriteExportAuditAsync(
            "routarr.exports.dispatch_exceptions",
            tenantId,
            actorUserId,
            rows.Count,
            cancellationToken);
        return (
            "text/csv",
            $"routarr-dispatch-exceptions-export-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.csv",
            Encoding.UTF8.GetBytes(builder.ToString()));
    }

    private async Task WriteExportAuditAsync(
        string action,
        Guid tenantId,
        Guid actorUserId,
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
