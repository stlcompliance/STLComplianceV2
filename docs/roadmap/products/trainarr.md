# TrainArr Rollout Plan

| Field | Value |
| --- | --- |
| Product key | `trainarr` |
| Category | LMS / qualifications |
| Entry release | R4 — Training and qualification gate |
| Completion release | R4 — Training and qualification gate |
| Expansion release | R12 — Expansion, portals, advanced integrations, AI, and category depth |
| Role | Training definitions, assignments, evaluation, certificates, qualifications, remediation, and renewals. |
| Roadmap slice | Qualification and retraining gate |
| Must not violate | Own qualification truth while StaffArr owns people and incidents. |
| Feature rows retained | 73 |
| Workflow rows retained | 15 |

## Release mapping

| Stage | Stage name | Features | Workflows |
| --- | --- | --- | --- |
| R4 | Training and qualification gate | 38 | 13 |
| R12 | Expansion, portals, advanced integrations, AI, and category depth | 35 | 2 |

## Implementation interpretation

- Current/represented capabilities are hardened in R4 unless they are only supporting another release gate.
- Common category baseline remains retained for R4.
- Advanced, widely requested, or democratized capabilities remain retained for R12 unless pulled forward by a vertical slice.
- Do not implement this product by copying another product's source truth.
- Do not call this product complete until its release gates pass for data, authorization, tenant scope, UI, evidence, recovery, and reportability.

## Source docs

- [Feature catalog](../../products/trainarr/FEATURESET.md)
- [Workflow catalog](../../products/trainarr/WORKFLOWS.md)
- [Product manifest](../../products/trainarr/README_manifest.md)
- [Complete feature rollout CSV](../reference/feature-rollout-map.csv)
- [Complete workflow rollout CSV](../reference/workflow-rollout-map.csv)
