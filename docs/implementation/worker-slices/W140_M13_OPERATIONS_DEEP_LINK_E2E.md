# Worker 140 — M13 operations product deep-link E2E (MaintainArr / RoutArr / SupplyArr)

## Scope

Extends W133/W134 companion field-inbox deep links to **operations products** where SPA routes now exist:

| Product | Deep link path | Workspace route | Field inbox `deepLinkUrl` |
|---------|----------------|-----------------|---------------------------|
| MaintainArr | `/work-orders/{id}` | `WorkOrderWorkspacePage` | `MaintainArr__FrontendBaseUrl` |
| RoutArr | `/trips/{id}` | `TripWorkspacePage` | `RoutArr__FrontendBaseUrl` |
| SupplyArr | `/receiving/{id}` | `ReceivingWorkspacePage` | `SupplyArr__FrontendBaseUrl` |

- **Playwright** — `companion-field-inbox-operations-deep-links.spec.ts` (3 tests, `E2E_LIVE` skip)
- **e2eApi** — `ensureMaintainArrFieldInboxFixture`, `ensureRoutArrFieldInboxFixture`, `ensureSupplyArrFieldInboxFixture`
- **Catalog** — `StlE2ePlaywrightSpecCatalog.CompanionFieldInboxMaintainarrDeepLinkSpec` in `DeepLinkSmokeSpecs`
- **Config** — `docker-compose.yml` + `render.yaml` frontend base URLs on product APIs

## Verification

```powershell
./scripts/ops/e2e-stack-up.ps1
./scripts/ops/e2e-frontends-preview.ps1
$env:E2E_LIVE = "1"
cd tests/e2e-playwright
npm test
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "Category=E2e"
```

## Out of scope

- MaintainArr `/inspections/{id}` workspace (path exists on API only)
- Direct product-app deep-link specs without companion (TrainArr pattern optional follow-up)
