# Product-owner SLO adoption for M13 load tests

## Slice name

M13 product-owner SLO adoption — official V1 load SLO document, product-owner profile in `StlLoadTestSloCatalog`, cross-product journey k6 scenarios (TrainArr qualification check, RoutArr dispatch workflow gate), operator script updates, Load.Tests unit coverage

## Products touched

- **STLCompliance.Shared** — `StlLoadTestSloCatalog` product-owner profile, `StlLoadTestJourneyDefaults`
- **tests/load-k6** — `slo-product-owner.json`, journey helpers, two new scenarios, PO thresholds on all seven scenarios
- **scripts/ops** — `load-test-run.ps1`, `load-test-run.sh` (seven scenarios)
- **docs/operations** — `PRODUCT_OWNER_LOAD_SLO_V1.md`
- **tests/STLCompliance.Load.Tests** — catalog profile + journey default tests

## Shared additions

| File | Purpose |
|------|---------|
| `StlLoadTestSloCatalog.cs` | `ProductOwnerTargets`, `ActiveProfile`, journey scenario keys |
| `StlLoadTestJourneyDefaults.cs` | Demo subject person + qualification/rule-pack keys for k6 |

## k6 scenarios

| Key | Script | Flow |
|-----|--------|------|
| `trainarr-qualification-check` | `scenarios/trainarr-qualification-check.js` | Handoff TrainArr → `POST /api/qualification-checks` |
| `routarr-dispatch-workflow-gate` | `scenarios/routarr-dispatch-workflow-gate.js` | Handoff RoutArr → create trip → `POST /api/dispatch-workflow-gates/check` |

Shared journey helpers: `tests/load-k6/lib/stl-journey.js`.

## SLO profile

| Profile | Env | Source |
|---------|-----|--------|
| `product-owner` (default) | `STL_LOAD_SLO_PROFILE=product-owner` or unset | `StlLoadTestSloCatalog.ProductOwnerTargets`, `slo-product-owner.json` |
| `engineering-defaults` | `STL_LOAD_SLO_PROFILE=engineering-defaults` | `StlLoadTestSloCatalog.EngineeringDefaults`, `slo-defaults.json` |

## Environment variables

| Variable | Default | Purpose |
|----------|---------|---------|
| `STL_LOAD_SLO_PROFILE` | `product-owner` | Active SLO profile |
| `STL_LOAD_SUBJECT_PERSON_ID` | demo admin GUID | Journey subject person |
| `STL_LOAD_QUALIFICATION_KEY` | `hazmat_endorsement` | TrainArr qualification check |
| `STL_LOAD_RULE_PACK_KEY` | `driver_qualification` | Compliance Core rule pack |

## Tests

### Backend unit (`STLCompliance.Load.Tests`)

- `EngineeringDefaults_includes_seven_scenarios`
- `ProductOwnerTargets_includes_seven_scenarios`
- `GetByScenarioKey_returns_product_owner_ready_target_by_default`
- `GetByScenarioKey_returns_engineering_target_when_profile_set`
- `GetByScenarioKey_returns_journey_scenario`
- `Journey_defaults_match_demo_platform_seeder`
- Existing evaluator tests (unchanged behavior against active profile)

## Verification commands

```powershell
dotnet build STLCompliance.slnx -c Release
dotnet test tests/STLCompliance.Load.Tests/STLCompliance.Load.Tests.csproj -c Release --filter "Category=Load&Category!=Live"
./scripts/ops/load-test-run.ps1 -Scenario trainarr-qualification-check -Vus 2 -Duration 10s
./scripts/ops/load-test-run.ps1 -Scenario routarr-dispatch-workflow-gate -Vus 2 -Duration 10s
```

## Remaining gaps

- Compliance Core rule packs / dispatch gates not auto-seeded in all environments — journey scenarios may return warn/block outcomes but should HTTP 200
- Render production soak runs against PO SLOs not yet scheduled in CI (nightly still runs subset of live probes)
- Additional journeys (MaintainArr asset readiness, SupplyArr reorder evaluation) deferred

## Next recommended slice

StaffArr bulk person onboarding import, or extend nightly live k6 to cover all seven PO scenarios.
