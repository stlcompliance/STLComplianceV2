using STLCompliance.Shared.Data;

namespace RoutArr.Api.Entities;

/// <summary>
/// Rebuildable mirror of equipment/vehicle identifiers used for dispatch assignment.
/// </summary>
public sealed class RoutarrVehicleRef : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string VehicleRefKey { get; set; } = string.Empty;

    public string DisplayLabel { get; set; } = string.Empty;

    public string? AssetTag { get; set; }

    public string SourceProduct { get; set; } = "maintainarr";

    public DateTimeOffset SourceUpdatedAt { get; set; }

    public DateTimeOffset MirroredAt { get; set; }
}
