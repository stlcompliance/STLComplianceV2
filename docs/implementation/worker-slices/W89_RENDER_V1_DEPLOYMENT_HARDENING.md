# Worker 89 — Render V1 deployment hardening

## Slice name

M1/M13 deployment — full V1 `render.yaml`, env groups, static frontends, `shared-worker`, internal API URLs, health checks, env documentation.

## Products touched

- **render.yaml** — 7 APIs, 7 static frontends, 8 workers (7 product + `shared-worker`), 7 Postgres DBs, Redis Key Value, 5 env groups.
- **shared-worker** — `workers/shared-worker/Dockerfile`.
- **STLCompliance.Shared** — `StlServiceUrl` for host:port normalization; JWT env aliases in `StlJwtAuthenticationExtensions`.
- **Docs** — `docs/deployment/ENV_VARS_V1.md`, this file.

## render.yaml inventory

| Type | Count | Names |
|------|-------|-------|
| PostgreSQL | 7 | `*-db` per product |
| Key Value | 1 | `redis` |
| Web (Docker API) | 7 | `nexarr-api` … `compliancecore-api` |
| Worker | 8 | `shared-worker` + 7 product workers |
| Static | 7 | `suite-frontend`, product frontends |

**Health:** all APIs use `healthCheckPath: /health/ready`.

**Internal URLs:** `stl-internal-api-urls` group (`http://{service-name}:10000` on Render private network).

**Public URLs:** onrender.com defaults for CORS, NexArr `Launch__Products__*`, and Vite `VITE_*` build args.

**Secrets:** `sync: false` for `Handoff__ServiceToken`, cross-product `*__ServiceToken`, and each `shared-worker` job token — provision via NexArr after first deploy.

## Post-deploy checklist

1. Apply Blueprint (or sync) from repo root.
2. Set all `sync: false` service tokens in Dashboard (see `ENV_VARS_V1.md`).
3. Confirm each API `/health/ready` returns 200.
4. Run `render blueprints validate render.yaml` locally when CLI is available.
5. Rebuild static sites after API URLs change (custom domains).
6. Attach persistent disks before relying on evidence file paths in production.

## Tests

No new automated tests — infrastructure slice. Existing `STLCompliance.Health.Tests` covers `/health/ready` contract.

## Out of scope

- `stlcompliancesite` static app (not in repo).
- Companion field inbox (Worker 90+).
- Render CLI install in CI (documented only).

## Next slice

**Companion app field inbox** or **M13 load/E2E** per `01_MILESTONE_MASTERPLAN.md`.
