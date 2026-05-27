using STLCompliance.Shared.Data;

namespace ComplianceCore.Api.Entities;

public sealed class SdsReference : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string SdsKey { get; set; } = string.Empty;

    public Guid? MaterialKeyId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string Manufacturer { get; set; } = string.Empty;

    public string DocumentUrl { get; set; } = string.Empty;

    public DateOnly? RevisionDate { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public MaterialKey? MaterialKey { get; set; }
}
