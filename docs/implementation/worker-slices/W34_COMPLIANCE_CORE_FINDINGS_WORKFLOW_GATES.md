# Worker 34 — Compliance Core findings + workflow gate API

## Slice name

M5 findings + workflow gates — compliance findings linked to evaluation runs, workflow gate definitions/checks with allow/warn/block outcomes tied to rule evaluation, JWT user APIs and service-token internal gate check, compliancecore-frontend findings list + gate check panel, integration and frontend tests

## Products touched

- **Compliance Core API** (`apps/compliancecore-api`): findings entities, workflow gate definitions/check results, user and internal APIs
- **Compliance Core Frontend** (`apps/compliancecore-frontend`): Findings & gates tab
- **Integration tests** (`tests/STLCompliance.ComplianceCore.Auth.Tests`): `ComplianceCoreFindingsWorkflowGateTests`

## Schema

### Compliance Core migration `ComplianceCoreFindingsWorkflowGates`

- `compliancecore_findings` — tenant-scoped findings linked to rule packs and optional evaluation runs (severity warn/block, status open/acknowledged/resolved)
- `compliancecore_workflow_gate_definitions` — gate key + label linked to a rule pack
- `compliancecore_workflow_gate_check_results` — persisted gate check outcomes with reasons JSON

## API + auth changes

### Compliance Core user APIs (JWT + Compliance Core entitlement)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/findings` | read: entitled users (same as rule evaluation) |
| POST | `/api/findings` | manage: `compliance_admin`, `compliance_reviewer`, `tenant_admin` |
| PATCH | `/api/findings/{id}/status` | manage: findings.manage roles |
| GET | `/api/workflow-gates` | read: entitled users |
| POST | `/api/workflow-gates` | manage: rule pack create roles |
| POST | `/api/workflow-gates/check` | check: entitled users |

`POST /api/rule-packs/{id}/evaluate` accepts optional `emitFindings` — creates findings for failed rules and unresolved facts when evaluation does not pass.

Gate check runs rule pack evaluation (facts from request), maps pass → allow, fail → block, unresolved facts → warn (same outcome model as internal evaluate). Optional `emitFindings` on check persists findings.

### Compliance Core internal API (NexArr service token → Compliance Core)

| Method | Route | Auth |
|--------|-------|------|
| POST | `/api/internal/workflow-gate-check` | service token → `compliancecore`, scope `compliancecore.workflow.gates.check` |

Resolves facts via fact source registry (like internal evaluate), evaluates linked rule pack, returns gate outcome with reasons. Optional `emitFindings` persists evaluation snapshot + findings.

`POST /api/internal/evaluate` now accepts optional `emitFindings`.

## Frontend changes

- Home page seventh tab: **Findings & gates**
- **FindingsWorkflowGatesPanel** — findings list, workflow gate definitions, gate check form with fact inputs and allow/warn/block result display, seed sample gate

## Tests

### Backend integration (`ComplianceCoreFindingsWorkflowGateTests`)

- `Evaluate_with_emit_findings_creates_findings_for_failed_rules`
- `Workflow_gate_check_blocks_when_rules_fail`
- `Workflow_gate_check_allows_when_all_rules_pass`
- `Internal_workflow_gate_check_requires_service_token`
- `Internal_workflow_gate_check_warns_on_unresolved_facts`
- `Findings_manage_denies_tenant_member_create`

### Frontend unit

- `FindingsWorkflowGatesPanel.test.tsx`

## Verification commands

```powershell
dotnet build -c Release
dotnet test "tests/STLCompliance.ComplianceCore.Auth.Tests/STLCompliance.ComplianceCore.Auth.Tests.csproj" -c Release
cd apps/compliancecore-frontend
npm install
npm run test -- --run
npm run build
```

## Remaining gaps

- Waiver-aware evaluations and non-waivable rules deferred
- Batch workflow gate checks across multiple gates deferred
- Audit package export deferred
- Product API fact fetch adapters still context-only for unresolved product_api sources

## Next recommended slice

**TrainArr batch qualification checks** (M10) or **Compliance Core 9-CSV import/export** (M5) per milestone priority.
