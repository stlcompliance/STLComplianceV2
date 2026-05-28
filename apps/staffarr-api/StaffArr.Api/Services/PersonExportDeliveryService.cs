using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;

namespace StaffArr.Api.Services;

public sealed class PersonExportDeliveryService(
    StaffArrDbContext db,
    PeopleExportService exportService,
    IStaffArrAuditService audit,
    PersonExportDeliveryNotificationService notificationService)
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
                var failedRunId = await RecordFailureRunAsync(candidate, ex.Message, asOf, cancellationToken);
                await notificationService.NotifyFailureAsync(candidate, ex.Message, failedRunId, cancellationToken);
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
        var deliveryRunId = Guid.NewGuid();
        db.PersonExportDeliveryRuns.Add(new PersonExportDeliveryRun
        {
            Id = deliveryRunId,
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

        var result = new PersonExportDeliveryResult(
            schedule.TenantId,
            export.ExportId,
            export.PersonCount,
            completedAt);

        await notificationService.NotifySuccessAsync(schedule, result, deliveryRunId, cancellationToken);

        return result;
    }

    private async Task<Guid> RecordFailureRunAsync(
        TenantPersonExportSchedule schedule,
        string reason,
        DateTimeOffset asOfUtc,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var runId = Guid.NewGuid();
        db.PersonExportDeliveryRuns.Add(new PersonExportDeliveryRun
        {
            Id = runId,
            TenantId = schedule.TenantId,
            ExportId = Guid.Empty,
            PersonCount = 0,
            Status = "failed",
            IntervalHours = schedule.IntervalHours,
            SkipReason = PersonExportDeliveryRules.TruncateSkipReason(reason),
            StartedAt = asOfUtc,
            CompletedAt = now,
            CreatedAt = now,
        });

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "person.export.scheduled_delivery.failed",
            schedule.TenantId,
            WorkerActorUserId,
            "person_export_delivery",
            runId.ToString(),
            "failed",
            reasonCode: reason,
            cancellationToken: cancellationToken);

        return runId;
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
