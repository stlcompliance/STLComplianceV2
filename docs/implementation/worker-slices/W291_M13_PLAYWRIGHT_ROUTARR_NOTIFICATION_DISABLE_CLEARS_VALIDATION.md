# W291 — M13 Playwright: RoutArr dispatch notification disable clears validation

Builds on **W127** (dispatch notification settings API), **W289** (webhook URL persistence smoke), **W290** (empty/invalid webhook validation smoke).

Adds **settings-only** Playwright smoke verifying that **unchecking “Enable dispatch notifications” clears client-side webhook validation** without persisting an invalid enabled+empty webhook state, and that **save after disable** persists `isEnabled: false` with a null webhook.

## Scope

### Frontend (`apps/routarr-frontend`)

| Change | Coverage |
|--------|----------|
| `NotificationSettingsPanel` | Existing enable-toggle handler clears `webhookError` / `saveError` on disable (W290); W291 adds Vitest `clears webhook validation when disabling notifications` |

No panel markup changes required — reuses `notification-settings-webhook-error` and `notification-settings-enabled` test ids from W290.

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-settings-notification-disable-clears-validation-smoke.spec.ts` | `/settings` | Enable + empty webhook → required error; uncheck enable → error hidden without save; reload shows initial persisted settings; disable-then-save persists disabled + null webhook; API GET match + disabled PUT acceptance; restore original settings |

No trip create/assign/status mutations. No process-batch.

### `e2eApi` helpers

Added:

- `assertRoutArrDispatchNotificationSettingsMatch`
- `assertRoutArrDispatchNotificationSettingsUpsertAcceptedWhenDisabled`

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrSettingsNotificationDisableClearsValidationSmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w291`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-settings-notification-disable-clears-validation-smoke.spec.ts
```

Requires RoutArr API and frontend (5180). Demo admin role for notification settings (`routarr.dispatch.manage`).

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
cd apps/routarr-frontend; npm test -- NotificationSettingsPanel
cd tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/routarr-settings-notification-disable-clears-validation-smoke.spec.ts
```

## Out of scope

- Trip lifecycle / outbox enqueue / process-batch (W280–W288)
- Per-event toggle changes (W286/W287)
- Live webhook sink delivery
- Re-enable-after-disable webhook restore UX (W292 — complete)

## Next recommended slice

**W292** — re-enable after disable preserves prior webhook smoke (complete). See **W293** — disable-and-save preserves webhook in API smoke.
