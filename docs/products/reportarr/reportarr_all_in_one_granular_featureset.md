# ReportArr — Scope, Ownership, and Boundaries

## Product purpose

ReportArr is the reporting, dashboard, analytics, KPI, scheduled-report, export, and audit-report product for the STL Compliance / ARR suite.

ReportArr is not the operational source of truth. It consumes events, read APIs, snapshots, and product facts from source products and builds useful read models, dashboards, reports, exports, and analytical views.

ReportArr answers:

- What is the current cross-suite status?
- Which assets, people, orders, inventory, routes, quality issues, training items, or compliance items need attention?
- What KPIs are improving or worsening?
- What reports are scheduled?
- What report exports exist?
- What data is stale?
- Which source product produced each fact?
- Which source object should the user drill into?
- What is audit readiness across a scope?

## ReportArr owns

```text
- Report definitions
- Dashboard definitions
- Dashboard widgets
- KPI definitions
- Metric definitions
- Analytics/read models
- Dataset definitions
- Dataset refresh state
- Product event ingestion state
- Cross-product report runs
- Scheduled report delivery
- Export jobs
- Generated report output references
- Audit report packages
- Drilldown definitions
- Report access policies
- Data freshness indicators
- Source traceability
```

## ReportArr does not own

```text
- Platform login
- Tenant entitlement
- Person master
- Permission assignment truth
- Training completion truth
- Certification truth
- Regulatory/rulepack meaning
- Asset truth
- Work order truth
- Inventory balance
- Stock ledger
- Supplier/vendor master
- Procurement truth
- Route/trip execution truth
- Customer master
- Order lifecycle
- Document/file storage truth
- Quality hold/release truth
- Mobile task execution truth
- Accounting execution
```

## External product dependencies

```text
NexArr
- Product entitlement
- Login/handoff
- Service tokens
- Platform audit and access events

StaffArr
- People, org, sites, locations, permissions, incidents, readiness snapshots

TrainArr
- Training assignments, completions, qualifications, expirations, remediation

Compliance Core
- Rulepacks, compliance evaluations, requirement status, missing evidence, audit scope

MaintainArr
- Assets, defects, inspections, PMs, work orders, downtime, parts demand

LoadArr
- Inventory balances, receipts, putaway, reservations, picks, issues, counts, adjustments

SupplyArr
- Suppliers, purchase requests, purchase orders, sourcing, supplier status

RoutArr
- Routes, trips, stops, ETAs, delivery proof, transportation exceptions

CustomArr
- Customers, customer contacts, customer locations, customer requirements/issues

OrdArr
- Requests, orders, order lines, fulfillment dependencies, blockers, closure

RecordArr
- Generated report files, evidence packages, document status, record packages

AssurArr
- Nonconformance, quality holds, CAPA, audits, findings, quality scorecards

Field Companion
- Mobile task/action completion, sync failures, offline action metrics, capture metrics
```

## Core source-of-truth rules

```text
1. ReportArr owns reporting read models, not operational facts.
2. Source products remain authoritative for their domains.
3. ReportArr must preserve source product and source object traceability.
4. ReportArr must expose data freshness and source timestamps.
5. ReportArr should not mutate source product operational state.
6. ReportArr may request report exports and store generated files in RecordArr.
7. Compliance Core owns compliance meaning; ReportArr displays/report it.
8. RecordArr owns report output files.
9. NexArr/StaffArr control access to reporting surfaces.
10. ReportArr can aggregate across products but cannot become a shadow operational system.
```

## Standard ReportArr object envelope

```text
ReportArrObject
- id
- tenantId
- objectNumber
- objectType
- status
- title
- description
- ownerPersonId
- sourceProductRefs
- sourceDatasetRefs
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
- lastRunAt
- lastRefreshedAt
- auditTrail
- eventLog
```

## ReportArr object prefixes

```text
DS      Dataset
RM      Read model
SRC     Source connector
ING     Ingestion cursor
REF     Refresh job
KPI     KPI definition
MET     Metric definition
DASH    Dashboard
WID     Dashboard widget
RPT     Report definition
RUN     Report run
SCH     Report schedule
EXP     Export job
AUDR    Audit report package
DRL     Drilldown definition
ALRT    Reporting alert
FRESH   Data freshness status
```

## Standard source traceability

