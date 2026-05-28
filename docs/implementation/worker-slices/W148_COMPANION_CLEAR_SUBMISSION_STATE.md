# Worker 148 — Companion clear submission state (M11)

## Scope

Post-sync and post-upload UX for companion field tasks: per-task submission status chips, activity toasts, NexArr submission-status API backed by `nexarr_companion_field_submissions`, offline queue integration, tests, and docs.

## NexArr

- Table `nexarr_companion_field_submissions` records acknowledge sync and evidence upload outcomes (synced/failed) per user/tenant/task.
- `GET /api/companion/field-tasks/submission-status?taskKeys=...` returns latest status per task and submission kind.
- Offline sync and evidence submit paths write submission records after success (or failure for evidence).

## Companion frontend

- `submissionState.ts` — local in-flight phases (queued, syncing, uploading) and short-lived toasts.
- `useFieldTaskSubmissionState` merges local state with server status.
- `SubmissionActivityBanner`, `TaskSubmissionStatusBadge` on inbox task cards.
- `useOfflineQueue` and `FieldTaskEvidencePanel` update local state and toasts; sync/upload refresh server status.

## Tests

- `submissionState.test.ts` (Vitest)
- `NexArrCompanionFieldSubmissionTests.cs`
- `companion-field-submission-state.spec.ts` (`E2E_LIVE`)
- `StlE2ePlaywrightSpecCatalog.CompanionFieldSubmissionStateSpec`

## Boundaries

NexArr stores **submission receipts** for companion UX only; TrainArr remains evidence authority. Acknowledgments are companion-local until synced to NexArr offline actions.
