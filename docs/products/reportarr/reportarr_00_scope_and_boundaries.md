# ReportArr — Scope, Ownership, and Boundaries

## Product purpose

ReportArr is the reporting, dashboard, analytics, KPI, scheduled-report, export, and audit-report product for the STL Compliance / ARR suite.

ReportArr is not the operational source of truth. It consumes events, read APIs, snapshots, and product facts from source products and builds useful read models, dashboards, reports, exports, and analytical views.

ReportArr answers:

- What is the current cross-suite status?
- Which assets, people, orders, inventory, routes, quality issues, training items, or compliance items need attention?
- What KPIs are improving or worsening?
- What reports are scheduled?
- What report exports exist?
- What data is stale?
- Which source product produced each fact?
- Which source object should the user drill into?
- What is audit readiness across a scope?

## ReportArr owns

```text
- Report definitions
- Dashboard definitions
- Dashboard widgets
- KPI definitions
- Metric definitions
- Analytics/read models
- Dataset definitions
- Dataset refresh state
- Product event ingestion state
- Cross-product report runs
- Scheduled report delivery
- Export jobs
- Generated report output references
- Audit report packages
- Drilldown definitions
- Report access policies
- Data freshness indicators
- Source traceability
```

## ReportArr does not own

```text
- Platform login
- Tenant entitlement
- Person master
- Permission assignment truth
- Training completion truth
- Certification truth
- Regulatory/rulepack meaning
- Asset truth
- Work order truth
- Inventory balance
- Stock ledger
- Supplier/vendor master
- Procurement truth
- Route/trip execution truth
- Customer master
- Order lifecycle
- Document/file storage truth
- Quality hold/release truth
- Mobile task execution truth
- Accounting execution
```

## External product dependencies

```text
NexArr
- Product entitlement
- Login/handoff
- Service tokens
- Platform audit and access events

StaffArr
- People, org, sites, locations, permissions, incidents, readiness snapshots

TrainArr
- Training assignments, completions, qualifications, expirations, remediation

Compliance Core
- Rulepacks, compliance evaluations, requirement status, missing evidence, audit scope

MaintainArr
- Assets, defects, inspections, PMs, work orders, downtime, parts demand

LoadArr
- Inventory balances, receipts, putaway, reservations, picks, issues, counts, adjustments

SupplyArr
- Suppliers, purchase requests, purchase orders, sourcing, supplier status

RoutArr
- Routes, trips, stops, ETAs, delivery proof, transportation exceptions

CustomArr
- Customers, customer contacts, customer locations, customer requirements/issues

OrdArr
- Requests, orders, order lines, fulfillment dependencies, blockers, closure

RecordArr
- Generated report files, evidence packages, document status, record packages

AssurArr
- Nonconformance, quality holds, CAPA, audits, findings, quality scorecards

Field Companion
- Mobile task/action completion, sync failures, offline action metrics, capture metrics
```

## Core source-of-truth rules

```text
1. ReportArr owns reporting read models, not operational facts.
2. Source products remain authoritative for their domains.
3. ReportArr must preserve source product and source object traceability.
4. ReportArr must expose data freshness and source timestamps.
5. ReportArr should not mutate source product operational state.
6. ReportArr may request report exports and store generated files in RecordArr.
7. Compliance Core owns compliance meaning; ReportArr displays/report it.
8. RecordArr owns report output files.
9. NexArr/StaffArr control access to reporting surfaces.
10. ReportArr can aggregate across products but cannot become a shadow operational system.
```

## Standard ReportArr object envelope

```text
ReportArrObject
- id
- tenantId
- objectNumber
- objectType
- status
- title
- description
- ownerPersonId
- sourceProductRefs
- sourceDatasetRefs
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
- lastRunAt
- lastRefreshedAt
- auditTrail
- eventLog
```

## ReportArr object prefixes

```text
DS      Dataset
RM      Read model
SRC     Source connector
ING     Ingestion cursor
REF     Refresh job
KPI     KPI definition
MET     Metric definition
DASH    Dashboard
WID     Dashboard widget
RPT     Report definition
RUN     Report run
SCH     Report schedule
EXP     Export job
AUDR    Audit report package
DRL     Drilldown definition
ALRT    Reporting alert
FRESH   Data freshness status
```

## Standard source traceability

```text
SourceTrace
- sourceProduct
- sourceObjectType
- sourceObjectId
- sourceObjectNumber
- sourceEventId
- sourceEventType
- sourceUpdatedAt
- ingestedAt
- sourceStatusSnapshot
- displayNameSnapshot
```

## Data freshness rule

Every dashboard/report/read model should show or carry freshness metadata.

```text
FreshnessStatus
- fresh
- slightly_stale
- stale
- failed
- rebuilding
- unknown
```

## Standard report access rule

ReportArr should enforce:

```text
- NexArr product entitlement
- StaffArr person identity
- StaffArr permissions/roles
- Report-specific access policy
- Source-product sensitivity when required
- RecordArr access policy for generated exports
```
