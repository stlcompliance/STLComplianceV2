# NexArr End Goal and Granular Feature Set

## 1. End Goal

NexArr is the secure front door and platform control plane for the STL Compliance / Arr ecosystem.

Its job is not to run maintenance, routing, training, staffing, supply, or compliance workflows directly. Its job is to make every product trust the same platform-level truth before those products begin their own work.

At completion, NexArr should provide:

- One trusted login and product-launch experience for all Arr products.
- One platform-level tenant and entitlement system.
- One canonical platform identity gate for users and service clients.
- One product registry that knows what products exist, what tenants can access, and how each product is launched.
- One secure handoff mechanism from NexArr into product applications.
- One platform-admin console for managing tenants, products, product access, service credentials, and platform-level access.
- One audit trail for platform access, entitlement decisions, launch events, and service-to-service trust.
- A clean boundary where NexArr validates identity, tenant membership, and product entitlement, then hands off to the product that owns its own domain permissions and behavior.

NexArr should feel like an enterprise SaaS identity and product-access layer: boring, fast, secure, predictable, and hard to accidentally bypass.

---

## 2. Platform Role

NexArr owns platform access.

That means NexArr answers questions like:

- Is this person allowed to log in?
- Does this person belong to this tenant?
- Is this tenant valid and active?
- Is this tenant entitled to this product?
- Is this user allowed to launch this product for this tenant?
- Is this service client allowed to call another product on behalf of the platform?
- Is this request coming from a trusted product or platform service?
- What products are available to this tenant?
- Where should the user be sent after product launch?

NexArr does **not** own:

- MaintainArr work orders, assets, inspections, PMs, or maintenance authorization.
- RoutArr routes, dispatch, driver assignment, transportation authorization, or trip rules.
- StaffArr rich personnel records, departments, teams, positions, reporting structure, or product-specific people operations.
- TrainArr training workflows, evaluations, certificates, or training-derived qualifications.
- SupplyArr vendors, customers, parts, purchase workflows, or supply approvals.
- Compliance Core rule packs, compliance findings, regulatory mappings, or normalized compliance evaluations.

NexArr may own the minimal platform identity/person record needed for login and cross-product identity linkage, while StaffArr owns the richer workforce/personnel model.

---

## 3. Core Design Principles

### 3.1 NexArr Is the Only Product Login Gate

Products should not maintain separate login systems that compete with NexArr.

Each product may have its own local session after launch, but the source of login truth is NexArr.

Products should accept:

- NexArr launch handoff.
- NexArr-issued service/user tokens where appropriate.
- NexArr tenant and entitlement confirmation.

Products should reject:

- Direct unauthenticated access to protected product UIs.
- Locally invented platform-admin bypasses.
- Hardcoded tenant assumptions.
- Frontend-only entitlement checks.
- Product-owned platform login behavior.

### 3.2 Products Own Domain Authorization After Handoff

NexArr says:

> This user is real, this tenant is real, and this tenant may access this product.

The product says:

> Given that trusted platform identity, what can this user do inside this product?

Examples:

- MaintainArr decides whether the user can close a work order.
- RoutArr decides whether the user can dispatch a driver.
- StaffArr decides whether the user can edit a personnel record.
- TrainArr decides whether the user can sign off an evaluation.
- SupplyArr decides whether the user can approve a purchase order.
- Compliance Core decides whether the user can edit rule packs, assuming the platform says they are a platform admin.

#### Permission Assignment and Enforcement Boundary

NexArr must not become the product permission system. The canonical split is:

- **NexArr** owns login, tenant membership, platform roles, product entitlement, launch authorization, and service trust.
- **StaffArr** owns the person-to-permission assignment ledger, permission templates, assignment scopes, approval history, and workforce authorization visibility.
- **Each product** owns its own permission catalog, domain authorization rules, sensitive-action checks, and server-side enforcement.

A product session is valid only after NexArr validates identity, tenant, and entitlement. A sensitive product action is valid only after the product backend evaluates its own authorization rules using StaffArr-provided assignments and any applicable Compliance Core outcomes.

### 3.3 Tenant Entitlement Is Server-Enforced

Product access must be enforced by backend code, not merely hidden in the UI.

A tenant being “entitled” to a product should be checked consistently across:

- NexArr admin console.
- NexArr launch endpoint.
- Product callback endpoint.
- Product API session creation.
- Product quick-switch metadata.
- Service-to-service API calls.

### 3.4 Service Clients Are First-Class Platform Citizens

Each product should authenticate to NexArr and to other products using platform-issued service credentials.

Service clients should have:

- Client ID.
- Client secret or private key.
- Product association.
- Tenant scope rules where needed.
- Allowed audience/resource list.
- Rotation support.
- Audit trail.
- Revocation support.

### 3.5 No Backdoor Ownership Drift

No product should gain platform authority just because its frontend needs a button or its backend needs convenience access.

Examples of forbidden drift:

- RoutArr deciding which tenants exist.
- MaintainArr deciding whether a tenant is entitled to StaffArr.
- StaffArr issuing NexArr login sessions.
- TrainArr granting platform-admin status.
- Compliance Core allowing access without NexArr platform-admin validation.
- Frontend quick-switch menus generating product access without backend entitlement checks.

---

## 4. Major System Areas

## 4.1 Authentication and Login

### End State

