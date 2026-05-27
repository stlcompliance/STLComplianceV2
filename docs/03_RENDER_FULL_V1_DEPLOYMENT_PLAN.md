# Render Full V1 Deployment Plan

## Resource Inventory

### Static Sites

1. stlcompliancesite
2. suite-frontend

### Web Services

3. nexarr-api
4. staffarr-api
5. trainarr-api
6. maintainarr-api
7. routarr-api
8. supplyarr-api
9. compliancecore-api

### Background Workers

10. nexarr-worker
11. staffarr-worker
12. trainarr-worker
13. maintainarr-worker
14. routarr-worker
15. supplyarr-worker
16. compliancecore-worker

### PostgreSQL Databases

17. nexarr-db
18. staffarr-db
19. trainarr-db
20. maintainarr-db
21. routarr-db
22. supplyarr-db
23. compliancecore-db

### Key Value

24. redis

## Stack by Resource Type

| Resource Type | Stack |
|---|---|
| Static sites | React, TypeScript, Vite, Tailwind CSS, lucide-react |
| APIs | Docker, .NET 10, ASP.NET Core 10, EF Core 10, Npgsql, PostgreSQL |
| Workers | Docker, .NET 10 Worker Service, EF Core 10, Npgsql, PostgreSQL |
| Databases | Render PostgreSQL, one per product |
| Cache/coordination | Render Key Value / Redis-compatible service |

## API Ownership

| Service | Database | Purpose |
|---|---|---|
| nexarr-api | nexarr-db | login, tenants, entitlements, licensing, service tokens, launch |
| staffarr-api | staffarr-db | people, org, permissions, certifications, readiness, history |
| trainarr-api | trainarr-db | training programs, evidence, signoffs, qualifications |
| maintainarr-api | maintainarr-db | assets, inspections, defects, work orders, PM, readiness |
| routarr-api | routarr-db | routes, trips, dispatch, DVIR, proof, exceptions |
| supplyarr-api | supplyarr-db | vendors, parts, inventory, PR/PO, receiving, pricing, lead times |
| compliancecore-api | compliancecore-db | vocabulary, keys, mappings, rule packs, SDS/HazCom, evaluation |

## Worker Ownership

| Worker | Database | Jobs |
|---|---|---|
| nexarr-worker | nexarr-db | token cleanup, entitlement reconciliation, licensing checks, platform audit rollups |
| staffarr-worker | staffarr-db | certification expiration, permission projection, readiness, audit packages |
| trainarr-worker | trainarr-db | due training, reminders, escalation, qualification publication |
| maintainarr-worker | maintainarr-db | PM due-state, WO generation, inspection generation, defect escalation |
| routarr-worker | routarr-db | route state, trip closeout, eligibility snapshots, DVIR follow-up |
| supplyarr-worker | supplyarr-db | reorder evaluation, pricing snapshots, lead-time snapshots, procurement reminders |
| compliancecore-worker | compliancecore-db | vocabulary maintenance, key normalization, rule publication, SDS/HazCom reference work |

## Environment Groups

- stl-shared: ASPNETCORE_ENVIRONMENT, LOG_LEVEL, OTEL_ENABLED, REDIS_URL, public URLs
- stl-auth: JWT issuer/audience/signing, renewal token pepper, service-token issuer/audience/signing
- stl-nexarr, stl-staffarr, stl-trainarr, stl-maintainarr, stl-routarr, stl-supplyarr, stl-compliancecore: product DB URL and product config
- stl-frontend: VITE API base URLs and safe public config

Anything prefixed with VITE_ is public to the browser. Secrets never go into frontend variables.

## render.yaml

render.yaml defines static sites, APIs, workers, databases, Redis/Key Value, environment groups, build commands, start commands, and health checks. It is the infrastructure source of truth.
