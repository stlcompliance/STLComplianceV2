# MaintainArr — Workflows, Status Logic, Events, and APIs

## Major workflow: manual work order

```text
1. User creates WorkOrder.
2. User selects asset and location.
3. User enters title, description, priority, and severity.
4. MaintainArr sets status to requested.
5. Supervisor triages.
6. Supervisor approves or rejects.
7. Planner adds tasks, parts demand, labor plan, schedule, and safety requirements.
8. WorkOrder moves to planned/scheduled/assigned.
9. Technician starts work.
10. Technician completes tasks, records labor, requests/uses parts, and attaches evidence.
11. WorkOrder moves to completed_pending_review.
12. Supervisor reviews.
13. Compliance/quality review occurs if required.
14. Asset returns to service if allowed.
15. WorkOrder closes.
```

## Major workflow: inspection failure to work order

```text
1. Inspection item fails.
2. MaintainArr creates Defect.
3. MaintainArr evaluates severity and out-of-service rules.
4. MaintainArr creates WorkOrder from Defect.
5. WorkOrder inherits inspection, defect, asset, component, location, compliance, and evidence context.
6. Required part demand is created if known.
7. WorkOrder proceeds through normal workflow.
8. Defect closes after repair verification.
```

## Major workflow: PM auto-generation

```text
1. PM scheduler evaluates active PM plans.
2. Due/overdue PMOccurrence is created.
3. MaintainArr generates WorkOrder or Inspection.
4. Generated object references PMOccurrence.
5. Completion of WorkOrder/Inspection satisfies occurrence.
6. Next occurrence is calculated.
```

## Major workflow: route breakdown

```text
1. RoutArr sends route exception to MaintainArr.
2. MaintainArr creates Defect and WorkOrder.
3. Asset readiness changes to down or unsafe.
4. RoutArr handles trip impact.
5. MaintainArr handles repair.
6. StaffArr receives incident if person/safety issue exists.
7. TrainArr receives remediation request if qualification issue exists.
```

## Major workflow: quality hold blocks asset

```text
1. AssurArr places hold on asset or maintenance-related object.
2. MaintainArr creates WorkOrderBlocker.
3. Asset cannot return to service while hold is active.
4. AssurArr releases hold.
5. MaintainArr resolves blocker.
6. Return-to-service may proceed.
```

## Major workflow: parts unavailable

```text
1. WorkOrder has required PartDemand.
2. LoadArr reports unavailable/backordered.
3. WorkOrder enters waiting_parts.
4. SupplyArr creates purchase request if needed.
5. LoadArr receives and reserves/levels stock.
6. LoadArr issues part.
7. MaintainArr resumes WorkOrder.
```

## Asset readiness calculation inputs

```text
AssetReadinessInputs
- asset.status
- open safety-critical defects
- open critical defects
- overdue compliance inspections
- failed inspections
- active work orders
- active downtime
- active quality holds
- active compliance blockers
- expired required documents
- required PM overdue status
- manual supervisor override
```

## Suggested readiness logic

```text
unsafe
- Any active safety-critical defect
- Any active out-of-service safety inspection failure
- Any active safety/compliance hold that blocks use

down
- Active critical defect
- Active downtime
- Active work order requiring out-of-service
- Required repair incomplete

limited
- Deferred defect with restrictions
- Non-critical active defect
- Warning-level compliance issue
- Supervisor-limited operation

ready
- No active blockers
- Required inspections/PM acceptable
- Asset status active

unknown
- Missing required readiness data
```

## MaintainArr emitted events

```text
maintainarr.asset.created
maintainarr.asset.updated
maintainarr.asset.status_changed
maintainarr.asset.readiness_changed
maintainarr.asset.location_changed
maintainarr.asset.retired

maintainarr.component.created
maintainarr.component.installed
maintainarr.component.removed
maintainarr.component.failed
maintainarr.component.replaced

maintainarr.meter_reading.recorded
maintainarr.meter_reading.rejected

maintainarr.defect.created
maintainarr.defect.triaged
maintainarr.defect.deferred
maintainarr.defect.work_order_created
maintainarr.defect.repaired
maintainarr.defect.verified
maintainarr.defect.closed

maintainarr.inspection_template.created
maintainarr.inspection_template.activated
maintainarr.inspection.scheduled
maintainarr.inspection.started
maintainarr.inspection.paused
maintainarr.inspection.resumed
maintainarr.inspection.completed
maintainarr.inspection.failed

maintainarr.pm_plan.created
maintainarr.pm_plan.activated
maintainarr.pm_occurrence.created
maintainarr.pm_occurrence.due
maintainarr.pm_occurrence.overdue
maintainarr.pm_occurrence.work_order_generated
maintainarr.pm_occurrence.inspection_generated
maintainarr.pm_occurrence.completed

maintainarr.work_order.created
maintainarr.work_order.requested
maintainarr.work_order.triaged
maintainarr.work_order.approved
maintainarr.work_order.rejected
maintainarr.work_order.planned
maintainarr.work_order.scheduled
maintainarr.work_order.assigned
maintainarr.work_order.started
maintainarr.work_order.paused
maintainarr.work_order.blocked
maintainarr.work_order.unblocked
maintainarr.work_order.completed
maintainarr.work_order.closed
maintainarr.work_order.canceled

maintainarr.part_demand.created
maintainarr.part_demand.status_changed
maintainarr.part_usage.recorded

maintainarr.labor_entry.created
maintainarr.labor_entry.submitted
maintainarr.labor_entry.approved
maintainarr.labor_entry.rejected

maintainarr.asset.downtime_started
maintainarr.asset.downtime_ended

maintainarr.vendor_work.created
maintainarr.vendor_work.completed
```

