# W299 — M13 Playwright: RoutArr dispatch notification disable preserve vs explicit-clear contrast

Builds on **W127** (dispatch notification settings API), **W293** (disable-and-save preserves webhook in API), **W298** (explicit webhook clear on disable).

Adds **settings-only** Playwright smoke that runs **both disable paths in one browser session**: first disable-without-clear preserves the webhook (W293), then re-enable and disable-with-explicit-clear removes it (W298).

## Scope

### Backend (`apps/routarr-api`)

No changes — reuses W293 preserve-on-disable and W298 `ClearNotificationWebhookOnDisable` upsert flag.

### Frontend (`apps/routarr-frontend`)

No changes — reuses W298 `notification-settings-clear-webhook-on-disable` checkbox and W293 default preserve behavior.

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-settings-notification-disable-preserve-vs-explicit-clear-contrast-smoke.spec.ts` | `/settings` | Seed webhook; phase 1 disable without clear-on-disable + save → API/UI preserve; phase 2 re-enable then disable with clear-on-disable + save → API/UI null webhook; restore original settings |

No trip create/assign/status mutations. No process-batch.

### `e2eApi` helpers

Added:

- `routArrDispatchNotificationDisableContrastWebhookUrl`
- `seedRoutArrDispatchNotificationSettingsForDisableContrast`

Reuses:

- `assertRoutArrDispatchNotificationSettingsMatch`

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrSettingsNotificationDisablePreserveVsExplicitClearContrastSmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests` assertions (method renamed to `w230_w299`)

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-settings-notification-disable-preserve-vs-explicit-clear-contrast-smoke.spec.ts
```

Requires RoutArr API and frontend (5180). Demo admin role for notification settings (`routarr.dispatch.manage`).

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
cd tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/routarr-settings-notification-disable-preserve-vs-explicit-clear-contrast-smoke.spec.ts
```

## Out of scope

- Trip lifecycle / outbox enqueue / process-batch (W280–W288)
- Per-event toggle changes (W286/W287)
- Live webhook sink delivery
- Backend or panel behavior changes (covered by W293/W298)

## Next recommended slice

Continue M9/M12 RoutArr backlog or M13 Playwright coverage per `00_SLICE_STATE.md` — e.g. RoutArr dispatch notification settings re-enable-after-explicit-clear empty webhook smoke, or next SupplyArr/RoutArr product-admin smokes.
