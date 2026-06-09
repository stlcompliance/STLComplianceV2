using System.Text.Json;
using STLCompliance.Shared.Data;

namespace MaintainArr.Api.Entities;

public sealed class MaintenancePart : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string PartNumber { get; set; } = string.Empty;

    public string NormalizedPartNumber { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string CategoryKey { get; set; } = "maintenance";

    public string UnitOfMeasure { get; set; } = "each";

    public string Status { get; set; } = MaintenancePartStatuses.Active;

    public string SourceType { get; set; } = MaintenancePartSourceTypes.Manual;

    public string SourceLabel { get; set; } = "MaintainArr maintenance profile";

    public Guid? SupplyArrPartId { get; set; }

    public string? ManufacturerName { get; set; }

    public string? ManufacturerPartNumber { get; set; }

    public string? SdsDocumentId { get; set; }

    public string? ComplianceCoreMaterialKey { get; set; }

    public string ComplianceCoreHazardKeysJson { get; set; } = "[]";

    public string? Notes { get; set; }

    public string? CreatedByPersonId { get; set; }

    public string? UpdatedByPersonId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public IReadOnlyList<string> ComplianceCoreHazardKeys => DeserializeList(ComplianceCoreHazardKeysJson);

    internal static string SerializeList(IEnumerable<string> values) =>
        JsonSerializer.Serialize(
            values
                .Select(value => value.Trim())
                .Where(value => value.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase));

    internal static IReadOnlyList<string> DeserializeList(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<string[]>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}

public static class MaintenancePartStatuses
{
    public const string Draft = "draft";
    public const string Active = "active";
    public const string Inactive = "inactive";
    public const string Discontinued = "discontinued";

    public static IReadOnlySet<string> All { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Draft,
        Active,
        Inactive,
        Discontinued,
    };
}

public static class MaintenancePartSourceTypes
{
    public const string Manual = "manual";
    public const string SupplyArrSnapshot = "supplyarr_snapshot";

    public static IReadOnlySet<string> All { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Manual,
        SupplyArrSnapshot,
    };
}
