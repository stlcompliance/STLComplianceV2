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
