using STLCompliance.Shared.Data;

namespace MaintainArr.Api.Entities;

public sealed class MaintenanceInboundPlatformEvent : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string SourceProduct { get; set; } = string.Empty;

    public Guid SourceEventId { get; set; }

    public string EventKind { get; set; } = string.Empty;

    public string RelatedEntityType { get; set; } = string.Empty;

    public Guid RelatedEntityId { get; set; }

    public Guid CorrelationId { get; set; }

    public string PayloadJson { get; set; } = string.Empty;

    public string Outcome { get; set; } = MaintenanceInboundEventOutcomes.Processed;

    public Guid? CreatedDefectId { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Defect? CreatedDefect { get; set; }
}

public static class MaintenanceInboundEventOutcomes
{
    public const string Processed = "processed";

    public const string Ignored = "ignored";
}
