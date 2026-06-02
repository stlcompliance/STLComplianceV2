# STL Suite Render Log Audit

Generated: 2026-06-02T12:14:48Z

## Scope

- Workspace: STLC (`tea-d89b49mq1p3s73fmkiqg`)
- Frontend: `suite-frontend` (`srv-d8ehn9sm0tmc73es8r10`)
- API inspected for Suite platform routes: `nexarr-api` (`srv-d8ehnqkm0tmc73es9h80`)
- Browser crawl/mutation window checked: 2026-06-02T10:20:00Z through 2026-06-02T10:45:00Z

## Findings

- `suite-frontend` did not show 500/502/503/504 traffic during the checked window.
- `nexarr-api` returned no 500/502/503/504 responses during the checked window.
- `nexarr-api` logged upstream probe connection errors when Suite loaded `/app/platform-admin/orchestration`.
- The platform worker-health endpoint still returned `200`; the error was emitted by the health probe's outbound HTTP client.

## Error Log Entries

- 2026-06-02T10:29:45Z, `nexarr-api`: health probe attempted `http://localhost:5102/health/ready` through `http://localhost:5107/health/ready`; outbound socket connection refused.
- 2026-06-02T10:33:12Z, `nexarr-api`: same worker-health probe failure against localhost product API ports.
- 2026-06-02T10:30:44Z, `nexarr-api`: `GET /favicon.ico` returned `404`; this is low-impact and unrelated to the Suite route crawl.

## Root Cause

Render environment variables such as `StaffArr__BaseUrl` are normalized by .NET configuration into hierarchical keys like `StaffArr:BaseUrl`. NexArr was reading the literal flat key `StaffArr__BaseUrl`, so production fell back to local defaults (`localhost:5102` through `localhost:5107`).

## Local Fix

- `NexArrServiceRegistration` now reads `Product:BaseUrl` hierarchical keys first and keeps the older flat key fallback for local appsettings compatibility.
- `PlatformSeeder` can now refresh default localhost product catalog URLs to configured production URLs during startup.
- `PlatformHealthApiTests` includes a regression check for Render-style hierarchical product URL keys.
