# RoutArr — Scope, Ownership, and Boundaries

## Product purpose

RoutArr is the transportation, dispatch, route, trip, stop, ETA, proof, and route-exception execution product for the STL Compliance / ARR suite.

RoutArr answers:

- What route or trip needs to happen?
- Which driver is assigned?
- Which vehicle/trailer/equipment is assigned?
- Is the driver allowed/qualified?
- Is the equipment ready?
- What stops are planned?
- What is the current trip status?
- When did the driver arrive/depart?
- What proof was captured?
- What route exception occurred?
- What inbound appointment/ETA should LoadArr know about?
- What customer/order/delivery impact exists?

## RoutArr owns

```text
- Dispatch plan
- Dispatch board
- Route
- Trip
- Stop
- Stop sequence
- Driver assignment context
- Equipment assignment context
- ETA
- Arrival/departure events
- Proof of pickup
- Proof of delivery
- Route exception
- Transportation delay
- Dock appointment notification
- Inbound transportation visibility
- Transportation readiness validation result
- Route/trip execution status
- Transportation-origin events
```

## RoutArr does not own

```text
- Platform login
- Tenant entitlement
- Person master
- Driver employee profile
- Product permission assignment truth
- Training/certification truth
- Asset maintenance truth
- Vehicle readiness truth
- Inventory balance
- Stock ledger
- Warehouse receiving
- Dock receiving workflow
- Supplier/vendor master
- Customer master
- Customer order lifecycle
- Document/file storage truth
- Quality hold/release decision
- Regulatory rulepack meaning
- Reporting read models
- Accounting execution
```

## External product dependencies

```text
NexArr
- Product entitlement
- Login/handoff
- Service tokens

StaffArr
- Person references
- Driver/dispatcher/supervisor references
- Internal site/location/depot/dock identity
- Permission checks
- Personnel incidents from route/driver issues

TrainArr
- Driver qualification
- Equipment/route/customer/site qualification requirements
- Remediation training after incidents

Compliance Core
- Transportation rulepacks
- Driver/equipment/document compliance checks
- Evidence requirements
- Controlled catalogs

MaintainArr
- Vehicle/equipment asset references
- Asset readiness
- Open defects/out-of-service status
- Breakdown-generated defects/work orders

LoadArr
- Inbound dock appointment coordination
- Receiving readiness/status
- Staged inventory readiness for outbound delivery
- Load/pick/stage status where applicable

SupplyArr
- Supplier pickup/delivery context
- Supplier/carrier references where relevant

CustomArr
- Customer master
- Customer locations
- Customer contacts
- Customer delivery requirements
- Customer activity updates

OrdArr
- Order delivery demand
- Fulfillment dependencies
- Delivery status updates
- Order blockers

RecordArr
- BOL
- POD
- Signature
- Photos
- Route exception evidence
- Delivery/transport documents

AssurArr
- Shipment/order/asset quality holds
- Freight damage nonconformance
- Delivery quality issues
- Quality release before dispatch/delivery

ReportArr
- Transportation dashboards
- On-time KPIs
- Exception trends
- Proof capture metrics

Field Companion
- Driver mobile trip execution
- Stop actions
- Proof capture
- Exception reporting
- Document upload
```

## Core source-of-truth rules

```text
1. RoutArr owns transportation execution.
2. StaffArr owns driver/person identity and internal location identity.
3. TrainArr owns driver/equipment qualification truth.
4. MaintainArr owns vehicle/equipment readiness truth.
5. LoadArr owns receiving, staged inventory, dock receiving workflow, and stock truth.
6. CustomArr owns customer/location/contact truth.
7. OrdArr owns order lifecycle and fulfillment commitment.
8. RecordArr owns proof/document files.
9. AssurArr owns quality hold/release decisions.
10. Compliance Core owns transportation compliance meaning.
11. ReportArr owns reporting outputs, not trip truth.
12. RoutArr may notify LoadArr of dock appointments but must not perform receiving.
```

## Standard RoutArr object envelope

```text
RoutArrObject
- id
- tenantId
- objectNumber
- objectType
- status
- title
- description
- sourceProduct
- sourceObjectRef
- staffarrSiteId
- staffarrLocationId
- driverPersonId
- vehicleAssetRef
- customerRef
- orderRefs
- recordRefs
- complianceRefs
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
- completedAt
- canceledAt
- auditTrail
- eventLog
```

## RoutArr object prefixes

```text
DSP    Dispatch plan
RTE    Route
TRIP   Trip
STOP   Stop
DRV    Driver assignment validation
EQP    Equipment assignment validation
ETA    ETA event
ARR    Arrival/departure event
PROOF  Proof event
EXC    Route exception
DAPT   Dock appointment notification
LOAD   Transportation load visibility
DOC    Transportation document requirement
BLK    Transportation blocker
```

## Standard stop location reference

