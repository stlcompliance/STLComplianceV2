namespace TrainArr.Api.Contracts;

public sealed record IntegrationSettingsResponse(
    bool StaffArrIntegrationEnabled,
    bool StaffArrIncidentIntakeEnabled,
    bool StaffArrPublicationDeliveryEnabled,
    bool ComplianceCoreIntegrationEnabled,
    bool ComplianceCoreQualificationChecksEnabled,
    bool RoutarrIntegrationEnabled,
    bool RoutarrQualificationDispatchEnabled,
    DateTimeOffset? UpdatedAt);

public sealed record UpsertIntegrationSettingsRequest(
    bool StaffArrIntegrationEnabled,
    bool StaffArrIncidentIntakeEnabled,
    bool StaffArrPublicationDeliveryEnabled,
    bool ComplianceCoreIntegrationEnabled,
    bool ComplianceCoreQualificationChecksEnabled,
    bool RoutarrIntegrationEnabled,
    bool RoutarrQualificationDispatchEnabled);

public sealed record IntegrationProbeItem(
    string IntegrationKey,
    string DisplayName,
    string Status,
    int? HttpStatusCode,
    string? Message,
    DateTimeOffset ProbedAt);

public sealed record IntegrationProbesResponse(
    DateTimeOffset ProbedAt,
    IReadOnlyList<IntegrationProbeItem> Items);
