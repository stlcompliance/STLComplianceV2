using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;

namespace TrainArr.Api.Services;

public sealed class RulePackImpactWorkerService(
    TrainArrDbContext db,
    RulePackImpactSettingsService settingsService,
    RulePackImpactService impactService,
    ITrainArrAuditService audit)
{
    public const string ProcessImpactScansActionScope = "trainarr.rulepack_impact.scan";

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000f5");

    public async Task<PendingRulePackImpactScansResponse> ListPendingAsync(
        Guid? tenantId,
        DateTimeOffset? asOfUtc,
        int? batchSize,
        int? stalenessHours,
        CancellationToken cancellationToken = default)
    {
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = RulePackImpactRules.NormalizeBatchSize(batchSize);
        var normalizedStalenessHours = RulePackImpactRules.NormalizeStalenessHours(stalenessHours);
        var items = await LoadPendingCandidatesAsync(
            tenantId,
            asOf,
            normalizedBatchSize,
            normalizedStalenessHours,
            cancellationToken);

        var responseItems = items
            .Select(x => new PendingRulePackImpactItem(x.RulePackKey, x.TenantId, x.LastComputedAt))
            .ToList();

        return new PendingRulePackImpactScansResponse(
            asOf,
            normalizedStalenessHours,
            normalizedBatchSize,
            responseItems);
    }

    public async Task<ProcessRulePackImpactScansResponse> ProcessBatchAsync(
        ProcessRulePackImpactScansRequest request,
        CancellationToken cancellationToken = default)
    {
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = RulePackImpactRules.NormalizeBatchSize(request.BatchSize);
        var stalenessHours = RulePackImpactRules.NormalizeStalenessHours(request.StalenessHours);
        var candidates = await LoadPendingCandidatesAsync(
            request.TenantId,
            asOf,
            batchSize,
            stalenessHours,
            cancellationToken);

        var assessedRulePackKeys = new List<string>();
        var attentionRequiredRulePackKeys = new List<string>();
        var skipped = new List<RulePackImpactScanSkip>();

        foreach (var candidate in candidates)
        {
            try
            {
                var settings = await settingsService.LoadSnapshotAsync(candidate.TenantId, cancellationToken);
                var autoUpdateBaselines = settings?.AutoUpdateRequirementBaselines == true;

                var assessment = await impactService.AssessAsync(
                    candidate.TenantId,
                    WorkerActorUserId,
                    candidate.RulePackKey,
                    expectedVersionNumber: null,
                    expectedStatus: null,
                    cancellationToken);

                var now = DateTimeOffset.UtcNow;
                var triggers = string.Join(",", assessment.Triggers);
                var runOutcome = assessment.Summary.RequiresAttention ? "attention_required" : "assessed";

                var existingState = await db.RulePackImpactStates
                    .FirstOrDefaultAsync(
                        x => x.TenantId == candidate.TenantId && x.RulePackKey == candidate.RulePackKey,
                        cancellationToken);

                if (existingState is null)
                {
                    existingState = new RulePackImpactState
                    {
                        Id = Guid.NewGuid(),
                        TenantId = candidate.TenantId,
                        RulePackKey = candidate.RulePackKey,
                        CreatedAt = now,
                    };
                    db.RulePackImpactStates.Add(existingState);
                }

                existingState.RequiresAttention = assessment.Summary.RequiresAttention;
                existingState.HasDrift = assessment.Summary.HasDrift;
                existingState.Triggers = Truncate(triggers, 512);
                existingState.BaselineVersionNumber = assessment.Drift?.BaselineVersionNumber;
                existingState.CurrentVersionNumber = assessment.Drift?.CurrentVersionNumber;
                existingState.BaselineStatus = assessment.Drift?.BaselineStatus;
                existingState.CurrentStatus = assessment.Drift?.CurrentStatus;
                existingState.RequirementCount = assessment.Summary.RequirementCount;
                existingState.DefinitionCount = assessment.Summary.DefinitionCount;
                existingState.ProgramCount = assessment.Summary.ProgramCount;
                existingState.ActiveAssignmentCount = assessment.Summary.ActiveAssignmentCount;
                existingState.ActiveQualificationCount = assessment.Summary.ActiveQualificationCount;
                existingState.LastAssessmentId = assessment.AssessmentId;
                existingState.ComputedAt = now;
                existingState.UpdatedAt = now;

                if (RulePackImpactRules.ShouldAutoUpdateBaselines(
                        autoUpdateBaselines,
                        assessment.Summary.RequiresAttention,
                        assessment.Drift?.PackNotFound == true)
                    && assessment.CurrentState is not null)
                {
                    await UpdateRequirementBaselinesAsync(
                        candidate.TenantId,
                        candidate.RulePackKey,
                        assessment.CurrentState.VersionNumber,
                        assessment.CurrentState.Status,
                        now,
                        cancellationToken);
                }

                db.RulePackImpactRuns.Add(new RulePackImpactRun
                {
                    Id = Guid.NewGuid(),
                    TenantId = candidate.TenantId,
                    RulePackKey = candidate.RulePackKey,
                    Outcome = runOutcome,
                    RequiresAttention = assessment.Summary.RequiresAttention,
                    ProcessedAt = now,
                    CreatedAt = now,
                });

                await db.SaveChangesAsync(cancellationToken);
                assessedRulePackKeys.Add(candidate.RulePackKey);
                if (assessment.Summary.RequiresAttention)
                {
                    attentionRequiredRulePackKeys.Add(candidate.RulePackKey);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                db.RulePackImpactRuns.Add(new RulePackImpactRun
                {
                    Id = Guid.NewGuid(),
                    TenantId = candidate.TenantId,
                    RulePackKey = candidate.RulePackKey,
                    Outcome = "skipped",
                    RequiresAttention = false,
                    SkipReason = Truncate(ex.Message, 512),
                    ProcessedAt = DateTimeOffset.UtcNow,
                    CreatedAt = DateTimeOffset.UtcNow,
                });
                await db.SaveChangesAsync(cancellationToken);
                skipped.Add(new RulePackImpactScanSkip(candidate.RulePackKey, ex.Message));
            }
        }

        if (assessedRulePackKeys.Count > 0 && request.TenantId is Guid tenantId)
        {
            await audit.WriteAsync(
                "rule_pack_impact.batch",
                tenantId,
                WorkerActorUserId,
                "rule_pack_impact",
                $"{assessedRulePackKeys.Count}",
                "Succeeded",
                cancellationToken: cancellationToken);
        }

        return new ProcessRulePackImpactScansResponse(
            asOf,
            batchSize,
            candidates.Count,
            assessedRulePackKeys.Count,
            attentionRequiredRulePackKeys.Count,
            skipped.Count,
            assessedRulePackKeys,
            attentionRequiredRulePackKeys,
            skipped);
    }

    public async Task<RulePackImpactStatesResponse> ListRecentStatesAsync(
        Guid tenantId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var take = RulePackImpactRules.NormalizeStateListLimit(limit);
        var rows = await db.RulePackImpactStates
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.ComputedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(x => new RulePackImpactStateItem(
                x.RulePackKey,
                x.RequiresAttention,
                x.HasDrift,
                ParseTriggers(x.Triggers),
                x.BaselineVersionNumber,
                x.CurrentVersionNumber,
                x.BaselineStatus,
                x.CurrentStatus,
                x.ActiveAssignmentCount,
                x.ActiveQualificationCount,
                x.ComputedAt))
            .ToList();

        return new RulePackImpactStatesResponse(items);
    }

    public async Task<RulePackImpactRunsResponse> ListRecentRunsAsync(
        Guid tenantId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var take = RulePackImpactRules.NormalizeRunListLimit(limit);
        var rows = await db.RulePackImpactRuns
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.ProcessedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(x => new RulePackImpactRunItem(
                x.Id,
                x.RulePackKey,
                x.Outcome,
                x.RequiresAttention,
                x.SkipReason,
                x.ProcessedAt))
            .ToList();

        return new RulePackImpactRunsResponse(items);
    }

    private async Task UpdateRequirementBaselinesAsync(
        Guid tenantId,
        string rulePackKey,
        int currentVersionNumber,
        string currentStatus,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var requirements = await db.TrainingRulePackRequirements
            .Where(x => x.TenantId == tenantId && x.RulePackKey == rulePackKey)
            .ToListAsync(cancellationToken);

        foreach (var requirement in requirements)
        {
            requirement.KnownVersionNumber = currentVersionNumber;
            requirement.KnownStatus = currentStatus;
            requirement.UpdatedAt = now;
        }
    }

    private async Task<IReadOnlyList<PendingImpactCandidate>> LoadPendingCandidatesAsync(
        Guid? tenantId,
        DateTimeOffset asOfUtc,
        int batchSize,
        int stalenessHours,
        CancellationToken cancellationToken)
    {
        var settingsQuery = db.TenantRulePackImpactSettings
            .AsNoTracking()
            .Where(x => x.IsEnabled);

        if (tenantId is Guid scopedTenantId)
        {
            settingsQuery = settingsQuery.Where(x => x.TenantId == scopedTenantId);
        }

        var tenantSettings = await settingsQuery.ToListAsync(cancellationToken);
        var results = new List<PendingImpactCandidate>();

        foreach (var settings in tenantSettings)
        {
            if (results.Count >= batchSize)
            {
                break;
            }

            var tenantStalenessHours = RulePackImpactRules.NormalizeStalenessHours(settings.StalenessHours);
            var effectiveStalenessHours = tenantId is null
                ? tenantStalenessHours
                : RulePackImpactRules.NormalizeStalenessHours(stalenessHours);

            var rulePackKeys = await db.TrainingRulePackRequirements
                .AsNoTracking()
                .Where(x => x.TenantId == settings.TenantId)
                .Select(x => x.RulePackKey)
                .Distinct()
                .ToListAsync(cancellationToken);

            var states = await db.RulePackImpactStates
                .AsNoTracking()
                .Where(x => x.TenantId == settings.TenantId)
                .ToDictionaryAsync(x => x.RulePackKey, x => x.ComputedAt, cancellationToken);

            foreach (var rulePackKey in rulePackKeys.OrderBy(x => x))
            {
                if (results.Count >= batchSize)
                {
                    break;
                }

                states.TryGetValue(rulePackKey, out var lastComputedAt);
                if (!RulePackImpactRules.IsStale(lastComputedAt, asOfUtc, effectiveStalenessHours))
                {
                    continue;
                }

                results.Add(new PendingImpactCandidate(rulePackKey, settings.TenantId, lastComputedAt));
            }
        }

        return results;
    }

    private static IReadOnlyList<string> ParseTriggers(string triggers) =>
        string.IsNullOrWhiteSpace(triggers)
            ? Array.Empty<string>()
            : triggers.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];

    private sealed record PendingImpactCandidate(
        string RulePackKey,
        Guid TenantId,
        DateTimeOffset? LastComputedAt);
}
