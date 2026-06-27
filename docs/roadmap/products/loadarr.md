# LoadArr Rollout Plan

| Field | Value |
| --- | --- |
| Product key | `loadarr` |
| Category | WMS / inventory |
| Entry release | R5 — Procure, receive, put away, reserve, and issue |
| Completion release | R5 — Procure, receive, put away, reserve, and issue |
| Expansion release | R12 — Expansion, portals, advanced integrations, AI, and category depth |
| Role | Receiving, putaway, locations-as-references, item balances, stock ledger, reservations, picks, issues, transfers, counts, and discrepancies. |
| Roadmap slice | Parts/procurement/inventory loop |
| Must not violate | Replace fixture/no-op/local-success behavior before any production inventory reliance. |
| Feature rows retained | 71 |
| Workflow rows retained | 15 |

## Release mapping

| Stage | Stage name | Features | Workflows |
| --- | --- | --- | --- |
| R5 | Procure, receive, put away, reserve, and issue | 36 | 13 |
| R12 | Expansion, portals, advanced integrations, AI, and category depth | 35 | 2 |

## Implementation interpretation

- Current/represented capabilities are hardened in R5 unless they are only supporting another release gate.
- Common category baseline remains retained for R5.
- Advanced, widely requested, or democratized capabilities remain retained for R12 unless pulled forward by a vertical slice.
- Do not implement this product by copying another product's source truth.
- Do not call this product complete until its release gates pass for data, authorization, tenant scope, UI, evidence, recovery, and reportability.

## Source docs

- [Feature catalog](../../products/loadarr/FEATURESET.md)
- [Workflow catalog](../../products/loadarr/WORKFLOWS.md)
- [Product manifest](../../products/loadarr/README_manifest.md)
- [Complete feature rollout CSV](../reference/feature-rollout-map.csv)
- [Complete workflow rollout CSV](../reference/workflow-rollout-map.csv)