```text
SourceTrace
- sourceProduct
- sourceObjectType
- sourceObjectId
- sourceObjectNumber
- sourceEventId
- sourceEventType
- sourceUpdatedAt
- ingestedAt
- sourceStatusSnapshot
- displayNameSnapshot
```

## Data freshness rule

Every dashboard/report/read model should show or carry freshness metadata.

```text
FreshnessStatus
- fresh
- slightly_stale
- stale
- failed
- rebuilding
- unknown
```

## Standard report access rule

ReportArr should enforce:

```text
- NexArr product entitlement
- StaffArr person identity
- StaffArr permissions/roles
- Report-specific access policy
- Source-product sensitivity when required
- RecordArr access policy for generated exports
```


---


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
  - FieldCompanion
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


---


# ReportArr — Dashboard and Widget Model

## Dashboard

A Dashboard is an interactive reporting surface composed of widgets, filters, drilldowns, and access rules.

```text
Dashboard
- dashboardId
- tenantId
- dashboardNumber
- dashboardKey
- title
- description
- dashboardType
  - executive
  - product
  - compliance
  - maintenance
  - training
  - workforce
  - inventory
  - procurement
  - transportation
  - customer
  - order
  - quality
  - mobile
  - audit
  - custom
- status
  - draft
  - active
  - paused
  - archived
- ownerPersonId
- defaultDateRange
- widgetRefs
- filterRefs
- drilldownRefs
- accessPolicyRef
- freshnessStatus
- lastViewedAt
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
```

## Dashboard status definitions

```text
draft
- Dashboard is being configured.

active
- Dashboard is available to permitted users.

paused
- Dashboard exists but should not be shown by default.

archived
- Dashboard is retained for history.
```

## Dashboard widget

```text
DashboardWidget
- widgetId
- tenantId
- dashboardId
- widgetKey
- title
- description
- widgetType
  - metric
  - chart
  - table
  - trend
  - exception_list
  - map
  - timeline
  - status_card
  - heatmap
  - gauge
  - markdown
  - drilldown_link
- status
  - active
  - hidden
  - failed
  - archived
- datasetRef
- readModelRef
- queryDefinition
- visualizationSettings
- filterBindings
- drilldownTargetRef
- sortOrder
- layout
- freshnessStatus
- lastRenderedAt
```

## Widget visualization settings

```text
WidgetVisualizationSettings
- widgetId
- chartType
  - line
  - bar
  - stacked_bar
  - area
  - pie
  - donut
  - table
  - number
  - gauge
  - heatmap
  - map
  - timeline
- xField
- yField
- seriesField
- groupField
- valueField
- labelField
- dateField
- colorRuleRefs
- thresholdRefs
- displayFormat
- showLegend
- showDataLabels
- maxRows
```

## Dashboard filter

```text
DashboardFilter
- filterId
- dashboardId
- filterKey
- label
- filterType
  - date_range
  - product
  - site
  - department
  - person
  - asset
  - customer
  - supplier
  - status
  - severity
  - category
  - custom
- datasetFieldKey
- defaultValue
- allowedValuesSource
- required
- visible
```

## Drilldown definition

Drilldowns let a user go from a report number to source details.

```text
DrilldownDefinition
- drilldownId
- tenantId
- title
- description
- sourceWidgetRef
- targetType
  - dashboard
  - report
  - product_object
  - record_package
  - external_url
- targetRef
- parameterMappings
- requiredPermissionRefs
- status
```

## Reporting alert

A ReportingAlert is triggered by a metric, threshold, stale data, or exception query.

```text
ReportingAlert
- alertId
- tenantId
- alertNumber
- title
- description
- alertType
  - threshold
  - exception
  - stale_data
  - failed_refresh
  - scheduled_report_failed
  - compliance_gap
  - overdue_action
- status
  - active
  - triggered
  - acknowledged
  - resolved
  - muted
  - archived
- datasetRef
- metricRef
- condition
- severity
  - low
  - moderate
  - high
  - critical
- triggeredAt
- acknowledgedByPersonId
- acknowledgedAt
- resolvedAt
- notificationRefs
```

## Dashboard access policy

```text
DashboardAccessPolicy
- accessPolicyId
- tenantId
- dashboardId
- visibility
  - private
  - role_based
  - product_based
  - tenant_wide
  - auditor
- allowedPersonRefs
- allowedRoleRefs
- allowedPermissionRefs
- sourceProductRestrictions
- exportAllowed
- createdAt
- updatedAt
```

