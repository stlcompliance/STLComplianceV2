using STLCompliance.Shared.Data;

namespace MaintainArr.Api.Entities;

public sealed class AssetReadinessCheck : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid AssetId { get; set; }

    public string? AssetTag { get; set; }

    public string? VehicleRefKey { get; set; }

    public string SourceProduct { get; set; } = string.Empty;

    public string RequestedBy { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string ReadinessStatus { get; set; } = string.Empty;

    public string ReadinessBasis { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}
