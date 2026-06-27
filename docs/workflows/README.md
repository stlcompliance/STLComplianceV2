# STL Compliance Cross-Product Workflow Packs

These packs are now treated as rollout proof slices. They remain cross-product workflows, not a generic workflow-owner product.

| Workflow pack | Primary rollout stage | Proof purpose |
| --- | --- | --- |
| `defect-to-work-order-to-parts-to-return-to-service.md` | R3 → R5 | Maintenance execution, evidence, readiness, then parts/inventory linkage. |
| `incident-to-retraining.md` | R4 | StaffArr incident truth and TrainArr qualification/remediation. |
| `procure-to-receive-to-putaway.md` | R5 | SupplyArr procurement expectation plus LoadArr receiving/putaway/ledger. |
| `quality-hold-release.md` | R6 | AssurArr quality decision blocking/release across affected products. |
| `order-to-fulfillment.md` | R7B → R8 | CustomArr customer truth, OrdArr orchestration, LoadArr/RoutArr execution. |
| `vendor-order-completion-and-dispatch.md` | R8 | External completion context and dispatch readiness. |

Roadmap control: [../roadmap/vertical-slice-backlog.md](../roadmap/vertical-slice-backlog.md)

---

# STL Compliance Cross-Product Workflow Packs

Workflow packs define real execution across existing products without creating a generic workflow owner.

## Packs

- `order-to-fulfillment.md`
- `procure-to-receive-to-putaway.md`
- `defect-to-work-order-to-parts-to-return-to-service.md`
- `incident-to-retraining.md`
- `quality-hold-release.md`
- `vendor-order-completion-and-dispatch.md`

Each pack must identify the trigger, participating products, owner at every step, main and alternate flows, blockers, permissions, APIs, events, handoffs, durable state, evidence, Field Companion behavior, ReportArr effects, page routes, and recovery behavior. A workflow is not complete when only the happy path renders.