## Dashboard families

## Executive dashboard

```text
- Compliance readiness
- Open critical blockers
- Asset downtime
- Work order aging
- Training overdue
- Inventory health
- Route on-time rate
- Order fulfillment performance
- Supplier quality
- Customer complaint trend
- CAPA aging
```

## Maintenance dashboard

```text
- Asset readiness
- Open work orders
- Emergency work
- Waiting parts
- PM compliance
- Defect severity
- Downtime
- Technician workload
```

## Inventory dashboard

```text
- Inventory value snapshot/reference
- Stockout risk
- On-hand/available/reserved
- Receiving cycle time
- Putaway backlog
- Pick performance
- Count variance
- Holds/quarantine
```

## Training/workforce dashboard

```text
- Training completion
- Overdue assignments
- Expiring qualifications
- People readiness
- Incident-driven retraining
- Qualification coverage by site/role
```

## Transportation dashboard

```text
- On-time departure
- On-time arrival
- Active trips
- Late stops
- Route exceptions
- Proof capture rate
- Breakdown delays
```

## Quality dashboard

```text
- Open NCRs
- Active holds
- CAPA aging
- Audit findings
- Supplier quality issues
- Customer complaints
- Repeat nonconformances
```

## Compliance dashboard

```text
- Rulepack status
- Missing evidence
- Invalid evidence
- Expiring evidence
- Noncompliant objects
- Manual review queue
- Audit readiness
```

## Dashboard workflow

```text
1. User creates dashboard or selects template.
2. User adds widgets.
3. Widgets are bound to datasets/read models.
4. Filters and drilldowns are configured.
5. Access policy is configured.
6. Dashboard becomes active.
7. Users view dashboard.
8. Freshness and drilldown links remain visible.
```

## Widget render workflow

```text
1. User opens dashboard.
2. ReportArr validates access.
3. ReportArr resolves widget datasets.
4. QueryDefinition runs against read model.
5. Widget renders metric/chart/table/list.
6. Widget includes freshness/source metadata.
7. User can drill into source object if allowed.
```

## Events

```text
reportarr.dashboard.created
reportarr.dashboard.updated
reportarr.dashboard.activated
reportarr.dashboard.archived
reportarr.dashboard.viewed

reportarr.widget.created
reportarr.widget.updated
reportarr.widget.rendered
reportarr.widget.render_failed

reportarr.filter.created
reportarr.drilldown.created
reportarr.drilldown.used

reportarr.alert.created
reportarr.alert.triggered
reportarr.alert.acknowledged
reportarr.alert.resolved
reportarr.alert.muted
```


---


# ReportArr — Report Definition, Run, Schedule, and Export Model

## Report definition

A ReportDefinition defines a reusable report with parameters, datasets, layout, access policy, and export formats.

```text
ReportDefinition
- reportDefinitionId
- tenantId
- reportNumber
- reportKey
- title
- description
- reportType
  - operational
  - compliance
  - audit
  - executive
  - exception
  - scheduled
  - management_review
  - customer
  - supplier
  - product
  - custom
- status
  - draft
  - active
  - paused
  - archived
- datasetRefs
- readModelRefs
- parameterRefs
- defaultFilters
- layoutDefinition
- sectionRefs
- exportFormats
  - pdf
  - csv
  - xlsx
  - json
  - html
  - zip
- accessPolicyRef
- ownerPersonId
- createdAt
- createdByPersonId
- updatedAt
- updatedByPersonId
```

## Report parameter

```text
ReportParameter
- parameterId
- reportDefinitionId
- parameterKey
- label
- parameterType
  - date_range
  - single_select
  - multi_select
  - text
  - number
  - boolean
  - person
  - site
  - location
  - asset
  - customer
  - supplier
  - product
  - status
  - severity
- required
- defaultValue
- allowedValuesSource
- validationRules
```

## Report section

```text
ReportSection
- sectionId
- reportDefinitionId
- sequence
- title
- description
- sectionType
  - summary
  - table
  - chart
  - metric_grid
  - narrative
  - exception_list
  - evidence_matrix
  - source_trace
  - appendix
- datasetRef
- queryDefinition
- layoutSettings
```

## Report run

A ReportRun is one execution of a report definition.

