using System.Globalization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed class DriverTimeTrackingService(
    RoutArrDbContext db,
    RoutArrAuthorizationService authorization,
    IRoutArrAuditService audit)
{
    public const string ReadAction = "driver_time_tracking.read";
    public const string CreateAction = "driver_time_tracking.create";
    public const string UpdateAction = "driver_time_tracking.update";

    public async Task<DriverTimeTrackingResponse> GetAsync(
        ClaimsPrincipal principal,
        string? date,
        CancellationToken cancellationToken = default)
    {
        authorization.RequireDriverPortalRead(principal);
        var tenantId = principal.GetTenantId();
        var actorUserId = principal.GetUserId();
        var personId = principal.GetPersonId().ToString();
        var targetDate = ResolveDate(date);
        var windowStart = new DateTimeOffset(targetDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), TimeSpan.Zero);
        var windowEnd = windowStart.AddDays(1);

        var entries = await db.DriverTimeEntries
            .AsNoTracking()
            .Where(x =>
                x.TenantId == tenantId
                && x.PersonId == personId
                && x.StartsAt < windowEnd
                && (x.EndsAt ?? DateTimeOffset.UtcNow) > windowStart)
            .OrderBy(x => x.StartsAt)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var rows = entries.Select(entry => MapEntry(entry, windowStart, windowEnd)).ToList();
        var summary = BuildSummary(rows, windowStart, windowEnd);

        await audit.WriteAsync(
            ReadAction,
            tenantId,
            actorUserId,
            "driver_time_tracking",
            personId,
            "success",
            reasonCode: targetDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            cancellationToken: cancellationToken);

        return new DriverTimeTrackingResponse(
            targetDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            windowStart,
            windowEnd,
            summary,
            rows,
            DateTimeOffset.UtcNow);
    }

    public async Task<DriverTimeEntryResponse> CreateAsync(
        ClaimsPrincipal principal,
        CreateDriverTimeEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        authorization.RequireDriverPortalExecute(principal);
        var tenantId = principal.GetTenantId();
        var actorUserId = principal.GetUserId();
        var personId = principal.GetPersonId().ToString();

        ValidateEntryType(request.EntryType);
        ValidateInterval(request.StartsAt, request.EndsAt);

        var entry = new DriverTimeEntry
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PersonId = personId,
            EntryType = request.EntryType.Trim().ToLowerInvariant(),
            StartsAt = request.StartsAt,
            EndsAt = request.EndsAt,
            Notes = request.Notes?.Trim() ?? string.Empty,
            EditReason = "Created via driver portal time tracking",
            CreatedByUserId = actorUserId,
            UpdatedByUserId = null,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        db.DriverTimeEntries.Add(entry);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            CreateAction,
            tenantId,
            actorUserId,
            "driver_time_entry",
            entry.Id.ToString(),
            "success",
            reasonCode: entry.EntryType,
            cancellationToken: cancellationToken);

        return MapEntry(entry, windowStart: null, windowEnd: null);
    }

    public async Task<DriverTimeEntryResponse> UpdateAsync(
        ClaimsPrincipal principal,
        Guid entryId,
        UpdateDriverTimeEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        authorization.RequireDriverPortalExecute(principal);
        var tenantId = principal.GetTenantId();
        var actorUserId = principal.GetUserId();
        var actorPersonId = principal.GetPersonId().ToString();

        if (string.IsNullOrWhiteSpace(request.EditReason))
        {
            throw new StlApiException(
                "validation.required",
                "Edit reason is required when correcting a driver time entry.",
                400);
        }

        var entry = await db.DriverTimeEntries
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == entryId, cancellationToken)
            ?? throw new StlApiException("not_found", "Driver time entry not found.", 404);

        if (!string.Equals(entry.PersonId.Trim(), actorPersonId, StringComparison.Ordinal))
        {
            throw new StlApiException(
                "auth.forbidden",
                "Driver time entries can only be edited by the owning driver.",
                403);
        }

        if (!string.IsNullOrWhiteSpace(request.EntryType))
        {
            ValidateEntryType(request.EntryType);
            entry.EntryType = request.EntryType.Trim().ToLowerInvariant();
        }

        if (request.StartsAt.HasValue)
        {
            entry.StartsAt = request.StartsAt.Value;
        }

        if (request.EndsAt.HasValue || request.EndsAt is null)
        {
            entry.EndsAt = request.EndsAt;
        }

        ValidateInterval(entry.StartsAt, entry.EndsAt);

        if (request.Notes is not null)
        {
            entry.Notes = request.Notes.Trim();
        }

        entry.EditReason = request.EditReason.Trim();
        entry.UpdatedByUserId = actorUserId;
        entry.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            UpdateAction,
            tenantId,
            actorUserId,
            "driver_time_entry",
            entry.Id.ToString(),
            "success",
            reasonCode: entry.EntryType,
            cancellationToken: cancellationToken);

        return MapEntry(entry, windowStart: null, windowEnd: null);
    }

    private static void ValidateEntryType(string entryType)
    {
        if (string.IsNullOrWhiteSpace(entryType) || !DriverTimeEntryTypes.All.Contains(entryType.Trim()))
        {
            throw new StlApiException(
                "validation.invalid",
                "Entry type must be on_duty, off_duty, or break.",
                400);
        }
    }

    private static void ValidateInterval(DateTimeOffset startsAt, DateTimeOffset? endsAt)
    {
        if (endsAt.HasValue && endsAt.Value < startsAt)
        {
            throw new StlApiException(
                "validation.invalid",
                "End time must be after start time.",
                400);
        }
    }

    private static DateOnly ResolveDate(string? date)
    {
        if (!string.IsNullOrWhiteSpace(date)
            && DateOnly.TryParseExact(date.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            return parsed;
        }

        return DateOnly.FromDateTime(DateTime.UtcNow);
    }

    private static DriverTimeEntryResponse MapEntry(
        DriverTimeEntry entry,
        DateTimeOffset? windowStart,
        DateTimeOffset? windowEnd)
    {
        var durationMinutes = CalculateDurationMinutes(entry, windowStart, windowEnd);
        return new DriverTimeEntryResponse(
            entry.Id,
            entry.PersonId,
            entry.EntryType,
            entry.StartsAt,
            entry.EndsAt,
            entry.Notes,
            entry.EditReason,
            entry.EndsAt is null,
            durationMinutes,
            entry.CreatedByUserId,
            entry.UpdatedByUserId,
            entry.CreatedAt,
            entry.UpdatedAt);
    }

    private static int CalculateDurationMinutes(
        DriverTimeEntry entry,
        DateTimeOffset? windowStart,
        DateTimeOffset? windowEnd)
    {
        var start = entry.StartsAt;
        var end = entry.EndsAt ?? DateTimeOffset.UtcNow;
        if (windowStart.HasValue && start < windowStart.Value)
        {
            start = windowStart.Value;
        }

        if (windowEnd.HasValue && end > windowEnd.Value)
        {
            end = windowEnd.Value;
        }

        if (end <= start)
        {
            return 0;
        }

        return (int)Math.Round((end - start).TotalMinutes, MidpointRounding.AwayFromZero);
    }

    private static DriverTimeTrackingSummaryResponse BuildSummary(
        IReadOnlyList<DriverTimeEntryResponse> rows,
        DateTimeOffset windowStart,
        DateTimeOffset windowEnd)
    {
        var onDutyMinutes = rows
            .Where(x => string.Equals(x.EntryType, DriverTimeEntryTypes.OnDuty, StringComparison.OrdinalIgnoreCase))
            .Sum(x => x.DurationMinutes);
        var offDutyMinutes = rows
            .Where(x => string.Equals(x.EntryType, DriverTimeEntryTypes.OffDuty, StringComparison.OrdinalIgnoreCase))
            .Sum(x => x.DurationMinutes);
        var breakMinutes = rows
            .Where(x => string.Equals(x.EntryType, DriverTimeEntryTypes.Break, StringComparison.OrdinalIgnoreCase))
            .Sum(x => x.DurationMinutes);

        var workdayStartAt = rows.Count == 0 ? (DateTimeOffset?)null : rows.Min(x => x.StartsAt);
        var workdayEndAt = rows.Count == 0
            ? (DateTimeOffset?)null
            : rows.Max(x => x.EndsAt ?? x.StartsAt);

        var openEntryCount = rows.Count(x => x.IsOpen);
        var shortHaulCandidate = onDutyMinutes <= 660 && openEntryCount == 0;
        var shortHaulException = !shortHaulCandidate;
        var summaryNote = shortHaulCandidate
            ? "Operational time logs are within the short-haul-style threshold."
            : "Time logs exceed the short-haul-style threshold or still have open entries.";

        return new DriverTimeTrackingSummaryResponse(
            rows.Count,
            onDutyMinutes,
            offDutyMinutes,
            breakMinutes,
            openEntryCount,
            workdayStartAt,
            workdayEndAt,
            shortHaulCandidate,
            shortHaulException,
            summaryNote);
    }
}