NexArr provides a clean, secure login experience for human users and a separate credential model for services.

### Features

- Email/password login.
- Secure password hashing.
- Password reset flow.
- Optional email verification.
- Optional multi-factor authentication.
- Session management.
- Refresh token support.
- Secure logout.
- Device/session listing.
- Revoke session.
- Account lockout / throttling.
- Failed login tracking.
- Suspicious login detection.
- Remembered device support.
- Login audit events.
- Password policy configuration.
- Admin password reset.
- Disabled-account enforcement.
- Tenant-disabled enforcement.
- Product-disabled enforcement.
- Secure cookie configuration.
- CSRF protection where applicable.
- SameSite cookie handling for product handoff.
- HTTPS-only production behavior.
- Configurable token lifetime.
- Configurable refresh token lifetime.
- Token revocation strategy.
- Production-safe secret handling.

### Completion Criteria

- A user can log in once through NexArr and launch entitled products without re-entering credentials.
- Disabled users cannot launch products.
- Disabled tenants cannot launch products.
- Products cannot create valid sessions without NexArr trust.
- Failed login and launch attempts are auditable.

---

## 4.2 Platform Identity

### End State

NexArr owns the minimal platform identity needed to authenticate humans and link them across products.

### Features

- Platform person/user record.
- Stable `personId`.
- Login capability flag such as `canLogin` or `hasUserAccount`.
- Credential fields required only when login capability is enabled.
- User status:
  - active
  - invited
  - disabled
  - locked
  - pending verification
- Display name.
- Primary email.
- Optional secondary email.
- Phone number placeholder.
- Avatar/profile placeholder.
- Last login timestamp.
- Last product launch timestamp.
- Tenant memberships.
- Platform roles.
- Product launch eligibility.
- External identity provider mappings.
- Audit history for identity changes.
- API for products to resolve platform identity by `personId`.
- API for products to create minimal platform persons when permitted.
- API for StaffArr to enrich or synchronize people state as needed.

### Completion Criteria

- Every human that can log in has a stable platform identity.
- Products reference the same platform `personId`.
- No product invents a competing platform user ID.
- Raw passwords are never stored.
- Products can safely identify the same person across the ecosystem.

---

## 4.3 Tenant Management

### End State

NexArr owns platform tenant existence, tenant status, and tenant-level product access.

### Features

- Tenant creation.
- Tenant update.
- Tenant disable/enable.
- Tenant archive.
- Tenant display name.
- Tenant legal name.
- Tenant slug/code.
- Tenant status:
  - active
  - trial
  - suspended
  - disabled
  - archived
- Tenant contact information.
- Tenant billing metadata placeholder.
- Tenant product entitlements.
- Tenant service settings.
- Tenant environment metadata.
- Tenant launch domains.
- Tenant product dependency metadata.
- Tenant subscription tier placeholder.
- Tenant-level audit log.
- Tenant membership assignment.
- Tenant membership removal.
- Tenant switcher support.
- Multi-tenant user support.
- Tenant hint handling during launch.
- Tenant selection after login.
- Tenant-specific product catalog.
- Tenant-specific feature flag placeholder.
- Tenant data-plane configuration placeholder for self-hosted/hybrid product data.

### Completion Criteria

- NexArr is the source of truth for whether a tenant exists.
- Products cannot onboard a tenant independently without NexArr.
- Tenant disablement blocks all product launch.
- Multi-tenant users can choose the correct tenant before product launch.
- Tenant entitlement state is consistently enforced by backend code.

---

## 4.4 Product Registry

### End State

NexArr knows every product in the Arr ecosystem and how each product is launched.

### Features

- Product code.
- Product display name.
- Product description.
- Product icon/logo metadata.
- Product category.
- Product active/inactive status.
- Product launch URL.
- Product callback URL allowlist.
- Product API base URL.
- Product health URL.
- Product service audience.
- Product dependency metadata.
- Product required entitlement.
- Product minimum tier metadata.
- Product available-to-tenant flag.
- Product documentation URL placeholder.
- Product marketing URL placeholder.
- Product support URL placeholder.
- Product environment:
  - local
  - development
  - staging
  - production
- Product owner metadata.
- Product quick-switch visibility.
- Product sort order.
- Product status:
  - available
  - coming soon
  - maintenance
  - disabled
- Product launch method:
  - handoff code
  - signed token
  - service exchange
- Product callback validation rules.
- Product secret/client association.

### Completion Criteria

- Products can be added to the platform without hardcoding launch behavior throughout the codebase.
- Product launch URLs are validated against registered product configuration.
- Disabled products cannot be launched.
- Product registry drives the NexArr app launcher and product quick-switch metadata.

---

## 4.5 Tenant Product Entitlements

### End State

NexArr owns which tenants can access which products.

### Features

- Assign product entitlement to tenant.
- Remove product entitlement from tenant.
- Entitlement status:
  - active
  - trial
  - suspended
  - expired
  - disabled
- Entitlement access level:
  - none
  - read-only/demo
  - standard
  - full
  - platform-admin/internal
- Entitlement effective date.
- Entitlement expiration date.
- Subscription tier placeholder.
- Product dependency enforcement.
- Entitlement reason/comment.
- Entitlement audit trail.
- Bulk entitlement assignment.
- Entitlement template support.
- Tenant product catalog API.
- Entitlement check API.
- Product launch entitlement check.
- Product callback entitlement check.
- Service-token entitlement check.
- UI visibility based on entitlement.
- Backend rejection when entitlement is missing.
- Support for future billing integration.