```text
ReportRun
- reportRunId
- tenantId
- reportRunNumber
- reportDefinitionId
- status
  - queued
  - running
  - completed
  - completed_with_warnings
  - failed
  - canceled
- requestedByPersonId
- requestedAt
- startedAt
- completedAt
- parametersUsed
- filtersUsed
- outputFormat
- outputRecordRef
- outputPackageRef
- rowCount
- warningCount
- errorCount
- errorMessage
- sourceTraceSummary
- freshnessSummary
```

## Report run status definitions

```text
queued
- Report is waiting to execute.

running
- Report generation is active.

completed
- Report generated successfully.

completed_with_warnings
- Report generated but data freshness, missing sources, or partial issues exist.

failed
- Report did not generate.

canceled
- Report run was canceled.
```

## Report schedule

```text
ReportSchedule
- scheduleId
- tenantId
- reportDefinitionId
- title
- status
  - active
  - paused
  - canceled
  - expired
- cadence
  - hourly
  - daily
  - weekly
  - monthly
  - quarterly
  - annually
  - custom_cron
- timezone
- cronExpression
- nextRunAt
- lastRunAt
- startsAt
- endsAt
- parameters
- recipients
- deliveryMethod
  - email
  - recordarr_package
  - dashboard_notification
  - webhook
  - download_only
- createdByPersonId
- createdAt
- updatedAt
```

## Report recipient

```text
ReportRecipient
- recipientId
- scheduleId
- recipientType
  - person
  - email
  - role
  - team
  - external
  - webhook
- recipientRef
- email
- deliveryFormat
  - pdf
  - csv
  - xlsx
  - link
  - package
- status
  - active
  - inactive
```

## Export job

An ExportJob is a file-generation job that may be tied to a report, dashboard, table, audit package, or raw dataset.

```text
ExportJob
- exportJobId
- tenantId
- exportNumber
- exportType
  - report
  - dashboard
  - table
  - dataset
  - audit_package
  - chart
  - custom
- status
  - queued
  - running
  - completed
  - failed
  - canceled
  - expired
- requestedByPersonId
- requestedAt
- startedAt
- completedAt
- exportFormat
  - pdf
  - csv
  - xlsx
  - json
  - png
  - zip
- sourceRef
- outputRecordRef
- rowCount
- fileSizeBytesSnapshot
- expiresAt
- errorMessage
```

## Audit report package

An AuditReportPackage is a reporting-side audit view tied to Compliance Core and RecordArr evidence packages.

```text
AuditReportPackage
- auditReportPackageId
- tenantId
- packageNumber
- title
- description
- auditScope
- status
  - draft
  - assembling
  - complete
  - locked
  - archived
  - failed
- requestedByPersonId
- complianceEvaluationRefs
- sourceProductRefs
- sourceObjectRefs
- recordarrPackageRef
- reportRunRefs
- missingEvidenceSummary
- invalidEvidenceSummary
- readinessScore
- generatedAt
- lockedAt
```

## Audit scope

```text
AuditScope
- auditScopeId
- tenantId
- scopeType
  - rulepack
  - site
  - department
  - person
  - asset
  - vehicle
  - supplier
  - customer
  - order
  - date_range
  - incident
  - quality
  - custom
- dateRangeStart
- dateRangeEnd
- productFilters
- objectRefs
- rulepackRefs
- siteRefs
- departmentRefs
- includeEvidence
- includeSourceTrace
```

## Report access policy

```text
ReportAccessPolicy
- accessPolicyId
- tenantId
- reportDefinitionId
- visibility
  - private
  - role_based
  - permission_based
  - tenant_wide
  - auditor
- allowedPersonRefs
- allowedRoleRefs
- allowedPermissionRefs
- sourceProductRestrictions
- exportAllowed
- scheduleAllowed
- externalDeliveryAllowed
```

## Report run workflow

```text
1. User selects report definition.
2. ReportArr validates access.
3. User enters parameters.
4. ReportRun is queued.
5. ReportArr resolves datasets/read models.
6. Data freshness is checked.
7. Report sections render.
8. Export file is generated if requested.
9. Output is stored in RecordArr.
10. ReportRun completes or fails.
```

## Scheduled report workflow

```text
1. ReportSchedule reaches nextRunAt.
2. ReportArr creates ReportRun.
3. Report is generated.
4. Output file is stored in RecordArr.
5. Recipients are notified or delivery webhook is called.
6. Schedule calculates nextRunAt.
```

