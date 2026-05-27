# Product workspace shell session bootstrap

## Slice name

M3 shared product workspace shell — session bootstrap, tenant/user chrome, and auth gates across all product frontends

## Products touched

- **@stl/shared-ui** (`packages/shared-ui`): `ProductWorkspaceFrame`, `productWorkspaceAuth`, compact `ProductAppShell` variant
- **StaffArr, TrainArr, MaintainArr, RoutArr, SupplyArr, Compliance Core, Companion frontends**: `ProductWorkspaceLayout` wired to real `/api/me` bootstrap

## Schema

None (frontend + shared UI only).

## APIs consumed (real product APIs — no mocks in production paths)

| Method | Route | Use |
|--------|-------|-----|
| GET | `/api/me` | Session bootstrap in each product workspace layout |

Handoff redeem remains on `/api/auth/handoff/redeem` via existing `/launch` routes.

## Frontend changes

- **`ProductWorkspaceFrame`** centralizes unauthenticated, loading, forbidden, and expired session states
- **`ProductWorkspaceLayout`** in each product app:
  - redirects `/?handoff=` to `/launch`
  - loads stored JWT session
  - calls real `/api/me` via TanStack Query
  - clears stale session on `401/403`
  - passes tenant slug + display name into shared shell chrome
- **Companion** uses compact shell variant (top bar only, mobile-safe padding)
- Product nav stubs remain single-surface for now (`Workspace`, `Dispatch`, etc.)

## Permission keys

No new permission keys. Existing product APIs continue to enforce entitlement + product permissions server-side.

## Worker / events

None.

## Tests

- `packages/shared-ui/src/productWorkspaceAuth.test.ts`
- `packages/shared-ui/src/ProductWorkspaceFrame.test.tsx`
- `apps/staffarr-frontend/src/layouts/ProductWorkspaceLayout.test.tsx`

## Verification commands

```powershell
cd packages/shared-ui
npm install
npm run test
cd ../../apps/staffarr-frontend
npm run test
npm run build
cd ../trainarr-frontend
npm run build
```

## Remaining gaps

- Home pages still render duplicate product headers inside workspace content (dedupe in a follow-up slice)
- Product-specific multi-surface navigation beyond single workspace tab
- Playwright smoke should assert shell tenant/user chrome after handoff

## Next recommended slice

StaffArr audit package export foundations (`/api/audit-packages`) or product-owner SLO adoption for M13 load tests.
