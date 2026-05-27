# Worker 21 — StaffArr person timeline foundations

## Slice name

M4 workforce spine — aggregated person history timeline read model, paginated API, person-read-aligned authorization, profile UI, integration and frontend tests

## Products touched

- **StaffArr API** (`apps/staffarr-api`): `PersonTimelineService`, `GET /api/people/{personId}/timeline`
- **StaffArr Frontend** (`apps/staffarr-frontend`): `PersonTimelinePanel` on person profile (`HomePage`)
- **StaffArr integration tests** (`tests/STLCompliance.StaffArr.Auth.Tests`): timeline aggregation, pagination, auth

## Schema

No new migration. Timeline is an aggregated read model over existing StaffArr tables:

| Source table | Timeline categories / events |
|--------------|------------------------------|
| `staffarr_personnel_incidents` | `incident` / `incident_reported` |
| `staffarr_incident_trainarr_routings` (+ incidents) | `incident_routing` / `incident_routed_trainarr` (opaque `trainarr_remediation_id`) |
| `staffarr_person_readiness_overrides` | `readiness` / `readiness_override_granted`, `readiness_override_cleared` |
| `staffarr_person_certifications` (+ definitions) | `certification` / `certification_granted` |
| `staffarr_permission_history_events` | `permission` / existing permission event types |
| `staffarr_person_training_blockers` | `training_blocker` / `training_blocker_published`, `training_blocker_cleared` (opaque `trainarr_publication_id`) |

## API + auth changes

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/people/{personId}/timeline?page=&pageSize=` | Same as person profile read: platform admin, people.read roles, or `tenant_member` viewing self |

Returns `PagedResult<PersonTimelineEntryResponse>` ordered by `occurredAt` descending.

Entry fields: `entryId`, `category`, `eventType`, `title`, `detail`, `occurredAt`, `actorUserId`, `sourceEntityType`, `sourceEntityId`, `externalReferenceId` (opaque external IDs only; no cross-DB FKs).

## Frontend changes

- **Person timeline panel** on selected person profile — unified list with category badges, event labels, detail, external refs, timestamps
- `getPersonTimeline` API client; query invalidation on incident, readiness override, certification, and role-assignment mutations

## Tests

### Backend integration (`StaffArrHandoffApiTests`)

- `Person_timeline_aggregates_incidents_readiness_certifications_and_permissions`
- `Person_timeline_pagination_returns_has_next_page_when_events_exceed_page_size`
- `Person_timeline_allows_tenant_member_self_and_denies_other_people`

### Frontend unit

- `PersonTimelinePanel.test.tsx` — entry rendering and empty state

## Verification commands

```powershell
dotnet build -c Release
dotnet test "tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj" -c Release
cd apps/staffarr-frontend
npm run test -- --run
npm run build
```

## Remaining gaps

- Org assignment / manager linkage history not yet folded into person timeline (audit-only today)
- Readiness status transitions without override/cert/blocker changes are not materialized as discrete timeline rows
- TrainArr assignment/completion events remain M6 (TrainArr-owned workflow)
- Audit package export (`/api/person-history`, `/api/audit-packages`) not started

## Next recommended slice

**TrainArr training assignment engine** (M6) — natural follow-on after incident remediation intake and training blocker publication; alternatively next M4 backlog item from `00_SLICE_STATE.md` if M4 workforce spine is prioritized first.
