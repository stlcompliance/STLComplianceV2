# Roadmapped Release Stages

The rollout sequence is vertical first. Each release produces a provable operating loop or a necessary foundation for one.

## R0 — Trust gate and production truth

Make every already-visible surface truthful, tenant-scoped, permissioned, durable, testable, readable in light/dark, and honest in failure.

| Field | Definition |
| --- | --- |
| Entry condition | Existing docs and code are known to contain uneven maturity across products. |
| Exit condition | No production route relies on local success, fixture truth, process-global stores, anonymous unsafe access, misleading launch errors, or untested tenant boundaries. |
| Feature rows mapped | 0 |
| Workflow rows mapped | 0 |
| Product entry docs | Cross-suite gate or expansion backlog. |

### R0 is not optional

R0 blocks feature expansion wherever production truth is false. It specifically catches anonymous unsafe routes, missing tenant checks, process-global state, local-success UI paths, fixture-backed writes, misleading launch failures, incomplete browser-session hardening, inaccessible light/dark surfaces, and untested destructive actions.

## R1 — Foundation spine

Give every product a trustworthy way to identify users, tenants, people, locations, records, evidence, and shared reference data without shadow ownership.

| Field | Definition |
| --- | --- |
| Entry condition | R0 gates passing or explicitly tracked as release blockers. |
| Exit condition | Products can launch, authorize, reference people/locations/evidence/reference data, and expose shared page archetypes without local owner duplication. |
| Feature rows mapped | 65 |
| Workflow rows mapped | 37 |
| Product entry docs | [NexArr](products/nexarr.md), [StaffArr](products/staffarr.md), [RecordArr](products/recordarr.md) |

| Product | Role in this release | Must not violate |
| --- | --- | --- |
| NexArr | Foundation spine | Keep launch and authority truthful without recreating product entitlements for ordinary products. |
| StaffArr | Foundation spine and qualification gate | Remain the shared people/location authority while product actions stay owned by the product performing them. |
| RecordArr | Foundation evidence layer | Replace any in-memory/file-prototype truth before products rely on evidence persistence. |

## R2 — Compliance Core runtime baseline

Let products ask for applicability, required evidence, missing facts, and review outcomes without hardcoding regulatory meaning.

| Field | Definition |
| --- | --- |
| Entry condition | R1 identity, StaffArr context, RecordArr evidence references, and service-token patterns exist. |
| Exit condition | The first operational rule/evidence spine can produce unknown/conflict/missing/evidence-needed outcomes and bind to product workflows. |
| Feature rows mapped | 40 |
| Workflow rows mapped | 16 |
| Product entry docs | [Compliance Core](products/compliancecore.md) |

| Product | Role in this release | Must not violate |
| --- | --- | --- |
| Compliance Core | Compliance guidance baseline | Keep administrative authoring platform-admin-only while runtime guidance serves all products. |

## R3 — MaintainArr flagship operational slice

Prove that real operational work creates trustworthy compliance and maintenance evidence across products.

| Field | Definition |
| --- | --- |
| Entry condition | R1/R2 foundations exist enough for person/location/evidence/compliance calls. |
| Exit condition | Asset-to-work-to-return-to-service runs end-to-end with durable state, evidence, blockers, and explainable readiness. |
| Feature rows mapped | 59 |
| Workflow rows mapped | 16 |
| Product entry docs | [MaintainArr](products/maintainarr.md) |

| Product | Role in this release | Must not violate |
| --- | --- | --- |
| MaintainArr | First flagship operational slice | Prove asset-to-work-to-evidence without stealing inventory, training, quality, or document truth. |

## R4 — Training and qualification gate

Make person readiness and qualification checks real so operational work can be gated by training truth.

| Field | Definition |
| --- | --- |
| Entry condition | StaffArr people/incidents and MaintainArr work contexts can reference training requirements. |
| Exit condition | Products can check qualification truth; incidents can trigger retraining; renewed qualifications update readiness without local copies. |
| Feature rows mapped | 59 |
| Workflow rows mapped | 14 |
| Product entry docs | [TrainArr](products/trainarr.md) |

| Product | Role in this release | Must not violate |
| --- | --- | --- |
| TrainArr | Qualification and retraining gate | Own qualification truth while StaffArr owns people and incidents. |

## R5 — Procure, receive, put away, reserve, and issue

Connect maintenance and operating demand to procurement expectations, receiving, putaway, reservations, issues, and traceable inventory evidence.

| Field | Definition |
| --- | --- |
| Entry condition | Maintenace parts demand, StaffArr locations, RecordArr evidence, and platform reference data are available. |
| Exit condition | A part can be requested, ordered, received, inspected/excepted if needed, put away, reserved, issued, consumed, and traced. |
| Feature rows mapped | 73 |
| Workflow rows mapped | 29 |
| Product entry docs | [SupplyArr](products/supplyarr.md), [LoadArr](products/loadarr.md) |

| Product | Role in this release | Must not violate |
| --- | --- | --- |
| SupplyArr | Parts/procurement/inventory loop | Own commercial/procurement truth while LoadArr owns physical inventory and CustomArr owns customers. |
| LoadArr | Parts/procurement/inventory loop | Replace fixture/no-op/local-success behavior before any production inventory reliance. |

## R6 — Quality hold, release, and corrective action

Allow quality decisions to block or release assets, inventory, suppliers, orders, and records without taking over their source truth.

| Field | Definition |
| --- | --- |
| Entry condition | Affected products expose holdable/releasable references and evidence package hooks. |
| Exit condition | Quality holds and CAPA are permissioned, evidenced, auditable, and visibly block downstream operations until resolved. |
| Feature rows mapped | 33 |
| Workflow rows mapped | 14 |
| Product entry docs | [AssurArr](products/assurarr.md) |

