# STL Compliance Fixture, Demo, and No-Op Boundary Constitution

## 1. Audit drivers

FUNC-001 and SEC-002 through SEC-004 showed fixture-backed products and mutations that looked complete while discarding state or losing it on restart.

## 2. Prime directive

Production UI and APIs may never present fixtures, local-only changes, no-op handlers, or simulated completion as real tenant work.

## 3. Prohibited production behavior

- seeded fixture lists used as operational records
- `createLocal*` or equivalent fallback after API failure
- success toast before durable server confirmation
- submit handlers that discard writes
- generated reports/schedules/exports that are not stored or executed
- hard-coded owner-product references presented as live integration
- fake sync, import, OCR, scan, payment, posting, or dispatch success

## 4. Allowed demo behavior

A demo mode must be environment-gated, visibly labeled on every page, isolated from production tenants, use non-production credentials/storage, and be excluded from release-readiness claims. Demo routes and fixtures must have an owner and removal/maintenance policy.

## 5. Failure behavior

When a dependency or write fails, preserve the user’s input and show the actual state: failed, pending, offline, degraded, or not saved. Never silently switch to a fake local success path.

## 6. Static and runtime checks

CI should scan production bundles/routes for fixture imports, known local-fallback helpers, no-op handlers, and development-only data providers. E2E tests must verify persistence after refresh and service restart.