### Completion Criteria

- Entitling a tenant to RoutArr in NexArr allows RoutArr launch when all other requirements are valid.
- Removing a tenant’s entitlement blocks launch and product session creation.
- Entitlement checks behave consistently across all products.
- Entitlement decisions are logged.

---

## 4.6 Product Launch and Handoff

### End State

NexArr provides a secure, short-lived product handoff flow.

### Features

- Launch endpoint:
  - `/launch/{productCode}`
- Optional query parameters:
  - `productCode`
  - `returnUrl`
  - `tenantHint`
  - `state`
- Tenant selection when missing or ambiguous.
- Product entitlement validation.
- Product callback allowlist validation.
- Short-lived handoff code generation.
- One-time-use handoff codes.
- Handoff code expiration.
- Handoff code redemption endpoint.
- State preservation.
- Redirect to product callback.
- Product session bootstrap data.
- Signed launch payload.
- User identity claims.
- Tenant claims.
- Product claims.
- Entitlement claims.
- Platform role claims.
- Anti-replay protection.
- Launch failure page.
- Friendly entitlement error page.
- Friendly tenant-selection error page.
- Launch audit events.
- Callback audit events.
- Product health check before launch, optional.
- Return URL normalization.
- Environment-aware URL configuration.
- Support for local development callbacks.
- Support for production callbacks.
- Support for deep links into products.

### Completion Criteria

- A user can click a product tile in NexArr and land in the product as an authenticated user.
- A product cannot redeem an expired or reused handoff code.
- A product cannot redeem a handoff intended for another product.
- A launch with missing entitlement fails clearly.
- A launch with invalid callback URL fails safely.
- Product handoff works consistently across MaintainArr, RoutArr, StaffArr, SupplyArr, TrainArr, and Compliance Core.

---

## 4.7 App Launcher

### End State

NexArr provides the platform home screen where users see the products available to them.

### Features

- Product tile grid.
- Entitled products visible as launchable.
- Non-entitled products hidden or shown as unavailable depending on configuration.
- Product status badges.
- Recently used products.
- Tenant selector.
- User account menu.
- Platform admin entry point.
- Product search/filter.
- Product grouping.
- Responsive layout.
- Product icons/logos.
- Launch error display.
- Product unavailable display.
- Support/contact link.
- Branding for STL Compliance.
- White-label-ready tenant branding placeholder.
- Quick explanation of what each product does.

### Completion Criteria

- Users immediately understand which products they can access.
- Launching a product from the app launcher uses the secure handoff flow.
- The UI never implies access that the backend would reject.

---

## 4.8 Cross-Product Quick Switch Support

### End State

Products can display a unified quick-switch menu powered by NexArr, without owning platform access decisions.

### Features

- Product catalog endpoint for current user and tenant.
- Entitled products only.
- Product launch links generated by NexArr.
- Product status included.
- Current product identification.
- Tenant context included.
- User context included.
- No frontend-generated trust.
- No product-side entitlement guessing.
- Optional product dependency metadata.
- Optional recent-product metadata.
- Optional unread/attention counts from products later.
- Safe fallback when NexArr is unavailable.
- Cache rules for product catalog data.
- Forced refresh after entitlement changes.

### Completion Criteria

- Quick switch works across products.
- Quick switch cannot bypass NexArr.
- Clicking a product from quick switch routes through NexArr launch.
- Product frontends do not contain hardcoded entitlement authority.

---

## 4.9 Platform Admin Console

### End State

NexArr has the admin tools needed to operate the platform.

### Features

#### Tenant Administration

- Create tenant.
- Edit tenant.
- Disable tenant.
- Archive tenant.
- View tenant status.
- View tenant users.
- View tenant entitlements.
- View tenant launch history.
- View tenant service clients.
- Assign products.
- Remove products.
- Change entitlement access level.
- Change entitlement dates.
- Add tenant notes.
- View tenant audit log.

#### Product Administration

- Create product registry entry.
- Edit product.
- Enable/disable product.
- Configure launch URL.
- Configure callback allowlist.
- Configure API URL.
- Configure product status.
- Configure product dependencies.
- Configure product display metadata.
- View product tenants.
- View product launch activity.
- View product service clients.

#### User / Platform Identity Administration

- Search users.
- Create platform user.
- Invite user.
- Disable user.
- Lock/unlock user.
- Reset password.
- Assign tenant membership.
- Remove tenant membership.
- Assign platform role.
- Remove platform role.
- View login history.
- View product launch history.
- View identity audit log.

#### Service Client Administration

- Create service client.
- Rotate secret.
- Revoke client.
- Assign product audience.
- Assign tenant scope.
- View last used.
- View failed authentication attempts.
- View service-token audit log.

#### Platform Admin Security

- Platform admin role enforcement.
- Super-admin or owner role.
- Break-glass admin account strategy.
- Admin action audit logging.
- Sensitive action confirmation.
- Admin session timeout.
- Optional MFA requirement for platform admins.

### Completion Criteria

- A platform admin can fully onboard a tenant and enable product access without database edits.
- A platform admin can diagnose why a user cannot launch a product.
- A platform admin can rotate service credentials without code changes.
- All sensitive admin actions are auditable.

