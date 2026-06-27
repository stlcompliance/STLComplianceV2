# NexArr Rollout Plan

| Field | Value |
| --- | --- |
| Product key | `nexarr` |
| Category | Platform/IAM |
| Entry release | R1 — Foundation spine |
| Completion release | R1 — Foundation spine |
| Expansion release | R12 — Expansion, portals, advanced integrations, AI, and category depth |
| Role | Identity, tenant membership, launch/session/service identity, platform admin, and account authority. |
| Roadmap slice | Foundation spine |
| Must not violate | Keep launch and authority truthful without recreating product entitlements for ordinary products. |
| Feature rows retained | 69 |
| Workflow rows retained | 14 |

## Release mapping

| Stage | Stage name | Features | Workflows |
| --- | --- | --- | --- |
| R1 | Foundation spine | 36 | 11 |
| R12 | Expansion, portals, advanced integrations, AI, and category depth | 33 | 3 |

## Implementation interpretation

- Current/represented capabilities are hardened in R1 unless they are only supporting another release gate.
- Common category baseline remains retained for R1.
- Advanced, widely requested, or democratized capabilities remain retained for R12 unless pulled forward by a vertical slice.
- Do not implement this product by copying another product's source truth.
- Do not call this product complete until its release gates pass for data, authorization, tenant scope, UI, evidence, recovery, and reportability.

## Source docs

- [Feature catalog](../../products/nexarr/FEATURESET.md)
- [Workflow catalog](../../products/nexarr/WORKFLOWS.md)
- [Product manifest](../../products/nexarr/README_manifest.md)
- [Complete feature rollout CSV](../reference/feature-rollout-map.csv)
- [Complete workflow rollout CSV](../reference/workflow-rollout-map.csv)
