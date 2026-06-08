# StaffArr — Scope, Ownership, and Boundaries

## Product purpose

StaffArr is the people, organization, permission, internal location, personnel incident, readiness, and person-history authority for the STL Compliance / ARR suite.

StaffArr is not just an HR directory. It is the operational people-and-place backbone that other products use when assigning work, checking authority, locating internal assets/stock/work, routing incidents, and building audit history.

## StaffArr owns

```text
- Person master
- Person profile
- Person status
- Employment/work relationship context
- Person lifecycle
- Person-to-NexArr login linkage snapshot
- Organization units
- Sites
- Departments
- Teams
- Positions
- Reporting hierarchy
- Internal location identity
- Location hierarchy
- Location status
- Person assignment to org/location/position/team
- Product-neutral permission catalog mirror/configuration
- Role templates
- Permission assignments
- Temporary access grants
- Emergency overrides
- Restrictions against people
- Personnel incidents
- Personnel corrective actions where HR/person-related
- Person readiness view
- Person history/audit timeline
- Person audit package inputs
```

## StaffArr does not own

```text
- Platform login
- Tenant entitlement
- Product subscription
- Product launch/handoff
- Training program definitions
- Training assignment execution
- Certificate issuance truth
- Regulatory rulepack meaning
- Asset truth
- Work order truth
- Inventory balance
- Stock ledger
- Warehouse receiving
- Procurement/purchase order truth
- Route/trip execution truth
- Customer master
- Customer order lifecycle
- Document/file storage truth
- Quality hold/release decision
- Reporting read model truth
- Accounting execution
```

## External product dependencies

```text
NexArr
- Tenant
- Login account
- Product entitlement
- Platform account status
- Person login capability

TrainArr
- Training assignments
- Qualification definitions
- Person qualification status
- Certificate issuance
- Remediation assignment status

Compliance Core
- Regulatory/catalog meaning
- Role/location/person-related compliance requirements
- Evidence requirements
- Compliance evaluation

MaintainArr
- Assets
- Work orders
- Defects
- Maintenance-origin incidents
- Asset readiness facts when person assignment depends on equipment

LoadArr
- Inventory tasks assigned to StaffArr people
- WMS location behavior attached to StaffArr locations
- Inventory-origin incidents

SupplyArr
- Supplier/vendor relationship facts
- Procurement approval tasks assigned to StaffArr people
- Supplier-origin incidents when personnel action is needed

RoutArr
- Driver/dispatcher assignments
- Route/trip exceptions involving people
- Driver availability/readiness checks

CustomArr
- Customer-facing owner references
- Customer issue escalation to internal people

OrdArr
- Order owner references
- Approval/assignment references

RecordArr
- Documents, evidence, signatures, personnel files where stored as controlled records
- Audit packages

AssurArr
- Quality incidents involving people/processes
- CAPA assignments to StaffArr people
- Holds that restrict people/processes

ReportArr
- People, org, incident, readiness, and permission reporting

Field Companion
- Mobile person task inbox
- Incident reporting
- Approvals/signoffs where routed to StaffArr people
```

## Core source-of-truth rules

```text
1. StaffArr owns person profile and person status.
2. NexArr owns login capability and platform account security.
3. StaffArr references NexArr account status but does not own authentication.
4. TrainArr owns qualification/certification truth.
5. StaffArr displays qualification/readiness snapshots but does not issue certificates.
6. StaffArr owns internal location identity.
7. LoadArr owns WMS behavior and inventory inside those locations.
8. MaintainArr owns assets placed at StaffArr locations.
9. RoutArr owns trips/stops using StaffArr internal locations.
10. StaffArr owns permission assignment, but products enforce domain-specific action rules.
11. StaffArr owns personnel incident record once an incident is reported to the people layer.
12. The origin product owns the operational source event.
13. RecordArr owns the document/file/evidence object.
14. Compliance Core owns compliance meaning.
```

## Standard StaffArr object envelope

Every major StaffArr object should include:

```text
StaffArrObject
- id
- tenantId
- objectNumber
- objectType
- status
- displayName
- description
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
- sourceProduct
- sourceObjectRef
- recordRefs
- complianceRefs
- auditTrail
- eventLog
```

## StaffArr object prefixes

