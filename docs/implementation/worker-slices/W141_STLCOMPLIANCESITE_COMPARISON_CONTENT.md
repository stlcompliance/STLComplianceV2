# Worker 141 — STLComplianceSite comparison content (M3/M12)

## Scope

`apps/stlcompliancesite` — public **compare** page (marketing only):

- `/compare` — honest comparison vs spreadsheets and single-domain point tools; architecture/ownership dimensions (not a feature scorecard)
- `AlternativeComparisonTable`, scenario cards, suite honesty notes, marketing disclaimer
- Nav + footer + resources index + homepage CTA
- `SiteSeo` canonical/OG for `/compare`
- `buildStaticPublicPaths` includes `/compare` (sitemap at build)
- Vitest: `compare.test.ts`, routing smoke, `publicRoutes` assertion

## Out of scope

- Competitive naming of specific vendors
- Product APIs, entitlements, or checkout from marketing site
- Implementation maturity dashboard (separate slice)

## Verification

```bash
cd apps/stlcompliancesite
npm ci
npm test
npm run build
# dist/sitemap.xml contains /compare
```
