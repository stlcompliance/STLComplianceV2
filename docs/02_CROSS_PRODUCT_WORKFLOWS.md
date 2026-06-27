# STL Compliance Cross-Product Workflow Index

Cross-product workflows are the roadmap spine. The suite is not considered mature because individual product pages exist; it is mature when real work moves across product boundaries with source-of-truth discipline, durable state, permission checks, evidence, recovery, and reportability.

## Canonical workflow packs

| Workflow pack | Primary release | Purpose |
| --- | --- | --- |
| [defect-to-work-order-to-parts-to-return-to-service](workflows/defect-to-work-order-to-parts-to-return-to-service.md) | R3 → R5 | Proves MaintainArr work execution, RecordArr evidence, Compliance Core guidance, and later SupplyArr/LoadArr parts loops. |
| [incident-to-retraining](workflows/incident-to-retraining.md) | R4 | Proves StaffArr incident truth, TrainArr retraining, renewed qualification, and product readiness checks. |
| [procure-to-receive-to-putaway](workflows/procure-to-receive-to-putaway.md) | R5 | Proves SupplyArr procurement expectations and LoadArr receiving/putaway/inventory ledger. |
| [quality-hold-release](workflows/quality-hold-release.md) | R6 | Proves AssurArr quality decision authority across affected products. |
| [order-to-fulfillment](workflows/order-to-fulfillment.md) | R7B → R8 | Proves CustomArr customer truth, OrdArr orchestration, LoadArr/RoutArr execution, and completion packets. |
| [vendor-order-completion-and-dispatch](workflows/vendor-order-completion-and-dispatch.md) | R8 | Proves supplier/vendor completion status, dispatch readiness, and execution handoff. |

## Roadmap control

- Roadmap overview: [roadmap/README.md](roadmap/README.md)
- Release stages: [roadmap/rollout-stages.md](roadmap/rollout-stages.md)
- Vertical slice backlog: [roadmap/vertical-slice-backlog.md](roadmap/vertical-slice-backlog.md)
- Complete workflow rollout CSV: [roadmap/reference/workflow-rollout-map.csv](roadmap/reference/workflow-rollout-map.csv)

A cross-product workflow is not complete until every owner can prove durable state, authorization, tenant scope, failure behavior, evidence references, source drillback, and print/report readiness.
