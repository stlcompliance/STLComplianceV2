# Worker 220 — Compliance Core M12 source ingestion workflow

## Slice name

M12 source ingestion workflow — persisted ingestion batches and per-row jobs, validate/commit APIs for fact sources (admin JWT) and product facts (service token), audit logging, Admin workspace `SourceIngestionPanel`, integration + Vitest tests

## Products touched

- **Compliance Core API** (`apps/compliancecore-api`): `SourceIngestionService`, `compliancecore_source_ingestion_batches` / `compliancecore_source_ingestion_jobs`, `/api/source-ingestion/*`, `/api/integrations/source-ingestion/product-facts/*`
- **Compliance Core Frontend** (`apps/compliancecore-frontend`): `SourceIngestionPanel` on Admin workspace
- **Shared** (`STLCompliance.Shared`): `supplyarr-compliancecore` token profile widened with `compliancecore.sources.ingest`
- **Integration tests** (`tests/STLCompliance.ComplianceCore.Auth.Tests`): `ComplianceCoreSourceIngestionTests`

## Schema

### Migration `ComplianceCoreSourceIngestion`

**`compliancecore_source_ingestion_batches`**

| Column | Notes |
|--------|-------|
| id | PK |
| tenant_id | |
| ingestion_type | `fact_sources` or `product_facts` |
| phase | `validate` or `commit` |
| dry_run | bool |
| status | `completed` / `partial` / `failed` |
| total_jobs | int |
| success_count, error_count, skipped_count | int |
| created_by_user_id | nullable (null for service-token ingest) |
| source_product, publication_id | nullable — product fact batches |
| created_at, completed_at | timestamptz |

**`compliancecore_source_ingestion_jobs`**

| Column | Notes |
|--------|-------|
| id | PK |
| batch_id | FK cascade |
| row_index | int |
| job_key | source_key or fact_key |
| status | validated / created / accepted / skipped / error |
| entity_type, entity_id | nullable |
| error_code, message | nullable |

## API + auth

### Admin JWT (Compliance Core entitlement)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/source-ingestion/batches` | read: entitled users (`RequireSourceIngestionRead`) |
| GET | `/api/source-ingestion/batches/{id}` | read |
| POST | `/api/source-ingestion/fact-sources/validate` | manage: `tenant_admin`, `compliance_admin` |
| POST | `/api/source-ingestion/fact-sources/commit` | manage |

Request body: `{ "sources": [ { factDefinitionId, sourceKey, sourceType, label, description, productKey?, productReference?, configJson, priority } ] }` — max 50 rows.

### Service token integration

| Method | Route | Auth |
|--------|-------|------|
| POST | `/api/integrations/source-ingestion/product-facts/validate` | `compliancecore.sources.ingest` |
| POST | `/api/integrations/source-ingestion/product-facts/commit` | same |

Body: `ProductFactBulkIngestionRequest` (tenant, publication, source product, publishedAt, facts[]). Commit delegates to existing `ProductFactIngestionService.IngestAsync`.

Existing single-shot `POST /api/integrations/product-facts/ingest` (`compliancecore.facts.ingest`) unchanged.

## Audit events

- `source_ingestion.fact_sources.validate`
- `source_ingestion.fact_sources.commit`
- `source_ingestion.product_facts.validate`
- `source_ingestion.product_facts.commit`

Entity: `source_ingestion_batch`; reasonCode: `{ingestionType}:{success}/{total}`.

## Frontend

- **SourceIngestionPanel** on Admin workspace (above 9-CSV panel)
- JSON editor for fact source batches; Validate batch / Commit batch
- Recent batch list; per-job results after run

## Tests

### Backend (`ComplianceCoreSourceIngestionTests`)

- `Fact_source_ingestion_validate_persists_batch_without_creating_sources`
- `Fact_source_ingestion_commit_creates_sources_and_lists_batch`
- `Fact_source_ingestion_denies_member_role`
- `Product_fact_source_ingestion_validate_and_commit_via_service_token`

### Frontend (`SourceIngestionPanel.test.tsx`)

- Manager controls visible
- Read-only notice for non-managers

## Verification

```powershell
dotnet build -c Release
dotnet test "tests/STLCompliance.ComplianceCore.Auth.Tests/STLCompliance.ComplianceCore.Auth.Tests.csproj" -c Release --filter "FullyQualifiedName~SourceIngestion"
cd apps/compliancecore-frontend
npm run test -- --run SourceIngestion
npm run build
```

## Remaining gaps

- CSV upload for fact source batches (JSON only in UI)
- Async worker-driven ingestion jobs (batch API is synchronous validate/commit)
- Rule change monitoring, risk scoring (other M12 backlog items)

## Next recommended slice

**Compliance Core M12** — rule change monitoring, or **NexArr M12** audit export enhancements, or **RoutArr M12** audit package export per `00_SLICE_STATE.md`.
