# NexArr Rollout Plan

| Field | Value |
| --- | --- |
| Product key | `nexarr` |
| Category | Platform/IAM |
| Entry release | R1 — Foundation spine |
| Completion release | R1 — Foundation spine |
| Expansion release | R12 — Expansion, portals, advanced integrations, AI, and category depth |
| Role | Identity, tenant membership, launch/session/service identity, platform admin, and account authority. |
| Roadmap slice | Foundation spine |
| Must not violate | Keep launch and authority truthful without recreating product entitlements for ordinary products. |
| Feature rows retained | 69 |
| Workflow rows retained | 14 |

## Release mapping

| Stage | Stage name | Features | Workflows |
| --- | --- | --- | --- |
| R1 | Foundation spine | 36 | 11 |
| R12 | Expansion, portals, advanced integrations, AI, and category depth | 33 | 3 |

## R0 Trust Gate pass

Status: Clear for R0.

Pass notes:

- Legacy tenant lifecycle settings remain readable for platform-admin audit review, but server writes now force license-based suspend/reactivate automation inert.
- Legacy launch-destination/license reconciliation remains reachable only as a compatibility/audit surface; enabled worker runs no longer detect, grant, revoke, suspend, or reactivate tenant/product availability from license or entitlement records.
- Product launch remains based on active tenant membership, product operational state, and the Compliance Core studio platform-admin gate rather than product entitlements or licenses.
- Platform-admin lifecycle UI copy and disabled controls now reflect the retired fixed-suite model.

Files touched:

- `apps/nexarr-api/NexArr.Api/Entities/TenantLifecycleEntities.cs`
- `apps/nexarr-api/NexArr.Api/Entities/TenantProductLicenseEntities.cs`
- `apps/nexarr-api/NexArr.Api/Services/CompatibilityLegacyEntitlementReconciliationSettingsService.cs`
- `apps/nexarr-api/NexArr.Api/Services/CompatibilityLegacyEntitlementReconciliationWorkerService.cs`
- `apps/nexarr-api/NexArr.Api/Services/TenantLifecycleRules.cs`
- `apps/nexarr-api/NexArr.Api/Services/TenantLifecycleSettingsService.cs`
- `apps/suite-frontend/src/components/platform-admin/TenantLifecycleSettingsPanel.tsx`
- `apps/suite-frontend/src/components/platform-admin/TenantLifecycleSettingsPanel.test.tsx`
- `tests/STLCompliance.NexArr.Auth.Tests/NexArrLaunchDestinationReconciliationTests.cs`
- `tests/STLCompliance.NexArr.Auth.Tests/NexArrPlatformWorkerHealthOrchestrationTests.cs`
- `tests/STLCompliance.NexArr.Auth.Tests/NexArrTenantLifecycleTests.cs`
- `tests/STLCompliance.NexArr.Auth.Tests/TenantLifecycleRulesTests.cs`

Tests run:

- `dotnet test tests/STLCompliance.NexArr.Auth.Tests/STLCompliance.NexArr.Auth.Tests.csproj --filter "TenantLifecycle|LaunchDestinationReconciliation|WorkerHealthOrchestration"` — passed, 39 tests.
- `npm test -- TenantLifecycleSettingsPanel.test.tsx` from `apps/suite-frontend` — passed, 2 tests.
- `dotnet test tests/STLCompliance.NexArr.Auth.Tests/STLCompliance.NexArr.Auth.Tests.csproj --logger "console;verbosity=minimal"` — passed, 421 tests in 10m 57s.

Remaining blockers:

- No known NexArr R0 production-truth blocker remains in the audited launch/session/tenant lifecycle slice.

## R1 Foundation spine pass

Status: Clear for R1 in the audited NexArr foundation slice.

Pass notes:

