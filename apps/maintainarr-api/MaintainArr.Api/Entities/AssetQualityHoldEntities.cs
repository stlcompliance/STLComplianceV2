using STLCompliance.Shared.Data;

namespace MaintainArr.Api.Entities;

public sealed class AssetQualityHold : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid AssetId { get; set; }

    public string HoldType { get; set; } = string.Empty;

    public string SourceProduct { get; set; } = string.Empty;

    public string? SourceObjectRef { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Severity { get; set; } = "moderate";

    public string Status { get; set; } = "active";

    public DateTimeOffset CreatedAt { get; set; }

    public string? CreatedByPersonId { get; set; }

    public DateTimeOffset? ReleasedAt { get; set; }

    public string? ReleasedByPersonId { get; set; }

    public string? ReleaseReason { get; set; }
}
