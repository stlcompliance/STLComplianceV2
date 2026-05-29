using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Services;

public sealed class TrainingProgramVersionService(TrainArrDbContext db, ITrainArrAuditService audit)
{
    public async Task<IReadOnlyList<TrainingProgramVersionSummaryResponse>> ListForProgramAsync(
        Guid tenantId,
        Guid programId,
        CancellationToken cancellationToken = default)
    {
        await EnsureProgramExistsAsync(tenantId, programId, cancellationToken);

        return await db.TrainingProgramVersions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.TrainingProgramId == programId)
            .OrderByDescending(x => x.VersionNumber)
            .Select(x => new TrainingProgramVersionSummaryResponse(
                x.Id,
                x.TrainingProgramId,
                x.VersionNumber,
                x.Status,
                x.Name,
                x.VersionDefinitions.Count,
                x.PublishedAt,
                x.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<TrainingProgramVersionDetailResponse> GetAsync(
        Guid tenantId,
        Guid programVersionId,
        CancellationToken cancellationToken = default)
    {
        var version = await LoadVersionAsync(tenantId, programVersionId, cancellationToken);
        return MapDetail(version);
    }

    public async Task<TrainingProgramDetailResponse> StartRevisionAsync(
        Guid tenantId,
        Guid actorUserId,
        StartProgramRevisionRequest request,
        CancellationToken cancellationToken = default)
    {
        var program = await db.TrainingPrograms
            .Include(x => x.ProgramDefinitions)
            .ThenInclude(x => x.TrainingDefinition)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.TrainingProgramId, cancellationToken);

        if (program is null)
        {
            throw new StlApiException("training_programs.not_found", "Training program was not found.", 404);
        }

        if (!string.Equals(program.Status, "published", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "program_versions.validation",
                "Only published programs can start a new revision.",
                400);
        }

        program.Status = "draft";
        program.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "training_program.revision_start",
            tenantId,
            actorUserId,
            "training_program",
            program.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return TrainingProgramService.MapDetailFromEntity(program);
    }

    public async Task SnapshotPublishedVersionAsync(
        Guid tenantId,
        Guid actorUserId,
        TrainingProgram program,
        CancellationToken cancellationToken = default)
    {
        var nextVersion = await db.TrainingProgramVersions
            .Where(x => x.TenantId == tenantId && x.TrainingProgramId == program.Id)
            .Select(x => (int?)x.VersionNumber)
            .MaxAsync(cancellationToken) ?? 0;
        nextVersion += 1;

        var now = DateTimeOffset.UtcNow;
        var version = new TrainingProgramVersion
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TrainingProgramId = program.Id,
            VersionNumber = nextVersion,
            Status = "published",
            Name = program.Name,
            Description = program.Description,
            PublishedAt = now,
            PublishedByUserId = actorUserId,
            CreatedAt = now,
            VersionDefinitions = program.ProgramDefinitions
                .OrderBy(x => x.SortOrder)
                .Select(x => new TrainingProgramVersionDefinition
                {
                    TrainingProgramVersionId = default,
                    TrainingDefinitionId = x.TrainingDefinitionId,
                    SortOrder = x.SortOrder,
                })
                .ToList(),
        };

        foreach (var link in version.VersionDefinitions)
        {
            link.TrainingProgramVersionId = version.Id;
        }

        db.TrainingProgramVersions.Add(version);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "training_program_version.publish",
            tenantId,
            actorUserId,
            "training_program_version",
            version.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);
    }

    private async Task EnsureProgramExistsAsync(Guid tenantId, Guid programId, CancellationToken cancellationToken)
    {
        var exists = await db.TrainingPrograms.AnyAsync(
            x => x.TenantId == tenantId && x.Id == programId,
            cancellationToken);
        if (!exists)
        {
            throw new StlApiException("training_programs.not_found", "Training program was not found.", 404);
        }
    }

    private async Task<TrainingProgramVersion> LoadVersionAsync(
        Guid tenantId,
        Guid programVersionId,
        CancellationToken cancellationToken)
    {
        var version = await db.TrainingProgramVersions
            .AsNoTracking()
            .Include(x => x.TrainingProgram)
            .Include(x => x.VersionDefinitions)
            .ThenInclude(x => x.TrainingDefinition)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == programVersionId, cancellationToken);

        if (version is null)
        {
            throw new StlApiException("program_versions.not_found", "Program version was not found.", 404);
        }

        return version;
    }

    private static TrainingProgramVersionDetailResponse MapDetail(TrainingProgramVersion version) =>
        new(
            version.Id,
            version.TrainingProgramId,
            version.TrainingProgram.ProgramKey,
            version.VersionNumber,
            version.Status,
            version.Name,
            version.Description,
            version.VersionDefinitions
                .OrderBy(x => x.SortOrder)
                .Select(x => new TrainingProgramVersionDefinitionLinkResponse(
                    x.TrainingDefinitionId,
                    x.TrainingDefinition.DefinitionKey,
                    x.TrainingDefinition.Name,
                    x.SortOrder))
                .ToList(),
            version.PublishedAt,
            version.CreatedAt);
}
