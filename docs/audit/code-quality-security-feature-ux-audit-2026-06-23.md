> **Historical implementation evidence and remediation baseline.** This report describes the repository as audited on June 23, 2026. Current ownership, access, page, and release rules are defined by the canonical constitutions and platform documents in this package. References to product access grants in observed code or prior docs are findings to remove, not current architecture.

# STL Compliance Comprehensive Code, Security, Feature, Navigation, UX, and UI Consistency Audit

**Audit date:** June 23, 2026  
**Repository reviewed:** `STLComplianceV2-main`  
**Audit type:** Static architecture/security review, documentation-to-implementation comparison, CI/release review, and targeted frontend verification  
**Overall release verdict:** **Not production-ready**

---

## 1. Executive verdict

STLComplianceV2 contains two materially different levels of implementation maturity:

1. **The established platform spine is credible.** NexArr, StaffArr, TrainArr, MaintainArr, RoutArr, SupplyArr, Compliance Core, and the shared UI/platform packages contain substantial domain models, tenant-aware persistence, broad route coverage, and meaningful tests.
2. **Several newer products are production-looking prototypes.** LoadArr, OrdArr, RecordArr, and ReportArr expose broad, polished workflows over static fixtures or process-local singleton lists. These workflows do not provide durable system-of-record behavior and, in several cases, do not enforce tenant isolation.
3. **AssurArr is functionally broad but critically unsafe.** Its 109 quality-management endpoints are not protected by authorization and its service writes all records to one hard-coded tenant.
4. **The repository cannot currently prove releasability.** The primary CI workflow contains a guaranteed-failing migration check, the nightly browser workflow calls scripts that do not exist, clean frontend jobs do not install the source-aliased shared UI package dependencies, and important applications are absent from CI.
5. **The shared UI direction is good, but enforcement is incomplete.** Shared shell, navigation, forms, product switcher, print runtime, quick-create primitives, and theme tokens exist. A repository theme audit still reports 71 violations, and several newer applications bypass the shared design language with hard-coded dark or light colors.

### Immediate release decision

| Decision | Products / surfaces |
|---|---|
| **Block deployment** | AssurArr, LoadArr, OrdArr, RecordArr, ReportArr |
| **Allow only controlled internal testing after common security hardening** | NexArr, Field Companion, CustomArr, LedgArr, Suite shell |
| **Best-established products, still subject to common platform fixes** | StaffArr, TrainArr, MaintainArr, RoutArr, SupplyArr, Compliance Core |

The blocked applications should not merely receive warning banners. Their unsafe or simulated write routes should be disabled until tenant-aware durable implementations replace them.

---

## 2. Audit scope and limitations

### Reviewed

- Approximately **5,832 repository files** and roughly **1.2 million C#/TypeScript/CSS source lines**.
- Product constitutions and product manifests under `docs/constitutions/` and `docs/products/`.
- ASP.NET host, authorization, CORS, security headers, exception handling, migrations, identity/session flows, and representative endpoint groups.
- React authentication storage, navigation, shared UI consumption, error behavior, theme use, bundle composition, and representative workflows.
- GitHub Actions, frontend package topology, test coverage distribution, repository hygiene, and deployment assumptions.
- Targeted npm installs, builds, tests, dependency audits, and the repository theme audit.

### Not completed

- A full .NET compile and backend test run could not be executed because the environment did not contain the .NET 10 SDK and external SDK download failed due DNS/network resolution.
- No live deployment, database, Redis, service-to-service, browser penetration, load, or destructive tenant-isolation test was performed.
- No rendered-browser WCAG audit was run. Accessibility findings below are code-review findings, not a claim of complete WCAG conformance testing.

This means the report can establish the listed static defects with high confidence, but it should not be interpreted as proof that unlisted runtime defects do not exist.

---

## 3. Scorecard

| Area | Score | Assessment |
|---|---:|---|
| Security and tenant isolation | **2/10** | Multiple critical cross-tenant/anonymous access paths; otherwise solid controls in mature products cannot offset them. |
| Release engineering and CI confidence | **2/10** | Main and nightly workflows contain deterministic failures and omit important surfaces. |
| Code architecture and maintainability | **5/10** | Strong shared foundations and mature bounded contexts, undermined by giant files, source-level package coupling, and prototype stores. |
| Features versus documented expectations | **5/10** | Broad route/UI coverage, but several advertised systems of record are non-durable simulations. |
| Navigability and information architecture | **6/10** | Shared shell is good; several products overload flat or oversized navigation groups. |
| User friendliness and trustworthiness | **4/10** | Strong page primitives, but silent local-success fallbacks, raw IDs/JSON, browser dialogs, and inconsistent errors harm trust. |
| UI consistency and theme quality | **5/10** | Good central token architecture; 71 detected violations and dark-mode-specific implementations remain. |
| Automated test confidence | **4/10** | Mature apps have strong suites; newer apps often have zero to three frontend tests and thin backend coverage. |
| **Overall** | **4/10** | Strong platform potential, but not safe or honest enough for production release as a full suite. |

---

## 4. Critical and high-priority findings

| ID | Severity | Finding | Release consequence |
|---|---|---|---|
| SEC-001 | Critical | AssurArr domain endpoints are anonymous and all records use one hard-coded tenant. | Full AssurArr release block. |
| SEC-002 | Critical | RecordArr uses a global singleton fixture store; multiple read/write paths lack tenant and permission checks. | Full RecordArr release block. |
| SEC-003 | Critical | OrdArr stores all tenants’ orders in one singleton list and order records have no tenant field. | Full OrdArr release block. |
| SEC-004 | Critical | ReportArr stores datasets, dashboards, reports, KPIs, alerts, and audit packages globally in memory. | Full ReportArr release block. |
| FUNC-001 | Critical | LoadArr’s core WMS records and mutations are fixture-generated or discarded; frontend failures can be shown as successful local completions. | Full LoadArr release block. |
| REL-001 | Critical | CI and nightly browser verification are structurally broken. | No trustworthy release gate. |
| SEC-005 | High | Browser bearer/refresh tokens are stored in JavaScript-readable web storage while SPA HTML lacks a real CSP/frame-ancestor policy. | Common platform security gate. |
| SEC-006 | High | Field Companion stores geolocation, site/location, notes, and clock punches unencrypted in local storage. | Mobile/offline privacy gate. |
| SEC-007 | High | RecordArr accepts entire base64 files into memory without an enforced size limit or content scanning. | Upload abuse and availability risk. |
| SEC-008 | High | RecordArr and other thin workflows accept actor/person IDs from request bodies, allowing audit attribution spoofing. | Audit integrity gate. |
| SEC-009 | High | NexArr refresh-token rotation is not atomic and can issue multiple descendants under a race. | Authentication hardening gate. |
| SEC-010 | High | NexArr stores MFA shared secrets as ordinary plaintext database strings. | Credential-at-rest hardening gate. |
| SEC-011 | High | Field Companion issues the NexArr user ID as both `userId` and `personId`. | Identity-boundary correctness gate. |
| REL-002 | High | Most frontend CI jobs compile shared UI source without installing shared UI dependencies. | Clean-checkout builds are unreliable. |
| UI-001 | High | Theme audit reports 71 hard-coded/light-only color violations. | Light/dark consistency gate. |
| TEST-001 | High | Several feature-rich products have zero meaningful frontend tests; `--passWithNoTests` reports success. | Regressions can ship unnoticed. |

