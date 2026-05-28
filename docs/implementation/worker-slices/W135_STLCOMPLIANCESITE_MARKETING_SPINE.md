# Worker 135 — STLComplianceSite marketing spine (M3)

## Scope

Scaffold `apps/stlcompliancesite` — Vite + React static marketing SPA:

- Homepage with ARR positioning, maturity banner, product grid
- Products hub + per-product pages (NexArr, StaffArr, TrainArr, MaintainArr, RoutArr, SupplyArr, Compliance Core, Companion)
- Security, data ownership, demo/contact (no API), privacy, terms, 404
- Branding (`public/stl-logo.png`), SEO via `SiteSeo`
- `render.yaml` static site `stlcompliancesite` on port 5173
- CI job: build + vitest
- Docs: app README, slice doc, `ENV_VARS_V1`, slice state

## Out of scope

- Backend lead capture API
- NexArr callback allowlist changes (login uses suite URL)
- Products hub advanced CMS / resources library (M12 follow-up)

## Verification

```bash
cd apps/stlcompliancesite
npm ci
npm test
npm run build
```
