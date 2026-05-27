using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;

namespace StaffArr.Api.Services;

public sealed class CertificationExpirationService(
    StaffArrDbContext db,
    IStaffArrAuditService audit)
{
    public const string ProcessExpirationsActionScope = "staffarr.certifications.expire";

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000f2");

    public async Task<PendingCertificationExpirationsResponse> ListPendingAsync(
        Guid? tenantId,
        DateTimeOffset? asOfUtc,
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = NormalizeBatchSize(batchSize);
        var items = await LoadPendingCandidatesAsync(tenantId, asOf, normalizedBatchSize, cancellationToken);
        return new PendingCertificationExpirationsResponse(asOf, normalizedBatchSize, items);
    }

    public async Task<ProcessCertificationExpirationsResponse> ProcessBatchAsync(
        ProcessCertificationExpirationsRequest request,
        CancellationToken cancellationToken = default)
    {
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = NormalizeBatchSize(request.BatchSize);
        var candidates = await LoadPendingCandidatesAsync(request.TenantId, asOf, batchSize, cancellationToken);

        var expiredIds = new List<Guid>();
        var skipped = new List<CertificationExpirationSkip>();

        foreach (var candidate in candidates)
        {
            try
            {
                await ExpireByWorkerAsync(candidate.PersonCertificationId, cancellationToken);
                expiredIds.Add(candidate.PersonCertificationId);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                skipped.Add(new CertificationExpirationSkip(
                    candidate.PersonCertificationId,
                    ex.Message));
            }
        }

        return new ProcessCertificationExpirationsResponse(
            asOf,
            batchSize,
            candidates.Count,
            expiredIds.Count,
            skipped.Count,
            expiredIds,
            skipped);
    }

    public async Task ExpireByWorkerAsync(
        Guid personCertificationId,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.PersonCertifications.FirstOrDefaultAsync(
            x => x.Id == personCertificationId,
            cancellationToken);
        if (entity is null)
        {
            throw new STLCompliance.Shared.Contracts.StlApiException(
                "person_certification.not_found",
                "Person certification was not found.",
                404);
        }

        if (!CertificationExpirationRules.IsExpirableStatus(entity.Status))
        {
            throw new STLCompliance.Shared.Contracts.StlApiException(
                "person_certification.invalid_status",
                $"Person certification status '{entity.Status}' cannot be expired by the worker.",
                409);
        }

        var asOf = DateTimeOffset.UtcNow;
        if (!CertificationExpirationRules.ShouldExpire(entity.Status, entity.ExpiresAt, asOf))
        {
            throw new STLCompliance.Shared.Contracts.StlApiException(
                "person_certification.not_due",
                "Person certification is not past its expiration date.",
                409);
        }

        entity.Status = "expired";
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        entity.Notes = AppendAutoExpireNote(entity.Notes, entity.ExpiresAt!.Value);

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "person_certification.expire.auto",
            entity.TenantId,
            WorkerActorUserId,
            "person_certification",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);
    }

    private async Task<IReadOnlyList<PendingCertificationExpirationItem>> LoadPendingCandidatesAsync(
        Guid? tenantId,
        DateTimeOffset asOfUtc,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var query = db.PersonCertifications.AsNoTracking()
            .Where(x => CertificationExpirationRules.ExpirableStatuses.Contains(x.Status))
            .Where(x => x.ExpiresAt != null && x.ExpiresAt <= asOfUtc);

        if (tenantId is Guid scopedTenantId)
        {
            query = query.Where(x => x.TenantId == scopedTenantId);
        }

        return await query
            .OrderBy(x => x.ExpiresAt)
            .ThenBy(x => x.GrantedAt)
            .Take(batchSize)
            .Select(x => new PendingCertificationExpirationItem(
                x.Id,
                x.TenantId,
                x.PersonId,
                x.CertificationDefinitionId,
                x.SourceType,
                x.Status,
                x.ExpiresAt!.Value))
            .ToListAsync(cancellationToken);
    }

    private static int NormalizeBatchSize(int batchSize) =>
        batchSize is < 1 or > 500 ? 100 : batchSize;

    private static string AppendAutoExpireNote(string? existingNotes, DateTimeOffset expiresAt)
    {
        var autoNote = $"Automatically expired on schedule (expiresAt {expiresAt:yyyy-MM-dd}).";
        if (string.IsNullOrWhiteSpace(existingNotes))
        {
            return autoNote.Length <= 1024 ? autoNote : autoNote[..1024];
        }

        var combined = $"{existingNotes.Trim()} | {autoNote}";
        return combined.Length <= 1024 ? combined : combined[..1024];
    }
}
