using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using MaintainArr.Api.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services.ExternalIntelligence;

public sealed class NhtsaExternalIntelligenceClient(
    HttpClient httpClient,
    IOptions<ExternalIntelligenceOptions> options,
    ExternalProviderCacheService cacheService)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly ExternalIntelligenceOptions _options = options.Value;

    public Task<NhtsaEnvelope<NhtsaVinDecodeResult>> DecodeVinAsync(
        Guid tenantId,
        string vin,
        int? modelYear,
        CancellationToken cancellationToken = default)
    {
        var normalizedVin = NormalizeVin(vin);
        var cacheKey = $"decode:{normalizedVin}:{(modelYear.HasValue ? modelYear.Value.ToString(CultureInfo.InvariantCulture) : "*")}";
        var requestJson = JsonSerializer.Serialize(new { vin = normalizedVin, modelYear });

        return cacheService.GetOrCreateAsync<NhtsaEnvelope<NhtsaVinDecodeResult>>(
            tenantId,
            "nhtsa",
            "vin_decode",
            cacheKey,
            TimeSpan.FromMinutes(_options.DecodeCacheMinutes),
            requestJson,
            async ct =>
            {
                var relativePath = BuildDecodePath(normalizedVin, modelYear);
                var response = await httpClient.GetAsync(relativePath, ct);
                var payload = await response.Content.ReadAsStringAsync(ct);
                EnsureSuccess(response, payload, "VIN decode");
                return ((int)response.StatusCode, payload);
            },
            cancellationToken);
    }

    public Task<NhtsaEnvelope<NhtsaVinDecodeResult>> DecodeVinBatchAsync(
        Guid tenantId,
        IReadOnlyList<ExternalVinDecodeBatchItemRequest> items,
        CancellationToken cancellationToken = default)
    {
        var normalizedItems = items
            .Take(_options.MaxBatchSize)
            .Select(item => new ExternalVinDecodeBatchItemRequest(NormalizeVin(item.Vin), item.ModelYear))
            .ToList();

        if (normalizedItems.Count == 0)
        {
            throw new StlApiException("external_intelligence.batch_empty", "At least one VIN is required for batch decode.", 400);
        }

        var requestData = string.Join(
            ";",
            normalizedItems.Select(item =>
                item.ModelYear.HasValue
                    ? $"{item.Vin},{item.ModelYear.Value.ToString(CultureInfo.InvariantCulture)}"
                    : item.Vin));

        var cacheKey = $"batch-decode:{requestData}";
        var requestJson = JsonSerializer.Serialize(normalizedItems);

        return cacheService.GetOrCreateAsync<NhtsaEnvelope<NhtsaVinDecodeResult>>(
            tenantId,
            "nhtsa",
            "vin_batch_decode",
            cacheKey,
            TimeSpan.FromMinutes(_options.DecodeCacheMinutes),
            requestJson,
            async ct =>
            {
                using var content = new FormUrlEncodedContent(
                    new Dictionary<string, string>
                    {
                        ["format"] = "json",
                        ["data"] = requestData,
                    });

                var response = await httpClient.PostAsync("vehicles/DecodeVINValuesBatch/", content, ct);
                var payload = await response.Content.ReadAsStringAsync(ct);
                EnsureSuccess(response, payload, "VIN batch decode");
                return ((int)response.StatusCode, payload);
            },
            cancellationToken);
    }

    public Task<NhtsaEnvelope<NhtsaMakeResult>> GetAllMakesAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return cacheService.GetOrCreateAsync<NhtsaEnvelope<NhtsaMakeResult>>(
            tenantId,
            "nhtsa",
            "all_makes",
            "all-makes",
            TimeSpan.FromMinutes(_options.ReferenceCacheMinutes),
            "{}",
            async ct =>
            {
                var response = await httpClient.GetAsync("vehicles/GetAllMakes?format=json", ct);
                var payload = await response.Content.ReadAsStringAsync(ct);
                EnsureSuccess(response, payload, "Get all makes");
                return ((int)response.StatusCode, payload);
            },
            cancellationToken);
    }

    public Task<NhtsaEnvelope<NhtsaManufacturerResult>> GetAllManufacturersAsync(
        Guid tenantId,
        string? manufacturerType = null,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        var normalizedType = string.IsNullOrWhiteSpace(manufacturerType) ? null : manufacturerType.Trim();
        var cacheKey = $"manufacturers:{page}:{normalizedType ?? "*"}";
        var requestJson = JsonSerializer.Serialize(new { manufacturerType = normalizedType, page });

        var query = new StringBuilder("vehicles/GetAllManufacturers?format=json");
        query.Append("&page=").Append(page.ToString(CultureInfo.InvariantCulture));
        if (!string.IsNullOrWhiteSpace(normalizedType))
        {
            query.Append("&ManufacturerType=").Append(Uri.EscapeDataString(normalizedType));
        }

        return cacheService.GetOrCreateAsync<NhtsaEnvelope<NhtsaManufacturerResult>>(
            tenantId,
            "nhtsa",
            "all_manufacturers",
            cacheKey,
            TimeSpan.FromMinutes(_options.ReferenceCacheMinutes),
            requestJson,
            async ct =>
            {
                var response = await httpClient.GetAsync(query.ToString(), ct);
                var payload = await response.Content.ReadAsStringAsync(ct);
                EnsureSuccess(response, payload, "Get all manufacturers");
                return ((int)response.StatusCode, payload);
            },
            cancellationToken);
    }

    public Task<NhtsaEnvelope<NhtsaModelResult>> GetModelsForMakeAsync(
        Guid tenantId,
        string make,
        int? modelYear = null,
        string? vehicleType = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedMake = NormalizeReferenceValue(make);
        var normalizedVehicleType = string.IsNullOrWhiteSpace(vehicleType) ? null : vehicleType.Trim();
        var cacheKey = $"models:{normalizedMake}:{modelYear?.ToString(CultureInfo.InvariantCulture) ?? "*"}:{normalizedVehicleType ?? "*"}";
        var requestJson = JsonSerializer.Serialize(new { make = normalizedMake, modelYear, vehicleType = normalizedVehicleType });

        var path = modelYear.HasValue
            ? $"vehicles/GetModelsForMakeYear/make/{Uri.EscapeDataString(normalizedMake)}/modelyear/{modelYear.Value.ToString(CultureInfo.InvariantCulture)}"
            : $"vehicles/GetModelsForMake/{Uri.EscapeDataString(normalizedMake)}";
        if (!string.IsNullOrWhiteSpace(normalizedVehicleType))
        {
            path += $"/vehicletype/{Uri.EscapeDataString(normalizedVehicleType)}";
        }
        path += "?format=json";

        return cacheService.GetOrCreateAsync<NhtsaEnvelope<NhtsaModelResult>>(
            tenantId,
            "nhtsa",
            "models_for_make",
            cacheKey,
            TimeSpan.FromMinutes(_options.ReferenceCacheMinutes),
            requestJson,
            async ct =>
            {
                var response = await httpClient.GetAsync(path, ct);
                var payload = await response.Content.ReadAsStringAsync(ct);
                EnsureSuccess(response, payload, "Get models for make");
                return ((int)response.StatusCode, payload);
            },
            cancellationToken);
    }

    public Task<NhtsaEnvelope<NhtsaEquipmentPlantCodeResult>> GetEquipmentPlantCodesAsync(
        Guid tenantId,
        int year,
        string? equipmentType = null,
        string? reportType = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedEquipmentType = string.IsNullOrWhiteSpace(equipmentType) ? null : equipmentType.Trim();
        var normalizedReportType = string.IsNullOrWhiteSpace(reportType) ? null : reportType.Trim();
        var cacheKey = $"equipment-plant:{year}:{normalizedEquipmentType ?? "*"}:{normalizedReportType ?? "*"}";
        var requestJson = JsonSerializer.Serialize(new { year, equipmentType = normalizedEquipmentType, reportType = normalizedReportType });

        var query = new StringBuilder($"vehicles/GetEquipmentPlantCodes/{year.ToString(CultureInfo.InvariantCulture)}?format=json");
        if (!string.IsNullOrWhiteSpace(normalizedEquipmentType))
        {
            query.Append("&equipmentType=").Append(Uri.EscapeDataString(normalizedEquipmentType));
        }
        if (!string.IsNullOrWhiteSpace(normalizedReportType))
        {
            query.Append("&reportType=").Append(Uri.EscapeDataString(normalizedReportType));
        }

        return cacheService.GetOrCreateAsync<NhtsaEnvelope<NhtsaEquipmentPlantCodeResult>>(
            tenantId,
            "nhtsa",
            "equipment_plant_codes",
            cacheKey,
            TimeSpan.FromMinutes(_options.ReferenceCacheMinutes),
            requestJson,
            async ct =>
            {
                var response = await httpClient.GetAsync(query.ToString(), ct);
                var payload = await response.Content.ReadAsStringAsync(ct);
                EnsureSuccess(response, payload, "Get equipment plant codes");
                return ((int)response.StatusCode, payload);
            },
            cancellationToken);
    }

    public Task<NhtsaEnvelope<NhtsaRecallResult>> GetRecallsByVehicleAsync(
        Guid tenantId,
        string make,
        string model,
        int modelYear,
        CancellationToken cancellationToken = default)
    {
        var normalizedMake = NormalizeReferenceValue(make);
        var normalizedModel = NormalizeReferenceValue(model);
        var cacheKey = $"recalls:{normalizedMake}:{normalizedModel}:{modelYear}";
        var requestJson = JsonSerializer.Serialize(new { make = normalizedMake, model = normalizedModel, modelYear });

        var url = BuildSafetyUrl(
            $"recalls/recallsByVehicle?make={Uri.EscapeDataString(normalizedMake)}&model={Uri.EscapeDataString(normalizedModel)}&modelYear={modelYear.ToString(CultureInfo.InvariantCulture)}");

        return cacheService.GetOrCreateAsync<NhtsaEnvelope<NhtsaRecallResult>>(
            tenantId,
            "nhtsa",
            "recalls_by_vehicle",
            cacheKey,
            TimeSpan.FromMinutes(_options.RecallCacheMinutes),
            requestJson,
            async ct =>
            {
                var response = await httpClient.GetAsync(url, ct);
                var payload = await response.Content.ReadAsStringAsync(ct);
                EnsureSuccess(response, payload, "Get recalls by vehicle");
                return ((int)response.StatusCode, payload);
            },
            cancellationToken);
    }

    public Task<NhtsaEnvelope<NhtsaRecallResult>> GetRecallsByCampaignNumberAsync(
        Guid tenantId,
        string campaignNumber,
        CancellationToken cancellationToken = default)
    {
        var normalizedCampaign = NormalizeReferenceValue(campaignNumber);
        var cacheKey = $"recalls-campaign:{normalizedCampaign}";
        var requestJson = JsonSerializer.Serialize(new { campaignNumber = normalizedCampaign });

        var url = BuildSafetyUrl($"recalls/campaignNumber?campaignNumber={Uri.EscapeDataString(normalizedCampaign)}");

        return cacheService.GetOrCreateAsync<NhtsaEnvelope<NhtsaRecallResult>>(
            tenantId,
            "nhtsa",
            "recalls_by_campaign",
            cacheKey,
            TimeSpan.FromMinutes(_options.RecallCacheMinutes),
            requestJson,
            async ct =>
            {
                var response = await httpClient.GetAsync(url, ct);
                var payload = await response.Content.ReadAsStringAsync(ct);
                EnsureSuccess(response, payload, "Get recalls by campaign");
                return ((int)response.StatusCode, payload);
            },
            cancellationToken);
    }

    public Task<NhtsaEnvelope<NhtsaComplaintResult>> GetComplaintsByVehicleAsync(
        Guid tenantId,
        string make,
        string model,
        int modelYear,
        CancellationToken cancellationToken = default)
    {
        var normalizedMake = NormalizeReferenceValue(make);
        var normalizedModel = NormalizeReferenceValue(model);
        var cacheKey = $"complaints:{normalizedMake}:{normalizedModel}:{modelYear}";
        var requestJson = JsonSerializer.Serialize(new { make = normalizedMake, model = normalizedModel, modelYear });

        var url = BuildSafetyUrl(
            $"complaints/complaintsByVehicle?make={Uri.EscapeDataString(normalizedMake)}&model={Uri.EscapeDataString(normalizedModel)}&modelYear={modelYear.ToString(CultureInfo.InvariantCulture)}");

        return cacheService.GetOrCreateAsync<NhtsaEnvelope<NhtsaComplaintResult>>(
            tenantId,
            "nhtsa",
            "complaints_by_vehicle",
            cacheKey,
            TimeSpan.FromMinutes(_options.ComplaintCacheMinutes),
            requestJson,
            async ct =>
            {
                var response = await httpClient.GetAsync(url, ct);
                var payload = await response.Content.ReadAsStringAsync(ct);
                EnsureSuccess(response, payload, "Get complaints by vehicle");
                return ((int)response.StatusCode, payload);
            },
            cancellationToken);
    }

    public Task<ExternalProviderHealthResponse> GetHealthAsync(CancellationToken cancellationToken = default)
    {
        var checkedAt = DateTimeOffset.UtcNow;
        if (!_options.EnableNhtsa)
        {
            return Task.FromResult(new ExternalProviderHealthResponse(
                "nhtsa",
                "disabled",
                "NHTSA provider is disabled in configuration.",
                checkedAt,
                null));
        }

        return Task.FromResult(new ExternalProviderHealthResponse(
            "nhtsa",
            "healthy",
            "NHTSA provider is configured and ready.",
            checkedAt,
            null));
    }

    public string VehicleApiBaseUrl => _options.NhtsaVehicleApiBaseUrl;

    public string SafetyApiBaseUrl => _options.NhtsaSafetyApiBaseUrl;

    private string BuildDecodePath(string vin, int? modelYear)
    {
        var path = new StringBuilder("vehicles/DecodeVinValuesExtended/");
        path.Append(Uri.EscapeDataString(vin));
        path.Append("?format=json");
        if (modelYear.HasValue)
        {
            path.Append("&modelyear=").Append(modelYear.Value.ToString(CultureInfo.InvariantCulture));
        }

        return path.ToString();
    }

    private string BuildSafetyUrl(string relativePath) =>
        new Uri(new Uri(_options.NhtsaSafetyApiBaseUrl.TrimEnd('/') + "/"), relativePath).ToString();

    private static string NormalizeReferenceValue(string value)
    {
        var trimmed = value.Trim().ToUpperInvariant();
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

    private static void EnsureSuccess(HttpResponseMessage response, string payload, string operation)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var message = payload.Trim();
        if (message.Length > 256)
        {
            message = message[..256];
        }

        throw new StlApiException(
            "external_intelligence.nhtsa_failed",
            $"{operation} failed with {(int)response.StatusCode}: {message}",
            502);
    }
}

