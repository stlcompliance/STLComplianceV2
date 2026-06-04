# NexArr — Scope, Ownership, and Boundaries

## Product purpose

NexArr is the platform control plane and secure front door for the STL Compliance / ARR suite. It owns tenant identity, login, platform account security, product entitlement, product launch, handoff, service-to-service trust, and platform audit.

NexArr answers:

- Does this tenant exist?
- Is this tenant active?
- Is this person allowed to log in?
- Is this person a member of this tenant?
- Is this product entitled for this tenant?
- Is this person allowed to launch this product?
- Can this service client call another product?
- What scopes does this service token carry?
- Was platform access granted, denied, expired, revoked, or suspicious?

NexArr does not answer domain questions such as:

- Can this technician close this specific work order?
- Can this driver start this trip?
- Can this warehouse user issue this part?
- Is this person qualified to operate this forklift?
- Can this inventory move while quality-held?
- Is this asset ready for use?

Those questions belong to StaffArr, TrainArr, MaintainArr, LoadArr, RoutArr, AssurArr, and other product domains.

## NexArr owns

```text
- Tenant
- Tenant status
- Tenant membership validation
- Platform account
- Login capability
- Password/security account fields
- MFA state
- Session state
- Product entitlement
- Product access grant
- Product launch session
- Product handoff token
- Product registry
- Product dependency rules
- Service client registry
- Service token issuance
- Service token scopes
- Platform security policy
- Platform audit events
- Platform admin surface
```

## NexArr does not own

```text
- StaffArr person profile details beyond login/account linkage snapshots
- Product-local permissions after launch
- StaffArr org structure
- StaffArr internal locations
- TrainArr training/certification truth
- Compliance Core rulepacks
- MaintainArr assets/work orders
- LoadArr inventory/stock ledger
- SupplyArr suppliers/procurement
- RoutArr routes/trips
- CustomArr customers
- OrdArr orders
- RecordArr documents/files
- AssurArr quality holds/CAPA
- ReportArr analytics read models
- Field Companion task execution truth
- Financial accounting execution
```

## External product dependencies

```text
StaffArr
- Person profile
- Person status
- Person org assignment
- Permission assignments
- Person readiness snapshot
- Internal locations

TrainArr
- Qualification/certification status
- Training completion status

Compliance Core
- Platform compliance rule references where needed
- Security/evidence requirements if platform actions need compliance review

RecordArr
- Platform policy/legal/security documents if stored as controlled records
- Audit package exports if needed

ReportArr
- Platform/admin analytics and cross-product reporting

All products
- Product registry
- Entitlement check
- Handoff token redemption
- Service token validation
- Product access launch context
```

## Core source-of-truth rules

```text
1. NexArr is the only acceptable login/authentication gate.
2. NexArr owns product entitlement.
3. NexArr owns product launch/handoff.
4. NexArr owns service clients and service tokens.
5. StaffArr owns the person profile and product-neutral permissions.
6. NexArr may reference personId but must not become the HR/person profile system.
7. Products own product-domain authorization after NexArr launch.
8. Products must not implement their own platform login.
9. Products must validate handoff/service token context.
10. Products must enforce tenant isolation after handoff.
11. NexArr must not bypass product-domain rules.
12. Platform admin belongs in NexArr.
```

## Standard NexArr object envelope

```text
NexArrObject
- id
- tenantId
- objectNumber
- objectType
- status
- title
- description
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
- sourceIp
- userAgent
- correlationId
- auditTrail
- eventLog
```

## NexArr object prefixes

```text
TEN    Tenant
MEM    Tenant membership
ACC    Platform account
SESS   Platform session
MFA    MFA method/challenge
ENT    Product entitlement
PAG    Product access grant
PROD   Product registry entry
LAUN   Product launch session
HAND   Handoff token
SVC    Service client
TOK    Service token
SCOPE  Service/product scope
POL    Security policy
AUD    Platform audit entry
INV    Invitation
```

## Platform identity rule

```text
Person identity
- personId is the platform human identifier used across products.
- StaffArr owns the human profile and people/org/person history.
- NexArr owns whether that person can log in.
- NexArr owns platform credentials/security.
- Products reference personId, not product-local user IDs as human truth.
```

## Login capability rule

```text
A person can log in only when:
- personId exists
- tenant membership is valid
- PlatformAccount exists
- hasUserAccount is true
- canLogin is true
- account status allows login
- credential/SSO/MFA requirements are satisfied
- tenant/product entitlement rules allow entry to the target product
```
