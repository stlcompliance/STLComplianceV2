# Worker 2 — NexArr identity auth spine

## Slice name

NexArr Platform Access Spine — authentication, sessions, and `/api/me` context

## Products touched

- **NexArr** (primary): identity DB, auth APIs, audit on login
- **Shared .NET** (`STLCompliance.Shared`): JWT validation helpers, correlation ID, API error middleware

## Schema (NexArr PostgreSQL)

Migration `NexArrIdentitySpine`:

- `platform_users`, `user_credentials`, `user_sessions`
- `tenants`, `tenant_memberships`
- `product_catalog`, `tenant_product_entitlements`
- `platform_audit_events`
- Retains M1 `platform_metadata`

## APIs

| Method | Route | Auth |
|--------|-------|------|
| POST | `/api/auth/login` | Anonymous |
| POST | `/api/auth/renew` | Anonymous |
| POST | `/api/auth/logout` | Anonymous |
| GET | `/api/me` | Bearer JWT |
| GET | `/api/me/tenants` | Bearer JWT |
| GET | `/api/me/entitlements` | Bearer JWT |
| GET | `/api/me/navigation` | Bearer JWT |

## Permissions

Login enforces active user, tenant membership, active tenant status, and at least one active product entitlement (platform admins may bypass entitlement count). JWT carries `stl_tenant_id`, `stl_entitlements`, `stl_session_id`, `stl_platform_admin`.

## UI

Not in this slice (suite-frontend M3). Demo credentials seeded for local/Docker dev.

## Worker events

None (session cleanup worker deferred to M12).

## Tests

`tests/STLCompliance.NexArr.Auth.Tests` — login success/denied, `/api/me` auth gate, navigation entitlements (InMemory DB).

## Local dev seed

- Tenant: `demo-stl`
- User: `admin@demo.stl` / `ChangeMe!Demo2026`
- All suite products entitled

## Gaps / next

- Service tokens, handoff, platform-admin CRUD
- Suite-frontend login shell
- Product APIs validating NexArr JWT on protected routes (sample on StaffArr optional next)
