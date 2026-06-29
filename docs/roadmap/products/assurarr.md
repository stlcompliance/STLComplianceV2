# AssurArr Rollout Plan

| Field | Value |
| --- | --- |
| Product key | `assurarr` |
| Category | QMS |
| Entry release | R6 — Quality hold, release, and corrective action |
| Completion release | R6 — Quality hold, release, and corrective action |
| Expansion release | R12 — Expansion, portals, advanced integrations, AI, and category depth |
| Role | Nonconformance, quality holds, releases, CAPA, audits, findings, complaints, supplier quality, and quality status. |
| Roadmap slice | Quality hold and corrective action loop |
| Must not violate | Block and release via permissioned, evidenced quality decisions rather than shadow-owning affected records. |
| Feature rows retained | 68 |
| Workflow rows retained | 14 |

## Release mapping

| Stage | Stage name | Features | Workflows |
| --- | --- | --- | --- |
| R6 | Quality hold, release, and corrective action | 33 | 14 |
| R12 | Expansion, portals, advanced integrations, AI, and category depth | 35 | 0 |

## Implementation interpretation

- Current/represented capabilities are hardened in R6 unless they are only supporting another release gate.
- Common category baseline remains retained for R6.
- Advanced, widely requested, or democratized capabilities remain retained for R12 unless pulled forward by a vertical slice.
- Do not implement this product by copying another product's source truth.
- Do not call this product complete until its release gates pass for data, authorization, tenant scope, UI, evidence, recovery, and reportability.

## Source docs

- [Feature catalog](../../products/assurarr/FEATURESET.md)
- [Workflow catalog](../../products/assurarr/WORKFLOWS.md)
- [Product manifest](../../products/assurarr/README_manifest.md)
- [Complete feature rollout CSV](../reference/feature-rollout-map.csv)
- [Complete workflow rollout CSV](../reference/workflow-rollout-map.csv)

## R0 Trust Gate pass

Status: complete for the current AssurArr auth/session and persistence-configuration pass.

Files changed:

- `apps/assurarr-api/AssurArr.Api/AssurArrServiceRegistration.cs`
- `apps/assurarr-api/AssurArr.Api/Endpoints/AuthEndpoints.cs`
- `apps/assurarr-api/AssurArr.Api/Services/AssurArrSuiteLaunchCatalog.cs`
- `apps/assurarr-api/AssurArr.Api/Services/HandoffAuthService.cs`
- `apps/assurarr-frontend/src/App.test.tsx`
- `apps/assurarr-frontend/src/api/client.ts`
- `apps/assurarr-frontend/src/api/client.test.ts`
- `tests/STLCompliance.AssurArr.Api.Tests/AssurArrAuthEndpointsTests.cs`

Completed R0 fixes:

- AssurArr no longer falls back to EF InMemory outside the Testing environment. Missing `DATABASE_URL` or `ConnectionStrings:Database` now fails startup instead of creating production in-memory quality truth.
- Handoff redemption and session bootstrap now return a fixed ordinary-suite launch catalog rather than trusting NexArr-returned or claim-carried launchable product keys.
- Removed the handoff-time product availability gate so active tenant context plus AssurArr target-product match controls launch, while record/action permissions stay server-side.
- Session bootstrap no longer emits the retired `hasAssurArrAccess` product-access flag.
- The frontend now tolerates legacy `hasAssurArrAccess` input while stripping it from normalized session data.
- Compliance Core remains excluded from ordinary tenant launch availability.

Tests run:

- `dotnet test tests/STLCompliance.AssurArr.Api.Tests/STLCompliance.AssurArr.Api.Tests.csproj --filter "FullyQualifiedName~AssurArrAuthEndpointsTests|FullyQualifiedName~AssurArrAuthorizationTests" --logger "console;verbosity=minimal"` — passed 13 tests.
- `npm test -- client.test.ts App.test.tsx sessionStorage.test.ts` from `apps/assurarr-frontend` — passed 3 files / 7 tests.
- Current repo-state rerun: `dotnet test tests/STLCompliance.AssurArr.Api.Tests/STLCompliance.AssurArr.Api.Tests.csproj --no-build --filter "FullyQualifiedName~AssurArrAuthEndpointsTests|FullyQualifiedName~AssurArrAuthorizationTests" --logger "console;verbosity=minimal"` — passed 13 tests in 14s.
- Current repo-state rerun: `npm test -- --run client.test.ts App.test.tsx sessionStorage.test.ts` from `apps/assurarr-frontend` — passed 3 files / 8 tests in 4.71s.

