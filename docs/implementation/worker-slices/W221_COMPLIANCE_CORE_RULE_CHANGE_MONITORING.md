# Worker 221 — Compliance Core M12 rule change monitoring

## Slice name

M12 rule change monitoring — detect and log rule pack version/status/content changes, monitor snapshots, user monitoring API, Admin `RuleChangeMonitoringPanel`, `shared-worker` periodic scan job, integration + Vitest tests

## Products touched

- **Compliance Core API** (`apps/compliancecore-api`): `RuleChangeMonitoringService`, `compliancecore_rule_change_events`, `compliancecore_rule_pack_monitor_snapshots`, `compliancecore_rule_change_scan_runs`, user + internal APIs
- **Compliance Core Frontend** (`apps/compliancecore-frontend`): `RuleChangeMonitoringPanel` on Admin workspace
- **shared-worker** (`workers/shared-worker`): `ComplianceCoreRuleChangeMonitorJob`, `ComplianceCoreRuleChangeMonitorClient`
- **Shared** (`STLCompliance.Shared`): `worker-compliancecore-rule-changes` token profile with `compliancecore.rule_changes.monitor`
- **Integration tests** (`tests/STLCompliance.ComplianceCore.Auth.Tests`): `ComplianceCoreRuleChangeMonitoringTests`

## Schema

### Migration `ComplianceCoreRuleChangeMonitoring`

**`compliancecore_rule_change_events`** — tenant-scoped change log (pack key, program key, change type, summary, status/version transitions, content hashes, source `api` | `worker`, optional scan run FK)

**`compliancecore_rule_pack_monitor_snapshots`** — one row per rule pack (version, status, content SHA-256 hash) for worker drift detection

**`compliancecore_rule_change_scan_runs`** — batch audit for worker scans

### Change types

| Type | Trigger |
|------|---------|
| `version_created` | `RulePackService.CreateAsync` |
| `status_changed` | `RulePackService.UpdateStatusAsync` |
| `content_updated` | `RuleContentService.UpdateContentAsync` (hash changed) |
| `scan_detected` | `RuleChangeMonitoringService.ProcessScanBatchAsync` (snapshot drift) |

## API + auth

### User JWT (Compliance Core entitlement)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/rule-changes/summary` | read: entitled users |
| GET | `/api/rule-changes/events` | read — filters: `packKey`, `changeType`, `since`, `limit` |
| GET | `/api/rule-changes/events/{id}` | read |

### Internal service (`shared-worker` → Compliance Core)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/internal/rule-changes/pending` | service token scope `compliancecore.rule_changes.monitor` |
| POST | `/api/internal/rule-changes/process-scan` | same |

## Audit events

- `rule_change.detected` — per logged change event
- `rule_changes.scan.completed` — per worker scan run

## shared-worker configuration

`ComplianceCoreRuleChangeMonitor` section:

| Key | Default | Purpose |
|-----|---------|---------|
| `Enabled` | `true` | Toggle job |
| `ComplianceCoreBaseUrl` | `http://localhost:5107` | API base |
| `ServiceToken` | `""` | Bearer for internal scan API |
| `ScanIntervalMinutes` | `30` | Periodic scan interval |
| `BatchSize` | `100` | Max packs per scan |
| `TenantId` | `null` | Optional tenant filter |

Env var (Render): `ComplianceCoreRuleChangeMonitor__ServiceToken` with profile `worker-compliancecore-rule-changes`.

## Frontend

- **RuleChangeMonitoringPanel** — summary tiles (24h / 7d / status / scan counts), filters, recent event list
- Placed on Admin workspace above source ingestion panel

## Tests

### Backend (`ComplianceCoreRuleChangeMonitoringTests`)

- `Rule_pack_create_logs_version_created_event`
- `Rule_pack_status_update_logs_status_changed_event`
- `Rule_change_summary_returns_counts`
- `Internal_process_scan_detects_snapshot_drift`
- `Internal_process_scan_rejects_missing_service_token`

### Frontend (`RuleChangeMonitoringPanel.test.tsx`)

- Renders summary and event list

## Verification

```powershell
dotnet build -c Release
dotnet test "tests/STLCompliance.ComplianceCore.Auth.Tests/STLCompliance.ComplianceCore.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~RuleChangeMonitoring"
cd apps/compliancecore-frontend
npm run test -- --run RuleChangeMonitoring
npm run build
```

## Remaining gaps

- CSV bundle import does not emit change events inline (worker scan catches drift on next cycle)
- No email/webhook notifications on rule changes
- Risk scoring and predictive warnings remain separate M12 backlog items

## Next recommended slice

**Compliance Core M12** — risk scoring, or **NexArr M12** audit export enhancements, or **RoutArr M12** audit package export per `00_SLICE_STATE.md`.
