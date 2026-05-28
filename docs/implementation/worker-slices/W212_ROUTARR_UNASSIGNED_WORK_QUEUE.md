# Worker 212 — RoutArr M9 unassigned work queue panel

## Slice name

M9 unassigned work queue — focused API and Dispatch workspace panel for active trips without an assigned driver, aligned with dispatch board `workQueue.unassignedDriverTripCount`, with quick assign via existing assign-driver and bulk dispatch APIs.

## Products touched

- **RoutArr API** (`apps/routarr-api`): `UnassignedWorkQueueService`, `GET /api/dispatch/unassigned-work-queue`.
- **RoutArr Frontend** (`apps/routarr-frontend`): `UnassignedWorkQueuePanel` with per-trip and bulk driver assign.
- **Tests**: `RoutArrUnassignedWorkQueueTests`, `UnassignedWorkQueuePanel.test.tsx`.

## API

| Method | Route | Behavior |
|--------|-------|----------|
| `GET` | `/api/dispatch/unassigned-work-queue?scope=daily\|weekly` | Lists scoped unassigned active trips + `driverRefs` + board-aligned count; audit `dispatch_unassigned_work_queue.read` |

### Selection rules (matches board work queue)

- Trip in board scope window
- Dispatch status in `planned`, `assigned`, `dispatched`, `in_progress`
- `assignedDriverPersonId` is null/empty
- Late/at-risk flags via `DispatchBoardRules`
- Route and pending-stop counts from scoped routes/stops

No new migration.

### Assign actions (existing APIs, not new endpoints)

- Single trip: `PATCH /api/trips/{id}/assign-driver`
- Multi-select: `POST /api/dispatch/bulk/apply` with `BulkDispatchActionItem` driver only

## Frontend

- Panel on Dispatch workspace (after command center)
- Trip list with late/at-risk highlighting
- Per-row driver dropdown + Assign
- Bulk: select trips, choose driver, **Assign N selected** via bulk apply
- Invalidates unassigned queue, board, command center, active trips, and trips queries on success

## Tests

| Suite | Coverage |
|-------|----------|
| `RoutArrUnassignedWorkQueueTests` | List unassigned trip; assign-driver removes from queue; bulk apply clears item |
| `UnassignedWorkQueuePanel.test.tsx` | Renders queue and bulk assign control |

## Verification commands

```powershell
dotnet test "tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~RoutArrUnassignedWorkQueue"
cd apps/routarr-frontend
npm run test -- UnassignedWorkQueuePanel
```

## Relationship to W209–211

Command center and board surface aggregate counts; W212 gives operators a **dedicated triage surface** for the board’s unassigned-driver work queue metric with one-click assignment paths already built in Workers 78–80.

## Next recommended RoutArr slice

**Worker 213 — RoutArr M9 trip execution / driver portal** (`start trip`, `complete trip`, today’s assigned trips) or **RoutArr M12** transportation reporting cluster per backlog priority.
