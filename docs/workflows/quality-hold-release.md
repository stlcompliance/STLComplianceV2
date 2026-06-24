# Workflow Pack — Quality Hold and Release

## Purpose

This workflow defines how AssurArr quality holds block use, movement, fulfillment, dispatch, or closeout across products and how release decisions clear blockers.

## Trigger

```text
AssurArr hold created
```

Possible origins:

```text
- receiving discrepancy
- nonconformance
- customer complaint
- supplier issue
- failed inspection
- damaged inventory
- suspect part
- asset quality concern
- audit finding
```

## Participating products

```text
AssurArr
LoadArr
SupplyArr
MaintainArr
OrdArr
RoutArr
CustomArr
RecordArr
Compliance Core
Field Companion
ReportArr
StaffArr
```

## Source-of-truth table

| Business truth | Owner |
|---|---|
| quality hold/release, nonconformance, CAPA | AssurArr |
| inventory balance/movement | LoadArr |
| supplier/vendor context | SupplyArr |
| asset/work-order readiness | MaintainArr |
| order lifecycle/handoffs | OrdArr |
| route/trip execution | RoutArr |
| customer requirements/complaints context | CustomArr |
| stored evidence/package | RecordArr |
| regulatory/evidence meaning | Compliance Core |
| user/authority context | StaffArr |

## Main flow

1. Quality issue is identified.
2. AssurArr creates nonconformance and/or hold.
3. AssurArr identifies affected objects:
   - inventory
   - supplier
   - asset
   - order
   - customer
   - trip/route
   - document/evidence package
4. Affected products create or update product-owned blockers.
5. LoadArr prevents held inventory from being picked/issued/released where applicable.
6. OrdArr blocks affected order handoffs or closeout.
7. MaintainArr blocks asset return-to-service if asset/part quality is affected.
8. RoutArr blocks dispatch if trip/equipment/order is affected.
9. AssurArr manages investigation and CAPA.
10. RecordArr stores evidence.
11. AssurArr releases hold or rejects/contains affected object.
12. Affected products clear local blockers after verifying release event.
13. ReportArr projects hold aging, recurrence, supplier/customer/product impact.

## Required events

```text
assurarr.nonconformance.created
assurarr.hold.created
assurarr.hold.scope_updated
assurarr.capa.opened
assurarr.capa.closed
assurarr.hold.released
assurarr.hold.rejected
loadarr.inventory_hold.created
loadarr.inventory_hold.released
ordarr.order.blocked
maintainarr.asset.readiness_changed
routarr.dispatch.blocked
recordarr.package.completed
```

## Required handoffs

```text
loadarr -> assurarr: receiving quality review
supplyarr -> assurarr: supplier quality review
maintainarr -> assurarr: asset/part quality review
customarr -> assurarr: customer complaint quality review
assurarr -> maintainarr: corrective work request
assurarr -> supplyarr: supplier corrective action request
assurarr -> recordarr: quality release package
```

## Blockers

Common blockers:

```text
- inventory on quality hold
- supplier under quality review
- customer complaint unresolved
- CAPA overdue
- evidence missing
- release approval required
- affected object scope unknown
```

## Release decision

AssurArr owns release decision.

Release should include:

```text
- affected object refs
- release scope
- release reason
- reviewer/approver
- evidence refs
- residual warnings
- downstream products to notify
```

## Affected product behavior

Products must not ignore active AssurArr holds when the hold affects their record.

Examples:

```text
- LoadArr cannot pick held inventory.
- OrdArr cannot complete affected order unless allowed with warning/override.
- MaintainArr cannot return asset to service if held part blocks readiness.
- RoutArr cannot dispatch affected load/equipment if hold blocks movement.
```

## Field Companion behavior

Field Companion may support quality photo capture, nonconformance report, hold label/status display, CAPA task completion, and release evidence capture.

Field Companion cannot release a hold unless AssurArr exposes an explicit permissioned release action.

## Evidence package

RecordArr quality release package should include:

```text
- nonconformance
- hold record
- affected object list
- investigation evidence
- CAPA
- test/inspection evidence
- release approval
- downstream unblock events
- overrides
```

## Non-goals

AssurArr does not own inventory balances, order lifecycle, dispatch execution, asset readiness, or supplier commercial truth.

AssurArr owns quality decisions that may block those product workflows.
