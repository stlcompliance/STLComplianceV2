# Worker 201 — SupplyArr M8 warranty claims

**Product:** SupplyArr  
**Milestone:** M8  
**Backlog:** `[M8] warranty claims` (`02_PRODUCT_IMPLEMENTATION_BACKLOG.md`)

## Delivered

- **Migration:** `supplyarr_warranty_claims` — links vendor party, part, optional PO/PO line, optional receiving receipt/line; workflow timestamps and notes.
- **API:** `GET/POST /api/warranty-claims`, `GET/PUT /api/warranty-claims/{id}`, workflow actions `submit`, `record-vendor-response`, `close`, `deny`, `cancel`.
- **Statuses:** `draft` → `submitted` → `vendor_responded` → `closed` | `denied`; `cancelled` from draft/submitted.
- **Auth:** `RequireReturnRead` / `RequireReturnManage` (same receiving/returns roles).
- **Audit:** `warranty_claim.*` actions per transition.
- **Outbox:** `warranty_claim.created|updated|submitted|vendor_responded|closed|denied|cancelled`.
- **Frontend:** `WarrantyClaimsPanel` on Receiving workspace (`/receiving`).
- **Tests:** `SupplyArrWarrantyClaimTests`, `WarrantyClaimsPanel.test.tsx`.

## Verification

```bash
dotnet build apps/supplyarr-api/SupplyArr.Api/SupplyArr.Api.csproj
dotnet test tests/STLCompliance.SupplyArr.Auth.Tests --filter SupplyArrWarrantyClaimTests
cd apps/supplyarr-frontend && npm test -- --run WarrantyClaimsPanel
```

## Next slice

SupplyArr M8 feature backlog is largely complete after warranty claims. Recommended cross-product next items: **TrainArr M10 StaffArr acknowledgement tracking**, **MaintainArr M12 maintenance/executive reports**, or **NexArr M12** platform workers not yet sliced — see suite backlog scan in completion report.