---

# 5. Security audit

## SEC-001 — AssurArr exposes anonymous quality-management operations

**Evidence**

- `apps/assurarr-api/AssurArr.Api/Endpoints/AssurArrEndpoints.cs:10-11` creates the main and integration groups without `RequireAuthorization()`.
- The file maps **109** domain routes. Representative anonymous reads and writes appear at lines `13-33`.
- Only the separate session groups in `Endpoints/AuthEndpoints.cs` require authorization.
- No application-wide fallback authorization policy was found.
- `apps/assurarr-api/AssurArr.Api/Services/AssurArrQualityService.cs:2721-2736` assigns `TenantId = DefaultTenantId`; `DefaultTenantId` is the constant `22222222-2222-2222-2222-222222222222`.

**Impact**

An unauthenticated caller can read or mutate quality records. Even after adding authentication, every tenant would operate against the same tenant identifier. This compromises confidentiality, integrity, auditability, and all tenant-bound quality workflows.

**Required correction**

- Apply deny-by-default authorization globally and explicitly require AssurArr entitlement and route-specific permissions.
- Resolve tenant and actor identity exclusively from validated claims/service-token context.
- Remove `DefaultTenantId` entirely.
- Add `TenantId` predicates to every query and tenant-scoped unique indexes to every tenant-owned entity.
- Separate browser and integration authorization. Integration routes should require validated service credentials and narrow scopes, not ordinary user authentication.
- Add automated anonymous, wrong-entitlement, wrong-permission, and cross-tenant tests for every endpoint family.

**Acceptance test**

For two tenants with colliding record names, neither browser nor service-token callers can list, retrieve, update, transition, or infer the other tenant’s records. Anonymous requests return 401; authenticated but unauthorized requests return 403.

---

## SEC-002 — RecordArr is global, non-durable, and fail-open

**Evidence**

- `apps/recordarr-api/RecordArr.Api/RecordArrServiceRegistration.cs:12-16` registers `RecordArrStore` as a singleton.
- `apps/recordarr-api/RecordArr.Api/Data/RecordArrDbContext.cs:6-11` defines no RecordArr domain tables.
- `apps/recordarr-api/RecordArr.Api/Data/RecordArrStore.cs:9-41` holds records, files, OCR results, metadata, retention, legal holds, controlled documents, access grants, external shares, and access logs in process-local lists.
- The constructor initializes and seeds fixtures at lines `43-76`.
- The route group is authenticated at `Endpoints/WorkspaceEndpoints.cs:9-11`, but multiple mutations do not receive `HttpContext` or a principal. Examples include metadata/links/comments at lines `35-69`, record status/archive/purge at `141-157`, and global access log/capture-request reads at `159-163`.
- Store methods such as `UpdateRecordStatus`, `ArchiveRecord`, and `PurgeRecord` at `Data/RecordArrStore.cs:830-870` accept a record ID and request-supplied actor without tenant context.
- `CanAccessRecord` at `Data/RecordArrStore.cs:2304-2341` never compares `record.TenantId` to the principal’s tenant and explicitly allows access when no active policy exists at lines `2322-2329`.

**Impact**

Authenticated users can potentially read or mutate other tenants’ records by identifier. Restarting or scaling the API loses or fragments all in-memory state. The access model is fail-open, contradicting RecordArr’s role as the suite’s evidence and records system of record.

**Required correction**

- Replace the singleton with scoped EF-backed repositories/services.
- Persist every domain aggregate and event with `TenantId`.
- Require principal and permission checks on every read and mutation; default to deny when no policy is present.
- Derive actor/person identity from claims.
- Make legal hold, retention, archive, purge, external share, and access-log changes transactional and append-only where appropriate.
- Add database uniqueness/concurrency constraints and restart/multi-replica tests.

---

## SEC-003 — OrdArr shares one order collection across all tenants

**Evidence**

- `apps/ordarr-api/OrdArr.Api/OrdArrServiceRegistration.cs:20-23` registers `OrdArrStore` as a singleton.
- `apps/ordarr-api/OrdArr.Api/Data/OrdArrDbContext.cs:6` contains no OrdArr domain sets.
- `apps/ordarr-api/OrdArr.Api/Data/OrdArrStore.cs:8-17` holds `_orders` and an idempotency dictionary in process memory.
- Dashboard/list/detail methods use the complete `_orders` collection after checking only entitlement, for example lines `28-75` and `120-143`.
- Order creation scopes the idempotency dictionary key by tenant at line `189`, but the created `OrdArrOrderDetailResponse` does not contain `TenantId`; its record definition at lines `1043-1082` has no tenant field.

**Impact**

Any tenant entitled to OrdArr can view or mutate the same global order list. State disappears after restart, and multiple replicas would each expose different order populations.

**Required correction**

- Implement durable OrdArr entities with `TenantId` on order, line, hold, return, timeline, handoff, and completion/financial packet records.
- Use tenant-scoped database queries and composite unique keys.
- Move idempotency to a durable table with a unique `(TenantId, OperationKey, IdempotencyKey)` constraint.
- Enforce per-action permissions in addition to entitlement.
- Add cross-tenant, duplicate-idempotency, concurrency, restart, and multi-replica tests.

---

## SEC-004 — ReportArr data is global and disappears on restart

**Evidence**

- `apps/reportarr-api/ReportArr.Api/ReportArrServiceRegistration.cs:14-16` registers `ReportArrStore` as a singleton.
- `apps/reportarr-api/ReportArr.Api/Data/ReportArrDbContext.cs:6-11` defines no reporting domain tables.
- `apps/reportarr-api/ReportArr.Api/Data/ReportArrStore.cs:124-158` declares global lists for datasets, connectors, events, read models, dashboards, reports, schedules, recipients, exports, metrics, KPIs, alerts, audit scopes, and audit packages.
- Those lists are initialized empty at lines `160-166`.
- `GetSummary` at lines `272-292` reports counts from global lists. `GetDatasets` at lines `296-303` filters by source-product access, not tenant ownership.

**Impact**

Tenant A’s reporting artifacts can become visible to tenant B if source-product checks pass. Scheduled reports, alert state, report runs, exports, and audit packages are lost after process restart. This does not meet ReportArr’s documented role as a durable cross-product reporting system.

**Required correction**

- Persist definitions, access policies, schedules, runs, outputs, read-model cursors, lineage, and alert state.
- Tenant-scope every artifact and query before applying product/role access rules.
- Run schedules and refresh jobs through durable workers with leases/idempotency.
- Store large outputs in controlled object storage/RecordArr, retaining immutable metadata and hashes.
- Add tenant-isolation and scheduler-recovery tests.

---

## SEC-005 — Browser tokens are exposed to any successful same-origin script

**Evidence**

