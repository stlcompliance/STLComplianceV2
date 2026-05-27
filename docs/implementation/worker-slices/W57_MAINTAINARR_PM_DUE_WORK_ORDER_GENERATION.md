# Worker 57 — MaintainArr auto work-order generation on PM due

## Slice name

M7 maintenance spine — extend PM due scan to idempotently create work orders when schedules become due/overdue; linked work order on due PM list API and frontend; service-token internal hook via existing `process-due-scan`; unit and integration tests.

## Products touched

- **MaintainArr API** (`apps/maintainarr-api`): `PmWorkOrderGenerationRules`, `WorkOrderService.EnsureForDuePmScheduleAsync`, extended `PmDueScanService.ProcessBatchAsync`, linked WO fields on `PmScheduleResponse` for `/due`, migration index on `(tenant_id, pm_schedule_id, status)`.
- **shared-worker** (`workers/shared-worker`): `MaintainArrPmDueScanClient` and job logging include work-order generation counts.
- **maintainarr-frontend**: `PmDuePanel` shows linked work order number/status on due list.
- **Tests**: `PmWorkOrderGenerationRulesTests`, extended `MaintainArrPmDueScanWorkerTests`, frontend `PmDuePanel.test.tsx`.

## Schema

Migration: `MaintainArrPmDueWorkOrderGeneration`

- Index `IX_maintainarr_work_orders_TenantId_PmScheduleId_Status` on `maintainarr_work_orders` for idempotent active work-order lookup by PM schedule.

No new tables — work orders reuse existing `pmScheduleId` linkage from Worker 56.

## API + auth changes

### Extended internal PM due scan (service token)

Existing `POST /api/internal/pm/process-due-scan` (`maintainarr.pm.scan`, source `shared-worker`) now:

1. Marks schedules due/overdue (unchanged).
2. Ensures an open/in-progress work order exists for each due/overdue candidate (idempotent — returns existing active WO when present).

Response adds:

| Field | Description |
|-------|-------------|
| `workOrdersCreatedCount` | New work orders created this batch |
| `workOrdersLinkedCount` | Existing active work orders returned without duplicate create |
| `workOrderGenerationSkippedCount` | PM schedules where WO generation failed |
| `createdWorkOrderIds` | IDs of newly created work orders |
| `workOrderGenerationSkipped` | Per-schedule skip reasons |

Work orders created by scan use `source = pm_schedule`, worker actor user id, priority `medium` (due) or `high` (overdue), title `PM: {schedule name}`.

### User API (JWT)

`GET /api/preventive-maintenance/due` response (`PmScheduleResponse`) adds optional:

- `linkedWorkOrderId`
- `linkedWorkOrderNumber`
- `linkedWorkOrderStatus`

Populated when an open or in-progress work order exists for the schedule.

## Frontend changes

- `PmDuePanel` — new **Work order** column with WO number/status or “Pending generation”.
- `PmScheduleResponse` TypeScript type extended with linked work order fields.

## Tests

### Unit (`PmWorkOrderGenerationRulesTests`)

- `ShouldEnsureWorkOrder` due/overdue guard
- Priority mapping for due vs overdue
- Title and description builders

### Integration (`MaintainArrPmDueScanWorkerTests`)

- `Process_due_scan_marks_past_due_schedule_as_due` — asserts WO created
- `Process_due_scan_work_order_generation_is_idempotent`
- `Due_list_includes_linked_work_order`

### Frontend

- `PmDuePanel.test.tsx` — linked WO and pending-generation states

## Verification commands

```powershell
dotnet build -c Release
dotnet test "tests/STLCompliance.Shared.Worker.Tests/STLCompliance.Shared.Worker.Tests.csproj" -c Release --filter "FullyQualifiedName~Pm"
dotnet test "tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~PmDue"
cd apps/maintainarr-frontend
npm run test
npm run build
```

## Remaining gaps

- Catch-up WO generation for overdue schedules no longer picked up by due scan batch (overdue not in updatable due-status set)
- PM program builder (group schedules into programs) not implemented
- Manual “create work order” action on due PM row deferred (auto scan covers primary path)
- WO completion does not advance PM schedule `nextDueAt` (future slice)

## Next recommended slice

**MaintainArr PM program builder** per M7 milestone priority — see `docs/implementation/worker-slices/00_SLICE_STATE.md`.
