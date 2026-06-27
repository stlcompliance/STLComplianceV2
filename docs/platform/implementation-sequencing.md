# STL Compliance Implementation Sequencing

This document is now the platform pointer to the roadmapped rollout layer. The previous implementation sequencing text is preserved at `implementation-sequencing.pre-roadmap-2026-06-27.md`.

## Authoritative rollout sequence

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

## Required reading for implementation

- [../roadmap/README.md](../roadmap/README.md)
- [../roadmap/rollout-stages.md](../roadmap/rollout-stages.md)
- [../roadmap/release-gates-and-acceptance.md](../roadmap/release-gates-and-acceptance.md)
- [../roadmap/vertical-slice-backlog.md](../roadmap/vertical-slice-backlog.md)
- [../roadmap/product-rollout-lanes.md](../roadmap/product-rollout-lanes.md)
- [../roadmap/sequencing-rationale.md](../roadmap/sequencing-rationale.md)

## Implementation rule

Do not use product feature catalogs as an unordered sprint backlog. Use them as retained end-state scope. The current release train is controlled by the roadmap, and every release train must pass the release gates before being treated as complete.

## Core sequence summary

1. R0 repairs production truth and audit blockers.
2. R1 establishes identity, people, locations, evidence, reference data, shared UI, quick create, and print/export discipline.
3. R2 gives products compliance guidance without hardcoded regulatory conclusions.
4. R3 proves the MaintainArr operational slice.
5. R4 adds qualification/retraining truth.
6. R5 adds procurement and inventory ledger truth.
7. R6 adds quality holds, release, and CAPA.
8. R7A establishes CustomArr customer master truth.
9. R7B establishes OrdArr order/request orchestration.
10. R8 executes transportation through RoutArr.
11. R9 moves selected actions into Field Companion after owning APIs are safe.
12. R10 formalizes ReportArr read models and reporting.
13. R11 adds LedgArr bridge-first finance.
14. R12 retains category depth, portals, advanced integrations, AI, and public-site expansion.
