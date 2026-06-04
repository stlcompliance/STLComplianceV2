using Microsoft.EntityFrameworkCore;

using ComplianceCore.Api.Contracts;

using ComplianceCore.Api.Data;

using ComplianceCore.Api.Entities;

using STLCompliance.Shared.Contracts;



namespace ComplianceCore.Api.Services;



public sealed class RulePackService(

    ComplianceCoreDbContext db,

    IComplianceCoreAuditService auditService,

    RuleChangeMonitoringService ruleChangeMonitoring)

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

        await ruleChangeMonitoring.RecordVersionCreatedAsync(
            tenantId,
            actorUserId,
            entity,
            program.ProgramKey,
            cancellationToken);

        return MapResponse(entity, program);

    }

    public async Task<RulePackResponse> GetAsync(
        Guid tenantId,
        Guid rulePackId,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.RulePacks.AsNoTracking().FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == rulePackId && x.IsActive,
            cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("rule_packs.not_found", "Rule pack was not found.", 404);
        }

        var program = await db.RegulatoryPrograms.AsNoTracking()
            .FirstAsync(x => x.Id == entity.RegulatoryProgramId, cancellationToken);
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

        var previousStatus = entity.Status;

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

        await ruleChangeMonitoring.RecordStatusChangedAsync(
            tenantId,
            actorUserId,
            entity,
            program.ProgramKey,
            previousStatus,
            status,
            cancellationToken);

        return MapResponse(entity, program);

    }

    public async Task<RulePackResponse> PatchAsync(
        Guid tenantId,
        Guid? actorUserId,
        Guid rulePackId,
        PatchRulePackRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.RulePacks.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == rulePackId && x.IsActive,
            cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("rule_packs.not_found", "Rule pack was not found.", 404);
        }

        var changed = false;
        if (request.Label is not null)
        {
            entity.Label = GoverningBodyService.NormalizeLabel(request.Label, "rule_packs.validation", "Label");
            changed = true;
        }

        if (request.Description is not null)
        {
            entity.Description = GoverningBodyService.NormalizeDescription(request.Description, "rule_packs.validation");
            changed = true;
        }

        if (request.Status is not null)
        {
            var nextStatus = NormalizeStatus(request.Status);
            if (!string.Equals(entity.Status, nextStatus, StringComparison.OrdinalIgnoreCase))
            {
                ValidateStatusTransition(entity.Status, nextStatus);
                entity.Status = nextStatus;
                changed = true;
            }
        }

        if (changed)
        {
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
            await auditService.WriteAsync(
                "rule_pack.patch",
                tenantId,
                actorUserId,
                "rule_pack",
                entity.Id.ToString(),
                "success",
                cancellationToken: cancellationToken);
        }

        var program = await db.RegulatoryPrograms.AsNoTracking()
            .FirstAsync(x => x.Id == entity.RegulatoryProgramId, cancellationToken);
        return MapResponse(entity, program);
    }

    public async Task<RulePackResponse> CloneAsync(
        Guid tenantId,
        Guid? actorUserId,
        Guid sourceRulePackId,
        CloneRulePackRequest request,
        CancellationToken cancellationToken = default)
    {
        var source = await db.RulePacks.AsNoTracking().FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == sourceRulePackId && x.IsActive,
            cancellationToken);
        if (source is null)
        {
            throw new StlApiException("rule_packs.not_found", "Rule pack was not found.", 404);
        }

        var baseCloneKey = $"{source.PackKey}_clone";
        var chosenKey = request.PackKey?.Trim();
        if (string.IsNullOrWhiteSpace(chosenKey))
        {
            chosenKey = baseCloneKey;
            var exists = await db.RulePacks.AnyAsync(
                x => x.TenantId == tenantId && x.PackKey == chosenKey,
                cancellationToken);
            if (exists)
            {
                chosenKey = $"{baseCloneKey}_{Guid.NewGuid():N}"[..Math.Min(32, baseCloneKey.Length + 9)];
            }
        }

        var created = await CreateAsync(
            tenantId,
            actorUserId,
            new CreateRulePackRequest(
                source.RegulatoryProgramId,
                chosenKey!,
                request.Label ?? $"{source.Label} (Clone)",
                request.Description ?? source.Description),
            cancellationToken);

        if (request.CopyContent && !string.IsNullOrWhiteSpace(source.RuleContentJson))
        {
            var cloned = await db.RulePacks.FirstAsync(
                x => x.TenantId == tenantId && x.Id == created.RulePackId && x.IsActive,
                cancellationToken);
            cloned.RuleContentJson = source.RuleContentJson;
            cloned.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
        }

        await auditService.WriteAsync(
            "rule_pack.clone",
            tenantId,
            actorUserId,
            "rule_pack",
            created.RulePackId.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, created.RulePackId, cancellationToken);
    }

    public async Task<RulePackDiffResponse> DiffAsync(
        Guid tenantId,
        Guid baseRulePackId,
        Guid? compareRulePackId,
        CancellationToken cancellationToken = default)
    {
        var basePack = await db.RulePacks.AsNoTracking().FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == baseRulePackId && x.IsActive,
            cancellationToken);
        if (basePack is null)
        {
            throw new StlApiException("rule_packs.not_found", "Rule pack was not found.", 404);
        }

        RulePack? comparePack;
        if (compareRulePackId.HasValue)
        {
            comparePack = await db.RulePacks.AsNoTracking().FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == compareRulePackId.Value && x.IsActive,
                cancellationToken);
        }
        else
        {
            comparePack = await db.RulePacks.AsNoTracking()
                .Where(x =>
                    x.TenantId == tenantId
                    && x.PackKey == basePack.PackKey
                    && x.VersionNumber < basePack.VersionNumber
                    && x.IsActive)
                .OrderByDescending(x => x.VersionNumber)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (comparePack is null)
        {
            throw new StlApiException(
                "rule_packs.diff_compare_not_found",
                "Comparison rule pack was not found.",
                404);
        }

        var baseHash = RuleChangeHash.Compute(basePack.RuleContentJson);
        var compareHash = RuleChangeHash.Compute(comparePack.RuleContentJson);
        var metadataChanged =
            !string.Equals(basePack.Label, comparePack.Label, StringComparison.Ordinal)
            || !string.Equals(basePack.Description, comparePack.Description, StringComparison.Ordinal)
            || !string.Equals(basePack.Status, comparePack.Status, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(basePack.PackKey, comparePack.PackKey, StringComparison.Ordinal);
        var contentChanged = !string.Equals(baseHash, compareHash, StringComparison.Ordinal);

        var baseContent = ParseContentOrEmpty(basePack.RuleContentJson);
        var compareContent = ParseContentOrEmpty(comparePack.RuleContentJson);
        var baseRulesByKey = baseContent.Rules.ToDictionary(x => x.RuleKey, StringComparer.OrdinalIgnoreCase);
        var compareRulesByKey = compareContent.Rules.ToDictionary(x => x.RuleKey, StringComparer.OrdinalIgnoreCase);
        var ruleKeys = baseRulesByKey.Keys
            .Union(compareRulesByKey.Keys, StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var ruleChanges = new List<RulePackRuleDiffResponse>();
        foreach (var ruleKey in ruleKeys)
        {
            var hasBase = baseRulesByKey.TryGetValue(ruleKey, out var baseRule);
            var hasCompare = compareRulesByKey.TryGetValue(ruleKey, out var compareRule);

            if (hasBase && hasCompare)
            {
                var modified =
                    !string.Equals(baseRule.Label, compareRule.Label, StringComparison.Ordinal)
                    || !string.Equals(baseRule.FactKey, compareRule.FactKey, StringComparison.Ordinal)
                    || baseRule.ExpectedValue != compareRule.ExpectedValue
                    || baseRule.NonWaivable != compareRule.NonWaivable
                    || baseRule.RemediationRequired != compareRule.RemediationRequired
                    || baseRule.ReviewRequired != compareRule.ReviewRequired;

                if (modified)
                {
                    ruleChanges.Add(new RulePackRuleDiffResponse(
                        ruleKey,
                        "modified",
                        baseRule.Label,
                        compareRule.Label,
                        baseRule.FactKey,
                        compareRule.FactKey,
                        baseRule.ExpectedValue,
                        compareRule.ExpectedValue,
                        baseRule.NonWaivable,
                        compareRule.NonWaivable,
                        baseRule.RemediationRequired,
                        compareRule.RemediationRequired,
                        baseRule.ReviewRequired,
                        compareRule.ReviewRequired));
                }
            }
            else if (hasBase)
            {
                ruleChanges.Add(new RulePackRuleDiffResponse(
                    ruleKey,
                    "removed",
                    baseRule.Label,
                    null,
                    baseRule.FactKey,
                    null,
                    baseRule.ExpectedValue,
                    null,
                    baseRule.NonWaivable,
                    null,
                    baseRule.RemediationRequired,
                    null,
                    baseRule.ReviewRequired,
                    null));
            }
            else if (hasCompare)
            {
                ruleChanges.Add(new RulePackRuleDiffResponse(
                    ruleKey,
                    "added",
                    null,
                    compareRule.Label,
                    null,
                    compareRule.FactKey,
                    null,
                    compareRule.ExpectedValue,
                    null,
                    compareRule.NonWaivable,
                    null,
                    compareRule.RemediationRequired,
                    null,
                    compareRule.ReviewRequired));
            }
        }

        return new RulePackDiffResponse(
            basePack.Id,
            comparePack.Id,
            basePack.VersionNumber,
            comparePack.VersionNumber,
            basePack.Status,
            comparePack.Status,
            metadataChanged,
            contentChanged,
            baseHash,
            compareHash,
            ruleChanges.Count(x => x.ChangeType == "added"),
            ruleChanges.Count(x => x.ChangeType == "removed"),
            ruleChanges.Count(x => x.ChangeType == "modified"),
            ruleChanges);
    }



    public async Task<RulePackResponse> RestorePublishedVersionForRollbackAsync(

        Guid tenantId,

        Guid? actorUserId,

        Guid rulePackId,

        CancellationToken cancellationToken = default)

    {

        var entity = await db.RulePacks.FirstOrDefaultAsync(

            x => x.TenantId == tenantId && x.Id == rulePackId && x.IsActive,

            cancellationToken);

        if (entity is null)

        {

            throw new StlApiException("rule_packs.not_found", "Rule pack was not found.", 404);

        }



        if (!string.Equals(entity.Status, RulePackStatuses.Archived, StringComparison.OrdinalIgnoreCase))

        {

            throw new StlApiException(

                "rule_packs.invalid_rollback_restore",

                "Only archived rule pack versions can be restored during rollback.",

                409);

        }



        var previousStatus = entity.Status;

        entity.Status = RulePackStatuses.Published;

        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);



        await auditService.WriteAsync(

            "rule_pack.rollback.restore",

            tenantId,

            actorUserId,

            "rule_pack",

            entity.Id.ToString(),

            "success",

            reasonCode: RulePackStatuses.Published,

            cancellationToken: cancellationToken);

        var program = await db.RegulatoryPrograms.AsNoTracking()

            .FirstAsync(x => x.Id == entity.RegulatoryProgramId, cancellationToken);

        await ruleChangeMonitoring.RecordStatusChangedAsync(

            tenantId,

            actorUserId,

            entity,

            program.ProgramKey,

            previousStatus,

            RulePackStatuses.Published,

            cancellationToken);

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



    private static RulePackContentBody ParseContentOrEmpty(string? contentJson)

    {

        if (string.IsNullOrWhiteSpace(contentJson))

        {

            return new RulePackContentBody(1, "all", []);

        }



        return RuleEvaluator.ParseContent(contentJson);

    }



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


