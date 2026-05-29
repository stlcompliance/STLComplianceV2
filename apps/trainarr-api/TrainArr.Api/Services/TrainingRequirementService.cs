using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Services;

public sealed class TrainingRequirementService(
    TrainArrDbContext db,
    ITrainArrAuditService audit,
    TrainingMatrixService matrixService)
{
    private static readonly HashSet<string> AllowedSources = new(StringComparer.OrdinalIgnoreCase)
    {
        TrainingRequirementSources.Internal,
        TrainingRequirementSources.RulePack,
        TrainingRequirementSources.Citation,
    };

    private static readonly HashSet<string> AllowedLevels = new(StringComparer.OrdinalIgnoreCase)
    {
        "required",
        "recommended",
    };

    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "active",
        "inactive",
    };

    public async Task<TrainingRequirementBuilderViewResponse> GetBuilderViewAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var profiles = await db.TrainingApplicabilityProfiles
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.ScopeType)
            .ThenBy(x => x.Label)
            .Select(x => new TrainingApplicabilityProfileResponse(
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
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        var requirements = await ListAsync(tenantId, cancellationToken);
        return new TrainingRequirementBuilderViewResponse(profiles, requirements);
    }

    public async Task<IReadOnlyList<TrainingRequirementResponse>> ListAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await db.TrainingRequirements
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Label)
            .Select(MapProjection)
            .ToListAsync(cancellationToken);
    }

    public async Task<TrainingRequirementResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateTrainingRequirementRequest request,
        CancellationToken cancellationToken = default)
    {
        var requirementKey = NormalizeRequirementKey(request.RequirementKey);
        var label = NormalizeLabel(request.Label);
        var requirementSource = NormalizeRequirementSource(request.RequirementSource);
        var requirementLevel = NormalizeRequirementLevel(request.RequirementLevel);
        await ValidateTargetsAsync(tenantId, request.TrainingProgramId, request.TrainingDefinitionId, cancellationToken);
        await ValidateApplicabilityProfileAsync(tenantId, request.ApplicabilityProfileId, cancellationToken);

        var duplicate = await db.TrainingRequirements.AnyAsync(
            x => x.TenantId == tenantId && x.RequirementKey == requirementKey,
            cancellationToken);
        if (duplicate)
        {
            throw new StlApiException(
                "training_requirements.validation",
                "A requirement with this key already exists.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        var requirement = new TrainingRequirement
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RequirementKey = requirementKey,
            Label = label,
            Description = NormalizeOptionalDescription(request.Description),
            RequirementSource = requirementSource,
            SourceKey = NormalizeOptionalSourceKey(request.SourceKey),
            TrainingProgramId = request.TrainingProgramId,
            TrainingDefinitionId = request.TrainingDefinitionId,
            ApplicabilityProfileId = request.ApplicabilityProfileId,
            RequirementLevel = requirementLevel,
            SortOrder = request.SortOrder,
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.TrainingRequirements.Add(requirement);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "training_requirements.create",
            tenantId,
            actorUserId,
            "training_requirement",
            requirement.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await MapEntryAsync(tenantId, requirement.Id, cancellationToken);
    }

    public async Task<TrainingRequirementResponse> UpdateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid requirementId,
        UpdateTrainingRequirementRequest request,
        CancellationToken cancellationToken = default)
    {
        var requirement = await db.TrainingRequirements.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == requirementId,
            cancellationToken);
        if (requirement is null)
        {
            throw new StlApiException("training_requirements.not_found", "Training requirement was not found.", 404);
        }

        requirement.Label = NormalizeLabel(request.Label);
        requirement.Description = NormalizeOptionalDescription(request.Description);
        requirement.RequirementLevel = NormalizeRequirementLevel(request.RequirementLevel);
        requirement.SortOrder = request.SortOrder;
        requirement.Status = NormalizeStatus(request.Status);
        requirement.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "training_requirements.update",
            tenantId,
            actorUserId,
            "training_requirement",
            requirement.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await MapEntryAsync(tenantId, requirement.Id, cancellationToken);
    }

    public async Task DeleteAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid requirementId,
        CancellationToken cancellationToken = default)
    {
        var requirement = await db.TrainingRequirements.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == requirementId,
            cancellationToken);
        if (requirement is null)
        {
            throw new StlApiException("training_requirements.not_found", "Training requirement was not found.", 404);
        }

        db.TrainingRequirements.Remove(requirement);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "training_requirements.delete",
            tenantId,
            actorUserId,
            "training_requirement",
            requirement.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);
    }

    public async Task<SyncRequirementToMatrixResponse> SyncToMatrixAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid requirementId,
        CancellationToken cancellationToken = default)
    {
        var requirement = await db.TrainingRequirements
            .AsNoTracking()
            .Include(x => x.ApplicabilityProfile)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == requirementId, cancellationToken);
        if (requirement is null)
        {
            throw new StlApiException("training_requirements.not_found", "Training requirement was not found.", 404);
        }

        if (requirement.ApplicabilityProfile is null)
        {
            throw new StlApiException(
                "training_requirements.validation",
                "Link an applicability profile before syncing to the training matrix.",
                400);
        }

        if (requirement.TrainingProgramId is null && requirement.TrainingDefinitionId is null)
        {
            throw new StlApiException(
                "training_requirements.validation",
                "Requirement must target a program or definition before syncing to the matrix.",
                400);
        }

        var profile = requirement.ApplicabilityProfile;
        var matrixEntry = await matrixService.CreateAsync(
            tenantId,
            actorUserId,
            new CreateTrainingMatrixEntryRequest(
                profile.ScopeKey,
                profile.Label,
                requirement.TrainingProgramId,
                requirement.TrainingDefinitionId,
                requirement.RequirementLevel,
                requirement.SortOrder),
            cancellationToken);

        return new SyncRequirementToMatrixResponse(
            requirementId,
            matrixEntry.MatrixEntryId,
            matrixEntry.ApplicabilityKey);
    }

    private async Task<TrainingRequirementResponse> MapEntryAsync(
        Guid tenantId,
        Guid requirementId,
        CancellationToken cancellationToken)
    {
        return await db.TrainingRequirements
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Id == requirementId)
            .Select(MapProjection)
            .FirstAsync(cancellationToken);
    }

    private static readonly System.Linq.Expressions.Expression<
        Func<TrainingRequirement, TrainingRequirementResponse>> MapProjection =
        x => new TrainingRequirementResponse(
            x.Id,
            x.RequirementKey,
            x.Label,
            x.Description,
            x.RequirementSource,
            x.SourceKey,
            x.TrainingProgramId,
            x.TrainingProgram != null ? x.TrainingProgram.Name : null,
            x.TrainingDefinitionId,
            x.TrainingDefinition != null ? x.TrainingDefinition.Name : null,
            x.ApplicabilityProfileId,
            x.ApplicabilityProfile != null ? x.ApplicabilityProfile.ProfileKey : null,
            x.ApplicabilityProfile != null ? x.ApplicabilityProfile.Label : null,
            x.RequirementLevel,
            x.SortOrder,
            x.Status,
            x.CreatedAt,
            x.UpdatedAt);

    private async Task ValidateTargetsAsync(
        Guid tenantId,
        Guid? trainingProgramId,
        Guid? trainingDefinitionId,
        CancellationToken cancellationToken)
    {
        if (trainingProgramId is null && trainingDefinitionId is null)
        {
            throw new StlApiException(
                "training_requirements.validation",
                "A requirement must reference a training program or definition.",
                400);
        }

        if (trainingProgramId is not null && trainingDefinitionId is not null)
        {
            throw new StlApiException(
                "training_requirements.validation",
                "A requirement cannot reference both a program and a definition.",
                400);
        }

        if (trainingProgramId is { } programId)
        {
            var programExists = await db.TrainingPrograms.AnyAsync(
                x => x.TenantId == tenantId && x.Id == programId,
                cancellationToken);
            if (!programExists)
            {
                throw new StlApiException("training_programs.not_found", "Training program was not found.", 404);
            }
        }

        if (trainingDefinitionId is { } definitionId)
        {
            var definitionExists = await db.TrainingDefinitions.AnyAsync(
                x => x.TenantId == tenantId && x.Id == definitionId && x.Status == "active",
                cancellationToken);
            if (!definitionExists)
            {
                throw new StlApiException(
                    "training_definitions.not_found",
                    "Training definition was not found.",
                    404);
            }
        }
    }

    private async Task ValidateApplicabilityProfileAsync(
        Guid tenantId,
        Guid? applicabilityProfileId,
        CancellationToken cancellationToken)
    {
        if (applicabilityProfileId is null)
        {
            return;
        }

        var exists = await db.TrainingApplicabilityProfiles.AnyAsync(
            x => x.TenantId == tenantId && x.Id == applicabilityProfileId,
            cancellationToken);
        if (!exists)
        {
            throw new StlApiException(
                "training_applicability.not_found",
                "Applicability profile was not found.",
                404);
        }
    }

    private static string NormalizeRequirementKey(string requirementKey)
    {
        var normalized = requirementKey.Trim().ToLowerInvariant();
        if (normalized.Length < 2 || normalized.Length > 64)
        {
            throw new StlApiException(
                "training_requirements.validation",
                "Requirement key must be between 2 and 64 characters.",
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
                "training_requirements.validation",
                "Label must be between 2 and 128 characters.",
                400);
        }

        return trimmed;
    }

    private static string NormalizeRequirementSource(string requirementSource)
    {
        var normalized = requirementSource.Trim().ToLowerInvariant();
        if (!AllowedSources.Contains(normalized))
        {
            throw new StlApiException(
                "training_requirements.validation",
                $"Requirement source must be one of: {string.Join(", ", AllowedSources.OrderBy(x => x))}.",
                400);
        }

        return normalized;
    }

    private static string NormalizeRequirementLevel(string requirementLevel)
    {
        var normalized = requirementLevel.Trim().ToLowerInvariant();
        if (!AllowedLevels.Contains(normalized))
        {
            throw new StlApiException(
                "training_requirements.validation",
                $"Requirement level must be one of: {string.Join(", ", AllowedLevels.OrderBy(x => x))}.",
                400);
        }

        return normalized;
    }

    private static string NormalizeStatus(string status)
    {
        var normalized = status.Trim().ToLowerInvariant();
        if (!AllowedStatuses.Contains(normalized))
        {
            throw new StlApiException(
                "training_requirements.validation",
                $"Status must be one of: {string.Join(", ", AllowedStatuses.OrderBy(x => x))}.",
                400);
        }

        return normalized;
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
                "training_requirements.validation",
                "Description must be 512 characters or fewer.",
                400);
        }

        return trimmed;
    }

    private static string? NormalizeOptionalSourceKey(string? sourceKey)
    {
        if (string.IsNullOrWhiteSpace(sourceKey))
        {
            return null;
        }

        return sourceKey.Trim();
    }
}