```text
StopLocationRef
- locationType
  - staffarr_internal_location
  - customer_location
  - supplier_location
  - ad_hoc_address
- staffarrLocationId
- customerLocationId
- supplierLocationId
- addressSnapshot
- displayNameSnapshot
- contactSnapshot
- instructionsSnapshot
- lastResolvedAt
```

## Standard transportation source trace

```text
TransportationSourceRef
- sourceProduct
- sourceObjectType
- sourceObjectId
- sourceObjectNumber
- displayNameSnapshot
- statusSnapshot
- lastResolvedAt
```


---


# RoutArr — Dispatch, Route, and Trip Model

## Dispatch plan

A DispatchPlan is a planning container for routes/trips for a date, shift, site, dispatcher, lane, customer, or operational group.

```text
DispatchPlan
- dispatchPlanId
- tenantId
- dispatchNumber
- title
- description
- dispatchDate
- dispatchType
  - daily
  - shift
  - customer
  - site
  - lane
  - inbound
  - outbound
  - mixed
  - emergency
  - ad_hoc
- status
  - draft
  - planning
  - ready_for_release
  - released
  - in_progress
  - completed
  - canceled
  - archived
- plannerPersonId
- dispatcherPersonId
- staffarrSiteId
- routeRefs
- tripRefs
- blockerRefs
- notes
- createdAt
- createdByPersonId
- updatedAt
- releasedAt
- releasedByPersonId
- completedAt
- canceledAt
- cancelReason
```

## Dispatch status definitions

```text
draft
- Dispatch plan exists but is not ready.

planning
- Routes/trips are being built and validated.

ready_for_release
- Plan is validated and ready to release.

released
- Routes/trips are released to drivers.

in_progress
- One or more trips are active.

completed
- All planned dispatch work is complete.

canceled
- Dispatch plan was canceled.

archived
- Retained for history only.
```

## Route

A Route is a planned sequence or transportation path. It may contain one or more trips or be equivalent to a trip depending implementation.

```text
Route
- routeId
- tenantId
- routeNumber
- name
- description
- routeType
  - pickup
  - delivery
  - linehaul
  - shuttle
  - service
  - mixed
  - internal_transfer
  - inbound
  - outbound
  - milk_run
  - emergency
- status
  - draft
  - planned
  - validated
  - assigned
  - released
  - in_progress
  - completed
  - canceled
- dispatchPlanRef
- originLocationRef
- destinationLocationRef
- plannedStartAt
- plannedEndAt
- actualStartAt
- actualEndAt
- driverPersonId
- vehicleAssetRef
- trailerAssetRefs
- stopRefs
- tripRefs
- orderRefs
- loadRefs
- customerRefs
- qualificationCheckRefs
- vehicleReadinessCheckRefs
- complianceCheckRefs
- blockerRefs
- exceptionRefs
- createdAt
- createdByPersonId
- updatedAt
```

## Route status definitions

```text
draft
- Route exists but is incomplete.

planned
- Route has stops/order context but is not validated.

validated
- Driver, equipment, compliance, and dependency checks have passed or warnings are accepted.

assigned
- Driver/equipment assignment exists.

released
- Route is released for execution.

in_progress
- Route execution has started.

completed
- Route execution is complete.

canceled
- Route was canceled.
```

## Trip

A Trip is a concrete execution instance, usually assigned to a driver and equipment.

```text
Trip
- tripId
- tenantId
- tripNumber
- routeId
- dispatchPlanRef
- tripType
  - pickup
  - delivery
  - inbound
  - outbound
  - transfer
  - service
  - linehaul
  - shuttle
  - mixed
- status
  - planned
  - assigned
  - validation_failed
  - released
  - driver_acknowledged
  - started
  - en_route
  - at_stop
  - delayed
  - exception
  - completed
  - canceled
- driverPersonId
- codriverPersonId
- vehicleAssetRef
- trailerAssetRefs
- equipmentRefs
- startLocationRef
- endLocationRef
- plannedDepartAt
- actualDepartAt
- plannedArriveAt
- actualArriveAt
- currentEta
- stopRefs
- activeStopRef
- orderRefs
- loadRefs
- customerRefs
- validationRefs
- exceptionRefs
- proofRefs
- documentRefs
- complianceRefs
- fieldCompanionSessionRefs
- createdAt
- updatedAt
- completedAt
- canceledAt
- cancelReason
```

## Trip status definitions

```text
planned
- Trip exists but is not assigned or released.

assigned
- Driver/equipment assignment exists.

validation_failed
- Driver, equipment, compliance, hold, or dependency validation failed.

released
- Trip has been released to driver.

driver_acknowledged
- Driver has accepted/acknowledged the trip.

started
- Driver started trip.

en_route
- Driver is traveling between stops.

at_stop
- Driver is currently at a stop.

delayed
- Delay exists but trip may continue.

exception
- Active serious exception exists.

completed
- Trip is complete.

canceled
- Trip was canceled.
```

