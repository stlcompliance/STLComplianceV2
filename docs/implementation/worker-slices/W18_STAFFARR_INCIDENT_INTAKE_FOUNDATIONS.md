# Worker 18 — StaffArr incident intake foundations

## Slice name

M4 workforce spine — `staffarr.incidents.manage`, personnel incident persistence, intake/list/detail APIs, audit logging, authorized UI

## Products touched

- **StaffArr API** (`apps/staffarr-api`): `staffarr_personnel_incidents` table, incident service, `/api/incidents` endpoints, audit events
- **StaffArr Frontend** (`apps/staffarr-frontend`): personnel incidents panel on selected profile (list, detail, intake form)
- **StaffArr integration tests** (`tests/STLCompliance.StaffArr.Auth.Tests`): intake happy path, auth denial, self-scoped member read, validation

## Schema

Migration `StaffArrIncidentIntakeFoundations`:

- `staffarr_personnel_incidents` — tenant-scoped incident records with `personId` (StaffArr-local FK only), reason category, severity, status, title, description, occurrence/report timestamps

## API + auth changes

| Method | Route | Auth |
|--------|-------|------|
| POST | `/api/incidents` | `staffarr.incidents.manage` (tenant_admin, staffarr_admin, hr_admin, platform admin) |
| GET | `/api/incidents` | read scope; optional `personId` filter |
| GET | `/api/incidents/{incidentId}` | read scope for incident subject person |

### Reason categories (controlled vocabulary seed)

`safety`, `conduct`, `injury`, `equipment`, `training_compliance`, `policy`, `other`

### Severity

`low`, `medium`, `high`, `critical`

### Read authorization

- Writers and supervisors can list/read any person in tenant.
- `tenant_member` may list/read only when `personId` matches self; unfiltered list is forbidden.

## Permission keys

- Enforced via role mapping: `staffarr.incidents.manage` documented in `docs/21_PERMISSION_KEYS_AND_DEFAULT_ROLES.md`
- API gates: `RequireIncidentsManageWrite`, `RequireIncidentsRead`

## Frontend changes

- **Personnel incidents** panel on StaffArr home for selected person
- Lists incidents from real API, loads detail on selection, intake form for authorized roles
- Mutations invalidate incident list and select created record

## Tests

### Backend integration

- `Personnel_incident_intake_creates_list_and_detail_records`
- `Personnel_incident_intake_denies_supervisor_role`
- `Personnel_incident_list_allows_tenant_member_for_self_only`
- `Personnel_incident_intake_rejects_future_occurrence`

### Frontend unit

- `IncidentsPanel.test.tsx` — list rendering and intake submit

## Remaining gaps

- Incident routing to TrainArr not implemented (separate M4 row)
- Incident close/update workflows not implemented
- TrainArr training blocker display and cross-product incident forwarding remain open
- Companion app incident acknowledgements remain M11 scope

## Next recommended slice

StaffArr incident routing to TrainArr or StaffArr training blocker display per M4 backlog order.
