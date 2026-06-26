# NexArr — Scope, Ownership, and Boundaries

## Product purpose

NexArr is the platform workspace and secure front door for the STL Compliance suite. It owns tenant identity, tenant membership, platform accounts, login/session security, product registry, launch/handoff context, platform administration, service-to-service trust, and platform security audit.

NexArr answers:

- Does this tenant exist and is it active?
- Is this account allowed to authenticate?
- Is this account an active member of the selected tenant?
- Which StaffArr person, when any, is linked to this account?
- What tenant/session/actor context should a product receive?
- Is the requested destination an active ordinary product or the platform-admin-only Compliance Core studio?
- Is this caller a validated platform administrator?
- Can this service client call this route for this tenant and scope?
- Was platform access attempted, accepted, denied, revoked, or suspicious?

NexArr does not answer domain questions such as whether a technician can close a work order, a driver can start a trip, inventory can move, a person is qualified, or an order may complete. The owning product evaluates those questions with StaffArr authority context, workflow state, and owner data.

## NexArr owns

- Tenant and tenant status
- Tenant membership
- Platform account and login capability
- Account-to-StaffArr-person linkage
- Password, SSO, MFA, recovery, and security policy
- Session, refresh-token family, device/session revocation
- Static product registry and canonical destinations
- Product launch session and handoff token
- Compliance Core studio platform-admin gate
- Platform admin and break-glass administration
- Service client, secret, token, and scope
- Platform access/security audit and suspicious-activity signals

## NexArr does not own

- Tenant-specific product availability; ordinary products are available to all active tenant members
- Product-local permissions or record scope
- StaffArr person/profile/org/location truth
- Training and qualification truth
- Compliance rule meaning or evidence requirements
- Assets, work orders, inventory, procurement, transport, customers, orders, records, quality, reports, or finance
- Field Companion task execution truth

## Access model

1. NexArr validates account and tenant membership.
2. NexArr issues launch/session context for any active ordinary product.
3. Every product validates local permissions and record scope at its API boundary.
4. Compliance Core studio requires server-side platform-admin validation.
5. Compliance Core runtime APIs are callable through authorized tenant/product/service workflows.
6. Platform-admin status does not silently bypass tenant-domain permissions.

## Product dependencies

All products rely on NexArr for identity, tenant context, session/launch context, service identity, and platform-admin validation where applicable. They do not call a product-availability grant service.

NexArr may consume:

- StaffArr person status and account-link context
- StaffArr authority context for delegated account administration
- RecordArr controlled platform/security documents and exported audit packages
- ReportArr platform-security analytics

## Source-of-truth rules

- NexArr is the only platform authentication gate.
- StaffArr owns the human/business person; NexArr owns the account and login.
- Products reference `personId` for human truth and must not equate it to `userId`.
- Product-domain authorization remains with the owning product.
- No product implements a separate platform login or trusts launch context as action permission.
- Service tokens are narrow, tenant-aware, scoped, rotated, and audited.
- No cross-database foreign keys are permitted.

## Object prefixes

- `TEN` tenant
- `MEM` tenant membership
- `ACC` platform account
- `SESS` platform session
- `MFA` MFA method/challenge
- `PROD` product registry entry
- `LAUN` product launch session
- `HAND` handoff token
- `SVC` service client
- `TOK` service token
- `POL` platform security policy
- `AUD` platform audit entry
- `INV` invitation

## Login capability

A person/account can sign in for a tenant only when:

- the account exists and is active
- the tenant exists and is active
- tenant membership is active and unexpired
- account credential/SSO/MFA requirements are satisfied
- security/lockout/risk policy allows the session

After sign-in, all active ordinary product destinations are available. Product actions remain locally permissioned.
