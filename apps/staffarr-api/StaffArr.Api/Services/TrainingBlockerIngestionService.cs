using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class TrainingBlockerIngestionService(
    StaffArrDbContext db,
    IStaffArrAuditService audit)
{
    private static readonly HashSet<string> AllowedBlockerTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "missing_assignment",
        "overdue",
        "failed",
        "suspended"
    };

    public async Task<TrainingBlockerIngestionResponse> IngestAsync(
        IngestTrainingBlockerRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsurePersonExistsAsync(request.TenantId, request.PersonId, cancellationToken);

        var qualificationKey = NormalizeQualificationKey(request.QualificationKey);
        var qualificationName = NormalizeQualificationName(request.QualificationName);
        var blockerType = NormalizeBlockerType(request.BlockerType);
        var message = NormalizeMessage(request.Message);
        var now = DateTimeOffset.UtcNow;

        var existing = await db.PersonTrainingBlockers.FirstOrDefaultAsync(
            x => x.TenantId == request.TenantId && x.TrainarrPublicationId == request.TrainarrPublicationId,
            cancellationToken);

        if (existing is null)
        {
            existing = new PersonTrainingBlocker
            {
                Id = Guid.NewGuid(),
                TenantId = request.TenantId,
                PersonId = request.PersonId,
                TrainarrPublicationId = request.TrainarrPublicationId,
                CreatedAt = now
            };
            db.PersonTrainingBlockers.Add(existing);
        }

        existing.PersonId = request.PersonId;
        existing.QualificationKey = qualificationKey;
        existing.QualificationName = qualificationName;
        existing.BlockerType = blockerType;
        existing.Message = message;
        existing.Status = "active";
        existing.PublishedAt = now;
        existing.ExpiresAt = request.ExpiresAt;
        existing.ClearedAt = null;
        existing.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "training_blocker.ingest",
            request.TenantId,
            null,
            "person_training_blocker",
            existing.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return new TrainingBlockerIngestionResponse(existing.Id, existing.TrainarrPublicationId, existing.Status);
    }

    public async Task<TrainingBlockerIngestionResponse> ClearAsync(
        ClearTrainingBlockerRequest request,
        CancellationToken cancellationToken = default)
    {
        var existing = await db.PersonTrainingBlockers.FirstOrDefaultAsync(
            x => x.TenantId == request.TenantId
                && x.TrainarrPublicationId == request.TrainarrPublicationId
                && x.PersonId == request.PersonId,
            cancellationToken);

        if (existing is null)
        {
            throw new StlApiException(
                "training_blockers.not_found",
                "Training blocker publication was not found.",
                404);
        }

        var now = DateTimeOffset.UtcNow;
        existing.Status = "cleared";
        existing.ClearedAt = now;
        existing.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "training_blocker.clear",
            request.TenantId,
            null,
            "person_training_blocker",
            existing.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return new TrainingBlockerIngestionResponse(existing.Id, existing.TrainarrPublicationId, existing.Status);
    }

    public async Task<IReadOnlyList<PersonTrainingBlocker>> GetActiveBlockersAsync(
        Guid tenantId,
        Guid personId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        return await db.PersonTrainingBlockers
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && x.PersonId == personId
                && x.Status == "active"
                && (x.ExpiresAt == null || x.ExpiresAt > now))
            .OrderBy(x => x.QualificationName)
            .ToListAsync(cancellationToken);
    }

    private async Task EnsurePersonExistsAsync(
        Guid tenantId,
        Guid personId,
        CancellationToken cancellationToken)
    {
        var exists = await db.People.AnyAsync(
            x => x.TenantId == tenantId && x.Id == personId,
            cancellationToken);
        if (!exists)
        {
            throw new StlApiException("people.not_found", "Person was not found.", 404);
        }
    }

    private static string NormalizeQualificationKey(string qualificationKey)
    {
        var normalized = qualificationKey.Trim().ToLowerInvariant();
        if (normalized.Length < 3 || normalized.Length > 128)
        {
            throw new StlApiException(
                "training_blockers.validation",
                "Qualification key must be between 3 and 128 characters.",
                400);
        }

        return normalized;
    }

    private static string NormalizeQualificationName(string qualificationName)
    {
        var trimmed = qualificationName.Trim();
        if (trimmed.Length < 3 || trimmed.Length > 128)
        {
            throw new StlApiException(
                "training_blockers.validation",
                "Qualification name must be between 3 and 128 characters.",
                400);
        }

        return trimmed;
    }

    private static string NormalizeBlockerType(string blockerType)
    {
        var normalized = blockerType.Trim().ToLowerInvariant();
        if (!AllowedBlockerTypes.Contains(normalized))
        {
            throw new StlApiException(
                "training_blockers.validation",
                $"Blocker type must be one of: {string.Join(", ", AllowedBlockerTypes.OrderBy(x => x))}.",
                400);
        }

        return normalized;
    }

    private static string NormalizeMessage(string message)
    {
        var trimmed = message.Trim();
        if (trimmed.Length < 16 || trimmed.Length > 1024)
        {
            throw new StlApiException(
                "training_blockers.validation",
                "Blocker message must be between 16 and 1024 characters.",
                400);
        }

        return trimmed;
    }
}