| Product | Role in this release | Must not violate |
| --- | --- | --- |
| AssurArr | Quality hold and corrective action loop | Block and release via permissioned, evidenced quality decisions rather than shadow-owning affected records. |

## R7A — Customer master baseline

Provide customer accounts, contacts, locations, requirements, contracts, preferences, and eligibility before orders or dispatch consume customer truth.

| Field | Definition |
| --- | --- |
| Entry condition | StaffArr/RecordArr/Compliance Core foundations and user-facing CRM surfaces are durable enough for trusted customer onboarding. |
| Exit condition | Customer requirements can be queried by OrdArr, RoutArr, SupplyArr, AssurArr, ReportArr, and external portal workflows. |
| Feature rows mapped | 35 |
| Workflow rows mapped | 14 |
| Product entry docs | [CustomArr](products/customarr.md) |

| Product | Role in this release | Must not violate |
| --- | --- | --- |
| CustomArr | Customer master before order orchestration | Be the customer source of truth before OrdArr, RoutArr, or SupplyArr consumes customer requirements. |

## R7B — Order/request orchestration baseline

Turn customer/internal demand into owned execution handoffs, exceptions, completion packets, and bill-ready intent.

| Field | Definition |
| --- | --- |
| Entry condition | CustomArr customer truth and execution-product readiness contracts exist. |
| Exit condition | An order/request can prove who requested work, why it exists, what products own each step, what is blocked, and what completed. |
| Feature rows mapped | 31 |
| Workflow rows mapped | 13 |
| Product entry docs | [OrdArr](products/ordarr.md) |

| Product | Role in this release | Must not violate |
| --- | --- | --- |
| OrdArr | Order/request orchestration after customer master baseline | Explain why work is happening while execution products own how work is performed. |

## R8 — Dispatch and transportation execution

Route and dispatch work only after driver, equipment, customer, order, inventory, and compliance readiness are explainable.

| Field | Definition |
| --- | --- |
| Entry condition | Orders, customer requirements, asset readiness, training qualification, and inventory/dock context are available. |
| Exit condition | A dispatch can be planned, assigned, executed, excepted, completed, and traced back to source demand and readiness snapshots. |
| Feature rows mapped | 38 |
| Workflow rows mapped | 15 |
| Product entry docs | [RoutArr](products/routarr.md) |

| Product | Role in this release | Must not violate |
| --- | --- | --- |
| RoutArr | Dispatch and transportation execution | Dispatch only against explicit readiness snapshots from owning products. |

## R9 — Field Companion mobile execution

Put selected product actions in the field once owning APIs can enforce workflow, permissions, and idempotency.

| Field | Definition |
| --- | --- |
| Entry condition | Each mobile action has an owning product API, retry semantics, evidence rules, and clear blocked/degraded states. |
| Exit condition | Mobile/offline users can complete assigned work without Field Companion becoming a hidden source of truth. |
| Feature rows mapped | 33 |
| Workflow rows mapped | 13 |
| Product entry docs | [Field Companion](products/fieldcompanion.md) |

| Product | Role in this release | Must not violate |
| --- | --- | --- |
| Field Companion | Mobile execution after owning APIs are durable | Never become a mobile source of truth; replay all actions through owning APIs. |

## R10 — ReportArr operational reporting

Turn accumulated source events, evidence, and workflow history into reports without mutating source products.

| Field | Definition |
| --- | --- |
| Entry condition | Enough products emit source refs, events, evidence refs, status history, and read-model contracts. |
| Exit condition | Reports are exportable, schedulable, provenance-aware, and can store audit-ready outputs in RecordArr. |
| Feature rows mapped | 33 |
| Workflow rows mapped | 13 |
| Product entry docs | [ReportArr](products/reportarr.md) |

| Product | Role in this release | Must not violate |
| --- | --- | --- |
| ReportArr | Operational reporting after source events exist | ReportArr projects and explains source truth; it must not correct source truth. |

## R11 — LedgArr bridge-first finance

Govern financial handoff packets after operating loops produce reliable, auditable source evidence.

| Field | Definition |
| --- | --- |
| Entry condition | Orders, inventory, procurement, maintenance, quality, and dispatch produce trustworthy packet source records. |
| Exit condition | Bill-ready/invoice-ready/AP/AR/inventory valuation/fixed-asset packets can be reviewed, controlled, and bridged externally without absorbing operational truth. |
| Feature rows mapped | 40 |
| Workflow rows mapped | 20 |
| Product entry docs | [LedgArr](products/ledgarr.md) |

| Product | Role in this release | Must not violate |
| --- | --- | --- |
| LedgArr | Bridge-first finance after operating loops produce trustworthy packets | Start bridge-first; do not absorb operating truth or become a full ERP gravity well prematurely. |

## R12 — Expansion, portals, advanced integrations, AI, and category depth

Preserve every common, advanced, and widely unavailable feature while preventing it from disrupting the first shippable operating spine.

| Field | Definition |
| --- | --- |
| Entry condition | Earlier release trains have proven the suite can execute and explain real work. |
| Exit condition | Expansion features are pulled forward only when their source owners, release gates, and cross-product contracts are ready. |
| Feature rows mapped | 530 |
| Workflow rows mapped | 19 |
| Product entry docs | [STLComplianceSite](products/stlcompliancesite.md) |

| Product | Role in this release | Must not violate |
| --- | --- | --- |
| STLComplianceSite | Public presence and lead handoff once product promises match rollout reality | Market the staged product truth without promising unshipped operational loops. |

### R12 is retained scope, not a trash bin

R12 contains advanced, broad, or category-depth features that remain valid product goals. They should be pulled forward deliberately when they become necessary to complete a vertical slice or customer-ready release.
