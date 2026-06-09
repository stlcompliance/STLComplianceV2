using System.Globalization;
using System.Text.Json;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using MaintainArr.Api.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services.ExternalIntelligence;

public sealed class ExternalIntelligenceService(
    MaintainArrDbContext db,
    NhtsaExternalIntelligenceClient nhtsaClient,
    AssetQualityHoldService assetQualityHoldService,
    WorkOrderService workOrderService,
    IMaintainArrAuditService audit,
    IOptions<ExternalIntelligenceOptions> options)
{
    private readonly ExternalIntelligenceOptions _options = options.Value;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
    };

    private static readonly HashSet<string> SpecFieldKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "make",
        "manufacturer",
        "model",
        "modelYear",
        "series",
        "trim",
        "configuration",
        "cabType",
        "bodyType",
        "drivetrain",
        "axleConfiguration",
        "tireConfiguration",
        "fuelType",
        "aftertreatmentType",
        "hybridType",
        "brakeType",
        "brakeSystemType",
        "trailerType",
        "meterType",
        "primaryMeterType",
        "meterUnit",
        "usageProfile",
        "telematicsProvider",
        "diagnosticProtocol",
        "faultCodeStandard",
    };

    private static readonly HashSet<string> ComponentFieldKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "engineMake",
        "engineModel",
        "transmissionMake",
        "transmissionModel",
        "tireSize",
        "wheelSize",
        "wheelMaterial",
        "suspensionType",
        "parkingBrakeType",
    };

    private static readonly IReadOnlyList<ExternalIntelligenceProviderSummaryResponse> ProviderRegistry =
    [
        new(
            "nhtsa",
            "NHTSA / vPIC",
            "VIN decode, recalls, complaints, and plant/reference lookups.",
            "NHTSA",
            "active",
            true,
            true,
            true,
            true,
            true,
            null,
            null,
            null),
        new(
            "transport-canada",
            "Transport Canada",
            "Placeholder for future Transport Canada vehicle and recall mirrors.",
            "Transport Canada",
            "planned",
            false,
            false,
            false,
            false,
            false,
            null,
            null,
            "Not yet implemented."),
        new(
            "fmcsa",
            "FMCSA",
            "Placeholder for future FMCSA compliance and equipment integrations.",
            "FMCSA",
            "planned",
            false,
            false,
            false,
            false,
            false,
            null,
            null,
            "Not yet implemented."),
        new(
            "epa",
            "EPA",
            "Placeholder for future EPA equipment and emissions references.",
            "EPA",
            "planned",
            false,
            false,
            false,
            false,
            false,
            null,
            null,
            "Not yet implemented."),
        new(
            "carb",
            "CARB",
            "Placeholder for future California emissions and equipment references.",
            "CARB",
            "planned",
            false,
            false,
            false,
            false,
            false,
            null,
            null,
            "Not yet implemented."),
        new(
            "parts-reference",
            "Parts Reference",
            "Placeholder for future external parts fitment and interchange references.",
            "External Parts Reference",
            "planned",
            false,
            false,
            false,
            false,
            false,
            null,
            null,
            "Not yet implemented."),
    ];

    public IReadOnlyList<ExternalIntelligenceProviderSummaryResponse> GetProviders() =>
        ProviderRegistry.ToArray();

    public async Task<IReadOnlyList<ExternalProviderHealthResponse>> GetProviderHealthAsync(CancellationToken cancellationToken = default)
    {
        var health = new List<ExternalProviderHealthResponse>(ProviderRegistry.Count);
        foreach (var provider in ProviderRegistry)
        {
            if (string.Equals(provider.ProviderKey, "nhtsa", StringComparison.OrdinalIgnoreCase))
            {
                health.Add(await nhtsaClient.GetHealthAsync(cancellationToken));
                continue;
            }

            health.Add(new ExternalProviderHealthResponse(
                provider.ProviderKey,
                provider.Status,
                provider.LastError ?? "Provider is not enabled yet.",
                DateTimeOffset.UtcNow,
                null));
        }

        return health;
    }

    public async Task<AssetExternalIntelligenceOverviewResponse> GetOverviewAsync(
        Guid tenantId,
        Guid assetId,
        CancellationToken cancellationToken = default)
    {
        var asset = await GetAssetAsync(tenantId, assetId, cancellationToken);
        var assetFieldValues = await LoadAssetFieldValuesAsync(tenantId, assetId, cancellationToken);

        var identifiers = await db.AssetExternalIdentifiers.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.AssetId == assetId)
            .OrderByDescending(x => x.IsPrimary)
            .ThenBy(x => x.SourceSystem)
            .ThenBy(x => x.IdentifierType)
            .ThenBy(x => x.IdentifierValue)
            .Select(x => new ExternalAssetIdentifierResponse(
                x.Id,
                x.AssetId,
                x.SourceSystem,
                x.IdentifierType,
                x.IdentifierValue,
                x.NormalizedValue,
                x.IsPrimary,
                x.IsVerified,
                ParseMetadata(x.MetadataJson),
                x.ObservedAt,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        var snapshots = await db.AssetEnrichmentSnapshots.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.AssetId == assetId)
            .OrderByDescending(x => x.CapturedAt)
            .ThenByDescending(x => x.CreatedAt)
            .Take(12)
            .Select(x => new AssetEnrichmentSnapshotResponse(
                x.Id,
                x.AssetId,
                x.ProviderKey,
                x.SnapshotType,
                x.SourceObjectRef,
                x.Summary,
                BuildSnapshotDetails(x.PayloadJson, x.SnapshotType),
                x.CapturedAt,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        var suggestions = await db.AssetEnrichmentSuggestions.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.AssetId == assetId)
            .OrderByDescending(x => x.UpdatedAt)
            .ThenBy(x => x.FieldLabel)
            .Select(x => new AssetEnrichmentSuggestionResponse(
                x.Id,
                x.AssetId,
                x.SnapshotId,
                x.ProviderKey,
                x.FieldKey,
                x.FieldLabel,
                x.CurrentValue,
                x.ProposedValue,
                x.Reason,
                x.Confidence,
                x.Status,
                x.ReviewedByPersonId,
                x.ReviewedAt,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        var recalls = await db.AssetRecallSnapshots.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.AssetId == assetId)
            .OrderByDescending(x => x.CapturedAt)
            .ThenBy(x => x.CampaignNumber)
            .Select(x => new AssetRecallSnapshotResponse(
                x.Id,
                x.AssetId,
                x.ProviderKey,
                x.CampaignNumber,
                x.ActionNumber,
                x.Manufacturer,
                x.Component,
                x.Summary,
                x.Consequence,
                x.Remedy,
                x.Notes,
                x.ModelYear,
                x.Make,
                x.Model,
                x.ReportReceivedDate,
                x.Status,
                x.QualityHoldId,
                x.CapturedAt,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        var complaintSnapshot = await db.AssetEnrichmentSnapshots.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.AssetId == assetId && x.SnapshotType == "complaint_refresh")
            .OrderByDescending(x => x.CapturedAt)
            .ThenByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var complaints = complaintSnapshot is null
            ? []
            : ParseComplaintSignals(complaintSnapshot.PayloadJson).Take(5).ToList();

        var summary = new AssetExternalIntelligenceSummaryResponse(
            identifiers.Count,
            snapshots.Count,
            suggestions.Count,
            recalls.Count(recall => string.Equals(recall.Status, "active", StringComparison.OrdinalIgnoreCase)),
            complaints.Count,
            snapshots.FirstOrDefault()?.CapturedAt ?? complaintSnapshot?.CapturedAt ?? recalls.FirstOrDefault()?.CapturedAt);

        return new AssetExternalIntelligenceOverviewResponse(
            asset.Id,
            GetVinValue(assetFieldValues, identifiers),
            ProviderRegistry.ToArray(),
            summary,
            identifiers,
            snapshots,
            suggestions,
            recalls,
            complaints);
    }

    public async Task<IReadOnlyList<ExternalAssetIdentifierResponse>> ListIdentifiersAsync(
        Guid tenantId,
        Guid assetId,
        CancellationToken cancellationToken = default)
    {
        return await db.AssetExternalIdentifiers.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.AssetId == assetId)
            .OrderByDescending(x => x.IsPrimary)
            .ThenBy(x => x.SourceSystem)
            .ThenBy(x => x.IdentifierType)
            .ThenBy(x => x.IdentifierValue)
            .Select(x => new ExternalAssetIdentifierResponse(
                x.Id,
                x.AssetId,
                x.SourceSystem,
                x.IdentifierType,
                x.IdentifierValue,
                x.NormalizedValue,
                x.IsPrimary,
                x.IsVerified,
                ParseMetadata(x.MetadataJson),
                x.ObservedAt,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AssetEnrichmentSnapshotResponse>> ListSnapshotsAsync(
        Guid tenantId,
        Guid assetId,
        string? snapshotType = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.AssetEnrichmentSnapshots.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.AssetId == assetId);
        if (!string.IsNullOrWhiteSpace(snapshotType))
        {
            query = query.Where(x => x.SnapshotType == snapshotType);
        }

        return await query
            .OrderByDescending(x => x.CapturedAt)
            .ThenByDescending(x => x.CreatedAt)
            .Select(x => new AssetEnrichmentSnapshotResponse(
                x.Id,
                x.AssetId,
                x.ProviderKey,
                x.SnapshotType,
                x.SourceObjectRef,
                x.Summary,
                BuildSnapshotDetails(x.PayloadJson, x.SnapshotType),
                x.CapturedAt,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AssetEnrichmentSuggestionResponse>> ListSuggestionsAsync(
        Guid tenantId,
        Guid assetId,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.AssetEnrichmentSuggestions.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.AssetId == assetId);
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }

        return await query
            .OrderByDescending(x => x.UpdatedAt)
            .ThenBy(x => x.FieldLabel)
            .Select(x => new AssetEnrichmentSuggestionResponse(
                x.Id,
                x.AssetId,
                x.SnapshotId,
                x.ProviderKey,
                x.FieldKey,
                x.FieldLabel,
                x.CurrentValue,
                x.ProposedValue,
                x.Reason,
                x.Confidence,
                x.Status,
                x.ReviewedByPersonId,
                x.ReviewedAt,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AssetRecallSnapshotResponse>> ListRecallsAsync(
        Guid tenantId,
        Guid assetId,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.AssetRecallSnapshots.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.AssetId == assetId);
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }

        return await query
            .OrderByDescending(x => x.CapturedAt)
            .ThenBy(x => x.CampaignNumber)
            .Select(x => new AssetRecallSnapshotResponse(
                x.Id,
                x.AssetId,
                x.ProviderKey,
                x.CampaignNumber,
                x.ActionNumber,
                x.Manufacturer,
                x.Component,
                x.Summary,
                x.Consequence,
                x.Remedy,
                x.Notes,
                x.ModelYear,
                x.Make,
                x.Model,
                x.ReportReceivedDate,
                x.Status,
                x.QualityHoldId,
                x.CapturedAt,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<ExternalVinDecodeResponse> DecodeVinAsync(
        Guid tenantId,
        string vin,
        int? modelYear,
        Guid? assetId = null,
        bool persist = false,
        CancellationToken cancellationToken = default)
    {
        var normalizedVin = NormalizeVin(vin);
        var envelope = await nhtsaClient.DecodeVinAsync(tenantId, normalizedVin, modelYear, cancellationToken);
        var result = envelope.Results.FirstOrDefault() ?? new NhtsaVinDecodeResult { VIN = normalizedVin };
        var assetFieldValues = assetId.HasValue
            ? await LoadAssetFieldValuesAsync(tenantId, assetId.Value, cancellationToken)
            : new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        var decodedFields = BuildDecodedFields(result, normalizedVin, modelYear);
        var suggestions = BuildSuggestions(
            tenantId,
            assetId,
            decodedFields,
            assetFieldValues,
            result,
            normalizedVin,
            persist,
            snapshotId: null);
        var identifiers = BuildIdentifiers(assetId, normalizedVin, result);

        Guid? snapshotId = null;
        DateTimeOffset? capturedAt = null;

        if (persist && assetId.HasValue)
        {
            var snapshot = await UpsertDecodeSnapshotAsync(
                tenantId,
                assetId.Value,
                normalizedVin,
                result,
                decodedFields,
                suggestions,
                identifiers,
                cancellationToken);
            snapshotId = snapshot.Id;
            capturedAt = snapshot.CapturedAt;
        }

        return new ExternalVinDecodeResponse(
            "nhtsa",
            normalizedVin,
            normalizedVin,
            modelYear ?? TryParseInt(result.ModelYear),
            normalizedVin.Contains('*') || normalizedVin.Length < 17,
            envelope.SearchCriteria,
            envelope.Message,
            result.ErrorCode,
            result.ErrorText,
            result.AdditionalErrorText,
            decodedFields,
            suggestions,
            identifiers,
            snapshotId,
            capturedAt);
    }

    public async Task<IReadOnlyList<ExternalVinDecodeBatchItemResponse>> DecodeVinBatchAsync(
        Guid tenantId,
        ExternalVinDecodeBatchRequest request,
        CancellationToken cancellationToken = default)
    {
        var results = new List<ExternalVinDecodeBatchItemResponse>();
        foreach (var item in request.Items.Take(_options.MaxBatchSize))
        {
            try
            {
                var decoded = await DecodeVinAsync(tenantId, item.Vin, item.ModelYear, null, false, cancellationToken);
                results.Add(new ExternalVinDecodeBatchItemResponse(item.Vin, item.ModelYear, decoded, null));
            }
            catch (Exception ex)
            {
                results.Add(new ExternalVinDecodeBatchItemResponse(item.Vin, item.ModelYear, null, ex.Message));
            }
        }

        return results;
    }

    public async Task<AssetExternalIntelligenceOverviewResponse> RefreshAssetAsync(
        Guid tenantId,
        Guid actorUserId,
        string? actorPersonId,
        Guid assetId,
        CancellationToken cancellationToken = default)
    {
        await GetAssetAsync(tenantId, assetId, cancellationToken);
        var fieldValues = await LoadAssetFieldValuesAsync(tenantId, assetId, cancellationToken);
        var vin = ResolveVin(fieldValues);
        if (string.IsNullOrWhiteSpace(vin))
        {
            throw new StlApiException(
                "external_intelligence.vin_required",
                "An asset VIN is required before external intelligence can be refreshed.",
                400);
        }

        var decode = await DecodeVinAsync(tenantId, vin, TryParseInt(fieldValues.GetValueOrDefault("modelYear")), assetId, persist: true, cancellationToken);
        var decodedMake = decode.DecodedFields.GetValueOrDefault("Make") ?? fieldValues.GetValueOrDefault("make");
        var decodedModel = decode.DecodedFields.GetValueOrDefault("Model") ?? fieldValues.GetValueOrDefault("model");
        var decodedModelYear = decode.DecodedFields.GetValueOrDefault("ModelYear") ?? fieldValues.GetValueOrDefault("modelYear");
        var parsedYear = TryParseInt(decodedModelYear) ?? decode.ModelYear;

        if (!string.IsNullOrWhiteSpace(decodedMake) && !string.IsNullOrWhiteSpace(decodedModel) && parsedYear.HasValue)
        {
            var recallEnvelope = await nhtsaClient.GetRecallsByVehicleAsync(tenantId, decodedMake!, decodedModel!, parsedYear.Value, cancellationToken);
            await PersistRecallsAsync(
                tenantId,
                actorUserId,
                actorPersonId,
                assetId,
                decode.NormalizedVin,
                recallEnvelope,
                cancellationToken);

            var complaintEnvelope = await nhtsaClient.GetComplaintsByVehicleAsync(tenantId, decodedMake!, decodedModel!, parsedYear.Value, cancellationToken);
            await PersistComplaintSnapshotAsync(
                tenantId,
                assetId,
                decode.NormalizedVin,
                complaintEnvelope,
                cancellationToken);
        }

        return await GetOverviewAsync(tenantId, assetId, cancellationToken);
    }

    public async Task<AssetEnrichmentSuggestionResponse> AcceptSuggestionAsync(
        Guid tenantId,
        Guid actorUserId,
        string? actorPersonId,
        Guid assetId,
        Guid suggestionId,
        CancellationToken cancellationToken = default)
    {
        var suggestion = await GetSuggestionEntityAsync(tenantId, assetId, suggestionId, cancellationToken);
        if (string.Equals(suggestion.Status, "accepted", StringComparison.OrdinalIgnoreCase))
        {
            return await MapSuggestionAsync(suggestion, cancellationToken);
        }

        suggestion.Status = "accepted";
        suggestion.ReviewedByPersonId = actorPersonId;
        suggestion.ReviewedAt = DateTimeOffset.UtcNow;
        suggestion.UpdatedAt = suggestion.ReviewedAt.Value;

        if (!string.IsNullOrWhiteSpace(suggestion.ProposedValue))
        {
            await UpsertControlledValueAsync(
                tenantId,
                assetId,
                suggestion.FieldKey,
                suggestion.ProposedValue,
                suggestion.ReviewedAt.Value,
                cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "external_intelligence.suggestion_accept",
            tenantId,
            actorUserId,
            "asset_enrichment_suggestion",
            suggestion.Id.ToString(),
            suggestion.Status,
            cancellationToken: cancellationToken);

        return await MapSuggestionAsync(suggestion, cancellationToken);
    }

    public async Task<AssetEnrichmentSuggestionResponse> RejectSuggestionAsync(
        Guid tenantId,
        Guid actorUserId,
        string? actorPersonId,
        Guid assetId,
        Guid suggestionId,
        CancellationToken cancellationToken = default)
    {
        var suggestion = await GetSuggestionEntityAsync(tenantId, assetId, suggestionId, cancellationToken);
        suggestion.Status = "rejected";
        suggestion.ReviewedByPersonId = actorPersonId;
        suggestion.ReviewedAt = DateTimeOffset.UtcNow;
        suggestion.UpdatedAt = suggestion.ReviewedAt.Value;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "external_intelligence.suggestion_reject",
            tenantId,
            actorUserId,
            "asset_enrichment_suggestion",
            suggestion.Id.ToString(),
            suggestion.Status,
            cancellationToken: cancellationToken);

        return await MapSuggestionAsync(suggestion, cancellationToken);
    }

    public async Task<WorkOrderDetailResponse> CreateRecallWorkOrderAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid assetId,
        Guid recallId,
        CancellationToken cancellationToken = default)
    {
        var asset = await GetAssetAsync(tenantId, assetId, cancellationToken);
        var recall = await db.AssetRecallSnapshots
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.AssetId == assetId && x.Id == recallId, cancellationToken)
            ?? throw new StlApiException("external_intelligence.recall_not_found", "Recall snapshot was not found.", 404);

        var title = $"{recall.CampaignNumber} - {recall.Component}";
        var description = string.Join(
            "\n\n",
            new[]
            {
                recall.Summary,
                recall.Consequence,
                recall.Remedy,
                string.IsNullOrWhiteSpace(recall.Notes) ? null : $"Notes: {recall.Notes}",
            }.Where(part => !string.IsNullOrWhiteSpace(part)));

        var draftPlan = JsonSerializer.Serialize(new
        {
            source = "nhtsa_recall",
            campaignNumber = recall.CampaignNumber,
            recallId = recall.Id,
            assetId = asset.Id,
            assetTag = asset.AssetTag,
            make = recall.Make,
            model = recall.Model,
            modelYear = recall.ModelYear,
            component = recall.Component,
        }, JsonOptions);

        var request = new CreateWorkOrderRequest(
            asset.Id,
            title,
            description,
            WorkOrderPriorities.High,
            null,
            null,
            null,
            draftPlan);

        return await workOrderService.CreateDraftAsync(tenantId, actorUserId, request, cancellationToken);
    }

    public async Task<IReadOnlyList<ReferenceOptionResponse>> GetMakesAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var envelope = await nhtsaClient.GetAllMakesAsync(tenantId, cancellationToken);
        return envelope.Results
            .Where(result => !string.IsNullOrWhiteSpace(result.Make_ID) || !string.IsNullOrWhiteSpace(result.Make_Name))
            .OrderBy(result => result.Make_Name)
            .Select(result =>
            {
                var key = NormalizeReferenceValue(result.Make_ID ?? result.Make_Name ?? string.Empty);
                var label = string.IsNullOrWhiteSpace(result.Make_Name) ? key : result.Make_Name!.Trim();
                return new ReferenceOptionResponse(
                    key,
                    result.Make_ID,
                    label,
                    "nhtsa_reference",
                    "NHTSA",
                    key,
                    label,
                    true);
            })
            .ToList();
    }

    public async Task<IReadOnlyList<ReferenceOptionResponse>> GetManufacturersAsync(
        Guid tenantId,
        string? manufacturerType = null,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        var envelope = await nhtsaClient.GetAllManufacturersAsync(tenantId, manufacturerType, page, cancellationToken);
        return envelope.Results
            .Where(result => !string.IsNullOrWhiteSpace(result.Mfr_ID) || !string.IsNullOrWhiteSpace(result.Mfr_Name))
            .OrderBy(result => result.Mfr_Name)
            .Select(result =>
            {
                var key = NormalizeReferenceValue(result.Mfr_ID ?? result.Mfr_Name ?? string.Empty);
                var label = string.IsNullOrWhiteSpace(result.Mfr_Name) ? key : result.Mfr_Name!.Trim();
                return new ReferenceOptionResponse(
                    key,
                    result.Mfr_ID,
                    label,
                    "nhtsa_reference",
                    "NHTSA",
                    key,
                    label,
                    true);
            })
            .ToList();
    }

    public async Task<IReadOnlyList<ReferenceOptionResponse>> GetModelsForMakeAsync(
        Guid tenantId,
        string make,
        int? modelYear = null,
        string? vehicleType = null,
        CancellationToken cancellationToken = default)
    {
        var envelope = await nhtsaClient.GetModelsForMakeAsync(tenantId, make, modelYear, vehicleType, cancellationToken);
        return envelope.Results
            .Where(result => !string.IsNullOrWhiteSpace(result.Model_ID) || !string.IsNullOrWhiteSpace(result.Model_Name))
            .OrderBy(result => result.Model_Name)
            .Select(result =>
            {
                var key = NormalizeReferenceValue(result.Model_ID ?? result.Model_Name ?? string.Empty);
                var label = string.IsNullOrWhiteSpace(result.Model_Name) ? key : result.Model_Name!.Trim();
                return new ReferenceOptionResponse(
                    key,
                    result.Model_ID,
                    label,
                    "nhtsa_reference",
                    "NHTSA",
                    key,
                    label,
                    true);
            })
            .ToList();
    }

    public async Task<IReadOnlyList<ReferenceOptionResponse>> GetEquipmentPlantCodesAsync(
        Guid tenantId,
        int year,
        string? equipmentType = null,
        string? reportType = null,
        CancellationToken cancellationToken = default)
    {
        var envelope = await nhtsaClient.GetEquipmentPlantCodesAsync(tenantId, year, equipmentType, reportType, cancellationToken);
        return envelope.Results
            .Where(result => !string.IsNullOrWhiteSpace(result.PlantID) || !string.IsNullOrWhiteSpace(result.PlantName))
            .OrderBy(result => result.PlantName)
            .Select(result =>
            {
                var key = NormalizeReferenceValue(result.PlantID ?? result.PlantName ?? string.Empty);
                var label = string.IsNullOrWhiteSpace(result.PlantName) ? key : result.PlantName!.Trim();
                var metadata = new Dictionary<string, object?>
                {
                    ["equipmentType"] = result.EquipmentType,
                    ["dotCode"] = result.DotCode,
                    ["dotCodeOld"] = result.DotCodeOld,
                    ["status"] = result.Status,
                    ["reportType"] = result.ReportType,
                    ["year"] = result.Year,
                };

                return new ReferenceOptionResponse(
                    key,
                    result.PlantID,
                    label,
                    "nhtsa_reference",
                    "NHTSA",
                    key,
                    label,
                    true);
            })
            .ToList();
    }

    private async Task<Asset> GetAssetAsync(Guid tenantId, Guid assetId, CancellationToken cancellationToken)
    {
        return await db.Assets.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == assetId, cancellationToken)
            ?? throw new StlApiException("assets.not_found", "Asset was not found.", 404);
    }

    private async Task<Dictionary<string, string?>> LoadAssetFieldValuesAsync(
        Guid tenantId,
        Guid assetId,
        CancellationToken cancellationToken)
    {
        var fieldValues = await db.AssetCustomFieldValues.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.AssetId == assetId)
            .ToListAsync(cancellationToken);

        var dictionary = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var field in fieldValues)
        {
            dictionary[field.FieldKey] = DeserializeScalar(field.ValueJson);
        }

        return dictionary;
    }

    private static string? DeserializeScalar(string valueJson)
    {
        try
        {
            var value = JsonSerializer.Deserialize<object?>(valueJson, JsonOptions);
            return value?.ToString();
        }
        catch
        {
            return valueJson;
        }
    }

    private async Task<AssetEnrichmentSnapshot> UpsertDecodeSnapshotAsync(
        Guid tenantId,
        Guid assetId,
        string vin,
        NhtsaVinDecodeResult result,
        IReadOnlyDictionary<string, string?> decodedFields,
        IReadOnlyList<AssetEnrichmentSuggestionResponse> suggestions,
        IReadOnlyList<ExternalAssetIdentifierResponse> identifiers,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var snapshot = new AssetEnrichmentSnapshot
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AssetId = assetId,
            ProviderKey = "nhtsa",
            SnapshotType = "vin_decode",
            SourceObjectRef = vin,
            Summary = BuildDecodeSummaryText(decodedFields),
            PayloadJson = JsonSerializer.Serialize(new
            {
                vin,
                decodedFields,
                suggestions = suggestions.Select(suggestion => new
                {
                    suggestion.FieldKey,
                    suggestion.FieldLabel,
                    suggestion.CurrentValue,
                    suggestion.ProposedValue,
                    suggestion.Reason,
                    suggestion.Confidence,
                }),
                identifiers = identifiers.Select(identifier => new
                {
                    identifier.IdentifierType,
                    identifier.IdentifierValue,
                    identifier.NormalizedValue,
                }),
            }, JsonOptions),
            CapturedAt = now,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.AssetEnrichmentSnapshots.Add(snapshot);
        await db.SaveChangesAsync(cancellationToken);

        await UpsertDecodeSuggestionsAsync(tenantId, assetId, snapshot.Id, suggestions, cancellationToken);
        await UpsertDecodeIdentifiersAsync(tenantId, assetId, identifiers, cancellationToken);

        await audit.WriteAsync(
            "external_intelligence.decode.persist",
            tenantId,
            Guid.Empty,
            "asset_enrichment_snapshot",
            snapshot.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return snapshot;
    }

    private async Task UpsertDecodeSuggestionsAsync(
        Guid tenantId,
        Guid assetId,
        Guid snapshotId,
        IReadOnlyList<AssetEnrichmentSuggestionResponse> suggestions,
        CancellationToken cancellationToken)
    {
        var entities = await db.AssetEnrichmentSuggestions
            .Where(x => x.TenantId == tenantId && x.AssetId == assetId)
            .ToListAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        foreach (var suggestion in suggestions)
        {
            var entity = entities.FirstOrDefault(x =>
                string.Equals(x.ProviderKey, suggestion.ProviderKey, StringComparison.OrdinalIgnoreCase)
                && string.Equals(x.FieldKey, suggestion.FieldKey, StringComparison.OrdinalIgnoreCase));

            if (entity is null)
            {
                entity = new AssetEnrichmentSuggestion
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    AssetId = assetId,
                    SnapshotId = snapshotId,
                    ProviderKey = suggestion.ProviderKey,
                    FieldKey = suggestion.FieldKey,
                    FieldLabel = suggestion.FieldLabel,
                    CreatedAt = now,
                };
                db.AssetEnrichmentSuggestions.Add(entity);
            }

            entity.SnapshotId = snapshotId;
            entity.FieldLabel = suggestion.FieldLabel;
            entity.CurrentValue = suggestion.CurrentValue;
            entity.ProposedValue = suggestion.ProposedValue;
            entity.Reason = suggestion.Reason;
            entity.Confidence = suggestion.Confidence;
            entity.Status = suggestion.Status;
            entity.ReviewedByPersonId = suggestion.ReviewedByPersonId;
            entity.ReviewedAt = suggestion.ReviewedAt;
            entity.UpdatedAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task UpsertDecodeIdentifiersAsync(
        Guid tenantId,
        Guid assetId,
        IReadOnlyList<ExternalAssetIdentifierResponse> identifiers,
        CancellationToken cancellationToken)
    {
        var entities = await db.AssetExternalIdentifiers
            .Where(x => x.TenantId == tenantId && x.AssetId == assetId)
            .ToListAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        foreach (var identifier in identifiers)
        {
            var entity = entities.FirstOrDefault(x =>
                string.Equals(x.SourceSystem, identifier.SourceSystem, StringComparison.OrdinalIgnoreCase)
                && string.Equals(x.IdentifierType, identifier.IdentifierType, StringComparison.OrdinalIgnoreCase)
                && string.Equals(x.NormalizedValue, identifier.NormalizedValue, StringComparison.OrdinalIgnoreCase));

            if (entity is null)
            {
                entity = new AssetExternalIdentifier
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    AssetId = assetId,
                    SourceSystem = identifier.SourceSystem,
                    IdentifierType = identifier.IdentifierType,
                    IdentifierValue = identifier.IdentifierValue,
                    NormalizedValue = identifier.NormalizedValue,
                    IsPrimary = identifier.IsPrimary,
                    IsVerified = identifier.IsVerified,
                    MetadataJson = SerializeMetadata(identifier.Metadata),
                    ObservedAt = identifier.ObservedAt,
                    CreatedAt = now,
                };
                db.AssetExternalIdentifiers.Add(entity);
            }

            entity.IdentifierValue = identifier.IdentifierValue;
            entity.IsPrimary = identifier.IsPrimary;
            entity.IsVerified = identifier.IsVerified;
            entity.MetadataJson = SerializeMetadata(identifier.Metadata);
            entity.ObservedAt = identifier.ObservedAt;
            entity.UpdatedAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task PersistRecallsAsync(
        Guid tenantId,
        Guid actorUserId,
        string? actorPersonId,
        Guid assetId,
        string vin,
        NhtsaEnvelope<NhtsaRecallResult> envelope,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var existing = await db.AssetRecallSnapshots
            .Where(x => x.TenantId == tenantId && x.AssetId == assetId)
            .ToListAsync(cancellationToken);

        var activeCampaigns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var recall in envelope.Results)
        {
            var campaignNumber = recall.NHTSACampaignNumber?.Trim();
            if (string.IsNullOrWhiteSpace(campaignNumber))
            {
                continue;
            }

            activeCampaigns.Add(campaignNumber);
            var entity = existing.FirstOrDefault(x => string.Equals(x.CampaignNumber, campaignNumber, StringComparison.OrdinalIgnoreCase));
            if (entity is null)
            {
                entity = new AssetRecallSnapshot
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    AssetId = assetId,
                    ProviderKey = "nhtsa",
                    CampaignNumber = campaignNumber,
                    CreatedAt = now,
                };
                db.AssetRecallSnapshots.Add(entity);
            }

            entity.ActionNumber = recall.NHTSAActionNumber?.Trim();
            entity.Manufacturer = recall.Manufacturer?.Trim() ?? string.Empty;
            entity.Component = recall.Component?.Trim() ?? string.Empty;
            entity.Summary = recall.Summary?.Trim() ?? string.Empty;
            entity.Consequence = recall.Consequence?.Trim() ?? string.Empty;
            entity.Remedy = recall.Remedy?.Trim() ?? string.Empty;
            entity.Notes = recall.Notes?.Trim() ?? string.Empty;
            entity.ModelYear = recall.ModelYear?.Trim();
            entity.Make = recall.Make?.Trim();
            entity.Model = recall.Model?.Trim();
            entity.ReportReceivedDate = recall.ReportReceivedDate?.Trim();
            entity.Status = "active";
            entity.CapturedAt = now;
            entity.UpdatedAt = now;

            var hold = await assetQualityHoldService.CreateAsync(
                tenantId,
                actorUserId,
                new CreateAssetQualityHoldRequest(
                    assetId,
                    "nhtsa_recall",
                    "nhtsa",
                    campaignNumber,
                    $"{campaignNumber} - {entity.Component}",
                    string.Join(" ", new[]
                    {
                        entity.Summary,
                        entity.Consequence,
                        entity.Remedy,
                    }.Where(part => !string.IsNullOrWhiteSpace(part))),
                    "high",
                    actorPersonId),
                cancellationToken);
            entity.QualityHoldId = hold.HoldId;
        }

        foreach (var entity in existing.Where(x => !activeCampaigns.Contains(x.CampaignNumber) && string.Equals(x.Status, "active", StringComparison.OrdinalIgnoreCase)))
        {
            if (entity.QualityHoldId.HasValue)
            {
                await assetQualityHoldService.ReleaseAsync(
                    tenantId,
                    actorUserId,
                    entity.QualityHoldId.Value,
                    new ReleaseAssetQualityHoldRequest(
                        entity.QualityHoldId.Value,
                        actorPersonId,
                        "No longer returned by the latest NHTSA recall lookup."),
                    cancellationToken);
            }

            entity.Status = "resolved";
            entity.UpdatedAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "external_intelligence.recall.refresh",
            tenantId,
            actorUserId,
            "asset_recall_snapshot",
            assetId.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);
    }

    private async Task PersistComplaintSnapshotAsync(
        Guid tenantId,
        Guid assetId,
        string vin,
        NhtsaEnvelope<NhtsaComplaintResult> envelope,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var snapshot = new AssetEnrichmentSnapshot
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AssetId = assetId,
            ProviderKey = "nhtsa",
            SnapshotType = "complaint_refresh",
            SourceObjectRef = vin,
            Summary = $"{envelope.Count} complaint signal(s) returned by NHTSA.",
            PayloadJson = JsonSerializer.Serialize(envelope.Results.Select(result => new
            {
                result.OdiNumber,
                result.Manufacturer,
                result.Crash,
                result.Fire,
                result.NumberOfInjuries,
                result.NumberOfDeaths,
                result.DateOfIncident,
                result.DateComplaintFiled,
                result.Vin,
                result.Components,
                result.Summary,
            }), JsonOptions),
            CapturedAt = now,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.AssetEnrichmentSnapshots.Add(snapshot);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "external_intelligence.complaint.refresh",
            tenantId,
            Guid.Empty,
            "asset_enrichment_snapshot",
            snapshot.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);
    }

    private static IReadOnlyDictionary<string, string?> BuildDecodedFields(
        NhtsaVinDecodeResult result,
        string vin,
        int? modelYear)
    {
        var fields = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["VIN"] = vin,
            ["Make"] = Clean(result.Make),
            ["Manufacturer"] = Clean(result.Manufacturer),
            ["Model"] = Clean(result.Model),
            ["ModelYear"] = modelYear?.ToString(CultureInfo.InvariantCulture) ?? Clean(result.ModelYear),
            ["VehicleType"] = Clean(result.VehicleType),
            ["BodyClass"] = Clean(result.BodyClass),
            ["BrakeSystemType"] = Clean(result.BrakeSystemType),
            ["DriveType"] = Clean(result.DriveType),
            ["FuelTypePrimary"] = Clean(result.FuelTypePrimary),
            ["FuelTypeSecondary"] = Clean(result.FuelTypeSecondary),
            ["EngineManufacturer"] = Clean(result.EngineManufacturer),
            ["EngineModel"] = Clean(result.EngineModel),
            ["EngineConfiguration"] = Clean(result.EngineConfiguration),
            ["EngineCylinders"] = Clean(result.EngineCylinders),
            ["DisplacementL"] = Clean(result.DisplacementL),
            ["GVWR"] = Clean(result.GVWR),
            ["PlantCompanyName"] = Clean(result.PlantCompanyName),
            ["PlantCountry"] = Clean(result.PlantCountry),
            ["PlantState"] = Clean(result.PlantState),
            ["PlantCity"] = Clean(result.PlantCity),
            ["Trim"] = Clean(result.Trim),
            ["Series"] = Clean(result.Series),
            ["VehicleDescriptor"] = Clean(result.VehicleDescriptor),
        };

        var assetClass = InferAssetClass(result);
        if (!string.IsNullOrWhiteSpace(assetClass))
        {
            fields["assetClass"] = assetClass;
        }

        var assetType = InferAssetType(result);
        if (!string.IsNullOrWhiteSpace(assetType))
        {
            fields["assetType"] = assetType;
        }

        return fields;
    }

    private IReadOnlyList<AssetEnrichmentSuggestionResponse> BuildSuggestions(
        Guid tenantId,
        Guid? assetId,
        IReadOnlyDictionary<string, string?> decodedFields,
        IReadOnlyDictionary<string, string?> currentValues,
        NhtsaVinDecodeResult result,
        string vin,
        bool persist,
        Guid? snapshotId)
    {
        var now = DateTimeOffset.UtcNow;
        var suggestions = new List<AssetEnrichmentSuggestionResponse>();

        foreach (var definition in GetSuggestionDefinitions(result, vin))
        {
            if (string.IsNullOrWhiteSpace(definition.Value))
            {
                continue;
            }

            var current = currentValues.GetValueOrDefault(definition.FieldKey);
            if (!ShouldSurfaceSuggestion(current, definition.Value))
            {
                continue;
            }

            suggestions.Add(new AssetEnrichmentSuggestionResponse(
                Guid.NewGuid(),
                assetId ?? Guid.Empty,
                snapshotId,
                "nhtsa",
                definition.FieldKey,
                definition.FieldLabel,
                current,
                definition.Value,
                definition.Reason,
                definition.Confidence,
                persist ? "pending" : "preview",
                null,
                null,
                now,
                now));
        }

        if (assetId is Guid assetGuid)
        {
            return suggestions
                .Select(suggestion => suggestion with { AssetId = assetGuid })
                .ToList();
        }

        return suggestions;
    }

    private static IEnumerable<(string FieldKey, string FieldLabel, string? Value, string Reason, double Confidence)> GetSuggestionDefinitions(
        NhtsaVinDecodeResult result,
        string vin)
    {
        yield return ("VIN", "VIN", vin, "VIN entered for decode.", 1.0);
        yield return ("make", "Make", Clean(result.Make), "NHTSA decoded the vehicle make.", 0.99);
        yield return ("manufacturer", "Manufacturer", Clean(result.Manufacturer), "NHTSA decoded the manufacturer.", 0.98);
        yield return ("model", "Model", Clean(result.Model), "NHTSA decoded the model.", 0.98);
        yield return ("modelYear", "Model Year", Clean(result.ModelYear), "NHTSA decoded the model year.", 0.99);
        yield return ("series", "Series", Clean(result.Series), "NHTSA decoded the vehicle series.", 0.85);
        yield return ("trim", "Trim", Clean(result.Trim), "NHTSA decoded the trim.", 0.85);
        yield return ("bodyType", "Body Type", InferBodyType(result), "Body class was normalized to a MaintainArr body type.", 0.82);
        yield return ("fuelType", "Fuel Type", NormalizeFuelType(result), "Primary fuel type was normalized for the asset fieldset.", 0.8);
        yield return ("brakeSystemType", "Brake System Type", Clean(result.BrakeSystemType), "Brake system type decoded from NHTSA.", 0.8);
        yield return ("driveType", "Drive Type", Clean(result.DriveType), "Drive type decoded from NHTSA.", 0.8);
        yield return ("engineMake", "Engine Make", Clean(result.EngineManufacturer), "Engine manufacturer decoded from NHTSA.", 0.88);
        yield return ("engineModel", "Engine Model", Clean(result.EngineModel), "Engine model decoded from NHTSA.", 0.88);
        yield return ("configuration", "Configuration", InferConfiguration(result), "Configuration inferred from body class and vehicle descriptor.", 0.7);
        yield return ("cabType", "Cab Type", InferCabType(result), "Cab type inferred from vehicle descriptors.", 0.65);
        yield return ("assetClass", "Asset Class", InferAssetClass(result), "Asset class inferred from the decoded vehicle type.", 0.7);
        yield return ("assetType", "Asset Type", InferAssetType(result), "Asset type inferred from the decoded vehicle type.", 0.6);
    }

    private async Task UpsertControlledValueAsync(
        Guid tenantId,
        Guid assetId,
        string fieldKey,
        string? value,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var serialized = JsonSerializer.Serialize(value, JsonOptions);

        var custom = await db.AssetCustomFieldValues
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.AssetId == assetId && x.FieldKey == fieldKey,
                cancellationToken);
        if (custom is null)
        {
            custom = new AssetCustomFieldValue
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                AssetId = assetId,
                FieldKey = fieldKey,
                ValueJson = serialized,
                CreatedAt = now,
            };
            db.AssetCustomFieldValues.Add(custom);
        }
        else
        {
            custom.ValueJson = serialized;
            custom.UpdatedAt = now;
        }

        if (SpecFieldKeys.Contains(fieldKey))
        {
            var spec = await db.AssetSpecs
                .FirstOrDefaultAsync(
                    x => x.TenantId == tenantId && x.AssetId == assetId && x.SpecKey == fieldKey,
                    cancellationToken);
            if (spec is null)
            {
                spec = new AssetSpec
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    AssetId = assetId,
                    SpecKey = fieldKey,
                    ValueJson = serialized,
                    CreatedAt = now,
                };
                db.AssetSpecs.Add(spec);
            }
            else
            {
                spec.ValueJson = serialized;
                spec.UpdatedAt = now;
            }
        }

        if (ComponentFieldKeys.Contains(fieldKey))
        {
            var component = await db.AssetComponents
                .FirstOrDefaultAsync(
                    x => x.TenantId == tenantId && x.AssetId == assetId && x.ComponentKey == fieldKey,
                    cancellationToken);
            if (component is null)
            {
                component = new AssetComponent
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    AssetId = assetId,
                    ComponentKey = fieldKey,
                    ValueJson = serialized,
                    CreatedAt = now,
                };
                db.AssetComponents.Add(component);
            }
            else
            {
                component.ValueJson = serialized;
                component.UpdatedAt = now;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<AssetEnrichmentSuggestionResponse> MapSuggestionAsync(
        AssetEnrichmentSuggestion entity,
        CancellationToken cancellationToken)
    {
        return new AssetEnrichmentSuggestionResponse(
            entity.Id,
            entity.AssetId,
            entity.SnapshotId,
            entity.ProviderKey,
            entity.FieldKey,
            entity.FieldLabel,
            entity.CurrentValue,
            entity.ProposedValue,
            entity.Reason,
            entity.Confidence,
            entity.Status,
            entity.ReviewedByPersonId,
            entity.ReviewedAt,
            entity.CreatedAt,
            entity.UpdatedAt);
    }

    private async Task<AssetEnrichmentSuggestion> GetSuggestionEntityAsync(
        Guid tenantId,
        Guid assetId,
        Guid suggestionId,
        CancellationToken cancellationToken)
    {
        return await db.AssetEnrichmentSuggestions
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.AssetId == assetId && x.Id == suggestionId, cancellationToken)
            ?? throw new StlApiException("external_intelligence.suggestion_not_found", "Suggestion was not found.", 404);
    }

    private async Task<IEnumerable<string?>> LoadRecallCampaignNumbersAsync(
        Guid tenantId,
        Guid assetId,
        CancellationToken cancellationToken)
    {
        return await db.AssetRecallSnapshots.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.AssetId == assetId && string.Equals(x.Status, "active", StringComparison.OrdinalIgnoreCase))
            .Select(x => x.CampaignNumber)
            .ToListAsync(cancellationToken);
    }

    private static ExternalAssetIdentifierResponse[] BuildIdentifiers(
        Guid? assetId,
        string vin,
        NhtsaVinDecodeResult result)
    {
        var identifiers = new List<ExternalAssetIdentifierResponse>();
        var now = DateTimeOffset.UtcNow;
        if (!string.IsNullOrWhiteSpace(vin))
        {
            identifiers.Add(CreateIdentifier(assetId, "nhtsa", "vin", vin, vin, true, true, new Dictionary<string, object?>
            {
                ["make"] = result.Make,
                ["model"] = result.Model,
                ["modelYear"] = result.ModelYear,
            }, now));
        }

        if (!string.IsNullOrWhiteSpace(vin) && vin!.Length >= 3)
        {
            var wmi = vin[..3];
            identifiers.Add(CreateIdentifier(assetId, "nhtsa", "wmi", wmi, wmi, false, false, new Dictionary<string, object?>
            {
                ["vehicleDescriptor"] = result.VehicleDescriptor,
            }, now));
        }

        if (!string.IsNullOrWhiteSpace(result.MakeID))
        {
            identifiers.Add(CreateIdentifier(assetId, "nhtsa", "make_id", result.MakeID!, result.MakeID!, false, false, null, now));
        }

        if (!string.IsNullOrWhiteSpace(result.ManufacturerId))
        {
            identifiers.Add(CreateIdentifier(assetId, "nhtsa", "manufacturer_id", result.ManufacturerId!, result.ManufacturerId!, false, false, null, now));
        }

        if (!string.IsNullOrWhiteSpace(result.ModelID))
        {
            identifiers.Add(CreateIdentifier(assetId, "nhtsa", "model_id", result.ModelID!, result.ModelID!, false, false, null, now));
        }

        if (!string.IsNullOrWhiteSpace(result.VehicleDescriptor))
        {
            identifiers.Add(CreateIdentifier(assetId, "nhtsa", "vehicle_descriptor", result.VehicleDescriptor!, result.VehicleDescriptor!, false, false, null, now));
        }

        return identifiers.ToArray();
    }

    private static ExternalAssetIdentifierResponse CreateIdentifier(
        Guid? assetId,
        string sourceSystem,
        string identifierType,
        string identifierValue,
        string normalizedValue,
        bool isPrimary,
        bool isVerified,
        IReadOnlyDictionary<string, object?>? metadata,
        DateTimeOffset observedAt) =>
        new(
            Guid.NewGuid(),
            assetId ?? Guid.Empty,
            sourceSystem,
            identifierType,
            identifierValue,
            normalizedValue,
            isPrimary,
            isVerified,
            metadata?.ToDictionary(item => item.Key, item => item.Value?.ToString()),
            observedAt,
            observedAt,
            observedAt);

    private static string SerializeMetadata(IReadOnlyDictionary<string, string?>? metadata)
    {
        if (metadata is null || metadata.Count == 0)
        {
            return "{}";
        }

        return JsonSerializer.Serialize(metadata, JsonOptions);
    }

    private static IReadOnlyDictionary<string, string?>? ParseMetadata(string metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson) || string.Equals(metadataJson, "{}", StringComparison.Ordinal))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string?>>(metadataJson, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private static IReadOnlyDictionary<string, string?> BuildSnapshotDetails(string payloadJson, string snapshotType)
    {
        if (string.IsNullOrWhiteSpace(payloadJson))
        {
            return new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        }

        try
        {
            if (string.Equals(snapshotType, "complaint_refresh", StringComparison.OrdinalIgnoreCase))
            {
                var complaints = JsonSerializer.Deserialize<List<NhtsaComplaintResult>>(payloadJson, JsonOptions) ?? [];
                var first = complaints.FirstOrDefault();
                return new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    ["count"] = complaints.Count.ToString(CultureInfo.InvariantCulture),
                    ["topSummary"] = first?.Summary,
                    ["manufacturer"] = first?.Manufacturer,
                    ["vin"] = first?.Vin,
                };
            }

            if (string.Equals(snapshotType, "vin_decode", StringComparison.OrdinalIgnoreCase))
            {
                var decode = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(payloadJson, JsonOptions) ?? [];
                if (decode.TryGetValue("decodedFields", out var decodedFieldsElement)
                    && decodedFieldsElement.ValueKind == JsonValueKind.Object)
                {
                    var decodedFields = decodedFieldsElement.Deserialize<Dictionary<string, string?>>(JsonOptions) ?? [];
                    return BuildDecodeFieldSummary(decodedFields);
                }
            }

            var parsed = JsonSerializer.Deserialize<Dictionary<string, object?>>(payloadJson, JsonOptions) ?? [];
            return parsed.ToDictionary(
                item => item.Key,
                item => item.Value?.ToString(),
                StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private static IReadOnlyList<AssetComplaintSignalResponse> ParseComplaintSignals(string payloadJson)
    {
        try
        {
            var complaints = JsonSerializer.Deserialize<List<NhtsaComplaintResult>>(payloadJson, JsonOptions) ?? [];
            return complaints
                .Select(result => new AssetComplaintSignalResponse(
                    result.OdiNumber ?? string.Empty,
                    result.Manufacturer,
                    IsTruthy(result.Crash),
                    IsTruthy(result.Fire),
                    TryParseInt(result.NumberOfInjuries),
                    TryParseInt(result.NumberOfDeaths),
                    result.DateOfIncident,
                    result.DateComplaintFiled,
                    result.Vin,
                    result.Components ?? [],
                    result.Summary ?? string.Empty))
                .Where(item => !string.IsNullOrWhiteSpace(item.OdiNumber))
                .ToList();
        }
        catch
        {
            return [];
        }
    }

    private static IReadOnlyDictionary<string, string?> BuildDecodeFieldSummary(
        IReadOnlyDictionary<string, string?> decodedFields)
    {
        return new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["make"] = decodedFields.GetValueOrDefault("Make"),
            ["manufacturer"] = decodedFields.GetValueOrDefault("Manufacturer"),
            ["model"] = decodedFields.GetValueOrDefault("Model"),
            ["modelYear"] = decodedFields.GetValueOrDefault("ModelYear"),
            ["bodyClass"] = decodedFields.GetValueOrDefault("BodyClass"),
            ["vehicleType"] = decodedFields.GetValueOrDefault("VehicleType"),
            ["plantCompanyName"] = decodedFields.GetValueOrDefault("PlantCompanyName"),
        };
    }

    private static string BuildDecodeSummaryText(IReadOnlyDictionary<string, string?> decodedFields)
    {
        var modelYear = decodedFields.GetValueOrDefault("ModelYear");
        var make = decodedFields.GetValueOrDefault("Make");
        var model = decodedFields.GetValueOrDefault("Model");
        var parts = new[] { modelYear, make, model }.Where(part => !string.IsNullOrWhiteSpace(part)).ToArray();
        return parts.Length > 0
            ? $"VIN decoded to {string.Join(" ", parts)}."
            : "VIN decoded successfully.";
    }

    private static bool ShouldSurfaceSuggestion(string? current, string? proposed)
    {
        if (string.IsNullOrWhiteSpace(proposed))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(current))
        {
            return true;
        }

        return !string.Equals(current.Trim(), proposed.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private static string? ResolveVin(
        IReadOnlyDictionary<string, string?> fieldValues)
    {
        if (fieldValues.TryGetValue("VIN", out var vin) && !string.IsNullOrWhiteSpace(vin))
        {
            return vin;
        }

        return null;
    }

    private static string NormalizeReferenceValue(string value)
    {
        var trimmed = value.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            throw new StlApiException("external_intelligence.validation", "A reference value is required.", 400);
        }

        return trimmed;
    }

    private static string NormalizeVin(string value)
    {
        var normalized = value.Trim().ToUpperInvariant().Replace(" ", string.Empty);
        if (normalized.Length < 3 || normalized.Length > 17)
        {
            throw new StlApiException("external_intelligence.vin_invalid", "VIN must be between 3 and 17 characters.", 400);
        }

        foreach (var character in normalized)
        {
            if (character == '*')
            {
                continue;
            }

            if (!"ABCDEFGHJKLMNPRSTUVWXYZ0123456789".Contains(character))
            {
                throw new StlApiException(
                    "external_intelligence.vin_invalid",
                    "VIN contains invalid characters for NHTSA lookup.",
                    400);
            }
        }

        return normalized;
    }

    private static string? GetVinValue(
        IReadOnlyDictionary<string, string?> fieldValues,
        IReadOnlyList<ExternalAssetIdentifierResponse> identifiers)
    {
        if (fieldValues.TryGetValue("VIN", out var vin) && !string.IsNullOrWhiteSpace(vin))
        {
            return vin;
        }

        return identifiers.FirstOrDefault(identifier =>
            string.Equals(identifier.IdentifierType, "vin", StringComparison.OrdinalIgnoreCase))?.IdentifierValue;
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static bool IsTruthy(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return string.Equals(value.Trim(), "true", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value.Trim(), "y", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value.Trim(), "1", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value.Trim(), "yes", StringComparison.OrdinalIgnoreCase);
    }

    private static int? TryParseInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }

    private static string? InferAssetClass(NhtsaVinDecodeResult result)
    {
        var text = string.Join(' ', new[] { result.BodyClass, result.VehicleType }.Where(part => !string.IsNullOrWhiteSpace(part))).ToLowerInvariant();
        if (text.Contains("trailer"))
        {
            return "trailer";
        }

        if (text.Contains("forklift") || text.Contains("industrial truck"))
        {
            return "powered_industrial_truck";
        }

        if (text.Contains("tractor") || text.Contains("truck") || text.Contains("pickup") || text.Contains("van") || text.Contains("bus"))
        {
            return "vehicle";
        }

        return null;
    }

    private static string? InferAssetType(NhtsaVinDecodeResult result)
    {
        var text = string.Join(' ', new[] { result.BodyClass, result.VehicleType, result.VehicleDescriptor }.Where(part => !string.IsNullOrWhiteSpace(part))).ToLowerInvariant();
        if (text.Contains("pickup"))
        {
            return "pickup";
        }

        if (text.Contains("cargo van") || text.Contains("cargovan") || text.Contains("van"))
        {
            return "cargo_van";
        }

        if (text.Contains("box truck") || text.Contains("step van") || text.Contains("delivery"))
        {
            return "box_truck";
        }

        if (text.Contains("tractor"))
        {
            return "semi_tractor";
        }

        if (text.Contains("reefer") && text.Contains("trailer"))
        {
            return "reefer_trailer";
        }

        if (text.Contains("trailer"))
        {
            return "dry_van_trailer";
        }

        return null;
    }

    private static string? InferBodyType(NhtsaVinDecodeResult result)
    {
        var text = string.Join(' ', new[] { result.BodyClass, result.VehicleType }.Where(part => !string.IsNullOrWhiteSpace(part))).ToLowerInvariant();
        if (text.Contains("pickup"))
        {
            return "pickup";
        }

        if (text.Contains("cargo van") || text.Contains("van"))
        {
            return "cargo_van";
        }

        if (text.Contains("box"))
        {
            return "box";
        }

        if (text.Contains("tractor"))
        {
            return "tractor";
        }

        if (text.Contains("trailer"))
        {
            return "trailer";
        }

        return null;
    }

    private static string? NormalizeFuelType(NhtsaVinDecodeResult result)
    {
        var fuel = Clean(result.FuelTypePrimary);
        if (!string.IsNullOrWhiteSpace(fuel))
        {
            return fuel;
        }

        return Clean(result.FuelTypeSecondary);
    }

    private static string? InferConfiguration(NhtsaVinDecodeResult result)
    {
        var text = string.Join(' ', new[] { result.BodyClass, result.VehicleDescriptor }.Where(part => !string.IsNullOrWhiteSpace(part))).ToLowerInvariant();
        if (text.Contains("reefer"))
        {
            return "reefer";
        }

        if (text.Contains("liftgate"))
        {
            return "liftgate";
        }

        if (text.Contains("off-road") || text.Contains("off road"))
        {
            return "offroad";
        }

        return null;
    }

    private static string? InferCabType(NhtsaVinDecodeResult result)
    {
        var text = string.Join(' ', new[] { result.BodyClass, result.VehicleDescriptor }.Where(part => !string.IsNullOrWhiteSpace(part))).ToLowerInvariant();
        if (text.Contains("day cab"))
        {
            return "day_cab";
        }

        if (text.Contains("sleeper"))
        {
            return "sleeper_cab";
        }

        if (text.Contains("crew cab"))
        {
            return "crew_cab";
        }

        if (text.Contains("regular cab"))
        {
            return "regular_cab";
        }

        return null;
    }
}