## Trip segment

A TripSegment represents movement between stops.

```text
TripSegment
- segmentId
- tripId
- fromStopId
- toStopId
- sequence
- plannedDistance
- plannedDurationMinutes
- actualDurationMinutes
- plannedDepartAt
- actualDepartAt
- plannedArriveAt
- actualArriveAt
- status
  - planned
  - en_route
  - completed
  - skipped
  - canceled
- exceptionRefs
```

## Route load reference

RoutArr does not own inventory, but it can reference a load/order/shipment context.

```text
RouteLoadRef
- routeLoadRefId
- tenantId
- routeId
- tripId
- sourceProduct
  - ordarr
  - loadarr
  - supplyarr
  - maintainarr
  - manual
- sourceObjectRef
- loadNumberSnapshot
- loadDescriptionSnapshot
- statusSnapshot
- weightSnapshot
- volumeSnapshot
- handlingRequirementsSnapshot
- recordRefs
```

## Dispatch board item

```text
DispatchBoardItem
- boardItemId
- tenantId
- dispatchPlanId
- routeId
- tripId
- driverPersonId
- vehicleAssetRef
- status
- priority
- plannedDepartAt
- currentEta
- nextStopRef
- activeBlockers
- activeExceptions
- readinessStatus
- displayColorStatus
```

## Route blocker

```text
RouteBlocker
- blockerId
- tenantId
- routeId
- tripId
- blockerType
  - driver_unavailable
  - driver_unqualified
  - vehicle_unavailable
  - vehicle_not_ready
  - trailer_not_ready
  - quality_hold
  - order_block
  - inventory_not_staged
  - missing_document
  - compliance_failure
  - customer_requirement
  - dock_unavailable
  - system
- sourceProduct
- sourceObjectRef
- severity
  - low
  - moderate
  - high
  - critical
- status
  - active
  - resolved
  - overridden
  - canceled
- title
- description
- requiredAction
- createdAt
- resolvedAt
- resolvedByPersonId
- overrideReason
```

## Dispatch planning workflow

```text
1. Dispatcher creates DispatchPlan.
2. Dispatcher creates or imports route/order/load demand.
3. RoutArr builds Route and Trip records.
4. Driver and equipment are assigned.
5. RoutArr runs validation checks.
6. Blockers are created if validation fails.
7. Dispatcher resolves blockers or obtains override.
8. Route/trip is released.
9. Driver executes trip through Field Companion.
```

## Route lifecycle workflow

```text
1. Route is drafted.
2. Stops are added.
3. Orders/loads/customers are linked.
4. Driver/equipment assignment is selected.
5. Readiness/compliance checks run.
6. Route becomes validated.
7. Route is released.
8. Trip execution begins.
9. Stops are completed.
10. Route completes or is canceled.
```

## Events

```text
routarr.dispatch_plan.created
routarr.dispatch_plan.updated
routarr.dispatch_plan.ready_for_release
routarr.dispatch_plan.released
routarr.dispatch_plan.completed
routarr.dispatch_plan.canceled

routarr.route.created
routarr.route.planned
routarr.route.validated
routarr.route.assigned
routarr.route.released
routarr.route.started
routarr.route.completed
routarr.route.canceled

routarr.trip.created
routarr.trip.assigned
routarr.trip.validation_failed
routarr.trip.released
routarr.trip.driver_acknowledged
routarr.trip.started
routarr.trip.en_route
routarr.trip.at_stop
routarr.trip.delayed
routarr.trip.completed
routarr.trip.canceled

routarr.route_blocker.created
routarr.route_blocker.resolved
routarr.route_blocker.overridden
```


---


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
  - field_companion
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


---


# RoutArr — Driver, Equipment, Readiness, and Compliance Model

## Driver assignment

A DriverAssignment links a StaffArr person to a route/trip execution role. RoutArr owns the assignment record, not the person.

```text
DriverAssignment
- driverAssignmentId
- tenantId
- tripId
- routeId
- personId
- assignmentRole
  - primary_driver
  - co_driver
  - helper
  - trainee
  - trainer
  - supervisor
- status
  - proposed
  - assigned
  - acknowledged
  - declined
  - replaced
  - completed
  - canceled
- assignedByPersonId
- assignedAt
- acknowledgedAt
- declinedAt
- declineReason
- replacedByPersonId
- requiredQualificationRefs
- qualificationCheckRef
- permissionCheckRef
- readinessCheckRef
```

## Equipment assignment

EquipmentAssignment links MaintainArr assets to a trip. RoutArr owns the assignment, not the asset.

