# W253 — M13 Playwright RoutArr dispatch closeout panel smoke (W251)

Builds on **W251** (`DispatchCloseoutPanel` checklist/bulk/audit depth) and **W248** (RoutArr dispatch Playwright smoke pattern). No backend or migration changes — browser E2E catalog + read-only closeout panel smoke.

## Scope

### Playwright spec

`tests/e2e-playwright/tests/routarr-dispatch-closeout-panel-smoke.spec.ts`:

- Suite login → RoutArr handoff → `/dispatch`
- Scroll to `data-testid="dispatch-closeout-panel"`
- Assert heading **End-of-day closeout**, disposition selects (Remaining trips / Open stops)
- Assert **Preview closeout** visible; **Apply closeout** disabled until preview
- Optional `ensureRoutArrFieldInboxFixture()` in `beforeAll` — when live stack + seed succeed, expect **Trip closeout checklist**
- Click **Preview closeout** only (read-only); assert preview status message
- **No** apply/bulk closeout mutations

### Catalog + docs

- `StlE2ePlaywrightSpecCatalog.RoutArrDispatchCloseoutPanelSmokeSpec` in `ProductAdminSmokeSpecs` + `All`
- `StlE2ePlaywrightSpecCatalogTests` assertions (W253)
- `tests/e2e-playwright/README.md` spec table row

## Tests

| Suite | Coverage |
|-------|----------|
| `routarr-dispatch-closeout-panel-smoke.spec.ts` | Live handoff, panel chrome, preview-only closeout |
| `StlE2ePlaywrightSpecCatalogTests` | Catalog constant + ProductAdminSmokeSpecs membership |

## Verification

```powershell
dotnet test tests/STLCompliance.E2E/STLCompliance.E2E.csproj -c Release --filter "FullyQualifiedName~PlaywrightSpecCatalog"
cd tests/e2e-playwright
npm install
# With live stack: E2E_LIVE=1 npm test -- routarr-dispatch-closeout-panel-smoke
```

## Out of scope

- Apply / bulk closeout Playwright mutations
- New closeout API or panel features (see W251)
- Dispatch exception triage depth (W210)

## Next slice

- **RoutArr** — dispatch exception triage depth (W210 — SLA, templates, bulk actions)
- **NexArr M12** — platform-admin worker/service-token health orchestration UI