## Audit report workflow

```text
1. User selects audit scope.
2. ReportArr requests Compliance Core evaluations.
3. ReportArr requests evidence package state from RecordArr.
4. ReportArr aggregates source product facts.
5. AuditReportPackage is assembled.
6. RecordArr stores final package/export.
7. Package can be locked.
```

## Events

```text
reportarr.report_definition.created
reportarr.report_definition.updated
reportarr.report_definition.activated
reportarr.report_definition.archived

reportarr.report_run.queued
reportarr.report_run.started
reportarr.report_run.completed
reportarr.report_run.completed_with_warnings
reportarr.report_run.failed
reportarr.report_run.canceled

reportarr.schedule.created
reportarr.schedule.paused
reportarr.schedule.resumed
reportarr.schedule.canceled
reportarr.schedule.run_triggered

reportarr.export.queued
reportarr.export.started
reportarr.export.completed
reportarr.export.failed
reportarr.export.expired

reportarr.audit_package.created
reportarr.audit_package.assembling
reportarr.audit_package.completed
reportarr.audit_package.locked
reportarr.audit_package.failed
```


---


# ReportArr — KPI, Metric, Analytics, and Alert Model

## KPI definition

A KPI definition is a named performance indicator with formula, thresholds, source datasets, and display rules.

```text
KpiDefinition
- kpiId
- tenantId
- kpiNumber
- kpiKey
- title
- description
- category
  - compliance
  - maintenance
  - training
  - workforce
  - inventory
  - procurement
  - transportation
  - customer
  - order
  - quality
  - mobile
  - platform
  - executive
- status
  - draft
  - active
  - paused
  - archived
- formula
- sourceDatasetRefs
- sourceMetricRefs
- targetValue
- warningThreshold
- criticalThreshold
- higherIsBetter
- displayFormat
  - number
  - percentage
  - duration
  - currency_reference
  - count
  - rate
  - score
- ownerPersonId
- createdAt
- updatedAt
```

## Metric definition

A MetricDefinition is a reusable calculation that can support KPIs, dashboards, and reports.

```text
MetricDefinition
- metricId
- tenantId
- metricKey
- title
- description
- metricType
  - count
  - sum
  - average
  - median
  - percentage
  - ratio
  - duration
  - aging
  - trend
  - score
- sourceDatasetRef
- fieldRefs
- formula
- filterDefinition
- groupingOptions
- dateField
- status
```

## KPI value

```text
KpiValue
- kpiValueId
- tenantId
- kpiId
- periodStart
- periodEnd
- value
- targetValueSnapshot
- warningThresholdSnapshot
- criticalThresholdSnapshot
- status
  - good
  - warning
  - critical
  - unknown
- trend
  - improving
  - stable
  - worsening
  - unknown
- sourceTraceSummary
- calculatedAt
```

## Metric value

```text
MetricValue
- metricValueId
- tenantId
- metricId
- periodStart
- periodEnd
- value
- groupKey
- groupLabel
- sourceTraceSummary
- calculatedAt
```

## Analytics snapshot

An AnalyticsSnapshot freezes a point-in-time view of metrics/read models for trend and audit repeatability.

```text
AnalyticsSnapshot
- analyticsSnapshotId
- tenantId
- snapshotNumber
- snapshotType
  - daily
  - weekly
  - monthly
  - quarterly
  - audit
  - manual
- status
  - created
  - completed
  - failed
  - archived
- periodStart
- periodEnd
- datasetRefs
- kpiValueRefs
- metricValueRefs
- generatedAt
- generatedBy
  - system
  - person
```

## Trend analysis

```text
TrendAnalysis
- trendAnalysisId
- tenantId
- metricRef
- kpiRef
- periodStart
- periodEnd
- trend
  - improving
  - stable
  - worsening
  - volatile
  - unknown
- changeValue
- changePercent
- confidence
  - low
  - medium
  - high
- explanation
- generatedAt
```

## Exception query

Exception queries identify records needing attention.

```text
ExceptionQuery
- exceptionQueryId
- tenantId
- queryKey
- title
- description
- sourceDatasetRef
- condition
- severity
  - low
  - moderate
  - high
  - critical
- status
  - active
  - paused
  - archived
- ownerPersonId
```

## Exception result

