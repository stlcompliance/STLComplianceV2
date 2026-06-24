# ReportArr — Production Safety, Builder, Lineage, and Navigation

## Audit mandate

Persist tenant-scoped datasets, connectors, read models, dashboards, widgets, report definitions/versions, schedules, recipients, runs, exports, metrics, KPIs, alerts, audit scopes/packages, lineage, and worker cursors. Remove global process-local lists.

## Security and provenance

Enforce source-product, row, column, record, and field sensitivity during read-model materialization and query—not only rendering. Every output includes source, definition version, filters, as-of/freshness, permission scope, and content hash.

## Durable execution

Schedules run through durable workers with leases, retries, idempotency, and recovery after restart. Outputs suitable as evidence are archived to RecordArr with immutable lineage.

## Report builder

Use a clear multi-tab builder: Data, Fields, Filters, Grouping & Calculations, Layout, Access & Delivery, Preview. Mapping uses owner-backed datasets and human field labels. Validation identifies unavailable sources and unsupported joins before save.

## Pages

Provide dashboards, report library, builder, run history, schedules, metrics/KPIs, alerts, audit packages, datasets/lineage, and administration. Queue/runs show queued/running/partial/failed/complete truthfully.