public sealed class ExternalProviderCacheService(
    MaintainArr.Api.Data.MaintainArrDbContext db)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
    };

    public async Task<T> GetOrCreateAsync<T>(
        Guid tenantId,
        string providerKey,
        string operationKey,
        string cacheKey,
        TimeSpan ttl,
        string requestJson,
        Func<CancellationToken, Task<(int StatusCode, string ResponseJson)>> fetchAsync,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var existing = await db.ExternalProviderCacheEntries
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId
                    && x.ProviderKey == providerKey
                    && x.CacheKey == cacheKey,
                cancellationToken);

        if (existing is not null && existing.ExpiresAt > now)
        {
            await WriteAuditAsync(
                tenantId,
                providerKey,
                operationKey,
                cacheKey,
                "cache_hit",
                null,
                $"Returned cached {operationKey} payload.",
                cancellationToken);

            return Deserialize<T>(existing.ResponseJson);
        }

        var started = Stopwatch.StartNew();
        try
        {
            var fetched = await fetchAsync(cancellationToken);
            started.Stop();

            var entry = existing ?? new ExternalProviderCacheEntry
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ProviderKey = providerKey,
                CacheKey = cacheKey,
                OperationKey = operationKey,
                CreatedAt = now,
            };

            entry.OperationKey = operationKey;
            entry.RequestJson = requestJson;
            entry.ResponseJson = fetched.ResponseJson;
            entry.StatusCode = fetched.StatusCode;
            entry.ErrorMessage = null;
            entry.LastFetchedAt = now;
            entry.ExpiresAt = now.Add(ttl);
            entry.UpdatedAt = now;

            if (existing is null)
            {
                db.ExternalProviderCacheEntries.Add(entry);
            }

            await db.SaveChangesAsync(cancellationToken);

            await WriteAuditAsync(
                tenantId,
                providerKey,
                operationKey,
                cacheKey,
                "fetch_ok",
                (int)started.Elapsed.TotalMilliseconds,
                $"Fetched {operationKey} payload from NHTSA.",
                cancellationToken);

            return Deserialize<T>(fetched.ResponseJson);
        }
        catch (Exception ex) when (existing is not null && !string.IsNullOrWhiteSpace(existing.ResponseJson))
        {
            await WriteAuditAsync(
                tenantId,
                providerKey,
                operationKey,
                cacheKey,
                "stale_cache",
                null,
                $"Used stale cached {operationKey} payload because the live lookup failed: {ex.Message}",
                cancellationToken);

            return Deserialize<T>(existing.ResponseJson);
        }
    }

    private static T Deserialize<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, JsonOptions)
        ?? throw new InvalidOperationException("Cached provider payload could not be deserialized.");

    private async Task WriteAuditAsync(
        Guid tenantId,
        string providerKey,
        string operationKey,
        string cacheKey,
        string resultStatus,
        int? durationMs,
        string message,
        CancellationToken cancellationToken)
    {
        db.ExternalProviderAuditLogEntries.Add(new ExternalProviderAuditLogEntry
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProviderKey = providerKey,
            OperationKey = operationKey,
            CacheKey = cacheKey,
            ResultStatus = resultStatus,
            DurationMs = durationMs,
            Message = message,
            CreatedAt = DateTimeOffset.UtcNow,
        });

        await db.SaveChangesAsync(cancellationToken);
    }
}

