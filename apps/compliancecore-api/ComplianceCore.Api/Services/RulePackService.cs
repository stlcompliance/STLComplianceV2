using Microsoft.EntityFrameworkCore;

using ComplianceCore.Api.Contracts;

using ComplianceCore.Api.Data;

using ComplianceCore.Api.Entities;

using STLCompliance.Shared.Contracts;



namespace ComplianceCore.Api.Services;



public sealed class RulePackService(

    ComplianceCoreDbContext db,

    IComplianceCoreAuditService auditService)

{

    public async Task<IReadOnlyList<RulePackResponse>> ListAsync(

        Guid tenantId,

        Guid? regulatoryProgramId = null,

        CancellationToken cancellationToken = default)

    {

        var query = db.RulePacks

            .AsNoTracking()

            .Where(x => x.TenantId == tenantId && x.IsActive);



        if (regulatoryProgramId.HasValue)

        {

            query = query.Where(x => x.RegulatoryProgramId == regulatoryProgramId.Value);

        }



        return await query

            .OrderByDescending(x => x.UpdatedAt)

            .Join(

                db.RegulatoryPrograms.AsNoTracking(),

                pack => pack.RegulatoryProgramId,

                program => program.Id,

                (pack, program) => new RulePackResponse(

                    pack.Id,

                    pack.RegulatoryProgramId,

                    program.ProgramKey,

                    program.Label,

                    pack.PackKey,

                    pack.Label,

                    pack.Description,

                    pack.VersionNumber,

                    pack.Status,

                    pack.IsActive,

                    pack.CreatedAt,

                    pack.UpdatedAt))

            .ToListAsync(cancellationToken);

    }



    public async Task<RulePackResponse> CreateAsync(

        Guid tenantId,

        Guid? actorUserId,

        CreateRulePackRequest request,

        CancellationToken cancellationToken = default)

    {

        var packKey = GoverningBodyService.NormalizeKey(

            request.PackKey,

            "rule_packs.validation",

            "Pack key");

        var label = GoverningBodyService.NormalizeLabel(request.Label, "rule_packs.validation", "Label");

        var description = GoverningBodyService.NormalizeDescription(request.Description, "rule_packs.validation");



        var program = await db.RegulatoryPrograms.FirstOrDefaultAsync(

            x => x.TenantId == tenantId && x.Id == request.RegulatoryProgramId && x.IsActive,

            cancellationToken);

        if (program is null)

        {

            throw new StlApiException("rule_packs.program_not_found", "Regulatory program was not found.", 404);

        }



        var latestVersion = await db.RulePacks

            .Where(x => x.TenantId == tenantId && x.PackKey == packKey)

            .Select(x => (int?)x.VersionNumber)

            .MaxAsync(cancellationToken) ?? 0;



        var now = DateTimeOffset.UtcNow;

        var entity = new RulePack

        {

            Id = Guid.NewGuid(),

            TenantId = tenantId,

            RegulatoryProgramId = request.RegulatoryProgramId,

            PackKey = packKey,

            Label = label,

            Description = description,

            VersionNumber = latestVersion + 1,

            Status = RulePackStatuses.Draft,

            IsActive = true,

            CreatedAt = now,

            UpdatedAt = now

        };



        db.RulePacks.Add(entity);

        await db.SaveChangesAsync(cancellationToken);



        await auditService.WriteAsync(

            "rule_pack.create",

            tenantId,

            actorUserId,

            "rule_pack",

            entity.Id.ToString(),

            "success",

            cancellationToken: cancellationToken);



        return MapResponse(entity, program);

    }



    public async Task<RulePackResponse> UpdateStatusAsync(

        Guid tenantId,

        Guid? actorUserId,

        Guid rulePackId,

        UpdateRulePackStatusRequest request,

        CancellationToken cancellationToken = default)

    {

        var status = NormalizeStatus(request.Status);



        var entity = await db.RulePacks.FirstOrDefaultAsync(

            x => x.TenantId == tenantId && x.Id == rulePackId && x.IsActive,

            cancellationToken);

        if (entity is null)

        {

            throw new StlApiException("rule_packs.not_found", "Rule pack was not found.", 404);

        }



        if (string.Equals(entity.Status, status, StringComparison.OrdinalIgnoreCase))

        {

            var existingProgram = await db.RegulatoryPrograms.AsNoTracking()

                .FirstAsync(x => x.Id == entity.RegulatoryProgramId, cancellationToken);

            return MapResponse(entity, existingProgram);

        }



        ValidateStatusTransition(entity.Status, status);



        entity.Status = status;

        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);



        await auditService.WriteAsync(

            "rule_pack.status.update",

            tenantId,

            actorUserId,

            "rule_pack",

            entity.Id.ToString(),

            "success",

            reasonCode: status,

            cancellationToken: cancellationToken);



        var program = await db.RegulatoryPrograms.AsNoTracking()

            .FirstAsync(x => x.Id == entity.RegulatoryProgramId, cancellationToken);

        return MapResponse(entity, program);

    }



    private static RulePackResponse MapResponse(RulePack entity, RegulatoryProgram program) =>

        new(

            entity.Id,

            entity.RegulatoryProgramId,

            program.ProgramKey,

            program.Label,

            entity.PackKey,

            entity.Label,

            entity.Description,

            entity.VersionNumber,

            entity.Status,

            entity.IsActive,

            entity.CreatedAt,

            entity.UpdatedAt);



    private static string NormalizeStatus(string status)

    {

        var normalized = status.Trim().ToLowerInvariant();

        if (!RulePackStatuses.All.Contains(normalized))

        {

            throw new StlApiException(

                "rule_packs.validation",

                "Rule pack status is not recognized.",

                400);

        }



        return normalized;

    }



    private static void ValidateStatusTransition(string currentStatus, string nextStatus)

    {

        if (string.Equals(currentStatus, RulePackStatuses.Archived, StringComparison.OrdinalIgnoreCase))

        {

            throw new StlApiException(

                "rule_packs.invalid_transition",

                "Archived rule packs cannot change status.",

                409);

        }



        if (string.Equals(currentStatus, RulePackStatuses.Published, StringComparison.OrdinalIgnoreCase)

            && !string.Equals(nextStatus, RulePackStatuses.Archived, StringComparison.OrdinalIgnoreCase))

        {

            throw new StlApiException(

                "rule_packs.invalid_transition",

                "Published rule packs can only be archived.",

                409);

        }



        if (string.Equals(nextStatus, RulePackStatuses.Published, StringComparison.OrdinalIgnoreCase)

            && !string.Equals(currentStatus, RulePackStatuses.Review, StringComparison.OrdinalIgnoreCase))

        {

            throw new StlApiException(

                "rule_packs.invalid_transition",

                "Rule packs must be in review before publishing.",

                409);

        }

    }



    public async Task<IReadOnlyList<InternalRulePackLookupItem>> LookupByPackKeysAsync(

        Guid tenantId,

        IReadOnlyList<string> rulePackKeys,

        CancellationToken cancellationToken = default)

    {

        if (rulePackKeys.Count == 0)

        {

            return Array.Empty<InternalRulePackLookupItem>();

        }



        var normalizedKeys = rulePackKeys

            .Select(key => key.Trim().ToLowerInvariant())

            .Where(key => key.Length > 0)

            .Distinct(StringComparer.Ordinal)

            .ToList();



        if (normalizedKeys.Count == 0)

        {

            return Array.Empty<InternalRulePackLookupItem>();

        }



        return await db.RulePacks

            .AsNoTracking()

            .Where(x => x.TenantId == tenantId && normalizedKeys.Contains(x.PackKey))

            .Join(

                db.RegulatoryPrograms.AsNoTracking(),

                pack => pack.RegulatoryProgramId,

                program => program.Id,

                (pack, program) => new InternalRulePackLookupItem(

                    pack.PackKey,

                    pack.Label,

                    pack.Description,

                    program.ProgramKey,

                    program.Label,

                    pack.VersionNumber,

                    pack.Status,

                    pack.IsActive))

            .ToListAsync(cancellationToken);

    }

}


