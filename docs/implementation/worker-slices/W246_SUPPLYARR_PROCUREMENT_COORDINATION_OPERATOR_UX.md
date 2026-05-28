# W246 — SupplyArr M8/M10 procurement coordination operator UX

Builds on **W177** (procurement coordination worker), **W179/W194** (demand processing multi-source).

## Scope

### Settings (`DemandProcessingSettingsPanel`)

- Per-source help text for MaintainArr, RoutArr, TrainArr, StaffArr toggles
- Client-side validation: at least one source when worker enabled or auto PR draft enabled
- Server-side validation (`DemandProcessingRules.ValidateSettings`) on settings upsert

### Purchasing (`DemandProcessingPanel`)

- Split dashboard into **Pending queue** and **Recently processed**
- Per demand ref operator actions (requires `canCreatePr`):
  - **Retry processing** — `POST /api/demand-processing/{demandRefId}/retry-processing`
  - **Create PR draft** — `POST /api/demand-processing/{demandRefId}/create-pr-draft`
  - **View status** — `GET /api/demand-processing/{demandRefId}` with line availability
- Source link labels (product + reference key) on each row

### API (SupplyArr)

| Method | Path | Auth | Purpose |
|--------|------|------|---------|
| `GET` | `/api/demand-processing` | purchase read | Dashboard with `processedItems` + `pendingItems` |
| `GET` | `/api/demand-processing/{demandRefId}` | purchase read | Detail + line stock snapshot |
| `POST` | `/api/demand-processing/{demandRefId}/retry-processing` | purchase create | Force re-run worker evaluation |
| `POST` | `/api/demand-processing/{demandRefId}/create-pr-draft` | purchase create | Manual PR draft via intake services |

Audit: `supplyarr.demand_processing.retry`, `supplyarr.demand_processing.create_pr_draft`

## Tests

- `DemandProcessingRulesTests` — settings validation
- `SupplyArrDemandProcessingWorkerTests` — operator endpoints, dashboard queues, settings 400
- `DemandProcessingPanel.test.tsx` — pending queue + operator buttons
- `DemandProcessingSettingsPanel.test.tsx` — unchanged render path

## Verification

```powershell
dotnet test tests/STLCompliance.SupplyArr.Auth.Tests/STLCompliance.SupplyArr.Auth.Tests.csproj -c Release --filter "FullyQualifiedName~DemandProcessing"
cd apps/supplyarr-frontend
npm run test -- DemandProcessing
```

## Next slice

- **RoutArr M13** — driver portal Playwright (W213) → **W247 complete**; proof/DVIR dispatch read (W217)
- **MaintainArr M7/M12** — PM due-scan worker observability panel (W51)
- **SupplyArr M8** — procurement exception resolution workflow depth (W197 cluster)
