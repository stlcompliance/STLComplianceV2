# CustomArr Rollout Plan

| Field | Value |
| --- | --- |
| Product key | `customarr` |
| Category | CRM |
| Entry release | R7A — Customer master baseline |
| Completion release | R7A — Customer master baseline |
| Expansion release | R12 — Expansion, portals, advanced integrations, AI, and category depth |
| Role | Customer accounts, contacts, locations, requirements, contracts, preferences, onboarding, eligibility, and CRM/customer portal context. |
| Roadmap slice | Customer master before order orchestration |
| Must not violate | Be the customer source of truth before OrdArr, RoutArr, or SupplyArr consumes customer requirements. |
| Feature rows retained | 70 |
| Workflow rows retained | 16 |

## Release mapping

| Stage | Stage name | Features | Workflows |
| --- | --- | --- | --- |
| R7A | Customer master baseline | 35 | 14 |
| R12 | Expansion, portals, advanced integrations, AI, and category depth | 35 | 2 |

## Implementation interpretation

- Current/represented capabilities are hardened in R7A unless they are only supporting another release gate.
- Common category baseline remains retained for R7A.
- Advanced, widely requested, or democratized capabilities remain retained for R12 unless pulled forward by a vertical slice.
- Do not implement this product by copying another product's source truth.
- Do not call this product complete until its release gates pass for data, authorization, tenant scope, UI, evidence, recovery, and reportability.

## Source docs

- [Feature catalog](../../products/customarr/FEATURESET.md)
- [Workflow catalog](../../products/customarr/WORKFLOWS.md)
- [Product manifest](../../products/customarr/README_manifest.md)
- [Complete feature rollout CSV](../reference/feature-rollout-map.csv)
- [Complete workflow rollout CSV](../reference/workflow-rollout-map.csv)

## R0 Trust Gate pass

Status: complete for the current CustomArr auth/session and persistence-configuration pass.

Files changed:

- `apps/customarr-api/CustomArr.Api/CustomArrServiceRegistration.cs`
- `apps/customarr-api/CustomArr.Api/Data/CustomArrStore.cs`
- `apps/customarr-api/CustomArr.Api/Endpoints/AuthEndpoints.cs`
- `apps/customarr-api/CustomArr.Api/Services/CustomArrSuiteLaunchCatalog.cs`
- `apps/customarr-api/CustomArr.Api/Services/HandoffAuthService.cs`
- `apps/customarr-frontend/src/App.test.tsx`
- `apps/customarr-frontend/src/api/client.ts`
- `tests/STLCompliance.CustomArr.Api.Tests/CustomArrAuthEndpointsTests.cs`

Completed R0 fixes:

- CustomArr no longer falls back to EF InMemory outside Testing. Missing `DATABASE_URL` or `ConnectionStrings:Database` now fails startup instead of creating production in-memory CRM truth.
- Removed the standalone no-argument `CustomArrStore` path that created a private in-memory database.
- Handoff redemption and session bootstrap now return a fixed ordinary-suite launch catalog rather than trusting NexArr-returned or claim-carried launchable product keys.
- Removed the handoff-time product availability gate so active tenant context plus CustomArr target-product match controls launch, while record/action permissions stay server-side.
- Session bootstrap no longer emits the retired `hasCustomArrAccess` product-access flag.
- The frontend session type no longer carries `hasCustomArrAccess`; legacy payloads are tolerated and stripped during normalization.
- Compliance Core remains excluded from ordinary tenant launch availability.

Tests run:

- `dotnet test tests/STLCompliance.CustomArr.Api.Tests/STLCompliance.CustomArr.Api.Tests.csproj --filter "FullyQualifiedName~CustomArrAuthEndpointsTests|FullyQualifiedName~CustomArrCrmWorkspaceServiceTests|FullyQualifiedName~CustomArrTenantSettingsServiceTests" --logger "console;verbosity=minimal"` — passed 17 tests.
- `npm test -- App.test.tsx sessionStorage.test.ts` from `apps/customarr-frontend` — passed 2 files / 5 tests.

