# AssurArr Rollout Plan

| Field | Value |
| --- | --- |
| Product key | `assurarr` |
| Category | QMS |
| Entry release | R6 — Quality hold, release, and corrective action |
| Completion release | R6 — Quality hold, release, and corrective action |
| Expansion release | R12 — Expansion, portals, advanced integrations, AI, and category depth |
| Role | Nonconformance, quality holds, releases, CAPA, audits, findings, complaints, supplier quality, and quality status. |
| Roadmap slice | Quality hold and corrective action loop |
| Must not violate | Block and release via permissioned, evidenced quality decisions rather than shadow-owning affected records. |
| Feature rows retained | 68 |
| Workflow rows retained | 14 |

## Release mapping

| Stage | Stage name | Features | Workflows |
| --- | --- | --- | --- |
| R6 | Quality hold, release, and corrective action | 33 | 14 |
| R12 | Expansion, portals, advanced integrations, AI, and category depth | 35 | 0 |

## Implementation interpretation

- Current/represented capabilities are hardened in R6 unless they are only supporting another release gate.
- Common category baseline remains retained for R6.
- Advanced, widely requested, or democratized capabilities remain retained for R12 unless pulled forward by a vertical slice.
- Do not implement this product by copying another product's source truth.
- Do not call this product complete until its release gates pass for data, authorization, tenant scope, UI, evidence, recovery, and reportability.

## Source docs

- [Feature catalog](../../products/assurarr/FEATURESET.md)
- [Workflow catalog](../../products/assurarr/WORKFLOWS.md)
- [Product manifest](../../products/assurarr/README_manifest.md)
- [Complete feature rollout CSV](../reference/feature-rollout-map.csv)
- [Complete workflow rollout CSV](../reference/workflow-rollout-map.csv)
