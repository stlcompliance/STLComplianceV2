# MaintainArr — Scope, Ownership, and Boundaries

## Product purpose

MaintainArr is the maintenance execution system. It owns assets, components, preventive maintenance, inspections, defects, work orders, repair execution, maintenance readiness, downtime, labor context, parts demand, and maintenance closeout.

MaintainArr answers:

- What assets exist?
- What condition are they in?
- Are they ready, limited, down, unsafe, or retired?
- What inspections are required?
- What preventive maintenance is due?
- What defects exist?
- What work is needed?
- Who is assigned to the work?
- What parts are needed for the work?
- Was the work completed correctly?
- Can the asset return to service?
- What maintenance evidence exists?

## MaintainArr owns

```text
- Asset registry
- Asset components
- Asset hierarchy
- Asset readiness
- Asset operating status
- Asset compliance status snapshot
- Meter readings used for maintenance
- Preventive maintenance plans
- PM occurrences
- Inspection templates
- Inspection executions
- Inspection answers
- Defects
- Defect severity
- Work orders
- Work order tasks
- Work order labor entries
- Work order parts demand
- Work order part usage/installation
- Maintenance downtime
- Maintenance vendor work coordination
- Maintenance closeout
- Maintenance audit trail
- Maintenance-origin events
```

## MaintainArr does not own

```text
- Platform login
- Tenant entitlement
- Person master
- Permission assignment truth
- Internal location identity
- Training/certification truth
- Regulatory/rulepack meaning
- Document/file storage truth
- Inventory balance
- Stock ledger
- Receiving
- Putaway
- Pick/issue movement truth
- Supplier/vendor master
- Purchase requests
- Purchase orders
- Route/trip execution
- Customer master
- Customer order lifecycle
- Quality hold/release decision
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
- Technician/supervisor references
- Site/location references
- Product permission assignments
- Personnel incidents

TrainArr
- Qualification checks
- Required training/certification status
- Remediation assignment after incidents

Compliance Core
- Governing body catalogs
- Rulepacks
- Inspection/maintenance regulatory requirements
- Evidence requirement definitions
- Compliance evaluations

RecordArr
- Photos
- PDFs
- Manuals
- Inspection records
- Work order evidence
- Return-to-service evidence
- Vendor invoices/documents where stored as records

LoadArr
- Inventory availability
- Reservation
- Pick
- Issue
- Return
- Stock ledger
- Parts location behavior

SupplyArr
- Supplier/vendor sourcing
- Purchase requests
- Purchase orders
- Supplier status

RoutArr
- Route exceptions
- Asset breakdown events during trip execution
- Transportation impact

AssurArr
- Quality holds
- Nonconformance
- CAPA
- Asset/part quality release

ReportArr
- Maintenance dashboards
- KPIs
- Cross-product reporting

Field Companion
- Mobile execution of inspections, work orders, photos, signatures, meter readings, and task updates
```

## Core source-of-truth rules

```text
1. MaintainArr owns asset readiness.
2. MaintainArr owns work-order lifecycle.
3. MaintainArr owns maintenance demand for parts.
4. LoadArr owns whether parts physically exist and where they move.
5. SupplyArr owns how unavailable parts are purchased.
6. StaffArr owns internal location identity.
7. MaintainArr references StaffArr locations but does not create canonical locations.
8. StaffArr owns person identity and permissions.
9. TrainArr owns whether a person is qualified.
10. MaintainArr can block work if qualifications are missing.
11. Compliance Core owns regulatory meaning.
12. RecordArr owns the actual document/file/evidence object.
13. AssurArr owns quality hold/release decisions.
14. ReportArr owns reporting views, not maintenance truth.
```

## Standard MaintainArr object envelope

Every major MaintainArr object should include:

```text
MaintainArrObject
- id
- tenantId
- objectNumber
- objectType
- status
- title
- description
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
- sourceProduct
- sourceObjectRef
- staffarrSiteId
- staffarrLocationId
- assetRef
- recordRefs
- complianceRefs
- auditTrail
- eventLog
```

## Standard structured reference

```text
SuiteRef
- productKey
- objectType
- objectId
- humanReadableNumber
- displayNameSnapshot
- statusSnapshot
- versionSnapshot
- lastResolvedAt
```

## MaintainArr object prefixes

