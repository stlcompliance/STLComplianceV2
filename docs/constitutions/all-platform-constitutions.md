# STL Compliance Platform Constitutions Bundle

Generated bundle of platform constitutions.


---

<!-- platform-accessibility-time-localization-human-factors-constitution.md -->


# STL Compliance Accessibility, Time, Localization, and Human Factors Constitution

## 1. Purpose

This constitution defines human factors that must be consistent across STL Compliance: accessibility, time handling, localization readiness, units of measure, readable language, and field-safe interaction.

Regulated operational software must be hard to misuse, readable under pressure, and clear across locations and time zones.

## 2. Scope

This constitution applies to:

- UI accessibility
- Forms
- Tables/cards
- Status labels
- Date/time display
- Time zones
- Units of measure
- Localization readiness
- Plain-language validation
- High-risk confirmations
- Mobile/touch ergonomics
- Keyboard/focus behavior
- Charts and visual indicators

## 3. Prime directive

Critical operational meaning must never rely on color, icon, hover, relative time, abbreviation, or tribal knowledge alone.

## 4. Accessibility baseline

All primary workflows must support:

- Sufficient text contrast
- Keyboard navigation where applicable
- Visible focus states
- Readable status text
- Proper labels for inputs
- Meaningful table headers
- Accessible button/link labels
- Non-color status indicators
- Error messages tied to fields
- Reduced reliance on animation

## 5. Color and status

Color may reinforce meaning but must not be the only way to understand state.

Every status badge should include readable text.

Examples:

- `Blocked`
- `Ready`
- `Expired`
- `Needs review`
- `Pending sync`
- `Source unavailable`

## 6. Time storage

System timestamps should be stored in UTC unless a specific domain requires another representation.

The original source timezone/site timezone should be preserved when it affects business meaning.

## 7. Time display

UI must make time basis clear.

Show one or more of:

- User local time
- Site local time
- Route/terminal time
- UTC where needed for audit/admin
- Exact timestamp on hover/tap or secondary metadata

Do not display ambiguous dates for time-sensitive or audit-relevant records.

## 8. Relative time

Relative time is allowed only with enough context.

Good:

- `Due in 2 hours — Jun 10, 2026, 3:00 PM CDT`
- `Updated 8 minutes ago — source: LoadArr`

Bad:

- `Soon`
- `Recently`
- `Overdue` without due date where the due date matters

## 9. Date ranges and comparisons

Dashboards, reports, and metrics must state the comparison period.

Good:

- `up 8% vs last 7 days`
- `12 overdue as of Jun 10, 2026`
- `next 30 days`

Bad:

- `up 8%`
- `trending up`
- `better`

## 10. Units of measure

Units must be explicit.

Examples:

- Miles vs kilometers
- Hours vs days
- Pounds vs kilograms
- Gallons vs liters
- Each vs case vs pallet
- Fahrenheit vs Celsius

A numeric value without a unit is incomplete when unit matters.

## 11. Localization readiness

Even if English is the initial language, UI should avoid hardcoded date formats, number formats, currency formats, and unit assumptions where practical.

Text should be organized so future translation is possible.

Avoid embedding business logic in translated strings.

## 12. Plain-language validation

Validation messages should explain the problem and correction.

Good:

- `Select a StaffArr site before choosing a parts room.`
- `This driver cannot be assigned because required qualification Forklift Operator expires before the trip date.`

Bad:

- `Invalid field`
- `Error 400`
- `Rule failed`

## 13. High-risk confirmations

High-risk actions must use confirmation language that states the business effect.

Examples:

- `Dispatch trip`
- `Close work order`
- `Approve supplier`
- `Release inventory hold`
- `Publish rulepack`
- `Archive person record`
- `Send financial handoff`

Avoid vague buttons like `Done`, `OK`, or `Continue` for material state changes.

## 14. Mobile ergonomics

Mobile must use:

- Large tap targets
- Sticky primary actions where helpful
- Sheets instead of tiny modals
- Cards instead of dense tables
- No hover-only behavior
- Clear offline/sync state
- Camera/upload/signature flows designed for touch

## 15. Field-safe workflows

Field workflows should account for:

- Gloves
- Bad lighting
- Noise
- Interrupted work
- Weak connectivity
- Shared devices where policy allows
- Need to save draft/resume
- Large checklists
- Quick scan/search

## 16. Charts and data visualization

Charts must include readable labels or summaries.

Charts must not be the only way to understand a critical state.

A chart with operational effect should include drill-in or supporting table where practical.

## 17. Abbreviations and jargon

Use common industry abbreviations only where users are expected to understand them.

Where ambiguity exists, provide expanded labels or help text.

Examples:

- `PM` may mean preventive maintenance.
- `POD` may mean proof of delivery.
- `DQF` may mean driver qualification file.

Do not expose internal engineering jargon to ordinary users.

## 18. Anti-patterns

The following are not allowed:

- Color-only risk/status
- Hover-only critical information
- Tiny touch targets for field workflows
- Ambiguous dates in audit/compliance contexts
- Numbers without units where unit matters
- Generic error messages for user-correctable problems
- Vague labels for high-risk actions
- Raw enum/internal code shown as primary status
- Dense desktop table copied directly to mobile

## 19. Minimum acceptable implementation

A human-facing feature is minimally acceptable when it has:

1. Text-readable status
2. Accessible labels and focus behavior
3. Clear time basis
4. Explicit units where needed
5. Plain-language validation
6. Safe high-risk confirmation labels
7. Mobile-safe behavior when used on mobile
8. No critical meaning hidden behind color/hover alone


---

<!-- platform-api-integration-constitution.md -->


# STL Compliance Platform API / Integration Constitution

## 1. Purpose

The platform API and integration constitution defines how STL Compliance products communicate without violating product ownership.

The suite is intentionally distributed. Each product may have its own database, domain model, permissions, workflows, and release cycle. Integration must allow the suite to feel unified while preserving one owner per business truth.

## 2. Scope

This constitution applies to:

- Product APIs
- Suite frontend API calls
- Internal product-to-product APIs
- Service-token APIs
- Reference providers
- Handoff endpoints
- External system integration endpoints
- Worker/job APIs
- Reporting/read-model APIs
- Import and catalog APIs

It does not define database tables, UI component style, or product-specific domain behavior except where required for integration safety.

## 3. Prime Directive

A product may expose, request, consume, mirror, snapshot, or report on another product's record.

A product must not silently become the source of truth for another product's record.

All cross-product integration must happen through approved APIs, events, service-token workflows, handoffs, read models, mirrors, or snapshots.

Direct database reads across products are forbidden.

Direct cross-database foreign keys are forbidden.

## 4. API ownership

The product that owns a record owns the canonical API for that record.

Examples:

- StaffArr owns canonical people, worker authority, org structure, and internal locations APIs.
- TrainArr owns training programs, training assignments, evaluations, certifications, and qualification APIs.
- MaintainArr owns asset, work order, PM, inspection, defect, and maintenance readiness APIs.
- RoutArr owns route, trip, dispatch, stop, ETA, and transportation exception APIs.
- SupplyArr owns supplier, vendor, item, material, part, procurement, and purchasing-context APIs.
- LoadArr owns receiving, putaway, inventory balance, stock ledger, reservation, pick, issue, and warehouse movement APIs.
- CustomArr owns customer, customer contact, customer requirement, and customer relationship APIs.
- OrdArr owns order/request orchestration APIs.
- RecordArr owns document, file, record package, controlled document, versioning, retention, and access-history APIs.
- AssurArr owns nonconformance, CAPA, quality hold decision, release approval, and assurance case APIs.
- ReportArr owns report definition, report schedule, analytics, KPI view, export, and report snapshot APIs.
- Compliance Core owns rulepack, governing body, applicability, evidence requirement, mapping, exemption, exception, and evaluation APIs.
- NexArr owns login, tenant, entitlement, launch, platform admin, service clients, service tokens, and handoff-session APIs.

## 5. API classes

APIs must be classified so consumers know how to use them.

### 5.1 Product domain APIs

Product domain APIs operate on the product's owned records.

They may be called by the suite frontend, product-specific UI, product workers, or approved service clients.

### 5.2 Integration APIs

Integration APIs are designed for other STL products.

They should be stable, narrower, and safer than internal domain APIs.

Recommended path style:

- `/api/v1/integrations/{resource}`
- `/api/v1/integrations/{resource}/{id}`
- `/api/v1/integrations/{resource}/{id}/readiness`
- `/api/v1/integrations/{resource}/{id}/references`
- `/api/v1/integrations/{resource}/{id}/handoffs`

### 5.3 Reference provider APIs

Reference provider APIs power controlled selects, search, lookup, and validation for cross-product fields.

They must return stable identifiers, display labels, source product, status, archival state, and freshness where relevant.

### 5.4 Handoff APIs

Handoff APIs allow one product to request action from another product without owning the target product's workflow.

They must use explicit handoff states and idempotency.

### 5.5 Read-model APIs

Read-model APIs expose purpose-built summaries, dashboard data, reporting slices, and operational views.

They must show source and freshness. They must not pretend to be canonical source APIs unless the product owns the underlying truth.

### 5.6 External integration APIs

External integration APIs connect STL to QuickBooks, ERP, ELD, telematics, payroll, CRM, supplier APIs, carrier APIs, government APIs, or other outside systems.

They must preserve external ownership and expose mapping, sync status, direction, last successful sync, and last error where applicable.

## 6. Required request context

Every non-public authenticated API request must resolve:

- Tenant
- Actor
- Entitlement
- Product context
- Permission context
- Correlation ID
- Request source

Human requests must resolve a `personId` when the actor is a human.

Service requests must resolve:

- Service client ID
- Calling product
- Target product
- Tenant
- Scope
- Reason or operation
- Correlation ID

## 7. Required response metadata

Cross-product and dashboard/read-model API responses should include metadata sufficient to explain trust, freshness, and ownership.

Recommended metadata:

```json
{
  "meta": {
    "tenantId": "...",
    "sourceProduct": "MaintainArr",
    "resourceType": "asset",
    "resourceId": "...",
    "schemaVersion": "1.0",
    "fetchedAt": "2026-06-10T00:00:00Z",
    "freshness": "live|near_live|cached|stale|snapshot|unknown",
    "correlationId": "..."
  }
}
```

Responses that combine data from multiple products must identify each source, not only the aggregator.

## 8. Stable identifiers

APIs must use stable identifiers for cross-product references.

Rules:

- Human references use `personId`.
- Internal location references use StaffArr location/org identifiers.
- Tenant references use NexArr tenant identifiers.
- Product-owned business records use the owning product's stable ID.
- External IDs are mappings, not STL canonical IDs.
- Display labels are not identifiers.
- Human-readable numbers may be operational identifiers, but they do not replace stable IDs unless explicitly designed as the stable ID.

## 9. Write behavior

State-changing API calls must be explicit and auditable.

Required for state-changing calls:

- Authentication and tenant validation
- Entitlement validation through NexArr
- Product-local permission validation
- Idempotency key for creates/submits/handoffs/retries
- Business validation
- Source-of-truth validation
- Audit event or activity event where appropriate
- Plain-language error response on failure

A write API must not hide cross-product effects.

Examples of cross-product effects:

- Assigning training
- Creating a parts demand
- Reserving inventory
- Moving inventory
- Opening a work order
- Dispatching a trip
- Creating an assurance case
- Publishing a rule
- Creating a record package
- Sending a notification
- Starting an approval

## 10. Idempotency

Creates, submits, publishes, approvals, handoffs, external writebacks, and background retries must support idempotency.

Idempotency must be scoped by:

- Tenant
- Product
- Operation
- Actor or service client
- Idempotency key

Retries must not create duplicate business records, duplicate notifications, duplicate handoffs, duplicate files, duplicate external invoices, or duplicate inventory movements.

## 11. Pagination, sorting, filtering, and search

List APIs must support predictable query behavior.

Recommended conventions:

- `limit`
- `cursor` or `pageToken`
- `sort`
- `direction`
- `status`
- `q`
- `siteId` or StaffArr location reference where applicable
- `from` / `to` for date ranges
- `includeArchived`

Large operational records must not be returned unbounded.

Search APIs must return source product, resource type, stable ID, label, status, and canonical detail route when possible.

## 12. Validation APIs

When a UI depends on cross-product readiness, the owning product must expose validation or readiness endpoints rather than forcing the frontend to invent logic.

Examples:

- MaintainArr exposes asset readiness to RoutArr.
- TrainArr exposes qualification status to StaffArr, RoutArr, and Field Companion.
- LoadArr exposes inventory availability to MaintainArr and OrdArr.
- Compliance Core exposes evidence requirement and applicability evaluation to products.
- StaffArr exposes person authority and location validity to execution products.

