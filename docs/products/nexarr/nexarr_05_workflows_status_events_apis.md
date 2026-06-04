# NexArr — Workflows, Status Logic, Events, and APIs

## Major workflow: tenant onboarding

```text
1. Platform admin creates Tenant.
2. Tenant is assigned status trial or active.
3. Security policy is selected or defaulted.
4. Product entitlements are created.
5. Product dependencies are validated.
6. Initial person/admin is linked.
7. PlatformAccount is created.
8. Invitation is sent.
9. Admin accepts invitation and logs in.
10. NexArr displays product launcher.
```

## Major workflow: login

```text
1. User enters identifier.
2. NexArr resolves PlatformAccount.
3. NexArr validates tenant membership.
4. NexArr validates account status.
5. NexArr validates credentials or SSO.
6. NexArr applies lockout/risk policy.
7. NexArr requires MFA if policy says so.
8. NexArr creates PlatformSession.
9. NexArr records audit and login attempt.
10. User enters launcher.
```

## Major workflow: product launch

```text
1. User selects product from launcher.
2. NexArr checks tenant status.
3. NexArr checks ProductEntitlement.
4. NexArr checks ProductDependencyRule.
5. NexArr checks ProductAccessGrant.
6. NexArr creates ProductAccessDecision.
7. If allowed, NexArr creates ProductLaunchSession.
8. NexArr creates HandoffToken.
9. User is redirected to product.
10. Product redeems token.
11. Product creates local session.
12. Product loads StaffArr/product-local permissions.
```

## Major workflow: product access denied

```text
1. User selects product.
2. NexArr denies launch.
3. Denial reason is shown.
4. Optional request-access workflow starts.
5. Admin grants entitlement/access if appropriate.
6. Product launcher updates.
```

## Major workflow: service-to-service call

```text
1. Product authenticates as ServiceClient.
2. Product requests ServiceToken.
3. NexArr validates client, tenant, scopes, and audience.
4. NexArr issues token.
5. Source product calls target product.
6. Target product validates token.
7. Target product validates scope.
8. Target product performs action or rejects.
9. Service audit is retained.
```

## Major workflow: account disable/termination sync

```text
1. StaffArr terminates or suspends person.
2. StaffArr publishes person status event.
3. NexArr disables or suspends PlatformAccount according to policy.
4. NexArr revokes active sessions if required.
5. Product access grants may be revoked/suspended.
6. Products stop accepting new product sessions for that person.
```

## Major workflow: entitlement suspension

```text
1. Platform admin or billing/integration suspends ProductEntitlement.
2. NexArr marks entitlement suspended.
3. Product launch is denied for that product.
4. Product receives entitlement suspended event.
5. Existing product sessions may be allowed to expire or revoked depending policy.
```

## NexArr emitted events

```text
nexarr.tenant.created
nexarr.tenant.updated
nexarr.tenant.status_changed
nexarr.tenant.suspended
nexarr.tenant.canceled
nexarr.tenant.archived

nexarr.membership.invited
nexarr.membership.activated
nexarr.membership.suspended
nexarr.membership.removed

nexarr.account.created
nexarr.account.invited
nexarr.account.activated
nexarr.account.updated
nexarr.account.locked
nexarr.account.unlocked
nexarr.account.disabled
nexarr.account.password_reset_requested
nexarr.account.password_changed
nexarr.account.mfa_enabled
nexarr.account.mfa_disabled

nexarr.login.succeeded
nexarr.login.failed
nexarr.login.mfa_required
nexarr.login.mfa_failed
nexarr.login.lockout_triggered

nexarr.session.created
nexarr.session.refreshed
nexarr.session.revoked
nexarr.session.expired

nexarr.product.registered
nexarr.entitlement.created
nexarr.entitlement.activated
nexarr.entitlement.suspended
nexarr.entitlement.expired
nexarr.entitlement.canceled

nexarr.product_access.granted
nexarr.product_access.suspended
nexarr.product_access.revoked
nexarr.product_access.decision_evaluated

nexarr.product_launch.created
nexarr.product_launch.denied
nexarr.product_launch.redeemed
nexarr.handoff_token.issued
nexarr.handoff_token.redeemed
nexarr.handoff_token.expired
nexarr.handoff_redemption.rejected

nexarr.service_client.created
nexarr.service_token.issued
nexarr.service_token.revoked
nexarr.service_token.expired

nexarr.security_policy.updated
nexarr.audit.entry_created
nexarr.suspicious_activity.detected
```

## Integration APIs NexArr should expose

