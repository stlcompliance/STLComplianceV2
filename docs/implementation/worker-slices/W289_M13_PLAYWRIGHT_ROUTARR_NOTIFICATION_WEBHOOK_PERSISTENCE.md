# W289 — M13 Playwright: RoutArr dispatch notification webhook URL persistence

Builds on **W127** (dispatch notification settings API), **W279** (settings panel save/reload for toggles + webhook), **W288** (all-events process-batch journey with live UI webhook save).

Adds a **settings-only** Playwright smoke that focuses on **webhook URL change persistence** across two save/reload cycles, with API GET verification after each reload.

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-settings-notification-webhook-persistence-smoke.spec.ts` | `/settings` | Suite sign-in → handoff → `notification-settings-panel`: change webhook to W289 primary URL → Save → reload → UI value + API GET match → change to W289 alternate URL → Save → reload → UI + API match → restore original webhook → Save → reload → UI + API match |

No trip create/assign/status mutations. No process-batch. Event toggles and enable checkbox are left unchanged.

Webhook URLs use read-only `https://hooks.example.com/routarr-e2e-w289` and `...-w289-alt`.

### `e2eApi` helpers

Added:

- `routArrDispatchNotificationWebhookPersistencePrimaryUrl`
- `routArrDispatchNotificationWebhookPersistenceAlternateUrl`
- `getRoutArrDispatchNotificationSettings`
- `assertRoutArrDispatchNotificationWebhookUrlPersisted`

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrSettingsNotificationWebhookPersistenceSmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w289`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-settings-notification-webhook-persistence-smoke.spec.ts
```

Requires RoutArr API and frontend (5180). Demo admin role for notification settings (`routarr.dispatch.manage`).

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
cd tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/routarr-settings-notification-webhook-persistence-smoke.spec.ts
```

## Out of scope

- Trip lifecycle / outbox enqueue / process-batch (W280–W288)
- Enable toggle or per-event toggle changes (W279/W286/W287)
- Live webhook sink delivery
- Empty/invalid webhook validation UX

## Next recommended slice

**M13 Playwright — RoutArr dispatch notification settings panel empty-webhook validation smoke** (settings-only; invalid URL or required-field UX when notifications enabled).