```text
EquipmentAssignment
- equipmentAssignmentId
- tenantId
- tripId
- routeId
- equipmentRole
  - power_unit
  - trailer
  - dolly
  - forklift
  - service_vehicle
  - container
  - other
- assetRef
- status
  - proposed
  - assigned
  - readiness_failed
  - replaced
  - released
  - completed
  - canceled
- assignedByPersonId
- assignedAt
- readinessCheckRef
- defectSnapshot
- restrictionSnapshot
```

## Route assignment validation

A validation is a point-in-time result showing whether a route/trip can be released.

```text
RouteAssignmentValidation
- validationId
- tenantId
- routeId
- tripId
- status
  - pass
  - warning
  - fail
  - manual_review
- driverPersonId
- vehicleAssetRef
- trailerAssetRefs
- staffarrPermissionCheck
- staffarrPersonStatusCheck
- trainarrQualificationCheck
- maintainarrVehicleReadinessCheck
- maintainarrTrailerReadinessChecks
- complianceCoreRuleCheck
- assurarrQualityHoldCheck
- ordarrOrderBlockerCheck
- loadarrInventoryReadinessCheck
- customarrCustomerRequirementCheck
- activeBlockers
- warnings
- evaluatedAt
- evaluatedBy
  - system
  - person
```

## Driver readiness check

```text
DriverReadinessCheck
- driverReadinessCheckId
- tenantId
- personId
- tripId
- routeId
- status
  - ready
  - warning
  - blocked
  - unknown
- staffarrStatusSnapshot
- staffarrPermissionSnapshot
- trainarrQualificationSnapshot
- activeRestrictionRefs
- activeIncidentRefs
- missingRequirementRefs
- evaluatedAt
```

## Equipment readiness check

```text
EquipmentReadinessCheck
- equipmentReadinessCheckId
- tenantId
- assetRef
- tripId
- routeId
- status
  - ready
  - limited
  - blocked
  - unsafe
  - unknown
- maintainarrReadinessSnapshot
- openDefectSnapshots
- outOfServiceFlag
- inspectionStatusSnapshot
- pmStatusSnapshot
- activeHoldRefs
- evaluatedAt
```

## Transportation compliance check

```text
TransportationComplianceCheck
- complianceCheckId
- tenantId
- tripId
- routeId
- checkType
  - driver
  - vehicle
  - route
  - customer
  - document
  - load
  - jurisdiction
  - site
- status
  - pass
  - warning
  - fail
  - not_applicable
  - unknown
  - manual_review
- complianceCoreEvaluationRef
- missingEvidenceRefs
- warningRefs
- blockerRefs
- evaluatedAt
```

## Transportation document requirement

```text
TransportationDocumentRequirement
- documentRequirementId
- tenantId
- tripId
- stopId
- requirementType
  - bol
  - pod
  - registration
  - insurance
  - permit
  - customer_document
  - driver_document
  - vehicle_inspection
  - hazmat_document
  - other
- required
- status
  - missing
  - uploaded
  - accepted
  - rejected
  - waived
- complianceRef
- customerRequirementRef
- recordRefs
```

## Driver availability

```text
DriverAvailability
- availabilityId
- tenantId
- personId
- date
- status
  - available
  - assigned
  - unavailable
  - on_leave
  - restricted
  - unknown
- sourceProduct
  - staffarr
  - routarr
  - trainarr
  - manual
- sourceObjectRef
- notes
```

## Equipment availability

```text
EquipmentAvailability
- availabilityId
- tenantId
- assetRef
- date
- status
  - available
  - assigned
  - down
  - limited
  - out_of_service
  - on_hold
  - unknown
- sourceProduct
  - maintainarr
  - routarr
  - assurarr
  - manual
- sourceObjectRef
- notes
```

## Assignment validation workflow

```text
1. Dispatcher assigns driver and equipment.
2. RoutArr checks StaffArr person status and permission.
3. RoutArr checks TrainArr qualification.
4. RoutArr checks MaintainArr asset readiness.
5. RoutArr checks AssurArr quality holds.
6. RoutArr checks OrdArr order blockers.
7. RoutArr checks LoadArr inventory readiness if relevant.
8. RoutArr checks Compliance Core transportation requirements.
9. Validation returns pass/warning/fail.
10. Failing checks create blockers.
```

## Driver replacement workflow

```text
1. Assigned driver becomes unavailable, unqualified, restricted, or declines trip.
2. RoutArr marks DriverAssignment replaced/declined.
3. Dispatcher selects new driver.
4. Validation reruns.
5. Trip can release only after validation passes or override is accepted.
```

## Equipment replacement workflow

```text
1. Assigned asset becomes down/unsafe/held/unavailable.
2. RoutArr marks EquipmentAssignment readiness_failed or replaced.
3. Dispatcher selects replacement asset.
4. MaintainArr readiness check reruns.
5. Trip validation updates.
```

## Compliance document workflow

