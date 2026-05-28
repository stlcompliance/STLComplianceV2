# Product-owner load SLO targets (V1)

**Status:** Adopted — 2026-05-27  
**Scope:** M13 k6 load-test harness (`tests/load-k6`)  
**Profile key:** `product-owner` (`STL_LOAD_SLO_PROFILE=product-owner`, default)

## Purpose

These targets replace engineering-placeholder thresholds from Workers 100/104. They define the V1 operational baseline for the STL Compliance suite on Render starter-tier web services with docker-compose local parity.

## Measurement

| Metric | Source | Window |
|--------|--------|--------|
| p95 latency | k6 `http_req_duration` p(95) | Per scenario run |
| Error rate | k6 `http_req_failed` rate | Per scenario run |
| Minimum requests | k6 `http_reqs` count | Per scenario run |

Runs use `./scripts/ops/load-test-run.ps1` (or `.sh`) with default 5 VUs / 30s unless noted.

## Scenario targets

| Scenario key | User journey | p95 max | Error max | Min requests |
|--------------|--------------|---------|-----------|--------------|
| `api-health-liveness` | All 7 APIs `/health` | 400 ms | 0.5% | 50 |
| `api-health-ready` | All 7 APIs `/health/ready` | 1500 ms | 1% | 50 |
| `nexarr-platform-health` | NexArr `/api/platform/health` | 4000 ms | 3% | 20 |
| `nexarr-auth-me` | NexArr login + `/api/me` | 1200 ms | 1% | 30 |
| `product-auth-handoff-me` | Login → handoff → redeem → `/api/me` (6 products) | 6000 ms | 3% | 12 |
| `trainarr-qualification-check` | Handoff TrainArr → `POST /api/qualification-checks` (TrainArr + Compliance Core) | 10000 ms | 4% | 10 |
| `routarr-dispatch-workflow-gate` | Handoff RoutArr → create trip → `POST /api/dispatch-workflow-gates/check` (RoutArr + Compliance Core) | 12000 ms | 4% | 8 |
| `staffarr-person-readiness` | Handoff StaffArr → `GET /api/people/{personId}/readiness` | 8000 ms | 4% | 10 |
| `supplyarr-procurement-pr` | Handoff SupplyArr → vendor/part → PR submit/approve | 15000 ms | 5% | 6 |
| `maintainarr-work-order` | Handoff MaintainArr → asset chain → work order create/read | 18000 ms | 5% | 6 |
| `compliancecore-rule-evaluate` | Handoff Compliance Core → journey seed → rule pack evaluate | 12000 ms | 4% | 8 |

## Ownership

| Area | Owner | Notes |
|------|-------|-------|
| Control-plane auth / handoff | NexArr | `nexarr-auth-me`, `product-auth-handoff-me` |
| Platform aggregation | NexArr | `nexarr-platform-health` |
| API availability | Platform ops | Health scenarios |
| Training authorization | TrainArr | Qualification check journey |
| Dispatch compliance gates | RoutArr | Workflow gate journey |
| Workforce readiness | StaffArr | Person readiness journey |
| Procurement | SupplyArr | Purchase request approval journey |
| Maintenance execution | MaintainArr | Work order journey |
| Rule evaluation | Compliance Core | Operator rule pack evaluate journey |

## Engineering fallback

Set `STL_LOAD_SLO_PROFILE=engineering-defaults` to evaluate against placeholder thresholds (Workers 100/104) during harness development.

Canonical C# definitions: `StlLoadTestSloCatalog.ProductOwnerTargets`.  
JSON mirror: `tests/load-k6/slo-product-owner.json`.

## Review cadence

Revisit after first Render production month or when instance types change. Cross-product journey scenarios may tighten once Compliance Core rule packs are pre-seeded in all environments.