- `apps/suite-frontend/src/auth/authStorage.ts:5-23, 27-45` stores access token, refresh token, session ID, user ID, and tenant ID in `sessionStorage`.
- `apps/fieldcompanion-frontend/src/auth/sessionStorage.ts:5-35, 39-57` stores both bearer and refresh tokens in `sessionStorage`.
- Other product frontends also store access tokens in web storage.
- `packages/shared-dotnet/STLCompliance.Shared/Middleware/SecurityHeadersMiddlewareExtensions.cs:30-49` applies document headers without a Content Security Policy.
- `packages/shared-dotnet/STLCompliance.Shared/Hosting/StlApiHost.cs:90-129` branches SPA traffic before the API `UseStlSecurityHeaders()` middleware at lines `132-141`. Therefore the API CSP does not protect the bundled HTML document.

**Impact**

A successful XSS defect, compromised third-party script, or same-origin injection can read bearer and refresh tokens. Refresh-token theft substantially extends compromise duration.

**Required correction**

Preferred architecture:

- Move browser authentication to secure, `HttpOnly`, `Secure`, `SameSite` cookies through a same-origin backend-for-frontend/session model.
- Keep access tokens server-side or in short-lived memory only.
- Add CSRF protection for cookie-authenticated state-changing requests.
- Deliver a nonce/hash-based CSP on every SPA HTML response, including `frame-ancestors`.
- Eliminate inline script/style dependencies or explicitly nonce only unavoidable content.

At minimum, do not persist refresh tokens in JavaScript-readable storage and add a strict CSP before claiming browser hardening.

---

## SEC-006 — Field Companion persists sensitive workforce data in plaintext local storage

**Evidence**

- `apps/fieldcompanion-frontend/src/lib/offlineQueue.ts:17-28` defines offline clock-punch payloads containing timestamps, device, geolocation, site, location, and notes.
- Lines `56-79` read/write the entire queue using `window.localStorage`.
- Lines `116-156` enqueue and persist the complete clock-punch payload.

**Impact**

Workforce location and timekeeping data remains readable to local users, malware with profile access, browser extensions, and any successful XSS. Shared or lost devices can expose prior pending actions. Queue data can also be modified client-side and therefore cannot be treated as trusted evidence.

**Required correction**

- Store only the minimum offline data required.
- Use IndexedDB with an encrypted envelope and a non-exportable Web Crypto key where the threat model permits; otherwise require device reauthentication and clear pending data aggressively.
- Add user-visible pending-data controls and remote-session/device revocation behavior.
- Bind queued actions to tenant, person, device registration, monotonic sequence, and server-validated idempotency.
- Treat all offline timestamps/geolocation as asserted evidence requiring server-side validation and audit metadata.

---

## SEC-007 — RecordArr file upload is unbounded and fully buffered

**Evidence**

`apps/recordarr-api/RecordArr.Api/Endpoints/WorkspaceEndpoints.cs:90-115` accepts `FileContentBase64`, decodes the whole value into a byte array, wraps it in a `MemoryStream`, and saves it. No request-size check, decoded-size limit, MIME signature validation, malware scanning, or quarantine workflow is shown.

**Impact**

A caller can cause high memory pressure or process termination. Base64 adds roughly one-third transfer overhead. Unscanned content can enter the records repository and later be distributed or downloaded.

**Required correction**

- Replace JSON/base64 upload with streaming multipart or pre-signed object upload.
- Enforce limits at reverse proxy, ASP.NET request, route, tenant-plan, and storage layers.
- Validate extension, declared MIME, and file signature independently.
- Quarantine until malware/content scanning completes.
- Store content hash, scanner/version, scan result, and immutable evidence chain.

---

## SEC-008 — Audit actors can be supplied by the caller

**Evidence**

Representative RecordArr routes use `request.CreatedByPersonId`, `request.EditedByPersonId`, `request.UploadedByPersonId`, and `request.ActorPersonId` in `Endpoints/WorkspaceEndpoints.cs:47-69, 118-137, 147-156`. Similar UI payloads exist in LoadArr, for example `apps/loadarr-frontend/src/App.tsx:3018-3025`.

**Impact**

A caller can attribute actions to another employee. This breaks nonrepudiation and makes compliance/audit history unreliable.

**Required correction**

Derive actor identity from the authenticated principal or validated service identity. Keep “performed on behalf of” as a separate permission-gated field with both initiator and subject recorded.

---

## SEC-009 — Refresh-token rotation is vulnerable to concurrent reuse

**Evidence**

`apps/nexarr-api/NexArr.Api/Services/AuthService.cs:153-188` reads a valid refresh session, sets `RevokedAt`, saves, and then creates a replacement session. There is no surrounding serializable transaction, row-version check, unique rotation-family constraint, or atomic conditional update.

**Impact**

Two near-simultaneous renewals can both read an unrevoked session and issue separate descendants. This weakens refresh-token replay detection.

**Required correction**

- Atomically consume the token with an update conditioned on `RevokedAt IS NULL` and expected row version.
- Persist rotation family, parent, replacement, consumed time, and reuse detection.
- Revoke the entire family when reuse is detected.
- Add a concurrency test that sends simultaneous renewals and proves only one succeeds.

---

## SEC-010 — MFA shared secrets appear unencrypted at rest

**Evidence**

- `apps/nexarr-api/NexArr.Api/Entities/UserCredential.cs:15-19` stores `MfaSecret` as a string.
- `apps/nexarr-api/NexArr.Api/Data/NexArrDbContext.cs:135-142` configures only maximum length; no encryption converter or protected-secret service is visible.
- Recovery codes are separately represented as hashes, which is the correct direction for recovery codes.

**Impact**

A database read compromise exposes active TOTP seed material and permits generation of future MFA codes.

**Required correction**

Encrypt TOTP secrets using an application/KMS-managed envelope key with key versioning and rotation. Do not log or return secrets after enrollment confirmation. Preserve hashed one-time recovery codes.

---

## SEC-011 — Password reset claims delivery without a delivery mechanism

**Evidence**

`apps/nexarr-api/NexArr.Api/Services/PasswordResetService.cs:19-74` creates and stores a reset token, returns plaintext only in Development/Testing, and always responds that instructions were sent. No mail/notification delivery dependency is injected or invoked.

**Impact**

In production, users can be told that instructions were sent even though no delivery path is visible. This creates account-recovery failure and support burden.

**Required correction**

Publish a durable password-reset notification event or call an approved notification service after token persistence. Track delivery attempt/result without exposing account existence. Add expiry, resend throttling, delivery observability, and end-to-end recovery tests.

---

## SEC-012 — Field Companion conflates platform user and person identity

**Evidence**

`apps/nexarr-api/NexArr.Api/Services/FieldCompanionAuthService.cs:109-116` passes `record.UserId` as the person identifier when creating the access token. The response at lines `135-150` places `record.UserId` in both the user and person positions.

**Impact**

StaffArr-owned person operations can be attributed to a NexArr account ID rather than the canonical person ID. Users without linked person records may also be treated as valid workforce actors.

**Required correction**

Resolve the active tenant membership’s linked canonical person ID. Deny workforce actions when a required person link does not exist. Keep `userId` and `personId` as distinct claims and contract fields.

---

## SEC-013 — Reverse-proxy client identity and transport assumptions are not explicit

**Evidence**

- No `UseForwardedHeaders`, trusted proxy/network configuration, `UseHsts`, or `UseHttpsRedirection` call was found in the shared host or product applications.
- NexArr authentication throttling partitions on `httpContext.Connection.RemoteIpAddress` in `apps/nexarr-api/NexArr.Api/NexArrServiceRegistration.cs:160-170`.
- Login audit/IP capture also reads `RemoteIpAddress` in `Endpoints/AuthEndpoints.cs:19-25`.

