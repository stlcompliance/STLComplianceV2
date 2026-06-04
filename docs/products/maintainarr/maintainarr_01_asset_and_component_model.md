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
