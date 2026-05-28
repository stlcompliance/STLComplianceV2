# V1 Environment Variables (Render)

Reference for `render.yaml` groups and Dashboard secrets. .NET APIs use `Section__Key` (double underscore) unless noted.

## Groups

| Group | Purpose |
|-------|---------|
| `stl-shared` | `ASPNETCORE_ENVIRONMENT`, `LOG_LEVEL`, `OTEL_ENABLED`, `OTEL_SERVICE_NAME`, `OTEL_EXPORTER_OTLP_ENDPOINT` |
| `stl-auth` | JWT + service-token signing (`AUTH_SIGNING_KEY`, `Auth__*`, `SERVICE_TOKEN_*`) |
| `stl-internal-api-urls` | Private-network API base URLs (`http://{service}:10000`) for server-to-server calls |
| `stl-public-frontend-urls` | Documented public static-site URLs (onrender.com defaults) |
| `stl-public-api-urls` | Documented public API URLs for Vite build-time variables |

## Health checks

| Service type | Path |
|--------------|------|
| All Docker APIs | `GET /health` (liveness), `GET /health/ready` (DB readiness — Blueprint `healthCheckPath`), `GET /health/observability` (OTEL wiring status) |
| Workers | Process heartbeat only (no HTTP health endpoint) |

## OpenTelemetry (`stl-shared`)

| Variable | Default | Notes |
|----------|---------|-------|
| `OTEL_ENABLED` | `false` | When `true`, APIs and workers register ASP.NET Core / HTTP / runtime instrumentation and export metrics + traces |
| `OTEL_SERVICE_NAME` | product key (e.g. `nexarr`, `shared-worker`) | Override per service in Render Dashboard when needed |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | unset | When set (e.g. `http://otel-collector:4317`), export via OTLP; otherwise console exporter in Development/Testing only |

Operational smoke script (local docker-compose): `scripts/ops/otel-smoke.ps1`. Automated smoke tests: `dotnet test tests/STLCompliance.Otel.Tests --filter Category=Otel`.

## Auth (all APIs)

| Variable | Source | Notes |
|----------|--------|-------|
| `AUTH_SIGNING_KEY` | `generateValue` in `stl-auth` | Min 32 chars; user JWT validation |
| `Auth__Issuer` | `stl-compliance-nexarr` | |
| `Auth__Audience` | `stl-compliance-suite` | |
| `SERVICE_TOKEN_ISSUER` | `stl-compliance-services` | Cross-product bearer tokens |
| `SERVICE_TOKEN_AUDIENCE` | `stl-compliance-services` | |
| `SERVICE_TOKEN_SIGNING_KEY` | Dashboard (optional) | Falls back to `AUTH_SIGNING_KEY` when unset |

Aliases supported in code: `JWT_SIGNING_KEY`, `JWT_ISSUER`, `JWT_AUDIENCE`.

## Database

| Variable | Source |
|----------|--------|
| `DATABASE_URL` | `fromDatabase` per product DB |

## Redis

| Variable | Service |
|----------|---------|
| `REDIS_URL` | `nexarr-api` only (Key Value `redis`) |

## Internal cross-product URLs (`stl-internal-api-urls`)

Attached to APIs that call other products. Render private network port **10000**.

| Variable | Target |
|----------|--------|
| `NexArr__BaseUrl` | `http://nexarr-api:10000` |
| `StaffArr__BaseUrl` | `http://staffarr-api:10000` |
| `TrainArr__BaseUrl` | `http://trainarr-api:10000` |
| `MaintainArr__BaseUrl` | `http://maintainarr-api:10000` |
| `RoutArr__BaseUrl` | `http://routarr-api:10000` |
| `SupplyArr__BaseUrl` | `http://supplyarr-api:10000` |
| `ComplianceCore__BaseUrl` | `http://compliancecore-api:10000` |

## Integration service tokens (`sync: false`)

Issue tokens in NexArr (platform admin / service clients) with the scopes documented in worker slices, then set in each target API Dashboard:

| API | Variables | Typical scopes |
|-----|-----------|----------------|
| `staffarr-api` | `Handoff__ServiceToken`, `TrainArr__ServiceToken` | handoff redeem; TrainArr → StaffArr |
| `trainarr-api` | `Handoff__ServiceToken`, `StaffArr__ServiceToken`, `ComplianceCore__ServiceToken` | handoff; StaffArr ingest; Compliance Core evaluate |
| `maintainarr-api` | `Handoff__ServiceToken`, `SupplyArr__ServiceToken` | handoff; SupplyArr demand |
| `supplyarr-api` | `Handoff__ServiceToken`, `MaintainArr__ServiceToken` | handoff; MaintainArr callbacks |
| `routarr-api` | `Handoff__ServiceToken`, `TrainArr__ServiceToken`, `StaffArr__ServiceToken`, `MaintainArr__ServiceToken`, `ComplianceCore__ServiceToken` | handoff; eligibility; asset readiness; workflow gates |
| `compliancecore-api` | `Handoff__ServiceToken` | handoff |

