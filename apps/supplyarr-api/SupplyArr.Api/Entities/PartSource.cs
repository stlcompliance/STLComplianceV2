using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class PartSource : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid PartId { get; set; }

    public string SourceType { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Part Part { get; set; } = null!;
}