```text
PER    Person
ORG    Org unit
SITE   Site org unit
LOC    Internal location
POS    Position
TEAM   Team
PERM   Permission
ROLE   Role template
PASN   Person assignment
PGR    Permission grant
REST   Person restriction
INC    Personnel incident
ACT    Personnel action
RDY    Readiness snapshot
HIST   Person history event
PKG    Person audit package
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

## Standard person reference

Other products should store a reference shaped like:

```text
PersonRef
- personId
- personNumberSnapshot
- displayNameSnapshot
- primaryPositionSnapshot
- primaryDepartmentSnapshot
- primarySiteSnapshot
- statusSnapshot
- readinessSnapshot
- lastResolvedAt
```

## Standard location reference

Other products should store a reference shaped like:

```text
LocationRef
- staffarrLocationId
- locationNumberSnapshot
- locationNameSnapshot
- locationTypeSnapshot
- siteOrgUnitIdSnapshot
- siteNameSnapshot
- parentPathSnapshot
- statusSnapshot
- lastResolvedAt
```


---


# StaffArr — Person Model

## Person

A Person is the canonical human record used across the suite. A person may or may not be able to log in. Login capability is controlled through NexArr, but the human identity is represented through StaffArr/NexArr platform person identity rules.

A person can be an employee, contractor, vendor worker, customer contact, auditor, trainer, driver, mechanic, warehouse worker, supervisor, manager, admin, or other human actor.

## Person shape

```text
Person
- personId
- tenantId
- personNumber
- displayName
- legalFirstName
- legalMiddleName
- legalLastName
- preferredName
- pronouns
- status
  - applicant
  - pending_start
  - onboarding
  - active
  - leave
  - suspended
  - terminated
  - inactive
  - archived
- workRelationshipType
  - employee
  - contractor
  - temp
  - vendor_worker
  - customer_contact
  - auditor
  - service_account_contact
  - other
- employmentType
  - full_time
  - part_time
  - seasonal
  - temporary
  - contract
  - non_employee
- primaryEmail
- alternateEmail
- primaryPhone
- alternatePhone
- emergencyContactRefs
- startDate
- expectedStartDate
- terminationDate
- terminationReason
- rehireEligible
- managerPersonId
- primaryPositionId
- primaryDepartmentOrgUnitId
- primarySiteOrgUnitId
- primaryLocationId
- homeBaseLocationId
- teamRefs
- orgAssignmentRefs
- permissionAssignmentRefs
- restrictionRefs
- qualificationSnapshots
- readinessSnapshot
- activeIncidentRefs
- documentRefs
- nexarrAccountSnapshot
- hasUserAccountSnapshot
- canLoginSnapshot
- lastLoginAtSnapshot
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
- archivedAt
- archivedByPersonId
- auditTrail
```

## Person status definitions

```text
applicant
- Person exists for pre-hire/pre-engagement context.

pending_start
- Person is expected to start but is not active yet.

onboarding
- Person has started onboarding but may not be production-ready.

active
- Person is active and may be assigned work if permissions and qualifications allow.

leave
- Person is on leave and generally unavailable.

suspended
- Person is active in records but blocked from normal work.

terminated
- Person relationship ended.

inactive
- Person is not currently active but not necessarily terminated.

archived
- Person is retained for history only.
```

## Person contact

```text
PersonContact
- contactId
- personId
- contactType
  - email
  - phone
  - address
  - emergency_contact
- label
  - work
  - personal
  - mobile
  - home
  - emergency
- value
- preferred
- verified
- status
  - active
  - inactive
```

## Emergency contact

```text
EmergencyContact
- emergencyContactId
- personId
- displayName
- relationship
- phone
- alternatePhone
- email
- address
- priority
- status
```

## NexArr account snapshot

StaffArr stores only a reference/snapshot of login capability. NexArr owns the actual account/auth record.

```text
NexArrAccountSnapshot
- personId
- platformAccountId
- emailSnapshot
- usernameSnapshot
- hasUserAccount
- canLogin
- accountStatusSnapshot
  - invited
  - active
  - locked
  - disabled
  - archived
- mfaEnabledSnapshot
- lastLoginAtSnapshot
- lastResolvedAt
```

## Person assignment

A person assignment connects a person to a position, department, site, location, team, manager, or temporary responsibility.

```text
PersonAssignment
- assignmentId
- personId
- assignmentType
  - position
  - department
  - site
  - location
  - team
  - manager
  - temporary_role
  - acting_supervisor
  - trainer
  - evaluator
  - emergency_responsibility
- targetRef
- status
  - planned
  - active
  - ended
  - canceled
- primary
- effectiveAt
- endsAt
- assignedByPersonId
- reason
- sourceProduct
- sourceObjectRef
```

## Manager relationship

```text
ManagerRelationship
- relationshipId
- personId
- managerPersonId
- relationshipType
  - direct_manager
  - dotted_line
  - temporary_supervisor
  - project_supervisor
  - shift_supervisor
- status
  - active
  - ended
- effectiveAt
- endsAt
- reason
```

## Person availability

```text
PersonAvailability
- availabilityId
- personId
- status
  - available
  - unavailable
  - on_leave
  - restricted
  - assigned
  - unknown
- effectiveFrom
- effectiveTo
- reason
- sourceProduct
- sourceObjectRef
```

## Person document reference

StaffArr does not store the file. RecordArr stores the file.

```text
PersonDocumentRef
- personDocumentRefId
- personId
- recordarrRecordId
- documentType
  - identity_document
  - employment_document
  - policy_acknowledgement
  - medical_clearance_reference
  - certification_copy
  - disciplinary_record
  - incident_evidence
  - other
- statusSnapshot
- expiresAtSnapshot
- required
- complianceRef
```

## Person profile sections

```text
PersonDetail
- Header
  - displayName
  - personNumber
  - status
  - readiness
  - primary position
  - primary site
  - primary manager
- Identity
  - legal name
  - preferred name
  - contact information
  - work relationship
