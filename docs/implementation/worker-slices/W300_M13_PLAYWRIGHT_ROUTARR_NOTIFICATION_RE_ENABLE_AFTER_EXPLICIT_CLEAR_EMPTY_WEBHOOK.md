# W300 — M13 Playwright: RoutArr dispatch notification re-enable-after-explicit-clear empty webhook

Builds on **W127** (dispatch notification settings API), **W298** (explicit webhook clear on disable), **W299** (preserve vs explicit-clear contrast).

Adds **settings-only** Playwright smoke that seeds a **disabled + null webhook** state after explicit clear, then verifies **re-enable shows an empty field** (not the prior URL), **save without URL is blocked**, and **a new URL is required to persist enabled settings**.

Complements W299 by focusing on the post-clear re-enable UX path rather than side-by-side disable contrast.

## Scope

### Backend (`apps/routarr-api`)

No changes — reuses W298 `ClearNotificationWebhookOnDisable` upsert flag and disabled settings with `notificationWebhookUrl: null`.

### Frontend (`apps/routarr-frontend`)

No panel changes — reuses existing validation (`Webhook URL is required when dispatch notifications are enabled`) and re-enable handler that only restores webhook when API still has a saved URL.

| Change | Coverage |
|--------|----------|
| `NotificationSettingsPanel.test.tsx` | Vitest `requires new webhook URL when re-enabling after explicit clear removed saved URL` |

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `routarr-settings-notification-re-enable-after-explicit-clear-empty-webhook-smoke.spec.ts` | `/settings` | API seed disabled-after-explicit-clear; reload empty webhook; re-enable empty (not original URL); save without URL → required error; fill new URL + save → API/UI persist; restore original settings |

No trip create/assign/status mutations. No process-batch.

### `e2eApi` helpers

Added:

- `routArrDispatchNotificationReEnableAfterExplicitClearOriginalWebhookUrl`
- `routArrDispatchNotificationReEnableAfterExplicitClearNewWebhookUrl`
- `seedRoutArrDispatchNotificationSettingsDisabledAfterExplicitClear`

Reuses:

- `assertRoutArrDispatchNotificationSettingsMatch`
- `assertRoutArrDispatchNotificationSettingsExplicitClearOnDisable`

### Catalog

- `StlE2ePlaywrightSpecCatalog.RoutArrSettingsNotificationReEnableAfterExplicitClearEmptyWebhookSmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests` assertions (method renamed to `w230_w300`)

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/routarr-settings-notification-re-enable-after-explicit-clear-empty-webhook-smoke.spec.ts
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
# $env:E2E_LIVE='1'; npx playwright test tests/routarr-settings-notification-re-enable-after-explicit-clear-empty-webhook-smoke.spec.ts
```

## Out of scope

- Trip lifecycle / outbox enqueue / process-batch (W280–W288)
- Per-event toggle changes (W286/W287)
- Live webhook sink delivery
- Backend or explicit-clear panel behavior changes (covered by W298/W299)

## Next recommended slice

Continue M9/M12 RoutArr backlog or M13 Playwright coverage per `00_SLICE_STATE.md` — e.g. RoutArr dispatch notification settings panel live save after re-enable with new webhook reload persistence smoke, or next SupplyArr/RoutArr product-admin smokes.
