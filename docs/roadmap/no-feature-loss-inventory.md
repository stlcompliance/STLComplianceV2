# No-Feature-Loss Inventory

Generated: 2026-06-27

This package keeps the original product feature and workflow catalogs and adds a rollout map on top. The complete parsed inventory is also emitted as CSV for review.

## Parsed inventory totals

| Inventory | Rows |
| --- | ---: |
| Product feature rows | 1069 |
| Product workflow rows | 233 |
| Product directories in rollout map | 16 |
| Original files copied before roadmap additions | 575 |

## Feature rows by class

| Class | Rows |
| --- | --- |
| COMMON | 276 |
| COMMON · DEMOCRATIZE | 30 |
| COMMON · UNDERSERVED | 15 |
| CURRENT | 218 |
| DEMOCRATIZE | 155 |
| FOUNDATION | 150 |
| FOUNDATION · UNDERSERVED | 30 |
| UNDERSERVED | 195 |

## Feature rows by implementation state

| State | Rows |
| --- | --- |
| Durable | 161 |
| Partial | 62 |
| Scaffold | 18 |
| Target | 828 |

## Workflow rows by class

| Class | Rows |
| --- | --- |
| COMMON | 8 |
| COMMON · DEMOCRATIZE | 11 |
| COMMON · FOUNDATION | 2 |
| COMMON · UNDERSERVED | 17 |
| CURRENT | 1 |
| CURRENT · COMMON | 131 |
| CURRENT · DEMOCRATIZE | 9 |
| CURRENT · FOUNDATION | 5 |
| CURRENT · UNDERSERVED | 29 |
| CURRENT · UNDERSERVED · DEMOCRATIZE | 1 |
| DEMOCRATIZE · FOUNDATION | 1 |
| FOUNDATION | 3 |
| FOUNDATION · DEMOCRATIZE | 1 |
| UNDERSERVED | 6 |
| UNDERSERVED · DEMOCRATIZE | 7 |
| UNDERSERVED · FOUNDATION | 1 |

## Workflow rows by implementation state

| State | Rows |
| --- | --- |
| Durable | 110 |
| Partial | 74 |
| Scaffold | 15 |
| Target | 34 |

## Product inventory

| Product | Feature rows | Workflow rows | Entry release | Rollout doc |
| --- | --- | --- | --- | --- |
| AssurArr | 68 | 14 | R6 | products/assurarr.md |
| Compliance Core | 79 | 17 | R2 | products/compliancecore.md |
| CustomArr | 70 | 16 | R7A | products/customarr.md |
| Field Companion | 71 | 17 | R9 | products/fieldcompanion.md |
| LedgArr | 75 | 20 | R11 | products/ledgarr.md |
| LoadArr | 71 | 15 | R5 | products/loadarr.md |
| MaintainArr | 73 | 14 | R3 | products/maintainarr.md |
| NexArr | 69 | 14 | R1 | products/nexarr.md |
| OrdArr | 66 | 15 | R7B | products/ordarr.md |
| RecordArr | 69 | 15 | R1 | products/recordarr.md |
| ReportArr | 68 | 15 | R10 | products/reportarr.md |
| RoutArr | 73 | 15 | R8 | products/routarr.md |
| StaffArr | 72 | 15 | R1 | products/staffarr.md |
| STLComplianceSite | 0 | 0 | R12 | products/stlcompliancesite.md |
| SupplyArr | 72 | 16 | R5 | products/supplyarr.md |
| TrainArr | 73 | 15 | R4 | products/trainarr.md |

## Full inventories

- [reference/feature-rollout-map.csv](reference/feature-rollout-map.csv)
- [reference/workflow-rollout-map.csv](reference/workflow-rollout-map.csv)
- [reference/product-stage-summary.csv](reference/product-stage-summary.csv)

## Validation statement

No product `FEATURESET.md` or `WORKFLOWS.md` catalog was intentionally reduced. The roadmap adds sequencing and release gates so implementers know what to harden now, what to complete for the current category slice, and what remains retained expansion scope.
