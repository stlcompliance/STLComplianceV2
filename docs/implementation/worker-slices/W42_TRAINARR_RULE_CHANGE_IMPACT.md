# Worker 42 — TrainArr rule change impact

## Slice name

M6/M10 rule change impact — assess training domain impact when Compliance Core rule pack content/status changes or version drift is detected via linked requirements, GET/POST impact APIs with JWT admin auth, baseline version capture on validated requirement upsert, trainarr-frontend impact panel, audit events, cross-product tests

## Products touched

- **TrainArr API** (`apps/trainarr-api`): `RulePackImpactService`, `KnownVersionNumber`/`KnownStatus` on `trainarr_training_rule_pack_requirements`, `/api/rule-pack-impact` routes
- **Compliance Core API** (read-only): existing `POST /api/internal/rule-packs/lookup` for current pack metadata
- **TrainArr Frontend** (`apps/trainarr-frontend`): `RulePackImpactPanel` for trainarr admins
- **Integration tests**: `StaffArrTrainArrRulePackImpactTests`

## API + auth changes

### TrainArr user API (JWT + TrainArr entitlement)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/rule-pack-impact?rulePackKey=&expectedVersionNumber=&expectedStatus=` | trainarr admin |
| POST | `/api/rule-pack-impact/assess` | trainarr admin |

POST body: `{ rulePackKey, expectedVersionNumber?, expectedStatus? }`.

Response: assessment id, triggers (`version_drift`, `status_change`, `pack_inactive`, `pack_not_found`, `manual_assessment`), current Compliance Core state, drift summary, affected definitions/programs/assignments/qualifications, recommended actions, summary counts.

Audit: `rule_pack_impact.assess` on `rule_pack` target with trigger list as reason code when attention required.

### Drift baseline

When rule pack requirements are upserted with `validateWithComplianceCore=true`, TrainArr stores `KnownVersionNumber` and `KnownStatus` from Compliance Core lookup. Impact assessment compares these baselines (or optional request overrides) against the latest pack version returned by lookup.

### Ownership boundary

TrainArr orchestrates impact on training definitions, programs, assignments, and qualifications. Compliance Core owns rule pack content; no webhook/poll slice — TrainArr-initiated assessment API is sufficient for M6/M10.

## Frontend changes

- **RulePackImpactPanel** — admin enters rule pack key, runs assessment, shows triggers, drift, summary counts, recommended actions, affected assignments preview
- **HomePage** — panel visible for `trainarr_admin` / `tenant_admin`

## Tests

### Cross-product (`StaffArrTrainArrRulePackImpactTests`)

- `Rule_pack_impact_get_lists_affected_entities_with_version_drift`
- `Rule_pack_impact_post_assess_accepts_expected_version_override`
- `Rule_pack_impact_denies_member_role`
- `Rule_pack_impact_assessment_writes_audit_event`

### Frontend unit

- `RulePackImpactPanel.test.tsx`

## Verification commands

```powershell
dotnet build -c Release
dotnet test "tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~RulePackImpact"
cd apps/trainarr-frontend
npm install
npm run test -- --run
npm run build
```

## Remaining gaps

- No automatic background worker to poll Compliance Core for rule pack changes (M12 worker)
- No Compliance Core → TrainArr webhook notify path
- Impact assessment does not auto-create remediation assignments or re-run qualification checks
- Multiple pack versions in lookup: assessment uses highest version number only

## Next recommended slice

**Compliance Core admin batch evaluate UI** (M5/M10) or **TrainArr expiration scanning worker** (M12) per milestone priority.
