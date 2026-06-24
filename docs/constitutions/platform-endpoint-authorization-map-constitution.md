# STL Compliance Endpoint Authorization Map Constitution

## 1. Audit drivers

SEC-001 found 109 AssurArr domain routes without authorization. Other findings showed authenticated groups with individual fail-open mutations.

## 2. Prime directive

No endpoint exists outside an explicit authorization map. Route registration, permission metadata, tests, and documentation must agree.

## 3. Required matrix fields

Every browser, integration, worker, portal, upload, export, and admin route records:

- HTTP method and canonical route
- owning product and resource/action
- audience: tenant user, external actor, service, public intake, platform admin
- authentication requirement
- tenant-context source
- product permission key or service scope
- record/site/location/party scope
- actor source and on-behalf-of policy
- request sensitivity and response sensitivity
- idempotency/concurrency requirement
- audit requirement
- anonymous result and forbidden result
- tests proving each rule

## 4. Deny by default

Application hosts must use a fallback authorization policy or equivalent deny-by-default mechanism. Anonymous routes require an explicit marker, narrow purpose, abuse controls, and test.

## 5. Compliance Core distinction

Compliance Core administrative/studio endpoints require platform-admin validation. Runtime endpoints called by products require authenticated tenant/service context and contract-specific scope; they are not platform-admin-only.

## 6. Integration routes

Integration routes use service identity and scopes, not ordinary browser tokens. A route may support both audiences only when the policies are explicit and independently tested.

## 7. Response behavior

- unauthenticated: 401
- authenticated but unauthorized: 403
- tenant-mismatched or invisible record: consistent 404/403 policy that does not leak existence
- conflict: 409 with stable machine code and human-safe detail

## 8. Pull-request gate

Adding or changing an endpoint without changing the authorization matrix and negative tests fails review. Reflection/OpenAPI checks should compare mapped routes to the committed matrix and fail on omissions.
