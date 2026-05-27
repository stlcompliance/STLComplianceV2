# Worker 88 — Suite frontend shell enhancements

## Slice name

M3 suite shell — server-driven product surfaces, centralized nav icons, product route composition, entitlement-aware secondary navigation, topbar context.

## Products touched

- **NexArr API** (`apps/nexarr-api`): `ProductSurfaceCatalog`, extended `GET /api/me/navigation` with `NavigationSurfaceItem` surfaces and permission hints.
- **Suite Frontend** (`apps/suite-frontend`): `navIcons.ts`, `ProductShellLayout`, `ProductSurfaceNav`, `AppTopBar`, nested `/app/:productKey/:surfaceKey` routes.
- **Tests**: `ProductSurfaceCatalogTests`, extended `NexArrAuthApiTests`, `suiteNavigation.test.ts`, `ProductSurfaceNav.test.tsx`, updated `dashboard.test.ts`.

## Schema

None.

## API + auth changes

### NexArr

- `GET /api/me/navigation` — each entitled product now includes `surfaces[]`:
  - `surfaceKey`, `label`, `relativePath`, `iconKey`, `sortOrder`
  - `isEnabled` — entitlement + platform-admin gates
  - `permissionHint` — human-readable guidance when disabled or for external launch surfaces

Surfaces are built from `ProductSurfaceCatalog` (static per product, server-driven).

## Frontend changes

- **`src/navigation/navIcons.ts`** — centralized Lucide registry per design system doc.
- **`src/navigation/suiteNavigation.ts`** — path builders and active-surface resolution.
- **`AppShellLayout`** — enterprise topbar via `AppTopBar`, product switcher unchanged.
- **`ProductShellLayout`** — secondary nav from server surfaces; nested outlet for product pages.
- **Routes** — `/app/:productKey` and `/app/:productKey/:surfaceKey` compose to `ProductSurfacePage` (overview + launch handoff + placeholder surfaces).
- **`productIcons.tsx`** — delegates to `navIcons`.

## Tests

### Backend unit (`ProductSurfaceCatalogTests`)

- StaffArr surfaces include overview + launch hint
- NexArr tenants surface gated on platform admin
- Disabled surfaces when entitlement missing

### Backend integration (`NexArrAuthApiTests`)

- Navigation returns surfaces for entitled products

### Frontend unit

- `suiteNavigation.test.ts` — paths, active surface, launch detection
- `ProductSurfaceNav.test.tsx` — renders only enabled surfaces
- `dashboard.test.ts` — updated fixtures with `surfaces`

## Verification commands

```powershell
dotnet build "apps/nexarr-api/NexArr.Api/NexArr.Api.csproj" -c Release
dotnet test "tests/STLCompliance.NexArr.Auth.Tests/STLCompliance.NexArr.Auth.Tests.csproj" -c Release
cd apps/suite-frontend
npm install
npm run test
npm run build
```

## Remaining gaps

- Embedded product UIs (iframe/SDK) beyond handoff launch placeholders
- Fine-grained permission hints from owning product APIs (beyond entitlement/platform-admin)
- Shared `packages/ui` extraction, Playwright E2E, dark-theme polish on all product frontends
- Deployment/render.yaml hardening (M13)

## Next slice (Worker 89)

Recommended: **Deployment/render.yaml hardening** or **Companion app field inbox** per milestone priority — see `docs/implementation/worker-slices/00_SLICE_STATE.md`.
