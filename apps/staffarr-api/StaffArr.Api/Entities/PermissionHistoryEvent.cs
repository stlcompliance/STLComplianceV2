using STLCompliance.Shared.Data;

namespace StaffArr.Api.Entities;

public sealed class PermissionHistoryEvent : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid PersonId { get; set; }

    public Guid AssignmentId { get; set; }

    public Guid RoleTemplateId { get; set; }

    public Guid PermissionTemplateId { get; set; }

    public Guid? ActorUserId { get; set; }

    public string EventType { get; set; } = string.Empty;

    public string AssignmentStatus { get; set; } = string.Empty;

    public string RoleKey { get; set; } = string.Empty;

    public string RoleName { get; set; } = string.Empty;

    public string PermissionKey { get; set; } = string.Empty;

    public string PermissionName { get; set; } = string.Empty;

    public string ScopeType { get; set; } = "tenant";

    public string? ScopeValue { get; set; }

    public DateTimeOffset OccurredAt { get; set; }
}
