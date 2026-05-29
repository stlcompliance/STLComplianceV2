# W305 — M13 Playwright: RoutArr dispatch notification re-enable-with-new-webhook reload persistence (W300 follow-up)

Builds on **W127** (dispatch notification hooks), **W298** (explicit webhook clear on disable), **W300** (re-enable after explicit clear empty webhook + new URL save).

Adds **settings-only** Playwright smoke that, after explicit clear and saving a **new** webhook URL, verifies:

1. A **second** page reload still shows the new URL (not the pre-clear original).
2. **Toggle off/on** on the enable checkbox restores the new URL from API (not the pre-clear original).

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-settings-notification-re-enable-new-webhook-reload-persistence-smoke.spec.ts` | `/settings` | Seed explicit-clear → re-enable + new URL save → first reload → **second reload** → **toggle off/on** + API asserts + restore |

No trip mutations. No notification dispatch process-batch.

### `e2eApi` helpers

Added:

- `routArrDispatchNotificationReEnableNewWebhookReloadPersistenceOriginalWebhookUrl` (aliases W300 original)
- `routArrDispatchNotificationReEnableNewWebhookReloadPersistenceNewWebhookUrl` (`…/routarr-e2e-w305-new`)

Reuses:

- `seedRoutArrDispatchNotificationSettingsDisabledAfterExplicitClear`
- `assertRoutArrDispatchNotificationSettingsMatch`
- `assertRoutArrDispatchNotificationWebhookUrlPersisted`

### Frontend (`apps/routarr-frontend`)

| Change | Coverage |
|--------|----------|
| `NotificationSettingsPanel.test.tsx` | Vitest: enabled + new webhook from API → toggle off/on restores new URL, not W300 original |

No panel code changes required (re-enable handler already reads `settingsQuery.data.notificationWebhookUrl`).

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrSettingsNotificationReEnableNewWebhookReloadPersistenceSmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w305`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-settings-notification-re-enable-new-webhook-reload-persistence-smoke.spec.ts
```

Requires RoutArr API (5105) and frontend (5180). Demo admin with notification settings manage permission.

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
cd apps/routarr-frontend
npm run test -- NotificationSettingsPanel
cd ../../tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/routarr-settings-notification-re-enable-new-webhook-reload-persistence-smoke.spec.ts
```

## Out of scope

- Disable + save then re-enable reload path (separate follow-up)
- Notification dispatch journey / process-batch smokes
- Trip lifecycle mutations

## Next recommended slice

- **M13 Playwright** — SupplyArr procurement exception reject-waive journey smoke (W304 follow-up), or RoutArr dispatch notification disable-save-then-re-enable reload persistence (W305 follow-up)
