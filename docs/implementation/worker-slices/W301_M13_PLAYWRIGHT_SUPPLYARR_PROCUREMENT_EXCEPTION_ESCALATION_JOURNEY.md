# W301 — M13 Playwright: SupplyArr procurement exception escalation process-batch journey (W296/W297)

Builds on **W296** (escalation worker + internal process-batch API), **W297** (settings panel save/reload smoke).

Adds **journey** Playwright smoke that seeds an overdue SLA exception with escalation + notification settings enabled, verifies **Due for escalation** pending preview in UI, runs **internal process-batch** via service token, then asserts escalation event + worker run + pending `procurement_exception_sla_escalation` notification dispatch in API and **Recent escalation events / Recent runs** UI.

## Scope

### Backend (`apps/supplyarr-api`)

| Change | Coverage |
|--------|----------|
| `ProcurementExceptionEscalationEventItem.ExceptionKey` | Join exception key on `/api/procurement-exception-escalation-settings/events` for stable E2E/UI targeting |

### Frontend (`apps/supplyarr-frontend`)

| Change | Coverage |
|--------|----------|
| `ProcurementExceptionEscalationSettingsPanel` | `procurement-exception-escalation-event-{exceptionKey}`, `procurement-exception-escalation-run-{runId}`, `procurement-exception-escalation-run-summary` test ids; event rows show exception key |

### Playwright (`tests/e2e-playwright`)

| Spec | Route | Coverage |
|------|-------|----------|
| `supplyarr-settings-procurement-exception-escalation-journey-smoke.spec.ts` | `/settings` | Fixture pending row → process-batch → events/runs UI + notification pending assert |

No notification process-batch. No investigate/resolve on procurement exceptions panel.

### `e2eApi` helpers

Added:

- `ensureSupplyArrProcurementExceptionEscalationJourneyFixture`
- `upsertSupplyArrProcurementExceptionEscalationSettings`
- `upsertSupplyArrProcurementNotificationSettings`
- `issueSupplyArrProcurementExceptionEscalationWorkerToken`
- `processSupplyArrProcurementExceptionEscalationBatch`
- `assertSupplyArrProcurementExceptionEscalationEventsContain(FromHandoff)`
- `assertSupplyArrProcurementExceptionEscalationRunEscalated(FromHandoff)`
- `assertSupplyArrProcurementNotificationDispatchPending(FromHandoff)`

### Catalog

- `StlE2ePlaywrightSpecCatalog.SupplyArrSettingsProcurementExceptionEscalationJourneySmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests` assertions

## Prerequisites (live)

```powershell
scripts/ops/e2e-stack-up.ps1
scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
$env:E2E_LIVE='1'
npx playwright test tests/supplyarr-settings-procurement-exception-escalation-journey-smoke.spec.ts
```

Requires SupplyArr API and frontend (5179). Demo admin role for escalation + notification settings.

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
dotnet test tests/STLCompliance.SupplyArr.Auth.Tests/STLCompliance.SupplyArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~ProcurementExceptionEscalation"
cd apps/supplyarr-frontend
npm test -- ProcurementExceptionEscalationSettingsPanel
cd ../../tests/e2e-playwright
npm install
# With live stack:
# $env:E2E_LIVE='1'; npx playwright test tests/supplyarr-settings-procurement-exception-escalation-journey-smoke.spec.ts
```

## Out of scope

- Procurement notification process-batch / webhook delivery (W129 dispatch worker)
- Purchasing panel investigate/resolve (W295)
- RoutArr notification re-enable-with-new-webhook persistence (optional M13 follow-up)

## Next recommended slice

- **M13 Playwright** — RoutArr dispatch notification settings panel live save after re-enable with new webhook reload persistence smoke (W300 follow-up), or SupplyArr procurement notification process-batch journey smoke (pending dispatch → sent/failed)