Remaining blockers:

- No R0 blockers remain in the current CustomArr auth/session and persistence-configuration slice. Broader R7A customer-master workflow depth remains governed by the retained feature and workflow catalogs and must not be pulled into R0.

## R1 Foundation spine pass

Status: Not applicable. CustomArr has no R1 feature or workflow rows in the roadmap rollout maps, and its entry release remains R7A.

R1 scope audited:

- `docs/roadmap/reference/feature-rollout-map.csv` contains no CustomArr rows for `R1`.
- `docs/roadmap/reference/workflow-rollout-map.csv` contains no CustomArr rows for `R1`.
- CustomArr's product FEATURESET and WORKFLOWS remain retained full scope, but they do not authorize starting the R7A customer master baseline during the R1 suite stage.

Files touched:

- `docs/roadmap/products/customarr.md`

Tests run:

- Not run. This was a roadmap applicability/documentation pass only; no CustomArr code, UI, API, data-flow, or test files changed.

Remaining blockers: None for R1. CustomArr must wait for the suite to reach R7A before customer-master work begins.

R1 stage result: CustomArr is clear for the R1 suite gate as not applicable.

## R7A Customer master baseline pass

Status: complete for the current CustomArr customer-master baseline.

Files changed:

- `apps/customarr-api/CustomArr.Api/Data/CustomArrDbContext.cs`
- `tests/STLCompliance.CustomArr.Api.Tests/CustomArrCrmWorkspaceServiceTests.cs`
- `tests/STLCompliance.CustomArr.Api.Tests/CustomArrTenantSettingsServiceTests.cs`
- `tests/STLCompliance.MaintainArr.Auth.Tests/OrdArrCustomArrHandoffTests.cs`
- `docs/roadmap/products/customarr.md`
- `docs/roadmap/releases/r7a-customer-master-baseline.md`

Completed R7A fixes and verification:

- Added authenticated tenant query filters to all CustomArr tenant-owned customer, CRM workflow, settings, import/dedupe, portal, integration-reference, idempotency, and audit entities.
- Preserved CustomArr as the customer source of truth while keeping OrdArr, RoutArr, SupplyArr, AssurArr, ReportArr, and portal consumers on reference/eligibility/requirement queries rather than copied customer truth.
- Verified the customer-reference, customer-location, customer-contact, customer-requirement, agreement, and case reference search path remains live and tenant-scoped.
- Kept ordinary tenant users able to launch CustomArr and use server-side action permissions; no product entitlement/grant language was reintroduced.
- Updated direct CustomArr test DbContexts, including the cross-suite OrdArr handoff smoke, to run under an authenticated tenant context so the new query filters are exercised.

Tests run:

- `dotnet test tests/STLCompliance.CustomArr.Api.Tests/STLCompliance.CustomArr.Api.Tests.csproj --filter "FullyQualifiedName=STLCompliance.CustomArr.Api.Tests.CustomArrCrmWorkspaceServiceTests.DbContext_query_filter_scopes_customer_truth_to_authenticated_tenant" --logger "console;verbosity=minimal"` — passed 1 test.
- `dotnet test tests/STLCompliance.CustomArr.Api.Tests/STLCompliance.CustomArr.Api.Tests.csproj --logger "console;verbosity=minimal"` — passed 18 tests.
- `npm test` from `apps/customarr-frontend` — passed 2 files / 5 tests.
- `npm run test:theme` from `apps/customarr-frontend` — passed with no theme audit violations.
- `dotnet test tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj --filter "FullyQualifiedName=STLCompliance.MaintainArr.Auth.Tests.OrdArrCustomArrHandoffTests.CustomArr_portal_submission_hands_customer_reference_to_OrdArr_order" --logger "console;verbosity=minimal"` — passed 1 test.

Known warnings:

- The .NET test runs still emit existing NU1510 health-check package prune warnings and Microsoft.EntityFrameworkCore.Relational 10.0.4/10.0.8 conflict warnings.
- The MaintainArr smoke test project still emits an existing nullable warning in `MaintainArrWorkOrderTests.cs`.