- Organization
  - site
  - department
  - position
  - teams
  - manager
  - direct reports
- Access
  - NexArr login capability snapshot
  - entitled products snapshot
  - role assignments
  - direct permissions
  - temporary permissions
- Readiness
  - qualifications from TrainArr
  - missing qualifications
  - active restrictions
  - active incidents
  - blocked assignments
- Work context
  - current assignments
  - product-specific references
  - recent operational history
- Incidents and actions
  - open incidents
  - corrective actions
  - retraining requirements
- Documents
  - linked RecordArr records
  - expiring/missing documents
- History
  - lifecycle events
  - org changes
  - permission changes
  - qualification changes
  - incidents
  - overrides
```

## Person creation workflow

```text
1. Select person/work relationship type.
2. Enter required identity fields.
3. Enter contact fields.
4. Assign primary site, department, position, manager, and home location.
5. Decide whether person should have login capability.
6. If login is needed, request NexArr account creation/linkage.
7. Assign role templates/permissions.
8. TrainArr evaluates required onboarding/training.
9. StaffArr creates readiness snapshot.
10. Person enters onboarding or active status.
```

## Person lifecycle workflow

```text
1. Person is created as applicant/pending_start/onboarding.
2. Person becomes active.
3. Person receives org assignments and permissions.
4. TrainArr qualifications are completed.
5. Person becomes ready for certain work.
6. Incidents/restrictions may affect readiness.
7. Person may transfer departments/sites/positions.
8. Person may go on leave/suspension.
9. Person may terminate.
10. Person is archived after retention needs are satisfied.
```

## Person events

```text
staffarr.person.created
staffarr.person.updated
staffarr.person.status_changed
staffarr.person.activated
staffarr.person.suspended
staffarr.person.terminated
staffarr.person.archived
staffarr.person.manager_changed
staffarr.person.position_changed
staffarr.person.department_changed
staffarr.person.site_changed
staffarr.person.location_changed
staffarr.person.team_added
staffarr.person.team_removed
staffarr.person.login_snapshot_changed
```


---


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


---


# StaffArr — Permission, Role, and Access Assignment Model

## Purpose

StaffArr owns the suite permission assignment layer. NexArr controls whether someone can enter a product. StaffArr controls role/permission assignments. Each product still enforces domain-specific rules for its own actions.

Example:

```text
NexArr says: this person may launch MaintainArr.
StaffArr says: this person has maintainarr.work_orders.execute.
MaintainArr says: this person can complete this specific work order only if assigned, qualified, and not blocked.
```

## Permission catalog entry

A permission catalog entry is a named action capability.

```text
Permission
- permissionId
- productKey
- permissionKey
- displayName
- description
- category
  - read
  - create
  - update
  - delete
  - approve
  - execute
  - admin
  - override
  - export
  - integration
- riskLevel
  - low
  - moderate
  - high
  - critical
- status
  - active
  - deprecated
  - retired
- requiresApproval
- segregationOfDutyGroup
- allowedAssignmentScopes
  - direct
  - role
  - position
  - team
  - department
  - site
  - temporary
  - emergency
- productOwnedEnforcement
- createdAt
- updatedAt
```

## Role template

A role template groups permissions for a product or cross-product job function.

```text
RoleTemplate
- roleTemplateId
- tenantId
- roleKey
- displayName
- description
- roleType
  - product
  - cross_product
  - position_default
  - team_default
  - site_default
  - emergency
  - custom
- productKey
- permissionRefs
- status
  - draft
  - active
  - inactive
  - archived
- assignableByPermissionKey
- requiresApproval
- defaultForPositionRefs
- defaultForTeamRefs
- defaultForDepartmentRefs
- defaultForSiteRefs
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
```

## Permission assignment

A permission assignment grants one or more permissions/roles to a person.

```text
PermissionAssignment
- assignmentId
- tenantId
- personId
- productKey
- permissionRefs
- roleTemplateRefs
- assignmentSource
  - direct
  - position
  - team
  - department
  - site
  - temporary
  - emergency_override
  - system
- sourceRef
- status
  - pending
  - active
  - suspended
  - expired
  - revoked
  - rejected
- effectiveAt
- expiresAt
- assignedByPersonId
- approvedByPersonId
- approvalRef
- reason
- restrictions
- auditTrail
```

## Temporary access grant

```text
TemporaryAccessGrant
- grantId
- tenantId
- personId
- productKey
- permissionRefs
- roleTemplateRefs
- reason
- status
  - pending
  - active
  - expired
  - revoked
- requestedByPersonId
- approvedByPersonId
- effectiveAt
- expiresAt
- sourceProduct
- sourceObjectRef
```

## Emergency override

Emergency overrides should be rare, auditable, time-limited, and reviewable.

```text
EmergencyOverride
- overrideId
- tenantId
- personId
- productKey
- permissionRefs
- reason
- emergencyType
  - safety
  - compliance
  - downtime
  - customer_impact
  - system_outage
  - other
- status
  - active
  - ended
  - revoked
  - reviewed