---

## 4.10 Platform Roles

### End State

NexArr owns only platform-level roles, not product-domain roles.

### Suggested Roles

- Platform Owner
- Platform Admin
- Platform Support
- Tenant Admin
- Tenant User
- Service Client
- Product Service
- Read-Only Auditor

### Features

- Role assignment.
- Role removal.
- Role audit trail.
- Tenant-scoped platform roles.
- Global platform roles.
- Platform-admin-only routes.
- Tenant-admin-only routes.
- Role-based admin console visibility.
- Backend authorization policies.
- UI hiding for unauthorized admin features.
- Explicit denial responses.

### Completion Criteria

- Platform roles do not replace product roles.
- A platform admin can manage platform access.
- A product admin inside MaintainArr, RoutArr, StaffArr, TrainArr, SupplyArr, or Compliance Core is still enforced by that product unless explicitly elevated by platform policy.

---

## 4.11 Service-to-Service Authentication

### End State

All product APIs can trust platform-issued service credentials and signed service tokens.

### Features

- Service client registration.
- Service client secret storage.
- Secret hashing or secure storage.
- Secret rotation.
- Secret revocation.
- JWT issuance for service clients.
- Audience-specific service tokens.
- Product-specific service identities.
- Tenant-scoped service calls.
- Cross-product call policy.
- Token expiration.
- Token signing key management.
- RSA private key configuration.
- Public key discovery endpoint.
- JWKS endpoint.
- Service-token validation middleware.
- Service-token audit log.
- Failed service authentication audit log.
- Allowed products list.
- Allowed scopes list.
- Least-privilege defaults.
- Environment-specific clients.
- Local development client support.

### Completion Criteria

- MaintainArr can call NexArr using its configured service client and secret.
- RoutArr can call NexArr using its configured service client and secret.
- Products can validate NexArr-issued tokens without sharing raw secrets.
- Revoked service clients stop working.
- Misconfigured service clients fail clearly.

---

## 4.12 API Surface

### End State

NexArr exposes a stable API for platform identity, tenant access, product registry, entitlement checks, handoff redemption, and service authentication.

### Core API Groups

#### Auth

- `POST /api/v1/auth/login`
- `POST /api/v1/auth/logout`
- `POST /api/v1/auth/refresh`
- `POST /api/v1/auth/password/forgot`
- `POST /api/v1/auth/password/reset`
- `GET /api/v1/auth/me`
- `GET /api/v1/auth/sessions`
- `DELETE /api/v1/auth/sessions/{id}`

#### Tenants

- `GET /api/v1/tenants`
- `POST /api/v1/tenants`
- `GET /api/v1/tenants/{tenantId}`
- `PATCH /api/v1/tenants/{tenantId}`
- `POST /api/v1/tenants/{tenantId}/disable`
- `POST /api/v1/tenants/{tenantId}/enable`
- `GET /api/v1/tenants/{tenantId}/members`
- `POST /api/v1/tenants/{tenantId}/members`
- `DELETE /api/v1/tenants/{tenantId}/members/{personId}`

#### Products

- `GET /api/v1/products`
- `POST /api/v1/products`
- `GET /api/v1/products/{productCode}`
- `PATCH /api/v1/products/{productCode}`
- `POST /api/v1/products/{productCode}/disable`
- `POST /api/v1/products/{productCode}/enable`

#### Entitlements

- `GET /api/v1/tenants/{tenantId}/entitlements`
- `POST /api/v1/tenants/{tenantId}/entitlements`
- `PATCH /api/v1/tenants/{tenantId}/entitlements/{productCode}`
- `DELETE /api/v1/tenants/{tenantId}/entitlements/{productCode}`
- `GET /api/v1/entitlements/check`

#### Launch / Handoff

- `GET /launch/{productCode}`
- `POST /api/v1/handoff/redeem`
- `GET /api/v1/launch/catalog`
- `POST /api/v1/launch/validate`

#### Service Auth

- `POST /api/v1/service-token`
- `GET /api/v1/.well-known/jwks.json`
- `GET /api/v1/service-clients`
- `POST /api/v1/service-clients`
- `POST /api/v1/service-clients/{id}/rotate`
- `POST /api/v1/service-clients/{id}/revoke`

#### Audit

- `GET /api/v1/audit/events`
- `GET /api/v1/audit/events/{id}`
- `GET /api/v1/audit/tenants/{tenantId}`
- `GET /api/v1/audit/products/{productCode}`
- `GET /api/v1/audit/users/{personId}`

#### Health

- `GET /health`
- `GET /ready`
- `GET /api/v1/system/status`

### Completion Criteria

- Product integrations can be built against documented API contracts.
- API responses are consistent.
- Errors are clear and safe.
- Admin APIs are protected by platform-admin authorization.
- Service APIs require service authentication.

---

## 4.13 Eventing and Integration

### End State

NexArr emits platform events so products and Compliance Core can react to platform changes without direct database coupling.

### Events