Remaining blockers and deferrals:

- No R7A blockers remain for the customer master baseline in the audited slice.
- CU-WF-005 proposal/agreement order handoff, CU-WF-010 complaint-to-quality loop, and CU-WF-012 renewal/change workflow remain partial and are not expanded in R7A.
- CU-WF-015 field visit/offline account update and CU-WF-016 customer audit/data-room package remain R12 targets.
- Communication-provider integration, e-sign, advanced portal self-service, AI-assisted CRM, forecasting depth, and broader sales/service automation remain retained full scope for later stages.

R7A stage result: CustomArr is clear for the R7A suite gate.

## R12 Expansion, portals, advanced integrations, AI, and category depth pass

Status: Clear for the current CustomArr R12 trust pass, with offline field CRM, customer data-room packages, advanced portal depth, communication integrations, AI, and journey/revenue intelligence retained and deferred behind owner-backed contracts.

R12 scope audited:

- `docs/roadmap/reference/feature-rollout-map.csv` CustomArr rows for `R12`
- `docs/roadmap/reference/workflow-rollout-map.csv` CustomArr rows for `R12`
- `docs/products/customarr/FEATURESET.md`
- `docs/products/customarr/WORKFLOWS.md`
- Current CustomArr customer detail, commercial handoff, CRM module, portal/settings, integration-reference, and tenant-settings UI slices

Completed R12 fixes:

- CustomArr CRM record and activity surfaces no longer render raw source product keys such as `recordarr` or `customarr` to ordinary users. They now display product names while preserving useful source-ownership context.
- CRM record freshness now uses the same human-readable key formatting as other CustomArr statuses.
- Frontend tests now prove commercial records show the RecordArr display label and do not regress to raw source-product-key copy.

Deferred R12 blockers:

- `CU-WF-015` field visit and offline account update remains deferred until Field Companion can issue scoped offline packages, revalidate customer versions/permissions on sync, resolve conflicts explicitly, and purge local data after expiry.
- `CU-WF-016` customer audit/data-room package remains deferred until CustomArr can coordinate scoped package requests through RecordArr/NexArr with manifest, redaction, expiring access, acknowledgement, supplement, and revocation evidence.
- Unified communications inbox, conversation intelligence, email/SMS/telephony/provider sync, e-sign, partner/channel relationship management, CPQ contribution, customer journey orchestration, and advanced forecasting remain retained R12 scope but are not started in this pass.
- AI-assisted account research, revenue intelligence, risk/profitability views, customer data platform-lite identity resolution, and recommendation/proposal workflows remain deferred; future AI must stay cited, permissioned, reviewable, and non-committal.
- OrdArr order lifecycle, LedgArr finance, RecordArr documents/data rooms, Compliance Core eligibility/applicability, ReportArr projections, Field Companion offline capture, and external provider contracts remain source-owned dependencies rather than CustomArr-owned truth.

Files touched:

- `apps/customarr-frontend/src/App.tsx`
- `apps/customarr-frontend/src/App.test.tsx`
- `docs/roadmap/products/customarr.md`

Tests run:

- `rg -n "\\{record\\.sourceProductKey\\}|\\{item\\.sourceProductKey|record\\.freshness}</span>|>recordarr<|>customarr<" apps/customarr-frontend/src/App.tsx apps/customarr-frontend/src/App.test.tsx -S` - no matches.
- `npm test -- App.test.tsx sessionStorage.test.ts` from `apps/customarr-frontend` - passed 2 files / 5 tests.
- `npm run test:theme` from `apps/customarr-frontend` - passed with no violations.
- `dotnet test tests/STLCompliance.CustomArr.Api.Tests/STLCompliance.CustomArr.Api.Tests.csproj --logger "console;verbosity=minimal"` - passed 18 tests.

R12 stage result: CustomArr is clear for the R12 suite gate with the deferred blockers above documented. The next R12 product pass is OrdArr.
