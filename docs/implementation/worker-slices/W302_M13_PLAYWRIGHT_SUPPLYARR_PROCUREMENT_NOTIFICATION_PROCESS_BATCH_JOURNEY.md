# W302 — M13 Playwright: SupplyArr procurement notification process-batch journey (W301/W129)

Builds on **W301** (escalation process-batch journey + pending SLA escalation notification enqueue), **W129** (procurement notification dispatch worker + internal process-batch API), **W296** (escalation worker).

Adds **journey** Playwright smoke that seeds overdue SLA exception with escalation + notification settings, runs **escalation process-batch** to enqueue pending `procurement_exception_sla_escalation` dispatch, verifies **Recent dispatches** pending row in `notification-settings-panel`, runs **internal procurement-notifications process-batch** via service token, then asserts dispatch moves to **sent/failed/skipped** in API and UI.

## Scope

### Backend (`apps/supplyarr-api`)

| Change | Coverage |
|--------|----------|
| `ProcurementNotificationDispatchService.BuildPayload` | Explicit payload for `procurement_exception_sla_escalation` event kind |

### Frontend (`apps/supplyarr-frontend`)

| Change | Coverage |
|--------|----------|
| `NotificationSettingsPanel` | `notification-settings-panel`, `notification-settings-enabled`, `notification-settings-webhook`, `notification-settings-save`, `notification-dispatches-list`, `notification-dispatch-row-{relatedEntityId}` test ids; dispatch rows show related entity |

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `supplyarr-settings-procurement-notification-process-batch-journey-smoke.spec.ts` | `/settings` | Escalation batch → pending dispatch UI → notification batch → processed dispatch UI |

No investigate/resolve on procurement exceptions panel. No RoutArr notification re-enable persistence.

### `e2eApi` helpers

Added:

- `issueSupplyArrProcurementNotificationWorkerToken`
- `processSupplyArrProcurementNotificationBatch`
- `listSupplyArrProcurementNotificationDispatches`
- `assertSupplyArrProcurementNotificationDispatchProcessed(FromHandoff)`

Reuses W301 `ensureSupplyArrProcurementExceptionEscalationJourneyFixture` and escalation batch helpers.

### Catalog

- `StlE2ePlaywrightSpecCatalog.SupplyArrSettingsProcurementNotificationProcessBatchJourneySmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests` assertions

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/supplyarr-settings-procurement-notification-process-batch-journey-smoke.spec.ts
```

Requires SupplyArr API and frontend (5179). Demo admin role for notification + escalation settings. Webhook to `hooks.example.com` typically results in **failed** delivery (non-pending), which satisfies the journey assertion.

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
dotnet test tests/STLCompliance.SupplyArr.Auth.Tests/STLCompliance.SupplyArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~ProcurementNotification|FullyQualifiedName~ProcurementExceptionEscalation"
cd apps/supplyarr-frontend
npm test -- NotificationSettingsPanel
cd ../../tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/supplyarr-settings-procurement-notification-process-batch-journey-smoke.spec.ts
```

## Out of scope

- Purchasing panel investigate/resolve journey (W295 follow-up)
- RoutArr notification re-enable-with-new-webhook reload persistence (W300 follow-up)

## Next recommended slice

- **M13 Playwright** — SupplyArr procurement exception investigate/resolve journey smoke (W295 follow-up), or RoutArr dispatch notification re-enable-with-new-webhook reload persistence smoke (W300 follow-up)
