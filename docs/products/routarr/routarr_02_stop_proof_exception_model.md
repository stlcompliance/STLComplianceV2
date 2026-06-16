# RoutArr — Stop, Proof, and Exception Model

## Stop

A Stop is a planned or executed location event within a trip.

```text
Stop
- stopId
- tenantId
- stopNumber
- tripId
- routeId
- sequence
- stopType
  - pickup
  - delivery
  - dock_appointment
  - fuel
  - break
  - inspection
  - service
  - internal_transfer
  - customer_visit
  - supplier_pickup
  - maintenance
  - other
- status
  - planned
  - en_route
  - arrived
  - in_progress
  - completed
  - skipped
  - failed
  - canceled
- locationType
  - staffarr_internal_location
  - customer_location
  - supplier_location
  - ad_hoc_address
- staffarrLocationRef
- customerLocationRef
- supplierLocationRef
- addressSnapshot
- contactSnapshot
- plannedArriveAt
- actualArriveAt
- plannedDepartAt
- actualDepartAt
- appointmentWindowStart
- appointmentWindowEnd
- timeZone
- instructions
- accessRequirements
- requiredEvidence
- proofRefs
- exceptionRefs
- orderRefs
- loadRefs
- documentRefs
- createdAt
- updatedAt
```

`supplierLocationRef` must resolve to SupplyArr `SupplierLocation`, not a generic supplier address. Address snapshots may be stored for history, but SupplyArr owns the operational supplier/vendor location identity.

## Stop status definitions

```text
planned
- Stop is planned but not active.

en_route
- Driver is traveling to stop.

arrived
- Driver arrived at stop.

in_progress
- Stop work is being performed.

completed
- Stop is complete.

skipped
- Stop was intentionally skipped.

failed
- Stop could not be completed.

canceled
- Stop was canceled.
```

## Stop requirement

```text
StopRequirement
- stopRequirementId
- stopId
- requirementType
  - signature
  - photo
  - document_upload
  - bol
  - pod
  - temperature_reading
  - seal_number
  - trailer_number
  - customer_confirmation
  - appointment_confirmation
  - access_code
  - compliance_prompt
  - safety_acknowledgement
- required
- status
  - pending
  - satisfied
  - waived
  - failed
- complianceRef
- evidenceRecordRefs
```

## Stop event

```text
StopEvent
- stopEventId
- tenantId
- stopId
- tripId
- eventType
  - en_route
  - arrived
  - departed
  - started_service
  - completed_service
  - skipped
  - failed
  - delayed
  - proof_captured
  - exception_reported
- occurredAt
- actorPersonId
- source
  - fieldcompanion
  - dispatcher
  - integration
  - system
- geoCoordinates
- notes
- recordRefs
```

## Proof event

ProofEvent captures evidence for pickup, delivery, attempted delivery, refused delivery, damaged freight, customer acceptance, or other stop proof.

```text
ProofEvent
- proofEventId
- tenantId
- proofNumber
- tripId
- routeId
- stopId
- proofType
  - pickup
  - delivery
  - attempted_delivery
  - refused_delivery
  - damaged_delivery
  - customer_acceptance
  - customer_rejection
  - document_only
  - photo_only
- status
  - draft
  - captured
  - submitted
  - accepted
  - rejected
  - superseded
- capturedAt
- capturedByPersonId
- recipientName
- recipientTitle
- recipientCompany
- signatureRecordRef
- photoRecordRefs
- documentRecordRefs
- bolRecordRef
- podRecordRef
- notes
- geoCoordinates
- deviceSnapshot
- sourceProductUpdateRefs
- createdAt
- updatedAt
```

## Proof requirement

```text
ProofRequirement
- proofRequirementId
- stopId
- proofType
  - signature
  - photo
  - bol
  - pod
  - document
  - seal
  - temperature
  - customer_name
  - geolocation
- required
- minimumCount
- allowedDocumentTypes
- complianceRef
- customerRequirementRef
- status
  - pending
  - satisfied
  - waived
  - failed
```

## Route exception

A RouteException is a transportation execution problem or abnormal event.

