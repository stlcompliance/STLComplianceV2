# V1 Environment Variables (Render)

Reference for `render.yaml` groups and Dashboard secrets. .NET APIs use `Section__Key` (double underscore) unless noted.

## Groups

| Group | Purpose |
|-------|---------|
| `stl-shared` | `ASPNETCORE_ENVIRONMENT`, `LOG_LEVEL`, `OTEL_ENABLED`, `OTEL_SERVICE_NAME`, `OTEL_EXPORTER_OTLP_ENDPOINT` |
| `stl-auth` | JWT + service-token signing (`AUTH_SIGNING_KEY`, `Auth__*`, `SERVICE_TOKEN_*`) |
| `stl-internal-api-urls` | Server-to-server API base URLs for deployed Render services |
| `stl-public-frontend-urls` | Documented public static-site URLs (onrender.com defaults) |
| `stl-vite-product-frontend-urls` | Vite build-time product frontend launch bases for ProductSwitcher |
| `stl-public-api-urls` | Documented public API URLs for Vite build-time variables |

## Health checks

| Service type | Path |
|--------------|------|
| All Docker APIs | `GET /health` (liveness), `GET /health/ready` (DB readiness — Blueprint `healthCheckPath`), `GET /health/observability` (OTEL wiring status) |
| Workers | Process heartbeat only (no HTTP health endpoint) |

## Static site security headers

All static sites in `render.yaml` set:

| Header | Value |
|--------|-------|
| `X-Content-Type-Options` | `nosniff` |
| `X-Frame-Options` | `DENY` |
| `Referrer-Policy` | `strict-origin-when-cross-origin` |
| `Permissions-Policy` | `camera=(), microphone=(), geolocation=()` |

Sites: `stlcompliancesite`, `suite-frontend`, six Arr product frontends, `companion-frontend`.

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

## Cross-product API URLs (`stl-internal-api-urls`)

Attached to APIs that call other products. These values use the deployed Render HTTPS hosts so services do not depend on Docker-style service names resolving inside Render.

