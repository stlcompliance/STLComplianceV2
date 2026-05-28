using STLCompliance.Shared.Data;

namespace NexArr.Api.Entities;

public sealed class CompanionPushSubscription : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid UserId { get; set; }

    public string Endpoint { get; set; } = string.Empty;

    public string P256dhKey { get; set; } = string.Empty;

    public string AuthKey { get; set; } = string.Empty;

    public string? UserAgent { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
