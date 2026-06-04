# MaintainArr — Parts, Labor, Downtime, and Vendor Maintenance Model

## Parts boundary

MaintainArr owns parts demand and parts usage on maintenance work. LoadArr owns inventory balances, reservations, picks, issues, returns, locations, and stock ledger. SupplyArr owns purchasing and supplier sourcing.

## Part demand

A PartDemand is a maintenance need for a part/material/tool/consumable. It does not mean the item exists in inventory.

```text
PartDemand
- partDemandId
- tenantId
- workOrderId
- assetId
- componentId
- demandSource
  - technician
  - planner
  - pm_template
  - inspection_failure
  - defect
  - kit
  - system_suggestion
- requestedItemRef
- itemDescriptionSnapshot
- requestedQuantity
- unitOfMeasure
- demandStatus
  - draft
  - requested
  - availability_check_pending
  - available
  - reserved
  - partially_reserved
  - backordered
  - unavailable
  - substitution_requested
  - substitute_approved
  - ready_for_pickup
  - issued
  - partially_issued
  - canceled
- priority
  - normal
  - high
  - urgent
  - emergency
- neededBy
- requestedByPersonId
- approvedByPersonId
- loadarrAvailabilityCheckRef
- loadarrReservationRef
- loadarrPickRef
- loadarrIssueRef
- supplyarrPurchaseRequestRef
- substituteAllowed
- approvedSubstituteRefs
- notes
- createdAt
- updatedAt
```

## Part demand status definitions

```text
requested
- MaintainArr has requested the part.

availability_check_pending
- MaintainArr is waiting for LoadArr availability response.

available
- LoadArr reports stock is available.

reserved
- LoadArr has reserved full required quantity.

partially_reserved
- LoadArr reserved some but not all required quantity.

backordered
- LoadArr/SupplyArr indicates replenishment is needed.

unavailable
- No stock or sourcing path is currently available.

substitution_requested
- Alternative item requested.

substitute_approved
- Alternative item approved for use.

ready_for_pickup
- LoadArr has picked/staged the item.

issued
- LoadArr issued full quantity to the work order.

partially_issued
- LoadArr issued partial quantity.

canceled
- Demand no longer needed.
```

## Part usage

PartUsage records what was actually installed, consumed, removed, or returned during maintenance.

```text
PartUsage
- partUsageId
- tenantId
- workOrderId
- assetId
- componentId
- partDemandId
- loadarrIssueRef
- itemRef
- itemDescriptionSnapshot
- quantityUsed
- unitOfMeasure
- usageType
  - installed
  - consumed
  - removed
  - replaced
  - returned_unused
  - scrapped
- installedAt
- installedByPersonId
- removedComponentRef
- newComponentRef
- oldSerialNumber
- newSerialNumber
- warrantyFlag
- warrantyRef
- evidenceRecordRefs
- notes
```

## Parts kit

```text
MaintenancePartsKit
- kitId
- tenantId
- kitNumber
- title
- description
- assetTypeApplicability
- workOrderTypeApplicability
- pmPlanRef
- lineRefs
- status
  - draft
  - active
  - retired
```

## Parts kit line

```text
MaintenancePartsKitLine
- kitLineId
- kitId
- itemRef
- itemDescriptionSnapshot
- quantity
- unitOfMeasure
- required
- substituteAllowed
```

## Labor entry

A LaborEntry records time and activity performed against a work order.

```text
LaborEntry
- laborEntryId
- tenantId
- workOrderId
- personId
- laborType
  - diagnostic
  - repair
  - inspection
  - testing
  - calibration
  - cleanup
  - admin
  - travel
  - vendor_coordination
  - waiting
- status
  - draft
  - submitted
  - approved
  - rejected
  - corrected
- startedAt
- endedAt
- durationMinutes
- regularMinutes
- overtimeMinutes
- billableFlag
- notes
- submittedAt
- approvedByPersonId
- approvedAt
- rejectionReason
```

## Technician assignment

