using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Services;

public sealed class TrainingDefinitionService(TrainArrDbContext db)
{
    public async Task<IReadOnlyList<TrainingDefinitionResponse>> ListAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await db.TrainingDefinitions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Status == "active")
            .OrderBy(x => x.Name)
            .Select(x => MapResponse(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<TrainingDefinitionResponse> CreateAsync(
        Guid tenantId,
        CreateTrainingDefinitionRequest request,
        CancellationToken cancellationToken = default)
    {
        var definitionKey = NormalizeDefinitionKey(request.DefinitionKey);
        var name = NormalizeName(request.Name);
        var description = NormalizeDescription(request.Description);
        var qualificationKey = NormalizeQualificationKey(request.QualificationKey);
        var qualificationName = NormalizeQualificationName(request.QualificationName);

        var exists = await db.TrainingDefinitions.AnyAsync(
            x => x.TenantId == tenantId && x.DefinitionKey == definitionKey,
            cancellationToken);
        if (exists)
        {
            throw new StlApiException(
                "training_definitions.duplicate",
                "A training definition with this key already exists.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new TrainingDefinition
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            DefinitionKey = definitionKey,
            Name = name,
            Description = description,
            QualificationKey = qualificationKey,
            QualificationName = qualificationName,
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now
        };

        db.TrainingDefinitions.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return MapResponse(entity);
    }

    public async Task<TrainingDefinition> GetActiveDefinitionAsync(
        Guid tenantId,
        Guid trainingDefinitionId,
        CancellationToken cancellationToken = default)
    {
        var definition = await db.TrainingDefinitions.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == trainingDefinitionId && x.Status == "active",
            cancellationToken);
        if (definition is null)
        {
            throw new StlApiException(
                "training_definitions.not_found",
                "Training definition was not found.",
                404);
        }

        return definition;
    }

    private static TrainingDefinitionResponse MapResponse(TrainingDefinition entity) =>
        new(
            entity.Id,
            entity.DefinitionKey,
            entity.Name,
            entity.Description,
            entity.QualificationKey,
            entity.QualificationName,
            entity.Status,
            entity.CreatedAt);

    private static string NormalizeDefinitionKey(string definitionKey)
    {
        var normalized = definitionKey.Trim().ToLowerInvariant();
        if (normalized.Length < 3 || normalized.Length > 64)
        {
            throw new StlApiException(
                "training_definitions.validation",
                "Definition key must be between 3 and 64 characters.",
                400);
        }

        return normalized;
    }

    private static string NormalizeName(string name)
    {
        var trimmed = name.Trim();
        if (trimmed.Length < 3 || trimmed.Length > 128)
        {
            throw new StlApiException(
                "training_definitions.validation",
                "Definition name must be between 3 and 128 characters.",
                400);
        }

        return trimmed;
    }

    private static string NormalizeDescription(string description)
    {
        var trimmed = description.Trim();
        if (trimmed.Length < 8 || trimmed.Length > 1024)
        {
            throw new StlApiException(
                "training_definitions.validation",
                "Definition description must be between 8 and 1024 characters.",
                400);
        }

        return trimmed;
    }

    private static string NormalizeQualificationKey(string qualificationKey)
    {
        var normalized = qualificationKey.Trim().ToLowerInvariant();
        if (normalized.Length < 3 || normalized.Length > 128)
        {
            throw new StlApiException(
                "training_definitions.validation",
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
                "training_definitions.validation",
                "Qualification name must be between 3 and 128 characters.",
                400);
        }

        return trimmed;
    }
}
