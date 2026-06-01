using System.Text.Json;
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

    public async Task<FieldsetResponse> GetFieldsetAsync(Guid tenantId, string key, string purpose, CancellationToken cancellationToken)
    {
        var definition = await db.FieldsetDefinitions.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Key == key && x.Purpose == purpose && x.IsActive, cancellationToken)
            ?? throw new StlApiException("fieldset.not_found", $"Fieldset '{key}:{purpose}' was not found.", 404);

        var fields = await db.FieldsetFields.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.FieldsetId == definition.Id)
            .OrderBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);

        var resultFields = new List<FieldMetadataResponse>();
        foreach (var field in fields)
        {
            IReadOnlyList<CatalogOptionResponse>? options = null;
            if (!string.IsNullOrWhiteSpace(field.CatalogKey))
            {
                options = (await catalogService.GetAsync(tenantId, field.CatalogKey!, cancellationToken)).Options;
            }
            else if (!string.IsNullOrWhiteSpace(field.ReferenceKey) && _adapters.TryGetValue(field.SourceType, out var adapter))
            {
                var refOptions = await adapter.GetOptionsAsync(tenantId, field.ReferenceKey!, cancellationToken);
                options = refOptions.Select(x => new CatalogOptionResponse(x.Key, x.Label, string.Empty, 0, null, x.IsActive, null)).ToList();
            }

            resultFields.Add(new FieldMetadataResponse(
                field.Key,
                field.Label,
                field.DataType,
                field.ControlType,
                field.Required,
                field.CatalogKey,
                field.ReferenceKey,
                field.SourceType,
                field.SourceOfTruth,
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
                options));
        }

        return new FieldsetResponse(definition.Key, definition.Label, definition.EntityType, definition.Purpose, resultFields);
    }

    private static IReadOnlyDictionary<string, string>? ParseStringDict(string json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "{}") return null;
        return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
    }

    private static IReadOnlyDictionary<string, object?>? ParseObjectDict(string json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "{}") return null;
        return JsonSerializer.Deserialize<Dictionary<string, object?>>(json);
    }

    private static object? ParseDefault(string json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "null") return null;
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
    MaintainArrDbContext db,
    CatalogService catalogService,
    PendingCatalogValueService pendingCatalogValueService,
    IEnumerable<IExternalReferenceAdapter> adapters)
{
    private readonly Dictionary<string, IExternalReferenceAdapter> _adapters = adapters.ToDictionary(x => x.SourceType, StringComparer.OrdinalIgnoreCase);

    public async Task ValidateFieldsetValuesAsync(
        Guid tenantId,
        IReadOnlyList<FieldMetadataResponse> fields,
        IReadOnlyDictionary<string, object?> values,
        string personId,
        string sourceEntityType,
        string sourceEntityId,
        CancellationToken cancellationToken)
    {
        foreach (var field in fields)
        {
            values.TryGetValue(field.Key, out var raw);
            var value = raw?.ToString();
            if (field.Required && string.IsNullOrWhiteSpace(value))
            {
                throw new StlApiException("assets.validation", $"{field.Key} is required.", 400);
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(field.CatalogKey))
            {
                var catalog = await catalogService.GetAsync(tenantId, field.CatalogKey!, cancellationToken);
                var exists = catalog.Options.Any(x => string.Equals(x.Key, value, StringComparison.OrdinalIgnoreCase));
                if (!exists)
                {
                    if (field.AllowCustom && field.CustomRequiresApproval)
                    {
                        _ = await pendingCatalogValueService.CreateAsync(tenantId, field.CatalogKey!, value, personId, sourceEntityType, sourceEntityId, cancellationToken);
                        continue;
                    }

                    throw new StlApiException("assets.validation", $"Invalid value '{value}' for field '{field.Key}'. Allowed catalog: {field.CatalogKey}.", 400);
                }
            }
            else if (!string.IsNullOrWhiteSpace(field.ReferenceKey))
            {
                if (!_adapters.TryGetValue(field.Source, out var adapter))
                {
                    throw new StlApiException("references.adapter_missing", $"No adapter for source '{field.Source}'.", 500);
                }

                var exists = await adapter.ExistsAsync(tenantId, field.ReferenceKey!, value, cancellationToken);
                if (!exists)
                {
                    throw new StlApiException("assets.validation", $"Invalid external reference '{value}' for field '{field.Key}' from '{field.SourceOfTruth}'.", 400);
                }
            }
        }
    }
}