- `tenant.created`
- `tenant.updated`
- `tenant.disabled`
- `tenant.enabled`
- `tenant.archived`
- `tenant.entitlement.granted`
- `tenant.entitlement.updated`
- `tenant.entitlement.revoked`
- `user.created`
- `user.updated`
- `user.disabled`
- `user.enabled`
- `user.locked`
- `user.unlocked`
- `tenant.membership.added`
- `tenant.membership.removed`
- `product.created`
- `product.updated`
- `product.disabled`
- `product.enabled`
- `serviceClient.created`
- `serviceClient.rotated`
- `serviceClient.revoked`
- `launch.succeeded`
- `launch.failed`
- `handoff.redeemed`
- `handoff.failed`

### Features

- Event outbox table.
- Event publisher worker.
- Retry handling.
- Dead-letter handling.
- Event schema versioning.
- Idempotency key.
- Correlation ID.
- Causation ID.
- Tenant ID where applicable.
- Actor person ID where applicable.
- Actor service client ID where applicable.
- Product code where applicable.
- Webhook delivery placeholder.
- Message bus support placeholder.
- Compliance Core event ingestion support.
- Product event subscription model.

### Completion Criteria

- Products can mirror platform tenant/product/person references without cross-database foreign keys.
- Events are idempotent and traceable.
- Failed event delivery is visible and recoverable.
- Platform events do not expose secrets.

---

## 4.14 Audit Logging

### End State

NexArr provides a trustworthy audit trail for platform security and entitlement decisions.

### Audit Event Types

- Login succeeded.
- Login failed.
- Logout.
- Password changed.
- Password reset requested.
- Password reset completed.
- MFA changed.
- User created.
- User disabled.
- User enabled.
- User locked.
- User unlocked.
- Tenant created.
- Tenant updated.
- Tenant disabled.
- Tenant enabled.
- Product created.
- Product updated.
- Product disabled.
- Product enabled.
- Entitlement granted.
- Entitlement changed.
- Entitlement revoked.
- Product launch succeeded.
- Product launch failed.
- Handoff created.
- Handoff redeemed.
- Handoff expired.
- Handoff rejected.
- Service client created.
- Service client rotated.
- Service client revoked.
- Service token issued.
- Service token rejected.
- Admin role granted.
- Admin role removed.
- Configuration changed.

### Audit Fields

- Event ID.
- Event type.
- Timestamp.
- Actor type.
- Actor person ID.
- Actor service client ID.
- Target type.
- Target ID.
- Tenant ID.
- Product code.
- IP address.
- User agent.
- Correlation ID.
- Request ID.
- Before/after summary.
- Result:
  - success
  - failure
  - denied
- Failure reason.
- Metadata JSON.

### Completion Criteria

- Admins can reconstruct why access was allowed or denied.
- Security-relevant actions are never silent.
- Audit events survive normal application errors.
- Audit records are searchable.

---

## 4.15 Security Baseline

### End State

NexArr follows secure-by-default enterprise SaaS practices.

### Features

- HTTPS required in production.
- Secure cookies.
- HttpOnly cookies.
- SameSite cookie configuration.
- CSRF protections where needed.
- CORS allowlist.
- Rate limiting.
- Login throttling.
- Password hashing.
- Secret redaction in logs.
- No raw password logging.
- No raw token logging.
- No frontend-exposed secrets.
- Strict callback URL validation.
- Open redirect protection.
- Token audience validation.
- Token issuer validation.
- Token expiration validation.
- Replay protection for handoff codes.
- Least-privilege service clients.
- Environment-based configuration.
- Production secret validation on startup.
- Strong random key generation.
- RSA signing key support.
- JWKS support.
- Dependency vulnerability review process.
- Security headers:
  - Content-Security-Policy
  - X-Content-Type-Options
  - Referrer-Policy
  - X-Frame-Options or frame-ancestors
- Admin action confirmation.
- Platform-admin route protection.
- Backend authorization on all protected operations.
- No frontend-only access control.
- Explicit deny-by-default policies.

### Completion Criteria

- A missing or invalid entitlement cannot be bypassed from the frontend.
- A forged callback URL cannot steal handoff codes.
- A reused handoff code is rejected.
- A product cannot use another product’s service identity.
- Production fails fast when required secrets are missing.

---

## 4.16 Configuration and Environment Management

### End State

NexArr is configurable across local, staging, and production without code edits.

### Features

- Environment-based configuration.
- Database connection string.
- Public platform URL.
- Allowed origins.
- Cookie domain.
- Token issuer.
- Token audience.
- RSA private key.
- Service client settings.
- Product registry seed settings.
- Admin bootstrap settings.
- Seed platform owner account.
- Seed products.
- Seed development tenant.
- Seed service clients for local development.
- Separate local/staging/production URLs.
- Render-compatible environment variables.
- Docker-compatible environment variables.
- Startup validation.
- Safe defaults for local dev.
- Strict requirements for production.
- Secret rotation documentation.

### Completion Criteria

- Local development works without production secrets.
- Production refuses unsafe placeholder secrets.
- Product callback URLs do not need hardcoded code changes.
- Every app can point at NexArr using consistent service client environment variables.

---

## 4.17 Data Model

### End State

NexArr has a lean but complete platform-control-plane schema.

### Core Tables

#### `tenants`

- `id`
- `name`
- `slug`
- `legal_name`
- `status`
- `created_at`
- `updated_at`
- `disabled_at`
- `metadata`

#### `platform_persons`

- `id`
- `display_name`
- `primary_email`
- `status`
- `can_login`
- `created_at`
- `updated_at`
- `last_login_at`
- `metadata`

