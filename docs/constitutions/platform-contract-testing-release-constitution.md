# STL Compliance Contract, Testing, Migration, and Release Constitution

## 1. Purpose

This constitution defines how STL Compliance keeps multiple products aligned as APIs, events, migrations, permissions, dashboards, workflows, and UI surfaces evolve.

The suite can move fast, especially preproduction, but changes must still identify ownership, affected products, contract impact, and proof of alignment.

## 2. Scope

This constitution applies to:

- API contract tests
- Event schema tests
- Reference provider tests
- Service-token contract tests
- UI workflow tests
- Dashboard/read-model tests
- Migration/rebase policy
- Seed/reference data
- Release notes
- Breaking changes
- Cross-product integration tests

## 3. Prime directive

A product change that affects another product is not complete until the contract impact is known and tested.

Preproduction hard cutovers are allowed by project policy, but they must be intentional and traceable.

## 4. Contract types

Contract types include:

- API request/response shape
- API error shape
- Event schema
- Handoff schema
- Reference provider response
- Read model response
- Permission key
- Route/path contract
- External integration mapping
- Import CSV schema
- Export/report schema

## 5. API contract tests

APIs used by other products must have contract tests.

Contract tests should verify:

- Route/version
- Required request fields
- Required response fields
- Stable IDs
- Tenant behavior
- Permission behavior
- Error format
- Freshness/source metadata where applicable
- Idempotency for writes

## 6. Event schema tests

Cross-product events must have schema tests.

Tests should verify:

- Event name
- Schema version
- Required envelope fields
- Tenant ID
- Source product
- Source record type/ID
- Actor/correlation fields
- Payload required fields
- Backward compatibility or declared breaking change

## 7. Reference provider tests

Reference providers must prove:

- Tenant isolation
- Permission behavior
- Search behavior
- Stable ID return
- Display label return
- Archived/deprecated handling
- Source/freshness metadata where needed
- No free-text canonical reference creation

## 8. Service-token tests

Service-token flows must test:

- Correct scope required
- Missing/invalid token rejection
- Tenant scope
- Calling product identity
- User delegation when applicable
- Audit/correlation behavior
- Forbidden access paths

## 9. UI workflow tests

Primary workflows should test:

- Progressive create sections
- Draft behavior
- Controlled fields/reference selects
- Invalidated downstream sections
- Review/submit effects
- Detail source-of-truth labels
- Permission-aware rendering
- Loading/empty/error states
- No raw JSON to ordinary users

## 10. Dashboard/read-model tests

Dashboards and read models should test:

- Source provenance
- Freshness metadata
- Tenant isolation
- Permission-aware metrics
- Section-level errors
- Stale/source-unavailable state
- No frontend-only business rules
- Drill-in routes

## 11. Migration policy

Each product owns its database migrations.

No product may create foreign keys into another product's database.

Preproduction may allow:

- Destructive migrations
- Schema rebases
- Flattened migration baselines
- Hard cutovers
- Legacy/shadow model deletion

Production must use safe migration strategy unless explicitly approved.

## 12. Seed and reference data

Seed data must be deterministic.

Reference data must be loaded through approved import/catalog mechanisms.

Test/demo data must be clearly separated from production data.

No production feature should depend on fake dashboard/mock data.

## 13. Breaking changes

A breaking change must identify:

- Product making change
- Contract changed
- Affected products
- Required code changes
- Migration/data impact
- Permission impact
- Event/read-model impact
- UI route/workflow impact
- Cutover plan
- Tests updated

Preproduction can choose hard cutover, but not silent drift.

## 14. Release notes

Release notes for material changes should call out:

- Ownership changes
- API changes
- Event changes
- Permission changes
- Lifecycle/status changes
- Reference/catalog changes
- Dashboard/reporting changes
- External integration changes
- Migration/rebase actions
- Known degraded areas

## 15. Cross-product integration tests

High-value integration flows should have end-to-end tests or contract suites.

Examples:

- StaffArr incident → TrainArr retraining evaluation
- MaintainArr parts demand → LoadArr fulfillment → MaintainArr usage
- RoutArr trip dispatch → StaffArr/TrainArr/MaintainArr/LoadArr readiness checks
- SupplyArr procurement → LoadArr receiving → RecordArr documents
- AssurArr CAPA → MaintainArr corrective work → RecordArr evidence → ReportArr status
- Compliance Core evidence requirements → product evidence capture → RecordArr storage

## 16. Route and shell tests

Shared shell/product route changes must prove:

- NexArr launch/handoff still works
- Product switcher shows every active ordinary product to active tenant members and restricts Compliance Core studio to platform admins
- Tenant context is preserved
- Unauthorized product actions and data access are blocked
- Canonical detail/create routes still resolve

## 17. Test data safety

Tests must not require production tenant data.

Use fixtures, seed data, synthetic tenants, and deterministic IDs.

Mock external integrations must clearly indicate mock mode.

## 18. Anti-patterns

The following are not allowed:

- Cross-product API changes with no contract tests
- Event payload drift with no schema/version change
- Frontend route changes that break NexArr launch/handoff
- Migrations that add cross-database foreign keys
- Production dashboard logic backed by fake data
- Silent permission key changes
- Reference provider changes that allow free-text canonical records
- Release notes that omit cross-product impact

## 19. Minimum acceptable implementation

A material platform/product change is minimally acceptable when it has:

1. Ownership impact identified
2. Contract impact identified
3. Affected products listed
4. Tests updated
5. Migration/rebase decision documented
6. Permission/security impact checked
7. Event/read-model impact checked
8. Release note or implementation note
9. No silent cross-product drift
