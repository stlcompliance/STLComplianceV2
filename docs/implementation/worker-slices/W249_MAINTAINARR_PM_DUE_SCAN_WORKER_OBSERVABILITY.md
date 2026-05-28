# W249 — MaintainArr PM due-scan worker observability (W51)

Builds on **W51** / **W57** (PM due scan worker, internal `process-due-scan`, `shared-worker` `MaintainArrPmDueScanJob`).

## Scope

### API (`apps/maintainarr-api`)

| Area | Detail |
|------|--------|
| Tables | `maintainarr_tenant_pm_due_scan_settings`, `maintainarr_pm_due_scan_runs` |
| Settings | `GET/PUT /api/pm-due-scan-settings` — enable, scan interval (minutes), batch size, overdue grace days, `lastRunAt`, `pendingPmCount` |
| Observability | `GET /pending`, `GET /runs`, `POST /trigger` (manual due scan for tenant) |
| Worker | `PmDueScanService.ProcessBatchAsync` records runs + updates `LastRunAt`; `TenantId` null processes all **enabled** tenants respecting interval gate |
| Internal | `POST /api/internal/pm/process-due-scan` records runs (`recordRun: true`) |
| Auth | `RequirePmDueScanSettingsManage` (maintainarr admin / platform admin) |

### Frontend (`apps/maintainarr-frontend`)

- `PmDueScanSettingsPanel` on Settings workspace (`pm-due-scan-settings-panel`)
- Last run, pending PM count, interval/batch/grace controls, **Run due scan now**, pending preview, recent runs

### Shared worker

- `MaintainArrPmDueScanJob` already registered in `shared-worker` `Program.cs`
- When `MaintainArrPmDueScan:TenantId` is null, internal scan processes enabled tenants from DB settings

## Tests

| Suite | Coverage |
|-------|----------|
| `MaintainArrPmDueScanWorkerTests` | Settings get defaults + pending count; put + trigger + run history; admin-only put |
| `PmDueScanSettingsPanel.test.tsx` | Panel render, pending count, trigger button |

## Verification

```powershell
dotnet test tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~PmDueScan"
cd apps/maintainarr-frontend
npm run test -- --run PmDueScanSettingsPanel
```

## Out of scope

- Changing PM schedule CRUD or driver portal surfaces
- Playwright M13 smoke for this panel

## Next slice

- **SupplyArr M8** — procurement exception resolution depth (W197)
- **RoutArr** — dispatch closeout / drag-assign depth (W78/W82)
- **M13 Playwright** — MaintainArr PM due-scan settings smoke (optional)
