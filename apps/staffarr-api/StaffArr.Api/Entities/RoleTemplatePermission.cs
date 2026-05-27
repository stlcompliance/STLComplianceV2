using STLCompliance.Shared.Data;

namespace StaffArr.Api.Entities;

public sealed class RoleTemplatePermission : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid RoleTemplateId { get; set; }

    public Guid PermissionTemplateId { get; set; }

    public string ScopeType { get; set; } = "tenant";

    public string? ScopeValue { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
