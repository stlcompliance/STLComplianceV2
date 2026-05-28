# W240 — Compliance Core M12 audit delivery orchestration UI

Ties **W231** (M12 analytics batch worker), **W47** (scheduled rule pack evaluation), and audit package generation jobs into one Admin orchestration surface.

## Scope

### API (`compliancecore-api`)

| Endpoint | Auth | Purpose |
|----------|------|---------|
| `GET /api/audit-delivery-orchestration` | Read (admin/reviewer) | Worker settings snapshot, pending scheduled packs, M12 due steps, pending audit jobs, last run summaries |
| `POST /api/audit-delivery-orchestration/trigger-scheduled-evaluation` | Admin only | Manual scheduled rule evaluation for all published packs in tenant |
| `POST /api/audit-delivery-orchestration/trigger-m12-batch` | Admin only | Manual M12 batch (all enabled steps forced due; requires worker enabled) |

- `AuditDeliveryOrchestrationService` coordinates existing workers
- `ScheduledRuleEvaluationService.ProcessTenantManualAsync` — evaluates all eligible published packs (not interval-gated)
- `M12AnalyticsBatchWorkerService.ProcessTenantManualAsync` — runs enabled batch steps with admin actor audit attribution

### Frontend (`compliancecore-frontend`)

- `AuditDeliveryOrchestrationPanel` on Admin workspace (above M12 worker settings panel)
- `data-testid`: `compliancecore-audit-delivery-orchestration-panel`, orchestration subsections + trigger buttons

### Tests

- `ComplianceCoreAuditDeliveryOrchestrationTests` — auth, status read, manual triggers
- `AuditDeliveryOrchestrationPanel.test.tsx` — Vitest render/trigger visibility

## Verification

```powershell
dotnet test tests/STLCompliance.ComplianceCore.Auth.Tests/STLCompliance.ComplianceCore.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~AuditDeliveryOrchestration"
cd apps/compliancecore-frontend
npm test -- AuditDeliveryOrchestrationPanel
```

## Out of scope

- M13 Playwright for orchestration panel (W232 covers M12 settings smoke only)
- Email/webhook audit delivery notifications
- Replacing shared-worker internal `process-batch` jobs

## Next slice

- **Suite M13** — RoutArr Reports audit export Playwright (W227)
- **Compliance Core** — orchestration Playwright smoke (optional)
- **SupplyArr** — procurement automation depth