- Suite navigation now derives product availability from the fixed accessible product catalog instead of the legacy `launchableProductKeys` claim, so stale tokens cannot hide ordinary products in NexArr navigation surfaces.
- Generic reference-data imports now reject empty record sets with a clear `reference.import_empty` failure before creating ingestion jobs or staging records, removing the previous fake placeholder review row.
- Platform lifecycle overview copy now labels retired launch-destination reconciliation changes as compatibility review rather than grant/revoke product availability language.
- Field Companion scan/task helpers still use `FieldInboxRules.FieldProductKeys`; this is field-capable workflow routing, not product entitlement control.

Files touched:

- `apps/nexarr-api/NexArr.Api/Services/AuthService.cs`
- `apps/nexarr-api/NexArr.Api/Services/ReferenceDataService.cs`
- `apps/nexarr-api/NexArr.Api/Services/PlatformLifecycleOverviewService.cs`
- `tests/STLCompliance.NexArr.Auth.Tests/NexArrAuthApiTests.cs`
- `tests/STLCompliance.NexArr.Auth.Tests/NexArrPlatformAdminApiTests.cs`

Tests run:

- `dotnet test tests/STLCompliance.NexArr.Auth.Tests/STLCompliance.NexArr.Auth.Tests.csproj --filter "FullyQualifiedName~NexArrAuthApiTests" --logger "console;verbosity=minimal"` — passed, 29 tests.
- `dotnet test tests/STLCompliance.NexArr.Auth.Tests/STLCompliance.NexArr.Auth.Tests.csproj --filter "FullyQualifiedName~NexArrPlatformAdminApiTests" --logger "console;verbosity=minimal"` — passed, 42 tests.
- `dotnet test tests/STLCompliance.NexArr.Auth.Tests/STLCompliance.NexArr.Auth.Tests.csproj --filter "FullyQualifiedName~NexArrPlatformLifecycleOverviewTests|FullyQualifiedName~NexArrLaunchApiTests|FullyQualifiedName~NexArrServiceTokenTrustTests|FullyQualifiedName~NexArrTenantIntegrationTests|FullyQualifiedName~AiSmartImportGuardrailTests" --logger "console;verbosity=minimal"` — passed, 73 tests.
- Current repo-state rerun: `dotnet test tests/STLCompliance.NexArr.Auth.Tests/STLCompliance.NexArr.Auth.Tests.csproj --no-build --filter "FullyQualifiedName~NexArrAuthApiTests" --logger "console;verbosity=minimal"` — passed 33 tests in 1m 23s.
- Current repo-state rerun: `dotnet test tests/STLCompliance.NexArr.Auth.Tests/STLCompliance.NexArr.Auth.Tests.csproj --no-build --filter "FullyQualifiedName~NexArrPlatformAdminApiTests" --logger "console;verbosity=minimal"` — passed 42 tests in 1m 37s.
- Current repo-state rerun: `dotnet test tests/STLCompliance.NexArr.Auth.Tests/STLCompliance.NexArr.Auth.Tests.csproj --no-build --filter "FullyQualifiedName~NexArrPlatformLifecycleOverviewTests|FullyQualifiedName~NexArrLaunchApiTests|FullyQualifiedName~NexArrServiceTokenTrustTests|FullyQualifiedName~NexArrTenantIntegrationTests|FullyQualifiedName~AiSmartImportGuardrailTests" --logger "console;verbosity=minimal"` — passed 74 tests in 1m 25s.

Remaining blockers:

- No known NexArr R1 foundation-spine blocker remains in the audited identity, launch/navigation, service-token, tenant-integration, Smart Import, platform lifecycle, and reference-data ingestion slice.
- R12 target capabilities remain retained scope and were not expanded during this pass.

## R12 Expansion pass

Status: Clear for NexArr R12 with retained advanced IAM backlog explicitly deferred. The R12 release rule says expansion features are pulled forward only when source owners, gates, and cross-product contracts are ready; this pass hardened the represented identity-protocol integration surface and did not claim production SSO/SCIM readiness.

R12 scope audited:

- NexArr has 33 R12 feature rows and 3 R12 workflow rows: access certification, temporary privileged access/break-glass, identity incident response, permission simulation, external collaborator identity, cross-product access review, lifecycle automation, adaptive/threat detection, policy-as-code, credentialless workload identity, BYO IdP, SCIM/SSO depth, and shared foundation rows.
- The current slice already exposes tenant integration routes for OIDC callback, SAML metadata/ACS, and SCIM intake. These are useful R12 preparation points but are not complete production sign-in, provisioning, access review, or incident response workflows.

Completed in this pass:

- Converted generic OIDC callback, SAML metadata, SAML ACS, and SCIM provisioning routes from successful intake behavior to truthful `501 Not Implemented` problem responses that state no sign-in, provisioning, account, or tenant record was created or changed.
- Preserved existing OAuth callback, webhook, AS2, SFTP, CSV/XLSX, and other integration intake behavior that is already modeled as tenant integration intake rather than production identity protocol activation.
- Added regression coverage proving R12 identity protocol routes do not create tenant integration intake attempts or sync runs while the SSO/SCIM workflows remain target scope.

Files touched:

- `apps/nexarr-api/NexArr.Api/Endpoints/TenantIntegrationEndpoints.cs`
- `tests/STLCompliance.NexArr.Auth.Tests/NexArrTenantIntegrationTests.cs`
- `docs/roadmap/products/nexarr.md`

Tests run:

- `dotnet test tests/STLCompliance.NexArr.Auth.Tests/STLCompliance.NexArr.Auth.Tests.csproj --filter "FullyQualifiedName~NexArrTenantIntegrationTests" --logger "console;verbosity=minimal"` — passed 11 tests.
- `dotnet test tests/STLCompliance.NexArr.Auth.Tests/STLCompliance.NexArr.Auth.Tests.csproj --filter "FullyQualifiedName~NexArrAuthApiTests|FullyQualifiedName~NexArrLaunchApiTests|FullyQualifiedName~NexArrTenantIntegrationTests|FullyQualifiedName~NexArrPlatformIdentityIntegrationTests|FullyQualifiedName~NexArrPlatformIdentitySecurityTests" --logger "console;verbosity=minimal"` — passed 84 tests.
- Current repo-state rerun: `dotnet test tests/STLCompliance.NexArr.Auth.Tests/STLCompliance.NexArr.Auth.Tests.csproj --no-build --filter "FullyQualifiedName~NexArrTenantIntegrationTests|FullyQualifiedName~NexArrPlatformLifecycleOverviewTests|FullyQualifiedName~NexArrLaunchApiTests|FullyQualifiedName~NexArrServiceTokenTrustTests" --logger "console;verbosity=minimal"` — passed 52 tests in 1m 21s.

Remaining blockers:

- Advanced R12 IAM scope remains deferred until implementation-ready: access review/certification campaigns, temporary privileged access and break-glass use, identity incident response, permission simulator, external collaborator identity, adaptive authentication, identity threat detection, policy-as-code, BYO IdP production cutover, SCIM provisioning, lifecycle automation, credentialless workload identity, and managed-service delegation.
- Full `STLCompliance.NexArr.Auth.Tests` runtime now completes successfully in the current repo state; focused R12/R0/R1 identity and integration coverage remains available for faster targeted passes.

R12 product result: NexArr is clear for the R12 suite gate with the advanced target backlog explicitly deferred.

## Implementation interpretation

- Current/represented capabilities are hardened in R1 unless they are only supporting another release gate.
- Common category baseline remains retained for R1.
- Advanced, widely requested, or democratized capabilities remain retained for R12 unless pulled forward by a vertical slice.
- Do not implement this product by copying another product's source truth.
- Do not call this product complete until its release gates pass for data, authorization, tenant scope, UI, evidence, recovery, and reportability.

## Source docs

- [Feature catalog](../../products/nexarr/FEATURESET.md)
- [Workflow catalog](../../products/nexarr/WORKFLOWS.md)
- [Product manifest](../../products/nexarr/README_manifest.md)
- [Complete feature rollout CSV](../reference/feature-rollout-map.csv)
- [Complete workflow rollout CSV](../reference/workflow-rollout-map.csv)
