# Worker 39 — Compliance Core batch workflow gate checks

## Slice name

M5/M10 batch workflow gate checks — `POST /api/workflow-gates/check/batch` and `POST /api/internal/workflow-gate-check/batch` evaluating multiple gates with shared context/facts using existing W34 gate logic, JWT + service token auth, compliancecore-frontend batch panel, integration and frontend tests

## Products touched

- **Compliance Core API** (`apps/compliancecore-api`): batch contracts, `WorkflowGateService.CheckBatchForUserAsync` / `CheckBatchInternalAsync`, user and internal endpoints
- **Compliance Core Frontend** (`apps/compliancecore-frontend`): `BatchWorkflowGateCheckPanel` on Findings & gates tab
- **Integration tests** (`tests/STLCompliance.ComplianceCore.Auth.Tests`): batch cases in `ComplianceCoreFindingsWorkflowGateTests`

## API + auth changes

### Compliance Core user API (JWT + Compliance Core entitlement)

| Method | Route | Auth |
|--------|-------|------|
| POST | `/api/workflow-gates/check/batch` | check: same as single gate (`RequireWorkflowGateCheck`) |

Request: `items[]` with `gateKey` and optional per-item `facts` / `context` overrides; request-level shared `facts`, `context`, `emitFindings`. Duplicate gate keys deduplicated (last wins). Max 50 gates per batch.

Response: `batchId`, `results[]` (same shape as single check), `summary` (`total`, `allowCount`, `warnCount`, `blockCount`).

Each item runs full rule evaluation and persists a `workflow_gate_check_result` row. Audit: `workflow_gates.check_batch` on `workflow_gate_check_batch` target.

### Compliance Core internal API (NexArr service token → Compliance Core)

| Method | Route | Auth |
|--------|-------|------|
| POST | `/api/internal/workflow-gate-check/batch` | service token → `compliancecore`, scope `compliancecore.workflow.gates.check` |

Request: `tenantId`, `items[]` with `gateKey` and optional per-item `context`; shared `context`, `emitFindings`. Resolves facts via fact source registry per gate (same as single internal check).

## Frontend changes

- **BatchWorkflowGateCheckPanel** — multi-select gates, shared fact checkboxes, batch summary and per-gate outcomes
- Wired from **FindingsWorkflowGatesPanel** / Home page Findings & gates tab

## Tests

### Backend integration (`ComplianceCoreFindingsWorkflowGateTests`)

- `Workflow_gate_batch_check_returns_per_gate_results_and_summary`
- `Workflow_gate_batch_check_allows_when_shared_facts_pass`
- `Workflow_gate_batch_check_rejects_empty_items`
- `Internal_workflow_gate_batch_check_requires_service_token`
- `Internal_workflow_gate_batch_check_warns_on_unresolved_facts`

### Frontend unit

- `BatchWorkflowGateCheckPanel.test.tsx`

## Verification commands

```powershell
dotnet build -c Release
dotnet test "tests/STLCompliance.ComplianceCore.Auth.Tests/STLCompliance.ComplianceCore.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~Workflow_gate_batch|FullyQualifiedName~Internal_workflow_gate_batch"
cd apps/compliancecore-frontend
npm install
npm run test -- --run
npm run build
```

## Remaining gaps

- No concurrency limit on parallel gate evaluations within a batch (unlike TrainArr batch evaluate semaphore)
- Internal batch does not support per-item `facts` (internal path uses fact resolution only)
- Cross-product callers (TrainArr/StaffArr) not yet wired to batch internal gate check

## Next recommended slice

**TrainArr rule-pack requirement intake** (M6/M10) or **Compliance Core cross-product batch evaluate API** per milestone priority.
