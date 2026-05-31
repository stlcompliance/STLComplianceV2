using STLCompliance.Shared.Data;

namespace RoutArr.Api.Entities;

public sealed class DispatchMessage : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid TripId { get; set; }

    public Guid SenderUserId { get; set; }

    public string SenderPersonId { get; set; } = string.Empty;

    public string SenderRole { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public bool RequiresAcknowledgement { get; set; }

    public Guid? AcknowledgedByUserId { get; set; }

    public string? AcknowledgedByPersonId { get; set; }

    public DateTimeOffset? AcknowledgedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Trip Trip { get; set; } = null!;
}

public static class DispatchMessageSenderRoles
{
    public const string Dispatch = "dispatch";

    public const string Driver = "driver";
}