```text
ExceptionResult
- exceptionResultId
- exceptionQueryId
- tenantId
- sourceObjectRef
- title
- summary
- severity
- status
  - open
  - acknowledged
  - resolved
  - dismissed
- detectedAt
- acknowledgedByPersonId
- acknowledgedAt
- resolvedAt
- sourceTrace
```

## Common KPI families

## Platform and access

```text
- Login failure rate
- Product launch denial count
- Active tenant count
- Entitlement suspension count
- Service token failure count
- Suspicious activity count
```

## Staff/workforce

```text
- Active headcount
- People readiness rate
- Permission review overdue count
- Personnel incident count
- Active restriction count
- Open corrective action count
```

## Training

```text
- Training completion rate
- Overdue assignment count
- Expiring qualification count
- Expired qualification count
- Remediation assignment count
- Qualification coverage by role/site
```

## Compliance

```text
- Compliance status by rulepack
- Missing evidence count
- Invalid evidence count
- Expiring evidence count
- Manual review queue count
- Audit readiness score
```

## Maintenance

```text
- Asset readiness rate
- Open work order count
- Work order aging
- Waiting parts count
- PM compliance rate
- Overdue PM count
- Defect count by severity
- Asset downtime hours
- Repeat defect rate
```

## Inventory/WMS

```text
- Inventory accuracy rate
- Count variance value/quantity
- Stockout count
- Reservation fill rate
- Receiving cycle time
- Putaway cycle time
- Pick accuracy
- Held/quarantined quantity
```

## Procurement

```text
- Purchase request aging
- PO cycle time
- Supplier on-time rate
- Supplier quality issue count
- Average lead time
- Emergency purchase count
```

## Transportation

```text
- On-time departure rate
- On-time arrival rate
- Active late stops
- Route exception count
- Proof capture rate
- Breakdown delay count
```

## Customer/order

```text
- Order fulfillment rate
- Order blocker count
- Customer SLA performance
- Customer complaint count
- Customer issue aging
- On-time delivery by customer
```

## Quality

```text
- Open NCR count
- Active hold count
- Hold aging
- CAPA aging
- Overdue CAPA count
- Ineffective CAPA count
- Audit finding count
- Repeat nonconformance rate
```

## Mobile execution

```text
- Mobile task completion rate
- Offline action count
- Sync failure count
- Capture completion rate
- Secure upload completion rate
- Field task aging
```

## KPI calculation workflow

```text
1. Dataset/read model refresh completes.
2. MetricDefinition calculation runs.
3. MetricValue records are created.
4. KpiDefinition formula runs.
5. KpiValue is created.
6. Threshold status is assigned.
7. TrendAnalysis may run.
8. Dashboards/reports update.
```

## Exception workflow

```text
1. ExceptionQuery runs.
2. Matching source objects create ExceptionResults.
3. ReportingAlert may trigger.
4. User acknowledges or resolves.
5. Source product remains owner of actual correction.
```

## Events

```text
reportarr.kpi_definition.created
reportarr.kpi_definition.updated
reportarr.kpi_value.calculated
reportarr.metric_definition.created
reportarr.metric_value.calculated
reportarr.analytics_snapshot.created
reportarr.analytics_snapshot.completed
reportarr.trend_analysis.completed
reportarr.exception_query.created
reportarr.exception_result.created
reportarr.exception_result.acknowledged
reportarr.exception_result.resolved
```


---


# ReportArr — Workflows, Status Logic, Events, and APIs

## Major workflow: source event ingestion

```text
1. Source product emits domain event.
2. ReportArr receives event.
3. SourceEventReceipt is created.
4. ReportArr validates event schema/version.
5. Event is transformed into read model updates.
6. ReadModelRecord source trace is preserved.
7. Dataset freshness updates.
8. Related widgets/reports/KPIs become current or refreshable.
```

## Major workflow: scheduled refresh

```text
1. Dataset refresh schedule triggers.
2. ReportArr creates RefreshJob.
3. ReportArr queries source product read APIs.
4. Records are transformed into read model records.
5. Dataset freshness updates.
6. KPI/metric calculations run if configured.
7. Dashboards show updated data.
```

## Major workflow: dashboard viewing

```text
1. User opens dashboard.
2. ReportArr validates NexArr entitlement and StaffArr permissions.
3. Dashboard access policy is checked.
4. Widgets query read models.
5. Widgets render with freshness and source trace.
6. User drills down into source product if permitted.
```

