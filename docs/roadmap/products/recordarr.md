# RecordArr Rollout Plan

| Field | Value |
| --- | --- |
| Product key | `recordarr` |
| Category | DMS / evidence vault |
| Entry release | R1 — Foundation spine |
| Completion release | R3 — MaintainArr flagship operational slice |
| Expansion release | R12 — Expansion, portals, advanced integrations, AI, and category depth |
| Role | Document metadata, files, versions, record packets, retention, evidence references, and audit packages. |
| Roadmap slice | Foundation evidence layer |
| Must not violate | Replace any in-memory/file-prototype truth before products rely on evidence persistence. |
| Feature rows retained | 69 |
| Workflow rows retained | 15 |

## Release mapping

| Stage | Stage name | Features | Workflows |
| --- | --- | --- | --- |
| R1 | Foundation spine | 13 | 12 |
| R3 | MaintainArr flagship operational slice | 21 | 2 |
| R12 | Expansion, portals, advanced integrations, AI, and category depth | 35 | 1 |

## Implementation interpretation

- Current/represented capabilities are hardened in R1 unless they are only supporting another release gate.
- Common category baseline remains retained for R3.
- Advanced, widely requested, or democratized capabilities remain retained for R12 unless pulled forward by a vertical slice.
- Do not implement this product by copying another product's source truth.
- Do not call this product complete until its release gates pass for data, authorization, tenant scope, UI, evidence, recovery, and reportability.

## Source docs

- [Feature catalog](../../products/recordarr/FEATURESET.md)
- [Workflow catalog](../../products/recordarr/WORKFLOWS.md)
- [Product manifest](../../products/recordarr/README_manifest.md)
- [Complete feature rollout CSV](../reference/feature-rollout-map.csv)
- [Complete workflow rollout CSV](../reference/workflow-rollout-map.csv)
