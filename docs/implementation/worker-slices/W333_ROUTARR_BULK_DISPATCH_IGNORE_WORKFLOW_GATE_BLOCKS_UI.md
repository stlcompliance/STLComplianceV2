# W333 — RoutArr bulk dispatch UI `ignoreWorkflowGateBlocks` surfacing (W87 gap closure)

Builds on **W87** (Compliance Core dispatch workflow gates in assign/preview/bulk APIs), **W80** (`BulkDispatchPanel` + bulk preview/apply), **W212** (unassigned work queue bulk assign), and **W78** (`confirmDispatchAssignmentPreview` / `resolveAssignmentIgnoreFlags` patterns).

Closes the W87 remaining gap: bulk dispatch UI now surfaces Compliance Core workflow gate block/warn messaging in preview summaries and passes `ignoreWorkflowGateBlocks` (plus eligibility/dispatchability/availability ignore flags) on bulk apply after operator confirmation.

## Scope

### Shared bulk confirm lib (`apps/routarr-frontend/src/lib/bulkDispatch.ts`)

| Export | Purpose |
|--------|---------|
| `formatBulkDispatchItemSummary` | Per-trip preview summary including workflow gate block/warn text |
| `resolveBulkDispatchIgnoreFlags` | Aggregates ignore flags from driver/vehicle assignment previews |
| `formatBulkDispatchBlockedMessage` | Bulk confirm dialog summary by block kind (workflow gate, eligibility, etc.) |
| `confirmBulkDispatchPreview` | Bulk equivalent of `confirmDispatchAssignmentPreview` |
| `buildBulkDispatchPreviewResponse` | Reconstruct preview response from cached preview items |

### Frontend panels

| Panel | Change |
|-------|--------|
| `BulkDispatchPanel` | Preview summaries show workflow gate conflicts; apply uses `confirmBulkDispatchPreview` and sends all ignore flags including `ignoreWorkflowGateBlocks` |
| `UnassignedWorkQueuePanel` | Bulk assign path uses shared `confirmBulkDispatchPreview` instead of availability-only override |

### Tests

| Area | Coverage |
|------|----------|
| `bulkDispatch.test.ts` | Unit tests for summary, ignore-flag resolution, confirm block/warn |
| `BulkDispatchPanel.test.tsx` | Workflow gate block preview text + `ignoreWorkflowGateBlocks: true` on confirmed apply |
| `RoutArrDispatchWorkflowGateTests` | `Bulk_apply_blocked_when_workflow_gate_blocks_and_override_succeeds` cross-product integration |

## API (unchanged — W87)

Bulk apply already supported:

```json
{
  "items": [...],
  "ignoreWorkflowGateBlocks": true
}
```

This slice wires the RoutArr frontend to that contract.

## Verification

```powershell
dotnet build apps/routarr-api/RoutArr.Api/RoutArr.Api.csproj -c Release
dotnet test tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~WorkflowGate"
cd apps/routarr-frontend
npm run test -- src/lib/bulkDispatch.test.ts src/components/BulkDispatchPanel.test.tsx
npm run build
```

## Out of scope

- Materialized dispatch gate decision snapshots on trip assign
- Compliance Core dedicated hazmat/HOS rule packs (W87 seed still links to `driver_qualification`)
- SupplyArr procurement exception post-cancel reopen (blocked on API reopen support)

## Remaining milestone gaps

- **M13 Playwright** — bulk dispatch workflow gate override journey (**closed by W334**)
- **M13 Playwright** — unassigned queue bulk assign workflow gate override journey (**closed by W335**)
- RoutArr dispatch/notification depth Playwright if gaps remain
- Render V1 deployment hardening follow-ups per `00_SLICE_STATE.md`

## Next recommended slice

- **M13 Playwright** — RoutArr unassigned queue bulk assign workflow gate override journey (**W335**), or further RoutArr dispatch/notification depth per `00_SLICE_STATE.md`
