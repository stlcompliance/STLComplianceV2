# Worker 56 — MaintainArr work-order lifecycle

## Slice name

M7 maintenance spine — work orders linked to assets with optional defect/PM schedule refs, status lifecycle (open/in_progress/completed/cancelled), JWT APIs with StaffArr person id assignment, maintainarr-frontend list/detail and create-from-defect, integration and frontend tests.

## Products touched

- **MaintainArr API** (`apps/maintainarr-api`): `maintainarr_work_orders`, `WorkOrderService`, `/api/work-orders` and `/api/defects/{id}/work-orders`, audit events, EF migration `MaintainArrWorkOrders`.
- **MaintainArr Frontend** (`apps/maintainarr-frontend`): `WorkOrdersPanel`, defect panel create-from-defect action, API client methods.

## Schema

Migration: `MaintainArrWorkOrders`

Added MaintainArr table:

- `maintainarr_work_orders` — tenant-scoped work orders (`assetId`, optional `defectId`, optional `pmScheduleId`, `workOrderNumber`, `title`, `description`, `priority`, `status`, `source`, `assignedTechnicianPersonId` opaque string, `createdByUserId`, lifecycle timestamps)

Notes:

- Status lifecycle: `open`, `in_progress`, `completed`, `cancelled`.
- `assignedTechnicianPersonId` stores StaffArr person id as opaque string (no cross-DB FK).
- Create from defect is idempotent while an active work order exists for the defect.
- Labor/parts deep slices deferred.

## API + auth changes

### MaintainArr user API (JWT)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/work-orders` | Work orders read — managers see all; technicians see created or assigned |
| GET | `/api/work-orders/{id}` | Work orders read — same visibility as list |
| POST | `/api/work-orders` | Work orders create (technician+) |
| PATCH | `/api/work-orders/{id}` | Work orders perform — update open/in-progress orders they can access |
| PATCH | `/api/work-orders/{id}/status` | Work orders perform — lifecycle transitions; cancel requires close (manager+) |
| POST | `/api/defects/{id}/work-orders` | Work orders create + defect access — idempotent from open defect |

`MaintainArrAuthorizationService` adds `RequireWorkOrdersRead`, `RequireWorkOrdersCreate`, `RequireWorkOrdersPerform`, `RequireWorkOrdersClose`, `CanViewAllWorkOrders`, `CanCloseAnyWorkOrder`, and `RequireWorkOrderAccess`.

## Frontend changes

- `WorkOrdersPanel` — list/filter, manual create, detail view, status transitions
- `DefectsPanel` — “Open work order” on open/acknowledged/in_repair defects
- Home workspace integrates work orders panel above defects
- API client: list/get/create/status + create from defect

## Tests

### Backend unit (`WorkOrderStatusRulesTests`)

- Lifecycle transition matrix for open/in_progress/completed/cancelled

### Backend integration (`STLCompliance.MaintainArr.Auth.Tests`)

- `Manual_work_order_create_and_status_lifecycle`
- `Create_work_order_from_defect_is_idempotent`
- `Technician_cannot_view_other_users_unassigned_work_order`
- `Technician_can_complete_assigned_work_order`
- `Technician_cannot_cancel_work_order`

### Frontend unit

- `WorkOrdersPanel.test.tsx` — list + empty state
- `client.test.ts` — work orders list success path

## Verification commands

```powershell
dotnet build "apps/maintainarr-api/MaintainArr.Api/MaintainArr.Api.csproj" -c Release
dotnet test "tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj" -c Release
cd apps/maintainarr-frontend
npm run test
npm run build
```

## Remaining gaps

- Work-order board / kanban UI
- Task/job lines, labor tracking, evidence capture
- Auto work-order generation on PM due implemented (Worker 57)
- Asset readiness restriction from critical defects
- SupplyArr parts demand from work orders

## Next recommended slice

**MaintainArr PM program builder** or **auto WO generation on PM due** per M7 milestone priority — see `docs/implementation/worker-slices/00_SLICE_STATE.md`.
