# Field Companion Rollout Plan

| Field | Value |
| --- | --- |
| Product key | `fieldcompanion` |
| Category | MAM / mobile companion |
| Entry release | R9 — Field Companion mobile execution |
| Completion release | R9 — Field Companion mobile execution |
| Expansion release | R12 — Expansion, portals, advanced integrations, AI, and category depth |
| Role | Mobile assigned work, secure capture/upload, offline queueing, sync, scanning, and product action surfaces. |
| Roadmap slice | Mobile execution after owning APIs are durable |
| Must not violate | Never become a mobile source of truth; replay all actions through owning APIs. |
| Feature rows retained | 71 |
| Workflow rows retained | 17 |

## Release mapping

| Stage | Stage name | Features | Workflows |
| --- | --- | --- | --- |
| R9 | Field Companion mobile execution | 33 | 13 |
| R12 | Expansion, portals, advanced integrations, AI, and category depth | 38 | 4 |

## Implementation interpretation

- Current/represented capabilities are hardened in R9 unless they are only supporting another release gate.
- Common category baseline remains retained for R9.
- Advanced, widely requested, or democratized capabilities remain retained for R12 unless pulled forward by a vertical slice.
- Do not implement this product by copying another product's source truth.
- Do not call this product complete until its release gates pass for data, authorization, tenant scope, UI, evidence, recovery, and reportability.

## Source docs

- [Feature catalog](../../products/fieldcompanion/FEATURESET.md)
- [Workflow catalog](../../products/fieldcompanion/WORKFLOWS.md)
- [Product manifest](../../products/fieldcompanion/README_manifest.md)
- [Complete feature rollout CSV](../reference/feature-rollout-map.csv)
- [Complete workflow rollout CSV](../reference/workflow-rollout-map.csv)