```text
1. Route/trip requires document.
2. RoutArr creates TransportationDocumentRequirement.
3. Driver/dispatcher uploads document through Field Companion/RecordArr.
4. Compliance Core may evaluate document/evidence.
5. Requirement becomes accepted, rejected, waived, or missing.
6. Trip release/completion may depend on status.
```

## Events

```text
routarr.driver_assignment.created
routarr.driver_assignment.assigned
routarr.driver_assignment.acknowledged
routarr.driver_assignment.declined
routarr.driver_assignment.replaced
routarr.driver_assignment.completed

routarr.equipment_assignment.created
routarr.equipment_assignment.assigned
routarr.equipment_assignment.readiness_failed
routarr.equipment_assignment.replaced
routarr.equipment_assignment.completed

routarr.assignment_validation.completed
routarr.assignment_validation.failed
routarr.driver_readiness.checked
routarr.equipment_readiness.checked
routarr.transportation_compliance.checked

routarr.document_requirement.created
routarr.document_requirement.satisfied
routarr.document_requirement.rejected
routarr.document_requirement.waived
```


---


# RoutArr — Dock Appointment and Load Visibility Model

## Dock appointment notification

RoutArr notifies LoadArr about inbound transportation and dock appointment events when RoutArr has visibility or controls the move.

RoutArr does not own receiving. LoadArr owns receiving workflow, dock receiving execution, staging, putaway, inventory balance, and stock ledger.

```text
DockAppointmentNotification
- dockAppointmentNotificationId
- tenantId
- notificationNumber
- sourceTripId
- sourceRouteId
- sourceStopId
- loadarrExpectedReceiptRef
- staffarrSiteId
- staffarrDockLocationId
- appointmentType
  - request
  - update
  - cancel
  - eta_update
  - arrival
  - departure
  - delay
  - exception
- requestedWindowStart
- requestedWindowEnd
- confirmedWindowStart
- confirmedWindowEnd
- eta
- status
  - draft
  - sent
  - acknowledged
  - confirmed
  - rejected
  - updated
  - canceled
  - completed
- carrierSnapshot
- driverSnapshot
- vehicleSnapshot
- trailerSnapshot
- sourceProduct
- sourceObjectRef
- rejectionReason
- sentAt
- acknowledgedAt
- confirmedAt
- canceledAt
```

## Dock appointment response

```text
DockAppointmentResponse
- dockAppointmentResponseId
- tenantId
- dockAppointmentNotificationId
- respondingProduct
  - loadarr
  - routarr
- status
  - accepted
  - rejected
  - proposed_alternative
  - canceled
  - completed
- confirmedWindowStart
- confirmedWindowEnd
- assignedDockLocationId
- message
- respondedAt
- respondedByPersonId
```

## Transportation load visibility

TransportationLoadVisibility is RoutArr’s view of what is being transported. It is not inventory truth.

```text
TransportationLoadVisibility
- transportationLoadVisibilityId
- tenantId
- loadNumber
- tripId
- routeId
- sourceProduct
  - ordarr
  - loadarr
  - supplyarr
  - maintainarr
  - manual
- sourceObjectRef
- loadType
  - inbound_receipt
  - outbound_order
  - internal_transfer
  - customer_return
  - supplier_return
  - maintenance_transfer
  - mixed
- status
  - planned
  - ready
  - staged
  - loaded
  - in_transit
  - delivered
  - exception
  - canceled
- originLocationRef
- destinationLocationRef
- customerRef
- supplierRef
- orderRefs
- expectedReceiptRefs
- itemSummarySnapshot
- handlingRequirements
- temperatureRequirement
- hazmatFlag
- weightSnapshot
- volumeSnapshot
- sealNumber
- documentRefs
- createdAt
- updatedAt
```

## Load item summary

```text
LoadItemSummary
- loadItemSummaryId
- transportationLoadVisibilityId
- itemRef
- itemDescriptionSnapshot
- quantitySnapshot
- unitOfMeasure
- lotNumber
- serialNumber
- sourceProductLineRef
- handlingNotes
```

## Carrier reference

If a carrier is a supplier/vendor, SupplyArr owns the master. RoutArr stores route-specific snapshot/context.

```text
CarrierSnapshot
- supplierRef
- carrierNameSnapshot
- mcNumberSnapshot
- dotNumberSnapshot
- contactSnapshot
- phoneSnapshot
- statusSnapshot
```

## Transportation appointment

A TransportationAppointment is a RoutArr-side appointment object used for planning. LoadArr owns dock receiving schedule/confirmation for receiving operations.

```text
TransportationAppointment
- transportationAppointmentId
- tenantId
- appointmentNumber
- tripId
- stopId
- appointmentType
  - pickup
  - delivery
  - dock
  - customer
  - supplier
  - internal
- status
  - requested
  - confirmed
  - rejected
  - rescheduled
  - arrived
  - completed
  - canceled
- requestedWindowStart
- requestedWindowEnd
- confirmedWindowStart
- confirmedWindowEnd
- locationRef
- contactSnapshot
- loadVisibilityRef
- sourceProduct
- sourceObjectRef
- notes
```

