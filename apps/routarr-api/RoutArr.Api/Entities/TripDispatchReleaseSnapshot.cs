using STLCompliance.Shared.Data;

namespace RoutArr.Api.Entities;

public sealed class TripDispatchReleaseSnapshot : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid TripId { get; set; }

    public Guid ReleasedByUserId { get; set; }

    public DateTimeOffset ReleasedAt { get; set; }

    public bool DriverCanAssign { get; set; }

    public bool VehicleCanAssign { get; set; }

    public bool HasMissingExternalData { get; set; }

    public bool HasStaleExternalData { get; set; }

    public string Summary { get; set; } = string.Empty;

    public string SnapshotJson { get; set; } = string.Empty;

    public Trip Trip { get; set; } = null!;
}
