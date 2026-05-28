# Compliance Core staging seeds for journey k6 scenarios

## Slice name

M5/M13 Compliance Core load-test journey seed — idempotent `driver_qualification` rule pack, driver license fact source, dispatch workflow gates, operator scripts, staging soak pre-step

## Products touched

- **Compliance Core API** — `LoadTestJourneySeedService`, `POST /api/load-test-journey/seed`
- **STLCompliance.Shared** — `StlLoadTestJourneySeedCatalog`
- **Platform ops** — `scripts/ops/compliancecore-staging-journey-seed.*`
- **CI** — `load-staging-render.yml` journey seed step before soak
- **tests** — `ComplianceCoreLoadTestJourneySeedTests`, load catalog unit test

## API + auth changes

| Method | Route | Auth |
|--------|-------|------|
| POST | `/api/load-test-journey/seed` | JWT `RequireRulePacksCreate` + `RequireWorkflowGatesManage` (compliance_admin / tenant_admin) |

Idempotent seed ensures:

- Regulatory program chain (or reuses first active program)
- Active `driver_qualification` rule pack with boolean `driver_license_valid` rule content
- Static fact source returning `true`
- Dispatch workflow gates via existing `DispatchWorkflowGateSeedService`

## Operator workflow

```powershell
$env:RENDER_STAGING_NEXARR_API_URL = "https://nexarr-api-jdyi.onrender.com"
$env:RENDER_STAGING_COMPLIANCECORE_API_URL = "https://compliancecore-api-jdyi.onrender.com"
./scripts/ops/compliancecore-staging-journey-seed.ps1
```

Weekly **Load Staging Render** workflow runs this automatically before the k6 soak when staging secrets are configured.

## Tests

- `Load_test_journey_seed_is_idempotent_and_creates_rule_pack_and_dispatch_gates`
- `Load_test_journey_seed_denied_for_read_only_role`
- `Journey_seed_catalog_matches_load_test_defaults`

## Verification commands

```powershell
dotnet test tests/STLCompliance.ComplianceCore.Auth.Tests/STLCompliance.ComplianceCore.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~ComplianceCoreLoadTestJourneySeedTests"
dotnet test tests/STLCompliance.Load.Tests/STLCompliance.Load.Tests.csproj -c Release --filter "Journey_seed_catalog"
```

## Next recommended slice

StaffArr export preset persistence per tenant, or TrainArr staging qualification mirror seed for k6 journeys.
