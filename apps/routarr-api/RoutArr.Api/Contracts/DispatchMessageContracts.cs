namespace RoutArr.Api.Contracts;

public sealed record DispatchMessageResponse(
    Guid MessageId,
    Guid TripId,
    Guid SenderUserId,
    string SenderPersonId,
    string SenderRole,
    string Body,
    bool RequiresAcknowledgement,
    Guid? AcknowledgedByUserId,
    string? AcknowledgedByPersonId,
    DateTimeOffset? AcknowledgedAt,
    DateTimeOffset CreatedAt);

public sealed record DispatchThreadResponse(
    Guid TripId,
    IReadOnlyList<DispatchMessageResponse> Messages);

public sealed record CreateDispatchMessageRequest(
    string Body,
    bool RequiresAcknowledgement = false);
