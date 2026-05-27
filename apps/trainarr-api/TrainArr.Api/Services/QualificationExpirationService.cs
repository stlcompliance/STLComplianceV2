using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;

namespace TrainArr.Api.Services;

public sealed class QualificationExpirationService(
    TrainArrDbContext db,
    QualificationIssueService qualificationIssueService)
{
    public const string ProcessExpirationsActionScope = "trainarr.qualifications.expire";

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000f1");

    public async Task<PendingQualificationExpirationsResponse> ListPendingAsync(
        Guid? tenantId,
        DateTimeOffset? asOfUtc,
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = NormalizeBatchSize(batchSize);
        var items = await LoadPendingCandidatesAsync(tenantId, asOf, normalizedBatchSize, cancellationToken);
        return new PendingQualificationExpirationsResponse(asOf, normalizedBatchSize, items);
    }

    public async Task<ProcessQualificationExpirationsResponse> ProcessBatchAsync(
        ProcessQualificationExpirationsRequest request,
        CancellationToken cancellationToken = default)
    {
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = NormalizeBatchSize(request.BatchSize);
        var candidates = await LoadPendingCandidatesAsync(request.TenantId, asOf, batchSize, cancellationToken);

        var expiredIds = new List<Guid>();
        var skipped = new List<QualificationExpirationSkip>();

        foreach (var candidate in candidates)
        {
            try
            {
                await qualificationIssueService.ExpireByWorkerAsync(
                    candidate.TenantId,
                    candidate.QualificationIssueId,
                    cancellationToken);
                expiredIds.Add(candidate.QualificationIssueId);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                skipped.Add(new QualificationExpirationSkip(
                    candidate.QualificationIssueId,
                    ex.Message));
            }
        }

        return new ProcessQualificationExpirationsResponse(
            asOf,
            batchSize,
            candidates.Count,
            expiredIds.Count,
            skipped.Count,
            expiredIds,
            skipped);
    }

    private async Task<IReadOnlyList<PendingQualificationExpirationItem>> LoadPendingCandidatesAsync(
        Guid? tenantId,
        DateTimeOffset asOfUtc,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var query =
            from issue in db.QualificationIssues.AsNoTracking()
            join publication in db.CertificationPublications.AsNoTracking()
                on issue.GrantPublicationId equals publication.Id into publications
            from publication in publications.DefaultIfEmpty()
            where QualificationExpirationRules.ExpirableStatuses.Contains(issue.Status)
            select new
            {
                issue,
                GrantExpiresAt = publication != null ? publication.ExpiresAt : null
            };

        if (tenantId is Guid scopedTenantId)
        {
            query = query.Where(x => x.issue.TenantId == scopedTenantId);
        }

        var rows = await query
            .OrderBy(x => x.issue.ExpiresAt ?? x.GrantExpiresAt ?? DateTimeOffset.MaxValue)
            .ThenBy(x => x.issue.IssuedAt)
            .Take(batchSize * 4)
            .ToListAsync(cancellationToken);

        var results = new List<PendingQualificationExpirationItem>();
        foreach (var row in rows)
        {
            var effectiveExpiresAt = QualificationExpirationRules.ResolveEffectiveExpiresAt(
                row.issue.ExpiresAt,
                row.GrantExpiresAt);
            if (!QualificationExpirationRules.ShouldExpire(
                    row.issue.Status,
                    row.issue.ExpiresAt,
                    row.GrantExpiresAt,
                    asOfUtc))
            {
                continue;
            }

            results.Add(new PendingQualificationExpirationItem(
                row.issue.Id,
                row.issue.TenantId,
                row.issue.StaffarrPersonId,
                row.issue.QualificationKey,
                row.issue.QualificationName,
                row.issue.Status,
                effectiveExpiresAt!.Value));

            if (results.Count >= batchSize)
            {
                break;
            }
        }

        return results;
    }

    private static int NormalizeBatchSize(int batchSize) =>
        batchSize is < 1 or > 500 ? 100 : batchSize;
}