**Impact**

Behind a reverse proxy, all clients may appear to originate from the proxy, weakening rate-limit fairness and audit accuracy. Alternatively, blindly trusting forwarded headers later would permit spoofing. TLS/HSTS may be correctly handled by Render, but that contract is not represented or verified in application tests.

**Required correction**

Configure forwarded headers before authentication/rate limiting, restricted to known proxy networks and a safe forward limit. Add a deployment assertion/test that HTTPS and HSTS are enforced at exactly one trusted layer and that original scheme/client IP are correct.

---

## SEC-014 — The global Permissions Policy disables Field Companion capabilities

**Evidence**

`packages/shared-dotnet/STLCompliance.Shared/Middleware/SecurityHeadersMiddlewareExtensions.cs:8-10, 30-38` emits `camera=(), microphone=(), geolocation=()` for all SPA documents. Field Companion’s documented and implemented workflows require capture and geolocation.

**Impact**

When served through the shared host, browser APIs required for Field Companion may be blocked even after user permission. This is both a feature defect and a security-policy design defect.

**Required correction**

Create product-aware document policies. Keep capabilities disabled by default, but allow `'self'` only on the specific Field Companion routes that need them. Test actual response headers and browser permission behavior.

---

## SEC-015 — API exception handling is incomplete

**Evidence**

`packages/shared-dotnet/STLCompliance.Shared/Middleware/ApiExceptionMiddleware.cs:8-32` catches only `StlApiException`. Prototype stores frequently throw `InvalidOperationException`, for example `RecordArrStore.cs:834-838`.

**Impact**

Unexpected domain errors can escape the standard error envelope, produce inconsistent user messages, and potentially expose default framework behavior. Correlation IDs are not guaranteed in those responses.

**Required correction**

- Add a final exception handler that logs full server detail and returns a stable, non-sensitive 500 envelope with correlation ID.
- Convert expected not-found/conflict/validation paths into typed errors rather than general exceptions.
- Add contract tests for every status family.

---

## SEC-016 — CORS trusts every subdomain by default

**Evidence**

`packages/shared-dotnet/STLCompliance.Shared/Hosting/StlCorsPolicyExtensions.cs:9, 21-30, 43-50` automatically includes `https://*.stlcompliance.com`, enables wildcard subdomains, and allows any header and method.

**Impact**

Any compromised or unintentionally hosted subdomain becomes an allowed browser origin. The policy does not enable credentials, which reduces but does not remove risk because bearer-authenticated JavaScript can still call APIs.

**Required correction**

Use explicit per-environment origins for each frontend. Treat wildcard origin support as an opt-in exception with ownership and expiry. Restrict methods/headers where practical and test rejected origins.

---

## SEC-017 — Production application startup performs schema changes

**Evidence**

- `packages/shared-dotnet/STLCompliance.Shared/Hosting/StlApiHost.cs:149-155` applies migrations in Production.
- `StlApiHost.cs:279-327` executes `Database.MigrateAsync()` during startup.
- Lines `329-379` issue raw `CREATE TABLE/INDEX IF NOT EXISTS` DDL for print-export logs.

**Impact**

Application replicas race for schema changes, startup availability depends on migration success, and runtime database identities require DDL privileges. Raw DDL also bypasses the normal migration history/model review path.

**Required correction**

Move migrations to an explicit, single-run deployment job using a restricted migration identity. Represent print-export storage in versioned EF migrations. Application identities should receive only runtime DML permissions.

---

## SEC-018 — Access tokens are embedded in client query-cache keys

**Evidence**

Examples include:

- `apps/assurarr-frontend/src/App.tsx:450, 5227, 6029, 7721`
- `apps/compliancecore-frontend/src/components/AuditDeliveryOrchestrationPanel.tsx:31-53`
- `apps/compliancecore-frontend/src/workspace/useComplianceCoreWorkspaceState.tsx:99-245`

**Impact**

Tokens are duplicated through query-cache metadata, debugging tools, error captures, and memory snapshots. They also make cache identity depend on a secret rather than stable tenant/user/session scope.

**Required correction**

Use non-secret keys such as `[product, tenantId, userId, resource, filters]`. Inject the current token only inside the fetcher. Clear query caches on tenant/session change.

---

# 6. Release engineering and test confidence

## REL-001 — The main migration gate is guaranteed to fail

`/.github/workflows/ci.yml:53-58` uses shell `test -f` against four `*InitialPlatformFoundation*.cs` patterns. The current migrations do not consistently use those names. A glob with no match remains a literal path and fails the job.

**Correction**

Replace filename checks with executable migration validation:

- Build each DbContext.
- Generate an idempotent migration script.
- Fail on pending model changes.
- Apply migrations to ephemeral PostgreSQL databases and run readiness/tenant-smoke tests.

## REL-002 — Nightly browser workflow calls nonexistent scripts

`/.github/workflows/e2e-nightly.yml:129-135` executes:

- `scripts/ops/e2e-stack-up.sh`
- `scripts/ops/e2e-frontends-preview.sh`

Neither script is present. Worse, `.gitignore:25-27` ignores the entire `scripts/` directory, making accidental omission likely.

**Correction**

Stop ignoring `scripts/`. Commit versioned scripts or move the commands into a maintained task runner. Add a lightweight workflow-lint job that validates referenced local actions/scripts before the expensive test matrix.

## REL-003 — Main CI omits important applications and shared packages

The main workflow contains frontend jobs through OrdArr, but no RecordArr or Field Companion frontend job and no direct shared UI build/test/theme job. The nightly workflow includes only a subset of product frontends and its service stack is stale relative to the current portfolio.

**Correction**

Generate the matrix from one canonical product manifest. Every shippable frontend/API/worker/shared package must appear automatically. Fail when a product directory exists without a matrix entry.

## REL-004 — Frontends import shared UI source without installing its dependencies

Representative aliases:

- `apps/assurarr-frontend/tsconfig.app.json:18-19`
- `apps/assurarr-frontend/vite.config.ts:18`
- equivalent aliases in StaffArr, LoadArr, and most other apps

CI runs `npm ci` only inside each app. A clean AssurArr build failed because modules required by `packages/shared-ui/src` were absent until `npm ci` was run in `packages/shared-ui`.

**Correction**

Adopt a root npm/pnpm workspace with a single lockfile and declare `@stl/shared-ui` as a workspace dependency. Build/test the package explicitly; do not depend on undeclared sibling `node_modules` state.

## REL-005 — Shared UI tests pass but the process does not terminate

The shared UI run reported **21 files and 112 tests passed**, but the process remained alive until externally terminated. This would stall a CI job.

**Correction**

Run Vitest with hanging-process/open-handle diagnostics, isolate timer/listener leaks, and ensure every test cleans up rendered roots, observers, global listeners, fake timers, workers, and query clients.

## REL-006 — Zero-test products can report green

AssurArr, CustomArr, RecordArr, and ReportArr currently have zero frontend test files. Their package scripts use `vitest run --passWithNoTests`, so absence of tests is reported as success.

**Correction**

