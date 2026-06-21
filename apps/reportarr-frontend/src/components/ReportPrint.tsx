import { FileDown } from 'lucide-react'
import { useState, type ReactNode } from 'react'
import {
  DraftWatermark,
  PacketPreview,
  PrintableDocumentHeader,
  PrintablePageShell,
  downloadPrintPdf,
  type PrintDocumentRequest,
} from '@stl/shared-ui'
import type {
  ReportArrAuditPackageResponse,
  ReportArrDashboardAccessPolicyResponse,
  ReportArrDashboardFilterResponse,
  ReportArrDashboardResponse,
  ReportArrDashboardWidgetResponse,
  ReportArrDrilldownDefinitionResponse,
  ReportArrExportJobResponse,
  ReportArrReportDefinitionResponse,
  ReportArrReportParameterResponse,
  ReportArrReportRecipientResponse,
  ReportArrReportRunResponse,
  ReportArrReportScheduleResponse,
  ReportArrReportSectionResponse,
} from '../api/types'

const printApiBase =
  import.meta.env.VITE_REPORTARR_API_BASE ??
  (typeof globalThis.location?.origin === 'string' ? globalThis.location.origin : '')

type AuditToolbarProps = {
  accessToken: string
  auditPackage: ReportArrAuditPackageResponse
  actorDisplayName?: string
  tenantDisplayName?: string
}

type DashboardPreviewProps = {
  dashboard: ReportArrDashboardResponse
  policy: ReportArrDashboardAccessPolicyResponse | null
  filters: ReportArrDashboardFilterResponse[]
  drilldowns: ReportArrDrilldownDefinitionResponse[]
  widgets: ReportArrDashboardWidgetResponse[]
  actorDisplayName?: string
  tenantDisplayName?: string
}

type ReportRunPreviewProps = {
  reportRun: ReportArrReportRunResponse
  definition: ReportArrReportDefinitionResponse | null
  reportParameters: ReportArrReportParameterResponse[]
  reportSections: ReportArrReportSectionResponse[]
  exportJobs: ReportArrExportJobResponse[]
  actorDisplayName?: string
  tenantDisplayName?: string
}

type ReportSchedulePreviewProps = {
  schedule: ReportArrReportScheduleResponse
  definition: ReportArrReportDefinitionResponse | null
  recipients: ReportArrReportRecipientResponse[]
  actorDisplayName?: string
  tenantDisplayName?: string
}

type AuditPackagePreviewProps = {
  auditPackage: ReportArrAuditPackageResponse
  linkedRuns: ReportArrReportRunResponse[]
  actorDisplayName?: string
  tenantDisplayName?: string
}

function buttonClassName() {
  return 'inline-flex items-center gap-2 rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm font-medium text-slate-900 transition hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-60'
}

function triggerDownload(blob: Blob, fileName: string | null) {
  const objectUrl = URL.createObjectURL(blob)
  const anchor = document.createElement('a')
  anchor.href = objectUrl
  anchor.download = fileName ?? 'reportarr-print.pdf'
  document.body.appendChild(anchor)
  anchor.click()
  document.body.removeChild(anchor)
  URL.revokeObjectURL(objectUrl)
}

function formatDateTime(value: string | null | undefined) {
  if (!value) {
    return 'Not set'
  }

  const parsed = new Date(value)
  if (Number.isNaN(parsed.getTime())) {
    return value
  }

  return parsed.toLocaleString(undefined, {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: 'numeric',
    minute: '2-digit',
    timeZoneName: 'short',
  })
}

function formatToken(value: string | null | undefined) {
  const trimmed = value?.trim()
  if (!trimmed) {
    return 'Not set'
  }

  return trimmed
    .replace(/[_-]+/g, ' ')
    .replace(/\s+/g, ' ')
    .replace(/\b\w/g, (character) => character.toUpperCase())
}

function safeText(value: string | null | undefined, fallback = 'None recorded') {
  const trimmed = value?.trim()
  return trimmed && trimmed.length > 0 ? trimmed : fallback
}

function buildMetadataJson(
  title: string,
  sourceDisplayRef: string,
  actorDisplayName?: string,
  tenantDisplayName?: string,
) {
  return JSON.stringify({
    actorDisplayName,
    tenantDisplayName,
    title,
    sourceDisplayRef,
  })
}

