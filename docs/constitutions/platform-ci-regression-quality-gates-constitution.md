# STL Compliance CI Regression and Quality Gates Constitution

## 1. Audit drivers

REL-001 through REL-007 and TEST-001 found deterministic workflow failures, missing scripts, omitted products, source-alias dependency problems, hanging tests, and zero-test suites reporting green.

## 2. Prime directive

CI is a release control, not a collection of best-effort scripts. A green build must mean every production surface was discovered, built, tested, and checked by a real command.

## 3. Required gates

- clean-checkout dependency install
- frontend typecheck/build/test for every app and shared package
- backend restore/build/test for every service/worker/package
- migration generation/model drift/apply verification
- endpoint authorization-map comparison
- tenant-isolation and contract tests
- theme-token audit
- accessibility and browser smoke tests
- route/page inventory checks
- documentation link/manifest checks
- dependency/security audit with reviewed exceptions
- container/deployment configuration validation

## 4. No false green

The following fail CI:

- nonexistent script referenced by workflow
- `--passWithNoTests` for a production product
- skipped product not declared in an approved temporary exception
- migration check that does not execute
- hanging process forced successful by timeout
- theme or link audit ignored
- warnings configured as non-fatal when they indicate correctness/security issues

## 5. Test baseline by risk

Critical workflows require API authorization, tenant isolation, state machine, idempotency, concurrency, persistence/restart, integration contract, page-state, light/dark, keyboard/accessibility, and E2E tests. Test counts and changed-risk coverage are reported per product.

## 6. Exceptions

A temporary exception names the exact check, owner, reason, compensating control, issue link, and expiration. Expired exceptions fail the build.
