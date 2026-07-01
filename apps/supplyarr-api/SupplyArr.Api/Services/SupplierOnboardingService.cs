using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class SupplierOnboardingService(
    SupplyArrDbContext db,
    SupplierComplianceDocumentService complianceDocuments,
    IntegrationOutboxEnqueueService integrationOutbox,
    ISupplyArrAuditService audit)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<SupplierOnboardingDocumentRequirementsResponse> GetDocumentRequirementsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var requirements = await LoadRequirementDefinitionsAsync(tenantId, cancellationToken);
        return new SupplierOnboardingDocumentRequirementsResponse(
            requirements.Select(x => new OnboardingDocumentRequirementDefinition(
                x.DocumentTypeKey,
                x.Label,
                x.IsRequired)).ToList());
    }

    public async Task<SupplierOnboardingDocumentRequirementsResponse> UpsertDocumentRequirementsAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertSupplierOnboardingDocumentRequirementsRequest request,
        CancellationToken cancellationToken = default)
    {
        var keys = SupplierOnboardingRules.NormalizeRequiredTypeKeys(request.RequiredDocumentTypeKeys);
        var entity = await db.TenantSupplierOnboardingSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (entity is null)
        {
            entity = new TenantSupplierOnboardingSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.TenantSupplierOnboardingSettings.Add(entity);
        }

        entity.RequiredDocumentTypeKeysJson = JsonSerializer.Serialize(keys, JsonOptions);
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "supplier_onboarding.requirements.update",
            tenantId,
            actorUserId,
            "tenant_supplier_onboarding_settings",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetDocumentRequirementsAsync(tenantId, cancellationToken);
    }

    public async Task<SupplierOnboardingResponse> StartOnboardingAsync(
        Guid tenantId,
        Guid actorUserId,
        StartSupplierOnboardingRequest request,
        CancellationToken cancellationToken = default)
    {
        var supplier = await LoadOnboardableSupplierAsync(tenantId, request.SupplierId, cancellationToken);

        var existing = await db.SupplierOnboardings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.SupplierId == supplier.Id, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (existing is not null)
        {
            if (!SupplierOnboardingStatuses.Editable.Contains(existing.OnboardingStatus)
                && existing.OnboardingStatus != SupplierOnboardingStatuses.Suspended)
            {
                throw new StlApiException(
                    "supplier_onboarding.already_active",
                    "An onboarding record already exists for this supplier.",
                    409);
            }

            existing.OnboardingStatus = SupplierOnboardingStatuses.Draft;
            existing.Notes = NormalizeNotes(request.Notes ?? existing.Notes);
            existing.RejectionReason = string.Empty;
            existing.SubmittedAt = null;
            existing.SubmittedByUserId = null;
            existing.ReviewedAt = null;
            existing.ReviewedByUserId = null;
            existing.UpdatedAt = now;
        }
        else
        {
            existing = new SupplierOnboarding
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                SupplierId = supplier.Id,
                OnboardingStatus = SupplierOnboardingStatuses.Draft,
                Notes = NormalizeNotes(request.Notes),
                CreatedAt = now,
                UpdatedAt = now,
            };
            db.SupplierOnboardings.Add(existing);
        }

        supplier.ApprovalStatus = "pending";
        supplier.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "supplier_onboarding.start",
            tenantId,
            actorUserId,
            "supplier_onboarding",
            existing.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await MapOnboardingResponseAsync(tenantId, existing.Id, cancellationToken);
    }

    public async Task<SupplierOnboardingResponse> GetBySupplierAsync(
        Guid tenantId,
        Guid supplierId,
        CancellationToken cancellationToken = default)
    {
        var onboarding = await db.SupplierOnboardings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.SupplierId == supplierId, cancellationToken)
            ?? throw new StlApiException("supplier_onboarding.not_found", "Supplier onboarding was not found.", 404);

        return await MapOnboardingResponseAsync(tenantId, onboarding.Id, cancellationToken);
    }

    public async Task<IReadOnlyList<SupplierOnboardingResponse>> ListPendingReviewAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var ids = await db.SupplierOnboardings
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.OnboardingStatus == SupplierOnboardingStatuses.PendingReview)
            .OrderBy(x => x.SubmittedAt)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var results = new List<SupplierOnboardingResponse>();
        foreach (var id in ids)
        {
            results.Add(await MapOnboardingResponseAsync(tenantId, id, cancellationToken));
        }

        return results;
    }

    public async Task<SupplierOnboardingResponse> SubmitForReviewAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid supplierId,
        SubmitSupplierOnboardingForReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var onboarding = await LoadOnboardingTrackedAsync(tenantId, supplierId, cancellationToken);
        if (!SupplierOnboardingStatuses.Editable.Contains(onboarding.OnboardingStatus))
        {
            throw new StlApiException(
                "supplier_onboarding.invalid_transition",
                "Only draft or rejected onboarding can be submitted for review.",
                409);
        }

        var requirements = await LoadRequirementDefinitionsAsync(tenantId, cancellationToken);
        var asOf = DateTimeOffset.UtcNow;
        var missing = new List<string>();
        foreach (var requirement in requirements.Where(x => x.IsRequired))
        {
            var satisfied = await complianceDocuments.HasApprovedRequiredDocumentAsync(
                tenantId,
                supplierId,
                requirement.DocumentTypeKey,
                asOf,
                cancellationToken);
            if (!satisfied)
            {
                missing.Add(requirement.Label);
            }
        }

        if (missing.Count > 0)
        {
            throw new StlApiException(
                "supplier_onboarding.documents.incomplete",
                $"Required approved documents missing: {string.Join(", ", missing)}.",
                400);
        }

        var now = DateTimeOffset.UtcNow;
        onboarding.OnboardingStatus = SupplierOnboardingStatuses.PendingReview;
        onboarding.Notes = NormalizeNotes(request.Notes ?? onboarding.Notes);
        onboarding.SubmittedAt = now;
        onboarding.SubmittedByUserId = actorUserId;
        onboarding.RejectionReason = string.Empty;
        onboarding.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "supplier_onboarding.submit",
            tenantId,
            actorUserId,
            "supplier_onboarding",
            onboarding.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        await integrationOutbox.TryEnqueueAsync(
            tenantId,
            IntegrationOutboxEventKinds.SupplierOnboardingSubmitted,
            "supplier_onboarding",
            onboarding.Id,
            new IntegrationOutboxPayload(tenantId, $"Supplier onboarding submitted: {onboarding.Supplier.DisplayName}"),
            cancellationToken: cancellationToken);

        return await MapOnboardingResponseAsync(tenantId, onboarding.Id, cancellationToken);
    }

    public Task<SupplierOnboardingResponse> SubmitForReviewBySupplierAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid supplierId,
        SubmitSupplierOnboardingForReviewRequest request,
        CancellationToken cancellationToken = default) =>
        SubmitForReviewAsync(tenantId, actorUserId, supplierId, request, cancellationToken);

    public async Task<SupplierOnboardingResponse> ApproveAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid supplierId,
        CancellationToken cancellationToken = default)
    {
        var onboarding = await LoadOnboardingTrackedAsync(tenantId, supplierId, cancellationToken);
        if (!SupplierOnboardingStatuses.Reviewable.Contains(onboarding.OnboardingStatus))
        {
            throw new StlApiException(
                "supplier_onboarding.invalid_transition",
                "Only pending review onboarding can be approved.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        onboarding.OnboardingStatus = SupplierOnboardingStatuses.Approved;
        onboarding.ReviewedAt = now;
        onboarding.ReviewedByUserId = actorUserId;
        onboarding.UpdatedAt = now;

        onboarding.Supplier.ApprovalStatus = "approved";
        onboarding.Supplier.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "supplier_onboarding.approve",
            tenantId,
            actorUserId,
            "supplier_onboarding",
            onboarding.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        await integrationOutbox.TryEnqueueAsync(
            tenantId,
            IntegrationOutboxEventKinds.SupplierOnboardingApproved,
            "supplier_onboarding",
            onboarding.Id,
            new IntegrationOutboxPayload(tenantId, $"Supplier onboarding approved: {onboarding.Supplier.DisplayName}"),
            cancellationToken: cancellationToken);

        return await MapOnboardingResponseAsync(tenantId, onboarding.Id, cancellationToken);
    }

    public Task<SupplierOnboardingResponse> ApproveSupplierAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid supplierId,
        CancellationToken cancellationToken = default) =>
        ApproveAsync(tenantId, actorUserId, supplierId, cancellationToken);

    public async Task<SupplierOnboardingResponse> RejectAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid supplierId,
        RejectSupplierOnboardingRequest request,
        CancellationToken cancellationToken = default)
    {
        var onboarding = await LoadOnboardingTrackedAsync(tenantId, supplierId, cancellationToken);
        if (!SupplierOnboardingStatuses.Reviewable.Contains(onboarding.OnboardingStatus))
        {
            throw new StlApiException(
                "supplier_onboarding.invalid_transition",
                "Only pending review onboarding can be rejected.",
                409);
        }

        var reason = NormalizeRejectionReason(request.Reason);
        var now = DateTimeOffset.UtcNow;
        onboarding.OnboardingStatus = SupplierOnboardingStatuses.Rejected;
        onboarding.RejectionReason = reason;
        onboarding.ReviewedAt = now;
        onboarding.ReviewedByUserId = actorUserId;
        onboarding.UpdatedAt = now;

        onboarding.Supplier.ApprovalStatus = "restricted";
        onboarding.Supplier.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "supplier_onboarding.reject",
            tenantId,
            actorUserId,
            "supplier_onboarding",
            onboarding.Id.ToString(),
            "Succeeded",
            reasonCode: reason,
            cancellationToken: cancellationToken);

        await integrationOutbox.TryEnqueueAsync(
            tenantId,
            IntegrationOutboxEventKinds.SupplierOnboardingRejected,
            "supplier_onboarding",
            onboarding.Id,
            new IntegrationOutboxPayload(tenantId, $"Supplier onboarding rejected: {onboarding.Supplier.DisplayName}"),
            cancellationToken: cancellationToken);

        return await MapOnboardingResponseAsync(tenantId, onboarding.Id, cancellationToken);
    }

    public Task<SupplierOnboardingResponse> RejectSupplierAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid supplierId,
        RejectSupplierOnboardingRequest request,
        CancellationToken cancellationToken = default) =>
        RejectAsync(tenantId, actorUserId, supplierId, request, cancellationToken);

    public async Task<SupplierOnboardingResponse> SuspendAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid supplierId,
        SuspendSupplierOnboardingRequest request,
        CancellationToken cancellationToken = default)
    {
        var onboarding = await LoadOnboardingTrackedAsync(tenantId, supplierId, cancellationToken);
        if (onboarding.OnboardingStatus != SupplierOnboardingStatuses.Approved)
        {
            throw new StlApiException(
                "supplier_onboarding.invalid_transition",
                "Only approved onboarding can be suspended.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        onboarding.OnboardingStatus = SupplierOnboardingStatuses.Suspended;
        if (!string.IsNullOrWhiteSpace(request.Reason))
        {
            onboarding.Notes = $"{onboarding.Notes}\nSuspended: {request.Reason.Trim()}".Trim();
        }

        onboarding.UpdatedAt = now;
        onboarding.Supplier.ApprovalStatus = "restricted";
        onboarding.Supplier.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "supplier_onboarding.suspend",
            tenantId,
            actorUserId,
            "supplier_onboarding",
            onboarding.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        await integrationOutbox.TryEnqueueAsync(
            tenantId,
            IntegrationOutboxEventKinds.SupplierOnboardingSuspended,
            "supplier_onboarding",
            onboarding.Id,
            new IntegrationOutboxPayload(tenantId, $"Supplier onboarding suspended: {onboarding.Supplier.DisplayName}"),
            cancellationToken: cancellationToken);

        return await MapOnboardingResponseAsync(tenantId, onboarding.Id, cancellationToken);
    }

    public Task<SupplierOnboardingResponse> SuspendSupplierAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid supplierId,
        SuspendSupplierOnboardingRequest request,
        CancellationToken cancellationToken = default) =>
        SuspendAsync(tenantId, actorUserId, supplierId, request, cancellationToken);

    private async Task<SupplierOnboardingResponse> MapOnboardingResponseAsync(
        Guid tenantId,
        Guid onboardingId,
        CancellationToken cancellationToken)
    {
        var onboarding = await db.SupplierOnboardings
            .AsNoTracking()
            .Include(x => x.Supplier)
            .ThenInclude(x => x.ParentSupplier)
            .FirstAsync(x => x.TenantId == tenantId && x.Id == onboardingId, cancellationToken);

        var requirements = await LoadRequirementDefinitionsAsync(tenantId, cancellationToken);
        var asOf = DateTimeOffset.UtcNow;
        var checklist = new List<OnboardingDocumentRequirementStatus>();
        foreach (var requirement in requirements)
        {
            var docs = await db.SupplierComplianceDocuments
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId
                    && x.SupplierId == onboarding.SupplierId
                    && x.DocumentTypeKey == requirement.DocumentTypeKey
                    && x.ReviewStatus == SupplierComplianceDocumentReviewStatuses.Approved
                    && (x.ExpiresAt == null || x.ExpiresAt > asOf))
                .OrderByDescending(x => x.Version)
                .FirstOrDefaultAsync(cancellationToken);

            checklist.Add(new OnboardingDocumentRequirementStatus(
                requirement.DocumentTypeKey,
                requirement.Label,
                requirement.IsRequired,
                docs is not null,
                docs?.Id,
                docs?.ReviewStatus));
        }

        return new SupplierOnboardingResponse(
            onboarding.Id,
            onboarding.SupplierId,
            onboarding.Supplier.SupplierKey,
            onboarding.Supplier.UnitKind,
            onboarding.Supplier.ParentSupplierId,
            onboarding.Supplier.ParentSupplier?.DisplayName,
            onboarding.Supplier.DisplayName,
            onboarding.OnboardingStatus,
            onboarding.Notes,
            onboarding.SubmittedAt,
            onboarding.ReviewedAt,
            onboarding.RejectionReason,
            checklist,
            onboarding.CreatedAt,
            onboarding.UpdatedAt);
    }

    private async Task<IReadOnlyList<OnboardingDocumentRequirementDefinitionSnapshot>> LoadRequirementDefinitionsAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var settings = await db.TenantSupplierOnboardingSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        IReadOnlyList<string> keys;
        if (settings is null || string.IsNullOrWhiteSpace(settings.RequiredDocumentTypeKeysJson))
        {
            keys = SupplierOnboardingRules.DefaultRequirements.Select(x => x.DocumentTypeKey).ToList();
        }
        else
        {
            keys = JsonSerializer.Deserialize<List<string>>(settings.RequiredDocumentTypeKeysJson, JsonOptions)
                ?? [];
            if (keys.Count == 0)
            {
                keys = SupplierOnboardingRules.DefaultRequirements.Select(x => x.DocumentTypeKey).ToList();
            }
        }

        return keys.Select(key => new OnboardingDocumentRequirementDefinitionSnapshot(
            key,
            SupplierOnboardingRules.ResolveLabel(key),
            true)).ToList();
    }

    private async Task<Supplier> LoadOnboardableSupplierAsync(
        Guid tenantId,
        Guid supplierId,
        CancellationToken cancellationToken)
    {
        var supplier = await db.Suppliers.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == supplierId,
            cancellationToken)
            ?? throw new StlApiException("suppliers.not_found", "Supplier was not found.", 404);

        return supplier;
    }

    private async Task<SupplierOnboarding> LoadOnboardingTrackedAsync(
        Guid tenantId,
        Guid supplierId,
        CancellationToken cancellationToken) =>
        await db.SupplierOnboardings
            .Include(x => x.Supplier)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.SupplierId == supplierId, cancellationToken)
        ?? throw new StlApiException("supplier_onboarding.not_found", "Supplier onboarding was not found.", 404);

    private static string NormalizeNotes(string? value) => value?.Trim() ?? string.Empty;

    private static string NormalizeRejectionReason(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new StlApiException("supplier_onboarding.rejection_reason.required", "Rejection reason is required.", 400);
        }

        return value.Trim();
    }
}