public sealed class NhtsaEnvelope<T>
{
    public int Count { get; set; }

    public string? Message { get; set; }

    public string? SearchCriteria { get; set; }

    public List<T> Results { get; set; } = [];
}

public sealed class NhtsaVinDecodeResult
{
    public string? VIN { get; set; }

    public string? ErrorCode { get; set; }

    public string? ErrorText { get; set; }

    public string? AdditionalErrorText { get; set; }

    public string? Make { get; set; }

    public string? MakeID { get; set; }

    public string? Manufacturer { get; set; }

    public string? ManufacturerId { get; set; }

    public string? Model { get; set; }

    public string? ModelID { get; set; }

    public string? ModelYear { get; set; }

    public string? VehicleType { get; set; }

    public string? BodyClass { get; set; }

    public string? BrakeSystemType { get; set; }

    public string? DriveType { get; set; }

    public string? FuelTypePrimary { get; set; }

    public string? FuelTypeSecondary { get; set; }

    public string? EngineManufacturer { get; set; }

    public string? EngineModel { get; set; }

    public string? EngineConfiguration { get; set; }

    public string? EngineCylinders { get; set; }

    public string? DisplacementL { get; set; }

    public string? GVWR { get; set; }

    public string? PlantCompanyName { get; set; }

    public string? PlantCountry { get; set; }

