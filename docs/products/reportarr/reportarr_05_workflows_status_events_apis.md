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
