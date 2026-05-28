# Worker 139 — STLComplianceSite pricing narrative (M3)

## Scope

`apps/stlcompliancesite` — public **pricing & licensing** page (marketing only):

- `/pricing` — NexArr tenant entitlements narrative, per-product packaging examples, explicit no-checkout disclaimer
- Nav + footer links, resources index entry, homepage CTA
- `SiteSeo` canonical/OG for `/pricing`
- `buildStaticPublicPaths` includes `/pricing` (sitemap at build)
- Vitest: `pricing.test.ts`, routing smoke, `publicRoutes` assertion

## Out of scope

- Payment processing, shopping cart, or list prices
- NexArr entitlement APIs from marketing site
- Contract/provisioning automation

## Verification

```bash
cd apps/stlcompliancesite
npm ci
npm test
npm run build
# dist/sitemap.xml contains /pricing
```
