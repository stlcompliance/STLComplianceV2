using System.Text.Json;
using STLCompliance.Shared.Data;

namespace MaintainArr.Api.Entities;

public sealed class MaintenancePartsKit : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string KitNumber { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? KitCategoryKey { get; set; }

    public string? KitTypeKey { get; set; }

    public string? PriorityKey { get; set; }

    public string? OwningSiteRef { get; set; }

    public string? OwningTeamRef { get; set; }

    public string? OwnerPersonId { get; set; }

    public string? OwnerRoleKey { get; set; }

    public string TagsJson { get; set; } = "[]";

    public string AssetTypeApplicabilityJson { get; set; } = "[]";

    public string WorkOrderTypeApplicabilityJson { get; set; } = "[]";

    public string? PmPlanRef { get; set; }

    public string DefinitionJson { get; set; } = "{}";

    public string Status { get; set; } = MaintenancePartsKitStatuses.Draft;

    public int Version { get; set; } = 1;

    public string? CloneSourcePartsKitId { get; set; }

    public string? CreatedByPersonId { get; set; }

    public string? UpdatedByPersonId { get; set; }

    public string? ActivatedByPersonId { get; set; }

    public string? ApprovedByPersonId { get; set; }

    public string? RetiredByPersonId { get; set; }

    public DateTimeOffset? EffectiveAt { get; set; }

    public DateTimeOffset? ExpiresAt { get; set; }

    public DateTimeOffset? ActivatedAt { get; set; }

    public DateTimeOffset? ApprovedAt { get; set; }

    public DateTimeOffset? RetiredAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<MaintenancePartsKitLine> Lines { get; set; } = [];

    public IReadOnlyList<string> AssetTypeApplicability =>
        DeserializeList(AssetTypeApplicabilityJson);

    public IReadOnlyList<string> WorkOrderTypeApplicability =>
        DeserializeList(WorkOrderTypeApplicabilityJson);

    public IReadOnlyList<string> Tags => DeserializeList(TagsJson);

    public IReadOnlyList<Guid> LineRefs => Lines
        .OrderBy(x => x.SortOrder)
        .ThenBy(x => x.CreatedAt)
        .Select(x => x.Id)
        .ToArray();

    internal static string SerializeList(IEnumerable<string> values) =>
        JsonSerializer.Serialize(values.Select(value => value.Trim()).Where(value => value.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase));

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

public static class MaintenancePartsKitStatuses
{
    public const string Draft = "draft";
    public const string PendingApproval = "pending_approval";
    public const string Active = "active";
    public const string Paused = "paused";
    public const string Retired = "retired";
    public const string Archived = "archived";

    public static IReadOnlyCollection<string> All { get; } = new[]
    {
        Draft,
        PendingApproval,
        Active,
        Paused,
        Retired,
        Archived
    };
}

public sealed class MaintenancePartsKitLine : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid MaintenancePartsKitId { get; set; }

    public string ItemRef { get; set; } = string.Empty;

    public string ItemDescriptionSnapshot { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    public string UnitOfMeasure { get; set; } = string.Empty;

    public bool Required { get; set; }

    public bool SubstituteAllowed { get; set; }

    public int SortOrder { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public MaintenancePartsKit MaintenancePartsKit { get; set; } = null!;
}