#### `user_credentials`

- `person_id`
- `password_hash`
- `password_changed_at`
- `mfa_enabled`
- `locked_until`
- `failed_login_count`
- `created_at`
- `updated_at`

#### `tenant_memberships`

- `id`
- `tenant_id`
- `person_id`
- `status`
- `created_at`
- `updated_at`

#### `platform_roles`

- `id`
- `code`
- `name`
- `description`

#### `platform_role_assignments`

- `id`
- `person_id`
- `tenant_id`
- `role_id`
- `created_at`
- `created_by`

#### `products`

- `id`
- `code`
- `name`
- `description`
- `status`
- `launch_url`
- `api_base_url`
- `callback_allowlist`
- `icon`
- `sort_order`
- `metadata`

#### `tenant_product_entitlements`

- `id`
- `tenant_id`
- `product_code`
- `status`
- `access_level`
- `effective_at`
- `expires_at`
- `created_at`
- `updated_at`

#### `product_handoff_codes`

- `id`
- `code_hash`
- `tenant_id`
- `person_id`
- `product_code`
- `return_url`
- `state`
- `expires_at`
- `redeemed_at`
- `created_at`
- `metadata`

#### `service_clients`

- `id`
- `client_id`
- `client_secret_hash`
- `product_code`
- `status`
- `allowed_audiences`
- `allowed_scopes`
- `created_at`
- `updated_at`
- `last_used_at`

#### `audit_events`

- `id`
- `event_type`
- `timestamp`
- `actor_type`
- `actor_id`
- `target_type`
- `target_id`
- `tenant_id`
- `product_code`
- `result`
- `reason`
- `correlation_id`
- `metadata`

#### `outbox_events`

- `id`
- `event_type`
- `payload`
- `occurred_at`
- `published_at`
- `retry_count`
- `status`

### Completion Criteria

- Schema supports all current platform responsibilities.
- Schema avoids direct ownership of product-domain records.
- Schema supports auditability.
- Schema supports service-client trust.
- Schema supports future billing and hybrid deployment metadata without forcing it into v1 workflows.

---

## 4.18 Frontend UX

### End State

The NexArr frontend should look and feel like the enterprise control center for platform access.

### Primary Screens

- Login.
- Password reset.
- Tenant selector.
- Product launcher.
- Product launch failure page.
- Account/profile page.
- Session management page.
- Platform admin dashboard.
- Tenant list.
- Tenant detail.
- Tenant entitlement management.
- Product registry list.
- Product registry detail.
- User/person search.
- User/person detail.
- Service clients list.
- Service client detail.
- Audit log viewer.
- System status page.

### UX Requirements

- Clean app shell.
- Consistent sidebar/header.
- Product quick switch ready.
- Platform admin navigation separated from normal user launcher.
- Clear empty states.
- Clear error states.
- Clear launch-denied messages.
- Responsive layout.
- Accessible forms.
- Keyboard-friendly navigation.
- Loading states.
- Toast notifications.
- Confirmation modals for sensitive actions.
- Search and filtering for admin tables.
- Pagination for large datasets.
- Tenant context visible.
- Current user visible.
- No hidden authority in frontend-only logic.

### Completion Criteria

- A non-technical admin can onboard a tenant.
- A non-technical admin can entitle a tenant to a product.
- A non-technical admin can diagnose product launch failure.
- Users see a simple product launcher, not a confusing admin system.

---

## 4.19 Observability and Operations

### End State

NexArr can be operated confidently in production.

### Features

- Health endpoint.
- Readiness endpoint.
- Database connectivity check.
- Product registry health summary.
- Structured logging.
- Correlation IDs.
- Request IDs.
- Error IDs.
- Launch flow tracing.
- Service-token tracing.
- Audit event tracing.
- Admin action tracing.
- Metrics placeholder.
- Failed login metrics.
- Failed launch metrics.
- Handoff redemption metrics.
- Service-auth failure metrics.
- Product entitlement failure metrics.
- Background worker status.
- Outbox worker status.
- Alerting placeholder.
- Safe error pages.
- No secret leakage in logs.
- Admin-visible system status.

### Completion Criteria

- Platform admins can tell whether NexArr itself is healthy.
- Developers can trace a failed launch from NexArr to the product callback.
- Service-client misconfiguration is visible quickly.
- Logs are safe to share for troubleshooting.

---

## 4.20 Billing and Licensing Readiness

### End State

NexArr does not need full billing in v1, but its entitlement model should be ready for billing later.

### Features

- Tenant subscription tier field.
- Product entitlement effective date.
- Product entitlement expiration date.
- Product access level.
- Billing customer ID placeholder.
- Billing subscription ID placeholder.
- Trial status.
- Suspended status.
- Grace period placeholder.
- Product dependency support.
- Usage metric placeholder.
- Future invoice metadata placeholder.
- Manual entitlement override.
- Audit trail for manual override.
- Internal/free tenant marker.

### Completion Criteria

- Manual product entitlement works before billing integration exists.
- Billing can later drive entitlement changes without redesigning product access.
- Expired/suspended entitlements can block launches cleanly.

---

## 4.21 Hybrid / Customer-Hosted Data Plane Readiness

### End State

NexArr can remain a lean hosted control plane while product data may eventually live in customer-managed environments.

