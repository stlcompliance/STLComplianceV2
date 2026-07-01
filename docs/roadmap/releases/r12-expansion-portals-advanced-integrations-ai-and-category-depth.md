# R12 — Expansion, portals, advanced integrations, AI, and category depth

Preserve every common, advanced, and widely unavailable feature while preventing it from disrupting the first shippable operating spine.

| Field | Definition |
| --- | --- |
| Entry condition | Earlier release trains have proven the suite can execute and explain real work. |
| Exit condition | Expansion features are pulled forward only when their source owners, release gates, and cross-product contracts are ready. |
| Total feature rows mapped here | 530 |
| Total workflow rows mapped here | 19 |

## Product entry owners

| Product | Feature rows | Workflow rows | Role |
| --- | --- | --- | --- |
| STLComplianceSite | 0 | 0 | Public marketing, product/industry pages, legal/trust content, lead inquiry, and site-to-CustomArr handoff. |

## Inventory mapped to this release

| Product | Features mapped here | Workflows mapped here |
| --- | --- | --- |
| NexArr | 33 | 3 |
| StaffArr | 35 | 0 |
| RecordArr | 35 | 1 |
| Compliance Core | 39 | 1 |
| MaintainArr | 35 | 0 |
| TrainArr | 35 | 2 |
| SupplyArr | 35 | 0 |
| LoadArr | 35 | 2 |
| AssurArr | 35 | 0 |
| CustomArr | 35 | 2 |
| OrdArr | 35 | 2 |
| RoutArr | 35 | 0 |
| Field Companion | 38 | 4 |
| ReportArr | 35 | 2 |
| LedgArr | 35 | 0 |

## Acceptance focus

- Pass all applicable R0 gates.
- Respect source-of-truth ownership.
- Prove the vertical slice rather than only rendering screens.
- Preserve evidence, source references, audit history, and reportability hooks.
- Keep UI unified, readable, non-noisy, and truthful in degraded states.

## Related roadmap files

- [../rollout-stages.md](../rollout-stages.md)
- [../release-gates-and-acceptance.md](../release-gates-and-acceptance.md)
- [../vertical-slice-backlog.md](../vertical-slice-backlog.md)
- [../reference/feature-rollout-map.csv](../reference/feature-rollout-map.csv)
- [../reference/workflow-rollout-map.csv](../reference/workflow-rollout-map.csv)

## R12 suite-stage summary

Status: Complete for the stage-gated product pass. R12 is the final named roadmap stage in the current roadmap layer. All fifteen suite products in the rollout order have completed an R12 pass, with advanced/category-depth capabilities either fixed where already represented or explicitly deferred where they would require new owner-backed contracts, durable models, portals, AI governance, or broad product expansion.

Completed products:

- NexArr — completed with advanced IAM expansion retained and misleading entitlement/access-language risks kept out of ordinary product copy.
- StaffArr — completed with advanced workforce analytics, portals, legal-hold awareness, AI, and optimization scope deferred.
- Compliance Core — completed with administrative UI remaining platform-admin-only and runtime/guidance kept available to tenant workflows.
- RecordArr — completed with deferred durable DMS blockers; advanced DMS/data-room/AI/offline/disaster-recovery reliance is not production-clear until the durable migration closes.
- MaintainArr — completed with advanced maintenance optimization, vendor collaboration, AI, and reliability depth retained as deferred R12 scope.
- TrainArr — completed with offline learning, skills development planning, AI generation, external academy, and category-depth workflow blockers deferred.
- SupplyArr — completed with advanced supplier network, optimization, contract intelligence, third-party risk, multi-tier mapping, dynamic discounting, and control-tower blockers deferred.
- LoadArr — completed with advanced WMS automation, labor/robotics, slotting, yard, marketplace, AI, and category-depth blockers deferred.
- AssurArr — completed with advanced QMS, SPC, FMEA, regulated signatures, digital quality passports, and no-code quality workflow blockers deferred.
- CustomArr — completed with advanced CRM/account intelligence, external portals, AI, privacy, quoting, and deeper commercial workflow blockers deferred.
- OrdArr — completed with advanced orchestration, customer/self-service, AI, portal, marketplace, and category-depth blockers deferred.
- RoutArr — completed with advanced transportation control tower, optimization, marketplace/shared capacity, carbon/alternative-energy, autonomous handoff, freight audit/dispute, and document orchestration blockers deferred.
- ReportArr — completed with advanced BI/AI, semantic layer/lakehouse, forecasting/anomaly, embedded analytics, scenario, catalog, privacy-preserving analytics, streaming intelligence, and model governance blockers deferred.
- Field Companion — completed with raw device/browser fingerprint exposure removed from diagnostics, clock source labels, and push metadata; advanced offline/MDM/voice/CV/remote-expert/geofence/AI blockers deferred.
- LedgArr — completed with canonical source-product badge labels and local runtime details removed from normal bootstrap copy; advanced finance portals, automation, reconciliation, treasury, tax/e-invoicing, AI, and category-depth blockers deferred.

Not-applicable products:

- STLComplianceSite has an R12 lane as public site/lead intake, but zero mapped feature rows and zero mapped workflow rows. No implementation pass was required in this stage-gated suite-product loop.

Shared fixes:

- No broad shared-platform refactor was introduced during the R12 stage. Product-stage fixes stayed in the current slice unless a product already depended on the shared behavior.
- Cross-product labeling, product reference copy, source-badge display, privacy-safe device classification, and local-runtime detail removal were handled only where encountered in product R12 slices.

Tests run:

- Product frontend unit tests and theme audits were run for the R12 slices touched in NexArr, StaffArr, Compliance Core, MaintainArr, TrainArr, SupplyArr, LoadArr, AssurArr, CustomArr, OrdArr, RoutArr, ReportArr, Field Companion, and LedgArr.
- Product backend/auth tests were run for the R12 slices that touched server-side behavior, including Compliance Core, MaintainArr, TrainArr, SupplyArr, LoadArr, AssurArr, CustomArr, OrdArr, RoutArr, ReportArr, Field Companion-related NexArr auth coverage, and LedgArr.
- RecordArr's R12 pass reran focused auth, OpenAPI, and frontend regression coverage while carrying the same durable DMS blocker explicitly rather than masking it with new advanced-DMS work.

Deferred blockers:

- RecordArr durable DMS migration remains the most significant carried blocker for production-grade evidence vault, data-room, offline encrypted capture, semantic retrieval, long-term preservation, and disaster-recovery reliance.
- Advanced R12 portals, AI assistance, optimization/control-tower depth, external collaboration, no-code configuration, analytics/forecasting, marketplace/network features, e-sign/trust services, MDM/offline depth, and category-specific enterprise features remain retained backlog until pulled forward with source owners, tenant/action permissions, persistence, audit, evidence, recovery, and UI discipline.

Suite-stage result: R12 is complete for the current roadmap layer. The suite has no named R13 stage in `docs/roadmap/README.md` or `docs/roadmap/rollout-stages.md`; any further work should be planned as a new roadmap stage or as explicitly selected R12 backlog slices.
