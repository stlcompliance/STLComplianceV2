# Worker 61 — MaintainArr labor / evidence capture

## Slice name

M7 maintenance spine — work order task lines, labor entries (opaque StaffArr person id), and evidence metadata with on-disk storage (TrainArr W27 pattern), JWT APIs, maintainarr-frontend detail capture, integration and frontend tests.

## Products touched

- **MaintainArr API** (`apps/maintainarr-api`): `maintainarr_work_order_task_lines`, `maintainarr_work_order_labor_entries`, `maintainarr_work_order_evidence`, `WorkOrderLaborEvidenceService`, `MaintainArrEvidenceStorageService`, nested `/api/work-orders/{id}/tasks|labor|evidence`, EF migration `MaintainArrWorkOrderLaborEvidence`.
- **MaintainArr Frontend** (`apps/maintainarr-frontend`): `WorkOrderLaborEvidencePanel`, extended `WorkOrdersPanel` detail, API client methods, HomePage queries/mutations.

## Schema

Migration: `MaintainArrWorkOrderLaborEvidence`

Added MaintainArr tables:

- `maintainarr_work_order_task_lines` — tenant-scoped job/task lines on a work order (`title`, `description`, `sortOrder`, `status` pending/in_progress/completed)
- `maintainarr_work_order_labor_entries` — labor hours with opaque `personId`, optional `workOrderTaskLineId`, `laborTypeKey` (regular/overtime/travel)
- `maintainarr_work_order_evidence` — evidence metadata (`evidenceTypeKey`, file name, content type, size, `storageKey`, notes, uploader)

Notes:

- File bytes stored under `EvidenceStorage:RootPath` (default `data/maintainarr-evidence`) keyed by tenant/work order/evidence id (same pattern as TrainArr W27).
- Tasks, labor, and evidence can only be added while work order status is `open` or `in_progress`.
- Uploading evidence on an `open` work order transitions it to `in_progress` (sets `startedAt`).

## API + auth changes

### MaintainArr user API (JWT)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/work-orders/{workOrderId}/tasks` | Work orders read + work order access |
| POST | `/api/work-orders/{workOrderId}/tasks` | Work orders perform + work order access |
| GET | `/api/work-orders/{workOrderId}/labor` | Work orders read + work order access |
| POST | `/api/work-orders/{workOrderId}/labor` | Work orders perform + work order access |
| GET | `/api/work-orders/{workOrderId}/evidence` | Work orders read + work order access |
| POST | `/api/work-orders/{workOrderId}/evidence` | Work orders perform + work order access |

## Frontend changes

- `WorkOrderLaborEvidencePanel` — task list/add, labor log form, evidence upload (base64) inside work order detail
- `WorkOrdersPanel` — embeds labor/evidence panel when a work order is selected
- `HomePage` — React Query for tasks/labor/evidence; mutations for add task, log labor, upload evidence
- API client: `getWorkOrderTasks`, `createWorkOrderTask`, `getWorkOrderLabor`, `logWorkOrderLabor`, `getWorkOrderEvidence`, `uploadWorkOrderEvidence`

## Tests

### Backend integration (`STLCompliance.MaintainArr.Auth.Tests`)

- `Work_order_tasks_labor_and_evidence_lifecycle`
- `Cannot_add_labor_to_completed_work_order`
- `Technician_can_log_labor_on_assigned_work_order`
- `Technician_cannot_add_labor_to_unassigned_work_order`

### Frontend unit

- `WorkOrderLaborEvidencePanel.test.tsx` — empty selection + open work order sections
- `WorkOrdersPanel.test.tsx` — updated props for labor/evidence wiring

## Verification commands

```powershell
dotnet build "apps/maintainarr-api/MaintainArr.Api/MaintainArr.Api.csproj" -c Release
dotnet test "tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~LaborEvidence"
cd apps/maintainarr-frontend
npm run test
npm run build
```

## Remaining gaps

- Task line status updates / completion from UI
- Evidence download endpoint
- Maintenance history roll-up of labor hours
- SupplyArr parts demand from work orders

## Next recommended slice

**SupplyArr parts demand from work orders** or **RoutArr trip/dispatch foundations** per M7/M8 milestone priority — see `docs/implementation/worker-slices/00_SLICE_STATE.md`.
