# W233 — TrainArr M12 demand callback visibility

Surfaces SupplyArr procurement status on the training assignment material demand workspace (W193 status callbacks).

## API (TrainArr)

- Existing: `GET/POST /api/training-assignments/{assignmentId}/material-demand`, `POST .../publish` — line list already mirrors `ProcurementStatus`, PR/PO ids, received qty, message, `LastProcurementStatusAt`.
- **New:** `GET /api/training-assignments/{assignmentId}/material-demand/status-events` — assignment-scoped timeline from `trainarr_training_assignment_material_demand_status_events` (publications linked via published demand lines). Same read auth as list (`RequireAssignmentsRead`).

## UI (trainarr-frontend)

- `AssignmentMaterialDemandPanel` on assignment workspace: demand lines with procurement badges, optional status timeline, add/publish flows for admins (`canManageAssignments`).
- API client helpers: `getTrainingAssignmentMaterialDemand`, `createTrainingAssignmentMaterialDemandLine`, `publishTrainingAssignmentMaterialDemand`, `getTrainingAssignmentMaterialDemandStatusEvents`.

## Tests

- **Integration** (`TrainArrSupplyArrMaterialDemandTests`): `Pr_submit_updates_trainarr_procurement_status`, `Supplyarr_demand_status_callback_is_idempotent_for_trainarr`, status-events list after PR submit; SupplyArr factory wires `TrainArrDemandStatusClient`.
- **Vitest:** `AssignmentMaterialDemandPanel.test.tsx` (empty state, published line badge + timeline).

## Deploy notes

Requires W193 migrations applied (status event table + line procurement columns). SupplyArr outbound token `supplyarr-trainarr` with `trainarr.demand_status.write` for live callbacks.

## Next slice

- **Suite M13** — Playwright smoke for TrainArr assignment material demand panel (publish + mocked or seeded procurement status)
- **Compliance Core M12** — audit delivery orchestration UI (W231 hook depth)
- **SupplyArr M8/M10** — procurement automation beyond W187–W199 per backlog