Remaining blockers:

- No R0 blockers remain in the current AssurArr auth/session and persistence-configuration slice. Broader R6 quality workflow depth remains governed by the retained feature and workflow catalogs and must not be pulled into R0.

## R1 Foundation spine pass

Status: Not applicable. AssurArr has no R1 feature or workflow rows in the roadmap rollout maps, and its entry release remains R6.

R1 scope audited:

- `docs/roadmap/reference/feature-rollout-map.csv` contains no AssurArr rows for `R1`.
- `docs/roadmap/reference/workflow-rollout-map.csv` contains no AssurArr rows for `R1`.
- AssurArr's product FEATURESET and WORKFLOWS remain retained full scope, but they do not authorize starting the R6 quality hold, release, and corrective action loop during the R1 suite stage.

Files touched:

- `docs/roadmap/products/assurarr.md`

Tests run:

- Not run. This was a roadmap applicability/documentation pass only; no AssurArr code, UI, API, data-flow, or test files changed.

Remaining blockers: None for R1. AssurArr must wait for the suite to reach R6 before QMS workflow hardening begins.

R1 stage result: AssurArr is clear for the R1 suite gate as not applicable.

## R6 Quality hold, release, and corrective action pass

Status: Clear for the AssurArr-owned durable R6 quality decision loop, with deferred blockers for target/partial QMS depth that is retained but not required for this stage gate.

R6 scope audited:

- `docs/roadmap/releases/r6-quality-hold-release-and-corrective-action.md`
- `docs/roadmap/reference/feature-rollout-map.csv` AssurArr rows for `R6`
- `docs/roadmap/reference/workflow-rollout-map.csv` AssurArr rows for `R6`
- `docs/products/assurarr/FEATURESET.md`
- `docs/products/assurarr/WORKFLOWS.md`

Completed R6 fixes:

- AssurArr-owned quality records now apply a DbContext-level tenant query filter across nonconformances, holds, CAPA, actions, blockers, verification, audits, findings, reviews, releases, containment, disposition, supplier quality, complaints, metrics, risk profiles, status snapshots, and timeline events.
- The AssurArr API test harness now seeds through the same Testing-configured factory used by requests, preserving the R0 production database startup guard.
- Added regression coverage proving a quality hold created in one tenant is not visible in another tenant's hold list or detail route.

Verified R6 behavior:

- Nonconformance creation, status transitions, quality-status snapshots, hold creation/linkage, CAPA/root-cause/action/verification/effectiveness, audit/findings, supplier quality/SCAR, customer complaint quality cases, dashboard counts, and authorization tests remain passing.
- Quality decisions remain AssurArr-owned while affected objects stay referenced by product-qualified refs such as LoadArr inventory or SupplyArr supplier/order refs; no cross-product database ownership was introduced.

Deferred R6 blockers:

- `AS-WF-010` quality change control remains target scope.
- `AS-WF-011` deviation and temporary concession remains target scope.
- `AS-WF-012` risk/FMEA review and control action remains target scope.
- `AS-WF-013` management quality review remains partial.
- `AS-WF-014` quality audit/evidence package remains partial, especially where RecordArr durable evidence package retention is required.
- Rich downstream acknowledgement/unblock confirmations from affected products remain future depth unless each affected product exposes the corresponding owner-backed contract.

Files touched:

- `apps/assurarr-api/AssurArr.Api/Data/AssurArrDbContext.cs`
- `tests/STLCompliance.AssurArr.Api.Tests/AssurArrApiTests.cs`
- `docs/roadmap/products/assurarr.md`
- `docs/roadmap/releases/r6-quality-hold-release-and-corrective-action.md`

