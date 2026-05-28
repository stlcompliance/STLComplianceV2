# Worker 147 — Companion field evidence capture (M11)

## Scope

Photo / document / signature capture on TrainArr field-inbox assignments through the NexArr companion API, proxying to TrainArr evidence storage (real `TrainArrEvidenceStorageService` upload path).

## Deliverables

| Area | Change |
|------|--------|
| **NexArr** | `POST /api/companion/field-tasks/evidence` — parses `trainarr:assignment:{id}` task keys, forwards bearer token to TrainArr `POST /api/training-assignments/{id}/evidence` |
| **Companion UI** | `FieldTaskEvidencePanel` on TrainArr tasks (photo/document/signature) |
| **Tests** | `NexArrCompanionFieldEvidenceTests`, `evidenceCapture.test.ts`, Playwright `companion-field-task-evidence.spec.ts` |
| **Catalog** | `StlE2ePlaywrightSpecCatalog.CompanionFieldTaskEvidenceSpec` |

## Verification

```powershell
dotnet test tests/STLCompliance.NexArr.Auth.Tests -c Release --filter "CompanionFieldEvidence"
cd apps/companion-frontend; npm test -- --run
dotnet test tests/STLCompliance.E2E -c Release --filter "Category=E2e&FullyQualifiedName~Companion"
```

## Out of scope

- MaintainArr / RoutArr / SupplyArr evidence (returns 409 until product APIs exist)
- Object-storage provider swap (TrainArr local `EvidenceStorageOptions` root remains product-owned)

## Related

- W27 — TrainArr program builder / evidence capture
- W146 — Companion offline queue