## Load readiness check

RoutArr may ask LoadArr/OrdArr whether outbound load/order is ready for transportation.

```text
LoadReadinessCheck
- loadReadinessCheckId
- tenantId
- tripId
- routeId
- sourceProduct
  - loadarr
  - ordarr
  - supplyarr
  - maintainarr
- sourceObjectRef
- status
  - ready
  - not_ready
  - partially_ready
  - blocked
  - unknown
- readinessDetails
- blockerRefs
- checkedAt
```

## Inbound dock appointment workflow

```text
1. RoutArr has inbound trip/load visibility.
2. RoutArr creates DockAppointmentNotification.
3. RoutArr sends appointment request/update to LoadArr.
4. LoadArr validates dock/location receiving availability.
5. LoadArr confirms, rejects, or proposes alternative.
6. RoutArr updates TransportationAppointment.
7. RoutArr sends ETA updates as trip progresses.
8. Driver arrives.
9. RoutArr sends arrival event.
10. LoadArr performs receiving.
11. Driver departs.
12. RoutArr sends departure event.
```

## Outbound load readiness workflow

```text
1. OrdArr order requires delivery.
2. LoadArr picks/stages inventory.
3. RoutArr requests LoadReadinessCheck.
4. LoadArr returns ready/not ready/blocked.
5. If ready, trip can be released if other validations pass.
6. If not ready, RoutArr creates blocker.
7. Once LoadArr stages/updates, RoutArr resolves blocker.
```

## Supplier pickup workflow

```text
1. SupplyArr or OrdArr creates pickup need.
2. RoutArr creates route/trip/stop.
3. Supplier location/contact context is attached.
4. Driver executes pickup.
5. Proof of pickup is captured.
6. Load visibility status updates.
7. Receiving/transfer/order status updates flow to source products.
```

## Customer return workflow

```text
1. CustomArr/OrdArr creates return context.
2. RoutArr creates pickup trip.
3. Driver captures return proof/photos/documents.
4. LoadArr receives returned goods.
5. AssurArr handles quality issue if needed.
6. OrdArr/CustomArr receive status updates.
```

## Dock/load events

```text
routarr.dock_appointment.requested
routarr.dock_appointment.updated
routarr.dock_appointment.confirmed
routarr.dock_appointment.rejected
routarr.dock_appointment.canceled
routarr.dock_appointment.eta_updated
routarr.dock_appointment.arrived
routarr.dock_appointment.departed
routarr.dock_appointment.completed

routarr.transportation_load.created
routarr.transportation_load.ready
routarr.transportation_load.staged
routarr.transportation_load.loaded
routarr.transportation_load.in_transit
routarr.transportation_load.delivered
routarr.transportation_load.exception
routarr.transportation_load.canceled

routarr.load_readiness.checked
routarr.load_readiness.blocked
routarr.load_readiness.ready
```


---


# RoutArr — Workflows, Status Logic, Events, and APIs

## Major workflow: dispatch and route release

```text
1. Dispatcher creates DispatchPlan.
2. Orders/loads/stops are imported or manually added.
3. RoutArr creates Route and Trip.
4. Driver is selected from StaffArr people.
5. Vehicle/trailer assets are selected from MaintainArr.
6. RoutArr checks StaffArr permissions/person status.
7. RoutArr checks TrainArr qualifications.
8. RoutArr checks MaintainArr asset readiness.
9. RoutArr checks AssurArr holds.
10. RoutArr checks OrdArr order blockers and LoadArr readiness where applicable.
11. RoutArr checks Compliance Core transportation requirements.
12. If validation passes, trip is released.
13. Driver executes trip through Field Companion.
```

## Major workflow: driver trip execution

```text
1. Driver opens assigned trip in Field Companion.
2. Driver acknowledges trip.
3. Driver starts trip.
4. Driver travels to stop.
5. Driver marks arrival.
6. Driver completes stop requirements.
7. Driver captures proof/evidence.
8. Driver departs stop.
9. Trip advances to next stop.
10. Trip completes after final stop.
```

## Major workflow: delivery proof and order update

```text
1. Driver completes delivery stop.
2. Field Companion captures POD/signature/photo/document.
3. RecordArr stores evidence.
4. RoutArr creates ProofEvent.
5. OrdArr receives fulfillment/proof update.
6. CustomArr receives customer activity update.
7. ReportArr receives delivery performance facts.
```

## Major workflow: damaged freight

```text
1. Driver reports damaged freight.
2. RoutArr creates RouteException.
3. Driver captures photos/documents.
4. RecordArr stores evidence.
5. AssurArr creates Nonconformance/QualityHold if needed.
6. OrdArr receives order blocker/update.
7. CustomArr receives customer issue/activity if needed.
8. RoutArr resolves exception after mitigation.
```

