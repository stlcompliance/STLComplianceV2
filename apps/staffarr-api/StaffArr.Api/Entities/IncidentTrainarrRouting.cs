using STLCompliance.Shared.Data;

namespace StaffArr.Api.Entities;

public sealed class IncidentTrainarrRouting : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid IncidentId { get; set; }

    public Guid TrainarrRemediationId { get; set; }

    public string RoutingStatus { get; set; } = "routed";

    public DateTimeOffset RoutedAt { get; set; }

    public Guid RoutedByUserId { get; set; }

    public string? FailureReason { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
