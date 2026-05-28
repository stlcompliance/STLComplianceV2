# W234 — M13 Playwright: TrainArr assignment material demand smoke

Builds on **W232** (product admin Playwright pattern), **W233** (assignment material demand panel + status-events API).

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `trainarr-assignment-material-demand-smoke.spec.ts` | TrainArr `/assignments/{id}` | Suite sign-in → handoff → assignment workspace; `assignment-material-demand` panel; API-seeded procurement badge + timeline when stack supports publish/callback; UI fallback add line + optional publish |

### E2E API helpers (`support/e2eApi.ts`)

- `ensureTrainArrMaterialDemandFixture` — active assignment, demand line, publish to SupplyArr, `pr_submitted` status ingest (best-effort)
- `createTrainArrMaterialDemandLine`, `publishTrainArrMaterialDemand`
- `issueSupplyarrToTrainarrDemandStatusToken`, `ingestTrainarrSupplyarrDemandStatus`

### Catalog

- `StlE2ePlaywrightSpecCatalog.TrainArrAssignmentMaterialDemandSmokeSpec` in `ProductAdminSmokeSpecs`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_compliance_core_and_trainarr_w230_w234`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/trainarr-assignment-material-demand-smoke.spec.ts
```

Requires TrainArr API (5103) and SupplyArr API (5106) for full procurement seeding; without them the spec falls back to UI add-line / publish assertions.

## Verification

```powershell
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
```

## Out of scope

- Full SupplyArr PR submit E2E chain (covered in W233 integration tests)
- Settings/admin-only TrainArr panels

## Next slice

- **Suite M13** — RoutArr or SupplyArr admin settings Playwright parity
- **Compliance Core M12** — audit delivery orchestration UI
- **TrainArr** — companion deep link directly to material demand section
