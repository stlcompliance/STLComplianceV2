# W307 — M13 Playwright: RoutArr dispatch notification disable-save-then-re-enable reload persistence (W305 follow-up)

Builds on **W127** (dispatch notification hooks), **W293** (disable-and-save preserves webhook in API), **W305** (re-enable new webhook second reload + toggle off/on persistence).

Adds **settings-only** Playwright smoke that, after disable+save preserves the last webhook URL while disabled:

1. Reload while disabled still shows the preserved URL in UI + API.
2. Re-enable + **save** persists enabled state with the same webhook.
3. A **second** page reload still shows the preserved URL.
4. **Toggle off/on** on the enable checkbox restores the preserved URL from API (W305-style reload persistence on the disable-save path).

## Scope

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-settings-notification-disable-save-re-enable-reload-persistence-smoke.spec.ts` | `/settings` | Seed → disable+save → reload disabled → re-enable+save → first + second reload → toggle off/on + API asserts + restore |

No trip mutations. No notification dispatch process-batch.

### `e2eApi` helpers

Added:

- `routArrDispatchNotificationDisableSaveReEnableReloadPersistenceWebhookUrl` (`…/routarr-e2e-w307`)
- `seedRoutArrDispatchNotificationSettingsForDisableSaveReEnableReloadPersistence`

Reuses:

- `seedRoutArrDispatchNotificationSettingsForDisableSavePreserve`
- `assertRoutArrDispatchNotificationSettingsMatch`
- `assertRoutArrDispatchNotificationWebhookUrlPersisted`

### Frontend (`apps/routarr-frontend`)

| Change | Coverage |
|--------|----------|
| `NotificationSettingsPanel.test.tsx` | Vitest: enabled + preserved webhook from API → toggle off/on restores W307 URL |

No panel code changes required (re-enable handler already reads `settingsQuery.data.notificationWebhookUrl`).

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrSettingsNotificationDisableSaveReEnableReloadPersistenceSmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests.Product_admin_smoke_specs_include_maintainarr_through_compliancecore_w230_w307`

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-settings-notification-disable-save-re-enable-reload-persistence-smoke.spec.ts
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
# $env:E2E_LIVE='1'; npx playwright test tests/routarr-settings-notification-disable-save-re-enable-reload-persistence-smoke.spec.ts
```

## Out of scope

- Explicit webhook clear on disable (W298 path)
- Notification dispatch journey / process-batch smokes
- Trip lifecycle mutations

## Next recommended slice

- **M13 Playwright** — SupplyArr procurement exception cancel journey smoke (W306 follow-up), or next RoutArr/SupplyArr product-admin smokes per milestone plan
