using System.Text.Json;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed class ComplianceExceptionExemptionService(
    ComplianceCoreDbContext db,
    IComplianceCoreAuditService auditService)
{
    public async Task<IReadOnlyList<ComplianceExceptionExemptionResponse>> ListAsync(
        Guid tenantId,
        string? type = null,
        string? packKey = null,
        string? citationKey = null,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var query = db.ComplianceExceptionExemptions.AsNoTracking()
            .Where(item => item.TenantId == tenantId);

        if (!includeInactive)
        {
            query = query.Where(item => item.Active);
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            var normalizedType = NormalizeType(type);
            query = query.Where(item => item.Type == normalizedType);
        }

        if (!string.IsNullOrWhiteSpace(packKey))
        {
            query = query.Where(item => item.PackKey == packKey.Trim());
        }

        if (!string.IsNullOrWhiteSpace(citationKey))
        {
            query = query.Where(item => item.CitationKey == citationKey.Trim());
        }

        return await query
            .OrderBy(item => item.Label)
            .ThenBy(item => item.Key)
            .Select(item => ToResponse(item))
            .ToListAsync(cancellationToken);
    }

    public async Task<ComplianceExceptionExemptionResponse> GetAsync(
        Guid tenantId,
        Guid exceptionExemptionId,
        CancellationToken cancellationToken = default) =>
        ToResponse(await RequireAsync(tenantId, exceptionExemptionId, cancellationToken));

    public async Task<ComplianceExceptionExemptionResponse> CreateAsync(
        Guid tenantId,
        Guid actorPersonId,
        CreateComplianceExceptionExemptionRequest request,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var key = NormalizeRequiredKey(request.Key, "Key");
        if (await db.ComplianceExceptionExemptions.AnyAsync(item => item.TenantId == tenantId && item.Key == key, cancellationToken))
        {
            throw new StlApiException("exception_exemptions.duplicate_key", "An exception/exemption with that key already exists.", 409);
        }

        var item = new ComplianceExceptionExemption
        {
            ExceptionExemptionId = Guid.NewGuid(),
            TenantId = tenantId,
            Key = key,
            Label = RequireText(request.Label, "Label", 256),
            Type = NormalizeType(request.Type),
            GoverningBody = NormalizeOptional(request.GoverningBody),
            ProgramKey = NormalizeOptional(request.ProgramKey),
            PackKey = NormalizeOptional(request.PackKey),
            CitationKey = NormalizeOptional(request.CitationKey),
            ApplicabilityKey = NormalizeOptional(request.ApplicabilityKey),
            AppliesToSubjectKind = NormalizeOptional(request.AppliesToSubjectKind),
            AppliesToSourceProduct = NormalizeOptional(request.AppliesToSourceProduct),
            AppliesToSourceEntity = NormalizeOptional(request.AppliesToSourceEntity),
            EffectType = NormalizeEffectType(request.EffectType),
            ConditionLogicJson = NormalizeJsonObject(request.ConditionLogicJson),
            RequiredEvidenceOptionGroupId = request.RequiredEvidenceOptionGroupId,
            IssuingAuthority = NormalizeOptional(request.IssuingAuthority),
            AuthorizationNumber = NormalizeOptional(request.AuthorizationNumber),
            EffectiveAt = request.EffectiveAt,
            ExpiresAt = request.ExpiresAt,
            Active = request.Active,
            Description = NormalizeOptional(request.Description, 2000),
            CreatedAt = now,
            UpdatedAt = now
        };

        ValidateDates(item.EffectiveAt, item.ExpiresAt);
        await ValidateEvidenceOptionGroupAsync(tenantId, item.RequiredEvidenceOptionGroupId, cancellationToken);

        db.ComplianceExceptionExemptions.Add(item);
        await db.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync(
            "exception_exemption.created",
            tenantId,
            actorPersonId,
            "exception_exemption",
            item.ExceptionExemptionId.ToString(),
            "success",
            reasonCode: item.Type,
            cancellationToken: cancellationToken);

        return ToResponse(item);
    }

    public async Task<ComplianceExceptionExemptionResponse> UpdateAsync(
        Guid tenantId,
        Guid exceptionExemptionId,
        Guid actorPersonId,
        UpdateComplianceExceptionExemptionRequest request,
        CancellationToken cancellationToken = default)
    {
        var item = await RequireAsync(tenantId, exceptionExemptionId, cancellationToken, tracking: true);

        if (!string.IsNullOrWhiteSpace(request.Label))
        {
            item.Label = RequireText(request.Label, "Label", 256);
        }

        if (!string.IsNullOrWhiteSpace(request.Type))
        {
            item.Type = NormalizeType(request.Type);
        }

        if (!string.IsNullOrWhiteSpace(request.EffectType))
        {
            item.EffectType = NormalizeEffectType(request.EffectType);
        }

        item.GoverningBody = request.GoverningBody is null ? item.GoverningBody : NormalizeOptional(request.GoverningBody);
        item.ProgramKey = request.ProgramKey is null ? item.ProgramKey : NormalizeOptional(request.ProgramKey);
        item.PackKey = request.PackKey is null ? item.PackKey : NormalizeOptional(request.PackKey);
        item.CitationKey = request.CitationKey is null ? item.CitationKey : NormalizeOptional(request.CitationKey);
        item.ApplicabilityKey = request.ApplicabilityKey is null ? item.ApplicabilityKey : NormalizeOptional(request.ApplicabilityKey);
        item.AppliesToSubjectKind = request.AppliesToSubjectKind is null ? item.AppliesToSubjectKind : NormalizeOptional(request.AppliesToSubjectKind);
        item.AppliesToSourceProduct = request.AppliesToSourceProduct is null ? item.AppliesToSourceProduct : NormalizeOptional(request.AppliesToSourceProduct);
        item.AppliesToSourceEntity = request.AppliesToSourceEntity is null ? item.AppliesToSourceEntity : NormalizeOptional(request.AppliesToSourceEntity);
        item.ConditionLogicJson = request.ConditionLogicJson is null ? item.ConditionLogicJson : NormalizeJsonObject(request.ConditionLogicJson);
        item.RequiredEvidenceOptionGroupId = request.RequiredEvidenceOptionGroupId ?? item.RequiredEvidenceOptionGroupId;
        item.IssuingAuthority = request.IssuingAuthority is null ? item.IssuingAuthority : NormalizeOptional(request.IssuingAuthority);
        item.AuthorizationNumber = request.AuthorizationNumber is null ? item.AuthorizationNumber : NormalizeOptional(request.AuthorizationNumber);
        item.EffectiveAt = request.EffectiveAt ?? item.EffectiveAt;
        item.ExpiresAt = request.ExpiresAt ?? item.ExpiresAt;
        item.Active = request.Active ?? item.Active;
        item.Description = request.Description is null ? item.Description : NormalizeOptional(request.Description, 2000);
        item.UpdatedAt = DateTimeOffset.UtcNow;

        ValidateDates(item.EffectiveAt, item.ExpiresAt);
        await ValidateEvidenceOptionGroupAsync(tenantId, item.RequiredEvidenceOptionGroupId, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync(
            "exception_exemption.updated",
            tenantId,
            actorPersonId,
            "exception_exemption",
            item.ExceptionExemptionId.ToString(),
            "success",
            reasonCode: item.Type,
            cancellationToken: cancellationToken);

        return ToResponse(item);
    }

    public async Task DeactivateAsync(
        Guid tenantId,
        Guid exceptionExemptionId,
        Guid actorPersonId,
        CancellationToken cancellationToken = default)
    {
        var item = await RequireAsync(tenantId, exceptionExemptionId, cancellationToken, tracking: true);
        item.Active = false;
        item.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync(
            "exception_exemption.deactivated",
            tenantId,
            actorPersonId,
            "exception_exemption",
            item.ExceptionExemptionId.ToString(),
            "success",
            cancellationToken: cancellationToken);
    }

    public Task<IReadOnlyList<TheoreticalOptionResponse>> GetTypeOptionsAsync() =>
        Task.FromResult<IReadOnlyList<TheoreticalOptionResponse>>(
            ComplianceExceptionExemptionTypes.All
                .Select(key => new TheoreticalOptionResponse(key, Labelize(key), "Legal exception/exemption type.", "exception_exemption_type"))
                .ToList());

    public Task<IReadOnlyList<TheoreticalOptionResponse>> GetEffectOptionsAsync() =>
        Task.FromResult<IReadOnlyList<TheoreticalOptionResponse>>(
            ComplianceExceptionExemptionEffectTypes.All
                .Select(key => new TheoreticalOptionResponse(key, Labelize(key), "Effect on normal requirement evaluation.", "exception_exemption_effect"))
                .ToList());

    private async Task<ComplianceExceptionExemption> RequireAsync(
        Guid tenantId,
        Guid exceptionExemptionId,
        CancellationToken cancellationToken,
        bool tracking = false)
    {
        var query = tracking ? db.ComplianceExceptionExemptions : db.ComplianceExceptionExemptions.AsNoTracking();
        return await query.FirstOrDefaultAsync(item => item.TenantId == tenantId && item.ExceptionExemptionId == exceptionExemptionId, cancellationToken)
            ?? throw new StlApiException("exception_exemptions.not_found", "Exception/exemption record was not found.", 404);
    }

    private async Task ValidateEvidenceOptionGroupAsync(
        Guid tenantId,
        Guid? evidenceOptionGroupId,
        CancellationToken cancellationToken)
    {
        if (evidenceOptionGroupId is null)
        {
            return;
        }

        var exists = await db.ComplianceEvidenceOptionGroups.AnyAsync(
            group => group.TenantId == tenantId && group.EvidenceOptionGroupId == evidenceOptionGroupId,
            cancellationToken);
        if (!exists)
        {
            throw new StlApiException("exception_exemptions.invalid_evidence_option_group", "Required evidence option group was not found.", 400);
        }
    }

    private static string NormalizeType(string type)
    {
        var normalized = NormalizeRequiredKey(type, "Type");
        if (!ComplianceExceptionExemptionTypes.All.Contains(normalized))
        {
            throw new StlApiException("exception_exemptions.invalid_type", "Exception/exemption type is not supported.", 400);
        }

        return normalized;
    }

    private static string NormalizeEffectType(string effectType)
    {
        var normalized = NormalizeRequiredKey(effectType, "Effect type");
        if (!ComplianceExceptionExemptionEffectTypes.All.Contains(normalized))
        {
            throw new StlApiException("exception_exemptions.invalid_effect_type", "Exception/exemption effect type is not supported.", 400);
        }

        return normalized;
    }

    private static string NormalizeRequiredKey(string value, string label)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new StlApiException("exception_exemptions.validation", $"{label} is required.", 400);
        }

        return value.Trim().ToLowerInvariant();
    }

    private static string RequireText(string value, string label, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new StlApiException("exception_exemptions.validation", $"{label} is required.", 400);
        }

        return NormalizeOptional(value, maxLength);
    }

    private static string NormalizeOptional(string? value, int maxLength = 256)
    {
        var normalized = value?.Trim() ?? string.Empty;
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    private static string NormalizeJsonObject(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "{}";
        }

        using var document = JsonDocument.Parse(value);
        if (document.RootElement.ValueKind is not JsonValueKind.Object)
        {
            throw new StlApiException("exception_exemptions.invalid_condition_logic", "condition_logic_json must be a JSON object.", 400);
        }

        return document.RootElement.GetRawText();
    }

    private static void ValidateDates(DateTimeOffset? effectiveAt, DateTimeOffset? expiresAt)
    {
        if (effectiveAt is not null && expiresAt is not null && expiresAt <= effectiveAt)
        {
            throw new StlApiException("exception_exemptions.invalid_dates", "expires_at must be after effective_at.", 400);
        }
    }

    private static string Labelize(string value) =>
        string.Join(' ', value.Split('_', StringSplitOptions.RemoveEmptyEntries)).Trim();

    public static ComplianceExceptionExemptionResponse ToResponse(ComplianceExceptionExemption item) =>
        new(
            item.ExceptionExemptionId,
            item.TenantId,
            item.Key,
            item.Label,
            item.Type,
            item.GoverningBody,
            item.ProgramKey,
            item.PackKey,
            item.CitationKey,
            item.ApplicabilityKey,
            item.AppliesToSubjectKind,
            item.AppliesToSourceProduct,
            item.AppliesToSourceEntity,
            item.EffectType,
            item.ConditionLogicJson,
            item.RequiredEvidenceOptionGroupId,
            item.IssuingAuthority,
            item.AuthorizationNumber,
            item.EffectiveAt,
            item.ExpiresAt,
            item.Active,
            item.Description,
            item.CreatedAt,
            item.UpdatedAt);
}
