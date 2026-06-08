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
