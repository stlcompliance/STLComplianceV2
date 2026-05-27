# Worker 16 - StaffArr readiness calculation foundations

## Slice name

M4 workforce spine - person readiness read API, plain-English blockers from missing/expired/revoked readiness certifications, product-facing `/api/readiness` query surface, server-enforced authorization, and real StaffArr UI readiness summary on selected profile.

## Products touched

- **StaffArr API** (`apps/staffarr-api`): readiness calculation service, endpoints, shared effective-status resolver.
- **StaffArr Frontend** (`apps/staffarr-frontend`): readiness panel on selected profile with blockers and requirement checklist.
- **StaffArr integration tests** (`tests/STLCompliance.StaffArr.Auth.Tests`): readiness happy paths, expired blocker, query surface parity, auth denial.

## Schema

No new migration. Readiness is computed at read time from existing `staffarr_certification_definitions` and `staffarr_person_certifications` tables (Worker 15).

## API + auth changes

### StaffArr API endpoints

- `GET /api/people/{personId}/readiness` - canonical person readiness (per API conventions).
- `GET /api/readiness?personId={guid}` - product-facing query surface (same payload).

### Response shape

- `readinessStatus`: `ready` | `not_ready`
- `requirements[]`: per active readiness-category definition with `satisfied` | `missing` | `expired` | `revoked`
- `blockers[]`: plain-English messages for each unsatisfied requirement

### Authorization

- Reuses certification read scope via `RequireReadinessRead` (delegates to `RequireCertificationRead`).
- Writers and supervisors can read any person in tenant; `tenant_member` can read self only.

## Frontend changes

- Added **Workforce readiness** panel above certifications on StaffArr home for selected person.
- Shows ready/not-ready badge, blocker list, and requirement checklist from real API.
- Certification grant/revoke mutations invalidate readiness query.

## Tests

### Backend integration

- `Person_readiness_not_ready_without_certifications_lists_baseline_blockers`
- `Person_readiness_ready_when_all_baseline_certifications_active`
- `Person_readiness_expired_certification_produces_plain_english_blocker`
- `Person_readiness_query_surface_matches_nested_route`
- `Person_readiness_denies_unrelated_tenant_member_reads`

### Frontend unit

- `ReadinessPanel.test.tsx` renders status, blockers, and requirements.

## Remaining gaps

- `staffarr.readiness.override` manual overrides not implemented (separate M4 row).
- Team/site readiness rollups not implemented.
- Training blocker display and TrainArr publication ingestion remain open.
- Readiness recalculation worker and cross-product gate consumers remain M10+ scope.

## Next recommended slice

StaffArr manual readiness override foundations (`staffarr.readiness.override`, override record storage, override-aware calculation, authorized UI) or StaffArr incident intake foundations per M4 backlog order.
