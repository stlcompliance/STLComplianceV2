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