- activatedAt
- activatedByPersonId
- expiresAt
- endedAt
- endedByPersonId
- reviewRequired
- reviewedByPersonId
- reviewedAt
- reviewOutcome
```

## Permission check result

Other products can ask StaffArr for permission context.

```text
PermissionCheckResult
- checkId
- tenantId
- personId
- productKey
- requestedPermissionKey
- result
  - allowed
  - denied
  - conditional
  - unknown
- reason
- matchedAssignments
- activeRestrictions
- evaluatedAt
```

## Product permission examples

## NexArr-facing

StaffArr should not own login, but it may assign platform-admin/person-management permissions used alongside NexArr.

```text
nexarr.platform_admin
nexarr.tenant.read
nexarr.tenant.manage
nexarr.product_access.manage
```

## StaffArr

```text
staffarr.people.read
staffarr.people.create
staffarr.people.update
staffarr.people.status_change
staffarr.people.terminate
staffarr.people.archive

staffarr.organization.read
staffarr.organization.create
staffarr.organization.update
staffarr.organization.archive

staffarr.sites.read
staffarr.sites.create
staffarr.sites.update
staffarr.sites.archive

staffarr.locations.read
staffarr.locations.create
staffarr.locations.update
staffarr.locations.archive

staffarr.permissions.read
staffarr.permissions.assign
staffarr.permissions.revoke
staffarr.permissions.approve
staffarr.permissions.emergency_override

staffarr.incidents.read
staffarr.incidents.create
staffarr.incidents.triage
staffarr.incidents.investigate
staffarr.incidents.close

staffarr.readiness.read
staffarr.history.read
staffarr.audit_package.generate
```

## TrainArr

```text
trainarr.assignments.read
trainarr.assignments.create
trainarr.assignments.complete
trainarr.trainer.signoff
trainarr.evaluator.signoff
trainarr.programs.manage
trainarr.qualifications.read
trainarr.qualifications.override
```

## MaintainArr

```text
maintainarr.assets.read
maintainarr.assets.create
maintainarr.assets.update
maintainarr.assets.retire
maintainarr.assets.override_readiness

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

maintainarr.inspections.execute
maintainarr.pm.manage
maintainarr.parts.request
maintainarr.parts.use
maintainarr.labor.record
maintainarr.labor.approve
```

## LoadArr

```text
loadarr.inventory.read
loadarr.receiving.execute
loadarr.putaway.execute
loadarr.pick.execute
loadarr.issue.execute
loadarr.transfer.execute
loadarr.count.execute
loadarr.count.approve
loadarr.adjustments.approve
loadarr.location_profiles.manage
```

## SupplyArr

```text
supplyarr.suppliers.read
supplyarr.suppliers.create
supplyarr.suppliers.update
supplyarr.suppliers.approve
supplyarr.sourcing.manage
supplyarr.purchase_requests.create
supplyarr.purchase_requests.approve
supplyarr.purchase_orders.create
supplyarr.purchase_orders.approve
```

## RoutArr

```text
routarr.routes.read
routarr.routes.create
routarr.routes.dispatch
routarr.trips.execute
routarr.exceptions.create
routarr.exceptions.resolve
routarr.proof.capture
```

## Compliance Core

```text
compliancecore.catalogs.read
compliancecore.catalogs.manage
compliancecore.rulepacks.read
compliancecore.rulepacks.manage
compliancecore.evaluations.run
compliancecore.evidence_mapping.confirm
```

## RecordArr

```text
recordarr.records.read
recordarr.records.upload
recordarr.records.classify
recordarr.records.approve
recordarr.records.archive
recordarr.packages.generate
recordarr.retention.manage
```

## AssurArr

```text
assurarr.nonconformances.read
assurarr.nonconformances.create
assurarr.holds.place
assurarr.holds.release
assurarr.capa.manage
assurarr.audits.manage
```

## OrdArr / CustomArr / ReportArr

```text
customarr.customers.read
customarr.customers.manage
ordarr.orders.read
ordarr.orders.manage
ordarr.orders.close
reportarr.dashboards.read
reportarr.reports.run
reportarr.reports.manage
```

## Default StaffArr role templates

```text
Staff Viewer
- staffarr.people.read
- staffarr.organization.read
- staffarr.sites.read
- staffarr.locations.read
- staffarr.readiness.read

People Admin
- staffarr.people.read
- staffarr.people.create
- staffarr.people.update
- staffarr.people.status_change

Org Admin
- staffarr.organization.read
- staffarr.organization.create
- staffarr.organization.update
- staffarr.organization.archive
- staffarr.sites.read
- staffarr.sites.create
- staffarr.sites.update
- staffarr.sites.archive
- staffarr.locations.read
- staffarr.locations.create
- staffarr.locations.update
- staffarr.locations.archive
- staffarr.people.update

Permission Admin
- staffarr.permissions.read
- staffarr.permissions.assign
- staffarr.permissions.revoke
- staffarr.permissions.approve

Incident Reviewer
- staffarr.incidents.read
- staffarr.incidents.triage
- staffarr.incidents.investigate
- staffarr.incidents.close

