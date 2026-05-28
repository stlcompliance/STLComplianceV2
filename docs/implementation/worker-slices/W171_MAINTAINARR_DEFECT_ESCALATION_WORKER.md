# Worker 171 — MaintainArr defect escalation worker (M12)

**Products:** MaintainArr, shared-worker, maintainarr-frontend  
**Milestone:** M12  
**Backlog:** MaintainArr `[M12] defect escalation worker`

## Summary

Scheduled worker escalates stagnant open defects per tenant-configured severity thresholds. Actions include auto-acknowledge, work-order creation, severity bump on repeat escalation, and optional defect-escalated notification enqueue. Platform admins configure behavior and review pending preview, runs, and events from MaintainArr settings.

## Backend (MaintainArr)

### Schema

- `maintainarr_tenant_defect_escalation_settings` — tenant escalation policy
- `maintainarr_defect_escalation_runs` — batch run audit
- `maintainarr_defect_escalation_events` — per-defect escalation action audit
- `maintainarr_defects` — `LastEscalatedAt`, `EscalationCount`
- `maintainarr_tenant_notification_settings` — `NotifyOnDefectEscalated`

### Tenant admin APIs (JWT + maintainarr admin)

| Method | Path | Purpose |
|--------|------|---------|
| GET | `/api/defect-escalation-settings` | Read escalation settings |
| PUT | `/api/defect-escalation-settings` | Upsert escalation settings |
| GET | `/api/defect-escalation-settings/pending` | Preview pending escalations |
| GET | `/api/defect-escalation-settings/runs` | Recent worker runs |
| GET | `/api/defect-escalation-settings/events` | Recent escalation events |

### Internal APIs (service token)

| Method | Path | Scope |
|--------|------|-------|
| GET | `/api/internal/defect-escalation/pending` | `maintainarr.defects.escalate` |
| POST | `/api/internal/defect-escalation/process-batch` | same |

## Shared worker

- `MaintainArrDefectEscalationJob` — default 30 min interval, batch 25
- Config: `MaintainArrDefectEscalation__MaintainArrBaseUrl`, `MaintainArrDefectEscalation__ServiceToken`

## Frontend (maintainarr-frontend)

- Settings → `DefectEscalationSettingsPanel` — enable toggle, thresholds, actions, pending/runs/events preview
- Notification settings — `NotifyOnDefectEscalated` toggle

## Tests

- `DefectEscalationRulesTests` — threshold and severity rules
- `MaintainArrDefectEscalationWorkerTests` — auth, pending preview, batch escalate

## Next slice

Per backlog: MaintainArr `[M12] asset status rollup worker` or next open M12 row from `02_PRODUCT_IMPLEMENTATION_BACKLOG.md`.
