# RoutArr Rollout Plan

| Field | Value |
| --- | --- |
| Product key | `routarr` |
| Category | TMS |
| Entry release | R8 — Dispatch and transportation execution |
| Completion release | R8 — Dispatch and transportation execution |
| Expansion release | R12 — Expansion, portals, advanced integrations, AI, and category depth |
| Role | Transportation demand, dispatch, routes, trips, stops, driver/equipment snapshots, exceptions, proof, dock visibility, and freight packets. |
| Roadmap slice | Dispatch and transportation execution |
| Must not violate | Dispatch only against explicit readiness snapshots from owning products. |
| Feature rows retained | 73 |
| Workflow rows retained | 15 |

## Release mapping

| Stage | Stage name | Features | Workflows |
| --- | --- | --- | --- |
| R8 | Dispatch and transportation execution | 38 | 15 |
| R12 | Expansion, portals, advanced integrations, AI, and category depth | 35 | 0 |

## Implementation interpretation

- Current/represented capabilities are hardened in R8 unless they are only supporting another release gate.
- Common category baseline remains retained for R8.
- Advanced, widely requested, or democratized capabilities remain retained for R12 unless pulled forward by a vertical slice.
- Do not implement this product by copying another product's source truth.
- Do not call this product complete until its release gates pass for data, authorization, tenant scope, UI, evidence, recovery, and reportability.

## Source docs

- [Feature catalog](../../products/routarr/FEATURESET.md)
- [Workflow catalog](../../products/routarr/WORKFLOWS.md)
- [Product manifest](../../products/routarr/README_manifest.md)
- [Complete feature rollout CSV](../reference/feature-rollout-map.csv)
- [Complete workflow rollout CSV](../reference/workflow-rollout-map.csv)
