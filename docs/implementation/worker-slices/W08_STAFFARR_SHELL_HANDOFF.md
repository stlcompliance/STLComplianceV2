# Worker 8 — StaffArr shell + NexArr handoff redeem

## Slice name

M3 Product app shell — StaffArr authenticated entry via NexArr handoff (`?handoff=...`)

## Products touched

- **StaffArr API** (`apps/staffarr-api`): service-token-backed handoff redeem, JWT-protected bootstrap routes
- **StaffArr Frontend** (`apps/staffarr-frontend`): handoff entry flow, real API session bootstrap, entitlement-aware home shell
- **NexArr API** (consumer only): `/api/launch/handoff/redeem` as the upstream handoff contract

## Schema

No DB migration required for this slice. Handoff persistence remains in NexArr (`HandoffCodeRecord`).

## StaffArr API changes

| Method | Route | Auth | Purpose |
|--------|-------|------|---------|
| POST | `/api/auth/handoff/redeem` | Anonymous (service token enforced upstream call) | Redeem NexArr handoff and mint StaffArr JWT |
| GET | `/api/session` | JWT required | Bootstrap claims-backed session/person context for StaffArr shell |
| GET | `/api/me` | JWT + StaffArr entitlement | Return person/profile context derived from claims |

Notes:
- Redeem flow calls NexArr `/api/launch/handoff/redeem` using configured StaffArr service token.
- `/api/me` enforces entitlement server-side (`staffarr`) and returns `403` when missing.
- `/api/session` is minimal claims bootstrap endpoint for frontend initialization.

## StaffArr frontend changes

- Supports suite handoff entry from query string on root path: `/?handoff=...` redirects to launch redeem flow.
- Redeems handoff against real StaffArr API (`/api/auth/handoff/redeem`), stores JWT session, then renders authenticated home.
- Handles entitlement/auth failures explicitly:
  - redeem `401` => invalid/expired handoff messaging
  - redeem `403` => not entitled messaging
  - `/api/me` `401/403` => clear stale session and show relaunch guidance
- No mocked backend data used.

## Integration tests

`tests/STLCompliance.StaffArr.Auth.Tests/StaffArrHandoffApiTests.cs` now covers:
- redeem success (session token + `/api/me`)
- redeem invalid code (`401`)
- redeem forbidden due entitlement revoked (`403`)
- protected `/api/me` unauthorized without JWT (`401`)
- protected `/api/me` forbidden with token lacking StaffArr entitlement (`403`)
- `/api/session` bootstrap response using claims/personId

## Validation commands run

```powershell
dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj
cd apps/staffarr-frontend
npm install
npm run test
npm run build
```

## Gaps / next

- Add Playwright end-to-end from Suite launch to StaffArr shell with real handoff.
- Expand StaffArr app shell beyond identity card (navigation + first domain module).
