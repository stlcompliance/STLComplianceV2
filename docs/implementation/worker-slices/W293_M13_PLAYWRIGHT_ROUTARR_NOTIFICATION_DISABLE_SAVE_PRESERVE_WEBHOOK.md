# W293 — M13 Playwright: RoutArr dispatch notification disable-and-save preserves webhook

Builds on **W127** (dispatch notification settings API), **W289** (webhook URL persistence smoke), **W290** (empty/invalid webhook validation smoke), **W291** (disable-notifications clears validation smoke), **W292** (re-enable after disable preserves prior webhook smoke).

Adds **settings-only** Playwright smoke verifying that **disable + save keeps the last webhook URL in the API while `isEnabled=false`**, and that **reload and re-enable show the preserved URL**.

## Scope

### Backend (`apps/routarr-api`)

| Change | Coverage |
|--------|----------|
| `DispatchNotificationSettingsService.UpsertAsync` | When `isEnabled=false` and request webhook is null/empty, preserve existing `NotificationWebhookUrl` on the tenant settings row |
| `RoutArrNotificationTests` | `Notification_settings_disable_preserves_webhook_url` integration test |

### Frontend (`apps/routarr-frontend`)

No panel behavior change required — reload from API shows preserved webhook when disabled; re-enable restore from W292 unchanged.

| Change | Coverage |
|--------|----------|
| `NotificationSettingsPanel.test.tsx` | Vitest `persists saved webhook when disabling and saving with empty field` (client sends null; server preserves) |

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-settings-notification-disable-save-preserve-webhook-smoke.spec.ts` | `/settings` | Seed webhook via API; disable + save; API GET `isEnabled=false` with preserved webhook; reload UI; re-enable shows preserved URL; restore original settings |

No trip create/assign/status mutations. No process-batch.

### `e2eApi` helpers

Added:

- `routArrDispatchNotificationDisableSavePreserveWebhookUrl`
- `seedRoutArrDispatchNotificationSettingsForDisableSavePreserve`

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrSettingsNotificationDisableSavePreserveWebhookSmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w293`

### W291 adjustment

`routarr-settings-notification-disable-clears-validation-smoke.spec.ts` — disable-then-save API assertion now expects preserved webhook when the tenant had a webhook before disable-save (aligns with W293 backend behavior).

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-settings-notification-disable-save-preserve-webhook-smoke.spec.ts
```

Requires RoutArr API and frontend (5180). Demo admin role for notification settings (`routarr.dispatch.manage`).

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
dotnet test tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~Notification_settings"
cd apps/routarr-frontend; npm test -- NotificationSettingsPanel
cd tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/routarr-settings-notification-disable-save-preserve-webhook-smoke.spec.ts
```

## Out of scope

- Trip lifecycle / outbox enqueue / process-batch (W280–W288)
- Per-event toggle changes (W286/W287)
- Live webhook sink delivery
- Explicit “clear webhook on disable” UX (optional future slice)

## Next recommended slice

**M13 Playwright** — RoutArr dispatch notification settings panel explicit webhook clear on disable smoke, or continue M9/M12 RoutArr backlog per `00_SLICE_STATE.md`.
