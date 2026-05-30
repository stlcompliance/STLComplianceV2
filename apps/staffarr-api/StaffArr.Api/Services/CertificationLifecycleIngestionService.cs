using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class CertificationLifecycleIngestionService(
    StaffArrDbContext db,
    TrainingBlockerIngestionService trainingBlockerService,
    IStaffArrAuditService audit)
{
    private static readonly HashSet<string> AllowedLifecycleActions = new(StringComparer.OrdinalIgnoreCase)
    {
        "suspend",
        "reinstate",
        "revoke",
        "expire"
    };

    public async Task<CertificationLifecycleIngestionResponse> IngestAsync(
        IngestCertificationLifecycleRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsurePersonExistsAsync(request.TenantId, request.PersonId, cancellationToken);

        var lifecycleAction = NormalizeLifecycleAction(request.LifecycleAction);
        var qualificationKey = NormalizeQualificationKey(request.QualificationKey);
        var qualificationName = NormalizeQualificationName(request.QualificationName);
        var message = NormalizeMessage(request.Message);

        var certification = await db.PersonCertifications.FirstOrDefaultAsync(
            x => x.TenantId == request.TenantId
                && x.PersonId == request.PersonId
                && x.ExternalPublicationId == request.TrainarrGrantPublicationId,
            cancellationToken);
        if (certification is null)
        {
            throw new StlApiException(
                "certification_lifecycle.not_found",
                "TrainArr grant publication was not found for this person.",
                404);
        }

        if (certification.LastExternalLifecyclePublicationId == request.TrainarrLifecyclePublicationId)
        {
            return await MapExistingResponseAsync(
                certification,
                request,
                lifecycleAction,
                cancellationToken);
        }

        Guid? trainingBlockerId = null;
        switch (lifecycleAction)
        {
            case "suspend":
            {
                var blocker = await trainingBlockerService.IngestAsync(
                    new IngestTrainingBlockerRequest(
                        request.TenantId,
                        request.PersonId,
                        request.TrainarrLifecyclePublicationId,
                        qualificationKey,
                        qualificationName,
                        "suspended",
                        message,
                        request.ExpiresAt),
                    cancellationToken);
                trainingBlockerId = blocker.TrainingBlockerId;
                break;
            }
            case "revoke":
                certification.Status = "revoked";
                certification.UpdatedAt = DateTimeOffset.UtcNow;
                break;
            case "expire":
                certification.Status = "expired";
                certification.UpdatedAt = DateTimeOffset.UtcNow;
                break;
            case "reinstate":
                await ClearSuspendedTrainingBlockersAsync(
                    request.TenantId,
                    request.PersonId,
                    qualificationKey,
                    cancellationToken);
                certification.Status = "active";
                certification.UpdatedAt = DateTimeOffset.UtcNow;
                break;
        }

        certification.LastExternalLifecyclePublicationId = request.TrainarrLifecyclePublicationId;
        if (!string.Equals(lifecycleAction, "suspend", StringComparison.OrdinalIgnoreCase))
        {
            certification.Notes = AppendLifecycleNote(certification.Notes, lifecycleAction, message);
        }

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            $"person_certification.trainarr_{lifecycleAction}",
            request.TenantId,
            null,
            "person_certification",
            certification.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return new CertificationLifecycleIngestionResponse(
            certification.Id,
            request.TrainarrGrantPublicationId,
            request.TrainarrLifecyclePublicationId,
            lifecycleAction,
            PersonCertificationEffectiveStatus.Resolve(certification),
            trainingBlockerId);
    }

    private async Task<CertificationLifecycleIngestionResponse> MapExistingResponseAsync(
        PersonCertification certification,
        IngestCertificationLifecycleRequest request,
        string lifecycleAction,
        CancellationToken cancellationToken)
    {
        Guid? trainingBlockerId = null;
        if (string.Equals(lifecycleAction, "suspend", StringComparison.OrdinalIgnoreCase))
        {
            var blocker = await db.PersonTrainingBlockers.AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.TenantId == request.TenantId
                        && x.TrainarrPublicationId == request.TrainarrLifecyclePublicationId,
                    cancellationToken);
            trainingBlockerId = blocker?.Id;
        }

        return new CertificationLifecycleIngestionResponse(
            certification.Id,
            request.TrainarrGrantPublicationId,
            request.TrainarrLifecyclePublicationId,
            lifecycleAction,
            PersonCertificationEffectiveStatus.Resolve(certification),
            trainingBlockerId);
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

    private static string NormalizeLifecycleAction(string lifecycleAction)
    {
        var normalized = lifecycleAction.Trim().ToLowerInvariant();
        if (!AllowedLifecycleActions.Contains(normalized))
        {
            throw new StlApiException(
                "certification_lifecycle.validation",
                $"Lifecycle action must be one of: {string.Join(", ", AllowedLifecycleActions.OrderBy(x => x))}.",
                400);
        }

        return normalized;
    }

    private static string NormalizeQualificationKey(string qualificationKey)
    {
        var normalized = qualificationKey.Trim().ToLowerInvariant();
        if (normalized.Length < 3 || normalized.Length > 128)
        {
            throw new StlApiException(
                "certification_lifecycle.validation",
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
                "certification_lifecycle.validation",
                "Qualification name must be between 3 and 128 characters.",
                400);
        }

        return trimmed;
    }

    private static string NormalizeMessage(string message)
    {
        var trimmed = message.Trim();
        if (trimmed.Length < 16 || trimmed.Length > 1024)
        {
            throw new StlApiException(
                "certification_lifecycle.validation",
                "Lifecycle message must be between 16 and 1024 characters.",
                400);
        }

        return trimmed;
    }

    private static string AppendLifecycleNote(string? existingNotes, string lifecycleAction, string message)
    {
        var lifecycleNote = $"TrainArr {lifecycleAction}: {message}";
        if (string.IsNullOrWhiteSpace(existingNotes))
        {
            return lifecycleNote.Length <= 1024 ? lifecycleNote : lifecycleNote[..1024];
        }

        var combined = $"{existingNotes.Trim()} | {lifecycleNote}";
        return combined.Length <= 1024 ? combined : combined[..1024];
    }

    private async Task ClearSuspendedTrainingBlockersAsync(
        Guid tenantId,
        Guid personId,
        string qualificationKey,
        CancellationToken cancellationToken)
    {
        var blockers = await db.PersonTrainingBlockers
            .Where(x => x.TenantId == tenantId
                        && x.PersonId == personId
                        && x.Status == "active"
                        && x.BlockerType == "suspended"
                        && x.QualificationKey == qualificationKey)
            .ToListAsync(cancellationToken);

        if (blockers.Count == 0)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        foreach (var blocker in blockers)
        {
            blocker.Status = "cleared";
            blocker.ClearedAt = now;
            blocker.UpdatedAt = now;
        }
    }
}