## 13. Error format

Errors must be business-readable.

Recommended fields:

```json
{
  "error": {
    "code": "ASSET_NOT_READY",
    "message": "Vehicle TRK-1042 is blocked for dispatch because inspection clearance is expired.",
    "severity": "blocked",
    "sourceProduct": "MaintainArr",
    "target": {
      "resourceType": "asset",
      "resourceId": "..."
    },
    "retryable": false,
    "correlationId": "..."
  }
}
```

Do not expose raw stack traces, service-token claims, database errors, or raw rule JSON to ordinary users.

## 14. Versioning

All stable APIs must be versioned.

Recommended path style:

- `/api/v1/...`

Breaking changes require a new version or a coordinated hard cutover when the system is still preproduction.

Preproduction may allow destructive contract changes, but the change must still identify affected products and tests.

## 15. Service-token calls

Service-token calls must be least-privilege.

A service token must identify:

- Calling product
- Target product
- Tenant scope
- Allowed scopes
- Expiration/rotation policy
- Whether user delegation is present
- Whether the call is system-initiated or user-initiated

No product may use a broad service token as a backdoor around product-local permissions or ownership rules.

## 16. External API protection

External API integration credentials must be stored and used through approved integration services.

Products must not scatter vendor credentials through local configuration.

External writebacks must be explicit, idempotent, logged, and traceable to the STL source action.

## 17. Anti-patterns

The following are not allowed:

- Direct joins across product databases
- Foreign keys into another product's database
- Frontend-only cross-product business logic
- Free-text references to canonical records
- Product-local human identity replacing `personId`
- One product mutating another product's records without an approved API or handoff
- Unbounded list APIs
- Silent external writebacks
- API responses that mix source data without provenance
- Error messages that leak tenant data or raw internals
- Service tokens with broad unreviewed access

## 18. Minimum acceptable implementation

A cross-product API is minimally acceptable when it has:

1. Clear owning product
2. Stable versioned route
3. Tenant validation
4. NexArr entitlement validation
5. Product-local authorization where applicable
6. Stable identifiers
7. Idempotency for writes
8. Source and freshness metadata for cross-product/read-model responses
9. Business-readable errors
10. Audit/activity event for material state changes


---

<!-- platform-audit-evidence-retention-constitution.md -->


# STL Compliance Audit, Activity, Evidence, and Retention Constitution

## 1. Purpose

This constitution defines how STL Compliance records what happened, stores evidence, preserves document history, and applies retention without confusing activity feeds, audit logs, operational records, and stored files.

## 2. Scope

This constitution applies to:

- Activity feeds
- Audit logs
- Security audit
- Evidence files
- Document metadata
- Record packages
- Retention schedules
- Legal holds
- Versioning
- Access history
- Exported report artifacts
- Compliance evidence classification

## 3. Prime directive

Activity is for humans to understand recent change.

Audit is for accountability and traceability.

Evidence is for supporting a requirement, decision, or record.

Retention is for preserving or disposing records according to policy.

Do not collapse all four into generic notes.

## 4. Ownership

RecordArr owns:

- Stored files
- Document metadata
- File versions
- Controlled documents
- Record packages
- Retention schedules
- Legal holds
- Document approvals
- Evidence file storage
- Document access history

Compliance Core owns:

- Evidence requirements
- Evidence meaning
- Applicability
- Rule/evidence classification
- Compliance gap analysis

Products own:

- Operational records
- Domain activity
- Domain decisions
- Requests to attach/store evidence
- References to RecordArr documents

ReportArr owns:

- Report definitions
- Rendered report artifacts before storage
- Scheduled report generation
- Report subscriptions

RecordArr owns stored report artifacts once they are retained as records.

## 5. Activity feed

Activity feeds are user-facing summaries of recent change.

Activity items should include:

- Actor or source
- Plain-language action
- Target record
- Timestamp
- Source product
- Optional status/severity
- Drill-in link

Activity feeds must not expose raw event payloads, raw rule JSON, service-token claims, or database rows.

Activity feeds are not a complete immutable audit log unless explicitly designed and protected as one.

## 6. Audit log

Audit logs preserve accountability.

Audit records should include:

- Tenant ID
- Event/action ID
- Actor type
- Actor ID, using `personId` when human
- Service client when service-initiated
- Source product
- Target product when cross-product
- Target record type
- Target record ID
- Action
- Timestamp in UTC
- Correlation ID
- Outcome
- Before/after summary when appropriate
- Reason/justification when required
- IP/device/session context where appropriate and allowed

Audit logs should be tamper-evident or protected from ordinary user mutation.

## 7. Security audit

Security-sensitive events require stronger audit treatment.

Examples:

- Login
- Failed login
- Tenant switch
- Product launch
- Permission assignment
- Service token creation/rotation/revocation
- Break-glass activation
- Sensitive document access
- External credential changes
- Export/download of sensitive data
- Legal hold change

## 8. Evidence

Evidence is a document, file, record, signature, photo, inspection result, external status, or product event that supports a rule, requirement, decision, or audit package.

Evidence must have:

- Evidence ID or RecordArr document/record ID
- Source product
- Owning business record when applicable
- Evidence type
- Status
- Uploaded/captured/generated time
- Actor or service source
- Linked requirement when applicable
- Effective date when applicable
- Expiration date when applicable
- Review status when applicable

## 9. Evidence states

Recommended evidence states:

- `current`
- `expiring_soon`
- `expired`
- `pending_review`
- `approved`
- `rejected`
- `superseded`
- `missing`
- `not_applicable`
- `source_unavailable`

Evidence state must be readable in UI and reports.

## 10. RecordArr storage rule

Files that become records or evidence must be stored through RecordArr or a RecordArr-controlled storage path.

Products may upload, attach, capture, or request files, but RecordArr owns:

- Storage identity
- File version
- Metadata
- Access history
- Retention
- Legal hold
- Controlled document lifecycle where applicable

## 11. Compliance Core classification rule

Compliance Core determines what evidence is required and what evidence means for rules.

Products may collect evidence, but they do not invent regulatory meaning locally when Compliance Core owns that interpretation.

## 12. Versioning

Controlled documents and record packages must preserve versions.

A new version must not overwrite the historical version used in a prior decision.

Version metadata should include:

- Version number or ID
- Created by
- Created time
- Effective date
- Superseded date
- Approval state
- Replacement/superseding record

## 13. Retention

Retention controls how long records and evidence are preserved.

Retention schedules should define:

- Record category
- Owner
- Trigger date
- Retention duration
- Disposition action
- Legal/regulatory basis where applicable
- Exception handling

Retention must not delete records under legal hold.

## 14. Legal hold

Legal hold overrides ordinary cleanup and retention disposition.

Legal hold changes must be permission-gated and audit-visible.

A legal hold should record:

- Hold ID
- Scope
- Reason
- Applied by
- Applied time
- Released by
- Released time
- Affected records

## 15. Access history

RecordArr must preserve access history for controlled records and sensitive documents where required.

Access history should show:

- Who accessed
- When
- What action occurred: viewed, downloaded, uploaded, replaced, approved, rejected, shared
- Source product or route
- Tenant

## 16. Report exports

ReportArr may render exports.

If an export becomes a retained record, RecordArr owns the stored artifact.

The stored report should preserve:

- Report definition/version
- Filters
- Date/time generated
- Actor/service
- Source products
- Source freshness
- Snapshot/current distinction

## 17. User-facing audit display

Normal users should see readable audit/activity summaries.

Admin/debug views may expose technical payloads only with explicit permission.

Do not show raw JSON by default.

## 18. Anti-patterns

The following are not allowed:

- Evidence hidden only in notes
- Activity feed treated as complete immutable audit
- Stored files owned locally by every product
- Compliance evidence meaning invented in each frontend
- Legal hold bypass through retention cleanup
- Overwriting historical document versions
- Deleting evidence referenced by an audit package
- Raw event payloads shown to normal users
- Report exports stored outside RecordArr when retained as records

## 19. Minimum acceptable implementation

An evidence/audit feature is minimally acceptable when it has:

1. Clear owner of operational record
2. RecordArr file/document identity when a file is stored
3. Compliance Core evidence meaning when rules are involved
4. Activity feed for human context when useful
5. Audit log for accountability where required
6. Retention/legal-hold behavior when records are retained
7. Access history for sensitive/controlled documents
8. No raw payloads exposed to ordinary users


---

<!-- platform-contract-testing-release-constitution.md -->


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
- Product switcher respects entitlement
- Tenant context is preserved
- Unauthorized product access is blocked
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


---

<!-- platform-error-degraded-state-constitution.md -->


# STL Compliance Error, Degraded State, and Source Unavailable Constitution

## 1. Purpose

This constitution defines how STL Compliance communicates errors, partial failures, stale data, source outages, permission problems, and degraded operation without hiding risk or confusing users.

## 2. Scope

This constitution applies to:

- API errors
- UI error states
- Section-level failures
- Source product unavailable states
- External integration failures
- Stale snapshots
- Read model degradation
- Validation errors
- Permission denied/forbidden states
- Retry behavior
- Background job failures
- Sync failures

## 3. Prime directive

A source outage must be visible.

Stale data must be labeled.

Partial failure must not masquerade as healthy state.

User-facing errors must be plain business language, not raw technical payloads.

## 4. Error categories

Recommended error categories:

- `validation_error`
- `permission_denied`
- `not_found`
- `source_unavailable`
- `stale_data`
- `conflict`
- `blocked_by_rule`
- `blocked_by_workflow`
- `integration_failure`
- `sync_failure`
- `timeout`
- `rate_limited`
- `system_error`

## 5. Validation errors

Validation errors tell the user what is wrong and how to fix it.

They should identify:

- Field or section
- Problem
- Required correction
- Whether it blocks submission
- Source product/catalog when cross-product

Avoid generic messages like `Invalid input`.

## 6. Permission errors

Permission errors must not leak sensitive data.

If the record or section existence is sensitive, hide it or show a safe forbidden state.

If the user can know the record exists but not perform the action, explain the missing authority when safe.

## 7. Source unavailable

When a source product is unavailable, the UI/API should show:

- Which source is unavailable
- What data/actions are affected
- Whether a safe snapshot is shown
- Snapshot time/freshness
- Whether retry is available
- Whether the workflow is blocked or can continue pending review

Do not silently hide failed source data.

## 8. Stale data

Stale data must be labeled.

Staleness metadata should include:

- Last successful refresh
- Source product
- Expected freshness
- Staleness reason if known
- Refresh/retry option where allowed

Stale readiness or compliance data must not be displayed as current clearance.

## 9. Partial page failure

Pages should degrade by section when possible.

A failed evidence panel should not crash an entire detail page if the main record can render.

A failed chart should not prevent dashboard KPI cards from loading.

A failed cross-product signal should show a section-level warning.

## 10. Blocked actions

Blocked state-changing actions should explain:

- What is blocked
- Why it is blocked
- Source of blocker
- Required clearing action
- Whether override exists
- Who/which product owns the clearing action

Example:

`Trip cannot be dispatched because vehicle TRK-1042 is blocked by MaintainArr: annual inspection expired. Open asset readiness.`

## 11. Retry behavior

Retry must be explicit.

Do not repeatedly retry state-changing actions in a way that can duplicate work.

Retries for writes must use idempotency.

Background retries must be visible in operational/admin views when failures persist.

## 12. External integration failures

External integration failures should show:

- External system
- Last successful sync
- Last failed sync
- Affected records
- Retry/manual review state
- Business impact

External failure must not silently overwrite STL source truth.

## 13. Offline/sync failures

Mobile/offline sync failures must show:

- Operation
- Owning product
- Record/context
- Whether data remains local
- Retry option
- Conflict or rejection reason
- Whether action is confirmed or pending

## 14. API error response

Recommended API error shape:

```json
{
  "error": {
    "code": "SOURCE_UNAVAILABLE",
    "category": "source_unavailable",
    "message": "MaintainArr readiness is temporarily unavailable. Dispatch release cannot be confirmed.",
    "sourceProduct": "MaintainArr",
    "retryable": true,
    "blocked": true,
    "correlationId": "..."
  }
}
```

Do not expose stack traces, database exceptions, raw JSON payloads, or secrets to normal users.

## 15. Degraded dashboards and reports

Dashboards and reports with degraded data must show:

- Partial source status
- Missing source(s)
- Stale source(s)
- Whether metrics exclude unavailable data
- Whether values are snapshots

## 16. Error severity

Recommended severity labels:

- `critical`
- `high`
- `medium`
- `low`
- `info`

