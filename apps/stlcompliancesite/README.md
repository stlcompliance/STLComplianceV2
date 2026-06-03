# STLComplianceSite

Public static marketing site for the STL Compliance / Arr suite (port **5173**).

## Scope

- Marketing and education only — **no** product APIs, auth, tenant data, or business authority
- Client sign-in links to the suite (`VITE_SUITE_LOGIN_URL`)
- Demo/contact is client-side (mailto), not a backend form post
- `/compare` explains when spreadsheets or point tools fit vs. the bounded multi-product suite
- `/pricing` explains product licensing and secure suite access — no checkout or list prices

## Local dev

```bash
cd apps/stlcompliancesite
npm ci
npm run dev
```

Open http://localhost:5173

## Build

```bash
npm run build
npm run preview
npm test
```

Build emits `dist/sitemap.xml` and `dist/robots.txt` using `VITE_SITE_BASE_URL` (defaults to production Render URL).

## SEO

Per-route metadata via `SiteSeo` (title, description, canonical, Open Graph, Twitter Card). Homepage includes Organization JSON-LD. Product hub explains the connected product suite.

## Branding

Updated full-color PNGs are copied from the root `branding/` folder into `public/brand/`. Re-copy after logo updates.

## Deploy

Render static site `stlcompliancesite` — see root `render.yaml`.
