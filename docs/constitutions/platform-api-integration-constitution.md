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
- SupplyArr owns supplier, vendor, tenant commercial item, material, part, SKU, supplier location, procurement, and purchasing-context APIs.
- ReferenceDataCore owns shared public identifier, taxonomy, unit-of-measure, manufacturer, brand, and crosswalk APIs.
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
