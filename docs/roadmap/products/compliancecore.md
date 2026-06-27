# Compliance Core Rollout Plan

| Field | Value |
| --- | --- |
| Product key | `compliancecore` |
| Category | GRC / rule engine |
| Entry release | R2 — Compliance Core runtime baseline |
| Completion release | R2 — Compliance Core runtime baseline |
| Expansion release | R12 — Expansion, portals, advanced integrations, AI, and category depth |
| Role | Regulatory meaning, applicability, citations, rulepacks, evidence requirements, questionnaires, and normalized compliance facts. |
| Roadmap slice | Compliance guidance baseline |
| Must not violate | Keep administrative authoring platform-admin-only while runtime guidance serves all products. |
| Feature rows retained | 79 |
| Workflow rows retained | 17 |

## Release mapping

| Stage | Stage name | Features | Workflows |
| --- | --- | --- | --- |
| R2 | Compliance Core runtime baseline | 40 | 16 |
| R12 | Expansion, portals, advanced integrations, AI, and category depth | 39 | 1 |

## Implementation interpretation

- Current/represented capabilities are hardened in R2 unless they are only supporting another release gate.
- Common category baseline remains retained for R2.
- Advanced, widely requested, or democratized capabilities remain retained for R12 unless pulled forward by a vertical slice.
- Do not implement this product by copying another product's source truth.
- Do not call this product complete until its release gates pass for data, authorization, tenant scope, UI, evidence, recovery, and reportability.

## Source docs

- [Feature catalog](../../products/compliancecore/FEATURESET.md)
- [Workflow catalog](../../products/compliancecore/WORKFLOWS.md)
- [Product manifest](../../products/compliancecore/README_manifest.md)
- [Complete feature rollout CSV](../reference/feature-rollout-map.csv)
- [Complete workflow rollout CSV](../reference/workflow-rollout-map.csv)
