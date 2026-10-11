# GCP-2026-10-11-01: temporary Render-only release exception

- Owner: project owner, tseders93@gmail.com.
- Approval: the owner explicitly replied "temp exception" in the Cloud Run recovery chat on October 10, 2026 (America/Chicago).
- Scope: this Cloud Run recovery rollout and its corrective retries only. No Render deployment is authorized by this exception.
- Expiration: October 12, 2026 at 00:25 UTC. The build rejects the exception at or after expiration.
- Tracking issue: [GCP-2026-10-11-01 follow-up](#tracking-issue).

## Exact exempted checks

1. `STLCompliance.E2E.Catalog.StlRenderStagingShipGateCatalogTests.Operator_runbook_and_validate_scripts_exist`
2. `STLCompliance.E2E.Catalog.StlRenderBlueprintCatalogTests.Render_yaml_declares_blueprint_inventory_health_checks_private_urls_and_sync_false_tokens`

Reason: the existing Render catalog references missing V1 documentation and absent validation scripts. The Cloud Run recovery uses a separate deployment manifest, provisioned SQL instance, secrets, network, and storage. Exempting these named Render checks does not establish Render readiness.

## Compensating controls

- Default builds retain both checks. The exception requires the exact explicit Cloud Build substitution `_RELEASE_EXCEPTION=GCP-2026-10-11-01`.
- Every other existing backend test in the non-live release gate remains enforced, including application, authorization, tenant-isolation and integration tests. Frontend builds/tests remain enforced for all 17 frontends.
- Cloud Run manifest/bootstrap/security tests and real SQL, subnet and secret-version preflight remain enforced.
- Failed application tests or image builds stop the rollout. Do not broaden the filter to evade another failure.
- Preserve the old SQL instance. Verify API readiness, frontend proxy responses and authenticated launch after deployment; report unverified workflows honestly.

## Tracking issue

Status: open. Restore or reconcile Render catalog references, the environment-variable documentation, and the staging validation scripts. Rerun both exact checks without an exception, remove the temporary exception implementation when resolved, and do not infer full production readiness from a recovery rollout.

## Recovery attempt

Build [`f4d743fd-f33f-4f33-9eaf-a8d25c6ef1fc`](https://console.cloud.google.com/cloud-build/builds/f4d743fd-f33f-4f33-9eaf-a8d25c6ef1fc?project=747917404962) applied this exception and passed Cloud preflight and the 19 deployment/bootstrap/exception unit tests in its uploaded source.

The application gate then reported eight health-test failures (OrdArr and AssurArr factories started outside `Testing` without a database) and four OpenAPI snapshot mismatches (NexArr, StaffArr, Compliance Core, and LoadArr). Those failures are not covered by this exception. The build was cancelled to stop unnecessary paid work before image build/push or deployment; existing Cloud Run revisions were not changed. The full backend suite did not finish, so additional failures may remain.

Next: repair the health-test environment configuration and review the API contract drift before updating snapshots. Rerun application gates without expanding this exception.

## Test repair review

The database-free health fixtures now explicitly select `Testing`. Separate production tests assert that OrdArr and AssurArr still reject startup without a database; production validation was not relaxed. The health project passes all 60 tests.

The four OpenAPI baselines were compared semantically with generated documents and checked against current endpoint implementations:

- NexArr: two existing local-development bypass routes and their request schema. The existing policy rejects production use; no bypass setting was enabled.
- StaffArr: the existing NexArr-to-StaffArr internal person-provisioning route and request schema. Its service-token and tenant validation remain in place.
- Compliance Core: 60 existing report/index/export routes and their report tags. Existing route methods were neither removed nor changed by this repair; the generated schema ordering also changed.
- LoadArr: two existing draft-completion routes and nullable `clientRequestId` fields in receiving/transfer creation schemas. The schemas now record the existing required-property contract (the value may be null); this repair does not change request handling. Existing retry/dependency tests cover the workflow.

This is a baseline synchronization, not a new API rollout or a blanket snapshot regeneration. Future snapshot mismatches also write generated comparison documents under ignored `artifacts/openapi/` without overwriting checked-in expectations.

## Bounded test execution

Retry `02bca767-8cec-4efd-a9f9-8779a8ee3aac` passed health (60), OpenAPI (172), and E2E (136), but two in-memory API requests in MaintainArr and SupplyArr exceeded the existing 100-second HTTP timeout while solution projects and their test collections ran concurrently. It was cancelled before deployment.

The affected MaintainArr audit-package class (7 tests) and SupplyArr readiness-dashboard class (5 tests) pass locally with bounded concurrency. Serial retry `d1448f5b-99b3-4c9a-a31b-7c13057230f9` also passed all 256 Compliance Core tests, but its measured throughput would exceed the original two-hour build window. The full local MaintainArr suite subsequently passed 342 tests. Cloud validation now caps MSBuild at three nodes and xUnit at four collections per assembly using `tools/gcp/backend.runsettings`, with a four-hour overall build deadline and four visible .NET processors per test process. No assertions, HTTP timeouts, hang detection, or test exclusions were relaxed. The full bounded run is still required before deployment.

Retry `004803f7-b8a3-498f-bc0a-d9466dd228bc` passed 430 NexArr authentication tests, then exposed an incorrect SupplyArr serial/lot fixture: its positional `CreatePartRequest` boolean populated `IsTrackable` rather than `RequiresSerialLotTracking`. The fixture now uses the named argument and asserts that the created part requires tracking. Existing receiving rejection/acceptance assertions remain unchanged; no product ownership or production validation changed. The focused regression passes locally.
