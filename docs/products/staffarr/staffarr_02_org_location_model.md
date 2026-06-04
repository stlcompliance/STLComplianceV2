# StaffArr — Organization and Internal Location Model

## Org unit

An OrgUnit represents a structural part of the tenant organization.

Examples:

- Company
- Division
- Region
- Site
- Department
- Team
- Cost center
- Position group

```text
OrgUnit
- orgUnitId
- tenantId
- orgUnitNumber
- name
- description
- unitType
  - company
  - division
  - region
  - site
  - department
  - team
  - position_group
  - cost_center
  - business_unit
  - other
- parentOrgUnitId
- status
  - planned
  - active
  - inactive
  - archived
- managerPersonId
- effectiveStartDate
- effectiveEndDate
- addressRef
- locationRefs
- childOrgUnitRefs
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
- auditTrail
```

## Site

A Site is represented as an OrgUnit with `unitType = site`.

```text
Site
- orgUnitId
- tenantId
- orgUnitNumber
- name
- unitType: site
- status
  - planned
  - active
  - inactive
  - archived
- siteType
  - office
  - warehouse
  - plant
  - shop
  - yard
  - terminal
  - customer_embedded
  - mixed
  - other
- parentOrgUnitId
- siteManagerPersonId
- address
- timezone
- phone
- emergencyContact
- primaryLocationId
- locationRefs
- complianceRefs
- createdAt
- updatedAt
```

## Department

```text
Department
- orgUnitId
- tenantId
- orgUnitNumber
- name
- unitType: department
- status
  - active
  - inactive
  - archived
- parentOrgUnitId
- departmentManagerPersonId
- defaultSiteOrgUnitId
- defaultPermissionTemplateRefs
- defaultTrainingRequirementRefs
```

## Team

A Team can be modeled as an OrgUnit or as a Team object depending on implementation preference. StaffArr should support operational teams as first-class selectable entities.

```text
Team
- teamId
- tenantId
- teamNumber
- name
- description
- teamType
  - operational
  - maintenance
  - warehouse
  - dispatch
  - safety
  - quality
  - training
  - admin
  - project
  - emergency_response
- status
  - active
  - inactive
  - archived
- managerPersonId
- departmentOrgUnitId
- siteOrgUnitId
- memberPersonIds
- defaultPermissionRefs
- defaultLocationRefs
- createdAt
- updatedAt
```

## Position

A Position defines a role in the organization. A person may be assigned to one or more positions over time.

```text
Position
- positionId
- tenantId
- positionNumber
- title
- positionCode
- description
- status
  - draft
  - active
  - inactive
  - archived
- departmentOrgUnitId
- defaultSiteOrgUnitId
- reportsToPositionId
- defaultManagerPositionId
- defaultPermissionTemplateRefs
- defaultTrainingRequirementRefs
- requiredQualificationRefs
- complianceSensitive
- safetySensitive
- canSupervise
- canApprove
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
```

## Internal location

StaffArr owns canonical internal location identity. Other products attach behavior and operational facts to these locations.

```text
InternalLocation
- locationId
- tenantId
- locationNumber
- name
- description
- locationType
  - site
  - building
  - warehouse
  - dock
  - room
  - yard
  - parts_room
  - staging_area
  - quarantine_area
  - inspection_hold
  - receiving_staging
  - putaway_queue
  - maintenance_handoff
  - service_counter
  - technician_pickup
  - service_truck
  - shelf
  - bin
  - parking_area
  - work_cell
  - production_line
  - office
  - training_room
  - break_room
  - restricted_area
  - other
- parentLocationId
- siteOrgUnitId
- status
  - planned
  - active
  - inactive
  - restricted
  - archived
- address
- geoCoordinates
- pathSnapshot
- allowedProductUsage
  - maintainarr
  - loadarr
  - routarr
  - trainarr
  - staffarr
  - compliancecore
  - all
- safetyNotes
- accessRestrictions
- complianceRefs
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
- auditTrail
```

## Location type boundaries

```text
site
- StaffArr OrgUnit + location context for a physical site.

building
- Physical building inside a site.

warehouse
- Physical warehouse area. LoadArr may attach WMS behavior.

dock
- Physical dock door/area. RoutArr may schedule arrivals; LoadArr owns receiving behavior.

room
- Room or functional area.

yard
- Outdoor storage/parking/staging area.

parts_room
- Internal parts room identity. LoadArr owns inventory inside it.

staging_area
- Temporary staging identity. LoadArr owns stock movement behavior.

quarantine_area
- Physical quarantine identity. AssurArr owns hold decision; LoadArr owns stock behavior.

inspection_hold
- Area for inspection-held items/assets.

receiving_staging
- Physical staging location for received items before putaway.

putaway_queue
- Location identity for putaway queue.

maintenance_handoff
- Location for parts/assets handed to maintenance.

service_counter
- Counter where parts are issued or received.

technician_pickup
- Location for technician pickup.

service_truck
- Can be modeled as a StaffArr location if it holds stock/tools operationally.

shelf/bin
- If modeled as addressable locations, StaffArr owns identity; LoadArr owns stock balance.
```

## Location hierarchy example

```text
Sparta Site
- Main Warehouse
  - Receiving Dock 1
  - Receiving Dock 2
  - Receiving Staging
  - Inspection Hold
  - Quarantine
  - Parts Room
    - Shelf A
      - Bin A-01
      - Bin A-02
- Maintenance Shop
  - Service Counter
  - Maintenance Handoff
  - Technician Pickup
- Yard
  - Trailer Parking
  - Out-of-Service Row
```

## Location assignment

```text
LocationAssignment
- assignmentId
- locationId
- assignmentType
  - person_home_location
  - department_location
  - team_location
  - product_usage
  - restricted_access
- targetRef
- status
  - active
  - ended
- effectiveAt
- endsAt
- assignedByPersonId
```

## Org/location UI sections

```text
OrgStructurePage
- Org tree
- Site list
- Department list
- Team list
- Position list
- Location hierarchy
- People by org unit
- People by location
- Permission templates by role/position
- Training requirements by position
- Product usage by location
```

## Site creation workflow

```text
1. Create OrgUnit with unitType site.
2. Enter site name, address, timezone, and manager.
3. Create primary InternalLocation for site.
4. Add buildings/warehouses/docks/rooms/yards as child locations.
5. Assign departments/teams/people to site.
6. Products consume site/location references.
```

## Internal location creation workflow

```text
1. Select parent site.
2. Select parent location if nested.
3. Select location type.
4. Enter name/number.
5. Set active/restricted status.
6. Set allowed product usage.
7. Save location.
8. Publish location.created event.
9. LoadArr/MaintainArr/RoutArr/TrainArr consume location identity as needed.
```

## Org/location events

```text
staffarr.org_unit.created
staffarr.org_unit.updated
staffarr.org_unit.status_changed
staffarr.site.created
staffarr.site.updated
staffarr.department.created
staffarr.department.updated
staffarr.team.created
staffarr.team.updated
staffarr.position.created
staffarr.position.updated
staffarr.location.created
staffarr.location.updated
staffarr.location.status_changed
staffarr.location.moved
staffarr.location.restricted
staffarr.location.archived
```
