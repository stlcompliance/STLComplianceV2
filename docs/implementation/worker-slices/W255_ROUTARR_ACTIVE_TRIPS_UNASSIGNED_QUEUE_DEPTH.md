# W255 — RoutArr M9 active trips / unassigned queue depth (W211/W212)

Builds on **W211** (`ActiveTripsService`, `GET /api/dispatch/active-trips`, `ActiveTripsPanel`) and **W212** (`UnassignedWorkQueueService`, `GET /api/dispatch/unassigned-work-queue`, `UnassignedWorkQueuePanel`). No new migration — extends read APIs, enrichment, filters, assignment preview UX, and dispatch workspace panels.

## Scope

### Active trips depth

Query params on `GET /api/dispatch/active-trips`:

| Param | Behavior |
|-------|----------|
| `attentionOnly=true` | Late or at-risk trips only |
| `statusFilter=dispatched\|in_progress` | Filter by execution status (default all) |

Row enrichment:

- `assignedDriverDisplayName` from `routarr_staffarr_person_refs`
- `completedStopCount`, `totalStopCount`, `stopProgressPercent` from scoped route stops
- `openExceptionCount` from open/assigned dispatch exceptions linked to trip

Summary adds `unassignedCount`, `openExceptionCount`.

Rules: `ActiveTripsFilterRules`, `ActiveTripsProgressRules`.

### Unassigned work queue depth

Query param on `GET /api/dispatch/unassigned-work-queue`:

| Param | Behavior |
|-------|----------|
| `attentionOnly=true` | Late or at-risk unassigned trips only |

Response `summary` object:

- `unassignedCount`, `lateCount`, `atRiskCount`, `urgentCount` (late + at-risk)

Row adds `minutesUntilStart`; items sorted by urgency (late → at-risk → scheduled start).

Rules: `UnassignedWorkQueueUrgencyRules`.

### Frontend

**`ActiveTripsPanel`** (`data-testid="active-trips-panel"`):

- Needs-attention filter + status filter dropdown
- Driver display names, stop progress bar, open exception badges
- Summary tile for unassigned active trips

**`UnassignedWorkQueuePanel`** (`data-testid="unassigned-work-queue-panel"`):

- Urgent-only filter + urgent count in header
- Minutes-until-start on rows
- Preview-before-assign via shared `confirmDispatchAssignmentPreview` (W252)
- Bulk preview via `POST /api/dispatch/bulk/preview` before bulk apply
- Status message line (`unassigned-queue-status`)

## Tests

| Suite | Coverage |
|-------|----------|
| `ActiveTripsFilterRulesTests` | Status/attention filter validation |
| `ActiveTripsProgressRulesTests` | Stop progress percent |
| `UnassignedWorkQueueUrgencyRulesTests` | Urgency sort + attention filter |
| `RoutArrActiveTripsTests` | Attention filter, driver display name, open exception count |
| `RoutArrUnassignedWorkQueueTests` | Attention filter, summary counts, bulk assign |
| `ActiveTripsPanel.test.tsx` | Filters, progress, exception badge |
| `UnassignedWorkQueuePanel.test.tsx` | Urgent summary, attention filter |

## Verification

```powershell
dotnet build apps/routarr-api/RoutArr.Api/RoutArr.Api.csproj -c Release
dotnet test tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~ActiveTrips|FullyQualifiedName~UnassignedWork"
cd apps/routarr-frontend
npm run test -- --run ActiveTripsPanel UnassignedWorkQueuePanel
npm run build
```

## Out of scope

- Geo map coordinates / GPS tracking
- Auto-assign driver suggestions worker
- M13 Playwright depth smoke (optional follow-up)

## Next slice

- **M13 Playwright** — dispatch exception triage depth smoke (bulk/template interactions)
- **RoutArr M9** — proof/DVIR capture depth follow-ups
- **NexArr M12** — platform-admin worker health orchestration UI
