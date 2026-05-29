using STLCompliance.Shared.Data;

namespace ComplianceCore.Api.Entities;

public sealed class HazComReference : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string HazComKey { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? LinkedSdsKey { get; set; }

    public string LocationRef { get; set; } = string.Empty;

    public string DocumentUrl { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
