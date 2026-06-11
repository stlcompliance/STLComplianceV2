using STLCompliance.Shared.Data;

namespace StaffArr.Api.Entities;

public sealed class StaffPersonRole : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid PersonId { get; set; }

    public Guid RoleId { get; set; }

    public string AssignmentScopeType { get; set; } = "tenant";

    public string? AssignmentScopeRefId { get; set; }

    public DateTimeOffset? StartsAt { get; set; }

    public DateTimeOffset? EndsAt { get; set; }

    public Guid? AssignedByPersonId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