Remove `--passWithNoTests` for shippable packages. Establish minimum critical-flow suites and coverage floors by risk, not arbitrary global percentages.

## REL-007 — Dependency audit requires dev-tooling updates

A full npm audit of the representative CustomArr lock reported two high-severity development/build-tool findings: direct Vite `8.0.0-8.0.15` and transitive `undici <7.28.0`. `npm audit --omit=dev` reported zero production dependency vulnerabilities.

**Correction**

Upgrade the shared Vite/tooling baseline and refresh all app lockfiles together through the workspace. Keep production and development audit results distinct in release reporting.

---

# 7. Code quality and architecture

## CQ-001 — Architecture quality is uneven by product generation

The mature products generally use tenant-scoped EF entities, domain services, workers, integration clients, and extensive tests. The newer products frequently concentrate entire domains in one endpoint file/store/App component.

This is not merely stylistic inconsistency. It changes correctness characteristics: durable database constraints and tenant predicates in mature apps are replaced by mutable lists and process locks in newer apps.

**Correction**

Create a mandatory product implementation baseline covering:

- Durable tenant-scoped storage.
- Entitlement plus permission enforcement.
- Claim-derived actor identity.
- Outbox/idempotency/concurrency strategy.
- List/detail/create/edit/archive page conventions.
- Integration contract tests.
- Restart and multi-replica behavior.

A product should not be marked implemented until it meets the baseline.

## CQ-002 — Very large frontend files prevent safe, local reasoning

| File | Approx. lines |
|---|---:|
| `apps/reportarr-frontend/src/App.tsx` | 8,620 |
| `apps/assurarr-frontend/src/App.tsx` | 7,886 |
| `apps/loadarr-frontend/src/App.tsx` | 6,745 |
| `apps/recordarr-frontend/src/App.tsx` | 6,085 |
| `apps/ledgarr-frontend/src/App.tsx` | 5,150 |
| `apps/customarr-frontend/src/App.tsx` | 2,349 |

These files combine navigation, routing, API calls, mutations, types, fixtures, forms, tables, state machines, and page rendering.

**Impact**

- High merge-conflict rate.
- Difficult route-level code splitting.
- Broad rerender and regression blast radius.
- Tests require mounting too much application state.
- Developers cannot confidently determine ownership boundaries.

**Correction**

Split by domain feature and page route:

- `features/<aggregate>/api.ts`
- `features/<aggregate>/queries.ts`
- `features/<aggregate>/contracts.ts`
- `features/<aggregate>/pages/*`
- `features/<aggregate>/components/*`
- `routes/*`

Keep `App.tsx` limited to providers and route composition.

## CQ-003 — Several backend services are too large

Representative files include:

- `RecordArrStore.cs` — approximately 4,341 lines.
- `LedgArrStore.cs` — approximately 4,226 lines.
- MaintainArr `WorkOrderService` — approximately 4,135 lines.
- Compliance Core staged import service — approximately 3,994 lines.
- `AssurArrQualityService.cs` — approximately 3,537 lines.

**Correction**

Split by aggregate/application command, while keeping transactions in explicit orchestration services. Use query services for projections and command handlers for state changes. Do not replace one giant service with dozens of pass-through classes; boundaries should follow actual invariants.

## CQ-004 — Compilation warnings are explicitly non-fatal

`Directory.Build.props:3-7` targets .NET 10 with preview language features while setting `<TreatWarningsAsErrors>false</TreatWarningsAsErrors>`.

**Correction**

Enable warnings as errors in CI, with a reviewed temporary suppression file for existing debt. Preview language use should be intentional and documented because it increases toolchain volatility.

## CQ-005 — Error behavior is inconsistent and sometimes dishonest

The most serious example is LoadArr’s silent local fallback. `apps/loadarr-frontend/src/App.tsx:2940-3008` catches failed count/adjustment API calls, synthesizes local mutation results, and sets status to `completed`.

**Impact**

The user receives a successful completion state even though the backend did not accept or persist the operation. For inventory, counts, and adjustments, this can create physical/system divergence.

**Correction**

- Never transform a failed production write into success.
- Clearly label intentionally simulated demo mode and isolate it from production builds/data.
- Use shared error callouts with correlation IDs and safe retry.
- Preserve entered values so users can retry without rework.

## CQ-006 — Repository hygiene obscures source-of-truth status

The repository root contains `.codex-logs`, `audit-run-logs`, `docs.zip`, API output logs, generated CSV data, and local line-count scripts. `.gitignore` does not exclude several of these, while it incorrectly excludes the entire `scripts/` directory.

`README.md:5-24` still presents an early milestone state and lists only seven original APIs, despite the much larger current suite.

**Correction**

- Remove generated logs/archives from source control and store audit artifacts in CI artifact storage.
- Stop ignoring operational scripts.
- Rewrite the README from the canonical product manifest.
- Add repository checks for large/generated/unapproved root files.

## CQ-007 — Frontend package management is duplicated across the suite

Each app has its own lockfile and nearly identical React/Vite/Vitest dependencies. This permits version drift and makes a common security/tooling upgrade repetitive.

**Correction**

Adopt a workspace, centralize shared tool versions, and use package-level boundaries rather than source aliases into sibling folders.

---

# 8. Features versus documented expectations

A route or polished page is not counted as a complete feature when its state is static, process-local, cross-tenant, or discarded.

| Product | Documented role | Implementation assessment | Readiness | Most important gaps |
|---|---|---|---|---|
| NexArr | Identity, accounts, sessions, tenants, entitlements, launch/handoff, service trust | Substantial durable implementation and tests | Conditional | Atomic refresh rotation, MFA secret encryption, real password-reset delivery, Field person mapping, proxy/transport hardening |
| StaffArr | People, org/location hierarchy, roles/permissions, incidents, workforce profile | Strong, modular, test-rich | Best-established | Bundle splitting, replace browser dialogs, continue delegated NexArr account controls through explicit APIs |
| TrainArr | Programs, assignments, execution/signoff, qualifications, certificates, remediation | Strong domain breadth and high frontend test count | Best-established | Continue integration/E2E and performance validation; preserve ownership boundaries |
| MaintainArr | Assets/components, work orders, defects, inspections, PM, parts/labor/downtime | Strong domain breadth and tests | Best-established | Split giant services, remove raw diagnostic JSON from ordinary user views, replace browser dialogs |
| RoutArr | Dispatch/routes/trips/stops plus demand, tender, rating, visibility, capacity, yard/multimodal | Broad implementation and tests | Best-established | Replace browser confirms, validate new TMS expansion with end-to-end scenarios and performance tests |
| SupplyArr | Vendors/suppliers, item catalog, sourcing, purchasing, performance/compliance | Broadest API surface and strong tests | Best-established | Continue query/performance review and integration contract coverage |
| Compliance Core | Catalogs, rules, applicability, evidence, TSE/import, questionnaires | Substantial implementation and tests | Best-established | Keep platform-admin gate strict; convert raw JSON diagnostics into structured admin views where feasible |
| CustomArr | Customer system of record, contacts, locations, preferences/contracts, onboarding, portal handoff | Durable domain appears substantial, UI is concentrated and untested | Conditional | Zero frontend tests, monolithic App, prove customer tenant isolation and portal/order handoff E2E |
| LedgArr | Legal entities, GL, dimensions/periods, posting, AP/AR, valuation, assets/projects/budgets, ERP bridge | Tenant-scoped EF implementation is real, but large service/UI and thin tests | Conditional | Split store/App, expand posting/close/concurrency tests, reorganize navigation |
| AssurArr | Nonconformance, hold, RCA, CAPA, audits/findings/complaints, scorecards | Broad domain/API/UI, but anonymous and hard-coded tenant | Blocked | Authorization and tenant isolation must be rebuilt before feature acceptance |
| LoadArr | WMS receiving, putaway, balances, movement, reservation/pick/issue/transfer, counts/adjustments | Most operational records are fixtures; writes are often discarded; UI can fake success | Blocked | Implement durable inventory ledger/state machines, permissions, integrations, and honest error behavior |
| OrdArr | Order/request lifecycle, holds, handoffs, completion and finance packets | Process-local singleton orders with no tenant field | Blocked | Durable tenant model, permissions, database idempotency, workers/events, end-to-end fulfillment |
| RecordArr | Records/files, capture/scan/OCR, evidence mapping/packages/retention, controlled docs/access | Rich UI/contracts over global singleton fixture store; fail-open access paths | Blocked | Durable model, strict access, upload pipeline, immutable audit, legal hold/retention transactions |
| ReportArr | Datasets/read models, dashboards/widgets, reports/runs/schedules, KPIs/analytics/audit packages | Rich UI/contracts over global singleton lists | Blocked | Durable tenant model, connector ingestion, worker scheduling, output storage, access enforcement |
| Field Companion | Mobile task execution, secure capture, offline sync, product action surfaces | Good frontend test count and real handoff logic | Conditional | Correct person identity, safe token/offline storage, product-aware browser capability policy |
| Suite shell | Login, product launch, shared navigation/account/preferences | Strong shared surface and tests | Conditional | Browser session redesign, CSP, clean shared-package CI, cache clearing across tenant/session changes |