```text
AST    Asset
CMP    Asset component
MTR    Meter reading
DEF    Defect
WO     Work order
WOT    Work order task
LAB    Labor entry
PDEM   Part demand
PUSE   Part usage
INSP   Inspection
ITPL   Inspection template
PM     Preventive maintenance plan
PMO    PM occurrence
DT     Downtime
MWV    Maintenance vendor work
```


---


# MaintainArr — Asset and Component Model

## Asset

An asset is a maintainable thing. Examples include a truck, trailer, forklift, conveyor, dock leveler, compressor, production machine, facility system, safety device, tool, or service vehicle.

## Asset shape

```text
Asset
- assetId
- tenantId
- assetNumber
- name
- description
- assetType
  - vehicle
  - trailer
  - forklift
  - machine
  - tool
  - facility_system
  - building_equipment
  - dock_equipment
  - conveyor
  - production_equipment
  - safety_equipment
  - service_vehicle
  - other
- assetClass
  - power_unit
  - trailer
  - material_handling
  - production
  - facility
  - safety
  - tooling
  - fleet_support
  - other
- category
- status
  - draft
  - active
  - inactive
  - out_of_service
  - retired
  - sold
  - archived
- readinessStatus
  - ready
  - limited
  - down
  - unsafe
  - unknown
- complianceStatus
  - compliant
  - warning
  - noncompliant
  - not_applicable
  - unknown
- ownershipStatus
  - owned
  - leased
  - rented
  - customer_owned
  - vendor_owned
- criticality
  - low
  - normal
  - high
  - critical
- make
- model
- modelYear
- serialNumber
- vin
- plateNumber
- unitNumber
- fleetNumber
- barcode
- qrCode
- externalSystemRefs
- staffarrSiteId
- staffarrLocationId
- departmentOrgUnitId
- custodianPersonId
- assignedOperatorPersonId
- meterProfileRef
- currentMeterReadings
- componentRefs
- documentRefs
- activeDefectRefs
- activeWorkOrderRefs
- pmPlanRefs
- inspectionRequirementRefs
- regulatoryApplicabilityRefs
- warrantyRefs
- serviceStatusSummary
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
- activatedAt
- retiredAt
- retiredByPersonId
- retiredReason
- auditTrail
```

## Asset status definitions

```text
draft
- Asset record is being created and is not yet operational.

active
- Asset exists and may be used if readiness allows.

inactive
- Asset exists but is intentionally not in regular use.

out_of_service
- Asset is blocked from use due to defect, inspection failure, compliance issue, safety issue, or maintenance decision.

retired
- Asset is no longer maintained as an active operational object.

sold
- Asset left the organization through sale/disposal.

archived
- Asset record is retained for history only.
```

## Readiness status definitions

```text
ready
- Asset can be used normally.

limited
- Asset can be used with restrictions.

down
- Asset cannot be used because maintenance is required.

unsafe
- Asset must not be used because of safety-critical condition.

unknown
- Asset readiness has not been determined.
```

## Asset detail page sections

```text
AssetDetail
- Header
  - assetNumber
  - name
  - readinessStatus
  - status
  - location
  - primary meter
  - open defects
  - open work orders
- Identity
  - type
  - class
  - make
  - model
  - serial/VIN/unit
- Location and responsibility
  - site
  - location
  - department
  - custodian
  - assigned operator
- Readiness
  - active blockers
  - out-of-service reason
  - restrictions
  - return-to-service requirements
- Meters
  - current readings
  - reading history
- Components
  - installed components
  - component status
- Maintenance
  - PM plans
  - PM due/overdue
  - work order history
- Inspections
  - required inspections
  - completed inspections
  - failed inspection items
- Defects
  - open defects
  - deferred defects
  - closed defects
- Parts
  - installed parts/components
  - parts usage history
- Documents
  - manuals
  - photos
  - registrations
  - inspection evidence
  - repair evidence
- Compliance
  - applicable rulepacks
  - missing evidence
  - compliance status snapshot
- Timeline
  - audit events
  - status changes
  - maintenance events
```

## Asset creation workflow

