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

## V1 trip alias surface

```text
GET /api/v1/trips/by-number/{tripNumber}
```

## APIs RoutArr should consume

```text
NexArr
- POST /api/v1/platform/handoff/redeem
- POST /api/v1/platform/service-tokens/introspect
- GET /api/v1/platform/tenants/{tenantId}/entitlements/{productKey}

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
