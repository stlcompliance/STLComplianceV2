using Microsoft.EntityFrameworkCore;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed class M12AnalyticsBatchWorkerService(
    ComplianceCoreDbContext db,
    RiskScoringService riskScoringService,
    MissingEvidenceWarningService missingEvidenceWarningService,
    ControlEffectivenessService controlEffectivenessService,
    ReadinessForecastService readinessForecastService,
    AuditPackageGenerationService auditPackageGenerationService,
    IComplianceCoreAuditService auditService)
{
    public const string ProcessBatchActionScope = "compliancecore.m12_analytics.process_batch";

    public async Task<PendingM12AnalyticsBatchesResponse> ListPendingAsync(
        Guid? tenantId,
        DateTimeOffset? asOfUtc,
        int? intervalHours,
        int? batchSize,
        CancellationToken cancellationToken = default)
    {
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedIntervalHours = M12AnalyticsBatchRules.NormalizeIntervalHours(intervalHours);
        var normalizedBatchSize = M12AnalyticsBatchRules.NormalizeBatchSize(batchSize);
        var items = await BuildDueTenantItemsAsync(
            tenantId,
            asOf,
            normalizedIntervalHours,
            normalizedBatchSize,
            cancellationToken);

        return new PendingM12AnalyticsBatchesResponse(
            asOf,
            normalizedIntervalHours,
            normalizedBatchSize,
            items);
    }

    public async Task<ProcessM12AnalyticsBatchesResponse> ProcessBatchAsync(
        ProcessM12AnalyticsBatchesRequest request,
        CancellationToken cancellationToken = default)
    {
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var intervalHours = M12AnalyticsBatchRules.NormalizeIntervalHours(request.IntervalHours);
        var batchSize = M12AnalyticsBatchRules.NormalizeBatchSize(request.BatchSize);
        var dueItems = await BuildDueTenantItemsAsync(
            request.TenantId,
            asOf,
            intervalHours,
            batchSize,
            cancellationToken);

        var results = new List<M12AnalyticsBatchRunResult>();
        var processedCount = 0;
        var skippedCount = 0;

        foreach (var item in dueItems)
        {
            if (!item.RiskScoringDue
                && !item.MissingEvidenceDue
                && !item.ControlEffectivenessDue
                && !item.ReadinessForecastDue
                && !item.AuditDeliveryDue)
            {
                skippedCount++;
                continue;
            }

            var result = await ProcessTenantBatchAsync(
                item.TenantId,
                item.DefaultScopeKey,
                intervalHours,
                asOf,
                item,
                cancellationToken);
            results.Add(result);
            if (string.Equals(result.Status, M12AnalyticsBatchRunStatuses.Failed, StringComparison.OrdinalIgnoreCase))
            {
                skippedCount++;
            }
            else
            {
                processedCount++;
            }
        }

        return new ProcessM12AnalyticsBatchesResponse(
            asOf,
            intervalHours,
            batchSize,
            dueItems.Count,
            processedCount,
            skippedCount,
            results);
    }

    public async Task<M12AnalyticsBatchRunResult> ProcessTenantManualAsync(
        Guid tenantId,
        Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantM12AnalyticsWorkerSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken)
            ?? throw new StlApiException(
                "m12_analytics.worker_not_configured",
                "Enable and save M12 analytics worker settings before running a manual batch.",
                400);

        if (!settings.IsEnabled)
        {
            throw new StlApiException(
                "m12_analytics.worker_disabled",
                "M12 analytics worker is disabled for this tenant.",
                400);
        }

        var asOf = DateTimeOffset.UtcNow;
        var intervalHours = M12AnalyticsBatchRules.NormalizeIntervalHours(settings.IntervalHours);
        var due = new PendingM12AnalyticsBatchTenantItem(
            tenantId,
            settings.DefaultScopeKey,
            intervalHours,
            settings.RiskScoringEnabled,
            settings.MissingEvidenceEnabled,
            settings.ControlEffectivenessEnabled,
            settings.ReadinessForecastEnabled,
            settings.AuditDeliveryEnabled);

        return await ProcessTenantBatchAsync(
            tenantId,
            settings.DefaultScopeKey,
            intervalHours,
            asOf,
            due,
            actorUserId,
            cancellationToken);
    }

    private async Task<M12AnalyticsBatchRunResult> ProcessTenantBatchAsync(
        Guid tenantId,
        string scopeKey,
        int intervalHours,
        DateTimeOffset asOf,
        PendingM12AnalyticsBatchTenantItem due,
        CancellationToken cancellationToken) =>
        await ProcessTenantBatchAsync(
            tenantId,
            scopeKey,
            intervalHours,
            asOf,
            due,
            AuditPackageGenerationService.WorkerActorUserId,
            cancellationToken);

    private async Task<M12AnalyticsBatchRunResult> ProcessTenantBatchAsync(
        Guid tenantId,
        string scopeKey,
        int intervalHours,
        DateTimeOffset asOf,
        PendingM12AnalyticsBatchTenantItem due,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var settings = await db.TenantM12AnalyticsWorkerSettings
            .FirstAsync(x => x.TenantId == tenantId, cancellationToken);

        var run = new M12AnalyticsBatchRun
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            StartedAt = asOf,
            Status = M12AnalyticsBatchRunStatuses.InProgress,
            IntervalHours = intervalHours,
            ScopeKey = scopeKey,
        };

        db.M12AnalyticsBatchRuns.Add(run);
        await db.SaveChangesAsync(cancellationToken);

        try
        {
            var evaluateContext = (IReadOnlyDictionary<string, string>?)null;

            if (settings.ReadinessForecastEnabled && due.ReadinessForecastDue)
            {
                var forecast = await readinessForecastService.EvaluateAsync(
                    tenantId,
                    actorUserId,
                    new EvaluateReadinessForecastRequest(scopeKey, null, evaluateContext),
                    cancellationToken);
                run.ReadinessForecastRan = true;
                run.ReadinessForecastRunId = forecast.RunId;
                run.RiskScoringRan = true;
                run.MissingEvidenceRan = true;
                run.ControlEffectivenessRan = true;
            }
            else
            {
                if (settings.RiskScoringEnabled && due.RiskScoringDue)
                {
                    var risk = await riskScoringService.EvaluateAsync(
                        tenantId,
                        actorUserId,
                        new EvaluateRiskScoresRequest(scopeKey, null, evaluateContext),
                        cancellationToken);
                    run.RiskScoringRan = true;
                    run.RiskScoreRunId = risk.RunId;
                }

                if (settings.MissingEvidenceEnabled && due.MissingEvidenceDue)
                {
                    var missing = await missingEvidenceWarningService.EvaluateAsync(
                        tenantId,
                        actorUserId,
                        new EvaluateMissingEvidenceWarningsRequest(scopeKey, null, evaluateContext),
                        cancellationToken);
                    run.MissingEvidenceRan = true;
                    run.MissingEvidenceWarningRunId = missing.RunId;
                }

                if (settings.ControlEffectivenessEnabled && due.ControlEffectivenessDue)
                {
                    var effectiveness = await controlEffectivenessService.EvaluateAsync(
                        tenantId,
                        actorUserId,
                        new EvaluateControlEffectivenessRequest(scopeKey, null, evaluateContext),
                        cancellationToken);
                    run.ControlEffectivenessRan = true;
                    run.ControlEffectivenessRunId = effectiveness.RunId;
                }
            }

            if (settings.AuditDeliveryEnabled && due.AuditDeliveryDue)
            {
                var job = await auditPackageGenerationService.CreateJobAsync(
                    tenantId,
                    actorUserId,
                    new CreateAuditPackageGenerationJobRequest("zip", null, null),
                    cancellationToken);
                run.AuditDeliveryQueued = true;
                run.AuditPackageJobId = job.JobId;
                settings.LastAuditDeliveryRunAt = asOf;
            }

            var now = DateTimeOffset.UtcNow;
            run.CompletedAt = now;
            run.Status = M12AnalyticsBatchRunStatuses.Completed;
            settings.LastBatchRunAt = asOf;
            if (run.RiskScoringRan)
            {
                settings.LastRiskScoringRunAt = asOf;
            }

            if (run.MissingEvidenceRan)
            {
                settings.LastMissingEvidenceRunAt = asOf;
            }

            if (run.ControlEffectivenessRan)
            {
                settings.LastControlEffectivenessRunAt = asOf;
            }

            if (run.ReadinessForecastRan)
            {
                settings.LastReadinessForecastRunAt = asOf;
            }

            settings.UpdatedAt = now;
            await db.SaveChangesAsync(cancellationToken);

            await auditService.WriteAsync(
                "m12_analytics.batch.completed",
                tenantId,
                actorUserId,
                "m12_analytics_batch_run",
                run.Id.ToString(),
                "success",
                reasonCode: scopeKey,
                cancellationToken: cancellationToken);

            return new M12AnalyticsBatchRunResult(
                run.Id,
                tenantId,
                run.Status,
                run.RiskScoringRan,
                run.MissingEvidenceRan,
                run.ControlEffectivenessRan,
                run.ReadinessForecastRan,
                run.AuditDeliveryQueued,
                run.AuditPackageJobId,
                null);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            run.CompletedAt = DateTimeOffset.UtcNow;
            run.Status = M12AnalyticsBatchRunStatuses.Failed;
            run.ErrorMessage = ex.Message.Length > 2000 ? ex.Message[..2000] : ex.Message;
            await db.SaveChangesAsync(cancellationToken);

            await auditService.WriteAsync(
                "m12_analytics.batch.failed",
                tenantId,
                actorUserId,
                "m12_analytics_batch_run",
                run.Id.ToString(),
                "failed",
                reasonCode: ex.Message,
                cancellationToken: cancellationToken);

            return new M12AnalyticsBatchRunResult(
                run.Id,
                tenantId,
                run.Status,
                run.RiskScoringRan,
                run.MissingEvidenceRan,
                run.ControlEffectivenessRan,
                run.ReadinessForecastRan,
                run.AuditDeliveryQueued,
                run.AuditPackageJobId,
                run.ErrorMessage);
        }
    }

    private async Task<IReadOnlyList<PendingM12AnalyticsBatchTenantItem>> BuildDueTenantItemsAsync(
        Guid? tenantId,
        DateTimeOffset asOf,
        int intervalHours,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var query = db.TenantM12AnalyticsWorkerSettings
            .AsNoTracking()
            .Where(x => x.IsEnabled);

        if (tenantId.HasValue)
        {
            query = query.Where(x => x.TenantId == tenantId.Value);
        }

        var settingsRows = await query
            .OrderBy(x => x.LastBatchRunAt ?? DateTimeOffset.MinValue)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        var items = new List<PendingM12AnalyticsBatchTenantItem>();
        foreach (var settings in settingsRows)
        {
            var effectiveInterval = M12AnalyticsBatchRules.NormalizeIntervalHours(settings.IntervalHours);
            var riskDue = settings.RiskScoringEnabled
                && M12AnalyticsBatchRules.IsDue(settings.LastRiskScoringRunAt, effectiveInterval, asOf);
            var missingDue = settings.MissingEvidenceEnabled
                && M12AnalyticsBatchRules.IsDue(settings.LastMissingEvidenceRunAt, effectiveInterval, asOf);
            var controlDue = settings.ControlEffectivenessEnabled
                && M12AnalyticsBatchRules.IsDue(settings.LastControlEffectivenessRunAt, effectiveInterval, asOf);
            var forecastDue = settings.ReadinessForecastEnabled
                && M12AnalyticsBatchRules.IsDue(settings.LastReadinessForecastRunAt, effectiveInterval, asOf);
            var auditDue = settings.AuditDeliveryEnabled
                && M12AnalyticsBatchRules.IsDue(settings.LastAuditDeliveryRunAt, effectiveInterval, asOf);

            if (!riskDue && !missingDue && !controlDue && !forecastDue && !auditDue)
            {
                continue;
            }

            items.Add(new PendingM12AnalyticsBatchTenantItem(
                settings.TenantId,
                settings.DefaultScopeKey,
                effectiveInterval,
                riskDue,
                missingDue,
                controlDue,
                forecastDue,
                auditDue));
        }

        return items;
    }
}
