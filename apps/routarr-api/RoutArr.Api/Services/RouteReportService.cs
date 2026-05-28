using System.Text;
using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed class RouteReportService(RoutArrDbContext db)
{
    private const int RecentStopLimit = 25;

    public async Task<RouteReportSummaryResponse> GetSummaryAsync(
        Guid tenantId,
        string? scope,
        CancellationToken cancellationToken = default)
    {
        var normalizedScope = NormalizeScope(scope);
        var now = DateTimeOffset.UtcNow;
        var (windowStart, windowEnd) = GetWindow(normalizedScope, now);

        var routes = await db.Routes
            .AsNoTracking()
            .Include(x => x.Trip)
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        var scopedRoutes = routes.Where(x => IsRouteInScope(x, windowStart, windowEnd)).ToList();
        var routeIds = scopedRoutes.Select(x => x.Id).ToHashSet();

        var stops = await db.RouteStops
            .AsNoTracking()
            .Include(x => x.Route)
            .Where(x => x.TenantId == tenantId && routeIds.Contains(x.RouteId))
            .ToListAsync(cancellationToken);

        var scopedStops = stops.Where(x => IsStopInScope(x, windowStart, windowEnd)).ToList();
        var stopsByRoute = scopedStops.GroupBy(x => x.RouteId).ToDictionary(x => x.Key, x => x.ToList());

        var routeItems = scopedRoutes
            .Select(route =>
            {
                stopsByRoute.TryGetValue(route.Id, out var routeStops);
                routeStops ??= [];
                var total = routeStops.Count;
                var pending = CountStopStatus(routeStops, RouteStopStatuses.Pending);
                var arrived = CountStopStatus(routeStops, RouteStopStatuses.Arrived);
                var completed = CountStopStatus(routeStops, RouteStopStatuses.Completed);
                var skipped = CountStopStatus(routeStops, RouteStopStatuses.Skipped);
                return new RouteReportRouteSummaryItem(
                    route.Id,
                    route.RouteNumber,
                    route.Title,
                    route.RouteStatus,
                    route.TripId,
                    route.Trip?.TripNumber,
                    total,
                    pending,
                    arrived,
                    completed,
                    skipped,
                    ComputeCompletionPercent(total, completed, skipped));
            })
            .OrderBy(x => x.RouteNumber)
            .ToList();

        var recentStops = scopedStops
            .OrderByDescending(x => x.UpdatedAt)
            .Take(RecentStopLimit)
            .Select(MapStopRow)
            .ToList();

        return new RouteReportSummaryResponse(
            now,
            normalizedScope,
            windowStart,
            windowEnd,
            scopedRoutes.Count,
            scopedStops.Count,
            CountStopStatus(scopedStops, RouteStopStatuses.Pending),
            CountStopStatus(scopedStops, RouteStopStatuses.Arrived),
            CountStopStatus(scopedStops, RouteStopStatuses.Completed),
            CountStopStatus(scopedStops, RouteStopStatuses.Skipped),
            CountBy(scopedRoutes.Select(x => x.RouteStatus)),
            CountBy(scopedStops.Select(x => x.StopStatus)),
            CountBy(scopedStops.Select(x => x.StopType)),
            routeItems,
            recentStops);
    }

    public async Task<RouteReportRouteDetailResponse> GetRouteDetailAsync(
        Guid tenantId,
        Guid routeId,
        CancellationToken cancellationToken = default)
    {
        var route = await db.Routes
            .AsNoTracking()
            .Include(x => x.Trip)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == routeId, cancellationToken)
            ?? throw new StlApiException("reports.route_not_found", "Route was not found.", 404);

        var stops = await db.RouteStops
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.RouteId == routeId)
            .OrderBy(x => x.SequenceNumber)
            .ToListAsync(cancellationToken);

        var pending = CountStopStatus(stops, RouteStopStatuses.Pending);
        var completed = CountStopStatus(stops, RouteStopStatuses.Completed);
        var skipped = CountStopStatus(stops, RouteStopStatuses.Skipped);

        return new RouteReportRouteDetailResponse(
            route.Id,
            route.RouteNumber,
            route.Title,
            route.Description,
            route.RouteStatus,
            route.TripId,
            route.Trip?.TripNumber,
            route.Trip?.Title,
            stops.Count,
            pending,
            completed,
            skipped,
            ComputeCompletionPercent(stops.Count, completed, skipped),
            route.CreatedAt,
            route.UpdatedAt,
            route.ActivatedAt,
            route.CompletedAt,
            stops.Select(x => new RouteReportStopSummaryRow(
                x.Id,
                x.StopKey,
                x.Label,
                x.AddressLabel,
                x.StopType,
                x.StopStatus,
                x.SequenceNumber,
                x.ScheduledArrivalAt,
                x.ArrivedAt,
                x.CompletedAt,
                x.UpdatedAt)).ToList());
    }

    public async Task<RouteReportStopDetailResponse> GetStopDetailAsync(
        Guid tenantId,
        Guid stopId,
        CancellationToken cancellationToken = default)
    {
        var stop = await db.RouteStops
            .AsNoTracking()
            .Include(x => x.Route)
            .ThenInclude(x => x.Trip)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == stopId, cancellationToken)
            ?? throw new StlApiException("reports.stop_not_found", "Route stop was not found.", 404);

        return new RouteReportStopDetailResponse(
            stop.Id,
            stop.RouteId,
            stop.Route.RouteNumber,
            stop.Route.Title,
            stop.Route.TripId,
            stop.Route.Trip?.TripNumber,
            stop.StopKey,
            stop.Label,
            stop.AddressLabel,
            stop.StopType,
            stop.StopStatus,
            stop.SequenceNumber,
            stop.ScheduledArrivalAt,
            stop.ArrivedAt,
            stop.CompletedAt,
            stop.CreatedAt,
            stop.UpdatedAt);
    }

    public async Task<(string ContentType, string FileName, byte[] Content)> ExportSummaryCsvAsync(
        Guid tenantId,
        string? scope,
        CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(tenantId, scope, cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine(
            "routeNumber,title,routeStatus,tripNumber,totalStopCount,pendingStopCount,arrivedStopCount,completedStopCount,skippedStopCount,completionPercent");

        foreach (var route in summary.Routes)
        {
            builder.Append(CsvEscape(route.RouteNumber));
            builder.Append(',');
            builder.Append(CsvEscape(route.Title));
            builder.Append(',');
            builder.Append(CsvEscape(route.RouteStatus));
            builder.Append(',');
            builder.Append(CsvEscape(route.TripNumber ?? string.Empty));
            builder.Append(',');
            builder.Append(route.TotalStopCount);
            builder.Append(',');
            builder.Append(route.PendingStopCount);
            builder.Append(',');
            builder.Append(route.ArrivedStopCount);
            builder.Append(',');
            builder.Append(route.CompletedStopCount);
            builder.Append(',');
            builder.Append(route.SkippedStopCount);
            builder.Append(',');
            builder.AppendLine(route.CompletionPercent.ToString());
        }

        var fileName = $"routarr-route-report-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.csv";
        return ("text/csv", fileName, Encoding.UTF8.GetBytes(builder.ToString()));
    }

    private static RouteReportStopRow MapStopRow(RouteStop stop) =>
        new(
            stop.Id,
            stop.RouteId,
            stop.Route.RouteNumber,
            stop.StopKey,
            stop.Label,
            stop.StopType,
            stop.StopStatus,
            stop.SequenceNumber,
            stop.ScheduledArrivalAt,
            stop.UpdatedAt);

    private static int CountStopStatus(IEnumerable<RouteStop> stops, string status) =>
        stops.Count(x => string.Equals(x.StopStatus, status, StringComparison.OrdinalIgnoreCase));

    private static int ComputeCompletionPercent(int total, int completed, int skipped)
    {
        if (total == 0)
        {
            return 0;
        }

        return (int)Math.Round((completed + skipped) * 100.0 / total, MidpointRounding.AwayFromZero);
    }

    private static string NormalizeScope(string? scope) =>
        string.Equals(scope, DispatchBoardService.ScopeWeekly, StringComparison.OrdinalIgnoreCase)
            ? DispatchBoardService.ScopeWeekly
            : DispatchBoardService.ScopeDaily;

    private static (DateTimeOffset WindowStart, DateTimeOffset WindowEnd) GetWindow(string scope, DateTimeOffset now)
    {
        var dayStart = new DateTimeOffset(now.UtcDateTime.Date, TimeSpan.Zero);
        return scope == DispatchBoardService.ScopeWeekly
            ? (dayStart, dayStart.AddDays(7))
            : (dayStart, dayStart.AddDays(1));
    }

    private static bool IsRouteInScope(DispatchRoute route, DateTimeOffset windowStart, DateTimeOffset windowEnd) =>
        route.RouteStatus is RouteStatuses.Draft or RouteStatuses.Planned or RouteStatuses.Active
        || (route.CreatedAt >= windowStart && route.CreatedAt < windowEnd)
        || (route.Trip?.ScheduledStartAt is not null
            && OverlapsWindow(route.Trip.ScheduledStartAt, route.Trip.ScheduledEndAt, windowStart, windowEnd));

    private static bool IsStopInScope(RouteStop stop, DateTimeOffset windowStart, DateTimeOffset windowEnd) =>
        !RouteStopStatuses.Terminal.Contains(stop.StopStatus)
        || OverlapsWindow(stop.ScheduledArrivalAt, stop.ScheduledArrivalAt, windowStart, windowEnd)
        || (stop.CreatedAt >= windowStart && stop.CreatedAt < windowEnd);

    private static bool OverlapsWindow(
        DateTimeOffset? startAt,
        DateTimeOffset? endAt,
        DateTimeOffset windowStart,
        DateTimeOffset windowEnd)
    {
        if (startAt.HasValue && startAt.Value >= windowStart && startAt.Value < windowEnd)
        {
            return true;
        }

        if (endAt.HasValue && endAt.Value >= windowStart && endAt.Value < windowEnd)
        {
            return true;
        }

        if (startAt.HasValue && endAt.HasValue
            && startAt.Value < windowEnd
            && endAt.Value >= windowStart)
        {
            return true;
        }

        return false;
    }

    private static IReadOnlyList<RouteReportCountItem> CountBy(IEnumerable<string> keys) =>
        keys
            .GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
            .Select(g => new RouteReportCountItem(g.Key, g.Count()))
            .OrderBy(x => x.Key)
            .ToList();

    private static string CsvEscape(string value)
    {
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        }

        return value;
    }
}
