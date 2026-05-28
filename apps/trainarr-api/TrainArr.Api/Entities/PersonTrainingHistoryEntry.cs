using STLCompliance.Shared.Data;

namespace TrainArr.Api.Entities;

public sealed class PersonTrainingHistoryEntry : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid StaffarrPersonId { get; set; }

    public Guid SourceDomainEventId { get; set; }

    public string EventKind { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string RelatedEntityType { get; set; } = string.Empty;

    public Guid RelatedEntityId { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
