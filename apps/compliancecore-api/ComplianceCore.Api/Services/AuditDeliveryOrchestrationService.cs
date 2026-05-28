using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed class AuditDeliveryOrchestrationService(
    ComplianceCoreDbContext db,
    M12AnalyticsWorkerSettingsService workerSettingsService,
    M12AnalyticsBatchWorkerService m12BatchWorkerService,
    ScheduledRuleEvaluationService scheduledRuleEvaluationService)
{
    public async Task<AuditDeliveryOrchestrationStatusResponse> GetStatusAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var asOf = DateTimeOffset.UtcNow;
        var workerSettings = await workerSettingsService.GetAsync(tenantId, cancellationToken);

        var settingsEntity = await db.TenantM12AnalyticsWorkerSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var intervalHours = M12AnalyticsBatchRules.NormalizeIntervalHours(workerSettings.IntervalHours);
        var scheduledIntervalHours = ScheduledRuleEvaluationRules.NormalizeIntervalHours(null);

        var pendingScheduled = await scheduledRuleEvaluationService.ListPendingAsync(
            tenantId,
            asOf,
            batchSize: 500,
            intervalHours: scheduledIntervalHours,
            cancellationToken);

        var lastScheduledRun = await db.ScheduledRuleEvaluationRuns
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.StartedAt)
            .FirstOrDefaultAsync(cancellationToken);

        PendingM12AnalyticsBatchTenantItem? pendingM12 = null;
        var batchDue = false;
        if (settingsEntity is { IsEnabled: true })
        {
            var pendingM12Response = await m12BatchWorkerService.ListPendingAsync(
                tenantId,
                asOf,
                intervalHours,
                batchSize: 1,
                cancellationToken);
            pendingM12 = pendingM12Response.Items.FirstOrDefault();
            batchDue = pendingM12 is not null;
        }

        var lastM12Run = await db.M12AnalyticsBatchRuns
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.StartedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var pendingAuditJobs = await db.AuditPackageGenerationJobs
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Where(x => x.Status == AuditPackageGenerationJobStatuses.Pending
                || x.Status == AuditPackageGenerationJobStatuses.Processing)
            .CountAsync(cancellationToken);

        var recentAuditJobs = await db.AuditPackageGenerationJobs
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(5)
            .Select(x => new AuditPackageJobSummary(
                x.Id,
                x.Status,
                x.Format,
                x.CreatedAt,
                x.CompletedAt,
                x.PackageId,
                x.ErrorMessage))
            .ToListAsync(cancellationToken);

        return new AuditDeliveryOrchestrationStatusResponse(
            workerSettings,
            new AuditDeliveryScheduledEvaluationStatus(
                pendingScheduled.Items.Count,
                MapScheduledRun(lastScheduledRun)),
            new AuditDeliveryM12BatchStatus(
                settingsEntity?.IsEnabled ?? false,
                batchDue,
                pendingM12,
                MapM12Run(lastM12Run)),
            new AuditDeliveryAuditPackageStatus(pendingAuditJobs, recentAuditJobs));
    }

    public async Task<TriggerScheduledRuleEvaluationResponse> TriggerScheduledEvaluationAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var result = await scheduledRuleEvaluationService.ProcessTenantManualAsync(
            tenantId,
            emitFindings: true,
            cancellationToken);

        return new TriggerScheduledRuleEvaluationResponse(
            result.ScheduledRunId,
            result.EvaluatedCount,
            result.SkippedCount,
            result.AllowCount,
            result.WarnCount,
            result.BlockCount);
    }

    public async Task<TriggerM12AnalyticsBatchResponse> TriggerM12BatchAsync(
        Guid tenantId,
        Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        var result = await m12BatchWorkerService.ProcessTenantManualAsync(
            tenantId,
            actorUserId,
            cancellationToken);

        return new TriggerM12AnalyticsBatchResponse(
            result.RunId,
            result.Status,
            result.AuditDeliveryQueued,
            result.AuditPackageJobId,
            result.ErrorMessage);
    }

    private static ScheduledRuleEvaluationRunSummary? MapScheduledRun(ScheduledRuleEvaluationRun? run) =>
        run is null
            ? null
            : new ScheduledRuleEvaluationRunSummary(
                run.Id,
                run.StartedAt,
                run.CompletedAt,
                run.Status,
                run.PacksDueCount,
                run.EvaluatedCount,
                run.SkippedCount,
                run.AllowCount,
                run.WarnCount,
                run.BlockCount);

    private static M12AnalyticsBatchRunSummary? MapM12Run(M12AnalyticsBatchRun? run) =>
        run is null
            ? null
            : new M12AnalyticsBatchRunSummary(
                run.Id,
                run.StartedAt,
                run.CompletedAt,
                run.Status,
                run.ScopeKey,
                run.RiskScoringRan,
                run.MissingEvidenceRan,
                run.ControlEffectivenessRan,
                run.ReadinessForecastRan,
                run.AuditDeliveryQueued,
                run.AuditPackageJobId,
                run.ErrorMessage);
}