```text
RouteException
- exceptionId
- tenantId
- exceptionNumber
- tripId
- routeId
- stopId
- exceptionType
  - late_departure
  - late_arrival
  - missed_pickup
  - missed_delivery
  - refused_delivery
  - customer_unavailable
  - damaged_freight
  - breakdown
  - accident
  - weather
  - compliance_block
  - driver_unavailable
  - equipment_unavailable
  - dock_delay
  - inventory_not_ready
  - missing_document
  - wrong_address
  - access_denied
  - safety_issue
  - other
- severity
  - low
  - moderate
  - high
  - critical
- status
  - open
  - investigating
  - mitigated
  - escalated
  - resolved
  - closed
  - canceled
- reportedAt
- reportedByPersonId
- ownerPersonId
- description
- immediateAction
- staffarrLocationId
- affectedOrderRefs
- affectedLoadRefs
- affectedCustomerRefs
- affectedSupplierRefs
- maintainarrWorkOrderRef
- staffarrIncidentRef
- assurarrNonconformanceRef
- ordarrBlockerRefs
- recordRefs
- closedAt
- closedByPersonId
- closureSummary
```

## Exception escalation rule examples

```text
breakdown
- Create or notify MaintainArr defect/work order.
- Notify OrdArr if order impact exists.
- Notify CustomArr if customer impact exists.

accident or safety issue
- Notify StaffArr personnel incident.
- Notify MaintainArr if asset damage exists.
- Notify AssurArr if quality/customer/product damage exists.

damaged freight
- Notify AssurArr nonconformance.
- Notify OrdArr order blocker.
- Notify CustomArr customer issue/activity.
- Store evidence in RecordArr.

inventory_not_ready
- Notify LoadArr/OrdArr.
- Trip may be delayed or blocked.

missing_document
- Request RecordArr upload.
- Notify Compliance Core if compliance evidence issue exists.

dock_delay
- Notify LoadArr if inbound/outbound dock appointment affected.
```

## Delay event

```text
DelayEvent
- delayEventId
- tenantId
- tripId
- stopId
- delayType
  - traffic
  - weather
  - dock
  - customer
  - equipment
  - driver
  - inventory
  - compliance
  - unknown
- status
  - active
  - resolved
- startedAt
- endedAt
- durationMinutes
- etaImpactMinutes
- description
- source
  - driver
  - dispatcher
  - integration
  - system
```

## ETA event

```text
EtaEvent
- etaEventId
- tenantId
- tripId
- stopId
- eta
- etaSource
  - manual
  - driver_update
  - gps
  - integration
  - system_calculated
- confidence
  - low
  - medium
  - high
- reason
- createdAt
- createdByPersonId
```

## Stop execution workflow

```text
1. Driver opens stop in Field Companion.
2. Driver marks en route or system detects en route.
3. Driver arrives.
4. Stop requirements are shown.
5. Driver performs pickup/delivery/service action.
6. Required proof is captured.
7. Driver departs.
8. Stop status becomes completed.
9. Trip advances to next stop or completes.
```

## Proof workflow

```text
1. Stop requires proof.
2. Field Companion renders proof requirements.
3. Driver captures signature, photos, BOL/POD, notes, or other evidence.
4. RecordArr stores evidence.
5. RoutArr creates ProofEvent.
6. OrdArr/CustomArr/LoadArr receive proof/status updates as needed.
```

## Exception workflow

```text
1. Driver/dispatcher/system reports exception.
2. RoutArr creates RouteException.
3. RoutArr classifies severity and affected stops/orders/loads.
4. RoutArr updates route/trip status.
5. Cross-product escalations are created as needed.
6. Dispatcher mitigates/resolves.
7. Exception closes with evidence and summary.
```

## Events

```text
routarr.stop.created
routarr.stop.updated
routarr.stop.en_route
routarr.stop.arrived
routarr.stop.in_progress
routarr.stop.completed
routarr.stop.skipped
routarr.stop.failed
routarr.stop.canceled

routarr.stop_requirement.created
routarr.stop_requirement.satisfied
routarr.stop_requirement.waived
routarr.stop_requirement.failed

routarr.proof_event.created
routarr.proof_event.captured
routarr.proof_event.submitted
routarr.proof_event.accepted
routarr.proof_event.rejected

routarr.exception.created
routarr.exception.escalated
routarr.exception.mitigated
routarr.exception.resolved
routarr.exception.closed

routarr.delay.started
routarr.delay.resolved
routarr.eta.updated
```