## Major workflow: report run

```text
1. User selects ReportDefinition.
2. User enters parameters.
3. ReportArr creates ReportRun.
4. ReportArr queries datasets/read models.
5. Report sections are generated.
6. Export file is created if requested.
7. RecordArr stores output.
8. ReportRun completes.
```

## Major workflow: scheduled report

```text
1. ReportSchedule reaches nextRunAt.
2. ReportArr creates ReportRun.
3. Report is generated.
4. Output is stored in RecordArr.
5. Recipients are notified.
6. Next run is scheduled.
```

## Major workflow: audit readiness report

```text
1. User selects audit scope.
2. ReportArr requests Compliance Core evaluations.
3. ReportArr requests RecordArr evidence package status.
4. ReportArr gathers product facts from read models.
5. Missing/invalid/expiring evidence is summarized.
6. AuditReportPackage is assembled.
7. RecordArr stores final export/package.
8. Package can be locked.
```

## Major workflow: KPI calculation

```text
1. Dataset/read model is refreshed.
2. Metric calculations run.
3. KPI calculations run.
4. Thresholds are evaluated.
5. Trend analysis runs.
6. Alerts/exception results are created if needed.
7. Dashboards and reports update.
```

## Major workflow: stale data alert

```text
1. Dataset exceeds freshness threshold.
2. FreshnessStatus becomes stale or failed.
3. ReportingAlert is created.
4. Admin is notified.
5. Refresh/replay/repair is performed.
6. Alert resolves after successful refresh.
```

## ReportArr emitted events

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

reportarr.dashboard.created
reportarr.dashboard.updated
reportarr.dashboard.activated
reportarr.dashboard.viewed
reportarr.dashboard.archived

reportarr.widget.created
reportarr.widget.rendered
reportarr.widget.render_failed

reportarr.report_definition.created
reportarr.report_definition.updated
reportarr.report_definition.activated
reportarr.report_definition.archived

reportarr.report_run.queued
reportarr.report_run.started
reportarr.report_run.completed
reportarr.report_run.completed_with_warnings
reportarr.report_run.failed
reportarr.report_run.canceled

reportarr.schedule.created
reportarr.schedule.paused
reportarr.schedule.resumed
reportarr.schedule.canceled
reportarr.schedule.run_triggered

reportarr.export.queued
reportarr.export.started
reportarr.export.completed
reportarr.export.failed

reportarr.audit_package.created
reportarr.audit_package.completed
reportarr.audit_package.locked

reportarr.kpi_value.calculated
reportarr.metric_value.calculated
reportarr.alert.triggered
reportarr.alert.resolved
```

## Integration APIs ReportArr should expose

```text
GET /api/v1/integrations/datasets
GET /api/v1/integrations/datasets/{datasetId}
POST /api/v1/integrations/datasets
POST /api/v1/integrations/datasets/{datasetId}/refresh

GET /api/v1/integrations/read-models
GET /api/v1/integrations/read-models/{readModelId}
POST /api/v1/integrations/read-models/{readModelId}/rebuild

POST /api/v1/integrations/events
POST /api/v1/integrations/events/batch

GET /api/v1/integrations/dashboards
GET /api/v1/integrations/dashboards/{dashboardId}
POST /api/v1/integrations/dashboards
PATCH /api/v1/integrations/dashboards/{dashboardId}

GET /api/v1/integrations/widgets/{widgetId}/render

GET /api/v1/integrations/report-definitions
GET /api/v1/integrations/report-definitions/{reportDefinitionId}
POST /api/v1/integrations/report-definitions

POST /api/v1/integrations/report-runs
GET /api/v1/integrations/report-runs/{reportRunId}
POST /api/v1/integrations/report-runs/{reportRunId}/cancel

GET /api/v1/integrations/report-schedules
POST /api/v1/integrations/report-schedules
PATCH /api/v1/integrations/report-schedules/{scheduleId}

POST /api/v1/integrations/exports
GET /api/v1/integrations/exports/{exportJobId}

GET /api/v1/integrations/kpis
GET /api/v1/integrations/kpis/{kpiId}
POST /api/v1/integrations/kpis/{kpiId}/calculate

POST /api/v1/integrations/audit-packages
GET /api/v1/integrations/audit-packages/{auditReportPackageId}