Operational labels:

- `blocked`
- `degraded`
- `stale`
- `retrying`
- `needs_review`

Severity must be text-readable.

## 17. Technical diagnostics

Technical details belong in admin/debug/logging surfaces, not ordinary user workflows.

Diagnostics may include:

- Stack trace
- Raw payload
- Request/response body
- Service-token claims
- Database error

Only authorized technical/admin users should see this data.

## 18. Anti-patterns

The following are not allowed:

- Generic full-page failure for one failed widget
- Hiding failed cross-product sources
- Showing stale data as live
- Silent retries that duplicate writes
- Raw stack traces to ordinary users
- Permission errors that leak sensitive data
- Blocking actions with no explanation
- External sync failures hidden from operational users/admins
- Treating cached readiness as current clearance without label

## 19. Minimum acceptable implementation

An error/degraded-state implementation is minimally acceptable when it has:

1. Error category
2. Plain-language message
3. Source product/integration when relevant
4. Retryability
5. Blocking/degraded/stale state
6. Safe permission behavior
7. Section-level failure where possible
8. Correlation ID for support/debug
9. No raw internals to ordinary users


---

<!-- platform-events-handoffs-readmodels-constitution.md -->


# STL Compliance Events, Handoffs, and Read Models Constitution

## 1. Purpose

This constitution defines how STL Compliance products publish facts, request work, coordinate cross-product workflows, and build read models without breaking ownership boundaries.

Events, handoffs, and read models allow the suite to act coordinated while preserving product ownership.

## 2. Scope

This constitution applies to:

- Product domain events
- Integration events
- Outbox/inbox processing
- Handoff records
- Cross-product workflow requests
- Read models
- Mirrors
- Dashboard projections
- Reporting projections
- Event replay
- Dead-letter and review queues

## 3. Core definitions

### Event

An event is a fact that already happened.

Examples:

- `work_order.created`
- `asset.readiness_changed`
- `training_assignment.completed`
- `inventory_movement.posted`
- `route.dispatched`
- `evidence.uploaded`
- `capa.opened`

An event does not command another product to mutate blindly.

### Handoff

A handoff is an explicit request for another product to review, accept, reject, block, or complete work.

Examples:

- MaintainArr requests parts fulfillment from LoadArr.
- StaffArr forwards incident context to TrainArr for retraining evaluation.
- OrdArr requests fulfillment from LoadArr.
- AssurArr requests corrective repair from MaintainArr.
- RoutArr notifies LoadArr of an inbound dock appointment.

### Read model

A read model is a purpose-built projection used for dashboards, lists, queues, reports, or cross-product display.

A read model is not automatically the source of truth.

### Mirror

A mirror is a local read-only copy or projection of selected source fields from another product.

A mirror exists for performance, availability, filtering, or reporting convenience. It must not become a competing owner.

## 4. Prime directive

Events and read models may inform decisions, but source-of-truth corrections happen in the owning product.

A product must not repair another product's source record through event consumption unless an approved API/handoff explicitly grants that action.

## 5. Event envelope

Every cross-product event must include:

- Event ID
- Event type
- Event schema version
- Tenant ID
- Source product
- Source resource type
- Source resource ID
- Occurred time
- Emitted time
- Actor type: `human`, `service`, `integration`, `system`
- Actor ID, using `personId` when human
- Correlation ID
- Causation ID where applicable
- Idempotency key where applicable
- Payload

Recommended shape:

```json
{
  "eventId": "...",
  "eventType": "maintainarr.work_order.created",
  "schemaVersion": "1.0",
  "tenantId": "...",
  "sourceProduct": "MaintainArr",
  "source": {
    "resourceType": "work_order",
    "resourceId": "..."
  },
  "occurredAt": "2026-06-10T00:00:00Z",
  "emittedAt": "2026-06-10T00:00:01Z",
  "actor": {
    "type": "human",
    "personId": "..."
  },
  "correlationId": "...",
  "causationId": "...",
  "payload": {}
}
```

## 6. Event naming

Event names should be past tense facts.

Good:

- `asset.created`
- `route.dispatched`
- `certification.expired`
- `inventory_hold.released`
- `record.superseded`

Bad:

- `create_asset`
- `dispatch_route_now`
- `make_driver_ready`
- `fix_inventory`

Commands may exist internally, but cross-product event streams should publish facts.

## 7. Event payload rules

Payloads should include enough context for consumers to decide whether to fetch more detail.

Payloads should not dump entire records unless explicitly intended for a projection.

Payloads must not include secrets, raw service-token claims, unrestricted PII, or sensitive notes unless the event channel is explicitly authorized for that data.

Cross-product events should prefer stable IDs and summary fields.

## 8. Outbox rule

Products that publish material events should use an outbox pattern or equivalent reliability mechanism.

A state change and its event publication must not drift silently.

If event publication fails, the event must remain retryable or visible for operations/admin review.

## 9. Idempotent consumers

Consumers must treat events as at-least-once delivery unless the infrastructure proves otherwise.

Every event handler must be idempotent by event ID and tenant.

Reprocessing the same event must not duplicate:

- Tasks
- Notifications
- Inventory movements
- Handoffs
- Records
- Report snapshots
- External writebacks
- Approvals

## 10. Ordering and conflict rules

Consumers must not assume perfect global ordering across products.

When ordering matters, use:

- Source product sequence number
- Source resource version
- Occurred time
- Emitted time
- Last processed event ID
- Product-owned validation fetch

If a read model receives an older event after a newer event, it must preserve the newest known state or mark the projection for reconciliation.

## 11. Handoff lifecycle

All cross-product handoffs must use explicit states.

Recommended states:

- `requested`
- `received`
- `accepted`
- `rejected`
- `blocked`
- `in_progress`
- `waiting_on_source`
- `waiting_on_target`
- `completed`
- `cancelled`
- `expired`
- `failed`

Handoff state must not be inferred only from free-text notes or generic activity entries.

## 12. Handoff record fields

A handoff should include:

- Handoff ID
- Tenant ID
- Source product
- Source record type
- Source record ID
- Target product
- Target action type
- Target record type and ID when created/accepted
- Requested by actor or service
- Requested at
- Current state
- Priority/severity
- Due/needed by time where applicable
- Reason
- Required next action
- Source summary
- Related references
- Correlation ID
- Idempotency key
- Last updated time

## 13. Handoff authority

A handoff does not transfer ownership of the source record.

The target product owns its accepted work and target record.

Examples:

- MaintainArr owns the work order that created parts demand.
- LoadArr owns the stock reservation, pick, issue, and inventory movement.
- AssurArr owns the CAPA case.
- MaintainArr owns the corrective repair work order.
- StaffArr owns the incident/personnel history impact.
- TrainArr owns retraining assignment and completion.

## 14. Handoff acceptance

A target product may accept, reject, or block a handoff based on its own rules.

The source product may show target handoff state but must not force target acceptance.

A target product must explain rejection/blocking in plain language.

## 15. Read model ownership

The product that owns a read model owns that projection's schema, refresh logic, and display contract.

The read model owner does not automatically own the underlying source truth.

Examples:

- ReportArr may own a cross-product KPI read model.
- RoutArr may own a dispatch release readiness projection for dispatch screens.
- LoadArr may own an inbound dock readiness board that includes RoutArr appointment context.
- Compliance Core may own an evidence gap projection based on RecordArr documents and product events.

## 16. Read model metadata

Read models must expose source and freshness.

Recommended fields:

- Projection ID
- Tenant ID
- Projection owner
- Source products
- Source record references
- Last refreshed time
- Last source event time
- Last processed event ID or cursor
- Freshness state
- Staleness reason where applicable
- Confidence where applicable

## 17. Freshness states

Recommended freshness states:

- `live`
- `near_live`
- `cached`
- `stale`
- `partial`
- `source_unavailable`
- `rebuilding`
- `unknown`

UI must not show stale projections as live truth.

## 18. Reconciliation

Read models and mirrors must have a way to reconcile with source products.

Reconciliation may be:

- Scheduled rebuild
- Event replay
- On-demand refresh
- Source API revalidation
- Admin repair action
- Dead-letter retry

A projection with known gaps must be marked degraded or stale.

## 19. Dead-letter and review queues

Failed event processing must not disappear.

Failures should preserve:

- Event ID
- Tenant ID
- Source product
- Consumer product
- Handler name
- Error code
- Last error
- Retry count
- First failed at
- Last failed at
- Next retry at
- Manual review status

Business failures and technical failures should be distinguishable.

## 20. Dashboard and reporting use

Dashboards and ReportArr may consume events, handoffs, mirrors, and read models.

They must show source and freshness when data affects operational or compliance decisions.

Dashboard and report projections must not mutate source records.

## 21. Event replay

Events should be replayable where practical.

Consumers must handle replay without duplicating business effects.

Replay must be permission/system controlled, tenant-scoped, and auditable.

## 22. Anti-patterns

The following are not allowed:

- Treating events as guaranteed commands
- Updating source records in another product from an event handler without an approved API or handoff
- Building dashboards from undocumented event payload guesses
- Ignoring event schema versions
- Dropping failed events without visibility
- Using handoffs as vague notes instead of stateful records
- Hiding stale read models
- Showing mirrored data as live source truth
- Using external events to bypass product ownership

## 23. Minimum acceptable implementation

A cross-product event/handoff/read-model flow is minimally acceptable when it has:

1. Clear source product
2. Clear target/consumer product
3. Tenant-scoped event envelope
4. Schema version
5. Idempotent processing
6. Explicit handoff state when work is requested
7. Source/freshness metadata on projections
8. Dead-letter or review path
9. Plain-language failure state
10. No ownership ambiguity


---

<!-- platform-external-systems-integration-constitution.md -->


# STL Compliance External Systems Integration Constitution

## 1. Purpose

This constitution defines how STL Compliance integrates with external systems while preserving STL ownership boundaries and avoiding accidental replacement of finance, payroll, banking, tax, certified hardware, or specialized vendor systems.

## 2. Scope

This constitution applies to integrations with:

- QuickBooks
- ERP/accounting systems
- Payroll systems
- HRIS systems
- ELD systems
- Telematics systems
- Certified hardware systems
- Supplier APIs
- Carrier APIs
- CRM systems
- Government/public APIs
- External document systems
- Banking, tax, and payment systems

## 3. Prime directive

External systems remain external unless STL explicitly builds a replacement product.

STL may integrate, map, consume, snapshot, classify, report, and hand off.

STL must not silently become the system of record for external domains it does not own.

## 4. External ownership examples

QuickBooks/ERP owns:

- Invoices
- Bills
- Payments
- Accounts payable
- Accounts receivable
- Tax
- General ledger
- Bank reconciliation
- Accounting close

Payroll owns:

- Payroll execution
- Tax withholding
- Direct deposit
- Wage statements
- Payroll filings

ELD/telematics/hardware vendors own:

- Certified ELD capture
- Hardware-generated records
- Device telemetry
- Firmware/hardware lifecycle
- Hardware compliance certifications

External CRM may own:

- External sales pipeline
- Marketing automation
- CRM-specific workflows

## 5. STL ownership around integrations

STL may own:

- Operational customer/vendor records
- Completion packets
- Invoice-ready packets
- Bill-ready packets
- External ID mappings
- External status snapshots
- Operational use of external hardware data
- Evidence classification
- Related work/status decisions
- Sync status and integration health

## 6. Integration direction

Every integration must declare direction:

- `inbound`
- `outbound`
- `bidirectional`
- `read_only`
- `writeback`

Bidirectional integrations require conflict rules.

Writeback integrations require idempotency, reviewability, and audit.

## 7. External mappings

External mappings must include:

- STL tenant ID
- STL owning product
- STL entity type
- STL entity ID
- External system
- External entity type
- External ID
- Mapping status
- Sync direction
- Last verified time
- Last sync time
- Last error when applicable

External IDs must not replace STL canonical IDs unless the ownership constitution explicitly says the external system is the source of truth.

## 8. Snapshots

External status snapshots must be labeled as snapshots.

Examples:

- QuickBooks invoice status snapshot
- Payroll export status snapshot
- ELD hours-of-service status snapshot
- Supplier order status snapshot
- Carrier delivery status snapshot

Snapshots should include:

- External system
- External ID
- Snapshot time
- Status
- Source payload version/hash where appropriate
- Freshness

## 9. External credentials

External credentials/tokens must be managed through approved secure storage and integration configuration.

Credentials must be:

- Tenant-scoped or platform-scoped as appropriate
- Encrypted/secret-managed
- Permission-protected
- Rotatable
- Auditable when changed
- Not exposed to ordinary users

## 10. Financial integration rules

STL may prepare financial handoff packets.