function buildRequest(
  sourceEntityType: string,
  sourceEntityId: string,
  sourceDisplayRef: string,
  title: string,
  templateKey: string,
  documentStatus: PrintDocumentRequest['documentStatus'],
  actorDisplayName?: string,
  tenantDisplayName?: string,
): PrintDocumentRequest {
  return {
    sourceEntityType,
    sourceEntityId,
    sourceDisplayRef,
    templateKey,
    documentStatus,
    optionsJson: buildMetadataJson(title, sourceDisplayRef, actorDisplayName, tenantDisplayName),
  }
}

function SummaryCard({
  title,
  children,
}: {
  title: string
  children: ReactNode
}) {
  return (
    <section className="rounded-lg border border-slate-200 p-4">
      <h3 className="text-sm font-semibold uppercase tracking-wide text-slate-500">{title}</h3>
      <div className="mt-3">{children}</div>
    </section>
  )
}

function DefinitionList({ rows }: { rows: Array<{ label: string; value: string }> }) {
  return (
    <dl className="space-y-2 text-sm text-slate-700">
      {rows.map((row) => (
        <div key={row.label}>
          <dt className="font-medium text-slate-950">{row.label}</dt>
          <dd>{row.value}</dd>
        </div>
      ))}
    </dl>
  )
}

function BulletList({
  items,
  emptyLabel,
}: {
  items: string[]
  emptyLabel: string
}) {
  return (
    <ul className="space-y-2 text-sm text-slate-700">
      {items.length > 0 ? items.map((item) => <li key={item}>{item}</li>) : <li>{emptyLabel}</li>}
    </ul>
  )
}

function WatermarkedPrintShell({
  title,
  subtitle,
  footer,
  children,
}: {
  title: string
  subtitle: string
  footer: ReactNode
  children: ReactNode
}) {
  return (
    <div className="relative overflow-hidden rounded-xl">
      <DraftWatermark label="Working copy" />
      <PrintablePageShell title={title} subtitle={subtitle} footer={footer}>
        <div className="relative space-y-6">{children}</div>
      </PrintablePageShell>
    </div>
  )
}

function buildPreviewFooter(actorDisplayName?: string) {
  return (
    <div className="space-y-1">
      <p>Generated {formatDateTime(new Date().toISOString())} by {actorDisplayName || 'Authorized user'}.</p>
      <p>This preview hides workspace chrome and labels the output as a working copy.</p>
    </div>
  )
}

function summarizeRecipients(recipients: ReportArrReportRecipientResponse[]) {
  if (recipients.length === 0) {
    return 'No recipients configured'
  }

  const counts = recipients.reduce<Record<string, number>>((accumulator, recipient) => {
    const key = recipient.recipientType || 'unknown'
    accumulator[key] = (accumulator[key] ?? 0) + 1
    return accumulator
  }, {})

  return Object.entries(counts)
    .map(([key, count]) => `${count} ${formatToken(key).toLowerCase()} recipient${count === 1 ? '' : 's'}`)
    .join(', ')
}

export function AuditPackagePrintToolbarActions({
  accessToken,
  auditPackage,
  actorDisplayName,
  tenantDisplayName,
}: AuditToolbarProps) {
  const [pendingAction, setPendingAction] = useState<string | null>(null)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  async function startDownload(
    actionKey: string,
    templateKey: string,
    documentStatus: PrintDocumentRequest['documentStatus'],
  ) {
    if (!accessToken || !printApiBase) {
      return
    }

    setPendingAction(actionKey)
    setErrorMessage(null)
    try {
      const file = await downloadPrintPdf(
        printApiBase,
        accessToken,
        buildRequest(
          'audit_package',
          auditPackage.auditReportPackageId,
          auditPackage.packageNumber,
          auditPackage.title,
          templateKey,
          documentStatus,
          actorDisplayName,
          tenantDisplayName,
        ),
      )
      triggerDownload(file.blob, file.fileName)
    } catch (error) {
      setErrorMessage(error instanceof Error ? error.message : 'Print export failed.')
    } finally {
      setPendingAction(null)
    }
  }

  return (
    <>
      <button
        type="button"
        className={buttonClassName()}
        disabled={pendingAction === 'readiness'}
        onClick={() => {
          void startDownload('readiness', 'reportarr.compliance_readiness.packet', 'copy')
        }}
      >
        <FileDown className="h-4 w-4" />
        {pendingAction === 'readiness' ? 'Preparing readiness packet...' : 'Download Readiness Packet'}
      </button>
      <button
        type="button"
        className={buttonClassName()}
        disabled={pendingAction === 'summary'}
        onClick={() => {
          void startDownload('summary', 'reportarr.management.summary', 'copy')
        }}
      >
        <FileDown className="h-4 w-4" />
        {pendingAction === 'summary' ? 'Preparing summary...' : 'Download Management Summary'}
      </button>
      {errorMessage ? <span className="text-sm text-rose-700">{errorMessage}</span> : null}
    </>
  )
}

