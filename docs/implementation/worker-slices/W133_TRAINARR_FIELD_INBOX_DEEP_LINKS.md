# Worker 133 — TrainArr field-inbox deep links (M6/M13)

## Scope

Vertical slice connecting companion/product field inbox tasks to TrainArr assignment and evidence surfaces:

- **TrainArr API** — field inbox items include contextual `deepLinkPath`, optional `deepLinkUrl` (when `TrainArr:FrontendBaseUrl` is configured), and `blockedReason` for evidence/signoff gates
- **TrainArr Frontend** — SPA routes `/assignments/{id}` and `/assignments/{id}/evidence` with `AssignmentWorkspacePage` (detail, evidence upload, evaluation/signoffs)
- **Companion Frontend** — prefers API `deepLinkUrl` on inbox task cards when present
- **Shared** — `FieldInboxDeepLinkBuilder` helper for product frontend URL composition
- Tests and docs

## Deep link paths

| Situation | Path | Blocked reason |
|-----------|------|----------------|
| Assigned / in progress, no evidence | `/assignments/{id}/evidence` | Evidence required (or upload prompt) |
| In progress, evidence present, gate not met | `/assignments/{id}` | Evaluation and signoffs required |
| Otherwise | `/assignments/{id}` | — |

## Configuration

- `TrainArr:FrontendBaseUrl` — used to populate `deepLinkUrl` on field inbox items (Render: `TrainArr__FrontendBaseUrl` on `trainarr-api`)
- Companion still supports `VITE_TRAINARR_FRONTEND_BASE` fallback via `deepLinkPath` when `deepLinkUrl` is null

## Tests

- `FieldInboxDeepLinkBuilder` unit tests (NexArr auth test project)
- `TrainArrFieldInboxTests` — evidence deep link + `deepLinkUrl` composition
- `AssignmentWorkspacePage.test.tsx` — route render + evidence scroll focus
- `FieldInboxPanel.test.tsx` — companion prefers `deepLinkUrl`
