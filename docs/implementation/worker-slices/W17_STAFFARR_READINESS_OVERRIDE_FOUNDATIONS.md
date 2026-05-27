# Worker 17 — StaffArr manual readiness override foundations

## Slice name

M4 workforce spine — `staffarr.readiness.override`, override persistence, override-aware readiness calculation, authorized UI

## Products touched

- **StaffArr API** (`apps/staffarr-api`): `staffarr_person_readiness_overrides` table, override grant/clear endpoints, readiness calculation integration, audit events
- **StaffArr Frontend** (`apps/staffarr-frontend`): override banner, grant/clear forms on readiness panel
- **StaffArr integration tests** (`tests/STLCompliance.StaffArr.Auth.Tests`): override happy path, clear path, supervisor denial, validation

## Schema

Migration `StaffArrReadinessOverrideFoundations`:

- `staffarr_person_readiness_overrides` — tenant-scoped override records (`active` / `cleared`), reason, optional `expires_at`, grant/clear actor timestamps

## API + auth changes

| Method | Route | Auth |
|--------|-------|------|
| POST | `/api/people/{personId}/readiness/override` | `staffarr.readiness.override` (tenant_admin, staffarr_admin, hr_admin, platform admin) |
| DELETE | `/api/people/{personId}/readiness/override` | same |
| GET | `/api/people/{personId}/readiness` | extended response (unchanged read gate) |

### Readiness response extensions

- `readinessBasis`: `certifications` | `manual_override`
- `activeOverride`: summary when an effective override is active (not expired)

When an active override applies and certifications are not satisfied, `readinessStatus` is `ready` with `readinessBasis` `manual_override`; blockers remain visible for audit context.

## Permission keys

- Enforced via role mapping: `staffarr.readiness.override` documented in `docs/21_PERMISSION_KEYS_AND_DEFAULT_ROLES.md`
- API gate: `RequireReadinessOverrideWrite` on grant/clear

## Frontend changes

- Readiness panel shows override banner, grant form, and clear action for authorized roles
- Mutations invalidate readiness query after grant/clear

## Tests

### Backend integration

- `Person_readiness_override_grants_ready_status_with_manual_basis_while_blockers_remain`
- `Person_readiness_override_clear_restores_not_ready_without_certifications`
- `Person_readiness_override_denies_supervisor_role`
- `Person_readiness_override_rejects_past_expiration`

### Frontend unit

- `ReadinessPanel.test.tsx` — override banner, grant submit

## Remaining gaps

- Expired overrides are ignored at read time but not auto-cleared in storage (worker deferred to M12)
- Team/site readiness rollups unchanged
- Incident intake foundations completed in Worker 18

## Next recommended slice

StaffArr incident routing to TrainArr or training blocker display per M4 backlog.