Tests run:

- `dotnet test tests/STLCompliance.AssurArr.Api.Tests/STLCompliance.AssurArr.Api.Tests.csproj --filter "FullyQualifiedName=STLCompliance.AssurArr.Api.Tests.AssurArrApiTests.Quality_hold_reads_are_tenant_scoped" --logger "console;verbosity=minimal"` - passed 1 test.
- `dotnet test tests/STLCompliance.AssurArr.Api.Tests/STLCompliance.AssurArr.Api.Tests.csproj --logger "console;verbosity=minimal"` - passed 34 tests.
- `npm test` from `apps/assurarr-frontend` - passed 3 files / 7 tests.

R6 stage result: AssurArr is clear for the R6 suite gate with the deferred blockers above documented.

## R12 Expansion, portals, advanced integrations, AI, and category depth pass

Status: Clear for the current AssurArr R12 trust pass, with advanced QMS depth retained and deferred behind owner-backed collaboration, evidence, offline, AI, and analytics contracts.

R12 scope audited:

- `docs/roadmap/reference/feature-rollout-map.csv` AssurArr rows for `R12`
- `docs/roadmap/reference/workflow-rollout-map.csv` AssurArr rows for `R12`
- `docs/products/assurarr/FEATURESET.md`
- `docs/products/assurarr/WORKFLOWS.md`
- Current AssurArr dashboard/history, settings/reference, supplier quality, customer complaint, quality review, and evidence-reference UI slices

Completed R12 fixes:

- The AssurArr settings reference page no longer exposes raw integration endpoint paths, local preview ports, API ports, or frontend routing details to ordinary product users. It now presents capability-level dependency context while preserving ownership boundaries.
- Dashboard and reusable event sections no longer fall back to raw event subject identifiers when a linked record label is unavailable. They now show a human-readable unresolved-reference label instead of exposing opaque internal IDs.
- Frontend regression tests now prove the settings page stays free of raw endpoint/runtime details and event fallbacks do not display unresolved subject IDs.

Deferred R12 blockers:

- Supplier/customer collaboration portals, external reviewer access, and multi-party quality network behavior remain deferred until scoped portal permissions, access expiry, redaction, and owner-backed supplier/customer contracts exist.
- Frontline mobile quality capture, offline controlled procedures/forms, and Field Companion execution remain deferred until server-declared offline-safe actions, RecordArr evidence sync, and owner revalidation exist.
- AI-assisted triage, root-cause assistance, computer-vision defect assistance, predictive quality risk, and evidence completeness coaching remain deferred; any future AI output must remain reviewable, cited, permissioned, and non-committal.
- Advanced FMEA, SPC, electronic batch/device/history record review, validation/qualification management, regulated e-signature controls, digital quality passports, and no-code quality workflow configuration remain retained R12 scope but are not started in this pass.
- RecordArr durable evidence packages, Compliance Core applicability/rules, ReportArr projections, TrainArr training impact, and downstream product acknowledgement/unblock confirmations remain suite dependencies rather than AssurArr-owned source truth.

Files touched:

- `apps/assurarr-frontend/src/App.tsx`
- `apps/assurarr-frontend/src/App.test.tsx`
- `docs/roadmap/products/assurarr.md`

Tests run:

- `rg -n "GET /|POST /|Local preview port|API port|Frontend base URL|event\\.subjectType} \\{event\\.subjectId|subjectType} \\{event\\.subjectId" apps/assurarr-frontend/src/App.tsx -S` - no matches.
- `npm test -- App.test.tsx api/client.test.ts sessionStorage.test.ts` from `apps/assurarr-frontend` - passed 3 files / 8 tests.
- `npm run test:theme` from `apps/assurarr-frontend` - passed with no violations.
- `dotnet test tests/STLCompliance.AssurArr.Api.Tests/STLCompliance.AssurArr.Api.Tests.csproj --logger "console;verbosity=minimal"` - passed 34 tests.

R12 stage result: AssurArr is clear for the R12 suite gate with the deferred blockers above documented. The next R12 product pass is CustomArr.
