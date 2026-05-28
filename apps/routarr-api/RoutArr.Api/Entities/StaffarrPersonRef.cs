using STLCompliance.Shared.Data;

namespace RoutArr.Api.Entities;

/// <summary>
/// Rebuildable mirror of StaffArr person identifiers used for dispatch assignment.
/// </summary>
public sealed class StaffarrPersonRef : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string PersonId { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string SourceProduct { get; set; } = "staffarr";

    public DateTimeOffset SourceUpdatedAt { get; set; }

    public DateTimeOffset MirroredAt { get; set; }
}
