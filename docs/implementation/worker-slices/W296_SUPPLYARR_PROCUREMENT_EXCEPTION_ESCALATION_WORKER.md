# W296 — SupplyArr M8 procurement exception SLA escalation worker / notifications (W250)

Builds on **W250** (`SlaDueAt`, resolver workflow, `ProcurementExceptionsPanel`) and **W129** (procurement notification outbox).

## Scope

### Schema

Migration: `SupplyArrProcurementExceptionEscalationWorker`

| Table / column | Purpose |
|----------------|---------|
| `supplyarr_tenant_procurement_exception_escalation_settings` | Tenant escalation policy |
| `supplyarr_procurement_exception_escalation_events` | Per-exception escalation audit |
| `supplyarr_procurement_exception_escalation_runs` | Batch run audit |
| `supplyarr_procurement_exceptions` | `LastEscalatedAt`, `EscalationCount` |

### Notification event kind

- `procurement_exception_sla_escalation` on procurement notification outbox (repeatable enqueue per escalation)

### Tenant admin APIs (JWT + SupplyArr admin)

| Method | Path | Purpose |
|--------|------|---------|
| GET | `/api/procurement-exception-escalation-settings` | Read worker settings |
| PUT | `/api/procurement-exception-escalation-settings` | Upsert worker settings |
| GET | `/api/procurement-exception-escalation-settings/pending` | Preview due escalations |
| GET | `/api/procurement-exception-escalation-settings/runs` | Recent worker runs |
| GET | `/api/procurement-exception-escalation-settings/events` | Recent escalation events |

### Internal APIs (service token)

| Method | Path | Scope |
|--------|------|-------|
| GET | `/api/internal/procurement-exception-escalations/pending` | `supplyarr.procurement_exceptions.escalate` |
| POST | `/api/internal/procurement-exception-escalations/process-batch` | same |

Existing procurement notification dispatch worker continues to deliver pending webhook outbox rows.

### Shared worker

- `SupplyArrProcurementExceptionEscalationsJob` — default 60 min interval, batch 50
- Config: `SupplyArrProcurementExceptionEscalations__SupplyArrBaseUrl`, `SupplyArrProcurementExceptionEscalations__ServiceToken`

### Frontend (supplyarr-frontend)

- Settings → `ProcurementExceptionEscalationSettingsPanel` — enable toggle, cooldown, max escalations, notify toggle, pending/runs/events preview

## Tests

- `ProcurementExceptionEscalationRulesTests` — SLA breach, cooldown, max escalations
- `SupplyArrProcurementExceptionEscalationWorkerTests` — auth, pending preview, batch escalation + notification enqueue
- `ProcurementExceptionEscalationSettingsPanel.test.tsx` — panel render

## Verification

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.SupplyArr.Auth.Tests/STLCompliance.SupplyArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~ProcurementExceptionEscalation"
cd apps/supplyarr-frontend
npm run test -- ProcurementExceptionEscalationSettingsPanel
```

## Out of scope

- M13 Playwright escalation settings smoke
- StaffArr resolver directory lookup for auto-assign on escalation

## Next recommended slice

- **M13 Playwright** — RoutArr dispatch notification settings panel explicit webhook clear on disable smoke (settings-only; optional explicit clear when disabling with intent)
