# Worker 30 — Compliance Core rule version content + evaluation foundations

## Slice name

M5 rule version content + evaluation — JSON rule body on rule packs, synchronous fact-based evaluator, evaluation run persistence, read/manage/evaluate APIs with JWT auth, admin UI tab, audit events, integration and frontend tests

## Products touched

- **Compliance Core API** (`apps/compliancecore-api`): rule content on rule packs, rule evaluation runs
- **Compliance Core Frontend** (`apps/compliancecore-frontend`): Rule evaluation tab with content editor and evaluation form
- **Compliance Core integration tests** (`tests/STLCompliance.ComplianceCore.Auth.Tests`): rule content and evaluation auth/CRUD tests

## Schema

### Compliance Core migration `ComplianceCoreRuleEvaluation`

- `compliancecore_rule_packs.rule_content_json` — optional JSONB structured rule body (schema version 1, `all`/`any` logic, `fact_boolean` rules)
- `compliancecore_rule_evaluation_runs` — tenant-scoped evaluation runs with fact inputs JSON, per-rule results JSON, overall pass/fail

## API + auth changes

### Compliance Core user APIs (JWT + Compliance Core entitlement)

| Method | Route | Auth |
|--------|-------|------|
| GET/PUT | `/api/rule-packs/{id}/content` | read: entitled users; update: `tenant_admin`, `compliance_admin` |
| POST | `/api/rule-packs/{id}/evaluate` | evaluate: entitled users (`tenant_member`, `compliance_reviewer`, admins) |
| GET | `/api/rule-evaluations?rulePackId=` | read: entitled users |
| GET | `/api/rule-evaluations/{id}` | read: entitled users |

Rule content supports `fact_boolean` rules referencing fact keys with expected boolean values. Evaluator applies `all` or `any` logic — real pass/fail based on provided facts (missing facts fail).

## Frontend changes

- Home page fifth tab: **Rule evaluation**
- **RuleEvaluationPanel** — rule pack selector, JSON content editor (admin), fact checkbox inputs, evaluate action, result display, evaluation history
- Admin seed action attaches sample driver qualification rules to first rule pack

## Tests

### Backend integration (`ComplianceCoreRuleEvaluationTests`)

- `Rule_content_update_get_and_evaluate_pass_and_fail`
- `Rule_content_update_denies_member_role`
- `Rule_evaluation_member_can_run_and_read`
- `Rule_evaluation_requires_compliancecore_entitlement`
- `Rule_evaluation_denies_missing_fact`

### Frontend unit

- `RuleEvaluationPanel.test.tsx` — empty state, evaluation form, history row rendering, seed button

## Verification commands

```powershell
dotnet build -c Release
dotnet test "tests/STLCompliance.ComplianceCore.Auth.Tests/STLCompliance.ComplianceCore.Auth.Tests.csproj" -c Release
dotnet test "tests/STLCompliance.Health.Tests/STLCompliance.Health.Tests.csproj" -c Release --filter "FullyQualifiedName~ComplianceCore"
cd apps/compliancecore-frontend
npm install
npm run test -- --run
npm run build
```

## Remaining gaps

- Full any/all/none visual rule builder and additional rule types (numeric thresholds, date checks) deferred
- 9-CSV import/export, fact source registry deferred
- Internal resolve/validate endpoints and cross-product evaluation consumption deferred
- Async evaluation worker and findings/audit packages deferred

## Next recommended slice

**TrainArr qualification suspend/revoke/expire** (M6) or **Compliance Core fact source registry + internal resolve API** (M5) per milestone priority.
