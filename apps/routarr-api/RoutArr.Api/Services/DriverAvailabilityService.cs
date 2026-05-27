using RoutArr.Api.Contracts;

using RoutArr.Api.Data;

using RoutArr.Api.Entities;

using Microsoft.EntityFrameworkCore;

using STLCompliance.Shared.Contracts;



namespace RoutArr.Api.Services;



public sealed class DriverAvailabilityService(

    RoutArrDbContext db,

    IRoutArrAuditService audit)

{

    public const string ReadAction = "driver_availability.read";



    public const string PanelReadAction = "driver_availability_panel.read";



    public const string CreateAction = "driver_availability.create";



    public const string UpdateAction = "driver_availability.update";



    public const string DeleteAction = "driver_availability.delete";



    public async Task<IReadOnlyList<DriverAvailabilitySummaryResponse>> ListAsync(

        Guid tenantId,

        bool viewAll,

        string? actorPersonId,

        string? personId,

        string? scope,

        string? start,

        string? end,

        CancellationToken cancellationToken = default)

    {

        var now = DateTimeOffset.UtcNow;

        var (_, windowStart, windowEnd) = ResolveWindow(scope, start, end, now);



        var records = await LoadScopedRecordsAsync(

            tenantId,

            viewAll,

            actorPersonId,

            personId,

            windowStart,

            windowEnd,

            cancellationToken);



        var tripsByPerson = await LoadTripsByPersonAsync(tenantId, records, cancellationToken);



        return records

            .Select(record => MapSummary(record, tripsByPerson))

            .OrderBy(x => x.StartsAt)

            .ThenBy(x => x.PersonId)

            .ToList();

    }



    public async Task<DriverAvailabilityDetailResponse> GetAsync(

        Guid tenantId,

        Guid availabilityId,

        bool viewAll,

        string? actorPersonId,

        CancellationToken cancellationToken = default)

    {

        var record = await GetRecordAsync(tenantId, availabilityId, cancellationToken);

        EnsurePersonAccess(viewAll, actorPersonId, record.PersonId);



        var trips = await LoadTripsForPersonAsync(tenantId, record.PersonId, cancellationToken);

        return MapDetail(record, trips);

    }



    public async Task<DriverAvailabilityPanelResponse> GetPanelAsync(

        Guid tenantId,

        bool viewAll,

        string? actorPersonId,

        string? scope,

        string? start,

        string? end,

        CancellationToken cancellationToken = default)

    {

        var now = DateTimeOffset.UtcNow;

        var (normalizedScope, windowStart, windowEnd) = ResolveWindow(scope, start, end, now);



        var records = await LoadScopedRecordsAsync(

            tenantId,

            viewAll,

            actorPersonId,

            personId: null,

            windowStart,

            windowEnd,

            cancellationToken);



        var tripsByPerson = await LoadTripsByPersonAsync(tenantId, records, cancellationToken);



        var rows = records

            .Select(record =>

            {

                var conflicts = DriverAvailabilityRules

                    .FindConflictingTrips(record, tripsByPerson.GetValueOrDefault(record.PersonId) ?? [])

                    .Select(MapConflict)

                    .ToList();

                return new DriverAvailabilityPanelRow(

                    record.Id,

                    record.PersonId,

                    record.AvailabilityStatus,

                    record.StartsAt,

                    record.EndsAt,

                    record.Reason,

                    conflicts.Count > 0,

                    conflicts.Count,

                    conflicts);

            })

            .OrderBy(x => x.StartsAt)

            .ThenBy(x => x.PersonId)

            .ToList();



        var summary = new DriverAvailabilityPanelSummary(

            rows.Count,

            rows.Count(x => StatusEquals(x.AvailabilityStatus, DriverAvailabilityStatuses.Unavailable)),

            rows.Count(x => StatusEquals(x.AvailabilityStatus, DriverAvailabilityStatuses.Limited)),

            rows.Count(x => StatusEquals(x.AvailabilityStatus, DriverAvailabilityStatuses.Available)),

            rows.Count(x => x.HasConflict));



        await audit.WriteAsync(

            PanelReadAction,

            tenantId,

            null,

            "driver_availability_panel",

            normalizedScope,

            "success",

            cancellationToken: cancellationToken);



        return new DriverAvailabilityPanelResponse(

            normalizedScope,

            windowStart,

            windowEnd,

            summary,

            rows,

            now);

    }



    public async Task<DriverAvailabilityDetailResponse> CreateAsync(

        Guid tenantId,

        Guid actorUserId,

        CreateDriverAvailabilityRequest request,

        CancellationToken cancellationToken = default)

    {

        ValidateRequest(request.PersonId, request.AvailabilityStatus, request.StartsAt, request.EndsAt);



        var now = DateTimeOffset.UtcNow;

        var entity = new DriverAvailability

        {

            Id = Guid.NewGuid(),

            TenantId = tenantId,

            PersonId = request.PersonId.Trim(),

            AvailabilityStatus = NormalizeStatus(request.AvailabilityStatus),

            StartsAt = request.StartsAt,

            EndsAt = request.EndsAt,

            Reason = request.Reason?.Trim() ?? string.Empty,

            Notes = request.Notes?.Trim() ?? string.Empty,

            CreatedByUserId = actorUserId,

            CreatedAt = now,

            UpdatedAt = now,

        };



        db.DriverAvailabilities.Add(entity);

        await db.SaveChangesAsync(cancellationToken);



        await audit.WriteAsync(

            CreateAction,

            tenantId,

            actorUserId,

            "driver_availability",

            entity.Id.ToString(),

            entity.AvailabilityStatus,

            cancellationToken: cancellationToken);



        var trips = await LoadTripsForPersonAsync(tenantId, entity.PersonId, cancellationToken);

        return MapDetail(entity, trips);

    }



    public async Task<DriverAvailabilityDetailResponse> UpdateAsync(

        Guid tenantId,

        Guid actorUserId,

        Guid availabilityId,

        UpdateDriverAvailabilityRequest request,

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

            "driver_availability",

            entity.Id.ToString(),

            entity.AvailabilityStatus,

            cancellationToken: cancellationToken);



        var trips = await LoadTripsForPersonAsync(tenantId, entity.PersonId, cancellationToken);

        return MapDetail(entity, trips);

    }



    public async Task DeleteAsync(

        Guid tenantId,

        Guid actorUserId,

        Guid availabilityId,

        CancellationToken cancellationToken = default)

    {

        var entity = await GetTrackedRecordAsync(tenantId, availabilityId, cancellationToken);

        db.DriverAvailabilities.Remove(entity);

        await db.SaveChangesAsync(cancellationToken);



        await audit.WriteAsync(

            DeleteAction,

            tenantId,

            actorUserId,

            "driver_availability",

            entity.Id.ToString(),

            "deleted",

            cancellationToken: cancellationToken);

    }



    private async Task<List<DriverAvailability>> LoadScopedRecordsAsync(

        Guid tenantId,

        bool viewAll,

        string? actorPersonId,

        string? personId,

        DateTimeOffset windowStart,

        DateTimeOffset windowEnd,

        CancellationToken cancellationToken)

    {

        var query = db.DriverAvailabilities

            .AsNoTracking()

            .Where(x => x.TenantId == tenantId);



        if (!string.IsNullOrWhiteSpace(personId))

        {

            var normalizedPersonId = personId.Trim();

            query = query.Where(x => x.PersonId == normalizedPersonId);

        }



        if (!viewAll)

        {

            var scopedPersonId = actorPersonId?.Trim();

            if (string.IsNullOrWhiteSpace(scopedPersonId))

            {

                return [];

            }



            query = query.Where(x => x.PersonId == scopedPersonId);

        }



        var records = await query.ToListAsync(cancellationToken);

        return records

            .Where(x => DriverAvailabilityRules.RecordOverlapsWindow(x, windowStart, windowEnd))

            .ToList();

    }



    private async Task<Dictionary<string, List<Trip>>> LoadTripsByPersonAsync(

        Guid tenantId,

        IReadOnlyList<DriverAvailability> records,

        CancellationToken cancellationToken)

    {

        var personIds = records

            .Select(x => x.PersonId)

            .Distinct(StringComparer.Ordinal)

            .ToList();



        if (personIds.Count == 0)

        {

            return new Dictionary<string, List<Trip>>(StringComparer.Ordinal);

        }



        var trips = await db.Trips

            .AsNoTracking()

            .Where(x => x.TenantId == tenantId

                && x.AssignedDriverPersonId != null

                && personIds.Contains(x.AssignedDriverPersonId))

            .ToListAsync(cancellationToken);



        return trips

            .GroupBy(x => x.AssignedDriverPersonId!, StringComparer.Ordinal)

            .ToDictionary(x => x.Key, x => x.ToList(), StringComparer.Ordinal);

    }



    private async Task<List<Trip>> LoadTripsForPersonAsync(

        Guid tenantId,

        string personId,

        CancellationToken cancellationToken) =>

        await db.Trips

            .AsNoTracking()

            .Where(x => x.TenantId == tenantId && x.AssignedDriverPersonId == personId)

            .ToListAsync(cancellationToken);



    private async Task<DriverAvailability> GetRecordAsync(

        Guid tenantId,

        Guid availabilityId,

        CancellationToken cancellationToken)

    {

        var record = await db.DriverAvailabilities

            .AsNoTracking()

            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == availabilityId, cancellationToken);



        if (record is null)

        {

            throw new StlApiException(

                "driver_availability.not_found",

                "Driver availability record was not found.",

                404);

        }



        return record;

    }



    private async Task<DriverAvailability> GetTrackedRecordAsync(

        Guid tenantId,

        Guid availabilityId,

        CancellationToken cancellationToken)

    {

        var record = await db.DriverAvailabilities

            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == availabilityId, cancellationToken);



        if (record is null)

        {

            throw new StlApiException(

                "driver_availability.not_found",

                "Driver availability record was not found.",

                404);

        }



        return record;

    }



    private static void EnsurePersonAccess(bool viewAll, string? actorPersonId, string recordPersonId)

    {

        if (viewAll)

        {

            return;

        }



        var personId = actorPersonId?.Trim();

        if (personId is not null

            && string.Equals(personId, recordPersonId, StringComparison.Ordinal))

        {

            return;

        }



        throw new StlApiException(

            "auth.forbidden",

            "You can only access driver availability for your own person record.",

            403);

    }



    private static DriverAvailabilitySummaryResponse MapSummary(

        DriverAvailability record,

        IReadOnlyDictionary<string, List<Trip>> tripsByPerson)

    {

        var conflicts = DriverAvailabilityRules

            .FindConflictingTrips(record, tripsByPerson.GetValueOrDefault(record.PersonId) ?? [])

            .ToList();

        return new DriverAvailabilitySummaryResponse(

            record.Id,

            record.PersonId,

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



    private static DriverAvailabilityDetailResponse MapDetail(

        DriverAvailability record,

        IEnumerable<Trip> tripsForPerson)

    {

        var conflicts = DriverAvailabilityRules

            .FindConflictingTrips(record, tripsForPerson)

            .Select(MapConflict)

            .ToList();

        return new DriverAvailabilityDetailResponse(

            record.Id,

            record.PersonId,

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



    private static DriverAvailabilityTripConflictResponse MapConflict(Trip trip) =>

        new(

            trip.Id,

            trip.TripNumber,

            trip.Title,

            trip.DispatchStatus,

            trip.ScheduledStartAt,

            trip.ScheduledEndAt);



    private static void ValidateRequest(

        string personId,

        string availabilityStatus,

        DateTimeOffset startsAt,

        DateTimeOffset endsAt)

    {

        if (string.IsNullOrWhiteSpace(personId))

        {

            throw new StlApiException(

                "driver_availability.invalid_person",

                "Person id is required.",

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

                "driver_availability.invalid_window",

                "Availability end must be after start.",

                400);

        }

    }



    private static string NormalizeStatus(string status)

    {

        if (string.IsNullOrWhiteSpace(status))

        {

            throw new StlApiException(

                "driver_availability.invalid_status",

                "Availability status is required.",

                400);

        }



        var normalized = status.Trim().ToLowerInvariant();

        if (!DriverAvailabilityStatuses.All.Contains(normalized))

        {

            throw new StlApiException(

                "driver_availability.invalid_status",

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

                    "driver_availability.invalid_range",

                    "Driver availability start and end must both be provided for a custom range.",

                    400);

            }



            var windowStart = RouteCalendarRules.ParseUtcDate(start, "driver_availability.invalid_date");

            var windowEnd = RouteCalendarRules.ParseUtcDate(end, "driver_availability.invalid_date");

            if (windowEnd <= windowStart)

            {

                throw new StlApiException(

                    "driver_availability.invalid_range",

                    "Driver availability end must be after start.",

                    400);

            }



            var dayCount = (windowEnd - windowStart).TotalDays;

            if (dayCount > RouteCalendarRules.MaxCustomRangeDays)

            {

                throw new StlApiException(

                    "driver_availability.range_too_large",

                    $"Driver availability range cannot exceed {RouteCalendarRules.MaxCustomRangeDays} days.",

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

            "driver_availability.invalid_scope",

            "Driver availability scope must be daily or weekly.",

            400);

    }



    private static bool StatusEquals(string actual, string expected) =>

        string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);

}

