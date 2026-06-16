# STL Compliance V1 Environment Variables

This file is the catalog anchor for Render Blueprint environment-variable checks.

Runtime source of truth remains `render.yaml` plus the explicit operation catalogs in
`packages/shared-dotnet/STLCompliance.Shared/Operations`.

## Groups

- `stl-shared`
- `stl-auth`
- `stl-internal-api-urls`
- `stl-public-frontend-urls`
- `stl-vite-product-frontend-urls`
- `stl-public-api-urls`

## Product Coverage

The current Render Blueprint deploys the suite shell, implemented V1 product
frontends, the standalone public STL Compliance Site, the standalone public KB
site, and the Render-backed APIs/databases that are declared in
`StlRenderBlueprintCatalog`.
