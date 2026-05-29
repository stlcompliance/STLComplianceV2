# W254 — RoutArr M9 dispatch exception triage depth (W210)

Builds on **W210** (`routarr_dispatch_exceptions`, triage APIs, `DispatchExceptionQueuePanel`).

## Scope

### Migration `RoutArrDispatchExceptionResolutionDepth`

| Column | Purpose |
|--------|---------|
| `SlaDueAt` | Category-based default SLA on create; overridable on assign/bulk assign |
| `ResolutionTemplateKey` | Template applied on resolve |

Index: `TenantId` + `SlaDueAt` for overdue queue reads.

### Resolution templates (catalog)

Static templates in `DispatchExceptionResolutionTemplates`:

- `reassign_driver`, `reschedule_departure`, `swap_vehicle`, `route_replan`, `escalate_lead_dispatcher`

`GET /api/dispatch/exceptions/resolution-templates`

### Extended endpoints

| Method | Route | Behavior |
|--------|-------|----------|
| `GET` | `/api/dispatch/exceptions?overdueOnly=true` | Open-queue exceptions past `SlaDueAt` |
| `POST` | `/api/dispatch/exceptions/bulk/assign` | Assign up to 50 exceptions + optional SLA override |
| `POST` | `/api/dispatch/exceptions/bulk/resolve` | Resolve batch with optional template + notes |

Create/assign/resolve requests accept optional `slaDueAt`, `assignedToUserId`, `resolutionTemplateKey`.

List response adds `overdueCount`; item adds `slaDueAt`, `isSlaBreached`, `resolutionTemplateKey`.

Audit: `dispatch_exception.bulk_assign`, `dispatch_exception.bulk_resolve`.

### Frontend

`DispatchExceptionQueuePanel` improvements:

- Assign-to-me on create with category SLA
- SLA due display + breached badge + overdue tenant count/filter
- Resolution template picker for row/bulk resolve
- Row selection + bulk assign/resolve actions

## Tests

| Suite | Coverage |
|-------|----------|
| `RoutArrDispatchExceptionQueueTests` | Resolution depth: SLA on create, templates, bulk assign/resolve, overdue filter |
| `DispatchExceptionRulesTests` | SLA hours, template notes, breach detection |
| `DispatchExceptionQueuePanel.test.tsx` | SLA badge, templates, bulk controls |

## Verification

```powershell
dotnet build apps/routarr-api/RoutArr.Api/RoutArr.Api.csproj -c Release
dotnet test tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~DispatchException"
cd apps/routarr-frontend
npm run test -- --run DispatchExceptionQueuePanel
```

## Out of scope

- Automated SLA escalation worker / notification dispatch
- StaffArr resolver directory lookup (uses NexArr `userId` GUID assignment)

## Next slice

- **RoutArr M9** — active trips / unassigned queue depth (W211/W212) or proof/DVIR capture depth
- **M13 Playwright** — dispatch exception triage smoke (bulk/template interactions, optional)
- **NexArr M12** — platform-admin worker health orchestration UI (backlog)
