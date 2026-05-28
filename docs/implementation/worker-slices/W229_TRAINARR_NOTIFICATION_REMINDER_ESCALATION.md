# Worker 229 — TrainArr M12 notification settings + scheduled workers

Builds on **W123** (notification settings foundations) and **W161** (dispatch enhancements).

## Scope

- **Assignment due reminder worker** — scans open assignments approaching `DueAt`, tracks reminder counts on assignments, enqueues `assignment_due_reminder` webhooks
- **Assignment overdue escalation worker** — scans overdue open assignments, records escalation events, enqueues `assignment_overdue_escalation` webhooks
- **Notification settings** — `NotifyOnAssignmentDueReminder`, `NotifyOnAssignmentOverdueEscalation` toggles on tenant webhook settings
- **Settings APIs** — CRUD + pending preview + runs (+ escalation events)
- **shared-worker** — `TrainArrAssignmentDueRemindersJob`, `TrainArrAssignmentEscalationJob`
- **trainarr-frontend** — `AssignmentReminderEscalationSettingsPanel` on Settings workspace; notification panel toggles

## Schema

Migration: `TrainArrAssignmentReminderEscalationWorkers`

| Table / column | Purpose |
|----------------|---------|
| `trainarr_tenant_assignment_due_reminder_settings` | Due-soon lead, cooldown, max reminders |
| `trainarr_assignment_due_reminder_runs` | Batch run audit |
| `trainarr_tenant_assignment_escalation_settings` | Overdue threshold, cooldown, max escalations |
| `trainarr_assignment_escalation_events` | Per-assignment escalation audit |
| `trainarr_assignment_escalation_runs` | Batch run audit |
| `trainarr_training_assignments` | `LastDueReminderSentAt`, `DueReminderCount`, `LastEscalatedAt`, `EscalationCount` |
| `trainarr_tenant_training_notification_settings` | New notify toggles for reminder/escalation events |

## Tenant admin APIs

| Method | Path | Purpose |
|--------|------|---------|
| GET/PUT | `/api/assignment-due-reminder-settings` | Due reminder policy |
| GET | `/api/assignment-due-reminder-settings/pending` | Preview due reminders |
| GET | `/api/assignment-due-reminder-settings/runs` | Recent runs |
| GET/PUT | `/api/assignment-escalation-settings` | Escalation policy |
| GET | `/api/assignment-escalation-settings/pending` | Preview escalations |
| GET | `/api/assignment-escalation-settings/runs` | Recent runs |
| GET | `/api/assignment-escalation-settings/events` | Recent escalation events |
| GET/PUT | `/api/notification-settings` | Extended with reminder/escalation webhook toggles |

## Internal APIs (service token)

| Method | Path | Scope |
|--------|------|-------|
| GET/POST | `/api/internal/assignment-due-reminders/*` | `trainarr.assignments.due_reminders.dispatch` |
| GET/POST | `/api/internal/assignment-escalations/*` | `trainarr.assignments.escalate` |

Existing `trainarr.notifications.dispatch` worker continues to deliver pending webhook outbox rows.

## Shared worker

- `TrainArrAssignmentDueRemindersJob` — default 60 min, batch 50
- `TrainArrAssignmentEscalationJob` — default 60 min, batch 50
- Profiles: `worker-trainarr-due-reminders`, `worker-trainarr-assignment-escalation`
- Render env: `TrainArrAssignmentDueReminders__TrainArrBaseUrl`, `TrainArrAssignmentEscalation__TrainArrBaseUrl`

## Tests

- `AssignmentDueReminderRulesTests`, `AssignmentEscalationRulesTests`
- `TrainArrAssignmentReminderEscalationWorkerTests` — auth, due reminder batch, escalation batch
- `AssignmentReminderEscalationSettingsPanel.test.tsx`

## Verification

```powershell
dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj --filter "FullyQualifiedName~AssignmentDueReminder|FullyQualifiedName~AssignmentEscalation|FullyQualifiedName~TrainArrAssignmentReminder"
cd apps/trainarr-frontend
npm test -- AssignmentReminderEscalationSettingsPanel
```

## Out of scope

- StaffArr personnel audit export (W228)
- Replacing W123/W161 notification dispatch worker

## Next slice

Per backlog: **MaintainArr M12** audit export filter parity, **SupplyArr** integration depth, or **Compliance Core** scheduled delivery workers.
