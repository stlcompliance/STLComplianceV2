# Worker 144 — M13 Playwright/E2E expansion

## Scope

Extend `tests/e2e-playwright` with operator and multi-product journey smokes (M13):

| Spec | Journey |
|------|---------|
| `compliancecore-operator-rule-evaluate-smoke.spec.ts` | API journey seed → suite handoff → Rule evaluation tab → seed content → evaluate → pass |
| `suite-multi-product-handoff-journey.spec.ts` | One session: StaffArr → TrainArr → Compliance Core handoffs via suite launch surfaces |

Also:

- `handoffJourney.ts` helpers (`launchProductHandoffFromSuite`, `returnToSuiteApp`)
- `e2eApi.seedComplianceCoreJourney` + StaffArr/Compliance Core API redeem paths
- Compliance Core UI test ids on rule evaluation panel + tab
- `StlE2ePlaywrightSpecCatalog.OperatorJourneySmokeSpecs` + E2E catalog tests
- README updates

## Out of scope

- Re-running W134/W138/W140 deep-link or platform-admin specs
- Companion or operations deep-link additions

## Verification

```bash
./scripts/ops/e2e-stack-up.ps1
./scripts/ops/e2e-frontends-preview.ps1
cd tests/e2e-playwright
npm ci && npx playwright install chromium
$env:E2E_LIVE = "1"
npx playwright test tests/compliancecore-operator-rule-evaluate-smoke.spec.ts
npx playwright test tests/suite-multi-product-handoff-journey.spec.ts

dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "Category=E2e"
```
