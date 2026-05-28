# STLComplianceSite

Public static marketing site for the STL Compliance / Arr suite (port **5173**).

## Scope

- Marketing and education only — **no** product APIs, auth, tenant data, or business authority
- Client sign-in links to the suite (`VITE_SUITE_LOGIN_URL`)
- Demo/contact is client-side (mailto), not a backend form post

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

## Branding

`public/stl-logo.png` is copied from `branding/STLCompliance/STL Compliance-Full-monchrome.png`. Re-copy after logo updates.

## Deploy

Render static site `stlcompliancesite` — see root `render.yaml`.
