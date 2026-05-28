# Worker 137 — STLComplianceSite SEO / products hub hardening (M12)

## Scope

`apps/stlcompliancesite` marketing hardening:

- **SEO:** `SiteSeo` + `lib/seo.ts` — canonical URL, Open Graph, Twitter Card, robots, optional Organization JSON-LD on homepage
- **Products hub:** category sections, comparison table with public V1 maturity labels, maturity badges on cards
- **Resources:** `/resources` public education index (suite, ownership, trust, contact)
- **Sitemap / robots:** generated at build into `dist/` via Vite `closeBundle` plugin (`VITE_SITE_BASE_URL`)
- **Render:** `VITE_SITE_BASE_URL`, security headers on static site
- **Tests:** vitest for `seo`, `SiteSeo`, `publicRoutes`, `resources`, routing
- **Docs:** slice doc, `ENV_VARS_V1`, slice state

## Out of scope

- CMS / blog backend
- Server-side rendering or prerender per route
- Custom domain DNS (rebuild with updated `VITE_SITE_BASE_URL` when domain changes)

## Verification

```bash
cd apps/stlcompliancesite
npm ci
npm test
npm run build
# dist/sitemap.xml and dist/robots.txt present
```
