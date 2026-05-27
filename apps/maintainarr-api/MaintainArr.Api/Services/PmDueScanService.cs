using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class PmDueScanService(
    MaintainArrDbContext db,
    WorkOrderService workOrders,
    IMaintainArrAuditService audit)
{
    public const string ProcessDueScanActionScope = "maintainarr.pm.scan";

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000f4");

    public async Task<PendingPmDueResponse> ListPendingAsync(
        Guid? tenantId,
        DateTimeOffset? asOfUtc,
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = NormalizeBatchSize(batchSize);
        var items = await LoadPendingCandidatesAsync(tenantId, asOf, normalizedBatchSize, cancellationToken);
        return new PendingPmDueResponse(asOf, normalizedBatchSize, items);
    }

    public async Task<ProcessPmDueScanResponse> ProcessBatchAsync(
        ProcessPmDueScanRequest request,
        CancellationToken cancellationToken = default)
    {
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = NormalizeBatchSize(request.BatchSize ?? 100);
        var overdueGraceDays = NormalizeOverdueGraceDays(request.OverdueGraceDays);
        var candidates = await LoadPendingCandidatesAsync(request.TenantId, asOf, batchSize, cancellationToken);

        var updatedIds = new List<Guid>();
        var markedDue = 0;
        var markedOverdue = 0;
        var skipped = new List<PmDueScanSkip>();
        var createdWorkOrderIds = new List<Guid>();
        var workOrdersCreated = 0;
        var workOrdersLinked = 0;
        var workOrderGenerationSkipped = new List<PmWorkOrderGenerationSkip>();

        foreach (var candidate in candidates)
        {
            try
            {
                var outcome = await ApplyDueScanAsync(
                    candidate.PmScheduleId,
                    asOf,
                    overdueGraceDays,
                    cancellationToken);

                var effectiveDueStatus = outcome
                    ?? PmDueScanRules.ResolveTargetDueStatus(
                        "active",
                        candidate.DueStatus,
                        candidate.NextDueAt,
                        asOf,
                        overdueGraceDays);

                if (outcome is not null)
                {
                    updatedIds.Add(candidate.PmScheduleId);
                    if (string.Equals(outcome, PmDueStatuses.Due, StringComparison.OrdinalIgnoreCase))
                    {
                        markedDue++;
                    }
                    else if (string.Equals(outcome, PmDueStatuses.Overdue, StringComparison.OrdinalIgnoreCase))
                    {
                        markedOverdue++;
                    }
                }

                if (PmWorkOrderGenerationRules.ShouldEnsureWorkOrder(effectiveDueStatus))
                {
                    try
                    {
                        var workOrderResult = await workOrders.EnsureForDuePmScheduleAsync(
                            candidate.PmScheduleId,
                            effectiveDueStatus,
                            WorkerActorUserId,
                            cancellationToken);

                        if (workOrderResult.LinkedExisting)
                        {
                            workOrdersLinked++;
                        }
                        else
                        {
                            workOrdersCreated++;
                            createdWorkOrderIds.Add(workOrderResult.WorkOrderId);
                        }
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        workOrderGenerationSkipped.Add(
                            new PmWorkOrderGenerationSkip(candidate.PmScheduleId, ex.Message));
                    }
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                skipped.Add(new PmDueScanSkip(candidate.PmScheduleId, ex.Message));
            }
        }

        return new ProcessPmDueScanResponse(
            asOf,
            batchSize,
            candidates.Count,
            markedDue,
            markedOverdue,
            skipped.Count,
            workOrdersCreated,
            workOrdersLinked,
            workOrderGenerationSkipped.Count,
            updatedIds,
            createdWorkOrderIds,
            skipped,
            workOrderGenerationSkipped);
    }

    public async Task<string?> ApplyDueScanAsync(
        Guid pmScheduleId,
        DateTimeOffset asOfUtc,
        int overdueGraceDays,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.PmSchedules.FirstOrDefaultAsync(
            x => x.Id == pmScheduleId,
            cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("pm_schedule.not_found", "PM schedule was not found.", 404);
        }

        if (!PmDueScanRules.IsScannableScheduleStatus(entity.Status))
        {
            throw new StlApiException(
                "pm_schedule.not_scannable",
                $"PM schedule status '{entity.Status}' is not eligible for due scanning.",
                409);
        }

        var targetDueStatus = PmDueScanRules.ResolveTargetDueStatus(
            entity.Status,
            entity.DueStatus,
            entity.NextDueAt,
            asOfUtc,
            overdueGraceDays);

        if (string.Equals(targetDueStatus, entity.DueStatus, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        entity.DueStatus = targetDueStatus;
        entity.LastDueScanAt = asOfUtc;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            targetDueStatus == PmDueStatuses.Overdue
                ? "pm_schedule.due_scan.overdue"
                : "pm_schedule.due_scan.due",
            entity.TenantId,
            WorkerActorUserId,
            "pm_schedule",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return targetDueStatus;
    }

    private async Task<IReadOnlyList<PendingPmDueItem>> LoadPendingCandidatesAsync(
        Guid? tenantId,
        DateTimeOffset asOfUtc,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var query = db.PmSchedules.AsNoTracking()
            .Where(x => PmDueScanRules.ScannableScheduleStatuses.Contains(x.Status))
            .Where(x => PmDueScanRules.UpdatableDueStatuses.Contains(x.DueStatus))
            .Where(x => x.NextDueAt <= asOfUtc);

        if (tenantId is Guid scopedTenantId)
        {
            query = query.Where(x => x.TenantId == scopedTenantId);
        }

        return await query
            .Join(
                db.Assets.AsNoTracking(),
                schedule => new { schedule.AssetId, schedule.TenantId },
                asset => new { AssetId = asset.Id, asset.TenantId },
                (schedule, asset) => new { schedule, asset })
            .OrderBy(x => x.schedule.NextDueAt)
            .ThenBy(x => x.schedule.ScheduleKey)
            .Take(batchSize)
            .Select(x => new PendingPmDueItem(
                x.schedule.Id,
                x.schedule.TenantId,
                x.schedule.AssetId,
                x.asset.AssetTag,
                x.asset.Name,
                x.schedule.ScheduleKey,
                x.schedule.DueStatus,
                x.schedule.NextDueAt))
            .ToListAsync(cancellationToken);
    }

    private static int NormalizeBatchSize(int batchSize) =>
        batchSize is < 1 or > 500 ? 100 : batchSize;

    private static int NormalizeOverdueGraceDays(int? overdueGraceDays) =>
        overdueGraceDays is null or < 0 or > 30
            ? PmDueScanRules.DefaultOverdueGraceDays
            : overdueGraceDays.Value;
}
