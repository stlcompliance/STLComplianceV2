# STL Compliance Platform Scheduling And Created Events Constitution

## 1. Purpose

This constitution defines suite-wide created-event handling, schedulable demand, scheduled execution, and drag/drop scheduling boundaries.

Scheduling is a shared interaction pattern. It is not a product and it is not a central source of truth.

## 2. Prime Directive

The owning product owns the work record and the scheduled execution record.

The shared scheduling board may render, filter, validate, and launch product-owned actions, but it must call the owning product API for every write.

## 3. Four Separate Concepts

Submission or intake:
- A source product stores submitted information.
- Examples: CustomArr portal submission, Field Companion mobile defect report, RecordArr document upload.

Canonical business record:
- The owning product creates, accepts, rejects, holds, or updates the actual domain record.
- Examples: OrdArr order, MaintainArr work order, RoutArr trip, LoadArr dock appointment.

Schedulable demand:
- A product-owned record needs time, people, equipment, location, dock, bay, trainer, driver, technician, inspector, or other resources.
- This demand is not the same thing as scheduled execution.

Scheduled execution event:
- The owning product commits an assignment to a time, resource, location, or equipment window.
- The calendar block is a display of this product-owned scheduled execution.

## 4. Created Events And Commands

Created events announce facts that happened.

Commands and API requests ask an owning product to do something.

Consumers must not blindly mutate their own state from another product's created event. Any cross-product record creation must go through the target owning product's API, command handler, or explicit handoff.

## 5. Customer Portal To Order Rule

When a customer places an order through a CustomArr portal:

1. CustomArr stores the raw portal submission.
2. CustomArr emits `customarr.portalSubmission.created`.
3. CustomArr calls OrdArr create order/request with an idempotency key.
4. OrdArr creates or rejects the canonical order/request.
5. OrdArr emits `ordarr.order.requested` or `ordarr.order.created`.
6. OrdArr owns order acceptance, rejection, hold, promised windows, change, cancellation, and fulfillment orchestration.
7. Execution products create their own demand only through their own APIs or handlers.

CustomArr must not create RoutArr trips, LoadArr dock appointments, MaintainArr work orders, TrainArr assignments, AssurArr quality checks, or SupplyArr purchase orders directly for ordinary customer orders.

## 6. Requested, Promised, And Scheduled Windows

Requested window is what the customer or requester asked for.

Promised window is what the business committed to.

Scheduled execution is what an execution product assigned to resources.

These fields must remain distinct in APIs, UI, events, reports, and projections.

## 7. Product Scheduling Ownership

MaintainArr owns maintenance work order, inspection, PM, defect repair, and readiness-check scheduling.

RoutArr owns trip, route, transport demand, driver, tractor, trailer, and dispatch scheduling.

LoadArr owns dock appointment, receiving, staging, putaway, warehouse team, and door scheduling.

TrainArr owns training assignment, class, evaluation, trainer, trainee, classroom, and retraining scheduling.

AssurArr owns quality check, audit, corrective action, inspector, and quality review scheduling.

SupplyArr owns vendor confirmation, procurement follow-up, supplier visit, and material sourcing work only when SupplyArr owns the work.

OrdArr owns order requested/promised windows and fulfillment orchestration. It does not normally own execution schedules.

CustomArr owns custom workflow scheduling only when no first-class product owns that process.

StaffArr owns people, teams, shifts, availability, org structure, sites, and internal locations. It does not own product work schedules.

Compliance Core checks rules and facts. It does not schedule work.

RecordArr owns documents and evidence. It schedules only RecordArr-owned record package or review workflows when those exist.

Field Companion is an execution surface. It sends commands to owning products.

ReportArr reports from events and projections. It does not mutate schedules.

## 8. Required Product Scheduling API Semantics

Schedulable products should expose product-local scheduling endpoints with these semantics:

- list unscheduled demand
- list scheduled work
- list scheduling resources
- validate a proposed scheduling action
- schedule
- reschedule
- unschedule
- cancel
- complete or route to the product-equivalent completion action

The recommended path family is `/api/v1/scheduling/*`, but products may use existing route conventions when the semantic contract is preserved.

## 9. Validation

Scheduling validation must be product-owned and must distinguish:

- allowed
- warning
- blocked
- needs_review
- missing_facts
- missing_permissions
- resource_conflict
- qualification_conflict
- compliance_conflict
- asset_readiness_conflict
- location_conflict
- order_status_conflict

Validation must check owning-product status and permissions first. It may consult StaffArr, TrainArr, Compliance Core, MaintainArr, RoutArr, LoadArr, AssurArr, RecordArr, OrdArr, or SupplyArr as needed, but the final write remains with the owning product.

## 10. Overrides

Overrides must be explicit, permission-gated, reasoned, audited, and product-owned.

Overrides must store:

- actor
- permission or authority basis
- reason
- timestamp
- affected rule or conflict references

Overrides must not delete blocker history.

## 11. Shared Scheduling Board

The shared board may:

- render unscheduled demand
- render scheduled work
- render resource lanes
- render calendar, timeline, and board views
- support drag/drop, resize, unschedule, reschedule, assignment, and conflict display
- call owning product APIs
- show source, owner, freshness, blockers, warnings, and allowed actions

The shared board must not:

- directly write product tables
- bypass product permissions
- assume all schedulable items share the same fields
- treat requested windows as scheduled execution
- treat portal submissions as accepted orders
- treat order acceptance as dispatch scheduling
- show raw JSON in ordinary UI

## 12. Minimum Acceptable Implementation

A suite scheduling feature is minimally acceptable when it has:

1. Clear owning product.
2. Product-local write API.
3. Tenant and entitlement validation.
4. Product-local permission validation.
5. Shared DTOs for display only.
6. Requested, promised, and scheduled windows separated.
7. Validation result semantics.
8. Idempotency for state-changing retries.
9. Audit and event publication for material writes.
10. No central scheduler source of truth.