```text
1. Select asset type.
2. Enter required identity fields.
3. Select StaffArr site/location.
4. Select ownership/status.
5. Enter make/model/serial/VIN/unit fields where applicable.
6. Configure meter profile.
7. Add initial meter reading.
8. Add components if needed.
9. Attach initial documents through RecordArr.
10. Select or auto-evaluate compliance applicability through Compliance Core.
11. Attach PM plans/inspection requirements.
12. Review and activate asset.
```

## Asset component

A component is a maintainable child object installed on an asset or another component.

```text
AssetComponent
- componentId
- tenantId
- componentNumber
- parentAssetId
- parentComponentId
- name
- description
- componentType
  - engine
  - transmission
  - axle
  - brake_system
  - tire
  - wheel
  - battery
  - hydraulic
  - electrical
  - motor
  - pump
  - sensor
  - safety_device
  - attachment
  - belt
  - filter
  - cylinder
  - control_module
  - other
- status
  - planned
  - installed
  - removed
  - failed
  - replaced
  - retired
- make
- model
- serialNumber
- partNumberSnapshot
- installedPartUsageRef
- installDate
- installedByPersonId
- installedMeterReading
- removedDate
- removedByPersonId
- removedMeterReading
- removalReason
- warrantyStartDate
- warrantyEndDate
- expectedLifeHours
- expectedLifeMiles
- expectedLifeCycles
- condition
  - good
  - fair
  - poor
  - failed
  - unknown
- replacementPartRefs
- documentRefs
- defectRefs
- workOrderRefs
- auditTrail
```

## Component events

```text
maintainarr.component.created
maintainarr.component.installed
maintainarr.component.removed
maintainarr.component.failed
maintainarr.component.replaced
maintainarr.component.retired
```

## Meter profile

```text
MeterProfile
- meterProfileId
- assetId
- primaryMeterType
  - odometer
  - engine_hours
  - machine_hours
  - cycles
  - starts
  - fuel_hours
  - custom
- enabledMeterTypes
- meterUnits
- rolloverRules
- requiredReadingFrequency
- sourcePriority
  - telematics
  - inspection
  - work_order
  - manual
  - import
- validationRules
```

## Meter reading

```text
MeterReading
- meterReadingId
- assetId
- meterType
  - odometer
  - engine_hours
  - machine_hours
  - cycles
  - starts
  - fuel_hours
  - custom
- value
- unit
- readingAt
- recordedByPersonId
- source
  - manual
  - inspection
  - telematics
  - work_order
  - import
- sourceObjectRef
- confidence
  - high
  - medium
  - low
- validationStatus
  - accepted
  - suspicious
  - rejected
- validationMessage
- notes
```

## Warranty

```text
Warranty
- warrantyId
- assetId
- componentId
- warrantyType
  - manufacturer
  - extended
  - vendor
  - internal
- providerRef
- startDate
- endDate
- startMeter
- endMeter
- coverageDescription
- claimInstructions
- documentRefs
- status
  - active
  - expired
  - void
  - unknown
```

---


# MaintainArr — Work Order Model

## Purpose

A Work Order is MaintainArr’s main maintenance execution object. It is not just a status and description. It is a full operational container for maintenance need, asset context, planning, assignment, parts demand, labor, safety/compliance requirements, execution tasks, evidence, downtime, and closeout.

## WorkOrder shape

