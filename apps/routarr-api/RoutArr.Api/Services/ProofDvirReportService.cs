using System.Text;
using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed class ProofDvirReportService(RoutArrDbContext db)
{
    private const int RecentRecordLimit = 25;

    public async Task<ProofDvirReportSummaryResponse> GetSummaryAsync(
        Guid tenantId,
        string? scope,
        CancellationToken cancellationToken = default)
    {
        var normalizedScope = NormalizeScope(scope);
        var now = DateTimeOffset.UtcNow;
        var (windowStart, windowEnd) = GetWindow(normalizedScope, now);

        var proofs = await db.TripProofRecords
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        var dvirs = await db.TripDvirInspections
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        var scopedProofs = proofs
            .Where(x => x.CapturedAt >= windowStart && x.CapturedAt < windowEnd)
            .ToList();
        var scopedDvirs = dvirs
            .Where(x => x.SubmittedAt >= windowStart && x.SubmittedAt < windowEnd)
            .ToList();

        var tripIds = scopedProofs.Select(x => x.TripId)
            .Concat(scopedDvirs.Select(x => x.TripId))
            .Distinct()
            .ToHashSet();

        var trips = tripIds.Count == 0
            ? []
            : await db.Trips
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && tripIds.Contains(x.Id))
                .ToListAsync(cancellationToken);
        var tripById = trips.ToDictionary(x => x.Id);

        var proofsByTrip = scopedProofs.GroupBy(x => x.TripId).ToDictionary(x => x.Key, x => x.ToList());
        var dvirsByTrip = scopedDvirs.GroupBy(x => x.TripId).ToDictionary(x => x.Key, x => x.ToList());

        var tripItems = tripIds
            .Where(tripById.ContainsKey)
            .Select(tripId =>
            {
                var trip = tripById[tripId];
                proofsByTrip.TryGetValue(tripId, out var tripProofs);
                dvirsByTrip.TryGetValue(tripId, out var tripDvirs);
                tripProofs ??= [];
                tripDvirs ??= [];
                return new ProofDvirReportTripSummaryItem(
                    trip.Id,
                    trip.TripNumber,
                    trip.Title,
                    trip.DispatchStatus,
                    trip.AssignedDriverPersonId,
                    trip.VehicleRefKey,
                    tripProofs.Count,
                    tripDvirs.Any(x =>
                        string.Equals(x.Phase, DvirInspectionPhases.PreTrip, StringComparison.OrdinalIgnoreCase)),
                    tripDvirs.Any(x =>
                        string.Equals(x.Phase, DvirInspectionPhases.PostTrip, StringComparison.OrdinalIgnoreCase)),
                    tripDvirs.Count(x =>
                        string.Equals(x.Result, DvirInspectionResults.Fail, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(x.Result, DvirInspectionResults.Conditional, StringComparison.OrdinalIgnoreCase)));
            })
            .OrderBy(x => x.TripNumber)
            .ToList();

        var recentProofs = scopedProofs
            .OrderByDescending(x => x.CapturedAt)
            .Take(RecentRecordLimit)
            .Select(x => MapProofRow(x, tripById.GetValueOrDefault(x.TripId)?.TripNumber ?? string.Empty))
            .ToList();

        var recentDvirs = scopedDvirs
            .OrderByDescending(x => x.SubmittedAt)
            .Take(RecentRecordLimit)
            .Select(x => MapDvirRow(x, tripById.GetValueOrDefault(x.TripId)?.TripNumber ?? string.Empty))
            .ToList();

        return new ProofDvirReportSummaryResponse(
            now,
            normalizedScope,
            windowStart,
            windowEnd,
            scopedProofs.Count,
            scopedDvirs.Count,
            tripItems.Count,
            scopedDvirs.Count(x =>
                string.Equals(x.Phase, DvirInspectionPhases.PreTrip, StringComparison.OrdinalIgnoreCase)),
            scopedDvirs.Count(x =>
                string.Equals(x.Phase, DvirInspectionPhases.PostTrip, StringComparison.OrdinalIgnoreCase)),
            scopedDvirs.Count(x =>
                string.Equals(x.Result, DvirInspectionResults.Fail, StringComparison.OrdinalIgnoreCase)
                || string.Equals(x.Result, DvirInspectionResults.Conditional, StringComparison.OrdinalIgnoreCase)),
            CountBy(scopedProofs.Select(x => x.ProofType)),
            CountBy(scopedDvirs.Select(x => x.Phase)),
            CountBy(scopedDvirs.Select(x => x.Result)),
            tripItems,
            recentProofs,
            recentDvirs);
    }

    public async Task<ProofDvirReportTripDetailResponse> GetTripDetailAsync(
        Guid tenantId,
        Guid tripId,
        CancellationToken cancellationToken = default)
    {
        var trip = await db.Trips
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == tripId, cancellationToken)
            ?? throw new StlApiException("reports.trip_not_found", "Trip was not found.", 404);

        var proofs = await db.TripProofRecords
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.TripId == tripId)
            .OrderByDescending(x => x.CapturedAt)
            .ToListAsync(cancellationToken);

        var dvirs = await db.TripDvirInspections
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.TripId == tripId)
            .OrderBy(x => x.Phase)
            .ToListAsync(cancellationToken);

        return new ProofDvirReportTripDetailResponse(
            trip.Id,
            trip.TripNumber,
            trip.Title,
            trip.DispatchStatus,
            trip.AssignedDriverPersonId,
            trip.VehicleRefKey,
            trip.ScheduledStartAt,
            trip.ScheduledEndAt,
            proofs.Count,
            dvirs.Any(x =>
                string.Equals(x.Phase, DvirInspectionPhases.PreTrip, StringComparison.OrdinalIgnoreCase)),
            dvirs.Any(x =>
                string.Equals(x.Phase, DvirInspectionPhases.PostTrip, StringComparison.OrdinalIgnoreCase)),
            dvirs.Count(x =>
                string.Equals(x.Result, DvirInspectionResults.Fail, StringComparison.OrdinalIgnoreCase)
                || string.Equals(x.Result, DvirInspectionResults.Conditional, StringComparison.OrdinalIgnoreCase)),
            proofs.Select(x => MapProofRow(x, trip.TripNumber)).ToList(),
            dvirs.Select(x => MapDvirRow(x, trip.TripNumber)).ToList());
    }

    public async Task<ProofDvirReportProofDetailResponse> GetProofDetailAsync(
        Guid tenantId,
        Guid proofId,
        CancellationToken cancellationToken = default)
    {
        var proof = await db.TripProofRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == proofId, cancellationToken)
            ?? throw new StlApiException("reports.proof_not_found", "Trip proof record was not found.", 404);

        var trip = await db.Trips
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Id == proof.TripId)
            .Select(x => new { x.TripNumber, x.Title })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new StlApiException("reports.trip_not_found", "Trip was not found.", 404);

        return new ProofDvirReportProofDetailResponse(
            proof.Id,
            proof.TripId,
            trip.TripNumber,
            trip.Title,
            proof.ProofType,
            proof.CapturedByPersonId,
            proof.VehicleRefKey,
            proof.ReferenceKey,
            proof.Notes,
            proof.CapturedAt,
            proof.CreatedAt);
    }

    public async Task<ProofDvirReportDvirDetailResponse> GetDvirDetailAsync(
        Guid tenantId,
        Guid dvirId,
        CancellationToken cancellationToken = default)
    {
        var dvir = await db.TripDvirInspections
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == dvirId, cancellationToken)
            ?? throw new StlApiException("reports.dvir_not_found", "Trip DVIR inspection was not found.", 404);

        var trip = await db.Trips
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Id == dvir.TripId)
            .Select(x => new { x.TripNumber, x.Title })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new StlApiException("reports.trip_not_found", "Trip was not found.", 404);

        return new ProofDvirReportDvirDetailResponse(
            dvir.Id,
            dvir.TripId,
            trip.TripNumber,
            trip.Title,
            dvir.Phase,
            dvir.VehicleRefKey,
            dvir.Result,
            dvir.OdometerReading,
            dvir.DefectNotes,
            dvir.SubmittedByPersonId,
            dvir.SubmittedAt,
            dvir.CreatedAt);
    }

    public async Task<(string ContentType, string FileName, byte[] Content)> ExportSummaryCsvAsync(
        Guid tenantId,
        string? scope,
        CancellationToken cancellationToken = default)
    {
        var normalizedScope = NormalizeScope(scope);
        var now = DateTimeOffset.UtcNow;
        var (windowStart, windowEnd) = GetWindow(normalizedScope, now);

        var proofs = await db.TripProofRecords
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.CapturedAt >= windowStart && x.CapturedAt < windowEnd)
            .OrderByDescending(x => x.CapturedAt)
            .ToListAsync(cancellationToken);

        var dvirs = await db.TripDvirInspections
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.SubmittedAt >= windowStart && x.SubmittedAt < windowEnd)
            .OrderByDescending(x => x.SubmittedAt)
            .ToListAsync(cancellationToken);

        var tripIds = proofs.Select(x => x.TripId).Concat(dvirs.Select(x => x.TripId)).Distinct().ToList();
        var tripNumbers = tripIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await db.Trips
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && tripIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.TripNumber, cancellationToken);

        var builder = new StringBuilder();
        builder.AppendLine(
            "recordType,recordId,tripNumber,tripId,typeOrPhase,resultOrReference,personId,vehicleRefKey,timestamp,notes");

        foreach (var proof in proofs)
        {
            tripNumbers.TryGetValue(proof.TripId, out var tripNumber);
            AppendCsvRow(
                builder,
                "proof",
                proof.Id,
                tripNumber ?? string.Empty,
                proof.TripId,
                proof.ProofType,
                proof.ReferenceKey,
                proof.CapturedByPersonId,
                proof.VehicleRefKey ?? string.Empty,
                proof.CapturedAt,
                proof.Notes);
        }

        foreach (var dvir in dvirs)
        {
            tripNumbers.TryGetValue(dvir.TripId, out var tripNumber);
            AppendCsvRow(
                builder,
                "dvir",
                dvir.Id,
                tripNumber ?? string.Empty,
                dvir.TripId,
                dvir.Phase,
                dvir.Result,
                dvir.SubmittedByPersonId,
                dvir.VehicleRefKey,
                dvir.SubmittedAt,
                dvir.DefectNotes);
        }

        var fileName = $"routarr-proof-dvir-report-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.csv";
        return ("text/csv", fileName, Encoding.UTF8.GetBytes(builder.ToString()));
    }

    private static void AppendCsvRow(
        StringBuilder builder,
        string recordType,
        Guid recordId,
        string tripNumber,
        Guid tripId,
        string typeOrPhase,
        string resultOrReference,
        string personId,
        string vehicleRefKey,
        DateTimeOffset timestamp,
        string notes)
    {
        builder.Append(CsvEscape(recordType));
        builder.Append(',');
        builder.Append(recordId);
        builder.Append(',');
        builder.Append(CsvEscape(tripNumber));
        builder.Append(',');
        builder.Append(tripId);
        builder.Append(',');
        builder.Append(CsvEscape(typeOrPhase));
        builder.Append(',');
        builder.Append(CsvEscape(resultOrReference));
        builder.Append(',');
        builder.Append(CsvEscape(personId));
        builder.Append(',');
        builder.Append(CsvEscape(vehicleRefKey));
        builder.Append(',');
        builder.Append(timestamp.ToString("O"));
        builder.Append(',');
        builder.AppendLine(CsvEscape(notes));
    }

    private static ProofDvirReportProofRow MapProofRow(TripProofRecord entity, string tripNumber) =>
        new(
            entity.Id,
            entity.TripId,
            tripNumber,
            entity.ProofType,
            entity.CapturedByPersonId,
            entity.VehicleRefKey,
            entity.ReferenceKey,
            entity.CapturedAt);

    private static ProofDvirReportDvirRow MapDvirRow(TripDvirInspection entity, string tripNumber) =>
        new(
            entity.Id,
            entity.TripId,
            tripNumber,
            entity.Phase,
            entity.Result,
            entity.VehicleRefKey,
            entity.SubmittedByPersonId,
            entity.SubmittedAt);

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

    private static IReadOnlyList<ProofDvirReportCountItem> CountBy(IEnumerable<string> keys) =>
        keys
            .GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
            .Select(g => new ProofDvirReportCountItem(g.Key, g.Count()))
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
