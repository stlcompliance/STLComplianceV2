# RoutArr - Yard, Trailer, Gate, and Drop/Hook Model

RoutArr owns transportation yard events and dispatch impact. StaffArr owns internal location identity, MaintainArr owns asset truth/readiness, and LoadArr owns dock/warehouse receiving execution.

## Yard event

```text
TransportationYardEvent
- yardEventId
- transportationDemandId
- tripId
- eventType
  - gate_in
  - gate_out
  - trailer_dropped
  - trailer_hooked
  - staged_at_dock
  - moved_in_yard
  - seal_verified
  - detention_started
  - detention_ended
  - dwell_started
  - dwell_ended
- trailerAssetRef
- tractorAssetRef
- staffarrYardLocationRef
- staffarrDockLocationRef
- loadedEmptyStatus
  - loaded
  - empty
  - unknown
- sealNumber
- source
  - dispatcher
  - driver
  - fieldcompanion
  - gate_system
  - integration
  - system
- occurredAt
- evidenceRefs
- dispatchImpact
```

## Events

```text
routarr.gate.in
routarr.gate.out
routarr.trailer.dropped
routarr.trailer.hooked
routarr.trailer.staged_at_dock
routarr.trailer.seal_verified
routarr.detention.started
routarr.detention.ended
routarr.dwell.started
routarr.dwell.ended
```