```text
WorkOrder
- workOrderId
- tenantId
- workOrderNumber
- title
- description
- workOrderType
  - corrective
  - preventive
  - inspection_followup
  - defect_repair
  - emergency
  - project
  - compliance
  - recall
  - warranty
  - calibration
  - installation
  - removal
  - operator_request
  - vendor_work
- originType
  - manual
  - inspection_failure
  - defect
  - pm_due
  - asset_breakdown
  - operator_request
  - route_exception
  - quality_hold
  - compliance_requirement
  - customer_request
  - warranty_claim
  - recall_notice
- originRef
- status
  - draft
  - requested
  - triage
  - rejected
  - approved
  - planned
  - waiting_parts
  - waiting_labor
  - waiting_vendor
  - waiting_approval
  - waiting_compliance
  - scheduled
  - assigned
  - in_progress
  - paused
  - blocked
  - completed_pending_review
  - completed
  - closed
  - canceled
- priority
  - low
  - normal
  - high
  - urgent
  - emergency
- severity
  - cosmetic
  - minor
  - moderate
  - major
  - critical
  - safety_critical
- assetRef
- componentRefs
- defectRefs
- inspectionRefs
- pmPlanRef
- pmOccurrenceRef
- complianceRequirementRefs
- staffarrSiteId
- staffarrLocationId
- requestedByPersonId
- requestedAt
- triagedByPersonId
- triagedAt
- approvedByPersonId
- approvedAt
- plannedByPersonId
- plannedAt
- assignedSupervisorPersonId
- assignedTechnicianPersonIds
- requiredQualificationRefs
- qualificationCheckResults
- scheduledStartAt
- scheduledEndAt
- actualStartAt
- actualEndAt
- dueAt
- completedAt
- completedByPersonId
- reviewedAt
- reviewedByPersonId
- closedAt
- closedByPersonId
- canceledAt
- canceledByPersonId
- cancelReason
- downtimeRequired
- downtimeRef
- safetyImpact
- complianceImpact
- productionImpact
- customerImpact
- outOfServiceRequired
- lockoutTagoutRequired
- hotWorkRequired
- confinedSpaceRequired
- permitRequired
- permitRecordRefs
- taskRefs
- checklistRefs
- laborEntryRefs
- partDemandRefs
- partUsageRefs
- vendorWorkRefs
- approvalRefs
- blockerRefs
- documentRefs
- photoRefs
- closeout
- auditTrail
- eventLog
```

## WorkOrder type definitions

```text
corrective
- Repair work created in response to a failure, symptom, or defect.

preventive
- Scheduled maintenance generated by PM plan.

inspection_followup
- Work created from an inspection result.

defect_repair
- Work specifically tied to one or more defects.

emergency
- Immediate work required to address safety, critical downtime, compliance, or major operational risk.

project
- Larger planned maintenance effort with multiple tasks or phases.

compliance
- Work required primarily to satisfy a compliance requirement.

recall
- Work tied to manufacturer, regulatory, or internal recall.

warranty
- Work performed under warranty or to support a warranty claim.

calibration
- Work to calibrate an asset, sensor, meter, device, or tool.

installation
- Work to install an asset/component.

removal
- Work to remove an asset/component.

vendor_work
- Work coordinated by MaintainArr but performed by an external vendor.
```

## Status definitions

```text
draft
- Work order exists but has not been submitted.

requested
- Work has been requested but not triaged.

triage
- Supervisor/planner is evaluating priority, severity, safety, parts, labor, and asset impact.

rejected
- Request was rejected and will not proceed.

approved
- Work is approved to plan or execute.

planned
- Scope, tasks, expected parts, and labor plan exist.

waiting_parts
- Work cannot proceed until LoadArr fulfills required part demand.

waiting_labor
- Work cannot proceed until qualified labor is available.

waiting_vendor
- Work cannot proceed until vendor action is available.

waiting_approval
- Work cannot proceed until approval is granted.

waiting_compliance
- Work cannot proceed until compliance requirement/evidence is satisfied.

scheduled
- Work has scheduled start/end.

assigned
- Work has assigned technician/person/team.

in_progress
- Physical or administrative work has started.

paused
- Work started but was paused intentionally.

blocked
- Active blocker prevents progress.

completed_pending_review
- Technician marked complete, but supervisor/compliance/quality review remains.

completed
- Work is complete, but may not be administratively closed.

closed
- Work is complete, reviewed, evidence accepted, asset status updated, and no further action remains.

canceled
- Work will not be performed.
```

## WorkOrder status transition map

```text
draft -> requested
requested -> triage
requested -> canceled
triage -> rejected
triage -> approved
triage -> canceled
approved -> planned
approved -> scheduled
approved -> assigned
planned -> waiting_parts
planned -> waiting_labor
planned -> waiting_vendor
planned -> scheduled
scheduled -> assigned
assigned -> in_progress
in_progress -> paused
paused -> in_progress
in_progress -> blocked
blocked -> in_progress
in_progress -> completed_pending_review
completed_pending_review -> completed
completed -> closed
any_non_closed_status -> canceled when allowed
```

## WorkOrder task

A task is a discrete step within a work order.

