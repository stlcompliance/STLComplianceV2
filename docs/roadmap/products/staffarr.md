# StaffArr Rollout Plan

| Field | Value |
| --- | --- |
| Product key | `staffarr` |
| Category | HRM / people, roles, locations |
| Entry release | R1 — Foundation spine |
| Completion release | R4 — Training and qualification gate |
| Expansion release | R12 — Expansion, portals, advanced integrations, AI, and category depth |
| Role | People, roles, permissions context, org structure, sites, locations, incidents, and delegated account workflows. |
| Roadmap slice | Foundation spine and qualification gate |
| Must not violate | Remain the shared people/location authority while product actions stay owned by the product performing them. |
| Feature rows retained | 72 |
| Workflow rows retained | 15 |

## Release mapping

| Stage | Stage name | Features | Workflows |
| --- | --- | --- | --- |
| R1 | Foundation spine | 16 | 14 |
| R4 | Training and qualification gate | 21 | 1 |
| R12 | Expansion, portals, advanced integrations, AI, and category depth | 35 | 0 |

## Implementation interpretation

- Current/represented capabilities are hardened in R1 unless they are only supporting another release gate.
- Common category baseline remains retained for R4.
- Advanced, widely requested, or democratized capabilities remain retained for R12 unless pulled forward by a vertical slice.
- Do not implement this product by copying another product's source truth.
- Do not call this product complete until its release gates pass for data, authorization, tenant scope, UI, evidence, recovery, and reportability.

## Source docs

- [Feature catalog](../../products/staffarr/FEATURESET.md)
- [Workflow catalog](../../products/staffarr/WORKFLOWS.md)
- [Product manifest](../../products/staffarr/README_manifest.md)
- [Complete feature rollout CSV](../reference/feature-rollout-map.csv)
- [Complete workflow rollout CSV](../reference/workflow-rollout-map.csv)
