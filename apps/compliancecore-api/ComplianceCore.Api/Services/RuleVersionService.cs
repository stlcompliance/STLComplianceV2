using Microsoft.EntityFrameworkCore;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed class RuleVersionService(
    ComplianceCoreDbContext db,
    RulePackService rulePackService)
{
    public async Task<RuleVersionListResponse> ListAsync(
        Guid tenantId,
        string? packKey,
        CancellationToken cancellationToken = default)
    {
        var query = db.RulePacks.AsNoTracking().Where(x => x.TenantId == tenantId && x.IsActive);
        if (!string.IsNullOrWhiteSpace(packKey))
        {
            var normalizedPackKey = packKey.Trim().ToLowerInvariant();
            query = query.Where(x => x.PackKey == normalizedPackKey);
        }

        var items = await query
            .OrderBy(x => x.PackKey)
            .ThenByDescending(x => x.VersionNumber)
            .Join(
                db.RegulatoryPrograms.AsNoTracking(),
                pack => pack.RegulatoryProgramId,
                program => program.Id,
                (pack, program) => MapResponse(pack, program))
            .ToListAsync(cancellationToken);

        return new RuleVersionListResponse(items);
    }

    public async Task<RuleVersionListResponse> ListForRulePackIdAsync(
        Guid tenantId,
        Guid rulePackId,
        CancellationToken cancellationToken = default)
    {
        var packKey = await db.RulePacks
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Id == rulePackId && x.IsActive)
            .Select(x => x.PackKey)
            .FirstOrDefaultAsync(cancellationToken);
        if (packKey is null)
        {
            throw new StlApiException("rule_versions.not_found", "Rule version was not found.", 404);
        }

        return await ListAsync(tenantId, packKey, cancellationToken);
    }

    public async Task<RuleVersionResponse> PublishAsync(
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
            throw new StlApiException("rule_versions.not_found", "Rule version was not found.", 404);
        }

        if (!string.Equals(entity.Status, RulePackStatuses.Review, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "rule_versions.invalid_publish",
                "Only rule versions in review can be published through the rule-versions operator flow.",
                409);
        }

        var otherPublished = await db.RulePacks
            .Where(x =>
                x.TenantId == tenantId
                && x.PackKey == entity.PackKey
                && x.Id != rulePackId
                && x.IsActive
                && x.Status == RulePackStatuses.Published)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        foreach (var otherId in otherPublished)
        {
            await rulePackService.UpdateStatusAsync(
                tenantId,
                actorUserId,
                otherId,
                new UpdateRulePackStatusRequest(RulePackStatuses.Archived),
                cancellationToken);
        }

        var published = await rulePackService.UpdateStatusAsync(
            tenantId,
            actorUserId,
            rulePackId,
            new UpdateRulePackStatusRequest(RulePackStatuses.Published),
            cancellationToken);

        return MapResponse(published);
    }

    public async Task<RuleVersionRollbackResponse> RollbackAsync(
        Guid tenantId,
        Guid? actorUserId,
        Guid rulePackId,
        CancellationToken cancellationToken = default)
    {
        var current = await db.RulePacks.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == rulePackId && x.IsActive,
            cancellationToken);
        if (current is null)
        {
            throw new StlApiException("rule_versions.not_found", "Rule version was not found.", 404);
        }

        if (!string.Equals(current.Status, RulePackStatuses.Published, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "rule_versions.invalid_rollback",
                "Only the currently published rule version can be rolled back.",
                409);
        }

        if (current.VersionNumber <= 1)
        {
            throw new StlApiException(
                "rule_versions.no_prior_version",
                "No prior rule version exists to roll back to.",
                409);
        }

        var prior = await db.RulePacks.FirstOrDefaultAsync(
            x =>
                x.TenantId == tenantId
                && x.PackKey == current.PackKey
                && x.VersionNumber == current.VersionNumber - 1
                && x.IsActive,
            cancellationToken);
        if (prior is null)
        {
            throw new StlApiException(
                "rule_versions.prior_not_found",
                "The prior rule version could not be found.",
                404);
        }

        if (!string.Equals(prior.Status, RulePackStatuses.Archived, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "rule_versions.prior_not_archived",
                "The prior rule version must be archived before it can be restored.",
                409);
        }

        var archived = await rulePackService.UpdateStatusAsync(
            tenantId,
            actorUserId,
            current.Id,
            new UpdateRulePackStatusRequest(RulePackStatuses.Archived),
            cancellationToken);

        var restored = await rulePackService.RestorePublishedVersionForRollbackAsync(
            tenantId,
            actorUserId,
            prior.Id,
            cancellationToken);

        return new RuleVersionRollbackResponse(
            MapResponse(archived),
            MapResponse(restored));
    }

    private static RuleVersionResponse MapResponse(RulePack pack, Entities.RegulatoryProgram program) =>
        new(
            pack.Id,
            pack.PackKey,
            program.ProgramKey,
            program.Label,
            pack.VersionNumber,
            pack.Status,
            pack.IsActive,
            pack.CreatedAt,
            pack.UpdatedAt);

    private static RuleVersionResponse MapResponse(RulePackResponse pack) =>
        new(
            pack.RulePackId,
            pack.PackKey,
            pack.RegulatoryProgramKey,
            pack.RegulatoryProgramLabel,
            pack.VersionNumber,
            pack.Status,
            pack.IsActive,
            pack.CreatedAt,
            pack.UpdatedAt);
}
