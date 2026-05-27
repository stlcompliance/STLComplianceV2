# Worker 84 — RoutArr driver eligibility

## Slice name

M10 driver eligibility — check driver `personId` against TrainArr qualification checks and StaffArr readiness before trip assign; `/api/driver-eligibility` API, integration endpoints on TrainArr/StaffArr, assign/preview/bulk integration, routarr-frontend assignment warnings, cross-product tests.

## Products touched

- **RoutArr API** (`apps/routarr-api`): `DriverEligibilityService`, `DriverEligibilityRules`, TrainArr/StaffArr HTTP clients, `POST /api/driver-eligibility/check`, enhanced assignment preview + assign-driver with eligibility gates, audit events.
- **TrainArr API** (`apps/trainarr-api`): `POST /api/integrations/routarr-qualification-check` (service token scope `trainarr.qualification_checks.dispatch`).
- **StaffArr API** (`apps/staffarr-api`): `GET /api/integrations/routarr-readiness` (service token scope `staffarr.readiness.dispatch_gate`).
- **RoutArr Frontend** (`apps/routarr-frontend`): assignment panel eligibility warnings and override flags on drag-and-drop assign.
- **Tests**: `DriverEligibilityRulesTests`, `RoutArrDriverEligibilityTests`, updated `DispatchAssignmentPanel.test.tsx`.

## Schema

No new migration — eligibility is computed at request time via cross-product HTTP calls.

## API + auth changes

### RoutArr user API (JWT + RoutArr entitlement)

| Method | Route | Auth |
|--------|-------|------|
| POST | `/api/driver-eligibility/check` | `RequireTripsAssign` (`routarr.dispatch.assign`) |

Request: `personId`, optional `qualificationKey`, optional `rulePackKey`.

Response: merged `outcome` (`allow` \| `warn` \| `block`), `reasonCode`, `message`, `isBlocking`, optional `trainArr` and `staffArr` summaries.

Assignment integration:

- `POST /api/dispatch/assignments/preview` — driver previews include `driverEligibility` summary; blocking eligibility sets `hasBlockingConflicts`.
- `PATCH /api/trips/{tripId}/assign-driver` — blocks on eligibility `block` unless `ignoreEligibilityBlocks: true` (409).
- `POST /api/dispatch/bulk/preview|apply` — bulk driver items include eligibility; apply supports `ignoreEligibilityBlocks`.

### TrainArr integration API (NexArr service token → TrainArr)

| Method | Route | Auth |
|--------|-------|------|
| POST | `/api/integrations/routarr-qualification-check` | source `routarr`, target `trainarr`, scope `trainarr.qualification_checks.dispatch` |

Delegates to existing `QualificationCheckService.CheckAsync`.

### StaffArr integration API (NexArr service token → StaffArr)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/integrations/routarr-readiness?tenantId=&personId=` | source `routarr`, target `staffarr`, scope `staffarr.readiness.dispatch_gate` |

Returns `PersonReadinessResponse` for dispatch gate evaluation.

### Configuration

`apps/routarr-api/RoutArr.Api/appsettings.json`:

```json
"DriverEligibility": {
  "QualificationKey": "driver_qualification",
  "RulePackKey": null,
  "CheckTrainArrQualification": true,
  "CheckStaffArrReadiness": true
},
"TrainArr": {
  "BaseUrl": "http://localhost:5103",
  "ServiceToken": ""
},
"StaffArr": {
  "BaseUrl": "http://localhost:5102",
  "ServiceToken": ""
}
```

### Eligibility merge rules

- StaffArr `not_ready` → `block`
- TrainArr `block` → `block`
- TrainArr `warn` (with ready StaffArr) → `warn` (non-blocking; UI confirms)
- Both integrations unconfigured → `warn` (`eligibility_check_unavailable`)

Audit: `driver_eligibility.check`.

## Frontend changes

- `DispatchAssignmentPanel` — shows eligibility warnings in conflict messaging; confirms on warn; passes `ignoreEligibilityBlocks` on override.
- API client: `checkDriverEligibility`, extended preview/assign types.

## Tests

### Backend unit (`DriverEligibilityRulesTests`)

- StaffArr not_ready merges to block
- TrainArr block merges to block
- TrainArr warn merges to warn
- ApplyEligibility marks preview blocked

### Cross-product (`RoutArrDriverEligibilityTests`)

- `Driver_eligibility_check_reports_staffarr_not_ready`
- `Assign_driver_blocked_when_staffarr_not_ready_and_override_succeeds`

### Frontend unit

- `DispatchAssignmentPanel.test.tsx` — updated preview shape with `driverEligibility`

## Verification commands

```powershell
dotnet build "apps/routarr-api/RoutArr.Api/RoutArr.Api.csproj" -c Release
dotnet test "tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj" -c Release
cd apps/routarr-frontend
npm install
npm run test
npm run build
```

## Remaining gaps

- Compliance Core dispatch workflow gates
- Driver eligibility worker / materialized eligibility cache
- SupplyArr demand status callbacks to MaintainArr (next M10 slice option)

## Next slice (Worker 85)

Recommended: **SupplyArr demand status callbacks to MaintainArr** or **RoutArr asset dispatchability checks** per M10 milestone priority — see `docs/implementation/worker-slices/00_SLICE_STATE.md`.