```text
TechnicianAssignment
- assignmentId
- workOrderId
- personId
- assignmentRole
  - primary
  - helper
  - supervisor
  - inspector
  - specialist
  - vendor_contact
- status
  - assigned
  - accepted
  - declined
  - in_progress
  - completed
  - removed
- assignedAt
- assignedByPersonId
- acceptedAt
- completedAt
- requiredQualificationRefs
- qualificationCheckSnapshot
```

## Downtime

Downtime tracks time an asset is unavailable or restricted because of maintenance.

```text
AssetDowntime
- downtimeId
- tenantId
- assetId
- workOrderId
- defectId
- downtimeType
  - planned
  - unplanned
  - safety
  - waiting_parts
  - waiting_labor
  - waiting_vendor
  - compliance_hold
  - quality_hold
  - inspection_failure
- status
  - active
  - ended
  - adjusted
  - voided
- startedAt
- startedByPersonId
- endedAt
- endedByPersonId
- durationMinutes
- productionImpact
  - none
  - low
  - moderate
  - high
  - critical
- customerImpact
  - none
  - possible
  - confirmed
- reason
- notes
```

## Vendor maintenance work

MaintainArr may coordinate vendor work, but SupplyArr owns supplier/vendor master.

```text
MaintenanceVendorWork
- vendorWorkId
- tenantId
- workOrderId
- supplierRef
- vendorContactSnapshot
- status
  - requested
  - quoted
  - approved
  - scheduled
  - in_progress
  - completed
  - rejected
  - canceled
- workDescription
- quoteRecordRef
- approvalRef
- scheduledAt
- completedAt
- costEstimateSnapshot
- invoiceRecordRef
- warrantyFlag
- notes
```

## Maintenance permit reference

MaintainArr does not need to own every permit system, but work orders should reference permits when required.

```text
MaintenancePermitRef
- permitRefId
- workOrderId
- permitType
  - lockout_tagout
  - hot_work
  - confined_space
  - electrical
  - line_break
  - excavation
  - working_at_height
  - other
- sourceProduct
- sourceObjectRef
- recordRef
- statusSnapshot
- approvedByPersonId
- validFrom
- validTo
```

## Return to service

```text
ReturnToService
- returnToServiceId
- workOrderId
- assetId
- status
  - pending
  - approved
  - rejected
  - not_required
- requiredChecks
- completedChecks
- finalInspectionRef
- approvedByPersonId
- approvedAt
- rejectionReason
- finalReadinessStatus
- recordRefs
```

## Parts workflow

```text
1. Technician/planner adds PartDemand to WorkOrder.
2. MaintainArr sends demand to LoadArr.
3. LoadArr checks stock and reservation possibility.
4. LoadArr returns availability.
5. WorkOrder becomes waiting_parts if required part is unavailable.
6. LoadArr reserves/picks/issues item.
7. MaintainArr receives issue event.
8. Technician installs/uses part.
9. MaintainArr records PartUsage.
10. Asset/component history updates.
```

## Labor workflow

```text
1. Technician starts work order or task.
2. MaintainArr opens LaborEntry.
3. Technician pauses/stops/completes labor.
4. LaborEntry is submitted.
5. Supervisor approves or rejects.
6. Labor appears in work order cost/time summary.
```

## Downtime workflow

```text
1. Defect or work order requires downtime.
2. MaintainArr starts AssetDowntime.
3. Asset readiness changes to down/unsafe/limited.
4. Work proceeds.
5. Return-to-service check is completed.
6. MaintainArr ends downtime.
7. Asset readiness updates.
```

## Vendor workflow

```text
1. WorkOrder requires vendor support.
2. MaintainArr creates MaintenanceVendorWork.
3. SupplyArr supplier reference is selected.
4. Quote/document is stored in RecordArr.
5. Vendor work is scheduled.
6. Vendor completes work.
7. MaintainArr records completion and evidence.
8. WorkOrder proceeds to review/closeout.
```
