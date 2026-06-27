# MaintainArr Rollout Plan

| Field | Value |
| --- | --- |
| Product key | `maintainarr` |
| Category | CMMS / EAM |
| Entry release | R3 — MaintainArr flagship operational slice |
| Completion release | R3 — MaintainArr flagship operational slice |
| Expansion release | R12 — Expansion, portals, advanced integrations, AI, and category depth |
| Role | Assets, defects, inspections, preventive maintenance, work orders, readiness, downtime, and maintenance execution. |
| Roadmap slice | First flagship operational slice |
| Must not violate | Prove asset-to-work-to-evidence without stealing inventory, training, quality, or document truth. |
| Feature rows retained | 73 |
| Workflow rows retained | 14 |

## Release mapping

| Stage | Stage name | Features | Workflows |
| --- | --- | --- | --- |
| R3 | MaintainArr flagship operational slice | 38 | 14 |
| R12 | Expansion, portals, advanced integrations, AI, and category depth | 35 | 0 |

## Implementation interpretation

- Current/represented capabilities are hardened in R3 unless they are only supporting another release gate.
- Common category baseline remains retained for R3.
- Advanced, widely requested, or democratized capabilities remain retained for R12 unless pulled forward by a vertical slice.
- Do not implement this product by copying another product's source truth.
- Do not call this product complete until its release gates pass for data, authorization, tenant scope, UI, evidence, recovery, and reportability.

## Source docs

- [Feature catalog](../../products/maintainarr/FEATURESET.md)
- [Workflow catalog](../../products/maintainarr/WORKFLOWS.md)
- [Product manifest](../../products/maintainarr/README_manifest.md)
- [Complete feature rollout CSV](../reference/feature-rollout-map.csv)
- [Complete workflow rollout CSV](../reference/workflow-rollout-map.csv)
