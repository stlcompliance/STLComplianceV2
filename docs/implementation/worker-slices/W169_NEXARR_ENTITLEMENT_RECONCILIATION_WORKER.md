# Worker 169 — NexArr entitlement reconciliation worker (M12)

**Products:** NexArr, shared-worker, suite-frontend  
**Milestone:** M12  
**Backlog:** NexArr `[M12] entitlement reconciliation worker`

## Summary

Reconciles tenant product entitlements against subscription/licensing records. Platform admins configure auto-grant and auto-revoke behavior, preview pending drift, and review run history from the suite platform-admin UI. The shared worker calls NexArr internal batch APIs with scope `nexarr.entitlements.reconcile`.

## Backend (NexArr)

### Schema

- `nexarr_tenant_product_licenses` — per-tenant product licensing records (source of truth)
- `nexarr_platform_entitlement_reconciliation_settings` — singleton platform settings
- `nexarr_entitlement_reconciliation_runs` — batch run audit

### Platform admin APIs (JWT + platform admin)

| Method | Path | Purpose |
|--------|------|---------|
| GET | `/api/platform-admin/entitlement-reconciliation/settings` | Read reconciliation settings |
| PUT | `/api/platform-admin/entitlement-reconciliation/settings` | Upsert reconciliation settings |
| GET | `/api/platform-admin/entitlement-reconciliation/runs` | Recent reconciliation runs |
| GET | `/api/platform-admin/entitlement-reconciliation/pending` | Preview pending drift |
| GET | `/api/platform-admin/entitlement-reconciliation/licenses` | List tenant product licenses |
| PUT | `/api/platform-admin/entitlement-reconciliation/licenses` | Upsert tenant product license |

### Internal APIs (service token)

| Method | Path | Scope |
|--------|------|-------|
| GET | `/api/internal/entitlement-reconciliation/pending` | `nexarr.entitlements.reconcile` |
| POST | `/api/internal/entitlement-reconciliation/process-batch` | same |

## Shared worker

- `NexArrEntitlementReconciliationJob` — default 30 min interval, batch 50
- Config: `NexArrEntitlementReconciliation__NexArrBaseUrl`, `NexArrEntitlementReconciliation__ServiceToken`

## Frontend (suite-frontend)

- Route: `/app/platform-admin/entitlements`
- `EntitlementReconciliationSettingsPanel` — enable toggle, auto-grant/revoke, pending drift preview, recent runs

## Tests

- `NexArrEntitlementReconciliationTests` — auth, pending drift, batch revoke/grant, run history
- `EntitlementReconciliationRulesTests` — license validity and drift kind rules
- `EntitlementReconciliationSettingsPanel.test.tsx` — frontend panel

## Next slice

Per backlog: NexArr tenant lifecycle worker or remaining M12 items.