### Features

- Tenant data-plane metadata.
- Product deployment mode:
  - hosted
  - customer-hosted
  - hybrid
- Product API endpoint per tenant.
- Product health endpoint per tenant.
- Data-plane registration.
- Data-plane trust status.
- Data-plane service client.
- License heartbeat placeholder.
- Offline grace placeholder.
- Data-plane configuration audit.
- Treat customer-hosted product data as untrusted input.
- Signed service communication.
- Token audience separation.
- Environment-specific product URLs.
- Admin UI for deployment mode.

### Completion Criteria

- NexArr can launch a tenant into the correct hosted or customer-hosted product endpoint.
- Customer-hosted product data does not become trusted platform truth without validation.
- Platform access remains dependent on NexArr.

---

## 5. Product-Specific Integration Expectations

## 5.1 MaintainArr Integration

NexArr should provide MaintainArr with:

- User identity.
- Tenant identity.
- Product entitlement.
- Launch handoff.
- Service-token trust.
- Product catalog/quick-switch metadata.

MaintainArr owns:

- Assets.
- Maintenance records.
- Work orders.
- Inspections.
- PMs.
- Defects.
- Maintenance labor.
- Maintenance roles and permissions.
- Maintenance authorization decisions.

## 5.2 RoutArr Integration

NexArr should provide RoutArr with:

- User identity.
- Tenant identity.
- Product entitlement.
- Launch handoff.
- Service-token trust.
- Product catalog/quick-switch metadata.

RoutArr owns:

- Routes.
- Dispatch.
- Drivers as StaffArr/NexArr-linked people.
- Vehicle/equipment assignments.
- Trip execution.
- Transportation exceptions.
- Driver/product roles.
- Operational authorization decisions.

## 5.3 StaffArr Integration

NexArr should provide StaffArr with:

- Platform identity linkage.
- Tenant identity.
- Product entitlement.
- Launch handoff.
- Service-token trust.
- Product catalog/quick-switch metadata.

StaffArr owns:

- Rich people records.
- Org structure.
- Sites/places.
- Departments.
- Positions.
- Teams.
- Manager/subordinate hierarchy.
- Active/inactive workforce state.
- Personnel history.
- Cross-product permission assignments where designed.
- Personnel audit packages.

## 5.4 TrainArr Integration

NexArr should provide TrainArr with:

- User identity.
- Tenant identity.
- Product entitlement.
- Launch handoff.
- Service-token trust.
- Product catalog/quick-switch metadata.

TrainArr owns:

- Training programs.
- Training steps.
- Evaluations.
- Signoffs.
- Training records.
- Completion rules.
- Certificates/qualifications created by training.
- Retraining workflows.
- Training-derived authorization signals.

## 5.5 SupplyArr Integration

NexArr should provide SupplyArr with:

- User identity.
- Tenant identity.
- Product entitlement.
- Launch handoff.
- Service-token trust.
- Product catalog/quick-switch metadata.

SupplyArr owns:

- Vendors.
- Customers.
- Parts.
- Inventory/purchasing workflows.
- Vendor approvals.
- Supply documents.
- Purchase authorization decisions.

## 5.6 Compliance Core Integration

NexArr should provide Compliance Core with:

- Platform-admin validation.
- User identity.
- Tenant identity where needed.
- Product entitlement where needed.
- Launch handoff.
- Service-token trust.
- Product catalog/quick-switch metadata.

Compliance Core owns:

- Rule packs.
- Regulatory mappings.
- Compliance evaluation definitions.
- Rule normalization.
- Compliance findings.
- Cross-product compliance reporting.
- Platform-level compliance administration.

Compliance Core should be accessible only to platform admins unless a future scoped auditor model is intentionally designed.

---

## 6. Implementation Milestones

## Milestone 1: Platform Foundation

- Database schema baseline.
- Tenant model.
- Product registry model.
- Platform person/user model.
- Tenant membership model.
- Product entitlement model.
- Admin bootstrap user.
- Basic login.
- Basic product launcher.
- Health endpoint.

## Milestone 2: Secure Product Launch

- Launch endpoint.
- Callback allowlist.
- Handoff code table.
- Handoff generation.
- Handoff redemption.
- Handoff expiration.
- One-time redemption.
- Product callback integration.
- Friendly launch failure UI.
- Launch audit events.

## Milestone 3: Entitlement Enforcement

- Entitlement admin UI.
- Entitlement API.
- Product launch entitlement check.
- Product callback entitlement check.
- Tenant product catalog endpoint.
- Quick-switch support endpoint.
- Product-specific entitlement debugging.

## Milestone 4: Service Client System

- Service client model.
- Client secret generation.
- Secret rotation.
- Service-token endpoint.
- JWT signing.
- JWKS endpoint.
- Product service auth middleware guidance.
- Audit service-token issuance and failure.

## Milestone 5: Admin Console

- Tenant admin screens.
- Product admin screens.
- User/person admin screens.
- Entitlement admin screens.
- Service client admin screens.
- Audit viewer.
- System status screen.

## Milestone 6: Eventing and Observability

- Audit event hardening.
- Outbox table.
- Event publisher worker.
- Product integration events.
- Correlation IDs.
- Structured logs.
- Launch traceability.
- Failed launch diagnostics.

## Milestone 7: Enterprise Hardening