| Variable | Target |
|----------|--------|
| `NexArr__BaseUrl` | `https://nexarr-api-3zlb.onrender.com` |
| `StaffArr__BaseUrl` | `https://staffarr-api-58w6.onrender.com` |
| `TrainArr__BaseUrl` | `https://trainarr-api-ieni.onrender.com` |
| `MaintainArr__BaseUrl` | `https://maintainarr-api-gx03.onrender.com` |
| `RoutArr__BaseUrl` | `https://routarr-api-nmwr.onrender.com` |
| `SupplyArr__BaseUrl` | `https://supplyarr-api-gavo.onrender.com` |
| `ComplianceCore__BaseUrl` | `https://compliancecore-api-h69n.onrender.com` |

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
| `TrainArrQualificationExpiration__TrainArrBaseUrl` | `https://trainarr-api-ieni.onrender.com` |
| `TrainArrQualificationExpiration__ServiceToken` | scope `trainarr.qualifications.expire` |
| `TrainArrRecertificationAssignment__TrainArrBaseUrl` | same TrainArr host |
| `TrainArrRecertificationAssignment__ServiceToken` | scope `trainarr.recertification.assign` |
| `TrainArrQualificationRecalculation__TrainArrBaseUrl` | same TrainArr host |
| `TrainArrQualificationRecalculation__ServiceToken` | scope `trainarr.qualifications.recalculate` |
| `TrainArrStaffarrPublicationRetry__TrainArrBaseUrl` | same TrainArr host |
| `TrainArrStaffarrPublicationRetry__ServiceToken` | scope `trainarr.staffarr_publications.retry` |
| `TrainArrEventProcessing__TrainArrBaseUrl` | same TrainArr host |
| `TrainArrEventProcessing__ServiceToken` | scope `trainarr.events.process` |
| `StaffArrCertificationExpiration__StaffArrBaseUrl` | `https://staffarr-api-58w6.onrender.com` |
| `StaffArrCertificationExpiration__ServiceToken` | scope `staffarr.certifications.expire` |
| `StaffArrReadinessRollup__StaffArrBaseUrl` | same StaffArr host |
| `StaffArrReadinessRollup__ServiceToken` | scope `staffarr.readiness.rollup` |
| `StaffArrPermissionProjection__StaffArrBaseUrl` | same StaffArr host |
| `StaffArrPermissionProjection__ServiceToken` | scope `staffarr.permissions.project` |
| `MaintainArrPmDueScan__MaintainArrBaseUrl` | `https://maintainarr-api-gx03.onrender.com` |
| `MaintainArrPmDueScan__ServiceToken` | scope `maintainarr.pm.scan` |
| `MaintainArrAssetStatusRollup__MaintainArrBaseUrl` | `https://maintainarr-api-gx03.onrender.com` |
| `MaintainArrAssetStatusRollup__ServiceToken` | scope `maintainarr.asset_status.rollup` |
| `MaintainArrMaintenanceHistoryRollup__MaintainArrBaseUrl` | `https://maintainarr-api-gx03.onrender.com` |
| `MaintainArrMaintenanceHistoryRollup__ServiceToken` | scope `maintainarr.maintenance_history.rollup` |
| `MaintainArrDowntimeSync__MaintainArrBaseUrl` | `https://maintainarr-api-gx03.onrender.com` |
| `MaintainArrDowntimeSync__ServiceToken` | scope `maintainarr.downtime.sync` |
| `MaintainArrPlatformEventProcessing__MaintainArrBaseUrl` | `https://maintainarr-api-gx03.onrender.com` |
| `MaintainArrPlatformEventProcessing__ServiceToken` | scope `maintainarr.platform_events.process` |
| `MaintainArrDefectEscalation__MaintainArrBaseUrl` | `https://maintainarr-api-gx03.onrender.com` |
| `MaintainArrDefectEscalation__ServiceToken` | scope `maintainarr.defects.escalate` |
| `MaintainArrTechnicianRefRefresh__MaintainArrBaseUrl` | `https://maintainarr-api-gx03.onrender.com` |
| `MaintainArrTechnicianRefRefresh__ServiceToken` | scope `maintainarr.technician_refs.refresh` |
| `RoutArrTripCompletionRollup__RoutArrBaseUrl` | `https://routarr-api-nmwr.onrender.com` |
| `RoutArrTripCompletionRollup__ServiceToken` | scope `routarr.trips.completion.rollup` |
| `RoutArrIntegrationEvents__RoutArrBaseUrl` | `https://routarr-api-nmwr.onrender.com` |
| `RoutArrIntegrationEvents__ServiceToken` | scope `routarr.integration.events.process` |
| `TrainArrOrphanReference__TrainArrBaseUrl` | `https://trainarr-api-ieni.onrender.com` |
| `TrainArrOrphanReference__ServiceToken` | scope `trainarr.orphan_references.scan` |
| `SupplyArrReorderEvaluation__SupplyArrBaseUrl` | `https://supplyarr-api-gavo.onrender.com` |
| `SupplyArrReorderEvaluation__ServiceToken` | scope `supplyarr.reorder.evaluate` |
| `SupplyArrProcurementCoordination__SupplyArrBaseUrl` | `https://supplyarr-api-gavo.onrender.com` |
| `SupplyArrProcurementCoordination__ServiceToken` | scope `supplyarr.procurement.coordination` |
| `SupplyArrApprovalReminders__SupplyArrBaseUrl` | `https://supplyarr-api-gavo.onrender.com` |
| `SupplyArrApprovalReminders__ServiceToken` | scope `supplyarr.approval_reminders.dispatch` |
| `SupplyArrDemandProcessing__SupplyArrBaseUrl` | `https://supplyarr-api-gavo.onrender.com` |
| `SupplyArrDemandProcessing__ServiceToken` | scope `supplyarr.demand.process` |
| `ComplianceCoreScheduledEvaluation__ComplianceCoreBaseUrl` | `https://compliancecore-api-h69n.onrender.com` |
| `ComplianceCoreScheduledEvaluation__ServiceToken` | scope `compliancecore.rules.evaluate.scheduled` |
| `ComplianceCoreRuleChangeMonitor__ComplianceCoreBaseUrl` | `https://compliancecore-api-h69n.onrender.com` |
| `ComplianceCoreRuleChangeMonitor__ServiceToken` | scope `compliancecore.rule_changes.monitor` |
| `ComplianceCoreWaiverExpiration__ComplianceCoreBaseUrl` | `https://compliancecore-api-h69n.onrender.com` |
| `ComplianceCoreWaiverExpiration__ServiceToken` | scope `compliancecore.waivers.expire_batch` |
| `ComplianceCoreFactSourceSync__ComplianceCoreBaseUrl` | `https://compliancecore-api-h69n.onrender.com` |
| `ComplianceCoreFactSourceSync__ServiceToken` | scope `compliancecore.fact_sources.sync` |
| `ComplianceCoreM12AnalyticsBatch__ServiceToken` | scope `compliancecore.m12_analytics.process_batch` |
| `ComplianceCoreAuditPackageGeneration__ServiceToken` | scope `compliancecore.audit_packages.generate` |
| `StaffArrPersonExportDelivery__ServiceToken` | scope `staffarr.people.export.scheduled` |
| `StaffArrPersonnelHistoryRollup__ServiceToken` | scope `staffarr.personnel.history.rollup` |
| `SupplyArrLeadTimeSnapshot__ServiceToken` | scope `supplyarr.leadtime.snapshots.capture` |
| `SupplyArrAvailabilitySnapshot__ServiceToken` | scope `supplyarr.availability.snapshots.capture` |
| `SupplyArrProcurementExceptionEscalations__ServiceToken` | scope `supplyarr.procurement_exceptions.escalate` |
| `SupplyArrIntegrationEvents__SupplyArrBaseUrl` | `https://supplyarr-api-gavo.onrender.com` |
| `SupplyArrIntegrationEvents__ServiceToken` | scope `supplyarr.integration.events.process` |
| `NexArrServiceTokenCleanup__NexArrBaseUrl` | `https://nexarr-api-3zlb.onrender.com` |
| `NexArrServiceTokenCleanup__ServiceToken` | scope `nexarr.service_tokens.cleanup.purge` |
| `NexArrEntitlementReconciliation__NexArrBaseUrl` | `https://nexarr-api-3zlb.onrender.com` |
| `NexArrEntitlementReconciliation__ServiceToken` | scope `nexarr.entitlements.reconcile` |
| `NexArrTenantLifecycle__NexArrBaseUrl` | `https://nexarr-api-3zlb.onrender.com` |
| `NexArrTenantLifecycle__ServiceToken` | scope `nexarr.tenants.lifecycle.process` |
| `NexArrPlatformOutboxPublisher__NexArrBaseUrl` | `https://nexarr-api-3zlb.onrender.com` |
| `NexArrPlatformOutboxPublisher__ServiceToken` | scope `nexarr.platform_outbox.publish` |

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