Supervisor
- staffarr.people.read
- staffarr.readiness.read
- staffarr.incidents.create
- staffarr.incidents.triage for direct reports
- approval permissions by product

Platform People Admin
- people admin
- org admin
- permission admin
- incident reviewer
```

## Permission assignment workflow

```text
1. Admin opens person profile.
2. Admin selects product, role, or permission.
3. StaffArr checks whether assignment requires approval.
4. If approval is required, StaffArr creates pending PermissionAssignment.
5. Approver approves or rejects.
6. StaffArr activates permission.
7. StaffArr publishes permission changed event.
8. Product mirrors or queries permission state.
9. Product still applies local domain rules.
```

## Emergency override workflow

```text
1. Authorized supervisor starts emergency override.
2. Supervisor selects person, product, permission, reason, and expiration.
3. StaffArr activates temporary override.
4. Event is published to target product.
5. Override expires or is manually ended.
6. Review is required after use.
7. Audit entry is retained.
```

## Permission events

```text
staffarr.permission_catalog.imported
staffarr.permission_catalog.updated
staffarr.role_template.created
staffarr.role_template.updated
staffarr.role_template.archived
staffarr.permission_assignment.requested
staffarr.permission_assignment.approved
staffarr.permission_assignment.activated
staffarr.permission_assignment.suspended
staffarr.permission_assignment.revoked
staffarr.permission_assignment.expired
staffarr.temporary_access.granted
staffarr.temporary_access.expired
staffarr.emergency_override.activated
staffarr.emergency_override.ended
staffarr.emergency_override.reviewed
```


---


# StaffArr — Incident, Readiness, Restriction, and History Model

## Personnel incident

A PersonnelIncident is StaffArr’s central people-impact record. It may originate from MaintainArr, RoutArr, LoadArr, AssurArr, StaffArr itself, Field Companion, or another product.

The origin product owns the operational source event. StaffArr owns the personnel-facing incident record.

```text
PersonnelIncident
- incidentId
- tenantId
- incidentNumber
- title
- description
- incidentType
  - safety
  - compliance
  - quality
  - training
  - attendance
  - behavior
  - equipment_misuse
  - route_exception
  - inventory_variance
  - customer_complaint
  - policy_violation
  - injury
  - near_miss
  - property_damage
  - security
  - other
- severity
  - low
  - moderate
  - high
  - critical
- status
  - reported
  - triage
  - investigating
  - action_required
  - waiting_training
  - waiting_external_review
  - resolved
  - closed
  - canceled
- occurredAt
- reportedAt
- reportedByPersonId
- involvedPersonIds
- witnessPersonIds
- supervisorPersonId
- ownerPersonId
- sourceProduct
- sourceObjectRef
- staffarrSiteId
- staffarrLocationId
- immediateActionTaken
- injuryOrDamageFlag
- complianceImpactFlag
- trainingImpactFlag
- qualityImpactFlag
- customerImpactFlag
- requiresTrainArrReview
- trainarrAssignmentRefs
- restrictionRefs
- correctiveActionRefs
- recordRefs
- closedAt
- closedByPersonId
- closureSummary
- auditTrail
```

## Incident status definitions

```text
reported
- Incident has been reported but not reviewed.

triage
- Supervisor/safety/HR is determining classification, severity, and next steps.

investigating
- Facts are being collected.

action_required
- Corrective action, restriction, retraining, review, or documentation is needed.

waiting_training
- TrainArr remediation or qualification update is pending.

waiting_external_review
- External review, legal, customer, vendor, or regulatory response is pending.

resolved
- Required actions are complete.

closed
- Administrative review is complete and incident is closed.

canceled
- Incident was invalid, duplicate, or created in error.
```

## Personnel corrective action

```text
PersonnelCorrectiveAction
- correctiveActionId
- incidentId
- personId
- actionType
  - coaching
  - retraining
  - warning
  - restriction
  - suspension
  - reassignment
  - observation_period
  - policy_acknowledgement
  - other
- status
  - open
  - in_progress
  - completed
  - canceled
- assignedByPersonId
- assignedToPersonId
- dueAt
- completedAt
- completionSummary
- trainarrAssignmentRef
- recordRefs
```

## Person restriction

A restriction limits what a person can do until lifted, expired, or overridden.

```text
PersonRestriction
- restrictionId
- tenantId
- personId
- restrictionType
  - cannot_operate_asset
  - cannot_drive
  - cannot_dispatch
  - cannot_train
  - cannot_evaluate
  - cannot_approve
  - cannot_work_unsupervised
  - site_restricted
  - location_restricted
  - task_restricted
  - product_restricted
  - customer_restricted
- sourceProduct
- sourceObjectRef
- incidentRef
- reason
- status
  - active
  - lifted
  - expired
  - overridden
  - canceled
- effectiveAt
- expiresAt
- liftedAt
- liftedByPersonId
- liftReason
- affectedProductKeys
- affectedObjectRefs
- recordRefs
```

## Readiness snapshot

StaffArr owns the readiness view, not all readiness facts. It composes data from StaffArr, TrainArr, product restrictions, incidents, permissions, and assignment requirements.

```text
PersonReadinessSnapshot
- readinessSnapshotId
- tenantId
- personId
- overallStatus
  - ready
  - limited
  - blocked
  - onboarding
  - unknown
