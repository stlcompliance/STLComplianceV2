using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Services;

public sealed class TrainingMatrixService(TrainArrDbContext db, ITrainArrAuditService audit)
{
    private static readonly HashSet<string> AllowedLevels = new(StringComparer.OrdinalIgnoreCase)
    {
        "required",
        "recommended",
    };

    public async Task<TrainingMatrixViewResponse> GetViewAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var entries = await ListEntriesAsync(tenantId, cancellationToken);
        var keys = entries
            .Select(x => x.ApplicabilityKey)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();
        return new TrainingMatrixViewResponse(keys, entries);
    }

    public async Task<IReadOnlyList<TrainingMatrixEntryResponse>> ListEntriesAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await db.TrainingMatrixEntries
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.ApplicabilityKey)
            .ThenBy(x => x.SortOrder)
            .Select(x => new TrainingMatrixEntryResponse(
                x.Id,
                x.ApplicabilityKey,
                x.ApplicabilityLabel,
                x.TrainingProgramId,
                x.TrainingProgram != null ? x.TrainingProgram.Name : null,
                x.TrainingDefinitionId,
                x.TrainingDefinition != null ? x.TrainingDefinition.Name : null,
                x.RequirementLevel,
                x.SortOrder,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<TrainingMatrixEntryResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateTrainingMatrixEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        var applicabilityKey = NormalizeApplicabilityKey(request.ApplicabilityKey);
        var applicabilityLabel = NormalizeApplicabilityLabel(request.ApplicabilityLabel);
        var requirementLevel = NormalizeRequirementLevel(request.RequirementLevel);
        await ValidateTargetsAsync(tenantId, request.TrainingProgramId, request.TrainingDefinitionId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var entry = new TrainingMatrixEntry
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ApplicabilityKey = applicabilityKey,
            ApplicabilityLabel = applicabilityLabel,
            TrainingProgramId = request.TrainingProgramId,
            TrainingDefinitionId = request.TrainingDefinitionId,
            RequirementLevel = requirementLevel,
            SortOrder = request.SortOrder,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.TrainingMatrixEntries.Add(entry);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "training_matrix.create",
            tenantId,
            actorUserId,
            "training_matrix_entry",
            entry.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await MapEntryAsync(tenantId, entry.Id, cancellationToken);
    }

    public async Task<TrainingMatrixEntryResponse> UpdateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid matrixEntryId,
        UpdateTrainingMatrixEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        var entry = await db.TrainingMatrixEntries.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == matrixEntryId,
            cancellationToken);
        if (entry is null)
        {
            throw new StlApiException("training_matrix.not_found", "Training matrix entry was not found.", 404);
        }

        entry.ApplicabilityLabel = NormalizeApplicabilityLabel(request.ApplicabilityLabel);
        entry.RequirementLevel = NormalizeRequirementLevel(request.RequirementLevel);
        entry.SortOrder = request.SortOrder;
        entry.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "training_matrix.update",
            tenantId,
            actorUserId,
            "training_matrix_entry",
            entry.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await MapEntryAsync(tenantId, entry.Id, cancellationToken);
    }

    public async Task DeleteAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid matrixEntryId,
        CancellationToken cancellationToken = default)
    {
        var entry = await db.TrainingMatrixEntries.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == matrixEntryId,
            cancellationToken);
        if (entry is null)
        {
            throw new StlApiException("training_matrix.not_found", "Training matrix entry was not found.", 404);
        }

        db.TrainingMatrixEntries.Remove(entry);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "training_matrix.delete",
            tenantId,
            actorUserId,
            "training_matrix_entry",
            entry.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);
    }

    private async Task<TrainingMatrixEntryResponse> MapEntryAsync(
        Guid tenantId,
        Guid entryId,
        CancellationToken cancellationToken)
    {
        var mapped = await db.TrainingMatrixEntries
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Id == entryId)
            .Select(x => new TrainingMatrixEntryResponse(
                x.Id,
                x.ApplicabilityKey,
                x.ApplicabilityLabel,
                x.TrainingProgramId,
                x.TrainingProgram != null ? x.TrainingProgram.Name : null,
                x.TrainingDefinitionId,
                x.TrainingDefinition != null ? x.TrainingDefinition.Name : null,
                x.RequirementLevel,
                x.SortOrder,
                x.CreatedAt,
                x.UpdatedAt))
            .FirstAsync(cancellationToken);
        return mapped;
    }

    private async Task ValidateTargetsAsync(
        Guid tenantId,
        Guid? trainingProgramId,
        Guid? trainingDefinitionId,
        CancellationToken cancellationToken)
    {
        if (trainingProgramId is null && trainingDefinitionId is null)
        {
            throw new StlApiException(
                "training_matrix.validation",
                "A matrix entry must reference a training program or definition.",
                400);
        }

        if (trainingProgramId is not null && trainingDefinitionId is not null)
        {
            throw new StlApiException(
                "training_matrix.validation",
                "A matrix entry cannot reference both a program and a definition.",
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

    private static string NormalizeApplicabilityKey(string applicabilityKey)
    {
        var normalized = applicabilityKey.Trim().ToLowerInvariant();
        if (normalized.Length < 2 || normalized.Length > 64)
        {
            throw new StlApiException(
                "training_matrix.validation",
                "Applicability key must be between 2 and 64 characters.",
                400);
        }

        return normalized;
    }

    private static string NormalizeApplicabilityLabel(string applicabilityLabel)
    {
        var trimmed = applicabilityLabel.Trim();
        if (trimmed.Length < 2 || trimmed.Length > 128)
        {
            throw new StlApiException(
                "training_matrix.validation",
                "Applicability label must be between 2 and 128 characters.",
                400);
        }

        return trimmed;
    }

    private static string NormalizeRequirementLevel(string requirementLevel)
    {
        var normalized = requirementLevel.Trim().ToLowerInvariant();
        if (!AllowedLevels.Contains(normalized))
        {
            throw new StlApiException(
                "training_matrix.validation",
                $"Requirement level must be one of: {string.Join(", ", AllowedLevels.OrderBy(x => x))}.",
                400);
        }

        return normalized;
    }
}