## Integration APIs MaintainArr should expose

```text
GET /api/v1/integrations/assets
GET /api/v1/integrations/assets/{assetId}
GET /api/v1/integrations/assets/{assetId}/readiness
POST /api/v1/integrations/asset-readiness-checks

GET /api/v1/integrations/work-orders/{workOrderId}
POST /api/v1/integrations/work-orders
POST /api/v1/integrations/work-orders/{workOrderId}/status-updates
POST /api/v1/integrations/work-orders/{workOrderId}/blockers
POST /api/v1/integrations/work-orders/{workOrderId}/closeout

POST /api/v1/integrations/defects
GET /api/v1/integrations/defects/{defectId}
POST /api/v1/integrations/defects/{defectId}/status-updates

POST /api/v1/integrations/inspections
GET /api/v1/integrations/inspections/{inspectionId}
POST /api/v1/integrations/inspections/{inspectionId}/answers

POST /api/v1/integrations/route-exceptions
POST /api/v1/integrations/quality-holds
POST /api/v1/integrations/quality-hold-releases

POST /api/v1/integrations/part-demand-status-updates
POST /api/v1/integrations/part-issue-events
POST /api/v1/integrations/supplier-work-status
```

## APIs MaintainArr should consume

```text
NexArr
- POST /api/v1/platform/handoff/redeem
- POST /api/v1/platform/service-tokens/introspect
- GET /api/v1/platform/tenants/{tenantId}/entitlements/{productKey}

StaffArr
- GET /persons/{personId}
- GET /persons/{personId}/permissions
- GET /locations
- GET /locations/{locationId}
- POST /incidents

TrainArr
- POST /qualification-checks
- GET /persons/{personId}/qualifications
- POST /remediation-requests

Compliance Core
- GET /catalogs/governing-bodies
- GET /rulepacks
- POST /evaluations
- POST /evidence-mapping/suggest

RecordArr
- POST /records
- GET /records/{recordId}
- POST /upload-sessions
- POST /record-packages

LoadArr
- POST /availability-checks
- POST /reservations
- POST /work-order-demands
- GET /reservations/{reservationId}
- GET /picks/{pickId}
- POST /returns

SupplyArr
- GET /suppliers
- GET /sourcing-records
- POST /purchase-requests

AssurArr
- GET /holds
- POST /quality-events

ReportArr
- POST /events
```

## Permission examples

```text
maintainarr.assets.read
maintainarr.assets.create
maintainarr.assets.update
maintainarr.assets.retire
maintainarr.assets.override_readiness

maintainarr.components.read
maintainarr.components.manage

maintainarr.work_orders.read
maintainarr.work_orders.create
maintainarr.work_orders.triage
maintainarr.work_orders.approve
maintainarr.work_orders.plan
maintainarr.work_orders.assign
maintainarr.work_orders.execute
maintainarr.work_orders.review
maintainarr.work_orders.close
maintainarr.work_orders.cancel

maintainarr.defects.read
maintainarr.defects.create
maintainarr.defects.triage
maintainarr.defects.defer
maintainarr.defects.close

maintainarr.inspections.read
maintainarr.inspections.execute
maintainarr.inspections.review
maintainarr.inspection_templates.manage

maintainarr.pm.read
maintainarr.pm.manage
maintainarr.pm.skip

maintainarr.parts.request
maintainarr.parts.use
maintainarr.parts.substitute_approve

maintainarr.labor.record
maintainarr.labor.approve

maintainarr.vendor_work.manage
maintainarr.return_to_service.approve
```

## Default role examples

```text
Maintenance Viewer
- read assets, work orders, defects, inspections, PMs

Operator
- report defects
- submit operator requests
- complete assigned inspections

Technician
- execute assigned work orders
- record labor
- request parts
- record part usage
- complete assigned inspections

Lead Technician
- technician permissions
- triage defects
- assign work
- review work

Maintenance Planner
- plan work orders
- manage PM schedules
- create part demand
- schedule work

Maintenance Supervisor
- approve/reject work
- close work orders
- approve labor
- approve return to service
- defer defects

Maintenance Admin
- manage asset registry
- manage templates
- manage PM plans
- configure fieldsets/catalogs

Compliance Maintenance Reviewer
- review compliance work orders
- review inspection evidence
- approve compliance closeout
```
