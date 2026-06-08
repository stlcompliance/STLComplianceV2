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