## Major workflow: vehicle breakdown

```text
1. Driver reports breakdown.
2. RoutArr creates RouteException.
3. RoutArr notifies MaintainArr.
4. MaintainArr creates Defect/WorkOrder.
5. RoutArr delays/replans trip.
6. OrdArr receives fulfillment delay if orders affected.
7. CustomArr receives customer impact if needed.
8. StaffArr receives incident if driver/safety/personnel issue exists.
```

## Major workflow: inbound dock appointment

```text
1. RoutArr controls/sees inbound trip.
2. RoutArr sends DockAppointmentNotification to LoadArr.
3. LoadArr confirms/rejects/proposes alternative.
4. RoutArr sends ETA updates.
5. Driver arrives.
6. RoutArr sends arrival event.
7. LoadArr performs receiving.
8. Driver departs.
9. RoutArr sends departure event.
```

## Major workflow: compliance block

```text
1. RoutArr validates trip.
2. Compliance Core returns failed/warning evaluation.
3. RoutArr creates blocker or warning.
4. Dispatcher resolves missing document/evidence/condition.
5. RecordArr stores required evidence.
6. Compliance Core reevaluates if needed.
7. Trip releases only when allowed or override is approved.
```

## RoutArr emitted events

```text
routarr.dispatch_plan.created
routarr.dispatch_plan.updated
routarr.dispatch_plan.released
routarr.dispatch_plan.completed
routarr.dispatch_plan.canceled

routarr.route.created
routarr.route.planned
routarr.route.validated
routarr.route.assigned
routarr.route.released
routarr.route.started
routarr.route.completed
routarr.route.canceled

routarr.trip.created
routarr.trip.assigned
routarr.trip.validation_failed
routarr.trip.released
routarr.trip.driver_acknowledged
routarr.trip.started
routarr.trip.en_route
routarr.trip.at_stop
routarr.trip.delayed
routarr.trip.exception
routarr.trip.completed
routarr.trip.canceled

routarr.stop.created
routarr.stop.en_route
routarr.stop.arrived
routarr.stop.in_progress
routarr.stop.completed
routarr.stop.skipped
routarr.stop.failed
routarr.stop.canceled

routarr.proof_event.captured
routarr.proof_event.accepted
routarr.proof_event.rejected

routarr.exception.created
routarr.exception.escalated
routarr.exception.resolved
routarr.exception.closed

routarr.driver_assignment.assigned
routarr.driver_assignment.acknowledged
routarr.driver_assignment.declined
routarr.driver_assignment.replaced

routarr.equipment_assignment.assigned
routarr.equipment_assignment.readiness_failed
routarr.equipment_assignment.replaced

routarr.assignment_validation.completed
routarr.assignment_validation.failed

routarr.dock_appointment.requested
routarr.dock_appointment.updated
routarr.dock_appointment.confirmed
routarr.dock_appointment.rejected
routarr.dock_appointment.eta_updated
routarr.dock_appointment.arrived
routarr.dock_appointment.departed

routarr.eta.updated
routarr.delay.started
routarr.delay.resolved
```

## Integration APIs RoutArr should expose

```text
GET /api/v1/integrations/dispatch-plans
GET /api/v1/integrations/dispatch-plans/{dispatchPlanId}
POST /api/v1/integrations/dispatch-plans

GET /api/v1/integrations/routes
GET /api/v1/integrations/routes/{routeId}
POST /api/v1/integrations/routes
POST /api/v1/integrations/routes/{routeId}/release
POST /api/v1/integrations/routes/{routeId}/cancel

GET /api/v1/integrations/trips
GET /api/v1/integrations/trips/{tripId}
POST /api/v1/integrations/trips
POST /api/v1/integrations/trips/{tripId}/assign-driver
POST /api/v1/integrations/trips/{tripId}/assign-equipment
POST /api/v1/integrations/trips/{tripId}/start
POST /api/v1/integrations/trips/{tripId}/complete
POST /api/v1/integrations/trips/{tripId}/cancel

GET /api/v1/integrations/stops/{stopId}
POST /api/v1/integrations/stops
POST /api/v1/integrations/stops/{stopId}/arrive
POST /api/v1/integrations/stops/{stopId}/depart
POST /api/v1/integrations/stops/{stopId}/complete
POST /api/v1/integrations/stops/{stopId}/fail

POST /api/v1/integrations/proof-events
GET /api/v1/integrations/proof-events/{proofEventId}

POST /api/v1/integrations/exceptions
GET /api/v1/integrations/exceptions/{exceptionId}
POST /api/v1/integrations/exceptions/{exceptionId}/resolve
POST /api/v1/integrations/exceptions/{exceptionId}/close

POST /api/v1/integrations/assignment-validations
POST /api/v1/integrations/driver-readiness-checks
POST /api/v1/integrations/equipment-readiness-checks

POST /api/v1/integrations/dock-appointments
POST /api/v1/integrations/dock-appointments/{notificationId}/status-updates
POST /api/v1/integrations/load-readiness-checks

GET /api/v1/integrations/eta/{tripId}
POST /api/v1/integrations/eta-updates
```

