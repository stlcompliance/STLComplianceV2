using Microsoft.EntityFrameworkCore;

using TrainArr.Api.Contracts;

using TrainArr.Api.Data;

using TrainArr.Api.Entities;

using STLCompliance.Shared.Contracts;



namespace TrainArr.Api.Services;



public sealed class TrainingProgramService(
    TrainArrDbContext db,
    ITrainArrAuditService audit,
    TrainingProgramVersionService programVersionService)

{

    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)

    {

        "draft",

        "published"

    };



    public async Task<IReadOnlyList<TrainingProgramSummaryResponse>> ListAsync(

        Guid tenantId,

        CancellationToken cancellationToken = default)

    {

        return await db.TrainingPrograms

            .AsNoTracking()

            .Where(x => x.TenantId == tenantId)

            .OrderByDescending(x => x.UpdatedAt)

            .Select(x => new TrainingProgramSummaryResponse(

                x.Id,

                x.ProgramKey,

                x.Name,

                x.Status,

                x.ProgramDefinitions.Count,

                db.TrainingProgramVersions.Count(v =>
                    v.TenantId == x.TenantId &&
                    v.TrainingProgramId == x.Id &&
                    v.Status == "published"),

                x.CreatedAt,

                x.UpdatedAt))

            .ToListAsync(cancellationToken);

    }



    public async Task<TrainingProgramDetailResponse> GetAsync(

        Guid tenantId,

        Guid programId,

        CancellationToken cancellationToken = default)

    {

        var program = await LoadProgramAsync(tenantId, programId, cancellationToken);

        return MapDetail(program);

    }



    public async Task<TrainingProgramDetailResponse> CreateAsync(

        Guid tenantId,

        Guid actorUserId,

        CreateTrainingProgramRequest request,

        CancellationToken cancellationToken = default)

    {

        var programKey = NormalizeProgramKey(request.ProgramKey);

        var name = NormalizeName(request.Name);

        var description = NormalizeDescription(request.Description);



        var exists = await db.TrainingPrograms.AnyAsync(

            x => x.TenantId == tenantId && x.ProgramKey == programKey,

            cancellationToken);

        if (exists)

        {

            throw new StlApiException(

                "training_programs.duplicate",

                "A training program with this key already exists.",

                409);

        }



        var definitions = await LoadActiveDefinitionsAsync(tenantId, request.TrainingDefinitionIds, cancellationToken);

        var now = DateTimeOffset.UtcNow;

        var program = new TrainingProgram

        {

            Id = Guid.NewGuid(),

            TenantId = tenantId,

            ProgramKey = programKey,

            Name = name,

            Description = description,

            Status = "draft",

            CreatedAt = now,

            UpdatedAt = now,

            ProgramDefinitions = definitions

                .Select((definition, index) => new TrainingProgramDefinition

                {

                    TrainingProgramId = default,

                    TrainingDefinitionId = definition.Id,

                    SortOrder = index

                })

                .ToList()

        };



        foreach (var link in program.ProgramDefinitions)

        {

            link.TrainingProgramId = program.Id;

        }



        db.TrainingPrograms.Add(program);

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(

            "training_program.create",

            tenantId,

            actorUserId,

            "training_program",

            program.Id.ToString(),

            "Succeeded",

            cancellationToken: cancellationToken);



        return MapDetail(await LoadProgramAsync(tenantId, program.Id, cancellationToken));

    }



    public async Task<TrainingProgramDetailResponse> UpdateAsync(

        Guid tenantId,

        Guid actorUserId,

        Guid programId,

        UpdateTrainingProgramRequest request,

        CancellationToken cancellationToken = default)

    {

        var program = await LoadProgramAsync(tenantId, programId, cancellationToken, tracking: true);

        var previousStatus = program.Status;
        var status = NormalizeStatus(request.Status);

        var definitions = await LoadActiveDefinitionsAsync(tenantId, request.TrainingDefinitionIds, cancellationToken);

        program.Name = NormalizeName(request.Name);

        program.Description = NormalizeDescription(request.Description);

        program.Status = status;

        program.UpdatedAt = DateTimeOffset.UtcNow;



        db.TrainingProgramDefinitions.RemoveRange(program.ProgramDefinitions);

        program.ProgramDefinitions = definitions

            .Select((definition, index) => new TrainingProgramDefinition

            {

                TrainingProgramId = program.Id,

                TrainingDefinitionId = definition.Id,

                SortOrder = index

            })

            .ToList();



        await db.SaveChangesAsync(cancellationToken);

        if (status == "published" &&
            !string.Equals(previousStatus, "published", StringComparison.OrdinalIgnoreCase))
        {
            var publishedProgram = await LoadProgramAsync(tenantId, program.Id, cancellationToken);
            await programVersionService.SnapshotPublishedVersionAsync(
                tenantId,
                actorUserId,
                publishedProgram,
                cancellationToken);
        }

        await audit.WriteAsync(

            status == "published" ? "training_program.publish" : "training_program.update",

            tenantId,

            actorUserId,

            "training_program",

            program.Id.ToString(),

            "Succeeded",

            cancellationToken: cancellationToken);



        return MapDetail(await LoadProgramAsync(tenantId, program.Id, cancellationToken));

    }



    private async Task<TrainingProgram> LoadProgramAsync(

        Guid tenantId,

        Guid programId,

        CancellationToken cancellationToken,

        bool tracking = false)

    {

        var query = db.TrainingPrograms

            .Include(x => x.ProgramDefinitions)

            .ThenInclude(x => x.TrainingDefinition)

            .Include(x => x.ContentReferences)

            .Where(x => x.TenantId == tenantId && x.Id == programId);



        var program = tracking

            ? await query.FirstOrDefaultAsync(cancellationToken)

            : await query.AsNoTracking().FirstOrDefaultAsync(cancellationToken);



        if (program is null)

        {

            throw new StlApiException("training_programs.not_found", "Training program was not found.", 404);

        }



        return program;

    }



    private async Task<IReadOnlyList<TrainingDefinition>> LoadActiveDefinitionsAsync(

        Guid tenantId,

        IReadOnlyList<Guid> trainingDefinitionIds,

        CancellationToken cancellationToken)

    {

        if (trainingDefinitionIds.Count == 0)

        {

            throw new StlApiException(

                "training_programs.validation",

                "At least one training definition is required.",

                400);

        }



        var distinctIds = trainingDefinitionIds.Distinct().ToList();

        var definitions = await db.TrainingDefinitions

            .Where(x => x.TenantId == tenantId && x.Status == "active" && distinctIds.Contains(x.Id))

            .ToListAsync(cancellationToken);



        if (definitions.Count != distinctIds.Count)

        {

            throw new StlApiException(

                "training_programs.definitions_not_found",

                "One or more training definitions were not found.",

                400);

        }



        return distinctIds

            .Select(id => definitions.Single(d => d.Id == id))

            .ToList();

    }



    public static TrainingProgramDetailResponse MapDetailFromEntity(TrainingProgram program) =>
        MapDetail(program);

    private static TrainingProgramDetailResponse MapDetail(TrainingProgram program) =>

        new(

            program.Id,

            program.ProgramKey,

            program.Name,

            program.Description,

            program.Status,

            program.ProgramDefinitions

                .OrderBy(x => x.SortOrder)

                .Select(x => new TrainingProgramDefinitionLinkResponse(

                    x.TrainingDefinitionId,

                    x.TrainingDefinition.DefinitionKey,

                    x.TrainingDefinition.Name,

                    x.SortOrder))

                .ToList(),

            program.ContentReferences

                .OrderBy(x => x.CreatedAt)

                .Select(x => new TrainingProgramContentReferenceResponse(

                    x.Id,

                    x.TrainingProgramId,

                    x.ContentType,

                    x.Title,

                    x.ReferenceValue,

                    x.Notes,

                    x.LocaleTag,

                    x.CreatedByUserId,

                    x.CreatedAt))

                .ToList(),

            program.CreatedAt,

            program.UpdatedAt);



    private static string NormalizeProgramKey(string programKey)

    {

        var normalized = programKey.Trim().ToLowerInvariant();

        if (normalized.Length < 3 || normalized.Length > 64)

        {

            throw new StlApiException(

                "training_programs.validation",

                "Program key must be between 3 and 64 characters.",

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

                "training_programs.validation",

                "Program name must be between 3 and 128 characters.",

                400);

        }



        return trimmed;

    }



    private static string NormalizeDescription(string description)

    {

        var trimmed = description.Trim();

        if (trimmed.Length < 8 || trimmed.Length > 2048)

        {

            throw new StlApiException(

                "training_programs.validation",

                "Program description must be between 8 and 2048 characters.",

                400);

        }



        return trimmed;

    }



    private static string NormalizeStatus(string status)

    {

        var normalized = status.Trim().ToLowerInvariant();

        if (!AllowedStatuses.Contains(normalized))

        {

            throw new StlApiException(

                "training_programs.validation",

                $"Program status must be one of: {string.Join(", ", AllowedStatuses.OrderBy(x => x))}.",

                400);

        }



        return normalized;

    }

}


