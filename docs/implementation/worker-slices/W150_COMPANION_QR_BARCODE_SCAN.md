# Companion QR and barcode scan (M11)

## Scope

Field operators scan QR codes or barcodes on asset labels, work orders, or training assignments to jump to the matching field-inbox task. NexArr resolves scans against the aggregated companion field inbox with server-side entitlement checks.

## Deliverables

| Area | Change |
|------|--------|
| **NexArr** | `POST /api/companion/scan/resolve` — parse scan payload, validate JWT + tenant + product entitlement, match task in aggregated field inbox |
| **Payload parser** | Direct task keys (`trainarr:assignment:{id}`), `stl-field-task:` prefix, JSON `{ "taskKey": "…" }`, product deep-link paths, `?taskKey=` query URLs |
| **Companion UI** | `FieldScanPanel` — camera scan via `@zxing/browser`, manual code entry, highlight matched inbox row, optional product deep link |
| **Tests** | `CompanionScanPayloadParserTests`, `NexArrCompanionScanResolveTests`, `scanPayload.test.ts`, Playwright `companion-field-scan.spec.ts` |
| **Catalog** | `StlE2ePlaywrightSpecCatalog.CompanionFieldScanSpec` |

## Verification

```powershell
dotnet test tests/STLCompliance.NexArr.Auth.Tests -c Release --filter "CompanionScan"
cd apps/companion-frontend; npm test -- --run
dotnet test tests/STLCompliance.E2E -c Release --filter "Category=E2e&FullyQualifiedName~Companion"
```

## Boundaries

- No product domain persistence in NexArr; resolution uses existing field-inbox aggregation only.
- Scans for valid task keys not assigned to the user return `denied` with `scan.not_in_inbox`.
- Camera requires browser `getUserMedia`; manual entry supports E2E and accessibility.

## Related

- W90 — Companion field inbox
- W133 — TrainArr deep links on field inbox items
- W147 — Field evidence capture
- W149 — Product switcher
