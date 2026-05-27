# Worker 3 — NexArr tenant, entitlement, and service-token admin

## Slice name

NexArr Platform Access Spine — tenant/entitlement admin APIs and service-token issuance/validation

## Products touched

- **NexArr** (primary): admin APIs, service clients/tokens, audit on sensitive actions
- **Shared .NET** (`STLCompliance.Shared`): service-token claim types, `StlServiceTokenOptions`, entitlement helpers on `ClaimsPrincipal`

## Schema (NexArr PostgreSQL)

Migration `NexArrTenantEntitlementAdmin`:

- `service_clients` — registered service identities (client key, source product, allowed products)
- `service_tokens` — issued token registry (JTI, hash, tenant scope, expiry, revoke)

Retains M2 identity spine tables from `NexArrIdentitySpine`.

## APIs

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/tenants` | JWT; platform admin (all) or tenant admin (own tenant) |
| GET | `/api/tenants/{id}` | JWT; platform admin or tenant admin (own tenant) |
| POST | `/api/tenants` | JWT; platform admin |
| PUT | `/api/tenants/{id}` | JWT; platform admin |
| PATCH | `/api/tenants/{id}/status` | JWT; platform admin |
| GET | `/api/products` | JWT; NexArr entitlement or platform admin |
| GET | `/api/products/{productKey}` | JWT; NexArr entitlement or platform admin |
| POST | `/api/products` | JWT; platform admin |
| PUT | `/api/products/{productKey}` | JWT; platform admin |
| GET | `/api/entitlements?tenantId=` | JWT; platform admin or tenant admin (scoped) |
| POST | `/api/entitlements` | JWT; platform admin or tenant admin (own tenant) |
| POST | `/api/entitlements/{id}/revoke` | JWT; platform admin or tenant admin (own tenant) |
| GET | `/api/service-tokens/clients` | JWT; platform admin |
| POST | `/api/service-tokens/clients` | JWT; platform admin |
| GET | `/api/service-tokens` | JWT; platform admin |
| POST | `/api/service-tokens` | JWT; platform admin |
| POST | `/api/service-tokens/validate` | JWT; NexArr entitlement or platform admin |
| POST | `/api/service-tokens/{id}/revoke` | JWT; platform admin |

## Permissions

- **Platform admin** (`stl_platform_admin=true`): full tenant/product/entitlement/service-client/service-token control.
- **Tenant admin** (`tenant_admin` membership on JWT tenant): read own tenant, list/grant/revoke entitlements for JWT tenant only.
- Service-token issuance checks tenant entitlements for scoped tokens.
- Validation checks JWT signature, registry, expiry, revocation, client active state, and live tenant entitlements.

## UI

Not in this slice (M3 suite shell).

## Worker events

None (service-token cleanup worker deferred to M12).

## Tests

`tests/STLCompliance.NexArr.Auth.Tests/NexArrAdminApiTests.cs` — tenant CRUD auth gates, entitlement grant/deny, service-token issue/validate/revoke (InMemory DB).

## Local dev seed

- Platform admin: `admin@demo.stl` / `ChangeMe!Demo2026`
- Tenant admin: `tenant-admin@demo.stl` / `ChangeMe!Demo2026`

## Gaps / next

- Product launch context, handoff codes, callback allowlist
- `/api/platform-admin/*` dashboard surfaces
- Product APIs validating NexArr JWT + service tokens on protected routes
- Suite-frontend login shell (M3)