export function DashboardPrintPreview({
  dashboard,
  policy,
  filters,
  drilldowns,
  widgets,
  actorDisplayName,
  tenantDisplayName,
}: DashboardPreviewProps) {
  const sourceDatasetCount = new Set(widgets.map((widget) => widget.datasetRef).filter(Boolean)).size
  const sourceReadModelCount = new Set(widgets.map((widget) => widget.readModelRef).filter(Boolean)).size

  return (
    <WatermarkedPrintShell
      title={`${dashboard.title} dashboard snapshot`}
      subtitle={`${tenantDisplayName || 'Current tenant workspace'} · ReportArr · ${dashboard.dashboardNumber}`}
      footer={buildPreviewFooter(actorDisplayName)}
    >
      <PrintableDocumentHeader
        title={dashboard.title}
        metadata={
          <div className="flex flex-wrap items-center gap-2">
            <span className="inline-flex items-center rounded-full border border-slate-300 bg-white px-3 py-1 text-xs font-semibold uppercase tracking-wide text-slate-700">
              {dashboard.dashboardNumber}
            </span>
            <span className="inline-flex items-center rounded-full border border-slate-300 bg-white px-3 py-1 text-xs font-semibold uppercase tracking-wide text-slate-700">
              {formatToken(dashboard.freshnessStatus)}
            </span>
            <span className="inline-flex items-center rounded-full border border-slate-300 bg-white px-3 py-1 text-xs font-semibold uppercase tracking-wide text-slate-700">
              {widgets.length} widget{widgets.length === 1 ? '' : 's'}
            </span>
          </div>
        }
      />

      <section className="grid gap-4 md:grid-cols-2">
        <SummaryCard title="Snapshot summary">
          <DefinitionList
            rows={[
              { label: 'Dashboard key', value: dashboard.dashboardKey },
              { label: 'Dashboard type', value: formatToken(dashboard.dashboardType) },
              { label: 'Status', value: formatToken(dashboard.status) },
              { label: 'Freshness', value: formatToken(dashboard.freshnessStatus) },
              { label: 'Default date range', value: dashboard.defaultDateRange },
              { label: 'Last viewed', value: formatDateTime(dashboard.lastViewedAt) },
            ]}
          />
        </SummaryCard>
        <SummaryCard title="Source boundaries">
          <DefinitionList
            rows={[
              { label: 'Dataset sources referenced', value: String(sourceDatasetCount) },
              { label: 'Read model sources referenced', value: String(sourceReadModelCount) },
              { label: 'Filters configured', value: String(filters.length) },
              { label: 'Drilldowns configured', value: String(drilldowns.length) },
              { label: 'Export policy', value: policy?.exportAllowed ? 'Allowed' : 'Blocked' },
              { label: 'Policy visibility', value: formatToken(policy?.visibility) },
            ]}
          />
        </SummaryCard>
      </section>

      <SummaryCard title="Approved dashboard notes">
        <BulletList
          items={[
            safeText(dashboard.description, 'No dashboard description provided.'),
            `Ownership: ReportArr owns this dashboard snapshot and export surface.`,
            `Reference-only data: underlying operational records remain owned by the source products feeding the datasets and read models.`,
          ]}
          emptyLabel="No approved notes are available."
        />
      </SummaryCard>

      <section className="grid gap-4 md:grid-cols-2">
        <SummaryCard title="Widget summary">
          <BulletList
            items={widgets.map((widget) =>
              `${widget.title} · ${formatToken(widget.widgetType)} · ${formatToken(widget.status)} · freshness ${formatToken(widget.freshnessStatus)}`,
            )}
            emptyLabel="No widgets are configured for this dashboard."
          />
        </SummaryCard>
        <SummaryCard title="Filters and drilldowns">
          <BulletList
            items={[
              ...filters.map((filter) =>
                `${filter.label} · ${formatToken(filter.filterType)} · required ${filter.required ? 'yes' : 'no'} · default ${safeText(filter.defaultValue, 'none')}`,
              ),
              ...drilldowns.map((drilldown) =>
                `${drilldown.title} · ${formatToken(drilldown.targetType)} · ${formatToken(drilldown.status)}`,
              ),
            ]}
            emptyLabel="No filters or drilldowns are configured."
          />
        </SummaryCard>
      </section>
    </WatermarkedPrintShell>
  )
}

