# Repo Structure, Local Dev, and CI

## Monorepo Shape

```txt
/stlcompliance
  /apps
    /stlcompliancesite
    /suite-frontend
    /nexarr-api
    /staffarr-api
    /trainarr-api
    /maintainarr-api
    /routarr-api
    /supplyarr-api
    /compliancecore-api
  /workers
    /nexarr-worker
    /staffarr-worker
    /trainarr-worker
    /maintainarr-worker
    /routarr-worker
    /supplyarr-worker
    /compliancecore-worker
  /packages
    /ui
    /design-tokens
    /contracts
    /shared-dotnet
  docker-compose.yml
  render.yaml
  README.md
```

## Local URLs

- stlcompliancesite: http://localhost:5173
- suite-frontend: http://localhost:5174
- nexarr-api: http://localhost:5101
- staffarr-api: http://localhost:5102
- trainarr-api: http://localhost:5103
- maintainarr-api: http://localhost:5104
- routarr-api: http://localhost:5105
- supplyarr-api: http://localhost:5106
- compliancecore-api: http://localhost:5107

## CI Checks

- dotnet restore
- dotnet build
- dotnet test
- npm ci
- npm run build
- npm run lint where configured
- typecheck
- Playwright smoke tests
- Docker build for every API and worker
- render.yaml validation where possible

## Shared Package Rules

Shared packages may contain UI primitives, design tokens, OpenAPI clients, event envelopes, error models, pagination models, service-token helpers, health helpers, and logging helpers.

Shared packages may not contain product authority, product database models, cross-product DbContext sharing, or hidden domain services.
