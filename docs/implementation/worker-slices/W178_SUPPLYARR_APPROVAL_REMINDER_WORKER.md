# Worker 178 — SupplyArr approval reminder worker (M12)

**Products:** SupplyArr, shared-worker, supplyarr-frontend  
**Milestone:** M12  
**Backlog:** SupplyArr `[M12] approval reminder worker`

## Summary

Scheduled worker scans submitted purchase requests and draft purchase orders awaiting approval, sends periodic reminders with configurable thresholds and cooldowns, tracks reminder state per subject, and enqueues webhook notifications via the existing procurement notification outbox.

## Backend (SupplyArr)

### Schema

Migration: `SupplyArrApprovalReminderWorker`

- `supplyarr_tenant_approval_reminder_settings` — tenant worker policy
- `supplyarr_approval_reminder_states` — per-subject reminder tracking
- `supplyarr_approval_reminder_runs` — batch run audit

### Tenant admin APIs (JWT + SupplyArr admin)

| Method | Path | Purpose |
|--------|------|---------|
| GET | `/api/approval-reminder-settings` | Read worker settings |
| PUT | `/api/approval-reminder-settings` | Upsert worker settings |
| GET | `/api/approval-reminder-settings/pending` | Preview due reminders |
| GET | `/api/approval-reminder-settings/runs` | Recent worker runs |

### Read APIs (JWT + purchase read)

| Method | Path | Purpose |
|--------|------|---------|
| GET | `/api/approval-reminders` | Dashboard of awaiting approvals + reminder state |

### Internal APIs (service token)

| Method | Path | Scope |
|--------|------|-------|
| GET | `/api/internal/approval-reminders/pending` | `supplyarr.approval_reminders.dispatch` |
| POST | `/api/internal/approval-reminders/process-batch` | same |

## Shared worker

- `SupplyArrApprovalRemindersJob` — default 60 min interval, batch 50
- Config: `SupplyArrApprovalReminders__SupplyArrBaseUrl`, `SupplyArrApprovalReminders__ServiceToken`

## Frontend (supplyarr-frontend)

- Settings → `ApprovalReminderSettingsPanel` — enable toggle, thresholds, cooldown, pending/runs preview
- Purchasing → `ApprovalRemindersPanel` — overdue and awaiting-approval dashboard

## Tests

- `ApprovalReminderRulesTests` — threshold, cooldown, max reminders, overdue
- `SupplyArrApprovalReminderWorkerTests` — auth, pending preview, batch reminder + notification enqueue, dashboard read
- `ApprovalReminderSettingsPanel.test.tsx` — panel render
- `ApprovalRemindersPanel.test.tsx` — dashboard render

## Next slice

Per backlog: SupplyArr `[M12] demand processing worker`.