export function ReportRunPrintPreview({
  reportRun,
  definition,
  reportParameters,
  reportSections,
  exportJobs,
  actorDisplayName,
  tenantDisplayName,
}: ReportRunPreviewProps) {
  return (
    <WatermarkedPrintShell
      title={`${reportRun.title} report preview`}
      subtitle={`${tenantDisplayName || 'Current tenant workspace'} · ReportArr · ${reportRun.reportRunNumber}`}
      footer={buildPreviewFooter(actorDisplayName)}
    >
      <PrintableDocumentHeader
        title={reportRun.title}
        metadata={
          <div className="flex flex-wrap items-center gap-2">
            <span className="inline-flex items-center rounded-full border border-slate-300 bg-white px-3 py-1 text-xs font-semibold uppercase tracking-wide text-slate-700">
              {reportRun.reportRunNumber}
            </span>
            <span className="inline-flex items-center rounded-full border border-slate-300 bg-white px-3 py-1 text-xs font-semibold uppercase tracking-wide text-slate-700">
              {formatToken(reportRun.status)}
            </span>
            <span className="inline-flex items-center rounded-full border border-slate-300 bg-white px-3 py-1 text-xs font-semibold uppercase tracking-wide text-slate-700">
              {formatToken(reportRun.outputFormat)}
            </span>
          </div>
        }
      />

      <section className="grid gap-4 md:grid-cols-2">
        <SummaryCard title="Run summary">
          <DefinitionList
            rows={[
              { label: 'Report number', value: definition?.reportNumber ?? 'Not available' },
              { label: 'Report type', value: formatToken(definition?.reportType) },
              { label: 'Requested', value: formatDateTime(reportRun.requestedAt) },
              { label: 'Completed', value: formatDateTime(reportRun.completedAt) },
              { label: 'Rows returned', value: String(reportRun.rowCount) },
              { label: 'Freshness', value: formatToken(reportRun.freshnessStatus) },
            ]}
          />
        </SummaryCard>
        <SummaryCard title="Execution health">
          <DefinitionList
            rows={[
              { label: 'Warnings', value: String(reportRun.warningCount) },
              { label: 'Errors', value: String(reportRun.errorCount) },
              { label: 'Definition datasets', value: String(definition?.datasetRefs.length ?? 0) },
              { label: 'Definition read models', value: String(definition?.readModelRefs.length ?? 0) },
              { label: 'Configured parameters', value: String(reportParameters.length) },
              { label: 'Configured sections', value: String(reportSections.length) },
            ]}
          />
        </SummaryCard>
      </section>

      <SummaryCard title="Approved run notes">
        <BulletList
          items={[
            safeText(definition?.description, 'No report description provided.'),
            safeText(reportRun.freshnessSummary, 'No freshness summary was recorded.'),
            safeText(reportRun.errorMessage, 'No error message was recorded.'),
            'Ownership: ReportArr owns report generation, rendered output, and snapshot history.',
          ]}
          emptyLabel="No approved run notes are available."
        />
      </SummaryCard>

      <section className="grid gap-4 md:grid-cols-2">
        <SummaryCard title="Parameters and filters">
          <BulletList
            items={[
              ...reportRun.parametersUsed.map((parameter) => `Parameter: ${parameter}`),
              ...reportRun.filtersUsed.map((filter) => `Filter: ${filter}`),
            ]}
            emptyLabel="No parameters or filters were recorded for this run."
          />
        </SummaryCard>
        <SummaryCard title="Sections and export history">
          <BulletList
            items={[
              ...reportSections.map((section) => `${section.sequence}. ${section.title} · ${formatToken(section.sectionType)}`),
              ...exportJobs.slice(0, 5).map((job) =>
                `${formatToken(job.exportType)} · ${formatToken(job.exportFormat)} · ${formatToken(job.status)} · generated ${formatDateTime(job.generatedAt)}`,
              ),
            ]}
            emptyLabel="No sections or export history are available."
          />
        </SummaryCard>
      </section>
    </WatermarkedPrintShell>
  )
}

