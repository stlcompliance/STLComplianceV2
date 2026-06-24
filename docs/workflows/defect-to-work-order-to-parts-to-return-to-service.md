# Workflow Pack — Defect to Work Order to Parts to Return-to-Service

## Purpose

This workflow defines how a defect becomes maintenance work, parts demand, inventory/procurement activity, evidence capture, and return-to-service decision.

## Trigger

```text
MaintainArr defect created
```

Possible origins:

```text
- inspection answer
- operator report
- Field Companion report
- route/trip exception
- PM finding
- quality finding
- manual supervisor entry
```

## Participating products

```text
MaintainArr
LoadArr
SupplyArr
StaffArr
TrainArr
RecordArr
Compliance Core
AssurArr
Field Companion
ReportArr
RoutArr where defect affects route/trip
Platform Reference Data service where asset/part identity needs lookup
```

## Source-of-truth table

| Business truth | Owner |
|---|---|
| asset, defect, work order, readiness | MaintainArr |
| parts inventory, reservation, issue, return | LoadArr |
| parts sourcing, supplier/vendor, PO | SupplyArr |
| technician person/permission/location context | StaffArr |
| qualification/certification status | TrainArr |
| evidence files/package | RecordArr |
| regulatory/evidence requirements | Compliance Core |
| quality hold/release | AssurArr |
| trip/route impact | RoutArr |

## Main flow

1. Defect is created in MaintainArr.
2. MaintainArr classifies severity and asset readiness impact.
3. MaintainArr stores photos/evidence in RecordArr if attached.
4. MaintainArr asks Compliance Core if evidence, inspection, or rule context is required.
5. MaintainArr checks StaffArr/TrainArr context for qualified assignee where needed.
6. MaintainArr creates or links work order.
7. Work order creates parts demand.
8. LoadArr checks inventory availability.
9. If available, LoadArr reserves/issues parts.
10. If unavailable, SupplyArr creates purchase request/order context.
11. Work is assigned to technician/team.
12. Field Companion may show work task and capture labor/photos/signature.
13. Work order tasks are completed.
14. MaintainArr evaluates return-to-service.
15. Asset readiness changes.
16. RecordArr assembles work-order/return-to-service evidence package.
17. ReportArr projects downtime, defect rate, PM effectiveness, and parts delay.

## Required events

```text
maintainarr.defect.created
maintainarr.asset.readiness_changed
maintainarr.work_order.created
maintainarr.parts_demand.created
loadarr.reservation.created
loadarr.issue.completed
supplyarr.purchase_request.created
supplyarr.purchase_order.issued
maintainarr.work_order.closed
maintainarr.asset.returned_to_service
recordarr.package.completed
```

## Required handoffs

```text
maintainarr -> loadarr: reserve/issue parts
loadarr -> supplyarr: procure unavailable parts
maintainarr -> trainarr: qualification check
maintainarr -> recordarr: evidence package
maintainarr -> compliancecore: evidence/requirement evaluation
maintainarr -> assurarr: quality review where defect is quality-related
maintainarr -> routarr: route/trip impact where asset was assigned
```

## Blockers

Common blockers:

```text
- defect severity makes asset unsafe
- required inspection evidence missing
- technician not qualified
- required part unavailable
- part is on quality hold
- work order task incomplete
- return-to-service approval required
- compliance fact unknown/conflicted
```

## Return-to-service decision

MaintainArr owns return-to-service.

Return-to-service should consider:

```text
- defect status
- work order completion
- inspection/pass criteria
- required parts installed
- technician qualification
- evidence requirements
- quality holds
- unresolved blockers
- override policy
```

## Field Companion behavior

Field Companion may support defect report capture, photo/video upload, guided inspection, work-order task completion, labor entry, part scan/use submission, signature capture, and offline queue sync.

## Evidence package

RecordArr should include:

```text
- defect record
- inspection result
- photos/videos
- work order
- parts issue/install refs
- labor summary
- vendor repair docs
- return-to-service approval
- blockers/overrides
```

## RoutArr impact

If the asset is assigned to an active or planned trip:

1. MaintainArr emits readiness change.
2. RoutArr evaluates trip/equipment readiness.
3. RoutArr blocks dispatch or creates route exception as needed.
4. OrdArr may receive customer/order impact if tied to order fulfillment.

## Non-goals

MaintainArr does not own inventory balance.

LoadArr does not decide asset readiness.

SupplyArr does not decide return-to-service.