STL must not own invoices, bills, payments, tax, ledger, or accounting close unless a future ownership constitution explicitly creates that product/domain.

Financial handoff packets should include:

- Source product(s)
- Operational completion summary
- Customer/vendor mapping
- Amount/cost/revenue snapshot when operationally derived
- Supporting documents
- Approval status
- External system target
- Handoff status

External finance system response becomes an external status snapshot.

## 11. ELD/telematics/hardware rules

STL must not pretend phone/mobile workflows replace certified hardware where certified hardware is required.

STL may consume:

- HOS status
- Vehicle telemetry
- GPS/location events
- Fault codes
- Driver logs
- Device health

STL must show source and freshness when external hardware data affects dispatch, compliance, maintenance, or reporting decisions.

## 12. Supplier and parts provider rules

Supplier integrations may provide:

- Catalog data
- Availability
- Price snapshots
- Lead time snapshots
- Order status
- Shipment status
- Invoices/bills handoff metadata

SupplyArr owns supplier/item/procurement context inside STL.

LoadArr owns receiving/inventory movement.

QuickBooks/ERP owns financial execution.

## 13. External writebacks

External writebacks must be:

- Explicit
- Idempotent
- Tenant-scoped
- Permission/service-token scoped
- Audited
- Retry-safe
- Failure-visible

No external writeback may occur as an undocumented side effect.

## 14. Sync failures

Integration failure states must show:

- External system
- Affected tenant/product/record
- Last successful sync
- Last failed sync
- Error category
- Retryable/manual review state
- Next retry when applicable
- Business impact

Failure must not silently corrupt STL source records.

## 15. Import from external systems

Inbound external data must be classified as:

- Canonical external truth
- Candidate reference data
- Tenant operational import
- Snapshot
- Evidence
- Mapping candidate

Imported external data must not bypass staging/review where ambiguity exists.

## 16. Anti-patterns

The following are not allowed:

- Treating QuickBooks status as STL invoice ownership
- Treating ELD/telematics data as STL hardware ownership
- Using external IDs as STL canonical IDs without explicit decision
- Silent external writebacks
- Credentials stored in product code or frontend config
- Sync failures hidden from users/admins
- Bidirectional sync without conflict rules
- External data overwriting product-owned truth without validation
- Mobile app pretending to be certified hardware

## 17. Minimum acceptable implementation

An external integration is minimally acceptable when it has:

1. Declared external system and ownership boundary
2. Sync direction
3. External ID mapping
4. Credential/security model
5. Source/freshness metadata
6. Idempotent writeback where applicable
7. Failure visibility
8. Audit/activity for material actions
9. No silent ownership transfer


---

<!-- platform-list-board-queue-constitution.md -->


# STL Compliance List, Board, Queue, and Search Result Constitution

## 1. Purpose

This constitution defines the everyday operational surfaces users use to find, triage, sort, filter, search, and act on records across STL Compliance.

Dashboards summarize the operating picture. Detail pages explain one record. Create pages make new records. Lists, boards, queues, and search results help users manage sets of records.

## 2. Scope

This constitution applies to:

- List pages
- Management tables
- Boards
- Queues
- Review tables
- Kanban-style views
- Search result pages
- Saved views
- Bulk action surfaces
- Mobile list/card views

## 3. Prime directive

A list or queue must help users find the right record and take the next valid action without hiding ownership, status, or risk.

Lists must not become ungoverned CRUD dumps.

## 4. Surface definitions

### List

A list is a general record-finding and management surface.

Examples:

- Assets
- People
- Suppliers
- Training programs
- Work orders
- Documents
- Rules

### Board

A board is an operational state view, usually grouped by status, lane, time, assignee, site, or priority.

Examples:

- Work order board
- Dispatch board
- Receiving board
- Training evaluation board
- CAPA board

### Queue

A queue is a triage surface for work requiring attention.

Examples:

- Incident review queue
- Import review queue
- Evidence gap queue
- Supplier approval queue
- Inventory exception queue

### Search result

A search result helps users find records across one product or multiple products.

## 5. List page structure

A standard list page should include:

- Page header
- Description/scope where helpful
- Primary action when permitted
- Search/filter command bar
- Saved view selector where useful
- Table or card list
- Bulk action area when applicable
- Empty/loading/error states
- Pagination or cursor loading

## 6. Required row/card fields

Operational rows/cards should show the fields needed for triage.

Common fields:

- Identifier
- Human-readable title/name
- Status
- Lifecycle category
- Owner/assignee/team where relevant
- Site/location where relevant
- Date/due/ETA/effective/expiration where relevant
- Severity/risk/priority where relevant
- Source product when cross-product
- Next action when relevant
- Drill-in link

Do not show raw IDs as the primary label unless the ID is operationally meaningful.

## 7. Source and ownership

Cross-product rows must show source product when ownership matters.

Examples:

- A RoutArr dispatch queue may show MaintainArr vehicle readiness as a read-only source signal.
- A StaffArr readiness queue may show TrainArr qualification state as a source signal.
- A SupplyArr purchase list may show LoadArr receiving state as a handoff/reference.

The current product must not expose edit actions for another product's canonical record unless it is an approved handoff/API action.

## 8. Search

Search should support plain language where possible and stable identifiers where known.

Search results should include:

- Source product
- Entity type
- Stable ID
- Display label
- Status
- Context
- Last updated/freshness when cross-product
- Canonical detail route

Cross-product search must not leak records across tenants or permissions.

## 9. Filters

Filters should be meaningful and product-specific.

Common filters:

- Status
- Lifecycle category
- Site/location
- Owner/assignee/team
- Date range
- Due/overdue
- Severity/risk
- Source product
- Archive state
- Review state
- Handoff state

Filters should be backed by product APIs or read models, not frontend-only guesses over unbounded data.

## 10. Saved views

Saved views are useful for repeated work.

A saved view should preserve:

- Filters
- Sort
- Columns/fields
- Density
- Grouping/lane where applicable
- Scope
- Owner: user/team/product default

Saved views must respect permission changes.

## 11. Sorting

Default sort should reflect operational urgency.

Examples:

- Work orders: blocked/safety critical, asset down, due date
- Training: overdue, expiring soon, required before duty
- Trips: exception, ETA risk, departure time
- Receiving: appointment time, blocked, supplier risk
- CAPA: criticality, due date, recurrence

## 12. Boards

Boards should group work by meaningful states.

Board lanes must map to product-owned status/lifecycle rules.

Boards must support:

- Clear lane labels
- Record cards with key fields
- Drag/drop only when the transition is valid and permission-gated
- Blocked states
- Empty lane states
- Mobile-safe fallback

Drag/drop must not bypass required workflow validation.

## 13. Queues

Queues should prioritize work requiring action.

A queue item should show:

- What needs review/action
- Why it matters
- Severity/priority
- Age/due time
- Source product
- Required next action
- Owner/queue
- Drill-in

Queues must not bury critical blockers under informational items.

## 14. Bulk actions

Bulk actions must be permission-gated and review-before-submit.

Bulk actions should show:

- Selected count
- Affected records
- Records that will be skipped
- Validation errors
- Cross-product effects
- Confirmation for destructive/state-changing actions

Bulk actions must be idempotent and auditable when state-changing.

## 15. Mobile behavior

Tables collapse into cards on mobile.

Mobile cards must show:

- Highest-value fields first
- Status
- Due/ETA/expiration where relevant
- Owner/assignee where relevant
- Next action
- Source product when cross-product

Filters should become sheets. Sort should become compact menus. Bulk actions should appear only when touch-safe.

## 16. Empty/loading/error states

Every list, board, queue, and search surface must define:

- Loading state
- Empty state
- No results after filters
- Permission-limited state
- Source unavailable state
- Error/retry state
- Archived/deleted/superseded state when relevant

Empty states should say whether the absence of records is good, bad, or neutral.

## 17. No raw JSON

Rows, cards, filters, queue items, and search results must not show raw JSON, database rows, internal payloads, service-token claims, or rule blobs to ordinary users.

## 18. Performance

List APIs must be paginated or cursor-based.

The frontend must not fetch unbounded datasets to filter locally.

Use read models or summary endpoints for heavy operational boards.

## 19. Anti-patterns

The following are not allowed:

- Generic CRUD tables with no operational meaning
- Full record editing inside dashboard/list rows
- Drag/drop workflow transitions without validation
- Free-text references to canonical records
- Hiding ownership/source on cross-product rows
- Unbounded list queries
- Bulk actions without review
- Mobile tables that require horizontal scrolling for normal use
- Raw JSON in row details

## 20. Minimum acceptable implementation

A list/board/queue is minimally acceptable when it has:

1. Clear purpose and owning product
2. Tenant- and permission-safe data
3. Search/filter/sort appropriate to the workflow
4. Status/lifecycle visibility
5. Canonical drill-in route
6. Loading/empty/error states
7. Mobile-safe behavior
8. No ownership ambiguity
9. No raw JSON


---

<!-- platform-mobile-offline-capture-sync-constitution.md -->


# STL Compliance Mobile, Offline, Capture, and Sync Constitution

## 1. Purpose

This constitution defines how STL Compliance supports real mobile work, offline capture, evidence capture, signatures, QR/code workflows, and sync without making Field Companion or local device storage the source of truth.

## 2. Scope

This constitution applies to:

- Field Companion
- Product mobile surfaces
- Offline drafts
- Mobile task inbox
- Photo capture
- Document capture
- Signature capture
- QR/barcode scanning
- Poor-connection workflows
- Sync queues
- Conflict handling
- Device-local storage
- Mobile notifications

## 3. Prime directive

Mobile is a first-class work surface, not just a viewer.

Field Companion is an execution surface, not a source-of-truth product.

Mobile actions write back to the owning product.

Offline actions are pending until confirmed by the owning product.

## 4. Field Companion ownership

Field Companion owns:

- Mobile task inbox surface
- Product switcher or entitled task surface
- Guided execution screens
- Photo/document/signature capture UI
- Offline-capable field action UI
- Push/in-app task surface

Field Companion does not own:

- Final operational records
- Training truth
- Maintenance truth
- Inventory truth
- Dispatch truth
- Compliance interpretation
- Document storage
- Certified hardware truth

## 5. Mobile action ownership

A mobile action must route to the owning product API.

Examples:

- Work order update → MaintainArr
- Training signoff → TrainArr
- Incident self-report → StaffArr or the owning incident intake flow
- Receiving action → LoadArr
- Trip update → RoutArr
- Evidence upload → product intake + RecordArr storage
- CAPA verification → AssurArr
- Document acknowledgment → RecordArr where controlled

## 6. Offline state

Offline records must be visibly marked.

Recommended states:

- `online`
- `offline_available`
- `offline_draft`
- `pending_sync`
- `syncing`
- `synced`
- `sync_failed`
- `conflict`
- `server_rejected`

Pending local actions must not be displayed as final, active, approved, dispatched, posted, issued, or completed until the owning product confirms.

## 7. Offline drafts

Offline drafts may exist for long workflows.

Offline drafts must preserve:

- Local draft ID
- Intended owning product
- Tenant
- Actor/person
- Captured fields
- Captured files pending upload
- Validation known locally
- Missing server validation warnings
- Created/updated times

Offline drafts must show that server-side validation is pending.

## 8. Sync queue

The mobile sync queue should preserve:

- Operation ID
- Tenant
- Owning product
- Target record/reference
- Operation type
- Idempotency key
- Local timestamp
- Payload summary
- Attachments pending upload
- Retry count
- Current sync state
- Last error

Sync must be idempotent.

## 9. Conflict handling

Conflicts must not silently overwrite source truth.

Conflict UI should explain:

- What changed locally
- What changed on the server
- Which fields conflict
- Which source owns the current truth
- Available actions: discard local, apply allowed changes, create note, submit for review, retry

High-risk conflicts should require review.

## 10. Capture rules

Photo, document, audio note, signature, and scan capture must include enough context to attach correctly.

Capture metadata should include:

- Tenant
- Captured by `personId`
- Captured time
- Device time and server receive time
- Owning product
- Target record/context
- Capture type
- Location metadata only when allowed/needed
- Upload/sync state

Files that become evidence or records must be stored through RecordArr.

## 11. Signature and signoff

Signatures and signoffs must be tied to:

- Person
- Role/authority where relevant
- Record
- Action being signed
- Timestamp
- Attestation text
- Device/session context where appropriate

Do not treat a scribble alone as an auditable signoff without identity and intent.

## 12. QR/barcode/code scanning

Scans must resolve to canonical records through owning-product APIs or reference providers.

Examples:

- Asset QR → MaintainArr asset
- Location QR → StaffArr location
- Inventory/bin/item scan → LoadArr/SupplyArr depending on object
- Document QR → RecordArr document
- Trip/load code → RoutArr/LoadArr/OrdArr depending on owner

Scans must not create free-text references.

## 13. Poor connection behavior

Mobile must show connection state when it affects work.

Critical safety/compliance actions should clearly show whether they are:

- Captured locally
- Pending sync
- Confirmed by server
- Rejected
- Requiring review

## 14. Device-local storage

Device-local storage must be minimized and protected.

Sensitive data should not be stored offline unless necessary.

Offline data should support:

- Expiration/cleanup
- Encryption where available
- Tenant separation
- User logout cleanup where appropriate
- Lost-device risk mitigation

## 15. Notifications and tasks

Mobile task surfaces should prioritize action needed, not product hierarchy.

Each task must still show or resolve:

- Owning product
- Record
- Required action
- Due/severity
- Offline capability
- Sync state after action

## 16. Certified hardware boundary

Mobile workflows must not pretend to replace ELDs, certified telematics, dedicated scanners, or other regulated/specialized hardware where those remain external systems.

Mobile may supplement, display, capture supporting evidence, and orchestrate around hardware data.

## 17. Anti-patterns

The following are not allowed:

- Showing pending offline action as confirmed
- Field Companion owning final operational records
- Device-local data as permanent source truth
- Silent conflict overwrite
- QR scans creating free-text references
- Signature capture without person/action/time context
- Evidence files stored outside RecordArr when retained
- Mobile pretending to replace certified hardware
- Tiny desktop controls copied onto mobile

## 18. Minimum acceptable implementation

A mobile/offline feature is minimally acceptable when it has:

1. Owning product for every action
2. Clear offline/pending/synced state
3. Idempotent sync operation
4. Conflict handling
5. RecordArr storage for retained evidence/files
6. Canonical reference resolution for scans
7. Device-local storage controls
8. Plain-language failure recovery
9. No false confirmation of unsynced actions


---

<!-- platform-notifications-tasks-inbox-constitution.md -->


# STL Compliance Notifications, Tasks, and Inbox Constitution

## 1. Purpose

This constitution defines how STL Compliance surfaces work, alerts, reminders, blockers, approvals, and messages without creating a dedicated NotificationArr too early or confusing notifications with source-of-truth work records.

## 2. Scope

This constitution applies to:

- In-app notifications
- Mobile push notifications
- Email/SMS delivery where implemented
- Task inboxes
- Field Companion task surfaces
- Approval requests
- Reminders
- Escalations
- Notification grouping/deduplication
- User preferences
- System-generated alerts

## 3. Prime directive

A notification is not the work.

A task points to work.

The product that owns the required action owns the task/work record.

Delivery channels do not own the business truth.

## 4. Notification vs task

### Notification

A notification informs a user that something happened, changed, is due, is blocked, or needs attention.

### Task

A task is an actionable item tied to a product-owned workflow or record.

Examples:

- TrainArr owns a training signoff task.
- MaintainArr owns a work order assignment task.
- LoadArr owns a receiving task.
- AssurArr owns a CAPA review task.
- RecordArr owns a document approval/read-and-acknowledge task.
- StaffArr owns personnel/incident review tasks.
- RoutArr owns dispatch/trip update tasks.

### Inbox

An inbox aggregates tasks and notifications for the user.

An inbox may be shell-level or Field Companion-level, but it must not become the owner of product work.

## 5. Required notification fields

A notification should include:

- Notification ID
- Tenant ID
- Source product
- Source record type
- Source record ID
- Recipient: person/team/role/queue
- Title
- Plain-language message
- Severity
- Reason/category
- Created time
- Due time or urgency when relevant
- Action route
- Read/acknowledged state
- Delivery channels attempted
- Correlation ID

## 6. Required task fields

A task should include:

- Task ID
- Owning product
- Tenant ID
- Related record type
- Related record ID
- Assigned person/team/role/queue
- Required action
- Status
- Priority/severity
- Due time
- Blocking effect where relevant
- Source/reason
- Canonical action route

The task owner is the product that owns the required action.

## 7. Severity

Recommended severity levels:

- `critical`
- `high`
- `medium`
- `low`
- `info`

Recommended special states:

- `blocked`
- `approval_required`
- `review_required`
- `due_soon`
- `overdue`

Severity must be text-readable, not color-only.

## 8. Delivery channels

Delivery channels may include:

- In-app notification
- Field Companion inbox
- Push notification
- Email
- SMS
- Webhook
- External system handoff

Channels are delivery methods, not source-of-truth systems.

A failed delivery must not erase the underlying task or business state.

## 9. Preferences

Notification preferences may control delivery channel and frequency.

Preferences must not suppress required safety, compliance, legal, security, or urgent operational notifications unless an explicit policy allows it.

Preferences should be scoped by:

- User/person
- Tenant
- Product
- Category
- Severity
- Channel

## 10. Deduplication and grouping

The platform should prevent noisy duplicates.

Group notifications when:

- Many records need the same review
- A repeated event occurs for the same record
- A batch import produces many similar warnings
- A dashboard/queue is a better surface than individual alerts

Critical blockers should not be hidden inside low-priority groups.

## 11. Escalation

Escalation rules must be explicit.

An escalation should define:

- Trigger
- Delay/threshold
- Original owner/assignee
- Escalation recipient
- Message
- Blocking effect
- Source product
- Audit/activity behavior

Examples:

- Overdue training signoff escalates to manager.
- Open incident review escalates to safety lead.
- Unresolved receiving exception escalates to AssurArr or LoadArr supervisor.
- CAPA due date breach escalates to assurance owner.

## 12. Field Companion

Field Companion may aggregate mobile tasks across products.

Field Companion owns the mobile task surface, not the underlying records.

A Field Companion task action must write back to the owning product API.

Offline mobile tasks must show sync state and must not pretend pending local actions are confirmed.

## 13. Approval notifications

Approval notifications must identify:

- What is being approved
- Owning product
- Requested by
- Why approval is needed
- Due time
- Consequence of approval/rejection
- Canonical route to approve/reject

Approval must happen through the owning product's authorized workflow, not only by clicking a notification.

## 14. Read and acknowledge

Read/acknowledge records may be operational notifications or RecordArr-controlled records depending on context.

If the acknowledgment is a controlled document, SOP, policy, or retained evidence, RecordArr owns the stored record/acknowledgment artifact.

## 15. Templates

Templates may exist for consistent notification language.

Templates must not encode hidden business rules that belong to products or Compliance Core.

Template content should include placeholders that resolve safely and do not leak restricted data.

## 16. Anti-patterns

The following are not allowed:

- Treating notifications as the source-of-truth work record
- Creating tasks with no owning product
- Delivery failure deleting business tasks
- Suppressing critical compliance/safety notifications by ordinary preference
- Spamming one notification per repeated event when grouping is appropriate
- Notifications with no action route or source record
- Cross-product task actions that bypass the owning product
- Raw event payloads in notifications

## 17. Minimum acceptable implementation

A notification/task feature is minimally acceptable when it has:

1. Source product
2. Owning product for the task/action
3. Tenant-safe recipient resolution
4. Plain-language title/message
5. Severity/category
6. Action route to canonical workflow
7. Delivery/read state
8. Dedup/group behavior where needed
9. Audit/activity for material alerts or approvals
10. No ownership confusion


---

<!-- platform-record-lifecycle-status-constitution.md -->


# STL Compliance Record Lifecycle and Status Constitution

## 1. Purpose

This constitution standardizes lifecycle language across STL Compliance products so users, APIs, workflows, dashboards, reports, and audit records use consistent meaning.

Products may have domain-specific statuses, but they must map to platform lifecycle concepts.

## 2. Scope

This constitution applies to:

- Draft records
- Submitted records
- Approvals
- Active/inactive state
- Blocked/watch states
- Completion/closeout
- Cancellation
- Archive and supersession
- Deletion rules
- State transitions
- Lifecycle events
- Status badges and UI labels

## 3. Prime directive

A status must mean something.

Do not use vague labels that hide the business effect of a record.

A lifecycle transition must be explicit, permission-gated, auditable, and owned by the product that owns the record.

## 4. Platform lifecycle categories

Products should map local statuses to these platform lifecycle categories where applicable:

- `draft`
- `submitted`
- `pending_review`
- `pending_approval`
- `approved`
- `rejected`
- `active`
- `scheduled`
- `in_progress`
- `blocked`
- `watch`
- `completed`
- `closed`
- `cancelled`
- `inactive`
- `archived`
- `superseded`
- `deleted`

Not every product needs every state.

## 5. Draft

A draft is an incomplete or not-final record.

Rules:

- Drafts may have stable IDs.
- Drafts must be clearly labeled.
- Drafts may preserve completed sections and validation state.
- Draft saves must not trigger final workflows.
- Drafts must not appear as active, approved, published, dispatched, issued, posted, or completed.

Examples of effects that must not happen on ordinary draft save:

- Training assignment notification
- Inventory movement
- Route dispatch
- Work order release
- Evidence package finalization
- Rule publication
- External writeback

## 6. Submitted

Submitted means the user has intentionally moved the record from draft/intake into a reviewable or actionable state.

A submitted record may still require approval, acceptance, assignment, or activation.

Submit actions must clearly explain business effects before execution.

## 7. Pending review

Pending review means a human or system must evaluate the record before it can proceed.

Examples:

- Compliance Core import mapping review
- AssurArr finding review
- SupplyArr supplier approval review
- RecordArr document review
- TrainArr evaluation signoff

Pending review must identify the reviewing product, role, team, person, or queue where possible.

## 8. Pending approval

Pending approval means the record is waiting for an authority decision.

Approvals must record:

- Approver role/person/team
- Approval reason
- Requested time
- Due time when applicable
- Decision
- Decision time
- Decision notes when required

Approval and review are related but not identical.

## 9. Approved and rejected

Approved means the approving authority accepted the record or transition.

Rejected means the approving authority refused it.

Rejected records must provide a reason and next path when possible:

- Revise draft
- Resubmit
- Cancel
- Archive
- Escalate

## 10. Active

Active means the record is valid for current operational use.

Examples:

- Active person
- Active asset
- Active supplier
- Active qualification
- Active rulepack
- Active document version

Active must not be used for records that are only scheduled, draft, pending approval, or archived.

## 11. Scheduled

Scheduled means planned for future execution.

Examples:

- Scheduled trip
- Scheduled inspection
- Scheduled training session
- Scheduled report
- Scheduled PM

Scheduled records may still be blocked before start.

## 12. In progress

In progress means execution has started and is not complete.

Products should define what starts progress.

Examples:

- Work order started
- Trip departed
- Training assignment begun
- Receiving started
- CAPA action underway

## 13. Blocked

Blocked means the record cannot proceed until a required issue is resolved.

A blocker must include:

- Source product or rule
- Reason
- Severity
- Required clearing action
- Owner or queue when available

Blocked is not just a color. It is a business state.

## 14. Watch

Watch means the record may proceed but should be monitored.

Examples:

- Certification expiring soon
- Low inventory warning
- ETA risk
- Evidence due soon
- PM due soon

Watch state must not be confused with blocked state.

## 15. Completed and closed

Completed means the operational work is finished.

Closed means all required review, documentation, evidence, financial handoff, or administrative closeout is done.

Some products may use only one of these if the distinction is not useful.

Examples:

- A work order may be completed by a technician but not closed until reviewed.
- A route may be completed when deliveries are done but not closed until POD/evidence is verified.
- A CAPA may have actions completed but remain open until effectiveness check passes.

## 16. Cancelled

Cancelled means the record was intentionally stopped before completion.

Cancelled records must preserve history and reason.

Cancellation must not be used as deletion.

## 17. Inactive

Inactive means not usable for current operations but still present.

Examples:

- Inactive person
- Inactive supplier
- Inactive asset
- Inactive rulepack

Inactive records may remain visible in history.

## 18. Archived

Archived means retained for history but removed from normal active workflows.

Archived records must not appear in default active selects unless explicitly allowed.

Archived is not deleted.

## 19. Superseded

Superseded means replaced by a newer record or version.

Superseded records should point to the replacement where applicable.

Examples:

- Document version superseded by a new effective version
- Rule mapping superseded by new citation interpretation
- Training program superseded by revised program
- Report snapshot superseded by regenerated package

## 20. Deleted

Deletion should be rare.

Production deletion of business records must be restricted, audited, and usually soft-delete or tombstone-based.

Preproduction may allow destructive deletion or rebase by explicit project policy.

Deleted records referenced by history should resolve to a safe tombstone state rather than breaking pages.

