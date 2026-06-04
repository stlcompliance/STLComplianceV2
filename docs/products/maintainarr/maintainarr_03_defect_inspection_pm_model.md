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
