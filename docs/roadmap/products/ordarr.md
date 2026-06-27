# OrdArr Rollout Plan

| Field | Value |
| --- | --- |
| Product key | `ordarr` |
| Category | OMS |
| Entry release | R7B — Order/request orchestration baseline |
| Completion release | R7B — Order/request orchestration baseline |
| Expansion release | R12 — Expansion, portals, advanced integrations, AI, and category depth |
| Role | Orders, requests, order lifecycle, triage, handoffs, exception coordination, completion packets, and bill-ready intent. |
| Roadmap slice | Order/request orchestration after customer master baseline |
| Must not violate | Explain why work is happening while execution products own how work is performed. |
| Feature rows retained | 66 |
| Workflow rows retained | 15 |

## Release mapping

| Stage | Stage name | Features | Workflows |
| --- | --- | --- | --- |
| R7B | Order/request orchestration baseline | 31 | 13 |
| R12 | Expansion, portals, advanced integrations, AI, and category depth | 35 | 2 |

## Implementation interpretation

- Current/represented capabilities are hardened in R7B unless they are only supporting another release gate.
- Common category baseline remains retained for R7B.
- Advanced, widely requested, or democratized capabilities remain retained for R12 unless pulled forward by a vertical slice.
- Do not implement this product by copying another product's source truth.
- Do not call this product complete until its release gates pass for data, authorization, tenant scope, UI, evidence, recovery, and reportability.

## Source docs

- [Feature catalog](../../products/ordarr/FEATURESET.md)
- [Workflow catalog](../../products/ordarr/WORKFLOWS.md)
- [Product manifest](../../products/ordarr/README_manifest.md)
- [Complete feature rollout CSV](../reference/feature-rollout-map.csv)
- [Complete workflow rollout CSV](../reference/workflow-rollout-map.csv)
