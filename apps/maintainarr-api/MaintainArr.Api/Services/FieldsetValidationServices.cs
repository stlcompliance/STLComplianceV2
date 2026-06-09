using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class FieldsetService(
    MaintainArrDbContext db,
    CatalogService catalogService,
    IEnumerable<IExternalReferenceAdapter> adapters)
{
    private readonly Dictionary<string, IExternalReferenceAdapter> _adapters = adapters.ToDictionary(x => x.SourceType, StringComparer.OrdinalIgnoreCase);

    public Task<FieldsetResponse> GetAssetsFieldsetAsync(Guid tenantId, string purpose, CancellationToken cancellationToken) =>
        GetFieldsetAsync(tenantId, "assets", purpose, cancellationToken);

    public Task<FieldsetResponse> GetDefectsFieldsetAsync(Guid tenantId, string purpose, CancellationToken cancellationToken) =>
        GetFieldsetAsync(tenantId, "defects", purpose, cancellationToken);

    public Task<FieldsetResponse> GetWorkOrdersFieldsetAsync(Guid tenantId, string purpose, CancellationToken cancellationToken) =>
        GetFieldsetAsync(tenantId, "work-orders", purpose, cancellationToken);

    public Task<FieldsetResponse> GetInspectionTemplatesFieldsetAsync(Guid tenantId, string purpose, CancellationToken cancellationToken) =>
        GetFieldsetAsync(tenantId, "inspection-templates", purpose, cancellationToken);

    public async Task<FieldsetResponse> GetFieldsetAsync(Guid tenantId, string key, string purpose, CancellationToken cancellationToken)
    {
        var definition = await db.FieldsetDefinitions.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Key == key && x.Purpose == purpose && x.IsActive, cancellationToken)
            ?? throw new StlApiException("fieldset.not_found", $"Fieldset '{key}:{purpose}' was not found.", 404);

        var fields = await db.FieldsetFields.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.FieldsetId == definition.Id)
            .OrderBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);

        var catalogKeys = fields
            .Where(x => !string.IsNullOrWhiteSpace(x.CatalogKey))
            .Select(x => x.CatalogKey!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var catalogOptionsByKey = await LoadCatalogOptionsByKeyAsync(tenantId, catalogKeys, cancellationToken);

        var resultFields = new List<FieldMetadataResponse>();
        foreach (var field in fields)
        {
            IReadOnlyList<CatalogOptionResponse>? options = null;
            if (!string.IsNullOrWhiteSpace(field.CatalogKey))
            {
                options = catalogOptionsByKey.TryGetValue(field.CatalogKey!, out var catalogOptions)
                    ? catalogOptions
                    : (await catalogService.GetAsync(tenantId, field.CatalogKey!, cancellationToken)).Options;
            }
            else if (!string.IsNullOrWhiteSpace(field.ReferenceKey) && _adapters.TryGetValue(field.SourceType, out var adapter))
            {
                var refOptions = await adapter.GetOptionsAsync(tenantId, field.ReferenceKey!, cancellationToken);
                options = refOptions
                    .Select((x, index) => new CatalogOptionResponse(
                        x.Key,
                        x.Label,
                        string.Empty,
                        index,
                        null,
                        x.IsActive,
                        null,
                        new Dictionary<string, object?>
                        {
                            ["source"] = x.Source,
                            ["sourceOfTruth"] = x.SourceOfTruth,
                            ["storedValue"] = x.StoredValue,
                            ["displayValue"] = x.DisplayValue,
                            ["externalId"] = x.Id,
                        }))
                    .ToList();
            }

            resultFields.Add(new FieldMetadataResponse(
                field.Key,
                field.Label,
                field.Description,
                field.DataType,
                field.ControlType,
                field.Required,
                field.CatalogKey,
                field.ReferenceKey,
                field.SourceType,
                field.SourceOfTruth,
                InferStoredValue(field),
                InferDisplayValue(field),
                field.AllowCustom,
                field.CustomRequiresApproval,
                field.DrivesLogic,
                field.DrivesInspectionBranching,
                field.DrivesPMApplicability,
                field.DrivesCompliance,
                field.DrivesReporting,
                field.DrivesReadiness,
                ParseStringDict(field.DependencyJson),
                ParseObjectDict(field.ValidationJson),
                ParseDefault(field.DefaultValueJson),
                ParseObjectDict(field.VisibilityJson),
                field.SectionKey,
                options));
        }

        return new FieldsetResponse(definition.Key, definition.Label, definition.EntityType, definition.Purpose, resultFields);
    }

    private async Task<IReadOnlyDictionary<string, IReadOnlyList<CatalogOptionResponse>>> LoadCatalogOptionsByKeyAsync(
        Guid tenantId,
        IReadOnlyCollection<string> catalogKeys,
        CancellationToken cancellationToken)
    {
        if (catalogKeys.Count == 0)
        {
            return new Dictionary<string, IReadOnlyList<CatalogOptionResponse>>(StringComparer.OrdinalIgnoreCase);
        }

        var keys = catalogKeys.ToArray();
        var catalogs = await db.CatalogDefinitions.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.IsActive && keys.Contains(x.Key))
            .OrderBy(x => x.Label)
            .ToListAsync(cancellationToken);
        if (catalogs.Count == 0)
        {
            return new Dictionary<string, IReadOnlyList<CatalogOptionResponse>>(StringComparer.OrdinalIgnoreCase);
        }

        var catalogIds = catalogs.Select(x => x.Id).ToArray();
        var options = await db.CatalogOptions.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.IsActive && catalogIds.Contains(x.CatalogId))
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Label)
            .ToListAsync(cancellationToken);

        var optionById = options.ToDictionary(x => x.Id);
        var optionsByCatalogId = options
            .GroupBy(x => x.CatalogId)
            .ToDictionary(x => x.Key, x => x.ToList());
        var optionIds = options.Select(x => x.Id).ToArray();
        var dependenciesByOptionId = optionIds.Length == 0
            ? new Dictionary<Guid, Dictionary<string, string>>()
            : (await db.CatalogOptionDependencies.AsNoTracking()
                    .Where(x => x.TenantId == tenantId && optionIds.Contains(x.CatalogOptionId))
                    .ToListAsync(cancellationToken))
                .GroupBy(x => x.CatalogOptionId)
                .ToDictionary(
                    x => x.Key,
                    x => x.ToDictionary(d => d.DependsOnCatalogKey, d => d.DependsOnOptionKey));

        var result = new Dictionary<string, IReadOnlyList<CatalogOptionResponse>>(StringComparer.OrdinalIgnoreCase);
        foreach (var catalog in catalogs)
        {
            result[catalog.Key] = optionsByCatalogId.TryGetValue(catalog.Id, out var catalogOptions)
                ? catalogOptions
                    .Select(o => new CatalogOptionResponse(
                        o.Key,
                        o.Label,
                        o.Description,
                        o.SortOrder,
                        o.ParentOptionId.HasValue && optionById.TryGetValue(o.ParentOptionId.Value, out var parent)
                            ? parent.Key
                            : null,
                        o.IsActive,
                        dependenciesByOptionId.TryGetValue(o.Id, out var dependency)
                            ? dependency
                            : new Dictionary<string, string>(),
                        ParseObjectDict(o.MetadataJson)))
                    .ToList()
                : [];
        }

        return result;
    }

    private static string InferStoredValue(FieldsetField field)
    {
        if (string.Equals(field.SourceType, "compliancecore_reference", StringComparison.OrdinalIgnoreCase))
        {
            return "stable_key";
        }

        if (field.ReferenceKey is not null)
        {
            return "id";
        }

        return "catalog_key";
    }

    private static string InferDisplayValue(FieldsetField field)
    {
        if (string.Equals(field.SourceType, "compliancecore_reference", StringComparison.OrdinalIgnoreCase))
        {
            return "mirrored_label";
        }

        if (field.ReferenceKey is not null)
        {
            return "mirroredDisplayName";
        }

        return "catalog_label";
    }

    private static IReadOnlyDictionary<string, string>? ParseStringDict(string json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "{}")
        {
            return null;
        }

        return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
    }

    private static IReadOnlyDictionary<string, object?>? ParseObjectDict(string json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "{}")
        {
            return null;
        }

        return JsonSerializer.Deserialize<Dictionary<string, object?>>(json);
    }

    private static object? ParseDefault(string json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "null")
        {
            return null;
        }

        return JsonSerializer.Deserialize<object>(json);
    }
}

