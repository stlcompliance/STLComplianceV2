# W297 — M13 Playwright SupplyArr procurement exception escalation settings panel smoke (W296)

Builds on **W296** (`ProcurementExceptionEscalationSettingsPanel`, pending/runs/events APIs), **W295** (procurement exceptions fixture pattern), and **W279** (settings save/reload Playwright pattern).

## Scope

### Playwright spec

`tests/e2e-playwright/tests/supplyarr-settings-procurement-exception-escalation-smoke.spec.ts`

- Suite login → SupplyArr handoff → `/settings`
- `procurement-exception-escalation-settings-panel` visible
- Enable toggle + cooldown hours + max escalations + notify toggle save/reload persistence (UI + API GET)
- When enabled, **Due for escalation** pending preview shows overdue fixture row (or empty state)
- Recent runs + recent escalation events sections visible (empty or list)
- Restore original settings at end
- Read-only: no internal escalation process-batch trigger

### e2eApi helpers

| Helper | Purpose |
|--------|---------|
| `ensureSupplyArrProcurementExceptionEscalationFixture` | Overdue SLA exception for pending preview |
| `getSupplyArrProcurementExceptionEscalationSettings` | API GET settings after save |
| `assertSupplyArrProcurementExceptionEscalationPendingContains` | API pending preview assert |

### Frontend test IDs

Added on `ProcurementExceptionEscalationSettingsPanel`:

- `procurement-exception-escalation-enabled`
- `procurement-exception-escalation-cooldown-hours`
- `procurement-exception-escalation-max-escalations`
- `procurement-exception-escalation-notify`
- `procurement-exception-escalation-save`
- `procurement-exception-escalation-pending-empty` / `-pending-list`
- `procurement-exception-escalation-pending-{exceptionKey}`
- `procurement-exception-escalation-runs-empty` / `-runs-list`
- `procurement-exception-escalation-events-empty` / `-events-list`

### Catalog

- `StlE2ePlaywrightSpecCatalog.SupplyArrSettingsProcurementExceptionEscalationSmokeSpec`
- Registered in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests` assertion

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "Category=E2e&FullyQualifiedName~StlE2ePlaywrightSpecCatalog"
cd apps/supplyarr-frontend
npm run test -- ProcurementExceptionEscalationSettingsPanel
cd ../../tests/e2e-playwright
npm test -- --grep "procurement exception escalation"
# Live stack:
# $env:E2E_LIVE=1; npx playwright test supplyarr-settings-procurement-exception-escalation-smoke.spec.ts
```

## Out of scope

- Live internal escalation process-batch journey smoke
- RoutArr dispatch notification webhook clear-on-disable smoke

## Next recommended slice

- **M13 Playwright** — RoutArr dispatch notification settings panel explicit webhook clear on disable smoke (settings-only; optional explicit clear when disabling with intent)
