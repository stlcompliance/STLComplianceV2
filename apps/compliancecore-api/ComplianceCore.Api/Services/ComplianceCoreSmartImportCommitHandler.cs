using System.Text.Json;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.SmartImport;

namespace ComplianceCore.Api.Services;

public sealed class ComplianceCoreSmartImportCommitHandler(ComplianceCoreDbContext db) : ISmartImportDestinationCommitHandler
{
    public string ProductKey => "compliancecore";

    public async Task<SmartImportDestinationCommitResponse> CommitAsync(
        string entityType,
        SmartImportDestinationCommitRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!SmartImportDestinationCommitResponses.IsCreateOperation(request.Operation))
        {
            return SmartImportDestinationCommitResponses.ReviewRequired(
                "compliancecore.smart_import.operation_not_supported",
                "ComplianceCore Smart Import commits currently support reviewed create operations only.");
        }

        if (entityType.Contains("citation", StringComparison.OrdinalIgnoreCase)
            || entityType.Contains("requirement", StringComparison.OrdinalIgnoreCase))
        {
            return await CommitCitationAsync(request, cancellationToken);
        }

        if (entityType.Contains("rule", StringComparison.OrdinalIgnoreCase)
            || entityType.Contains("pack", StringComparison.OrdinalIgnoreCase))
        {
            return await CommitRulePackAsync(request, cancellationToken);
        }