GET /api/v1/integrations/alerts
POST /api/v1/integrations/alerts/{alertId}/acknowledge
POST /api/v1/integrations/alerts/{alertId}/resolve
```

## APIs ReportArr should consume

```text
NexArr
- POST /handoff/redeem
- POST /service-tokens/introspect
- GET /entitlements/{productKey}
- GET /audit

StaffArr
- GET /persons
- GET /persons/{personId}
- GET /persons/{personId}/readiness
- GET /org-units
- GET /locations
- GET /incidents

TrainArr
- GET /assignments
- GET /qualifications
- GET /certificates

Compliance Core
- GET /rulepacks
- GET /evaluations
- POST /evaluations
- POST /tse/evaluate

MaintainArr
- GET /assets
- GET /work-orders
- GET /defects
- GET /inspections
- GET /pm-occurrences
- GET /downtime

LoadArr
- GET /items
- GET /balances
- GET /receipts
- GET /stock-movements
- GET /counts
- GET /adjustments
- GET /discrepancies

SupplyArr
- GET /suppliers
- GET /purchase-requests
- GET /purchase-orders
- GET /sourcing-records

RoutArr
- GET /routes
- GET /trips
- GET /stops
- GET /exceptions
- GET /proof-events

CustomArr
- GET /customers
- GET /customer-locations
- GET /customer-issues

OrdArr
- GET /requests
- GET /orders
- GET /order-lines
- GET /fulfillment-records
- GET /blockers

RecordArr
- POST /records
- POST /record-packages
- GET /record-packages/{packageId}

AssurArr
- GET /nonconformances
- GET /holds
- GET /capas
- GET /audits
- GET /findings
- GET /scorecards

Field Companion
- GET /mobile/tasks
- GET /offline-actions/status
- GET /sync-failures
```

## Permission examples

```text
reportarr.dashboards.read
reportarr.dashboards.create
reportarr.dashboards.update
reportarr.dashboards.archive

reportarr.widgets.manage

reportarr.reports.read
reportarr.reports.create
reportarr.reports.update
reportarr.reports.run
reportarr.reports.schedule
reportarr.reports.export

reportarr.datasets.read
reportarr.datasets.manage
reportarr.datasets.refresh
reportarr.read_models.rebuild

reportarr.kpis.read
reportarr.kpis.manage
reportarr.metrics.read
reportarr.metrics.manage

reportarr.audit_reports.create
reportarr.audit_reports.lock

reportarr.alerts.read
reportarr.alerts.manage
reportarr.alerts.acknowledge
reportarr.alerts.resolve

reportarr.admin
```

## Default role examples

```text
Report Viewer
- View permitted dashboards and reports.

Report Runner
- Run reports and export allowed outputs.

Report Builder
- Create dashboards, widgets, reports, and report definitions.

Report Scheduler
- Create and manage scheduled reports.

Analytics Admin
- Manage datasets, read models, KPIs, metrics, and refresh jobs.

Executive Viewer
- View executive dashboards and high-level reports.

Compliance Reporter
- Run compliance/audit reports and audit packages.

Operations Reporter
- View operations dashboards across maintenance, inventory, routes, orders, quality.

ReportArr Admin
- Manage all ReportArr settings, datasets, reports, schedules, alerts, and access policies.
```

## ReportArr UI surfaces

```text
/app/reportarr
- dashboard
- dashboards
- dashboard detail
- reports
- report builder
- report run detail
- schedules
- exports
- datasets
- read models
- refresh jobs
- KPIs
- metrics
- alerts
- audit packages
- source connectors
- ingestion status
- settings
```

## Dashboard detail UI

```text
DashboardDetailPage
- Header
- Filters
- Widget grid
- Freshness indicator
- Source trace summary
- Drilldowns
- Export action
- Access settings
```

## Report builder UI

```text
ReportBuilderPage
- Report metadata
- Dataset selection
- Parameters
- Sections
- Layout
- Filters
- Export formats
- Access policy
- Preview
- Save/activate
```

## Dataset detail UI

```text
DatasetDetailPage
- Dataset header
- Source products
- Schema/fields
- Freshness
- Refresh history
- Ingestion errors
- Lineage
- Read models
- Dependent dashboards/reports
```

## Alert detail UI

```text
AlertDetailPage
- Alert header
- Trigger condition
- Severity
- Source dataset/metric
- Trigger history
- Acknowledgement/resolution
- Related dashboards/reports
```