## 21. Lifecycle transition rules

Every state transition must define:

- From state
- To state
- Actor/permission required
- Validation required
- Business effect
- Events emitted
- Notifications/handoffs triggered
- Audit entry

State transitions must not happen as hidden side effects of ordinary field edits unless explicitly documented.

## 22. Status display

UI statuses must use readable labels.

Avoid showing only raw enum values.

Use badges, text, and explanations where decisions are affected.

Color may reinforce state but must not be the only signal.

## 23. Product-specific mapping examples

### MaintainArr work order

- `draft` → planned but not released
- `submitted` → requested
- `approved` → authorized
- `scheduled` → planned for a date/person/team
- `in_progress` → work started
- `blocked` → cannot continue
- `completed` → technician finished
- `closed` → reviewed and finalized
- `cancelled` → stopped

### TrainArr assignment

- `assigned`
- `in_progress`
- `pending_evaluation`
- `completed`
- `expired`
- `revoked`
- `remediation_required`

These must map to platform lifecycle categories when reported cross-suite.

### RecordArr document

- `draft`
- `pending_review`
- `approved`
- `effective`
- `expired`
- `superseded`
- `archived`
- `legal_hold`

## 24. Anti-patterns

The following are not allowed:

- Using `complete` when evidence/approval is still missing
- Using `active` for draft records
- Hiding state changes in generic save actions
- Treating cancelled as deleted
- Treating archived as unavailable history
- Allowing blocked records to proceed without override/audit
- Product-specific status labels that cannot map to platform lifecycle for reports
- Frontend-only lifecycle rules

## 25. Minimum acceptable implementation

A lifecycle model is minimally acceptable when it has:

1. Clear owned record type
2. Defined local statuses
3. Mapping to platform lifecycle categories where applicable
4. Explicit transition rules
5. Permission checks for state changes
6. Audit/activity events for material transitions
7. Plain-language status labels
8. Safe archive/supersede/delete behavior


---

<!-- platform-reference-data-ingestion-constitution.md -->


# STL Compliance Platform Reference Data and Ingestion Constitution

## 1. Purpose

This constitution defines how STL Compliance imports, stages, reviews, approves, updates, and serves platform reference data.

Platform reference data helps products use shared controlled values without turning tenant-owned operational data into global truth.

## 2. Scope

This constitution applies to platform-owned or platform-curated datasets such as:

- Vehicle taxonomy
- Asset class/type catalogs
- Make/model/year reference data
- SDS and hazardous material catalogs where platform-curated
- Governing body catalogs
- Compliance vocabulary
- Part/category taxonomy
- UPC/SKU/item reference catalogs where platform-neutral
- Unit-of-measure catalogs
- Location type vocabulary
- Document/evidence type catalogs
- Common external system type catalogs

It does not apply to tenant-owned operational records such as a tenant's actual assets, people, vendors, customers, inventory balances, work orders, trips, documents, or orders.

## 3. Prime directive

Platform reference data must not silently become tenant-owned truth.

Tenant operational data must not silently become platform reference data.

All imports are staged first, then reviewed and approved before becoming canonical platform reference data.

## 4. Dataset ownership

A platform dataset must have one owner.

Possible owners:

- Compliance Core for regulatory vocabulary, governing bodies, citations, rulepacks, evidence types, applicability terms
- MaintainArr for asset taxonomy consumption and maintenance-oriented reference fields when not regulatory
- SupplyArr for item/material/part category reference consumption
- LoadArr for inventory/warehouse operational vocabulary consumption
- StaffArr for internal location type vocabulary consumption
- NexArr/platform admin for tenant/product/package/platform catalogs

Where a dataset is shared across products, the ownership constitution decides the owner.

## 5. Platform reference vs tenant-owned data

Platform reference examples:

- `vehicle.class.passenger_car`
- `asset.type.semi_tractor`
- `governing_body.fmcsa`
- `evidence_type.driver_qualification_file`
- `material.hazard_class.flammable_gas`
- `uom.each`

Tenant-owned examples:

- Tenant's truck `TRK-1042`
- Tenant's employee `Marcus Hill`
- Tenant's warehouse bin count
- Tenant's O'Reilly vendor account
- Tenant's work order
- Tenant's SDS file attachment
- Tenant's customer requirement note

Do not include tenant-owned or tenant-derived operational values in platform reference imports unless explicitly promoted through a governance process.

## 6. Import routing columns

Reference-data imports should support routing columns:

- `product`
- `dataset`
- `dataset_key`

or:

- `product`
- `dataset`

The routing fields tell the import system which product/dataset owner should review the row.

## 7. Identity columns

Reference-data imports should include:

- `entity_type`
- `canonical_key`
- `display_name`

The `canonical_key` should be stable, readable, and namespaced enough to avoid collisions.

Examples:

- `asset.class.passenger_vehicle`
- `asset.type.semi_tractor`
- `governing_body.osha`
- `docs.req.driver_qualification_file`
- `material.hazard_class.flammable_gas`

## 8. Optional provenance columns

Imports should support:

- `source_system`
- `source_key`
- `confidence`
- `fields_json`

Additional useful columns:

- `description`
- `parent_key`
- `status`
- `effective_date`
- `deprecated_at`
- `replacement_key`
- `notes`

## 9. Staging state

Every imported row starts staged.

Recommended states:

- `staged`
- `needs_mapping`
- `duplicate_candidate`
- `needs_review`
- `approved`
- `rejected`
- `merged`
- `deprecated`
- `superseded`

Rows must not become canonical merely because they were imported.

## 10. Review behavior

Review must be row-by-row or batch-with-review-summary.

A reviewer should see:

- Proposed canonical key
- Display name
- Dataset
- Source system
- Source key
- Confidence
- Duplicate candidates
- Parent/category mapping
- Changed fields
- Missing required fields
- Suggested merge/supersede behavior

Ambiguous imports must stay reviewable.

## 11. Confidence

Confidence is a review aid, not final truth.

Suggested confidence scale:

- `1.0` exact trusted match
- `0.8-0.99` high confidence
- `0.5-0.79` needs review
- `<0.5` low confidence/manual review

Low-confidence rows must not auto-approve.

## 12. Canonical keys

Canonical keys should be:

- Stable
- Lowercase
- Dot-separated or similarly namespaced
- Human readable
- Not tenant-specific
- Not source-system-specific unless the dataset intentionally maps that source

Do not use database IDs as portable canonical keys.

## 13. Field JSON

`fields_json` may carry dataset-specific structured fields.

Rules:

- It must validate against the dataset schema.
- It must not hide required identity/routing fields.
- It must not contain tenant-owned operational values.
- It must not be exposed to normal users as raw JSON.
- UI must render fields in readable form.

## 14. Updates and deprecation

Reference data updates must preserve historical resolvability.

Do not delete canonical keys that may exist on historical records.

Use:

- `deprecated`
- `replacement_key`
- `superseded`
- `merged`

Records referencing old keys should still resolve with a warning or replacement suggestion.

## 15. Product consumption

Products should consume reference data through APIs/catalog providers, not copied CSVs.

Catalog providers should return:

- Canonical key
- Display name
- Description
- Parent/grouping
- Status
- Effective/deprecated state
- Source/provenance where useful

## 16. Source provenance

Approved rows must preserve provenance.

Provenance should include:

- Source system
- Source key
- Import batch ID
- Import time
- Reviewed by
- Reviewed time
- Confidence
- Merge/supersede history

## 17. Import batch audit

Each import batch should record:

- Batch ID
- Uploaded by
- Uploaded time
- File name/source
- Target product/dataset
- Row counts
- Validation errors
- Approved/rejected counts
- Review status

## 18. Anti-patterns

The following are not allowed:

- Auto-promoting tenant values to platform catalogs
- Product-specific duplicate catalogs for shared platform reference data
- Free-text controlled values where catalog values exist
- Deleting deprecated keys that historical records reference
- Raw JSON shown to ordinary users
- Importing rows without source/provenance
- Auto-approving ambiguous source data
- Using tenant asset names, vendor names, or customer names as platform reference values

## 19. Minimum acceptable implementation

A platform reference import is minimally acceptable when it has:

1. Product/dataset routing
2. Canonical key and display name
3. Source/provenance
4. Staging before approval
5. Review state
6. Duplicate/merge/deprecation handling
7. Tenant-owned data exclusion
8. Catalog API/provider for product consumption
9. Historical key resolvability


---

<!-- platform-reference-snapshot-mirror-constitution.md -->


# STL Compliance Reference, Snapshot, and Mirror Constitution

## 1. Purpose

This constitution defines how STL Compliance products reference records owned by other products, when labels may be cached, when snapshots are acceptable, and how mirrors remain read-only projections instead of duplicate masters.

## 2. Scope

This constitution applies to:

- Cross-product reference fields
- Controlled selects
- Search/select providers
- Cached display labels
- Snapshot fields
- Mirror tables
- Reference validation
- Archived, deleted, merged, and superseded references
- External ID mappings

## 3. Prime directive

A reference is not ownership.

A snapshot is not live truth.

A mirror is not a duplicate master.

Products must make the distinction visible in data models, APIs, and UI where it affects decisions.

## 4. Reference definition

A reference points to a canonical record owned by another product.

Required reference fields:

- Source product
- Entity type
- Stable source ID
- Display label or label snapshot
- Reference status when known
- Fetched or validated timestamp when used for decisions

Recommended shape:

```json
{
  "sourceProduct": "StaffArr",
  "entityType": "person",
  "id": "person_123",
  "displayNameSnapshot": "Marcus Hill",
  "statusSnapshot": "active",
  "snapshotAt": "2026-06-10T00:00:00Z"
}
```

## 5. Canonical reference owners

Use canonical source IDs from the owning product.

- Human references: StaffArr/NexArr `personId`
- Internal people/authority/location references: StaffArr
- Tenant and entitlement references: NexArr
- Training, certifications, qualifications: TrainArr
- Assets, components, defects, work orders, PMs, inspections: MaintainArr
- Routes, trips, stops, dispatch exceptions: RoutArr
- Vendors, suppliers, parts, items, materials, procurement context: SupplyArr
- Inventory, stock ledger, holds, reservations, picks, issues, receiving: LoadArr
- Customers, customer contacts, customer requirements: CustomArr
- Orders and requests: OrdArr
- Documents, files, record packages, versions, retention: RecordArr
- Rules, rulepacks, governing bodies, evidence requirements, applicability: Compliance Core
- Nonconformance, CAPA, assurance cases, release decisions: AssurArr
- Reports, report definitions, report snapshots, scheduled exports: ReportArr

## 6. Selected, not typed

Canonical references must be selected, searched, scanned, or resolved through approved APIs/reference providers.

Free-text names are allowed only for narratives, notes, descriptions, or non-canonical descriptive fields.

Examples:

- A work order references a StaffArr person, not typed mechanic name.
- A trip references a MaintainArr asset, not typed truck number alone.
- A purchase request references a SupplyArr item, not a typed part description alone.
- A compliance mapping references a Compliance Core governing body/citation, not free-text law names alone.

## 7. Display labels

Products may cache display labels for usability and history.

Cached display labels must not be treated as identifiers.

When a label may have changed, show one of:

- Current label fetched from source
- Snapshot label with snapshot time
- Archived/superseded label with warning
- Source unavailable state

## 8. Snapshot rule

A snapshot captures what was known at a point in time.

Snapshots are valid when:

- A historical decision must be preserved.
- A report must preserve generated criteria/results.
- External system status was known at a time.
- A record package needs audit history.
- A source product may later change a label/status but the original business context matters.

Snapshots must include:

- Source product
- Source entity type
- Source entity ID
- Snapshot values
- Snapshot time
- Snapshot reason when material

## 9. Mirror rule

A mirror is a local projection of selected source fields.

Mirrors are allowed for:

- Performance
- Search/filtering
- Dashboard summaries
- Offline support
- Reporting
- Integration staging
- Readiness panels
- Resilience when source is temporarily unavailable

Mirrors must be read-only from the consuming product's perspective unless the mirror owner is explicitly responsible for a local projection field.

A mirror must expose freshness.

## 10. Mirror fields

Recommended mirror metadata:

- Tenant ID
- Source product
- Source entity type
- Source entity ID
- Mirrored fields
- Source version or cursor when available
- Last source event ID
- Last refreshed time
- Freshness state
- Reconciliation state

## 11. Archived, deleted, superseded, and merged references

References must handle source lifecycle changes.

Possible reference states:

- `active`
- `inactive`
- `archived`
- `deleted`
- `superseded`
- `merged`
- `unknown`
- `source_unavailable`

Historical records should preserve old references when needed for audit, but current workflows must prevent unsafe use of invalid references.