## APIs RoutArr should consume

```text
NexArr
- POST /handoff/redeem
- POST /service-tokens/introspect
- GET /entitlements/{productKey}

StaffArr
- GET /persons/{personId}
- GET /persons/{personId}/permissions
- GET /persons/{personId}/readiness
- GET /locations/{locationId}
- GET /sites
- POST /incidents

TrainArr
- POST /qualification-checks
- GET /persons/{personId}/qualifications

Compliance Core
- GET /rulepacks
- POST /evaluations
- GET /evidence-requirements

MaintainArr
- GET /assets/{assetId}
- GET /assets/{assetId}/readiness
- POST /route-exceptions
- POST /work-orders

LoadArr
- POST /dock-appointments
- POST /load-readiness-checks
- GET /expected-receipts/{expectedReceiptId}
- POST /receiving-status-updates

SupplyArr
- GET /suppliers/{supplierId}
- GET /purchase-orders/{purchaseOrderId}

CustomArr
- GET /customers/{customerId}
- GET /customer-locations/{customerLocationId}
- POST /customer-activities
- POST /customer-issues

OrdArr
- GET /orders/{orderId}
- POST /orders/{orderId}/fulfillment-records
- POST /orders/{orderId}/blockers
- POST /orders/{orderId}/status-updates

RecordArr
- POST /records
- POST /upload-sessions
- GET /records/{recordId}

AssurArr
- GET /holds
- POST /nonconformances
- POST /quality-events

ReportArr
- POST /events
```

## Permission examples

```text
routarr.dispatch.read
routarr.dispatch.create
routarr.dispatch.release
routarr.dispatch.cancel

routarr.routes.read
routarr.routes.create
routarr.routes.update
routarr.routes.validate
routarr.routes.release
routarr.routes.cancel

routarr.trips.read
routarr.trips.create
routarr.trips.assign_driver
routarr.trips.assign_equipment
routarr.trips.execute
routarr.trips.complete
routarr.trips.cancel

routarr.stops.read
routarr.stops.execute
routarr.stops.override

routarr.proof.capture
routarr.proof.review
routarr.proof.reject

routarr.exceptions.read
routarr.exceptions.create
routarr.exceptions.resolve
routarr.exceptions.close

routarr.dock_appointments.create
routarr.dock_appointments.update
routarr.dock_appointments.cancel

routarr.admin
```

## Default role examples

```text
RoutArr Viewer
- Read dispatch, routes, trips, stops, and exceptions.

Dispatcher
- Create/edit routes and trips.
- Assign drivers/equipment.
- Release dispatch plans.
- Manage exceptions.

Driver
- View assigned trips.
- Execute stops.
- Capture proof.
- Report exceptions.

Dispatch Supervisor
- Approve overrides.
- Resolve exceptions.
- Reassign drivers/equipment.
- Review proof.

Transportation Admin
- Manage route settings, stop templates, validation rules, and permissions.

Dock Appointment Coordinator
- Manage inbound/outbound appointment coordination.

RoutArr Admin
- Full RoutArr configuration and administrative access.
```

## RoutArr UI surfaces

```text
/app/routarr
- dashboard
- dispatch board
- dispatch plans
- route planner
- routes
- route detail
- trips
- trip detail
- stops
- exceptions
- proof review
- dock appointments
- load visibility
- driver/equipment assignment
- validation/blockers
- settings
```

## Dispatch board UI

```text
DispatchBoardPage
- Date/shift/site filters
- Route/trip cards
- Driver assignment
- Equipment assignment
- Status lanes
- Active blockers
- Active exceptions
- ETA indicators
- Release controls
```

## Trip detail UI

```text
TripDetailPage
- Header
  - tripNumber
  - status
  - driver
  - vehicle/trailer
  - current ETA
- Stops
  - sequence
  - planned/actual times
  - proof status
- Validation
  - driver readiness
  - equipment readiness
  - compliance checks
  - blockers
- Load/order context
- Exceptions
- Documents/proof
- Timeline
```

## Stop detail UI

```text
StopDetailPage
- Stop header
- Location/contact/instructions
- Appointment window
- Requirements
- Proof capture/review
- Arrival/departure events
- Exceptions
- Linked orders/loads
```

## Exception detail UI

```text
ExceptionDetailPage
- Exception header
- Severity/status
- Source trip/stop
- Affected orders/customers/loads
- Evidence
- Escalations
- Mitigation actions
- Closure summary
- Timeline
```
