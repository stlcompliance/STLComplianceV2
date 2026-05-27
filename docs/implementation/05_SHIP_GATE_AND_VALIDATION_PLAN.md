# Ship Gate and Validation Plan

## Full Suite Ship Gate

The suite is ready when every product passes the same proof standard:

- API deploys on Render.
- Worker deploys on Render.
- PostgreSQL database exists and migrations run cleanly.
- Static frontends deploy on Render.
- `/health` returns expected status.
- Authentication uses NexArr.
- Tenant context is validated server-side.
- Product entitlement is validated server-side.
- Product permission is enforced server-side.
- OpenAPI exists and matches implemented routes.
- Database records are tenant-scoped where applicable.
- Event outbox/inbox exists where cross-product facts are emitted or consumed.
- Audit logging exists for sensitive actions.
- Import/export/report claims have working evidence.
- Frontend uses real APIs for shipped screens.
- No hidden localStorage/admin toggle can unlock protected UI.
- No product owns another product's source-of-truth records.
- No cross-product database foreign keys exist.
- Build, lint, typecheck, unit tests, integration tests, and E2E journeys pass.

## Required End-to-End Test Journeys

1. Tenant onboarding and entitlement in NexArr.
2. Product launch from NexArr into every product.
3. StaffArr person creation, org assignment, permission assignment, readiness calculation.
4. TrainArr program creation, assignment, evidence, signoff, qualification, StaffArr publication.
5. MaintainArr asset creation, inspection failure, defect, work order, repair, readiness restoration.
6. SupplyArr vendor onboarding, document expiration, part catalog, purchase request, approval, PO, receiving.
7. RoutArr route creation, dispatch assignment, eligibility/readiness checks, trip execution, DVIR, proof, closeout.
8. Compliance Core vocabulary, rule pack, fact contract, deterministic evaluation, finding, gate response, audit export.
9. Work order parts demand from MaintainArr to SupplyArr and status return.
10. Route assignment gate using StaffArr, TrainArr, MaintainArr, and Compliance Core.
11. Incident to StaffArr record, TrainArr remediation, and readiness recovery.
12. Companion app task inbox and offline-resilient submission for assigned product work.
13. Cross-product audit package export.

## Evidence Required Per Feature

Every feature matrix row requires at least one of these evidence types:

- backend endpoint path
- frontend route/component path
- database migration/entity path
- worker/job path
- test path
- OpenAPI path
- event contract path
- report/export sample path
- audit log assertion
- E2E journey assertion

A feature is not complete when it exists only as:

- TODO text
- mock-only state
- static sample data
- frontend-only UI
- documentation-only claim
- unprotected backend route
- route with no tenant/permission enforcement
