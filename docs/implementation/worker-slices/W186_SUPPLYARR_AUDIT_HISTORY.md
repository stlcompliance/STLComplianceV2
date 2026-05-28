# Worker 186 — SupplyArr audit history (M12 complete)

## Slice name

M12 audit history — query tenant audit trail from `supplyarr_audit_events`.

## Products touched

- **SupplyArr API** (`apps/supplyarr-api`): `AuditHistoryService`, `GET /api/audit-history`, meta-audit on read.
- **SupplyArr Frontend** (`apps/supplyarr-frontend`): `AuditHistoryPanel` on Reports workspace.
- **Tests** (`tests/STLCompliance.SupplyArr.Auth.Tests`): `SupplyArrAuditHistoryTests`.

## Schema

No migration. Reads existing `supplyarr_audit_events` (written by `SupplyArrAuditService` across mutations and reports).

## API + auth changes

### Endpoint

- `GET /api/audit-history` — cursor-paginated list (default limit 50, max 100)

Query filters:

- `action` (contains, case-insensitive)
- `targetType` (exact)
- `targetId` (exact)
- `actorUserId`
- `result` (exact)
- `fromOccurredAt`, `toOccurredAt`
- `cursor` (keyset: occurredAt + id)
- `limit`

Response fields per item: `id`, `actorUserId`, `action`, `targetType`, `targetId`, `result`, `reasonCode`, `correlationId`, `occurredAt`.

### Authorization

- `RequireAuditHistoryRead` → `tenant_admin`, `supplyarr_admin`, `supplyarr_manager` (and platform admin)

### Audit

- `supplyarr.audit.history.read` recorded when history is queried (`reasonCode`: `result_count:{n}`)

## Frontend changes

- `AuditHistoryPanel` on Reports workspace (`/reports`)
- Filters + load-more pagination via cursor
- Permission gate: `canReadAuditHistory`

## Tests

### Backend integration

- Filtered list by action/target
- Meta-audit event on read
- Cursor pagination
- Unauthorized without JWT

### Frontend unit

- `AuditHistoryPanel.test.tsx` — renders rows; hidden without permission

## Verification commands

```powershell
dotnet build "apps/supplyarr-api/SupplyArr.Api/SupplyArr.Api.csproj" -c Release
dotnet test "tests/STLCompliance.SupplyArr.Auth.Tests/STLCompliance.SupplyArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~SupplyArrAuditHistoryTests"
cd apps/supplyarr-frontend
npm run test
npm run build
```

## M12 SupplyArr reporting slice group

With Worker 186, SupplyArr M12 backlog items for **vendor reports**, **parts/inventory reports**, **purchasing reports**, **compliance reports**, **forgiving search**, and **audit history** are implemented. Remaining SupplyArr backlog is primarily **M8** procurement depth (RFQs, supplier onboarding workflows, event outbox, etc.) and cross-milestone integrations (**M10**).

## Next slice (Worker 187)

Recommended: **SupplyArr M8 event outbox/inbox** (`02_PRODUCT_IMPLEMENTATION_BACKLOG.md` — first major incomplete SupplyArr feature after M12 reporting), unless the coordinator prioritizes another product milestone.
