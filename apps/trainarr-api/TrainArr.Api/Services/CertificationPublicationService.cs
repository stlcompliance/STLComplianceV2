using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Services;

public sealed class CertificationPublicationService(
    TrainArrDbContext db,
    StaffarrPublicationRetryService staffarrPublicationRetryService)
{
    private static readonly HashSet<string> AllowedBlockerTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "missing_assignment",
        "overdue",
        "failed",
        "suspended"
    };

    public async Task<CertificationPublicationResponse> PublishTrainingBlockerAsync(
        CreateCertificationPublicationRequest request,
        CancellationToken cancellationToken = default)
    {
        var qualificationKey = NormalizeQualificationKey(request.QualificationKey);
        var qualificationName = NormalizeQualificationName(request.QualificationName);
        var blockerType = NormalizeBlockerType(request.BlockerType);
        var message = NormalizeMessage(request.Message);
        var now = DateTimeOffset.UtcNow;
        var publicationId = Guid.NewGuid();

        var entity = new CertificationPublication
        {
            Id = publicationId,
            TenantId = request.TenantId,
            StaffarrPersonId = request.StaffarrPersonId,
            QualificationKey = qualificationKey,
            QualificationName = qualificationName,
            PublicationType = "training_blocker",
            BlockerType = blockerType,
            Message = message,
            Status = "published",
            ExpiresAt = request.ExpiresAt,
            PublishedAt = now,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.CertificationPublications.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        var payload = new StaffArrIngestTrainingBlockerPayload(
            request.TenantId,
            request.StaffarrPersonId,
            publicationId,
            qualificationKey,
            qualificationName,
            blockerType,
            message,
            request.ExpiresAt);

        await staffarrPublicationRetryService.EnqueueAndAttemptAsync(
            request.TenantId,
            publicationId,
            request.StaffarrPersonId,
            StaffarrPublicationOperationKinds.TrainingBlockerPublish,
            StaffarrPublicationRetryService.SerializePayload(payload),
            cancellationToken);

        return MapResponse(entity);
    }

    public async Task ClearTrainingBlockerAsync(
        Guid tenantId,
        Guid staffarrPersonId,
        Guid publicationId,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.CertificationPublications.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == publicationId && x.StaffarrPersonId == staffarrPersonId,
            cancellationToken);
        if (entity is null)
        {
            throw new StlApiException(
                "publications.not_found",
                "Certification publication was not found.",
                404);
        }

        if (string.Equals(entity.Status, "cleared", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        entity.Status = "cleared";
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        var payload = new StaffArrClearTrainingBlockerPayload(tenantId, staffarrPersonId, publicationId);
        await staffarrPublicationRetryService.EnqueueAndAttemptAsync(
            tenantId,
            publicationId,
            staffarrPersonId,
            StaffarrPublicationOperationKinds.TrainingBlockerClear,
            StaffarrPublicationRetryService.SerializePayload(payload),
            cancellationToken);
    }

    public async Task<CertificationPublicationResponse> PublishQualificationGrantAsync(
        PublishQualificationGrantRequest request,
        CancellationToken cancellationToken = default)
    {
        var qualificationKey = NormalizeQualificationKey(request.QualificationKey);
        var qualificationName = NormalizeQualificationName(request.QualificationName);
        var message = NormalizeGrantMessage(request.Notes, request.TrainingDefinitionName);
        var now = DateTimeOffset.UtcNow;
        var publicationId = Guid.NewGuid();

        var entity = new CertificationPublication
        {
            Id = publicationId,
            TenantId = request.TenantId,
            StaffarrPersonId = request.StaffarrPersonId,
            QualificationKey = qualificationKey,
            QualificationName = qualificationName,
            PublicationType = "qualification_grant",
            BlockerType = "issued",
            Message = message,
            Status = "published",
            ExpiresAt = request.ExpiresAt,
            PublishedAt = now,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.CertificationPublications.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        var payload = new StaffArrIngestCertificationGrantPayload(
            request.TenantId,
            request.StaffarrPersonId,
            publicationId,
            request.TrainingAssignmentId,
            qualificationKey,
            qualificationName,
            request.TrainingDefinitionName,
            now,
            request.ExpiresAt,
            request.Notes);

        await staffarrPublicationRetryService.EnqueueAndAttemptAsync(
            request.TenantId,
            publicationId,
            request.StaffarrPersonId,
            StaffarrPublicationOperationKinds.QualificationGrant,
            StaffarrPublicationRetryService.SerializePayload(payload),
            cancellationToken);

        return MapResponse(entity);
    }

    public Task<CertificationPublicationResponse> PublishQualificationSuspendAsync(
        PublishQualificationLifecycleRequest request,
        CancellationToken cancellationToken = default) =>
        PublishQualificationLifecycleAsync(request, "qualification_suspend", "suspended", cancellationToken);

    public Task<CertificationPublicationResponse> PublishQualificationRevokeAsync(
        PublishQualificationLifecycleRequest request,
        CancellationToken cancellationToken = default) =>
        PublishQualificationLifecycleAsync(request, "qualification_revoke", "revoked", cancellationToken);

    public Task<CertificationPublicationResponse> PublishQualificationExpireAsync(
        PublishQualificationLifecycleRequest request,
        CancellationToken cancellationToken = default) =>
        PublishQualificationLifecycleAsync(request, "qualification_expire", "expired", cancellationToken);

    private async Task<CertificationPublicationResponse> PublishQualificationLifecycleAsync(
        PublishQualificationLifecycleRequest request,
        string publicationType,
        string blockerType,
        CancellationToken cancellationToken)
    {
        var qualificationKey = NormalizeQualificationKey(request.QualificationKey);
        var qualificationName = NormalizeQualificationName(request.QualificationName);
        var message = NormalizeLifecycleMessage(request.Message);
        var now = DateTimeOffset.UtcNow;
        var publicationId = Guid.NewGuid();

        var entity = new CertificationPublication
        {
            Id = publicationId,
            TenantId = request.TenantId,
            StaffarrPersonId = request.StaffarrPersonId,
            QualificationKey = qualificationKey,
            QualificationName = qualificationName,
            PublicationType = publicationType,
            BlockerType = blockerType,
            Message = message,
            Status = "published",
            PublishedAt = now,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.CertificationPublications.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        var payload = new StaffArrIngestCertificationLifecyclePayload(
            request.TenantId,
            request.StaffarrPersonId,
            request.GrantPublicationId,
            publicationId,
            request.LifecycleAction,
            qualificationKey,
            qualificationName,
            message,
            null);

        await staffarrPublicationRetryService.EnqueueAndAttemptAsync(
            request.TenantId,
            publicationId,
            request.StaffarrPersonId,
            StaffarrPublicationOperationKinds.QualificationLifecycle,
            StaffarrPublicationRetryService.SerializePayload(payload),
            cancellationToken);

        return MapResponse(entity);
    }

    private static CertificationPublicationResponse MapResponse(CertificationPublication entity) =>
        new(
            entity.Id,
            entity.TenantId,
            entity.StaffarrPersonId,
            entity.QualificationKey,
            entity.QualificationName,
            entity.BlockerType,
            entity.Message,
            entity.Status,
            entity.PublishedAt);

    private static string NormalizeQualificationKey(string qualificationKey)
    {
        var normalized = qualificationKey.Trim().ToLowerInvariant();
        if (normalized.Length < 3 || normalized.Length > 128)
        {
            throw new StlApiException(
                "publications.validation",
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
                "publications.validation",
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
                "publications.validation",
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
                "publications.validation",
                "Blocker message must be between 16 and 1024 characters.",
                400);
        }

        return trimmed;
    }

    private static string NormalizeGrantMessage(string? notes, string trainingDefinitionName)
    {
        if (!string.IsNullOrWhiteSpace(notes))
        {
            var trimmed = notes.Trim();
            if (trimmed.Length >= 16 && trimmed.Length <= 1024)
            {
                return trimmed;
            }
        }

        var fallback = $"Qualification granted via TrainArr completion of {trainingDefinitionName}.";
        return fallback.Length >= 16 ? fallback : $"{fallback} Recorded.";
    }

    private static string NormalizeLifecycleMessage(string message)
    {
        var trimmed = message.Trim();
        if (trimmed.Length < 16 || trimmed.Length > 1024)
        {
            throw new StlApiException(
                "publications.validation",
                "Lifecycle message must be between 16 and 1024 characters.",
                400);
        }

        return trimmed;
    }
}