- evaluatedAt
- evaluatedBy
  - system
  - person
- positionReadiness
- siteReadiness
- productReadiness
- qualificationSummary
- permissionSummary
- activeRestrictionRefs
- activeIncidentRefs
- missingQualificationRefs
- expiredQualificationRefs
- pendingTrainingAssignmentRefs
- missingDocumentRefs
- blockerRefs
- warningRefs
- sourceSnapshots
```

## Product readiness

```text
ProductReadiness
- productKey
- readinessStatus
  - ready
  - limited
  - blocked
  - not_entitled
  - unknown
- permissionStatus
  - sufficient
  - insufficient
  - unknown
- qualificationStatus
  - sufficient
  - missing
  - expired
  - not_required
  - unknown
- restrictionStatus
  - none
  - active
- blockers
- warnings
```

## Readiness blocker

```text
ReadinessBlocker
- blockerId
- personId
- blockerType
  - missing_permission
  - missing_qualification
  - expired_qualification
  - pending_training
  - active_incident
  - active_restriction
  - missing_document
  - suspended_status
  - leave_status
  - product_access_missing
  - compliance_issue
- sourceProduct
- sourceObjectRef
- severity
  - warning
  - blocking
  - critical
- title
- description
- requiredAction
- status
  - active
  - resolved
  - overridden
```

## Person history event

A PersonHistoryEvent is the audit-friendly timeline record.

```text
PersonHistoryEvent
- historyEventId
- tenantId
- personId
- eventType
  - person_created
  - status_changed
  - manager_changed
  - position_changed
  - department_changed
  - site_changed
  - location_changed
  - team_changed
  - permission_granted
  - permission_revoked
  - qualification_issued
  - qualification_expired
  - training_assigned
  - training_completed
  - incident_created
  - incident_closed
  - restriction_created
  - restriction_lifted
  - document_added
  - override_used
  - audit_package_generated
- occurredAt
- actorPersonId
- actorServiceClientId
- sourceProduct
- sourceObjectRef
- title
- summary
- beforeSnapshot
- afterSnapshot
- recordRefs
```

## Person audit package

A PersonAuditPackage is assembled for internal review, compliance audit, incident investigation, or legal/personnel review.

```text
PersonAuditPackage
- packageId
- tenantId
- packageNumber
- personId
- packageType
  - compliance
  - training
  - incident
  - employment_history
  - permission_review
  - full_person_history
- status
  - draft
  - assembling
  - complete
  - locked
  - archived
- requestedByPersonId
- requestedAt
- dateRangeStart
- dateRangeEnd
- includedSections
  - profile
  - org_history
  - permission_history
  - qualification_history
  - training_history
  - incident_history
  - restriction_history
  - document_refs
  - product_activity_refs
- sourceRefs
- recordarrPackageRef
- generatedAt
- lockedAt
```

## Incident routing rules

```text
MaintainArr incident examples
- Asset misuse
- Repeated repair error
- Unsafe repair behavior
- Missed inspection
- Safety-critical defect ignored

RoutArr incident examples
- Driver accident
- Refused delivery behavior issue
- HOS/compliance concern
- Vehicle misuse
- Missed route due to person issue

LoadArr incident examples
- Inventory variance with personnel concern
- Unsafe forklift operation
- Receiving error with training concern
- Pick/issue misconduct

AssurArr incident examples
- Repeat quality failure
- CAPA action not completed
- Process nonconformance tied to person/team

Field Companion incident examples
- Self-reported safety issue
- Near miss
- Injury
- Policy violation
```

## Incident-to-training workflow

```text
1. Product reports incident to StaffArr.
2. StaffArr creates PersonnelIncident.
3. StaffArr triages severity and impact flags.
4. StaffArr determines whether training review is required.
5. StaffArr sends remediation request to TrainArr.
6. TrainArr creates assignment.
7. StaffArr sets incident status to waiting_training.
8. TrainArr reports completion/failure.
9. StaffArr updates readiness and restrictions.
10. StaffArr closes incident after required actions.
```

## Restriction workflow

```text
1. Incident, qualification issue, supervisor action, or compliance finding creates restriction.
2. StaffArr publishes restriction event.
3. Products consume restriction and block affected actions.
4. Restriction expires or is lifted by authorized person.
5. StaffArr publishes restriction lifted event.
6. Person readiness recalculates.
```

## Readiness evaluation workflow

```text
1. StaffArr receives person/org/permission/incident/restriction change.
2. StaffArr requests or consumes TrainArr qualification status.
3. StaffArr checks active restrictions and incidents.
4. StaffArr checks product permission state.
5. StaffArr checks required documents where applicable.
6. StaffArr produces readiness snapshot.
7. Products may query readiness before assignment.
```

## Incident/readiness/history events

```text
staffarr.incident.created
staffarr.incident.triaged
staffarr.incident.status_changed
staffarr.incident.forwarded_to_trainarr
staffarr.incident.closed

