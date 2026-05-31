namespace NexArr.Api.Entities;

public sealed class PlatformRoleAssignment
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid? TenantId { get; set; }

    public string RoleKey { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public Guid CreatedByUserId { get; set; }

    public PlatformUser User { get; set; } = null!;
}
