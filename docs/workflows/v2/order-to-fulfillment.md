# Workflow Pack — Order to Fulfillment

## Purpose

This workflow defines how a customer or internal request becomes executable work across OrdArr, CustomArr, LoadArr, RoutArr, SupplyArr, MaintainArr, AssurArr, RecordArr, Compliance Core, Field Companion, and ReportArr.

## Trigger

```text
OrdArr order/request created
```

Possible sources:

```text
- internal user
- CustomArr customer context
- public site lead converted to request
- external portal submission
- integration import
```

## Participating products

```text
OrdArr
CustomArr
LoadArr
RoutArr
SupplyArr
MaintainArr
AssurArr
RecordArr
Compliance Core
ReferenceDataCore
Field Companion
ReportArr
NexArr
StaffArr
```

## Source-of-truth table

| Business truth | Owner |
|---|---|
| customer identity, contacts, locations, requirements | CustomArr |
| order/request lifecycle and handoffs | OrdArr |
| inventory balance, reservations, picks, issues | LoadArr |
| dispatch, routes, trips, stops, proof | RoutArr |
| supplier/vendor/procurement context | SupplyArr |
| maintenance work and asset readiness | MaintainArr |
| quality holds/releases and CAPA | AssurArr |
| evidence files and completion package | RecordArr |
| regulatory/evidence meaning | Compliance Core |
| public/reference identity and UOM/package lookup | ReferenceDataCore |
| people, permissions, internal locations | StaffArr |
| product entitlement and launch | NexArr |
| reports/read models | ReportArr |
| mobile capture/execution surface | Field Companion |

## Main flow

1. OrdArr creates order/request.
2. OrdArr links customer context from CustomArr.
3. OrdArr runs initial customer eligibility and requirement check.
4. OrdArr asks Compliance Core for missing facts or workflow questionnaire if needed.
5. OrdArr determines execution needs:
   - LoadArr fulfillment
   - RoutArr dispatch
   - MaintainArr service/maintenance work
   - SupplyArr procurement
   - AssurArr quality review
6. OrdArr creates handoffs to execution products.
7. Execution products accept, reject, or block handoffs.
8. LoadArr reserves/picks/issues inventory where applicable.
9. RoutArr creates route/trip where transportation is needed.
10. MaintainArr creates work order where maintenance/service execution is needed.
11. SupplyArr creates purchase/procurement context if needed.
12. AssurArr manages holds/nonconformance where quality issues exist.
13. RecordArr stores evidence and builds completion package.
14. OrdArr tracks all handoffs until complete or canceled.
15. OrdArr creates completion status and finance-ready packet reference.
16. ReportArr projects order cycle time, blockers, and performance.

## Required handoffs

```text
ordarr -> customarr: customer eligibility check
ordarr -> loadarr: fulfillment request
ordarr -> routarr: dispatch/route request
ordarr -> maintainarr: service/work request
ordarr -> supplyarr: procurement request
ordarr -> assurarr: quality review request
ordarr -> recordarr: completion package request
ordarr -> compliancecore: requirement/evidence evaluation
```

## Required events

```text
ordarr.order.created
ordarr.order.triaged
ordarr.handoff.requested
ordarr.handoff.accepted
ordarr.handoff.blocked
ordarr.handoff.completed
ordarr.order.blocked
ordarr.order.completed
loadarr.reservation.created
loadarr.pick.completed
routarr.trip.dispatched
routarr.proof.captured
maintainarr.work_order.closed
assurarr.hold.created
assurarr.hold.released
recordarr.package.completed
```

## Blockers

Common blockers:

```text
- customer not eligible
- customer requirement missing
- inventory unavailable
- package/UOM conversion unknown
- supplier blocked
- quality hold open
- asset not ready
- driver not qualified
- missing evidence
- external dependency incomplete
- compliance fact unknown/conflicted
```

The owner of each blocker is the product that owns the underlying truth.

## Field Companion behavior

Field Companion may show assigned tasks, pick/issue tasks, trip/proof tasks, work-order tasks, evidence capture tasks, and blocked-state explanations.

Field Companion submits updates to owning product APIs.

## External portal behavior

External actors may view limited order status, upload evidence, confirm completion, or answer scoped questions.

External portal submissions go to the owning product review/update flow.

## Evidence

RecordArr package should include:

```text
- order snapshot
- customer requirement checks
- handoff statuses
- execution records
- proof files/signatures/photos
- quality hold/release records
- evidence gaps
- overrides
- completion packet
```

## Closeout

OrdArr may close the order when:

```text
- required handoffs are complete or canceled with approved reason
- blocking issues are cleared or overridden
- required evidence package is complete or accepted with warnings
- customer-facing status is ready
- finance-ready packet is generated when needed
```

## Non-goals

OrdArr does not own execution records.

External finance systems own invoices, bills, payments, tax, and general ledger execution.
