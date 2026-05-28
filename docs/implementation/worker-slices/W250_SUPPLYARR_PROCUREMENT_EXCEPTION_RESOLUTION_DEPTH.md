# W250 — SupplyArr M8 procurement exception resolution depth (W197)

Builds on **W197** (`supplyarr_procurement_exceptions`, workflow APIs, `ProcurementExceptionsPanel`).

## Scope

### API extensions (no base table redesign)

Migration `SupplyArrProcurementExceptionResolutionDepth` adds:

| Column | Purpose |
|--------|---------|
| `SlaDueAt` | Category-based default SLA on create; overridable on update/assign |
| `ResolutionTemplateKey` | Template applied on resolve |
| `LinkedPurchaseRequestId` | Follow-up PR action link |
| `LinkedPurchaseOrderId` | Follow-up PO action link |

`AssignedToUserId` retained from W197.

### Resolution templates (catalog)

Static templates in `ProcurementExceptionResolutionTemplates`:

- `vendor_requote`, `pr_resubmit`, `po_reissue`, `policy_waiver_documented`, `escalate_to_manager`

`GET /api/procurement-exceptions/resolution-templates`

### New / extended endpoints

| Method | Route | Behavior |
|--------|-------|----------|
| `GET` | `/api/procurement-exceptions?overdueOnly=true` | Active exceptions past `SlaDueAt` |
| `POST` | `/api/procurement-exceptions/{id}/assign` | Assign resolver + optional SLA override |
| `PUT` | `/api/procurement-exceptions/{id}/link-actions` | Link follow-up PR/PO records |
| `POST` | `/api/procurement-exceptions/{id}/resolve` | Optional `resolutionTemplateKey` merges template notes |

Create/update requests accept optional `slaDueAt`; create accepts `assignedToUserId`.

Response adds `slaDueAt`, `isSlaBreached`, `resolutionTemplateKey`, linked PR/PO ids and keys.

### Frontend

`ProcurementExceptionsPanel` improvements:

- Assign-to-me on create and per exception
- SLA due display + breached badge + overdue tenant count
- Resolution template picker on resolve
- PR/PO link actions for selected exception
- Exception detail selection for resolver workflow

## Tests

| Suite | Coverage |
|-------|----------|
| `SupplyArrProcurementExceptionTests` | Resolution depth: SLA on create, templates, link PR/PO, template resolve, overdue filter |
| `ProcurementExceptionRulesTests` | SLA hours, template notes, breach detection |
| `ProcurementExceptionsPanel.test.tsx` | Panel render with templates |

## Verification

```powershell
dotnet test tests/STLCompliance.SupplyArr.Auth.Tests/STLCompliance.SupplyArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~ProcurementException"
dotnet test tests/STLCompliance.SupplyArr.Auth.Tests/STLCompliance.SupplyArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~ProcurementExceptionRules"
cd apps/supplyarr-frontend
npm run test -- --run ProcurementExceptionsPanel
```

## Out of scope

- Automated SLA escalation worker / notifications
- StaffArr resolver directory lookup (uses NexArr `userId` GUID assignment)

## Next slice

- **RoutArr** — dispatch closeout / drag-assign depth (W78/W82)
- **M13 Playwright** — SupplyArr procurement exceptions smoke (optional)
- **MaintainArr** — additional M13 smokes or PM program depth
