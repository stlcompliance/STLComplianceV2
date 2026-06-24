# STL Compliance Security, Tenancy, Authority, and Permission Constitution

## 1. Purpose

This constitution defines the security model that keeps STL Compliance tenant-safe, product-safe, ownership-safe, and auditable.

## 2. Prime directive

NexArr validates platform identity, active tenant membership, session/launch context, service identity, and platform-admin status. Each owning product validates domain permissions, record scope, workflow state, and blockers at its API boundary.

Product availability is nonvariable: every active tenant member can launch every ordinary product. The Compliance Core administrative studio is platform-admin-only, while Compliance Core runtime operation remains available through authorized product and service workflows.

No frontend visibility decision, launch token, platform-admin flag, service token, or product route may bypass tenant scope or owning-product authorization.

## 3. Scope

This constitution applies to:

- human authentication, MFA, recovery, and sessions
- tenant selection and membership
- product launch and handoff
- Compliance Core studio versus runtime access
- service clients/tokens/scopes
- StaffArr authority context and product permissions
- record/site/location/party scope
- break-glass and support access
- permission-aware UI
- tenant isolation across all stores and derived systems
- external portals and no-login intake
- security-significant audit

## 4. NexArr authority

NexArr owns:

- platform authentication and account security
- tenant identity and membership
- account-to-StaffArr-person linkage
- session, device, launch, and handoff context
- static product registry and operational destination status
- platform-admin and break-glass platform access
- service clients, secrets, tokens, and scopes
- platform access/security audit

NexArr does not own product-domain permissions or tenant-specific product availability.

## 5. Product authority

Each product answers:

- Which records can this actor see?
- Which actions and fields are allowed?
- What site/location/department/team/customer/supplier scope applies?
- Which state transitions, approvals, reasons, or evidence are required?
- Which readiness or blocker results prevent execution?

Authorization must be server-side. UI checks are usability only.

## 6. Compliance Core boundary

- Every studio/admin page and administrative API requires server-side platform-admin validation.
- Runtime APIs used for questionnaires, facts, applicability, evidence requirements, readiness, citations, and evaluations use authenticated tenant/user/service context and contract-specific scope.
- Ordinary users receive plain-language Compliance Core results in their current product and do not require studio access.

## 7. StaffArr authority context

StaffArr owns people, org structure, internal locations, role/permission assignments, delegations, temporary authority, and work-authority context.

Products consume that context and still make their own action decision. A role name is context, not a substitute for a permission check.

## 8. Tenant isolation

Every request, aggregate, child record, job, event, read model, cache, search entry, file, notification, schedule, export, audit record, and external mapping is tenant-scoped unless explicitly platform-global.

Tenant comes from validated session/service context, not ordinary request fields. Queries scope by tenant before record. Cache/object/index/idempotency keys include tenant. Cross-tenant defects are release blockers.

## 9. Identity and actor separation

- NexArr `userId`: platform account
- StaffArr `personId`: human/business person
- tenant membership: NexArr
- work authority: StaffArr
- product action permission: owning product
- service client: non-human caller
- delegated subject: separate from initiating actor

These identifiers may be linked but are never interchangeable. Actor fields are derived from validated context, not caller-supplied audit fields.

## 10. Permission model

Use `{productKey}.{domain}.{action}`. Permissions are explicit, composable, action-oriented, and independently evaluated from product launch.

Examples:

- `maintainarr.work_orders.close`
- `staffarr.people.update`
- `loadarr.inventory.adjust`
- `routarr.trips.dispatch`
- `recordarr.records.read_sensitive`
- `compliancecore.rulepacks.publish`

Record-level scope and workflow state may further limit an otherwise granted permission.

## 11. Platform admin versus product authority

Platform admin belongs to NexArr and grants platform administration plus Compliance Core studio access. It does not automatically grant unrestricted tenant-domain actions.

Product administration is governed by product permissions, commonly assigned/projected through StaffArr. Support/break-glass use is explicit, tenant-scoped, reasoned, time-limited, and audited.

## 12. Service identity

Service calls identify calling product/client, target, tenant, scope, operation/reason, correlation/causation, and delegated actor when trusted and required.

Service tokens are narrow, audience-bound, short-lived where practical, rotated, revocable, and audited. They are not a backdoor around ownership or permissions.

## 13. Sensitive information

Sensitive sections may be completely hidden when existence is sensitive. Otherwise show a safe permission-limited state. Never leak restricted counts, labels, filenames, raw claims, or identifiers.

## 14. External portals and scoped intake

External/no-login access requires invitation or expiring scoped token, tenant and target scope, allowed actions, rate/size/type controls, abuse protection, safe upload/quarantine, and audit. It never grants general product access.

## 15. Browser session security

Long-lived/refresh credentials are not stored in JavaScript-readable persistence. SPA documents receive enforceable CSP and other security headers. Cookie-authenticated writes use CSRF/origin protection. Session/tenant changes partition and clear client caches.

## 16. Permission-aware UI

The UI should hide clearly unavailable actions, explain workflow blocks when safe, use human labels, and preserve work on denial/conflict. It must not imply that opening a product grants action authority or that a missing action means the product is unavailable.

## 17. Security audit

Audit login, tenant switch, launch, permission/role changes, platform-admin changes, service-token lifecycle, break-glass, sensitive access, external credentials/shares, data export, failed sensitive access, and all high-risk state changes.

## 18. Anti-patterns

Prohibited:

- product-local platform login
- product availability grants or per-tenant product gating
- frontend-only authorization
- hard-coded tenant/actor
- cross-tenant caches/queries/files
- broad unscoped service credentials
- platform admin as accidental universal domain admin
- `userId` used as `personId`
- caller-supplied audit actor
- insecure external links/uploads
- hidden Compliance Core studio route without server enforcement
- denying Compliance Core runtime merely because the user cannot open the studio

## 19. Minimum acceptable implementation

A secure feature proves:

1. identity and active tenant membership
2. endpoint authorization-map coverage
3. owning-product permission and record scope
4. tenant-safe durable queries/caches/events/files
5. trusted actor attribution
6. least-privilege service behavior
7. Compliance Core studio/runtime separation where applicable
8. permission-aware, non-leaking UI
9. security audit
10. anonymous, forbidden, wrong-tenant, stale-session, and service-scope negative tests
