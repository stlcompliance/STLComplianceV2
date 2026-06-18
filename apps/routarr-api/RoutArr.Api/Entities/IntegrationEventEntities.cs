using STLCompliance.Shared.Data;

namespace RoutArr.Api.Entities;

public sealed class TenantIntegrationEventSettings : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public bool IsEnabled { get; set; } = true;

    public int MaxAttempts { get; set; } = 5;

    public int RetryIntervalMinutes { get; set; } = 15;

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class IntegrationOutboxEvent : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string EventKind { get; set; } = string.Empty;

    public string IdempotencyKey { get; set; } = string.Empty;

    public string RelatedEntityType { get; set; } = string.Empty;

    public Guid RelatedEntityId { get; set; }

    public string PayloadJson { get; set; } = string.Empty;

    public string ProcessingStatus { get; set; } = IntegrationEventStatuses.Pending;

    public int AttemptCount { get; set; }

    public DateTimeOffset? NextRetryAt { get; set; }

    public string? ErrorMessage { get; set; }

    public Guid CorrelationId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? ProcessedAt { get; set; }
}

public static class IntegrationEventStatuses
{
    public const string Pending = "pending";

    public const string Processed = "processed";

    public const string Abandoned = "abandoned";
}

public static class RoutArrIntegrationOutboxEventKinds
{
    public const string RouteCreated = "routarr.route.created";

    public const string RouteUpdated = "routarr.route.updated";

    public const string TripCreated = "routarr.trip.created";

    public const string TripReleased = "routarr.trip.released";

    public const string TripDispatched = "routarr.trip.dispatched";

    public const string TripAccepted = "routarr.trip.accepted";

    public const string TripStarted = "routarr.trip.started";

    public const string TripCompleted = "routarr.trip.completed";

    public const string TripCancelled = "routarr.trip.canceled";

    public const string DriverAssignmentChanged = "routarr.driver.assignment.changed";

    public const string EquipmentAssignmentChanged = "routarr.equipment.assignment.changed";

    public const string ComplianceOverridePerformed = "routarr.compliance.override.performed";

    public const string DispatchOverridePerformed = "routarr.dispatch.override.performed";

    public const string ComplianceHoldCreated = "routarr.compliance.hold.created";

    public const string ComplianceHoldReleased = "routarr.compliance.hold.released";

    public const string StopArrived = "routarr.stop.arrived";

    public const string StopEnRoute = "routarr.stop.en_route";

    public const string StopCompleted = "routarr.stop.completed";

    public const string StopMissed = "routarr.stop.missed";

    public const string ProofCreated = "routarr.proof.created";

    public const string ProofCaptured = "routarr.proof.captured";

    public const string DriverReportedDefect = "routarr.driver_reported_defect.created";

    public const string IncidentCreated = "routarr.incident.created";

    public const string ExceptionCreated = "routarr.exception.created";

    public const string ExceptionResolved = "routarr.exception.resolved";

    public const string TransportationDemandCreated = "routarr.transportation_demand.created";

    public const string TransportationDemandPlanned = "routarr.transportation_demand.planned";

    public const string TransportationDemandStatusChanged = "routarr.transportation_demand.status_changed";

    public const string TenderCreated = "routarr.tender.created";

    public const string TenderAccepted = "routarr.tender.accepted";

    public const string TenderRejected = "routarr.tender.rejected";

    public const string FreightRateEstimated = "routarr.freight_rate.estimated";

    public const string FreightRateActualized = "routarr.freight_rate.actualized";

    public const string AccessorialCreated = "routarr.accessorial.created";

    public const string FreightCostVarianceDetected = "routarr.freight_cost.variance_detected";

    public const string VisibilityEventReceived = "routarr.visibility_event.received";

    public const string GateIn = "routarr.gate.in";

    public const string GateOut = "routarr.gate.out";

    public const string TrailerDropped = "routarr.trailer.dropped";

    public const string TrailerHooked = "routarr.trailer.hooked";

    public const string FreightClaimRequested = "routarr.freight_claim.requested";

    public const string FinancePacketContributionReady = "routarr.finance_packet.contribution_ready";

    public const string TenantSettingsUpdated = "routarr.tenant_settings.updated";

    public const string TenantSettingsReset = "routarr.tenant_settings.reset";

    public const string TenantSettingOverrideCreated = "routarr.tenant_setting_override.created";

    public const string TenantSettingOverrideUpdated = "routarr.tenant_setting_override.updated";

    public const string TenantSettingOverrideDeleted = "routarr.tenant_setting_override.deleted";

    public const string TenantSettingsValidationFailed = "routarr.tenant_settings.validation_failed";
}
