using STLCompliance.Shared.Data;

namespace StaffArr.Api.Entities;

public sealed class PermissionAuditLogEntry : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid? ActorPersonId { get; set; }

    public string Action { get; set; } = string.Empty;

    public Guid? RoleId { get; set; }

    public string? BeforeJson { get; set; }

    public string? AfterJson { get; set; }

    public string? Reason { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
