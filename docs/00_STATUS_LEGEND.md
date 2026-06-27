# STL Compliance Status and Rollout Legend

This legend keeps implementation maturity separate from rollout sequencing. A feature may be durable but scheduled later because its surrounding workflow is not ready. A feature may be early in the roadmap but still blocked by R0 truth gates.

## Implementation state

| State | Meaning | Release implication |
| --- | --- | --- |
| Durable | The repository contains persistent models, routes, or services that meaningfully represent the capability. | Must still pass tenancy, permission, restart, idempotency, UI, and evidence gates before release. |
| Partial | The capability is partly present but has known gaps, limited state paths, fixture assumptions, or incomplete UI/API coverage. | Cannot be relied on for production unless the R0 blockers for that slice are closed. |
| Scaffold | The capability is represented mainly as intent, prototype, fixture, in-memory behavior, UI shell, or placeholder. | Treat as roadmap inventory, not shipped truth. |
| Target | The capability is intentionally retained as future scope even when not currently implemented. | Stage by the roadmap; do not remove from product universe. |

## Feature class

| Class | Meaning | Roadmap treatment |
| --- | --- | --- |
| CURRENT | Implemented or meaningfully represented in the repository. | Harden in the product entry release, subject to R0 truth gates. |
| COMMON | Expected category baseline for that product type. | Complete as part of that product's category-completion release. |
| TARGET | Commonly requested, underserved, advanced, or democratized capability. | Retained in the expansion backlog unless pulled forward by an earlier vertical slice. |

## Rollout stages

| Stage | Name | Purpose |
| --- | --- | --- |
| R0 | Trust gate and production truth | Fix audit blockers before feature expansion. |
| R1 | Foundation spine | Identity, people, locations, records, UI shell, quick create, and reference data baseline. |
| R2 | Compliance Core runtime baseline | Narrow but real compliance guidance/evidence engine. |
| R3 | MaintainArr flagship operational slice | Asset, defect, inspection, work order, readiness, evidence, and parts-demand handoff. |
| R4 | Training and qualification gate | Programs, assignments, certificates, qualifications, incidents, and retraining. |
| R5 | Procure, receive, put away, reserve, and issue | SupplyArr procurement plus LoadArr inventory movement/ledger. |
| R6 | Quality hold, release, and corrective action | AssurArr nonconformance, holds, releases, CAPA, complaints, audits. |
| R7A | Customer master baseline | CustomArr CRM/customer truth before order orchestration. |
| R7B | Order/request orchestration baseline | OrdArr creates, triages, coordinates, and closes requests without owning execution. |
| R8 | Dispatch and transportation execution | RoutArr demand, dispatch, trips, stops, proof, exceptions, dock visibility. |
| R9 | Field Companion mobile execution | Mobile capture, task execution, offline queue, sync, and scan-assisted work. |
| R10 | ReportArr operational reporting | Read models, dashboards, exports, scheduled reports, and provenance drillbacks. |
| R11 | LedgArr bridge-first finance | Legal entities, financial packets, dimensions, posting controls, external ERP bridge. |
| R12 | Expansion, portals, advanced integrations, AI, and category depth | Retained full-scope backlog after the core vertical product is real. |

## Non-negotiable interpretation

The roadmap does not delete, downgrade, or hide feature rows. It controls when a row becomes eligible to implement or declare complete. The feature and workflow catalogs remain the complete product universe; the rollout map controls build order and release proof.
