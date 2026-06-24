# STL Compliance Environment Variables

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

## Public Domain Defaults

- Browser-facing product URLs default to `https://{product}.stlcompliance.com`.
- NexArr defaults to `https://app.stlcompliance.com`.
- Field Companion defaults to `https://fieldcompanion.stlcompliance.com`.
- The public marketing site defaults to `https://stlcompliance.com`.
- `stl-shared` sets `Cors__AllowedOriginPatterns=https://*.stlcompliance.com` so API CORS policies allow product subdomains by default. Product APIs may add local development origins, but they should not replace the shared wildcard default.
- Render `domains` entries in `render.yaml` declare the custom domains. DNS records and Render certificate verification must still be completed outside the Blueprint.

## Product Coverage

The current Render Blueprint deploys the suite shell, implemented product
frontends, the standalone public STL Compliance Site, the standalone public KB
site, and the Render-backed APIs/databases that are declared in
`StlRenderBlueprintCatalog`.