### Product-specific granular recommendations

#### AssurArr

- Do not patch only the route group. Every service query and write must become tenant-scoped.
- Add permissions by action family: view quality, manage nonconformance, manage CAPA, conduct audit, approve closure, administer quality settings, integration read/write.
- Make state transitions transactional and preserve immutable timeline/audit events.
- Add frontend tests for all critical transitions after backend safety is established.

#### LoadArr

- Model a durable stock ledger first; derive balances from ledger entries or maintain them transactionally with invariant tests.
- Persist expected receipts, receipt lines, putaway tasks, reservations, picks, transfers, counts, adjustments, holds, and exceptions.
- Use StaffArr location IDs and SupplyArr item IDs through owner APIs; snapshots are display/audit fields, not substitute ownership.
- Remove every `createLocal*` success fallback from production workflow paths.
- Treat inventory adjustment approval as permission-gated four-eyes control where tenant settings require it.

#### OrdArr

- Add order lifecycle state machine and reject invalid transitions server-side.
- Persist order events and product handoff correlation/idempotency.
- Keep CustomArr as customer truth; OrdArr stores references and snapshots.
- Make completion and financial packets durable contributions, not ad hoc response arrays.

#### RecordArr

- Store file metadata/version state in PostgreSQL and bytes in controlled object storage.
- Separate record identity, file versions, OCR/extraction jobs, metadata assertions, evidence mappings, controlled-document versions, retention, legal holds, and access grants.
- Purge must be impossible while any effective hold or retention prohibition applies.
- Access policy absence must deny, except for explicitly documented owner/admin defaults.
- Replace hard-coded StaffArr site options at `apps/recordarr-frontend/src/App.tsx:265-269` with live owner-backed reference search and quick create where allowed.

#### ReportArr

- Distinguish immutable report definition versions from individual runs.
- Persist schedules and execute them in a durable worker with leases and retry policy.
- Enforce row/column/source-product security when materializing read models, not only when rendering dashboards.
- Archive generated report evidence to RecordArr with content hash and lineage.

#### CustomArr

- Split the 2,349-line App before adding more CRM capability.
- Add tests for customer merge/deduplication, contact/location changes, portal visibility, and OrdArr handoff.
- Make quick-create customer/contact flows explicit owner-backed operations rather than local option injection.

#### LedgArr

- Split the 4,226-line store into posting, AP, AR, close, valuation, asset/project, budget, and bridge modules.
- Add invariant/concurrency tests for balanced journals, closed periods, duplicate packets, payment/application totals, and reversal rather than destructive edits.
- Keep legal entities distinct from Compliance Core governing bodies in API names and UI copy.

#### Mature products

StaffArr, TrainArr, MaintainArr, RoutArr, SupplyArr, and Compliance Core should not be rewritten. Apply focused hardening: common auth/CSP/session fixes, split the largest files, remove browser-native dialogs, improve route-level code splitting, and add live cross-product E2E coverage.

---

# 9. Navigation and information architecture

## NAV-001 — AssurArr navigation is too flat

`apps/assurarr-frontend/src/App.tsx:50-68` exposes roughly 17 primary navigation items separated mainly by section breaks.

**Recommended grouping**

- **Quality Work:** Dashboard, Nonconformances, Holds, CAPA, Complaints.
- **Audits:** Audit Program, Audits, Findings.
- **Quality Controls:** Inspections, Sampling, Supplier Quality, Scorecards.
- **Records and Analysis:** Reports, Trends, History.
- **Administration:** Settings, Integrations, Permissions.

The sidebar should show the groups, not every leaf at once.

## NAV-002 — LedgArr also has an overloaded flat sidebar

`apps/ledgarr-frontend/src/navigation/ledgarrNav.ts:19-37` contains roughly 17 top-level entries.

**Recommended grouping**

- **Finance Operations:** Dashboard, Journal, AP, AR, Payments.
- **Inventory and Assets:** Valuation, Fixed Assets, Projects.
- **Planning:** Budgets, Forecasts.
- **Close and Reporting:** Period Close, Reconciliation, Statements, Audit.
- **Administration:** Legal Entities, Chart of Accounts, Dimensions, Integrations, Settings, Permissions.

## NAV-003 — LoadArr grouping is structurally better, but “Work” is still overloaded

`apps/loadarr-frontend/src/App.tsx:877-949` uses parent groups, which is a good pattern. The Work group contains approximately 15 children, including dashboard, receiving, putaway, inventory, transfers, reservations, picking, staging, shipping, counts, exceptions, holds, and unexplained work.

**Recommended correction**

Split Work into:

- **Inbound:** Expected Receipts, Dock Schedule, Receiving, Putaway.
- **Inventory:** Inventory, Transfers, Reservations, Cycle Counts.
- **Outbound:** Picking, Staging, Shipping/Loadout.
- **Exceptions:** Exceptions, Holds, Unexplained.

Keep task counts/badges in parent labels so users can prioritize without expanding every group.

## NAV-004 — RecordArr has a stronger hierarchy, but labels overlap

`apps/recordarr-frontend/src/App.tsx:271-325` groups Controlled Documents, Packages, Retention, Holds, and Access. This is one of the better newer-app navigation structures.

Refine it by avoiding parent/child duplication such as “Packages” → “Record Packages” and “Holds” → “Legal Holds” when only one child exists. Either make the parent directly navigable or wait to introduce a group until it has multiple meaningful children.

