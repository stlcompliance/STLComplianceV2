using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed class AuditPackageService(
    RoutArrDbContext db,
    IRoutArrAuditService auditService)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
    };

    public AuditPackageManifestResponse GetManifest() =>
        new(
            PackageVersion: "1",
            Sections:
            [
                new("audit_events", "audit_events.json", "Audit events", "Tenant-scoped RoutArr audit trail (JSON)."),
                new("audit_events_csv", "audit_events.csv", "Audit events (CSV)", "Same audit events in CSV for spreadsheets."),
                new("proof_records", "proof_records.json", "Proof records", "Trip proof records with tamper evidence hashes."),
                new("proof_records_csv", "proof_records.csv", "Proof records (CSV)", "Trip proof records in CSV for spreadsheets."),
                new("dvir_inspections", "dvir_inspections.json", "DVIR inspections", "Pre-trip and post-trip DVIR records with tamper evidence hashes."),
                new("dvir_inspections_csv", "dvir_inspections.csv", "DVIR inspections (CSV)", "DVIR records in CSV for spreadsheets."),
                new("capture_attachments", "capture_attachments.json", "Capture attachments", "Proof/DVIR attachment metadata with tamper evidence hashes."),
                new("capture_attachments_csv", "capture_attachments.csv", "Capture attachments (CSV)", "Attachment metadata in CSV for spreadsheets."),
            ]);

    public async Task<AuditPackageFilterOptionsResponse> GetFilterOptionsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var query = db.AuditEvents.AsNoTracking().Where(x => x.TenantId == tenantId);

        var actions = await query
            .Select(x => x.Action)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        var results = await query
            .Select(x => x.Result)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        var targetTypes = await query
            .Select(x => x.TargetType)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        var actorUserIds = await query
            .Where(x => x.ActorUserId != null)
            .Select(x => x.ActorUserId!.Value)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        return new AuditPackageFilterOptionsResponse(actions, results, targetTypes, actorUserIds);
    }

    public async Task<AuditPackageExportSummaryResponse> GetExportSummaryAsync(
        Guid tenantId,
        AuditPackageFilter filter,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(filter.From, filter.To);
        var package = await LoadPackageDataAsync(tenantId, filter, cancellationToken);

        var byResult = package.AuditEvents
            .GroupBy(x => x.Result, StringComparer.OrdinalIgnoreCase)
            .Select(group => new AuditPackageBreakdownItem(group.Key, group.Count()))
            .OrderByDescending(x => x.Count)
            .ToList();

        var byAction = package.AuditEvents
            .GroupBy(x => x.Action, StringComparer.OrdinalIgnoreCase)
            .Select(group => new AuditPackageBreakdownItem(group.Key, group.Count()))
            .OrderByDescending(x => x.Count)
            .Take(15)
            .ToList();

        return new AuditPackageExportSummaryResponse(
            MapAppliedFilters(filter),
            package.Counts,
            byResult,
            byAction,
            DateTimeOffset.UtcNow);
    }

    public async Task<PagedResult<AuditEventExportItem>> ListAuditTimelineAsync(
        Guid tenantId,
        AuditPackageFilter filter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(filter.From, filter.To);
        page = page < 1 ? 1 : page;
        pageSize = pageSize switch
        {
            < 1 => 25,
            > 100 => 100,
            _ => pageSize,
        };

        var query = db.AuditEvents.AsNoTracking().Where(x => x.TenantId == tenantId);
        query = ApplyAuditEventFilters(query, filter);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AuditEventExportItem(
                x.Id,
                x.ActorUserId,
                x.Action,
                x.TargetType,
                x.TargetId,
                x.Result,
                x.ReasonCode,
                x.CorrelationId,
                x.OccurredAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<AuditEventExportItem>(
            items,
            page,
            pageSize,
            totalCount,
            page * pageSize < totalCount);
    }

    public async Task<AuditPackageExportResponse> BuildExportAsync(
        Guid tenantId,
        Guid? actorUserId,
        AuditPackageFilter filter,
        CancellationToken cancellationToken = default)
    {
        var package = await MaterializeExportAsync(tenantId, filter, cancellationToken);

        await auditService.WriteAsync(
            "audit_package.export",
            tenantId,
            actorUserId,
            "audit_package",
            package.PackageId.ToString(),
            "success",
            reasonCode: BuildFilterReasonCode(filter),
            cancellationToken: cancellationToken);

        return package;
    }

    public async Task<byte[]> ExportZipAsync(
        Guid tenantId,
        Guid? actorUserId,
        AuditPackageFilter filter,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(filter.From, filter.To);
        var package = await MaterializeExportAsync(tenantId, filter, cancellationToken);
        var zipBytes = await CreateZipBytesAsync(package, cancellationToken);

        await auditService.WriteAsync(
            "audit_package.export",
            tenantId,
            actorUserId,
            "audit_package",
            package.PackageId.ToString(),
            "success",
            reasonCode: BuildFilterReasonCode(filter),
            cancellationToken: cancellationToken);

        return zipBytes;
    }

    public async Task<byte[]> ExportAuditEventsCsvAsync(
        Guid tenantId,
        Guid? actorUserId,
        AuditPackageFilter filter,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(filter.From, filter.To);
        var package = await LoadPackageDataAsync(tenantId, filter, cancellationToken);
        var csvBytes = AuditPackageCsvWriter.WriteAuditEvents(package.AuditEvents);

        await auditService.WriteAsync(
            "audit_package.export_csv",
            tenantId,
            actorUserId,
            "audit_package",
            Guid.NewGuid().ToString(),
            "success",
            reasonCode: BuildFilterReasonCode(filter),
            cancellationToken: cancellationToken);

        return csvBytes;
    }

    public async Task<AuditPackageExportResponse> MaterializeExportAsync(
        Guid tenantId,
        AuditPackageFilter filter,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(filter.From, filter.To);
        var package = await LoadPackageDataAsync(tenantId, filter, cancellationToken);
        return package with { PackageId = Guid.NewGuid() };
    }

    public static AuditPackageFilter FromJob(AuditPackageGenerationJob job)
    {
        if (string.IsNullOrWhiteSpace(job.FilterJson))
        {
            return new AuditPackageFilter(job.FromUtc, job.ToUtc);
        }

        return JsonSerializer.Deserialize<AuditPackageFilter>(job.FilterJson, JsonOptions)
            ?? new AuditPackageFilter(job.FromUtc, job.ToUtc);
    }

    public static string SerializeFilter(AuditPackageFilter filter) =>
        JsonSerializer.Serialize(filter, JsonOptions);

    public async Task<byte[]> CreateZipBytesAsync(
        AuditPackageExportResponse package,
        CancellationToken cancellationToken = default)
    {
        await using var memory = new MemoryStream();
        using (var archive = new ZipArchive(memory, ZipArchiveMode.Create, leaveOpen: true))
        {
            await WriteJsonEntryAsync(archive, "manifest.json", new
            {
                package.PackageId,
                package.TenantId,
                package.GeneratedAt,
                package.DateRange,
                package.AppliedFilters,
                package.Counts,
                PackageVersion = "1",
            }, cancellationToken);

            await WriteJsonEntryAsync(archive, "audit_events.json", package.AuditEvents, cancellationToken);
            await WriteBytesEntryAsync(archive, "audit_events.csv", AuditPackageCsvWriter.WriteAuditEvents(package.AuditEvents), cancellationToken);
            await WriteJsonEntryAsync(archive, "proof_records.json", package.ProofRecords, cancellationToken);
            await WriteBytesEntryAsync(archive, "proof_records.csv", AuditPackageCsvWriter.WriteProofRecords(package.ProofRecords), cancellationToken);
            await WriteJsonEntryAsync(archive, "dvir_inspections.json", package.DvirInspections, cancellationToken);
            await WriteBytesEntryAsync(archive, "dvir_inspections.csv", AuditPackageCsvWriter.WriteDvirInspections(package.DvirInspections), cancellationToken);
            await WriteJsonEntryAsync(archive, "capture_attachments.json", package.CaptureAttachments, cancellationToken);
            await WriteBytesEntryAsync(archive, "capture_attachments.csv", AuditPackageCsvWriter.WriteCaptureAttachments(package.CaptureAttachments), cancellationToken);
        }

        return memory.ToArray();
    }

    private async Task<AuditPackageExportResponse> LoadPackageDataAsync(
        Guid tenantId,
        AuditPackageFilter filter,
        CancellationToken cancellationToken)
    {
        var auditEventsQuery = db.AuditEvents.AsNoTracking().Where(x => x.TenantId == tenantId);
        auditEventsQuery = ApplyAuditEventFilters(auditEventsQuery, filter);

        var auditEvents = await auditEventsQuery
            .OrderBy(x => x.OccurredAt)
            .Select(x => new AuditEventExportItem(
                x.Id,
                x.ActorUserId,
                x.Action,
                x.TargetType,
                x.TargetId,
                x.Result,
                x.ReasonCode,
                x.CorrelationId,
                x.OccurredAt))
            .ToListAsync(cancellationToken);

        var proofRecords = await (
            from proof in ApplyProofDateFilters(db.TripProofRecords.AsNoTracking().Where(x => x.TenantId == tenantId), filter)
            join trip in db.Trips.AsNoTracking() on proof.TripId equals trip.Id
            orderby proof.CapturedAt
            select new
            {
                proof,
                trip.TripNumber,
            })
            .ToListAsync(cancellationToken);

        var proofItems = proofRecords
            .Select(x => new ProofRecordExportItem(
                x.proof.Id,
                x.proof.TripId,
                x.TripNumber,
                x.proof.ProofType,
                x.proof.CapturedByPersonId,
                x.proof.VehicleRefKey,
                x.proof.ReferenceKey,
                x.proof.Notes,
                x.proof.CapturedAt,
                x.proof.CreatedAt,
                x.proof.UpdatedAt,
                ComputeEvidenceHash(
                    "proof",
                    x.proof.Id,
                    x.proof.TripId,
                    x.proof.ProofType,
                    x.proof.CapturedByPersonId,
                    x.proof.VehicleRefKey,
                    x.proof.ReferenceKey,
                    x.proof.Notes,
                    x.proof.CapturedAt,
                    x.proof.CreatedAt,
                    x.proof.UpdatedAt)))
            .ToList();

        var dvirRecords = await (
            from dvir in ApplyDvirDateFilters(db.TripDvirInspections.AsNoTracking().Where(x => x.TenantId == tenantId), filter)
            join trip in db.Trips.AsNoTracking() on dvir.TripId equals trip.Id
            orderby dvir.SubmittedAt
            select new
            {
                dvir,
                trip.TripNumber,
            })
            .ToListAsync(cancellationToken);

        var dvirItems = dvirRecords
            .Select(x => new DvirInspectionExportItem(
                x.dvir.Id,
                x.dvir.TripId,
                x.TripNumber,
                x.dvir.Phase,
                x.dvir.VehicleRefKey,
                x.dvir.Result,
                x.dvir.OdometerReading,
                x.dvir.DefectNotes,
                x.dvir.SubmittedByPersonId,
                x.dvir.SubmittedAt,
                x.dvir.CreatedAt,
                x.dvir.UpdatedAt,
                ComputeEvidenceHash(
                    "dvir",
                    x.dvir.Id,
                    x.dvir.TripId,
                    x.dvir.Phase,
                    x.dvir.VehicleRefKey,
                    x.dvir.Result,
                    x.dvir.OdometerReading,
                    x.dvir.DefectNotes,
                    x.dvir.SubmittedByPersonId,
                    x.dvir.SubmittedAt,
                    x.dvir.CreatedAt,
                    x.dvir.UpdatedAt)))
            .ToList();

        var attachmentRecords = await (
            from attachment in ApplyAttachmentDateFilters(db.TripCaptureAttachments.AsNoTracking().Where(x => x.TenantId == tenantId), filter)
            join trip in db.Trips.AsNoTracking() on attachment.TripId equals trip.Id
            orderby attachment.CreatedAt
            select new
            {
                attachment,
                trip.TripNumber,
            })
            .ToListAsync(cancellationToken);

        var attachmentItems = attachmentRecords
            .Select(x => new CaptureAttachmentExportItem(
                x.attachment.Id,
                x.attachment.TripId,
                x.TripNumber,
                x.attachment.SubjectType,
                x.attachment.SubjectId,
                x.attachment.AttachmentKind,
                x.attachment.FileName,
                x.attachment.ContentType,
                x.attachment.SizeBytes,
                x.attachment.StorageKey,
                x.attachment.Notes,
                x.attachment.CapturedByPersonId,
                x.attachment.CreatedAt,
                ComputeEvidenceHash(
                    "attachment",
                    x.attachment.Id,
                    x.attachment.TripId,
                    x.attachment.SubjectType,
                    x.attachment.SubjectId,
                    x.attachment.AttachmentKind,
                    x.attachment.FileName,
                    x.attachment.ContentType,
                    x.attachment.SizeBytes,
                    x.attachment.StorageKey,
                    x.attachment.Notes,
                    x.attachment.CapturedByPersonId,
                    x.attachment.CreatedAt)))
            .ToList();

        return new AuditPackageExportResponse(
            Guid.Empty,
            tenantId,
            DateTimeOffset.UtcNow,
            new AuditPackageDateRangeResponse(filter.From, filter.To),
            MapAppliedFilters(filter),
            new AuditPackageCountsResponse(
                auditEvents.Count,
                proofItems.Count,
                dvirItems.Count,
                attachmentItems.Count),
            auditEvents,
            proofItems,
            dvirItems,
            attachmentItems);
    }

    private static IQueryable<RoutArrAuditEvent> ApplyAuditEventFilters(
        IQueryable<RoutArrAuditEvent> query,
        AuditPackageFilter filter)
    {
        query = ApplyOccurredAtFilter(query, filter.From, filter.To);

        if (!string.IsNullOrWhiteSpace(filter.Action))
        {
            var action = filter.Action.Trim().ToLowerInvariant();
            query = query.Where(x => x.Action.ToLower() == action);
        }

        if (!string.IsNullOrWhiteSpace(filter.Result))
        {
            var result = filter.Result.Trim().ToLowerInvariant();
            query = query.Where(x => x.Result.ToLower() == result);
        }

        if (!string.IsNullOrWhiteSpace(filter.TargetType))
        {
            var targetType = filter.TargetType.Trim().ToLowerInvariant();
            query = query.Where(x => x.TargetType.ToLower() == targetType);
        }

        if (filter.ActorUserId is Guid actorUserId)
        {
            query = query.Where(x => x.ActorUserId == actorUserId);
        }

        return query;
    }

    private static IQueryable<RoutArrAuditEvent> ApplyOccurredAtFilter(
        IQueryable<RoutArrAuditEvent> query,
        DateTimeOffset? from,
        DateTimeOffset? to)
    {
        if (from is not null)
        {
            query = query.Where(x => x.OccurredAt >= from);
        }

        if (to is not null)
        {
            query = query.Where(x => x.OccurredAt <= to);
        }

        return query;
    }

    private static IQueryable<TripProofRecord> ApplyProofDateFilters(
        IQueryable<TripProofRecord> query,
        AuditPackageFilter filter)
    {
        if (filter.From is not null)
        {
            query = query.Where(x => x.CapturedAt >= filter.From);
        }

        if (filter.To is not null)
        {
            query = query.Where(x => x.CapturedAt <= filter.To);
        }

        return query;
    }

    private static IQueryable<TripDvirInspection> ApplyDvirDateFilters(
        IQueryable<TripDvirInspection> query,
        AuditPackageFilter filter)
    {
        if (filter.From is not null)
        {
            query = query.Where(x => x.SubmittedAt >= filter.From);
        }

        if (filter.To is not null)
        {
            query = query.Where(x => x.SubmittedAt <= filter.To);
        }

        return query;
    }

    private static IQueryable<TripCaptureAttachment> ApplyAttachmentDateFilters(
        IQueryable<TripCaptureAttachment> query,
        AuditPackageFilter filter)
    {
        if (filter.From is not null)
        {
            query = query.Where(x => x.CreatedAt >= filter.From);
        }

        if (filter.To is not null)
        {
            query = query.Where(x => x.CreatedAt <= filter.To);
        }

        return query;
    }

    private static async Task WriteJsonEntryAsync<T>(
        ZipArchive archive,
        string entryName,
        T payload,
        CancellationToken cancellationToken)
    {
        var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
        await using var entryStream = entry.Open();
        await JsonSerializer.SerializeAsync(entryStream, payload, JsonOptions, cancellationToken);
    }

    private static async Task WriteBytesEntryAsync(
        ZipArchive archive,
        string entryName,
        byte[] bytes,
        CancellationToken cancellationToken)
    {
        var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
        await using var entryStream = entry.Open();
        await entryStream.WriteAsync(bytes, cancellationToken);
    }

    private static string ComputeEvidenceHash(params object?[] values)
    {
        var canonical = string.Join(
            '\u001f',
            values.Select(value => value switch
            {
                null => string.Empty,
                DateTimeOffset dateTime => dateTime.ToString("O"),
                _ => value.ToString() ?? string.Empty,
            }));
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(canonical));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static void ValidateDateRange(DateTimeOffset? from, DateTimeOffset? to)
    {
        if (from is not null && to is not null && from > to)
        {
            throw new StlApiException(
                "audit_package.invalid_date_range",
                "The 'from' date must be before or equal to the 'to' date.",
                400);
        }
    }

    private static AuditPackageAppliedFiltersResponse MapAppliedFilters(AuditPackageFilter filter) =>
        new(filter.From, filter.To, filter.Action, filter.Result, filter.TargetType, filter.ActorUserId);

    private static string BuildFilterReasonCode(AuditPackageFilter filter)
    {
        var parts = new List<string>();
        if (filter.From is not null)
        {
            parts.Add($"from={filter.From:O}");
        }

        if (filter.To is not null)
        {
            parts.Add($"to={filter.To:O}");
        }

        if (!string.IsNullOrWhiteSpace(filter.Action))
        {
            parts.Add($"action={filter.Action}");
        }

        if (!string.IsNullOrWhiteSpace(filter.Result))
        {
            parts.Add($"result={filter.Result}");
        }

        return parts.Count == 0 ? "all" : string.Join(";", parts);
    }
}