```text
WorkOrderTask
- taskId
- workOrderId
- sequence
- title
- instructions
- taskType
  - diagnostic
  - repair
  - replace
  - inspect
  - test
  - clean
  - calibrate
  - lubricate
  - document
  - safety_step
  - permit_step
  - quality_check
  - return_to_service_check
- status
  - not_started
  - in_progress
  - completed
  - failed
  - skipped
  - blocked
- required
- assignedPersonId
- requiredQualificationRefs
- estimatedMinutes
- actualMinutes
- passFailRequired
- result
  - pass
  - fail
  - not_applicable
- measurements
- notes
- evidenceRecordRefs
- startedAt
- completedAt
- completedByPersonId
- skipReason
- failureReason
```

## WorkOrder checklist

```text
WorkOrderChecklist
- checklistId
- workOrderId
- title
- checklistType
  - repair_steps
  - safety
  - quality
  - return_to_service
  - compliance
- status
  - not_started
  - in_progress
  - completed
  - failed
- itemRefs
```

## WorkOrder checklist item

```text
WorkOrderChecklistItem
- checklistItemId
- checklistId
- sequence
- prompt
- helpText
- responseType
  - pass_fail
  - yes_no
  - numeric
  - text
  - select
  - multi_select
  - photo
  - signature
  - meter_reading
- required
- responseValue
- result
  - pass
  - fail
  - not_applicable
- requiresEvidenceOnFail
- evidenceRecordRefs
- completedAt
- completedByPersonId
```

## WorkOrder blocker

```text
WorkOrderBlocker
- blockerId
- workOrderId
- blockerType
  - parts
  - labor
  - qualification
  - safety
  - compliance
  - approval
  - vendor
  - quality_hold
  - document
  - asset_unavailable
  - location_unavailable
  - system
- sourceProduct
- sourceObjectRef
- title
- description
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
- requiredAction
- createdAt
- createdByPersonId
- resolvedAt
- resolvedByPersonId
- overrideReason
```

## WorkOrder approval

```text
WorkOrderApproval
- approvalId
- workOrderId
- approvalType
  - supervisor
  - safety
  - compliance
  - cost
  - downtime
  - vendor
  - defer_defect
  - return_to_service
- status
  - pending
  - approved
  - rejected
  - canceled
  - expired
- requestedAt
- requestedByPersonId
- approverPersonId
- decisionAt
- decisionReason
- evidenceRecordRefs
```

## WorkOrder closeout

```text
WorkOrderCloseout
- workOrderId
- completionSummary
- rootCause
  - wear
  - abuse
  - operator_error
  - maintenance_error
  - part_failure
  - design_issue
  - environmental
  - unknown
  - other
- correctiveAction
- preventiveActionRecommendation
- assetReturnedToService
- returnToServiceAt
- returnToServiceByPersonId
- postRepairInspectionRequired
- postRepairInspectionRef
- supervisorReviewRequired
- supervisorReviewedByPersonId
- supervisorReviewedAt
- complianceReviewRequired
- complianceReviewedByPersonId
- complianceReviewedAt
- qualityReviewRequired
- qualityReviewedByPersonId
- qualityReviewedAt
- evidenceAccepted
- unresolvedDefectRefs
- followUpWorkOrderRefs
- customerImpactSummary
- downtimeSummary
- finalAssetReadinessStatus
- finalStatus
```

## WorkOrder comments

```text
WorkOrderComment
- commentId
- workOrderId
- body
- visibility
  - internal
  - supervisor_only
  - auditor_visible
  - vendor_visible
- createdAt
- createdByPersonId
- editedAt
- editedByPersonId
- pinned
```

## WorkOrder timeline event

```text
WorkOrderTimelineEvent
- timelineEventId
- workOrderId
- eventType
- occurredAt
- actorPersonId
- actorServiceClientId
- summary
- sourceProduct
- sourceObjectRef
- beforeSnapshot
- afterSnapshot
```

## WorkOrder event examples

```text
maintainarr.work_order.created
maintainarr.work_order.requested
maintainarr.work_order.triaged
maintainarr.work_order.approved
maintainarr.work_order.planned
maintainarr.work_order.scheduled
maintainarr.work_order.assigned
maintainarr.work_order.started
maintainarr.work_order.paused
maintainarr.work_order.blocked
maintainarr.work_order.unblocked
maintainarr.work_order.completed_pending_review
maintainarr.work_order.completed
maintainarr.work_order.closed
maintainarr.work_order.canceled
```

## WorkOrder UI layout

