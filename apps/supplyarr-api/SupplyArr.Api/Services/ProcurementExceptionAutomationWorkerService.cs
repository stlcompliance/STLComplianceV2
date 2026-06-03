using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public sealed class ProcurementExceptionAutomationWorkerService(
    SupplyArrDbContext db,
    ProcurementExceptionService procurementExceptionService,
    ISupplyArrAuditService audit)
{
    public const string ProcessProcurementExceptionAutoClosesActionScope =
        "supplyarr.procurement_exceptions.auto_close";

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000fd");

    public async Task<PendingProcurementExceptionAutoClosesResponse> ListPendingAsync(
        Guid? tenantId,
        DateTimeOffset? asOfUtc,
        int? batchSize,
        CancellationToken cancellationToken = default)
    {
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = ProcurementExceptionAutomationRules.NormalizeBatchSize(batchSize);
        var candidates = await LoadPendingCandidatesAsync(tenantId, asOf, normalizedBatchSize, cancellationToken);

        var items = candidates
            .Select(x => new PendingProcurementExceptionAutoCloseItem(
                x.ProcurementExceptionId,
                x.ExceptionKey,
                x.SubjectType,
                x.SubjectId,
                x.SubjectKey,
                x.Title,
                x.Status,
                x.ResolvedAt,
                x.WaivedAt,
                x.CompletedAt,
                x.HoursCompleted,
                x.HoursUntilAutoClose))
            .ToList();

        return new PendingProcurementExceptionAutoClosesResponse(asOf, normalizedBatchSize, items);
    }

    public async Task<ProcessProcurementExceptionAutoClosesResponse> ProcessBatchAsync(
        ProcessProcurementExceptionAutoClosesRequest request,
        CancellationToken cancellationToken = default)
    {
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = ProcurementExceptionAutomationRules.NormalizeBatchSize(request.BatchSize);
        var candidates = await LoadPendingCandidatesAsync(request.TenantId, asOf, batchSize, cancellationToken);

        var closed = new List<ProcurementExceptionAutoCloseResult>();
        var skipped = new List<ProcurementExceptionAutoCloseSkip>();

        foreach (var candidate in candidates)
        {
            try
            {
                var response = await procurementExceptionService.CloseAsync(
                    candidate.TenantId,
                    WorkerActorUserId,
                    candidate.ProcurementExceptionId,
                    new CloseProcurementExceptionRequest(
                        "Automatically closed by procurement exception completion policy."),
                    cancellationToken);

                closed.Add(new ProcurementExceptionAutoCloseResult(
                    response.ExceptionId,
                    response.ExceptionKey,
                    response.Status,
                    response.ClosedAt ?? DateTimeOffset.UtcNow));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                skipped.Add(new ProcurementExceptionAutoCloseSkip(candidate.ProcurementExceptionId, ex.Message));
            }
        }

        if (request.TenantId is Guid tenantId && closed.Count > 0)
        {
            await audit.WriteAsync(
                "supplyarr.procurement_exception_auto_close.batch",
                tenantId,
                WorkerActorUserId,
                "procurement_exception_auto_close_run",
                $"{closed.Count}",
                "success",
                cancellationToken: cancellationToken);
        }

        return new ProcessProcurementExceptionAutoClosesResponse(
            asOf,
            batchSize,
            candidates.Count,
            closed.Count,
            skipped.Count,
            closed,
            skipped);
    }

    private async Task<IReadOnlyList<PendingProcurementExceptionAutoCloseCandidate>> LoadPendingCandidatesAsync(
        Guid? tenantId,
        DateTimeOffset asOfUtc,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var enabledTenantIds = await db.TenantProcurementExceptionEscalationSettings
            .AsNoTracking()
            .Where(x =>
                x.AutoCloseCompletedExceptionsEnabled
                && (tenantId == null || x.TenantId == tenantId))
            .Select(x => x.TenantId)
            .ToListAsync(cancellationToken);

        if (enabledTenantIds.Count == 0)
        {
            return [];
        }

        var settingsByTenant = await db.TenantProcurementExceptionEscalationSettings
            .AsNoTracking()
            .Where(x => enabledTenantIds.Contains(x.TenantId))
            .ToDictionaryAsync(x => x.TenantId, cancellationToken);

        var completedStatuses = new[]
        {
            ProcurementExceptionStatuses.Resolved,
            ProcurementExceptionStatuses.Waived,
        };

        var exceptions = await db.ProcurementExceptions
            .AsNoTracking()
            .Where(x =>
                enabledTenantIds.Contains(x.TenantId)
                && x.ClosedAt == null
                && x.Status != null
                && completedStatuses.Contains(x.Status))
            .OrderByDescending(x => x.UpdatedAt)
            .Take(batchSize * 4)
            .ToListAsync(cancellationToken);

        var candidates = new List<PendingProcurementExceptionAutoCloseCandidate>();

        foreach (var exception in exceptions)
        {
            if (!settingsByTenant.TryGetValue(exception.TenantId, out var settingsEntity))
            {
                continue;
            }

            var settings = ProcurementExceptionEscalationSettingsService.ToSnapshot(settingsEntity);
            if (!ProcurementExceptionAutomationRules.IsDueForAutoClose(exception, settings, asOfUtc))
            {
                continue;
            }

            var completedAt = ProcurementExceptionAutomationRules.GetCompletedAt(exception);
            var hoursCompleted = ProcurementExceptionAutomationRules.ComputeHoursCompleted(completedAt, asOfUtc);
            var hoursUntilAutoClose = ProcurementExceptionAutomationRules.ComputeHoursUntilAutoClose(
                exception,
                settings,
                asOfUtc) ?? 0;

            candidates.Add(new PendingProcurementExceptionAutoCloseCandidate(
                exception.TenantId,
                exception.Id,
                exception.ExceptionKey,
                exception.SubjectType,
                exception.SubjectId,
                exception.SubjectKey,
                exception.Title,
                exception.Status,
                exception.ResolvedAt,
                exception.WaivedAt,
                completedAt,
                hoursCompleted,
                hoursUntilAutoClose));
        }

        return candidates
            .OrderByDescending(x => x.HoursCompleted)
            .Take(batchSize)
            .ToList();
    }

    private sealed record PendingProcurementExceptionAutoCloseCandidate(
        Guid TenantId,
        Guid ProcurementExceptionId,
        string ExceptionKey,
        string SubjectType,
        Guid SubjectId,
        string SubjectKey,
        string Title,
        string Status,
        DateTimeOffset? ResolvedAt,
        DateTimeOffset? WaivedAt,
        DateTimeOffset? CompletedAt,
        double HoursCompleted,
        double HoursUntilAutoClose);
}
