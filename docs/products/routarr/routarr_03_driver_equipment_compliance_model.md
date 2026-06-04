# RoutArr — Driver, Equipment, Readiness, and Compliance Model

## Driver assignment

A DriverAssignment links a StaffArr person to a route/trip execution role. RoutArr owns the assignment record, not the person.

```text
DriverAssignment
- driverAssignmentId
- tenantId
- tripId
- routeId
- personId
- assignmentRole
  - primary_driver
  - co_driver
  - helper
  - trainee
  - trainer
  - supervisor
- status
  - proposed
  - assigned
  - acknowledged
  - declined
  - replaced
  - completed
  - canceled
- assignedByPersonId
- assignedAt
- acknowledgedAt
- declinedAt
- declineReason
- replacedByPersonId
- requiredQualificationRefs
- qualificationCheckRef
- permissionCheckRef
- readinessCheckRef
```

## Equipment assignment

EquipmentAssignment links MaintainArr assets to a trip. RoutArr owns the assignment, not the asset.

```text
EquipmentAssignment
- equipmentAssignmentId
- tenantId
- tripId
- routeId
- equipmentRole
  - power_unit
  - trailer
  - dolly
  - forklift
  - service_vehicle
  - container
  - other
- assetRef
- status
  - proposed
  - assigned
  - readiness_failed
  - replaced
  - released
  - completed
  - canceled
- assignedByPersonId
- assignedAt
- readinessCheckRef
- defectSnapshot
- restrictionSnapshot
```

## Route assignment validation

A validation is a point-in-time result showing whether a route/trip can be released.

```text
RouteAssignmentValidation
- validationId
- tenantId
- routeId
- tripId
- status
  - pass
  - warning
  - fail
  - manual_review
- driverPersonId
- vehicleAssetRef
- trailerAssetRefs
- staffarrPermissionCheck
- staffarrPersonStatusCheck
- trainarrQualificationCheck
- maintainarrVehicleReadinessCheck
- maintainarrTrailerReadinessChecks
- complianceCoreRuleCheck
- assurarrQualityHoldCheck
- ordarrOrderBlockerCheck
- loadarrInventoryReadinessCheck
- customarrCustomerRequirementCheck
- activeBlockers
- warnings
- evaluatedAt
- evaluatedBy
  - system
  - person
```

## Driver readiness check

```text
DriverReadinessCheck
- driverReadinessCheckId
- tenantId
- personId
- tripId
- routeId
- status
  - ready
  - warning
  - blocked
  - unknown
- staffarrStatusSnapshot
- staffarrPermissionSnapshot
- trainarrQualificationSnapshot
- activeRestrictionRefs
- activeIncidentRefs
- missingRequirementRefs
- evaluatedAt
```

## Equipment readiness check

```text
EquipmentReadinessCheck
- equipmentReadinessCheckId
- tenantId
- assetRef
- tripId
- routeId
- status
  - ready
  - limited
  - blocked
  - unsafe
  - unknown
- maintainarrReadinessSnapshot
- openDefectSnapshots
- outOfServiceFlag
- inspectionStatusSnapshot
- pmStatusSnapshot
- activeHoldRefs
- evaluatedAt
```

## Transportation compliance check

```text
TransportationComplianceCheck
- complianceCheckId
- tenantId
- tripId
- routeId
- checkType
  - driver
  - vehicle
  - route
  - customer
  - document
  - load
  - jurisdiction
  - site
- status
  - pass
  - warning
  - fail
  - not_applicable
  - unknown
  - manual_review
- complianceCoreEvaluationRef
- missingEvidenceRefs
- warningRefs
- blockerRefs
- evaluatedAt
```

## Transportation document requirement

```text
TransportationDocumentRequirement
- documentRequirementId
- tenantId
- tripId
- stopId
- requirementType
  - bol
  - pod
  - registration
  - insurance
  - permit
  - customer_document
  - driver_document
  - vehicle_inspection
  - hazmat_document
  - other
- required
- status
  - missing
  - uploaded
  - accepted
  - rejected
  - waived
- complianceRef
- customerRequirementRef
- recordRefs
```

## Driver availability

```text
DriverAvailability
- availabilityId
- tenantId
- personId
- date
- status
  - available
  - assigned
  - unavailable
  - on_leave
  - restricted
  - unknown
- sourceProduct
  - staffarr
  - routarr
  - trainarr
  - manual
- sourceObjectRef
- notes
```

## Equipment availability

```text
EquipmentAvailability
- availabilityId
- tenantId
- assetRef
- date
- status
  - available
  - assigned
  - down
  - limited
  - out_of_service
  - on_hold
  - unknown
- sourceProduct
  - maintainarr
  - routarr
  - assurarr
  - manual
- sourceObjectRef
- notes
```

## Assignment validation workflow

```text
1. Dispatcher assigns driver and equipment.
2. RoutArr checks StaffArr person status and permission.
3. RoutArr checks TrainArr qualification.
4. RoutArr checks MaintainArr asset readiness.
5. RoutArr checks AssurArr quality holds.
6. RoutArr checks OrdArr order blockers.
7. RoutArr checks LoadArr inventory readiness if relevant.
8. RoutArr checks Compliance Core transportation requirements.
9. Validation returns pass/warning/fail.
10. Failing checks create blockers.
```

## Driver replacement workflow

```text
1. Assigned driver becomes unavailable, unqualified, restricted, or declines trip.
2. RoutArr marks DriverAssignment replaced/declined.
3. Dispatcher selects new driver.
4. Validation reruns.
5. Trip can release only after validation passes or override is accepted.
```

## Equipment replacement workflow

```text
1. Assigned asset becomes down/unsafe/held/unavailable.
2. RoutArr marks EquipmentAssignment readiness_failed or replaced.
3. Dispatcher selects replacement asset.
4. MaintainArr readiness check reruns.
5. Trip validation updates.
```

## Compliance document workflow

```text
1. Route/trip requires document.
2. RoutArr creates TransportationDocumentRequirement.
3. Driver/dispatcher uploads document through Field Companion/RecordArr.
4. Compliance Core may evaluate document/evidence.
5. Requirement becomes accepted, rejected, waived, or missing.
6. Trip release/completion may depend on status.
```

## Events

```text
routarr.driver_assignment.created
routarr.driver_assignment.assigned
routarr.driver_assignment.acknowledged
routarr.driver_assignment.declined
routarr.driver_assignment.replaced
routarr.driver_assignment.completed

routarr.equipment_assignment.created
routarr.equipment_assignment.assigned
routarr.equipment_assignment.readiness_failed
routarr.equipment_assignment.replaced
routarr.equipment_assignment.completed

routarr.assignment_validation.completed
routarr.assignment_validation.failed
routarr.driver_readiness.checked
routarr.equipment_readiness.checked
routarr.transportation_compliance.checked

routarr.document_requirement.created
routarr.document_requirement.satisfied
routarr.document_requirement.rejected
routarr.document_requirement.waived
```