export function ReportSchedulePrintPreview({
  schedule,
  definition,
  recipients,
  actorDisplayName,
  tenantDisplayName,
}: ReportSchedulePreviewProps) {
  return (
    <WatermarkedPrintShell
      title={`${schedule.title} scheduled output`}
      subtitle={`${tenantDisplayName || 'Current tenant workspace'} · ReportArr · ${schedule.scheduleNumber}`}
      footer={buildPreviewFooter(actorDisplayName)}
    >
      <PrintableDocumentHeader
        title={schedule.title}
        metadata={
          <div className="flex flex-wrap items-center gap-2">
            <span className="inline-flex items-center rounded-full border border-slate-300 bg-white px-3 py-1 text-xs font-semibold uppercase tracking-wide text-slate-700">
              {schedule.scheduleNumber}
            </span>
            <span className="inline-flex items-center rounded-full border border-slate-300 bg-white px-3 py-1 text-xs font-semibold uppercase tracking-wide text-slate-700">
              {formatToken(schedule.status)}
            </span>
            <span className="inline-flex items-center rounded-full border border-slate-300 bg-white px-3 py-1 text-xs font-semibold uppercase tracking-wide text-slate-700">
              {formatToken(schedule.deliveryMethod)}
            </span>
          </div>
        }
      />

      <section className="grid gap-4 md:grid-cols-2">
        <SummaryCard title="Schedule summary">
          <DefinitionList
            rows={[
              { label: 'Report number', value: definition?.reportNumber ?? 'Not available' },
              { label: 'Cadence', value: formatToken(schedule.cadence) },
              { label: 'Timezone', value: schedule.timezone },
              { label: 'Next run', value: formatDateTime(schedule.nextRunAt) },
              { label: 'Last run', value: formatDateTime(schedule.lastRunAt) },
              { label: 'Delivery method', value: formatToken(schedule.deliveryMethod) },
            ]}
          />
        </SummaryCard>
        <SummaryCard title="Recipient handling">
          <DefinitionList
            rows={[
              { label: 'Configured recipients', value: String(recipients.length) },
              { label: 'Recipient mix', value: summarizeRecipients(recipients) },
              { label: 'Parameters configured', value: String(schedule.parameters.length) },
              { label: 'Starts at', value: formatDateTime(schedule.startsAt) },
              { label: 'Ends at', value: formatDateTime(schedule.endsAt) },
              { label: 'Cron expression', value: safeText(schedule.cronExpression, 'Not used') },
            ]}
          />
        </SummaryCard>
      </section>

      <SummaryCard title="Approved schedule notes">
        <BulletList
          items={[
            `Ownership: ReportArr owns the scheduled output and recipient orchestration for this report.`,
            `Reference-only data: source records remain owned by the upstream products feeding the report definition.`,
            `Delivery summary: ${summarizeRecipients(recipients)}.`,
          ]}
          emptyLabel="No approved schedule notes are available."
        />
      </SummaryCard>

      <section className="grid gap-4 md:grid-cols-2">
        <SummaryCard title="Parameters">
          <BulletList
            items={schedule.parameters}
            emptyLabel="No schedule parameters are configured."
          />
        </SummaryCard>
        <SummaryCard title="Recipient summary">
          <BulletList
            items={recipients.map((recipient) =>
              `${formatToken(recipient.recipientType)} · ${formatToken(recipient.deliveryFormat)} · ${formatToken(recipient.status)}`,
            )}
            emptyLabel="No recipients are configured."
          />
        </SummaryCard>
      </section>
    </WatermarkedPrintShell>
  )
}

