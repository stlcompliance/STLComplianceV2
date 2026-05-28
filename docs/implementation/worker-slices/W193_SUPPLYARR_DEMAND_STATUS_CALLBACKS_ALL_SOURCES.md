# W193 — SupplyArr demand status callbacks (all sources)

Extends **W85** (MaintainArr-only) so SupplyArr publishes procurement lifecycle updates to every demand intake source without cross-product database coupling.

## SupplyArr (publisher)

- Per-source callback services: `MaintainArrDemandStatusCallbackService` (unchanged API), `RoutArrDemandStatusCallbackService`, `TrainArrDemandStatusCallbackService`, `StaffArrDemandStatusCallbackService`.
- `SupplyArrDemandStatusCallbackCoordinator` fans out PR submit/approve/reject, PO create/issue, and receiving events to all four services in parallel.
- Shared deterministic callback keys: `DemandStatusCallbackPublicationId` (SHA-256 over tenant + demand ref + event + source record).
- HTTP clients + config: `MaintainArr__`, `RoutArr__`, `TrainArr__`, `StaffArr__` (`BaseUrl`, `ServiceToken`).
- Token catalog profiles: `supplyarr-maintainarr`, `supplyarr-routarr`, `supplyarr-trainarr`, `supplyarr-staffarr` with scopes `*.demand_status.write`.
- Demand ref tables: full procurement status vocabulary + `LastStatusCallbackAt` on RoutArr/TrainArr/StaffArr refs (MaintainArr already had this from W85).
- Intake services call source-specific `NotifyPrDraftedAsync` when auto-drafting PRs from demand refs.

## Owning products (subscribers)

Each product exposes `POST /api/integrations/supplyarr-demand-status` (service token from SupplyArr):

| Product | Scope | Status event table | Line mirror fields |
|---------|-------|------------------|-------------------|
| MaintainArr | `maintainarr.demand_status.write` | `maintainarr_work_order_parts_demand_status_events` | (W85) |
| RoutArr | `routarr.demand_status.write` | `routarr_trip_parts_demand_status_events` | procurement status on `routarr_trip_parts_demand_lines` |
| TrainArr | `trainarr.demand_status.write` | `trainarr_training_assignment_material_demand_status_events` | on `trainarr_training_assignment_material_demand_lines` |
| StaffArr | `staffarr.demand_status.write` | `staffarr_incident_supply_demand_status_events` | on `staffarr_incident_supply_demand_lines` |

Ingestion is idempotent on `SupplyarrCallbackPublicationId` (unique index). Published demand lines for the publication id receive mirrored procurement fields and audit events.

## Tests

- **MaintainArr** (W85 regression): `Pr_submit_updates_maintainarr_procurement_status`, idempotent callback.
- **RoutArr**: `Pr_submit_updates_routarr_procurement_status`, `Supplyarr_demand_status_callback_is_idempotent_for_routarr`.
- **StaffArr**: `Pr_submit_updates_staffarr_procurement_status`, `Supplyarr_demand_status_callback_is_idempotent_for_staffarr`.

## Deploy notes

Apply EF migrations for SupplyArr (demand ref `LastStatusCallbackAt` on RoutArr/TrainArr/StaffArr refs) and each product API (status event tables + line procurement columns) before enabling outbound tokens in SupplyArr.

## Next slice

**Worker 194 — DemandProcessingWorker multi-source auto-PR** (extend worker to draft PRs from RoutArr/TrainArr/StaffArr demand refs, not only MaintainArr), or remaining **M8** procurement automation per backlog.
