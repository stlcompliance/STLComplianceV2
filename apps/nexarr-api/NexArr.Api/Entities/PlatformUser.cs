namespace NexArr.Api.Entities;

public sealed class PlatformUser
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public bool IsPlatformAdmin { get; set; }

    public string ThemePreference { get; set; } = "dark";

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset ModifiedAt { get; set; }

    public UserCredential? Credential { get; set; }

    public ICollection<TenantMembership> Memberships { get; set; } = [];

    public ICollection<PlatformRoleAssignment> RoleAssignments { get; set; } = [];

    public ICollection<ExternalIdentityProviderMapping> ExternalIdentityProviderMappings { get; set; } = [];

    public ICollection<UserSession> Sessions { get; set; } = [];
}