## Companion Web Push (`nexarr-api`)

Browser push delivery for Companion operational notifications (handoff redeemed, field inbox refreshed). Set in Render Dashboard (`sync: false` in Blueprint).

| Variable | Notes |
|----------|-------|
| `CompanionWebPush__Subject` | VAPID subject — typically `mailto:…@yourdomain.com` |
| `CompanionWebPush__PublicKey` | VAPID public key (base64url); exposed via `GET /api/companion/push/vapid-public-key` |
| `CompanionWebPush__PrivateKey` | VAPID private key — never expose to clients |

When unset, subscribe APIs return `503 companion.push.vapid_unavailable` and dispatch falls back to webhook-only delivery.

## Static site build (Vite)

| Static site | Build variable | Public API |
|-------------|----------------|------------|
| `suite-frontend` | `VITE_NEXARR_API_URL` | `https://nexarr-api-3zlb.onrender.com` |
| `staffarr-frontend` | `VITE_STAFFARR_API_BASE` | `https://staffarr-api-58w6.onrender.com` |
| `trainarr-frontend` | `VITE_TRAINARR_API_BASE` | `https://trainarr-api-ieni.onrender.com` |
| `maintainarr-frontend` | `VITE_MAINTAINARR_API_BASE` | `https://maintainarr-api-gx03.onrender.com` |
| `routarr-frontend` | `VITE_ROUTARR_API_BASE` | `https://routarr-api-nmwr.onrender.com` |
| `supplyarr-frontend` | `VITE_SUPPLYARR_API_BASE` | `https://supplyarr-api-gavo.onrender.com` |
| `compliancecore-frontend` | `VITE_COMPLIANCECORE_API_BASE` | `https://compliancecore-api-h69n.onrender.com` |

Static sites cannot use private network hostnames; always use public HTTPS URLs (or custom domains — update Blueprint values after DNS cutover).

## Evidence storage

| API | Variable | Render note |
|-----|----------|-------------|
| `trainarr-api` | `EvidenceStorage__RootPath` | `/var/data/trainarr-evidence` — **10 GB persistent disk** attached in Blueprint |
| `maintainarr-api` | `EvidenceStorage__RootPath` | `/var/data/maintainarr-evidence` — **10 GB persistent disk** attached in Blueprint |

## Not in V1 Blueprint

- **stlcompliancesite** (`apps/stlcompliancesite`, port 5173) — static marketing SPA; `VITE_SUITE_LOGIN_URL`, `VITE_CONTACT_EMAIL`, `VITE_SITE_BASE_URL` (canonical/OG URLs and build-time `sitemap.xml` / `robots.txt`); no product APIs.
- **Companion app** — separate mobile slice (Worker 90+).

## Blueprint validation

```bash
./scripts/ops/render-blueprint-validate.sh
```

```powershell
./scripts/ops/render-blueprint-validate.ps1
```

Automated catalog gate (CI): `dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "Category=Ci&Area=RenderBlueprint"`.

Optional Render CLI (v2.7.0+):

```bash
render blueprints validate render.yaml
```

## Staging ship gate validation

After deploying to Render staging, operators validate live URLs with:

```powershell
./scripts/ops/render-staging-ship-gate-validate.ps1 -Phase local-catalog
# With RENDER_STAGING_*_API_URL exported:
./scripts/ops/render-staging-ship-gate-validate.ps1 -Phase api-health
```

See `docs/operations/RENDER_STAGING_SHIP_GATE_V1.md` for required/optional environment variables, GitHub workflow **Ship Gate Staging Render**, and canonical test filters (`Category=Ci&Area=RenderStagingShipGate`, `Category=Live&Area=RenderStagingShipGate`).
