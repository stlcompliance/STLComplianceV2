using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;

namespace TrainArr.Api.Services;

public sealed class RecertificationAssignmentService(
    TrainArrDbContext db,
    RecertificationSettingsService settingsService,
    TrainingAssignmentService assignmentService,
    ITrainArrAuditService audit)
{
    public const string ProcessAssignmentsActionScope = "trainarr.recertification.assign";

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000f3");

    public async Task<PendingRecertificationCandidatesResponse> ListPendingAsync(
        Guid? tenantId,
        DateTimeOffset? asOfUtc,
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = RecertificationAssignmentRules.NormalizeBatchSize(batchSize);
        var items = await LoadPendingCandidatesAsync(tenantId, asOf, normalizedBatchSize, cancellationToken);
        return new PendingRecertificationCandidatesResponse(asOf, normalizedBatchSize, items);
    }

    public async Task<ProcessRecertificationAssignmentsResponse> ProcessBatchAsync(
        ProcessRecertificationAssignmentsRequest request,
        CancellationToken cancellationToken = default)
    {
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = RecertificationAssignmentRules.NormalizeBatchSize(request.BatchSize);
        var candidates = await LoadPendingCandidatesAsync(request.TenantId, asOf, batchSize, cancellationToken);

        var createdAssignmentIds = new List<Guid>();
        var skipped = new List<RecertificationAssignmentSkip>();

        foreach (var candidate in candidates)
        {
            try
            {
                var assignment = await assignmentService.CreateRecertificationByWorkerAsync(
                    candidate.TenantId,
                    WorkerActorUserId,
                    candidate.QualificationIssueId,
                    candidate.TrainingDefinitionId,
                    candidate.StaffarrPersonId,
                    candidate.EffectiveExpiresAt,
                    cancellationToken);

                db.RecertificationAssignmentRuns.Add(new RecertificationAssignmentRun
                {
                    Id = Guid.NewGuid(),
                    TenantId = candidate.TenantId,
                    QualificationIssueId = candidate.QualificationIssueId,
                    TrainingAssignmentId = assignment.AssignmentId,
                    Outcome = "assigned",
                    ProcessedAt = DateTimeOffset.UtcNow,
                    CreatedAt = DateTimeOffset.UtcNow,
                });

                await db.SaveChangesAsync(cancellationToken);
                createdAssignmentIds.Add(assignment.AssignmentId);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                db.RecertificationAssignmentRuns.Add(new RecertificationAssignmentRun
                {
                    Id = Guid.NewGuid(),
                    TenantId = candidate.TenantId,
                    QualificationIssueId = candidate.QualificationIssueId,
                    Outcome = "skipped",
                    SkipReason = Truncate(ex.Message, 512),
                    ProcessedAt = DateTimeOffset.UtcNow,
                    CreatedAt = DateTimeOffset.UtcNow,
                });
                await db.SaveChangesAsync(cancellationToken);
                skipped.Add(new RecertificationAssignmentSkip(candidate.QualificationIssueId, ex.Message));
            }
        }

        if (createdAssignmentIds.Count > 0 && request.TenantId is Guid tenantId)
        {
            await audit.WriteAsync(
                "recertification_assignment.batch",
                tenantId,
                WorkerActorUserId,
                "recertification_assignment",
                $"{createdAssignmentIds.Count}",
                "Succeeded",
                cancellationToken: cancellationToken);
        }

        return new ProcessRecertificationAssignmentsResponse(
            asOf,
            batchSize,
            candidates.Count,
            createdAssignmentIds.Count,
            skipped.Count,
            createdAssignmentIds,
            skipped);
    }

    public async Task<RecertificationAssignmentRunsResponse> ListRecentRunsAsync(
        Guid tenantId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var take = RecertificationAssignmentRules.NormalizeRunListLimit(limit);
        var rows = await db.RecertificationAssignmentRuns
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.ProcessedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(x => new RecertificationAssignmentRunItem(
                x.Id,
                x.QualificationIssueId,
                x.TrainingAssignmentId,
                x.Outcome,
                x.SkipReason,
                x.ProcessedAt))
            .ToList();

        return new RecertificationAssignmentRunsResponse(items);
    }

    private async Task<IReadOnlyList<PendingRecertificationCandidate>> LoadPendingCandidatesAsync(
        Guid? tenantId,
        DateTimeOffset asOfUtc,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var settingsQuery = db.TenantRecertificationSettings
            .AsNoTracking()
            .Where(x => x.IsEnabled);

        if (tenantId is Guid scopedTenantId)
        {
            settingsQuery = settingsQuery.Where(x => x.TenantId == scopedTenantId);
        }

        var tenantSettings = await settingsQuery.ToListAsync(cancellationToken);
        var results = new List<PendingRecertificationCandidate>();

        foreach (var settings in tenantSettings)
        {
            if (results.Count >= batchSize)
            {
                break;
            }

            var leadDays = RecertificationAssignmentRules.NormalizeLeadDays(settings.LeadDays);
            var rawCandidates = await (
                from issue in db.QualificationIssues.AsNoTracking()
                join publication in db.CertificationPublications.AsNoTracking()
                    on issue.GrantPublicationId equals publication.Id into publications
                from publication in publications.DefaultIfEmpty()
                join sourceAssignment in db.TrainingAssignments.AsNoTracking()
                    on issue.TrainingAssignmentId equals sourceAssignment.Id
                join definition in db.TrainingDefinitions.AsNoTracking()
                    on sourceAssignment.TrainingDefinitionId equals definition.Id
                where issue.TenantId == settings.TenantId
                    && definition.Status == "active"
                select new
                {
                    QualificationIssueId = issue.Id,
                    issue.TenantId,
                    issue.StaffarrPersonId,
                    issue.QualificationKey,
                    issue.QualificationName,
                    issue.Status,
                    issue.ExpiresAt,
                    GrantExpiresAt = publication != null ? publication.ExpiresAt : null,
                    TrainingDefinitionId = definition.Id,
                    TrainingDefinitionName = definition.Name,
                })
                .ToListAsync(cancellationToken);

            foreach (var row in rawCandidates)
            {
                if (results.Count >= batchSize)
                {
                    break;
                }

                if (!RecertificationAssignmentRules.ShouldAssign(
                        row.Status,
                        row.ExpiresAt,
                        row.GrantExpiresAt,
                        asOfUtc,
                        leadDays))
                {
                    continue;
                }

                var effectiveExpiresAt = QualificationExpirationRules.ResolveEffectiveExpiresAt(
                    row.ExpiresAt,
                    row.GrantExpiresAt)!.Value;

                if (await HasBlockingAssignmentAsync(
                        row.TenantId,
                        row.QualificationIssueId,
                        row.StaffarrPersonId,
                        row.QualificationKey,
                        cancellationToken))
                {
                    continue;
                }

                if (await HasRecentRunAsync(row.TenantId, row.QualificationIssueId, cancellationToken))
                {
                    continue;
                }

                results.Add(new PendingRecertificationCandidate(
                    row.QualificationIssueId,
                    row.TenantId,
                    row.StaffarrPersonId,
                    row.TrainingDefinitionId,
                    row.TrainingDefinitionName,
                    row.QualificationKey,
                    row.QualificationName,
                    effectiveExpiresAt));
            }
        }

        return results;
    }

    private async Task<bool> HasBlockingAssignmentAsync(
        Guid tenantId,
        Guid qualificationIssueId,
        Guid staffarrPersonId,
        string qualificationKey,
        CancellationToken cancellationToken)
    {
        var activeRecertificationAssignmentIds = await db.TrainingAssignments
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && TrainingAssignmentService.ActiveAssignmentStatuses.Contains(x.Status)
                && x.AssignmentReason == "recertification"
                && x.StaffarrPersonId == staffarrPersonId)
            .Select(x => new { x.SourceQualificationIssueId, x.TrainingDefinitionId })
            .ToListAsync(cancellationToken);

        if (activeRecertificationAssignmentIds.Any(x => x.SourceQualificationIssueId == qualificationIssueId))
        {
            return true;
        }

        var activeDefinitionKeys = await db.TrainingDefinitions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.QualificationKey == qualificationKey)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        return activeRecertificationAssignmentIds
            .Any(x => activeDefinitionKeys.Contains(x.TrainingDefinitionId));
    }

    private async Task<bool> HasRecentRunAsync(
        Guid tenantId,
        Guid qualificationIssueId,
        CancellationToken cancellationToken)
    {
        return await db.RecertificationAssignmentRuns.AnyAsync(
            x => x.TenantId == tenantId
                && x.QualificationIssueId == qualificationIssueId
                && x.Outcome == "assigned",
            cancellationToken);
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}
