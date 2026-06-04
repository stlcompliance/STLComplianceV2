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
