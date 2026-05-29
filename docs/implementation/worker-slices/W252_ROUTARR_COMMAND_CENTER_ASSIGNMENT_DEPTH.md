# W252 — RoutArr dispatch command center assignment depth (W78/W209)

Builds on **W78** (assignment preview, conflict/eligibility/gate checks) and **W209** (dispatch command center). No new migration — extends command center UX to use the same assignment safety path as `DispatchAssignmentPanel`.

## Scope

### Shared assignment confirm helper

- `confirmDispatchAssignmentPreview` in `apps/routarr-frontend/src/lib/dispatchAssignment.ts`
- `dispatchAssignment.test.ts` — blocking decline/accept and eligibility warning paths
- `DispatchAssignmentPanel` refactored to use shared helper + `DRAG_MIME` constant

### Command center panel

`DispatchCommandCenterPanel` (`data-testid="dispatch-command-center-panel"`):

- Driver assign from select runs `POST /api/dispatch/assignments/preview` then `PATCH /api/trips/{id}/assign-driver` with ignore flags when user confirms blocks
- Draggable driver chips from command center `driverRefs`; drop onto trip cards (same MIME as W78)
- Status message line (`command-center-status`)
- Trip card test ids: `command-center-trip-*`, `command-center-assign-*`, `command-center-driver-chips`

Uses existing server auth: `RequireTripsAssign` on preview and assign-driver (unchanged).

## Tests

| Suite | Coverage |
|-------|----------|
| `dispatchAssignment.test.ts` | Confirm helper blocking + warning |
| `DispatchCommandCenterPanel.test.tsx` | Columns, driver chips, preview-before-assign |
| `DispatchAssignmentPanel.test.tsx` | Existing W78 panel (unchanged behavior via shared helper) |

## Verification

```powershell
cd apps/routarr-frontend
npm run test -- --run dispatchAssignment DispatchCommandCenterPanel DispatchAssignmentPanel
npm run build
dotnet test tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~DispatchCommandCenter|FullyQualifiedName~DispatchAssignment"
```

## Out of scope

- Vehicle assign from command center columns (driver-only in status columns)
- Bulk assignment from command center (see W80 bulk panel)
- M13 Playwright assign mutation smoke (read-only E2E remains W235)

## Next slice

- **M13 Playwright** — dispatch closeout panel smoke (`dispatch-closeout-panel`)
- **RoutArr** — dispatch exception triage depth (W210)
- **NexArr M12** — platform-admin worker health orchestration UI (optional)