staffarr.corrective_action.created
staffarr.corrective_action.completed

staffarr.restriction.created
staffarr.restriction.lifted
staffarr.restriction.expired

staffarr.readiness.evaluated
staffarr.readiness.changed
staffarr.readiness.blocker_created
staffarr.readiness.blocker_resolved

staffarr.person_history.event_created
staffarr.audit_package.created
staffarr.audit_package.locked
```


---


# StaffArr — Workflows, Status Logic, Events, and APIs

## Major workflow: person onboarding

```text
1. StaffArr creates Person.
2. StaffArr assigns primary site, department, position, manager, and home location.
3. StaffArr creates org/person assignment records.
4. StaffArr requests NexArr login account if canLogin is true.
5. StaffArr applies default role templates from position/team/site.
6. StaffArr publishes person.created and org assignment events.
7. TrainArr evaluates required onboarding/training.
8. StaffArr receives qualification/training snapshots.
9. StaffArr evaluates readiness.
10. Person moves from onboarding to active/ready when requirements are met.
```

## Major workflow: person transfer

```text
1. Admin changes person position, department, site, manager, or team.
2. StaffArr ends old assignment where appropriate.
3. StaffArr creates new assignment.
4. StaffArr recalculates inherited permissions.
5. StaffArr publishes person org change event.
6. TrainArr reevaluates training requirements.
7. Products update local mirrors/snapshots.
8. Readiness recalculates.
```

## Major workflow: termination/deactivation

```text
1. Admin starts termination/deactivation.
2. StaffArr changes person status.
3. StaffArr ends active assignments.
4. StaffArr revokes or schedules revocation of product permission assignments.
5. NexArr disables login if required.
6. StaffArr publishes status and permission events.
7. Products stop assigning new work to the person.
8. StaffArr retains history for audit/retention.
```

## Major workflow: site/location creation

```text
1. Admin creates site OrgUnit.
2. StaffArr creates primary site InternalLocation.
3. Admin adds child locations: buildings, docks, warehouses, rooms, yards, bins, etc.
4. StaffArr publishes location events.
5. MaintainArr uses locations for assets/work.
6. LoadArr attaches WMS behavior to locations.
7. RoutArr uses locations for depots/docks/internal stops.
8. TrainArr uses site/location scope for training applicability.
```

## Major workflow: permission assignment

```text
1. Admin selects person.
2. Admin selects product role or permission.
3. StaffArr checks approval requirement.
4. StaffArr creates pending or active PermissionAssignment.
5. StaffArr publishes permission event.
6. Products mirror or query permissions.
7. Product still enforces local domain rules.
```

## Major workflow: incident intake from product

```text
1. Origin product captures operational incident facts.
2. Origin product POSTs incident to StaffArr.
3. StaffArr creates PersonnelIncident.
4. StaffArr triages severity and people impact.
5. StaffArr creates restrictions/corrective actions if needed.
6. StaffArr forwards to TrainArr if training impact exists.
7. StaffArr updates person readiness.
8. ReportArr receives incident/readiness facts.
```

## Major workflow: readiness check before work assignment

```text
1. Product wants to assign person to work.
2. Product calls StaffArr readiness/permission endpoint.
3. StaffArr returns person status, permissions, restrictions, and qualification snapshots.
4. Product may call TrainArr directly for fresh qualification check if needed.
5. Product decides assignment using its own domain rules.
6. Assignment proceeds or is blocked with reason.
```

## StaffArr emitted events

```text
staffarr.person.created
staffarr.person.updated
staffarr.person.status_changed
staffarr.person.activated
staffarr.person.suspended
staffarr.person.terminated
staffarr.person.archived
staffarr.person.manager_changed
staffarr.person.position_changed
staffarr.person.department_changed
staffarr.person.site_changed
staffarr.person.location_changed
staffarr.person.team_added
staffarr.person.team_removed
staffarr.person.login_snapshot_changed

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

staffarr.permission_catalog.imported
staffarr.permission_catalog.updated
staffarr.role_template.created
staffarr.role_template.updated
staffarr.role_template.archived
staffarr.permission_assignment.requested
staffarr.permission_assignment.approved
staffarr.permission_assignment.activated
staffarr.permission_assignment.suspended
staffarr.permission_assignment.revoked
staffarr.permission_assignment.expired
staffarr.temporary_access.granted
staffarr.temporary_access.expired
staffarr.emergency_override.activated
staffarr.emergency_override.ended
staffarr.emergency_override.reviewed

staffarr.incident.created
staffarr.incident.triaged
staffarr.incident.status_changed
staffarr.incident.forwarded_to_trainarr
staffarr.incident.closed
staffarr.corrective_action.created
staffarr.corrective_action.completed
staffarr.restriction.created
staffarr.restriction.lifted
staffarr.restriction.expired
staffarr.readiness.evaluated
staffarr.readiness.changed
staffarr.person_history.event_created
staffarr.audit_package.created
```

## Integration APIs StaffArr should expose

```text
GET /api/v1/integrations/persons
GET /api/v1/integrations/persons/{personId}
GET /api/v1/integrations/persons/{personId}/summary
GET /api/v1/integrations/persons/{personId}/readiness
GET /api/v1/integrations/persons/{personId}/permissions
GET /api/v1/integrations/persons/{personId}/qualifications-snapshot
GET /api/v1/integrations/persons/{personId}/restrictions
GET /api/v1/integrations/persons/{personId}/history

