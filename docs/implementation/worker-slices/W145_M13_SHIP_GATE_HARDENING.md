# Worker 145 — M13 ship-gate hardening

## Scope

Consolidate M13 ship-gate checks into an explicit catalog and extend integration/live probes without duplicating W92 (OpenAPI snapshots), W95/W96 (tenant isolation), or W144 (Playwright journeys).

## Deliverables

| Area | Change |
|------|--------|
| **Catalog** | `StlM13ShipGateCatalog` — OpenAPI product keys, required path fragments, entitlement-denial probes, tenant-isolation minimums |
| **OpenAPI** | `ShipGateOpenApiCatalogTests` — snapshot presence; parity tests assert `RequiredOpenApiPathFragments` |
| **Entitlement denial** | `EntitlementDenialFlowTests` — six product `/api/me` + NexArr launch context (`403`) |
| **Tenant isolation** | `TenantIsolationFlowTests` — NexArr cross-tenant `GET /api/tenants/{tenantB}` |
| **Live probes** | `EntitlementDenialLiveTests` — StaffArr `/api/me` + NexArr launch denial (`E2E_LIVE`) |
| **Catalog tests** | `StlM13ShipGateCatalogTests` — minimum integration fact counts |

## Verification

```powershell
dotnet test tests/STLCompliance.OpenApi.Tests/STLCompliance.OpenApi.Tests.csproj -c Release --filter "Category=OpenApi"
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "Category=Integration&Area=EntitlementDenial|Category=Integration&Area=TenantIsolation|Category=E2e&Area=ShipGate"
```

Live (optional):

```powershell
$env:E2E_LIVE = "1"
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "Category=Live&Area=EntitlementDenial"
```

## Out of scope

- Re-running W144 Playwright operator/handoff specs
- OpenAPI snapshot regeneration (unless routes changed)
- New product feature APIs

## Related slices

- W92 — OpenAPI parity CI
- W95/W96 — tenant isolation E2E
- W144 — Playwright/E2E expansion