export function AuditPackagePrintPreview({
  auditPackage,
  linkedRuns,
  actorDisplayName,
  tenantDisplayName,
}: AuditPackagePreviewProps) {
  return (
    <WatermarkedPrintShell
      title={`${auditPackage.title} audit packet preview`}
      subtitle={`${tenantDisplayName || 'Current tenant workspace'} · ReportArr · ${auditPackage.packageNumber}`}
      footer={buildPreviewFooter(actorDisplayName)}
    >
      <PrintableDocumentHeader
        title={auditPackage.title}
        metadata={
          <div className="flex flex-wrap items-center gap-2">
            <span className="inline-flex items-center rounded-full border border-slate-300 bg-white px-3 py-1 text-xs font-semibold uppercase tracking-wide text-slate-700">
              {auditPackage.packageNumber}
            </span>
            <span className="inline-flex items-center rounded-full border border-slate-300 bg-white px-3 py-1 text-xs font-semibold uppercase tracking-wide text-slate-700">
              {formatToken(auditPackage.status)}
            </span>
            <span className="inline-flex items-center rounded-full border border-slate-300 bg-white px-3 py-1 text-xs font-semibold uppercase tracking-wide text-slate-700">
              {auditPackage.readinessScore}% ready
            </span>
          </div>
        }
      />

      <section className="grid gap-4 md:grid-cols-2">
        <SummaryCard title="Packet summary">
          <DefinitionList
            rows={[
              { label: 'Requested by', value: actorDisplayName || 'Authorized user' },
              { label: 'Generated at', value: formatDateTime(auditPackage.generatedAt) },
              { label: 'Locked at', value: formatDateTime(auditPackage.lockedAt) },
              { label: 'Source products', value: String(auditPackage.sourceProductRefs.length) },
              { label: 'Linked report runs', value: String(linkedRuns.length) },
              { label: 'Compliance evaluations', value: String(auditPackage.complianceEvaluationRefs.length) },
            ]}
          />
        </SummaryCard>
        <SummaryCard title="Readiness summary">
          <DefinitionList
            rows={[
              { label: 'Missing evidence', value: safeText(auditPackage.missingEvidenceSummary, 'No missing evidence.') },
              { label: 'Invalid evidence', value: safeText(auditPackage.invalidEvidenceSummary, 'No invalid evidence.') },
              { label: 'Scope type', value: formatToken(auditPackage.auditScope.scopeType) },
              { label: 'Audit window', value: `${formatDateTime(auditPackage.auditScope.dateRangeStart)} to ${formatDateTime(auditPackage.auditScope.dateRangeEnd)}` },
              { label: 'Include evidence', value: auditPackage.auditScope.includeEvidence ? 'Yes' : 'No' },
              { label: 'Include source trace', value: auditPackage.auditScope.includeSourceTrace ? 'Yes' : 'No' },
            ]}
          />
        </SummaryCard>
      </section>

      <PacketPreview
        title="Audit packet contents"
        sections={[
          {
            title: 'Audit scope',
            content: (
              <BulletList
                items={[
                  `Product filters: ${auditPackage.auditScope.productFilters.length > 0 ? auditPackage.auditScope.productFilters.map(formatToken).join(', ') : 'None'}`,
                  `Rulepacks in scope: ${auditPackage.auditScope.rulepackRefs.length}`,
                  `Sites in scope: ${auditPackage.auditScope.siteRefs.length}`,
                  `Departments in scope: ${auditPackage.auditScope.departmentRefs.length}`,
                  `Object references summarized: ${auditPackage.auditScope.objectRefs.length}`,
                ]}
                emptyLabel="No scope details are available."
              />
            ),
          },
          {
            title: 'Source products',
            content: (
              <BulletList
                items={auditPackage.sourceProductRefs.map((product) => formatToken(product))}
                emptyLabel="No source products are attached to this packet."
              />
            ),
          },
          {
            title: 'Included report runs',
            content: (
              <BulletList
                items={linkedRuns.map((run) =>
                  `${run.reportRunNumber} · ${run.title} · ${formatToken(run.status)} · ${run.rowCount} rows`,
                )}
                emptyLabel="No linked report runs are available."
              />
            ),
          },
          {
            title: 'Ownership note',
            content: (
              <BulletList
                items={[
                  'ReportArr owns audit packet assembly, readiness summaries, and report snapshots.',
                  'Source products retain ownership of the operational records referenced by the packet.',
                  'RecordArr owns the archived official copy once the packet is issued.',
                ]}
                emptyLabel="No ownership guidance is available."
              />
            ),
          },
        ]}
      />

      <SummaryCard title="Approved packet notes">
        <BulletList
          items={[
            safeText(auditPackage.description, 'No package description provided.'),
            `Readiness score: ${auditPackage.readinessScore}%.`,
            `Working-copy preview only: official issuance and archive remain controlled actions.`,
          ]}
          emptyLabel="No approved packet notes are available."
        />
      </SummaryCard>
    </WatermarkedPrintShell>
  )
}
