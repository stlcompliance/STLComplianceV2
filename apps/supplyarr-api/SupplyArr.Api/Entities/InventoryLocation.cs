using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class InventoryLocation : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string LocationKey { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string LocationType { get; set; } = "warehouse";

    public string AddressLine { get; set; } = string.Empty;

    public Guid? StaffarrSiteOrgUnitId { get; set; }

    public string StaffarrSiteNameSnapshot { get; set; } = string.Empty;

    public string StaffarrSiteResolutionStatus { get; set; } = InventoryLocationSiteResolutionStatuses.Unassigned;

    public string Status { get; set; } = "active";

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<InventoryBin> Bins { get; set; } = new List<InventoryBin>();
}

public static class InventoryLocationSiteResolutionStatuses
{
    public const string Active = "active";
    public const string Missing = "missing";
    public const string Inactive = "inactive";
    public const string Unresolved = "unresolved";
    public const string Unassigned = "unassigned";

    public static readonly IReadOnlySet<string> MovementSafe = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Active,
    };
}
