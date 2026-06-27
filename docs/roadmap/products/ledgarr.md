# LedgArr Rollout Plan

| Field | Value |
| --- | --- |
| Product key | `ledgarr` |
| Category | ERP / finance bridge |
| Entry release | R11 — LedgArr bridge-first finance |
| Completion release | R11 — LedgArr bridge-first finance |
| Expansion release | R12 — Expansion, portals, advanced integrations, AI, and category depth |
| Role | Legal entities, books, dimensions, financial packet governance, posting rules, AP/AR/inventory valuation snapshots, and external ERP bridges. |
| Roadmap slice | Bridge-first finance after operating loops produce trustworthy packets |
| Must not violate | Start bridge-first; do not absorb operating truth or become a full ERP gravity well prematurely. |
| Feature rows retained | 75 |
| Workflow rows retained | 20 |

## Release mapping

| Stage | Stage name | Features | Workflows |
| --- | --- | --- | --- |
| R11 | LedgArr bridge-first finance | 40 | 20 |
| R12 | Expansion, portals, advanced integrations, AI, and category depth | 35 | 0 |

## Implementation interpretation

- Current/represented capabilities are hardened in R11 unless they are only supporting another release gate.
- Common category baseline remains retained for R11.
- Advanced, widely requested, or democratized capabilities remain retained for R12 unless pulled forward by a vertical slice.
- Do not implement this product by copying another product's source truth.
- Do not call this product complete until its release gates pass for data, authorization, tenant scope, UI, evidence, recovery, and reportability.

## Source docs

- [Feature catalog](../../products/ledgarr/FEATURESET.md)
- [Workflow catalog](../../products/ledgarr/WORKFLOWS.md)
- [Product manifest](../../products/ledgarr/README_manifest.md)
- [Complete feature rollout CSV](../reference/feature-rollout-map.csv)
- [Complete workflow rollout CSV](../reference/workflow-rollout-map.csv)
