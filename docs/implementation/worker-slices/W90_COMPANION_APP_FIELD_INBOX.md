# Worker 90 — Companion app field inbox

## Slice name

M11 companion field inbox — per-product `/api/field-inbox`, NexArr `/api/companion/field-inbox` aggregation proxy, companion handoff auth, mobile-first `companion-frontend`, tests.

## Products touched

- **STLCompliance.Shared** — `FieldInboxContracts`, `FieldInboxRules` (shared task shape + merge ordering).
- **MaintainArr API** — `FieldInboxService`, `GET /api/field-inbox` (assigned work orders + in-progress inspections).
- **RoutArr API** — active trips for driver/self scope.
- **TrainArr API** — active training assignments for person.
- **StaffArr API** — open incidents for self/supervisor scope.
- **SupplyArr API** — draft receiving receipts (self or warehouse manager view-all).
- **NexArr API** — `POST /api/companion/auth/handoff/redeem`, `GET /api/companion/me`, `GET /api/companion/field-inbox` (forwards bearer to entitled product APIs), `CreateSessionAccessToken` with tenant role + person claims, `CompanionProductClient`, seeder + launch profile for `companion`.
- **Companion Frontend** (`apps/companion-frontend`, port **5181**) — NexArr handoff launch, aggregated inbox UI, product filter chips, deep links to product frontends.
- **render.yaml** — `companion-frontend` static site + product deep-link build args.
- **Tests** — `FieldInboxRulesTests`, `NexArrCompanionFieldInboxTests`, `MaintainArrFieldInboxTests`, companion frontend unit tests.

## API + auth

### Per-product field inbox

Each entitled product exposes:

- `GET /api/field-inbox` — returns `FieldInboxResponse` (`summary` + normalized `items[]`).

Tasks use stable `taskKey` values (`{product}:{type}:{id}`), `deepLinkPath` for product SPA routes, and optional `blockedReason`.

### NexArr companion proxy

- `POST /api/companion/auth/handoff/redeem` — browser handoff for `companion` product; mints suite JWT with entitlements, `tenant_role`, and `person_id`.
- `GET /api/companion/me` — session bootstrap for companion UI.
- `GET /api/companion/field-inbox` — requires bearer token; fans out to entitled products using configured `StaffArr__BaseUrl`, `TrainArr__BaseUrl`, etc.; returns `AggregatedFieldInboxResponse` with per-product `sources[]` (entitled/fetched/error metadata).

Authorization: platform admin, `companion` entitlement, or any field-product entitlement.

## Frontend

Mobile-first field inbox at `http://localhost:5181` (Vite proxies `/api` → NexArr `5101`).

- Launch via suite/NexArr handoff (`/launch?handoff=…`).
- Summary cards, product filter chips, task cards with status/priority/due, “Open in {Product}” deep links when `VITE_*_FRONTEND_BASE` env vars are set.
- 60s inbox refresh while signed in.

## Tests

### Backend

- `FieldInboxRulesTests` — blocked-task ordering in aggregate merge.
- `NexArrCompanionFieldInboxTests` — companion handoff redeem, aggregated inbox slices, auth gate.
- `MaintainArrFieldInboxTests` — assigned work order appears in product field inbox.

### Frontend

- `src/lib/fieldInbox.test.ts` — label/filter/format helpers.
- `src/components/FieldInboxPanel.test.tsx` — renders tasks + product filter interaction.

## Verification commands

```powershell
dotnet build "apps/nexarr-api/NexArr.Api/NexArr.Api.csproj" -c Release
dotnet test "tests/STLCompliance.NexArr.Auth.Tests/STLCompliance.NexArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~FieldInbox|FullyQualifiedName~Companion"
dotnet test "tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~FieldInbox"
cd apps/companion-frontend
npm install
npm run test
npm run build
```

## Out of scope (follow-up)

- Offline queue / idempotent submission sync
- Photo/signature/QR evidence capture in companion UI
- Push notifications
- M13 Playwright E2E harness (next slice option)

## Next slice

**M13 load/E2E verification harness** per `docs/implementation/01_MILESTONE_MASTERPLAN.md` and `docs/implementation/worker-slices/00_SLICE_STATE.md`.
