# R7A — Customer master baseline

Provide customer accounts, contacts, locations, requirements, contracts, preferences, and eligibility before orders or dispatch consume customer truth.

| Field | Definition |
| --- | --- |
| Entry condition | StaffArr/RecordArr/Compliance Core foundations and user-facing CRM surfaces are durable enough for trusted customer onboarding. |
| Exit condition | Customer requirements can be queried by OrdArr, RoutArr, SupplyArr, AssurArr, ReportArr, and external portal workflows. |
| Total feature rows mapped here | 35 |
| Total workflow rows mapped here | 14 |

## Product entry owners

| Product | Feature rows | Workflow rows | Role |
| --- | --- | --- | --- |
| CustomArr | 70 | 16 | Customer accounts, contacts, locations, requirements, contracts, preferences, onboarding, eligibility, and CRM/customer portal context. |

## Inventory mapped to this release

| Product | Features mapped here | Workflows mapped here |
| --- | --- | --- |
| CustomArr | 35 | 14 |

## Acceptance focus

- Pass all applicable R0 gates.
- Respect source-of-truth ownership.
- Prove the vertical slice rather than only rendering screens.
- Preserve evidence, source references, audit history, and reportability hooks.
- Keep UI unified, readable, non-noisy, and truthful in degraded states.

## Related roadmap files

- [../rollout-stages.md](../rollout-stages.md)
- [../release-gates-and-acceptance.md](../release-gates-and-acceptance.md)
- [../vertical-slice-backlog.md](../vertical-slice-backlog.md)
- [../reference/feature-rollout-map.csv](../reference/feature-rollout-map.csv)
- [../reference/workflow-rollout-map.csv](../reference/workflow-rollout-map.csv)

## Suite stage pass summary

Status: complete. R7A has one applicable product in the rollout maps: CustomArr.

Completed products:

- CustomArr — complete. The customer-master baseline was audited and hardened with DbContext-level tenant query filters for CustomArr tenant-owned records, plus focused verification for customer-reference and customer-requirement query safety.

Not-applicable products:

- NexArr, StaffArr, Compliance Core, RecordArr, MaintainArr, TrainArr, SupplyArr, LoadArr, AssurArr, OrdArr, RoutArr, ReportArr, Field Companion, and LedgArr have no R7A feature or workflow rows in the roadmap rollout maps.

Shared fixes:

- None required outside the CustomArr-owned customer master and test context surface.
- One existing cross-suite OrdArr/CustomArr handoff smoke test was updated to construct CustomArr under an authenticated tenant context so the new CustomArr tenant query filters are exercised correctly.

Tests run:

- `dotnet test tests/STLCompliance.CustomArr.Api.Tests/STLCompliance.CustomArr.Api.Tests.csproj --filter "FullyQualifiedName=STLCompliance.CustomArr.Api.Tests.CustomArrCrmWorkspaceServiceTests.DbContext_query_filter_scopes_customer_truth_to_authenticated_tenant" --logger "console;verbosity=minimal"` — passed 1 test.
- `dotnet test tests/STLCompliance.CustomArr.Api.Tests/STLCompliance.CustomArr.Api.Tests.csproj --logger "console;verbosity=minimal"` — passed 18 tests.
- `npm test` from `apps/customarr-frontend` — passed 2 files / 5 tests.
- `npm run test:theme` from `apps/customarr-frontend` — passed with no theme audit violations.
- `dotnet test tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj --filter "FullyQualifiedName=STLCompliance.MaintainArr.Auth.Tests.OrdArrCustomArrHandoffTests.CustomArr_portal_submission_hands_customer_reference_to_OrdArr_order" --logger "console;verbosity=minimal"` — passed 1 test.

Deferred blockers:

- No R7A blockers remain for CustomArr in the audited customer-master baseline.
- Proposal/agreement order handoff, complaint-to-quality loop, renewal/change workflow, field/offline CRM, customer data-room packages, communication-provider integration, e-sign, advanced portal self-service, AI CRM assistance, and deeper sales/service automation remain retained roadmap scope for later stages.

Stage result: R7A is suite-complete. The suite may advance to R7B.