POST /api/v1/integrations/person-readiness-checks
POST /api/v1/integrations/permission-checks
POST /api/v1/integrations/assignment-checks

GET /api/v1/integrations/org-units
GET /api/v1/integrations/org-units/{orgUnitId}
GET /api/v1/integrations/sites
GET /api/v1/integrations/sites/{orgUnitId}
GET /api/v1/integrations/departments
GET /api/v1/integrations/positions
GET /api/v1/integrations/teams

GET /api/v1/integrations/locations
GET /api/v1/integrations/locations/{locationId}
GET /api/v1/integrations/sites/{siteOrgUnitId}/locations
GET /api/v1/integrations/locations/{locationId}/children

POST /api/v1/integrations/incidents
GET /api/v1/integrations/incidents/{incidentId}
POST /api/v1/integrations/incidents/{incidentId}/status-updates
POST /api/v1/integrations/incidents/{incidentId}/training-impact

POST /api/v1/integrations/restrictions
POST /api/v1/integrations/restrictions/{restrictionId}/lift

POST /api/v1/integrations/person-history-events
POST /api/v1/integrations/audit-packages
```

## APIs StaffArr should consume

```text
NexArr
- GET /platform/tenants/{tenantId}
- GET /platform/accounts/by-person/{personId}
- POST /platform/accounts
- PATCH /platform/accounts/{accountId}
- GET /platform/entitlements/{productKey}

TrainArr
- GET /persons/{personId}/qualifications
- GET /persons/{personId}/training-assignments
- POST /remediation-requests
- POST /training-requirement-evaluations

Compliance Core
- GET /catalogs/governing-bodies
- POST /evaluations
- GET /rulepacks

RecordArr
- POST /records
- GET /records/{recordId}
- POST /record-packages

ReportArr
- POST /events
```

## Permission examples for StaffArr itself

```text
staffarr.people.read
staffarr.people.create
staffarr.people.update
staffarr.people.status_change
staffarr.people.terminate
staffarr.people.archive

staffarr.organization.read
staffarr.organization.create
staffarr.organization.update
staffarr.organization.archive

staffarr.sites.read
staffarr.sites.create
staffarr.sites.update
staffarr.sites.archive

staffarr.locations.read
staffarr.locations.create
staffarr.locations.update
staffarr.locations.archive

staffarr.positions.read
staffarr.positions.manage
staffarr.teams.read
staffarr.teams.manage

staffarr.permissions.read
staffarr.permissions.assign
staffarr.permissions.revoke
staffarr.permissions.approve
staffarr.permissions.emergency_override

staffarr.incidents.read
staffarr.incidents.create
staffarr.incidents.triage
staffarr.incidents.investigate
staffarr.incidents.close

staffarr.restrictions.read
staffarr.restrictions.create
staffarr.restrictions.lift

staffarr.readiness.read
staffarr.history.read
staffarr.audit_package.generate
staffarr.admin
```

## Default StaffArr roles

```text
Staff Viewer
- Read people, organization, sites, locations, readiness.

People Admin
- Create/update people and manage lifecycle.

Org Admin
- Manage organization units, sites, departments, positions, teams, and locations.

Permission Admin
- Assign/revoke permissions and manage role templates.

Incident Reviewer
- Triage/investigate/close personnel incidents.

Supervisor
- View direct reports, readiness, create incidents, approve certain actions.

Location Admin
- Manage internal locations and location hierarchy.

Platform People Admin
- Broad StaffArr administration excluding NexArr auth/security ownership.

Auditor
- Read person history, incidents, readiness, and generate audit packages.
```

## StaffArr UI surfaces

```text
/app/staffarr
- dashboard
- people
- person detail
- org structure
- sites
- locations
- departments
- positions
- teams
- permissions
- role templates
- incidents
- restrictions
- readiness
- audit packages
- settings
```

## Person detail UI

```text
PersonDetailPage
- Summary header
- Status/readiness cards
- Org assignment
- Manager/direct reports
- Product access/permissions
- Qualifications snapshot
- Active blockers/restrictions
- Incidents
- Documents
- History timeline
- Audit package action
```

## Location detail UI

```text
LocationDetailPage
- Location header
- Parent path
- Site context
- Status
- Allowed product usage
- Child locations
- People assigned
- Product behavior snapshots
  - LoadArr WMS profile
  - MaintainArr assets at location
  - RoutArr dock/depot usage
- Safety/access notes
- History
```

## Incident detail UI

```text
IncidentDetailPage
- Incident header
- Status/severity
- Involved people
- Source product/source object
- Timeline
- Immediate action
- Investigation notes
- Restrictions
- Corrective actions
- Training impact
- Record evidence
- Closure
```
