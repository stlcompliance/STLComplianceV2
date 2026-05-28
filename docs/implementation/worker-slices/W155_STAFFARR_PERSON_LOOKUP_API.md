# Worker 155 — StaffArr product-facing person lookup API (M4)

## Slice name

M4 workforce spine — product-facing person lookup read model (identity + org placement + active assignments), JWT query/nested routes, TrainArr integration endpoint with service token scope `staffarr.person.lookup`, StaffArr People workspace UI, integration and frontend tests

## Products touched

- **StaffArr API** (`apps/staffarr-api`): `PersonLookupService`, `/api/person-lookup`, `/api/people/{personId}/lookup`, `/api/integrations/person-lookup`, authorization helper
- **StaffArr Frontend** (`apps/staffarr-frontend`): `PersonLookupPanel` on People workspace, API client/types, workspace query wiring
- **StaffArr integration tests** (`tests/STLCompliance.StaffArr.Auth.Tests`): lookup aggregation, query parity, email lookup, auth, TrainArr integration token

## Schema

No new migration. Lookup is computed at read time from existing `staffarr_people`, `staffarr_org_units`, and `staffarr_org_unit_assignments` tables.

## API + auth changes

### StaffArr JWT (entitlement + people read / self)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/person-lookup?personId=` | `RequirePersonLookupRead` (people read roles or self) |
| GET | `/api/person-lookup?email=` | `RequirePeopleRead`, then self-check on resolved person |
| GET | `/api/people/{personId}/lookup` | `RequirePersonLookupRead` |

Response: `PersonLookupResponse` with identity fields, `placement` (primary org unit, manager, active assignments with denormalized names and assignment path), and `lookedUpAt`.

### StaffArr integration (NexArr service token → StaffArr)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/integrations/person-lookup?tenantId=&personId=` | source `trainarr`, target `staffarr`, scope `staffarr.person.lookup` |
| GET | `/api/integrations/person-lookup?tenantId=&email=` | Same |

## Permission keys

- Reuses people read scope via `RequirePersonLookupRead` (delegates to `RequirePeopleRead` except self-access for `tenant_member`)
- Integration scope: `staffarr.person.lookup`

## Frontend changes

- **Person lookup panel** on People workspace for selected person — identity, placement, active assignment paths from real API

## Tests

### Backend integration (`StaffArrPersonLookupTests`)

- `Person_lookup_returns_identity_placement_and_active_assignments`
- `Person_lookup_query_surface_matches_nested_route_and_email_lookup`
- `Person_lookup_denies_unrelated_tenant_member_reads`
- `Integration_person_lookup_allows_trainarr_service_token`
- `Integration_person_lookup_rejects_routarr_source_token`

### Frontend unit

- `PersonLookupPanel.test.tsx`

## Remaining gaps

- RoutArr/MaintainArr/SupplyArr integration consumers not wired yet (TrainArr scope only on integration route)
- `/api/person-history` rollup API and M12 personnel history rollup worker remain open
- Person lookup does not include effective permissions or readiness (separate product-facing APIs)

## Next recommended slice

**M12 personnel history rollup worker** or next open M12 worker backlog row from `00_SLICE_STATE.md`.
