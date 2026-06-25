# NexArr — Workflows, Status Logic, Events, and APIs

## Tenant onboarding

1. Create tenant and security policy.
2. Invite/create initial account and membership.
3. Link StaffArr person when applicable.
4. Configure platform and product-owned settings.
5. Show all active ordinary products in the launcher.
6. Establish StaffArr roles/permissions and onboarding tasks.
7. Run Compliance Core onboarding questionnaires through tenant/product setup surfaces.

## Login

1. Validate identifier/credential or SSO assertion.
2. Evaluate lockout/risk/MFA policy.
3. Select/validate active tenant membership.
4. Create session and audit event.
5. Return the ordinary product catalog; include Compliance Core studio only for platform administrators.

## Product launch

1. Validate session and tenant membership.
2. Validate destination registry/operational state and return URL.
3. Validate platform-admin status only for Compliance Core studio.
4. Issue one-time handoff.
5. Destination redeems and applies local permissions.

## Product permission limitation

When a user opens a product but lacks an action permission, the product returns 403 and presents a clear local permission state. NexArr does not grant product permissions and does not hide the product.

## Account disable/termination sync

StaffArr status changes may request NexArr account/session action through a permissioned contract. NexArr disables login/revokes sessions; it does not edit StaffArr person truth. All actions are attributed and audited.

## Product operational incident

Platform administrators may mark a product destination degraded, maintenance, or temporarily unavailable. This status applies consistently and is not tenant-specific. Products still expose honest degraded behavior when safe.

## Events

### Tenant and membership

- `nexarr.tenant.created|activated|suspended|canceled|archived`
- `nexarr.membership.invited|activated|suspended|removed|expired`
- `nexarr.account_person_link.verified|superseded|removed`

### Account/session/security

- `nexarr.account.invited|activated|disabled|locked|unlocked`
- `nexarr.login.succeeded|failed`
- `nexarr.mfa.enrolled|challenged|succeeded|failed|removed`
- `nexarr.session.created|refreshed|revoked|expired`
- `nexarr.refresh_token.reuse_detected`
- `nexarr.password_reset.requested|delivery_accepted|completed|failed`

### Launch/platform administration

- `nexarr.product_launch.requested|issued|redeemed|denied|expired|revoked`
- `nexarr.product_registry.updated`
- `nexarr.product_operational_state.changed`
- `nexarr.platform_admin.granted|revoked`
- `nexarr.compliancecore_studio_access.denied`

### Service trust

- `nexarr.service_client.created|disabled`
- `nexarr.service_secret.rotated`
- `nexarr.service_token.issued|revoked|introspected`
- `nexarr.service_call.denied`

## APIs NexArr exposes

### Tenant and membership

```text
GET/POST /api/v1/platform/tenants
GET/PATCH /api/v1/platform/tenants/{tenantId}
GET/POST /api/v1/platform/tenants/{tenantId}/memberships
GET/PATCH /api/v1/platform/memberships/{membershipId}
POST /api/v1/platform/account-person-links/{linkId}/verify
```

### Session and account

```text
POST /api/v1/platform/auth/login
POST /api/v1/platform/auth/refresh
POST /api/v1/platform/auth/logout
POST /api/v1/platform/password-reset/request
POST /api/v1/platform/password-reset/complete
GET /api/v1/platform/session/context
POST /api/v1/platform/sessions/{sessionId}/revoke
```

### Product registry and launch

```text
GET /api/v1/platform/products
GET /api/v1/platform/products/launcher
GET/PATCH /api/v1/platform/products/{productKey}/operational-state
POST /api/v1/platform/handoff/issue
POST /api/v1/platform/handoff/redeem
GET /api/v1/platform/launch-sessions/{launchSessionId}
```

No session-context endpoint exists.

### Service clients

```text
GET/POST /api/v1/platform/service-clients
POST /api/v1/platform/service-clients/{id}/rotate-secret
POST /api/v1/platform/service-tokens/issue
POST /api/v1/platform/service-tokens/introspect
POST /api/v1/platform/service-tokens/revoke
```

## Permissions

Examples:

- `nexarr.tenants.read|manage|suspend`
- `nexarr.memberships.read|invite|suspend|remove`
- `nexarr.accounts.read|provision|disable|recover`
- `nexarr.sessions.read|revoke`
- `nexarr.product_registry.read|manage`
- `nexarr.product_operational_state.manage`
- `nexarr.service_clients.read|manage|rotate`
- `nexarr.platform_admin.read|manage`
- `nexarr.audit.read|export`

These permissions govern platform administration. Product-domain permissions are evaluated by owning products and commonly assigned/projected through StaffArr.

## UI surfaces

- Sign in, MFA, recovery, tenant selection
- Suite launcher/product switcher
- My account, sessions/devices, security, preferences
- Tenant and membership administration
- Account provisioning/recovery actions, including delegated StaffArr-backed flows
- Product registry and operational status
- Service clients/scopes
- Platform administrators and break-glass audit
- Platform access/security audit

There is no product-access-gate page.

## Compliance Core studio

The launcher and all admin routes expose Compliance Core studio only to platform administrators. Runtime use from other products follows authenticated tenant/service contracts and is not blocked by studio visibility.