## 12. External IDs

External IDs are mappings, not STL canonical IDs.

A SupplyArr vendor may map to a QuickBooks vendor ID.

A RoutArr trip may map to a carrier or telematics record.

A StaffArr person may map to payroll or HRIS.

External ID mappings must include:

- External system name
- External entity type
- External ID
- Direction of sync
- Last verified time
- Status

Do not use external IDs as primary STL references unless the external system is explicitly the system of record for that domain.

## 13. Reference validation

Before final submission, workflows with cross-product references must validate that required references:

- Exist
- Belong to the tenant
- Are usable for the intended action
- Are not archived/deleted unless historical use is allowed
- Meet permission rules
- Meet readiness/authority rules when relevant

Validation failures must be business-readable.

## 14. UI labeling

UI should label cross-product fields when ownership matters.

Examples:

- `Driver — StaffArr person`
- `Vehicle — MaintainArr asset`
- `Qualification — TrainArr certification`
- `Evidence requirement — Compliance Core`
- `Attachment — RecordArr document`
- `Inventory availability — LoadArr snapshot`

## 15. Read-only cross-product sections

A product may display another product's data, but if the current product does not own the action, it must provide a link or handoff to the owning product instead of pretending to edit locally.

## 16. Anti-patterns

The following are not allowed:

- Free-text canonical references
- Product-local duplicate person records
- Product-local duplicate location masters
- Copying another product's full record as editable local data
- Treating cached labels as source truth
- Showing snapshots as live current data
- Letting archived references silently pass current workflow validation
- Reusing external IDs as STL canonical IDs without explicit ownership decision
- Mirrors that mutate source-owned fields

## 17. Minimum acceptable implementation

A cross-product reference is minimally acceptable when it has:

1. Source product
2. Source entity type
3. Stable source ID
4. Display label or snapshot label
5. Source/freshness state when used for decisions
6. Validation through approved provider/API
7. Clear UI ownership labeling when displayed
8. Safe behavior for archived/superseded/source-unavailable states


---

<!-- platform-reporting-metrics-provenance-constitution.md -->


# STL Compliance Reporting, Metrics, KPI, and Provenance Constitution

## 1. Purpose

This constitution defines how STL Compliance creates, defines, calculates, snapshots, displays, schedules, exports, and governs reports, dashboards, metrics, and KPIs.

ReportArr may report across the suite, but source corrections happen in the owning product.

## 2. Scope

This constitution applies to:

- Product metrics
- Cross-product KPIs
- Dashboards
- Report definitions
- Scheduled reports
- Exports
- Report snapshots
- Audit readiness reports
- Compliance posture reports
- Executive summaries
- Data provenance metadata
- Metric definitions

## 3. Prime directive

A metric without a definition, source, owner, and freshness is incomplete.

ReportArr reports; it does not correct source truth.

The frontend must not invent operational business rules for metrics.

## 4. Metric ownership

Every metric must have an owner.

The owner may be:

- A product that owns the domain truth
- ReportArr for a cross-product metric definition/projection
- Compliance Core for compliance interpretation metrics
- A read model owner for a derived operational view

The metric owner defines calculation behavior and source dependencies.

## 5. Metric definition

A metric definition should include:

- Metric key
- Display name
- Description
- Owner
- Source product(s)
- Source record types
- Calculation rule
- Inclusion/exclusion rules
- Time basis
- Tenant scope
- Permission scope
- Freshness expectation
- Drill-in route
- Caveats

## 6. KPI source metadata

KPI responses should include:

- Value
- Label
- Period
- Comparison period where applicable
- Source product(s)
- Last updated
- Freshness
- Confidence where applicable
- Drill-in
- Warnings/degraded state

## 7. Current state vs snapshot

Reports and dashboards must distinguish current state from historical snapshots.

Examples:

- Current asset readiness may change after the report is viewed.
- A generated audit packet must preserve the readiness/evidence state at generation time.
- A scheduled report export must preserve filters and generation timestamp.

## 8. Corrections

Corrections happen in the source product.

Examples:

- Incorrect work order status is fixed in MaintainArr.
- Incorrect training completion is fixed in TrainArr.
- Incorrect inventory balance is fixed in LoadArr.
- Incorrect customer record is fixed in CustomArr.
- Incorrect evidence file metadata is fixed in RecordArr.
- Incorrect rule interpretation is fixed in Compliance Core.

ReportArr may provide drill-in links and correction guidance, not mutate source records directly.

## 9. Report definitions

A report definition should include:

- Report key/name
- Owner
- Purpose
- Source products
- Dataset/query/read model dependencies
- Filters
- Grouping/sorting
- Columns/sections
- Time basis
- Permission rules
- Output formats
- Schedule eligibility
- Retention behavior if exported/stored

## 10. Scheduled reports

Scheduled reports should record:

- Schedule ID
- Report definition/version
- Tenant
- Recipients
- Delivery channel
- Frequency
- Filters
- Timezone
- Last run
- Next run
- Last result
- Failure state
- Actor/service that configured it

Scheduled report delivery must respect permissions and recipient access.

## 11. Exports

Exports must preserve:

- Report definition/version
- Filters and parameters
- Generation time
- Generated by actor/service
- Source products
- Source freshness
- Snapshot/current distinction
- Tenant
- Export format

If retained as a record, the export artifact belongs in RecordArr.

## 12. Compliance reports

Compliance reports must identify:

- Rulepack/governing body/source
- Applicability basis
- Evidence requirements
- Evidence status
- Missing/expired/rejected evidence
- Exceptions/exemptions
- Evaluation time
- Source products
- Confidence/freshness where applicable

Compliance Core owns interpretation. ReportArr owns report rendering/analytics.

## 13. Cross-product metrics

Cross-product metrics must not hide source disagreement.

When sources disagree or are stale, reports should show:

- Source unavailable/partial state
- Staleness
- Confidence
- Excluded source
- Reconciliation warning

## 14. Frontend rule

The frontend may format, sort, color, chart, and display metric values.

The frontend must not invent:

- Readiness calculations
- Compliance pass/fail
- Qualification status
- Dispatch release status
- PM due state
- Inventory availability
- Approval state

Those must come from product services, read models, source APIs, or Compliance Core.

## 15. Drill-in

Metrics should drill into canonical records or source-backed lists.

Examples:

- Open defects → MaintainArr defects list
- Expiring certifications → TrainArr/StaffArr readiness list
- Receiving exceptions → LoadArr/AssurArr queue
- Evidence gaps → Compliance Core/RecordArr view
- Supplier risk → SupplyArr supplier detail/list

## 16. Retention and access

Reports with compliance, audit, legal, or financial support value may require retention.

Stored report artifacts must route through RecordArr when retained.

Report access must respect product permissions and document sensitivity.

## 17. Anti-patterns

The following are not allowed:

- KPI cards with no source or definition
- Frontend-only production metrics
- ReportArr mutating source records
- Reports that mix live and snapshot data without labeling
- Scheduled reports sent to recipients without access
- Export files stored outside RecordArr when retained as records
- Compliance posture calculated without Compliance Core when rule interpretation is involved
- Metrics that cannot drill to supporting data or explanation

## 18. Minimum acceptable implementation

A report/metric is minimally acceptable when it has:

1. Definition
2. Owner
3. Source product(s)
4. Calculation basis
5. Time basis
6. Freshness/source metadata
7. Permission handling
8. Drill-in or explanation
9. Snapshot/current distinction when relevant
10. Correction path to source product


---

<!-- platform-security-tenancy-authority-constitution.md -->


# STL Compliance Security, Tenancy, Authority, and Permission Constitution

## 1. Purpose

This constitution defines the security model that keeps STL Compliance tenant-safe, product-safe, and ownership-safe.

NexArr is the secure front door and platform control plane. Products own domain authorization after NexArr validates identity, tenant, and entitlement.

## 2. Scope

This constitution applies to:

- Human login
- Tenant selection
- Product entitlement
- Product launch and handoff
- Service clients and service tokens
- Product-local permissions
- StaffArr authority context
- Break-glass access
- Permission-aware UI
- Tenant isolation
- Sensitive sections
- External sharing and secure upload flows
- Audit of security-significant actions

## 3. Prime directive

No product may implement a separate platform login or bypass NexArr entitlement validation.

No product may leak data across tenants.

No product may use service identity as a backdoor around ownership, tenancy, or permission rules.

## 4. NexArr authority

NexArr owns:

- Platform login
- Authentication
- Tenant identity
- Tenant membership
- Product entitlement
- Product launch
- Platform admin
- Service clients
- Service tokens
- Handoff sessions
- Break-glass platform access
- Platform access audit events

NexArr answers:

- Is the user valid?
- Which tenant is the user acting in?
- Which products may the tenant/person access?
- Which product launch/handoff links are valid?
- Which service clients may call which products?

## 5. Product authority

After NexArr validates identity, tenant, and entitlement, each product answers:

- Which product records can this actor see?
- Which product actions can this actor perform?
- Which fields or sections are restricted?
- Which workflow transitions are allowed?
- Which approvals or escalations are required?

Product authorization must not be replaced by frontend checks alone.

## 6. StaffArr authority context

StaffArr owns people, org structure, internal locations, role assignments, permission assignments, delegations, temporary authority, and work-authority context.

Products may consume StaffArr authority context, but product-local rules still decide whether the action is allowed inside the product.

Examples:

- StaffArr says a person is a shop manager at a site.
- MaintainArr decides whether that role may close a work order.
- StaffArr says a person is a driver.
- RoutArr decides whether they may be assigned to a trip based on dispatch rules and readiness checks.
- TrainArr says a qualification is valid.
- RoutArr decides whether that qualification satisfies a dispatch release condition.

## 7. Tenant isolation

Every request, job, event, read model, cache key, file reference, search index entry, notification, and external mapping must be tenant-scoped unless explicitly platform-global.

Tenant ID must be included in:

- API authorization context
- Database query scope
- Event envelope
- Outbox/inbox records
- Read model rows
- Cache keys
- Search indexes
- File metadata
- Audit logs
- External integration mappings

Cross-tenant bugs are platform-critical defects.

## 8. Human identity

Human actors must resolve to `personId` when acting as people.

Login capability is not the same as personhood.

Recommended distinction:

- Person: `personId`
- Login account: `hasUserAccount` / authentication subject
- Tenant membership: NexArr
- Work authority: StaffArr
- Product permission: product-local permission model, usually informed by StaffArr assignments

Do not create product-local human identities as source truth.

## 9. Permission model

Permissions should be explicit, composable, and action-oriented.

Recommended permission pattern:

- `{product}.{domain}.{action}`

Examples:

- `maintainarr.work_orders.create`
- `maintainarr.work_orders.close`
- `staffarr.people.read`
- `staffarr.permissions.assign`
- `loadarr.inventory.adjust`
- `routarr.trips.dispatch`
- `recordarr.documents.read_sensitive`
- `compliancecore.rulepacks.publish`

Permissions must be checked server-side for state-changing actions and sensitive reads.

## 10. Platform admin vs product admin

Platform admin is not the same as product admin.

- Platform admin belongs to NexArr.
- Product admin belongs to the product's local authorization model.
- StaffArr may provide authority and role assignment context.
- A platform admin must not automatically become unrestricted inside every product unless explicitly granted by policy.

## 11. Service tokens

Service tokens must be least-privilege, scoped, and auditable.

Service-token calls must identify:

- Calling product
- Target product
- Tenant
- Scope
- Operation/reason
- Correlation ID
- User delegation when present

Service tokens must not be shared broadly across products.

## 12. User delegation

When a service call is triggered by a human action, the call should preserve delegated actor context where appropriate.

Example:

A MaintainArr user submits a work order that requests parts from LoadArr. The MaintainArr-to-LoadArr service call should identify both MaintainArr as the calling product and the initiating `personId` if business/audit rules require it.

## 13. Break-glass access

Break-glass access must be:

- Explicit
- Time-limited
- Tenant-scoped
- Reason-required
- Logged
- Reviewable
- Revocable

Break-glass must not become normal admin workflow.

## 14. Sensitive sections

Sensitive sections may be hidden entirely when the existence of the data is sensitive.

Examples:

- HR notes
- disciplinary details
- medical details
- private incident notes
- service-token details
- platform admin controls
- sensitive documents
- legal holds

When the section can be known but details are restricted, show a permission-limited state.

Do not leak sensitive counts, labels, or file names when the user lacks permission.

## 15. Secure no-login upload flows

No-login upload flows are allowed only through secure, scoped, expiring links.

They must include:

