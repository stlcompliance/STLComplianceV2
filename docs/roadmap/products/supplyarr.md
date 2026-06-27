# SupplyArr Rollout Plan

| Field | Value |
| --- | --- |
| Product key | `supplyarr` |
| Category | SRM / procurement |
| Entry release | R5 — Procure, receive, put away, reserve, and issue |
| Completion release | R5 — Procure, receive, put away, reserve, and issue |
| Expansion release | R12 — Expansion, portals, advanced integrations, AI, and category depth |
| Role | Suppliers, vendors, procurement expectations, purchase requests/orders, sourcing, pricing, and lead-time context. |
| Roadmap slice | Parts/procurement/inventory loop |
| Must not violate | Own commercial/procurement truth while LoadArr owns physical inventory and CustomArr owns customers. |
| Feature rows retained | 72 |
| Workflow rows retained | 16 |

## Release mapping

| Stage | Stage name | Features | Workflows |
| --- | --- | --- | --- |
| R5 | Procure, receive, put away, reserve, and issue | 37 | 16 |
| R12 | Expansion, portals, advanced integrations, AI, and category depth | 35 | 0 |

## Implementation interpretation

- Current/represented capabilities are hardened in R5 unless they are only supporting another release gate.
- Common category baseline remains retained for R5.
- Advanced, widely requested, or democratized capabilities remain retained for R12 unless pulled forward by a vertical slice.
- Do not implement this product by copying another product's source truth.
- Do not call this product complete until its release gates pass for data, authorization, tenant scope, UI, evidence, recovery, and reportability.

## Source docs

- [Feature catalog](../../products/supplyarr/FEATURESET.md)
- [Workflow catalog](../../products/supplyarr/WORKFLOWS.md)
- [Product manifest](../../products/supplyarr/README_manifest.md)
- [Complete feature rollout CSV](../reference/feature-rollout-map.csv)
- [Complete workflow rollout CSV](../reference/workflow-rollout-map.csv)
