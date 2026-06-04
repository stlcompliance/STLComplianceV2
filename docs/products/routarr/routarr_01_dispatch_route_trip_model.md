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