- MFA.
- Password reset.
- Session management.
- Security headers.
- Rate limiting.
- Admin action confirmation.
- Production startup validation.
- Secret rotation documentation.
- Backup/restore strategy.
- Disaster recovery notes.

---

## 7. Definition of Complete

NexArr can be considered complete when all of the following are true:

### Access

- Users authenticate through NexArr.
- Products do not own competing platform login.
- Disabled users cannot access products.
- Disabled tenants cannot access products.
- Missing entitlements block launch.
- Revoked service clients stop working.

### Tenant and Product Management

- Platform admins can create tenants.
- Platform admins can register products.
- Platform admins can grant/revoke product access.
- Platform admins can manage service clients.
- Platform admins can diagnose launch failures.

### Launch

- Product launch works consistently for all products.
- Handoff is short-lived and one-time-use.
- Invalid return URLs are rejected.
- Product callbacks are allowlisted.
- State is preserved safely.
- Multi-tenant launch selection works.

### Integration

- Products can validate NexArr handoff.
- Products can use NexArr service tokens.
- Products can consume tenant/product catalog data.
- Products can use NexArr `personId` as platform identity linkage.
- Products still own their own domain authorization.

### Audit and Security

- Login attempts are audited.
- Launch attempts are audited.
- Entitlement changes are audited.
- Service-token activity is audited.
- Admin actions are audited.
- Secrets are not logged.
- Production unsafe config fails fast.

### UX

- Normal users see a simple app launcher.
- Platform admins see a useful control console.
- Errors explain what went wrong without leaking sensitive details.
- Product access state is clear and consistent.

### Architecture

- NexArr remains lean.
- Products remain separately owned.
- No cross-product database foreign keys are required.
- Cross-product communication uses APIs, events, and service tokens.
- Frontends cannot undermine ownership rules.
- NexArr remains the trusted control plane even if product data planes become customer-hosted.

---


---

## Audit-Informed Feature Additions

### 10.1 Canonical Product Manifest Contract

NexArr should own the registered product manifest used by app launch, quick switch, service trust, and deployment diagnostics.

Each product manifest should include:

- `productCode`
- Product display name
- Product owner metadata
- Product category
- Launch URL
- Canonical callback path, defaulting to `/auth/nexarr/callback`
- Callback URL allowlist
- API base URL
- Health URL
- Service-token audience
- Product status
- Entitlement dependency rules
- Product dependency metadata
- Public marketing/documentation URL
- Environment metadata
- Optional self-hosted/customer-hosted data-plane metadata

If a product requires a different callback path, the exception must live in the product manifest and be validated by NexArr. It must not be hardcoded in random product frontend code.

### 10.2 Launch Diagnostics

NexArr should provide operator-facing diagnostics for product launch failures.

Features:

- Launch attempt lookup by user, tenant, product, timestamp, and correlation ID.
- Entitlement failure explanation.
- Tenant inactive/suspended explanation.
- Product disabled/maintenance explanation.
- Callback allowlist failure explanation.
- Expired/reused handoff code explanation.
- Product health failure explanation.
- Service-client/audience failure explanation.
- Safe redaction of secrets and token values.
- Admin-visible remediation hint.

Completion criteria:

- A platform admin can explain why a product launch failed without reading raw logs.
- Failed launches are auditable and correlated across NexArr and the destination product.

### 10.3 Cross-Product Quick-Switch Catalog

NexArr should provide the only trusted product catalog used by product quick-switch menus.

Features:

- `GET /api/v1/launch/catalog` or equivalent.
- Returns only products the current user and tenant may access.
- Includes product display metadata, status, launch URL through NexArr, tenant context, and current product indicator.
- Supports cache invalidation after entitlement, tenant, product, or user-status changes.
- Does not expose hidden products as launchable.

Completion criteria:

- Product frontends can display a unified switcher without owning entitlement decisions.
- Clicking a product always routes through NexArr `/launch/{productCode}`.

### 10.4 Platform Session and Service Trust Completion

NexArr is complete only when both human and service access are operationally manageable.

Features:

- Human session list and revoke flow.
- Product launch session audit.
- Service-client creation, rotation, revocation, and last-used tracking.
- JWKS/public-key endpoint for product token validation.
- Audience and scope validation.
- Failed service-auth audit events.
- Production startup failure when signing keys or required service trust settings are missing.

Completion criteria:

- Products validate NexArr-issued trust without sharing raw secrets.
- Revoked service clients stop working.
- Wrong audience and expired service token failures are safe and auditable.


## 8. Non-Goals

NexArr should not become:

- A CMMS.
- A dispatch system.
- A training LMS.
- An HR/personnel management system beyond minimal platform identity.
- A supply chain management system.
- A compliance rule authoring system.
- A universal product database.
- A dumping ground for product-specific roles and permissions.
- A frontend-only entitlement switchboard.
- A substitute for product-owned authorization.

Keeping NexArr narrow is what makes the whole Arr ecosystem cleaner.

---

## 9. Plain-English Summary

NexArr is the bouncer, front desk, keycard system, and product directory for the platform.

It checks who you are, which company you belong to, which products that company has, and whether you are allowed through the door. Once you enter a product, that product decides what work you can do inside.

NexArr should be strong enough that every product can trust it, but narrow enough that it does not swallow the responsibilities of the products it launches.
