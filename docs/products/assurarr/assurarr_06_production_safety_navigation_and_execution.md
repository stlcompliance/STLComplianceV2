# AssurArr — Production Safety, Navigation, and Quality Execution

## Audit mandate

AssurArr cannot be accepted until every quality route is authenticated, tenant-scoped, permissioned, durable, and tested. Remove every hard-coded tenant and apply the endpoint authorization-map constitution to all domain and integration routes.

## Required permissions

At minimum separate: quality read, nonconformance manage, hold place/release, disposition approve, CAPA manage, CAPA close/effectiveness approve, audit plan/conduct, finding manage/close, complaint manage, supplier quality manage, scorecard read/admin, integration read/write, and settings admin.

## Durable lifecycle

Nonconformance, containment, hold, disposition, root cause, CAPA action, verification, audit, finding, complaint, supplier issue, and release decisions are tenant-owned durable aggregates with server state machines, concurrency, immutable timelines, evidence references, and outbox events.

## Navigation

Use grouped navigation:

- Quality Work: Dashboard, Nonconformances, Holds, CAPA, Complaints
- Audits: Audit Program, Audits, Findings
- Quality Controls: Inspections, Sampling, Supplier Quality, Scorecards
- Records and Analysis: Reports, Trends, History
- Administration: Settings, Integrations, Permissions

Do not expose every leaf as a flat primary route.

## Primary pages

Each primary record has list, drawer, detail, create/edit or guided workflow, lifecycle actions, documents/evidence, related records, history, and print/report. Closure/release pages show missing evidence, approvals, and downstream blockers in a decision panel.

## Cross-product effects

Quality holds reference LoadArr inventory/locations, SupplyArr items/suppliers, MaintainArr assets/work, OrdArr orders, and RecordArr evidence through owner-backed references. AssurArr owns the quality decision; owners enforce resulting execution blocks.