    public string? PlantState { get; set; }

    public string? PlantCity { get; set; }

    public string? Trim { get; set; }

    public string? Series { get; set; }

    public string? VehicleDescriptor { get; set; }
}

public sealed class NhtsaMakeResult
{
    public string? Make_ID { get; set; }

    public string? Make_Name { get; set; }
}

public sealed class NhtsaManufacturerResult
{
    public string? Mfr_ID { get; set; }

    public string? Mfr_Name { get; set; }

    public string? Mfr_CommonName { get; set; }

    public string? Country { get; set; }
}

public sealed class NhtsaModelResult
{
    public string? Model_ID { get; set; }

    public string? Model_Name { get; set; }

    public string? Make_ID { get; set; }

    public string? Make_Name { get; set; }

    public string? VehicleType_Name { get; set; }
}

public sealed class NhtsaEquipmentPlantCodeResult
{
    public string? PlantID { get; set; }

    public string? PlantName { get; set; }

    public string? EquipmentType { get; set; }

    public string? DotCode { get; set; }

    public string? DotCodeOld { get; set; }

    public string? Status { get; set; }

    public string? Year { get; set; }

    public string? ReportType { get; set; }
}

public sealed class NhtsaRecallResult
{
    public string? Manufacturer { get; set; }

    public string? NHTSACampaignNumber { get; set; }

