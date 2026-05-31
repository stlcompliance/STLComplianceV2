using System.Text;
using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed class DispatchReportService(RoutArrDbContext db)
{
    private const int RecentExceptionLimit = 25;
    private const int DefaultAlertLimit = 100;
    private const int MaxAlertLimit = 500;

    public async Task<DispatchReportSummaryResponse> GetSummaryAsync(
        Guid tenantId,
        string? scope,
        CancellationToken cancellationToken = default)
    {
        var normalizedScope = NormalizeScope(scope);
        var now = DateTimeOffset.UtcNow;
        var (windowStart, windowEnd) = GetWindow(normalizedScope, now);

        var trips = await db.Trips
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        var scopedTrips = trips.Where(x => IsTripInScope(x, windowStart, windowEnd)).ToList();
        var tripIds = scopedTrips.Select(x => x.Id).ToHashSet();
        var releaseSnapshots = await db.TripDispatchReleaseSnapshots
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && tripIds.Contains(x.TripId))
            .ToListAsync(cancellationToken);
        var releaseSnapshotByTrip = releaseSnapshots.ToDictionary(x => x.TripId);

        var settingsEntity = await db.TenantTripExecutionSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);
        var captureSettings = TripExecutionCaptureRules.ResolveSettings(settingsEntity);

        var proofTypesByTrip = await db.TripProofRecords
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && tripIds.Contains(x.TripId))
            .Select(x => new { x.TripId, x.ProofType })
            .ToListAsync(cancellationToken);
        var proofLookup = proofTypesByTrip
            .GroupBy(x => x.TripId)
            .ToDictionary(
                x => x.Key,
                x => x.Select(y => y.ProofType).ToHashSet(StringComparer.OrdinalIgnoreCase));

        var missingRequiredProofByTrip = scopedTrips.ToDictionary(
            x => x.Id,
            x => DispatchBoardRules.CountMissingRequiredProof(x, captureSettings, proofLookup.GetValueOrDefault(x.Id)));

        var routes = await db.Routes
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.TripId.HasValue && tripIds.Contains(x.TripId.Value))
            .Select(x => x.TripId!.Value)
            .ToListAsync(cancellationToken);
        var routeCountByTrip = routes
            .GroupBy(x => x)
            .ToDictionary(x => x.Key, x => x.Count());

        var exceptions = await db.DispatchExceptions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(cancellationToken);
        var scopedExceptions = exceptions
            .Where(x => IsExceptionInScope(x, windowStart, windowEnd))
            .ToList();

        var openExceptionCountByTrip = scopedExceptions
            .Where(x => x.TripId.HasValue && DispatchExceptionStatuses.OpenQueue.Contains(x.Status))
            .GroupBy(x => x.TripId!.Value)
            .ToDictionary(x => x.Key, x => x.Count());

        var tripItems = scopedTrips
            .Select(trip =>
            {
                var isLate = DispatchBoardRules.IsLateTrip(trip, now);
                var isAtRisk = !isLate && DispatchBoardRules.IsAtRiskTrip(trip, now);
                var isUnassigned = string.IsNullOrWhiteSpace(trip.AssignedDriverPersonId);
                return new DispatchReportTripSummaryItem(
                    trip.Id,
                    trip.TripNumber,
                    trip.Title,
                    trip.DispatchStatus,
                    trip.AssignedDriverPersonId,
                    trip.VehicleRefKey,
                    trip.ScheduledStartAt,
                    trip.ScheduledEndAt,
                    isLate,
                    isAtRisk,
                    isUnassigned,
                    routeCountByTrip.GetValueOrDefault(trip.Id),
                    missingRequiredProofByTrip.GetValueOrDefault(trip.Id),
                    openExceptionCountByTrip.GetValueOrDefault(trip.Id),
                    releaseSnapshotByTrip.ContainsKey(trip.Id),
                    releaseSnapshotByTrip.GetValueOrDefault(trip.Id)?.HasMissingExternalData == true,
                    releaseSnapshotByTrip.GetValueOrDefault(trip.Id)?.HasStaleExternalData == true);
            })
            .OrderBy(x => x.ScheduledStartAt ?? DateTimeOffset.MaxValue)
            .ThenBy(x => x.TripNumber)
            .ToList();

        var recentExceptions = scopedExceptions
            .OrderByDescending(x => x.UpdatedAt)
            .Take(RecentExceptionLimit)
            .Select(MapExceptionRow)
            .ToList();

        return new DispatchReportSummaryResponse(
            now,
            normalizedScope,
            windowStart,
            windowEnd,
            scopedTrips.Count,
            tripItems.Count(x => x.IsLate),
            tripItems.Count(x => x.IsAtRisk),
            tripItems.Count(x => x.IsUnassigned),
            tripItems.Count(x => x.MissingRequiredProofCount > 0),
            scopedExceptions.Count(x => DispatchExceptionStatuses.OpenQueue.Contains(x.Status)),
            scopedExceptions.Count(x =>
                string.Equals(x.Category, DispatchExceptionCategories.Delay, StringComparison.OrdinalIgnoreCase)),
            tripItems.Count(x => x.HasDispatchReleaseSnapshot),
            tripItems.Count(x => x.ReleaseHasMissingExternalData),
            tripItems.Count(x => x.ReleaseHasStaleExternalData),
            CountBy(scopedTrips.Select(x => x.DispatchStatus)),
            CountBy(scopedExceptions.Select(x => x.Status)),
            CountBy(scopedExceptions.Select(x => x.Category)),
            tripItems,
            recentExceptions);
    }

    public async Task<DispatchReportTripDetailResponse> GetTripDetailAsync(
        Guid tenantId,
        Guid tripId,
        CancellationToken cancellationToken = default)
    {
        var trip = await db.Trips
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == tripId, cancellationToken)
            ?? throw new StlApiException("reports.trip_not_found", "Trip was not found.", 404);
        var releaseSnapshot = await db.TripDispatchReleaseSnapshots
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.TripId == tripId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var isLate = DispatchBoardRules.IsLateTrip(trip, now);
        var isAtRisk = !isLate && DispatchBoardRules.IsAtRiskTrip(trip, now);

        var settingsEntity = await db.TenantTripExecutionSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);
        var captureSettings = TripExecutionCaptureRules.ResolveSettings(settingsEntity);

        var proofTypes = await db.TripProofRecords
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.TripId == tripId)
            .Select(x => x.ProofType)
            .ToListAsync(cancellationToken);
        var missingRequiredProofCount = DispatchBoardRules.CountMissingRequiredProof(
            trip,
            captureSettings,
            proofTypes.ToHashSet(StringComparer.OrdinalIgnoreCase));

        var routeCount = await db.Routes
            .AsNoTracking()
            .CountAsync(x => x.TenantId == tenantId && x.TripId == tripId, cancellationToken);

        var pendingStopCount = await (
            from stop in db.RouteStops.AsNoTracking()
            join route in db.Routes.AsNoTracking() on stop.RouteId equals route.Id
            where stop.TenantId == tenantId
                && route.TripId == tripId
                && stop.StopStatus == RouteStopStatuses.Pending
            select stop.Id)
            .CountAsync(cancellationToken);

        var tripExceptions = await db.DispatchExceptions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.TripId == tripId)
            .Select(x => new { x.Category })
            .ToListAsync(cancellationToken);

        return new DispatchReportTripDetailResponse(
            trip.Id,
            trip.TripNumber,
            trip.Title,
            trip.Description,
            trip.DispatchStatus,
            trip.AssignedDriverPersonId,
            trip.VehicleRefKey,
            trip.ScheduledStartAt,
            trip.ScheduledEndAt,
            trip.DispatchedAt,
            trip.StartedAt,
            trip.CompletedAt,
            trip.CancelledAt,
            isLate,
            isAtRisk,
            routeCount,
            pendingStopCount,
            missingRequiredProofCount,
            tripExceptions.Count,
            tripExceptions.Count(x =>
                string.Equals(x.Category, DispatchExceptionCategories.Delay, StringComparison.OrdinalIgnoreCase)),
            trip.CreatedAt,
            trip.UpdatedAt,
            releaseSnapshot is null
                ? null
                : new TripDispatchReleaseSnapshotResponse(
                    releaseSnapshot.Id,
                    releaseSnapshot.ReleasedAt,
                    releaseSnapshot.ReleasedByUserId,
                    releaseSnapshot.DriverCanAssign,
                    releaseSnapshot.VehicleCanAssign,
                    releaseSnapshot.HasMissingExternalData,
                    releaseSnapshot.HasStaleExternalData,
                    releaseSnapshot.Summary));
    }

    public async Task<DispatchReportExceptionDetailResponse> GetExceptionDetailAsync(
        Guid tenantId,
        Guid exceptionId,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.DispatchExceptions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == exceptionId, cancellationToken)
            ?? throw new StlApiException("reports.exception_not_found", "Dispatch exception was not found.", 404);

        string? tripNumber = null;
        string? tripTitle = null;
        if (entity.TripId.HasValue)
        {
            var trip = await db.Trips
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.Id == entity.TripId.Value)
                .Select(x => new { x.TripNumber, x.Title })
                .FirstOrDefaultAsync(cancellationToken);
            if (trip is not null)
            {
                tripNumber = trip.TripNumber;
                tripTitle = trip.Title;
            }
        }

        return new DispatchReportExceptionDetailResponse(
            entity.Id,
            entity.ExceptionKey,
            entity.Title,
            entity.Description,
            entity.Category,
            entity.Status,
            entity.TripId,
            tripNumber,
            tripTitle,
            entity.AssignedToUserId,
            entity.ResolutionNotes,
            entity.CreatedByUserId,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.AssignedAt,
            entity.ResolvedByUserId,
            entity.ResolvedAt);
    }

    public async Task<(string ContentType, string FileName, byte[] Content)> ExportSummaryCsvAsync(
        Guid tenantId,
        string? scope,
        CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(tenantId, scope, cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine(
            "tripNumber,title,dispatchStatus,assignedDriverPersonId,vehicleRefKey,scheduledStartAt,scheduledEndAt,isLate,isAtRisk,isUnassigned,routeCount,missingRequiredProofCount,openExceptionCount");

        foreach (var trip in summary.Trips)
        {
            builder.Append(CsvEscape(trip.TripNumber));
            builder.Append(',');
            builder.Append(CsvEscape(trip.Title));
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
            builder.Append(trip.IsLate ? "true" : "false");
            builder.Append(',');
            builder.Append(trip.IsAtRisk ? "true" : "false");
            builder.Append(',');
            builder.Append(trip.IsUnassigned ? "true" : "false");
            builder.Append(',');
            builder.Append(trip.RouteCount);
            builder.Append(',');
            builder.Append(trip.MissingRequiredProofCount);
            builder.Append(',');
            builder.AppendLine(trip.OpenExceptionCount.ToString());
        }

        var fileName = $"routarr-dispatch-report-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.csv";
        return ("text/csv", fileName, Encoding.UTF8.GetBytes(builder.ToString()));
    }

    public async Task<DispatchTimeSummaryResponse> GetTimeSummaryAsync(
        Guid tenantId,
        string? scope,
        CancellationToken cancellationToken = default)
    {
        var normalizedScope = NormalizeScope(scope);
        var now = DateTimeOffset.UtcNow;
        var (windowStart, windowEnd) = GetWindow(normalizedScope, now);

        var trips = await db.Trips
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        var rows = trips
            .Where(x => IsTimeSummaryTripInScope(x, windowStart, windowEnd))
            .OrderBy(x => x.StartedAt ?? x.ScheduledStartAt ?? x.CreatedAt)
            .ThenBy(x => x.TripNumber)
            .Select(MapTimeSummaryTrip)
            .ToList();

        return new DispatchTimeSummaryResponse(
            now,
            normalizedScope,
            windowStart,
            windowEnd,
            rows.Count,
            rows.Count(x => x.IsCompleted),
            rows.Count(x => x.IsClosed),
            rows.Sum(x => x.ScheduledDurationMinutes ?? 0),
            rows.Sum(x => x.ActualDurationMinutes ?? 0),
            rows.Sum(x => x.VarianceMinutes ?? 0),
            rows);
    }

    public async Task<(string ContentType, string FileName, byte[] Content)> ExportTimeSummaryCsvAsync(
        Guid tenantId,
        string? scope,
        CancellationToken cancellationToken = default)
    {
        var summary = await GetTimeSummaryAsync(tenantId, scope, cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine(
            "tripNumber,title,dispatchStatus,assignedDriverPersonId,vehicleRefKey,scheduledStartAt,scheduledEndAt,startedAt,completedAt,closedAt,scheduledDurationMinutes,actualDurationMinutes,varianceMinutes,isCompleted,isClosed");

        foreach (var trip in summary.Trips)
        {
            builder.Append(CsvEscape(trip.TripNumber));
            builder.Append(',');
            builder.Append(CsvEscape(trip.Title));
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
            builder.Append(trip.StartedAt?.ToString("O") ?? string.Empty);
            builder.Append(',');
            builder.Append(trip.CompletedAt?.ToString("O") ?? string.Empty);
            builder.Append(',');
            builder.Append(trip.ClosedAt?.ToString("O") ?? string.Empty);
            builder.Append(',');
            builder.Append(trip.ScheduledDurationMinutes?.ToString() ?? string.Empty);
            builder.Append(',');
            builder.Append(trip.ActualDurationMinutes?.ToString() ?? string.Empty);
            builder.Append(',');
            builder.Append(trip.VarianceMinutes?.ToString() ?? string.Empty);
            builder.Append(',');
            builder.Append(trip.IsCompleted ? "true" : "false");
            builder.Append(',');
            builder.AppendLine(trip.IsClosed ? "true" : "false");
        }

        var fileName = $"routarr-time-summary-report-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.csv";
        return ("text/csv", fileName, Encoding.UTF8.GetBytes(builder.ToString()));
    }

    public async Task<IReadOnlyList<DispatchReportAlertResponse>> GetAlertsAsync(
        Guid tenantId,
        string? scope,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(tenantId, scope, cancellationToken);
        var safeLimit = Math.Clamp(limit ?? DefaultAlertLimit, 1, MaxAlertLimit);
        var alerts = new List<DispatchReportAlertResponse>();

        foreach (var trip in summary.Trips.Where(x => x.MissingRequiredProofCount > 0))
        {
            alerts.Add(new DispatchReportAlertResponse(
                "proof_missing",
                "high",
                trip.TripId,
                trip.TripNumber,
                null,
                $"Trip {trip.TripNumber} is missing {trip.MissingRequiredProofCount} required proof item(s).",
                summary.GeneratedAt));
        }

        foreach (var exception in summary.RecentExceptions)
        {
            var severity = string.Equals(exception.Status, DispatchExceptionStatuses.Assigned, StringComparison.OrdinalIgnoreCase)
                ? "critical"
                : "medium";
            var alertType = string.Equals(exception.Status, DispatchExceptionStatuses.Assigned, StringComparison.OrdinalIgnoreCase)
                ? "exception_escalated"
                : "exception_created";
            alerts.Add(new DispatchReportAlertResponse(
                alertType,
                severity,
                exception.TripId,
                summary.Trips.FirstOrDefault(x => x.TripId == exception.TripId)?.TripNumber,
                exception.ExceptionId,
                $"Dispatch exception {exception.ExceptionKey} is {exception.Status}.",
                exception.UpdatedAt));
        }

        return alerts
            .OrderByDescending(x => x.DetectedAt)
            .Take(safeLimit)
            .ToList();
    }

    private static DispatchReportExceptionRow MapExceptionRow(DispatchException entity) =>
        new(
            entity.Id,
            entity.ExceptionKey,
            entity.Title,
            entity.Category,
            entity.Status,
            entity.TripId,
            entity.CreatedAt,
            entity.UpdatedAt);

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

    private static bool IsTripInScope(Trip trip, DateTimeOffset windowStart, DateTimeOffset windowEnd)
    {
        if (trip.DispatchStatus is TripDispatchStatuses.Dispatched or TripDispatchStatuses.InProgress)
        {
            return true;
        }

        return OverlapsWindow(trip.ScheduledStartAt, trip.ScheduledEndAt, windowStart, windowEnd)
            || (trip.UpdatedAt >= windowStart && trip.UpdatedAt < windowEnd);
    }

    private static bool IsExceptionInScope(DispatchException entity, DateTimeOffset windowStart, DateTimeOffset windowEnd) =>
        (entity.UpdatedAt >= windowStart && entity.UpdatedAt < windowEnd)
        || (entity.CreatedAt >= windowStart && entity.CreatedAt < windowEnd);

    private static bool IsTimeSummaryTripInScope(Trip trip, DateTimeOffset windowStart, DateTimeOffset windowEnd) =>
        trip.StartedAt.HasValue
        && OverlapsWindow(trip.StartedAt, trip.CompletedAt ?? trip.ClosedAt, windowStart, windowEnd)
        || (trip.CompletedAt.HasValue && trip.CompletedAt.Value >= windowStart && trip.CompletedAt.Value < windowEnd)
        || (trip.ClosedAt.HasValue && trip.ClosedAt.Value >= windowStart && trip.ClosedAt.Value < windowEnd);

    private static DispatchTimeSummaryTripRow MapTimeSummaryTrip(Trip trip)
    {
        var scheduledDuration = ComputeMinutes(trip.ScheduledStartAt, trip.ScheduledEndAt);
        var actualDuration = ComputeMinutes(trip.StartedAt, trip.CompletedAt ?? trip.ClosedAt);
        int? variance = scheduledDuration.HasValue && actualDuration.HasValue
            ? actualDuration.Value - scheduledDuration.Value
            : null;

        return new DispatchTimeSummaryTripRow(
            trip.Id,
            trip.TripNumber,
            trip.Title,
            trip.DispatchStatus,
            trip.AssignedDriverPersonId,
            trip.VehicleRefKey,
            trip.ScheduledStartAt,
            trip.ScheduledEndAt,
            trip.StartedAt,
            trip.CompletedAt,
            trip.ClosedAt,
            scheduledDuration,
            actualDuration,
            variance,
            string.Equals(trip.DispatchStatus, TripDispatchStatuses.Completed, StringComparison.OrdinalIgnoreCase),
            trip.ClosedAt.HasValue);
    }

    private static int? ComputeMinutes(DateTimeOffset? startAt, DateTimeOffset? endAt)
    {
        if (!startAt.HasValue || !endAt.HasValue || endAt.Value < startAt.Value)
        {
            return null;
        }

        return (int)Math.Round((endAt.Value - startAt.Value).TotalMinutes, MidpointRounding.AwayFromZero);
    }

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

    private static IReadOnlyList<DispatchReportCountItem> CountBy(IEnumerable<string> keys) =>
        keys
            .GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
            .Select(g => new DispatchReportCountItem(g.Key, g.Count()))
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
