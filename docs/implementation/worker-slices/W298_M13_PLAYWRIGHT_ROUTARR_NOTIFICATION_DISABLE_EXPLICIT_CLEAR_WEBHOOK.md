# W298 — M13 Playwright: RoutArr dispatch notification explicit webhook clear on disable

Builds on **W127** (dispatch notification settings API), **W291** (disable clears validation), **W292** (re-enable preserves webhook), **W293** (disable-and-save preserves webhook in API).

Adds **settings-only** Playwright smoke verifying that **disable + explicit clear intent removes the saved webhook from the API and UI**, while default disable-without-clear continues to preserve (W293).

## Scope

### Backend (`apps/routarr-api`)

| Change | Coverage |
|--------|----------|
| `UpsertDispatchNotificationSettingsRequest.ClearNotificationWebhookOnDisable` | Optional upsert flag (default `false`) |
| `DispatchNotificationSettingsService.UpsertAsync` | Skip webhook preservation when `ClearNotificationWebhookOnDisable=true` on disabled upsert |
| `RoutArrNotificationTests` | `Notification_settings_disable_explicit_clear_webhook_url` integration test |

### Frontend (`apps/routarr-frontend`)

| Change | Coverage |
|--------|----------|
| `NotificationSettingsPanel` | `notification-settings-clear-webhook-on-disable` checkbox when disabled; sends `clearNotificationWebhookOnDisable` on save |
| `UpsertDispatchNotificationSettingsRequest` type | Optional `clearNotificationWebhookOnDisable` |
| `NotificationSettingsPanel.test.tsx` | Vitest `clears saved webhook in API when disabling with explicit clear intent` |

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-settings-notification-disable-explicit-clear-webhook-smoke.spec.ts` | `/settings` | Seed webhook; disable + check clear-on-disable + save; API GET null webhook; reload empty field; re-enable shows empty; restore original settings |

No trip create/assign/status mutations. No process-batch.

### `e2eApi` helpers

Added:

- `routArrDispatchNotificationExplicitClearWebhookUrl`
- `seedRoutArrDispatchNotificationSettingsForExplicitClear`
- `assertRoutArrDispatchNotificationSettingsExplicitClearOnDisable`

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrSettingsNotificationDisableExplicitClearWebhookSmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests` assertions

### OpenAPI

- `routarr.openapi.json` snapshot — `clearNotificationWebhookOnDisable` on upsert request schema

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-settings-notification-disable-explicit-clear-webhook-smoke.spec.ts
```

Requires RoutArr API and frontend (5180). Demo admin role for notification settings (`routarr.dispatch.manage`).

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
dotnet test tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~Notification_settings"
dotnet test tests/STLCompliance.OpenApi.Tests/STLCompliance.OpenApi.Tests.csproj -c Release --filter "FullyQualifiedName~routarr"
cd apps/routarr-frontend; npm test -- NotificationSettingsPanel
cd tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/routarr-settings-notification-disable-explicit-clear-webhook-smoke.spec.ts
```

## Out of scope

- Trip lifecycle / outbox enqueue / process-batch (W280–W288)
- Per-event toggle changes (W286/W287)
- Live webhook sink delivery
- Changing W293 default preserve behavior without explicit clear intent

## Next recommended slice

**M13 Playwright** — RoutArr dispatch notification settings panel disable-without-clear vs explicit-clear contrast smoke (single spec comparing preserve vs clear paths in one session) — **completed as W299**.
