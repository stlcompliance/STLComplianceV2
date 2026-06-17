# RoutArr - Visibility, Tracking, and Telematics Model

RoutArr consumes and normalizes tracking signals. ELD, telematics, GPS, and hardware vendors remain external owners of certified hardware capture.

## Transportation visibility event

```text
TransportationVisibilityEvent
- visibilityEventId
- transportationDemandId
- tripId
- stopId
- eventType
  - position
  - status_update
  - geofence_arrival
  - geofence_departure
  - eta_update
  - carrier_check_call
  - appointment_update
  - exception
  - trailer_tracking
  - eld_hos_snapshot
- source
  - fieldcompanion
  - dispatcher
  - telematics
  - eld
  - carrier_api
  - customer_portal
  - vendor_portal
  - system_calculated
  - manual_check_call
- sourceOccurredAt
- receivedAt
- normalizedStatus
- latitude
- longitude
- eta
- etaConfidence
- freshnessState
- reviewStatus
  - accepted
  - review_required
  - rejected
  - superseded
- rawExternalRef
- summary
```

## Tracking snapshot

```text
TransportationTrackingSnapshot
- trackingSnapshotId
- transportationDemandId
- tripId
- currentStatus
- currentLatitude
- currentLongitude
- currentEta
- etaConfidence
- lastVisibilityEventRef
- trackingSource
- sourceHierarchyApplied
- freshnessState
- staleReason
- updatedAt
```

## Events

```text
routarr.visibility_event.received
routarr.visibility_event.review_required
routarr.visibility_event.accepted
routarr.visibility_event.rejected
routarr.tracking_snapshot.updated
routarr.geofence.arrived
routarr.geofence.departed
routarr.eta.confidence_changed
```

