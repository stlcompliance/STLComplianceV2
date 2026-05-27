using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed class RuleContentService(
    ComplianceCoreDbContext db,
    IComplianceCoreAuditService auditService)
{
    public async Task<RulePackContentResponse> GetContentAsync(
        Guid tenantId,
        Guid rulePackId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadEditableRulePackAsync(tenantId, rulePackId, cancellationToken);
        return MapContentResponse(entity);
    }

    public async Task<RulePackContentResponse> UpdateContentAsync(
        Guid tenantId,
        Guid? actorUserId,
        Guid rulePackId,
        UpdateRulePackContentRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadEditableRulePackAsync(tenantId, rulePackId, cancellationToken);
        EnsureContentEditable(entity.Status);

        var serialized = RuleEvaluator.SerializeContent(request.Content);
        entity.RuleContentJson = serialized;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "rule_pack.content.update",
            tenantId,
            actorUserId,
            "rule_pack",
            entity.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return MapContentResponse(entity);
    }

    private async Task<RulePack> LoadEditableRulePackAsync(
        Guid tenantId,
        Guid rulePackId,
        CancellationToken cancellationToken)
    {
        var entity = await db.RulePacks.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == rulePackId && x.IsActive,
            cancellationToken);

        if (entity is null)
        {
            throw new StlApiException("rule_packs.not_found", "Rule pack was not found.", 404);
        }

        return entity;
    }

    private static void EnsureContentEditable(string status)
    {
        if (string.Equals(status, RulePackStatuses.Archived, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "rule_content.not_editable",
                "Archived rule packs cannot be edited.",
                409);
        }
    }

    private static RulePackContentResponse MapContentResponse(RulePack entity)
    {
        RulePackContentBody? content = null;
        if (!string.IsNullOrWhiteSpace(entity.RuleContentJson))
        {
            content = RuleEvaluator.ParseContent(entity.RuleContentJson);
        }

        return new RulePackContentResponse(
            entity.Id,
            entity.PackKey,
            entity.VersionNumber,
            entity.Status,
            content is not null,
            content,
            entity.UpdatedAt);
    }
}
