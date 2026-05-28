# Worker 231 — Compliance Core M12 scheduled evaluation / audit delivery workers

Builds on **W222–225** (risk scoring, missing evidence, control effectiveness, readiness forecasting). Wires **shared-worker** periodic batches per tenant settings with optional audit package delivery hook.

## Scope

- **Tenant settings** — `compliancecore_tenant_m12_analytics_worker_settings` (master enable, interval hours, default scope, per-step toggles, audit delivery hook, last-run timestamps)
- **Batch runs** — `compliancecore_m12_analytics_batch_runs` audit rows per worker execution
- **Internal APIs** — `GET /api/internal/m12-analytics-batches/pending`, `POST /process-batch` (scope `compliancecore.m12_analytics.process_batch`)
- **User APIs** — `GET`/`PUT /api/m12-analytics-worker-settings` (tenant admin, compliance admin)
- **shared-worker** — `ComplianceCoreM12AnalyticsBatchJob` + client (default 60 min scan, 24 h tenant interval)
- **compliancecore-frontend** — `M12AnalyticsWorkerSettingsPanel` on Admin workspace
- **Tests** — `ComplianceCoreM12AnalyticsBatchWorkerTests`, Vitest panel test

## Batch behavior

When a tenant is enabled and a step is due (per `IntervalHours` and last-run timestamp):

| Step enabled | Action |
|--------------|--------|
| Readiness forecast (due) | Runs `ReadinessForecastService.EvaluateAsync` (includes risk, missing evidence, control effectiveness) |
| Otherwise | Runs individual enabled steps: risk scoring, missing evidence, control effectiveness |
| Audit delivery (due) | Enqueues `AuditPackageGenerationJob` (ZIP); existing `ComplianceCoreAuditPackageGenerationJob` processes artifact |

## shared-worker configuration

`ComplianceCoreM12AnalyticsBatch` section:

| Key | Default | Purpose |
|-----|---------|---------|
| `Enabled` | `true` | Toggle job |
| `ComplianceCoreBaseUrl` | `http://localhost:5107` | API base |
| `ServiceToken` | `""` | Bearer for internal batch API |
| `ScanIntervalMinutes` | `60` | Worker poll interval |
| `BatchSize` | `25` | Max tenants per batch |
| `IntervalHours` | `24` | Default due interval when tenant settings omit override |
| `TenantId` | `null` | Optional single-tenant filter |

Render env: `ComplianceCoreM12AnalyticsBatch__ComplianceCoreBaseUrl`, token profile `worker-compliancecore-m12-analytics` → `compliancecore.m12_analytics.process_batch`.

## Migration

`ComplianceCoreM12AnalyticsBatchWorkers` — settings + batch run tables.

## Verification

```powershell
dotnet test tests/STLCompliance.ComplianceCore.Auth.Tests/STLCompliance.ComplianceCore.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~M12Analytics"
cd apps/compliancecore-frontend
npm test -- M12AnalyticsWorkerSettingsPanel
```

## Out of scope

- Replacing W47 scheduled **rule pack** evaluation (`compliancecore.rules.evaluate.scheduled`)
- W221 rule change monitor job (separate schedule)
- Email/webhook audit delivery (job enqueue only)