## `shared-worker` jobs

| Variable | Purpose |
|----------|---------|
| `TrainArrQualificationExpiration__TrainArrBaseUrl` | `http://trainarr-api:10000` |
| `TrainArrQualificationExpiration__ServiceToken` | scope `trainarr.qualifications.expire` |
| `StaffArrCertificationExpiration__StaffArrBaseUrl` | `http://staffarr-api:10000` |
| `StaffArrCertificationExpiration__ServiceToken` | scope `staffarr.certifications.expire` |
| `StaffArrReadinessRollup__StaffArrBaseUrl` | same StaffArr host |
| `StaffArrReadinessRollup__ServiceToken` | scope `staffarr.readiness.rollup` |
| `StaffArrPermissionProjection__StaffArrBaseUrl` | same StaffArr host |
| `StaffArrPermissionProjection__ServiceToken` | scope `staffarr.permissions.project` |
| `MaintainArrPmDueScan__MaintainArrBaseUrl` | `http://maintainarr-api:10000` |
| `MaintainArrPmDueScan__ServiceToken` | scope `maintainarr.pm.scan` |
| `SupplyArrReorderEvaluation__SupplyArrBaseUrl` | `http://supplyarr-api:10000` |
| `SupplyArrReorderEvaluation__ServiceToken` | scope `supplyarr.reorder.evaluate` |
| `ComplianceCoreScheduledEvaluation__ComplianceCoreBaseUrl` | `http://compliancecore-api:10000` |
| `ComplianceCoreScheduledEvaluation__ServiceToken` | scope `compliancecore.rules.evaluate.scheduled` |

## CORS (product APIs)

| API | Variable | Default public origin |
|-----|----------|------------------------|
| `staffarr-api` | `Cors__StaffArrFrontendOrigin` | `https://staffarr-frontend.onrender.com` |
| `trainarr-api` | `Cors__TrainArrFrontendOrigin` | `https://trainarr-frontend.onrender.com` |
| `maintainarr-api` | `Cors__MaintainArrFrontendOrigin` | `https://maintainarr-frontend.onrender.com` |
| `routarr-api` | `Cors__RoutArrFrontendOrigin` | `https://routarr-frontend.onrender.com` |
| `supplyarr-api` | `Cors__SupplyArrFrontendOrigin` | `https://supplyarr-frontend.onrender.com` |
| `compliancecore-api` | `Cors__ComplianceCoreFrontendOrigin` | `https://compliancecore-frontend.onrender.com` |

## NexArr launch URLs

`Launch__Products__{product}__BaseUrl` and `Launch__Products__{product}__LaunchPath` on `nexarr-api` — public frontend URLs for handoff redirects.

## Static site build (Vite)

| Static site | Build variable | Public API |
|-------------|----------------|------------|
| `suite-frontend` | `VITE_NEXARR_API_URL` | `https://nexarr-api.onrender.com` |
| `staffarr-frontend` | `VITE_STAFFARR_API_BASE` | `https://staffarr-api.onrender.com` |
| `trainarr-frontend` | `VITE_TRAINARR_API_BASE` | `https://trainarr-api.onrender.com` |
| `maintainarr-frontend` | `VITE_MAINTAINARR_API_BASE` | `https://maintainarr-api.onrender.com` |
| `routarr-frontend` | `VITE_ROUTARR_API_BASE` | `https://routarr-api.onrender.com` |
| `supplyarr-frontend` | `VITE_SUPPLYARR_API_BASE` | `https://supplyarr-api.onrender.com` |
| `compliancecore-frontend` | `VITE_COMPLIANCECORE_API_BASE` | `https://compliancecore-api.onrender.com` |

Static sites cannot use private network hostnames; always use public HTTPS URLs (or custom domains — update Blueprint values after DNS cutover).

## Evidence storage

| API | Variable | Render note |
|-----|----------|-------------|
| `trainarr-api` | `EvidenceStorage__RootPath` | `/var/data/trainarr-evidence` — ephemeral unless a persistent disk is attached |
| `maintainarr-api` | `EvidenceStorage__RootPath` | `/var/data/maintainarr-evidence` — same |

## Not in V1 Blueprint

- **stlcompliancesite** (`apps/stlcompliancesite`, port 5173) — static marketing SPA; `VITE_SUITE_LOGIN_URL`, `VITE_CONTACT_EMAIL`; no product APIs.
- **Companion app** — separate mobile slice (Worker 90+).

## Blueprint validation

```bash
render blueprints validate render.yaml
```

Requires [Render CLI](https://render.com/docs/cli) v2.7.0+. JSON Schema: `https://render.com/schema/render.yaml.json`
