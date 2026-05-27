using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed class EquipmentAvailabilityService(
    RoutArrDbContext db,
    IRoutArrAuditService audit)
{
    public const string ReadAction = "equipment_availability.read";

    public const string PanelReadAction = "equipment_availability_panel.read";

    public const string CreateAction = "equipment_availability.create";

    public const string UpdateAction = "equipment_availability.update";

    public const string DeleteAction = "equipment_availability.delete";

    public async Task<IReadOnlyList<EquipmentAvailabilitySummaryResponse>> ListAsync(
        Guid tenantId,
        string? vehicleRefKey,
        string? scope,
        string? start,
        string? end,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var (_, windowStart, windowEnd) = ResolveWindow(scope, start, end, now);

        var records = await LoadScopedRecordsAsync(
            tenantId,
            vehicleRefKey,
            windowStart,
            windowEnd,
            cancellationToken);

        var tripsByVehicle = await LoadTripsByVehicleAsync(tenantId, records, cancellationToken);

        return records
            .Select(record => MapSummary(record, tripsByVehicle))
            .OrderBy(x => x.StartsAt)
            .ThenBy(x => x.VehicleRefKey)
            .ToList();
    }

    public async Task<EquipmentAvailabilityDetailResponse> GetAsync(
        Guid tenantId,
        Guid availabilityId,
        CancellationToken cancellationToken = default)
    {
        var record = await GetRecordAsync(tenantId, availabilityId, cancellationToken);
        var trips = await LoadTripsForVehicleAsync(tenantId, record.VehicleRefKey, cancellationToken);
        return MapDetail(record, trips);
    }

    public async Task<EquipmentAvailabilityPanelResponse> GetPanelAsync(
        Guid tenantId,
        string? scope,
        string? start,
        string? end,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var (normalizedScope, windowStart, windowEnd) = ResolveWindow(scope, start, end, now);

        var records = await LoadScopedRecordsAsync(
            tenantId,
            vehicleRefKey: null,
            windowStart,
            windowEnd,
            cancellationToken);

        var tripsByVehicle = await LoadTripsByVehicleAsync(tenantId, records, cancellationToken);

        var rows = records
            .Select(record =>
            {
                var conflicts = EquipmentAvailabilityRules
                    .FindConflictingTrips(record, tripsByVehicle.GetValueOrDefault(record.VehicleRefKey) ?? [])
                    .Select(MapConflict)
                    .ToList();
                return new EquipmentAvailabilityPanelRow(
                    record.Id,
                    record.VehicleRefKey,
                    record.AvailabilityStatus,
                    record.StartsAt,
                    record.EndsAt,
                    record.Reason,
                    conflicts.Count > 0,
                    conflicts.Count,
                    conflicts);
            })
            .OrderBy(x => x.StartsAt)
            .ThenBy(x => x.VehicleRefKey)
            .ToList();

        var summary = new EquipmentAvailabilityPanelSummary(
            rows.Count,
            rows.Count(x => StatusEquals(x.AvailabilityStatus, EquipmentAvailabilityStatuses.Unavailable)),
            rows.Count(x => StatusEquals(x.AvailabilityStatus, EquipmentAvailabilityStatuses.Limited)),
            rows.Count(x => StatusEquals(x.AvailabilityStatus, EquipmentAvailabilityStatuses.Available)),
            rows.Count(x => x.HasConflict));

        await audit.WriteAsync(
            PanelReadAction,
            tenantId,
            null,
            "equipment_availability_panel",
            normalizedScope,
            "success",
            cancellationToken: cancellationToken);

        return new EquipmentAvailabilityPanelResponse(
            normalizedScope,
            windowStart,
            windowEnd,
            summary,
            rows,
            now);
    }

    public async Task<EquipmentAvailabilityDetailResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateEquipmentAvailabilityRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request.VehicleRefKey, request.AvailabilityStatus, request.StartsAt, request.EndsAt);

        var now = DateTimeOffset.UtcNow;
        var entity = new EquipmentAvailability
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            VehicleRefKey = request.VehicleRefKey.Trim(),
            AvailabilityStatus = NormalizeStatus(request.AvailabilityStatus),
            StartsAt = request.StartsAt,
            EndsAt = request.EndsAt,
            Reason = request.Reason?.Trim() ?? string.Empty,
            Notes = request.Notes?.Trim() ?? string.Empty,
            CreatedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.EquipmentAvailabilities.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            CreateAction,
            tenantId,
            actorUserId,
            "equipment_availability",
            entity.Id.ToString(),
            entity.AvailabilityStatus,
            cancellationToken: cancellationToken);

        var trips = await LoadTripsForVehicleAsync(tenantId, entity.VehicleRefKey, cancellationToken);
        return MapDetail(entity, trips);
    }

    public async Task<EquipmentAvailabilityDetailResponse> UpdateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid availabilityId,
        UpdateEquipmentAvailabilityRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetTrackedRecordAsync(tenantId, availabilityId, cancellationToken);

        var status = request.AvailabilityStatus is not null
            ? NormalizeStatus(request.AvailabilityStatus)
            : entity.AvailabilityStatus;
        var startsAt = request.StartsAt ?? entity.StartsAt;
        var endsAt = request.EndsAt ?? entity.EndsAt;
        ValidateWindow(status, startsAt, endsAt);

        entity.AvailabilityStatus = status;
        entity.StartsAt = startsAt;
        entity.EndsAt = endsAt;
        if (request.Reason is not null)
        {
            entity.Reason = request.Reason.Trim();
        }

        if (request.Notes is not null)
        {
            entity.Notes = request.Notes.Trim();
        }

        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            UpdateAction,
            tenantId,
            actorUserId,
            "equipment_availability",
            entity.Id.ToString(),
            entity.AvailabilityStatus,
            cancellationToken: cancellationToken);

        var trips = await LoadTripsForVehicleAsync(tenantId, entity.VehicleRefKey, cancellationToken);
        return MapDetail(entity, trips);
    }

    public async Task DeleteAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid availabilityId,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetTrackedRecordAsync(tenantId, availabilityId, cancellationToken);
        db.EquipmentAvailabilities.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            DeleteAction,
            tenantId,
            actorUserId,
            "equipment_availability",
            entity.Id.ToString(),
            "deleted",
            cancellationToken: cancellationToken);
    }

    private async Task<List<EquipmentAvailability>> LoadScopedRecordsAsync(
        Guid tenantId,
        string? vehicleRefKey,
        DateTimeOffset windowStart,
        DateTimeOffset windowEnd,
        CancellationToken cancellationToken)
    {
        var query = db.EquipmentAvailabilities
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(vehicleRefKey))
        {
            var normalizedKey = vehicleRefKey.Trim();
            query = query.Where(x => x.VehicleRefKey == normalizedKey);
        }

        var records = await query.ToListAsync(cancellationToken);
        return records
            .Where(x => EquipmentAvailabilityRules.RecordOverlapsWindow(x, windowStart, windowEnd))
            .ToList();
    }

    private async Task<Dictionary<string, List<Trip>>> LoadTripsByVehicleAsync(
        Guid tenantId,
        IReadOnlyList<EquipmentAvailability> records,
        CancellationToken cancellationToken)
    {
        var vehicleRefKeys = records
            .Select(x => x.VehicleRefKey)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (vehicleRefKeys.Count == 0)
        {
            return new Dictionary<string, List<Trip>>(StringComparer.Ordinal);
        }

        var trips = await db.Trips
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && x.VehicleRefKey != null
                && vehicleRefKeys.Contains(x.VehicleRefKey))
            .ToListAsync(cancellationToken);

        return trips
            .GroupBy(x => x.VehicleRefKey!, StringComparer.Ordinal)
            .ToDictionary(x => x.Key, x => x.ToList(), StringComparer.Ordinal);
    }

    private async Task<List<Trip>> LoadTripsForVehicleAsync(
        Guid tenantId,
        string vehicleRefKey,
        CancellationToken cancellationToken) =>
        await db.Trips
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.VehicleRefKey == vehicleRefKey)
            .ToListAsync(cancellationToken);

    private async Task<EquipmentAvailability> GetRecordAsync(
        Guid tenantId,
        Guid availabilityId,
        CancellationToken cancellationToken)
    {
        var record = await db.EquipmentAvailabilities
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == availabilityId, cancellationToken);

        if (record is null)
        {
            throw new StlApiException(
                "equipment_availability.not_found",
                "Equipment availability record was not found.",
                404);
        }

        return record;
    }

    private async Task<EquipmentAvailability> GetTrackedRecordAsync(
        Guid tenantId,
        Guid availabilityId,
        CancellationToken cancellationToken)
    {
        var record = await db.EquipmentAvailabilities
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == availabilityId, cancellationToken);

        if (record is null)
        {
            throw new StlApiException(
                "equipment_availability.not_found",
                "Equipment availability record was not found.",
                404);
        }

        return record;
    }

    private static EquipmentAvailabilitySummaryResponse MapSummary(
        EquipmentAvailability record,
        IReadOnlyDictionary<string, List<Trip>> tripsByVehicle)
    {
        var conflicts = EquipmentAvailabilityRules
            .FindConflictingTrips(record, tripsByVehicle.GetValueOrDefault(record.VehicleRefKey) ?? [])
            .ToList();
        return new EquipmentAvailabilitySummaryResponse(
            record.Id,
            record.VehicleRefKey,
            record.AvailabilityStatus,
            record.StartsAt,
            record.EndsAt,
            record.Reason,
            record.Notes,
            conflicts.Count > 0,
            conflicts.Count,
            record.CreatedByUserId,
            record.CreatedAt,
            record.UpdatedAt);
    }

    private static EquipmentAvailabilityDetailResponse MapDetail(
        EquipmentAvailability record,
        IEnumerable<Trip> tripsForVehicle)
    {
        var conflicts = EquipmentAvailabilityRules
            .FindConflictingTrips(record, tripsForVehicle)
            .Select(MapConflict)
            .ToList();
        return new EquipmentAvailabilityDetailResponse(
            record.Id,
            record.VehicleRefKey,
            record.AvailabilityStatus,
            record.StartsAt,
            record.EndsAt,
            record.Reason,
            record.Notes,
            conflicts.Count > 0,
            conflicts,
            record.CreatedByUserId,
            record.CreatedAt,
            record.UpdatedAt);
    }

    private static EquipmentAvailabilityTripConflictResponse MapConflict(Trip trip) =>
        new(
            trip.Id,
            trip.TripNumber,
            trip.Title,
            trip.DispatchStatus,
            trip.ScheduledStartAt,
            trip.ScheduledEndAt);

    private static void ValidateRequest(
        string vehicleRefKey,
        string availabilityStatus,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt)
    {
        if (string.IsNullOrWhiteSpace(vehicleRefKey))
        {
            throw new StlApiException(
                "equipment_availability.invalid_vehicle",
                "Vehicle ref key is required.",
                400);
        }

        ValidateWindow(NormalizeStatus(availabilityStatus), startsAt, endsAt);
    }

    private static void ValidateWindow(string status, DateTimeOffset startsAt, DateTimeOffset endsAt)
    {
        _ = NormalizeStatus(status);
        if (endsAt <= startsAt)
        {
            throw new StlApiException(
                "equipment_availability.invalid_window",
                "Availability end must be after start.",
                400);
        }
    }

    private static string NormalizeStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            throw new StlApiException(
                "equipment_availability.invalid_status",
                "Availability status is required.",
                400);
        }

        var normalized = status.Trim().ToLowerInvariant();
        if (!EquipmentAvailabilityStatuses.All.Contains(normalized))
        {
            throw new StlApiException(
                "equipment_availability.invalid_status",
                "Availability status must be available, unavailable, or limited.",
                400);
        }

        return normalized;
    }

    private static (string Scope, DateTimeOffset Start, DateTimeOffset End) ResolveWindow(
        string? scope,
        string? start,
        string? end,
        DateTimeOffset now)
    {
        if (!string.IsNullOrWhiteSpace(start) || !string.IsNullOrWhiteSpace(end))
        {
            if (string.IsNullOrWhiteSpace(start) || string.IsNullOrWhiteSpace(end))
            {
                throw new StlApiException(
                    "equipment_availability.invalid_range",
                    "Equipment availability start and end must both be provided for a custom range.",
                    400);
            }

            var windowStart = RouteCalendarRules.ParseUtcDate(start, "equipment_availability.invalid_date");
            var windowEnd = RouteCalendarRules.ParseUtcDate(end, "equipment_availability.invalid_date");
            if (windowEnd <= windowStart)
            {
                throw new StlApiException(
                    "equipment_availability.invalid_range",
                    "Equipment availability end must be after start.",
                    400);
            }

            var dayCount = (windowEnd - windowStart).TotalDays;
            if (dayCount > RouteCalendarRules.MaxCustomRangeDays)
            {
                throw new StlApiException(
                    "equipment_availability.range_too_large",
                    $"Equipment availability range cannot exceed {RouteCalendarRules.MaxCustomRangeDays} days.",
                    400);
            }

            return (RouteCalendarService.ScopeCustom, windowStart, windowEnd);
        }

        var normalizedScope = NormalizeScope(scope);
        var dayStart = RouteCalendarRules.ToUtcDayStart(now);
        var window = normalizedScope == RouteCalendarService.ScopeWeekly
            ? (dayStart, dayStart.AddDays(7))
            : (dayStart, dayStart.AddDays(1));
        return (normalizedScope, window.Item1, window.Item2);
    }

    private static string NormalizeScope(string? scope)
    {
        if (string.IsNullOrWhiteSpace(scope))
        {
            return RouteCalendarService.ScopeDaily;
        }

        var normalized = scope.Trim().ToLowerInvariant();
        if (normalized is RouteCalendarService.ScopeDaily or RouteCalendarService.ScopeWeekly)
        {
            return normalized;
        }

        throw new StlApiException(
            "equipment_availability.invalid_scope",
            "Equipment availability scope must be daily or weekly.",
            400);
    }

    private static bool StatusEquals(string actual, string expected) =>
        string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);
}