```text
WorkOrderDetail
- Header
  - workOrderNumber
  - title
  - status
  - priority
  - severity
  - asset
  - due date
  - assigned techs
- Left/primary content
  - problem/request
  - asset context
  - tasks/checklists
  - labor
  - parts demand
  - part usage
  - notes/comments
  - closeout
- Right/context rail
  - status timeline
  - blockers
  - approvals
  - safety flags
  - compliance flags
  - documents/photos
  - related defects
  - related inspections
  - PM occurrence
  - downtime
```


---


# MaintainArr — Defect, Inspection, and Preventive Maintenance Model

## Defect

A defect is a known problem, failure, unsafe condition, compliance concern, or degraded condition affecting an asset or component.

```text
Defect
- defectId
- tenantId
- defectNumber
- title
- description
- assetRef
- componentRef
- discoveredByPersonId
- discoveredAt
- discoverySource
  - inspection
  - operator_report
  - technician
  - route_exception
  - pm
  - quality_hold
  - audit
  - compliance_check
  - customer_report
- severity
  - cosmetic
  - minor
  - moderate
  - major
  - critical
  - safety_critical
- status
  - open
  - triage
  - deferred
  - work_order_created
  - in_repair
  - repaired
  - verified
  - closed
  - canceled
- outOfServiceRequired
- safetyImpact
- complianceImpact
- productionImpact
- customerImpact
- deferAllowed
- deferApprovalRef
- deferredUntil
- deferredReason
- workOrderRefs
- inspectionRefs
- evidenceRecordRefs
- closedAt
- closedByPersonId
- closureReason
- auditTrail
```

## Defect severity definitions

```text
cosmetic
- Does not affect safe operation, compliance, or function.

minor
- Low operational impact; should be repaired but does not block use.

moderate
- Meaningful degradation; may require scheduling soon.

major
- Significant operational issue; may limit or block use.

critical
- Serious failure; asset likely down.

safety_critical
- Unsafe condition; asset should be out of service until resolved.
```

## Defect workflow

```text
1. Defect is discovered.
2. Defect is created with source context.
3. MaintainArr evaluates severity and out-of-service requirement.
4. Defect enters triage.
5. Supervisor chooses:
   - create work order
   - defer with approval
   - mark duplicate
   - cancel if invalid
6. Work order repairs defect.
7. Repair is verified.
8. Defect is closed.
```

## Inspection template

An inspection template defines the checklist used during inspection execution.

```text
InspectionTemplate
- templateId
- tenantId
- templateNumber
- title
- description
- inspectionType
  - pre_trip
  - post_trip
  - periodic
  - pm
  - safety
  - compliance
  - quality
  - return_to_service
  - custom
- status
  - draft
  - active
  - retired
  - archived
- version
- assetTypeApplicability
- assetClassApplicability
- complianceRefs
- sectionRefs
- estimatedDurationMinutes
- requiresSignature
- requiresMeterReading
- requiresLocationConfirmation
- effectiveAt
- retiredAt
- createdByPersonId
- updatedByPersonId
```

## Inspection section

```text
InspectionSection
- sectionId
- templateId
- sequence
- title
- description
- required
- itemRefs
```

## Inspection item

```text
InspectionItem
- itemId
- sectionId
- sequence
- prompt
- helpText
- responseType
  - pass_fail
  - yes_no
  - numeric
  - text
  - photo
  - signature
  - select
  - multi_select
  - meter_reading
- required
- controlledOptions
- acceptableRangeMin
- acceptableRangeMax
- unitOfMeasure
- failCreatesDefect
- failSeverityDefault
- requiresPhotoOnFail
- requiresCommentOnFail
- complianceRefs
- conditionalDisplayRules
```

## Inspection execution

```text
Inspection
- inspectionId
- tenantId
- inspectionNumber
- inspectionTemplateId
- inspectionTemplateVersion
- assetRef
- assignedPersonId
- status
  - scheduled
  - assigned
  - in_progress
  - paused
  - completed
  - failed
  - canceled
- inspectionType
  - pre_trip
  - post_trip
  - periodic
  - pm
  - safety
  - compliance
  - quality
  - return_to_service
  - custom
- sourceProduct
- sourceObjectRef
- scheduledAt
- startedAt
- completedAt
- canceledAt
- cancelReason
- pauseEvents
- totalDurationMinutes
- activeDurationMinutes
- breakDurationMinutes
- longDurationFlag
- odometerReading
- hourMeterReading
- staffarrLocationId
- checklistSectionResults
- failedItemRefs
- defectRefs
- generatedWorkOrderRefs
- operatorComplaint
- evidenceRecordRefs
- inspectorSignatureRecordRef
- supervisorReviewRequired
- supervisorReviewedByPersonId
- supervisorReviewedAt
- auditTrail
```

