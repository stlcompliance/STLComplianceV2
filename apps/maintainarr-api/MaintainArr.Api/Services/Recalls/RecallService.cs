using System.Globalization;
using System.Text.Json;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using MaintainArr.Api.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services.Recalls;

public sealed class RecallService(
    MaintainArrDbContext db,
    AssetService assetService,
    AssetQualityHoldService assetQualityHoldService,
    WorkOrderService workOrderService,
    IMaintainArrAuditService audit,
    RecallRegistry registry,
    RecallReadinessPolicy readinessPolicy,
    IOptions<RecallOptions> options)
{
    private readonly RecallOptions _options = options.Value;

    public IReadOnlyList<RecallProviderSummaryResponse> GetProviders() => registry.GetProviders();

    public Task<IReadOnlyList<RecallProviderHealthResponse>> GetProviderHealthAsync(
        CancellationToken cancellationToken = default) =>
        registry.GetProviderHealthAsync(cancellationToken);

    public async Task<IReadOnlyList<RecallCampaignResponse>> SearchByVehicleAsync(
        Guid tenantId,
        RecallVehicleSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateVehicleSearchRequest(request);
        var provider = registry.GetProvider(RecallSourceTypes.Nhtsa);
        if (provider is null || !provider.Enabled)
        {
            return [];
        }

        var campaigns = new List<RecallCampaign>();
        var seeds = await provider.FindCampaignsByVehicleAsync(tenantId, request.Year, request.Make, request.Model, cancellationToken);
        foreach (var seed in seeds)
        {
            campaigns.Add(await UpsertCampaignAsync(tenantId, seed, cancellationToken));
        }

        await db.SaveChangesAsync(cancellationToken);
        return await MapCampaignsAsync(tenantId, campaigns.Select(x => x.Id).ToList(), cancellationToken);
    }

    public async Task<IReadOnlyList<RecallCampaignResponse>> SearchByCampaignAsync(
        Guid tenantId,
        RecallCampaignSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateCampaignSearchRequest(request);
        var provider = registry.GetProvider(RecallSourceTypes.Nhtsa);
        if (provider is null || !provider.Enabled)
        {
            return [];
        }

        var seed = await provider.FindCampaignByCampaignNumberAsync(tenantId, request.CampaignNumber, cancellationToken);
        if (seed is null)
        {
            return [];
        }

        var campaign = await UpsertCampaignAsync(tenantId, seed, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return await MapCampaignsAsync(tenantId, [campaign.Id], cancellationToken);
    }

    public async Task<IReadOnlyList<RecallCampaignResponse>> ListCampaignsAsync(
        Guid tenantId,
        string? sourceProvider = null,
        string? status = null,
        string? campaignNumber = null,
        string? component = null,
        int limit = 100,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        var take = Math.Clamp(limit, 1, 250);
        var skip = Math.Max(offset, 0);

        var query = db.RecallCampaigns
            .AsNoTracking()
            .Include(x => x.Applicabilities)
            .Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(sourceProvider))
        {
            var normalized = sourceProvider.Trim();
            query = query.Where(x => x.SourceProvider == normalized);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalized = status.Trim();
            query = query.Where(x => x.CampaignStatus == normalized);
        }

        if (!string.IsNullOrWhiteSpace(campaignNumber))
        {
            var normalized = campaignNumber.Trim();
            query = query.Where(x =>
                x.NhtsaCampaignNumber == normalized
                || x.ManufacturerCampaignNumber == normalized
                || x.SourceProviderRecordId == normalized);
        }

        if (!string.IsNullOrWhiteSpace(component))
        {
            var normalized = component.Trim();
            query = query.Where(x => x.Component.Contains(normalized));
        }

        var campaigns = await query
            .OrderByDescending(x => x.UpdatedAt)
            .ThenByDescending(x => x.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return await MapCampaignsAsync(tenantId, campaigns.Select(x => x.Id).ToList(), cancellationToken);
    }

    public async Task<RecallCampaignResponse> GetCampaignAsync(
        Guid tenantId,
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        var campaign = await db.RecallCampaigns
            .AsNoTracking()
            .Include(x => x.Applicabilities)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == campaignId, cancellationToken)
            ?? throw new StlApiException("recall.campaign_not_found", "Recall campaign was not found.", 404);

        return await MapCampaignAsync(tenantId, campaign, cancellationToken);
    }

    public async Task<RecallCampaignResponse> CreateCampaignAsync(
        Guid tenantId,
        Guid actorUserId,
        string? actorPersonId,
        CreateRecallCampaignRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateCampaignRequest(request);
        var campaign = await UpsertCampaignAsync(tenantId, ToSeed(request), cancellationToken);

        if (request.AffectedAssetIds is { Count: > 0 } assetIds)
        {
            foreach (var assetId in assetIds.Distinct())
            {
                await UpsertManualCaseAsync(tenantId, actorUserId, actorPersonId, assetId, campaign, request, cancellationToken);
            }
        }

        if (request.CreateWorkOrdersNow && request.AffectedAssetIds is { Count: > 0 } workOrderAssetIds)
        {
            foreach (var assetId in workOrderAssetIds.Distinct())
            {
                var caseEntity = await db.AssetRecallCases.FirstOrDefaultAsync(
                    x => x.TenantId == tenantId && x.AssetId == assetId && x.RecallCampaignId == campaign.Id,
                    cancellationToken);
                if (caseEntity is not null)
                {
                    await CreateWorkOrderInternalAsync(tenantId, actorUserId, actorPersonId, caseEntity.AssetId, caseEntity.Id, false, cancellationToken);
                }
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "recall.campaign_create",
            tenantId,
            actorUserId,
            "recall_campaign",
            campaign.Id.ToString("D"),
            campaign.CampaignStatus,
            cancellationToken: cancellationToken);

        return await MapCampaignAsync(tenantId, campaign, cancellationToken);
    }

    public async Task<RecallCampaignResponse> UpdateCampaignAsync(
        Guid tenantId,
        Guid actorUserId,
        string? actorPersonId,
        Guid campaignId,
        UpdateRecallCampaignRequest request,
        CancellationToken cancellationToken = default)
    {
        var campaign = await db.RecallCampaigns
            .Include(x => x.Applicabilities)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == campaignId, cancellationToken)
            ?? throw new StlApiException("recall.campaign_not_found", "Recall campaign was not found.", 404);

        ApplyUpdate(campaign, request);
        if (request.Applicability is not null)
        {
            ReplaceApplicabilities(campaign, request.Applicability);
        }

        campaign.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "recall.campaign_update",
            tenantId,
            actorUserId,
            "recall_campaign",
            campaign.Id.ToString("D"),
            campaign.CampaignStatus,
            cancellationToken: cancellationToken);

        return await MapCampaignAsync(tenantId, campaign, cancellationToken);
    }

    public async Task<IReadOnlyList<AssetRecallCaseResponse>> ListAsync(
        Guid tenantId,
        Guid? assetId = null,
        string? status = null,
        string? sourceProvider = null,
        string? campaignNumber = null,
        string? component = null,
        int limit = 100,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        var take = Math.Clamp(limit, 1, 250);
        var skip = Math.Max(offset, 0);

        var query = db.AssetRecallCases
            .AsNoTracking()
            .Include(x => x.RecallCampaign)
                .ThenInclude(x => x.Applicabilities)
            .Where(x => x.TenantId == tenantId);

        if (assetId is Guid id)
        {
            query = query.Where(x => x.AssetId == id);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalized = status.Trim();
            query = query.Where(x => x.Status == normalized);
        }

        if (!string.IsNullOrWhiteSpace(sourceProvider))
        {
            var normalized = sourceProvider.Trim();
            query = query.Where(x => x.RecallCampaign.SourceProvider == normalized);
        }

        if (!string.IsNullOrWhiteSpace(campaignNumber))
        {
            var normalized = campaignNumber.Trim();
            query = query.Where(x =>
                x.RecallCampaign.NhtsaCampaignNumber == normalized
                || x.RecallCampaign.ManufacturerCampaignNumber == normalized
                || x.RecallCampaign.SourceProviderRecordId == normalized);
        }

        if (!string.IsNullOrWhiteSpace(component))
        {
            var normalized = component.Trim();
            query = query.Where(x => x.RecallCampaign.Component.Contains(normalized));
        }

        var cases = await query
            .OrderByDescending(x => x.UpdatedAt)
            .ThenByDescending(x => x.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return cases
            .Select(MapCaseResponse)
            .ToArray();
    }

    public async Task<IReadOnlyList<AssetRecallCaseResponse>> RefreshAssetAsync(
        Guid tenantId,
        Guid actorUserId,
        string? actorPersonId,
        Guid assetId,
        CancellationToken cancellationToken = default)
    {
        var profile = await LoadAssetProfileAsync(tenantId, assetId, cancellationToken);
        var provider = registry.GetProvider(RecallSourceTypes.Nhtsa);
        if (provider is null || !provider.Enabled)
        {
            return await ListAsync(tenantId, assetId, cancellationToken: cancellationToken);
        }

        if (profile.ModelYear is null || string.IsNullOrWhiteSpace(profile.Make) || string.IsNullOrWhiteSpace(profile.Model))
        {
            throw new StlApiException(
                "recall.vehicle_profile_required",
                "Model year, make, and model are required to refresh recalls. Run VIN decode or populate those asset fields first.",
                400);
        }

        var seeds = await provider.FindCampaignsByVehicleAsync(
            tenantId,
            profile.ModelYear.Value,
            profile.Make!,
            profile.Model!,
            cancellationToken);

        var activeCampaignIds = new HashSet<Guid>();
        foreach (var seed in seeds)
        {
            var campaign = await UpsertCampaignAsync(tenantId, seed, cancellationToken);
            activeCampaignIds.Add(campaign.Id);
            var caseEntity = await UpsertAssetCaseAsync(tenantId, actorUserId, actorPersonId, profile, campaign, seed, cancellationToken);
            await ApplyHoldAndAutomationAsync(tenantId, actorUserId, actorPersonId, profile, campaign, caseEntity, cancellationToken);
        }

        var existingCases = await db.AssetRecallCases
            .Include(x => x.RecallCampaign)
            .Where(x => x.TenantId == tenantId && x.AssetId == assetId)
            .ToListAsync(cancellationToken);

        foreach (var existing in existingCases.Where(x =>
                     x.RecallCampaign.SourceProvider == RecallSourceTypes.Nhtsa
                     && !activeCampaignIds.Contains(x.RecallCampaignId)
                     && !RecallHelpers.IsResolvedCaseStatus(x.Status)
                     && !RecallHelpers.IsVerifiedOpenStatus(x.Status)))
        {
            existing.Status = RecallCaseStatuses.Superseded;
            existing.ReadinessImpact = RecallReadinessImpacts.NoHold;
            existing.LastRefreshedAt = DateTimeOffset.UtcNow;
            existing.ActionStatus = RecallActionStatuses.Cancelled;
            if (existing.ReadinessHoldId.HasValue)
            {
                await ReleaseHoldAsync(tenantId, actorUserId, actorPersonId, existing.RecallCampaign, existing, cancellationToken);
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "recall.asset_refresh",
            tenantId,
            actorUserId,
            "asset_recall_case",
            assetId.ToString("D"),
            "Succeeded",
            cancellationToken: cancellationToken);

        if (_options.AutoCreateWorkOrder)
        {
            foreach (var caseEntity in await db.AssetRecallCases
                         .Include(x => x.RecallCampaign)
                         .Where(x => x.TenantId == tenantId && x.AssetId == assetId)
                         .Where(x => x.WorkOrderId == null)
                         .Where(x => RecallHelpers.RequiresHold(x.Status, x.ReadinessImpact) || RecallHelpers.IsVerifiedOpenStatus(x.Status))
                         .ToListAsync(cancellationToken))
            {
                await CreateWorkOrderInternalAsync(tenantId, actorUserId, actorPersonId, caseEntity.AssetId, caseEntity.Id, false, cancellationToken);
            }
        }

        return await ListAsync(tenantId, assetId, cancellationToken: cancellationToken);
    }

    public async Task<AssetRecallCaseResponse> VerifyAsync(
        Guid tenantId,
        Guid actorUserId,
        string? actorPersonId,
        Guid assetId,
        Guid caseId,
        VerifyAssetRecallRequest request,
        CancellationToken cancellationToken = default)
    {
        var caseEntity = await GetCaseAsync(tenantId, assetId, caseId, cancellationToken);

        caseEntity.VerificationSource = NormalizeOptional(request.VerificationSource, 64)
            ?? throw new StlApiException("recall.verification_source_required", "Verification source is required.", 400);
        caseEntity.VerificationMethod = NormalizeOptional(request.VerificationMethod, 64)
            ?? throw new StlApiException("recall.verification_method_required", "Verification method is required.", 400);
        caseEntity.VerificationStatus = NormalizeOptional(request.VerificationStatus, 32)
            ?? throw new StlApiException("recall.verification_status_required", "Verification status is required.", 400);
        caseEntity.VerifiedByPersonId = NormalizeOptional(request.VerifiedByPersonId, 128);
        caseEntity.VerifiedAt = request.VerifiedAt ?? DateTimeOffset.UtcNow;
        caseEntity.EvidenceDocumentId = request.EvidenceDocumentId;
        caseEntity.EvidenceUrl = NormalizeOptional(request.EvidenceUrl, 512);
        caseEntity.EvidenceText = NormalizeOptional(request.EvidenceText, 1024);
        caseEntity.ProviderRawJson = NormalizeOptional(request.ProviderRawJson, int.MaxValue);
        caseEntity.ExpiresAt = request.ExpiresAt;
        caseEntity.UpdatedAt = DateTimeOffset.UtcNow;
        caseEntity.ActionType = RecallActionTypes.NoteOnly;
        caseEntity.ActionStatus = RecallActionStatuses.Completed;

        if (string.Equals(caseEntity.VerificationStatus, RecallCaseStatuses.CompletedVerified, StringComparison.OrdinalIgnoreCase)
            && _options.RequireEvidenceForCompletedVerified
            && caseEntity.EvidenceDocumentId is null
            && string.IsNullOrWhiteSpace(caseEntity.EvidenceUrl)
            && string.IsNullOrWhiteSpace(caseEntity.EvidenceText)
            && string.IsNullOrWhiteSpace(caseEntity.ProviderRawJson))
        {
            throw new StlApiException(
                "recall.evidence_required",
                "Completed verified recalls require evidence when configured.",
                400);
        }

        caseEntity.Status = NormalizeVerificationStatusToCaseStatus(caseEntity.VerificationStatus);
        caseEntity.ReadinessImpact = readinessPolicy.DetermineReadinessImpact(caseEntity.RecallCampaign, caseEntity);
        if (RecallHelpers.RequiresHold(caseEntity.Status, caseEntity.ReadinessImpact))
        {
            await CreateHoldAsync(tenantId, actorUserId, actorPersonId, caseEntity.RecallCampaign, caseEntity, cancellationToken);
        }
        else if (caseEntity.ReadinessHoldId.HasValue)
        {
            await ReleaseHoldAsync(tenantId, actorUserId, actorPersonId, caseEntity.RecallCampaign, caseEntity, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "recall.verify",
            tenantId,
            actorUserId,
            actorPersonId,
            "asset_recall_case",
            caseEntity.Id.ToString("D"),
            caseEntity.Status,
            cancellationToken: cancellationToken);

        return MapCaseResponse(caseEntity);
    }

    public async Task<AssetRecallCaseResponse> DismissAsync(
        Guid tenantId,
        Guid actorUserId,
        string? actorPersonId,
        Guid assetId,
        Guid caseId,
        DismissAssetRecallRequest request,
        CancellationToken cancellationToken = default)
    {
        var caseEntity = await GetCaseAsync(tenantId, assetId, caseId, cancellationToken);
        caseEntity.Status = RecallCaseStatuses.Dismissed;
        caseEntity.VerificationStatus = RecallCaseStatuses.Unknown;
        caseEntity.DismissedByPersonId = NormalizeOptional(request.DismissedByPersonId, 128) ?? actorPersonId;
        caseEntity.DismissedAt = DateTimeOffset.UtcNow;
        caseEntity.DismissalReason = NormalizeOptional(request.DismissalReason, 1024)
            ?? throw new StlApiException("recall.dismissal_reason_required", "Dismissal reason is required.", 400);
        caseEntity.ReadinessImpact = RecallReadinessImpacts.NoHold;
        caseEntity.ActionStatus = RecallActionStatuses.Cancelled;
        caseEntity.UpdatedAt = DateTimeOffset.UtcNow;

        if (caseEntity.ReadinessHoldId.HasValue)
        {
            await ReleaseHoldAsync(tenantId, actorUserId, actorPersonId, caseEntity.RecallCampaign, caseEntity, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "recall.dismiss",
            tenantId,
            actorUserId,
            actorPersonId,
            "asset_recall_case",
            caseEntity.Id.ToString("D"),
            caseEntity.Status,
            cancellationToken: cancellationToken);

        return MapCaseResponse(caseEntity);
    }

    public async Task<AssetRecallCaseResponse> CreateReadinessHoldAsync(
        Guid tenantId,
        Guid actorUserId,
        string? actorPersonId,
        Guid assetId,
        Guid caseId,
        CancellationToken cancellationToken = default)
    {
        var caseEntity = await GetCaseAsync(tenantId, assetId, caseId, cancellationToken);
        await CreateHoldAsync(tenantId, actorUserId, actorPersonId, caseEntity.RecallCampaign, caseEntity, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return MapCaseResponse(caseEntity);
    }

    public async Task<AssetRecallCaseResponse> ReleaseReadinessHoldAsync(
        Guid tenantId,
        Guid actorUserId,
        string? actorPersonId,
        Guid assetId,
        Guid caseId,
        ReleaseRecallHoldRequest request,
        CancellationToken cancellationToken = default)
    {
        var caseEntity = await GetCaseAsync(tenantId, assetId, caseId, cancellationToken);
        await ReleaseHoldAsync(
            tenantId,
            actorUserId,
            actorPersonId,
            caseEntity.RecallCampaign,
            caseEntity,
            request,
            cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return MapCaseResponse(caseEntity);
    }

    public async Task<WorkOrderDetailResponse> CreateWorkOrderAsync(
        Guid tenantId,
        Guid actorUserId,
        string? actorPersonId,
        Guid assetId,
        Guid caseId,
        CancellationToken cancellationToken = default) =>
        await CreateWorkOrderInternalAsync(tenantId, actorUserId, actorPersonId, assetId, caseId, false, cancellationToken);

    public async Task<WorkOrderDetailResponse> CreateInspectionItemAsync(
        Guid tenantId,
        Guid actorUserId,
        string? actorPersonId,
        Guid assetId,
        Guid caseId,
        CancellationToken cancellationToken = default) =>
        await CreateWorkOrderInternalAsync(tenantId, actorUserId, actorPersonId, assetId, caseId, true, cancellationToken);

    public async Task<RecallDashboardResponse> GetDashboardAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var cases = await db.AssetRecallCases
            .AsNoTracking()
            .Include(x => x.RecallCampaign)
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        var assets = await db.Assets
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ToDictionaryAsync(x => x.Id, x => x, cancellationToken);

        var generatedAt = DateTimeOffset.UtcNow;
        var activeCases = cases.Where(x => !RecallHelpers.IsResolvedCaseStatus(x.Status)).ToList();
        var attentionItems = activeCases
            .Select(caseEntity => BuildDashboardItem(caseEntity, assets))
            .Where(item => item is not null)
            .Select(item => item!)
            .OrderByDescending(item => item.ReadinessImpact, StringComparer.OrdinalIgnoreCase)
            .ThenByDescending(item => item.DetectedAt)
            .Take(25)
            .ToList();

        var monthStart = new DateTimeOffset(generatedAt.UtcDateTime.Date.AddDays(-(generatedAt.UtcDateTime.Day - 1)), TimeSpan.Zero);

        return new RecallDashboardResponse(
            generatedAt,
            cases.Count(x => RecallHelpers.IsVerifiedOpenStatus(x.Status)),
            cases.Count(x => string.Equals(x.Status, RecallCaseStatuses.PotentialMatch, StringComparison.OrdinalIgnoreCase)
                || string.Equals(x.Status, RecallCaseStatuses.NeedsVinCheck, StringComparison.OrdinalIgnoreCase)
                || string.Equals(x.Status, RecallCaseStatuses.NeedsSerialCheck, StringComparison.OrdinalIgnoreCase)
                || string.Equals(x.Status, RecallCaseStatuses.NeedsManualReview, StringComparison.OrdinalIgnoreCase)),
            cases.Count(x => string.Equals(x.ReadinessImpact, RecallReadinessImpacts.ParkIt, StringComparison.OrdinalIgnoreCase)),
            cases.Count(x => string.Equals(x.ReadinessImpact, RecallReadinessImpacts.ParkOutside, StringComparison.OrdinalIgnoreCase)),
            cases.Count(x => x.WorkOrderId.HasValue),
            cases.Count(x => string.Equals(x.Status, RecallCaseStatuses.CompletedVerified, StringComparison.OrdinalIgnoreCase)
                && x.VerifiedAt.HasValue
                && x.VerifiedAt.Value >= monthStart),
            activeCases.Count(x => NextReviewAt(x) < generatedAt),
            assets.Count(asset => !cases.Any(caseEntity => caseEntity.AssetId == asset.Key)),
            attentionItems);
    }

    private static void ValidateVehicleSearchRequest(RecallVehicleSearchRequest request)
    {
        if (request.Year <= 0)
        {
            throw new StlApiException("recall.validation", "Year is required.", 400);
        }

        if (string.IsNullOrWhiteSpace(request.Make) || string.IsNullOrWhiteSpace(request.Model))
        {
            throw new StlApiException("recall.validation", "Make and model are required.", 400);
        }
    }

    private static void ValidateCampaignSearchRequest(RecallCampaignSearchRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CampaignNumber))
        {
            throw new StlApiException("recall.validation", "Campaign number is required.", 400);
        }
    }

    private static void ValidateCampaignRequest(CreateRecallCampaignRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SourceProvider))
        {
            throw new StlApiException("recall.validation", "Source provider is required.", 400);
        }

        if (string.IsNullOrWhiteSpace(request.SourceType))
        {
            throw new StlApiException("recall.validation", "Source type is required.", 400);
        }

        if (string.IsNullOrWhiteSpace(request.Manufacturer))
        {
            throw new StlApiException("recall.validation", "Manufacturer is required.", 400);
        }

        if (string.IsNullOrWhiteSpace(request.Component))
        {
            throw new StlApiException("recall.validation", "Component is required.", 400);
        }

        if (string.IsNullOrWhiteSpace(request.CampaignStatus))
        {
            throw new StlApiException("recall.validation", "Campaign status is required.", 400);
        }

        if (string.IsNullOrWhiteSpace(request.Summary))
        {
            throw new StlApiException("recall.validation", "Summary is required.", 400);
        }

        if (string.IsNullOrWhiteSpace(request.Consequence))
        {
            throw new StlApiException("recall.validation", "Consequence is required.", 400);
        }

        if (string.IsNullOrWhiteSpace(request.Remedy))
        {
            throw new StlApiException("recall.validation", "Remedy is required.", 400);
        }

        if (string.IsNullOrWhiteSpace(request.RecallType))
        {
            throw new StlApiException("recall.validation", "Recall type is required.", 400);
        }
    }

    private static RecallCampaignSeed ToSeed(CreateRecallCampaignRequest request) =>
        new(
            request.SourceProvider.Trim(),
            request.SourceType.Trim(),
            NormalizeOptional(request.SourceProviderRecordId, 128)
                ?? NormalizeOptional(request.NhtsaCampaignNumber, 64)
                ?? NormalizeOptional(request.ManufacturerCampaignNumber, 128)
                ?? Guid.NewGuid().ToString("D"),
            NormalizeOptional(request.NhtsaCampaignNumber, 64),
            NormalizeOptional(request.NhtsaActionNumber, 64),
            NormalizeOptional(request.ManufacturerCampaignNumber, 128),
            NormalizeOptional(request.CampaignTitle, 256),
            request.Manufacturer.Trim(),
            request.Component.Trim(),
            NormalizeOptional(request.ReportReceivedDate, 64),
            NormalizeOptional(request.CampaignStartDate, 64),
            NormalizeOptional(request.CampaignEndDate, 64),
            NormalizeOptional(request.CampaignStatus, 32) ?? RecallCampaignStatuses.Unknown,
            request.PotentialUnitsAffected,
            request.Summary.Trim(),
            request.Consequence.Trim(),
            request.Remedy.Trim(),
            request.Notes.Trim(),
            request.ParkIt,
            request.ParkOutside,
            request.OverTheAirUpdate,
            NormalizeOptional(request.RecallType, 64) ?? "unknown",
            NormalizeOptional(request.SourceUrl, 512),
            DateTimeOffset.UtcNow,
            NormalizeOptional(request.SourceRawJson, int.MaxValue),
            (request.Applicability ?? [])
                .Select(MapApplicabilityRequest)
                .ToArray());

    private static RecallCampaignApplicabilityRequest MapApplicabilityRequest(RecallCampaignApplicabilityRequest request) => request;

    private async Task<RecallCampaign> UpsertCampaignAsync(
        Guid tenantId,
        RecallCampaignSeed seed,
        CancellationToken cancellationToken)
    {
        var providerRecordId = NormalizeOptional(seed.SourceProviderRecordId, 128)
            ?? NormalizeOptional(seed.NhtsaCampaignNumber, 64)
            ?? NormalizeOptional(seed.ManufacturerCampaignNumber, 128)
            ?? Guid.NewGuid().ToString("D");

        var campaign = await db.RecallCampaigns
            .Include(x => x.Applicabilities)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId
                    && x.SourceProvider == seed.SourceProvider
                    && x.SourceProviderRecordId == providerRecordId,
                cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (campaign is null)
        {
            campaign = new RecallCampaign
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.RecallCampaigns.Add(campaign);
        }

        campaign.SourceProvider = NormalizeOptional(seed.SourceProvider, 64) ?? RecallSourceTypes.Manual;
        campaign.SourceType = NormalizeOptional(seed.SourceType, 64) ?? RecallSourceTypes.Manual;
        campaign.SourceProviderRecordId = providerRecordId;
        campaign.NhtsaCampaignNumber = NormalizeOptional(seed.NhtsaCampaignNumber, 64);
        campaign.NhtsaActionNumber = NormalizeOptional(seed.NhtsaActionNumber, 64);
        campaign.ManufacturerCampaignNumber = NormalizeOptional(seed.ManufacturerCampaignNumber, 128);
        campaign.CampaignTitle = NormalizeOptional(seed.CampaignTitle, 256);
        campaign.Manufacturer = NormalizeOptional(seed.Manufacturer, 256) ?? string.Empty;
        campaign.Component = NormalizeOptional(seed.Component, 256) ?? string.Empty;
        campaign.ReportReceivedDate = NormalizeOptional(seed.ReportReceivedDate, 64);
        campaign.CampaignStartDate = NormalizeOptional(seed.CampaignStartDate, 64);
        campaign.CampaignEndDate = NormalizeOptional(seed.CampaignEndDate, 64);
        campaign.CampaignStatus = NormalizeOptional(seed.CampaignStatus, 32) ?? RecallCampaignStatuses.Unknown;
        campaign.PotentialUnitsAffected = seed.PotentialUnitsAffected;
        campaign.Summary = NormalizeOptional(seed.Summary, 1024) ?? string.Empty;
        campaign.Consequence = NormalizeOptional(seed.Consequence, 1024) ?? string.Empty;
        campaign.Remedy = NormalizeOptional(seed.Remedy, 1024) ?? string.Empty;
        campaign.Notes = NormalizeOptional(seed.Notes, 1024) ?? string.Empty;
        campaign.ParkIt = seed.ParkIt;
        campaign.ParkOutside = seed.ParkOutside;
        campaign.OverTheAirUpdate = seed.OverTheAirUpdate;
        campaign.RecallType = NormalizeOptional(seed.RecallType, 64) ?? "unknown";
        campaign.SourceRawJson = NormalizeOptional(seed.SourceRawJson, int.MaxValue);
        campaign.SourceUrl = NormalizeOptional(seed.SourceUrl, 512);
        campaign.FetchedAt = seed.FetchedAt ?? now;
        campaign.UpdatedAt = now;

        ReplaceApplicabilities(campaign, seed.Applicability);
        await UpsertMakeModelAliasesAsync(campaign, seed.Applicability, cancellationToken);

        return campaign;
    }

    private async Task UpsertMakeModelAliasesAsync(
        RecallCampaign campaign,
        IReadOnlyList<RecallCampaignApplicabilityRequest> applicability,
        CancellationToken cancellationToken)
    {
        foreach (var item in applicability.Where(x => !string.IsNullOrWhiteSpace(x.Make) || !string.IsNullOrWhiteSpace(x.Model)))
        {
            var rawMake = NormalizeOptional(item.Make, 128);
            var rawModel = NormalizeOptional(item.Model, 128);
            if (rawMake is null || rawModel is null)
            {
                continue;
            }

            var normalizedMake = NormalizeComparisonKey(rawMake);
            var normalizedModel = NormalizeComparisonKey(rawModel);
            var alias = await db.RecallMakeModelAliases.FirstOrDefaultAsync(
                x => x.Provider == campaign.SourceProvider
                    && x.RawMake == rawMake
                    && x.RawModel == rawModel,
                cancellationToken);

            if (alias is null)
            {
                alias = new RecallMakeModelAlias
                {
                    Id = Guid.NewGuid(),
                    Provider = campaign.SourceProvider,
                    RawMake = rawMake,
                    RawModel = rawModel,
                    NormalizedMake = normalizedMake,
                    NormalizedModel = normalizedModel,
                    Confidence = 1.0,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow,
                };
                db.RecallMakeModelAliases.Add(alias);
            }
            else
            {
                alias.NormalizedMake = normalizedMake;
                alias.NormalizedModel = normalizedModel;
                alias.Confidence = Math.Max(alias.Confidence, 0.9);
                alias.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }
    }

    private static void ReplaceApplicabilities(
        RecallCampaign campaign,
        IReadOnlyList<RecallCampaignApplicabilityRequest> applicability)
    {
        if (campaign.Applicabilities.Count > 0)
        {
            campaign.Applicabilities.Clear();
        }

        var now = DateTimeOffset.UtcNow;
        foreach (var item in applicability)
        {
            campaign.Applicabilities.Add(new RecallCampaignApplicability
            {
                Id = Guid.NewGuid(),
                RecallCampaignId = campaign.Id,
                ModelYear = item.ModelYear,
                Make = NormalizeOptional(item.Make, 128),
                Model = NormalizeOptional(item.Model, 128),
                AssetClass = NormalizeOptional(item.AssetClass, 128),
                AssetType = NormalizeOptional(item.AssetType, 128),
                BodyClass = NormalizeOptional(item.BodyClass, 128),
                VehicleType = NormalizeOptional(item.VehicleType, 128),
                FuelType = NormalizeOptional(item.FuelType, 128),
                EngineFamily = NormalizeOptional(item.EngineFamily, 128),
                EngineManufacturer = NormalizeOptional(item.EngineManufacturer, 128),
                ComponentCategory = NormalizeOptional(item.ComponentCategory, 128),
                TireBrand = NormalizeOptional(item.TireBrand, 128),
                TireLine = NormalizeOptional(item.TireLine, 128),
                TireSize = NormalizeOptional(item.TireSize, 128),
                EquipmentMake = NormalizeOptional(item.EquipmentMake, 128),
                EquipmentModel = NormalizeOptional(item.EquipmentModel, 128),
                SerialRangeStart = NormalizeOptional(item.SerialRangeStart, 128),
                SerialRangeEnd = NormalizeOptional(item.SerialRangeEnd, 128),
                ProductionStartDate = item.ProductionStartDate,
                ProductionEndDate = item.ProductionEndDate,
                Notes = NormalizeOptional(item.Notes, 1024),
                CreatedAt = now,
                UpdatedAt = now,
            });
        }
    }

    private static void ApplyUpdate(RecallCampaign campaign, UpdateRecallCampaignRequest request)
    {
        if (request.CampaignTitle is not null)
        {
            campaign.CampaignTitle = NormalizeOptional(request.CampaignTitle, 256);
        }

        if (request.Manufacturer is not null)
        {
            campaign.Manufacturer = NormalizeOptional(request.Manufacturer, 256) ?? string.Empty;
        }

        if (request.Component is not null)
        {
            campaign.Component = NormalizeOptional(request.Component, 256) ?? string.Empty;
        }

        if (request.ReportReceivedDate is not null)
        {
            campaign.ReportReceivedDate = NormalizeOptional(request.ReportReceivedDate, 64);
        }

        if (request.CampaignStartDate is not null)
        {
            campaign.CampaignStartDate = NormalizeOptional(request.CampaignStartDate, 64);
        }

        if (request.CampaignEndDate is not null)
        {
            campaign.CampaignEndDate = NormalizeOptional(request.CampaignEndDate, 64);
        }

        if (request.CampaignStatus is not null)
        {
            campaign.CampaignStatus = NormalizeOptional(request.CampaignStatus, 32) ?? RecallCampaignStatuses.Unknown;
        }

        if (request.PotentialUnitsAffected.HasValue)
        {
            campaign.PotentialUnitsAffected = request.PotentialUnitsAffected;
        }

        if (request.Summary is not null)
        {
            campaign.Summary = NormalizeOptional(request.Summary, 1024) ?? string.Empty;
        }

        if (request.Consequence is not null)
        {
            campaign.Consequence = NormalizeOptional(request.Consequence, 1024) ?? string.Empty;
        }

        if (request.Remedy is not null)
        {
            campaign.Remedy = NormalizeOptional(request.Remedy, 1024) ?? string.Empty;
        }

        if (request.Notes is not null)
        {
            campaign.Notes = NormalizeOptional(request.Notes, 1024) ?? string.Empty;
        }

        if (request.ParkIt.HasValue)
        {
            campaign.ParkIt = request.ParkIt.Value;
        }

        if (request.ParkOutside.HasValue)
        {
            campaign.ParkOutside = request.ParkOutside.Value;
        }

        if (request.OverTheAirUpdate.HasValue)
        {
            campaign.OverTheAirUpdate = request.OverTheAirUpdate.Value;
        }

        if (request.RecallType is not null)
        {
            campaign.RecallType = NormalizeOptional(request.RecallType, 64) ?? "unknown";
        }

        if (request.SourceUrl is not null)
        {
            campaign.SourceUrl = NormalizeOptional(request.SourceUrl, 512);
        }

        if (request.SourceRawJson is not null)
        {
            campaign.SourceRawJson = NormalizeOptional(request.SourceRawJson, int.MaxValue);
        }
    }

    private async Task UpsertManualCaseAsync(
        Guid tenantId,
        Guid actorUserId,
        string? actorPersonId,
        Guid assetId,
        RecallCampaign campaign,
        CreateRecallCampaignRequest request,
        CancellationToken cancellationToken)
    {
        var asset = await db.Assets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == assetId, cancellationToken)
            ?? throw new StlApiException("asset.not_found", "Asset was not found.", 404);

        var caseEntity = await db.AssetRecallCases.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.AssetId == assetId && x.RecallCampaignId == campaign.Id,
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (caseEntity is null)
        {
            caseEntity = new AssetRecallCase
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                AssetId = assetId,
                RecallCampaignId = campaign.Id,
                DetectedAt = now,
                CreatedAt = now,
            };
            db.AssetRecallCases.Add(caseEntity);
        }

        caseEntity.MatchBasis = RecallMatchBases.Manual;
        caseEntity.MatchConfidence = RecallMatchConfidenceLevels.Medium;
        caseEntity.MatchScore = 0.5m;
        caseEntity.Status = string.IsNullOrWhiteSpace(caseEntity.Status)
            ? RecallCaseStatuses.NeedsManualReview
            : caseEntity.Status;
        caseEntity.ReadinessImpact = readinessPolicy.DetermineReadinessImpact(campaign, caseEntity);
        caseEntity.Reason = $"{campaign.CampaignNumberLabel()} applies to {asset.AssetTag} and requires manual review.";
        caseEntity.VerificationStatus = caseEntity.VerificationStatus == RecallCaseStatuses.Unknown
            ? RecallCaseStatuses.Unknown
            : caseEntity.VerificationStatus;
        caseEntity.ActionType = string.IsNullOrWhiteSpace(caseEntity.ActionType) ? RecallActionTypes.NoteOnly : caseEntity.ActionType;
        caseEntity.ActionStatus = string.IsNullOrWhiteSpace(caseEntity.ActionStatus) ? RecallActionStatuses.Planned : caseEntity.ActionStatus;
        caseEntity.LastRefreshedAt = now;
        caseEntity.ProviderRawJson ??= request.SourceRawJson;
        caseEntity.ExpiresAt ??= now.AddDays(_options.DefaultRecheckDays);
        caseEntity.UpdatedAt = now;

        if (request.CreateCandidatesNow && readinessPolicy.ShouldCreateHold(campaign, caseEntity))
        {
            await CreateHoldAsync(tenantId, actorUserId, actorPersonId, campaign, caseEntity, cancellationToken);
        }
    }

    private async Task<AssetRecallCase> UpsertAssetCaseAsync(
        Guid tenantId,
        Guid actorUserId,
        string? actorPersonId,
        RecallAssetProfile profile,
        RecallCampaign campaign,
        RecallCampaignSeed seed,
        CancellationToken cancellationToken)
    {
        var caseEntity = await db.AssetRecallCases.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.AssetId == profile.AssetId && x.RecallCampaignId == campaign.Id,
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (caseEntity is null)
        {
            caseEntity = new AssetRecallCase
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                AssetId = profile.AssetId,
                RecallCampaignId = campaign.Id,
                DetectedAt = now,
                CreatedAt = now,
            };
            db.AssetRecallCases.Add(caseEntity);
        }

        var preserveManualDecision = !string.IsNullOrWhiteSpace(caseEntity.Status)
            && (RecallHelpers.IsResolvedCaseStatus(caseEntity.Status)
                || RecallHelpers.IsVerifiedOpenStatus(caseEntity.Status)
                || RecallHelpers.IsCompletedStatus(caseEntity.Status)
                || string.Equals(caseEntity.Status, RecallCaseStatuses.Monitoring, StringComparison.OrdinalIgnoreCase)
                || string.Equals(caseEntity.Status, RecallCaseStatuses.NeedsManualReview, StringComparison.OrdinalIgnoreCase)
                || string.Equals(caseEntity.Status, RecallCaseStatuses.Superseded, StringComparison.OrdinalIgnoreCase));

        caseEntity.MatchBasis = NormalizeOptional(seed.SourceType, 64) is not null
            ? seed.SourceType
            : caseEntity.MatchBasis;
        caseEntity.MatchConfidence = readinessPolicy.DetermineMatchConfidence(seed);
        caseEntity.MatchScore = caseEntity.MatchConfidence switch
        {
            RecallMatchConfidenceLevels.High => 0.95m,
            RecallMatchConfidenceLevels.Medium => 0.7m,
            _ => 0.4m,
        };
        caseEntity.Status = preserveManualDecision ? caseEntity.Status : readinessPolicy.DetermineMatchStatus(seed);
        caseEntity.ReadinessImpact = readinessPolicy.DetermineReadinessImpact(campaign, caseEntity);
        caseEntity.Reason = preserveManualDecision && !string.IsNullOrWhiteSpace(caseEntity.Reason)
            ? caseEntity.Reason
            : BuildMatchReason(profile, campaign, seed);
        caseEntity.VerificationStatus = string.IsNullOrWhiteSpace(caseEntity.VerificationStatus)
            ? RecallCaseStatuses.Unknown
            : caseEntity.VerificationStatus;
        caseEntity.VerifiedByPersonId = NormalizeOptional(caseEntity.VerifiedByPersonId, 128);
        caseEntity.VerifiedAt ??= null;
        caseEntity.ProviderRawJson = seed.SourceRawJson ?? caseEntity.ProviderRawJson;
        caseEntity.ExpiresAt ??= now.AddDays(_options.DefaultRecheckDays);
        caseEntity.ActionType = string.IsNullOrWhiteSpace(caseEntity.ActionType) ? RecallActionTypes.NoteOnly : caseEntity.ActionType;
        caseEntity.ActionStatus = string.IsNullOrWhiteSpace(caseEntity.ActionStatus) ? RecallActionStatuses.Planned : caseEntity.ActionStatus;
        caseEntity.LastRefreshedAt = now;
        caseEntity.UpdatedAt = now;

        if (!preserveManualDecision && readinessPolicy.ShouldCreateHold(campaign, caseEntity))
        {
            await CreateHoldAsync(tenantId, actorUserId, actorPersonId, campaign, caseEntity, cancellationToken);
        }
        else if (caseEntity.ReadinessHoldId.HasValue && !RecallHelpers.RequiresHold(caseEntity.Status, caseEntity.ReadinessImpact))
        {
            await ReleaseHoldAsync(tenantId, actorUserId, actorPersonId, campaign, caseEntity, cancellationToken);
        }

        return caseEntity;
    }

    private async Task ApplyHoldAndAutomationAsync(
        Guid tenantId,
        Guid actorUserId,
        string? actorPersonId,
        RecallAssetProfile profile,
        RecallCampaign campaign,
        AssetRecallCase caseEntity,
        CancellationToken cancellationToken)
    {
        if (ReadinessNeedsHold(caseEntity, campaign))
        {
            await CreateHoldAsync(tenantId, actorUserId, actorPersonId, campaign, caseEntity, cancellationToken);
        }
        else if (caseEntity.ReadinessHoldId.HasValue)
        {
            await ReleaseHoldAsync(tenantId, actorUserId, actorPersonId, campaign, caseEntity, cancellationToken);
        }

        if (_options.AutoCreateWorkOrder
            && caseEntity.WorkOrderId is null
            && (RecallHelpers.RequiresHold(caseEntity.Status, caseEntity.ReadinessImpact)
                || RecallHelpers.IsVerifiedOpenStatus(caseEntity.Status)))
        {
            await CreateWorkOrderInternalAsync(tenantId, actorUserId, actorPersonId, caseEntity.AssetId, caseEntity.Id, false, cancellationToken);
        }
    }

    private static bool ReadinessNeedsHold(AssetRecallCase caseEntity, RecallCampaign campaign) =>
        RecallHelpers.RequiresHold(caseEntity.Status, caseEntity.ReadinessImpact)
        || string.Equals(caseEntity.Status, RecallCaseStatuses.VinConfirmedOpen, StringComparison.OrdinalIgnoreCase)
        || string.Equals(caseEntity.Status, RecallCaseStatuses.SerialConfirmedOpen, StringComparison.OrdinalIgnoreCase)
        || string.Equals(caseEntity.ReadinessImpact, RecallReadinessImpacts.ParkIt, StringComparison.OrdinalIgnoreCase)
        || string.Equals(caseEntity.ReadinessImpact, RecallReadinessImpacts.ParkOutside, StringComparison.OrdinalIgnoreCase)
        || string.Equals(caseEntity.ReadinessImpact, RecallReadinessImpacts.DoNotDrive, StringComparison.OrdinalIgnoreCase)
        || string.Equals(caseEntity.ReadinessImpact, RecallReadinessImpacts.OutOfService, StringComparison.OrdinalIgnoreCase);

    private async Task<AssetQualityHoldResponse> CreateHoldAsync(
        Guid tenantId,
        Guid actorUserId,
        string? actorPersonId,
        RecallCampaign campaign,
        AssetRecallCase caseEntity,
        CancellationToken cancellationToken)
    {
        var hold = await assetQualityHoldService.CreateAsync(
            tenantId,
            actorUserId,
            new CreateAssetQualityHoldRequest(
                caseEntity.AssetId,
                "recall_hold",
                "maintainarr",
                caseEntity.Id.ToString("D"),
                $"{campaign.CampaignNumberLabel()} - {campaign.Component}",
                BuildHoldDescription(campaign, caseEntity),
                readinessPolicy.DetermineHoldSeverity(campaign, caseEntity),
                actorPersonId),
            cancellationToken);

        caseEntity.ReadinessHoldId = hold.HoldId;
        return hold;
    }

    private async Task<AssetQualityHoldResponse> ReleaseHoldAsync(
        Guid tenantId,
        Guid actorUserId,
        string? actorPersonId,
        RecallCampaign campaign,
        AssetRecallCase caseEntity,
        CancellationToken cancellationToken) =>
        await ReleaseHoldAsync(
            tenantId,
            actorUserId,
            actorPersonId,
            campaign,
            caseEntity,
            new ReleaseRecallHoldRequest(actorPersonId, "Recall hold released."),
            cancellationToken);

    private async Task<AssetQualityHoldResponse> ReleaseHoldAsync(
        Guid tenantId,
        Guid actorUserId,
        string? actorPersonId,
        RecallCampaign campaign,
        AssetRecallCase caseEntity,
        ReleaseRecallHoldRequest request,
        CancellationToken cancellationToken)
    {
        if (!caseEntity.ReadinessHoldId.HasValue)
        {
            throw new StlApiException("recall.hold_not_found", "Recall hold was not found.", 404);
        }

        var hold = await assetQualityHoldService.ReleaseAsync(
            tenantId,
            actorUserId,
            caseEntity.ReadinessHoldId.Value,
            new ReleaseAssetQualityHoldRequest(caseEntity.ReadinessHoldId.Value, request.ReleasedByPersonId, request.ReleaseReason),
            cancellationToken);
        caseEntity.ReadinessHoldId = null;
        return hold;
    }

    private async Task<WorkOrderDetailResponse> CreateWorkOrderInternalAsync(
        Guid tenantId,
        Guid actorUserId,
        string? actorPersonId,
        Guid assetId,
        Guid caseId,
        bool inspectionItem,
        CancellationToken cancellationToken)
    {
        var caseEntity = await GetCaseAsync(tenantId, assetId, caseId, cancellationToken);

        if (caseEntity.WorkOrderId.HasValue)
        {
            return await workOrderService.GetAsync(tenantId, caseEntity.WorkOrderId.Value, cancellationToken);
        }

        var asset = await db.Assets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == caseEntity.AssetId, cancellationToken)
            ?? throw new StlApiException("asset.not_found", "Asset was not found.", 404);

        var titlePrefix = inspectionItem ? "Inspection" : "Recall";
        var title = $"{titlePrefix}: {caseEntity.RecallCampaign.Component} / {caseEntity.RecallCampaign.CampaignNumberLabel()}";
        var description = BuildWorkOrderDescription(caseEntity.RecallCampaign, caseEntity);
        var draftPlan = JsonSerializer.Serialize(new
        {
            recallCampaignId = caseEntity.RecallCampaignId,
            recallCaseId = caseEntity.Id,
            assetId = caseEntity.AssetId,
            assetTag = asset.AssetTag,
            campaignNumber = caseEntity.RecallCampaign.CampaignNumberLabel(),
            component = caseEntity.RecallCampaign.Component,
            matchBasis = caseEntity.MatchBasis,
            status = caseEntity.Status,
            readinessImpact = caseEntity.ReadinessImpact,
            verificationStatus = caseEntity.VerificationStatus,
            actionType = inspectionItem ? RecallActionTypes.InspectionItem : RecallActionTypes.WorkOrder,
        }, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        var request = new CreateWorkOrderRequest(
            caseEntity.AssetId,
            title,
            description,
            ResolveRecallPriority(caseEntity),
            null,
            null,
            null,
            draftPlan,
            null,
            null,
            WorkOrderSources.Recall,
            inspectionItem ? WorkOrderTypes.InspectionFollowup : WorkOrderTypes.Recall,
            WorkOrderOriginTypes.RecallNotice,
            caseEntity.RecallCampaign.CampaignNumberLabel());

        var workOrder = inspectionItem
            ? await workOrderService.CreateDraftAsync(tenantId, actorUserId, request, cancellationToken)
            : await workOrderService.CreateAsync(tenantId, actorUserId, request, cancellationToken);

        caseEntity.WorkOrderId = workOrder.WorkOrderId;
        caseEntity.ActionType = inspectionItem ? RecallActionTypes.InspectionItem : RecallActionTypes.WorkOrder;
        caseEntity.ActionStatus = caseEntity.ActionStatus == RecallActionStatuses.Completed
            ? caseEntity.ActionStatus
            : RecallActionStatuses.Open;
        caseEntity.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "recall.work_order_create",
            tenantId,
            actorUserId,
            actorPersonId,
            "asset_recall_case",
            caseEntity.Id.ToString("D"),
            workOrder.Status,
            cancellationToken: cancellationToken);

        return workOrder;
    }

    private static string ResolveRecallPriority(AssetRecallCase caseEntity)
    {
        if (RecallHelpers.IsVerifiedOpenStatus(caseEntity.Status))
        {
            return string.Equals(caseEntity.ReadinessImpact, RecallReadinessImpacts.DoNotDrive, StringComparison.OrdinalIgnoreCase)
                || string.Equals(caseEntity.ReadinessImpact, RecallReadinessImpacts.OutOfService, StringComparison.OrdinalIgnoreCase)
                ? WorkOrderPriorities.Urgent
                : WorkOrderPriorities.High;
        }

        if (string.Equals(caseEntity.ReadinessImpact, RecallReadinessImpacts.ParkIt, StringComparison.OrdinalIgnoreCase)
            || string.Equals(caseEntity.ReadinessImpact, RecallReadinessImpacts.ParkOutside, StringComparison.OrdinalIgnoreCase))
        {
            return WorkOrderPriorities.High;
        }

        return WorkOrderPriorities.Medium;
    }

    private static string BuildWorkOrderDescription(RecallCampaign campaign, AssetRecallCase caseEntity)
    {
        var parts = new List<string>
        {
            $"Campaign: {campaign.CampaignNumberLabel()}",
            $"Source: {campaign.SourceProvider} ({campaign.SourceType})",
            $"Component: {campaign.Component}",
            $"Status: {caseEntity.Status}",
            $"Readiness impact: {caseEntity.ReadinessImpact}",
            $"Verification: {caseEntity.VerificationStatus}",
            campaign.Manufacturer,
            campaign.Summary,
            campaign.Consequence,
            campaign.Remedy,
        };

        if (!string.IsNullOrWhiteSpace(campaign.Notes))
        {
            parts.Add($"Notes: {campaign.Notes}");
        }

        return string.Join("\n\n", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
    }

    private static string BuildHoldDescription(RecallCampaign campaign, AssetRecallCase caseEntity)
    {
        var parts = new[]
        {
            $"Campaign: {campaign.CampaignNumberLabel()}",
            $"Component: {campaign.Component}",
            $"Status: {caseEntity.Status}",
            $"Readiness impact: {caseEntity.ReadinessImpact}",
            campaign.Summary,
            campaign.Consequence,
            campaign.Remedy,
        };

        return string.Join("\n\n", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
    }

    private static string BuildMatchReason(RecallAssetProfile profile, RecallCampaign campaign, RecallCampaignSeed seed)
    {
        var basis = seed.SourceProvider switch
        {
            RecallSourceTypes.Nhtsa => $"NHTSA campaign match for {profile.ModelYear} {profile.Make} {profile.Model}.",
            _ => $"Recall campaign match for {profile.AssetTag}.",
        };

        var warnings = new List<string>();
        if (campaign.ParkIt)
        {
            warnings.Add("park it");
        }
        else if (campaign.ParkOutside)
        {
            warnings.Add("park outside");
        }

        if (campaign.OverTheAirUpdate)
        {
            warnings.Add("OTA update available");
        }

        return warnings.Count == 0 ? basis : $"{basis} {string.Join(", ", warnings)}.";
    }

    private static string NormalizeVerificationStatusToCaseStatus(string verificationStatus) =>
        verificationStatus.ToLowerInvariant() switch
        {
            RecallCaseStatuses.VinConfirmedOpen => RecallCaseStatuses.VinConfirmedOpen,
            RecallCaseStatuses.SerialConfirmedOpen => RecallCaseStatuses.SerialConfirmedOpen,
            RecallCaseStatuses.ConfirmedNotApplicable => RecallCaseStatuses.ConfirmedNotApplicable,
            RecallCaseStatuses.CompletedClaimed => RecallCaseStatuses.CompletedClaimed,
            RecallCaseStatuses.CompletedVerified => RecallCaseStatuses.CompletedVerified,
            _ => RecallCaseStatuses.NeedsManualReview,
        };

    private async Task<IReadOnlyList<RecallCampaignResponse>> MapCampaignsAsync(
        Guid tenantId,
        IReadOnlyList<Guid> campaignIds,
        CancellationToken cancellationToken)
    {
        if (campaignIds.Count == 0)
        {
            return [];
        }

        var campaigns = await db.RecallCampaigns
            .AsNoTracking()
            .Include(x => x.Applicabilities)
            .Where(x => x.TenantId == tenantId && campaignIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        return (await Task.WhenAll(campaigns.Select(campaign => MapCampaignAsync(tenantId, campaign, cancellationToken)))).ToArray();
    }

    private async Task<RecallCampaignResponse> MapCampaignAsync(
        Guid tenantId,
        RecallCampaign campaign,
        CancellationToken cancellationToken)
    {
        var caseEntities = await db.AssetRecallCases
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.RecallCampaignId == campaign.Id)
            .ToListAsync(cancellationToken);

        var counts = new
        {
            Total = caseEntities.Count,
            Open = caseEntities.Count(x => !RecallHelpers.IsResolvedCaseStatus(x.Status)),
            VerifiedOpen = caseEntities.Count(x => RecallHelpers.IsVerifiedOpenStatus(x.Status)),
        };

        var applicabilities = campaign.Applicabilities
            .OrderBy(x => x.CreatedAt)
            .Select(MapApplicability)
            .ToList();

        return new RecallCampaignResponse(
            campaign.Id,
            campaign.SourceProvider,
            campaign.SourceType,
            campaign.SourceProviderRecordId,
            campaign.NhtsaCampaignNumber,
            campaign.NhtsaActionNumber,
            campaign.ManufacturerCampaignNumber,
            campaign.CampaignTitle,
            campaign.Manufacturer,
            campaign.Component,
            campaign.ReportReceivedDate,
            campaign.CampaignStartDate,
            campaign.CampaignEndDate,
            campaign.CampaignStatus,
            campaign.PotentialUnitsAffected,
            campaign.Summary,
            campaign.Consequence,
            campaign.Remedy,
            campaign.Notes,
            campaign.ParkIt,
            campaign.ParkOutside,
            campaign.OverTheAirUpdate,
            campaign.RecallType,
            campaign.SourceUrl,
            campaign.FetchedAt,
            applicabilities,
            counts?.Total ?? 0,
            counts?.Open ?? 0,
            counts?.VerifiedOpen ?? 0,
            campaign.CreatedAt,
            campaign.UpdatedAt);
    }

    private static RecallCampaignApplicabilityResponse MapApplicability(RecallCampaignApplicability entity) =>
        new(
            entity.Id,
            entity.RecallCampaignId,
            entity.ModelYear,
            entity.Make,
            entity.Model,
            entity.AssetClass,
            entity.AssetType,
            entity.BodyClass,
            entity.VehicleType,
            entity.FuelType,
            entity.EngineFamily,
            entity.EngineManufacturer,
            entity.ComponentCategory,
            entity.TireBrand,
            entity.TireLine,
            entity.TireSize,
            entity.EquipmentMake,
            entity.EquipmentModel,
            entity.SerialRangeStart,
            entity.SerialRangeEnd,
            entity.ProductionStartDate,
            entity.ProductionEndDate,
            entity.Notes,
            entity.CreatedAt,
            entity.UpdatedAt);

    private AssetRecallCaseResponse MapCaseResponse(AssetRecallCase entity)
    {
        var campaign = entity.RecallCampaign;
        var primaryApplicability = campaign.Applicabilities.OrderBy(x => x.CreatedAt).FirstOrDefault();

        return new AssetRecallCaseResponse(
            entity.Id,
            entity.AssetId,
            entity.RecallCampaignId,
            campaign.NhtsaCampaignNumber ?? campaign.ManufacturerCampaignNumber ?? campaign.SourceProviderRecordId ?? entity.RecallCampaignId.ToString("D"),
            campaign.CampaignTitle,
            campaign.Manufacturer,
            campaign.Component,
            campaign.Summary,
            campaign.Consequence,
            campaign.Remedy,
            campaign.Notes,
            primaryApplicability?.ModelYear?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            primaryApplicability?.Make ?? string.Empty,
            primaryApplicability?.Model ?? string.Empty,
            campaign.ReportReceivedDate,
            campaign.SourceProvider,
            campaign.SourceType,
            campaign.SourceUrl,
            campaign.FetchedAt,
            entity.MatchBasis,
            entity.MatchConfidence,
            entity.MatchScore,
            entity.Status,
            entity.ReadinessImpact,
            entity.Reason,
            entity.VerificationStatus,
            entity.VerificationSource,
            entity.VerificationMethod,
            entity.VerifiedByPersonId,
            entity.VerifiedAt,
            entity.DismissedByPersonId,
            entity.DismissedAt,
            entity.DismissalReason,
            campaign.ParkIt,
            campaign.ParkOutside,
            campaign.OverTheAirUpdate,
            entity.EvidenceDocumentId,
            entity.EvidenceUrl,
            entity.EvidenceText,
            entity.WorkOrderId,
            entity.InspectionRunId,
            entity.DefectId,
            entity.ReadinessHoldId,
            entity.ActionType,
            entity.ActionStatus,
            entity.DetectedAt,
            entity.LastRefreshedAt,
            entity.CreatedAt,
            entity.UpdatedAt);
    }

    private async Task<AssetRecallCase> GetCaseAsync(
        Guid tenantId,
        Guid assetId,
        Guid caseId,
        CancellationToken cancellationToken)
    {
        return await db.AssetRecallCases
            .Include(x => x.RecallCampaign)
                .ThenInclude(x => x.Applicabilities)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.AssetId == assetId && x.Id == caseId, cancellationToken)
            ?? throw new StlApiException("recall.case_not_found", "Recall case was not found.", 404);
    }

    private async Task<RecallAssetProfile> LoadAssetProfileAsync(
        Guid tenantId,
        Guid assetId,
        CancellationToken cancellationToken)
    {
        var asset = await db.Assets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == assetId, cancellationToken)
            ?? throw new StlApiException("asset.not_found", "Asset was not found.", 404);

        var fieldContext = await assetService.GetFieldContextAsync(tenantId, assetId, cancellationToken);
        var fieldValues = fieldContext.Fields.ToDictionary(
            field => field.Key,
            field => NormalizeFieldValue(field.DisplayValue, field.StoredValue),
            StringComparer.OrdinalIgnoreCase);

        var vin = NormalizeOptional(ResolveString(fieldValues, "VIN", "vin"), 17)
            ?? await ResolveVinAsync(tenantId, assetId, fieldValues, cancellationToken);

        var modelYear = TryParseInt(ResolveString(fieldValues, "modelYear", "year", "ModelYear", "Year"));
        var make = NormalizeOptional(ResolveString(fieldValues, "make", "Make"), 128);
        var model = NormalizeOptional(ResolveString(fieldValues, "model", "Model"), 128);

        if ((modelYear is null || string.IsNullOrWhiteSpace(make) || string.IsNullOrWhiteSpace(model)) && !string.IsNullOrWhiteSpace(vin))
        {
            var decoded = await LoadLatestVinDecodeSnapshotAsync(tenantId, assetId, cancellationToken);
            modelYear ??= decoded.ModelYear;
            make ??= decoded.Make;
            model ??= decoded.Model;
        }

        return new RecallAssetProfile(
            asset.Id,
            asset.AssetTag,
            asset.Name,
            asset.SiteRef,
            vin,
            modelYear,
            make,
            model);
    }

    private async Task<(int? ModelYear, string? Make, string? Model)> LoadLatestVinDecodeSnapshotAsync(
        Guid tenantId,
        Guid assetId,
        CancellationToken cancellationToken)
    {
        var snapshot = await db.AssetEnrichmentSnapshots
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.AssetId == assetId && x.SnapshotType == "vin_decode")
            .OrderByDescending(x => x.CapturedAt)
            .ThenByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (snapshot is null)
        {
            return (null, null, null);
        }

        try
        {
            using var document = JsonDocument.Parse(snapshot.PayloadJson);
            if (!document.RootElement.TryGetProperty("decodedFields", out var decodedFields))
            {
                return (null, null, null);
            }

            var modelYear = TryParseInt(ReadJsonString(decodedFields, "modelYear"));
            var make = NormalizeOptional(ReadJsonString(decodedFields, "make"), 128);
            var model = NormalizeOptional(ReadJsonString(decodedFields, "model"), 128);
            return (modelYear, make, model);
        }
        catch
        {
            return (null, null, null);
        }
    }

    private static string? ReadJsonString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.ToString(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => value.ToString(),
        };
    }

    private static string? NormalizeFieldValue(string? displayValue, object? storedValue)
    {
        var value = storedValue?.ToString() ?? displayValue;
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string? ResolveString(
        IReadOnlyDictionary<string, string?> values,
        params string[] keys)
    {
        foreach (var key in keys)
        {
            if (values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }

    private async Task<string?> ResolveVinAsync(
        Guid tenantId,
        Guid assetId,
        IReadOnlyDictionary<string, string?> values,
        CancellationToken cancellationToken)
    {
        if (values.TryGetValue("VIN", out var vin) && !string.IsNullOrWhiteSpace(vin))
        {
            return vin;
        }

        var identifier = await db.AssetExternalIdentifiers
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.AssetId == assetId)
            .Where(x => string.Equals(x.IdentifierType, "vin", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.IsPrimary)
            .ThenByDescending(x => x.IsVerified)
            .ThenByDescending(x => x.ObservedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return identifier?.IdentifierValue;
    }

    private async Task<AssetRecallCaseResponse> MapCaseResponseAsync(Guid tenantId, Guid caseId, CancellationToken cancellationToken)
    {
        var entity = await db.AssetRecallCases
            .AsNoTracking()
            .Include(x => x.RecallCampaign)
                .ThenInclude(x => x.Applicabilities)
            .FirstAsync(x => x.TenantId == tenantId && x.Id == caseId, cancellationToken);
        return MapCaseResponse(entity);
    }

    private static int? TryParseInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }

    private static string NormalizeComparisonKey(string value)
    {
        var normalized = value.Trim();
        return new string(normalized
            .Where(character => char.IsLetterOrDigit(character) || char.IsWhiteSpace(character))
            .ToArray())
            .Replace(" ", string.Empty)
            .ToUpperInvariant();
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        return normalized.Length > maxLength ? normalized[..maxLength] : normalized;
    }

    private DateTimeOffset NextReviewAt(AssetRecallCase caseEntity) =>
        (caseEntity.LastRefreshedAt ?? caseEntity.DetectedAt).AddDays(_options.DefaultRecheckDays);

    private RecallDashboardItemResponse? BuildDashboardItem(
        AssetRecallCase caseEntity,
        IReadOnlyDictionary<Guid, Asset> assets)
    {
        if (!assets.TryGetValue(caseEntity.AssetId, out var asset))
        {
            return null;
        }

        return new RecallDashboardItemResponse(
            caseEntity.Id,
            caseEntity.AssetId,
            asset.AssetTag,
            asset.Name,
            caseEntity.RecallCampaign.NhtsaCampaignNumber ?? caseEntity.RecallCampaign.ManufacturerCampaignNumber ?? caseEntity.RecallCampaign.SourceProviderRecordId ?? caseEntity.RecallCampaignId.ToString("D"),
            caseEntity.RecallCampaign.Component,
            caseEntity.MatchBasis,
            caseEntity.MatchConfidence,
            caseEntity.Status,
            caseEntity.ReadinessImpact,
            caseEntity.RecallCampaign.SourceProvider,
            caseEntity.DetectedAt,
            caseEntity.LastRefreshedAt,
            NextReviewAt(caseEntity),
            caseEntity.WorkOrderId,
            asset.SiteRef);
    }

    private static string BuildProviderCaseReason(RecallCampaign campaign, string assetTag) =>
        $"{campaign.CampaignNumberLabel()} applies to {assetTag} and requires manual review.";

    private static string NormalizeMatchBasis(string? value) =>
        NormalizeOptional(value, 64) ?? RecallMatchBases.Provider;

    private static string NormalizeCampaignStatus(string? value) =>
        NormalizeOptional(value, 32) ?? RecallCampaignStatuses.Unknown;

    private static string NormalizeReadinessImpact(string? value) =>
        NormalizeOptional(value, 64) ?? RecallReadinessImpacts.Advisory;

    private static string NormalizeSourceType(string? value) =>
        NormalizeOptional(value, 64) ?? RecallSourceTypes.Manual;

    private static string NormalizeSourceProvider(string? value) =>
        NormalizeOptional(value, 64) ?? RecallSourceTypes.Manual;
}

internal static class RecallCampaignExtensions
{
    public static string CampaignNumberLabel(this RecallCampaign campaign) =>
        campaign.NhtsaCampaignNumber
        ?? campaign.ManufacturerCampaignNumber
        ?? campaign.SourceProviderRecordId
        ?? campaign.Id.ToString("D");
}

internal sealed record RecallAssetProfile(
    Guid AssetId,
    string AssetTag,
    string AssetName,
    string? SiteRef,
    string? Vin,
    int? ModelYear,
    string? Make,
    string? Model);