        return SmartImportDestinationCommitResponses.ReviewRequired(
            "compliancecore.smart_import.entity_type_not_supported",
            $"ComplianceCore does not have a Smart Import commit handler for entity type '{entityType}'.");
    }

    private async Task<SmartImportDestinationCommitResponse> CommitRulePackAsync(
        SmartImportDestinationCommitRequest request,
        CancellationToken cancellationToken)
    {
        var existing = await db.RulePacks.FirstOrDefaultAsync(
            pack => pack.TenantId == request.TenantId && pack.Id == request.CommitStepId,
            cancellationToken);
        if (existing is not null)
        {
            return Committed(existing.Id, existing.Label);
        }

        var payload = request.DeterministicPayload;
        var program = await ResolveRegulatoryProgramAsync(request.TenantId, payload, cancellationToken);
        if (program is null)
        {
            return MissingProgram();
        }

        var shortId = SmartImportPayloadReader.ShortId(request.CommitStepId);
        var label = SmartImportPayloadReader.DisplayName(payload, $"Imported rule pack {shortId}");
        var packKey = SmartImportPayloadReader.SlugKey(
            SmartImportPayloadReader.GetString(payload, "packKey", "rulePackKey", "key") ?? label,
            $"si_rule_pack_{shortId}",
            64);
        var version = SmartImportPayloadReader.GetInt(payload, 1, "versionNumber", "packVersion", "version");
        var duplicate = await db.RulePacks.FirstOrDefaultAsync(
            pack => pack.TenantId == request.TenantId && pack.PackKey == packKey && pack.VersionNumber == version,
            cancellationToken);
        if (duplicate is not null)
        {
            return Committed(duplicate.Id, duplicate.Label);
        }

        var now = DateTimeOffset.UtcNow;
        var rulePack = new RulePack
        {
            Id = request.CommitStepId,
            TenantId = request.TenantId,
            RegulatoryProgramId = program.Id,
            PackKey = packKey,
            Label = SmartImportPayloadReader.Truncate(label, 128),
            Description = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "description", "notes") ?? "Created as a draft by reviewed Smart Import commit.",
                1024),
            VersionNumber = version,
            Status = RulePackStatuses.Draft,
            IsActive = SmartImportPayloadReader.GetBool(payload, true, "isActive", "active"),
            RuleContentJson = NormalizeJson(SmartImportPayloadReader.GetString(payload, "ruleContentJson", "contentJson")),
            CreatedAt = now,
            UpdatedAt = now
        };

        db.RulePacks.Add(rulePack);
        AddAudit(request, "smart_import.rule_pack_draft_created", "rule_pack", rulePack.Id.ToString("D"), now);
        await db.SaveChangesAsync(cancellationToken);
        return Committed(rulePack.Id, rulePack.Label);
    }

    private async Task<SmartImportDestinationCommitResponse> CommitCitationAsync(
        SmartImportDestinationCommitRequest request,
        CancellationToken cancellationToken)
    {
        var existing = await db.RegulatoryCitations.FirstOrDefaultAsync(
            citation => citation.TenantId == request.TenantId && citation.Id == request.CommitStepId,
            cancellationToken);
        if (existing is not null)
        {
            return Committed(existing.Id, existing.Label);
        }

        var payload = request.DeterministicPayload;
        var program = await ResolveRegulatoryProgramAsync(request.TenantId, payload, cancellationToken);
        if (program is null)
        {
            return MissingProgram();
        }

        var shortId = SmartImportPayloadReader.ShortId(request.CommitStepId);
        var label = SmartImportPayloadReader.DisplayName(payload, $"Imported citation {shortId}");
        var citationKey = SmartImportPayloadReader.SlugKey(
            SmartImportPayloadReader.GetString(payload, "citationKey", "key", "sourceReference") ?? label,
            $"si_citation_{shortId}",
            64);
        var version = SmartImportPayloadReader.GetInt(payload, 1, "versionNumber", "citationVersion", "version");
        var duplicate = await db.RegulatoryCitations.FirstOrDefaultAsync(
            citation => citation.TenantId == request.TenantId
                && citation.CitationKey == citationKey
                && citation.VersionNumber == version,
            cancellationToken);
        if (duplicate is not null)
        {
            return Committed(duplicate.Id, duplicate.Label);
        }

        var rulePackId = await ResolveRulePackIdAsync(request.TenantId, payload, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var citationEntity = new RegulatoryCitation
        {
            Id = request.CommitStepId,
            TenantId = request.TenantId,
            RegulatoryProgramId = program.Id,
            RulePackId = rulePackId,
            CitationKey = citationKey,
            Label = SmartImportPayloadReader.Truncate(label, 128),
            SourceReference = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "sourceReference", "citation", "reference") ?? citationKey,
                256),
            Description = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "description", "notes") ?? "Created by reviewed Smart Import commit.",
                1024),
            VersionNumber = version,
            IsActive = SmartImportPayloadReader.GetBool(payload, true, "isActive", "active"),
            CreatedAt = now,
            UpdatedAt = now
        };

        db.RegulatoryCitations.Add(citationEntity);
        AddAudit(request, "smart_import.citation_created", "regulatory_citation", citationEntity.Id.ToString("D"), now);
        await db.SaveChangesAsync(cancellationToken);
        return Committed(citationEntity.Id, citationEntity.Label);
    }

    private async Task<RegulatoryProgram?> ResolveRegulatoryProgramAsync(
        Guid tenantId,
        JsonElement payload,
        CancellationToken cancellationToken)
    {
        var programId = SmartImportPayloadReader.GetGuid(payload, "regulatoryProgramId", "programId");
        if (programId is not null)
        {
            return await db.RegulatoryPrograms.FirstOrDefaultAsync(
                program => program.TenantId == tenantId && program.Id == programId.Value,
                cancellationToken);
        }

        var programKey = SmartImportPayloadReader.GetString(payload, "programKey", "regulatoryProgramKey");
        if (string.IsNullOrWhiteSpace(programKey))
        {
            return null;
        }

        return await db.RegulatoryPrograms.FirstOrDefaultAsync(
            program => program.TenantId == tenantId && program.ProgramKey == programKey,
            cancellationToken);
    }

    private async Task<Guid?> ResolveRulePackIdAsync(
        Guid tenantId,
        JsonElement payload,
        CancellationToken cancellationToken)
    {
        var rulePackId = SmartImportPayloadReader.GetGuid(payload, "rulePackId", "packId");
        if (rulePackId is not null)
        {
            var exists = await db.RulePacks.AnyAsync(
                pack => pack.TenantId == tenantId && pack.Id == rulePackId.Value,
                cancellationToken);
            return exists ? rulePackId.Value : null;
        }

        var packKey = SmartImportPayloadReader.GetString(payload, "packKey", "rulePackKey");
        if (string.IsNullOrWhiteSpace(packKey))
        {
            return null;
        }

        var version = SmartImportPayloadReader.GetInt(payload, 1, "packVersion", "rulePackVersion");
        return await db.RulePacks
            .Where(pack => pack.TenantId == tenantId && pack.PackKey == packKey && pack.VersionNumber == version)
            .Select(pack => (Guid?)pack.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private void AddAudit(
        SmartImportDestinationCommitRequest request,
        string action,
        string targetType,
        string targetId,
        DateTimeOffset occurredAt)
    {
        db.AuditEvents.Add(new ComplianceCoreAuditEvent
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            ActorUserId = request.ApprovedByPersonId,
            Action = action,
            TargetType = targetType,
            TargetId = targetId,
            Result = "success",
            ReasonCode = "smart_import",
            CorrelationId = request.CommitPlanId,
            OccurredAt = occurredAt
        });
    }

    private static string NormalizeJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return "{}";
        }

        try
        {
            using var parsed = JsonDocument.Parse(json);
            return parsed.RootElement.GetRawText();
        }
        catch (JsonException)
        {
            return "{}";
        }
    }

    private static SmartImportDestinationCommitResponse MissingProgram() =>
        SmartImportDestinationCommitResponses.ReviewRequired(
            "compliancecore.smart_import.regulatory_program_required",
            "ComplianceCore Smart Import commits require an existing regulatoryProgramId or programKey in the approved payload.");

    private static SmartImportDestinationCommitResponse Committed(Guid id, string displayName) =>
        SmartImportDestinationCommitResponses.Committed(id.ToString("D"), displayName);
}
