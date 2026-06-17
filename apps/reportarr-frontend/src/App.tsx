import { useEffect, useMemo, useState, type ReactNode } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  AlertTriangle,
  BarChart3,
  Bell,
  CheckCircle2,
  Database,
  FileText,
  Gauge,
  History,
  LayoutDashboard,
  Layers3,
  PlugZap,
  PlayCircle,
  Plus,
  RefreshCcw,
  Settings,
  ShieldCheck,
  Users,
  Workflow,
} from 'lucide-react'
import { Link, Navigate, Route, Routes, useLocation, useNavigate, useParams } from 'react-router-dom'
import {
  DetailEmptyState,
  ProfileDetailsLayout,
  ApiErrorCallout,
  ProductWorkspaceFrame,
  StaticSearchPicker,
  buildProductLaunchUrlMap,
  formatProductLaunchError,
  getLaunchCatalog,
  getErrorMessage,
  resolveProductWorkspaceBootstrapError,
  resolveSuiteHomeUrl,
  useProductWorkspaceLaunch,
  type DetailTone,
  type PickerOption,
  type ProductNavItem,
} from '@stl/shared-ui'
import {
  acknowledgeAlert,
  calculateKpi,
  cancelReportRun,
  createAuditPackage,
  createDashboard,
  createDataset,
  createExport,
  createReportDefinition,
  createReportRun,
  createReportSchedule,
  getAuditPackage,
  getDashboard,
  getDataset,
  getExportJob,
  getMe,
  getSessionBootstrap,
  getWorkspaceSummary,
  listDashboardAccessPolicies,
  listDashboardFilters,
  listAlerts,
  listAuditPackages,
  listAuditScopes,
  listDashboards,
  listDatasetFields,
  listDatasetLineage,
  listDrilldowns,
  listIngestionCursors,
  listDatasets,
  listExportJobs,
  listExceptionQueries,
  listExceptionResults,
  getReadModel,
  getReportRun,
  listKpiValues,
  listKpis,
  listMetrics,
  listMetricValues,
  listReadModels,
  listReadModelRecords,
  listRefreshJobs,
  listReportDefinitions,
  listReportAccessPolicies,
  listReportParameters,
  listReportRuns,
  listReportSchedules,
  listReportRecipients,
  listReportSections,
  listSourceConnectors,
  listSourceEvents,
  listAnalyticsSnapshots,
  listTrendAnalyses,
  listWidgets,
  listWidgetVisualizations,
  lockAuditPackage,
  receiveEvent,
  receiveEventBatch,
  refreshDataset,
  rebuildReadModel,
  renderWidget,
  resolveAlert,
  updateReportDefinition,
  updateReportSchedule,
  updateDashboard,
} from './api/client'
import type {
  ReportArrAlertResponse,
  ReportArrAuditPackageResponse,
  ReportArrDashboardResponse,
  ReportArrDatasetFieldResponse,
  ReportArrDatasetLineageResponse,
  ReportArrDatasetResponse,
  ReportArrExportJobResponse,
  ReportArrMeResponse,
  ReportArrIngestionCursorResponse,
  ReportArrReadModelResponse,
  ReportArrReadModelRecordResponse,
  ReportArrRefreshJobResponse,
  ReportArrMetricValueResponse,
  ReportArrReportDefinitionResponse,
  ReportArrReportParameterResponse,
  ReportArrReportRunResponse,
  ReportArrReportScheduleResponse,
  ReportArrReportRecipientResponse,
  ReportArrReportSectionResponse,
  ReportArrMetricDefinitionResponse,
  ReportArrSourceConnectorResponse,
  ReportArrSourceEventReceiptResponse,
  ReportArrIntegrationEventRequest,
  ReportArrKpiValueResponse,
  ReportArrExceptionQueryResponse,
  ReportArrExceptionResultResponse,
  ReportArrWidgetVisualizationSettingsResponse,
} from './api/types'
import { clearSession, loadSession, type StoredReportArrSession } from './auth/sessionStorage'
import { LaunchPage } from './LaunchPage'

const suiteHomeUrl = resolveSuiteHomeUrl(import.meta.env.VITE_SUITE_URL)
const productLaunchUrls = buildProductLaunchUrlMap(import.meta.env)
const apiBase = import.meta.env.VITE_REPORTARR_API_BASE ?? ''

const staffPersonOptions: PickerOption[] = [
  { value: 'person-exec-lead', label: 'Jordan Lee - Executive lead' },
  { value: 'person-analytics-lead', label: 'Priya Shah - Analytics lead' },
  { value: 'person-ops-analyst', label: 'Mateo Alvarez - Operations analyst' },
  { value: 'person-compliance-reporter', label: 'Avery Brooks - Compliance reporter' },
]

const navItems: ProductNavItem[] = [
  { label: 'Overview', to: '/', icon: LayoutDashboard as ProductNavItem['icon'] },
  { label: 'Datasets', to: '/datasets', icon: Database as ProductNavItem['icon'] },
  { label: 'Read models', to: '/read-models', icon: Gauge as ProductNavItem['icon'] },
  { label: 'Refresh jobs', to: '/refresh-jobs', icon: RefreshCcw as ProductNavItem['icon'] },
  { label: 'Dashboards', to: '/dashboards', icon: BarChart3 as ProductNavItem['icon'] },
  { label: 'Reports', to: '/reports', icon: FileText as ProductNavItem['icon'] },
  { label: 'Report builder', to: '/reports/builder', icon: Plus as ProductNavItem['icon'] },
  { label: 'Schedules', to: '/reports/schedules', icon: Workflow as ProductNavItem['icon'] },
  { label: 'Exports', to: '/reports/exports', icon: FileText as ProductNavItem['icon'] },
  { label: 'KPIs', to: '/kpis', icon: Gauge as ProductNavItem['icon'] },
  { label: 'Metrics', to: '/metrics', icon: BarChart3 as ProductNavItem['icon'] },
  { label: 'Alerts', to: '/alerts', icon: Bell as ProductNavItem['icon'] },
  { label: 'Audit', to: '/audit', icon: ShieldCheck as ProductNavItem['icon'], sectionBreakBefore: true },
  { label: 'Source connectors', to: '/source-connectors', icon: PlugZap as ProductNavItem['icon'], sectionBreakBefore: true },
  { label: 'Ingestion status', to: '/ingestion-status', icon: History as ProductNavItem['icon'] },
  { label: 'Settings', to: '/settings', icon: Settings as ProductNavItem['icon'], sectionBreakBefore: true },
]

const reportExportFormatOptions = ['pdf', 'csv', 'xlsx', 'json', 'html', 'zip'] as const
const exportTypeOptions = ['report', 'dashboard', 'table', 'dataset', 'audit_package', 'chart', 'custom'] as const

function formatDate(value: string | null | undefined): string {
  if (!value) {
    return 'n/a'
  }
  const parsed = new Date(value)
  return Number.isNaN(parsed.getTime())
    ? value
    : parsed.toLocaleString(undefined, {
        month: 'short',
        day: 'numeric',
        hour: 'numeric',
        minute: '2-digit',
      })
}

function formatNumber(value: number): string {
  return new Intl.NumberFormat(undefined).format(value)
}

function summarizeConfiguredField(value: string | null | undefined, label: string): string {
  return value?.trim() ? `${label} configured` : 'n/a'
}

function summarizeText(value: string | null | undefined, maxLength = 120): string {
  const trimmed = value?.trim()
  if (!trimmed) {
    return 'n/a'
  }
  return trimmed.length <= maxLength ? trimmed : `${trimmed.slice(0, maxLength - 1).trimEnd()}…`
}

function parseCsvList(value: string): string[] {
  return value
    .split(',')
    .map((item) => item.trim())
    .filter(Boolean)
}

function matchesRole(roleKey: string, candidates: string[]): boolean {
  return candidates.some((candidate) => roleKey.toLowerCase() === candidate.toLowerCase())
}

function canUseReportArrAction(roleKey: string, isPlatformAdmin: boolean, allowedRoles: string[]): boolean {
  return isPlatformAdmin || matchesRole(roleKey, allowedRoles)
}

function SectionHeader({
  eyebrow,
  title,
  description,
  action,
}: {
  eyebrow: string
  title: string
  description: string
  action?: ReactNode
}) {
  return (
    <div className="reportarr-section-header">
      <div>
        <p className="reportarr-eyebrow">{eyebrow}</p>
        <h1>{title}</h1>
        <p>{description}</p>
      </div>
      {action}
    </div>
  )
}

function Panel({
  title,
  icon,
  children,
}: {
  title: string
  icon?: ReactNode
  children: ReactNode
}) {
  return (
    <section className="reportarr-panel">
      <div className="reportarr-panel-header">
        <div className="flex items-center gap-2">
          {icon}
          <h2>{title}</h2>
        </div>
      </div>
      {children}
    </section>
  )
}

function StatCard({ label, value, note }: { label: string; value: string | number; note: string }) {
  return (
    <div className="reportarr-stat">
      <div className="reportarr-stat-label">{label}</div>
      <div className="reportarr-stat-value">{value}</div>
      <div className="reportarr-stat-note">{note}</div>
    </div>
  )
}

function Pill({ children }: { children: ReactNode }) {
  return <span className="reportarr-pill">{children}</span>
}

function EmptyState({ title }: { title: string }) {
  return <div className="reportarr-empty">{title}</div>
}

type ReportDetailMetric = {
  label: string
  value: string | number
  hint: string
  icon?: ReactNode
  tone?: DetailTone
}

type ReportDetailField = {
  label: string
  value: string | number
  source: string
}

type ReportDetailRailSection = {
  title: string
  icon?: ReactNode
  content: ReactNode
}

type ReportDetailShellProps = {
  testId?: string
  backLabel: string
  backTo: string
  breadcrumbs: string[]
  icon: ReactNode
  title: string
  subtitle: ReactNode
  badges: Array<{ label: string; tone?: DetailTone }>
  actions?: ReactNode
  metrics: ReportDetailMetric[]
  snapshotTitle: string
  snapshotSubtitle: string
  snapshotFields: ReportDetailField[]
  decisionTitle: string
  decisionBadge: { label: string; tone?: DetailTone }
  decisionIcon?: ReactNode
  decisionSummary: string
  decisionDetail: string
  allowedChecks: number
  blockedChecks: number
  railSections: ReportDetailRailSection[]
  mainContent?: ReactNode
}

function ReportDetailShell({
  testId,
  backLabel,
  backTo,
  breadcrumbs,
  icon,
  title,
  subtitle,
  badges,
  actions,
  metrics,
  snapshotTitle,
  snapshotSubtitle,
  snapshotFields,
  decisionTitle,
  decisionBadge,
  decisionIcon,
  decisionSummary,
  decisionDetail,
  allowedChecks,
  blockedChecks,
  railSections,
  mainContent,
}: ReportDetailShellProps) {
  return (
    <div className="space-y-6" data-testid={testId}>
      <ProfileDetailsLayout
        backLabel={backLabel}
        backTo={backTo}
        breadcrumbs={breadcrumbs}
        icon={icon}
        title={title}
        subtitle={subtitle}
        badges={badges}
        actions={actions}
        metrics={metrics}
        tabs={['Overview', 'Related records', 'History']}
        snapshotTitle={snapshotTitle}
        snapshotSubtitle={snapshotSubtitle}
        snapshotFields={snapshotFields}
        decisionTitle={decisionTitle}
        decisionBadge={decisionBadge}
        decisionIcon={decisionIcon}
        decisionSummary={decisionSummary}
        decisionDetail={decisionDetail}
        allowedChecks={allowedChecks}
        blockedChecks={blockedChecks}
        railSections={railSections}
        mainContent={mainContent}
      />
    </div>
  )
}

function makeEventRow(overrides: Partial<ReportArrIntegrationEventRequest> = {}): ReportArrIntegrationEventRequest {
  return {
    sourceProduct: 'routarr',
    sourceEventId: 'evt-9001',
    eventType: 'trip.completed',
    sourceObjectRef: 'trip-7781',
    correlationId: 'corr-9001',
    ...overrides,
  }
}

function TextInput({
  value,
  onChange,
  placeholder,
}: {
  value: string
  onChange: (value: string) => void
  placeholder?: string
}) {
  return (
    <input
      className="reportarr-input"
      value={value}
      placeholder={placeholder}
      onChange={(event) => onChange(event.target.value)}
    />
  )
}

function OwnerPersonPicker({
  value,
  onChange,
}: {
  value: string
  onChange: (value: string) => void
}) {
  return (
    <StaticSearchPicker
      value={value}
      onChange={onChange}
      options={staffPersonOptions}
      placeholder="Owner person"
    />
  )
}

function TextArea({
  value,
  onChange,
  placeholder,
}: {
  value: string
  onChange: (value: string) => void
  placeholder?: string
}) {
  return (
    <textarea
      className="reportarr-textarea"
      value={value}
      placeholder={placeholder}
      onChange={(event) => onChange(event.target.value)}
    />
  )
}

function Select({
  value,
  onChange,
  options,
}: {
  value: string
  onChange: (value: string) => void
  options: readonly string[]
}) {
  return (
    <select className="reportarr-input" value={value} onChange={(event) => onChange(event.target.value)}>
      {options.map((option) => (
        <option key={option} value={option}>
          {option}
        </option>
      ))}
    </select>
  )
}

function useWorkspaceSessionBootstrap() {
  const session = loadSession()
  const sessionQuery = useQuery({
    queryKey: ['reportarr', 'session', session?.accessToken],
    queryFn: () => getSessionBootstrap(session!.accessToken),
    enabled: Boolean(session?.accessToken),
    retry: false,
  })

  const launchCatalogQuery = useQuery({
    queryKey: ['reportarr', 'launch-catalog', session?.accessToken],
    queryFn: () => getLaunchCatalog(apiBase, session!.accessToken, 'reportarr'),
    enabled: Boolean(session?.accessToken),
    retry: false,
  })

  useEffect(() => {
    if (sessionQuery.isError && resolveProductWorkspaceBootstrapError(sessionQuery.error)) {
      clearSession()
    }
  }, [sessionQuery.error, sessionQuery.isError])

  useEffect(() => {
    if (launchCatalogQuery.isError && resolveProductWorkspaceBootstrapError(launchCatalogQuery.error)) {
      clearSession()
    }
  }, [launchCatalogQuery.error, launchCatalogQuery.isError])

  const sessionBootstrapError = sessionQuery.isError
    ? resolveProductWorkspaceBootstrapError(sessionQuery.error)
    : null
  const launchBootstrapError = launchCatalogQuery.isError
    ? resolveProductWorkspaceBootstrapError(launchCatalogQuery.error)
    : null
  const bootstrapError = sessionBootstrapError ?? launchBootstrapError

  return { session, sessionQuery, launchCatalogQuery, bootstrapError }
}

function useReportArrWorkspace() {
  const queryClient = useQueryClient()
  const { session, sessionQuery, launchCatalogQuery, bootstrapError } = useWorkspaceSessionBootstrap()
  const workspaceSession =
    session && sessionQuery.data && !bootstrapError
      ? {
          userDisplayName: session.displayName,
          tenantDisplayName: session.tenantDisplayName,
          tenantSlug: session.tenantSlug,
        }
      : null

  const switcherEntitlements =
    launchCatalogQuery.data?.products.map((product) => product.productKey) ??
    sessionQuery.data?.entitlements ??
    []

  const launch = useProductWorkspaceLaunch({
    apiBase,
    accessToken: session?.accessToken ?? '',
    currentProductKey: 'reportarr',
    suiteHomeUrl,
    productLaunchUrls,
  })

  return {
    session,
    sessionQuery,
    launchCatalogQuery,
    bootstrapError,
    workspaceSession,
    switcherEntitlements,
    queryClient,
    launch,
  }
}

function DashboardPage({
  accessToken,
  roleKey,
  isPlatformAdmin,
}: {
  accessToken: string
  roleKey: string
  isPlatformAdmin: boolean
}) {
  const queryClient = useQueryClient()
  const summaryQuery = useQuery({
    queryKey: ['reportarr', 'summary'],
    queryFn: () => getWorkspaceSummary(accessToken),
    enabled: Boolean(accessToken),
  })
  const [form, setForm] = useState({
    reportKey: 'exec-summary',
    title: 'Executive summary',
    description: 'Leadership summary across the suite.',
    reportType: 'executive',
    layoutDefinition: 'layout:grid:2x2',
    exportFormats: ['pdf', 'csv'],
    ownerPersonId: 'person-exec-lead',
  })

  const createMutation = useMutation({
    mutationFn: () => createReportDefinition(accessToken, form),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['reportarr'] })
    },
  })

  const summary = summaryQuery.data
  const canBuild = canUseReportArrAction(roleKey, isPlatformAdmin, ['report_builder', 'reportarr_builder', 'tenant_admin', 'reportarr_admin'])

  return (
    <div className="reportarr-page">
      <SectionHeader
        eyebrow="Overview"
        title="Reporting command center"
        description="Monitor operational datasets, dashboards, report definitions, KPI health, alerts, and audit packages from one workspace."
        action={
          <Pill>
            <ShieldCheck className="h-4 w-4" />
            {summary ? `Freshness ${summary.freshnessStatus}` : 'Loading summary'}
          </Pill>
        }
      />

      {summaryQuery.isError ? (
        <ApiErrorCallout title="Unable to load summary" message={getErrorMessage(summaryQuery.error, 'Failed to load ReportArr summary.')} />
      ) : null}

      <div className="reportarr-grid cols-4">
        <StatCard label="Datasets" value={summary ? formatNumber(summary.datasetCount) : '—'} note="Source-aligned read models" />
        <StatCard label="Dashboards" value={summary ? formatNumber(summary.dashboardCount) : '—'} note="Operational and executive views" />
        <StatCard label="Reports" value={summary ? formatNumber(summary.reportDefinitionCount) : '—'} note="Definitions, schedules, and exports" />
        <StatCard label="Audits" value={summary ? formatNumber(summary.auditPackageCount) : '—'} note="Evidence-backed packages" />
      </div>

      {canBuild ? (
        <Panel title="Create report definition" icon={<Plus className="h-4 w-4 text-cyan-300" />}>
        <div className="grid gap-3 md:grid-cols-2">
          <TextInput value={form.reportKey} onChange={(value) => setForm({ ...form, reportKey: value })} placeholder="report-key" />
          <TextInput value={form.title} onChange={(value) => setForm({ ...form, title: value })} placeholder="Title" />
          <TextInput value={form.reportType} onChange={(value) => setForm({ ...form, reportType: value })} placeholder="Type" />
          <TextInput value={form.layoutDefinition} onChange={(value) => setForm({ ...form, layoutDefinition: value })} placeholder="Layout definition" />
          <OwnerPersonPicker value={form.ownerPersonId} onChange={(ownerPersonId) => setForm({ ...form, ownerPersonId })} />
          <div className="md:col-span-2">
            <TextArea value={form.description} onChange={(value) => setForm({ ...form, description: value })} placeholder="Description" />
          </div>
          <div className="md:col-span-2">
            <div className="mb-2 text-sm text-slate-300">Export formats</div>
            <div className="flex flex-wrap gap-2">
              {reportExportFormatOptions.map((format) => {
                const active = form.exportFormats.includes(format)
                return (
                  <button
                    key={format}
                    type="button"
                    className={[
                      'reportarr-button secondary',
                      active ? 'ring-2 ring-cyan-400 bg-cyan-400/10 text-cyan-100' : '',
                    ].join(' ')}
                    onClick={() =>
                      setForm((current) => ({
                        ...current,
                        exportFormats: current.exportFormats.includes(format)
                          ? current.exportFormats.filter((item) => item !== format)
                          : [...current.exportFormats, format],
                      }))
                    }
                  >
                    {format}
                  </button>
                )
              })}
            </div>
          </div>
        </div>
        <div className="mt-4 flex flex-wrap items-center gap-3">
          <button className="reportarr-button" type="button" onClick={() => createMutation.mutate()} disabled={createMutation.isPending}>
            {createMutation.isPending ? 'Creating…' : 'Create report'}
          </button>
          {createMutation.isError ? <span className="text-sm text-rose-300">{getErrorMessage(createMutation.error, 'Create failed')}</span> : null}
        </div>
        </Panel>
      ) : null}

      <div className="reportarr-grid cols-2">
        <Panel title="Recent datasets" icon={<Database className="h-4 w-4 text-cyan-300" />}>
          <ListOfDatasets datasets={summary?.recentDatasets ?? []} />
        </Panel>
        <Panel title="Recent reports" icon={<FileText className="h-4 w-4 text-cyan-300" />}>
          <ListOfReports reports={summary?.recentReports ?? []} />
        </Panel>
        <Panel title="Recent dashboards" icon={<BarChart3 className="h-4 w-4 text-cyan-300" />}>
          <ListOfDashboards dashboards={summary?.recentDashboards ?? []} />
        </Panel>
        <Panel title="Recent alerts" icon={<Bell className="h-4 w-4 text-cyan-300" />}>
          <ListOfAlerts alerts={summary?.recentAlerts ?? []} />
        </Panel>
      </div>
    </div>
  )
}

function ListOfDatasets({ datasets }: { datasets: ReportArrDatasetResponse[] }) {
  if (!datasets.length) return <EmptyState title="No datasets yet." />
  return (
    <div className="reportarr-stack">
      {datasets.map((dataset) => (
        <Link key={dataset.datasetId} to={`/datasets/${dataset.datasetId}`} className="reportarr-row">
          <div className="reportarr-row-main">
            <strong>{dataset.datasetNumber}</strong>
            <span>{dataset.title}</span>
            <small>{dataset.description}</small>
          </div>
          <div className="reportarr-row-meta">
            <Pill>{dataset.freshnessStatus}</Pill>
            <Pill>{dataset.datasetType}</Pill>
          </div>
        </Link>
      ))}
    </div>
  )
}

function ListOfDashboards({ dashboards }: { dashboards: ReportArrDashboardResponse[] }) {
  if (!dashboards.length) return <EmptyState title="No dashboards yet." />
  return (
    <div className="reportarr-stack">
      {dashboards.map((dashboard) => (
        <Link key={dashboard.dashboardId} to={`/dashboards/${dashboard.dashboardId}`} className="reportarr-row">
          <div className="reportarr-row-main">
            <strong>{dashboard.dashboardNumber}</strong>
            <span>{dashboard.title}</span>
            <small>{dashboard.description}</small>
          </div>
          <div className="reportarr-row-meta">
            <Pill>{dashboard.status}</Pill>
            <Pill>{dashboard.freshnessStatus}</Pill>
          </div>
        </Link>
      ))}
    </div>
  )
}

function ListOfReports({ reports }: { reports: ReportArrReportDefinitionResponse[] }) {
  if (!reports.length) return <EmptyState title="No reports yet." />
  return (
    <div className="reportarr-stack">
      {reports.map((report) => (
        <div key={report.reportDefinitionId} className="reportarr-row">
          <div className="reportarr-row-main">
            <strong>{report.reportNumber}</strong>
            <span>{report.title}</span>
            <small>{report.description}</small>
          </div>
          <div className="reportarr-row-meta">
            <Pill>{report.status}</Pill>
            <Pill>{report.reportType}</Pill>
          </div>
        </div>
      ))}
    </div>
  )
}

function ListOfAlerts({ alerts }: { alerts: ReportArrAlertResponse[] }) {
  if (!alerts.length) return <EmptyState title="No alerts yet." />
  return (
    <div className="reportarr-stack">
      {alerts.map((alert) => (
        <Link key={alert.alertId} to={`/alerts/${alert.alertId}`} className="reportarr-row">
          <div className="reportarr-row-main">
            <strong>{alert.alertNumber}</strong>
            <span>{alert.title}</span>
            <small>{alert.description}</small>
            <small>{alert.condition}</small>
          </div>
          <div className="reportarr-row-meta">
            <Pill>{alert.status}</Pill>
            <Pill>{alert.severity}</Pill>
          </div>
        </Link>
      ))}
    </div>
  )
}

function DatasetsPage({
  accessToken,
  roleKey,
  isPlatformAdmin,
}: {
  accessToken: string
  roleKey: string
  isPlatformAdmin: boolean
}) {
  const queryClient = useQueryClient()
  const datasetsQuery = useQuery({
    queryKey: ['reportarr', 'datasets'],
    queryFn: () => listDatasets(accessToken),
    enabled: Boolean(accessToken),
  })
  const datasetFieldsQuery = useQuery({
    queryKey: ['reportarr', 'dataset-fields'],
    queryFn: () => listDatasetFields(accessToken),
    enabled: Boolean(accessToken),
  })
  const datasetLineageQuery = useQuery({
    queryKey: ['reportarr', 'dataset-lineage'],
    queryFn: () => listDatasetLineage(accessToken),
    enabled: Boolean(accessToken),
  })
  const dashboardsQuery = useQuery({
    queryKey: ['reportarr', 'dashboards'],
    queryFn: () => listDashboards(accessToken),
    enabled: Boolean(accessToken),
  })
  const widgetsQuery = useQuery({
    queryKey: ['reportarr', 'widgets'],
    queryFn: () => listWidgets(accessToken),
    enabled: Boolean(accessToken),
  })
  const reportsQuery = useQuery({
    queryKey: ['reportarr', 'reports'],
    queryFn: () => listReportDefinitions(accessToken),
    enabled: Boolean(accessToken),
  })
  const connectorsQuery = useQuery({
    queryKey: ['reportarr', 'source-connectors'],
    queryFn: () => listSourceConnectors(accessToken),
    enabled: Boolean(accessToken),
  })
  const ingestionCursorsQuery = useQuery({
    queryKey: ['reportarr', 'ingestion-cursors'],
    queryFn: () => listIngestionCursors(accessToken),
    enabled: Boolean(accessToken),
  })
  const readModelsQuery = useQuery({
    queryKey: ['reportarr', 'read-models'],
    queryFn: () => listReadModels(accessToken),
    enabled: Boolean(accessToken),
  })
  const refreshJobsQuery = useQuery({
    queryKey: ['reportarr', 'refresh-jobs'],
    queryFn: () => listRefreshJobs(accessToken),
    enabled: Boolean(accessToken),
  })
  const sourceEventsQuery = useQuery({
    queryKey: ['reportarr', 'source-events'],
    queryFn: () => listSourceEvents(accessToken),
    enabled: Boolean(accessToken),
  })
  const [selectedDatasetId, setSelectedDatasetId] = useState('ds-001')
  const [selectedDatasetFieldId, setSelectedDatasetFieldId] = useState('')
  const [selectedSourceConnectorId, setSelectedSourceConnectorId] = useState('')
  const [selectedReadModelId, setSelectedReadModelId] = useState('')
  const [selectedRefreshJobId, setSelectedRefreshJobId] = useState('')
  const [form, setForm] = useState({
    datasetKey: 'executive-snapshot',
    title: 'Executive operations snapshot',
    description: 'Cross-suite operational summary for leadership.',
    datasetType: 'executive',
    sourceProducts: 'staffarr,maintainarr,loadarr,routarr,supplyarr,trainarr,compliancecore,assurarr',
    ownerPersonId: 'person-analytics-lead',
  })

  const createMutation = useMutation({
    mutationFn: () =>
      createDataset(accessToken, {
        ...form,
        sourceProducts: form.sourceProducts.split(',').map((part) => part.trim()).filter(Boolean),
      }),
    onSuccess: async (dataset) => {
      setSelectedDatasetId(dataset.datasetId)
      await queryClient.invalidateQueries({ queryKey: ['reportarr'] })
    },
  })
  const refreshMutation = useMutation({
    mutationFn: () => refreshDataset(accessToken, selectedDatasetId, { requestedByPersonId: 'person-analytics-lead' }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['reportarr'] })
    },
  })

  useEffect(() => {
    const fields = (datasetFieldsQuery.data ?? []).filter((field) => field.datasetId === selectedDatasetId)
    if (!fields.length) return
    if (!fields.some((field) => field.fieldId === selectedDatasetFieldId)) {
      setSelectedDatasetFieldId(fields[0].fieldId)
    }
  }, [datasetFieldsQuery.data, selectedDatasetFieldId, selectedDatasetId])

  useEffect(() => {
    const connectors = connectorsQuery.data ?? []
    if (!connectors.length) return
    if (!connectors.some((connector) => connector.sourceConnectorId === selectedSourceConnectorId)) {
      setSelectedSourceConnectorId(connectors[0].sourceConnectorId)
    }
  }, [connectorsQuery.data, selectedSourceConnectorId])

  useEffect(() => {
    const readModels = readModelsQuery.data ?? []
    if (!readModels.length) return
    if (!readModels.some((model) => model.readModelId === selectedReadModelId)) {
      setSelectedReadModelId(readModels[0].readModelId)
    }
  }, [readModelsQuery.data, selectedReadModelId])

  useEffect(() => {
    const jobs = refreshJobsQuery.data ?? []
    if (!jobs.length) return
    if (!jobs.some((job) => job.refreshJobId === selectedRefreshJobId)) {
      setSelectedRefreshJobId(jobs[0].refreshJobId)
    }
  }, [refreshJobsQuery.data, selectedRefreshJobId])

  const selectedDataset = datasetsQuery.data?.find((dataset) => dataset.datasetId === selectedDatasetId) ?? null
  const canManageDatasets = canUseReportArrAction(roleKey, isPlatformAdmin, ['analytics_admin', 'reportarr_admin', 'tenant_admin'])

  return (
    <div className="reportarr-page">
      <SectionHeader
        eyebrow="Datasets"
        title="Dataset and read model registry"
        description="Manage dataset freshness, source-product coverage, and downstream read-model rebuilds."
        action={<Pill><Database className="h-4 w-4" /> {datasetsQuery.data?.length ?? 0} datasets</Pill>}
      />
      {canManageDatasets ? (
        <Panel title="Create dataset" icon={<Plus className="h-4 w-4 text-cyan-300" />}>
        <div className="grid gap-3 md:grid-cols-2">
          <TextInput value={form.datasetKey} onChange={(value) => setForm({ ...form, datasetKey: value })} placeholder="dataset-key" />
          <TextInput value={form.title} onChange={(value) => setForm({ ...form, title: value })} placeholder="Title" />
          <TextInput value={form.datasetType} onChange={(value) => setForm({ ...form, datasetType: value })} placeholder="Type" />
          <OwnerPersonPicker value={form.ownerPersonId} onChange={(ownerPersonId) => setForm({ ...form, ownerPersonId })} />
          <div className="md:col-span-2">
            <TextInput value={form.sourceProducts} onChange={(value) => setForm({ ...form, sourceProducts: value })} placeholder="source product keys comma separated" />
          </div>
          <div className="md:col-span-2">
            <TextArea value={form.description} onChange={(value) => setForm({ ...form, description: value })} placeholder="Description" />
          </div>
        </div>
        <div className="mt-4 flex flex-wrap items-center gap-3">
          <button type="button" className="reportarr-button" onClick={() => createMutation.mutate()} disabled={createMutation.isPending}>
            {createMutation.isPending ? 'Creating…' : 'Create dataset'}
          </button>
          <button type="button" className="reportarr-button secondary" onClick={() => refreshMutation.mutate()} disabled={refreshMutation.isPending}>
            {refreshMutation.isPending ? 'Refreshing…' : 'Refresh selected dataset'}
          </button>
        </div>
        </Panel>
      ) : null}

      <div className="reportarr-grid cols-2">
        <Panel title="Datasets" icon={<Database className="h-4 w-4 text-cyan-300" />}>
          <div className="reportarr-stack">
            {(datasetsQuery.data ?? []).map((dataset) => (
              <button
                key={dataset.datasetId}
                type="button"
                className={['reportarr-row reportarr-row-button', dataset.datasetId === selectedDatasetId ? 'active' : ''].join(' ')}
                onClick={() => setSelectedDatasetId(dataset.datasetId)}
              >
                <div className="reportarr-row-main">
                  <strong>{dataset.datasetNumber}</strong>
                  <span>{dataset.title}</span>
                  <small>{dataset.description}</small>
                </div>
                <div className="reportarr-row-meta">
                  <Pill>{dataset.status}</Pill>
                  <Pill>{dataset.freshnessStatus}</Pill>
                </div>
              </button>
            ))}
            {!datasetsQuery.data?.length ? <EmptyState title="No datasets yet." /> : null}
          </div>
        </Panel>
        <Panel title="Selected dataset" icon={<Gauge className="h-4 w-4 text-cyan-300" />}>
          {selectedDataset ? (
            (() => {
              const dataset = selectedDataset
              const dependentDashboards = (dashboardsQuery.data ?? []).filter((dashboard) => {
                const dashboardWidgets = (widgetsQuery.data ?? []).filter((widget) => dashboard.widgetRefs.includes(widget.widgetId))
                return dashboardWidgets.some((widget) => widget.datasetRef === dataset.datasetId)
              })
              const dependentReports = (reportsQuery.data ?? []).filter((report) => report.datasetRefs.includes(dataset.datasetId))
              return (
                <div className="space-y-2 text-sm text-slate-300">
                  <p><strong className="text-slate-100">Freshness:</strong> {dataset.freshnessStatus}</p>
                  <p><strong className="text-slate-100">Sources:</strong> {dataset.sourceProducts.join(', ')}</p>
                  <p><strong className="text-slate-100">Connectors:</strong> {dataset.sourceConnectors.join(', ') || 'manual-import'}</p>
                  <p><strong className="text-slate-100">Refresh:</strong> {dataset.refreshMode} · {dataset.refreshFrequency}</p>
                  <p><strong className="text-slate-100">Last refreshed:</strong> {formatDate(dataset.lastRefreshedAt)}</p>
                  <p><strong className="text-slate-100">Last successful refresh:</strong> {formatDate(dataset.lastSuccessfulRefreshAt)}</p>
                  <p><strong className="text-slate-100">Last failed refresh:</strong> {formatDate(dataset.lastFailedRefreshAt)}</p>
                  <p><strong className="text-slate-100">Fields:</strong> {datasetFieldsQuery.data?.filter((field) => field.datasetId === dataset.datasetId).length ?? 0}</p>
                  <p><strong className="text-slate-100">Defined fields:</strong> {dataset.fieldDefinitions.join(', ') || 'none'}</p>
                  <p><strong className="text-slate-100">Retention:</strong> {dataset.retentionPolicy}</p>
                  <p><strong className="text-slate-100">Updated:</strong> {formatDate(dataset.updatedAt)}</p>
                  <p><strong className="text-slate-100">Traceability:</strong> {dataset.sourceTraceabilityRules}</p>
                  <p><strong className="text-slate-100">Dependent dashboards:</strong> {dependentDashboards.map((item) => item.dashboardNumber).join(', ') || 'none'}</p>
                  <p><strong className="text-slate-100">Dependent reports:</strong> {dependentReports.map((item) => item.reportNumber).join(', ') || 'none'}</p>
                </div>
              )
            })()
          ) : (
            <EmptyState title="Select a dataset to inspect details." />
          )}
        </Panel>
        <Panel title="Source connectors" icon={<PlugZap className="h-4 w-4 text-cyan-300" />}>
          <ConnectorsList
            connectors={connectorsQuery.data ?? []}
            selectedConnectorId={selectedSourceConnectorId}
            onSelectConnector={setSelectedSourceConnectorId}
          />
        </Panel>
        <Panel title="Selected source connector" icon={<PlugZap className="h-4 w-4 text-cyan-300" />}>
          {connectorsQuery.data?.find((connector) => connector.sourceConnectorId === selectedSourceConnectorId) ? (
            (() => {
              const connector = connectorsQuery.data!.find((item) => item.sourceConnectorId === selectedSourceConnectorId)!
              return (
                <div className="space-y-2 text-sm text-slate-300">
                  <p><strong className="text-slate-100">Source product:</strong> {connector.sourceProduct}</p>
                  <p><strong className="text-slate-100">Connector type:</strong> {connector.connectorType}</p>
                  <p><strong className="text-slate-100">Status:</strong> {connector.status}</p>
                  <p><strong className="text-slate-100">Service client:</strong> {connector.serviceClientRef}</p>
                  <p><strong className="text-slate-100">Last connected:</strong> {formatDate(connector.lastConnectedAt)}</p>
                  <p><strong className="text-slate-100">Last error:</strong> {formatDate(connector.lastErrorAt)}</p>
                  <p><strong className="text-slate-100">Error message:</strong> {connector.lastErrorMessage ?? 'none'}</p>
                  <p><strong className="text-slate-100">Supported event types:</strong> {connector.supportedEventTypes.join(', ') || 'none'}</p>
                  <p><strong className="text-slate-100">Supported datasets:</strong> {connector.supportedDatasets.join(', ') || 'none'}</p>
                </div>
              )
            })()
          ) : (
            <EmptyState title="Select a source connector to inspect details." />
          )}
        </Panel>
        <Panel title="Dataset fields" icon={<Layers3 className="h-4 w-4 text-cyan-300" />}>
          <DatasetFieldsList
            fields={datasetFieldsQuery.data ?? []}
            datasetId={selectedDatasetId}
            selectedFieldId={selectedDatasetFieldId}
            onSelectField={setSelectedDatasetFieldId}
          />
        </Panel>
        <Panel title="Selected dataset field" icon={<Layers3 className="h-4 w-4 text-cyan-300" />}>
          {datasetFieldsQuery.data?.find((field) => field.fieldId === selectedDatasetFieldId) ? (
            (() => {
              const field = datasetFieldsQuery.data!.find((item) => item.fieldId === selectedDatasetFieldId)!
              return (
                <div className="space-y-2 text-sm text-slate-300">
                  <p><strong className="text-slate-100">Field key:</strong> {field.fieldKey}</p>
                  <p><strong className="text-slate-100">Display name:</strong> {field.displayName}</p>
                  <p><strong className="text-slate-100">Description:</strong> {field.description}</p>
                  <p><strong className="text-slate-100">Data type:</strong> {field.dataType}</p>
                  <p><strong className="text-slate-100">Source product:</strong> {field.sourceProduct}</p>
                  <p><strong className="text-slate-100">Source field path:</strong> {field.sourceFieldPath}</p>
                  <p><strong className="text-slate-100">Aggregation allowed:</strong> {field.aggregationAllowed ? 'yes' : 'no'}</p>
                  <p><strong className="text-slate-100">Filter allowed:</strong> {field.filterAllowed ? 'yes' : 'no'}</p>
                  <p><strong className="text-slate-100">Group allowed:</strong> {field.groupAllowed ? 'yes' : 'no'}</p>
                  <p><strong className="text-slate-100">Sort allowed:</strong> {field.sortAllowed ? 'yes' : 'no'}</p>
                  <p><strong className="text-slate-100">PII sensitive:</strong> {field.piiSensitive ? 'yes' : 'no'}</p>
                  <p><strong className="text-slate-100">Restricted:</strong> {field.restricted ? 'yes' : 'no'}</p>
                  <p><strong className="text-slate-100">Compliance sensitive:</strong> {field.complianceSensitive ? 'yes' : 'no'}</p>
                </div>
              )
            })()
          ) : (
            <EmptyState title="Select a dataset field to inspect details." />
          )}
        </Panel>
        <Panel title="Dataset lineage" icon={<Workflow className="h-4 w-4 text-cyan-300" />}>
          <DatasetLineageList lineage={datasetLineageQuery.data ?? []} datasetId={selectedDatasetId} />
        </Panel>
        <Panel title="Ingestion errors" icon={<AlertTriangle className="h-4 w-4 text-cyan-300" />}>
          <IngestionErrorsList
            events={sourceEventsQuery.data ?? []}
            dataset={selectedDataset ?? datasetsQuery.data?.[0] ?? null}
          />
        </Panel>
        <Panel title="Ingestion cursors" icon={<History className="h-4 w-4 text-cyan-300" />}>
          <IngestionCursorsList cursors={ingestionCursorsQuery.data ?? []} />
        </Panel>
        <Panel title="Read models" icon={<Layers3 className="h-4 w-4 text-cyan-300" />}>
          <ReadModelsList
            readModels={readModelsQuery.data ?? []}
            selectedReadModelId={selectedReadModelId}
            onSelectReadModel={setSelectedReadModelId}
          />
        </Panel>
        <Panel title="Selected read model" icon={<Gauge className="h-4 w-4 text-cyan-300" />}>
          {readModelsQuery.data?.find((model) => model.readModelId === selectedReadModelId) ? (
            (() => {
              const model = readModelsQuery.data!.find((item) => item.readModelId === selectedReadModelId)!
              return (
                <div className="space-y-2 text-sm text-slate-300">
                  <p><strong className="text-slate-100">Number:</strong> {model.readModelNumber}</p>
                  <p><strong className="text-slate-100">Key:</strong> {model.readModelKey}</p>
                  <p><strong className="text-slate-100">Title:</strong> {model.title}</p>
                  <p><strong className="text-slate-100">Description:</strong> {model.description}</p>
                  <p><strong className="text-slate-100">Type:</strong> {model.readModelType}</p>
                  <p><strong className="text-slate-100">Status:</strong> {model.status}</p>
                  <p><strong className="text-slate-100">Primary entity:</strong> {model.primaryEntityType}</p>
                  <p><strong className="text-slate-100">Primary source:</strong> {model.primarySourceProduct}</p>
                  <p><strong className="text-slate-100">Datasets:</strong> {model.datasetRefs.join(', ') || 'none'}</p>
                  <p><strong className="text-slate-100">Fields:</strong> {model.fieldDefinitions.join(', ') || 'none'}</p>
                  <p><strong className="text-slate-100">Refresh jobs:</strong> {model.refreshJobRefs.join(', ') || 'none'}</p>
                  <p><strong className="text-slate-100">Last rebuilt:</strong> {formatDate(model.lastRebuiltAt)}</p>
                  <p><strong className="text-slate-100">Last updated:</strong> {formatDate(model.lastUpdatedAt)}</p>
                  <p><strong className="text-slate-100">Updated:</strong> {formatDate(model.updatedAt)}</p>
                </div>
              )
            })()
          ) : (
            <EmptyState title="Select a read model to inspect details." />
          )}
        </Panel>
        <Panel title="Refresh history" icon={<RefreshCcw className="h-4 w-4 text-cyan-300" />}>
          <RefreshJobsList
            refreshJobs={refreshJobsQuery.data ?? []}
            datasetId={selectedDatasetId}
            selectedRefreshJobId={selectedRefreshJobId}
            onSelectRefreshJob={setSelectedRefreshJobId}
          />
        </Panel>
        <Panel title="Selected refresh job" icon={<RefreshCcw className="h-4 w-4 text-cyan-300" />}>
          {refreshJobsQuery.data?.find((job) => job.refreshJobId === selectedRefreshJobId) ? (
            (() => {
              const job = refreshJobsQuery.data!.find((item) => item.refreshJobId === selectedRefreshJobId)!
              return (
                <div className="space-y-2 text-sm text-slate-300">
                  <p><strong className="text-slate-100">Dataset:</strong> {job.datasetId}</p>
                  <p><strong className="text-slate-100">Read model:</strong> {job.readModelId ?? 'n/a'}</p>
                  <p><strong className="text-slate-100">Refresh type:</strong> {job.refreshType}</p>
                  <p><strong className="text-slate-100">Status:</strong> {job.status}</p>
                  <p><strong className="text-slate-100">Requested by:</strong> {job.requestedByPersonId}</p>
                  <p><strong className="text-slate-100">Queued at:</strong> {formatDate(job.queuedAt)}</p>
                  <p><strong className="text-slate-100">Started at:</strong> {formatDate(job.startedAt)}</p>
                  <p><strong className="text-slate-100">Completed at:</strong> {formatDate(job.completedAt)}</p>
                  <p><strong className="text-slate-100">Processed:</strong> {job.recordsProcessed}</p>
                  <p><strong className="text-slate-100">Created:</strong> {job.recordsCreated}</p>
                  <p><strong className="text-slate-100">Updated:</strong> {job.recordsUpdated}</p>
                  <p><strong className="text-slate-100">Skipped:</strong> {job.recordsSkipped}</p>
                  <p><strong className="text-slate-100">Error count:</strong> {job.errorCount}</p>
                  <p><strong className="text-slate-100">Error message:</strong> {job.errorMessage ?? 'none'}</p>
                </div>
              )
            })()
          ) : (
            <EmptyState title="Select a refresh job to inspect details." />
          )}
        </Panel>
      </div>
    </div>
  )
}

function ConnectorsList({
  connectors,
  selectedConnectorId = '',
  onSelectConnector = () => {},
}: {
  connectors: ReportArrSourceConnectorResponse[]
  selectedConnectorId?: string
  onSelectConnector?: (connectorId: string) => void
}) {
  if (!connectors.length) return <EmptyState title="No connectors yet." />
  return (
    <div className="reportarr-stack">
      {connectors.map((connector) => (
        <button
          key={connector.sourceConnectorId}
          type="button"
          className={['reportarr-row reportarr-row-button', connector.sourceConnectorId === selectedConnectorId ? 'active' : ''].join(' ')}
          onClick={() => onSelectConnector(connector.sourceConnectorId)}
        >
          <div className="reportarr-row-main">
            <strong>{connector.sourceProduct}</strong>
            <span>{connector.connectorType}</span>
            <small>{connector.serviceClientRef}</small>
            <small>last connected {formatDate(connector.lastConnectedAt)}</small>
            <small>last error {formatDate(connector.lastErrorAt)} {connector.lastErrorMessage ?? ''}</small>
            <small>{connector.supportedEventTypes.join(', ') || 'No event types'} · {connector.supportedDatasets.join(', ') || 'No datasets'}</small>
          </div>
          <div className="reportarr-row-meta">
            <Pill>{connector.status}</Pill>
          </div>
        </button>
      ))}
    </div>
  )
}

function DatasetFieldsList({
  fields,
  datasetId,
  selectedFieldId = '',
  onSelectField = () => {},
}: {
  fields: ReportArrDatasetFieldResponse[]
  datasetId: string
  selectedFieldId?: string
  onSelectField?: (fieldId: string) => void
}) {
  const visibleFields = fields.filter((field) => field.datasetId === datasetId)
  if (!visibleFields.length) return <EmptyState title="No dataset fields yet." />
  return (
    <div className="reportarr-stack">
      {visibleFields.map((field) => (
        <button
          key={field.fieldId}
          type="button"
          className={['reportarr-row reportarr-row-button', field.fieldId === selectedFieldId ? 'active' : ''].join(' ')}
          onClick={() => onSelectField(field.fieldId)}
        >
          <div className="reportarr-row-main">
            <strong>{field.fieldKey}</strong>
            <span>{field.displayName}</span>
            <small>{field.sourceProduct} · {field.sourceFieldPath}</small>
          </div>
          <div className="reportarr-row-meta">
            <Pill>{field.dataType}</Pill>
            <Pill>{field.filterAllowed ? 'filter' : 'view'}</Pill>
          </div>
        </button>
      ))}
    </div>
  )
}

function DatasetLineageList({ lineage, datasetId }: { lineage: ReportArrDatasetLineageResponse[]; datasetId: string }) {
  const visibleLineage = lineage.filter((item) => item.datasetId === datasetId)
  if (!visibleLineage.length) return <EmptyState title="No dataset lineage yet." />
  return (
    <div className="reportarr-stack">
      {visibleLineage.map((item) => (
        <div key={item.lineageId} className="reportarr-row">
          <div className="reportarr-row-main">
            <strong>{item.sourceProduct}</strong>
            <span>{item.sourceObjectType}</span>
            <small>{item.datasetFieldKey}</small>
            <small>{item.transformationDescription}</small>
          </div>
          <div className="reportarr-row-meta">
            <Pill>{item.confidence}</Pill>
          </div>
        </div>
      ))}
    </div>
  )
}

function IngestionCursorsList({
  cursors,
  selectedCursorId = '',
  onSelectCursor = () => {},
}: {
  cursors: ReportArrIngestionCursorResponse[]
  selectedCursorId?: string
  onSelectCursor?: (cursorId: string) => void
}) {
  if (!cursors.length) return <EmptyState title="No ingestion cursors yet." />
  return (
    <div className="reportarr-stack">
      {cursors.map((cursor) => (
        <button
          key={cursor.ingestionCursorId}
          type="button"
          className={['reportarr-row reportarr-row-button', cursor.ingestionCursorId === selectedCursorId ? 'active' : ''].join(' ')}
          onClick={() => onSelectCursor(cursor.ingestionCursorId)}
        >
          <div className="reportarr-row-main">
            <strong>{cursor.sourceProduct}</strong>
            <span>{cursor.cursorType}</span>
            <small>{cursor.cursorValue}</small>
            <small>event {cursor.lastEventId ?? 'n/a'} · last event {formatDate(cursor.lastEventAt)}</small>
            <small>last ingested {formatDate(cursor.lastIngestedAt)}</small>
            <small>{cursor.status} · last event {formatDate(cursor.lastEventAt)}</small>
          </div>
          <div className="reportarr-row-meta">
            <Pill>{cursor.sourceConnectorId}</Pill>
          </div>
        </button>
      ))}
    </div>
  )
}

function ReadModelsList({
  readModels,
  selectedReadModelId = '',
  onSelectReadModel = () => {},
}: {
  readModels: ReportArrReadModelResponse[]
  selectedReadModelId?: string
  onSelectReadModel?: (readModelId: string) => void
}) {
  if (!readModels.length) return <EmptyState title="No read models yet." />
  return (
    <div className="reportarr-stack">
      {readModels.map((model) => (
        <button
          key={model.readModelId}
          type="button"
          className={['reportarr-row reportarr-row-button', model.readModelId === selectedReadModelId ? 'active' : ''].join(' ')}
          onClick={() => onSelectReadModel(model.readModelId)}
        >
          <div className="reportarr-row-main">
            <strong>{model.readModelNumber}</strong>
            <span>{model.title}</span>
            <small>{model.description}</small>
            <small>{model.primaryEntityType} · {model.primarySourceProduct}</small>
            <small>{model.datasetRefs.join(', ') || 'No datasets'}</small>
            <small>{model.fieldDefinitions.join(', ') || 'No field definitions'}</small>
            <small>{model.refreshJobRefs.join(', ') || 'No refresh jobs'}</small>
            <small>rebuilt {formatDate(model.lastRebuiltAt)} · updated {formatDate(model.lastUpdatedAt)}</small>
          </div>
          <div className="reportarr-row-meta">
            <Pill>{model.status}</Pill>
          </div>
        </button>
      ))}
    </div>
  )
}

function ReadModelRecordsList({
  records,
  selectedRecordId = '',
  onSelectRecord = () => {},
}: {
  records: ReportArrReadModelRecordResponse[]
  selectedRecordId?: string
  onSelectRecord?: (recordId: string) => void
}) {
  if (!records.length) return <EmptyState title="No read model records yet." />
  return (
    <div className="reportarr-stack">
      {records.map((record) => (
        <button
          key={record.readModelRecordId}
          type="button"
          className={['reportarr-row reportarr-row-button', record.readModelRecordId === selectedRecordId ? 'active' : ''].join(' ')}
          onClick={() => onSelectRecord(record.readModelRecordId)}
        >
          <div className="reportarr-row-main">
            <strong>{record.primaryEntityRef}</strong>
            <span>{record.readModelId}</span>
            <small>{record.statusSnapshot}</small>
            <small>{record.sourceTraces.join(', ')}</small>
            <small>effective {formatDate(record.effectiveAt)} · source updated {formatDate(record.lastSourceUpdatedAt)}</small>
            <small>ingested {formatDate(record.ingestedAt)}</small>
          </div>
          <div className="reportarr-row-meta">
            <Pill>{formatDate(record.updatedAt)}</Pill>
          </div>
        </button>
      ))}
    </div>
  )
}

function ReadModelsPage({ accessToken }: { accessToken: string }) {
  const readModelsQuery = useQuery({
    queryKey: ['reportarr', 'read-models'],
    queryFn: () => listReadModels(accessToken),
    enabled: Boolean(accessToken),
  })
  const readModelRecordsQuery = useQuery({
    queryKey: ['reportarr', 'read-model-records'],
    queryFn: () => listReadModelRecords(accessToken),
    enabled: Boolean(accessToken),
  })
  const [selectedReadModelId, setSelectedReadModelId] = useState('')
  const [selectedReadModelRecordId, setSelectedReadModelRecordId] = useState('')

  useEffect(() => {
    const models = readModelsQuery.data ?? []
    if (!models.length) {
      setSelectedReadModelId('')
      setSelectedReadModelRecordId('')
      return
    }
    if (!models.some((model) => model.readModelId === selectedReadModelId)) {
      setSelectedReadModelId(models[0].readModelId)
    }
  }, [readModelsQuery.data, selectedReadModelId])

  const selectedReadModel = readModelsQuery.data?.find((item) => item.readModelId === selectedReadModelId) ?? null
  const visibleRecords = selectedReadModel ? readModelRecordsQuery.data?.filter((record) => record.readModelId === selectedReadModel.readModelId) ?? [] : []
  const safeRecordId = visibleRecords.some((record) => record.readModelRecordId === selectedReadModelRecordId)
    ? selectedReadModelRecordId
    : visibleRecords[0]?.readModelRecordId ?? ''
  useEffect(() => {
    setSelectedReadModelRecordId(safeRecordId)
  }, [safeRecordId])

  return (
    <div className="reportarr-page">
      <SectionHeader eyebrow="Datasets" title="Read model registry" description="Inspect every read model, its source trace, and indexed records." action={<Pill>Read models</Pill>} />
      <div className="reportarr-grid cols-2">
        <Panel title="Read models" icon={<Gauge className="h-4 w-4 text-cyan-300" />}>
          <ReadModelsList readModels={readModelsQuery.data ?? []} selectedReadModelId={selectedReadModelId} onSelectReadModel={setSelectedReadModelId} />
        </Panel>
        <Panel title="Selected read model" icon={<Gauge className="h-4 w-4 text-cyan-300" />}>
          {selectedReadModel ? (
            <div className="space-y-2 text-sm text-slate-300">
              <p><strong className="text-slate-100">Read model:</strong> {selectedReadModel.readModelNumber}</p>
              <p><strong className="text-slate-100">Title:</strong> {selectedReadModel.title}</p>
              <p><strong className="text-slate-100">Status:</strong> {selectedReadModel.status}</p>
              <p><strong className="text-slate-100">Primary source:</strong> {selectedReadModel.primarySourceProduct}</p>
              <p><strong className="text-slate-100">Datasets:</strong> {selectedReadModel.datasetRefs.join(', ') || 'none'}</p>
              <p><strong className="text-slate-100">Refresh jobs:</strong> {selectedReadModel.refreshJobRefs.join(', ') || 'none'}</p>
              <p><strong className="text-slate-100">Last rebuilt:</strong> {formatDate(selectedReadModel.lastRebuiltAt)}</p>
              <p>
                <Link className="reportarr-button secondary" to={`/read-models/${selectedReadModel.readModelId}`}>
                  Open read model detail
                </Link>
              </p>
            </div>
          ) : <EmptyState title="Select a read model to inspect." />}
        </Panel>
        <Panel title="Records" icon={<History className="h-4 w-4 text-cyan-300" />}>
          <ReadModelRecordsList records={visibleRecords} selectedRecordId={selectedReadModelRecordId} onSelectRecord={setSelectedReadModelRecordId} />
        </Panel>
      </div>
    </div>
  )
}

function RefreshJobsPage({ accessToken }: { accessToken: string }) {
  const refreshJobsQuery = useQuery({
    queryKey: ['reportarr', 'refresh-jobs'],
    queryFn: () => listRefreshJobs(accessToken),
    enabled: Boolean(accessToken),
  })
  const datasetsQuery = useQuery({
    queryKey: ['reportarr', 'datasets'],
    queryFn: () => listDatasets(accessToken),
    enabled: Boolean(accessToken),
  })
  const [selectedRefreshJobId, setSelectedRefreshJobId] = useState('')

  useEffect(() => {
    const jobs = refreshJobsQuery.data ?? []
    if (!jobs.length) {
      setSelectedRefreshJobId('')
      return
    }
    if (!jobs.some((job) => job.refreshJobId === selectedRefreshJobId)) {
      setSelectedRefreshJobId(jobs[0].refreshJobId)
    }
  }, [refreshJobsQuery.data, selectedRefreshJobId])

  const selectedRefreshJob = refreshJobsQuery.data?.find((item) => item.refreshJobId === selectedRefreshJobId) ?? null
  const selectedDataset = selectedRefreshJob ? datasetsQuery.data?.find((dataset) => dataset.datasetId === selectedRefreshJob.datasetId) ?? null : null

  return (
    <div className="reportarr-page">
      <SectionHeader eyebrow="Datasets" title="Refresh jobs" description="Review dataset refresh execution, timing, and error metrics." action={<Pill>Refresh jobs</Pill>} />
      <div className="reportarr-grid cols-2">
        <Panel title="Refresh jobs" icon={<RefreshCcw className="h-4 w-4 text-cyan-300" />}>
          <RefreshJobsList refreshJobs={refreshJobsQuery.data ?? []} selectedRefreshJobId={selectedRefreshJobId} onSelectRefreshJob={setSelectedRefreshJobId} />
        </Panel>
        <Panel title="Selected refresh job" icon={<RefreshCcw className="h-4 w-4 text-cyan-300" />}>
          {selectedRefreshJob ? (
            <div className="space-y-2 text-sm text-slate-300">
              <p><strong className="text-slate-100">Job:</strong> {selectedRefreshJob.refreshJobId}</p>
              <p><strong className="text-slate-100">Type:</strong> {selectedRefreshJob.refreshType}</p>
              <p><strong className="text-slate-100">Dataset:</strong> {selectedDataset ? `${selectedDataset.datasetNumber} · ${selectedDataset.title}` : selectedRefreshJob.datasetId}</p>
              <p><strong className="text-slate-100">Status:</strong> {selectedRefreshJob.status}</p>
              <p><strong className="text-slate-100">Requested by:</strong> {selectedRefreshJob.requestedByPersonId}</p>
              <p><strong className="text-slate-100">Queued:</strong> {formatDate(selectedRefreshJob.queuedAt)}</p>
              <p><strong className="text-slate-100">Started:</strong> {formatDate(selectedRefreshJob.startedAt)}</p>
              <p><strong className="text-slate-100">Completed:</strong> {formatDate(selectedRefreshJob.completedAt)}</p>
              <p><strong className="text-slate-100">Records:</strong> {formatNumber(selectedRefreshJob.recordsCreated)} created · {formatNumber(selectedRefreshJob.recordsUpdated)} updated</p>
              <p><strong className="text-slate-100">Skipped / errored:</strong> {formatNumber(selectedRefreshJob.recordsSkipped)} / {formatNumber(selectedRefreshJob.errorCount)}</p>
              <p>
                <Link className="reportarr-button secondary" to={`/refresh-jobs/${selectedRefreshJob.refreshJobId}`}>
                  Open refresh-job detail
                </Link>
              </p>
            </div>
          ) : <EmptyState title="Select a refresh job to inspect." />}
        </Panel>
      </div>
    </div>
  )
}

function ReportBuilderPage({
  accessToken,
  roleKey,
  isPlatformAdmin,
}: {
  accessToken: string
  roleKey: string
  isPlatformAdmin: boolean
}) {
  const queryClient = useQueryClient()
  const reportsQuery = useQuery({
    queryKey: ['reportarr', 'reports'],
    queryFn: () => listReportDefinitions(accessToken),
    enabled: Boolean(accessToken),
  })
  const policiesQuery = useQuery({
    queryKey: ['reportarr', 'report-access-policies'],
    queryFn: () => listReportAccessPolicies(accessToken),
    enabled: Boolean(accessToken),
  })
  const [reportForm, setReportForm] = useState({
    reportKey: 'operational-pack',
    title: 'Operational report pack',
    description: 'Summarizes operational posture for the current cycle.',
    reportType: 'operational',
    layoutDefinition: 'layout:split:summary',
    exportFormats: ['pdf', 'csv'],
    ownerPersonId: 'person-ops-analyst',
    datasetRefs: '',
    readModelRefs: '',
    parameterRefs: '',
    defaultFilters: '',
    sectionRefs: '',
    accessPolicyRef: '',
  })
  const [selectedReportId, setSelectedReportId] = useState('rpt-001')

  const canBuildReports = canUseReportArrAction(roleKey, isPlatformAdmin, ['report_builder', 'reportarr_builder', 'tenant_admin', 'reportarr_admin'])
  const createReportMutation = useMutation({
    mutationFn: () =>
      createReportDefinition(accessToken, {
        ...reportForm,
        accessPolicyRef: reportForm.accessPolicyRef || undefined,
        datasetRefs: parseCsvList(reportForm.datasetRefs),
        readModelRefs: parseCsvList(reportForm.readModelRefs),
        parameterRefs: parseCsvList(reportForm.parameterRefs),
        defaultFilters: parseCsvList(reportForm.defaultFilters),
        sectionRefs: parseCsvList(reportForm.sectionRefs),
      }),
    onSuccess: async (report) => {
      setSelectedReportId(report.reportDefinitionId)
      await queryClient.invalidateQueries({ queryKey: ['reportarr'] })
    },
  })
  const updateReportMutation = useMutation({
    mutationFn: (status: string) => {
      if (!selectedReportId) {
        throw new Error('No report selected.')
      }
      return updateReportDefinition(accessToken, selectedReportId, {
        status,
        requestedByPersonId: 'person-ops-analyst',
      })
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['reportarr'] })
    },
  })

  const selectedReport = reportsQuery.data?.find((item) => item.reportDefinitionId === selectedReportId) ?? null
  const selectedPolicy = policiesQuery.data?.find((policy) => policy.reportDefinitionId === selectedReportId) ?? null

  useEffect(() => {
    const reports = reportsQuery.data ?? []
    if (!reports.length) {
      setSelectedReportId('')
      return
    }
    if (!reports.some((report) => report.reportDefinitionId === selectedReportId)) {
      setSelectedReportId(reports[0].reportDefinitionId)
    }
  }, [reportsQuery.data, selectedReportId])

  const canUpdateSelectedReport = selectedPolicy?.allowedPermissionRefs.some((permission) => permission === 'reportarr.reports.update') ?? false

  return (
    <div className="reportarr-page">
      <SectionHeader
        eyebrow="Reports"
        title="Report builder"
        description="Create a report definition, define layout and export formats, then activate and preview the definition."
      />
      {canBuildReports ? (
        <>
          <Panel title="Report metadata" icon={<FileText className="h-4 w-4 text-cyan-300" />}>
            <div className="grid gap-3 md:grid-cols-2">
              <TextInput value={reportForm.reportKey} onChange={(value) => setReportForm({ ...reportForm, reportKey: value })} placeholder="report-key" />
              <TextInput value={reportForm.title} onChange={(value) => setReportForm({ ...reportForm, title: value })} placeholder="Title" />
              <TextInput value={reportForm.reportType} onChange={(value) => setReportForm({ ...reportForm, reportType: value })} placeholder="Type" />
              <TextInput value={reportForm.layoutDefinition} onChange={(value) => setReportForm({ ...reportForm, layoutDefinition: value })} placeholder="Layout definition" />
              <OwnerPersonPicker value={reportForm.ownerPersonId} onChange={(ownerPersonId) => setReportForm({ ...reportForm, ownerPersonId })} />
              <div className="md:col-span-2">
                <div className="mb-2 text-sm text-slate-300">Access policy</div>
                <select
                  className="reportarr-input"
                  value={reportForm.accessPolicyRef || 'default'}
                  onChange={(event) => {
                    const nextValue = event.target.value === 'default' ? '' : event.target.value
                    setReportForm({ ...reportForm, accessPolicyRef: nextValue })
                  }}
                >
                  <option value="default">Use default policy</option>
                  {policiesQuery.data?.map((policy) => (
                    <option key={policy.accessPolicyId} value={policy.accessPolicyId}>
                      {policy.accessPolicyId}
                    </option>
                  ))}
                </select>
              </div>
              <div className="md:col-span-2">
                <TextArea value={reportForm.description} onChange={(value) => setReportForm({ ...reportForm, description: value })} placeholder="Description" />
              </div>
              <div className="md:col-span-2">
                <TextArea
                  value={reportForm.datasetRefs}
                  onChange={(value) => setReportForm({ ...reportForm, datasetRefs: value })}
                  placeholder="Dataset references (comma-separated IDs)"
                />
              </div>
              <div className="md:col-span-2">
                <TextArea
                  value={reportForm.readModelRefs}
                  onChange={(value) => setReportForm({ ...reportForm, readModelRefs: value })}
                  placeholder="Read model references (comma-separated IDs)"
                />
              </div>
              <div className="md:col-span-2">
                <TextArea
                  value={reportForm.parameterRefs}
                  onChange={(value) => setReportForm({ ...reportForm, parameterRefs: value })}
                  placeholder="Parameter references (comma-separated IDs)"
                />
              </div>
              <div className="md:col-span-2">
                <TextArea
                  value={reportForm.defaultFilters}
                  onChange={(value) => setReportForm({ ...reportForm, defaultFilters: value })}
                  placeholder="Default filters (comma-separated IDs)"
                />
              </div>
              <div className="md:col-span-2">
                <TextArea
                  value={reportForm.sectionRefs}
                  onChange={(value) => setReportForm({ ...reportForm, sectionRefs: value })}
                  placeholder="Section references (comma-separated IDs)"
                />
              </div>
              <div className="md:col-span-2">
                <div className="mb-2 text-sm text-slate-300">Export formats</div>
                <div className="flex flex-wrap gap-2">
                  {reportExportFormatOptions.map((format) => {
                    const active = reportForm.exportFormats.includes(format)
                    return (
                      <button
                        key={format}
                        type="button"
                        className={['reportarr-button secondary', active ? 'ring-2 ring-cyan-400 bg-cyan-400/10 text-cyan-100' : ''].join(' ')}
                        onClick={() =>
                          setReportForm((current) => ({
                            ...current,
                            exportFormats: current.exportFormats.includes(format)
                              ? current.exportFormats.filter((item) => item !== format)
                              : [...current.exportFormats, format],
                          }))
                        }
                      >
                        {format}
                      </button>
                    )
                  })}
                </div>
              </div>
            </div>
            <div className="mt-4 flex flex-wrap gap-3">
              <button
                className="reportarr-button"
                type="button"
                onClick={() => createReportMutation.mutate()}
                disabled={createReportMutation.isPending}
              >
                {createReportMutation.isPending ? 'Creating…' : 'Save report definition'}
              </button>
              <button
                className="reportarr-button secondary"
                type="button"
                onClick={() => updateReportMutation.mutate('active')}
                disabled={updateReportMutation.isPending || !selectedReportId || !canUpdateSelectedReport}
              >
                {updateReportMutation.isPending ? 'Saving…' : 'Activate selected report'}
              </button>
              <button
                className="reportarr-button secondary"
                type="button"
                onClick={() => updateReportMutation.mutate('archived')}
                disabled={updateReportMutation.isPending || !selectedReportId || !canUpdateSelectedReport}
              >
                {updateReportMutation.isPending ? 'Saving…' : 'Archive selected report'}
              </button>
            </div>
          </Panel>
          <Panel title="Preview" icon={<PlayCircle className="h-4 w-4 text-cyan-300" />}>
            <div className="space-y-2 text-sm text-slate-300">
              <p><strong className="text-slate-100">Selected report:</strong> {selectedReport ? selectedReport.reportNumber : 'none selected'}</p>
              <p><strong className="text-slate-100">Status:</strong> {selectedReport?.status ?? 'n/a'}</p>
              <p><strong className="text-slate-100">Datasets:</strong> {(selectedReport?.datasetRefs ?? parseCsvList(reportForm.datasetRefs)).join(', ') || 'none'}</p>
              <p><strong className="text-slate-100">Read models:</strong> {(selectedReport?.readModelRefs ?? parseCsvList(reportForm.readModelRefs)).join(', ') || 'none'}</p>
              <p><strong className="text-slate-100">Parameters:</strong> {(selectedReport?.parameterRefs ?? parseCsvList(reportForm.parameterRefs)).join(', ') || 'none'}</p>
              <p><strong className="text-slate-100">Filters:</strong> {(selectedReport?.defaultFilters ?? parseCsvList(reportForm.defaultFilters)).join(', ') || 'none'}</p>
              <p><strong className="text-slate-100">Sections:</strong> {(selectedReport?.sectionRefs ?? parseCsvList(reportForm.sectionRefs)).join(', ') || 'none'}</p>
              <p><strong className="text-slate-100">Title:</strong> {selectedReport?.title ?? reportForm.title}</p>
              <p><strong className="text-slate-100">Layout:</strong> {selectedReport ? summarizeConfiguredField(selectedReport.layoutDefinition, 'layout') : reportForm.layoutDefinition}</p>
              <p><strong className="text-slate-100">Export formats:</strong> {(selectedReport?.exportFormats ?? reportForm.exportFormats).join(', ') || 'n/a'}</p>
              <p><strong className="text-slate-100">Access policy:</strong> {selectedPolicy ? selectedPolicy.visibility : 'default'}</p>
              <p><strong className="text-slate-100">Requested policy ref:</strong> {reportForm.accessPolicyRef || 'none'}</p>
            </div>
          </Panel>
        </>
      ) : (
        <EmptyState title="You do not have permission to build reports." />
      )}

      {canBuildReports ? (
        <div className="mt-4">
          <Link className="reportarr-button secondary" to="/reports">
            Open full reports operations
          </Link>
        </div>
      ) : null}
    </div>
  )
}

function RefreshJobsList({
  refreshJobs,
  selectedRefreshJobId = '',
  datasetId,
  onSelectRefreshJob = () => {},
}: {
  refreshJobs: ReportArrRefreshJobResponse[]
  selectedRefreshJobId?: string
  datasetId?: string
  onSelectRefreshJob?: (refreshJobId: string) => void
}) {
  const visibleRefreshJobs = datasetId
    ? refreshJobs.filter((job) => job.datasetId === datasetId)
    : refreshJobs
  if (!visibleRefreshJobs.length) return <EmptyState title={datasetId ? 'No refresh jobs for this dataset yet.' : 'No refresh jobs yet.'} />
  return (
    <div className="reportarr-stack">
      {visibleRefreshJobs.map((job) => (
        <button
          key={job.refreshJobId}
          type="button"
          className={['reportarr-row reportarr-row-button', job.refreshJobId === selectedRefreshJobId ? 'active' : ''].join(' ')}
          onClick={() => onSelectRefreshJob(job.refreshJobId)}
        >
          <div className="reportarr-row-main">
            <strong>{job.refreshJobId}</strong>
            <span>{job.refreshType} · {job.datasetId}</span>
            <small>{job.status} · queued {formatDate(job.queuedAt)} · started {formatDate(job.startedAt)}</small>
            <small>completed {formatDate(job.completedAt)} · requested by {job.requestedByPersonId}</small>
            <small>{job.recordsProcessed} processed · {job.recordsCreated} created · {job.recordsUpdated} updated · {job.recordsSkipped} skipped</small>
            <small>{job.errorCount} errors {job.errorMessage ? `· ${job.errorMessage}` : ''}</small>
          </div>
          <div className="reportarr-row-meta">
            <Pill>{job.recordsProcessed} rows</Pill>
          </div>
        </button>
      ))}
    </div>
  )
}

function IngestionErrorsList({
  events,
  dataset,
}: {
  events: ReportArrSourceEventReceiptResponse[]
  dataset: ReportArrDatasetResponse | null
}) {
  if (!dataset) {
    return <EmptyState title="Select a dataset to inspect ingestion errors." />
  }

  const failedEvents = events.filter(
    (event) =>
      event.status === 'failed' &&
      dataset.sourceProducts.some((sourceProduct) => sourceProduct.toLowerCase() === event.sourceProduct.toLowerCase()),
  )

  if (!failedEvents.length) {
    return <EmptyState title="No failed ingestion events for this dataset." />
  }

  return (
    <div className="reportarr-stack">
      {failedEvents.map((event) => (
        <div key={event.sourceEventReceiptId} className="reportarr-row">
          <div className="reportarr-row-main">
            <strong>{event.sourceEventId}</strong>
            <span>{event.sourceProduct} · {event.eventType}</span>
            <small>{event.sourceObjectRef ?? 'n/a'}</small>
            <small>{event.failureReason ?? 'No failure reason provided.'}</small>
            <small>received {formatDate(event.receivedAt)} · processed {formatDate(event.processedAt)}</small>
            <small>{event.correlationId ?? 'no correlation'}</small>
          </div>
          <div className="reportarr-row-meta">
            <Pill>{event.status}</Pill>
          </div>
        </div>
      ))}
    </div>
  )
}

function DashboardsPage({ accessToken, roleKey, isPlatformAdmin }: { accessToken: string; roleKey: string; isPlatformAdmin: boolean }) {
  const queryClient = useQueryClient()
  const canBuildReports = canUseReportArrAction(roleKey, isPlatformAdmin, ['report_builder', 'reportarr_builder', 'tenant_admin', 'reportarr_admin'])
  const canRunReports = canUseReportArrAction(roleKey, isPlatformAdmin, ['report_runner', 'reportarr_runner', 'report_builder', 'tenant_admin', 'reportarr_admin'])
  const dashboardPoliciesQuery = useQuery({
    queryKey: ['reportarr', 'dashboard-access-policies'],
    queryFn: () => listDashboardAccessPolicies(accessToken),
    enabled: Boolean(accessToken),
  })
  const dashboardsQuery = useQuery({
    queryKey: ['reportarr', 'dashboards'],
    queryFn: () => listDashboards(accessToken),
    enabled: Boolean(accessToken),
  })
  const widgetsQuery = useQuery({
    queryKey: ['reportarr', 'widgets'],
    queryFn: () => listWidgets(accessToken),
    enabled: Boolean(accessToken),
  })
  const dashboardFiltersQuery = useQuery({
    queryKey: ['reportarr', 'dashboard-filters'],
    queryFn: () => listDashboardFilters(accessToken),
    enabled: Boolean(accessToken),
  })
  const drilldownsQuery = useQuery({
    queryKey: ['reportarr', 'drilldowns'],
    queryFn: () => listDrilldowns(accessToken),
    enabled: Boolean(accessToken),
  })
  const visualizationsQuery = useQuery({
    queryKey: ['reportarr', 'widget-visualizations'],
    queryFn: () => listWidgetVisualizations(accessToken),
    enabled: Boolean(accessToken),
  })
  const [selectedDashboardId, setSelectedDashboardId] = useState('dash-001')
  const [selectedWidgetId, setSelectedWidgetId] = useState('')
  const [selectedWidgetVisualizationId, setSelectedWidgetVisualizationId] = useState('')
  const [form, setForm] = useState({
    dashboardKey: 'executive-command',
    title: 'Executive command',
    description: 'Overview for leadership.',
    dashboardType: 'executive',
    defaultDateRange: 'last_30_days',
    ownerPersonId: 'person-exec-lead',
  })

  const createMutation = useMutation({
    mutationFn: () => createDashboard(accessToken, form),
    onSuccess: async (dashboard) => {
      setSelectedDashboardId(dashboard.dashboardId)
      await queryClient.invalidateQueries({ queryKey: ['reportarr'] })
    },
  })
  const selectedDashboard = dashboardsQuery.data?.find((dashboard) => dashboard.dashboardId === selectedDashboardId) ?? null
  const selectedDashboardPolicy = dashboardPoliciesQuery.data?.find((policy) => policy.dashboardId === selectedDashboardId) ?? null
  const canUpdateSelectedDashboard = Boolean(
    selectedDashboardPolicy?.allowedPermissionRefs.some((permission) => permission === 'reportarr.dashboards.update') &&
    (selectedDashboardPolicy.exportAllowed || selectedDashboardPolicy.visibility),
  )
  const selectedDashboardWidgets = (widgetsQuery.data ?? []).filter((widget) => selectedDashboard?.widgetRefs.includes(widget.widgetId))
  useEffect(() => {
    const widgets = selectedDashboardWidgets
    if (!widgets.length) return
    if (!widgets.some((widget) => widget.widgetId === selectedWidgetId)) {
      setSelectedWidgetId(widgets[0].widgetId)
    }
  }, [selectedWidgetId, selectedDashboardWidgets])

  useEffect(() => {
    const visualizations = visualizationsQuery.data ?? []
    if (!visualizations.length) return
    if (!visualizations.some((item) => item.widgetId === selectedWidgetVisualizationId)) {
      setSelectedWidgetVisualizationId(visualizations[0].widgetId)
    }
  }, [selectedWidgetVisualizationId, visualizationsQuery.data])

  const updateMutation = useMutation({
    mutationFn: (status: string) =>
      updateDashboard(accessToken, selectedDashboardId, {
        title: form.title,
        description: form.description,
        status,
        defaultDateRange: form.defaultDateRange,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['reportarr'] })
    },
  })
  const exportMutation = useMutation({
    mutationFn: () =>
      createExport(accessToken, {
        reportRunId: null,
        exportType: 'dashboard',
        sourceRef: selectedDashboardId,
        exportFormat: 'pdf',
        requestedByPersonId: 'person-exec-lead',
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['reportarr'] })
    },
  })

  return (
    <div className="reportarr-page">
      <SectionHeader
        eyebrow="Dashboards"
        title="Dashboard and widget registry"
        description="Track dashboard ownership, filters, widgets, and the widget render settings that feed the user-facing scorecards."
        action={<Pill><BarChart3 className="h-4 w-4" /> {dashboardsQuery.data?.length ?? 0} dashboards</Pill>}
      />
      {canBuildReports ? (
        <Panel title="Create or update dashboard" icon={<Plus className="h-4 w-4 text-cyan-300" />}>
          <div className="grid gap-3 md:grid-cols-2">
            <TextInput value={form.dashboardKey} onChange={(value) => setForm({ ...form, dashboardKey: value })} placeholder="dashboard-key" />
            <TextInput value={form.title} onChange={(value) => setForm({ ...form, title: value })} placeholder="Title" />
            <TextInput value={form.dashboardType} onChange={(value) => setForm({ ...form, dashboardType: value })} placeholder="Type" />
            <TextInput value={form.defaultDateRange} onChange={(value) => setForm({ ...form, defaultDateRange: value })} placeholder="Default date range" />
            <OwnerPersonPicker value={form.ownerPersonId} onChange={(ownerPersonId) => setForm({ ...form, ownerPersonId })} />
            <div className="md:col-span-2">
              <TextArea value={form.description} onChange={(value) => setForm({ ...form, description: value })} placeholder="Description" />
            </div>
          </div>
          <div className="mt-4 flex flex-wrap gap-3">
            <button className="reportarr-button" type="button" onClick={() => createMutation.mutate()} disabled={createMutation.isPending}>
              {createMutation.isPending ? 'Creating…' : 'Create dashboard'}
            </button>
            <button className="reportarr-button secondary" type="button" onClick={() => updateMutation.mutate('active')} disabled={updateMutation.isPending || !canUpdateSelectedDashboard}>
              {updateMutation.isPending ? 'Activating…' : 'Activate selected'}
            </button>
            <button className="reportarr-button secondary" type="button" onClick={() => updateMutation.mutate('paused')} disabled={updateMutation.isPending || !canUpdateSelectedDashboard}>
              {updateMutation.isPending ? 'Pausing…' : 'Pause selected'}
            </button>
            <button className="reportarr-button secondary" type="button" onClick={() => updateMutation.mutate('archived')} disabled={updateMutation.isPending || !canUpdateSelectedDashboard}>
              {updateMutation.isPending ? 'Archiving…' : 'Archive selected'}
            </button>
          </div>
        </Panel>
      ) : null}
      <div className="reportarr-grid cols-2">
        <Panel title="Dashboards" icon={<BarChart3 className="h-4 w-4 text-cyan-300" />}>
          <div className="reportarr-stack">
            {(dashboardsQuery.data ?? []).map((dashboard) => (
              <button
                key={dashboard.dashboardId}
                type="button"
                className={['reportarr-row reportarr-row-button', dashboard.dashboardId === selectedDashboardId ? 'active' : ''].join(' ')}
                onClick={() => setSelectedDashboardId(dashboard.dashboardId)}
              >
                <div className="reportarr-row-main">
                  <strong>{dashboard.dashboardNumber}</strong>
                  <span>{dashboard.title}</span>
                  <small>{dashboard.description}</small>
                </div>
                <div className="reportarr-row-meta">
                  <Pill>{dashboard.status}</Pill>
                  <Pill>{dashboard.freshnessStatus}</Pill>
                </div>
              </button>
            ))}
            {!dashboardsQuery.data?.length ? <EmptyState title="No dashboards yet." /> : null}
          </div>
        </Panel>
        <Panel title="Selected dashboard" icon={<Gauge className="h-4 w-4 text-cyan-300" />}>
          {dashboardsQuery.data?.find((dashboard) => dashboard.dashboardId === selectedDashboardId) ? (
            (() => {
              const dashboard = dashboardsQuery.data!.find((item) => item.dashboardId === selectedDashboardId)!
              const dashboardWidgets = (widgetsQuery.data ?? []).filter((widget) => dashboard.widgetRefs.includes(widget.widgetId))
              const dashboardFilters = (dashboardFiltersQuery.data ?? []).filter((filter) => filter.dashboardId === dashboard.dashboardId)
              const dashboardDrilldowns = (drilldownsQuery.data ?? []).filter((drilldown) => dashboard.drilldownRefs.includes(drilldown.drilldownId))
              const sourceTraceSummary = dashboardWidgets.length
                ? dashboardWidgets
                    .map((widget) => `${widget.widgetKey}: ${widget.datasetRef}${widget.readModelRef ? ` / ${widget.readModelRef}` : ''}`)
                    .join('; ')
                : 'No widget source trace available.'
              return (
                <div className="space-y-2 text-sm text-slate-300">
                  <p><strong className="text-slate-100">Freshness:</strong> {dashboard.freshnessStatus}</p>
                  <p><strong className="text-slate-100">Type:</strong> {dashboard.dashboardType}</p>
                  <p><strong className="text-slate-100">Default range:</strong> {dashboard.defaultDateRange}</p>
                  <p><strong className="text-slate-100">Widgets:</strong> {dashboard.widgetRefs.join(', ') || 'none'}</p>
                  <p><strong className="text-slate-100">Filters:</strong> {dashboardFilters.map((filter) => `${filter.filterKey} (${filter.label})`).join(', ') || dashboard.filterRefs.join(', ') || 'none'}</p>
                  <p><strong className="text-slate-100">Filter defaults:</strong> {dashboardFilters.map((filter) => `${filter.filterKey}=${filter.defaultValue || 'n/a'}`).join(', ') || 'none'}</p>
                  <p><strong className="text-slate-100">Filter visibility:</strong> {dashboardFilters.map((filter) => `${filter.filterKey}:${filter.visible ? 'visible' : 'hidden'}`).join(', ') || 'none'}</p>
                  <p><strong className="text-slate-100">Drilldowns:</strong> {dashboardDrilldowns.map((drilldown) => `${drilldown.title} (${drilldown.targetType})`).join(', ') || 'none'}</p>
                  <p><strong className="text-slate-100">Access policy:</strong> {dashboard.accessPolicyRef}</p>
                  <p><strong className="text-slate-100">Export allowed:</strong> {selectedDashboardPolicy?.exportAllowed ? 'yes' : 'no'}</p>
                  <p><strong className="text-slate-100">Source trace summary:</strong> {sourceTraceSummary}</p>
                  <p><strong className="text-slate-100">Drilldown links:</strong> {dashboardDrilldowns.map((drilldown) => drilldown.targetRef).join(', ') || 'none'}</p>
                  <p><strong className="text-slate-100">Last viewed:</strong> {formatDate(dashboard.lastViewedAt)}</p>
                  <p><strong className="text-slate-100">Created by:</strong> {dashboard.createdByPersonId}</p>
                  <p><strong className="text-slate-100">Updated by:</strong> {dashboard.updatedByPersonId}</p>
                  {canRunReports && selectedDashboardPolicy?.exportAllowed ? (
                    <div className="pt-2">
                      <button className="reportarr-button secondary" type="button" onClick={() => exportMutation.mutate()} disabled={exportMutation.isPending}>
                        {exportMutation.isPending ? 'Exporting…' : 'Export dashboard'}
                      </button>
                    </div>
                  ) : canRunReports ? (
                    <div className="pt-2 text-xs text-slate-400">
                      Dashboard exports are disabled by the selected access policy.
                    </div>
                  ) : null}
                </div>
              )
            })()
          ) : (
            <EmptyState title="Select a dashboard to inspect details." />
          )}
        </Panel>
        <Panel title="Widgets" icon={<Layers3 className="h-4 w-4 text-cyan-300" />}>
          <div className="reportarr-stack">
            {selectedDashboardWidgets.map((widget) => (
              <button
                key={widget.widgetId}
                type="button"
                className={['reportarr-row reportarr-row-button', widget.widgetId === selectedWidgetId ? 'active' : ''].join(' ')}
                onClick={() => setSelectedWidgetId(widget.widgetId)}
              >
                <div className="reportarr-row-main">
                  <strong>{widget.widgetKey}</strong>
                  <span>{widget.title}</span>
                  <small>{widget.datasetRef} · {widget.readModelRef}</small>
                  <small>{widget.description}</small>
                </div>
                <div className="reportarr-row-meta">
                  <Pill>{widget.status}</Pill>
                  <Pill>{widget.widgetType}</Pill>
                </div>
              </button>
            ))}
            {!selectedDashboardWidgets.length ? <EmptyState title="No widgets on the selected dashboard." /> : null}
          </div>
        </Panel>
        <Panel title="Selected widget" icon={<Layers3 className="h-4 w-4 text-cyan-300" />}>
          {widgetsQuery.data?.find((widget) => widget.widgetId === selectedWidgetId) ? (
            (() => {
              const widget = widgetsQuery.data!.find((item) => item.widgetId === selectedWidgetId)!
              const visualization = visualizationsQuery.data?.find((item) => item.widgetId === widget.widgetId)
              return (
                <div className="space-y-2 text-sm text-slate-300">
                  <p><strong className="text-slate-100">Widget key:</strong> {widget.widgetKey}</p>
                  <p><strong className="text-slate-100">Title:</strong> {widget.title}</p>
                  <p><strong className="text-slate-100">Description:</strong> {widget.description}</p>
                  <p><strong className="text-slate-100">Type:</strong> {widget.widgetType}</p>
                  <p><strong className="text-slate-100">Status:</strong> {widget.status}</p>
                  <p><strong className="text-slate-100">Dataset ref:</strong> {widget.datasetRef}</p>
                  <p><strong className="text-slate-100">Read model ref:</strong> {widget.readModelRef}</p>
                  <p><strong className="text-slate-100">Query definition:</strong> {summarizeConfiguredField(widget.queryDefinition, 'query')}</p>
                  <p><strong className="text-slate-100">Filter bindings:</strong> {widget.filterBindings.join(', ') || 'none'}</p>
                  <p><strong className="text-slate-100">Drilldown target:</strong> {widget.drilldownTargetRef}</p>
                  <p><strong className="text-slate-100">Sort order:</strong> {widget.sortOrder}</p>
                  <p><strong className="text-slate-100">Layout:</strong> {summarizeConfiguredField(widget.layout, 'layout')}</p>
                  <p><strong className="text-slate-100">Visualization settings:</strong> {visualization ? `${visualization.chartType} · ${visualization.displayFormat} · max ${visualization.maxRows}` : 'configured'}</p>
                  {visualization ? (
                    <>
                      <p><strong className="text-slate-100">Fields:</strong> {[visualization.xField, visualization.yField, visualization.seriesField, visualization.groupField, visualization.valueField, visualization.labelField, visualization.dateField].filter(Boolean).join(', ') || 'none'}</p>
                      <p><strong className="text-slate-100">Rules:</strong> {[
                        visualization.colorRuleRefs.length ? `${visualization.colorRuleRefs.length} color rules` : null,
                        visualization.thresholdRefs.length ? `${visualization.thresholdRefs.length} thresholds` : null,
                        visualization.showLegend ? 'legend on' : null,
                        visualization.showDataLabels ? 'labels on' : null,
                      ].filter(Boolean).join(' · ') || 'none'}</p>
                    </>
                  ) : null}
                  <p><strong className="text-slate-100">Freshness:</strong> {widget.freshnessStatus}</p>
                  <p><strong className="text-slate-100">Last rendered:</strong> {formatDate(widget.lastRenderedAt)}</p>
                  <div className="pt-2">
                    <button
                      className="reportarr-button secondary"
                      type="button"
                      onClick={() => renderWidget(accessToken, widget.widgetId).then(() => queryClient.invalidateQueries({ queryKey: ['reportarr'] }))}
                    >
                      Render widget
                    </button>
                  </div>
                </div>
              )
            })()
          ) : (
            <EmptyState title="Select a widget to inspect details." />
          )}
        </Panel>
        <Panel title="Widget visualizations" icon={<PlayCircle className="h-4 w-4 text-cyan-300" />}>
          <VisualizationList
            visualizations={visualizationsQuery.data ?? []}
            selectedWidgetVisualizationId={selectedWidgetVisualizationId}
            onSelectWidgetVisualization={setSelectedWidgetVisualizationId}
          />
        </Panel>
        <Panel title="Selected visualization" icon={<PlayCircle className="h-4 w-4 text-cyan-300" />}>
          {visualizationsQuery.data?.find((item) => item.widgetId === selectedWidgetVisualizationId) ? (
            (() => {
              const item = visualizationsQuery.data!.find((visualization) => visualization.widgetId === selectedWidgetVisualizationId)!
              return (
                <div className="space-y-2 text-sm text-slate-300">
                  <p><strong className="text-slate-100">Widget:</strong> {item.widgetId}</p>
                  <p><strong className="text-slate-100">Chart type:</strong> {item.chartType}</p>
                  <p><strong className="text-slate-100">X field:</strong> {item.xField ?? 'n/a'}</p>
                  <p><strong className="text-slate-100">Y field:</strong> {item.yField ?? 'n/a'}</p>
                  <p><strong className="text-slate-100">Series field:</strong> {item.seriesField ?? 'n/a'}</p>
                  <p><strong className="text-slate-100">Group field:</strong> {item.groupField ?? 'n/a'}</p>
                  <p><strong className="text-slate-100">Value field:</strong> {item.valueField ?? 'n/a'}</p>
                  <p><strong className="text-slate-100">Label field:</strong> {item.labelField ?? 'n/a'}</p>
                  <p><strong className="text-slate-100">Date field:</strong> {item.dateField ?? 'n/a'}</p>
                  <p><strong className="text-slate-100">Color rules:</strong> {item.colorRuleRefs.join(', ') || 'none'}</p>
                  <p><strong className="text-slate-100">Thresholds:</strong> {item.thresholdRefs.join(', ') || 'none'}</p>
                  <p><strong className="text-slate-100">Display format:</strong> {item.displayFormat}</p>
                  <p><strong className="text-slate-100">Legend:</strong> {item.showLegend ? 'yes' : 'no'}</p>
                  <p><strong className="text-slate-100">Data labels:</strong> {item.showDataLabels ? 'yes' : 'no'}</p>
                  <p><strong className="text-slate-100">Max rows:</strong> {item.maxRows}</p>
                </div>
              )
            })()
          ) : (
            <EmptyState title="Select a visualization to inspect details." />
          )}
        </Panel>
      </div>
    </div>
  )
}

function VisualizationList({
  visualizations,
  selectedWidgetVisualizationId = '',
  onSelectWidgetVisualization = () => {},
}: {
  visualizations: ReportArrWidgetVisualizationSettingsResponse[]
  selectedWidgetVisualizationId?: string
  onSelectWidgetVisualization?: (widgetId: string) => void
}) {
  if (!visualizations.length) return <EmptyState title="No widget visualizations yet." />
  return (
    <div className="reportarr-stack">
      {visualizations.map((item) => (
        <button
          key={item.widgetId}
          type="button"
          className={['reportarr-row reportarr-row-button', item.widgetId === selectedWidgetVisualizationId ? 'active' : ''].join(' ')}
          onClick={() => onSelectWidgetVisualization(item.widgetId)}
        >
          <div className="reportarr-row-main">
            <strong>{item.widgetId}</strong>
            <span>{item.chartType}</span>
            <small>{item.displayFormat}</small>
            <small>legend {item.showLegend ? 'on' : 'off'} · labels {item.showDataLabels ? 'on' : 'off'}</small>
            <small>{item.colorRuleRefs.join(', ') || 'No color rules'}</small>
            <small>{item.thresholdRefs.join(', ') || 'No thresholds'}</small>
          </div>
          <div className="reportarr-row-meta">
            <Pill>{item.maxRows} rows</Pill>
          </div>
        </button>
      ))}
    </div>
  )
}

function ReportsPage({
  accessToken,
  roleKey,
  isPlatformAdmin,
}: {
  accessToken: string
  roleKey: string
  isPlatformAdmin: boolean
}) {
  const queryClient = useQueryClient()
  const reportsQuery = useQuery({
    queryKey: ['reportarr', 'reports'],
    queryFn: () => listReportDefinitions(accessToken),
    enabled: Boolean(accessToken),
  })
  const schedulesQuery = useQuery({
    queryKey: ['reportarr', 'report-schedules'],
    queryFn: () => listReportSchedules(accessToken),
    enabled: Boolean(accessToken),
  })
  const recipientsQuery = useQuery({
    queryKey: ['reportarr', 'report-recipients'],
    queryFn: () => listReportRecipients(accessToken),
    enabled: Boolean(accessToken),
  })
  const reportParametersQuery = useQuery({
    queryKey: ['reportarr', 'report-parameters'],
    queryFn: () => listReportParameters(accessToken),
    enabled: Boolean(accessToken),
  })
  const reportSectionsQuery = useQuery({
    queryKey: ['reportarr', 'report-sections'],
    queryFn: () => listReportSections(accessToken),
    enabled: Boolean(accessToken),
  })
  const dashboardsQuery = useQuery({
    queryKey: ['reportarr', 'dashboards'],
    queryFn: () => listDashboards(accessToken),
    enabled: Boolean(accessToken),
  })
  const dashboardPoliciesQuery = useQuery({
    queryKey: ['reportarr', 'dashboard-access-policies'],
    queryFn: () => listDashboardAccessPolicies(accessToken),
    enabled: Boolean(accessToken),
  })
  const reportPoliciesQuery = useQuery({
    queryKey: ['reportarr', 'report-access-policies'],
    queryFn: () => listReportAccessPolicies(accessToken),
    enabled: Boolean(accessToken),
  })
  const runsQuery = useQuery({
    queryKey: ['reportarr', 'report-runs'],
    queryFn: () => listReportRuns(accessToken),
    enabled: Boolean(accessToken),
  })
  const exportsQuery = useQuery({
    queryKey: ['reportarr', 'exports'],
    queryFn: () => listExportJobs(accessToken),
    enabled: Boolean(accessToken),
  })
  const [selectedReportId, setSelectedReportId] = useState('rpt-001')
  const [selectedReportRunId, setSelectedReportRunId] = useState('run-001')
  const [selectedReportSectionId, setSelectedReportSectionId] = useState('')
  const [selectedScheduleId, setSelectedScheduleId] = useState('')
  const [selectedReportRecipientId, setSelectedReportRecipientId] = useState('')
  const [selectedReportParameterId, setSelectedReportParameterId] = useState('')
  const [selectedExportJobId, setSelectedExportJobId] = useState('')
  const [reportForm, setReportForm] = useState({
    reportKey: 'operational-pack',
    title: 'Operational report pack',
    description: 'Summarizes operational posture for the current cycle.',
    reportType: 'operational',
    layoutDefinition: 'layout:split:summary',
    exportFormats: ['pdf', 'csv'],
    ownerPersonId: 'person-ops-analyst',
    datasetRefs: '',
    readModelRefs: '',
    parameterRefs: '',
    defaultFilters: '',
    sectionRefs: '',
    accessPolicyRef: '',
  })
  const [scheduleForm, setScheduleForm] = useState({
    title: 'Weekly operational pack',
    cadence: 'weekly',
    timezone: 'America/Chicago',
    cronExpression: '',
    deliveryMethod: 'email',
    recipients: 'person-ops-lead,person-audit-lead',
    parameters: 'date_range:last_30_days',
  })
  const [runForm, setRunForm] = useState({
    exportFormat: 'pdf',
    parametersUsed: 'period=last_30_days',
    filtersUsed: 'date_range:last_30_days',
  })
  const [exportForm, setExportForm] = useState({
    exportType: 'report',
    sourceRef: '',
  })
  const selectedReportPolicy = reportPoliciesQuery.data?.find((policy) => policy.reportDefinitionId === selectedReportId) ?? null
  const selectedDashboardForExport = dashboardsQuery.data?.find((dashboard) => dashboard.dashboardId === exportForm.sourceRef) ?? null
  const selectedDashboardExportPolicy = selectedDashboardForExport
    ? dashboardPoliciesQuery.data?.find((policy) => policy.dashboardId === selectedDashboardForExport.dashboardId) ?? null
    : null

  const createReportMutation = useMutation({
    mutationFn: () =>
      createReportDefinition(accessToken, {
        ...reportForm,
        accessPolicyRef: reportForm.accessPolicyRef || undefined,
        datasetRefs: parseCsvList(reportForm.datasetRefs),
        readModelRefs: parseCsvList(reportForm.readModelRefs),
        parameterRefs: parseCsvList(reportForm.parameterRefs),
        defaultFilters: parseCsvList(reportForm.defaultFilters),
        sectionRefs: parseCsvList(reportForm.sectionRefs),
      }),
    onSuccess: async (report) => {
      setSelectedReportId(report.reportDefinitionId)
      await queryClient.invalidateQueries({ queryKey: ['reportarr'] })
    },
  })
  const updateReportMutation = useMutation({
    mutationFn: (status: string) => {
      const selected = reportsQuery.data?.find((report) => report.reportDefinitionId === selectedReportId)
      if (!selected) {
        throw new Error('Selected report not found.')
      }
      return updateReportDefinition(accessToken, selectedReportId, {
        status,
        requestedByPersonId: 'person-ops-analyst',
      })
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['reportarr'] })
    },
  })
  const runMutation = useMutation({
    mutationFn: () =>
      createReportRun(accessToken, {
        reportDefinitionId: selectedReportId,
        requestedByPersonId: 'person-ops-analyst',
        exportFormat: runForm.exportFormat || null,
        parametersUsed: runForm.parametersUsed.split(',').map((item) => item.trim()).filter(Boolean),
        filtersUsed: runForm.filtersUsed.split(',').map((item) => item.trim()).filter(Boolean),
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['reportarr'] })
    },
  })
  const scheduleMutation = useMutation({
    mutationFn: () =>
      createReportSchedule(accessToken, {
        reportDefinitionId: selectedReportId,
        title: scheduleForm.title,
        cadence: scheduleForm.cadence,
        timezone: scheduleForm.timezone,
        cronExpression: scheduleForm.cronExpression || null,
        deliveryMethod: scheduleForm.deliveryMethod,
        recipients: scheduleForm.recipients.split(',').map((item) => item.trim()).filter(Boolean),
        parameters: scheduleForm.parameters.split(',').map((item) => item.trim()).filter(Boolean),
        requestedByPersonId: 'person-ops-analyst',
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['reportarr'] })
    },
  })
  const updateScheduleMutation = useMutation({
    mutationFn: ({ scheduleId, status }: { scheduleId: string; status: string }) => {
      const selected = schedulesQuery.data?.find((schedule) => schedule.scheduleId === scheduleId)
      if (!selected) {
        throw new Error('Selected schedule not found.')
      }
      return updateReportSchedule(accessToken, scheduleId, {
        status,
        cadence: selected.cadence,
        nextRunAt: selected.nextRunAt,
        requestedByPersonId: 'person-ops-analyst',
      })
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['reportarr'] })
    },
  })
  const cancelMutation = useMutation({
    mutationFn: () => cancelReportRun(accessToken, runsQuery.data?.[0]?.reportRunId ?? '', { requestedByPersonId: 'person-ops-analyst', reason: 'Manual cancellation from UI.' }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['reportarr'] })
    },
  })
  const exportMutation = useMutation({
    mutationFn: () =>
      createExport(accessToken, {
        reportRunId: exportForm.exportType === 'report' ? runsQuery.data?.[0]?.reportRunId ?? null : null,
        exportType: exportForm.exportType,
        sourceRef: exportForm.sourceRef || null,
        exportFormat: runForm.exportFormat,
        requestedByPersonId: 'person-ops-analyst',
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['reportarr'] })
    },
  })

  useEffect(() => {
    if (!runsQuery.data?.length) {
      return
    }
    if (!runsQuery.data.some((run) => run.reportRunId === selectedReportRunId)) {
      setSelectedReportRunId(runsQuery.data[0].reportRunId)
    }
  }, [runsQuery.data, selectedReportRunId])

  useEffect(() => {
    const sections = reportSectionsQuery.data?.filter((section) => section.reportDefinitionId === selectedReportId) ?? []
    if (!sections.length) {
      setSelectedReportSectionId('')
      return
    }
    if (!sections.some((section) => section.sectionId === selectedReportSectionId)) {
      setSelectedReportSectionId(sections[0].sectionId)
    }
  }, [reportSectionsQuery.data, selectedReportId, selectedReportSectionId])

  useEffect(() => {
    const schedules = schedulesQuery.data?.filter((schedule) => schedule.reportDefinitionId === selectedReportId) ?? []
    if (!schedules.length) {
      setSelectedScheduleId('')
      return
    }
    if (!schedules.some((schedule) => schedule.scheduleId === selectedScheduleId)) {
      setSelectedScheduleId(schedules[0].scheduleId)
    }
  }, [schedulesQuery.data, selectedReportId, selectedScheduleId])

  useEffect(() => {
    const visibleRecipients = (recipientsQuery.data ?? []).filter((recipient) => recipient.scheduleId === selectedScheduleId)
    if (!visibleRecipients.length) {
      setSelectedReportRecipientId('')
      return
    }
    if (!visibleRecipients.some((recipient) => recipient.recipientId === selectedReportRecipientId)) {
      setSelectedReportRecipientId(visibleRecipients[0].recipientId)
    }
  }, [recipientsQuery.data, selectedScheduleId, selectedReportRecipientId])

  useEffect(() => {
    const parameters = reportParametersQuery.data?.filter((parameter) => parameter.reportDefinitionId === selectedReportId) ?? []
    if (!parameters.length) {
      setSelectedReportParameterId('')
      return
    }
    if (!parameters.some((parameter) => parameter.parameterId === selectedReportParameterId)) {
      setSelectedReportParameterId(parameters[0].parameterId)
    }
  }, [reportParametersQuery.data, selectedReportId, selectedReportParameterId])

  useEffect(() => {
    const exportsList = exportsQuery.data ?? []
    if (!exportsList.length) {
      setSelectedExportJobId('')
      return
    }
    if (!exportsList.some((job) => job.exportJobId === selectedExportJobId)) {
      setSelectedExportJobId(exportsList[0].exportJobId)
    }
  }, [exportsQuery.data, selectedExportJobId])
  const canBuildReports = canUseReportArrAction(roleKey, isPlatformAdmin, ['report_builder', 'reportarr_builder', 'tenant_admin', 'reportarr_admin'])
  const canScheduleReports = canUseReportArrAction(roleKey, isPlatformAdmin, ['report_scheduler', 'reportarr_scheduler', 'report_builder', 'tenant_admin', 'reportarr_admin'])
  const canRunReports = canUseReportArrAction(roleKey, isPlatformAdmin, ['report_runner', 'reportarr_runner', 'report_builder', 'tenant_admin', 'reportarr_admin'])
  const scheduleDeliveryOptions = selectedReportPolicy?.externalDeliveryAllowed
    ? ['email', 'recordarr_package', 'dashboard_notification', 'webhook', 'download_only']
    : ['email', 'recordarr_package', 'dashboard_notification', 'download_only']
  const canUpdateSelectedReport = selectedReportPolicy?.allowedPermissionRefs.some((permission) => permission === 'reportarr.reports.update') ?? false

  useEffect(() => {
    if (selectedReportPolicy?.externalDeliveryAllowed) {
      return
    }
    if (scheduleForm.deliveryMethod === 'webhook') {
      setScheduleForm((current) => ({
        ...current,
        deliveryMethod: 'email',
      }))
    }
  }, [scheduleForm.deliveryMethod, selectedReportPolicy?.externalDeliveryAllowed])

  return (
    <div className="reportarr-page">
      <SectionHeader
        eyebrow="Reports"
        title="Report definitions, schedules, and history"
        description="Configure evidence-backed report sections, create scheduled delivery, and inspect the generated output history."
        action={<Pill><FileText className="h-4 w-4" /> {reportsQuery.data?.length ?? 0} reports</Pill>}
      />

      {canBuildReports ? (
        <Panel title="Create report definition" icon={<Plus className="h-4 w-4 text-cyan-300" />}>
        <div className="grid gap-3 md:grid-cols-2">
          <TextInput value={reportForm.reportKey} onChange={(value) => setReportForm({ ...reportForm, reportKey: value })} placeholder="report-key" />
          <TextInput value={reportForm.title} onChange={(value) => setReportForm({ ...reportForm, title: value })} placeholder="Title" />
          <TextInput value={reportForm.reportType} onChange={(value) => setReportForm({ ...reportForm, reportType: value })} placeholder="Type" />
          <TextInput value={reportForm.layoutDefinition} onChange={(value) => setReportForm({ ...reportForm, layoutDefinition: value })} placeholder="Layout definition" />
          <OwnerPersonPicker value={reportForm.ownerPersonId} onChange={(ownerPersonId) => setReportForm({ ...reportForm, ownerPersonId })} />
          <div className="md:col-span-2">
            <div className="mb-2 text-sm text-slate-300">Access policy</div>
            <select
              className="reportarr-input"
              value={reportForm.accessPolicyRef || 'default'}
              onChange={(event) => {
                const nextValue = event.target.value === 'default' ? '' : event.target.value
                setReportForm({ ...reportForm, accessPolicyRef: nextValue })
              }}
            >
              <option value="default">Use default policy</option>
              {reportPoliciesQuery.data?.map((policy) => (
                <option key={policy.accessPolicyId} value={policy.accessPolicyId}>
                  {policy.accessPolicyId}
                </option>
              ))}
            </select>
          </div>
          <div className="md:col-span-2">
            <TextArea value={reportForm.description} onChange={(value) => setReportForm({ ...reportForm, description: value })} placeholder="Description" />
          </div>
          <div className="md:col-span-2">
            <TextArea
              value={reportForm.datasetRefs}
              onChange={(value) => setReportForm({ ...reportForm, datasetRefs: value })}
              placeholder="Dataset references (comma-separated IDs)"
            />
          </div>
          <div className="md:col-span-2">
            <TextArea
              value={reportForm.readModelRefs}
              onChange={(value) => setReportForm({ ...reportForm, readModelRefs: value })}
              placeholder="Read model references (comma-separated IDs)"
            />
          </div>
          <div className="md:col-span-2">
            <TextArea
              value={reportForm.parameterRefs}
              onChange={(value) => setReportForm({ ...reportForm, parameterRefs: value })}
              placeholder="Parameter references (comma-separated IDs)"
            />
          </div>
          <div className="md:col-span-2">
            <TextArea
              value={reportForm.defaultFilters}
              onChange={(value) => setReportForm({ ...reportForm, defaultFilters: value })}
              placeholder="Default filters (comma-separated IDs)"
            />
          </div>
          <div className="md:col-span-2">
            <TextArea
              value={reportForm.sectionRefs}
              onChange={(value) => setReportForm({ ...reportForm, sectionRefs: value })}
              placeholder="Section references (comma-separated IDs)"
            />
          </div>
          <div className="md:col-span-2">
            <div className="mb-2 text-sm text-slate-300">Export formats</div>
            <div className="flex flex-wrap gap-2">
              {reportExportFormatOptions.map((format) => {
                const active = reportForm.exportFormats.includes(format)
                return (
                  <button
                    key={format}
                    type="button"
                    className={[
                      'reportarr-button secondary',
                      active ? 'ring-2 ring-cyan-400 bg-cyan-400/10 text-cyan-100' : '',
                    ].join(' ')}
                    onClick={() =>
                      setReportForm((current) => ({
                        ...current,
                        exportFormats: current.exportFormats.includes(format)
                          ? current.exportFormats.filter((item) => item !== format)
                          : [...current.exportFormats, format],
                      }))
                    }
                  >
                    {format}
                  </button>
                )
              })}
            </div>
          </div>
        </div>
        <button className="reportarr-button mt-4" type="button" onClick={() => createReportMutation.mutate()} disabled={createReportMutation.isPending}>
          {createReportMutation.isPending ? 'Creating…' : 'Create report'}
        </button>
        <div className="mt-3 flex flex-wrap gap-3">
          <button className="reportarr-button secondary" type="button" onClick={() => updateReportMutation.mutate('active')} disabled={updateReportMutation.isPending || !canUpdateSelectedReport}>
            {updateReportMutation.isPending ? 'Activating…' : 'Activate selected'}
          </button>
          <button className="reportarr-button secondary" type="button" onClick={() => updateReportMutation.mutate('paused')} disabled={updateReportMutation.isPending || !canUpdateSelectedReport}>
            {updateReportMutation.isPending ? 'Pausing…' : 'Pause selected'}
          </button>
          <button className="reportarr-button secondary" type="button" onClick={() => updateReportMutation.mutate('archived')} disabled={updateReportMutation.isPending || !canUpdateSelectedReport}>
            {updateReportMutation.isPending ? 'Archiving…' : 'Archive selected'}
          </button>
        </div>
        </Panel>
      ) : null}

      {(canScheduleReports || canRunReports) ? (
        <Panel title="Schedule delivery and exports" icon={<Workflow className="h-4 w-4 text-cyan-300" />}>
        <div className="grid gap-3 md:grid-cols-2">
          <TextInput value={scheduleForm.title} onChange={(value) => setScheduleForm({ ...scheduleForm, title: value })} placeholder="Schedule title" />
          <TextInput value={scheduleForm.recipients} onChange={(value) => setScheduleForm({ ...scheduleForm, recipients: value })} placeholder="Recipients comma separated" />
          <TextInput value={scheduleForm.timezone} onChange={(value) => setScheduleForm({ ...scheduleForm, timezone: value })} placeholder="Timezone" />
          <TextInput value={scheduleForm.cronExpression} onChange={(value) => setScheduleForm({ ...scheduleForm, cronExpression: value })} placeholder="Cron expression (optional)" />
          <TextInput value={scheduleForm.parameters} onChange={(value) => setScheduleForm({ ...scheduleForm, parameters: value })} placeholder="Parameters comma separated" />
          <Select value={scheduleForm.cadence} onChange={(value) => setScheduleForm({ ...scheduleForm, cadence: value })} options={['hourly', 'daily', 'weekly', 'monthly', 'quarterly', 'annually', 'custom_cron']} />
          <Select value={scheduleForm.deliveryMethod} onChange={(value) => setScheduleForm({ ...scheduleForm, deliveryMethod: value })} options={scheduleDeliveryOptions} />
          <Select value={runForm.exportFormat} onChange={(value) => setRunForm({ ...runForm, exportFormat: value })} options={['pdf', 'xlsx', 'csv', 'html', 'json', 'png', 'zip']} />
          <TextInput value={runForm.parametersUsed} onChange={(value) => setRunForm({ ...runForm, parametersUsed: value })} placeholder="Run parameters comma separated" />
          <TextInput value={runForm.filtersUsed} onChange={(value) => setRunForm({ ...runForm, filtersUsed: value })} placeholder="Run filters comma separated" />
        </div>
        <div className="mt-4 flex flex-wrap gap-3">
          <button
            className="reportarr-button"
            type="button"
            onClick={() => scheduleMutation.mutate()}
            disabled={scheduleMutation.isPending || !selectedReportPolicy?.scheduleAllowed}
          >
            {scheduleMutation.isPending ? 'Saving…' : 'Create schedule'}
          </button>
          <button className="reportarr-button secondary" type="button" onClick={() => runMutation.mutate()} disabled={runMutation.isPending || !selectedReportId}>
            {runMutation.isPending ? 'Running…' : 'Run selected report'}
          </button>
          <button
            className="reportarr-button secondary"
            type="button"
            onClick={() => exportMutation.mutate()}
            disabled={
              exportMutation.isPending
              || !selectedReportPolicy?.exportAllowed
              || (exportForm.exportType === 'report' && !runsQuery.data?.length)
              || (exportForm.exportType !== 'report' && !exportForm.sourceRef)
              || (exportForm.exportType === 'dashboard' && selectedDashboardExportPolicy?.exportAllowed === false)
            }
          >
            {exportMutation.isPending ? 'Exporting…' : 'Create export'}
          </button>
          {canRunReports ? (
            <button className="reportarr-button secondary" type="button" onClick={() => cancelMutation.mutate()} disabled={cancelMutation.isPending || !runsQuery.data?.length}>
              {cancelMutation.isPending ? 'Cancelling…' : 'Cancel latest run'}
            </button>
          ) : null}
        </div>
        <div className="mt-4 grid gap-3 md:grid-cols-2">
          <Select value={exportForm.exportType} onChange={(value) => setExportForm({ ...exportForm, exportType: value })} options={exportTypeOptions} />
          <TextInput value={exportForm.sourceRef} onChange={(value) => setExportForm({ ...exportForm, sourceRef: value })} placeholder="Source ref (optional)" />
        </div>
        <div className="mt-3 text-sm text-slate-300">
          Export type defaults to <code>report</code> and can point to a dashboard, dataset, chart, audit package, or custom source ref instead.
        </div>
        {selectedReportPolicy ? (
          <div className="mt-3 text-xs text-slate-400">
            Schedule {selectedReportPolicy.scheduleAllowed ? 'is' : 'is not'} allowed for the selected report. Export {selectedReportPolicy.exportAllowed ? 'is' : 'is not'} allowed. External delivery {selectedReportPolicy.externalDeliveryAllowed ? 'is' : 'is not'} allowed.
            {!selectedReportPolicy.externalDeliveryAllowed ? ' Webhook delivery is hidden for this report.' : ''}
            {canUpdateSelectedReport ? '' : ' Report status changes are disabled for this report.'}
          </div>
        ) : null}
        {exportForm.exportType === 'dashboard' && selectedDashboardForExport ? (
          <div className="mt-2 text-xs text-slate-400">
            Dashboard export policy for {selectedDashboardForExport.dashboardNumber}: {selectedDashboardExportPolicy?.exportAllowed ? 'allowed' : 'blocked'}.
          </div>
        ) : null}
        </Panel>
      ) : null}

      <div className="reportarr-grid cols-2">
        <Panel title="Report definitions" icon={<FileText className="h-4 w-4 text-cyan-300" />}>
          <div className="reportarr-stack">
            {(reportsQuery.data ?? []).map((report) => (
              <button
                key={report.reportDefinitionId}
                type="button"
                className={['reportarr-row reportarr-row-button', report.reportDefinitionId === selectedReportId ? 'active' : ''].join(' ')}
                onClick={() => setSelectedReportId(report.reportDefinitionId)}
              >
                <div className="reportarr-row-main">
                  <strong>{report.reportNumber}</strong>
                  <span>{report.title}</span>
                  <small>{report.description}</small>
                  <small>{report.reportType} · {report.ownerPersonId}</small>
                  <small>datasets {report.datasetRefs.join(', ') || 'none'}</small>
                  <small>read models {report.readModelRefs.join(', ') || 'none'}</small>
                  <small>parameters {report.parameterRefs.join(', ') || 'none'}</small>
                  <small>defaults {report.defaultFilters.join(', ') || 'none'}</small>
                  <small>{summarizeConfiguredField(report.layoutDefinition, 'layout')}</small>
                  <small>sections {report.sectionRefs.join(', ') || 'none'}</small>
                  <small>policy {report.accessPolicyRef}</small>
                  <small>created {formatDate(report.createdAt)} by {report.createdByPersonId}</small>
                  <small>updated {formatDate(report.updatedAt)} by {report.updatedByPersonId}</small>
                </div>
                <div className="reportarr-row-meta">
                  <Pill>{report.status}</Pill>
                  <Pill>{report.exportFormats.join(', ')}</Pill>
                </div>
              </button>
            ))}
            {!reportsQuery.data?.length ? <EmptyState title="No reports yet." /> : null}
          </div>
        </Panel>
        <Panel title="Report preview" icon={<PlayCircle className="h-4 w-4 text-cyan-300" />}>
          {reportsQuery.data?.find((report) => report.reportDefinitionId === selectedReportId) ? (
            (() => {
              const report = reportsQuery.data!.find((item) => item.reportDefinitionId === selectedReportId)!
              const sections = reportSectionsQuery.data?.filter((section) => section.reportDefinitionId === report.reportDefinitionId) ?? []
              const parameters = reportParametersQuery.data?.filter((parameter) => parameter.reportDefinitionId === report.reportDefinitionId) ?? []
              const policy = reportPoliciesQuery.data?.find((item) => item.reportDefinitionId === report.reportDefinitionId)
              return (
                <div className="space-y-2 text-sm text-slate-300">
                  <p><strong className="text-slate-100">Type:</strong> {report.reportType}</p>
                  <p><strong className="text-slate-100">Layout:</strong> {summarizeConfiguredField(report.layoutDefinition, 'layout')}</p>
                  <p><strong className="text-slate-100">Sections:</strong> {sections.map((section) => `${section.sequence}:${section.sectionType}`).join(', ') || 'none'}</p>
                  <p><strong className="text-slate-100">Parameters:</strong> {parameters.map((parameter) => parameter.parameterKey).join(', ') || 'none'}</p>
                  <p><strong className="text-slate-100">Default filters:</strong> {report.defaultFilters.join(', ') || 'none'}</p>
                  <p><strong className="text-slate-100">Export formats:</strong> {report.exportFormats.join(', ')}</p>
                  <p><strong className="text-slate-100">Access policy:</strong> {policy?.visibility ?? report.accessPolicyRef}</p>
                  <p><strong className="text-slate-100">Preview:</strong> This report will render {sections.length || 0} section{sections.length === 1 ? '' : 's'} with {report.datasetRefs.length} dataset reference{report.datasetRefs.length === 1 ? '' : 's'}.</p>
                </div>
              )
            })()
          ) : (
            <EmptyState title="Select a report to preview." />
          )}
        </Panel>
        <Panel title="Report access policies" icon={<ShieldCheck className="h-4 w-4 text-cyan-300" />}>
          <div className="reportarr-stack">
            {(reportPoliciesQuery.data ?? []).map((policy) => (
              <div key={policy.accessPolicyId} className="reportarr-row">
                <div className="reportarr-row-main">
                  <strong>{policy.visibility}</strong>
                  <span>{policy.reportDefinitionId}</span>
                  <small>{policy.allowedPermissionRefs.join(', ') || 'No permission restrictions'}</small>
                </div>
                <div className="reportarr-row-meta">
                  <Pill>{policy.exportAllowed ? 'export on' : 'export off'}</Pill>
                </div>
              </div>
            ))}
            {!reportPoliciesQuery.data?.length ? <EmptyState title="No report access policies." /> : null}
          </div>
        </Panel>
        <Panel title="Latest runs" icon={<PlayCircle className="h-4 w-4 text-cyan-300" />}>
          <div className="grid gap-4 lg:grid-cols-[1.2fr_0.8fr]">
            <ReportRunsList
              reportRuns={runsQuery.data ?? []}
              selectedReportRunId={selectedReportRunId}
              onSelectReportRun={setSelectedReportRunId}
            />
            <ReportRunDetail
              reportRun={runsQuery.data?.find((run) => run.reportRunId === selectedReportRunId) ?? null}
            />
          </div>
        </Panel>
        <Panel title="Schedules" icon={<Workflow className="h-4 w-4 text-cyan-300" />}>
          <ReportSchedulesList
            schedules={schedulesQuery.data ?? []}
            reportDefinitionId={selectedReportId}
            selectedScheduleId={selectedScheduleId}
            onSelectSchedule={setSelectedScheduleId}
            onStatusChange={(scheduleId, status) => updateScheduleMutation.mutate({ scheduleId, status })}
            isUpdating={updateScheduleMutation.isPending}
          />
        </Panel>
        <Panel title="Selected schedule" icon={<Workflow className="h-4 w-4 text-cyan-300" />}>
          <ReportScheduleDetail
            schedule={schedulesQuery.data?.find((schedule) => schedule.scheduleId === selectedScheduleId) ?? null}
            recipients={(recipientsQuery.data ?? []).filter((recipient) => recipient.scheduleId === selectedScheduleId)}
          />
        </Panel>
        <Panel title="Recipients" icon={<Bell className="h-4 w-4 text-cyan-300" />}>
          <ReportRecipientsList
            recipients={recipientsQuery.data ?? []}
            scheduleIds={(schedulesQuery.data ?? []).filter((schedule) => schedule.reportDefinitionId === selectedReportId).map((schedule) => schedule.scheduleId)}
            selectedRecipientId={selectedReportRecipientId}
            onSelectRecipient={setSelectedReportRecipientId}
          />
        </Panel>
        <Panel title="Selected recipient" icon={<Bell className="h-4 w-4 text-cyan-300" />}>
          {recipientsQuery.data?.find((recipient) => recipient.recipientId === selectedReportRecipientId) ? (
            (() => {
              const recipient = recipientsQuery.data!.find((item) => item.recipientId === selectedReportRecipientId)!
              return (
                <div className="space-y-2 text-sm text-slate-300">
                  <p><strong className="text-slate-100">Schedule:</strong> {recipient.scheduleId}</p>
                  <p><strong className="text-slate-100">Type:</strong> {recipient.recipientType}</p>
                  <p><strong className="text-slate-100">Recipient ref:</strong> {recipient.recipientRef}</p>
                  <p><strong className="text-slate-100">Email:</strong> {recipient.email ?? 'n/a'}</p>
                  <p><strong className="text-slate-100">Delivery format:</strong> {recipient.deliveryFormat}</p>
                  <p><strong className="text-slate-100">Status:</strong> {recipient.status}</p>
                </div>
              )
            })()
          ) : (
            <EmptyState title="Select a recipient to inspect details." />
          )}
        </Panel>
        <Panel title="Report parameters" icon={<Gauge className="h-4 w-4 text-cyan-300" />}>
          <ReportParametersList
            parameters={reportParametersQuery.data ?? []}
            reportDefinitionId={selectedReportId}
            selectedParameterId={selectedReportParameterId}
            onSelectParameter={setSelectedReportParameterId}
          />
        </Panel>
        <Panel title="Selected parameter" icon={<Gauge className="h-4 w-4 text-cyan-300" />}>
          {reportParametersQuery.data?.find((parameter) => parameter.parameterId === selectedReportParameterId) ? (
            (() => {
              const parameter = reportParametersQuery.data!.find((item) => item.parameterId === selectedReportParameterId)!
              return (
                <div className="space-y-2 text-sm text-slate-300">
                  <p><strong className="text-slate-100">Parameter key:</strong> {parameter.parameterKey}</p>
                  <p><strong className="text-slate-100">Label:</strong> {parameter.label}</p>
                  <p><strong className="text-slate-100">Type:</strong> {parameter.parameterType}</p>
                  <p><strong className="text-slate-100">Required:</strong> {parameter.required ? 'yes' : 'no'}</p>
                  <p><strong className="text-slate-100">Default value:</strong> {parameter.defaultValue || 'n/a'}</p>
                  <p><strong className="text-slate-100">Allowed values source:</strong> {parameter.allowedValuesSource}</p>
                  <p><strong className="text-slate-100">Validation rules:</strong> {summarizeConfiguredField(parameter.validationRules, 'validation rules')}</p>
                </div>
              )
            })()
          ) : (
            <EmptyState title="Select a parameter to inspect details." />
          )}
        </Panel>
        <Panel title="Report sections" icon={<Layers3 className="h-4 w-4 text-cyan-300" />}>
          <ReportSectionsList
            sections={reportSectionsQuery.data ?? []}
            reportDefinitionId={selectedReportId}
            selectedReportSectionId={selectedReportSectionId}
            onSelectSection={setSelectedReportSectionId}
          />
        </Panel>
        <Panel title="Selected section" icon={<Layers3 className="h-4 w-4 text-cyan-300" />}>
          <ReportSectionDetail
            section={reportSectionsQuery.data?.find((section) => section.sectionId === selectedReportSectionId) ?? null}
          />
        </Panel>
        <Panel title="Exports" icon={<History className="h-4 w-4 text-cyan-300" />}>
          <ExportJobsList
            exports={exportsQuery.data ?? []}
            selectedExportJobId={selectedExportJobId}
            onSelectExportJob={setSelectedExportJobId}
          />
        </Panel>
        <Panel title="Selected export job" icon={<History className="h-4 w-4 text-cyan-300" />}>
          <ExportJobDetail exportJob={exportsQuery.data?.find((job) => job.exportJobId === selectedExportJobId) ?? null} />
        </Panel>
      </div>
    </div>
  )
}

function ReportRunsList({
  reportRuns,
  selectedReportRunId,
  onSelectReportRun,
}: {
  reportRuns: ReportArrReportRunResponse[]
  selectedReportRunId?: string
  onSelectReportRun?: (reportRunId: string) => void
}) {
  const navigate = useNavigate()
  if (!reportRuns.length) return <EmptyState title="No report runs yet." />
  return (
    <div className="reportarr-stack">
      {reportRuns.map((run) => (
        <div
          key={run.reportRunId}
          className={['reportarr-row reportarr-row-button', run.reportRunId === selectedReportRunId ? 'active' : ''].join(' ')}
          role="button"
          tabIndex={0}
          onClick={() => onSelectReportRun?.(run.reportRunId)}
          onKeyDown={(event) => {
            if (event.key === 'Enter' || event.key === ' ') {
              onSelectReportRun?.(run.reportRunId)
            }
          }}
        >
          <div className="reportarr-row-main">
            <strong>{run.reportRunNumber}</strong>
            <span>{run.title}</span>
            <small>{run.freshnessSummary}</small>
            <small>{run.sourceTraceSummary}</small>
            <small>{run.outputFormat}</small>
            <small>{run.parametersUsed.join(', ') || 'No parameters used'}</small>
            <small>{run.filtersUsed.join(', ') || 'No filters used'}</small>
            <small>{run.outputRecordRef ?? 'No output record'} · {run.outputPackageRef ?? 'No output package'}</small>
            {run.errorMessage ? <small>{run.errorMessage}</small> : null}
          </div>
          <div className="reportarr-row-meta">
            <Pill>{run.status}</Pill>
            <Pill>{run.warningCount} warnings</Pill>
            <Pill>{run.errorCount} errors</Pill>
            <Pill>{run.rowCount} rows</Pill>
            <button
              type="button"
              className="reportarr-button secondary text-xs"
              onClick={(event) => {
                event.stopPropagation()
                navigate(`/reports/runs/${run.reportRunId}`)
              }}
            >
              Open details
            </button>
          </div>
        </div>
      ))}
    </div>
  )
}

function ReportRunDetail({ reportRun }: { reportRun: ReportArrReportRunResponse | null }) {
  if (!reportRun) return <DetailEmptyState text="Select a report run to inspect details." />
  const tone: DetailTone = reportRun.errorCount > 0 ? 'bad' : reportRun.warningCount > 0 ? 'warn' : 'good'
  return (
    <ReportDetailShell
      backLabel="Reports"
      backTo="/reports"
      breadcrumbs={[reportRun.reportDefinitionId, reportRun.exportJobId ?? 'Run']}
      icon={<PlayCircle className="h-8 w-8" />}
      title={reportRun.reportDefinitionId}
      subtitle="Selected report run details."
      badges={[
        { label: `Rows ${formatNumber(reportRun.rowCount)}`, tone },
        { label: reportRun.outputFormat, tone: 'info' },
      ]}
      metrics={[
        { label: 'Rows', value: formatNumber(reportRun.rowCount), hint: 'Rows produced by the run', icon: <FileText className="h-5 w-5" />, tone },
        { label: 'Warnings', value: reportRun.warningCount, hint: 'Warnings emitted during generation', icon: <AlertTriangle className="h-5 w-5" />, tone: reportRun.warningCount > 0 ? 'warn' : 'good' },
        { label: 'Errors', value: reportRun.errorCount, hint: 'Errors emitted during generation', icon: <AlertTriangle className="h-5 w-5" />, tone: reportRun.errorCount > 0 ? 'bad' : 'good' },
        { label: 'Parameters', value: reportRun.parametersUsed.length, hint: 'Parameters supplied to the run', icon: <Gauge className="h-5 w-5" />, tone: reportRun.parametersUsed.length > 0 ? 'info' : 'neutral' },
      ]}
      snapshotTitle="Report run snapshot"
      snapshotSubtitle="Timing, outputs, and source trace summary."
      snapshotFields={[
        { label: 'Requested by', value: reportRun.requestedByPersonId, source: 'ReportArr run' },
        { label: 'Requested at', value: formatDate(reportRun.requestedAt), source: 'ReportArr run' },
        { label: 'Started at', value: formatDate(reportRun.startedAt), source: 'ReportArr run' },
        { label: 'Completed at', value: formatDate(reportRun.completedAt), source: 'ReportArr run' },
        { label: 'Parameters used', value: reportRun.parametersUsed.join(', ') || 'none', source: 'ReportArr run' },
        { label: 'Filters used', value: reportRun.filtersUsed.join(', ') || 'none', source: 'ReportArr run' },
        { label: 'Output format', value: reportRun.outputFormat, source: 'ReportArr output' },
        { label: 'Output record', value: reportRun.outputRecordRef ?? 'n/a', source: 'ReportArr output' },
        { label: 'Output package', value: reportRun.outputPackageRef ?? 'n/a', source: 'ReportArr output' },
        { label: 'Export job', value: reportRun.exportJobId ?? 'n/a', source: 'ReportArr output' },
      ]}
      decisionTitle="Run outcome"
      decisionBadge={{ label: reportRun.errorCount > 0 ? 'Errors' : reportRun.warningCount > 0 ? 'Warnings' : 'Healthy', tone }}
      decisionIcon={<PlayCircle className="h-5 w-5 text-sky-300" />}
      decisionSummary={reportRun.errorCount > 0 ? 'The report run completed with errors.' : reportRun.warningCount > 0 ? 'The report run completed with warnings.' : 'The report run completed successfully.'}
      decisionDetail={reportRun.sourceTraceSummary}
      allowedChecks={Math.max(0, reportRun.rowCount)}
      blockedChecks={reportRun.errorCount}
      railSections={[]}
      mainContent={
        <section className="rounded-xl border border-slate-800 bg-slate-950/50 p-4">
          <h3 className="text-lg font-semibold text-white">Freshness summary</h3>
          <p className="mt-2 text-sm text-slate-400">{reportRun.freshnessSummary}</p>
          {reportRun.errorMessage ? <p className="mt-3 text-sm text-amber-200">{reportRun.errorMessage}</p> : null}
        </section>
      }
    />
  )
}

function ReportSchedulesList({
  schedules,
  reportDefinitionId,
  selectedScheduleId = '',
  onSelectSchedule = () => {},
  onStatusChange,
  isUpdating,
}: {
  schedules: ReportArrReportScheduleResponse[]
  reportDefinitionId: string
  selectedScheduleId?: string
  onSelectSchedule?: (scheduleId: string) => void
  onStatusChange?: (scheduleId: string, status: string) => void
  isUpdating?: boolean
}) {
  const visibleSchedules = schedules.filter((schedule) => schedule.reportDefinitionId === reportDefinitionId)
  const navigate = useNavigate()
  if (!visibleSchedules.length) return <EmptyState title="No schedules yet." />
  return (
    <div className="reportarr-stack">
      {visibleSchedules.map((schedule) => (
        <div
          key={schedule.scheduleId}
          className={['reportarr-row reportarr-row-button', schedule.scheduleId === selectedScheduleId ? 'active' : ''].join(' ')}
          role="button"
          tabIndex={0}
          onClick={() => onSelectSchedule(schedule.scheduleId)}
          onKeyDown={(event) => {
            if (event.key === 'Enter' || event.key === ' ') {
              onSelectSchedule(schedule.scheduleId)
            }
          }}
        >
          <div className="reportarr-row-main">
            <strong>{schedule.scheduleNumber}</strong>
            <span>{schedule.title}</span>
            <small>{schedule.deliveryMethod} · {schedule.timezone} · next {formatDate(schedule.nextRunAt)}</small>
            <small>last {formatDate(schedule.lastRunAt)} · start {formatDate(schedule.startsAt)} · end {formatDate(schedule.endsAt)}</small>
            <small>{schedule.parameters.join(', ') || 'No parameters'}</small>
            <small>{schedule.recipients.join(', ') || 'No recipients'}</small>
            <small>{schedule.cronExpression ?? 'No cron expression'}</small>
            <small>created by {schedule.createdByPersonId}</small>
          </div>
          <div className="reportarr-row-meta">
            <Pill>{schedule.status}</Pill>
            <Pill>{schedule.cadence}</Pill>
            {onStatusChange ? (
              <div className="flex flex-wrap justify-end gap-2">
                <button
                  type="button"
                  className="reportarr-button secondary text-xs"
                  onClick={(event) => {
                    event.stopPropagation()
                    navigate(`/reports/schedules/${schedule.scheduleId}`)
                  }}
                >
                  Open details
                </button>
                <button
                  type="button"
                  className="reportarr-button secondary"
                  onClick={() => onStatusChange(schedule.scheduleId, 'active')}
                  disabled={isUpdating || schedule.status === 'active'}
                >
                  Activate
                </button>
                <button
                  type="button"
                  className="reportarr-button secondary"
                  onClick={() => onStatusChange(schedule.scheduleId, 'paused')}
                  disabled={isUpdating || schedule.status === 'paused'}
                >
                  Pause
                </button>
                <button
                  type="button"
                  className="reportarr-button secondary"
                  onClick={() => onStatusChange(schedule.scheduleId, 'canceled')}
                  disabled={isUpdating || schedule.status === 'canceled'}
                >
                  Cancel
                </button>
              </div>
            ) : null}
          </div>
        </div>
      ))}
    </div>
  )
}

function ReportScheduleDetail({
  schedule,
  recipients,
}: {
  schedule: ReportArrReportScheduleResponse | null
  recipients: ReportArrReportRecipientResponse[]
}) {
  if (!schedule) return <DetailEmptyState text="Select a schedule to inspect details." />
  const tone: DetailTone = schedule.status === 'active' ? 'good' : schedule.status === 'paused' ? 'warn' : schedule.status === 'canceled' ? 'bad' : 'info'
  return (
    <ReportDetailShell
      backLabel="Reports"
      backTo="/reports"
      breadcrumbs={[schedule.reportDefinitionId, schedule.scheduleNumber]}
      icon={<Workflow className="h-8 w-8" />}
      title={schedule.title}
      subtitle="Selected report schedule details."
      badges={[
        { label: schedule.status, tone },
        { label: schedule.deliveryMethod, tone: 'info' },
      ]}
      metrics={[
        { label: 'Recipients', value: schedule.recipients.length, hint: 'Configured recipients', icon: <Bell className="h-5 w-5" />, tone: schedule.recipients.length > 0 ? 'good' : 'neutral' },
        { label: 'Parameters', value: schedule.parameters.length, hint: 'Configured parameters', icon: <Gauge className="h-5 w-5" />, tone: schedule.parameters.length > 0 ? 'info' : 'neutral' },
        { label: 'Next run', value: formatDate(schedule.nextRunAt), hint: 'When this schedule will run next', icon: <PlayCircle className="h-5 w-5" />, tone: 'info' },
        { label: 'Recipient records', value: recipients.length, hint: 'Resolved recipient records', icon: <Users className="h-5 w-5" />, tone: recipients.length > 0 ? 'good' : 'neutral' },
      ]}
      snapshotTitle="Schedule snapshot"
      snapshotSubtitle="Delivery cadence, timing, and ownership."
      snapshotFields={[
        { label: 'Schedule number', value: schedule.scheduleNumber, source: 'ReportArr schedule' },
        { label: 'Status', value: schedule.status, source: 'ReportArr schedule' },
        { label: 'Cadence', value: schedule.cadence, source: 'ReportArr schedule' },
        { label: 'Timezone', value: schedule.timezone, source: 'ReportArr schedule' },
        { label: 'Delivery method', value: schedule.deliveryMethod, source: 'ReportArr schedule' },
        { label: 'Cron', value: schedule.cronExpression ?? 'none', source: 'ReportArr schedule' },
        { label: 'Next run', value: formatDate(schedule.nextRunAt), source: 'ReportArr schedule' },
        { label: 'Last run', value: formatDate(schedule.lastRunAt), source: 'ReportArr schedule' },
        { label: 'Starts at', value: formatDate(schedule.startsAt), source: 'ReportArr schedule' },
        { label: 'Ends at', value: formatDate(schedule.endsAt), source: 'ReportArr schedule' },
      ]}
      decisionTitle="Schedule decision"
      decisionBadge={{ label: schedule.status, tone }}
      decisionIcon={<Workflow className="h-5 w-5 text-sky-300" />}
      decisionSummary={schedule.status === 'active' ? 'The schedule is active.' : schedule.status === 'paused' ? 'The schedule is paused.' : 'The schedule is not active.'}
      decisionDetail={`ReportArr tracks ${schedule.parameters.length} parameter(s) and ${schedule.recipients.length} recipient(s) for this schedule.`}
      allowedChecks={Math.max(0, schedule.recipients.length + schedule.parameters.length)}
      blockedChecks={schedule.status === 'canceled' ? 1 : 0}
      railSections={[]}
      mainContent={
        <section className="rounded-xl border border-slate-800 bg-slate-950/50 p-4">
          <h3 className="text-lg font-semibold text-white">Recipients</h3>
          <p className="mt-2 text-sm text-slate-400">{schedule.recipients.join(', ') || 'none'}</p>
          <p className="mt-3 text-sm text-slate-400">{recipients.map((recipient) => recipient.recipientRef).join(', ') || 'none'}</p>
        </section>
      }
    />
  )
}

function ReportRecipientsList({
  recipients,
  scheduleIds,
  selectedRecipientId = '',
  onSelectRecipient = () => {},
}: {
  recipients: ReportArrReportRecipientResponse[]
  scheduleIds: string[]
  selectedRecipientId?: string
  onSelectRecipient?: (recipientId: string) => void
}) {
  const visibleRecipients = recipients.filter((recipient) => scheduleIds.includes(recipient.scheduleId))
  if (!visibleRecipients.length) return <EmptyState title="No recipients yet." />
  return (
    <div className="reportarr-stack">
      {visibleRecipients.map((recipient) => (
        <button
          key={recipient.recipientId}
          type="button"
          className={['reportarr-row reportarr-row-button', recipient.recipientId === selectedRecipientId ? 'active' : ''].join(' ')}
          onClick={() => onSelectRecipient(recipient.recipientId)}
        >
          <div className="reportarr-row-main">
            <strong>{recipient.recipientRef}</strong>
            <span>{recipient.recipientType}</span>
            <small>{recipient.deliveryFormat}</small>
            <small>{recipient.email ?? 'No email'}</small>
          </div>
          <div className="reportarr-row-meta">
            <Pill>{recipient.status}</Pill>
          </div>
        </button>
      ))}
    </div>
  )
}

function ReportParametersList({
  parameters,
  reportDefinitionId,
  selectedParameterId = '',
  onSelectParameter = () => {},
}: {
  parameters: ReportArrReportParameterResponse[]
  reportDefinitionId: string
  selectedParameterId?: string
  onSelectParameter?: (parameterId: string) => void
}) {
  const visibleParameters = parameters.filter((parameter) => parameter.reportDefinitionId === reportDefinitionId)
  if (!visibleParameters.length) return <EmptyState title="No report parameters yet." />
  return (
    <div className="reportarr-stack">
      {visibleParameters.map((parameter) => (
        <button
          key={parameter.parameterId}
          type="button"
          className={['reportarr-row reportarr-row-button', parameter.parameterId === selectedParameterId ? 'active' : ''].join(' ')}
          onClick={() => onSelectParameter(parameter.parameterId)}
        >
          <div className="reportarr-row-main">
            <strong>{parameter.parameterKey}</strong>
            <span>{parameter.label}</span>
            <small>{parameter.parameterType}</small>
            <small>{parameter.allowedValuesSource}</small>
            <small>{summarizeConfiguredField(parameter.validationRules, 'validation rules')}</small>
          </div>
          <div className="reportarr-row-meta">
            <Pill>{parameter.required ? 'required' : 'optional'}</Pill>
          </div>
        </button>
      ))}
    </div>
  )
}

function ReportSectionsList({
  sections,
  reportDefinitionId,
  selectedReportSectionId = '',
  onSelectSection = () => {},
}: {
  sections: ReportArrReportSectionResponse[]
  reportDefinitionId: string
  selectedReportSectionId?: string
  onSelectSection?: (sectionId: string) => void
}) {
  const visibleSections = sections.filter((section) => section.reportDefinitionId === reportDefinitionId)
  if (!visibleSections.length) return <EmptyState title="No report sections yet." />
  return (
    <div className="reportarr-stack">
      {visibleSections.map((section) => (
        <button
          key={section.sectionId}
          type="button"
          className={['reportarr-row reportarr-row-button', section.sectionId === selectedReportSectionId ? 'active' : ''].join(' ')}
          onClick={() => onSelectSection(section.sectionId)}
        >
          <div className="reportarr-row-main">
            <strong>{section.sequence}. {section.title}</strong>
            <span>{section.sectionType}</span>
            <small>{section.datasetRef}</small>
            <small>{summarizeConfiguredField(section.queryDefinition, 'query')}</small>
            <small>{summarizeConfiguredField(section.layoutSettings, 'layout')}</small>
          </div>
        </button>
      ))}
    </div>
  )
}

function ReportSectionDetail({ section }: { section: ReportArrReportSectionResponse | null }) {
  if (!section) return <EmptyState title="Select a report section to inspect details." />

  const evidenceBacked =
    section.sectionType === 'evidence_matrix' || section.sectionType === 'source_trace' || section.sectionType === 'appendix'

  return (
    <div className="space-y-2 text-sm text-slate-300">
      <p><strong className="text-slate-100">Section:</strong> {section.sequence}. {section.title}</p>
      <p><strong className="text-slate-100">Type:</strong> {section.sectionType}</p>
      <p><strong className="text-slate-100">Dataset:</strong> {section.datasetRef}</p>
      <p><strong className="text-slate-100">Description:</strong> {section.description}</p>
      <p><strong className="text-slate-100">Query:</strong> {summarizeConfiguredField(section.queryDefinition, 'query')}</p>
      <p><strong className="text-slate-100">Layout:</strong> {summarizeConfiguredField(section.layoutSettings, 'layout')}</p>
      <p><strong className="text-slate-100">Evidence-backed:</strong> {evidenceBacked ? 'yes' : 'no'}</p>
      <p><strong className="text-slate-100">Preview:</strong> This section will render against {section.datasetRef} with the configured layout and source trace summary preserved in the report output.</p>
    </div>
  )
}

function ExportJobsList({
  exports,
  selectedExportJobId = '',
  onSelectExportJob = () => {},
}: {
  exports: ReportArrExportJobResponse[]
  selectedExportJobId?: string
  onSelectExportJob?: (exportJobId: string) => void
}) {
  const navigate = useNavigate()
  if (!exports.length) return <EmptyState title="No exports yet." />
  return (
    <div className="reportarr-stack">
      {exports.map((job) => (
        <div
          key={job.exportJobId}
          className={['reportarr-row reportarr-row-button', job.exportJobId === selectedExportJobId ? 'active' : ''].join(' ')}
          role="button"
          tabIndex={0}
          onClick={() => onSelectExportJob(job.exportJobId)}
          onKeyDown={(event) => {
            if (event.key === 'Enter' || event.key === ' ') {
              onSelectExportJob(job.exportJobId)
            }
          }}
        >
          <div className="reportarr-row-main">
            <strong>{job.exportNumber}</strong>
            <span>{job.title}</span>
            <small>{job.exportType} · {job.exportFormat} · {job.rowCount} rows</small>
            <small>{job.sourceRef ?? 'No source ref'} · output {job.outputRecordRef ?? 'n/a'}</small>
            <small>size {formatNumber(job.fileSizeBytesSnapshot)} bytes · expires {formatDate(job.expiresAt)}</small>
            <small>requested {formatDate(job.requestedAt)}</small>
            <small>{job.recordArrPackageRef ?? 'No RecordArr package'}</small>
          </div>
          <div className="reportarr-row-meta">
            <Pill>{job.status}</Pill>
            <button
              type="button"
              className="reportarr-button secondary text-xs"
              onClick={(event) => {
                event.stopPropagation()
                navigate(`/reports/exports/${job.exportJobId}`)
              }}
            >
              Open details
            </button>
          </div>
        </div>
      ))}
    </div>
  )
}

function ExportJobDetail({ exportJob }: { exportJob: ReportArrExportJobResponse | null }) {
  if (!exportJob) return <DetailEmptyState text="Select an export job to inspect details." />
  const tone: DetailTone = exportJob.status === 'completed' ? 'good' : exportJob.status === 'failed' ? 'bad' : 'warn'
  return (
    <ReportDetailShell
      backLabel="Reports"
      backTo="/reports"
      breadcrumbs={[exportJob.exportNumber, exportJob.title]}
      icon={<FileText className="h-8 w-8" />}
      title={exportJob.title}
      subtitle="Selected export job details."
      badges={[
        { label: exportJob.status, tone },
        { label: exportJob.exportFormat, tone: 'info' },
      ]}
      metrics={[
        { label: 'Rows', value: formatNumber(exportJob.rowCount), hint: 'Rows included in the export', icon: <Gauge className="h-5 w-5" />, tone: exportJob.rowCount > 0 ? 'good' : 'neutral' },
        { label: 'File size', value: formatNumber(exportJob.fileSizeBytesSnapshot), hint: 'Snapshot file size in bytes', icon: <FileText className="h-5 w-5" />, tone: 'info' },
        { label: 'Requested', value: formatDate(exportJob.requestedAt), hint: 'When the export was requested', icon: <PlayCircle className="h-5 w-5" />, tone: 'info' },
        { label: 'Delivered', value: formatDate(exportJob.deliveredAt), hint: 'When the export was delivered', icon: <CheckCircle2 className="h-5 w-5" />, tone: exportJob.deliveredAt ? 'good' : 'neutral' },
      ]}
      snapshotTitle="Export job snapshot"
      snapshotSubtitle="Delivery, output, and lifecycle details."
      snapshotFields={[
        { label: 'Export number', value: exportJob.exportNumber, source: 'ReportArr export' },
        { label: 'Report run', value: exportJob.reportRunId ?? 'n/a', source: 'ReportArr export' },
        { label: 'Export type', value: exportJob.exportType, source: 'ReportArr export' },
        { label: 'Format', value: exportJob.exportFormat, source: 'ReportArr export' },
        { label: 'Requested by', value: exportJob.requestedByPersonId, source: 'ReportArr request' },
        { label: 'Requested at', value: formatDate(exportJob.requestedAt), source: 'ReportArr request' },
        { label: 'Started at', value: formatDate(exportJob.startedAt), source: 'ReportArr request' },
        { label: 'Completed at', value: formatDate(exportJob.completedAt), source: 'ReportArr request' },
        { label: 'Source ref', value: exportJob.sourceRef ?? 'n/a', source: 'ReportArr export' },
        { label: 'Output record', value: exportJob.outputRecordRef ?? 'n/a', source: 'ReportArr export' },
        { label: 'RecordArr package', value: exportJob.recordArrPackageRef ?? 'n/a', source: 'ReportArr export' },
        { label: 'Expires at', value: formatDate(exportJob.expiresAt), source: 'ReportArr export' },
      ]}
      decisionTitle="Export decision"
      decisionBadge={{ label: exportJob.status, tone }}
      decisionIcon={<FileText className="h-5 w-5 text-sky-300" />}
      decisionSummary={exportJob.status === 'completed' ? 'The export completed successfully.' : exportJob.status === 'failed' ? 'The export failed and needs review.' : 'The export is still in progress.'}
      decisionDetail={exportJob.errorMessage || 'No export error message is currently recorded.'}
      allowedChecks={Math.max(0, exportJob.rowCount)}
      blockedChecks={exportJob.status === 'failed' ? 1 : 0}
      railSections={[]}
      mainContent={
        <section className="rounded-xl border border-slate-800 bg-slate-950/50 p-4">
          <h3 className="text-lg font-semibold text-white">Output and lifecycle</h3>
          {exportJob.errorMessage ? <p className="mt-2 text-sm text-amber-200">{exportJob.errorMessage}</p> : null}
        </section>
      }
    />
  )
}

function KpisPage({ accessToken }: { accessToken: string }) {
  const queryClient = useQueryClient()
  const kpisQuery = useQuery({
    queryKey: ['reportarr', 'kpis'],
    queryFn: () => listKpis(accessToken),
    enabled: Boolean(accessToken),
  })
  const metricsQuery = useQuery({
    queryKey: ['reportarr', 'metrics'],
    queryFn: () => listMetrics(accessToken),
    enabled: Boolean(accessToken),
  })
  const kpiValuesQuery = useQuery({
    queryKey: ['reportarr', 'kpi-values'],
    queryFn: () => listKpiValues(accessToken),
    enabled: Boolean(accessToken),
  })
  const metricValuesQuery = useQuery({
    queryKey: ['reportarr', 'metric-values'],
    queryFn: () => listMetricValues(accessToken),
    enabled: Boolean(accessToken),
  })
  const snapshotsQuery = useQuery({
    queryKey: ['reportarr', 'analytics-snapshots'],
    queryFn: () => listAnalyticsSnapshots(accessToken),
    enabled: Boolean(accessToken),
  })
  const trendsQuery = useQuery({
    queryKey: ['reportarr', 'trend-analyses'],
    queryFn: () => listTrendAnalyses(accessToken),
    enabled: Boolean(accessToken),
  })
  const exceptionQueriesQuery = useQuery<ReportArrExceptionQueryResponse[]>({
    queryKey: ['reportarr', 'exception-queries'],
    queryFn: () => listExceptionQueries(accessToken),
    enabled: Boolean(accessToken),
  })
  const exceptionResultsQuery = useQuery<ReportArrExceptionResultResponse[]>({
    queryKey: ['reportarr', 'exception-results'],
    queryFn: () => listExceptionResults(accessToken),
    enabled: Boolean(accessToken),
  })
  const [selectedKpiId, setSelectedKpiId] = useState('kpi-001')
  const [selectedKpiValueId, setSelectedKpiValueId] = useState('')
  const [selectedMetricId, setSelectedMetricId] = useState('')
  const [selectedMetricValueId, setSelectedMetricValueId] = useState('')
  const [selectedSnapshotId, setSelectedSnapshotId] = useState('')
  const [selectedTrendId, setSelectedTrendId] = useState('')
  const [selectedExceptionQueryId, setSelectedExceptionQueryId] = useState('')
  const [selectedExceptionResultId, setSelectedExceptionResultId] = useState('')
  const [form, setForm] = useState({
    periodStart: new Date(Date.now() - 7 * 24 * 60 * 60 * 1000).toISOString(),
    periodEnd: new Date().toISOString(),
  })

  useEffect(() => {
    const values = kpiValuesQuery.data ?? []
    if (!values.length) return
    if (!values.some((value) => value.kpiValueId === selectedKpiValueId)) {
      setSelectedKpiValueId(values[0].kpiValueId)
    }
  }, [kpiValuesQuery.data, selectedKpiValueId])

  useEffect(() => {
    const values = metricValuesQuery.data ?? []
    if (!values.length) return
    if (!values.some((value) => value.metricValueId === selectedMetricValueId)) {
      setSelectedMetricValueId(values[0].metricValueId)
    }
  }, [metricValuesQuery.data, selectedMetricValueId])

  useEffect(() => {
    const metrics = metricsQuery.data ?? []
    if (!metrics.length) return
    if (!metrics.some((metric) => metric.metricId === selectedMetricId)) {
      setSelectedMetricId(metrics[0].metricId)
    }
  }, [metricsQuery.data, selectedMetricId])

  useEffect(() => {
    const snapshots = snapshotsQuery.data ?? []
    if (!snapshots.length) return
    if (!snapshots.some((snapshot) => snapshot.analyticsSnapshotId === selectedSnapshotId)) {
      setSelectedSnapshotId(snapshots[0].analyticsSnapshotId)
    }
  }, [selectedSnapshotId, snapshotsQuery.data])

  useEffect(() => {
    const trends = trendsQuery.data ?? []
    if (!trends.length) return
    if (!trends.some((trend) => trend.trendAnalysisId === selectedTrendId)) {
      setSelectedTrendId(trends[0].trendAnalysisId)
    }
  }, [selectedTrendId, trendsQuery.data])

  useEffect(() => {
    const queries = exceptionQueriesQuery.data ?? []
    if (!queries.length) return
    if (!queries.some((query) => query.exceptionQueryId === selectedExceptionQueryId)) {
      setSelectedExceptionQueryId(queries[0].exceptionQueryId)
    }
  }, [exceptionQueriesQuery.data, selectedExceptionQueryId])

  const selectedExceptionQuery = exceptionQueriesQuery.data?.find((query) => query.exceptionQueryId === selectedExceptionQueryId) ?? null
  const selectedExceptionResults = (exceptionResultsQuery.data ?? []).filter((result) => result.exceptionQueryId === selectedExceptionQueryId)

  useEffect(() => {
    const results = selectedExceptionResults
    if (!results.length) return
    if (!results.some((result) => result.exceptionResultId === selectedExceptionResultId)) {
      setSelectedExceptionResultId(results[0].exceptionResultId)
    }
  }, [selectedExceptionResults, selectedExceptionResultId])

  const calcMutation = useMutation({
    mutationFn: () =>
      calculateKpi(accessToken, selectedKpiId, {
        periodStart: form.periodStart,
        periodEnd: form.periodEnd,
        requestedByPersonId: 'person-analytics-lead',
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['reportarr'] })
    },
  })

  return (
    <div className="reportarr-page">
      <SectionHeader
        eyebrow="KPIs"
        title="Metric and KPI intelligence"
        description="Measure operational and compliance targets, then recalculate the latest KPI value against the selected time window."
        action={<Pill><Gauge className="h-4 w-4" /> {kpisQuery.data?.length ?? 0} KPIs</Pill>}
      />
      <Panel title="Recalculate KPI" icon={<PlayCircle className="h-4 w-4 text-cyan-300" />}>
        <div className="grid gap-3 md:grid-cols-2">
          <TextInput value={form.periodStart} onChange={(value) => setForm({ ...form, periodStart: value })} placeholder="Period start" />
          <TextInput value={form.periodEnd} onChange={(value) => setForm({ ...form, periodEnd: value })} placeholder="Period end" />
        </div>
        <button className="reportarr-button mt-4" type="button" onClick={() => calcMutation.mutate()} disabled={calcMutation.isPending || !selectedKpiId}>
          {calcMutation.isPending ? 'Calculating…' : 'Calculate KPI'}
        </button>
      </Panel>

      <div className="reportarr-grid cols-2">
        <Panel title="KPI definitions" icon={<Gauge className="h-4 w-4 text-cyan-300" />}>
          <div className="reportarr-stack">
            {(kpisQuery.data ?? []).map((kpi) => (
              <button
                key={kpi.kpiId}
                type="button"
                className={['reportarr-row reportarr-row-button', kpi.kpiId === selectedKpiId ? 'active' : ''].join(' ')}
                onClick={() => setSelectedKpiId(kpi.kpiId)}
              >
                <div className="reportarr-row-main">
                  <strong>{kpi.kpiNumber}</strong>
                  <span>{kpi.title}</span>
                  <small>{kpi.description}</small>
                  <small>{summarizeConfiguredField(kpi.formula, 'formula')}</small>
                  <small>datasets {kpi.sourceDatasetRefs.join(', ') || 'none'}</small>
                  <small>metrics {kpi.sourceMetricRefs.join(', ') || 'none'}</small>
                  <small>target {kpi.targetValue ?? 'n/a'} · warn {kpi.warningThreshold ?? 'n/a'} · crit {kpi.criticalThreshold ?? 'n/a'}</small>
                  <small>owner {kpi.ownerPersonId} · higher {kpi.higherIsBetter ? 'yes' : 'no'}</small>
                </div>
                <div className="reportarr-row-meta">
                  <Pill>{kpi.status}</Pill>
                  <Pill>{kpi.displayFormat}</Pill>
                </div>
              </button>
            ))}
            {!kpisQuery.data?.length ? <EmptyState title="No KPIs yet." /> : null}
          </div>
        </Panel>
        <Panel title="Selected KPI" icon={<Gauge className="h-4 w-4 text-cyan-300" />}>
          {kpisQuery.data?.find((kpi) => kpi.kpiId === selectedKpiId) ? (
            (() => {
              const kpi = kpisQuery.data!.find((item) => item.kpiId === selectedKpiId)!
              return (
                <div className="space-y-2 text-sm text-slate-300">
                  <p><strong className="text-slate-100">KPI number:</strong> {kpi.kpiNumber}</p>
                  <p><strong className="text-slate-100">Key:</strong> {kpi.kpiKey}</p>
                  <p><strong className="text-slate-100">Title:</strong> {kpi.title}</p>
                  <p><strong className="text-slate-100">Description:</strong> {kpi.description}</p>
                  <p><strong className="text-slate-100">Category:</strong> {kpi.category}</p>
                  <p><strong className="text-slate-100">Status:</strong> {kpi.status}</p>
                  <p><strong className="text-slate-100">Formula:</strong> {summarizeConfiguredField(kpi.formula, 'formula')}</p>
                  <p><strong className="text-slate-100">Source datasets:</strong> {kpi.sourceDatasetRefs.join(', ') || 'none'}</p>
                  <p><strong className="text-slate-100">Source metrics:</strong> {kpi.sourceMetricRefs.join(', ') || 'none'}</p>
                  <p><strong className="text-slate-100">Target:</strong> {kpi.targetValue ?? 'n/a'}</p>
                  <p><strong className="text-slate-100">Warning threshold:</strong> {kpi.warningThreshold ?? 'n/a'}</p>
                  <p><strong className="text-slate-100">Critical threshold:</strong> {kpi.criticalThreshold ?? 'n/a'}</p>
                  <p><strong className="text-slate-100">Higher is better:</strong> {kpi.higherIsBetter ? 'yes' : 'no'}</p>
                  <p><strong className="text-slate-100">Display format:</strong> {kpi.displayFormat}</p>
                  <p><strong className="text-slate-100">Owner:</strong> {kpi.ownerPersonId}</p>
                  <p><strong className="text-slate-100">Created:</strong> {formatDate(kpi.createdAt)}</p>
                  <p><strong className="text-slate-100">Updated:</strong> {formatDate(kpi.updatedAt)}</p>
                </div>
              )
            })()
          ) : (
            <EmptyState title="Select a KPI to inspect details." />
          )}
        </Panel>
        <Panel title="Metrics" icon={<BarChart3 className="h-4 w-4 text-cyan-300" />}>
          <div className="reportarr-stack">
            {(metricsQuery.data ?? []).map((metric) => (
              <button
                key={metric.metricId}
                type="button"
                className={['reportarr-row reportarr-row-button', metric.metricId === selectedMetricId ? 'active' : ''].join(' ')}
                onClick={() => setSelectedMetricId(metric.metricId)}
              >
                <div className="reportarr-row-main">
                  <strong>{metric.metricKey}</strong>
                  <span>{metric.title}</span>
                  <small>{metric.description}</small>
                  <small>{summarizeConfiguredField(metric.formula, 'formula')}</small>
                  <small>dataset {metric.sourceDatasetRef} · fields {metric.fieldRefs.join(', ') || 'none'}</small>
                  <small>{summarizeConfiguredField(metric.filterDefinition, 'filter')}</small>
                  <small>{summarizeConfiguredField(metric.groupingOptions, 'grouping')} · date {metric.dateField}</small>
                </div>
                <div className="reportarr-row-meta">
                  <Pill>{metric.metricType}</Pill>
                  <Pill>{metric.status}</Pill>
                </div>
              </button>
            ))}
            {!metricsQuery.data?.length ? <EmptyState title="No metrics yet." /> : null}
          </div>
        </Panel>
        <Panel title="Selected metric" icon={<BarChart3 className="h-4 w-4 text-cyan-300" />}>
          {metricsQuery.data?.find((metric) => metric.metricId === selectedMetricId) ? (
            (() => {
              const metric = metricsQuery.data!.find((item) => item.metricId === selectedMetricId)!
              return (
                <div className="space-y-2 text-sm text-slate-300">
                  <p><strong className="text-slate-100">Metric key:</strong> {metric.metricKey}</p>
                  <p><strong className="text-slate-100">Title:</strong> {metric.title}</p>
                  <p><strong className="text-slate-100">Description:</strong> {metric.description}</p>
                  <p><strong className="text-slate-100">Type:</strong> {metric.metricType}</p>
                  <p><strong className="text-slate-100">Status:</strong> {metric.status}</p>
                  <p><strong className="text-slate-100">Source dataset:</strong> {metric.sourceDatasetRef}</p>
                  <p><strong className="text-slate-100">Field refs:</strong> {metric.fieldRefs.join(', ') || 'none'}</p>
                  <p><strong className="text-slate-100">Formula:</strong> {summarizeConfiguredField(metric.formula, 'formula')}</p>
                  <p><strong className="text-slate-100">Filter definition:</strong> {summarizeConfiguredField(metric.filterDefinition, 'filter definition')}</p>
                  <p><strong className="text-slate-100">Grouping options:</strong> {summarizeConfiguredField(metric.groupingOptions, 'grouping options')}</p>
                  <p><strong className="text-slate-100">Date field:</strong> {metric.dateField}</p>
                </div>
              )
            })()
          ) : (
            <EmptyState title="Select a metric to inspect details." />
          )}
        </Panel>
        <Panel title="KPI values" icon={<History className="h-4 w-4 text-cyan-300" />}>
          <KpiValuesList
            values={kpiValuesQuery.data ?? []}
            selectedKpiValueId={selectedKpiValueId}
            onSelectKpiValue={setSelectedKpiValueId}
          />
        </Panel>
        <Panel title="Selected KPI value" icon={<History className="h-4 w-4 text-cyan-300" />}>
          {kpiValuesQuery.data?.find((value) => value.kpiValueId === selectedKpiValueId) ? (
            (() => {
              const value = kpiValuesQuery.data!.find((item) => item.kpiValueId === selectedKpiValueId)!
              return (
                <div className="space-y-2 text-sm text-slate-300">
                  <p><strong className="text-slate-100">KPI:</strong> {value.kpiId}</p>
                  <p><strong className="text-slate-100">Value:</strong> {value.value}</p>
                  <p><strong className="text-slate-100">Trend:</strong> {value.trend}</p>
                  <p><strong className="text-slate-100">Status:</strong> {value.status}</p>
                  <p><strong className="text-slate-100">Period:</strong> {formatDate(value.periodStart)} to {formatDate(value.periodEnd)}</p>
                  <p><strong className="text-slate-100">Target snapshot:</strong> {value.targetValueSnapshot ?? 'n/a'}</p>
                  <p><strong className="text-slate-100">Warning snapshot:</strong> {value.warningThresholdSnapshot ?? 'n/a'}</p>
                  <p><strong className="text-slate-100">Critical snapshot:</strong> {value.criticalThresholdSnapshot ?? 'n/a'}</p>
                  <p><strong className="text-slate-100">Source trace:</strong> {value.sourceTraceSummary}</p>
                  <p><strong className="text-slate-100">Calculated:</strong> {formatDate(value.calculatedAt)}</p>
                </div>
              )
            })()
          ) : (
            <EmptyState title="Select a KPI value to inspect details." />
          )}
        </Panel>
        <Panel title="Metric values" icon={<History className="h-4 w-4 text-cyan-300" />}>
          <MetricValuesList
            values={metricValuesQuery.data ?? []}
            selectedMetricValueId={selectedMetricValueId}
            onSelectMetricValue={setSelectedMetricValueId}
          />
        </Panel>
        <Panel title="Selected metric value" icon={<History className="h-4 w-4 text-cyan-300" />}>
          {metricValuesQuery.data?.find((value) => value.metricValueId === selectedMetricValueId) ? (
            (() => {
              const value = metricValuesQuery.data!.find((item) => item.metricValueId === selectedMetricValueId)!
              return (
                <div className="space-y-2 text-sm text-slate-300">
                  <p><strong className="text-slate-100">Metric:</strong> {value.metricId}</p>
                  <p><strong className="text-slate-100">Value:</strong> {value.value}</p>
                  <p><strong className="text-slate-100">Period:</strong> {formatDate(value.periodStart)} to {formatDate(value.periodEnd)}</p>
                  <p><strong className="text-slate-100">Group key:</strong> {value.groupKey ?? 'n/a'}</p>
                  <p><strong className="text-slate-100">Group label:</strong> {value.groupLabel ?? 'n/a'}</p>
                  <p><strong className="text-slate-100">Source trace:</strong> {value.sourceTraceSummary}</p>
                  <p><strong className="text-slate-100">Calculated:</strong> {formatDate(value.calculatedAt)}</p>
                </div>
              )
            })()
          ) : (
            <EmptyState title="Select a metric value to inspect details." />
          )}
        </Panel>
        <Panel title="Analytics snapshots" icon={<ShieldCheck className="h-4 w-4 text-cyan-300" />}>
          <div className="reportarr-stack">
            {(snapshotsQuery.data ?? []).map((snapshot) => (
              <button
                key={snapshot.analyticsSnapshotId}
                type="button"
                className={['reportarr-row reportarr-row-button', snapshot.analyticsSnapshotId === selectedSnapshotId ? 'active' : ''].join(' ')}
                onClick={() => setSelectedSnapshotId(snapshot.analyticsSnapshotId)}
              >
                <div className="reportarr-row-main">
                  <strong>{snapshot.snapshotNumber}</strong>
                  <span>{snapshot.snapshotType}</span>
                  <small>{snapshot.datasetRefs.join(', ')}</small>
                  <small>{formatDate(snapshot.periodStart)} to {formatDate(snapshot.periodEnd)}</small>
                  <small>{snapshot.kpiValueRefs.length} KPI values · {snapshot.metricValueRefs.length} metric values</small>
                  <small>generated {formatDate(snapshot.generatedAt)}</small>
                </div>
                <div className="reportarr-row-meta">
                  <Pill>{snapshot.status}</Pill>
                  <Pill>{snapshot.generatedBy}</Pill>
                </div>
              </button>
            ))}
            {!snapshotsQuery.data?.length ? <EmptyState title="No analytics snapshots yet." /> : null}
          </div>
        </Panel>
        <Panel title="Selected analytics snapshot" icon={<ShieldCheck className="h-4 w-4 text-cyan-300" />}>
          {snapshotsQuery.data?.find((snapshot) => snapshot.analyticsSnapshotId === selectedSnapshotId) ? (
            (() => {
              const snapshot = snapshotsQuery.data!.find((item) => item.analyticsSnapshotId === selectedSnapshotId)!
              return (
                <div className="space-y-2 text-sm text-slate-300">
                  <p><strong className="text-slate-100">Type:</strong> {snapshot.snapshotType}</p>
                  <p><strong className="text-slate-100">Status:</strong> {snapshot.status}</p>
                  <p><strong className="text-slate-100">Period:</strong> {formatDate(snapshot.periodStart)} to {formatDate(snapshot.periodEnd)}</p>
                  <p><strong className="text-slate-100">Dataset refs:</strong> {snapshot.datasetRefs.join(', ') || 'none'}</p>
                  <p><strong className="text-slate-100">KPI value refs:</strong> {snapshot.kpiValueRefs.join(', ') || 'none'}</p>
                  <p><strong className="text-slate-100">Metric value refs:</strong> {snapshot.metricValueRefs.join(', ') || 'none'}</p>
                  <p><strong className="text-slate-100">Generated at:</strong> {formatDate(snapshot.generatedAt)}</p>
                  <p><strong className="text-slate-100">Generated by:</strong> {snapshot.generatedBy}</p>
                </div>
              )
            })()
          ) : (
            <EmptyState title="Select a snapshot to inspect details." />
          )}
        </Panel>
        <Panel title="Trend analyses" icon={<Workflow className="h-4 w-4 text-cyan-300" />}>
          <div className="reportarr-stack">
            {(trendsQuery.data ?? []).map((trend) => (
              <button
                key={trend.trendAnalysisId}
                type="button"
                className={['reportarr-row reportarr-row-button', trend.trendAnalysisId === selectedTrendId ? 'active' : ''].join(' ')}
                onClick={() => setSelectedTrendId(trend.trendAnalysisId)}
              >
                <div className="reportarr-row-main">
                  <strong>{trend.metricRef}</strong>
                  <span>{trend.trend}</span>
                  <small>{trend.explanation}</small>
                  <small>{formatDate(trend.periodStart)} to {formatDate(trend.periodEnd)}</small>
                  <small>change {trend.changeValue} ({trend.changePercent}%)</small>
                  <small>generated {formatDate(trend.generatedAt)}</small>
                </div>
                <div className="reportarr-row-meta">
                  <Pill>{trend.confidence}</Pill>
                </div>
              </button>
            ))}
            {!trendsQuery.data?.length ? <EmptyState title="No trend analyses yet." /> : null}
          </div>
        </Panel>
        <Panel title="Selected trend analysis" icon={<Workflow className="h-4 w-4 text-cyan-300" />}>
          {trendsQuery.data?.find((trend) => trend.trendAnalysisId === selectedTrendId) ? (
            (() => {
              const trend = trendsQuery.data!.find((item) => item.trendAnalysisId === selectedTrendId)!
              return (
                <div className="space-y-2 text-sm text-slate-300">
                  <p><strong className="text-slate-100">Metric ref:</strong> {trend.metricRef}</p>
                  <p><strong className="text-slate-100">KPI ref:</strong> {trend.kpiRef ?? 'n/a'}</p>
                  <p><strong className="text-slate-100">Trend:</strong> {trend.trend}</p>
                  <p><strong className="text-slate-100">Period:</strong> {formatDate(trend.periodStart)} to {formatDate(trend.periodEnd)}</p>
                  <p><strong className="text-slate-100">Change value:</strong> {trend.changeValue}</p>
                  <p><strong className="text-slate-100">Change percent:</strong> {trend.changePercent}%</p>
                  <p><strong className="text-slate-100">Confidence:</strong> {trend.confidence}</p>
                  <p><strong className="text-slate-100">Explanation:</strong> {trend.explanation}</p>
                  <p><strong className="text-slate-100">Generated at:</strong> {formatDate(trend.generatedAt)}</p>
                </div>
              )
            })()
          ) : (
            <EmptyState title="Select a trend analysis to inspect details." />
          )}
        </Panel>
        <Panel title="Exception queries" icon={<AlertTriangle className="h-4 w-4 text-cyan-300" />}>
          <div className="reportarr-stack">
            {(exceptionQueriesQuery.data ?? []).map((query) => (
              <button
                key={query.exceptionQueryId}
                type="button"
                className={['reportarr-row reportarr-row-button', query.exceptionQueryId === selectedExceptionQueryId ? 'active' : ''].join(' ')}
                onClick={() => setSelectedExceptionQueryId(query.exceptionQueryId)}
              >
                <div className="reportarr-row-main">
                  <strong>{query.queryKey}</strong>
                  <span>{query.title}</span>
                  <small>{query.description}</small>
                  <small>{query.condition}</small>
                  <small>owner {query.ownerPersonId}</small>
                </div>
                <div className="reportarr-row-meta">
                  <Pill>{query.status}</Pill>
                  <Pill>{query.severity}</Pill>
                </div>
              </button>
            ))}
            {!exceptionQueriesQuery.data?.length ? <EmptyState title="No exception queries yet." /> : null}
          </div>
        </Panel>
        <Panel title="Selected exception query" icon={<AlertTriangle className="h-4 w-4 text-cyan-300" />}>
          {exceptionQueriesQuery.data?.find((query) => query.exceptionQueryId === selectedExceptionQueryId) ? (
            (() => {
              const query = exceptionQueriesQuery.data!.find((item) => item.exceptionQueryId === selectedExceptionQueryId)!
              return (
                <div className="space-y-2 text-sm text-slate-300">
                  <p><strong className="text-slate-100">Query key:</strong> {query.queryKey}</p>
                  <p><strong className="text-slate-100">Title:</strong> {query.title}</p>
                  <p><strong className="text-slate-100">Description:</strong> {query.description}</p>
                  <p><strong className="text-slate-100">Source dataset:</strong> {query.sourceDatasetRef}</p>
                  <p><strong className="text-slate-100">Condition:</strong> {query.condition}</p>
                  <p><strong className="text-slate-100">Severity:</strong> {query.severity}</p>
                  <p><strong className="text-slate-100">Status:</strong> {query.status}</p>
                  <p><strong className="text-slate-100">Owner:</strong> {query.ownerPersonId}</p>
                </div>
              )
            })()
          ) : (
            <EmptyState title="Select an exception query to inspect details." />
          )}
        </Panel>
        <Panel title="Exception results" icon={<AlertTriangle className="h-4 w-4 text-cyan-300" />}>
          <div className="reportarr-stack">
            {selectedExceptionQuery ? (
              selectedExceptionResults.map((result) => (
                <button
                  key={result.exceptionResultId}
                  type="button"
                  className={['reportarr-row reportarr-row-button', result.exceptionResultId === selectedExceptionResultId ? 'active' : ''].join(' ')}
                  onClick={() => setSelectedExceptionResultId(result.exceptionResultId)}
                >
                  <div className="reportarr-row-main">
                    <strong>{result.sourceObjectRef}</strong>
                    <span>{result.title}</span>
                    <small>{result.summary}</small>
                    <small>{summarizeText(result.sourceTrace, 96)}</small>
                    <small>ack {result.acknowledgedByPersonId ?? 'n/a'} · {formatDate(result.acknowledgedAt)}</small>
                    <small>resolved {formatDate(result.resolvedAt)}</small>
                  </div>
                  <div className="reportarr-row-meta">
                    <Pill>{result.status}</Pill>
                    <Pill>{result.severity}</Pill>
                  </div>
                </button>
              ))
            ) : (
              <EmptyState title="Select an exception query to inspect its results." />
            )}
            {selectedExceptionQuery && !selectedExceptionResults.length ? <EmptyState title="No exception results for the selected query." /> : null}
          </div>
        </Panel>
        <Panel title="Selected exception result" icon={<AlertTriangle className="h-4 w-4 text-cyan-300" />}>
          {selectedExceptionResults.find((result) => result.exceptionResultId === selectedExceptionResultId) ? (
            (() => {
              const result = selectedExceptionResults.find((item) => item.exceptionResultId === selectedExceptionResultId)!
              return (
                <div className="space-y-2 text-sm text-slate-300">
                  <p><strong className="text-slate-100">Query:</strong> {result.exceptionQueryId}</p>
                  <p><strong className="text-slate-100">Source object:</strong> {result.sourceObjectRef}</p>
                  <p><strong className="text-slate-100">Title:</strong> {result.title}</p>
                  <p><strong className="text-slate-100">Summary:</strong> {result.summary}</p>
                  <p><strong className="text-slate-100">Severity:</strong> {result.severity}</p>
                  <p><strong className="text-slate-100">Status:</strong> {result.status}</p>
                  <p><strong className="text-slate-100">Detected:</strong> {formatDate(result.detectedAt)}</p>
                  <p><strong className="text-slate-100">Acknowledged by:</strong> {result.acknowledgedByPersonId ?? 'n/a'}</p>
                  <p><strong className="text-slate-100">Acknowledged at:</strong> {formatDate(result.acknowledgedAt)}</p>
                  <p><strong className="text-slate-100">Resolved at:</strong> {formatDate(result.resolvedAt)}</p>
                  <p><strong className="text-slate-100">Source trace:</strong> {summarizeText(result.sourceTrace, 160)}</p>
                </div>
              )
            })()
          ) : (
            <EmptyState title="Select an exception result to inspect details." />
          )}
        </Panel>
      </div>
    </div>
  )
}

function KpiValuesList({
  values,
  selectedKpiValueId = '',
  onSelectKpiValue = () => {},
}: {
  values: ReportArrKpiValueResponse[]
  selectedKpiValueId?: string
  onSelectKpiValue?: (kpiValueId: string) => void
}) {
  if (!values.length) return <EmptyState title="No KPI values yet." />
  return (
    <div className="reportarr-stack">
      {values.map((value) => (
        <button
          key={value.kpiValueId}
          type="button"
          className={['reportarr-row reportarr-row-button', value.kpiValueId === selectedKpiValueId ? 'active' : ''].join(' ')}
          onClick={() => onSelectKpiValue(value.kpiValueId)}
        >
          <div className="reportarr-row-main">
            <strong>{value.kpiId}</strong>
            <span>{value.value}</span>
            <small>{value.trend} · {value.status}</small>
            <small>period {formatDate(value.periodStart)} to {formatDate(value.periodEnd)}</small>
            <small>{value.sourceTraceSummary}</small>
          </div>
          <div className="reportarr-row-meta">
            <Pill>{formatDate(value.calculatedAt)}</Pill>
          </div>
        </button>
      ))}
    </div>
  )
}

function AlertsPage({
  accessToken,
  roleKey,
  isPlatformAdmin,
}: {
  accessToken: string
  roleKey: string
  isPlatformAdmin: boolean
}) {
  const queryClient = useQueryClient()
  const alertsQuery = useQuery({
    queryKey: ['reportarr', 'alerts'],
    queryFn: () => listAlerts(accessToken),
    enabled: Boolean(accessToken),
  })
  const dashboardsQuery = useQuery({
    queryKey: ['reportarr', 'dashboards'],
    queryFn: () => listDashboards(accessToken),
    enabled: Boolean(accessToken),
  })
  const widgetsQuery = useQuery({
    queryKey: ['reportarr', 'widgets'],
    queryFn: () => listWidgets(accessToken),
    enabled: Boolean(accessToken),
  })
  const reportsQuery = useQuery({
    queryKey: ['reportarr', 'reports'],
    queryFn: () => listReportDefinitions(accessToken),
    enabled: Boolean(accessToken),
  })
  const metricsQuery = useQuery({
    queryKey: ['reportarr', 'metrics'],
    queryFn: () => listMetrics(accessToken),
    enabled: Boolean(accessToken),
  })
  const [selectedAlertId, setSelectedAlertId] = useState('alrt-001')

  const acknowledgeMutation = useMutation({
    mutationFn: () => acknowledgeAlert(accessToken, selectedAlertId, { requestedByPersonId: 'person-compliance-reporter' }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['reportarr'] })
    },
  })
  const resolveMutation = useMutation({
    mutationFn: () => resolveAlert(accessToken, selectedAlertId, { requestedByPersonId: 'person-compliance-reporter' }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['reportarr'] })
    },
  })
  const canManageAlerts = canUseReportArrAction(roleKey, isPlatformAdmin, ['compliance_reporter', 'operations_reporter', 'reportarr_admin', 'tenant_admin'])

  return (
    <div className="reportarr-page">
      <SectionHeader
        eyebrow="Alerts"
        title="Alert backlog"
        description="Triage stale datasets, failed refreshes, and KPI exceptions with acknowledge and resolve actions."
        action={<Pill><Bell className="h-4 w-4" /> {alertsQuery.data?.length ?? 0} alerts</Pill>}
      />

      {canManageAlerts ? (
        <div className="flex flex-wrap gap-3">
          <button className="reportarr-button" type="button" onClick={() => acknowledgeMutation.mutate()} disabled={acknowledgeMutation.isPending}>
            {acknowledgeMutation.isPending ? 'Acknowledging…' : 'Acknowledge selected'}
          </button>
          <button className="reportarr-button secondary" type="button" onClick={() => resolveMutation.mutate()} disabled={resolveMutation.isPending}>
            {resolveMutation.isPending ? 'Resolving…' : 'Resolve selected'}
          </button>
        </div>
      ) : null}

      <div className="reportarr-grid cols-2">
        <Panel title="Alerts" icon={<AlertTriangle className="h-4 w-4 text-cyan-300" />}>
          <div className="reportarr-stack">
            {(alertsQuery.data ?? []).map((alert) => (
              <button
                key={alert.alertId}
                type="button"
                className={['reportarr-row reportarr-row-button', alert.alertId === selectedAlertId ? 'active' : ''].join(' ')}
                onClick={() => setSelectedAlertId(alert.alertId)}
              >
                <div className="reportarr-row-main">
                  <strong>{alert.alertNumber}</strong>
                  <span>{alert.title}</span>
                  <small>{alert.description}</small>
                </div>
                <div className="reportarr-row-meta">
                  <Pill>{alert.status}</Pill>
                  <Pill>{alert.severity}</Pill>
                </div>
              </button>
            ))}
            {!alertsQuery.data?.length ? <EmptyState title="No alerts yet." /> : null}
          </div>
        </Panel>
        <Panel title="Selected alert" icon={<CheckCircle2 className="h-4 w-4 text-cyan-300" />}>
          {alertsQuery.data?.find((alert) => alert.alertId === selectedAlertId) ? (
            (() => {
              const alert = alertsQuery.data!.find((item) => item.alertId === selectedAlertId)!
              const sourceDatasets = [
                alert.datasetRef,
                metricsQuery.data?.find((metric) => metric.metricId === alert.metricRef)?.sourceDatasetRef,
              ].filter((item): item is string => Boolean(item))
              const relatedDashboards = (dashboardsQuery.data ?? []).filter((dashboard) => {
                const dashboardWidgets = (widgetsQuery.data ?? []).filter((widget) => dashboard.widgetRefs.includes(widget.widgetId))
                return dashboardWidgets.some((widget) => sourceDatasets.includes(widget.datasetRef))
              })
              const relatedReports = (reportsQuery.data ?? []).filter((report) =>
                report.datasetRefs.some((datasetRef) => sourceDatasets.includes(datasetRef)),
              )
              return (
                <div className="space-y-2 text-sm text-slate-300">
                  <p><strong className="text-slate-100">Alert number:</strong> {alert.alertNumber}</p>
                  <p><strong className="text-slate-100">Type:</strong> {alert.alertType}</p>
                  <p><strong className="text-slate-100">Severity:</strong> {alert.severity}</p>
                  <p><strong className="text-slate-100">Status:</strong> {alert.status}</p>
                  <p><strong className="text-slate-100">Dataset:</strong> {alert.datasetRef}</p>
                  <p><strong className="text-slate-100">Metric:</strong> {alert.metricRef}</p>
                  <p><strong className="text-slate-100">Condition:</strong> {alert.condition}</p>
                  <p><strong className="text-slate-100">Triggered:</strong> {formatDate(alert.triggeredAt)}</p>
                  <p><strong className="text-slate-100">Acknowledged:</strong> {formatDate(alert.acknowledgedAt)}</p>
                  <p><strong className="text-slate-100">Acknowledged by:</strong> {alert.acknowledgedByPersonId ?? 'n/a'}</p>
                  <p><strong className="text-slate-100">Resolved:</strong> {formatDate(alert.resolvedAt)}</p>
                  <p><strong className="text-slate-100">Notifications:</strong> {alert.notificationRefs.join(', ') || 'none'}</p>
                  <p><strong className="text-slate-100">Related dashboards:</strong> {relatedDashboards.map((item) => item.dashboardNumber).join(', ') || 'none'}</p>
                  <p><strong className="text-slate-100">Related reports:</strong> {relatedReports.map((item) => item.reportNumber).join(', ') || 'none'}</p>
                </div>
              )
            })()
          ) : (
            <EmptyState title="Select an alert to inspect details." />
          )}
        </Panel>
      </div>
    </div>
  )
}

function AuditPage({
  accessToken,
  roleKey,
  isPlatformAdmin,
}: {
  accessToken: string
  roleKey: string
  isPlatformAdmin: boolean
}) {
  const queryClient = useQueryClient()
  const auditPackagesQuery = useQuery({
    queryKey: ['reportarr', 'audit-packages'],
  queryFn: () => listAuditPackages(accessToken),
    enabled: Boolean(accessToken),
  })
  const auditScopesQuery = useQuery({
    queryKey: ['reportarr', 'audit-scopes'],
    queryFn: () => listAuditScopes(accessToken),
    enabled: Boolean(accessToken),
  })
  const [form, setForm] = useState({
    auditScopeId: 'scope-001',
    title: 'Quarterly executive audit',
    description: 'Executive audit readiness across the suite.',
  })
  const [selectedAuditPackageId, setSelectedAuditPackageId] = useState('')
  const [selectedAuditScopeId, setSelectedAuditScopeId] = useState('')

  const createMutation = useMutation({
    mutationFn: () => createAuditPackage(accessToken, { ...form, requestedByPersonId: 'person-compliance-reporter' }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['reportarr'] })
    },
  })
  const lockMutation = useMutation({
    mutationFn: (auditPackageId: string) => lockAuditPackage(accessToken, auditPackageId, { requestedByPersonId: 'person-compliance-reporter' }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['reportarr'] })
    },
  })
  const canManageAudits = canUseReportArrAction(roleKey, isPlatformAdmin, ['compliance_reporter', 'reportarr_admin', 'tenant_admin'])

  useEffect(() => {
    const packages = auditPackagesQuery.data ?? []
    if (!packages.length) {
      setSelectedAuditPackageId('')
      return
    }
    if (!packages.some((item) => item.auditReportPackageId === selectedAuditPackageId)) {
      setSelectedAuditPackageId(packages[0].auditReportPackageId)
    }
  }, [auditPackagesQuery.data, selectedAuditPackageId])

  useEffect(() => {
    const scopes = auditScopesQuery.data ?? []
    if (!scopes.length) {
      setSelectedAuditScopeId('')
      return
    }
    if (!scopes.some((scope) => scope.auditScopeId === selectedAuditScopeId)) {
      setSelectedAuditScopeId(scopes[0].auditScopeId)
    }
  }, [auditScopesQuery.data, selectedAuditScopeId])

  return (
    <div className="reportarr-page">
      <SectionHeader
        eyebrow="Audit"
        title="Evidence-backed audit packages"
        description="Curate source products, report runs, and missing-evidence summaries into a reviewable audit package."
        action={<Pill><ShieldCheck className="h-4 w-4" /> {auditPackagesQuery.data?.length ?? 0} packages</Pill>}
      />

      {canManageAudits ? (
        <Panel title="Create audit package" icon={<Plus className="h-4 w-4 text-cyan-300" />}>
        <div className="grid gap-3 md:grid-cols-2">
          <Select value={form.auditScopeId} onChange={(value) => setForm({ ...form, auditScopeId: value })} options={(auditScopesQuery.data ?? []).map((scope) => scope.auditScopeId)} />
          <TextInput value={form.title} onChange={(value) => setForm({ ...form, title: value })} placeholder="Title" />
          <TextInput value={form.description} onChange={(value) => setForm({ ...form, description: value })} placeholder="Description" />
        </div>
        <button className="reportarr-button mt-4" type="button" onClick={() => createMutation.mutate()} disabled={createMutation.isPending}>
          {createMutation.isPending ? 'Creating…' : 'Create audit package'}
        </button>
        </Panel>
      ) : null}

      <Panel title="Audit packages" icon={<ShieldCheck className="h-4 w-4 text-cyan-300" />}>
        <div className="reportarr-stack">
          {(auditPackagesQuery.data ?? []).map((pkg) => (
            <button
              key={pkg.auditReportPackageId}
              type="button"
              className={['reportarr-row reportarr-row-button', pkg.auditReportPackageId === selectedAuditPackageId ? 'active' : ''].join(' ')}
              onClick={() => setSelectedAuditPackageId(pkg.auditReportPackageId)}
            >
              <div className="reportarr-row-main">
                <strong>{pkg.packageNumber}</strong>
                <span>{pkg.title}</span>
                <small>{pkg.missingEvidenceSummary} · {pkg.invalidEvidenceSummary}</small>
                <small>{pkg.auditScope.scopeType} · evidence {pkg.auditScope.includeEvidence ? 'on' : 'off'} · trace {pkg.auditScope.includeSourceTrace ? 'on' : 'off'}</small>
                <small>evaluations {pkg.complianceEvaluationRefs.join(', ') || 'none'}</small>
                <small>sources {pkg.sourceProductRefs.join(', ') || 'none'} · objects {pkg.sourceObjectRefs.join(', ') || 'none'}</small>
                <small>{pkg.recordArrPackageRef ?? 'No RecordArr package'} · {pkg.reportRunRefs.length} report runs</small>
                <small>generated {formatDate(pkg.generatedAt)} · locked {formatDate(pkg.lockedAt)}</small>
              </div>
              <div className="reportarr-row-meta">
                <Pill>{pkg.status}</Pill>
                <Pill>{formatNumber(pkg.readinessScore)}%</Pill>
                {canManageAudits ? (
                  <button
                    className="reportarr-button secondary"
                    type="button"
                    onClick={() => lockMutation.mutate(pkg.auditReportPackageId)}
                    disabled={lockMutation.isPending || pkg.status === 'locked'}
                  >
                    {lockMutation.isPending && pkg.status !== 'locked' ? 'Locking…' : 'Lock package'}
                  </button>
                ) : null}
              </div>
            </button>
          ))}
          {!auditPackagesQuery.data?.length ? <EmptyState title="No audit packages yet." /> : null}
        </div>
      </Panel>
      <Panel title="Selected audit package" icon={<ShieldCheck className="h-4 w-4 text-cyan-300" />}>
        <AuditPackageDetail
          auditPackage={auditPackagesQuery.data?.find((pkg) => pkg.auditReportPackageId === selectedAuditPackageId) ?? null}
        />
      </Panel>
      <Panel title="Audit scopes" icon={<Layers3 className="h-4 w-4 text-cyan-300" />}>
        <div className="reportarr-stack">
          {(auditScopesQuery.data ?? []).map((scope) => (
            <button
              key={scope.auditScopeId}
              type="button"
              className={['reportarr-row reportarr-row-button', scope.auditScopeId === selectedAuditScopeId ? 'active' : ''].join(' ')}
              onClick={() => setSelectedAuditScopeId(scope.auditScopeId)}
            >
              <div className="reportarr-row-main">
                <strong>{scope.scopeType}</strong>
                <span>{scope.auditScopeId}</span>
                <small>{scope.productFilters.join(', ') || 'No product filters'}</small>
              </div>
              <div className="reportarr-row-meta">
                <Pill>{scope.includeEvidence ? 'evidence' : 'no evidence'}</Pill>
                <Pill>{scope.includeSourceTrace ? 'trace' : 'no trace'}</Pill>
              </div>
            </button>
          ))}
          {!auditScopesQuery.data?.length ? <EmptyState title="No audit scopes yet." /> : null}
        </div>
      </Panel>
      <Panel title="Selected audit scope" icon={<Layers3 className="h-4 w-4 text-cyan-300" />}>
        {auditScopesQuery.data?.find((scope) => scope.auditScopeId === selectedAuditScopeId) ? (
          (() => {
            const scope = auditScopesQuery.data!.find((item) => item.auditScopeId === selectedAuditScopeId)!
            return (
              <div className="space-y-2 text-sm text-slate-300">
                <p><strong className="text-slate-100">Scope type:</strong> {scope.scopeType}</p>
                <p><strong className="text-slate-100">Date range:</strong> {formatDate(scope.dateRangeStart)} to {formatDate(scope.dateRangeEnd)}</p>
                <p><strong className="text-slate-100">Product filters:</strong> {scope.productFilters.join(', ') || 'none'}</p>
                <p><strong className="text-slate-100">Object refs:</strong> {scope.objectRefs.join(', ') || 'none'}</p>
                <p><strong className="text-slate-100">Rulepack refs:</strong> {scope.rulepackRefs.join(', ') || 'none'}</p>
                <p><strong className="text-slate-100">Site refs:</strong> {scope.siteRefs.join(', ') || 'none'}</p>
                <p><strong className="text-slate-100">Department refs:</strong> {scope.departmentRefs.join(', ') || 'none'}</p>
                <p><strong className="text-slate-100">Include evidence:</strong> {scope.includeEvidence ? 'yes' : 'no'}</p>
                <p><strong className="text-slate-100">Include source trace:</strong> {scope.includeSourceTrace ? 'yes' : 'no'}</p>
              </div>
            )
          })()
        ) : (
          <EmptyState title="Select an audit scope to inspect details." />
        )}
      </Panel>
    </div>
  )
}

function AuditPackageDetail({ auditPackage }: { auditPackage: ReportArrAuditPackageResponse | null }) {
  if (!auditPackage) return <EmptyState title="Select an audit package to inspect details." />

  return (
    <div className="space-y-2 text-sm text-slate-300">
      <p><strong className="text-slate-100">Package:</strong> {auditPackage.packageNumber}</p>
      <p><strong className="text-slate-100">Title:</strong> {auditPackage.title}</p>
      <p><strong className="text-slate-100">Status:</strong> {auditPackage.status}</p>
      <p><strong className="text-slate-100">Description:</strong> {auditPackage.description}</p>
      <p><strong className="text-slate-100">Requested by:</strong> {auditPackage.requestedByPersonId}</p>
      <p><strong className="text-slate-100">Scope:</strong> {auditPackage.auditScope.scopeType}</p>
      <p><strong className="text-slate-100">Evidence included:</strong> {auditPackage.auditScope.includeEvidence ? 'yes' : 'no'}</p>
      <p><strong className="text-slate-100">Source trace included:</strong> {auditPackage.auditScope.includeSourceTrace ? 'yes' : 'no'}</p>
      <p><strong className="text-slate-100">Compliance evaluations:</strong> {auditPackage.complianceEvaluationRefs.join(', ') || 'none'}</p>
      <p><strong className="text-slate-100">Source products:</strong> {auditPackage.sourceProductRefs.join(', ') || 'none'}</p>
      <p><strong className="text-slate-100">Source objects:</strong> {auditPackage.sourceObjectRefs.join(', ') || 'none'}</p>
      <p><strong className="text-slate-100">RecordArr package:</strong> {auditPackage.recordArrPackageRef ?? 'n/a'}</p>
      <p><strong className="text-slate-100">Report runs:</strong> {auditPackage.reportRunRefs.join(', ') || 'none'}</p>
      <p><strong className="text-slate-100">Missing evidence:</strong> {auditPackage.missingEvidenceSummary}</p>
      <p><strong className="text-slate-100">Invalid evidence:</strong> {auditPackage.invalidEvidenceSummary}</p>
      <p><strong className="text-slate-100">Readiness score:</strong> {formatNumber(auditPackage.readinessScore)}%</p>
      <p><strong className="text-slate-100">Generated at:</strong> {formatDate(auditPackage.generatedAt)}</p>
      <p><strong className="text-slate-100">Locked at:</strong> {formatDate(auditPackage.lockedAt)}</p>
    </div>
  )
}

function IntegrationsPage({
  accessToken,
  roleKey,
  isPlatformAdmin,
}: {
  accessToken: string
  roleKey: string
  isPlatformAdmin: boolean
}) {
  const queryClient = useQueryClient()
  const connectorsQuery = useQuery({
    queryKey: ['reportarr', 'source-connectors'],
    queryFn: () => listSourceConnectors(accessToken),
    enabled: Boolean(accessToken),
  })
  const readModelsQuery = useQuery({
    queryKey: ['reportarr', 'read-models'],
    queryFn: () => listReadModels(accessToken),
    enabled: Boolean(accessToken),
  })
  const readModelRecordsQuery = useQuery({
    queryKey: ['reportarr', 'read-model-records'],
    queryFn: () => listReadModelRecords(accessToken),
    enabled: Boolean(accessToken),
  })
  const refreshJobsQuery = useQuery({
    queryKey: ['reportarr', 'refresh-jobs'],
    queryFn: () => listRefreshJobs(accessToken),
    enabled: Boolean(accessToken),
  })
  const ingestionCursorsQuery = useQuery({
    queryKey: ['reportarr', 'ingestion-cursors'],
    queryFn: () => listIngestionCursors(accessToken),
    enabled: Boolean(accessToken),
  })
  const sourceEventsQuery = useQuery({
    queryKey: ['reportarr', 'source-events'],
    queryFn: () => listSourceEvents(accessToken),
    enabled: Boolean(accessToken),
  })
  const widgetsQuery = useQuery({
    queryKey: ['reportarr', 'widgets'],
    queryFn: () => listWidgets(accessToken),
    enabled: Boolean(accessToken),
  })
  const visualizationsQuery = useQuery({
    queryKey: ['reportarr', 'widget-visualizations'],
    queryFn: () => listWidgetVisualizations(accessToken),
    enabled: Boolean(accessToken),
  })
  const metricsQuery = useQuery({
    queryKey: ['reportarr', 'metrics'],
    queryFn: () => listMetrics(accessToken),
    enabled: Boolean(accessToken),
  })
  const [eventForm, setEventForm] = useState({
    sourceProduct: 'routarr',
    sourceEventId: 'evt-9001',
    eventType: 'trip.completed',
    sourceObjectRef: 'trip-7781',
    correlationId: 'corr-9001',
  })
  const [batchEventForms, setBatchEventForms] = useState<ReportArrIntegrationEventRequest[]>([
    makeEventRow(),
    makeEventRow({
      sourceProduct: 'loadarr',
      sourceEventId: 'evt-9002',
      eventType: 'inventory.balance.updated',
      sourceObjectRef: 'bal-551',
      correlationId: 'corr-9002',
    }),
  ])
  const [selectedReadModelId, setSelectedReadModelId] = useState('rm-001')
  const [selectedSourceConnectorId, setSelectedSourceConnectorId] = useState('')
  const [selectedIngestionCursorId, setSelectedIngestionCursorId] = useState('')
  const [selectedRefreshJobId, setSelectedRefreshJobId] = useState('')
  const [selectedReadModelRecordId, setSelectedReadModelRecordId] = useState('')
  const [selectedWidgetId, setSelectedWidgetId] = useState('')
  const [selectedMetricId, setSelectedMetricId] = useState('')
  const [selectedSourceEventId, setSelectedSourceEventId] = useState('')

  useEffect(() => {
    const connectors = connectorsQuery.data ?? []
    if (!connectors.length) return
    if (!connectors.some((connector) => connector.sourceConnectorId === selectedSourceConnectorId)) {
      setSelectedSourceConnectorId(connectors[0].sourceConnectorId)
    }
  }, [connectorsQuery.data, selectedSourceConnectorId])

  useEffect(() => {
    const cursors = ingestionCursorsQuery.data ?? []
    if (!cursors.length) return
    if (!cursors.some((cursor) => cursor.ingestionCursorId === selectedIngestionCursorId)) {
      setSelectedIngestionCursorId(cursors[0].ingestionCursorId)
    }
  }, [ingestionCursorsQuery.data, selectedIngestionCursorId])

  useEffect(() => {
    const jobs = refreshJobsQuery.data ?? []
    if (!jobs.length) return
    if (!jobs.some((job) => job.refreshJobId === selectedRefreshJobId)) {
      setSelectedRefreshJobId(jobs[0].refreshJobId)
    }
  }, [refreshJobsQuery.data, selectedRefreshJobId])

  useEffect(() => {
    const records = readModelRecordsQuery.data ?? []
    if (!records.length) return
    if (!records.some((record) => record.readModelRecordId === selectedReadModelRecordId)) {
      setSelectedReadModelRecordId(records[0].readModelRecordId)
    }
  }, [readModelRecordsQuery.data, selectedReadModelRecordId])

  useEffect(() => {
    const widgets = widgetsQuery.data ?? []
    if (!widgets.length) return
    if (!widgets.some((widget) => widget.widgetId === selectedWidgetId)) {
      setSelectedWidgetId(widgets[0].widgetId)
    }
  }, [selectedWidgetId, widgetsQuery.data])

  useEffect(() => {
    const events = sourceEventsQuery.data ?? []
    if (!events.length) return
    if (!events.some((event) => event.sourceEventReceiptId === selectedSourceEventId)) {
      setSelectedSourceEventId(events[0].sourceEventReceiptId)
    }
  }, [selectedSourceEventId, sourceEventsQuery.data])

  useEffect(() => {
    const metrics = metricsQuery.data ?? []
    if (!metrics.length) return
    if (!metrics.some((metric) => metric.metricId === selectedMetricId)) {
      setSelectedMetricId(metrics[0].metricId)
    }
  }, [metricsQuery.data, selectedMetricId])

  const eventMutation = useMutation({
    mutationFn: () => receiveEvent(accessToken, eventForm),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['reportarr'] })
    },
  })
  const batchEventMutation = useMutation({
    mutationFn: () => receiveEventBatch(accessToken, { events: batchEventForms }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['reportarr'] })
    },
  })
  const rebuildMutation = useMutation({
    mutationFn: () => rebuildReadModel(accessToken, selectedReadModelId, 'person-analytics-lead'),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['reportarr'] })
    },
  })
  const canManageDatasets = canUseReportArrAction(roleKey, isPlatformAdmin, ['analytics_admin', 'reportarr_admin', 'tenant_admin'])

  return (
    <div className="reportarr-page">
      <SectionHeader
        eyebrow="Integrations"
        title="Cross-product data plane"
        description="Inspect source connectors, read models, event ingestion, and the refresh jobs that keep the reporting layer aligned to source truth."
        action={<Pill><PlugZap className="h-4 w-4" /> {connectorsQuery.data?.length ?? 0} connectors</Pill>}
      />

      {canManageDatasets ? (
        <Panel title="Batch receive events" icon={<Workflow className="h-4 w-4 text-cyan-300" />}>
        <div className="space-y-4">
          {batchEventForms.map((row, index) => (
            <div key={`${row.sourceEventId}-${index}`} className="rounded-2xl border border-slate-700/80 bg-slate-950/30 p-4">
              <div className="mb-3 flex items-center justify-between gap-3">
                <strong className="text-sm text-slate-100">Event {index + 1}</strong>
                <button
                  type="button"
                  className="reportarr-button secondary"
                  onClick={() => setBatchEventForms((current) => current.filter((_, rowIndex) => rowIndex !== index))}
                  disabled={batchEventForms.length === 1}
                >
                  Remove
                </button>
              </div>
              <div className="grid gap-3 md:grid-cols-2">
                <TextInput
                  value={row.sourceProduct}
                  onChange={(value) =>
                    setBatchEventForms((current) =>
                      current.map((item, rowIndex) => (rowIndex === index ? { ...item, sourceProduct: value } : item)),
                    )
                  }
                  placeholder="source product"
                />
                <TextInput
                  value={row.sourceEventId}
                  onChange={(value) =>
                    setBatchEventForms((current) =>
                      current.map((item, rowIndex) => (rowIndex === index ? { ...item, sourceEventId: value } : item)),
                    )
                  }
                  placeholder="source event id"
                />
                <TextInput
                  value={row.eventType}
                  onChange={(value) =>
                    setBatchEventForms((current) =>
                      current.map((item, rowIndex) => (rowIndex === index ? { ...item, eventType: value } : item)),
                    )
                  }
                  placeholder="event type"
                />
                <TextInput
                  value={row.sourceObjectRef ?? ''}
                  onChange={(value) =>
                    setBatchEventForms((current) =>
                      current.map((item, rowIndex) => (rowIndex === index ? { ...item, sourceObjectRef: value } : item)),
                    )
                  }
                  placeholder="source object ref"
                />
                <div className="md:col-span-2">
                  <TextInput
                    value={row.correlationId ?? ''}
                    onChange={(value) =>
                      setBatchEventForms((current) =>
                        current.map((item, rowIndex) => (rowIndex === index ? { ...item, correlationId: value } : item)),
                      )
                    }
                    placeholder="correlation id"
                  />
                </div>
              </div>
            </div>
          ))}
        </div>
        <div className="mt-4 flex flex-wrap gap-3">
          <button
            type="button"
            className="reportarr-button secondary"
            onClick={() => setBatchEventForms((current) => [...current, makeEventRow({ sourceEventId: `evt-${9000 + current.length + 1}`, correlationId: `corr-${9000 + current.length + 1}` })])}
          >
            Add event row
          </button>
          <button className="reportarr-button" type="button" onClick={() => batchEventMutation.mutate()} disabled={batchEventMutation.isPending || batchEventForms.length === 0}>
            {batchEventMutation.isPending ? 'Receiving…' : 'Receive batch'}
          </button>
          {batchEventMutation.isError ? <span className="text-sm text-rose-300">{getErrorMessage(batchEventMutation.error, 'Batch receive failed')}</span> : null}
        </div>
        </Panel>
      ) : null}

      {canManageDatasets ? (
        <Panel title="Receive event" icon={<Workflow className="h-4 w-4 text-cyan-300" />}>
        <div className="grid gap-3 md:grid-cols-2">
          <TextInput value={eventForm.sourceProduct} onChange={(value) => setEventForm({ ...eventForm, sourceProduct: value })} placeholder="source product" />
          <TextInput value={eventForm.sourceEventId} onChange={(value) => setEventForm({ ...eventForm, sourceEventId: value })} placeholder="source event id" />
          <TextInput value={eventForm.eventType} onChange={(value) => setEventForm({ ...eventForm, eventType: value })} placeholder="event type" />
          <TextInput value={eventForm.sourceObjectRef ?? ''} onChange={(value) => setEventForm({ ...eventForm, sourceObjectRef: value })} placeholder="source object ref" />
          <div className="md:col-span-2">
            <TextInput value={eventForm.correlationId ?? ''} onChange={(value) => setEventForm({ ...eventForm, correlationId: value })} placeholder="correlation id" />
          </div>
        </div>
        <div className="mt-4 flex flex-wrap gap-3">
          <button className="reportarr-button" type="button" onClick={() => eventMutation.mutate()} disabled={eventMutation.isPending}>
            {eventMutation.isPending ? 'Receiving…' : 'Receive event'}
          </button>
          <button className="reportarr-button secondary" type="button" onClick={() => rebuildMutation.mutate()} disabled={rebuildMutation.isPending}>
            {rebuildMutation.isPending ? 'Rebuilding…' : 'Rebuild selected read model'}
          </button>
        </div>
        </Panel>
      ) : null}

      <div className="reportarr-grid cols-2">
        <Panel title="Source connectors" icon={<PlugZap className="h-4 w-4 text-cyan-300" />}>
          <ConnectorsList
            connectors={connectorsQuery.data ?? []}
            selectedConnectorId={selectedSourceConnectorId}
            onSelectConnector={setSelectedSourceConnectorId}
          />
        </Panel>
        <Panel title="Selected source connector" icon={<PlugZap className="h-4 w-4 text-cyan-300" />}>
          {connectorsQuery.data?.find((connector) => connector.sourceConnectorId === selectedSourceConnectorId) ? (
            (() => {
              const connector = connectorsQuery.data!.find((item) => item.sourceConnectorId === selectedSourceConnectorId)!
              return (
                <div className="space-y-2 text-sm text-slate-300">
                  <p><strong className="text-slate-100">Source product:</strong> {connector.sourceProduct}</p>
                  <p><strong className="text-slate-100">Connector type:</strong> {connector.connectorType}</p>
                  <p><strong className="text-slate-100">Status:</strong> {connector.status}</p>
                  <p><strong className="text-slate-100">Service client:</strong> {connector.serviceClientRef}</p>
                  <p><strong className="text-slate-100">Last connected:</strong> {formatDate(connector.lastConnectedAt)}</p>
                  <p><strong className="text-slate-100">Last error:</strong> {formatDate(connector.lastErrorAt)}</p>
                  <p><strong className="text-slate-100">Error message:</strong> {connector.lastErrorMessage ?? 'none'}</p>
                  <p><strong className="text-slate-100">Supported event types:</strong> {connector.supportedEventTypes.join(', ') || 'none'}</p>
                  <p><strong className="text-slate-100">Supported datasets:</strong> {connector.supportedDatasets.join(', ') || 'none'}</p>
                </div>
              )
            })()
          ) : (
            <EmptyState title="Select a source connector to inspect details." />
          )}
        </Panel>
        <Panel title="Read models" icon={<Layers3 className="h-4 w-4 text-cyan-300" />}>
          <div className="reportarr-stack">
            {(readModelsQuery.data ?? []).map((model) => (
              <button
                key={model.readModelId}
                type="button"
                className={['reportarr-row reportarr-row-button', model.readModelId === selectedReadModelId ? 'active' : ''].join(' ')}
                onClick={() => setSelectedReadModelId(model.readModelId)}
              >
                <div className="reportarr-row-main">
                  <strong>{model.readModelNumber}</strong>
                  <span>{model.title}</span>
                  <small>{model.datasetRefs.join(', ')}</small>
                </div>
                <div className="reportarr-row-meta">
                  <Pill>{model.status}</Pill>
                </div>
              </button>
            ))}
            {!readModelsQuery.data?.length ? <EmptyState title="No read models yet." /> : null}
          </div>
        </Panel>
        <Panel title="Selected read model" icon={<Gauge className="h-4 w-4 text-cyan-300" />}>
          {readModelsQuery.data?.find((model) => model.readModelId === selectedReadModelId) ? (
            (() => {
              const model = readModelsQuery.data!.find((item) => item.readModelId === selectedReadModelId)!
              return (
                <div className="space-y-2 text-sm text-slate-300">
                  <p><strong className="text-slate-100">Type:</strong> {model.readModelType}</p>
                  <p><strong className="text-slate-100">Primary entity:</strong> {model.primaryEntityType}</p>
                  <p><strong className="text-slate-100">Primary source:</strong> {model.primarySourceProduct}</p>
                  <p><strong className="text-slate-100">Datasets:</strong> {model.datasetRefs.join(', ') || 'none'}</p>
                  <p><strong className="text-slate-100">Fields:</strong> {model.fieldDefinitions.join(', ') || 'none'}</p>
                  <p><strong className="text-slate-100">Refresh jobs:</strong> {model.refreshJobRefs.join(', ') || 'none'}</p>
                  <p><strong className="text-slate-100">Last rebuilt:</strong> {formatDate(model.lastRebuiltAt)}</p>
                  <p><strong className="text-slate-100">Last updated:</strong> {formatDate(model.lastUpdatedAt)}</p>
                  <p><strong className="text-slate-100">Updated:</strong> {formatDate(model.updatedAt)}</p>
                </div>
              )
            })()
          ) : (
            <EmptyState title="Select a read model to inspect details." />
          )}
        </Panel>
        <Panel title="Read model records" icon={<History className="h-4 w-4 text-cyan-300" />}>
          <ReadModelRecordsList
            records={readModelRecordsQuery.data ?? []}
            selectedRecordId={selectedReadModelRecordId}
            onSelectRecord={setSelectedReadModelRecordId}
          />
        </Panel>
        <Panel title="Selected read model record" icon={<History className="h-4 w-4 text-cyan-300" />}>
          {readModelRecordsQuery.data?.find((record) => record.readModelRecordId === selectedReadModelRecordId) ? (
            (() => {
              const record = readModelRecordsQuery.data!.find((item) => item.readModelRecordId === selectedReadModelRecordId)!
              return (
                <div className="space-y-2 text-sm text-slate-300">
                  <p><strong className="text-slate-100">Read model:</strong> {record.readModelId}</p>
                  <p><strong className="text-slate-100">Primary entity:</strong> {record.primaryEntityRef}</p>
                  <p><strong className="text-slate-100">Status snapshot:</strong> {record.statusSnapshot}</p>
                  <p><strong className="text-slate-100">Source traces:</strong> {record.sourceTraces.join(', ') || 'none'}</p>
                  <p><strong className="text-slate-100">Effective at:</strong> {formatDate(record.effectiveAt)}</p>
                  <p><strong className="text-slate-100">Last source updated:</strong> {formatDate(record.lastSourceUpdatedAt)}</p>
                  <p><strong className="text-slate-100">Ingested at:</strong> {formatDate(record.ingestedAt)}</p>
                  <p><strong className="text-slate-100">Updated at:</strong> {formatDate(record.updatedAt)}</p>
                </div>
              )
            })()
          ) : (
            <EmptyState title="Select a read model record to inspect details." />
          )}
        </Panel>
        <Panel title="Source events" icon={<History className="h-4 w-4 text-cyan-300" />}>
          <EventsList
            events={sourceEventsQuery.data ?? []}
            selectedEventId={selectedSourceEventId}
            onSelectEvent={setSelectedSourceEventId}
          />
        </Panel>
        <Panel title="Selected source event" icon={<History className="h-4 w-4 text-cyan-300" />}>
          {sourceEventsQuery.data?.find((event) => event.sourceEventReceiptId === selectedSourceEventId) ? (
            (() => {
              const event = sourceEventsQuery.data!.find((item) => item.sourceEventReceiptId === selectedSourceEventId)!
              return (
                <div className="space-y-2 text-sm text-slate-300">
                  <p><strong className="text-slate-100">Source product:</strong> {event.sourceProduct}</p>
                  <p><strong className="text-slate-100">Source event:</strong> {event.sourceEventId}</p>
                  <p><strong className="text-slate-100">Event type:</strong> {event.eventType}</p>
                  <p><strong className="text-slate-100">Source object:</strong> {event.sourceObjectRef ?? 'n/a'}</p>
                  <p><strong className="text-slate-100">Received at:</strong> {formatDate(event.receivedAt)}</p>
                  <p><strong className="text-slate-100">Processed at:</strong> {formatDate(event.processedAt)}</p>
                  <p><strong className="text-slate-100">Status:</strong> {event.status}</p>
                  <p><strong className="text-slate-100">Failure reason:</strong> {event.failureReason ?? 'none'}</p>
                  <p><strong className="text-slate-100">Correlation id:</strong> {event.correlationId ?? 'n/a'}</p>
                </div>
              )
            })()
          ) : (
            <EmptyState title="Select a source event to inspect details." />
          )}
        </Panel>
        <Panel title="Refresh jobs" icon={<RefreshCcw className="h-4 w-4 text-cyan-300" />}>
          <RefreshJobsList
            refreshJobs={refreshJobsQuery.data ?? []}
            selectedRefreshJobId={selectedRefreshJobId}
            onSelectRefreshJob={setSelectedRefreshJobId}
          />
        </Panel>
        <Panel title="Selected refresh job" icon={<RefreshCcw className="h-4 w-4 text-cyan-300" />}>
          {refreshJobsQuery.data?.find((job) => job.refreshJobId === selectedRefreshJobId) ? (
            (() => {
              const job = refreshJobsQuery.data!.find((item) => item.refreshJobId === selectedRefreshJobId)!
              return (
                <div className="space-y-2 text-sm text-slate-300">
                  <p><strong className="text-slate-100">Dataset:</strong> {job.datasetId}</p>
                  <p><strong className="text-slate-100">Read model:</strong> {job.readModelId ?? 'n/a'}</p>
                  <p><strong className="text-slate-100">Refresh type:</strong> {job.refreshType}</p>
                  <p><strong className="text-slate-100">Status:</strong> {job.status}</p>
                  <p><strong className="text-slate-100">Requested by:</strong> {job.requestedByPersonId}</p>
                  <p><strong className="text-slate-100">Queued at:</strong> {formatDate(job.queuedAt)}</p>
                  <p><strong className="text-slate-100">Started at:</strong> {formatDate(job.startedAt)}</p>
                  <p><strong className="text-slate-100">Completed at:</strong> {formatDate(job.completedAt)}</p>
                  <p><strong className="text-slate-100">Processed:</strong> {job.recordsProcessed}</p>
                  <p><strong className="text-slate-100">Created:</strong> {job.recordsCreated}</p>
                  <p><strong className="text-slate-100">Updated:</strong> {job.recordsUpdated}</p>
                  <p><strong className="text-slate-100">Skipped:</strong> {job.recordsSkipped}</p>
                  <p><strong className="text-slate-100">Error count:</strong> {job.errorCount}</p>
                  <p><strong className="text-slate-100">Error message:</strong> {job.errorMessage ?? 'none'}</p>
                </div>
              )
            })()
          ) : (
            <EmptyState title="Select a refresh job to inspect details." />
          )}
        </Panel>
        <Panel title="Ingestion cursors" icon={<Workflow className="h-4 w-4 text-cyan-300" />}>
          <IngestionCursorsList
            cursors={ingestionCursorsQuery.data ?? []}
            selectedCursorId={selectedIngestionCursorId}
            onSelectCursor={setSelectedIngestionCursorId}
          />
        </Panel>
        <Panel title="Selected ingestion cursor" icon={<Gauge className="h-4 w-4 text-cyan-300" />}>
          {ingestionCursorsQuery.data?.find((cursor) => cursor.ingestionCursorId === selectedIngestionCursorId) ? (
            (() => {
              const cursor = ingestionCursorsQuery.data!.find((item) => item.ingestionCursorId === selectedIngestionCursorId)!
              return (
                <div className="space-y-2 text-sm text-slate-300">
                  <p><strong className="text-slate-100">Source connector:</strong> {cursor.sourceConnectorId}</p>
                  <p><strong className="text-slate-100">Source product:</strong> {cursor.sourceProduct}</p>
                  <p><strong className="text-slate-100">Cursor type:</strong> {cursor.cursorType}</p>
                  <p><strong className="text-slate-100">Cursor value:</strong> {cursor.cursorValue}</p>
                  <p><strong className="text-slate-100">Last event:</strong> {cursor.lastEventId ?? 'n/a'}</p>
                  <p><strong className="text-slate-100">Last event at:</strong> {formatDate(cursor.lastEventAt)}</p>
                  <p><strong className="text-slate-100">Last ingested at:</strong> {formatDate(cursor.lastIngestedAt)}</p>
                  <p><strong className="text-slate-100">Status:</strong> {cursor.status}</p>
                </div>
              )
            })()
          ) : (
            <EmptyState title="Select an ingestion cursor to inspect details." />
          )}
        </Panel>
        <Panel title="Widgets" icon={<BarChart3 className="h-4 w-4 text-cyan-300" />}>
          <div className="reportarr-stack">
            {(widgetsQuery.data ?? []).map((widget) => (
              <button
                key={widget.widgetId}
                type="button"
                className={['reportarr-row reportarr-row-button', widget.widgetId === selectedWidgetId ? 'active' : ''].join(' ')}
                onClick={() => setSelectedWidgetId(widget.widgetId)}
              >
                <div className="reportarr-row-main">
                  <strong>{widget.widgetKey}</strong>
                  <span>{widget.title}</span>
                  <small>{widget.datasetRef} · {widget.readModelRef}</small>
                  <small>{widget.freshnessStatus}</small>
                  <small>{widget.description}</small>
                  <small>{summarizeConfiguredField(widget.queryDefinition, 'query')}</small>
                  <small>{widget.filterBindings.join(', ') || 'No filter bindings'} · {widget.drilldownTargetRef}</small>
                  <small>order {widget.sortOrder} · {summarizeConfiguredField(widget.layout, 'layout')}</small>
                  <small>{summarizeConfiguredField(widget.visualizationSettings, 'visualization')}</small>
                  <small>last rendered {formatDate(widget.lastRenderedAt)}</small>
                </div>
                <div className="reportarr-row-meta">
                  <Pill>{widget.widgetType}</Pill>
                </div>
              </button>
            ))}
            {!widgetsQuery.data?.length ? <EmptyState title="No widgets yet." /> : null}
          </div>
        </Panel>
        <Panel title="Selected widget" icon={<BarChart3 className="h-4 w-4 text-cyan-300" />}>
          {widgetsQuery.data?.find((widget) => widget.widgetId === selectedWidgetId) ? (
            (() => {
              const widget = widgetsQuery.data!.find((item) => item.widgetId === selectedWidgetId)!
              const visualization = visualizationsQuery.data?.find((item) => item.widgetId === widget.widgetId)
              return (
                <div className="space-y-2 text-sm text-slate-300">
                  <p><strong className="text-slate-100">Widget key:</strong> {widget.widgetKey}</p>
                  <p><strong className="text-slate-100">Title:</strong> {widget.title}</p>
                  <p><strong className="text-slate-100">Description:</strong> {widget.description}</p>
                  <p><strong className="text-slate-100">Dashboard:</strong> {widget.dashboardId}</p>
                  <p><strong className="text-slate-100">Type:</strong> {widget.widgetType}</p>
                  <p><strong className="text-slate-100">Status:</strong> {widget.status}</p>
                  <p><strong className="text-slate-100">Dataset ref:</strong> {widget.datasetRef}</p>
                  <p><strong className="text-slate-100">Read model ref:</strong> {widget.readModelRef}</p>
                  <p><strong className="text-slate-100">Query definition:</strong> {summarizeConfiguredField(widget.queryDefinition, 'query')}</p>
                  <p><strong className="text-slate-100">Filter bindings:</strong> {widget.filterBindings.join(', ') || 'none'}</p>
                  <p><strong className="text-slate-100">Drilldown target:</strong> {widget.drilldownTargetRef}</p>
                  <p><strong className="text-slate-100">Sort order:</strong> {widget.sortOrder}</p>
                  <p><strong className="text-slate-100">Layout:</strong> {summarizeConfiguredField(widget.layout, 'layout')}</p>
                  <p><strong className="text-slate-100">Visualization settings:</strong> {summarizeConfiguredField(widget.visualizationSettings, 'visualization')}</p>
                  {visualization ? (
                    <>
                      <p><strong className="text-slate-100">Visualization chart:</strong> {visualization.chartType}</p>
                      <p><strong className="text-slate-100">Fields:</strong> {[visualization.xField, visualization.yField, visualization.seriesField, visualization.groupField, visualization.valueField, visualization.labelField, visualization.dateField].filter(Boolean).join(', ') || 'none'}</p>
                      <p><strong className="text-slate-100">Display format:</strong> {visualization.displayFormat}</p>
                      <p><strong className="text-slate-100">Legend:</strong> {visualization.showLegend ? 'yes' : 'no'}</p>
                      <p><strong className="text-slate-100">Data labels:</strong> {visualization.showDataLabels ? 'yes' : 'no'}</p>
                      <p><strong className="text-slate-100">Max rows:</strong> {visualization.maxRows}</p>
                    </>
                  ) : null}
                  <p><strong className="text-slate-100">Freshness:</strong> {widget.freshnessStatus}</p>
                  <p><strong className="text-slate-100">Last rendered:</strong> {formatDate(widget.lastRenderedAt)}</p>
                </div>
              )
            })()
          ) : (
            <EmptyState title="Select a widget to inspect details." />
          )}
        </Panel>
        <Panel title="Metrics" icon={<Gauge className="h-4 w-4 text-cyan-300" />}>
          <MetricDefinitionsList metrics={metricsQuery.data ?? []} selectedMetricId={selectedMetricId} onSelectMetric={setSelectedMetricId} />
        </Panel>
        <Panel title="Selected metric" icon={<Gauge className="h-4 w-4 text-cyan-300" />}>
          {metricsQuery.data?.find((metric) => metric.metricId === selectedMetricId) ? (
            (() => {
              const metric = metricsQuery.data!.find((item) => item.metricId === selectedMetricId)!
              return (
                <div className="space-y-2 text-sm text-slate-300">
                  <p><strong className="text-slate-100">Metric key:</strong> {metric.metricKey}</p>
                  <p><strong className="text-slate-100">Title:</strong> {metric.title}</p>
                  <p><strong className="text-slate-100">Description:</strong> {metric.description}</p>
                  <p><strong className="text-slate-100">Type:</strong> {metric.metricType}</p>
                  <p><strong className="text-slate-100">Status:</strong> {metric.status}</p>
                  <p><strong className="text-slate-100">Source dataset:</strong> {metric.sourceDatasetRef}</p>
                  <p><strong className="text-slate-100">Field refs:</strong> {metric.fieldRefs.join(', ') || 'none'}</p>
                  <p><strong className="text-slate-100">Formula:</strong> {summarizeConfiguredField(metric.formula, 'formula')}</p>
                  <p><strong className="text-slate-100">Filter definition:</strong> {summarizeConfiguredField(metric.filterDefinition, 'filter definition')}</p>
                  <p><strong className="text-slate-100">Grouping options:</strong> {summarizeConfiguredField(metric.groupingOptions, 'grouping options')}</p>
                  <p><strong className="text-slate-100">Date field:</strong> {metric.dateField}</p>
                </div>
              )
            })()
          ) : (
            <EmptyState title="Select a metric to inspect details." />
          )}
        </Panel>
      </div>
    </div>
  )
}

function MetricDefinitionsList({
  metrics,
  selectedMetricId = '',
  onSelectMetric = () => {},
}: {
  metrics: ReportArrMetricDefinitionResponse[]
  selectedMetricId?: string
  onSelectMetric?: (metricId: string) => void
}) {
  if (!metrics.length) return <EmptyState title="No metrics yet." />
  return (
    <div className="reportarr-stack">
      {metrics.map((metric) => (
        <button
          key={metric.metricId}
          type="button"
          className={['reportarr-row reportarr-row-button', metric.metricId === selectedMetricId ? 'active' : ''].join(' ')}
          onClick={() => onSelectMetric(metric.metricId)}
        >
          <div className="reportarr-row-main">
            <strong>{metric.metricKey}</strong>
            <span>{metric.title}</span>
            <small>{metric.sourceDatasetRef}</small>
          </div>
          <div className="reportarr-row-meta">
            <Pill>{metric.metricType}</Pill>
          </div>
        </button>
      ))}
    </div>
  )
}

function MetricValuesList({
  values,
  selectedMetricValueId = '',
  onSelectMetricValue = () => {},
}: {
  values: ReportArrMetricValueResponse[]
  selectedMetricValueId?: string
  onSelectMetricValue?: (metricValueId: string) => void
}) {
  if (!values.length) return <EmptyState title="No metric values yet." />
  return (
    <div className="reportarr-stack">
      {values.map((value) => (
        <button
          key={value.metricValueId}
          type="button"
          className={['reportarr-row reportarr-row-button', value.metricValueId === selectedMetricValueId ? 'active' : ''].join(' ')}
          onClick={() => onSelectMetricValue(value.metricValueId)}
        >
          <div className="reportarr-row-main">
            <strong>{value.metricId}</strong>
            <span>{value.value}</span>
            <small>{value.sourceTraceSummary}</small>
          </div>
          <div className="reportarr-row-meta">
            <Pill>{formatDate(value.calculatedAt)}</Pill>
          </div>
        </button>
      ))}
    </div>
  )
}

function EventsList({
  events,
  selectedEventId = '',
  onSelectEvent = () => {},
}: {
  events: ReportArrSourceEventReceiptResponse[]
  selectedEventId?: string
  onSelectEvent?: (eventId: string) => void
}) {
  if (!events.length) return <EmptyState title="No source events yet." />
  return (
    <div className="reportarr-stack">
      {events.map((event) => (
        <button
          key={event.sourceEventReceiptId}
          type="button"
          className={['reportarr-row reportarr-row-button', event.sourceEventReceiptId === selectedEventId ? 'active' : ''].join(' ')}
          onClick={() => onSelectEvent(event.sourceEventReceiptId)}
        >
          <div className="reportarr-row-main">
            <strong>{event.sourceEventId}</strong>
            <span>{event.eventType}</span>
            <small>{event.sourceProduct} · {formatDate(event.receivedAt)}</small>
            <small>object {event.sourceObjectRef ?? 'n/a'} · processed {formatDate(event.processedAt)}</small>
            <small>{event.status} · {event.failureReason ?? 'no failure'} · {event.correlationId ?? 'no correlation'}</small>
          </div>
          <div className="reportarr-row-meta">
            <Pill>{event.status}</Pill>
          </div>
        </button>
      ))}
    </div>
  )
}

function SourceEventDetail({ event }: { event: ReportArrSourceEventReceiptResponse | null }) {
  if (!event) return <EmptyState title="Select a source event to inspect details." />
  return (
    <div className="space-y-2 text-sm text-slate-300">
      <p><strong className="text-slate-100">Source event:</strong> {event.sourceEventId}</p>
      <p><strong className="text-slate-100">Receipt:</strong> {event.sourceEventReceiptId}</p>
      <p><strong className="text-slate-100">Source product:</strong> {event.sourceProduct}</p>
      <p><strong className="text-slate-100">Event type:</strong> {event.eventType}</p>
      <p><strong className="text-slate-100">Source object:</strong> {event.sourceObjectRef ?? 'n/a'}</p>
      <p><strong className="text-slate-100">Correlation:</strong> {event.correlationId ?? 'n/a'}</p>
      <p><strong className="text-slate-100">Received at:</strong> {formatDate(event.receivedAt)}</p>
      <p><strong className="text-slate-100">Processed at:</strong> {formatDate(event.processedAt)}</p>
      <p><strong className="text-slate-100">Status:</strong> {event.status}</p>
      <p><strong className="text-slate-100">Failure reason:</strong> {event.failureReason ?? 'n/a'}</p>
    </div>
  )
}

function HistoryPage({ accessToken, roleKey, isPlatformAdmin }: { accessToken: string; roleKey: string; isPlatformAdmin: boolean }) {
  const canViewSourceEvents = isPlatformAdmin || canUseReportArrAction(roleKey, isPlatformAdmin, ['analytics_admin', 'reportarr_admin', 'tenant_admin'])
  const reportRunsQuery = useQuery({
    queryKey: ['reportarr', 'report-runs'],
    queryFn: () => listReportRuns(accessToken),
    enabled: Boolean(accessToken),
  })
  const exportsQuery = useQuery({
    queryKey: ['reportarr', 'exports'],
    queryFn: () => listExportJobs(accessToken),
    enabled: Boolean(accessToken),
  })
  const sourceEventsQuery = useQuery({
    queryKey: ['reportarr', 'source-events'],
    queryFn: () => listSourceEvents(accessToken),
    enabled: Boolean(accessToken) && canViewSourceEvents,
  })
  const [selectedReportRunId, setSelectedReportRunId] = useState('')
  const [selectedExportJobId, setSelectedExportJobId] = useState('')
  const [selectedSourceEventId, setSelectedSourceEventId] = useState('')

  useEffect(() => {
    const runs = reportRunsQuery.data ?? []
    if (!runs.length) {
      setSelectedReportRunId('')
      return
    }
    if (!runs.some((run) => run.reportRunId === selectedReportRunId)) {
      setSelectedReportRunId(runs[0].reportRunId)
    }
  }, [reportRunsQuery.data, selectedReportRunId])

  useEffect(() => {
    const exports = exportsQuery.data ?? []
    if (!exports.length) {
      setSelectedExportJobId('')
      return
    }
    if (!exports.some((job) => job.exportJobId === selectedExportJobId)) {
      setSelectedExportJobId(exports[0].exportJobId)
    }
  }, [exportsQuery.data, selectedExportJobId])

  useEffect(() => {
    const events = sourceEventsQuery.data ?? []
    if (!events.length) {
      setSelectedSourceEventId('')
      return
    }
    if (!events.some((event) => event.sourceEventReceiptId === selectedSourceEventId)) {
      setSelectedSourceEventId(events[0].sourceEventReceiptId)
    }
  }, [selectedSourceEventId, sourceEventsQuery.data])

  return (
    <div className="reportarr-page">
      <SectionHeader
        eyebrow="History"
        title="Report and delivery history"
        description="Review generated reports, exports, and source events in a single audit-friendly timeline."
        action={<Pill><History className="h-4 w-4" /> Timeline</Pill>}
      />
      <div className="reportarr-grid cols-2">
        <Panel title="Report runs" icon={<PlayCircle className="h-4 w-4 text-cyan-300" />}>
          <ReportRunsList
            reportRuns={reportRunsQuery.data ?? []}
            selectedReportRunId={selectedReportRunId}
            onSelectReportRun={setSelectedReportRunId}
          />
        </Panel>
        <Panel title="Selected report run" icon={<PlayCircle className="h-4 w-4 text-cyan-300" />}>
          <ReportRunDetail reportRun={reportRunsQuery.data?.find((run) => run.reportRunId === selectedReportRunId) ?? null} />
        </Panel>
        <Panel title="Exports" icon={<FileText className="h-4 w-4 text-cyan-300" />}>
          <ExportJobsList
            exports={exportsQuery.data ?? []}
            selectedExportJobId={selectedExportJobId}
            onSelectExportJob={setSelectedExportJobId}
          />
        </Panel>
        <Panel title="Selected export job" icon={<FileText className="h-4 w-4 text-cyan-300" />}>
          <ExportJobDetail exportJob={exportsQuery.data?.find((job) => job.exportJobId === selectedExportJobId) ?? null} />
        </Panel>
        {canViewSourceEvents ? (
          <>
            <Panel title="Source events" icon={<Workflow className="h-4 w-4 text-cyan-300" />}>
              <EventsList
                events={sourceEventsQuery.data ?? []}
                selectedEventId={selectedSourceEventId}
                onSelectEvent={setSelectedSourceEventId}
              />
            </Panel>
            <Panel title="Selected source event" icon={<Workflow className="h-4 w-4 text-cyan-300" />}>
              <SourceEventDetail event={sourceEventsQuery.data?.find((event) => event.sourceEventReceiptId === selectedSourceEventId) ?? null} />
            </Panel>
          </>
        ) : (
          <Panel title="Source events" icon={<Workflow className="h-4 w-4 text-cyan-300" />}>
            <EmptyState title="Source event receipts are restricted to ReportArr admins." />
          </Panel>
        )}
      </div>
    </div>
  )
}

function SettingsPage({
  session,
  accessToken,
  me,
  roleKey,
  isPlatformAdmin,
}: {
  session: StoredReportArrSession | null
  accessToken: string
  me: ReportArrMeResponse | null
  roleKey: string
  isPlatformAdmin: boolean
}) {
  const canViewAccessPolicies = isPlatformAdmin || canUseReportArrAction(roleKey, isPlatformAdmin, ['reportarr_admin', 'tenant_admin'])
  const dashboardPoliciesQuery = useQuery({
    queryKey: ['reportarr', 'dashboard-access-policies'],
    queryFn: () => listDashboardAccessPolicies(accessToken),
    enabled: Boolean(accessToken) && canViewAccessPolicies,
  })
  const dashboardFiltersQuery = useQuery({
    queryKey: ['reportarr', 'dashboard-filters'],
    queryFn: () => listDashboardFilters(accessToken),
    enabled: Boolean(accessToken),
  })
  const drilldownsQuery = useQuery({
    queryKey: ['reportarr', 'drilldowns'],
    queryFn: () => listDrilldowns(accessToken),
    enabled: Boolean(accessToken),
  })
  const reportPoliciesQuery = useQuery({
    queryKey: ['reportarr', 'report-access-policies'],
    queryFn: () => listReportAccessPolicies(accessToken),
    enabled: Boolean(accessToken),
  })
  const [selectedDashboardPolicyId, setSelectedDashboardPolicyId] = useState('')
  const [selectedReportPolicyId, setSelectedReportPolicyId] = useState('')
  const [selectedDashboardFilterId, setSelectedDashboardFilterId] = useState('')
  const [selectedDrilldownId, setSelectedDrilldownId] = useState('')

  useEffect(() => {
    const policies = dashboardPoliciesQuery.data ?? []
    if (!policies.length) {
      setSelectedDashboardPolicyId('')
      return
    }
    if (!policies.some((policy) => policy.accessPolicyId === selectedDashboardPolicyId)) {
      setSelectedDashboardPolicyId(policies[0].accessPolicyId)
    }
  }, [dashboardPoliciesQuery.data, selectedDashboardPolicyId])

  useEffect(() => {
    const policies = reportPoliciesQuery.data ?? []
    if (!policies.length) {
      setSelectedReportPolicyId('')
      return
    }
    if (!policies.some((policy) => policy.accessPolicyId === selectedReportPolicyId)) {
      setSelectedReportPolicyId(policies[0].accessPolicyId)
    }
  }, [reportPoliciesQuery.data, selectedReportPolicyId])

  useEffect(() => {
    const filters = dashboardFiltersQuery.data ?? []
    if (!filters.length) {
      setSelectedDashboardFilterId('')
      return
    }
    if (!filters.some((filter) => filter.filterId === selectedDashboardFilterId)) {
      setSelectedDashboardFilterId(filters[0].filterId)
    }
  }, [dashboardFiltersQuery.data, selectedDashboardFilterId])

  useEffect(() => {
    const drilldowns = drilldownsQuery.data ?? []
    if (!drilldowns.length) {
      setSelectedDrilldownId('')
      return
    }
    if (!drilldowns.some((drilldown) => drilldown.drilldownId === selectedDrilldownId)) {
      setSelectedDrilldownId(drilldowns[0].drilldownId)
    }
  }, [drilldownsQuery.data, selectedDrilldownId])

  return (
    <div className="reportarr-page">
      <SectionHeader
        eyebrow="Settings"
        title="Workspace settings"
        description="Verify the current identity, tenant, API base, and launch wiring used by this ReportArr workspace."
        action={<Pill><Settings className="h-4 w-4" /> Local preview</Pill>}
      />
      <Panel title="Session details" icon={<ShieldCheck className="h-4 w-4 text-cyan-300" />}>
        <div className="space-y-2 text-sm text-slate-300">
          <p><strong className="text-slate-100">API base:</strong> {apiBase || '/api proxy'}</p>
          <p><strong className="text-slate-100">Preview port:</strong> 5185</p>
          <p><strong className="text-slate-100">Suite home:</strong> {suiteHomeUrl}</p>
          <p><strong className="text-slate-100">Access token:</strong> {accessToken ? 'present' : 'missing'}</p>
          <p><strong className="text-slate-100">Signed in as:</strong> {session?.displayName ?? 'signed out'}</p>
          <p><strong className="text-slate-100">Tenant:</strong> {session?.tenantDisplayName ?? 'n/a'}</p>
          <p><strong className="text-slate-100">Me endpoint:</strong> {me ? `${me.displayName} · ${me.productKey}` : 'n/a'}</p>
        </div>
      </Panel>
      <div className="reportarr-grid cols-2">
        {canViewAccessPolicies ? (
          <>
            <Panel title="Dashboard access policies" icon={<ShieldCheck className="h-4 w-4 text-cyan-300" />}>
              <div className="reportarr-stack">
                {(dashboardPoliciesQuery.data ?? []).map((policy) => (
                  <button
                    key={policy.accessPolicyId}
                    type="button"
                    className={['reportarr-row reportarr-row-button', policy.accessPolicyId === selectedDashboardPolicyId ? 'active' : ''].join(' ')}
                    onClick={() => setSelectedDashboardPolicyId(policy.accessPolicyId)}
                  >
                    <div className="reportarr-row-main">
                      <strong>{policy.visibility}</strong>
                      <span>{policy.dashboardId}</span>
                      <small>people {policy.allowedPersonRefs.join(', ') || 'none'}</small>
                      <small>roles {policy.allowedRoleRefs.join(', ') || 'none'}</small>
                      <small>permissions {policy.allowedPermissionRefs.join(', ') || 'none'}</small>
                      <small>products {policy.sourceProductRestrictions.join(', ') || 'none'}</small>
                      <small>generated {formatDate(policy.createdAt)} · updated {formatDate(policy.updatedAt)}</small>
                    </div>
                    <div className="reportarr-row-meta">
                      <Pill>{policy.exportAllowed ? 'export on' : 'export off'}</Pill>
                    </div>
                  </button>
                ))}
                {!dashboardPoliciesQuery.data?.length ? <EmptyState title="No dashboard access policies." /> : null}
              </div>
            </Panel>
            <Panel title="Selected dashboard policy" icon={<ShieldCheck className="h-4 w-4 text-cyan-300" />}>
              {dashboardPoliciesQuery.data?.find((policy) => policy.accessPolicyId === selectedDashboardPolicyId) ? (
                (() => {
                  const policy = dashboardPoliciesQuery.data!.find((item) => item.accessPolicyId === selectedDashboardPolicyId)!
                  return (
                    <div className="space-y-2 text-sm text-slate-300">
                      <p><strong className="text-slate-100">Dashboard:</strong> {policy.dashboardId}</p>
                      <p><strong className="text-slate-100">Visibility:</strong> {policy.visibility}</p>
                      <p><strong className="text-slate-100">Allowed people:</strong> {policy.allowedPersonRefs.join(', ') || 'none'}</p>
                      <p><strong className="text-slate-100">Allowed roles:</strong> {policy.allowedRoleRefs.join(', ') || 'none'}</p>
                      <p><strong className="text-slate-100">Allowed permissions:</strong> {policy.allowedPermissionRefs.join(', ') || 'none'}</p>
                      <p><strong className="text-slate-100">Source product restrictions:</strong> {policy.sourceProductRestrictions.join(', ') || 'none'}</p>
                      <p><strong className="text-slate-100">Export allowed:</strong> {policy.exportAllowed ? 'yes' : 'no'}</p>
                      <p><strong className="text-slate-100">Created at:</strong> {formatDate(policy.createdAt)}</p>
                      <p><strong className="text-slate-100">Updated at:</strong> {formatDate(policy.updatedAt)}</p>
                    </div>
                  )
                })()
              ) : (
                <EmptyState title="Select a dashboard access policy to inspect details." />
              )}
            </Panel>
            <Panel title="Report access policies" icon={<FileText className="h-4 w-4 text-cyan-300" />}>
              <div className="reportarr-stack">
                {(reportPoliciesQuery.data ?? []).map((policy) => (
                  <button
                    key={policy.accessPolicyId}
                    type="button"
                    className={['reportarr-row reportarr-row-button', policy.accessPolicyId === selectedReportPolicyId ? 'active' : ''].join(' ')}
                    onClick={() => setSelectedReportPolicyId(policy.accessPolicyId)}
                  >
                    <div className="reportarr-row-main">
                      <strong>{policy.visibility}</strong>
                      <span>{policy.reportDefinitionId}</span>
                      <small>people {policy.allowedPersonRefs.join(', ') || 'none'}</small>
                      <small>roles {policy.allowedRoleRefs.join(', ') || 'none'}</small>
                      <small>permissions {policy.allowedPermissionRefs.join(', ') || 'none'}</small>
                      <small>products {policy.sourceProductRestrictions.join(', ') || 'none'}</small>
                      <small>generated {formatDate(policy.createdAt)} · updated {formatDate(policy.updatedAt)}</small>
                    </div>
                    <div className="reportarr-row-meta">
                      <Pill>{policy.exportAllowed ? 'export on' : 'export off'}</Pill>
                      <Pill>{policy.scheduleAllowed ? 'schedule on' : 'schedule off'}</Pill>
                      <Pill>{policy.externalDeliveryAllowed ? 'delivery on' : 'delivery off'}</Pill>
                    </div>
                  </button>
                ))}
                {!reportPoliciesQuery.data?.length ? <EmptyState title="No report access policies." /> : null}
              </div>
            </Panel>
            <Panel title="Selected report policy" icon={<FileText className="h-4 w-4 text-cyan-300" />}>
              {reportPoliciesQuery.data?.find((policy) => policy.accessPolicyId === selectedReportPolicyId) ? (
                (() => {
                  const policy = reportPoliciesQuery.data!.find((item) => item.accessPolicyId === selectedReportPolicyId)!
                  return (
                    <div className="space-y-2 text-sm text-slate-300">
                      <p><strong className="text-slate-100">Report:</strong> {policy.reportDefinitionId}</p>
                      <p><strong className="text-slate-100">Visibility:</strong> {policy.visibility}</p>
                      <p><strong className="text-slate-100">Allowed people:</strong> {policy.allowedPersonRefs.join(', ') || 'none'}</p>
                      <p><strong className="text-slate-100">Allowed roles:</strong> {policy.allowedRoleRefs.join(', ') || 'none'}</p>
                      <p><strong className="text-slate-100">Allowed permissions:</strong> {policy.allowedPermissionRefs.join(', ') || 'none'}</p>
                      <p><strong className="text-slate-100">Source product restrictions:</strong> {policy.sourceProductRestrictions.join(', ') || 'none'}</p>
                      <p><strong className="text-slate-100">Export allowed:</strong> {policy.exportAllowed ? 'yes' : 'no'}</p>
                      <p><strong className="text-slate-100">Schedule allowed:</strong> {policy.scheduleAllowed ? 'yes' : 'no'}</p>
                      <p><strong className="text-slate-100">External delivery allowed:</strong> {policy.externalDeliveryAllowed ? 'yes' : 'no'}</p>
                      <p><strong className="text-slate-100">Created at:</strong> {formatDate(policy.createdAt)}</p>
                      <p><strong className="text-slate-100">Updated at:</strong> {formatDate(policy.updatedAt)}</p>
                    </div>
                  )
                })()
              ) : (
                <EmptyState title="Select a report access policy to inspect details." />
              )}
            </Panel>
          </>
        ) : (
          <Panel title="Access policies" icon={<ShieldCheck className="h-4 w-4 text-cyan-300" />}>
            <EmptyState title="Access policies are restricted to ReportArr admins." />
          </Panel>
        )}
        <Panel title="Dashboard filters" icon={<Layers3 className="h-4 w-4 text-cyan-300" />}>
          <div className="reportarr-stack">
            {(dashboardFiltersQuery.data ?? []).map((filter) => (
              <button
                key={filter.filterId}
                type="button"
                className={['reportarr-row reportarr-row-button', filter.filterId === selectedDashboardFilterId ? 'active' : ''].join(' ')}
                onClick={() => setSelectedDashboardFilterId(filter.filterId)}
              >
                <div className="reportarr-row-main">
                  <strong>{filter.label}</strong>
                  <span>{filter.filterType}</span>
                  <small>{filter.datasetFieldKey}</small>
                  <small>{filter.filterKey}</small>
                  <small>default {filter.defaultValue} · source {filter.allowedValuesSource}</small>
                  <small>{filter.visible ? 'visible' : 'hidden'} · {filter.required ? 'required' : 'optional'}</small>
                </div>
                <div className="reportarr-row-meta">
                  <Pill>{filter.required ? 'required' : 'optional'}</Pill>
                </div>
              </button>
            ))}
            {!dashboardFiltersQuery.data?.length ? <EmptyState title="No dashboard filters." /> : null}
          </div>
        </Panel>
        <Panel title="Selected dashboard filter" icon={<Layers3 className="h-4 w-4 text-cyan-300" />}>
          {dashboardFiltersQuery.data?.find((filter) => filter.filterId === selectedDashboardFilterId) ? (
            (() => {
              const filter = dashboardFiltersQuery.data!.find((item) => item.filterId === selectedDashboardFilterId)!
              return (
                <div className="space-y-2 text-sm text-slate-300">
                  <p><strong className="text-slate-100">Label:</strong> {filter.label}</p>
                  <p><strong className="text-slate-100">Key:</strong> {filter.filterKey}</p>
                  <p><strong className="text-slate-100">Type:</strong> {filter.filterType}</p>
                  <p><strong className="text-slate-100">Dashboard:</strong> {filter.dashboardId}</p>
                  <p><strong className="text-slate-100">Dataset field:</strong> {filter.datasetFieldKey}</p>
                  <p><strong className="text-slate-100">Default value:</strong> {filter.defaultValue || 'n/a'}</p>
                  <p><strong className="text-slate-100">Allowed values source:</strong> {filter.allowedValuesSource}</p>
                  <p><strong className="text-slate-100">Required:</strong> {filter.required ? 'yes' : 'no'}</p>
                  <p><strong className="text-slate-100">Visible:</strong> {filter.visible ? 'yes' : 'no'}</p>
                </div>
              )
            })()
          ) : (
            <EmptyState title="Select a dashboard filter to inspect details." />
          )}
        </Panel>
        <Panel title="Drilldowns" icon={<Workflow className="h-4 w-4 text-cyan-300" />}>
          <div className="reportarr-stack">
            {(drilldownsQuery.data ?? []).map((drilldown) => (
              <button
                key={drilldown.drilldownId}
                type="button"
                className={['reportarr-row reportarr-row-button', drilldown.drilldownId === selectedDrilldownId ? 'active' : ''].join(' ')}
                onClick={() => setSelectedDrilldownId(drilldown.drilldownId)}
              >
                <div className="reportarr-row-main">
                  <strong>{drilldown.title}</strong>
                  <span>{drilldown.targetType}</span>
                  <small>{drilldown.targetRef}</small>
                </div>
                <div className="reportarr-row-meta">
                  <Pill>{drilldown.status}</Pill>
                </div>
              </button>
            ))}
            {!drilldownsQuery.data?.length ? <EmptyState title="No drilldowns." /> : null}
          </div>
        </Panel>
        <Panel title="Selected drilldown" icon={<Workflow className="h-4 w-4 text-cyan-300" />}>
          {drilldownsQuery.data?.find((drilldown) => drilldown.drilldownId === selectedDrilldownId) ? (
            (() => {
              const drilldown = drilldownsQuery.data!.find((item) => item.drilldownId === selectedDrilldownId)!
              return (
                <div className="space-y-2 text-sm text-slate-300">
                  <p><strong className="text-slate-100">Title:</strong> {drilldown.title}</p>
                  <p><strong className="text-slate-100">Description:</strong> {drilldown.description}</p>
                  <p><strong className="text-slate-100">Source widget:</strong> {drilldown.sourceWidgetRef}</p>
                  <p><strong className="text-slate-100">Target type:</strong> {drilldown.targetType}</p>
                  <p><strong className="text-slate-100">Target ref:</strong> {drilldown.targetRef}</p>
                  <p><strong className="text-slate-100">Parameter mappings:</strong> {drilldown.parameterMappings.join(', ') || 'none'}</p>
                  <p><strong className="text-slate-100">Required permissions:</strong> {drilldown.requiredPermissionRefs.join(', ') || 'none'}</p>
                  <p><strong className="text-slate-100">Status:</strong> {drilldown.status}</p>
                </div>
              )
            })()
          ) : (
            <EmptyState title="Select a drilldown to inspect details." />
          )}
        </Panel>
      </div>
    </div>
  )
}

function ReportRunDetailPage({ accessToken }: { accessToken: string }) {
  const { reportRunId } = useParams<{ reportRunId: string }>()
  const query = useQuery({
    queryKey: ['reportarr', 'report-runs', reportRunId, accessToken],
    queryFn: () => getReportRun(accessToken, reportRunId!),
    enabled: Boolean(accessToken) && Boolean(reportRunId),
  })

  if (!reportRunId) {
    return <Navigate to="/reports" replace />
  }

  if (query.isLoading) {
    return <div className="reportarr-page">Loading report run…</div>
  }

  const reportRun = query.data ?? null

  return (
    <div className="reportarr-page">
      <SectionHeader
        eyebrow="Reports"
        title="Report run detail"
        description={`Inspect a single report run (${reportRunId}).`}
      />
      <Panel title="Report run detail">
        <ReportRunDetail reportRun={reportRun} />
      </Panel>
    </div>
  )
}

function ReportScheduleDetailPage({ accessToken }: { accessToken: string }) {
  const { scheduleId } = useParams<{ scheduleId: string }>()
  const query = useQuery({
    queryKey: ['reportarr', 'report-schedules', scheduleId, accessToken],
    queryFn: () => listReportSchedules(accessToken),
    enabled: Boolean(accessToken) && Boolean(scheduleId),
  })
  const recipientsQuery = useQuery({
    queryKey: ['reportarr', 'report-recipients', scheduleId, accessToken],
    queryFn: () => listReportRecipients(accessToken),
    enabled: Boolean(accessToken) && Boolean(scheduleId),
  })

  if (!scheduleId) {
    return <Navigate to="/reports" replace />
  }

  if (query.isLoading) {
    return <div className="reportarr-page">Loading schedule…</div>
  }

  const reportSchedule = query.data?.find((item) => item.scheduleId === scheduleId) ?? null

  return (
    <div className="reportarr-page">
      <SectionHeader
        eyebrow="Reports"
        title="Report schedule detail"
        description={`Inspect a single schedule (${scheduleId}).`}
      />
      <Panel title="Report schedule detail">
        <ReportScheduleDetail
          schedule={reportSchedule}
          recipients={(recipientsQuery.data ?? []).filter((recipient) => recipient.scheduleId === scheduleId)}
        />
      </Panel>
    </div>
  )
}

function ExportJobDetailPage({ accessToken }: { accessToken: string }) {
  const { exportJobId } = useParams<{ exportJobId: string }>()
  const query = useQuery({
    queryKey: ['reportarr', 'export-jobs', exportJobId, accessToken],
    queryFn: () => getExportJob(accessToken, exportJobId!),
    enabled: Boolean(accessToken) && Boolean(exportJobId),
  })

  if (!exportJobId) {
    return <Navigate to="/reports" replace />
  }

  if (query.isLoading) {
    return <div className="reportarr-page">Loading export job…</div>
  }

  const exportJob = query.data ?? null

  return (
    <div className="reportarr-page">
      <SectionHeader
        eyebrow="Reports"
        title="Export job detail"
        description={`Inspect a single export job (${exportJobId}).`}
      />
      <Panel title="Export job detail">
        <ExportJobDetail exportJob={exportJob} />
      </Panel>
    </div>
  )
}

function DatasetDetailPage({ accessToken }: { accessToken: string }) {
  const { datasetId } = useParams<{ datasetId: string }>()
  const query = useQuery({
    queryKey: ['reportarr', 'datasets', datasetId, accessToken],
    queryFn: () => getDataset(accessToken, datasetId!),
    enabled: Boolean(accessToken) && Boolean(datasetId),
  })
  const fieldsQuery = useQuery({
    queryKey: ['reportarr', 'dataset-fields', datasetId, accessToken],
    queryFn: () => listDatasetFields(accessToken),
    enabled: Boolean(accessToken) && Boolean(datasetId),
  })
  const lineageQuery = useQuery({
    queryKey: ['reportarr', 'dataset-lineage', datasetId, accessToken],
    queryFn: () => listDatasetLineage(accessToken),
    enabled: Boolean(accessToken) && Boolean(datasetId),
  })
  const refreshJobsQuery = useQuery({
    queryKey: ['reportarr', 'refresh-jobs', accessToken],
    queryFn: () => listRefreshJobs(accessToken),
    enabled: Boolean(accessToken) && Boolean(datasetId),
  })
  const readModelsQuery = useQuery({
    queryKey: ['reportarr', 'read-models', accessToken],
    queryFn: () => listReadModels(accessToken),
    enabled: Boolean(accessToken) && Boolean(datasetId),
  })
  const sourceEventsQuery = useQuery({
    queryKey: ['reportarr', 'source-events', accessToken],
    queryFn: () => listSourceEvents(accessToken),
    enabled: Boolean(accessToken) && Boolean(datasetId),
  })
  const dashboardsQuery = useQuery({
    queryKey: ['reportarr', 'dashboards', accessToken],
    queryFn: () => listDashboards(accessToken),
    enabled: Boolean(accessToken) && Boolean(datasetId),
  })
  const reportDefinitionsQuery = useQuery({
    queryKey: ['reportarr', 'report-definitions', accessToken],
    queryFn: () => listReportDefinitions(accessToken),
    enabled: Boolean(accessToken) && Boolean(datasetId),
  })
  const widgetsQuery = useQuery({
    queryKey: ['reportarr', 'widgets', accessToken],
    queryFn: () => listWidgets(accessToken),
    enabled: Boolean(accessToken) && Boolean(datasetId),
  })

  if (!datasetId) {
    return <Navigate to="/datasets" replace />
  }

  if (query.isLoading) {
    return <div className="reportarr-page">Loading dataset…</div>
  }

  const dataset = query.data ?? null
  const datasetFields = fieldsQuery.data?.filter((field) => field.datasetId === datasetId) ?? []
  const datasetLineage = lineageQuery.data?.filter((lineage) => lineage.datasetId === datasetId) ?? []
  const datasetRefreshJobs = (refreshJobsQuery.data ?? [])
    .filter((job) => job.datasetId === datasetId)
    .sort((a, b) => new Date(b.queuedAt).getTime() - new Date(a.queuedAt).getTime())
  const ingestionErrors = (sourceEventsQuery.data ?? [])
    .filter((event) => event.sourceObjectRef === datasetId && event.status === 'failed')
    .sort((a, b) => new Date(b.receivedAt).getTime() - new Date(a.receivedAt).getTime())
  const readModels = (readModelsQuery.data ?? []).filter((model) => model.datasetRefs.includes(datasetId))
  const datasetWidgetRefs = (widgetsQuery.data ?? []).filter((widget) => widget.datasetRef === datasetId).map((widget) => widget.widgetId)
  const dependentDashboards =
    dashboardsQuery.data?.filter((dashboard) => dashboard.widgetRefs.some((ref) => datasetWidgetRefs.includes(ref))) ?? []
  const dependentReports =
    reportDefinitionsQuery.data?.filter((report) => report.datasetRefs.includes(datasetId)) ?? []
  const datasetFieldCount = datasetFields.length
  const datasetRefreshJobCount = datasetRefreshJobs.length
  const readModelCount = readModels.length
  const dependentDashboardCount = dependentDashboards.length
  const dependentReportCount = dependentReports.length
  const freshnessTone: DetailTone = dataset?.freshnessStatus?.toLowerCase().includes('fresh')
    ? 'good'
    : dataset?.freshnessStatus?.toLowerCase().includes('stale')
      ? 'warn'
      : 'info'
  const statusTone: DetailTone = dataset?.status === 'active' ? 'good' : dataset?.status === 'failed' ? 'bad' : 'neutral'

  return (
    <ReportDetailShell
      backLabel="Datasets"
      backTo="/datasets"
      breadcrumbs={dataset ? [dataset.datasetKey, dataset.title] : ['Dataset detail']}
      icon={<Database className="h-8 w-8" />}
      title={dataset?.title ?? 'Dataset detail'}
      subtitle={`Inspect a single dataset (${datasetId}).`}
      badges={[
        { label: dataset?.status ?? 'Unknown', tone: statusTone },
        { label: dataset?.freshnessStatus ?? 'Unknown', tone: freshnessTone },
      ]}
      metrics={[
        {
          label: 'Fields',
          value: datasetFieldCount,
          hint: 'Schema fields defined for this dataset',
          icon: <FileText className="h-5 w-5" />,
          tone: datasetFieldCount > 0 ? 'info' : 'neutral',
        },
        {
          label: 'Refresh jobs',
          value: datasetRefreshJobCount,
          hint: 'Recent refresh activity',
          icon: <RefreshCcw className="h-5 w-5" />,
          tone: datasetRefreshJobCount > 0 ? 'info' : 'neutral',
        },
        {
          label: 'Read models',
          value: readModelCount,
          hint: 'Downstream read models using this dataset',
          icon: <Gauge className="h-5 w-5" />,
          tone: readModelCount > 0 ? 'good' : 'neutral',
        },
        {
          label: 'Dashboards',
          value: dependentDashboardCount,
          hint: 'Dashboards driven by this dataset',
          icon: <BarChart3 className="h-5 w-5" />,
          tone: dependentDashboardCount > 0 ? 'good' : 'neutral',
        },
      ]}
      snapshotTitle="Dataset snapshot"
      snapshotSubtitle="Configuration, source traceability, and freshness state."
      snapshotFields={[
        { label: 'Dataset number', value: dataset?.datasetNumber ?? 'n/a', source: 'ReportArr dataset' },
        { label: 'Dataset key', value: dataset?.datasetKey ?? 'n/a', source: 'ReportArr dataset' },
        { label: 'Type', value: dataset?.datasetType ?? 'n/a', source: 'ReportArr dataset' },
        { label: 'Refresh mode', value: `${dataset?.refreshMode ?? 'n/a'} · ${dataset?.refreshFrequency ?? 'n/a'}`, source: 'ReportArr dataset' },
        { label: 'Freshness', value: dataset?.freshnessStatus ?? 'n/a', source: 'Calculated state' },
        { label: 'Last refreshed', value: formatDate(dataset?.lastRefreshedAt ?? null), source: 'ReportArr refresh' },
        { label: 'Last successful refresh', value: formatDate(dataset?.lastSuccessfulRefreshAt ?? null), source: 'ReportArr refresh' },
        { label: 'Last failed refresh', value: formatDate(dataset?.lastFailedRefreshAt ?? null), source: 'ReportArr refresh' },
        { label: 'Source products', value: dataset?.sourceProducts.join(', ') || 'none', source: 'Source trace' },
        { label: 'Source connectors', value: dataset?.sourceConnectors.join(', ') || 'none', source: 'Source trace' },
        { label: 'Owner', value: dataset?.ownerPersonId ?? 'n/a', source: 'ReportArr ownership' },
      ]}
      decisionTitle="Data freshness decision"
      decisionBadge={{ label: dataset?.freshnessStatus ?? 'Unknown', tone: freshnessTone }}
      decisionIcon={<Database className="h-5 w-5 text-sky-300" />}
      decisionSummary={dataset?.freshnessStatus?.toLowerCase().includes('fresh')
        ? 'Dataset refresh state is current.'
        : dataset?.freshnessStatus?.toLowerCase().includes('stale')
          ? 'Dataset refresh state needs attention.'
          : 'Dataset freshness state is informational.'}
      decisionDetail={dataset
        ? `Source traceability is ${dataset.sourceTraceabilityRules ? 'defined' : 'not defined'}, with ${datasetFieldCount} field definitions and ${datasetRefreshJobCount} refresh jobs tracked in ReportArr.`
        : 'No dataset record was found.'}
      allowedChecks={Math.max(0, readModelCount + dependentDashboardCount)}
      blockedChecks={ingestionErrors.length}
      railSections={[
        {
          title: 'Source traceability',
          icon: <ShieldCheck className="h-5 w-5" />,
          content: dataset ? (
            <div className="space-y-3 text-sm text-slate-300">
              <p><strong className="text-slate-100">Traceability rules:</strong> {dataset.sourceTraceabilityRules || 'none'}</p>
              <p><strong className="text-slate-100">Schema version:</strong> {dataset.schemaVersion}</p>
              <p><strong className="text-slate-100">Retention policy:</strong> {dataset.retentionPolicy}</p>
              <p><strong className="text-slate-100">Field definitions:</strong> {dataset.fieldDefinitions.join(', ') || 'none'}</p>
            </div>
          ) : (
            <DetailEmptyState text="No dataset metadata is available." />
          ),
        },
        {
          title: 'Downstream consumers',
          icon: <PlayCircle className="h-5 w-5" />,
          content: (
            <div className="space-y-3 text-sm text-slate-300">
              <p><strong className="text-slate-100">Dashboards:</strong> {dependentDashboardCount}</p>
              <p><strong className="text-slate-100">Reports:</strong> {dependentReportCount}</p>
              <p><strong className="text-slate-100">Read models:</strong> {readModelCount}</p>
            </div>
          ),
        },
      ]}
      mainContent={
        <div className="space-y-6">
          <section className="rounded-xl border border-slate-800 bg-slate-950/50 p-4">
            <div className="flex items-start justify-between gap-3">
              <div>
                <h3 className="text-lg font-semibold text-white">Schema and fields</h3>
                <p className="mt-1 text-sm text-slate-400">Field definitions and source paths attached to this dataset.</p>
              </div>
              <Pill>{datasetFieldCount > 0 ? `${datasetFieldCount} fields` : 'No fields'}</Pill>
            </div>
            <div className="mt-4 space-y-3">
              {datasetFields.length ? (
                datasetFields.map((field) => (
                  <div key={field.fieldId} className="rounded-lg border border-slate-800 bg-slate-900/70 p-3 text-sm text-slate-300">
                    <p className="font-medium text-white">{field.fieldKey}</p>
                    <p className="mt-1 text-slate-400">
                      {field.dataType} · {field.sourceProduct}.{field.sourceFieldPath}
                    </p>
                  </div>
                ))
              ) : (
                <DetailEmptyState text="No dataset fields are defined." />
              )}
            </div>
          </section>

          <div className="grid gap-6 xl:grid-cols-2">
            <section className="rounded-xl border border-slate-800 bg-slate-950/50 p-4">
              <div className="flex items-start justify-between gap-3">
                <div>
                  <h3 className="text-lg font-semibold text-white">Refresh history</h3>
                  <p className="mt-1 text-sm text-slate-400">Recent refresh jobs and their outcomes.</p>
                </div>
                <Pill>{datasetRefreshJobCount > 0 ? `${datasetRefreshJobCount} jobs` : 'No jobs'}</Pill>
              </div>
              <div className="mt-4 space-y-3">
                {datasetRefreshJobs.length ? (
                  datasetRefreshJobs.slice(0, 5).map((job) => (
                    <div key={job.refreshJobId} className="rounded-lg border border-slate-800 bg-slate-900/70 p-3 text-sm text-slate-300">
                      <p className="font-medium text-white">
                        <Link className="text-cyan-300 underline" to={`/refresh-jobs/${job.refreshJobId}`}>
                          {job.status}
                        </Link>
                      </p>
                      <p className="mt-1 text-slate-400">
                        queued {formatDate(job.queuedAt)} · started {formatDate(job.startedAt)} · records created {formatNumber(job.recordsCreated)}
                      </p>
                    </div>
                  ))
                ) : (
                  <DetailEmptyState text="No refresh jobs have been recorded for this dataset." />
                )}
              </div>
            </section>

            <section className="rounded-xl border border-slate-800 bg-slate-950/50 p-4">
              <div className="flex items-start justify-between gap-3">
                <div>
                  <h3 className="text-lg font-semibold text-white">Lineage and errors</h3>
                  <p className="mt-1 text-sm text-slate-400">Upstream lineage records and failed ingestion events.</p>
                </div>
                <Pill>{ingestionErrors.length > 0 ? `${ingestionErrors.length} errors` : 'No errors'}</Pill>
              </div>
              <div className="mt-4 space-y-3">
                {datasetLineage.length ? (
                  datasetLineage.map((lineage) => (
                    <div key={lineage.lineageId} className="rounded-lg border border-slate-800 bg-slate-900/70 p-3 text-sm text-slate-300">
                      <p className="font-medium text-white">
                        {lineage.sourceProduct}.{lineage.sourceObjectType} → {lineage.datasetFieldKey}
                      </p>
                      <p className="mt-1 text-slate-400">{lineage.transformationDescription}</p>
                    </div>
                  ))
                ) : (
                  <DetailEmptyState text="No lineage records are available." />
                )}
                {ingestionErrors.length ? (
                  ingestionErrors.slice(0, 5).map((event) => (
                    <div key={event.sourceEventReceiptId} className="rounded-lg border border-amber-500/30 bg-amber-500/10 p-3 text-sm text-slate-200">
                      <p className="font-medium text-white">
                        <Link className="text-cyan-300 underline" to={`/history/events/${event.sourceEventReceiptId}`}>
                          {event.sourceObjectRef}
                        </Link>
                      </p>
                      <p className="mt-1 text-slate-300">
                        {event.eventType} · {formatDate(event.receivedAt)}
                      </p>
                    </div>
                  ))
                ) : (
                  <DetailEmptyState text="No ingestion errors are currently associated with this dataset." />
                )}
              </div>
            </section>
          </div>

          <div className="grid gap-6 xl:grid-cols-2">
            <section className="rounded-xl border border-slate-800 bg-slate-950/50 p-4">
              <div className="flex items-start justify-between gap-3">
                <div>
                  <h3 className="text-lg font-semibold text-white">Read models</h3>
                  <p className="mt-1 text-sm text-slate-400">Read models that currently depend on this dataset.</p>
                </div>
                <Pill>{readModelCount > 0 ? `${readModelCount} models` : 'No models'}</Pill>
              </div>
              <div className="mt-4 space-y-3">
                {readModels.length ? (
                  readModels.map((readModel) => (
                    <div key={readModel.readModelId} className="rounded-lg border border-slate-800 bg-slate-900/70 p-3 text-sm text-slate-300">
                      <Link className="font-medium text-cyan-300 underline" to={`/read-models/${readModel.readModelId}`}>
                        {readModel.title}
                      </Link>
                      <p className="mt-1 text-slate-400">{readModel.readModelType} · {readModel.status}</p>
                    </div>
                  ))
                ) : (
                  <DetailEmptyState text="No downstream read models are linked." />
                )}
              </div>
            </section>

            <section className="rounded-xl border border-slate-800 bg-slate-950/50 p-4">
              <div className="flex items-start justify-between gap-3">
                <div>
                  <h3 className="text-lg font-semibold text-white">Dependent dashboards and reports</h3>
                  <p className="mt-1 text-sm text-slate-400">Objects that consume this dataset through widgets or dataset refs.</p>
                </div>
                <Pill>{dependentDashboardCount + dependentReportCount > 0 ? `${dependentDashboardCount + dependentReportCount} consumers` : 'No consumers'}</Pill>
              </div>
              <div className="mt-4 space-y-4">
                <div>
                  <p className="text-sm font-semibold text-white">Dashboards</p>
                  <div className="mt-2 space-y-2">
                    {dependentDashboards.length ? (
                      dependentDashboards.map((dashboard) => (
                        <div key={dashboard.dashboardId} className="rounded-lg border border-slate-800 bg-slate-900/70 p-3 text-sm text-slate-300">
                          <Link className="text-cyan-300 underline" to={`/dashboards/${dashboard.dashboardId}`}>
                            {dashboard.title}
                          </Link>
                        </div>
                      ))
                    ) : (
                      <DetailEmptyState text="No dashboards currently consume this dataset." />
                    )}
                  </div>
                </div>
                <div>
                  <p className="text-sm font-semibold text-white">Reports</p>
                  <div className="mt-2 space-y-2">
                    {dependentReports.length ? (
                      dependentReports.map((report) => (
                        <div key={report.reportDefinitionId} className="rounded-lg border border-slate-800 bg-slate-900/70 p-3 text-sm text-slate-300">
                          <p className="text-white">{report.title}</p>
                        </div>
                      ))
                    ) : (
                      <DetailEmptyState text="No reports currently consume this dataset." />
                    )}
                  </div>
                </div>
              </div>
            </section>
          </div>
        </div>
      }
    />
  )
}

function DashboardDetailPage({ accessToken }: { accessToken: string }) {
  const { dashboardId } = useParams<{ dashboardId: string }>()
  const query = useQuery({
    queryKey: ['reportarr', 'dashboards', dashboardId, accessToken],
    queryFn: () => getDashboard(accessToken, dashboardId!),
    enabled: Boolean(accessToken) && Boolean(dashboardId),
  })
  const queryClient = useQueryClient()
  const policiesQuery = useQuery({
    queryKey: ['reportarr', 'dashboard-access-policies', accessToken],
    queryFn: () => listDashboardAccessPolicies(accessToken),
    enabled: Boolean(accessToken) && Boolean(dashboardId),
  })
  const filtersQuery = useQuery({
    queryKey: ['reportarr', 'dashboard-filters', accessToken],
    queryFn: () => listDashboardFilters(accessToken),
    enabled: Boolean(accessToken) && Boolean(dashboardId),
  })
  const drilldownsQuery = useQuery({
    queryKey: ['reportarr', 'drilldowns', accessToken],
    queryFn: () => listDrilldowns(accessToken),
    enabled: Boolean(accessToken) && Boolean(dashboardId),
  })
  const widgetsQuery = useQuery({
    queryKey: ['reportarr', 'widgets', accessToken],
    queryFn: () => listWidgets(accessToken),
    enabled: Boolean(accessToken) && Boolean(dashboardId),
  })
  const exportMutation = useMutation({
    mutationFn: () =>
      createExport(accessToken, {
        reportRunId: null,
        exportType: 'dashboard',
        sourceRef: dashboardId!,
        exportFormat: 'pdf',
        requestedByPersonId: query.data?.ownerPersonId ?? 'person-ops-analyst',
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['reportarr'] })
    },
  })

  if (!dashboardId) {
    return <Navigate to="/dashboards" replace />
  }

  if (query.isLoading) {
    return <div className="reportarr-page">Loading dashboard…</div>
  }

  const dashboard = query.data ?? null
  const dashboardPolicy = policiesQuery.data?.find((item) => item.dashboardId === dashboardId) ?? null
  const dashboardFilters = filtersQuery.data?.filter((item) => item.dashboardId === dashboardId) ?? []
  const dashboardDrilldowns = drilldownsQuery.data?.filter((item) => item.dashboardId === dashboardId) ?? []
  const dashboardWidgets =
    (widgetsQuery.data ?? [])
      .filter((item) => (dashboard ? dashboard.widgetRefs.includes(item.widgetId) : false))
      .sort((a, b) => a.sortOrder - b.sortOrder)
  const sourceDatasetRefs = [...new Set(dashboardWidgets.map((widget) => widget.datasetRef).filter(Boolean))]
  const sourceReadModelRefs = [...new Set(dashboardWidgets.map((widget) => widget.readModelRef).filter(Boolean))]
  const dashboardWidgetCount = dashboardWidgets.length
  const dashboardFilterCount = dashboardFilters.length
  const dashboardDrilldownCount = dashboardDrilldowns.length
  const freshnessTone: DetailTone = dashboard?.freshnessStatus?.toLowerCase().includes('fresh')
    ? 'good'
    : dashboard?.freshnessStatus?.toLowerCase().includes('stale')
      ? 'warn'
      : 'info'
  const exportAllowed = dashboardPolicy?.exportAllowed ?? false

  return (
    <ReportDetailShell
      backLabel="Dashboards"
      backTo="/dashboards"
      breadcrumbs={dashboard ? [dashboard.dashboardKey, dashboard.title] : ['Dashboard detail']}
      icon={<BarChart3 className="h-8 w-8" />}
      title={dashboard?.title ?? 'Dashboard detail'}
      subtitle={`Inspect a single dashboard (${dashboardId}).`}
      badges={[
        { label: dashboard?.status ?? 'Unknown', tone: dashboard?.status === 'active' ? 'good' : dashboard?.status === 'draft' ? 'warn' : 'neutral' },
        { label: dashboard?.freshnessStatus ?? 'Unknown', tone: freshnessTone },
      ]}
      actions={dashboard ? (
        <>
          <button
            className="reportarr-button"
            type="button"
            onClick={() => exportMutation.mutate()}
            disabled={exportMutation.isPending || !exportAllowed}
          >
            {exportMutation.isPending ? 'Exporting…' : 'Export dashboard'}
          </button>
          {!exportAllowed ? <small className="ml-2 self-center text-xs text-amber-200">Export blocked by policy.</small> : null}
        </>
      ) : undefined}
      metrics={[
        {
          label: 'Widgets',
          value: dashboardWidgetCount,
          hint: 'Linked widget definitions on this dashboard',
          icon: <Layers3 className="h-5 w-5" />,
          tone: dashboardWidgetCount > 0 ? 'info' : 'neutral',
        },
        {
          label: 'Filters',
          value: dashboardFilterCount,
          hint: 'Supported dashboard filters',
          icon: <Gauge className="h-5 w-5" />,
          tone: dashboardFilterCount > 0 ? 'info' : 'neutral',
        },
        {
          label: 'Drilldowns',
          value: dashboardDrilldownCount,
          hint: 'Available drill-in targets',
          icon: <PlayCircle className="h-5 w-5" />,
          tone: dashboardDrilldownCount > 0 ? 'info' : 'neutral',
        },
        {
          label: 'Source traces',
          value: sourceDatasetRefs.length + sourceReadModelRefs.length,
          hint: 'Datasets and read models referenced by widgets',
          icon: <Database className="h-5 w-5" />,
          tone: sourceDatasetRefs.length + sourceReadModelRefs.length > 0 ? 'good' : 'neutral',
        },
      ]}
      snapshotTitle="Dashboard snapshot"
      snapshotSubtitle="Owned configuration and source trace context."
      snapshotFields={[
        { label: 'Dashboard number', value: dashboard?.dashboardNumber ?? 'n/a', source: 'ReportArr dashboard' },
        { label: 'Dashboard key', value: dashboard?.dashboardKey ?? 'n/a', source: 'ReportArr dashboard' },
        { label: 'Type', value: dashboard?.dashboardType ?? 'n/a', source: 'ReportArr dashboard' },
        { label: 'Default range', value: dashboard?.defaultDateRange ?? 'n/a', source: 'ReportArr dashboard' },
        { label: 'Freshness', value: dashboard?.freshnessStatus ?? 'n/a', source: 'Calculated state' },
        { label: 'Widget refs', value: dashboard?.widgetRefs.join(', ') || 'none', source: 'ReportArr dashboard' },
        { label: 'Filter refs', value: dashboard?.filterRefs.join(', ') || 'none', source: 'ReportArr dashboard' },
        { label: 'Drilldown refs', value: dashboard?.drilldownRefs.join(', ') || 'none', source: 'ReportArr dashboard' },
        { label: 'Last viewed', value: formatDate(dashboard?.lastViewedAt ?? null), source: 'ReportArr activity' },
      ]}
      decisionTitle="Access decision"
      decisionBadge={{ label: exportAllowed ? 'Export allowed' : 'Export blocked', tone: exportAllowed ? 'good' : 'warn' }}
      decisionIcon={exportAllowed ? <CheckCircle2 className="h-5 w-5 text-emerald-300" /> : <AlertTriangle className="h-5 w-5 text-amber-300" />}
      decisionSummary={exportAllowed ? 'Dashboard exports are available.' : 'Dashboard export is currently blocked.'}
      decisionDetail={dashboardPolicy
        ? `Visibility is ${dashboardPolicy.visibility || 'not set'} and access is limited to the configured roles and people.`
        : 'No access policy record was returned for this dashboard.'}
      allowedChecks={exportAllowed ? 3 : 1}
      blockedChecks={exportAllowed ? 0 : 2}
      railSections={[
        {
          title: 'Access policy',
          icon: <ShieldCheck className="h-5 w-5" />,
          content: dashboardPolicy ? (
            <div className="space-y-3 text-sm text-slate-300">
              <p><strong className="text-slate-100">Visibility:</strong> {dashboardPolicy.visibility || 'none'}</p>
              <p><strong className="text-slate-100">Export allowed:</strong> {dashboardPolicy.exportAllowed ? 'yes' : 'no'}</p>
              <p><strong className="text-slate-100">Allowed roles:</strong> {dashboardPolicy.allowedRoleRefs.join(', ') || 'none'}</p>
              <p><strong className="text-slate-100">Allowed persons:</strong> {dashboardPolicy.allowedPersonRefs.join(', ') || 'none'}</p>
              <p><strong className="text-slate-100">Source restrictions:</strong> {dashboardPolicy.sourceProductRestrictions.join(', ') || 'none'}</p>
            </div>
          ) : (
            <DetailEmptyState text="No access policy is available for this dashboard." />
          ),
        },
        {
          title: 'Source trace',
          icon: <Database className="h-5 w-5" />,
          content: (
            <div className="space-y-3 text-sm text-slate-300">
              <p><strong className="text-slate-100">Datasets:</strong> {sourceDatasetRefs.join(', ') || 'none'}</p>
              <p><strong className="text-slate-100">Read models:</strong> {sourceReadModelRefs.join(', ') || 'none'}</p>
              <p><strong className="text-slate-100">Widgets:</strong> {dashboardWidgets.map((widget) => widget.widgetId).join(', ') || 'none'}</p>
            </div>
          ),
        },
      ]}
      mainContent={
        <div className="space-y-6">
          <section className="rounded-xl border border-slate-800 bg-slate-950/50 p-4">
            <div className="flex items-start justify-between gap-3">
              <div>
                <h3 className="text-lg font-semibold text-white">Widget grid</h3>
                <p className="mt-1 text-sm text-slate-400">Widgets, source bindings, and current status for this dashboard.</p>
              </div>
              <Pill>{dashboardWidgetCount > 0 ? `${dashboardWidgetCount} widgets` : 'No widgets'}</Pill>
            </div>
            <div className="mt-4 space-y-3">
              {dashboardWidgets.length ? (
                dashboardWidgets.map((widget) => (
                  <div key={widget.widgetId} className="rounded-lg border border-slate-800 bg-slate-900/70 p-3 text-sm text-slate-300">
                    <p className="font-medium text-white">{widget.title}</p>
                    <p className="mt-1 text-slate-400">
                      {widget.widgetType} · {widget.status}
                    </p>
                    <p className="mt-1 text-xs text-slate-500">
                      Dataset: {widget.datasetRef || 'none'} · Read model: {widget.readModelRef || 'none'}
                    </p>
                  </div>
                ))
              ) : (
                <DetailEmptyState text="No widgets are configured for this dashboard." />
              )}
            </div>
          </section>

          <div className="grid gap-6 xl:grid-cols-2">
            <section className="rounded-xl border border-slate-800 bg-slate-950/50 p-4">
              <div className="flex items-start justify-between gap-3">
                <div>
                  <h3 className="text-lg font-semibold text-white">Filters</h3>
                  <p className="mt-1 text-sm text-slate-400">Filter definitions available to dashboard viewers.</p>
                </div>
                <Pill>{dashboardFilterCount > 0 ? `${dashboardFilterCount} filters` : 'No filters'}</Pill>
              </div>
              <div className="mt-4 space-y-3">
                {dashboardFilters.length ? (
                  dashboardFilters.map((filter) => (
                    <div key={filter.filterId} className="rounded-lg border border-slate-800 bg-slate-900/70 p-3 text-sm text-slate-300">
                      <p className="font-medium text-white">{filter.label}</p>
                      <p className="mt-1 text-slate-400">
                        {filter.filterType} · required {String(filter.required)} · default {filter.defaultValue || 'none'}
                      </p>
                    </div>
                  ))
                ) : (
                  <DetailEmptyState text="No dashboard filters are configured." />
                )}
              </div>
            </section>

            <section className="rounded-xl border border-slate-800 bg-slate-950/50 p-4">
              <div className="flex items-start justify-between gap-3">
                <div>
                  <h3 className="text-lg font-semibold text-white">Drilldowns</h3>
                  <p className="mt-1 text-sm text-slate-400">Canonical drill-in targets exposed from this dashboard.</p>
                </div>
                <Pill>{dashboardDrilldownCount > 0 ? `${dashboardDrilldownCount} drilldowns` : 'No drilldowns'}</Pill>
              </div>
              <div className="mt-4 space-y-3">
                {dashboardDrilldowns.length ? (
                  dashboardDrilldowns.map((drilldown) => (
                    <div key={drilldown.drilldownId} className="rounded-lg border border-slate-800 bg-slate-900/70 p-3 text-sm text-slate-300">
                      <p className="font-medium text-white">{drilldown.title}</p>
                      <p className="mt-1 text-slate-400">
                        {drilldown.targetType} → {drilldown.targetRef}
                      </p>
                      <p className="mt-1 text-xs text-slate-500">{drilldown.parameterMappings.join(', ') || 'No parameter mappings'}</p>
                    </div>
                  ))
                ) : (
                  <DetailEmptyState text="No drilldowns are configured." />
                )}
              </div>
            </section>
          </div>
        </div>
      }
    />
  )
}

function AlertDetailPage({ accessToken }: { accessToken: string }) {
  const { alertId } = useParams<{ alertId: string }>()
  const query = useQuery({
    queryKey: ['reportarr', 'alerts', alertId, accessToken],
    queryFn: () => listAlerts(accessToken),
    enabled: Boolean(accessToken) && Boolean(alertId),
  })
  const datasetsQuery = useQuery({
    queryKey: ['reportarr', 'datasets', accessToken],
    queryFn: () => listDatasets(accessToken),
    enabled: Boolean(accessToken) && Boolean(alertId),
  })
  const metricsQuery = useQuery({
    queryKey: ['reportarr', 'metrics', accessToken],
    queryFn: () => listMetrics(accessToken),
    enabled: Boolean(accessToken) && Boolean(alertId),
  })
  const dashboardsQuery = useQuery({
    queryKey: ['reportarr', 'dashboards', accessToken],
    queryFn: () => listDashboards(accessToken),
    enabled: Boolean(accessToken) && Boolean(alertId),
  })
  const reportDefinitionsQuery = useQuery({
    queryKey: ['reportarr', 'report-definitions', accessToken],
    queryFn: () => listReportDefinitions(accessToken),
    enabled: Boolean(accessToken) && Boolean(alertId),
  })
  const widgetsQuery = useQuery({
    queryKey: ['reportarr', 'widgets', accessToken],
    queryFn: () => listWidgets(accessToken),
    enabled: Boolean(accessToken) && Boolean(alertId),
  })

  if (!alertId) {
    return <Navigate to="/alerts" replace />
  }

  if (query.isLoading) {
    return <div className="reportarr-page">Loading alert…</div>
  }

  const alert = query.data?.find((item) => item.alertId === alertId) ?? null
  const dataset = alert ? datasetsQuery.data?.find((item) => item.datasetId === alert.datasetRef) ?? null : null
  const metric = alert ? metricsQuery.data?.find((item) => item.metricId === alert.metricRef) ?? null : null
  const triggerHistory = query.data
    ? query.data
      .filter(
        (item) =>
          item.alertId !== alertId &&
          ((alert?.datasetRef && item.datasetRef === alert.datasetRef) || (alert?.metricRef && item.metricRef === alert.metricRef)),
      )
      .sort((a, b) => new Date(b.triggeredAt ?? 0).getTime() - new Date(a.triggeredAt ?? 0).getTime())
      .slice(0, 5)
    : []
  const relatedDashboardIds = dashboardsQuery.data && alert
    ? dashboardsQuery.data
        .flatMap((dashboard) => dashboard.widgetRefs)
        .filter((widgetId) => (widgetsQuery.data ?? []).some((widget) => widget.widgetId === widgetId && widget.datasetRef === alert.datasetRef))
        .map((widgetId) => {
          const dashboard = dashboardsQuery.data!.find((item) => item.widgetRefs.includes(widgetId))
          return dashboard?.dashboardId
        })
        .filter((dashboardId): dashboardId is string => Boolean(dashboardId))
    : []
  const relatedDashboards = dashboardsQuery.data?.filter((item) => relatedDashboardIds.includes(item.dashboardId)) ?? []
  const relatedReports = alert && dataset ? reportDefinitionsQuery.data?.filter((item) => item.datasetRefs.includes(dataset.datasetId)) ?? [] : []
  const relatedDashboardCount = relatedDashboards.length
  const relatedReportCount = relatedReports.length
  const triggerHistoryCount = triggerHistory.length
  const sourceRefCount = Number(Boolean(alert?.datasetRef)) + Number(Boolean(alert?.metricRef))
  const resolutionTone: DetailTone = alert?.resolvedAt ? 'good' : alert?.acknowledgedAt ? 'warn' : 'bad'

  return (
    <ReportDetailShell
      backLabel="Alerts"
      backTo="/alerts"
      breadcrumbs={alert ? [alert.alertNumber, alert.title] : ['Alert detail']}
      icon={<Bell className="h-8 w-8" />}
      title={alert?.title ?? 'Alert detail'}
      subtitle={`Inspect a single alert (${alertId}).`}
      badges={[
        { label: alert?.severity ?? 'Unknown', tone: alert?.severity === 'critical' ? 'bad' : alert?.severity === 'high' ? 'warn' : 'info' },
        { label: alert?.status ?? 'Unknown', tone: resolutionTone },
      ]}
      metrics={[
        {
          label: 'Trigger history',
          value: triggerHistoryCount,
          hint: 'Related trigger records for the same condition',
          icon: <History className="h-5 w-5" />,
          tone: triggerHistoryCount > 0 ? 'info' : 'neutral',
        },
        {
          label: 'Dashboards',
          value: relatedDashboardCount,
          hint: 'Dashboards consuming this alert source',
          icon: <BarChart3 className="h-5 w-5" />,
          tone: relatedDashboardCount > 0 ? 'good' : 'neutral',
        },
        {
          label: 'Reports',
          value: relatedReportCount,
          hint: 'Report definitions tied to the same dataset',
          icon: <FileText className="h-5 w-5" />,
          tone: relatedReportCount > 0 ? 'good' : 'neutral',
        },
        {
          label: 'Source refs',
          value: sourceRefCount,
          hint: 'Dataset and metric references on this alert',
          icon: <Database className="h-5 w-5" />,
          tone: sourceRefCount > 0 ? 'info' : 'neutral',
        },
      ]}
      snapshotTitle="Alert snapshot"
      snapshotSubtitle="Core identity, state, and linked source records."
      snapshotFields={[
        { label: 'Alert number', value: alert?.alertNumber ?? 'n/a', source: 'ReportArr alert' },
        { label: 'Type', value: alert?.alertType ?? 'n/a', source: 'ReportArr alert' },
        { label: 'Severity', value: alert?.severity ?? 'n/a', source: 'ReportArr alert' },
        { label: 'Status', value: alert?.status ?? 'n/a', source: 'ReportArr alert' },
        { label: 'Condition', value: alert?.condition ?? 'n/a', source: 'ReportArr alert' },
        { label: 'Source dataset', value: dataset ? `${dataset.datasetKey} (${dataset.datasetId})` : alert?.datasetRef || 'not set', source: 'ReportArr alert / dataset' },
        { label: 'Source metric', value: metric ? `${metric.metricKey} (${metric.metricId})` : alert?.metricRef || 'not set', source: 'ReportArr alert / metric' },
        { label: 'Triggered at', value: formatDate(alert?.triggeredAt ?? null), source: 'ReportArr event' },
        { label: 'Acknowledged by', value: alert?.acknowledgedByPersonId ?? 'n/a', source: 'ReportArr alert' },
        { label: 'Acknowledged at', value: formatDate(alert?.acknowledgedAt ?? null), source: 'ReportArr event' },
        { label: 'Resolved at', value: formatDate(alert?.resolvedAt ?? null), source: 'ReportArr event' },
      ]}
      decisionTitle="Attention decision"
      decisionBadge={{ label: alert?.resolvedAt ? 'Resolved' : alert?.acknowledgedAt ? 'Acknowledged' : 'Open', tone: resolutionTone }}
      decisionIcon={alert?.resolvedAt ? <CheckCircle2 className="h-5 w-5 text-emerald-300" /> : <AlertTriangle className="h-5 w-5 text-amber-300" />}
      decisionSummary={alert?.resolvedAt
        ? 'The alert is resolved.'
        : alert?.acknowledgedAt
          ? 'The alert is acknowledged and still being watched.'
          : 'The alert is open and needs attention.'}
      decisionDetail={alert
        ? `This alert is tied to ${relatedDashboardCount} dashboard(s) and ${relatedReportCount} report(s), with ${triggerHistoryCount} related trigger record(s) in the same condition lineage.`
        : 'No alert record was found.'}
      allowedChecks={Math.max(0, relatedDashboardCount + relatedReportCount)}
      blockedChecks={alert?.resolvedAt ? 0 : 1}
      railSections={[
        {
          title: 'Source signal',
          icon: <Database className="h-5 w-5" />,
          content: (
            <div className="space-y-3 text-sm text-slate-300">
              <p><strong className="text-slate-100">Dataset:</strong> {dataset ? `${dataset.datasetKey} (${dataset.datasetId})` : alert?.datasetRef || 'not set'}</p>
              <p><strong className="text-slate-100">Metric:</strong> {metric ? `${metric.metricKey} (${metric.metricId})` : alert?.metricRef || 'not set'}</p>
              <p><strong className="text-slate-100">Description:</strong> {alert?.description ?? 'none'}</p>
            </div>
          ),
        },
        {
          title: 'Downstream reach',
          icon: <PlayCircle className="h-5 w-5" />,
          content: (
            <div className="space-y-3 text-sm text-slate-300">
              <p><strong className="text-slate-100">Dashboards:</strong> {relatedDashboards.map((item) => item.title).join(', ') || 'none'}</p>
              <p><strong className="text-slate-100">Reports:</strong> {relatedReports.map((item) => item.title).join(', ') || 'none'}</p>
            </div>
          ),
        },
      ]}
      mainContent={
        <div className="space-y-6">
          <section className="rounded-xl border border-slate-800 bg-slate-950/50 p-4">
            <div className="flex items-start justify-between gap-3">
              <div>
                <h3 className="text-lg font-semibold text-white">Trigger history</h3>
                <p className="mt-1 text-sm text-slate-400">Recent alerts triggered by the same source dataset or metric.</p>
              </div>
              <Pill>{triggerHistoryCount > 0 ? `${triggerHistoryCount} related alerts` : 'No related alerts'}</Pill>
            </div>
            <div className="mt-4 space-y-3">
              {triggerHistory.length ? (
                triggerHistory.map((item) => (
                  <div key={item.alertId} className="rounded-lg border border-slate-800 bg-slate-900/70 p-3 text-sm text-slate-300">
                    <Link className="font-medium text-cyan-300 underline" to={`/alerts/${item.alertId}`}>
                      {formatDate(item.triggeredAt)}
                    </Link>
                    <p className="mt-1 text-slate-400">
                      {item.status} · {item.alertType}
                    </p>
                  </div>
                ))
              ) : (
                <DetailEmptyState text="No related trigger history exists for this condition." />
              )}
            </div>
          </section>

          <div className="grid gap-6 xl:grid-cols-2">
            <section className="rounded-xl border border-slate-800 bg-slate-950/50 p-4">
              <div className="flex items-start justify-between gap-3">
                <div>
                  <h3 className="text-lg font-semibold text-white">Dashboards</h3>
                  <p className="mt-1 text-sm text-slate-400">Dashboards that include widgets tied to the same source dataset.</p>
                </div>
                <Pill>{relatedDashboardCount > 0 ? `${relatedDashboardCount} dashboards` : 'No dashboards'}</Pill>
              </div>
              <div className="mt-4 space-y-3">
                {relatedDashboards.length ? (
                  relatedDashboards.map((item) => (
                    <div key={item.dashboardId} className="rounded-lg border border-slate-800 bg-slate-900/70 p-3 text-sm text-slate-300">
                      <Link className="text-cyan-300 underline" to={`/dashboards/${item.dashboardId}`}>
                        {item.title}
                      </Link>
                    </div>
                  ))
                ) : (
                  <DetailEmptyState text="No dashboards are linked to this alert source." />
                )}
              </div>
            </section>

            <section className="rounded-xl border border-slate-800 bg-slate-950/50 p-4">
              <div className="flex items-start justify-between gap-3">
                <div>
                  <h3 className="text-lg font-semibold text-white">Reports</h3>
                  <p className="mt-1 text-sm text-slate-400">Report definitions using the same source dataset.</p>
                </div>
                <Pill>{relatedReportCount > 0 ? `${relatedReportCount} reports` : 'No reports'}</Pill>
              </div>
              <div className="mt-4 space-y-3">
                {relatedReports.length ? (
                  relatedReports.map((item) => (
                    <div key={item.reportDefinitionId} className="rounded-lg border border-slate-800 bg-slate-900/70 p-3 text-sm text-slate-300">
                      <p className="font-medium text-white">{item.title}</p>
                    </div>
                  ))
                ) : (
                  <DetailEmptyState text="No report definitions are linked to this alert source." />
                )}
              </div>
            </section>
          </div>
        </div>
      }
    />
  )
}

function AuditPackageDetailPage({ accessToken }: { accessToken: string }) {
  const { auditReportPackageId } = useParams<{ auditReportPackageId: string }>()
  const query = useQuery({
    queryKey: ['reportarr', 'audit-packages', auditReportPackageId, accessToken],
    queryFn: () => getAuditPackage(accessToken, auditReportPackageId!),
    enabled: Boolean(accessToken) && Boolean(auditReportPackageId),
  })

  if (!auditReportPackageId) {
    return <Navigate to="/audit" replace />
  }

  if (query.isLoading) {
    return <div className="reportarr-page">Loading audit package…</div>
  }

  const auditPackage = query.data ?? null
  const readinessScore = auditPackage?.readinessScore ?? 0
  const missingCount = auditPackage?.missingEvidenceSummary ? auditPackage.missingEvidenceSummary.split(',').filter(Boolean).length : 0
  const invalidCount = auditPackage?.invalidEvidenceSummary ? auditPackage.invalidEvidenceSummary.split(',').filter(Boolean).length : 0
  const readinessTone: DetailTone = auditPackage ? (readinessScore >= 90 ? 'good' : readinessScore >= 70 ? 'warn' : 'bad') : 'neutral'

  return (
    <ReportDetailShell
      backLabel="Audit"
      backTo="/audit"
      breadcrumbs={auditPackage ? [auditPackage.packageNumber, auditPackage.title] : ['Audit package detail']}
      icon={<ShieldCheck className="h-8 w-8" />}
      title={auditPackage?.title ?? 'Audit package detail'}
      subtitle={`Inspect a single audit package (${auditReportPackageId}).`}
      badges={[
        { label: auditPackage?.status ?? 'Unknown', tone: readinessTone },
        { label: `${auditPackage?.readinessScore ?? 0}% ready`, tone: readinessTone },
      ]}
      metrics={[
        {
          label: 'Readiness',
          value: `${readinessScore}%`,
          hint: 'Overall package readiness score',
          icon: <Gauge className="h-5 w-5" />,
          tone: readinessTone,
        },
        {
          label: 'Missing evidence',
          value: missingCount,
          hint: 'Evidence gaps called out by the package',
          icon: <AlertTriangle className="h-5 w-5" />,
          tone: missingCount > 0 ? 'warn' : 'good',
        },
        {
          label: 'Invalid evidence',
          value: invalidCount,
          hint: 'Evidence items marked invalid',
          icon: <FileText className="h-5 w-5" />,
          tone: invalidCount > 0 ? 'bad' : 'good',
        },
        {
          label: 'Locked state',
          value: auditPackage?.lockedAt ? 'Locked' : 'Unlocked',
          hint: 'Whether the package is finalized',
          icon: <ShieldCheck className="h-5 w-5" />,
          tone: auditPackage?.lockedAt ? 'info' : 'neutral',
        },
      ]}
      snapshotTitle="Audit package snapshot"
      snapshotSubtitle="Readiness, lock state, and evidence summary."
      snapshotFields={[
        { label: 'Package number', value: auditPackage?.packageNumber ?? 'n/a', source: 'ReportArr audit package' },
        { label: 'Status', value: auditPackage?.status ?? 'n/a', source: 'ReportArr audit package' },
        { label: 'Requested by', value: auditPackage?.requestedByPersonId ?? 'n/a', source: 'ReportArr request' },
        { label: 'Generated at', value: formatDate(auditPackage?.generatedAt ?? null), source: 'ReportArr event' },
        { label: 'Locked at', value: formatDate(auditPackage?.lockedAt ?? null), source: 'ReportArr event' },
        { label: 'Missing evidence', value: auditPackage?.missingEvidenceSummary || 'none', source: 'ReportArr audit package' },
        { label: 'Invalid evidence', value: auditPackage?.invalidEvidenceSummary || 'none', source: 'ReportArr audit package' },
      ]}
      decisionTitle="Audit readiness"
      decisionBadge={{ label: auditPackage?.status ?? 'Unknown', tone: readinessTone }}
      decisionIcon={<ShieldCheck className="h-5 w-5 text-sky-300" />}
      decisionSummary={readinessScore >= 90
        ? 'The package is ready for audit review.'
        : readinessScore >= 70
          ? 'The package is mostly ready but still has gaps.'
          : 'The package has significant evidence gaps.'}
      decisionDetail={auditPackage
        ? `ReportArr tracks ${missingCount} missing evidence item(s) and ${invalidCount} invalid evidence item(s) for this package.`
        : 'No audit package record was found.'}
      allowedChecks={Math.max(0, 5 - missingCount - invalidCount)}
      blockedChecks={missingCount + invalidCount}
      railSections={[
        {
          title: 'Evidence gaps',
          icon: <AlertTriangle className="h-5 w-5" />,
          content: (
            <div className="space-y-3 text-sm text-slate-300">
              <p><strong className="text-slate-100">Missing evidence:</strong> {auditPackage?.missingEvidenceSummary || 'none'}</p>
              <p><strong className="text-slate-100">Invalid evidence:</strong> {auditPackage?.invalidEvidenceSummary || 'none'}</p>
            </div>
          ),
        },
        {
          title: 'Lifecycle',
          icon: <History className="h-5 w-5" />,
          content: (
            <div className="space-y-3 text-sm text-slate-300">
              <p><strong className="text-slate-100">Generated:</strong> {formatDate(auditPackage?.generatedAt ?? null)}</p>
              <p><strong className="text-slate-100">Locked:</strong> {formatDate(auditPackage?.lockedAt ?? null)}</p>
            </div>
          ),
        },
      ]}
      mainContent={
        auditPackage ? (
          <section className="rounded-xl border border-slate-800 bg-slate-950/50 p-4">
            <h3 className="text-lg font-semibold text-white">Package summary</h3>
            <p className="mt-2 text-sm text-slate-400">
              {auditPackage.description || 'No description provided for this package.'}
            </p>
          </section>
        ) : (
          <DetailEmptyState text="No audit package record is available." />
        )
      }
    />
  )
}

function SourceConnectorDetailPage({ accessToken }: { accessToken: string }) {
  const { sourceConnectorId } = useParams<{ sourceConnectorId: string }>()
  const query = useQuery({
    queryKey: ['reportarr', 'source-connectors', sourceConnectorId, accessToken],
    queryFn: () => listSourceConnectors(accessToken),
    enabled: Boolean(accessToken) && Boolean(sourceConnectorId),
  })

  if (!sourceConnectorId) {
    return <Navigate to="/integrations" replace />
  }

  if (query.isLoading) {
    return <div className="reportarr-page">Loading source connector…</div>
  }

  const connector = query.data?.find((item) => item.sourceConnectorId === sourceConnectorId) ?? null
  const supportedEventCount = connector?.supportedEventTypes.length ?? 0
  const supportedDatasetCount = connector?.supportedDatasets.length ?? 0
  const connectorTone: DetailTone = connector?.status === 'connected' ? 'good' : connector?.status === 'error' ? 'bad' : 'warn'

  return (
    <ReportDetailShell
      backLabel="Source connectors"
      backTo="/integrations"
      breadcrumbs={connector ? [connector.sourceProduct, connector.connectorType] : ['Source connector detail']}
      icon={<PlugZap className="h-8 w-8" />}
      title={connector?.connectorType ?? 'Source connector detail'}
      subtitle={`Inspect a single source connector (${sourceConnectorId}).`}
      badges={[
        { label: connector?.status ?? 'Unknown', tone: connectorTone },
        { label: connector?.sourceProduct ?? 'Unknown source', tone: 'info' },
      ]}
      metrics={[
        {
          label: 'Event types',
          value: supportedEventCount,
          hint: 'Supported inbound event types',
          icon: <History className="h-5 w-5" />,
          tone: supportedEventCount > 0 ? 'good' : 'neutral',
        },
        {
          label: 'Datasets',
          value: supportedDatasetCount,
          hint: 'Supported downstream datasets',
          icon: <Database className="h-5 w-5" />,
          tone: supportedDatasetCount > 0 ? 'good' : 'neutral',
        },
        {
          label: 'Last connected',
          value: formatDate(connector?.lastConnectedAt ?? null),
          hint: 'Most recent successful connection time',
          icon: <CheckCircle2 className="h-5 w-5" />,
          tone: connector?.lastConnectedAt ? 'good' : 'neutral',
        },
        {
          label: 'Last error',
          value: formatDate(connector?.lastErrorAt ?? null),
          hint: 'Most recent error timestamp',
          icon: <AlertTriangle className="h-5 w-5" />,
          tone: connector?.lastErrorAt ? 'warn' : 'good',
        },
      ]}
      snapshotTitle="Connector snapshot"
      snapshotSubtitle="Source product, service client, and sync state."
      snapshotFields={[
        { label: 'Source product', value: connector?.sourceProduct ?? 'n/a', source: 'ReportArr connector' },
        { label: 'Connector type', value: connector?.connectorType ?? 'n/a', source: 'ReportArr connector' },
        { label: 'Status', value: connector?.status ?? 'n/a', source: 'ReportArr connector' },
        { label: 'Service client', value: connector?.serviceClientRef ?? 'n/a', source: 'ReportArr connector' },
        { label: 'Last connected', value: formatDate(connector?.lastConnectedAt ?? null), source: 'ReportArr event' },
        { label: 'Last error', value: formatDate(connector?.lastErrorAt ?? null), source: 'ReportArr event' },
        { label: 'Last error message', value: connector?.lastErrorMessage ?? 'none', source: 'ReportArr connector' },
        { label: 'Supported event types', value: connector?.supportedEventTypes.join(', ') || 'none', source: 'ReportArr connector' },
        { label: 'Supported datasets', value: connector?.supportedDatasets.join(', ') || 'none', source: 'ReportArr connector' },
      ]}
      decisionTitle="Integration health"
      decisionBadge={{ label: connector?.status ?? 'Unknown', tone: connectorTone }}
      decisionIcon={<PlugZap className="h-5 w-5 text-sky-300" />}
      decisionSummary={connector?.status === 'connected'
        ? 'The connector is connected and healthy.'
        : connector?.status === 'error'
          ? 'The connector is reporting an error.'
          : 'The connector state needs review.'}
      decisionDetail={connector
        ? `This connector supports ${supportedEventCount} event type(s) and ${supportedDatasetCount} dataset target(s).`
        : 'No source connector record was found.'}
      allowedChecks={Math.max(0, supportedEventCount + supportedDatasetCount)}
      blockedChecks={connector?.lastErrorAt ? 1 : 0}
      railSections={[
        {
          title: 'Supported interfaces',
          icon: <Workflow className="h-5 w-5" />,
          content: (
            <div className="space-y-3 text-sm text-slate-300">
              <p><strong className="text-slate-100">Event types:</strong> {connector?.supportedEventTypes.join(', ') || 'none'}</p>
              <p><strong className="text-slate-100">Datasets:</strong> {connector?.supportedDatasets.join(', ') || 'none'}</p>
            </div>
          ),
        },
      ]}
      mainContent={
        connector?.lastErrorMessage ? (
          <section className="rounded-xl border border-slate-800 bg-slate-950/50 p-4">
            <h3 className="text-lg font-semibold text-white">Last error message</h3>
            <p className="mt-2 text-sm text-slate-400">{connector.lastErrorMessage}</p>
          </section>
        ) : (
          <DetailEmptyState text="No connector error message is currently recorded." />
        )
      }
    />
  )
}

function RefreshJobDetailPage({ accessToken }: { accessToken: string }) {
  const { refreshJobId } = useParams<{ refreshJobId: string }>()
  const query = useQuery({
    queryKey: ['reportarr', 'refresh-jobs', refreshJobId, accessToken],
    queryFn: () => listRefreshJobs(accessToken),
    enabled: Boolean(accessToken) && Boolean(refreshJobId),
  })

  if (!refreshJobId) {
    return <Navigate to="/datasets" replace />
  }

  if (query.isLoading) {
    return <div className="reportarr-page">Loading refresh job…</div>
  }

  const refreshJob = query.data?.find((item) => item.refreshJobId === refreshJobId) ?? null
  const refreshJobTone: DetailTone = refreshJob?.status === 'completed' ? 'good' : refreshJob?.status === 'failed' ? 'bad' : 'warn'

  return (
    <ReportDetailShell
      backLabel="Datasets"
      backTo="/datasets"
      breadcrumbs={refreshJob ? [refreshJob.datasetId, refreshJob.refreshType] : ['Refresh job detail']}
      icon={<RefreshCcw className="h-8 w-8" />}
      title={refreshJob?.refreshType ?? 'Refresh job detail'}
      subtitle={`Inspect a single refresh job (${refreshJobId}).`}
      badges={[
        { label: refreshJob?.status ?? 'Unknown', tone: refreshJobTone },
        { label: refreshJob?.readModelId ? 'Read model linked' : 'Dataset only', tone: refreshJob?.readModelId ? 'info' : 'neutral' },
      ]}
      metrics={[
        {
          label: 'Created',
          value: formatNumber(refreshJob?.recordsCreated ?? 0),
          hint: 'Records created by the job',
          icon: <Plus className="h-5 w-5" />,
          tone: (refreshJob?.recordsCreated ?? 0) > 0 ? 'good' : 'neutral',
        },
        {
          label: 'Updated',
          value: formatNumber(refreshJob?.recordsUpdated ?? 0),
          hint: 'Records updated by the job',
          icon: <RefreshCcw className="h-5 w-5" />,
          tone: (refreshJob?.recordsUpdated ?? 0) > 0 ? 'good' : 'neutral',
        },
        {
          label: 'Skipped',
          value: formatNumber(refreshJob?.recordsSkipped ?? 0),
          hint: 'Records skipped during refresh',
          icon: <Gauge className="h-5 w-5" />,
          tone: (refreshJob?.recordsSkipped ?? 0) > 0 ? 'warn' : 'good',
        },
        {
          label: 'Errors',
          value: formatNumber(refreshJob?.errorCount ?? 0),
          hint: 'Refresh errors recorded',
          icon: <AlertTriangle className="h-5 w-5" />,
          tone: (refreshJob?.errorCount ?? 0) > 0 ? 'bad' : 'good',
        },
      ]}
      snapshotTitle="Refresh job snapshot"
      snapshotSubtitle="Queue, execution, and outcome details."
      snapshotFields={[
        { label: 'Dataset', value: refreshJob?.datasetId ?? 'n/a', source: 'ReportArr refresh job' },
        { label: 'Read model', value: refreshJob?.readModelId ?? 'n/a', source: 'ReportArr refresh job' },
        { label: 'Status', value: refreshJob?.status ?? 'n/a', source: 'ReportArr refresh job' },
        { label: 'Type', value: refreshJob?.refreshType ?? 'n/a', source: 'ReportArr refresh job' },
        { label: 'Requested by', value: refreshJob?.requestedByPersonId ?? 'n/a', source: 'ReportArr request' },
        { label: 'Queued at', value: formatDate(refreshJob?.queuedAt ?? null), source: 'ReportArr event' },
        { label: 'Started at', value: formatDate(refreshJob?.startedAt ?? null), source: 'ReportArr event' },
        { label: 'Completed at', value: formatDate(refreshJob?.completedAt ?? null), source: 'ReportArr event' },
      ]}
      decisionTitle="Refresh decision"
      decisionBadge={{ label: refreshJob?.status ?? 'Unknown', tone: refreshJobTone }}
      decisionIcon={<RefreshCcw className="h-5 w-5 text-sky-300" />}
      decisionSummary={refreshJob?.status === 'completed'
        ? 'The refresh job completed successfully.'
        : refreshJob?.status === 'failed'
          ? 'The refresh job failed and needs review.'
          : 'The refresh job is still moving through the queue.'}
      decisionDetail={refreshJob
        ? `ReportArr recorded ${formatNumber(refreshJob.recordsCreated)} created, ${formatNumber(refreshJob.recordsUpdated)} updated, and ${formatNumber(refreshJob.errorCount)} error(s).`
        : 'No refresh job record was found.'}
      allowedChecks={Math.max(0, (refreshJob?.recordsCreated ?? 0) + (refreshJob?.recordsUpdated ?? 0))}
      blockedChecks={refreshJob?.errorCount ?? 0}
      railSections={[]}
      mainContent={
        refreshJob ? (
          <section className="rounded-xl border border-slate-800 bg-slate-950/50 p-4">
            <h3 className="text-lg font-semibold text-white">Outcome summary</h3>
            <p className="mt-2 text-sm text-slate-400">
              This job queued {formatDate(refreshJob.queuedAt)}, started {formatDate(refreshJob.startedAt)}, and completed {formatDate(refreshJob.completedAt)}.
            </p>
          </section>
        ) : (
          <DetailEmptyState text="No refresh job record is available." />
        )
      }
    />
  )
}

function SourceEventReceiptDetailPage({ accessToken }: { accessToken: string }) {
  const { sourceEventReceiptId } = useParams<{ sourceEventReceiptId: string }>()
  const query = useQuery({
    queryKey: ['reportarr', 'source-events', sourceEventReceiptId, accessToken],
    queryFn: () => listSourceEvents(accessToken),
    enabled: Boolean(accessToken) && Boolean(sourceEventReceiptId),
  })

  if (!sourceEventReceiptId) {
    return <Navigate to="/history" replace />
  }

  if (query.isLoading) {
    return <div className="reportarr-page">Loading source event…</div>
  }

  const event = query.data?.find((item) => item.sourceEventReceiptId === sourceEventReceiptId) ?? null

  return (
    <div className="reportarr-page">
      <SectionHeader eyebrow="History" title="Source event detail" description={`Inspect a single source event (${sourceEventReceiptId}).`} />
      <Panel title="Source event detail">
        <SourceEventDetail event={event} />
      </Panel>
    </div>
  )
}

function ReadModelDetailPage({ accessToken }: { accessToken: string }) {
  const { readModelId } = useParams<{ readModelId: string }>()
  const query = useQuery({
    queryKey: ['reportarr', 'read-models', readModelId, accessToken],
    queryFn: () => getReadModel(accessToken, readModelId!),
    enabled: Boolean(accessToken) && Boolean(readModelId),
  })

  if (!readModelId) {
    return <Navigate to="/datasets" replace />
  }

  if (query.isLoading) {
    return <div className="reportarr-page">Loading read model…</div>
  }

  const readModel = query.data ?? null
  const datasetCount = readModel?.datasetRefs.length ?? 0
  const refreshJobCount = readModel?.refreshJobRefs.length ?? 0
  const readModelTone: DetailTone = readModel?.status === 'active' ? 'good' : readModel?.status === 'failed' ? 'bad' : 'warn'

  return (
    <ReportDetailShell
      backLabel="Read models"
      backTo="/read-models"
      breadcrumbs={readModel ? [readModel.readModelKey, readModel.title] : ['Read model detail']}
      icon={<Gauge className="h-8 w-8" />}
      title={readModel?.title ?? 'Read model detail'}
      subtitle={`Inspect a single read model (${readModelId}).`}
      badges={[
        { label: readModel?.status ?? 'Unknown', tone: readModelTone },
        { label: readModel?.primarySourceProduct ?? 'Unknown source', tone: 'info' },
      ]}
      metrics={[
        {
          label: 'Datasets',
          value: datasetCount,
          hint: 'Source datasets feeding this model',
          icon: <Database className="h-5 w-5" />,
          tone: datasetCount > 0 ? 'good' : 'neutral',
        },
        {
          label: 'Refresh jobs',
          value: refreshJobCount,
          hint: 'Refresh jobs tracked for this model',
          icon: <RefreshCcw className="h-5 w-5" />,
          tone: refreshJobCount > 0 ? 'good' : 'neutral',
        },
        {
          label: 'Last rebuilt',
          value: formatDate(readModel?.lastRebuiltAt ?? null),
          hint: 'Most recent rebuild time',
          icon: <History className="h-5 w-5" />,
          tone: readModel?.lastRebuiltAt ? 'info' : 'neutral',
        },
        {
          label: 'Last updated',
          value: formatDate(readModel?.lastUpdatedAt ?? null),
          hint: 'Most recent metadata update',
          icon: <PlayCircle className="h-5 w-5" />,
          tone: readModel?.lastUpdatedAt ? 'info' : 'neutral',
        },
      ]}
      snapshotTitle="Read model snapshot"
      snapshotSubtitle="Key identity and dependency information."
      snapshotFields={[
        { label: 'Read model number', value: readModel?.readModelNumber ?? 'n/a', source: 'ReportArr read model' },
        { label: 'Key', value: readModel?.readModelKey ?? 'n/a', source: 'ReportArr read model' },
        { label: 'Type', value: readModel?.readModelType ?? 'n/a', source: 'ReportArr read model' },
        { label: 'Status', value: readModel?.status ?? 'n/a', source: 'ReportArr read model' },
        { label: 'Primary source', value: readModel?.primarySourceProduct ?? 'n/a', source: 'ReportArr read model' },
        { label: 'Primary entity', value: readModel?.primaryEntityType ?? 'n/a', source: 'ReportArr read model' },
        { label: 'Datasets', value: readModel?.datasetRefs.join(', ') || 'none', source: 'ReportArr dependency' },
        { label: 'Refresh jobs', value: readModel?.refreshJobRefs.join(', ') || 'none', source: 'ReportArr dependency' },
      ]}
      decisionTitle="Read model decision"
      decisionBadge={{ label: readModel?.status ?? 'Unknown', tone: readModelTone }}
      decisionIcon={<Gauge className="h-5 w-5 text-sky-300" />}
      decisionSummary={readModel?.status === 'active'
        ? 'The read model is active.'
        : readModel?.status === 'failed'
          ? 'The read model needs repair.'
          : 'The read model state is informational.'}
      decisionDetail={readModel
        ? `ReportArr tracks ${datasetCount} dataset source(s) and ${refreshJobCount} refresh job reference(s) for this read model.`
        : 'No read model record was found.'}
      allowedChecks={Math.max(0, datasetCount + refreshJobCount)}
      blockedChecks={readModel?.status === 'failed' ? 1 : 0}
      railSections={[]}
      mainContent={
        readModel ? (
          <section className="rounded-xl border border-slate-800 bg-slate-950/50 p-4">
            <h3 className="text-lg font-semibold text-white">Dependency summary</h3>
            <p className="mt-2 text-sm text-slate-400">
              This model is currently fed by {datasetCount} dataset(s) and {refreshJobCount} refresh job reference(s).
            </p>
          </section>
        ) : (
          <DetailEmptyState text="No read model record is available." />
        )
      }
    />
  )
}

export function App() {
  const location = useLocation()
  const { session, sessionQuery, launchCatalogQuery, bootstrapError, workspaceSession, switcherEntitlements, launch } = useReportArrWorkspace()
  const [bootstrapRedirected, setBootstrapRedirected] = useState(false)

  useEffect(() => {
    if (bootstrapError && !bootstrapRedirected) {
      clearSession()
      setBootstrapRedirected(true)
    }
  }, [bootstrapError, bootstrapRedirected])

  const accessToken = session?.accessToken ?? ''
  const meQuery = useQuery({
    queryKey: ['reportarr', 'me'],
    queryFn: () => getMe(accessToken),
    enabled: Boolean(accessToken) && !bootstrapError,
  })
  const sessionRoleKey = sessionQuery.data?.tenantRoleKey ?? ''
  const isPlatformAdmin = sessionQuery.data?.isPlatformAdmin ?? false
  const routerBasename = import.meta.env.VITE_ROUTER_BASENAME?.replace(/\/+$/, '') ?? ''
  const normalizedPathname = (() => {
    const pathname = location.pathname.replace(/\/+$/, '') || '/'
    if (routerBasename && pathname.startsWith(routerBasename)) {
      const stripped = pathname.slice(routerBasename.length)
      return stripped || '/'
    }
    return pathname
  })()
  const currentTitle = useMemo(() => {
    if (normalizedPathname.startsWith('/datasets')) return 'Datasets'
    if (normalizedPathname.startsWith('/read-models')) return 'Read models'
    if (normalizedPathname.startsWith('/refresh-jobs')) return 'Refresh jobs'
    if (normalizedPathname.startsWith('/dashboards')) return 'Dashboards'
    if (normalizedPathname.startsWith('/reports/builder')) return 'Report builder'
    if (normalizedPathname.startsWith('/reports/schedules')) return 'Report schedules'
    if (normalizedPathname.startsWith('/reports/exports')) return 'Report exports'
    if (normalizedPathname.startsWith('/reports')) return 'Reports'
    if (normalizedPathname.startsWith('/kpis')) return 'KPIs'
    if (normalizedPathname.startsWith('/metrics')) return 'Metrics'
    if (normalizedPathname.startsWith('/alerts')) return 'Alerts'
    if (normalizedPathname.startsWith('/audit')) return 'Audit'
    if (normalizedPathname.startsWith('/source-connectors') || normalizedPathname.startsWith('/integrations')) return 'Source connectors'
    if (normalizedPathname.startsWith('/ingestion-status') || normalizedPathname.startsWith('/history')) return 'Ingestion status'
    if (normalizedPathname.startsWith('/settings')) return 'Settings'
    return 'Overview'
  }, [normalizedPathname])

  if (normalizedPathname.startsWith('/launch')) {
    return <LaunchPage />
  }

  return (
    <ProductWorkspaceFrame
      productName="ReportArr"
      productKey="reportarr"
      workspaceSubtitle="Reporting, dashboards, KPIs, and audit history"
      navItems={navItems}
      entitlements={switcherEntitlements}
      suiteHomeUrl={suiteHomeUrl}
      productLaunchUrls={productLaunchUrls}
      onSelectProduct={(productKey) => {
        if (session?.accessToken) {
          void launch.mutate(productKey)
        }
      }}
      onSignOut={() => {
        clearSession()
        window.location.assign(suiteHomeUrl)
      }}
      isProductLaunchPending={launch.isPending}
      productLaunchError={launch.isError ? formatProductLaunchError(launch.error) : null}
      aiAssistance={
        session?.accessToken ? { apiBase, accessToken: session.accessToken } : undefined
      }
      workspaceSession={workspaceSession}
      isBootstrapping={
        Boolean(session?.accessToken) && (sessionQuery.isLoading || launchCatalogQuery.isLoading)
      }
      bootstrapError={bootstrapError}
    >
      <Routes>
        <Route index element={<DashboardPage accessToken={accessToken} roleKey={sessionRoleKey} isPlatformAdmin={isPlatformAdmin} />} />
        <Route path="/datasets" element={<DatasetsPage accessToken={accessToken} roleKey={sessionRoleKey} isPlatformAdmin={isPlatformAdmin} />} />
        <Route path="/read-models" element={<ReadModelsPage accessToken={accessToken} />} />
        <Route path="/refresh-jobs" element={<RefreshJobsPage accessToken={accessToken} />} />
        <Route path="/dashboards" element={<DashboardsPage accessToken={accessToken} roleKey={sessionRoleKey} isPlatformAdmin={isPlatformAdmin} />} />
        <Route path="/reports" element={<ReportsPage accessToken={accessToken} roleKey={sessionRoleKey} isPlatformAdmin={isPlatformAdmin} />} />
        <Route path="/reports/builder" element={<ReportBuilderPage accessToken={accessToken} roleKey={sessionRoleKey} isPlatformAdmin={isPlatformAdmin} />} />
        <Route path="/reports/schedules" element={<ReportsPage accessToken={accessToken} roleKey={sessionRoleKey} isPlatformAdmin={isPlatformAdmin} />} />
        <Route path="/reports/exports" element={<ReportsPage accessToken={accessToken} roleKey={sessionRoleKey} isPlatformAdmin={isPlatformAdmin} />} />
        <Route path="/reports/runs/:reportRunId" element={<ReportRunDetailPage accessToken={accessToken} />} />
        <Route path="/reports/schedules/:scheduleId" element={<ReportScheduleDetailPage accessToken={accessToken} />} />
        <Route path="/reports/exports/:exportJobId" element={<ExportJobDetailPage accessToken={accessToken} />} />
        <Route path="/kpis" element={<KpisPage accessToken={accessToken} />} />
        <Route path="/metrics" element={<KpisPage accessToken={accessToken} />} />
        <Route path="/dashboards/:dashboardId" element={<DashboardDetailPage accessToken={accessToken} />} />
        <Route path="/alerts" element={<AlertsPage accessToken={accessToken} roleKey={sessionRoleKey} isPlatformAdmin={isPlatformAdmin} />} />
        <Route path="/alerts/:alertId" element={<AlertDetailPage accessToken={accessToken} />} />
        <Route path="/audit" element={<AuditPage accessToken={accessToken} roleKey={sessionRoleKey} isPlatformAdmin={isPlatformAdmin} />} />
        <Route path="/audit/:auditReportPackageId" element={<AuditPackageDetailPage accessToken={accessToken} />} />
        <Route path="/integrations" element={<IntegrationsPage accessToken={accessToken} roleKey={sessionRoleKey} isPlatformAdmin={isPlatformAdmin} />} />
        <Route path="/source-connectors" element={<IntegrationsPage accessToken={accessToken} roleKey={sessionRoleKey} isPlatformAdmin={isPlatformAdmin} />} />
        <Route path="/integrations/:sourceConnectorId" element={<SourceConnectorDetailPage accessToken={accessToken} />} />
        <Route path="/source-connectors/:sourceConnectorId" element={<SourceConnectorDetailPage accessToken={accessToken} />} />
        <Route path="/history" element={<HistoryPage accessToken={accessToken} roleKey={sessionRoleKey} isPlatformAdmin={isPlatformAdmin} />} />
        <Route path="/ingestion-status" element={<HistoryPage accessToken={accessToken} roleKey={sessionRoleKey} isPlatformAdmin={isPlatformAdmin} />} />
        <Route path="/history/events/:sourceEventReceiptId" element={<SourceEventReceiptDetailPage accessToken={accessToken} />} />
        <Route path="/source-events/:sourceEventReceiptId" element={<SourceEventReceiptDetailPage accessToken={accessToken} />} />
        <Route path="/datasets/:datasetId" element={<DatasetDetailPage accessToken={accessToken} />} />
        <Route path="/read-models/:readModelId" element={<ReadModelDetailPage accessToken={accessToken} />} />
        <Route path="/refresh-jobs/:refreshJobId" element={<RefreshJobDetailPage accessToken={accessToken} />} />
        <Route path="/settings" element={<SettingsPage accessToken={accessToken} session={session} me={meQuery.data ?? null} roleKey={sessionRoleKey} isPlatformAdmin={isPlatformAdmin} />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
      <p className="mt-6 text-sm text-slate-400">Current view: {currentTitle}</p>
    </ProductWorkspaceFrame>
  )
}
