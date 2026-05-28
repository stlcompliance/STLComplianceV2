# W251 — RoutArr dispatch closeout depth (W82)

Builds on **W82** (`DispatchCloseoutService`, summary/preview/apply, `DispatchCloseoutPanel`). No new migration — extends closeout APIs, audit, and dispatch workspace UX.

## Scope

### Trip closeout checklist

- `DispatchCloseoutChecklistRules` — per-trip items: driver assigned, stops/routes closed, exceptions clear, DVIR/proof (recommended), disposition readiness
- `GET /api/dispatch/closeout/checklists?scope=daily|weekly&remainingTripDisposition=complete|cancel`
- Audit: `dispatch_closeout.checklists`

### Bulk closeout (subset of trips)

- `DispatchCloseoutRequest.TripIds` optional on preview/apply
- When set, only selected open trips and their routes/stops are closed
- Audit: `dispatch_closeout.bulk_apply` with selected count in result detail

### Closeout audit trail

- `GET /api/dispatch/closeout/audit?limit=25` — recent `dispatch_closeout.*`, `route_stop.closeout`, `route.closeout` events
- Audit: `dispatch_closeout.audit.list`

### Dispatch workspace panel

`DispatchCloseoutPanel` (`data-testid="dispatch-closeout-panel"`):

- Per-trip checklist with expand/collapse and ready/blocked badges
- Trip checkboxes for bulk closeout (select all / clear)
- Preview/apply respects selection vs full-window closeout
- Recent closeout audit list

## Tests

| Suite | Coverage |
|-------|----------|
| `RoutArrDispatchCloseoutTests` | Checklists, bulk apply one of two trips, audit bulk_apply entry |
| `DispatchCloseoutChecklistRulesTests` | Cancel ready, complete blocked on open stops |
| `DispatchCloseoutRulesTests` | Existing W82 rule unit tests (unchanged) |
| `DispatchCloseoutPanel.test.tsx` | Checklist + audit render, bulk selection label |

## Verification

```powershell
dotnet test tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~Closeout"
cd apps/routarr-frontend
npm run test -- --run DispatchCloseoutPanel
```

## Out of scope

- Closeout approval workflow / multi-dispatcher sign-off
- Automated closeout cron (Render worker)
- DVIR/proof as hard blockers on apply (checklist warnings only)

## Next slice

- **RoutArr** — drag-assign depth (W78) or dispatch exception triage depth
- **M13 Playwright** — dispatch closeout panel smoke (`dispatch-closeout-panel`)
- **MaintainArr / SupplyArr** — product backlog per `00_SLICE_STATE.md`
