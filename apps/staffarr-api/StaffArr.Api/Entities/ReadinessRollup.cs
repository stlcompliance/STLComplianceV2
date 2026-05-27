using STLCompliance.Shared.Data;

namespace StaffArr.Api.Entities;

public sealed class ReadinessRollup : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string ScopeType { get; set; } = "team";

    public Guid OrgUnitId { get; set; }

    public string OrgUnitName { get; set; } = string.Empty;

    public int TotalMembers { get; set; }

    public int ReadyCount { get; set; }

    public int NotReadyCount { get; set; }

    public int OverrideCount { get; set; }

    public decimal ReadyPercent { get; set; }

    public DateTimeOffset ComputedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
