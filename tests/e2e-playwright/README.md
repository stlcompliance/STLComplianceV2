# Browser E2E (Playwright)

Optional smoke tests for **suite-frontend**: NexArr login → unified dashboard → product launch surface → handoff redirect.

## Prerequisites

1. APIs via docker-compose (`5101`–`5107`)
2. Suite dev server: `cd apps/suite-frontend && npm run dev` (port **5174**)
3. StaffArr dev server for full handoff redirect: `cd apps/staffarr-frontend && npm run dev` (port **5175**)

## Run

```powershell
cd tests/e2e-playwright
npm install
npx playwright install chromium

$env:E2E_LIVE = "1"
npm test
```

Without `E2E_LIVE=1`, specs tagged `@requires-live` are excluded and individual tests skip when the stack is unreachable.

## Environment

| Variable | Default |
|----------|---------|
| `E2E_LIVE` | unset — tests skipped |
| `E2E_SUITE_URL` | `http://localhost:5174` |
| `E2E_NEXARR_URL` | `http://localhost:5101` |
| `E2E_DEMO_EMAIL` | `admin@demo.stl` |
| `E2E_DEMO_PASSWORD` | `ChangeMe!Demo2026` |
