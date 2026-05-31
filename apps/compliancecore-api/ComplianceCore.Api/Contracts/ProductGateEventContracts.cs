namespace ComplianceCore.Api.Contracts;

public sealed record ProductGateEventResponse(
    Guid EventId,
    Guid TenantId,
    Guid? ActorUserId,
    string Action,
    string Result,
    string? SourceProduct,
    Guid? CheckResultId,
    DateTimeOffset OccurredAt);
