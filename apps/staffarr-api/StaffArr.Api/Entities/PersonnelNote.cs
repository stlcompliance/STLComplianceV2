using STLCompliance.Shared.Data;

namespace StaffArr.Api.Entities;

public sealed class PersonnelNote : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid PersonId { get; set; }

    public string CategoryKey { get; set; } = "general";

    public string VisibilityKey { get; set; } = "hr_only";

    public string Subject { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public string Status { get; set; } = "active";

    public Guid CreatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
