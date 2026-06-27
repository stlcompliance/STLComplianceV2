# CustomArr Rollout Plan

| Field | Value |
| --- | --- |
| Product key | `customarr` |
| Category | CRM |
| Entry release | R7A — Customer master baseline |
| Completion release | R7A — Customer master baseline |
| Expansion release | R12 — Expansion, portals, advanced integrations, AI, and category depth |
| Role | Customer accounts, contacts, locations, requirements, contracts, preferences, onboarding, eligibility, and CRM/customer portal context. |
| Roadmap slice | Customer master before order orchestration |
| Must not violate | Be the customer source of truth before OrdArr, RoutArr, or SupplyArr consumes customer requirements. |
| Feature rows retained | 70 |
| Workflow rows retained | 16 |

## Release mapping

| Stage | Stage name | Features | Workflows |
| --- | --- | --- | --- |
| R7A | Customer master baseline | 35 | 14 |
| R12 | Expansion, portals, advanced integrations, AI, and category depth | 35 | 2 |

## Implementation interpretation

- Current/represented capabilities are hardened in R7A unless they are only supporting another release gate.
- Common category baseline remains retained for R7A.
- Advanced, widely requested, or democratized capabilities remain retained for R12 unless pulled forward by a vertical slice.
- Do not implement this product by copying another product's source truth.
- Do not call this product complete until its release gates pass for data, authorization, tenant scope, UI, evidence, recovery, and reportability.

## Source docs

- [Feature catalog](../../products/customarr/FEATURESET.md)
- [Workflow catalog](../../products/customarr/WORKFLOWS.md)
- [Product manifest](../../products/customarr/README_manifest.md)
- [Complete feature rollout CSV](../reference/feature-rollout-map.csv)
- [Complete workflow rollout CSV](../reference/workflow-rollout-map.csv)
