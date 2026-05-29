# W290 — M13 Playwright: RoutArr dispatch notification empty/invalid webhook validation

Builds on **W127** (dispatch notification settings API), **W279** (settings panel save/reload), **W289** (webhook URL persistence smoke).

Adds **settings-only** Playwright smoke for **required-field and invalid URL UX** when dispatch notifications are enabled, plus backend/API rejection for defense in depth.

## Scope

### Backend (`apps/routarr-api`)

| Change | Coverage |
|--------|----------|
| `DispatchNotificationRules.ValidateUpsertRequest` | Rejects `isEnabled=true` with null/whitespace webhook (`routarr.notification.webhook_required`) |
| `DispatchNotificationSettingsService.UpsertAsync` | Calls validation before `NormalizeWebhookUrl` (invalid URL still returns 400 via existing rules) |

### Frontend (`apps/routarr-frontend`)

| Change | Coverage |
|--------|----------|
| `NotificationSettingsPanel` | Client-side validation on Save; `notification-settings-webhook-error` + `notification-settings-save-error` test ids; clears errors on field edit |

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-settings-notification-webhook-validation-smoke.spec.ts` | `/settings` | Enable + empty webhook → required error, reload shows prior settings; invalid URL → absolute URL error, reload unchanged; valid URL saves; API PUT rejection for empty/invalid; restore original settings |

No trip create/assign/status mutations. No process-batch.

### `e2eApi` helpers

Added:

- `routArrDispatchNotificationWebhookValidationInvalidUrl`
- `routArrDispatchNotificationWebhookValidationValidUrl`
- `assertRoutArrDispatchNotificationSettingsUpsertRejected`

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrSettingsNotificationWebhookValidationSmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w290`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-settings-notification-webhook-validation-smoke.spec.ts
```

Requires RoutArr API and frontend (5180). Demo admin role for notification settings (`routarr.dispatch.manage`).

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~Notification"
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
cd apps/routarr-frontend; npm test -- NotificationSettingsPanel
cd tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/routarr-settings-notification-webhook-validation-smoke.spec.ts
```

## Out of scope

- Trip lifecycle / outbox enqueue / process-batch (W280–W288)
- Per-event toggle changes (W286/W287)
- Live webhook sink delivery
- HTTPS-only production scheme enforcement in browser (backend enforces in non-Testing env)

## Next recommended slice

**W291** — disable-notifications clears validation smoke (complete). See **W292** — re-enable after disable preserves prior webhook smoke.
