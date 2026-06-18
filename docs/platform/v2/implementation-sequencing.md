# STL Compliance V2 Implementation Sequencing

## Purpose

This document defines a practical V2 build order that respects existing product ownership boundaries.

The sequence favors foundation first, then execution loops, then external access, analytics, and AI assistance.

## Guiding principles

```text
1. Do not add new products unless ownership cannot fit existing products.
2. Keep one source of truth per business fact.
3. Build cross-product contracts before building elaborate UI.
4. Build review and audit before automation.
5. Build minimal published reference data early.
6. Keep payroll, banking, and specialized certified systems external; route financial execution through LedgArr and use external accounting/ERP only through LedgArr bridge modes.
7. Field Companion comes after product APIs can enforce workflow and permission rules.
8. ReportArr comes after enough source events/read models exist.
```

## Phase 0 — Alignment gate

Before V2 work starts:

- Product keys are normalized.
- Constitutions are adopted.
- Each product has a clear API namespace.
- Service-token scopes are named by product key.
- No product directly joins another product database.
- Each product exposes health/version endpoints.
- Current docs compile into a navigable index.

Exit criteria:

```text
- Product registry includes referencedatacore.
- Existing platform constitutions are present.
- V2 addendum constitutions are accepted.
- Duplicate compiled docs are not treated as canonical over granular docs.
```

## Phase 1 — Identity, authority, records, and reference foundations

Build or harden:

```text
NexArr
- login
- tenant membership
- entitlement
- platform admin validation
- product launch/handoff
- service-client token issuance

StaffArr
- person references
- role/permission assignment context
- internal org/location hierarchy
- teams/positions
- incident intake baseline

RecordArr
- stored records/files
- document metadata
- evidence mapping shell
- package shell
- retention baseline

ReferenceDataCore
- dataset model
- lookup API
- UPC/GTIN, VIN, manufacturer, brand, UOM shell
- import staging/review/publish baseline
```

Exit criteria:

```text
- Products can identify people, locations, records, and reference data without local shadow owners.
- Product APIs can validate user/service authority.
- Stored files and evidence references do not live inside random product tables as file truth.
```

## Phase 2 — Compliance Core baseline

Build or harden:

```text
Compliance Core
- governing body/catalog model
- citation/rulepack model
- requirement/applicability logic
- evidence requirement model
- TSE/import mapping
- questionnaire engine baseline
```

Exit criteria:

```text
- Products can ask Compliance Core for requirement/evidence guidance.
- Compliance Core can produce missing/unknown/conflict outcomes.
- Products do not hardcode regulatory conclusions.
```

## Phase 3 — MaintainArr operational loop

Build or harden:

```text
MaintainArr
- asset registry
- component model
- asset create/detail routes
- defect reports
- inspection templates/executions
- PM plans/occurrences
- work orders
- asset readiness
- parts demand handoff to LoadArr/SupplyArr
```

Exit criteria:

```text
- Asset readiness is MaintainArr-owned.
- Work orders can request parts without owning inventory.
- Inspection/defect evidence stores in RecordArr.
- Qualification checks can call StaffArr/TrainArr context when needed.
```

## Phase 4 — TrainArr qualification loop

Build or harden:

```text
TrainArr
- program builder
- assignments
- signoffs
- evaluations
- certificates/qualifications
- remediation/renewals
- StaffArr incident handoff
```

Exit criteria:

```text
- TrainArr owns qualification truth.
- StaffArr owns personnel record/history.
- Products can check whether a person is qualified.
```

## Phase 5 — SupplyArr and LoadArr procure-to-receive loop

Build or harden:

```text
SupplyArr
- supplier/vendor/dealer master
- item sourcing and internal SKU context
- purchase request/order
- supplier compliance/performance

LoadArr
- item/location/balance model
- receiving
- putaway
- reservation/pick/issue/transfer
- counts/adjustments/discrepancies
```

Exit criteria:

```text
- SupplyArr owns procurement context.
- LoadArr owns inventory balance and stock ledger.
- ReferenceDataCore supports item identity/UOM lookup.
- MaintainArr can request parts without owning inventory.
```

## Phase 6 — AssurArr holds and CAPA

Build or harden:

```text
AssurArr
- nonconformance
- hold/release
- CAPA actions
- audit findings
- complaints
- quality status scorecards
```

Exit criteria:

```text
- Quality holds can block inventory, orders, suppliers, assets, or releases.
- Hold release is permissioned, evidenced, and audited.
- CAPA can request work from owning products instead of owning their records.
```

## Phase 7 — OrdArr orchestration

Build or harden:

```text
OrdArr
- order/request intake
- order lifecycle
- handoffs to LoadArr/RoutArr/MaintainArr/SupplyArr
- exception coordination
- completion packets
- invoice-ready/bill-ready packets for LedgArr
```

Exit criteria:

```text
- OrdArr explains why work is happening.
- Execution products own how work is performed.
- Financial packets are prepared but accounting execution remains external.
```

## Phase 8 — CustomArr customer requirements

Build or harden:

```text
CustomArr
- customer accounts
- contacts
- locations
- requirements/contracts/preferences
- onboarding review
- eligibility checks
```

Exit criteria:

```text
- Customer truth is central.
- Order and dispatch decisions can check customer requirements.
- Customer contacts are ready for scoped portal access.
```

## Phase 9 — RoutArr dispatch/trip execution

Build or harden:

```text
RoutArr
- dispatch
- route/trip
- stop/proof/exception
- driver/equipment compliance snapshots
- dock appointment/load visibility
```

Exit criteria:

```text
- RoutArr owns dispatch/trip execution.
- RoutArr checks driver, qualification, equipment, order, customer, and inventory readiness.
- RoutArr can consume vendor completion status before dispatch when applicable.
```

## Phase 10 — Field Companion mobile execution

Build or harden:

```text
Field Companion
- assigned work surface
- mobile task sessions
- secure upload/capture
- offline sync
- product action surfaces
```

Exit criteria:

```text
- Mobile app does not own source records.
- Offline actions replay through owning product APIs.
- Blocked/retry states are clear to the user.
```

## Phase 11 — ReportArr read models

Build or harden:

```text
ReportArr
- datasets/read models
- dashboards/widgets
- scheduled reports
- KPI/metrics
- provenance drillbacks
```

Exit criteria:

```text
- ReportArr can read/project but not correct source truth.
- Reports show source product and source record references.
- Audit-ready outputs can be stored in RecordArr.
```

## Phase 12 — External access and integrations

Build or harden:

```text
External portal access
- customer status
- vendor completion update
- supplier document upload
- carrier status update
- auditor package access

Integrations
- inbound webhook intake
- outbound webhook delivery
- review queues
- external ID mappings
- retry/dead-letter handling
```

Exit criteria:

```text
- External actors have scoped access only.
- Integration failures create reviewable work.
- External systems remain external unless explicitly replaced.
```

## Phase 13 — AI-assisted intake and review

Build or harden:

```text
AI assistance
- upload classification
- field extraction candidates
- evidence mapping suggestions
- questionnaire answer suggestions
- conflict detection
- reviewable record proposals
```

Exit criteria:

```text
- AI produces proposals, not final records.
- All proposals include source references and confidence.
- Product owners approve or reject final effects.
```

## Hard stop conditions

Do not proceed to deeper automation if:

```text
- product ownership is unclear
- permission/action matrix is missing for risky actions
- cross-product handoffs are not idempotent
- blockers cannot be explained to users
- external portal access is not scoped/expiring/audited
- AI suggestions cannot cite source material
- events cannot be replayed safely
```
