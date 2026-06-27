# ReportArr Rollout Plan

| Field | Value |
| --- | --- |
| Product key | `reportarr` |
| Category | BI / reporting |
| Entry release | R10 — ReportArr operational reporting |
| Completion release | R10 — ReportArr operational reporting |
| Expansion release | R12 — Expansion, portals, advanced integrations, AI, and category depth |
| Role | Read models, dashboards, metrics, scheduled reports, exports, provenance drillbacks, and audit-ready report outputs. |
| Roadmap slice | Operational reporting after source events exist |
| Must not violate | ReportArr projects and explains source truth; it must not correct source truth. |
| Feature rows retained | 68 |
| Workflow rows retained | 15 |

## Release mapping

| Stage | Stage name | Features | Workflows |
| --- | --- | --- | --- |
| R10 | ReportArr operational reporting | 33 | 13 |
| R12 | Expansion, portals, advanced integrations, AI, and category depth | 35 | 2 |

## Implementation interpretation

- Current/represented capabilities are hardened in R10 unless they are only supporting another release gate.
- Common category baseline remains retained for R10.
- Advanced, widely requested, or democratized capabilities remain retained for R12 unless pulled forward by a vertical slice.
- Do not implement this product by copying another product's source truth.
- Do not call this product complete until its release gates pass for data, authorization, tenant scope, UI, evidence, recovery, and reportability.

## Source docs

- [Feature catalog](../../products/reportarr/FEATURESET.md)
- [Workflow catalog](../../products/reportarr/WORKFLOWS.md)
- [Product manifest](../../products/reportarr/README_manifest.md)
- [Complete feature rollout CSV](../reference/feature-rollout-map.csv)
- [Complete workflow rollout CSV](../reference/workflow-rollout-map.csv)
