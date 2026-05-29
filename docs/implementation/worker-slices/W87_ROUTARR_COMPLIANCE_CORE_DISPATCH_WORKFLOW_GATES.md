# Worker 87 — Compliance Core dispatch workflow gates for RoutArr

## Slice name

M10 Compliance Core dispatch workflow gates — RoutArr calls Compliance Core internal batch workflow-gate-check before dispatch assign with trip context; configurable gate keys per featureset (driver qualification, hazmat, hours of service); assign/preview/bulk integration with `ignoreWorkflowGateBlocks`; optional Compliance Core dispatch gate seed; routarr-frontend warnings; cross-product tests.

## Products touched

- **RoutArr API** (`apps/routarr-api`): `DispatchWorkflowGateService`, `ComplianceCoreWorkflowGateClient`, `DispatchWorkflowGateRules`, `DispatchWorkflowGateContextBuilder`, `POST /api/dispatch-workflow-gates/check`, enhanced assignment preview + assign-driver/assign-vehicle + bulk with workflow gate gates, audit events.
- **Compliance Core API** (`apps/compliancecore-api`): `DispatchWorkflowGateSeedService`, `POST /api/workflow-gates/seed/dispatch` (JWT manage) for minimal dispatch gate definitions.
- **RoutArr Frontend** (`apps/routarr-frontend`): assignment panel workflow gate warnings and override flags.
- **Tests**: `DispatchWorkflowGateRulesTests`, `RoutArrDispatchWorkflowGateTests`, updated `DispatchAssignmentPanel.test.tsx`.

## Schema

No new migration — workflow gate checks are computed at request time via Compliance Core internal batch API.

## API + auth changes

### RoutArr user API (JWT + RoutArr entitlement)

| Method | Route | Auth |
|--------|-------|------|
| POST | `/api/dispatch-workflow-gates/check` | `RequireTripsAssign` (`routarr.dispatch.assign`) |

Request: `tripId`, optional `driverPersonId`, optional `vehicleRefKey`, optional `assignmentKind`.

Response: merged `outcome` (`allow` \| `warn` \| `block`), per-gate summaries, `isBlocking`.

Assignment integration:

- `POST /api/dispatch/assignments/preview` — previews include `workflowGates` summary; blocking gates set `hasBlockingConflicts`.
- `PATCH /api/trips/{tripId}/assign-driver` — blocks on workflow gate `block` unless `ignoreWorkflowGateBlocks: true` (409).
- `PATCH /api/trips/{tripId}/assign-vehicle` — same for vehicle assignment gates.
- `POST /api/dispatch/bulk/preview|apply` — bulk items include workflow gate results; apply supports `ignoreWorkflowGateBlocks`.

### Compliance Core integration API (NexArr service token → Compliance Core)

| Method | Route | Auth |
|--------|-------|------|
| POST | `/api/internal/workflow-gate-check/batch` | source `routarr`, target `compliancecore`, scope `compliancecore.workflow.gates.check` |

RoutArr sends trip context facts (`tripId`, `personId`, `vehicleRefKey`, `hasHazmatLoad`, schedule, load types, etc.).

### Compliance Core dispatch gate seed (optional)

| Method | Route | Auth |
|--------|-------|------|
| POST | `/api/workflow-gates/seed/dispatch` | JWT `RequireWorkflowGatesManage` |

Ensures gate definitions: `dispatch_driver_qualification`, `dispatch_hazmat`, `dispatch_hours_of_service` linked to the tenant's `driver_qualification` rule pack (or first active pack).

### Configuration

`apps/routarr-api/RoutArr.Api/appsettings.json`:

```json
"ComplianceCore": {
  "BaseUrl": "http://localhost:5107",
  "ServiceToken": ""
},
"DispatchWorkflowGates": {
  "CheckComplianceCoreWorkflowGates": true,
  "DriverAssignmentGateKeys": [
    "dispatch_driver_qualification",
    "dispatch_hazmat",
    "dispatch_hours_of_service"
  ],
  "VehicleAssignmentGateKeys": [
    "dispatch_hazmat"
  ]
}
```

### Gate merge rules

- Any gate `block` → merged `block`
- Else any gate `warn` → merged `warn`
- Integration unconfigured or gate missing (404) → `warn` (`workflow_gate_check_unavailable`)
- Checks disabled via config → allow (no summary applied)

Audit: `dispatch_workflow_gate.check`.

## Frontend changes

- `DispatchAssignmentPanel` — shows workflow gate warnings in conflict messaging; confirms on warn; passes `ignoreWorkflowGateBlocks` on override.
- API client: `checkDispatchWorkflowGates`, extended preview/assign types.

## Tests

### Backend unit (`DispatchWorkflowGateRulesTests`)

- Block merges when any gate blocks
- Warn merges when any gate warns
- ApplyWorkflowGates marks preview blocked

### Cross-product (`RoutArrDispatchWorkflowGateTests`)

- `Dispatch_workflow_gate_check_reports_compliance_core_block`
- `Assign_driver_blocked_when_workflow_gate_blocks_and_override_succeeds`

### Frontend unit

- `DispatchAssignmentPanel.test.tsx` — updated preview shape with `workflowGates`

## Verification commands

```powershell
dotnet build "apps/routarr-api/RoutArr.Api/RoutArr.Api.csproj" -c Release
dotnet build "apps/compliancecore-api/ComplianceCore.Api/ComplianceCore.Api.csproj" -c Release
dotnet test "tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~WorkflowGate"
cd apps/routarr-frontend
npm install
npm run test
npm run build
```

## Remaining gaps

- Materialized dispatch gate decision snapshots on trip assign
- Compliance Core dedicated rule packs for hazmat and hours-of-service (seed currently links all dispatch gates to `driver_qualification`)
- Bulk dispatch UI surfacing `ignoreWorkflowGateBlocks` explicitly — **closed by W333**

## Next slice (Worker 88)

Recommended: **Deployment/render.yaml hardening** or **Companion app field inbox** per `docs/implementation/worker-slices/00_SLICE_STATE.md`.
