# Worker 5 — Suite frontend authenticated AppShell

## Slice name

M3 Suite Frontend — authenticated AppShell, NexArr auth, entitlement navigation, product launcher

## Products touched

- **Suite Frontend** (`apps/suite-frontend`): Vite/React shell on port 5174
- **NexArr** (consumer only): auth, `/api/me/*`, `/api/launch/*`; dev callback allowlist extended for `http://localhost:5174`

## Schema

None (frontend slice).

## APIs consumed (real NexArr — no mocked data)

| Method | Route | Use |
|--------|-------|-----|
| POST | `/api/auth/login` | Sign-in |
| POST | `/api/auth/renew` | Access token refresh |
| POST | `/api/auth/logout` | Sign-out |
| GET | `/api/me` | Profile + entitlements in JWT-backed session |
| GET | `/api/me/navigation` | Product switcher |
| GET | `/api/launch/context?productKey=` | Launch eligibility on product hub |
| POST | `/api/launch/handoff` | External product launch (callback = suite `/app/{product}`) |

## UI

- Login page (demo tenant from `VITE_DEMO_TENANT_ID`)
- App shell: sidebar product switcher, home dashboard, per-product hub routes `/app/:productKey`
- Permission gates: hide nav/launch when not entitled; platform-admin hint only when `isPlatformAdmin`
- In-suite `nexarr` route; other products launch via handoff redirect

## Structure (non-monolithic)

- `src/api/` — types, client, base URL
- `src/auth/` — storage, `AuthProvider`
- `src/app/routes.tsx` — routing
- `src/layouts/AppShellLayout.tsx` — shell chrome
- `src/components/` — `RequireAuth`, `PermissionGate`, `ProductSwitcher`
- `src/pages/` — login, home, product hub

## Local dev

```powershell
dotnet run --project apps/nexarr-api/NexArr.Api/NexArr.Api.csproj
cd apps/suite-frontend
npm install
npm run dev
# http://localhost:5174 — /api proxied to NexArr :5101
```

Credentials: `admin@demo.stl` / `ChangeMe!Demo2026` (see W02).

Env: `apps/suite-frontend/.env.example`

## Tests

- `apps/suite-frontend` Vitest: `permissions.test.ts`, `authStorage.test.ts`
- CI job `suite-frontend`: `npm ci`, `npm run build`, `npm test`

## Gaps / next (Worker 6+)

- Unified dashboard widgets
- NexArr platform-admin dashboard UI (`/api/platform-admin/*`)
- Shared `packages/ui` design system extraction
- Playwright E2E against docker-compose stack
- Product-specific embedded surfaces beyond handoff launch