    public string? NHTSAActionNumber { get; set; }

    public int? PotentialNumberofUnitsAffected { get; set; }

    public bool ParkIt { get; set; }

    public bool ParkOutSide { get; set; }

    public bool OverTheAirUpdate { get; set; }

    public string? ReportReceivedDate { get; set; }

    public string? Component { get; set; }

    public string? Summary { get; set; }

    public string? Consequence { get; set; }

    public string? Remedy { get; set; }

    public string? Notes { get; set; }

    public string? ModelYear { get; set; }

    public string? Make { get; set; }

    public string? Model { get; set; }
}

public sealed class NhtsaComplaintResult
{
    public string? OdiNumber { get; set; }

    public string? Manufacturer { get; set; }

    public string? Crash { get; set; }

    public string? Fire { get; set; }

    public string? NumberOfInjuries { get; set; }

    public string? NumberOfDeaths { get; set; }

    public string? DateOfIncident { get; set; }

    public string? DateComplaintFiled { get; set; }

    public string? Vin { get; set; }

    public List<string>? Components { get; set; }

    public string? Summary { get; set; }

    public List<NhtsaComplaintProductResult>? Products { get; set; }
}

public sealed class NhtsaComplaintProductResult
{
    public string? Manufacturer { get; set; }

    public string? ModelYear { get; set; }

    public string? Make { get; set; }

    public string? Model { get; set; }
}