```text
GET /api/v1/platform/me
GET /api/v1/platform/me/products
GET /api/v1/platform/me/sessions

GET /api/v1/platform/tenants
GET /api/v1/platform/tenants/{tenantId}
POST /api/v1/platform/tenants
PATCH /api/v1/platform/tenants/{tenantId}

GET /api/v1/platform/tenants/{tenantId}/memberships
POST /api/v1/platform/tenants/{tenantId}/memberships
PATCH /api/v1/platform/memberships/{membershipId}

GET /api/v1/platform/accounts/by-person/{personId}
POST /api/v1/platform/accounts
PATCH /api/v1/platform/accounts/{platformAccountId}
POST /api/v1/platform/accounts/{platformAccountId}/disable
POST /api/v1/platform/accounts/{platformAccountId}/unlock
POST /api/v1/platform/accounts/{platformAccountId}/password-reset

POST /api/v1/platform/login
POST /api/v1/platform/logout
POST /api/v1/platform/sessions/refresh
POST /api/v1/platform/sessions/{sessionId}/revoke

GET /api/v1/platform/products
GET /api/v1/platform/products/{productKey}
POST /api/v1/platform/products

GET /api/v1/platform/tenants/{tenantId}/entitlements
GET /api/v1/platform/tenants/{tenantId}/entitlements/{productKey}
POST /api/v1/platform/tenants/{tenantId}/entitlements
PATCH /api/v1/platform/entitlements/{entitlementId}

GET /api/v1/platform/product-access
POST /api/v1/platform/product-access/grants
POST /api/v1/platform/product-access/decisions
POST /api/v1/platform/product-access/revoke

POST /api/v1/platform/launch
POST /api/v1/platform/handoff/redeem
POST /api/v1/platform/handoff/revoke

GET /api/v1/platform/service-clients
POST /api/v1/platform/service-clients
PATCH /api/v1/platform/service-clients/{serviceClientId}
POST /api/v1/platform/service-clients/{serviceClientId}/rotate-secret
POST /api/v1/platform/service-clients/{serviceClientId}/revoke

POST /api/v1/platform/service-tokens
POST /api/v1/platform/service-tokens/introspect
POST /api/v1/platform/service-tokens/{serviceTokenId}/revoke

GET /api/v1/platform/security-policy
PATCH /api/v1/platform/security-policy
GET /api/v1/platform/audit
```

## APIs NexArr should consume

```text
StaffArr
- GET /persons/{personId}
- GET /persons/{personId}/summary
- GET /persons/{personId}/permissions
- POST /person-history-events

RecordArr
- POST /records
- GET /records/{recordId}
- POST /record-packages

ReportArr
- POST /events

Optional product health endpoints
- GET /health
- GET /version
- GET /status
```

## Permission examples

```text
nexarr.platform.read
nexarr.platform.admin

nexarr.tenants.read
nexarr.tenants.create
nexarr.tenants.update
nexarr.tenants.suspend
nexarr.tenants.cancel

nexarr.memberships.read
nexarr.memberships.invite
nexarr.memberships.suspend
nexarr.memberships.remove

nexarr.accounts.read
nexarr.accounts.create
nexarr.accounts.update
nexarr.accounts.disable
nexarr.accounts.unlock
nexarr.accounts.reset_password

nexarr.products.read
nexarr.products.manage

nexarr.entitlements.read
nexarr.entitlements.grant
nexarr.entitlements.update
nexarr.entitlements.suspend
nexarr.entitlements.cancel

nexarr.product_access.read
nexarr.product_access.grant
nexarr.product_access.revoke

nexarr.service_clients.read
nexarr.service_clients.manage
nexarr.service_tokens.issue
nexarr.service_tokens.revoke

nexarr.security_policy.read
nexarr.security_policy.manage
nexarr.audit.read
```

## Default role examples

```text
Platform Viewer
- Read tenants, products, entitlement summaries, and audit summaries.

Tenant Admin
- Manage own tenant membership and product access grants within allowed scope.

Platform Admin
- Manage tenants, product entitlements, product registry, platform access, and service clients.

Security Admin
- Manage security policies, sessions, MFA/account lockouts, and suspicious activity.

Service Integration Admin
- Manage service clients, scopes, token rotation, and integration access.

Support Admin
- View tenant/account/product access status for support without broad destructive rights.

Auditor
- Read platform audit, launch history, entitlement history, and access decisions.
```

## NexArr UI surfaces

```text
/app/nexarr
- dashboard
- product launcher
- tenants
- tenant detail
- memberships
- accounts
- product registry
- entitlements
- product access grants
- launch history
- service clients
- service tokens
- scopes
- security policies
- sessions
- login audit
- suspicious activity
- platform audit
- settings
```

## Tenant detail UI

```text
TenantDetailPage
- Tenant header
- Status
- Deployment mode
- Product entitlements
- Members
- Product access grants
- Security policy
- Service clients
- Usage/limits snapshot
- Audit history
```

## Account detail UI

```text
AccountDetailPage
- Account header
- Person reference
- Tenant memberships
- Login identifiers
- MFA methods
- Sessions
- Product access
- Login attempts
- Lock/disable controls
- Audit history
```

## Product entitlement UI

```text
ProductEntitlementPage
- Tenant
- Product
- Status
- Tier
- Feature flags
- Usage limits
- Dependencies
- Access grants
- Event history
```

## Service client UI

```text
ServiceClientPage
- Client identity
- Owning product
- Status
- Allowed scopes
- Allowed tenants
- Token policy
- Secrets/certificates
- Rotation status
- Last used
- Audit history
```
