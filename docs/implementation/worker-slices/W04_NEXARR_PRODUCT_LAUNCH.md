# Worker 4 — NexArr product launch context, handoff codes, callback allowlist

## Slice name

NexArr Platform Access Spine — product launch context, handoff codes, and callback allowlist validation

## Products touched

- **NexArr** (primary): launch profiles, handoff codes, callback allowlist, launch APIs, audit on sensitive actions
- **Shared .NET** (`STLCompliance.Shared`): `ClaimsPrincipal.GetSessionId()` for handoff session binding

## Schema (NexArr PostgreSQL)

Migration `NexArrProductLaunchSpine`:

- `product_launch_profiles` — per-product base URL and launch path
- `handoff_codes` — one-time launch handoff registry (hashed code, user/tenant/session, target product, optional callback)
- `product_callback_allowlist` — allowed callback URL patterns per product (platform-wide or tenant-scoped)

Retains M2 tables from `NexArrIdentitySpine` and `NexArrTenantEntitlementAdmin`.

## APIs

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/launch/context?productKey=` | JWT; platform admin or entitled user for JWT tenant |
| POST | `/api/launch/handoff` | JWT; platform admin or entitled user; callback validated against allowlist |
| POST | `/api/launch/handoff/redeem` | JWT + valid service token for target product **or** platform admin |
| POST | `/api/launch/callback/validate` | JWT; NexArr entitlement; platform admin or tenant admin (scoped tenant) |
| GET | `/api/launch/callback-allowlist?productKey=&tenantId=` | JWT; NexArr entitlement; tenant admin sees scoped entries |
| POST | `/api/launch/callback-allowlist` | JWT; platform admin |
| DELETE | `/api/launch/callback-allowlist/{entryId}` | JWT; platform admin |

## Permissions

- **Launch context / handoff create**: caller must have active entitlement to `productKey` on JWT tenant (platform admins bypass entitlement list but still require active launch profile).
- **Handoff redeem**: product service token whose source product matches handoff target (or is in allowed products) and tenant scope matches; platform admin may redeem without service token for break-glass.
- **Callback allowlist admin**: platform admin only for create/delete; list/validate available to NexArr users with tenant admin scope.
- Callback patterns: `origin` (exact scheme+host+port) or `prefix` (callback URL starts with pattern).

## Configuration

`Launch` section in `appsettings.json`:

- `HandoffLifetimeMinutes` (default 5)
- `Products.{productKey}.BaseUrl` / `LaunchPath` — seeded into `product_launch_profiles` on first dev/test seed

## UI

Not in this slice (M3 suite shell consumes launch context and handoff).

## Worker events

None.

## Tests

`tests/STLCompliance.NexArr.Auth.Tests/NexArrLaunchApiTests.cs` — launch context auth/entitlement, handoff create/redeem, callback validate, allowlist admin gate (InMemory DB).

## Local dev seed

Demo seed adds launch profiles for all suite products (localhost ports 5101–5107) and callback allowlist entries for `http://localhost:5173` (suite shell) plus per-product API origins for demo tenant.

## Gaps / next

- `/api/platform-admin/*` dashboard and launch diagnostics
- Product APIs validating NexArr JWT + service tokens on protected routes
- Suite-frontend product launcher (M3)
