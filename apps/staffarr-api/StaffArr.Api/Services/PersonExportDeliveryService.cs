using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;

namespace StaffArr.Api.Services;

public sealed class PersonExportDeliveryService(
    StaffArrDbContext db,
    PeopleExportService exportService,
    IStaffArrAuditService audit)
{
    public const string ProcessDeliveriesActionScope = "staffarr.people.export.scheduled";

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000f5");

    public async Task<PendingPersonExportDeliveriesResponse> ListPendingAsync(
        Guid? tenantId,
        DateTimeOffset? asOfUtc,
        int? batchSize,
        CancellationToken cancellationToken = default)
    {
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = PersonExportDeliveryRules.NormalizeBatchSize(batchSize);
        var candidates = await LoadPendingCandidatesAsync(tenantId, asOf, normalizedBatchSize, cancellationToken);

        var items = candidates
            .Select(x => new PendingPersonExportDeliveryItem(
                x.TenantId,
                x.IntervalHours,
                x.LastDeliveredAt))
            .ToList();

        return new PendingPersonExportDeliveriesResponse(asOf, normalizedBatchSize, items);
    }

    public async Task<ProcessPersonExportDeliveriesResponse> ProcessBatchAsync(
        ProcessPersonExportDeliveriesRequest request,
        CancellationToken cancellationToken = default)
    {
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = PersonExportDeliveryRules.NormalizeBatchSize(request.BatchSize);
        var candidates = await LoadPendingCandidatesAsync(request.TenantId, asOf, batchSize, cancellationToken);

        var deliveries = new List<PersonExportDeliveryResult>();
        var skipped = new List<PersonExportDeliverySkip>();

        foreach (var candidate in candidates)
        {
            try
            {
                var result = await DeliverAsync(candidate, asOf, cancellationToken);
                deliveries.Add(result);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                skipped.Add(new PersonExportDeliverySkip(candidate.TenantId, ex.Message));
            }
        }

        if (deliveries.Count > 0 && request.TenantId is Guid tenantId)
        {
            await audit.WriteAsync(
                "person.export.scheduled_delivery.batch",
                tenantId,
                WorkerActorUserId,
                "person_export_delivery",
                $"{deliveries.Count}",
                "success",
                cancellationToken: cancellationToken);
        }

        return new ProcessPersonExportDeliveriesResponse(
            asOf,
            batchSize,
            candidates.Count,
            deliveries.Count,
            skipped.Count,
            deliveries,
            skipped);
    }

    private async Task<PersonExportDeliveryResult> DeliverAsync(
        TenantPersonExportSchedule schedule,
        DateTimeOffset asOfUtc,
        CancellationToken cancellationToken)
    {
        var startedAt = DateTimeOffset.UtcNow;
        var preset = await db.TenantPersonExportPresets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == schedule.TenantId, cancellationToken);

        var export = await exportService.BuildExportAsync(
            schedule.TenantId,
            WorkerActorUserId,
            preset?.EmploymentStatus,
            preset?.OrgUnitId,
            cancellationToken);

        var completedAt = DateTimeOffset.UtcNow;
        db.PersonExportDeliveryRuns.Add(new PersonExportDeliveryRun
        {
            Id = Guid.NewGuid(),
            TenantId = schedule.TenantId,
            ExportId = export.ExportId,
            PersonCount = export.PersonCount,
            Status = "success",
            IntervalHours = schedule.IntervalHours,
            EmploymentStatus = preset?.EmploymentStatus,
            OrgUnitId = preset?.OrgUnitId,
            StartedAt = startedAt,
            CompletedAt = completedAt,
            CreatedAt = completedAt,
        });

        schedule.LastDeliveredAt = asOfUtc;
        schedule.UpdatedAt = completedAt;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "person.export.scheduled_delivery",
            schedule.TenantId,
            WorkerActorUserId,
            "person_export",
            export.ExportId.ToString(),
            "success",
            reasonCode: $"{export.PersonCount}",
            cancellationToken: cancellationToken);

        return new PersonExportDeliveryResult(
            schedule.TenantId,
            export.ExportId,
            export.PersonCount,
            completedAt);
    }

    private async Task<List<TenantPersonExportSchedule>> LoadPendingCandidatesAsync(
        Guid? tenantId,
        DateTimeOffset asOfUtc,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var query = db.TenantPersonExportSchedules
            .Where(x => x.IsEnabled);

        if (tenantId is Guid scopedTenantId)
        {
            query = query.Where(x => x.TenantId == scopedTenantId);
        }

        var schedules = await query
            .OrderBy(x => x.LastDeliveredAt ?? DateTimeOffset.MinValue)
            .ThenBy(x => x.TenantId)
            .ToListAsync(cancellationToken);

        return schedules
            .Where(x => PersonExportDeliveryRules.IsDue(x.LastDeliveredAt, asOfUtc, x.IntervalHours))
            .Take(batchSize)
            .ToList();
    }
}
