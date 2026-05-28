# Worker 185 — SupplyArr forgiving search

## Slice name

M12 forgiving search — cross-entity discovery with normalized/fuzzy matching.

## Products touched

- **SupplyArr API** (`apps/supplyarr-api`): `ForgivingSearchService`, `ForgivingSearchNormalizer`, `GET /api/search/forgiving`, audit events.
- **SupplyArr Frontend** (`apps/supplyarr-frontend`): `ForgivingSearchBar` in workspace shell (all routes).
- **Tests** (`tests/STLCompliance.SupplyArr.Auth.Tests`): `SupplyArrForgivingSearchTests`.

## Schema

No migration. Search reads existing SupplyArr tables with in-memory normalized scoring over tenant-scoped candidate caps (250 per entity family).

## API + auth changes

### Endpoint

- `GET /api/search/forgiving?q={query}&limit={optional}` — min query length 2, max 50 results (default 25)

### Entity coverage

- External parties (vendors, dealers, suppliers)
- Parts (keys, names, manufacturer numbers, aliases)
- Vendor SKUs (`PartVendorLink.VendorPartNumber`)
- Purchase requests and purchase orders
- Party compliance documents (from W184 table)

### Matching

- Normalizes query and haystacks by lowercasing and stripping non-alphanumeric characters (dash/space insensitive)
- Token-aware scoring; ranks merged results by match score

### Authorization

- `RequireForgivingSearch` → party read roles (same breadth as vendor reports)

### Audit

- `supplyarr.search.forgiving` with `reasonCode` `result_count:{n}`

## Frontend changes

- `ForgivingSearchBar` above workspace section headers (global within product shell)
- Debounced live search (300ms), dropdown results, navigate to `deepLinkPath` (`/parties`, `/catalog`, `/purchasing`)
- Permission gate: `canUseForgivingSearch`

## Tests

### Backend integration

- Normalizer fuzzy unit assertion
- Cross-entity search hits for part/vendor/SKU/PR/PO
- Short query rejected (400)
- Unauthorized without JWT

### Frontend unit

- `ForgivingSearchBar.test.tsx` — search results render; hidden without permission

## Verification commands

```powershell
dotnet build "apps/supplyarr-api/SupplyArr.Api/SupplyArr.Api.csproj" -c Release
dotnet test "tests/STLCompliance.SupplyArr.Auth.Tests/STLCompliance.SupplyArr.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~SupplyArrForgivingSearchTests"
cd apps/supplyarr-frontend
npm run test
npm run build
```

## Remaining gaps

- Dedicated DB indexes / full-text search for very large tenants
- Deep-link query params to focus a specific record in section panels
- M12 audit history (next slice)

## Next slice (Worker 186)

Recommended: **SupplyArr audit history** per `02_PRODUCT_IMPLEMENTATION_BACKLOG.md` and `00_SLICE_STATE.md`.
