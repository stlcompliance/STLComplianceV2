# ReportArr — Dataset and Read Model Model

## Dataset

A Dataset is a logical reporting source or combined reporting model. It may be event-driven, scheduled, API-refreshed, or manually refreshed.

```text
ReportDataset
- datasetId
- tenantId
- datasetKey
- datasetNumber
- title
- description
- datasetType
  - source_product
  - cross_product
  - compliance
  - operational
  - audit
  - executive
  - custom
- status
  - draft
  - active
  - paused
  - rebuilding
  - failed
  - archived
- sourceProducts
- sourceConnectors
- refreshMode
  - event_driven
  - scheduled
  - manual
  - hybrid
- refreshFrequency
  - realtime
  - hourly
  - daily
  - weekly
  - monthly
  - manual
- freshnessStatus
  - fresh
  - slightly_stale
  - stale
  - failed
  - rebuilding
  - unknown
- lastRefreshedAt
- lastSuccessfulRefreshAt
- lastFailedRefreshAt
- schemaVersion
- fieldDefinitions
- sourceTraceabilityRules
- retentionPolicy
- ownerPersonId
- createdAt
- updatedAt
```

## Dataset field

```text
DatasetField
- fieldId
- datasetId
- fieldKey
- displayName
- description
- dataType
  - string
  - number
  - boolean
  - date
  - datetime
  - duration
  - enum
  - object_ref
  - money
  - percentage
  - json
- sourceProduct
- sourceFieldPath
- aggregationAllowed
- filterAllowed
- groupAllowed
- sortAllowed
- piiSensitive
- restricted
- complianceSensitive
```

## Source connector

A SourceConnector defines how ReportArr receives data from a product.

```text
SourceConnector
- sourceConnectorId
- tenantId
- sourceProduct
  - nexarr
  - staffarr
  - trainarr
  - compliancecore
  - maintainarr
  - loadarr
  - supplyarr
  - routarr
  - customarr
  - ordarr
  - recordarr
  - assurarr
  - fieldcompanion
- connectorType
  - event_stream
  - api_poll
  - webhook
  - batch_import
  - direct_export
- status
  - active
  - paused
  - failed
  - disabled
- serviceClientRef
- lastConnectedAt
- lastErrorAt
- lastErrorMessage
- supportedEventTypes
- supportedDatasets
```

## Ingestion cursor

```text
IngestionCursor
- ingestionCursorId
- tenantId
- sourceConnectorId
- sourceProduct
- cursorType
  - event_offset
  - timestamp
  - page_token
  - sequence
- cursorValue
- lastEventId
- lastEventAt
- lastIngestedAt
- status
  - active
  - paused
  - failed
```

## Source event receipt

```text
SourceEventReceipt
- sourceEventReceiptId
- tenantId
- sourceProduct
- sourceEventId
- eventType
- sourceObjectRef
- receivedAt
- processedAt
- status
  - received
  - processed
  - skipped
  - failed
  - duplicate
- failureReason
- correlationId
```

## Read model

A ReadModel is a materialized reporting model built from source facts.

```text
ReadModel
- readModelId
- tenantId
- readModelKey
- title
- description
- readModelType
  - fact_table
  - dimension
  - aggregate
  - snapshot
  - timeline
  - scorecard
  - audit_matrix
- status
  - active
  - rebuilding
  - stale
  - failed
  - archived
- datasetRefs
- schemaVersion
- primaryEntityType
- primarySourceProduct
- fieldDefinitions
- refreshJobRefs
- lastRebuiltAt
- lastUpdatedAt
```

## Read model record

```text
ReadModelRecord
- readModelRecordId
- tenantId
- readModelId
- primaryEntityRef
- data
- sourceTraces
- statusSnapshot
- effectiveAt
- lastSourceUpdatedAt
- ingestedAt
- updatedAt
```

## Refresh job

```text
RefreshJob
- refreshJobId
- tenantId
- datasetId
- readModelId
- refreshType
  - full
  - incremental
  - replay
  - repair
  - manual
- status
  - queued
  - running
  - completed
  - failed
  - canceled
- requestedByPersonId
- queuedAt
- startedAt
- completedAt
- recordsProcessed
- recordsCreated
- recordsUpdated
- recordsSkipped
- errorCount
- errorMessage
```

## Dataset lineage

```text
DatasetLineage
- lineageId
- datasetId
- sourceProduct
- sourceObjectType
- sourceFieldPath
- datasetFieldKey
- transformationDescription
- confidence
```

## Common read models

```text
PeopleReadinessReadModel
- StaffArr people + TrainArr qualifications + incidents/restrictions.

MaintenanceReadModel
- MaintainArr assets, work orders, inspections, PMs, defects, downtime.

InventoryReadModel
- LoadArr balances, receipts, counts, picks, issues, movements.

ProcurementReadModel
- SupplyArr suppliers, PRs, POs, lead times, supplier status.

TransportationReadModel
- RoutArr trips, routes, stops, ETAs, exceptions, proof events.

ComplianceReadModel
- Compliance Core evaluations + RecordArr evidence status + product facts.

QualityReadModel
- AssurArr NCRs, holds, CAPAs, audits, findings, scorecards.

OrderFulfillmentReadModel
- OrdArr orders + LoadArr fulfillment + RoutArr delivery + RecordArr proof.

CustomerReadModel
- CustomArr customer status + orders + complaints + delivery performance.

MobileExecutionReadModel
- Field Companion tasks, offline actions, sync failures, mobile completion facts.
```

## Dataset refresh workflow

```text
1. Source product emits event or refresh schedule triggers.
2. ReportArr receives source event or polls source API.
3. SourceEventReceipt is recorded.
4. Dataset/read model transformation runs.
5. ReadModelRecord is created/updated.
6. Freshness status updates.
7. Dashboards/reports using the dataset update.
```

## Failed ingestion workflow

```text
1. Event/API ingestion fails.
2. SourceEventReceipt or RefreshJob is marked failed.
3. Dataset freshness becomes stale/failed.
4. Reporting alert may be created.
5. Retry/replay/repair can be triggered.
6. Failure remains visible to admins.
```

## Events

```text
reportarr.dataset.created
reportarr.dataset.updated
reportarr.dataset.activated
reportarr.dataset.paused
reportarr.dataset.failed
reportarr.dataset.archived

reportarr.source_connector.created
reportarr.source_connector.connected
reportarr.source_connector.failed
reportarr.source_connector.paused

reportarr.source_event.received
reportarr.source_event.processed
reportarr.source_event.failed

reportarr.read_model.created
reportarr.read_model.rebuilding
reportarr.read_model.rebuilt
reportarr.read_model.failed
reportarr.read_model.stale

reportarr.refresh_job.queued
reportarr.refresh_job.started
reportarr.refresh_job.completed
reportarr.refresh_job.failed
```