- Tenant scope
- Target product/record or intake context
- Expiration
- Upload limits
- Allowed file types
- Virus/malware scanning where available
- RecordArr storage when file becomes evidence/record
- Audit/access history

No-login upload must not grant broad application access.

## 16. Permission-aware UI

UI must not rely on hidden disabled controls as security.

The server remains authoritative.

UI should:

- Hide actions the user cannot perform
- Show permission-limited states where useful
- Avoid leaking sensitive values
- Explain blocked actions in plain language when safe
- Show the correct owning product for cross-product actions

## 17. Security audit

Security-significant actions must be audit-visible.

Examples:

- Login
- Tenant switch
- Product launch
- Permission assignment
- Role assignment
- Service token creation/rotation/revocation
- Break-glass access
- Sensitive document access
- External credential changes
- Failed access attempts to sensitive resources
- Data export

## 18. Anti-patterns

The following are not allowed:

- Product-local platform login
- Product bypass of NexArr entitlement
- Cross-tenant cache keys
- Frontend-only permission enforcement
- Broad service tokens with no purpose
- Treating platform admin as universal product admin by accident
- Exposing raw service-token claims to users
- Leaking sensitive section existence
- Using `userId` as canonical human identity
- External share links without scope or expiration

## 19. Minimum acceptable implementation

A secure product feature is minimally acceptable when it has:

1. NexArr identity/tenant/entitlement validation
2. Server-side product-local authorization
3. StaffArr `personId`/authority use where humans/roles/locations are involved
4. Tenant-safe query/cache/event scope
5. Least-privilege service-token behavior for service calls
6. Permission-aware UI without sensitive leakage
7. Audit for sensitive or state-changing actions
8. Clear error states for unauthorized/forbidden access


---

<!-- platform-settings-admin-configuration-constitution.md -->


# STL Compliance Settings, Admin, Configuration, and Setup Constitution

## 1. Purpose

This constitution defines where setup and configuration live so platform admin, product admin, tenant setup, product settings, reference data, integrations, permissions, and workflow configuration do not become tangled.

## 2. Scope

This constitution applies to:

- Platform admin
- Product admin
- Tenant setup
- Product settings
- User preferences
- Integration settings
- Workflow configuration
- Reference-data setup
- Product dependency settings
- Dangerous configuration changes
- Setup pages and setup wizards

## 3. Prime directive

Configuration must be owned by the product or platform authority that owns the underlying decision.

Settings screens must not become secret CRUD backdoors around guided workflows, ownership rules, or permission gates.

## 4. Configuration scopes

Every setting must declare scope:

- `platform`
- `tenant`
- `product`
- `site`
- `department`
- `role`
- `team`
- `person`
- `integration`
- `record`

A setting without scope is incomplete.

## 5. NexArr setup ownership

NexArr owns setup for:

- Tenant identity
- Tenant membership
- Product entitlement
- Product launch/handoff
- Platform admin
- Service clients
- Service tokens
- Platform-level integration authorization where required
- Product dependency visibility
- Platform access audit

No product may create its own platform admin system.

## 6. StaffArr setup ownership

StaffArr owns setup for:

- People
- Workers
- Org units
- Internal sites
- Buildings
- Rooms
- Docks
- Yards
- Operational locations
- Departments
- Positions
- Teams
- Manager relationships
- Permission assignments
- Role assignments
- Delegation/temporary authority

Products consume StaffArr people/location/authority context.

## 7. Compliance Core setup ownership

Compliance Core owns setup for:

- Governing bodies
- Rulepacks
- Regulations/citations
- Compliance vocabulary
- Evidence requirements
- Applicability logic
- Exemptions/exceptions
- Rule-to-product mappings
- Import mapping review where regulatory meaning is involved

Products must not build competing regulatory catalogs.

## 8. RecordArr setup ownership

RecordArr owns setup for:

- Controlled document categories
- Record categories
- Templates
- Retention schedules
- Legal hold behavior
- Document approval behavior
- Read-and-acknowledge configuration
- Evidence storage behavior

Compliance Core owns evidence meaning; RecordArr owns document/retention mechanics.

## 9. Product admin ownership

Each product owns settings for its domain execution.

Examples:

- MaintainArr: PM settings, work order defaults, inspection workflow configuration
- RoutArr: dispatch settings, route/trip defaults, exception handling defaults
- LoadArr: receiving/putaway/pick/issue behavior, inventory adjustment reason codes
- SupplyArr: supplier approval workflow defaults, purchasing thresholds/context
- TrainArr: training program workflow defaults, evaluator rules, remediation behavior
- AssurArr: CAPA workflow settings, severity defaults, release verification rules
- ReportArr: report schedules, subscriptions, metric display settings
- CustomArr: customer onboarding/requirements settings
- OrdArr: request/order intake and orchestration settings

## 10. User preferences

User preferences may control experience, not business truth.

Examples:

- Theme when supported
- Notification delivery preferences
- Saved views
- Table density
- Default landing page

User preferences must not bypass required safety/compliance/approval notifications or permission checks.

## 11. Integration settings

Integration settings must declare:

- External system
- Owning product/platform area
- Tenant scope
- Credential authority
- Sync direction
- Mapping behavior
- Failure behavior
- Writeback permissions
- Last sync status

External credentials must be permission-protected and secret-managed.

## 12. Setup wizards

Setup wizards are encouraged when configuration has dependencies.

A setup wizard should show:

- Required steps
- Owner of each configuration area
- Dependencies
- Completion state
- Blocking issues
- Safe defaults
- Review before activation

## 13. Dangerous settings

Dangerous settings require stronger treatment.

Examples:

- External writeback enablement
- Rulepack publish/activation
- Retention schedule changes
- Service token creation
- Break-glass access
- Inventory adjustment permissions
- Financial handoff configuration
- Deleting/archive behavior

Dangerous settings should require:

- Explicit permission
- Confirmation
- Plain-language impact preview
- Audit event
- Possibly two-person review depending on risk

## 14. Defaults

Defaults must be explicit.

Do not hide business-changing defaults in code without a settings surface or documented rule.

Defaults should identify whether they are:

- Platform defaults
- Tenant defaults
- Product defaults
- Site defaults
- User preferences

## 15. Dependency visibility

Setup should show product dependencies.

Examples:

- RoutArr dispatch may depend on StaffArr people, TrainArr qualifications, MaintainArr asset readiness, and LoadArr load readiness.
- MaintainArr parts workflow may depend on SupplyArr items/vendors and LoadArr inventory.
- Compliance reporting may depend on Compliance Core, RecordArr, and source product events.

## 16. Configuration audit

Configuration changes must be audit-visible when they affect security, workflow, compliance, retention, reporting, integrations, or external writebacks.

Audit should include:

- Changed setting
- Old/new summary
- Actor
- Time
- Scope
- Reason when required

## 17. Anti-patterns

The following are not allowed:

- Product-level platform admin
- Settings that bypass ownership rules
- Hidden settings that change business effects with no UI/audit
- Integration credentials in frontend or plain config
- Setup pages as unvalidated CRUD bypasses
- User preferences that suppress mandatory safety/compliance actions
- Product-specific duplicate regulatory catalogs
- Configuration with no scope

## 18. Minimum acceptable implementation

A settings/admin feature is minimally acceptable when it has:

1. Clear owner
2. Clear scope
3. Permission gate
4. Dependency awareness
5. Impact preview for dangerous settings
6. Audit for material changes
7. No hidden bypass of canonical workflows
8. Safe default behavior


---

<!-- platform-workflow-approval-assignment-escalation-constitution.md -->


# STL Compliance Workflow, Approval, Assignment, and Escalation Constitution

## 1. Purpose

This constitution defines how work is assigned, approved, blocked, escalated, reassigned, handed off, and closed across STL Compliance without creating a generic WorkflowArr before it is needed.

## 2. Scope

This constitution applies to:

- Product workflows
- Assignments
- Queues
- Approvals
- Reviews
- Escalations
- Blockers
- Reassignment
- Cross-product handoffs
- Workflow history
- Overrides
- Closeout

## 3. Prime directive

Work ownership stays with the product that owns the business record.

A cross-product workflow creates a handoff, reference, task, or event. It does not duplicate the source record or transfer ownership unless explicitly designed.

## 4. Workflow ownership

Examples:

- MaintainArr owns maintenance workflow.
- RoutArr owns dispatch/trip workflow.
- LoadArr owns receiving/warehouse/inventory workflow.
- TrainArr owns training/evaluation/remediation workflow.
- AssurArr owns nonconformance/CAPA/assurance workflow.
- RecordArr owns document approval/read-and-acknowledge workflow.
- SupplyArr owns supplier/procurement-context workflow.
- OrdArr owns order/request orchestration workflow.
- Compliance Core owns rule/mapping/evidence evaluation workflow.
- StaffArr owns personnel/authority/location/personnel-history workflow.

## 5. Assignment targets

Assignments may target:

- Person
- Team
- Role
- Queue
- Site/location context
- Service/system owner

Human assignment must use `personId` when assigned to a person.

Team/role/authority context should come from StaffArr where applicable.

## 6. Assignment fields

An assignment should include:

- Assignment ID
- Owning product
- Related record
- Assigned to person/team/role/queue
- Assigned by
- Assigned time
- Due/needed by time
- Priority
- Required action
- Status
- Reassignment history

## 7. Approval gates

Approval gates must define:

- What requires approval
- Why approval is required
- Who may approve
- What data/evidence is required before approval
- Approval due time when applicable
- Approval effect
- Rejection effect
- Override policy if any

Approval must be a clear state transition, not a hidden save side effect.

## 8. Review vs approval

Review means evaluation or checking.

Approval means authority decision.

A record may need review before approval.

Examples:

- A supplier document is reviewed for completeness, then supplier is approved.
- A CAPA action is reviewed for effectiveness, then case is closed.
- An import mapping is reviewed, then approved into a catalog.

## 9. Blockers

A blocker prevents workflow progress.

Blockers must include:

- Source product/rule
- Reason
- Severity
- Required clearing action
- Owner/queue when known
- Whether override is allowed
- Audit requirement for override

## 10. Overrides

Overrides must be rare and explicit.

An override should record:

- Overridden rule/blocker
- Actor
- Authority/permission
- Reason
- Time
- Expiration if temporary
- Risk acknowledgement
- Downstream notifications/events

Overrides must not delete the original blocker history.

## 11. Escalation

Escalation moves attention, not ownership, unless explicitly designed.

Escalation rules should define:

- Trigger
- Threshold/delay
- Original assignee/owner
- Escalation recipient
- Message/reason
- Business effect
- Audit/activity behavior

Examples:

- Overdue training → manager notification
- Repeated incident → StaffArr personnel review and TrainArr retraining review
- Supplier defect → AssurArr case and SupplyArr supplier issue
- Inventory exception → LoadArr hold and AssurArr nonconformance

## 12. Reassignment

Reassignment must preserve history.

A reassignment should record:

- Previous assignee
- New assignee
- Actor
- Reason when required
- Time
- Impact on due date/status

## 13. Cross-product handoff

A handoff must identify:

- Source product/record
- Target product/action
- Current handoff state
- Required next action
- Due/priority
- Source summary
- Target acceptance/rejection/blocking state

The target product owns its own accepted work.

## 14. Closeout

Closeout must define what complete means.

Closeout may require:

- Work completion
- Review
- Approval
- Evidence
- Document package
- Compliance evaluation
- External handoff
- Customer/vendor signoff
- Report snapshot

Completed and closed may be separate lifecycle states.

## 15. Workflow history

Detail views must show workflow history where relevant.

History should include:

- State changes
- Assignments
- Reassignments
- Approvals/rejections
- Blockers
- Overrides
- Handoffs
- Escalations
- Completion/closeout

Use plain language. Do not show raw event payloads to ordinary users.

## 16. Workflow UI

Workflow UI should make clear:

- Current state
- Required next action
- Who owns the action
- What is blocked
- What happens if user proceeds
- What evidence/approval is missing
- What product owns cross-product actions

## 17. Anti-patterns

The following are not allowed:

- Generic workflow engine owning product records by accident
- Assignment by free-text name
- Approval as hidden save side effect
- Escalation that loses original owner/history
- Blocking with no clear clearing action
- Overrides without reason/audit
- Cross-product workflow that edits target records without approved API/handoff
- Closeout before required evidence/review is complete

## 18. Minimum acceptable implementation

A workflow feature is minimally acceptable when it has:

1. Owning product/record
2. Explicit states/transitions
3. Assignment target using `personId`, team, role, or queue
4. Approval/review rules where applicable
5. Blocker model
6. Escalation behavior
7. Reassignment history
8. Handoff model for cross-product work
9. Workflow history on detail pages
10. Permission/audit controls
