using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Services;

public sealed class TrainingApplicabilityProfileService(TrainArrDbContext db, ITrainArrAuditService audit)
{
    private static readonly HashSet<string> AllowedScopeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        TrainingApplicabilityScopeTypes.RoleTemplate,
        TrainingApplicabilityScopeTypes.OrgUnit,
        TrainingApplicabilityScopeTypes.JobCode,
        TrainingApplicabilityScopeTypes.Site,
        TrainingApplicabilityScopeTypes.Custom,
    };

    public async Task<IReadOnlyList<TrainingApplicabilityProfileResponse>> ListAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await db.TrainingApplicabilityProfiles
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.ScopeType)
            .ThenBy(x => x.Label)
            .Select(MapProjection)
            .ToListAsync(cancellationToken);
    }

    public async Task<TrainingApplicabilityProfileResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateTrainingApplicabilityProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var scopeType = NormalizeScopeType(request.ScopeType);
        var scopeKey = NormalizeScopeKey(request.ScopeKey);
        var label = NormalizeLabel(request.Label);
        var profileKey = BuildProfileKey(scopeType, scopeKey);

        var duplicate = await db.TrainingApplicabilityProfiles.AnyAsync(
            x => x.TenantId == tenantId && x.ProfileKey == profileKey,
            cancellationToken);
        if (duplicate)
        {
            throw new StlApiException(
                "training_applicability.validation",
                "An applicability profile with this scope already exists.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        var profile = new TrainingApplicabilityProfile
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProfileKey = profileKey,
            Label = label,
            Description = NormalizeOptionalDescription(request.Description),
            ScopeType = scopeType,
            ScopeKey = scopeKey,
            SourceProduct = NormalizeOptionalSourceProduct(request.SourceProduct),
            SourceRecordId = NormalizeOptionalSourceRecordId(request.SourceRecordId),
            SourceUpdatedAt = request.SourceRecordId is not null ? now : null,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.TrainingApplicabilityProfiles.Add(profile);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "training_applicability.create",
            tenantId,
            actorUserId,
            "training_applicability_profile",
            profile.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return Map(profile);
    }

    public async Task<TrainingApplicabilityProfileResponse> UpdateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid profileId,
        UpdateTrainingApplicabilityProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var profile = await db.TrainingApplicabilityProfiles.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == profileId,
            cancellationToken);
        if (profile is null)
        {
            throw new StlApiException(
                "training_applicability.not_found",
                "Applicability profile was not found.",
                404);
        }

        profile.Label = NormalizeLabel(request.Label);
        profile.Description = NormalizeOptionalDescription(request.Description);
        profile.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "training_applicability.update",
            tenantId,
            actorUserId,
            "training_applicability_profile",
            profile.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return Map(profile);
    }

    public async Task DeleteAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid profileId,
        CancellationToken cancellationToken = default)
    {
        var profile = await db.TrainingApplicabilityProfiles.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == profileId,
            cancellationToken);
        if (profile is null)
        {
            throw new StlApiException(
                "training_applicability.not_found",
                "Applicability profile was not found.",
                404);
        }

        var inUse = await db.TrainingRequirements.AnyAsync(
            x => x.TenantId == tenantId && x.ApplicabilityProfileId == profileId,
            cancellationToken);
        if (inUse)
        {
            throw new StlApiException(
                "training_applicability.in_use",
                "Remove training requirements linked to this profile before deleting it.",
                409);
        }

        db.TrainingApplicabilityProfiles.Remove(profile);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "training_applicability.delete",
            tenantId,
            actorUserId,
            "training_applicability_profile",
            profile.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);
    }

    internal static string BuildProfileKey(string scopeType, string scopeKey) =>
        $"{scopeType}:{scopeKey}";

    private static TrainingApplicabilityProfileResponse Map(TrainingApplicabilityProfile profile) =>
        new(
            profile.Id,
            profile.ProfileKey,
            profile.Label,
            profile.Description,
            profile.ScopeType,
            profile.ScopeKey,
            profile.SourceProduct,
            profile.SourceRecordId,
            profile.SourceUpdatedAt,
            profile.CreatedAt,
            profile.UpdatedAt);

    private static readonly System.Linq.Expressions.Expression<
        Func<TrainingApplicabilityProfile, TrainingApplicabilityProfileResponse>> MapProjection =
        x => new TrainingApplicabilityProfileResponse(
            x.Id,
            x.ProfileKey,
            x.Label,
            x.Description,
            x.ScopeType,
            x.ScopeKey,
            x.SourceProduct,
            x.SourceRecordId,
            x.SourceUpdatedAt,
            x.CreatedAt,
            x.UpdatedAt);

    private static string NormalizeScopeType(string scopeType)
    {
        var normalized = scopeType.Trim().ToLowerInvariant();
        if (!AllowedScopeTypes.Contains(normalized))
        {
            throw new StlApiException(
                "training_applicability.validation",
                $"Scope type must be one of: {string.Join(", ", AllowedScopeTypes.OrderBy(x => x))}.",
                400);
        }

        return normalized;
    }

    private static string NormalizeScopeKey(string scopeKey)
    {
        var normalized = scopeKey.Trim().ToLowerInvariant();
        if (normalized.Length < 2 || normalized.Length > 64)
        {
            throw new StlApiException(
                "training_applicability.validation",
                "Scope key must be between 2 and 64 characters.",
                400);
        }

        return normalized;
    }

    private static string NormalizeLabel(string label)
    {
        var trimmed = label.Trim();
        if (trimmed.Length < 2 || trimmed.Length > 128)
        {
            throw new StlApiException(
                "training_applicability.validation",
                "Label must be between 2 and 128 characters.",
                400);
        }

        return trimmed;
    }

    private static string? NormalizeOptionalDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        var trimmed = description.Trim();
        if (trimmed.Length > 512)
        {
            throw new StlApiException(
                "training_applicability.validation",
                "Description must be 512 characters or fewer.",
                400);
        }

        return trimmed;
    }

    private static string? NormalizeOptionalSourceProduct(string? sourceProduct)
    {
        if (string.IsNullOrWhiteSpace(sourceProduct))
        {
            return null;
        }

        return sourceProduct.Trim();
    }

    private static string? NormalizeOptionalSourceRecordId(string? sourceRecordId)
    {
        if (string.IsNullOrWhiteSpace(sourceRecordId))
        {
            return null;
        }

        return sourceRecordId.Trim();
    }
}
