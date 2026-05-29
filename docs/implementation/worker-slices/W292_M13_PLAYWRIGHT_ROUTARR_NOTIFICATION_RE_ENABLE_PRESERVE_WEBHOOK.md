# W292 — M13 Playwright: RoutArr dispatch notification re-enable preserves webhook

Builds on **W127** (dispatch notification settings API), **W289** (webhook URL persistence smoke), **W290** (empty/invalid webhook validation smoke), **W291** (disable-notifications clears validation smoke).

Adds **settings-only** Playwright smoke verifying that **re-enabling dispatch notifications after disable clears validation reloads the last saved webhook URL from the API** without persisting the locally cleared field.

## Scope

### Frontend (`apps/routarr-frontend`)

| Change | Coverage |
|--------|----------|
| `NotificationSettingsPanel` | On enable toggle, restores `webhookUrl` from `settingsQuery.data.notificationWebhookUrl` when re-enabling |
| `NotificationSettingsPanel.test.tsx` | Vitest `restores saved webhook from API when re-enabling notifications` |

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-settings-notification-re-enable-preserve-webhook-smoke.spec.ts` | `/settings` | Seed saved webhook via API; clear field + validation error; disable clears error; re-enable restores saved webhook in UI; API GET unchanged; restore original settings |

No trip create/assign/status mutations. No process-batch.

### `e2eApi` helpers

Added:

- `routArrDispatchNotificationReEnablePreserveWebhookUrl`
- `seedRoutArrDispatchNotificationSettingsWithSavedWebhook`

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrSettingsNotificationReEnablePreserveWebhookSmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w292`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-settings-notification-re-enable-preserve-webhook-smoke.spec.ts
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
# $env:E2E_LIVE='1'; npx playwright test tests/routarr-settings-notification-re-enable-preserve-webhook-smoke.spec.ts
```

## Out of scope

- Trip lifecycle / outbox enqueue / process-batch (W280–W288)
- Per-event toggle changes (W286/W287)
- Live webhook sink delivery
- Disable+save preserving webhook in API when `isEnabled=false` (W293 — complete)

## Next recommended slice

See **W293** completion report and `00_SLICE_STATE.md` for the next M13 RoutArr notification settings slice.