## NAV-005 — Route alias accumulation needs governance

LoadArr maintains many legacy aliases in a large route table at `App.tsx:850-875`. Aliases are useful during migration, but permanent alias growth makes analytics, breadcrumbs, canonical links, authorization mapping, and tests harder.

**Correction**

Create a centralized redirect registry with owner, reason, introduced date, telemetry, and removal condition. Render canonical URLs after navigation.

---

# 10. User friendliness and operational trust

## UX-001 — Browser-native prompts and confirms are inconsistent

Examples:

- StaffArr role archive/clone uses `window.prompt` at `pages/roles/RolesPage.tsx:421, 435`.
- StaffArr account actions and recruiting use `window.confirm` in several files.
- RoutArr dispatch flows use browser confirms throughout dispatch components.
- MaintainArr and LedgArr settings use browser confirms.

**Problems**

- No consistent severity, destructive-action styling, help text, validation, or permission context.
- Poor control over keyboard/focus behavior and no inline API error placement.
- Prompts encourage unstructured text and cannot show dependent fields.

**Correction**

Use one shared confirmation/dialog system with:

- Action-specific title and consequence.
- Typed confirmation only for truly destructive actions.
- Structured reason field with controlled reason code plus optional note.
- Loading, error, retry, and correlation-ID states.
- Focus trapping and return-focus behavior.

## UX-002 — Some user-facing surfaces expose raw JSON

Examples include:

- LedgArr settings diff at `pages/settings/LedgArrSettingsPage.tsx:395`.
- MaintainArr external intelligence at `components/AssetExternalIntelligencePanel.tsx:347-366`.
- RecordArr data at `App.tsx:1979`.
- Compliance Core rule/test diagnostics.

Raw data can be appropriate for platform administrators and rule developers, but not as the primary explanation for ordinary end users.

**Correction**

Render labeled field changes, source/provenance, confidence, and human-readable explanations. Gate raw payloads behind an “Advanced technical details” disclosure and appropriate role.

## UX-003 — Internal identifiers leak into ordinary surfaces

Examples include Field Companion’s profile “Person ID” at `pages/ProfilePage.tsx:51` and LoadArr inventory/hold displays at `App.tsx:3216, 5105`.

**Correction**

Show names, codes, and contextual links. Keep internal IDs in platform-admin, permissions, diagnostics, and copy-link/debug affordances only.

## UX-004 — Cross-product references are not consistently live owner-backed data

RecordArr hard-codes StaffArr site options in `App.tsx:265-269`. LoadArr fixtures similarly stand in for StaffArr and SupplyArr data. This creates stale choices and gives the impression of integrated ownership without actual owner resolution.

**Correction**

Use shared searchable reference pickers backed by owner APIs, with snapshots for display/audit and quick create only when the owner exposes it and the caller has permission.

## UX-005 — Error handling does not consistently preserve user work

The shared UI contains a useful `ApiErrorCallout`, but giant app components implement ad hoc mutation states. LoadArr’s false-success behavior is the extreme case.

**Correction**

Every write surface should:

- Keep entered data on recoverable failure.
- Explain what was and was not saved.
- Show a correlation ID in expandable details.
- Offer safe retry only when idempotency is supported.
- Distinguish validation, permission, conflict, unavailable dependency, and unexpected errors.

## UX-006 — Automated accessibility enforcement is absent

No repository-wide axe/Playwright accessibility gate was found. The use of browser-native dialogs, very large tables/pages, and custom dark-only components increases the need for automated and manual checks.

**Correction**

Add:

- eslint accessibility rules.
- Component-level axe tests for shared primitives.
- Playwright keyboard/focus/landmark/name/contrast smoke tests for every product shell and critical workflow.
- Manual screen-reader checks for forms, tables, drawers, modal dialogs, and drag/drop scheduling.

---

# 11. UI consistency and theme audit

## UI-001 — The central theme exists but is not enforced

The intended design is present in `packages/shared-ui/src/theme.css` and shared components. The repository audit still reported:

- **31 forbidden light-only Tailwind class usages**.
- **40 raw color usages**.
- **71 total violations**.

Representative violations include:

- MaintainArr `AssetBulkImportPanel.tsx:182` and `ImportsSection.tsx:122` using `text-slate-950`.
- RecordArr numerous hard-coded colors in `src/index.css`, including lines `358-370`, `615`, `828-954`, and later sections.
- ReportArr raw/light-only colors around `App.tsx:2138-2608`.
- RecordArr’s `SectionHeader` at `App.tsx:347-366` is explicitly dark (`bg-slate-950`, `text-slate-50`, cyan accents), making light mode dependent on overrides rather than semantic tokens.

**Required correction**

- Treat `npm run audit:theme` as a mandatory CI gate for every frontend and shared UI.
- Replace raw palette classes with semantic surface/text/border/action/status tokens.
- Permit brand colors only through documented audit annotations.
- Add Storybook or a visual fixture route that renders every shared component in light/dark, normal/hover/focus/disabled/error states.
- Add screenshot comparisons for product shell, tables, forms, drawers, modals, empty/loading/error states, and print preview.

## UI-002 — Shared shell adoption is a real strength

`ProductWorkspaceFrame`, `ProductSwitcher`, account/preferences components, page headers, form primitives, print components, scheduling board, error callouts, and quick-create/reference primitives are tested in shared UI. The shared test run reported 112 passing tests.

The correct strategy is to strengthen this shared system, not replace it with product-local shell implementations.

## UI-003 — Product-local CSS is recreating a second design system

RecordArr’s large local stylesheet and the giant newer-app `App.tsx` files introduce bespoke card, panel, status, and typography styling. This creates a visual fork from mature products.

**Correction**

Promote genuinely reusable patterns into shared UI. Keep product-specific CSS limited to domain visualization needs, not basic cards/forms/tables/headers.

## UI-004 — Loading, empty, disabled, and error states need one contract

Several newer apps define local `LoadingCard`, `EmptyState`, and status treatments. These should share wording, spacing, icons, retry behavior, and accessibility semantics.

**Correction**

Create shared `PageState`, `SectionState`, `InlineError`, `PermissionDenied`, `DependencyUnavailable`, and `NoResults` patterns. Product code supplies domain text/actions only.

---

# 12. Frontend performance and composition

## PERF-001 — StaffArr’s main bundle is oversized and intended lazy loading is neutralized

A representative production build succeeded but produced:

- Main JavaScript: **816.92 kB minified / 195.41 kB gzip**.
- Vite warning for chunks larger than 500 kB.
- Ineffective dynamic imports for `ReportsSection`, `AdminSection`, and `EmploymentApplicationsPage` because those modules are also statically imported elsewhere.

**Correction**

- Define route modules once and lazy-load from all route entry points.
- Avoid barrel exports that eagerly pull page modules into the root graph.
- Create explicit vendor chunks only after route boundaries are fixed.
- Add per-app bundle budgets and fail on regressions.

## PERF-002 — Giant newer-app files prevent meaningful route splitting

AssurArr, LoadArr, RecordArr, ReportArr, and LedgArr place most routes in one App module. Even if individual components are added later, top-level imports and shared state can retain the entire application in the initial chunk.

