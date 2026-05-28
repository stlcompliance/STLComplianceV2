# Worker 199 — SupplyArr M10 Compliance Core fact publishing

## Summary

SupplyArr publishes rebuildable procurement facts to Compliance Core on integration outbox processing. Compliance Core ingests facts via a service-token integration API, stores them in `compliancecore_product_fact_mirrors`, and resolves them through the `product_mirror` fact source type.

## Compliance Core

- **Table:** `compliancecore_product_fact_mirrors` — tenant-scoped mirror rows keyed by `source_product`, `fact_key`, `scope_key`, with idempotent ingest via `idempotency_key`.
- **API:** `POST /api/integrations/product-facts/ingest` — scope `compliancecore.facts.ingest`, NexArr service token from `supplyarr` → `compliancecore`.
- **Resolve:** `FactResolveService` + `ProductFactMirrorService` resolve `product_mirror` sources (scope from context: `purchase_request_id`, `purchase_order_id`, `vendor_party_id`, `procurement_exception_id`, or `scope_key`).
- **Fact source type:** `product_mirror` added to `FactSourceTypes`.

## SupplyArr

- **Publisher:** `ComplianceCoreFactPublisherService` invoked from `IntegrationEventProcessingService` after successful outbox handling.
- **Client:** `ComplianceCoreFactPublicationClient` → Compliance Core ingest API; config `ComplianceCore:BaseUrl`, `ComplianceCore:ServiceToken`.
- **Events:** PR submit/approve, PO issued, receiving posted, vendor restrictions, procurement exceptions, supplier incident restriction applied.
- **Fact keys:** `supplyarr.purchase_request.status`, `supplyarr.purchase_order.status`, `supplyarr.receiving.receipt.posted`, `supplyarr.vendor_restriction.blocks_procurement`, `supplyarr.procurement_exception.status`, `supplyarr.procurement_exception.is_active`.
- **Integration token profile:** `supplyarr-compliancecore` in `StlIntegrationTokenCatalog`.

## Tests

- `ComplianceCoreProductFactMirrorTests` — ingest + internal resolve via `product_mirror`.
- `SupplyArrComplianceCoreFactPublishingTests` — submit PR → process outbox → mirror row + resolve.

## Build / test

```bash
dotnet build "apps/compliancecore-api/ComplianceCore.Api/ComplianceCore.Api.csproj" -c Release
dotnet build "apps/supplyarr-api/SupplyArr.Api/SupplyArr.Api.csproj" -c Release
dotnet test "tests/STLCompliance.ComplianceCore.Auth.Tests/STLCompliance.ComplianceCore.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~ProductFactMirror"
dotnet test "tests/STLCompliance.SupplyArr.Auth.Tests/STLCompliance.SupplyArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~ComplianceCoreFactPublishing"
```

## Next slice

Worker **200** — SupplyArr M8 supply readiness dashboard (complete). Next: **SupplyArr M8 warranty claims** or cross-product backlog per `00_SLICE_STATE.md`.
