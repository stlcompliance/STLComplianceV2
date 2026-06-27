# STL Compliance Roadmapped Rollout Docs

Generated: 2026-06-27

This directory reconfigures the documentation package from an unordered full-suite feature universe into a staged rollout system. No product features, workflows, ownership boundaries, or constitution discipline were intentionally removed.

## How to read the docs now

1. Start with this roadmap layer to decide what is eligible for the next release.
2. Use product `FEATURESET.md` and `WORKFLOWS.md` files as the complete feature universe.
3. Use constitutions as binding discipline. Roadmap pressure never overrides tenancy, permission, persistence, audit, source-of-truth, UI, accessibility, or failure-state requirements.
4. Use the CSV inventories in `reference/` to confirm every cataloged feature and workflow is still retained and assigned to a rollout stage.

## Roadmap spine

| Stage | Name | Feature rows mapped | Workflow rows mapped | Intent |
| --- | --- | --- | --- | --- |
| R0 | Trust gate and production truth | 0 | 0 | Fix audit blockers before feature expansion. |
| R1 | Foundation spine | 65 | 37 | Identity, people, locations, records, UI shell, quick create, and reference data baseline. |
| R2 | Compliance Core runtime baseline | 40 | 16 | Narrow but real compliance guidance/evidence engine. |
| R3 | MaintainArr flagship operational slice | 59 | 16 | Asset, defect, inspection, work order, readiness, evidence, and parts-demand handoff. |
| R4 | Training and qualification gate | 59 | 14 | Programs, assignments, certificates, qualifications, incidents, and retraining. |
| R5 | Procure, receive, put away, reserve, and issue | 73 | 29 | SupplyArr procurement plus LoadArr inventory movement/ledger. |
| R6 | Quality hold, release, and corrective action | 33 | 14 | AssurArr nonconformance, holds, releases, CAPA, complaints, audits. |
| R7A | Customer master baseline | 35 | 14 | CustomArr CRM/customer truth before order orchestration. |
| R7B | Order/request orchestration baseline | 31 | 13 | OrdArr creates, triages, coordinates, and closes requests without owning execution. |
| R8 | Dispatch and transportation execution | 38 | 15 | RoutArr demand, dispatch, trips, stops, proof, exceptions, dock visibility. |
| R9 | Field Companion mobile execution | 33 | 13 | Mobile capture, task execution, offline queue, sync, and scan-assisted work. |
| R10 | ReportArr operational reporting | 33 | 13 | Read models, dashboards, exports, scheduled reports, and provenance drillbacks. |
| R11 | LedgArr bridge-first finance | 40 | 20 | Legal entities, financial packets, dimensions, posting controls, external ERP bridge. |
| R12 | Expansion, portals, advanced integrations, AI, and category depth | 530 | 19 | Retained full-scope backlog after the core vertical product is real. |

## Core change from the old package

The old package was strong as a target-state encyclopedia, but it encouraged the feeling that every product needed every feature now. This reconfiguration keeps the encyclopedia and adds release control:

- `CURRENT` rows are hardened in the product entry release.
- `COMMON` rows are retained as the category baseline for that product's completion release.
- `TARGET` rows are retained as expansion backlog unless a vertical slice explicitly pulls them forward.
- R0 gates apply to every product before production reliance, especially partial/scaffolded areas.

## Primary roadmap files

- [roadmap-authority.md](roadmap-authority.md) — what this layer controls and what it does not override.
- [rollout-stages.md](rollout-stages.md) — complete release-train definitions.
- [release-gates-and-acceptance.md](release-gates-and-acceptance.md) — universal release gates and acceptance evidence.
- [vertical-slice-backlog.md](vertical-slice-backlog.md) — cross-product slices that prove the suite.
- [product-rollout-lanes.md](product-rollout-lanes.md) — product-by-product rollout position.
- [sequencing-rationale.md](sequencing-rationale.md) — why the order changed.
- [no-feature-loss-inventory.md](no-feature-loss-inventory.md) — validation summary and inventory links.
- [reference/feature-rollout-map.csv](reference/feature-rollout-map.csv) — complete feature inventory mapped to stages.
- [reference/workflow-rollout-map.csv](reference/workflow-rollout-map.csv) — complete workflow inventory mapped to stages.

## Practical execution rule

Do not implement horizontally across every app just to make dashboards, drawers, and create forms appear complete. Implement vertically until a real workflow can be executed, blocked, recovered, evidenced, printed, reported, and explained.