## Inspection answer

```text
InspectionAnswer
- answerId
- inspectionId
- itemId
- responseValue
- result
  - pass
  - fail
  - warning
  - not_applicable
- comment
- numericValue
- unitOfMeasure
- selectedOptions
- photoRecordRefs
- defectRef
- answeredAt
- answeredByPersonId
```

## Pause event

```text
InspectionPauseEvent
- pauseEventId
- inspectionId
- pausedAt
- resumedAt
- durationMinutes
- reason
  - break
  - interrupted
  - waiting_asset
  - waiting_supervisor
  - waiting_parts
  - other
- notes
```

## Preventive maintenance plan

A PM plan defines recurring maintenance requirements for one or more assets.

```text
PreventiveMaintenancePlan
- pmPlanId
- tenantId
- pmNumber
- title
- description
- status
  - draft
  - active
  - paused
  - retired
  - archived
- assetApplicability
  - specific_asset
  - asset_type
  - asset_class
  - location
  - custom_rule
- assetRefs
- assetTypeRefs
- assetClassRefs
- staffarrLocationRefs
- scheduleType
  - calendar
  - meter
  - hybrid
  - condition_based
- calendarInterval
  - daily
  - weekly
  - monthly
  - quarterly
  - semi_annual
  - annual
  - custom_days
- calendarIntervalValue
- meterType
  - odometer
  - engine_hours
  - machine_hours
  - cycles
  - custom
- meterIntervalValue
- graceWindowDays
- graceWindowMeter
- leadTimeDays
- autoGenerateWorkOrder
- autoGenerateInspection
- defaultWorkOrderTemplateRef
- inspectionTemplateRef
- estimatedDowntimeMinutes
- requiredQualificationRefs
- complianceRefs
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
```

## PM occurrence

```text
PMOccurrence
- occurrenceId
- tenantId
- pmPlanId
- assetId
- occurrenceNumber
- dueAt
- dueMeterType
- dueMeterValue
- status
  - upcoming
  - due
  - overdue
  - generated
  - skipped
  - completed
  - canceled
- generatedWorkOrderRef
- generatedInspectionRef
- completedAt
- completedByWorkOrderRef
- skippedByPersonId
- skippedAt
- skippedReason
```

## PM evaluation workflow

```text
1. Scheduler evaluates active PM plans.
2. MaintainArr checks calendar due dates and meter readings.
3. MaintainArr creates PMOccurrence for upcoming/due/overdue work.
4. If autoGenerateWorkOrder is true, MaintainArr creates WorkOrder.
5. If autoGenerateInspection is true, MaintainArr creates Inspection.
6. Asset PM status updates.
7. ReportArr receives PM facts.
```

## Inspection events

```text
maintainarr.inspection_template.created
maintainarr.inspection_template.activated
maintainarr.inspection.scheduled
maintainarr.inspection.started
maintainarr.inspection.paused
maintainarr.inspection.resumed
maintainarr.inspection.completed
maintainarr.inspection.failed
maintainarr.inspection.defect_created
```

## Defect events

```text
maintainarr.defect.created
maintainarr.defect.triaged
maintainarr.defect.deferred
maintainarr.defect.work_order_created
maintainarr.defect.repaired
maintainarr.defect.verified
maintainarr.defect.closed
```

## PM events

```text
maintainarr.pm_plan.created
maintainarr.pm_plan.activated
maintainarr.pm_occurrence.created
maintainarr.pm_occurrence.due
maintainarr.pm_occurrence.overdue
maintainarr.pm_occurrence.work_order_generated
maintainarr.pm_occurrence.inspection_generated
maintainarr.pm_occurrence.completed
maintainarr.pm_occurrence.skipped
```


---


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

---


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
- POST /handoff/redeem
- POST /service-tokens/introspect
- GET /entitlements/{productKey}

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