**Correction**

Split routes and query state by feature before attempting bundler tuning.

---

# 13. Test distribution

## Frontend test distribution

| Product | Frontend test files | App.tsx size | Interpretation |
|---|---:|---:|---|
| Suite | 53 | 25 | Strong shared/platform shell discipline |
| SupplyArr | 51 | 84 | Strong |
| TrainArr | 47 | 77 | Strong |
| Compliance Core | 45 | 105 | Strong |
| StaffArr | 44 | 100 | Strong, despite bundle size |
| MaintainArr | 43 | 163 | Strong |
| RoutArr | 36 | 77 | Strong |
| Field Companion | 23 | 47 | Good |
| LoadArr | 3 | 6,745 | Critically insufficient for claimed surface |
| LedgArr | 2 | 5,150 | Critically thin for financial workflows |
| OrdArr | 1 | 1,152 | Critically thin |
| AssurArr | 0 | 7,886 | No frontend regression confidence |
| CustomArr | 0 | 2,349 | No frontend regression confidence |
| RecordArr | 0 | 6,085 | No frontend regression confidence |
| ReportArr | 0 | 8,620 | No frontend regression confidence |

### Required test baseline by workflow risk

Every primary record should have tests for:

- List filtering/sorting/pagination and empty/error/loading states.
- Create validation, owner-backed reference selection, quick create, idempotent retry.
- Detail and drawer route behavior.
- Edit concurrency/conflict handling.
- Archive/restore or lifecycle transition permission gates.
- Tenant and permission isolation at API level.
- Print/report output.
- Cross-product handoff failure and retry.
- Light/dark rendering and keyboard operation.

Financial, inventory, records, identity, and quality state transitions need server-side invariant and concurrency tests in addition to UI tests.

---

# 14. Prioritized remediation backlog

## Gate A — Before any production release

1. Disable or isolate all unsafe AssurArr, LoadArr, OrdArr, RecordArr, and ReportArr domain routes.
2. Add application-wide deny-by-default authentication and per-product entitlement enforcement.
3. Remove hard-coded tenant IDs and process-global tenant data.
4. Fix the deterministic CI and nightly workflow failures.
5. Make every frontend build from a clean workspace and include every product/shared package in CI.
6. Remove LoadArr’s local-success fallbacks.
7. Add cross-tenant negative tests for all blocked products.

## Gate B — Security hardening

1. Redesign browser sessions to avoid JavaScript-readable refresh tokens.
2. Add SPA CSP/frame-ancestor protection and product-aware Permissions Policy.
3. Encrypt MFA secrets and make refresh rotation atomic.
4. Implement real password-reset delivery.
5. Correct Field Companion person identity.
6. Add trusted forwarded-header configuration and deployment-level HTTPS/HSTS assertions.
7. Stream, limit, quarantine, and scan RecordArr uploads.
8. Derive all actor identities from authenticated context.
9. Add a final safe exception envelope.

## Gate C — Feature truthfulness and durability

1. Implement durable tenant-owned models for LoadArr, OrdArr, RecordArr, and ReportArr.
2. Add transactional state machines, database idempotency, concurrency control, and outbox processing.
3. Prove restart and multi-replica consistency.
4. Replace fixtures/hard-coded cross-product references with owner-backed APIs.
5. Clearly separate demo fixtures from production code and builds.

## Gate D — UX/UI and maintainability

1. Reduce theme violations to zero and enforce the audit in CI.
2. Split giant App and backend service files along domain boundaries.
3. Replace browser prompts/confirms/alerts with shared accessible dialogs.
4. Remove ordinary-user raw IDs and JSON.
5. Reorganize AssurArr, LedgArr, and LoadArr navigation.
6. Add bundle budgets and fix ineffective lazy imports.
7. Add accessibility and visual-regression gates.

---

# 15. Definition of done for a production-capable product

A product is not production-capable until all of the following are true:

- Every tenant-owned record is durably stored with an explicit tenant key.
- Every query and mutation is tenant-scoped and fail-closed.
- Authentication, entitlement, and permission checks are tested separately.
- Actor identity comes from claims/service identity, never ordinary request input.
- State transitions and invariants are server enforced and transactionally safe.
- Idempotency survives restart and replica changes.
- The product builds and tests from a clean checkout through CI.
- No test job passes solely because zero tests were found.
- Critical list/create/detail/edit/archive/print/handoff flows have automated tests.
- Errors never masquerade as success and preserve user work where safe.
- Light and dark mode pass the theme audit and visual smoke tests.
- Keyboard/focus/accessibility checks cover the product shell and critical forms.
- Restart, migration, rollback, backup/restore, and multi-tenant negative tests pass.

---

# 16. Positive findings worth preserving

- The product ownership constitutions and product manifests are unusually comprehensive and provide a strong basis for automated architecture checks.
- Mature apps consistently use owner references and tenant-scoped persistence more effectively than the newer prototypes.
- The shared UI package already contains the correct central abstractions: shell, switcher, account menu, page header, forms, quick create, scheduling, printing, and error presentation.
- NexArr’s handoff redemption validates target product, one-time use, expiration, tenant/user status, and entitlement before issuing Field Companion credentials (`FieldCompanionAuthService.cs:29-74`).
- Production JWT signing keys are rejected when missing/short in `StlApiHost.cs:77-84`.
- Refresh and reset tokens are stored as hashes rather than plaintext.
- RecordArr’s document-storage path handling includes filename/path containment protections; preserve that implementation while replacing the unsafe upload and global metadata store.
- LedgArr’s actual store is EF-backed and tenant-predicated, for example `LedgArrStore.cs:25-64`; it should be refactored, not replaced with a prototype rewrite.
- The mature frontend test distribution proves that the suite can sustain modular, tested products when the same standard is applied consistently.

---

# 17. Verification log

| Verification | Result |
|---|---|
| Repository structure and documentation mapping | Completed |
| Static endpoint/auth/tenant review | Completed across all products, with deep inspection of high-risk/newer products |
| Theme audit | Failed: 71 violations (31 forbidden light-only classes, 40 raw colors) |
| Shared UI tests | 112/112 reported passed; process did not terminate normally |
| Representative StaffArr production build | Passed; 816.92 kB main chunk and ineffective dynamic-import warnings |
| Representative AssurArr clean build | Initially failed because shared UI dependencies were not installed; passed after sibling shared UI installation during targeted verification |
| Representative newer-app frontend tests | AssurArr passed with zero tests because of `--passWithNoTests`; LoadArr and OrdArr had only very small suites |
| Production dependency audit, representative frontend | Zero production findings with `--omit=dev` |
| Full dependency audit, representative frontend | Two high-severity dev/build-tool findings (Vite and transitive undici) |
| .NET backend build/test | Not executed; .NET 10 SDK unavailable and network SDK retrieval failed |
| Live penetration/load/accessibility test | Not performed |

---

## Final assessment

STLComplianceV2 should be treated as a strong platform foundation plus an uneven set of product implementations—not as one uniformly production-ready suite. The highest-value move is not another broad feature expansion. It is to enforce one minimum product standard and bring AssurArr, LoadArr, OrdArr, RecordArr, and ReportArr up to the tenant isolation, persistence, authorization, testing, and user-trust level already demonstrated by the mature applications.