public sealed class PendingCatalogValueService(MaintainArrDbContext db)
{
    public async Task<PendingCatalogValue> CreateAsync(Guid tenantId, string catalogKey, string proposedValue, string personId, string sourceEntityType, string sourceEntityId, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var pending = new PendingCatalogValue
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CatalogKey = catalogKey,
            ProposedKey = proposedValue.Trim().ToLowerInvariant().Replace(' ', '_'),
            ProposedLabel = proposedValue.Trim(),
            ProposedByPersonId = personId,
            SourceEntityType = sourceEntityType,
            SourceEntityId = sourceEntityId,
            Status = "pending",
            CreatedAt = now,
            UpdatedAt = now,
        };
        db.PendingCatalogValues.Add(pending);
        await db.SaveChangesAsync(cancellationToken);
        return pending;
    }
}

public sealed class ControlledValueValidationService(
    CatalogService catalogService,
    PendingCatalogValueService pendingCatalogValueService,
    IEnumerable<IExternalReferenceAdapter> adapters)
{
    private static readonly HashSet<string> ComplianceCoreOwnedReferenceKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "governingBody",
        "governingBodyApplicability",
        "regulatoryAssetType",
        "regulatedUseType",
        "complianceCategory",
        "requiredEvidenceType",
        "inspectionRequirementType",
        "documentRequirementType",
        "rulepackApplicabilityKeys",
        "lawCitation",
        "ruleApplicabilityContext",
        "evidenceMappingType",
        "complianceOutcomeType",
        "exemptionType",
        "exceptionType",
    };

    private readonly Dictionary<string, IExternalReferenceAdapter> _adapters = adapters.ToDictionary(x => x.SourceType, StringComparer.OrdinalIgnoreCase);

    public async Task ValidateFieldsetValuesAsync(
        Guid tenantId,
        IReadOnlyList<FieldMetadataResponse> fields,
        IReadOnlyDictionary<string, object?> values,
        string personId,
        string sourceEntityType,
        string sourceEntityId,
        bool createPendingValues,
        CancellationToken cancellationToken)
    {
        var selectedByFieldKey = fields.ToDictionary(
            f => f.Key,
            f => ExtractValues(values.TryGetValue(f.Key, out var raw) ? raw : null),
            StringComparer.OrdinalIgnoreCase);

        var selectedByCatalogKey = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var field in fields)
        {
            if (!string.IsNullOrWhiteSpace(field.CatalogKey))
            {
                selectedByCatalogKey[field.CatalogKey!] = selectedByFieldKey[field.Key];
            }
        }

        foreach (var field in fields)
        {
            var selectedValues = selectedByFieldKey[field.Key];
            if (field.Required && selectedValues.Count == 0)
            {
                throw new StlApiException("assets.validation", $"{field.Key} is required.", 400);
            }

            if (selectedValues.Count == 0)
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(field.CatalogKey))
            {
                await ValidateCatalogFieldAsync(
                    tenantId,
                    field,
                    selectedValues,
                    selectedByCatalogKey,
                    personId,
                    sourceEntityType,
                    sourceEntityId,
                    createPendingValues,
                    cancellationToken);
                continue;
            }

            if (!string.IsNullOrWhiteSpace(field.ReferenceKey))
            {
                await ValidateReferenceFieldAsync(tenantId, field, selectedValues, cancellationToken);
                continue;
            }

            ValidateTypedField(field, selectedValues);
        }
    }

    private async Task ValidateCatalogFieldAsync(
        Guid tenantId,
        FieldMetadataResponse field,
        IReadOnlyList<string> selectedValues,
        IReadOnlyDictionary<string, IReadOnlyList<string>> selectedByCatalogKey,
        string personId,
        string sourceEntityType,
        string sourceEntityId,
        bool createPendingValues,
        CancellationToken cancellationToken)
    {
        var catalog = await catalogService.GetAsync(tenantId, field.CatalogKey!, cancellationToken);
        var optionMap = catalog.Options.ToDictionary(x => x.Key, x => x, StringComparer.OrdinalIgnoreCase);

        foreach (var selectedValue in selectedValues)
        {
            if (!optionMap.TryGetValue(selectedValue, out var option))
            {
                if (field.AllowCustom && field.CustomRequiresApproval)
                {
                    if (createPendingValues)
                    {
                        _ = await pendingCatalogValueService.CreateAsync(
                            tenantId,
                            field.CatalogKey!,
                            selectedValue,
                            personId,
                            sourceEntityType,
                            sourceEntityId,
                            cancellationToken);
                    }
                    continue;
                }

                if (field.AllowCustom)
                {
                    continue;
                }

                throw new StlApiException(
                    "assets.validation",
                    $"Invalid value '{selectedValue}' for field '{field.Key}'. Allowed catalog: {field.CatalogKey}.",
                    400);
            }

            var dependency = option.Dependency;
            if (dependency is null || dependency.Count == 0)
            {
                continue;
            }

            foreach (var dependencyRule in dependency)
            {
                if (!selectedByCatalogKey.TryGetValue(dependencyRule.Key, out var selectedParentValues)
                    || !selectedParentValues.Any(x => string.Equals(x, dependencyRule.Value, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new StlApiException(
                        "assets.validation",
                        $"Invalid value '{selectedValue}' for field '{field.Key}'. Expected parent '{dependencyRule.Key}' = '{dependencyRule.Value}'.",
                        400);
                }
            }
        }
    }

    private async Task ValidateReferenceFieldAsync(
        Guid tenantId,
        FieldMetadataResponse field,
        IReadOnlyList<string> selectedValues,
        CancellationToken cancellationToken)
    {
        if (field.ReferenceKey is not null
            && ComplianceCoreOwnedReferenceKeys.Contains(field.ReferenceKey)
            && !string.Equals(field.Source, "compliancecore_reference", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "references.ownership_violation",
                $"Field '{field.Key}' must resolve from Compliance Core.",
                400);
        }

        if (!_adapters.TryGetValue(field.Source, out var adapter))
        {
            throw new StlApiException("references.adapter_missing", $"No adapter for source '{field.Source}'.", 500);
        }

        foreach (var selectedValue in selectedValues)
        {
            var exists = await adapter.ExistsAsync(tenantId, field.ReferenceKey!, selectedValue, cancellationToken);
            if (!exists)
            {
                throw new StlApiException(
                    "assets.validation",
                    $"Invalid external reference '{selectedValue}' for field '{field.Key}' from '{field.SourceOfTruth}'.",
                    400);
            }
        }
    }

    private static IReadOnlyList<string> ExtractValues(object? raw)
    {
        if (raw is null)
        {
            return [];
        }

        if (raw is string text)
        {
            return string.IsNullOrWhiteSpace(text) ? [] : [text.Trim()];
        }

        if (raw is JsonElement json)
        {
            return json.ValueKind switch
            {
                JsonValueKind.Array => json.EnumerateArray()
                    .Select(x => x.ToString().Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList(),
                JsonValueKind.String => string.IsNullOrWhiteSpace(json.GetString()) ? [] : [json.GetString()!.Trim()],
                JsonValueKind.Null => [],
                _ => [json.ToString().Trim()],
            };
        }

        if (raw is IEnumerable<object?> list)
        {
            return list
                .Select(x => x?.ToString()?.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!)
                .ToList();
        }

        return [raw.ToString()!.Trim()];
    }

    private static void ValidateTypedField(FieldMetadataResponse field, IReadOnlyList<string> selectedValues)
    {
        var validation = field.Validation ?? new Dictionary<string, object?>();
        var minLength = ReadInt(validation, "minLength");
        var maxLength = ReadInt(validation, "maxLength");
        var min = ReadDecimal(validation, "min");
        var max = ReadDecimal(validation, "max");
        var pattern = ReadString(validation, "pattern");

        foreach (var value in selectedValues)
        {
            if (minLength.HasValue && value.Length < minLength.Value)
            {
                throw new StlApiException(
                    "assets.validation",
                    $"{field.Key} must be at least {minLength.Value} characters.",
                    400);
            }

            if (maxLength.HasValue && value.Length > maxLength.Value)
            {
                throw new StlApiException(
                    "assets.validation",
                    $"{field.Key} must be {maxLength.Value} characters or fewer.",
                    400);
            }

            if (!string.IsNullOrWhiteSpace(pattern)
                && !Regex.IsMatch(value, pattern, RegexOptions.None, TimeSpan.FromSeconds(1)))
            {
                throw new StlApiException(
                    "assets.validation",
                    $"{field.Key} format is invalid.",
                    400);
            }

            if (string.Equals(field.Type, "number", StringComparison.OrdinalIgnoreCase)
                || string.Equals(field.Type, "integer", StringComparison.OrdinalIgnoreCase)
                || string.Equals(field.Control, "number", StringComparison.OrdinalIgnoreCase))
            {
                if (!decimal.TryParse(value, out var parsed))
                {
                    throw new StlApiException(
                        "assets.validation",
                        $"{field.Key} must be numeric.",
                        400);
                }

                if (string.Equals(field.Type, "integer", StringComparison.OrdinalIgnoreCase)
                    && parsed != decimal.Truncate(parsed))
                {
                    throw new StlApiException(
                        "assets.validation",
                        $"{field.Key} must be a whole number.",
                        400);
                }

                if (min.HasValue && parsed < min.Value)
                {
                    throw new StlApiException(
                        "assets.validation",
                        $"{field.Key} must be at least {min.Value}.",
                        400);
                }

                if (max.HasValue && parsed > max.Value)
                {
                    throw new StlApiException(
                        "assets.validation",
                        $"{field.Key} must be {max.Value} or less.",
                        400);
                }
            }

            if (string.Equals(field.Type, "date", StringComparison.OrdinalIgnoreCase)
                && !DateOnly.TryParse(value, out _))
            {
                throw new StlApiException(
                    "assets.validation",
                    $"{field.Key} must be a valid date.",
                    400);
            }
        }
    }

    private static int? ReadInt(IReadOnlyDictionary<string, object?> values, string key)
    {
        if (!values.TryGetValue(key, out var raw) || raw is null)
        {
            return null;
        }

        if (raw is int intValue) return intValue;
        if (raw is long longValue) return checked((int)longValue);
        if (raw is JsonElement json)
        {
            if (json.ValueKind == JsonValueKind.Number && json.TryGetInt32(out var jsonInt)) return jsonInt;
            if (json.ValueKind == JsonValueKind.String && int.TryParse(json.GetString(), out var jsonStringInt)) return jsonStringInt;
        }

        return int.TryParse(raw.ToString(), out var parsed) ? parsed : null;
    }

    private static decimal? ReadDecimal(IReadOnlyDictionary<string, object?> values, string key)
    {
        if (!values.TryGetValue(key, out var raw) || raw is null)
        {
            return null;
        }

        if (raw is decimal decimalValue) return decimalValue;
        if (raw is int intValue) return intValue;
        if (raw is long longValue) return longValue;
        if (raw is double doubleValue) return (decimal)doubleValue;
        if (raw is JsonElement json)
        {
            if (json.ValueKind == JsonValueKind.Number && json.TryGetDecimal(out var jsonDecimal)) return jsonDecimal;
            if (json.ValueKind == JsonValueKind.String && decimal.TryParse(json.GetString(), out var jsonStringDecimal)) return jsonStringDecimal;
        }

        return decimal.TryParse(raw.ToString(), out var parsed) ? parsed : null;
    }

    private static string? ReadString(IReadOnlyDictionary<string, object?> values, string key)
    {
        if (!values.TryGetValue(key, out var raw) || raw is null)
        {
            return null;
        }

        if (raw is string text) return text;
        if (raw is JsonElement json && json.ValueKind == JsonValueKind.String) return json.GetString();
        return raw.ToString();
    }
}
