# Worker 7 — Suite unified dashboard (M3)

## Slice name

M3 Suite Frontend — unified home dashboard at `/app` with cross-product widgets

## Products touched

- **Suite Frontend** (`apps/suite-frontend`): home dashboard widgets, Vitest coverage
- **NexArr** (consumer only): `/api/me`, `/api/me/entitlements`, `/api/me/navigation`, `/api/me/tenants`

## Schema

None (frontend slice).

## APIs consumed (real NexArr — no mocked data)

| Method | Route | Widget use |
|--------|-------|------------|
| GET | `/api/me` | Profile (via `AuthProvider`) |
| GET | `/api/me/entitlements` | Entitlement detail + “what you need” |
| GET | `/api/me/navigation` | Quick launch product list |
| GET | `/api/me/tenants` | Tenant context + multi-tenant hints |
| GET | `/api/launch/context` | (existing) product hub / handoff |
| POST | `/api/launch/handoff` | Quick launch external products |

## UI (`/app`)

| Widget | Data source |
|--------|-------------|
| Quick launch | `/api/me/navigation` + JWT entitlements |
| Tenant context | `/api/me` + `/api/me/tenants` |
| Session | Stored session + `/api/me` |
| What you need | Derived actions (tenant status, entitlements, navigation, platform admin) |

Permission-aware: platform-admin actions only when `me.isPlatformAdmin`; launch buttons respect existing handoff flow.

## Structure

- `src/lib/dashboard.ts` — pure helpers (`buildWhatINeedActions`, `summarizeSession`, …)
- `src/lib/dashboard.test.ts` — Vitest
- `src/hooks/useDashboardData.ts` — parallel TanStack queries
- `src/components/dashboard/*` — widget components
- `src/pages/HomePage.tsx` — dashboard layout

## Tests

- `apps/suite-frontend`: `dashboard.test.ts` (+ existing `permissions.test.ts`, `authStorage.test.ts`)
- CI: `npm run build`, `npm test`; solution `dotnet test`

## Local dev

```powershell
dotnet run --project apps/nexarr-api/NexArr.Api/NexArr.Api.csproj
cd apps/suite-frontend
npm run dev
# http://localhost:5174/app after sign-in (admin@demo.stl)
```

## Gaps / next (Worker 8+)

- **StaffArr shell** with handoff redeem (recommended next slice)
- Shared `packages/ui` design system extraction
- Per-product launch readiness on home (batch `/api/launch/context` or server summary)
- Playwright E2E against docker-compose stack
- Platform audit search/export UI
