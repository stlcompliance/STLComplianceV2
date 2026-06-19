import { useEffect, useMemo, useState, type ReactNode } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  Archive,
  BadgeCheck,
  Clock3,
  FileText,
  FileUp,
  History,
  LayoutDashboard,
  LockKeyhole,
  MessageSquare,
  PackageSearch,
  ScanSearch,
  Settings,
  ShieldCheck,
  Upload,
} from 'lucide-react'
import { Navigate, Route, Routes, useLocation, useNavigate, useParams } from 'react-router-dom'
import {
  ApiErrorCallout,
  ControlledSelect,
  StaticSearchPicker,
  ProductWorkspaceFrame,
  buildSourceObjectRef,
  buildProductLaunchUrlMap,
  formatDisplayLabel,
  formatProductLaunchError,
  getSourceReferenceOption,
  getLaunchCatalog,
  getErrorMessage,
  listSourceReferenceOptions,
  resolveProductWorkspaceBootstrapError,
  resolveSuiteHomeUrl,
  SUITE_SOURCE_PRODUCT_OPTIONS,
  useProductWorkspaceLaunch,
  type PickerOption,
  type ProductNavItem,
  type SourceReferenceOption,
} from '@stl/shared-ui'
import { LaunchPage } from './LaunchPage'
import {
  activateLegalHold,
  applyManualCorrection,
  archiveRecord,
  confirmEvidenceMapping,
  createControlledDocument,
  createAccessGrant,
  createDocumentAcknowledgement,
  createDocumentDistribution,
  createDocumentReview,
  createDocumentVersion,
  archiveControlledDocument,
  createDisposalReview,
  createEvidenceMapping,
  createPhotoEvidence,
  listEvidenceCoverage,
  listFiles,
  createExternalShare,
  createRedaction,
  createLegalHold,
  createAccessPolicy,
  createPackage,
  archivePackage,
  createRecord,
  createScan,
  createSignatureRecord,
  createUploadSession,
  downloadFile,
  downloadPackage,
  getDashboard,
  getExtractionResult,
  getOcrResult,
  getPackageManifest,
  getRecord,
  listRecordMetadata,
  listRecordLinks,
  getRetentionStatus,
  getSessionBootstrap,
  listAccessGrants,
  listAccessLogs,
  listAccessPolicies,
  listControlledDocuments,
  listDocumentAcknowledgements,
  listDocumentDistributions,
  listDocumentReviews,
  listDocumentVersions,
  listDisposalReviews,
  listEvidenceMappings,
  listExternalShares,
  listLegalHolds,
  listPackages,
  listReminders,
  listRecords,
  listRetentionPolicies,
  listScans,
  listUploadSessions,
  listRedactions,
  rejectEvidenceMapping,
  releaseLegalHold,
  reviewExtractionResult,
  revokeExternalShare,
  recordExternalShareAccess,
  purgeRecord,
  promoteDocumentVersion,
  obsoleteControlledDocument,
  recalculateRetentionStatuses,
  refreshControlledDocumentWorkflows,
  refreshAccessGrants,
  refreshExternalShares,
  expireDocumentDistribution,
  expireExternalShare,
  supersedeControlledDocument,
  completeDocumentAcknowledgement,
  completeDocumentReview,
  completeDisposalReview,
  completeCaptureRequest,
  updateAccessPolicy,
  revokeAccessGrant,
  revokeDocumentDistribution,
  lockPackage,
  updateRecord,
  createRecordMetadata,
  createRecordLink,
  createRecordComment,
  createCaptureRequest,
  cancelCaptureRequest,
  expireCaptureRequest,
  type RecordArrAccessPolicy,
  type RecordArrUploadSession,
  type RecordArrFile,
  type RecordArrControlledDocument,
  type RecordArrLegalHold,
  type RecordArrPackage,
  type RecordArrReminder,
  type RecordArrRecord,
  listRecordComments,
  listCaptureRequests,
  updateRecordComment,
  skipCaptureRequest,
} from './api/client'
import { clearSession, loadSession, type StoredRecordArrSession } from './auth/sessionStorage'

const suiteHomeUrl = resolveSuiteHomeUrl(import.meta.env.VITE_SUITE_URL)
const productLaunchUrls = buildProductLaunchUrlMap(import.meta.env)
const apiBase = import.meta.env.VITE_RECORDARR_API_BASE ?? ''

const staffPersonOptions: PickerOption[] = [
  { value: 'person-record-admin', label: 'Riley Chen - Records admin' },
  { value: 'person-document-owner', label: 'Morgan Ellis - Document owner' },
  { value: 'person-compliance-reviewer', label: 'Sam Patel - Compliance reviewer' },
  { value: 'person-access-steward', label: 'Taylor Nguyen - Access steward' },
  { value: 'person-retention-manager', label: 'Jamie Brooks - Retention manager' },
]

const staffSiteOptions: PickerOption[] = [
  { value: 'staffarr-site-main', label: 'Sparta Operations Center - StaffArr site' },
  { value: 'staffarr-site-dallas', label: 'Dallas Distribution Hub - StaffArr site' },
  { value: 'staffarr-site-field', label: 'Field Operations - StaffArr site' },
]

const navItems: ProductNavItem[] = [
  { label: 'Dashboard', to: '/', icon: LayoutDashboard as ProductNavItem['icon'] },
  { label: 'Records', to: '/records', icon: FileText as ProductNavItem['icon'] },
  {
    label: 'Capture',
    to: '/capture',
    icon: FileUp as ProductNavItem['icon'],
    children: [
      { label: 'Upload Sessions', to: '/upload-sessions', icon: FileUp as ProductNavItem['icon'] },
      { label: 'Uploads', to: '/uploads', icon: Upload as ProductNavItem['icon'] },
      { label: 'Scan Processing', to: '/scan-processing', icon: ScanSearch as ProductNavItem['icon'] },
      { label: 'OCR Review', to: '/ocr-review', icon: FileText as ProductNavItem['icon'] },
      { label: 'Evidence Mappings', to: '/evidence-mappings', icon: BadgeCheck as ProductNavItem['icon'] },
    ],
  },
  {
    label: 'Controlled Documents',
    to: '/controlled-documents',
    icon: Archive as ProductNavItem['icon'],
    children: [
      { label: 'Reviews', to: '/document-reviews', icon: MessageSquare as ProductNavItem['icon'] },
      { label: 'Distributions', to: '/distributions', icon: Upload as ProductNavItem['icon'] },
      { label: 'Acknowledgements', to: '/acknowledgements', icon: BadgeCheck as ProductNavItem['icon'] },
    ],
  },
  {
    label: 'Packages',
    to: '/packages',
    icon: PackageSearch as ProductNavItem['icon'],
    children: [
      { label: 'Record Packages', to: '/record-packages', icon: PackageSearch as ProductNavItem['icon'] },
    ],
  },
  {
    label: 'Retention',
    to: '/retention',
    icon: Clock3 as ProductNavItem['icon'],
    children: [
      { label: 'Disposal Reviews', to: '/disposal-reviews', icon: History as ProductNavItem['icon'] },
    ],
  },
  {
    label: 'Holds',
    to: '/holds',
    icon: ShieldCheck as ProductNavItem['icon'],
    sectionBreakBefore: true,
    children: [
      { label: 'Legal Holds', to: '/legal-holds', icon: ShieldCheck as ProductNavItem['icon'] },
    ],
  },
  {
    label: 'Access',
    to: '/access',
    icon: LockKeyhole as ProductNavItem['icon'],
    children: [
      { label: 'External Shares', to: '/external-shares', icon: Upload as ProductNavItem['icon'] },
      { label: 'Redactions', to: '/redactions', icon: Archive as ProductNavItem['icon'] },
      { label: 'Access Logs', to: '/access-logs', icon: History as ProductNavItem['icon'] },
    ],
  },
  { label: 'Settings', to: '/settings', icon: Settings as ProductNavItem['icon'], sectionBreakBefore: true },
]

function formatDate(value: string | null | undefined): string {
  if (!value) {
    return 'n/a'
  }
  const date = new Date(value)
  return Number.isNaN(date.getTime())
    ? value
    : date.toLocaleString(undefined, {
        month: 'short',
        day: 'numeric',
        hour: 'numeric',
        minute: '2-digit',
      })
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
    <div className="flex flex-col gap-3 rounded-2xl border border-slate-700/70 bg-slate-950/75 p-5 shadow-xl shadow-slate-950/20 lg:flex-row lg:items-end lg:justify-between">
      <div className="space-y-2">
        <p className="text-xs font-semibold uppercase tracking-[0.2em] text-cyan-300">{eyebrow}</p>
        <h1 className="text-2xl font-semibold text-slate-50">{title}</h1>
        <p className="max-w-3xl text-sm text-slate-300">{description}</p>
      </div>
      {action}
    </div>
  )
}

function EmptyState({ title }: { title: string }) {
  return <div className="rounded-xl border border-dashed border-slate-700/80 p-4 text-sm text-slate-400">{title}</div>
}

function LoadingCard({ label }: { label: string }) {
  return (
    <div className="recordarr-card">
      <div className="recordarr-card-inner text-sm text-slate-300">{label}</div>
    </div>
  )
}

function MetricCard({
  title,
  value,
  hint,
}: {
  title: string
  value: string | number
  hint: string
}) {
  return (
    <div className="recordarr-card">
      <div className="recordarr-card-inner">
        <p className="recordarr-label">{title}</p>
        <p className="mt-2 text-3xl font-semibold text-slate-50">{value}</p>
        <p className="mt-2 text-sm text-slate-300">{hint}</p>
      </div>
    </div>
  )
}

function Field({
  label,
  children,
  wide,
}: {
  label: string
  children: ReactNode
  wide?: boolean
}) {
  return (
    <label className={wide ? 'md:col-span-2' : ''}>
      <div className="recordarr-label mb-2">{label}</div>
      {children}
    </label>
  )
}

function ReadableOption({ value }: { value: string }) {
  return <option value={value}>{formatDisplayLabel(value)}</option>
}

function toRecordOption(record: RecordArrRecord): PickerOption {
  return {
    value: record.recordId,
    label: `${record.recordNumber} - ${record.title}`,
    inactive: record.status === 'archived' || Boolean(record.purgedAt),
  }
}

function toControlledDocumentOption(document: RecordArrControlledDocument): PickerOption {
  return {
    value: document.controlledDocumentId,
    label: `${document.documentNumber} - ${document.title}`,
    inactive: document.status === 'archived' || document.status === 'obsolete',
  }
}

function toUploadSessionOption(session: RecordArrUploadSession): PickerOption {
  return {
    value: session.uploadSessionId,
    label: `${session.uploadSessionNumber} - ${session.uploadPurpose}`,
    inactive: session.status === 'completed' || session.status === 'revoked',
  }
}

function useRecordReferenceOptions(accessToken: string) {
  const recordsQuery = useQuery({
    queryKey: ['recordarr', 'record-reference-options', accessToken],
    queryFn: () => listRecords(accessToken),
    enabled: Boolean(accessToken),
    retry: false,
  })

  const options = useMemo(
    () => (recordsQuery.data ?? []).map(toRecordOption),
    [recordsQuery.data],
  )

  return { options, isLoading: recordsQuery.isLoading }
}

function PersonReferencePicker({
  value,
  onChange,
  placeholder = 'Search StaffArr people',
}: {
  value: string
  onChange: (value: string) => void
  placeholder?: string
}) {
  return (
    <StaticSearchPicker
      value={value}
      onChange={onChange}
      options={staffPersonOptions}
      placeholder={placeholder}
    />
  )
}

function StaffSiteReferencePicker({
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
      options={staffSiteOptions}
      placeholder="Search StaffArr sites"
    />
  )
}

function RecordReferencePicker({
  value,
  onChange,
  options,
  isLoading,
  placeholder = 'Search RecordArr records',
}: {
  value: string
  onChange: (value: string) => void
  options: PickerOption[]
  isLoading?: boolean
  placeholder?: string
}) {
  return (
    <StaticSearchPicker
      value={value}
      onChange={onChange}
      options={options}
      placeholder={isLoading ? 'Loading records...' : placeholder}
      disabled={isLoading}
    />
  )
}

function SourceProductPicker({
  id,
  value,
  onChange,
}: {
  id?: string
  value: string
  onChange: (value: string) => void
}) {
  return (
    <ControlledSelect
      id={id}
      value={value}
      onChange={onChange}
      options={SUITE_SOURCE_PRODUCT_OPTIONS}
      className="recordarr-select"
      emptyLabel="Select source product"
    />
  )
}

function SourceObjectRefPicker({
  id,
  value,
  sourceProduct,
  onChange,
}: {
  id?: string
  value: string
  sourceProduct?: string | null
  onChange: (value: string, selected?: SourceReferenceOption) => void
}) {
  return (
    <StaticSearchPicker
      id={id}
      value={value}
      onChange={(nextValue) => onChange(nextValue, getSourceReferenceOption(nextValue))}
      options={listSourceReferenceOptions(sourceProduct)}
      selectedOption={getSourceReferenceOption(value)}
      placeholder="Search source records"
    />
  )
}

function ControlledDocumentReferencePicker({
  value,
  onChange,
  options,
  placeholder = 'Search controlled documents',
}: {
  value: string
  onChange: (value: string) => void
  options: PickerOption[]
  placeholder?: string
}) {
  return (
    <StaticSearchPicker
      value={value}
      onChange={onChange}
      options={options}
      placeholder={placeholder}
    />
  )
}

function UploadSessionReferencePicker({
  value,
  onChange,
  options,
}: {
  value: string
  onChange: (value: string) => void
  options: PickerOption[]
}) {
  return (
    <StaticSearchPicker
      value={value}
      onChange={onChange}
      options={options}
      placeholder="Search upload sessions"
    />
  )
}

type WorkspacePageProps = {
  accessToken: string
  actorPersonId: string
}

function useWorkspaceSessionBootstrap() {
  const session = loadSession()
  const sessionQuery = useQuery({
    queryKey: ['recordarr', 'session', session?.accessToken],
    queryFn: () => getSessionBootstrap(session!.accessToken),
    enabled: Boolean(session?.accessToken),
    retry: false,
  })

  const launchCatalogQuery = useQuery({
    queryKey: ['recordarr', 'launch-catalog', session?.accessToken],
    queryFn: () => getLaunchCatalog(apiBase, session!.accessToken, 'recordarr'),
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

  return {
    session,
    sessionQuery,
    launchCatalogQuery,
    bootstrapError,
  }
}

function useRecordArrWorkspace() {
  const queryClient = useQueryClient()
  const { session, sessionQuery, launchCatalogQuery, bootstrapError } = useWorkspaceSessionBootstrap()
  const workspaceSession =
    session && sessionQuery.data && !bootstrapError
      ? {
          userId: session.userId,
          tenantId: session.tenantId,
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
    currentProductKey: 'recordarr',
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

function DashboardPage({ accessToken }: { accessToken: string }) {
  const remindersQuery = useQuery({
    queryKey: ['recordarr', 'reminders'],
    queryFn: () => listReminders(accessToken),
    enabled: Boolean(accessToken),
    staleTime: 15_000,
  })
  const dashboardQuery = useQuery({
    queryKey: ['recordarr', 'dashboard'],
    queryFn: () => getDashboard(accessToken),
    enabled: Boolean(accessToken),
    staleTime: 20_000,
  })

  const dashboard = dashboardQuery.data

  return (
    <div className="recordarr-page">
      <SectionHeader
        eyebrow="RecordArr"
        title="Records and evidence control center"
        description="Manage record capture, evidence packages, retention, holds, controlled documents, and access controls in one workspace."
        action={
          <span className="recordarr-pill">
            <BadgeCheck className="h-4 w-4" />
            {dashboard ? `Updated ${formatDate(dashboard.generatedAt)}` : 'Loading dashboard'}
          </span>
        }
      />
      {dashboardQuery.isError ? (
        <ApiErrorCallout
          title="Unable to load dashboard"
          message={getErrorMessage(dashboardQuery.error, 'Failed to load RecordArr dashboard.')}
        />
      ) : null}
      {remindersQuery.isError ? (
        <ApiErrorCallout
          title="Unable to load reminders"
          message={getErrorMessage(remindersQuery.error, 'Failed to load RecordArr reminders.')}
        />
      ) : null}
      {dashboardQuery.isLoading ? <LoadingCard label="Loading dashboard" /> : null}
      {dashboard ? (
        <>
          <div className="recordarr-grid cols-3">
            <MetricCard title="Records" value={dashboard.recordCount} hint={`${dashboard.activeCount} active / ${dashboard.reviewCount} in review`} />
            <MetricCard title="Upload sessions" value={dashboard.uploadSessionCount} hint="Active capture workflows" />
            <MetricCard title="Packages" value={dashboard.packageCount} hint="Evidence bundles and manifests" />
            <MetricCard title="Controlled docs" value={dashboard.controlledDocumentCount} hint="Versioned procedures and policies" />
            <MetricCard title="Legal holds" value={dashboard.legalHoldCount} hint="Active audit or preservation holds" />
            <MetricCard title="Ready for review" value={dashboard.recentRecords.length} hint="Recent record activity surfaced here" />
          </div>

          <div className="recordarr-grid cols-2">
            <Card title="Recent records" icon={<FileText className="h-4 w-4 text-cyan-300" />}>
              <SimpleRecordList records={dashboard.recentRecords} emptyLabel="No records yet." />
            </Card>
            <Card title="Reminders" icon={<Clock3 className="h-4 w-4 text-cyan-300" />}>
              <SimpleReminderList reminders={remindersQuery.data ?? []} emptyLabel="No reminders due right now." />
            </Card>
            <Card title="Open packages" icon={<PackageSearch className="h-4 w-4 text-cyan-300" />}>
              <SimplePackageList packages={dashboard.openPackages} emptyLabel="No packages yet." />
            </Card>
            <Card title="Controlled documents" icon={<Archive className="h-4 w-4 text-cyan-300" />}>
              <SimpleDocumentList documents={dashboard.controlledDocuments} emptyLabel="No controlled documents yet." />
            </Card>
            <Card title="Legal holds" icon={<ShieldCheck className="h-4 w-4 text-cyan-300" />}>
              <SimpleHoldList holds={dashboard.legalHolds} emptyLabel="No legal holds yet." />
            </Card>
          </div>
        </>
      ) : null}
    </div>
  )
}

function Card({ title, icon, children }: { title: string; icon: ReactNode; children: ReactNode }) {
  return (
    <div className="recordarr-card">
      <div className="recordarr-card-inner space-y-3">
        <div className="flex items-center gap-2">
          {icon}
          <h2 className="text-lg font-semibold text-slate-50">{title}</h2>
        </div>
        {children}
      </div>
    </div>
  )
}

function SimpleRecordList({ records, emptyLabel }: { records: RecordArrRecord[]; emptyLabel: string }) {
  if (records.length === 0) {
    return <EmptyState title={emptyLabel} />
  }
  return (
    <div className="space-y-3">
      {records.slice(0, 5).map((record) => (
        <div key={record.recordId} className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3">
          <div className="flex items-center justify-between gap-3">
            <strong className="text-sm text-slate-100">{record.recordNumber}</strong>
            <span className="recordarr-pill text-[0.7rem]">{record.status}</span>
          </div>
          <p className="mt-1 text-sm text-slate-300">{record.title}</p>
          <p className="mt-2 text-xs text-slate-400">{record.sourceProduct} · {record.currentFileName}</p>
        </div>
      ))}
    </div>
  )
}

function SimplePackageList({ packages, emptyLabel }: { packages: RecordArrPackage[]; emptyLabel: string }) {
  if (packages.length === 0) {
    return <EmptyState title={emptyLabel} />
  }
  return (
    <div className="space-y-3">
      {packages.slice(0, 5).map((pkg) => (
        <div key={pkg.packageId} className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3">
          <div className="flex items-center justify-between gap-3">
            <strong className="text-sm text-slate-100">{pkg.packageNumber}</strong>
            <span className="recordarr-pill text-[0.7rem]">{pkg.status}</span>
          </div>
          <p className="mt-1 text-sm text-slate-300">{pkg.title}</p>
          <p className="mt-2 text-xs text-slate-400">{pkg.recordRefs.length} record(s) · {pkg.sourceProduct}</p>
        </div>
      ))}
    </div>
  )
}

function SimpleDocumentList({ documents, emptyLabel }: { documents: RecordArrControlledDocument[]; emptyLabel: string }) {
  if (documents.length === 0) {
    return <EmptyState title={emptyLabel} />
  }
  return (
    <div className="space-y-3">
      {documents.slice(0, 5).map((document) => (
        <div key={document.controlledDocumentId} className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3">
          <div className="flex items-center justify-between gap-3">
            <strong className="text-sm text-slate-100">{document.documentNumber}</strong>
            <span className="recordarr-pill text-[0.7rem]">{document.status}</span>
          </div>
          <p className="mt-1 text-sm text-slate-300">{document.title}</p>
          <p className="mt-2 text-xs text-slate-400">Version {document.currentVersionId} · review due {formatDate(document.nextReviewAt)}</p>
        </div>
      ))}
    </div>
  )
}

function SimpleReminderList({ reminders, emptyLabel }: { reminders: RecordArrReminder[]; emptyLabel: string }) {
  if (reminders.length === 0) {
    return <EmptyState title={emptyLabel} />
  }

  return (
    <div className="space-y-3">
      {reminders.slice(0, 5).map((reminder) => (
        <div key={reminder.reminderId} className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3">
          <div className="flex items-center justify-between gap-3">
            <strong className="text-sm text-slate-100">{reminder.title}</strong>
            <span className="recordarr-pill text-[0.7rem]">{reminder.status}</span>
          </div>
          <p className="mt-1 text-sm text-slate-300">{reminder.description}</p>
          <p className="mt-2 text-xs text-slate-400">
            Due {formatDate(reminder.dueAt)} · {reminder.reminderType.replaceAll('_', ' ')}
          </p>
        </div>
      ))}
    </div>
  )
}

function SimpleHoldList({ holds, emptyLabel }: { holds: RecordArrLegalHold[]; emptyLabel: string }) {
  if (holds.length === 0) {
    return <EmptyState title={emptyLabel} />
  }
  return (
    <div className="space-y-3">
      {holds.slice(0, 5).map((hold) => (
        <div key={hold.legalHoldId} className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3">
          <div className="flex items-center justify-between gap-3">
            <strong className="text-sm text-slate-100">{hold.holdNumber}</strong>
            <span className="recordarr-pill text-[0.7rem]">{hold.status}</span>
          </div>
          <p className="mt-1 text-sm text-slate-300">{hold.title}</p>
          <p className="mt-2 text-xs text-slate-400">{hold.recordRefs.length} record(s) · {hold.holdType}</p>
        </div>
      ))}
    </div>
  )
}

function RecordsPage({ accessToken, actorPersonId }: WorkspacePageProps) {
  const queryClient = useQueryClient()
  const navigate = useNavigate()
  const [search, setSearch] = useState('')
  const [form, setForm] = useState({
    title: '',
    description: '',
    recordType: 'document',
    documentType: 'bol',
    classification: 'internal',
    sourceProduct: '',
    sourceObjectType: '',
    sourceObjectId: '',
    sourceObjectDisplayName: '',
    ownerPersonId: actorPersonId,
    uploadedByPersonId: actorPersonId,
    currentFileName: '',
    currentMimeType: 'application/pdf',
  })

  const recordsQuery = useQuery({
    queryKey: ['recordarr', 'records', search.trim()],
    queryFn: () => listRecords(accessToken, search),
    enabled: Boolean(accessToken),
  })

  const createMutation = useMutation({
    mutationFn: () => createRecord(accessToken, form),
    onSuccess: async (record) => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
      navigate(`/records/${record.recordId}`)
    },
  })

  return (
    <div className="recordarr-page">
      <SectionHeader
        eyebrow="Records"
        title="Record library"
        description="Capture, classify, and route records through the lifecycle with source links, versions, and expiry details."
        action={<span className="recordarr-pill"><FileText className="h-4 w-4" /> {recordsQuery.data?.length ?? 0} records</span>}
      />
      <div className="recordarr-card">
        <div className="recordarr-card-inner space-y-3">
          <div className="flex items-center gap-2">
            <BadgeCheck className="h-4 w-4 text-cyan-300" />
            <h2 className="text-lg font-semibold text-slate-50">Search records</h2>
          </div>
          <div className="grid gap-3 md:grid-cols-[minmax(0,1fr)_auto]">
            <Field label="Search term" wide>
              <input
                className="recordarr-input"
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                placeholder="Record number, title, source, file, or tag"
              />
            </Field>
            <div className="flex items-end gap-3">
              <button type="button" className="recordarr-button secondary" onClick={() => setSearch('')} disabled={!search}>
                Clear
              </button>
            </div>
          </div>
          <p className="text-sm text-slate-400">
            Search is server-side and matches record number, title, description, source, file name, and tags.
          </p>
        </div>
      </div>
      <div className="recordarr-card">
        <div className="recordarr-card-inner space-y-4">
          <div className="flex items-center gap-2">
            <FileUp className="h-4 w-4 text-cyan-300" />
            <h2 className="text-lg font-semibold text-slate-50">Create record</h2>
          </div>
          <div className="grid gap-3 md:grid-cols-2">
            <Field label="Title"><input className="recordarr-input" value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} /></Field>
            <Field label="Record type"><select className="recordarr-select" value={form.recordType} onChange={(e) => setForm({ ...form, recordType: e.target.value })}><ReadableOption value="document" /><ReadableOption value="photo" /><ReadableOption value="signature" /><ReadableOption value="video" /><ReadableOption value="audio" /><ReadableOption value="form_submission" /><ReadableOption value="generated_pdf" /><ReadableOption value="certificate" /><ReadableOption value="inspection_record" /><ReadableOption value="training_record" /><ReadableOption value="maintenance_record" /><ReadableOption value="receiving_record" /><ReadableOption value="delivery_record" /><ReadableOption value="quality_record" /><ReadableOption value="audit_evidence" /><ReadableOption value="evidence_package" /><ReadableOption value="report_output" /><ReadableOption value="other" /></select></Field>
            <Field label="Document type"><select className="recordarr-select" value={form.documentType} onChange={(e) => setForm({ ...form, documentType: e.target.value })}><ReadableOption value="bol" /><ReadableOption value="pod" /><ReadableOption value="packing_slip" /><ReadableOption value="invoice_reference" /><ReadableOption value="certificate" /><ReadableOption value="policy" /><ReadableOption value="procedure" /><ReadableOption value="work_instruction" /><ReadableOption value="form" /><ReadableOption value="safety_data_sheet" /><ReadableOption value="inspection_form" /><ReadableOption value="maintenance_evidence" /><ReadableOption value="training_evidence" /><ReadableOption value="quality_evidence" /><ReadableOption value="customer_document" /><ReadableOption value="supplier_document" /><ReadableOption value="contract" /><ReadableOption value="permit" /><ReadableOption value="photo_evidence" /><ReadableOption value="signature_evidence" /><ReadableOption value="other" /></select></Field>
            <Field label="Classification"><select className="recordarr-select" value={form.classification} onChange={(e) => setForm({ ...form, classification: e.target.value })}><ReadableOption value="public" /><ReadableOption value="internal" /><ReadableOption value="confidential" /><ReadableOption value="restricted" /><ReadableOption value="legal_hold" /></select></Field>
            <Field label="Source product"><SourceProductPicker value={form.sourceProduct} onChange={(sourceProduct) => setForm({ ...form, sourceProduct, sourceObjectType: '', sourceObjectId: '', sourceObjectDisplayName: '' })} /></Field>
            <Field label="Source reference">
              <SourceObjectRefPicker
                value={buildSourceObjectRef(form.sourceProduct, form.sourceObjectType, form.sourceObjectId)}
                sourceProduct={form.sourceProduct}
                onChange={(_, selected) => {
                  if (!selected) return
                  setForm({
                    ...form,
                    sourceProduct: selected.sourceProduct,
                    sourceObjectType: selected.sourceObjectType,
                    sourceObjectId: selected.sourceObjectId,
                    sourceObjectDisplayName: selected.sourceObjectDisplayName,
                  })
                }}
              />
            </Field>
            <Field label="Owner person"><PersonReferencePicker value={form.ownerPersonId} onChange={(ownerPersonId) => setForm({ ...form, ownerPersonId })} /></Field>
            <Field label="Uploaded by"><PersonReferencePicker value={form.uploadedByPersonId} onChange={(uploadedByPersonId) => setForm({ ...form, uploadedByPersonId })} /></Field>
            <Field label="Current file name"><input className="recordarr-input" value={form.currentFileName} onChange={(e) => setForm({ ...form, currentFileName: e.target.value })} /></Field>
            <Field label="Mime type"><input className="recordarr-input" value={form.currentMimeType} onChange={(e) => setForm({ ...form, currentMimeType: e.target.value })} /></Field>
            <Field label="Description" wide><textarea className="recordarr-textarea" value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} /></Field>
          </div>
          <div className="flex flex-wrap items-center gap-3">
            <button type="button" className="recordarr-button" onClick={() => createMutation.mutate()} disabled={createMutation.isPending}>
              {createMutation.isPending ? 'Creating...' : 'Create record'}
            </button>
            {createMutation.isError ? <span className="text-sm text-rose-300">{getErrorMessage(createMutation.error, 'Create failed')}</span> : null}
          </div>
        </div>
      </div>
      <RecordsTable records={recordsQuery.data ?? []} loading={recordsQuery.isLoading} onSelect={(recordId) => navigate(`/records/${recordId}`)} />
    </div>
  )
}

function RecordsTable({
  records,
  loading,
  onSelect,
}: {
  records: RecordArrRecord[]
  loading: boolean
  onSelect: (recordId: string) => void
}) {
  return (
    <div className="recordarr-card">
      <div className="recordarr-card-inner space-y-3">
        <div className="flex items-center gap-2">
          <BadgeCheck className="h-4 w-4 text-cyan-300" />
          <h2 className="text-lg font-semibold text-slate-50">Record list</h2>
        </div>
        {loading ? <LoadingCard label="Loading records" /> : null}
        {records.length === 0 && !loading ? <EmptyState title="No records yet." /> : null}
        {records.length > 0 ? (
          <div className="overflow-x-auto rounded-2xl border border-slate-700/70">
            <table className="recordarr-table">
              <thead>
                <tr>
                  <th>Record</th>
                  <th>Status</th>
                  <th>Source</th>
                  <th>File</th>
                  <th>Expiry</th>
                  <th />
                </tr>
              </thead>
              <tbody>
                {records.map((record) => (
                  <tr key={record.recordId}>
                    <td>
                      <div className="font-semibold text-slate-50">{record.recordNumber}</div>
                      <div className="text-sm text-slate-300">{record.title}</div>
                    </td>
                    <td><span className="recordarr-pill text-[0.7rem]">{record.status}</span></td>
                    <td className="text-sm text-slate-300">
                      <div>{record.sourceProduct}</div>
                      <div className="text-xs text-slate-400">{record.sourceObjectDisplayName}</div>
                    </td>
                    <td className="text-sm text-slate-300">{record.currentFileName}</td>
                    <td className="text-sm text-slate-300">{formatDate(record.expiresAt)}</td>
                    <td>
                      <button type="button" className="recordarr-button secondary whitespace-nowrap" onClick={() => onSelect(record.recordId)}>
                        Open
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : null}
      </div>
    </div>
  )
}

function RecordDetailPage({ accessToken, actorPersonId }: WorkspacePageProps) {
  const queryClient = useQueryClient()
  const params = useParams()
  const recordId = params.recordId ?? ''
  const { options: recordOptions, isLoading: recordOptionsLoading } = useRecordReferenceOptions(accessToken)
  const [status, setStatus] = useState('review')
  const [classification, setClassification] = useState('internal')
  const [metadataForm, setMetadataForm] = useState({
    key: '',
    value: '',
    valueType: 'string',
    source: 'user_entered',
    confidenceScore: 1,
    createdByPersonId: actorPersonId,
  })
  const [linkForm, setLinkForm] = useState({
    linkedRecordId: '',
    sourceObjectRef: '',
    linkType: 'source',
    createdByPersonId: actorPersonId,
  })
  const [commentForm, setCommentForm] = useState({
    body: '',
    visibility: 'internal',
    actorPersonId: actorPersonId,
  })
  const [editingCommentId, setEditingCommentId] = useState('')
  const [selectedFileDownload, setSelectedFileDownload] = useState('')
  const [signatureForm, setSignatureForm] = useState({
    signaturePurpose: 'proof_of_delivery',
    signerPersonId: actorPersonId,
    signerExternalName: '',
    signerTitle: '',
    attestationText: '',
    capturedByPersonId: actorPersonId,
    sourceProduct: '',
    sourceObjectRef: '',
    geoCoordinates: '',
    deviceSnapshot: '',
  })
  const [photoForm, setPhotoForm] = useState({
    photoPurpose: 'delivery',
    capturedByPersonId: actorPersonId,
    sourceProduct: '',
    sourceObjectRef: '',
    geoCoordinates: '',
    deviceSnapshot: '',
    notes: '',
  })

  const recordQuery = useQuery({
    queryKey: ['recordarr', 'records', recordId],
    queryFn: () => getRecord(accessToken, recordId),
    enabled: Boolean(accessToken && recordId),
  })
  const metadataQuery = useQuery({
    queryKey: ['recordarr', 'record-metadata', recordId],
    queryFn: () => listRecordMetadata(accessToken, recordId),
    enabled: Boolean(accessToken && recordId),
  })
  const linksQuery = useQuery({
    queryKey: ['recordarr', 'record-links', recordId],
    queryFn: () => listRecordLinks(accessToken, recordId),
    enabled: Boolean(accessToken && recordId),
  })
  const commentsQuery = useQuery({
    queryKey: ['recordarr', 'record-comments', recordId],
    queryFn: () => listRecordComments(accessToken, recordId),
    enabled: Boolean(accessToken && recordId),
  })
  const filesQuery = useQuery({
    queryKey: ['recordarr', 'files', recordId],
    queryFn: () => listFiles(accessToken, recordId),
    enabled: Boolean(accessToken && recordId),
  })
  const retentionQuery = useQuery({
    queryKey: ['recordarr', 'retention-status', recordId],
    queryFn: () => getRetentionStatus(accessToken, recordId),
    enabled: Boolean(accessToken && recordId),
  })
  const logsQuery = useQuery({
    queryKey: ['recordarr', 'access-logs', recordId],
    queryFn: () => listAccessLogs(accessToken, recordId),
    enabled: Boolean(accessToken),
  })
  const scansQuery = useQuery({
    queryKey: ['recordarr', 'scans'],
    queryFn: () => listScans(accessToken),
    enabled: Boolean(accessToken),
  })
  const mappingsQuery = useQuery({
    queryKey: ['recordarr', 'evidence-mappings'],
    queryFn: () => listEvidenceMappings(accessToken),
    enabled: Boolean(accessToken),
  })
  const packagesQuery = useQuery({
    queryKey: ['recordarr', 'packages'],
    queryFn: () => listPackages(accessToken),
    enabled: Boolean(accessToken),
  })
  const uploadsQuery = useQuery({
    queryKey: ['recordarr', 'upload-sessions'],
    queryFn: () => listUploadSessions(accessToken),
    enabled: Boolean(accessToken),
  })
  const holdsQuery = useQuery({
    queryKey: ['recordarr', 'legal-holds'],
    queryFn: () => listLegalHolds(accessToken),
    enabled: Boolean(accessToken),
  })
  const documentsQuery = useQuery({
    queryKey: ['recordarr', 'documents'],
    queryFn: () => listControlledDocuments(accessToken),
    enabled: Boolean(accessToken),
  })

  const updateMutation = useMutation({
    mutationFn: () => updateRecord(accessToken, recordId, { status, classification }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const archiveMutation = useMutation({
    mutationFn: () => archiveRecord(accessToken, recordId, { actorPersonId: 'person-record-admin' }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const purgeMutation = useMutation({
    mutationFn: () => purgeRecord(accessToken, recordId, { actorPersonId: 'person-record-admin' }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const createMetadataMutation = useMutation({
    mutationFn: () =>
      createRecordMetadata(accessToken, recordId, {
        ...metadataForm,
        confidenceScore: Number(metadataForm.confidenceScore),
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const createLinkMutation = useMutation({
    mutationFn: () =>
      createRecordLink(accessToken, recordId, {
        linkedRecordId: linkForm.linkedRecordId || null,
        sourceObjectRef: linkForm.sourceObjectRef || null,
        linkType: linkForm.linkType,
        createdByPersonId: linkForm.createdByPersonId,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const saveCommentMutation = useMutation({
    mutationFn: () =>
      editingCommentId
        ? updateRecordComment(accessToken, recordId, editingCommentId, {
            body: commentForm.body,
            visibility: commentForm.visibility,
            editedByPersonId: commentForm.actorPersonId,
          })
        : createRecordComment(accessToken, recordId, {
            body: commentForm.body,
            visibility: commentForm.visibility,
            createdByPersonId: commentForm.actorPersonId,
          }),
    onSuccess: async () => {
      setEditingCommentId('')
      setCommentForm((current) => ({ ...current, body: 'Reviewed and ready for internal use.' }))
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const createSignatureMutation = useMutation({
    mutationFn: () =>
      createSignatureRecord(accessToken, {
        recordId,
        signaturePurpose: signatureForm.signaturePurpose,
        signerPersonId: signatureForm.signerPersonId || null,
        signerExternalName: signatureForm.signerExternalName || null,
        signerTitle: signatureForm.signerTitle || null,
        attestationText: signatureForm.attestationText,
        capturedByPersonId: signatureForm.capturedByPersonId,
        sourceProduct: signatureForm.sourceProduct,
        sourceObjectRef: signatureForm.sourceObjectRef,
        geoCoordinates: signatureForm.geoCoordinates || null,
        deviceSnapshot: signatureForm.deviceSnapshot || null,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const createPhotoMutation = useMutation({
    mutationFn: () =>
      createPhotoEvidence(accessToken, {
        recordId,
        photoPurpose: photoForm.photoPurpose,
        capturedByPersonId: photoForm.capturedByPersonId,
        sourceProduct: photoForm.sourceProduct,
        sourceObjectRef: photoForm.sourceObjectRef,
        geoCoordinates: photoForm.geoCoordinates || null,
        deviceSnapshot: photoForm.deviceSnapshot || null,
        notes: photoForm.notes || null,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })

  const record = recordQuery.data
  useEffect(() => {
    if (record?.classification) {
      setClassification(record.classification)
    }
  }, [record?.classification])
  useEffect(() => {
    if (record?.sourceProduct && record?.sourceObjectType && record?.sourceObjectId) {
      setLinkForm((current) => ({
        ...current,
        sourceObjectRef: `${record.sourceProduct}:${record.sourceObjectType}:${record.sourceObjectId}`,
      }))
    }
  }, [record?.sourceProduct, record?.sourceObjectType, record?.sourceObjectId])
  useEffect(() => {
    if (!editingCommentId) {
      return
    }

    const selectedComment = commentsQuery.data?.find((comment) => comment.commentId === editingCommentId)
    if (selectedComment) {
      setCommentForm({
        body: selectedComment.body,
        visibility: selectedComment.visibility,
        actorPersonId: selectedComment.editedByPersonId ?? selectedComment.createdByPersonId,
      })
    }
  }, [commentsQuery.data, editingCommentId])
  const relevantLogs = logsQuery.data ?? []
  const relatedScans = (scansQuery.data ?? []).filter((scan) => scan.recordId === recordId)
  const relatedMappings = (mappingsQuery.data ?? []).filter((mapping) => mapping.recordId === recordId)
  const relatedPackages = (packagesQuery.data ?? []).filter((pkg) => pkg.recordRefs.includes(recordId))
  const relatedUploads = (uploadsQuery.data ?? []).filter((upload) => upload.uploadedRecordRefs.includes(recordId))
  const relatedDocuments = (documentsQuery.data ?? []).filter((document) => document.recordId === recordId)
  const recordComments = commentsQuery.data ?? []
  const recordFiles: RecordArrFile[] = filesQuery.data ?? []
  const selectedComment = recordComments.find((comment) => comment.commentId === editingCommentId) ?? null
  const currentFile = record ? recordFiles.find((file) => file.fileId === record.currentFileRef) ?? null : null
  const activeHold = (holdsQuery.data ?? []).find((hold) => hold.status === 'active' && hold.recordRefs.includes(recordId)) ?? null
  const timeline = useMemo(() => {
    if (!record) return []
    return [
      { key: 'uploaded', label: 'Uploaded', value: formatDate(record.uploadedAt) },
      { key: 'effective', label: 'Effective', value: formatDate(record.effectiveAt) },
      { key: 'expires', label: 'Expires', value: formatDate(record.expiresAt) },
      { key: 'archived', label: 'Archived', value: formatDate(record.archivedAt) },
      { key: 'purged', label: 'Purged', value: formatDate(record.purgedAt) },
      { key: 'status', label: 'Current status', value: record.status },
      { key: 'access', label: 'Access events', value: `${relevantLogs.length} logged` },
    ]
  }, [record, relevantLogs.length])

  return (
    <div className="recordarr-page">
      <SectionHeader
        eyebrow="Records"
        title={record ? record.title : 'Record detail'}
        description={record ? `${record.recordNumber} · ${record.sourceProduct} · ${record.currentFileName}` : 'Inspect lifecycle state, retention, and access activity for a single record.'}
        action={record ? <span className="recordarr-pill"><History className="h-4 w-4" /> {record.status}</span> : null}
      />
      {recordQuery.isLoading ? <LoadingCard label="Loading record" /> : null}
      {recordQuery.isError ? (
        <ApiErrorCallout title="Unable to load record" message={getErrorMessage(recordQuery.error, 'Failed to load record.')} />
      ) : null}
      {record ? (
        <>
          <div className="recordarr-grid cols-3">
            <MetricCard title="Classification" value={record.classification} hint={`${record.recordType} / ${record.documentType}`} />
            <MetricCard title="Effective" value={formatDate(record.effectiveAt)} hint="Lifecycle start" />
            <MetricCard title="Expires" value={formatDate(record.expiresAt)} hint="Retention clock" />
          </div>
          <div className="recordarr-grid cols-2">
            <Card title="Record snapshot" icon={<FileText className="h-4 w-4 text-cyan-300" />}>
              <div className="space-y-3 text-sm text-slate-300">
                <p><strong className="text-slate-100">Source:</strong> {record.sourceProduct} · {record.sourceObjectDisplayName}</p>
                <p><strong className="text-slate-100">Object:</strong> {record.sourceObjectType} · {record.sourceObjectId}</p>
                <p><strong className="text-slate-100">Owner:</strong> {record.ownerPersonId}</p>
                <p><strong className="text-slate-100">Record ref:</strong> {record.recordRef?.recordarrRecordId ?? record.recordId}</p>
                <p><strong className="text-slate-100">Version:</strong> v{record.versionNumber}</p>
                <p><strong className="text-slate-100">Current ref:</strong> {record.currentVersionRef}</p>
                <p><strong className="text-slate-100">Audit trail:</strong> {record.auditTrail.length} entries</p>
                <div className="flex flex-wrap gap-2 pt-1">
                  {record.tags.map((tag) => (
                    <span key={tag} className="recordarr-pill text-[0.7rem]">{tag}</span>
                  ))}
                </div>
                <div className="grid gap-3 rounded-xl border border-slate-800/80 bg-slate-950/50 p-3 text-xs text-slate-300 md:grid-cols-2">
                  <div>
                    <p className="font-semibold text-slate-100">Record refs</p>
                    <p className="mt-1">Sources: {record.sourceObjectRefs.length} · Files: {record.fileRefs.length} · Versions: {record.versionRefs.length}</p>
                    <p className="mt-1">OCR: {record.ocrResultRefs.length} · Extraction: {record.extractionResultRefs.length} · Evidence: {record.evidenceMappingRefs.length}</p>
                    <div className="mt-2 flex flex-wrap gap-2">
                      {[...record.sourceObjectRefs, ...record.fileRefs, ...record.versionRefs].slice(0, 6).map((ref) => (
                        <span key={ref} className="recordarr-pill text-[0.65rem]">{ref}</span>
                      ))}
                    </div>
                    {record.recordRef ? (
                      <div className="mt-3 rounded-lg border border-slate-800/80 bg-slate-950/40 p-3">
                        <p className="font-semibold text-slate-100">Structured record ref</p>
                        <p className="mt-1">ID: {record.recordRef.recordarrRecordId}</p>
                        <p className="mt-1">Title: {record.recordRef.titleSnapshot}</p>
                        <p className="mt-1">Status: {record.recordRef.statusSnapshot} · Classification: {record.recordRef.classificationSnapshot}</p>
                        <p className="mt-1">Retention: {record.recordRef.retentionStatusSnapshot ?? 'n/a'} · Expires: {formatDate(record.recordRef.expiresAtSnapshot)}</p>
                      </div>
                    ) : null}
                  </div>
                  <div>
                    <p className="font-semibold text-slate-100">Governance refs</p>
                    <p className="mt-1">Packages: {record.packageRefs.length} · Legal holds: {record.legalHoldRefs.length} · Compliance: {record.complianceRefs.length}</p>
                    <p className="mt-1">Retention policy: {record.retentionPolicyRef ?? 'n/a'} · Access policy: {record.accessPolicyRef ?? 'n/a'}</p>
                    <div className="mt-2 flex flex-wrap gap-2">
                      {[...record.packageRefs, ...record.legalHoldRefs, ...record.complianceRefs].slice(0, 6).map((ref) => (
                        <span key={ref} className="recordarr-pill text-[0.65rem]">{ref}</span>
                      ))}
                    </div>
                  </div>
                </div>
              </div>
            </Card>
            <Card title="Lifecycle control" icon={<BadgeCheck className="h-4 w-4 text-cyan-300" />}>
              <div className="grid gap-3 md:grid-cols-2">
                <Field label="Status">
                  <select className="recordarr-select" value={status} onChange={(e) => setStatus(e.target.value)}>
                    <ReadableOption value="draft" />
                    <ReadableOption value="processing" />
                    <ReadableOption value="review" />
                    <ReadableOption value="active" />
                    <ReadableOption value="approved" />
                    <ReadableOption value="rejected" />
                    <ReadableOption value="superseded" />
                    <ReadableOption value="archived" />
                    <ReadableOption value="expired" />
                    <ReadableOption value="purged" />
                  </select>
                </Field>
                <Field label="Classification">
                  <select className="recordarr-select" value={classification} onChange={(e) => setClassification(e.target.value)}>
                    <ReadableOption value="public" />
                    <ReadableOption value="internal" />
                    <ReadableOption value="confidential" />
                    <ReadableOption value="restricted" />
                    <ReadableOption value="legal_hold" />
                  </select>
                </Field>
                <Field label="Current version"><input className="recordarr-input" value={`v${record.versionNumber}`} readOnly /></Field>
              </div>
              <div className="mt-4 flex flex-wrap gap-3">
                <button type="button" className="recordarr-button" onClick={() => updateMutation.mutate()} disabled={updateMutation.isPending}>
                  {updateMutation.isPending ? 'Updating...' : 'Update status'}
                </button>
                <button type="button" className="recordarr-button secondary" onClick={() => archiveMutation.mutate()} disabled={archiveMutation.isPending || Boolean(activeHold)}>
                  {archiveMutation.isPending ? 'Archiving...' : 'Archive record'}
                </button>
                <button type="button" className="recordarr-button secondary" onClick={() => purgeMutation.mutate()} disabled={purgeMutation.isPending || Boolean(activeHold)}>
                  {purgeMutation.isPending ? 'Purging...' : 'Purge record'}
                </button>
                {updateMutation.isError ? <span className="text-sm text-rose-300">{getErrorMessage(updateMutation.error, 'Update failed')}</span> : null}
                {archiveMutation.isError ? <span className="text-sm text-rose-300">{getErrorMessage(archiveMutation.error, 'Archive failed')}</span> : null}
                {purgeMutation.isError ? <span className="text-sm text-rose-300">{getErrorMessage(purgeMutation.error, 'Purge failed')}</span> : null}
              </div>
                {activeHold ? <p className="mt-3 text-sm text-amber-300">Blocked by legal hold {activeHold.holdNumber}.</p> : null}
            </Card>
            <Card title="Retention and access" icon={<LockKeyhole className="h-4 w-4 text-cyan-300" />}>
              <div className="space-y-3 text-sm text-slate-300">
                <p><strong className="text-slate-100">Policy:</strong> {retentionQuery.data?.retentionPolicyRef ?? 'n/a'}</p>
                <p><strong className="text-slate-100">Retention status:</strong> {retentionQuery.data?.status ?? 'n/a'}</p>
                <p><strong className="text-slate-100">Next review:</strong> {formatDate(retentionQuery.data?.nextReviewAt ?? null)}</p>
                <p><strong className="text-slate-100">Last reviewed:</strong> {formatDate(retentionQuery.data?.lastReviewedAt ?? null)}</p>
                <p><strong className="text-slate-100">Related uploads:</strong> {relatedUploads.length}</p>
                <p><strong className="text-slate-100">Related packages:</strong> {relatedPackages.length}</p>
              </div>
            </Card>
          </div>
          <div className="recordarr-grid cols-2">
            <Card title="Files and evidence capture" icon={<FileUp className="h-4 w-4 text-cyan-300" />}>
              <div className="space-y-4 text-sm text-slate-300">
                <div className="space-y-2">
                  <p><strong className="text-slate-100">Current file ref:</strong> {record.currentFileRef}</p>
                  <p><strong className="text-slate-100">All file refs:</strong> {record.fileRefs.join(', ') || 'none'}</p>
                  <p><strong className="text-slate-100">Current file:</strong> {currentFile ? `${currentFile.originalFilename} (${currentFile.mimeType})` : record.currentFileName}</p>
                </div>
                <div className="space-y-2">
                  <div className="flex items-center justify-between gap-3">
                    <h3 className="text-sm font-semibold text-slate-100">Files</h3>
                    <span className="recordarr-pill text-[0.7rem]">{recordFiles.length} file(s)</span>
                  </div>
                  <div className="space-y-2">
                    {recordFiles.length > 0 ? recordFiles.map((file) => (
                      <div key={file.fileId} className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3">
                        <div className="flex flex-wrap items-center justify-between gap-3">
                          <strong className="text-sm text-slate-100">{file.originalFilename}</strong>
                          <span className="recordarr-pill text-[0.7rem]">{file.processingStatus}</span>
                        </div>
                        <p className="mt-1 text-xs text-slate-400">{file.fileNumber} · {file.mimeType} · {file.storageProvider}</p>
                        <div className="mt-3 flex flex-wrap gap-2">
                          <button
                            type="button"
                            className="recordarr-button secondary"
                            onClick={async () => setSelectedFileDownload(await downloadFile(accessToken, file.fileId))}
                          >
                            Inspect download text
                          </button>
                        </div>
                      </div>
                    )) : <EmptyState title="No files attached yet." />}
                  </div>
                </div>
                {selectedFileDownload ? (
                  <pre className="max-h-48 overflow-auto rounded-xl border border-slate-700/70 bg-slate-950/80 p-3 text-xs text-slate-300 whitespace-pre-wrap">
                    {selectedFileDownload}
                  </pre>
                ) : null}
              </div>
            </Card>
            <Card title="Capture evidence" icon={<ScanSearch className="h-4 w-4 text-cyan-300" />}>
              <div className="space-y-5">
                <div className="space-y-3">
                  <h3 className="text-sm font-semibold text-slate-100">Signature</h3>
                  <div className="grid gap-3 md:grid-cols-2">
                    <Field label="Purpose"><select className="recordarr-select" value={signatureForm.signaturePurpose} onChange={(e) => setSignatureForm({ ...signatureForm, signaturePurpose: e.target.value })}><ReadableOption value="proof_of_delivery" /><ReadableOption value="proof_of_pickup" /><ReadableOption value="training_acknowledgement" /><ReadableOption value="work_order_closeout" /><ReadableOption value="inspection_attestation" /><ReadableOption value="quality_release" /><ReadableOption value="customer_acceptance" /><ReadableOption value="policy_acknowledgement" /><ReadableOption value="other" /></select></Field>
                    <Field label="Signer person"><PersonReferencePicker value={signatureForm.signerPersonId} onChange={(signerPersonId) => setSignatureForm({ ...signatureForm, signerPersonId })} /></Field>
                    <Field label="Signer name"><input className="recordarr-input" value={signatureForm.signerExternalName} onChange={(e) => setSignatureForm({ ...signatureForm, signerExternalName: e.target.value })} placeholder="Optional" /></Field>
                    <Field label="Signer title"><input className="recordarr-input" value={signatureForm.signerTitle} onChange={(e) => setSignatureForm({ ...signatureForm, signerTitle: e.target.value })} placeholder="Optional" /></Field>
                    <Field label="Attestation" wide><textarea className="recordarr-textarea" value={signatureForm.attestationText} onChange={(e) => setSignatureForm({ ...signatureForm, attestationText: e.target.value })} rows={3} /></Field>
                  </div>
                  <button type="button" className="recordarr-button secondary" onClick={() => createSignatureMutation.mutate()} disabled={createSignatureMutation.isPending}>
                    {createSignatureMutation.isPending ? 'Capturing...' : 'Capture signature'}
                  </button>
                </div>
                <div className="space-y-3 border-t border-slate-800 pt-4">
                  <h3 className="text-sm font-semibold text-slate-100">Photo evidence</h3>
                  <div className="grid gap-3 md:grid-cols-2">
                    <Field label="Purpose"><select className="recordarr-select" value={photoForm.photoPurpose} onChange={(e) => setPhotoForm({ ...photoForm, photoPurpose: e.target.value })}><ReadableOption value="defect" /><ReadableOption value="damage" /><ReadableOption value="completion" /><ReadableOption value="before" /><ReadableOption value="after" /><ReadableOption value="receipt" /><ReadableOption value="delivery" /><ReadableOption value="quality" /><ReadableOption value="incident" /><ReadableOption value="audit" /><ReadableOption value="training" /><ReadableOption value="other" /></select></Field>
                    <Field label="Captured by"><PersonReferencePicker value={photoForm.capturedByPersonId} onChange={(capturedByPersonId) => setPhotoForm({ ...photoForm, capturedByPersonId })} /></Field>
                    <Field label="Notes" wide><textarea className="recordarr-textarea" value={photoForm.notes} onChange={(e) => setPhotoForm({ ...photoForm, notes: e.target.value })} rows={3} /></Field>
                  </div>
                  <button type="button" className="recordarr-button secondary" onClick={() => createPhotoMutation.mutate()} disabled={createPhotoMutation.isPending}>
                    {createPhotoMutation.isPending ? 'Capturing...' : 'Capture photo evidence'}
                  </button>
                </div>
              </div>
            </Card>
          </div>
          <div className="recordarr-grid cols-2">
            <Card title="Record metadata" icon={<Settings className="h-4 w-4 text-cyan-300" />}>
              <div className="grid gap-3 md:grid-cols-2">
                <Field label="Key"><input className="recordarr-input" value={metadataForm.key} onChange={(e) => setMetadataForm({ ...metadataForm, key: e.target.value })} /></Field>
                <Field label="Value"><input className="recordarr-input" value={metadataForm.value} onChange={(e) => setMetadataForm({ ...metadataForm, value: e.target.value })} /></Field>
                <Field label="Value type"><select className="recordarr-select" value={metadataForm.valueType} onChange={(e) => setMetadataForm({ ...metadataForm, valueType: e.target.value })}><ReadableOption value="string" /><ReadableOption value="number" /><ReadableOption value="boolean" /><ReadableOption value="date" /><ReadableOption value="datetime" /><ReadableOption value="enum" /><ReadableOption value="object_ref" /></select></Field>
                <Field label="Source"><select className="recordarr-select" value={metadataForm.source} onChange={(e) => setMetadataForm({ ...metadataForm, source: e.target.value })}><ReadableOption value="user" /><ReadableOption value="source_product" /><ReadableOption value="ocr" /><ReadableOption value="extraction" /><ReadableOption value="system" /><ReadableOption value="import" /></select></Field>
                <Field label="Confidence"><input className="recordarr-input" type="number" min="0" max="1" step="0.01" value={metadataForm.confidenceScore} onChange={(e) => setMetadataForm({ ...metadataForm, confidenceScore: Number(e.target.value) })} /></Field>
                <Field label="Created by"><PersonReferencePicker value={metadataForm.createdByPersonId} onChange={(createdByPersonId) => setMetadataForm({ ...metadataForm, createdByPersonId })} /></Field>
              </div>
              <button type="button" className="recordarr-button secondary mt-3" onClick={() => createMetadataMutation.mutate()} disabled={createMetadataMutation.isPending}>
                {createMetadataMutation.isPending ? 'Saving...' : 'Add metadata'}
              </button>
              <div className="mt-4 space-y-2">
                {(metadataQuery.data ?? []).map((metadata) => (
                  <div key={metadata.metadataId} className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3 text-sm text-slate-300">
                    <div className="flex items-center justify-between gap-3">
                      <strong className="text-slate-100">{metadata.key}</strong>
                      <span className="recordarr-pill text-[0.7rem]">{metadata.valueType}</span>
                    </div>
                    <p className="mt-1">{metadata.value}</p>
                    <p className="mt-1 text-xs text-slate-400">{metadata.source} · {metadata.verified ? 'verified' : 'unverified'}</p>
                  </div>
                ))}
                {!metadataQuery.data?.length ? <EmptyState title="No metadata yet." /> : null}
              </div>
            </Card>
            <Card title="Record links" icon={<Settings className="h-4 w-4 text-cyan-300" />}>
              <div className="grid gap-3 md:grid-cols-2">
                <Field label="Linked record"><RecordReferencePicker value={linkForm.linkedRecordId} onChange={(linkedRecordId) => setLinkForm({ ...linkForm, linkedRecordId })} options={recordOptions} isLoading={recordOptionsLoading} /></Field>
                <Field label="Source object ref"><SourceObjectRefPicker value={linkForm.sourceObjectRef} onChange={(sourceObjectRef) => setLinkForm({ ...linkForm, sourceObjectRef })} /></Field>
                <Field label="Link type"><select className="recordarr-select" value={linkForm.linkType} onChange={(e) => setLinkForm({ ...linkForm, linkType: e.target.value })}><ReadableOption value="source" /><ReadableOption value="evidence_for" /><ReadableOption value="supersedes" /><ReadableOption value="duplicate_of" /><ReadableOption value="attachment_to" /><ReadableOption value="package_member" /><ReadableOption value="generated_from" /><ReadableOption value="redacted_from" /><ReadableOption value="related_to" /></select></Field>
                <Field label="Created by"><PersonReferencePicker value={linkForm.createdByPersonId} onChange={(createdByPersonId) => setLinkForm({ ...linkForm, createdByPersonId })} /></Field>
              </div>
              <button type="button" className="recordarr-button secondary mt-3" onClick={() => createLinkMutation.mutate()} disabled={createLinkMutation.isPending}>
                {createLinkMutation.isPending ? 'Saving...' : 'Add link'}
              </button>
              <div className="mt-4 space-y-2">
                {(linksQuery.data ?? []).map((link) => (
                  <div key={link.recordLinkId} className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3 text-sm text-slate-300">
                    <div className="flex items-center justify-between gap-3">
                      <strong className="text-slate-100">{link.linkType}</strong>
                      <span className="recordarr-pill text-[0.7rem]">{formatDate(link.createdAt)}</span>
                    </div>
                    <p className="mt-1">{link.linkedRecordId ?? link.sourceObjectRef ?? 'Unspecified link'}</p>
                    <p className="mt-1 text-xs text-slate-400">Created by {link.createdByPersonId}</p>
                  </div>
                ))}
                {!linksQuery.data?.length ? <EmptyState title="No record links yet." /> : null}
              </div>
            </Card>
          </div>
          <Card title="Record comments" icon={<MessageSquare className="h-4 w-4 text-cyan-300" />}>
            <div className="grid gap-3 md:grid-cols-2">
              <Field label="Body" wide>
                <textarea
                  className="recordarr-textarea"
                  value={commentForm.body}
                  onChange={(e) => setCommentForm({ ...commentForm, body: e.target.value })}
                  rows={4}
                />
              </Field>
              <div className="grid gap-3 md:grid-cols-2">
                <Field label="Visibility">
                  <select className="recordarr-select" value={commentForm.visibility} onChange={(e) => setCommentForm({ ...commentForm, visibility: e.target.value })}>
                    <ReadableOption value="internal" />
                    <ReadableOption value="auditor_visible" />
                    <ReadableOption value="product_visible" />
                    <ReadableOption value="customer_visible" />
                    <ReadableOption value="supplier_visible" />
                  </select>
                </Field>
                <Field label="Comment author">
                  <PersonReferencePicker value={commentForm.actorPersonId} onChange={(actorPersonId) => setCommentForm({ ...commentForm, actorPersonId })} />
                </Field>
              </div>
            </div>
            <div className="mt-3 flex flex-wrap gap-3">
              <button type="button" className="recordarr-button secondary" onClick={() => saveCommentMutation.mutate()} disabled={saveCommentMutation.isPending}>
                {saveCommentMutation.isPending ? 'Saving...' : editingCommentId ? 'Save comment' : 'Add comment'}
              </button>
              {editingCommentId ? (
                <button
                  type="button"
                  className="recordarr-button"
                  onClick={() => {
                    setEditingCommentId('')
                    setCommentForm({ body: '', visibility: 'internal', actorPersonId })
                  }}
                >
                  Cancel edit
                </button>
              ) : null}
              {saveCommentMutation.isError ? <span className="text-sm text-rose-300">{getErrorMessage(saveCommentMutation.error, 'Comment save failed')}</span> : null}
            </div>
            <div className="mt-4 space-y-2">
              {recordComments.length > 0 ? recordComments.map((comment) => (
                <div key={comment.commentId} className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3 text-sm text-slate-300">
                  <div className="flex flex-wrap items-center justify-between gap-3">
                    <strong className="text-slate-100">{comment.createdByPersonId}</strong>
                    <span className="recordarr-pill text-[0.7rem]">{comment.visibility}</span>
                  </div>
                  <p className="mt-2 whitespace-pre-wrap">{comment.body}</p>
                  <p className="mt-2 text-xs text-slate-400">
                    Created {formatDate(comment.createdAt)}{comment.editedAt ? ` · Edited ${formatDate(comment.editedAt)} by ${comment.editedByPersonId ?? 'n/a'}` : ''}
                  </p>
                  <div className="mt-3 flex flex-wrap gap-2">
                    <button
                      type="button"
                      className="recordarr-button secondary"
                      onClick={() => setEditingCommentId(comment.commentId)}
                    >
                      Edit
                    </button>
                  </div>
                </div>
              )) : <EmptyState title="No comments yet." />}
            </div>
            {selectedComment ? (
              <p className="mt-3 text-xs text-slate-400">
                Editing {selectedComment.commentId}. The form is prefilled with the current comment body and visibility.
              </p>
            ) : null}
          </Card>
          <div className="recordarr-grid cols-2">
            <Card title="Evidence and packaging" icon={<PackageSearch className="h-4 w-4 text-cyan-300" />}>
              <div className="space-y-3">
                <div>
                  <h3 className="text-sm font-semibold text-slate-100">Evidence mappings</h3>
                  <div className="mt-2 space-y-2">
                    {relatedMappings.length > 0 ? relatedMappings.map((mapping) => (
                      <div key={mapping.evidenceMappingId} className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3 text-sm text-slate-300">
                        <div className="flex items-center justify-between gap-3">
                          <strong className="text-slate-100">{mapping.complianceRequirementRef}</strong>
                          <span className="recordarr-pill text-[0.7rem]">{mapping.status}</span>
                        </div>
                        <p className="mt-1">{mapping.evidenceTypeKey} · {mapping.mappingSource}</p>
                      </div>
                    )) : <EmptyState title="No evidence mappings for this record." />}
                  </div>
                </div>
                <div>
                  <h3 className="text-sm font-semibold text-slate-100">Packages</h3>
                  <div className="mt-2 space-y-2">
                    {relatedPackages.length > 0 ? relatedPackages.map((pkg) => (
                      <div key={pkg.packageId} className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3 text-sm text-slate-300">
                        <div className="flex items-center justify-between gap-3">
                          <strong className="text-slate-100">{pkg.packageNumber}</strong>
                          <span className="recordarr-pill text-[0.7rem]">{pkg.status}</span>
                        </div>
                        <p className="mt-1">{pkg.title}</p>
                      </div>
                    )) : <EmptyState title="No packages include this record." />}
                  </div>
                </div>
              </div>
            </Card>
            <Card title="Capture and processing" icon={<ScanSearch className="h-4 w-4 text-cyan-300" />}>
              <div className="space-y-3">
                <div>
                  <h3 className="text-sm font-semibold text-slate-100">Scans</h3>
                  <div className="mt-2 space-y-2">
                    {relatedScans.length > 0 ? relatedScans.map((scan) => (
                      <div key={scan.scanProcessingId} className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3 text-sm text-slate-300">
                        <div className="flex items-center justify-between gap-3">
                          <strong className="text-slate-100">{scan.originalFileName}</strong>
                          <span className="recordarr-pill text-[0.7rem]">{scan.status}</span>
                        </div>
                        <p className="mt-1">{scan.scanPurpose} · confidence {scan.confidenceScore.toFixed(2)}</p>
                        <p className="mt-1 text-xs text-slate-400">Edge {scan.edgeDetectionResult?.status ?? 'pending'} · enhancement {scan.enhancementSettings?.outputFormat ?? 'pending'}</p>
                      </div>
                    )) : <EmptyState title="No scans for this record." />}
                  </div>
                </div>
                <div>
                  <h3 className="text-sm font-semibold text-slate-100">Uploads and controlled docs</h3>
                  <div className="mt-2 space-y-2">
                    {relatedUploads.length > 0 ? relatedUploads.map((upload) => (
                      <div key={upload.uploadSessionId} className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3 text-sm text-slate-300">
                        <div className="flex items-center justify-between gap-3">
                          <strong className="text-slate-100">{upload.uploadSessionNumber}</strong>
                          <span className="recordarr-pill text-[0.7rem]">{upload.status}</span>
                        </div>
                        <p className="mt-1">{upload.uploadPurpose}</p>
                      </div>
                    )) : <EmptyState title="No upload sessions for this record." />}
                    {relatedDocuments.length > 0 ? relatedDocuments.map((document) => (
                      <div key={document.controlledDocumentId} className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3 text-sm text-slate-300">
                        <div className="flex items-center justify-between gap-3">
                          <strong className="text-slate-100">{document.documentNumber}</strong>
                          <span className="recordarr-pill text-[0.7rem]">{document.status}</span>
                        </div>
                        <p className="mt-1">{document.title}</p>
                      </div>
                    )) : null}
                  </div>
                </div>
              </div>
            </Card>
          </div>
          <Card title="Timeline" icon={<History className="h-4 w-4 text-cyan-300" />}>
            <div className="grid gap-3 md:grid-cols-2 lg:grid-cols-4">
              {timeline.map((entry) => (
                <div key={entry.key} className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3 text-sm text-slate-300">
                  <p className="text-xs uppercase tracking-[0.16em] text-[var(--color-text-muted)]">{entry.label}</p>
                  <p className="mt-1 text-slate-100">{entry.value}</p>
                </div>
              ))}
            </div>
          </Card>
          <Card title="Access trail" icon={<History className="h-4 w-4 text-cyan-300" />}>
            {relevantLogs.length === 0 ? <EmptyState title="No access logs for this record." /> : (
              <div className="space-y-3">
                {relevantLogs.slice(0, 8).map((entry) => (
                  <div key={entry.accessLogId} className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3">
                    <div className="flex items-center justify-between gap-3">
                      <strong className="text-sm text-slate-100">{entry.action}</strong>
                      <span className="recordarr-pill text-[0.7rem]">{entry.result}</span>
                    </div>
                    <p className="mt-1 text-sm text-slate-300">{entry.reasonCode ?? 'no reason code'}</p>
                    <p className="mt-2 text-xs text-slate-400">{formatDate(entry.occurredAt)} · {entry.actorPersonId ?? entry.actorServiceClientId ?? 'unknown actor'}</p>
                  </div>
                ))}
              </div>
            )}
          </Card>
        </>
      ) : null}
    </div>
  )
}

function CapturePage({ accessToken, actorPersonId }: WorkspacePageProps) {
  const queryClient = useQueryClient()
  const { options: recordOptions, isLoading: recordOptionsLoading } = useRecordReferenceOptions(accessToken)
  const [upload, setUpload] = useState({
    sourceProduct: '',
    sourceObjectType: '',
    sourceObjectId: '',
    uploadPurpose: '',
    requiresDocumentScan: true,
    requiresOcr: true,
    requiresManualReview: true,
  })
  const [captureRequest, setCaptureRequest] = useState({
    sourceProduct: '',
    sourceObjectRef: '',
    captureType: 'photo',
    title: '',
    instructions: '',
    required: true,
    uploadSessionRef: '',
    evidenceRequirementRef: '',
  })
  const [selectedScanId, setSelectedScanId] = useState('')
  const [scan, setScan] = useState({
    recordId: '',
    originalFileName: '',
    scanPurpose: '',
    edgeCoordinates: '',
    correctedByPersonId: actorPersonId,
  })
  const [mapping, setMapping] = useState({
    recordId: '',
    sourceProduct: '',
    sourceObjectType: '',
    sourceObjectId: '',
    complianceRequirementRef: '',
    evidenceTypeKey: '',
    mappingSource: 'manual_review',
    confidenceScore: 1,
  })
  const [extractionReview, setExtractionReview] = useState({
    reviewedByPersonId: actorPersonId,
    status: 'completed',
    failureReason: '',
  })

  const uploadSessionsQuery = useQuery({
    queryKey: ['recordarr', 'upload-sessions'],
    queryFn: () => listUploadSessions(accessToken),
    enabled: Boolean(accessToken),
  })
  const uploadSessionOptions = useMemo(
    () => (uploadSessionsQuery.data ?? []).map(toUploadSessionOption),
    [uploadSessionsQuery.data],
  )
  const captureRequestsQuery = useQuery({
    queryKey: ['recordarr', 'capture-requests'],
    queryFn: () => listCaptureRequests(accessToken),
    enabled: Boolean(accessToken),
  })
  const scansQuery = useQuery({
    queryKey: ['recordarr', 'scans'],
    queryFn: () => listScans(accessToken),
    enabled: Boolean(accessToken),
  })
  const mappingsQuery = useQuery({
    queryKey: ['recordarr', 'evidence-mappings'],
    queryFn: () => listEvidenceMappings(accessToken),
    enabled: Boolean(accessToken),
  })
  const coverageQuery = useQuery({
    queryKey: ['recordarr', 'evidence-coverage'],
    queryFn: () => listEvidenceCoverage(accessToken),
    enabled: Boolean(accessToken),
  })

  useEffect(() => {
    if (!selectedScanId && scansQuery.data?.[0]) {
      setSelectedScanId(scansQuery.data[0].scanProcessingId)
    }
  }, [scansQuery.data, selectedScanId])

  const uploadMutation = useMutation({
    mutationFn: () => createUploadSession(accessToken, upload),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const captureRequestMutation = useMutation({
    mutationFn: () => createCaptureRequest(accessToken, captureRequest),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const completeCaptureRequestMutation = useMutation({
    mutationFn: (captureRequestId: string) => completeCaptureRequest(accessToken, captureRequestId),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const skipCaptureRequestMutation = useMutation({
    mutationFn: (captureRequestId: string) => skipCaptureRequest(accessToken, captureRequestId),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const cancelCaptureRequestMutation = useMutation({
    mutationFn: (captureRequestId: string) => cancelCaptureRequest(accessToken, captureRequestId),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const expireCaptureRequestMutation = useMutation({
    mutationFn: (captureRequestId: string) => expireCaptureRequest(accessToken, captureRequestId),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const scanMutation = useMutation({
    mutationFn: () => createScan(accessToken, scan),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const correctionMutation = useMutation({
    mutationFn: () =>
      applyManualCorrection(accessToken, selectedScan?.scanProcessingId ?? '', {
        edgeCoordinates: scan.edgeCoordinates,
        correctedByPersonId: scan.correctedByPersonId,
      }),
  })
  const selectedScan = scansQuery.data?.find((entry) => entry.scanProcessingId === selectedScanId) ?? null
  const ocrQuery = useQuery({
    queryKey: ['recordarr', 'ocr-result', selectedScan?.ocrResultId],
    queryFn: () => getOcrResult(accessToken, selectedScan!.ocrResultId!),
    enabled: Boolean(accessToken && selectedScan?.ocrResultId),
  })
  const extractionQuery = useQuery({
    queryKey: ['recordarr', 'extraction-result', selectedScan?.extractionResultId],
    queryFn: () => getExtractionResult(accessToken, selectedScan!.extractionResultId!),
    enabled: Boolean(accessToken && selectedScan?.extractionResultId),
  })
  const reviewExtractionMutation = useMutation({
    mutationFn: () =>
      reviewExtractionResult(accessToken, extractionQuery.data?.extractionResultId ?? '', {
        reviewedByPersonId: extractionReview.reviewedByPersonId,
        status: extractionReview.status,
        failureReason: extractionReview.failureReason || null,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const mappingMutation = useMutation({
    mutationFn: () => createEvidenceMapping(accessToken, mapping),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })

  return (
    <div className="recordarr-page">
      <SectionHeader
        eyebrow="Capture"
        title="Upload, scan, and evidence mapping"
        description="Create capture sessions, track OCR and scan processing, and map records back to compliance requirements."
        action={<span className="recordarr-pill"><Upload className="h-4 w-4" /> {uploadSessionsQuery.data?.length ?? 0} sessions</span>}
      />
      <Card title="Capture requests" icon={<FileUp className="h-4 w-4 text-cyan-300" />}>
        <div className="grid gap-3 md:grid-cols-2">
          <Field label="Source product"><SourceProductPicker value={captureRequest.sourceProduct} onChange={(sourceProduct) => setCaptureRequest({ ...captureRequest, sourceProduct, sourceObjectRef: '' })} /></Field>
          <Field label="Source object ref">
            <SourceObjectRefPicker
              value={captureRequest.sourceObjectRef}
              sourceProduct={captureRequest.sourceProduct}
              onChange={(sourceObjectRef, selected) => setCaptureRequest({ ...captureRequest, sourceProduct: selected?.sourceProduct ?? captureRequest.sourceProduct, sourceObjectRef })}
            />
          </Field>
          <Field label="Capture type">
            <select className="recordarr-select" value={captureRequest.captureType} onChange={(e) => setCaptureRequest({ ...captureRequest, captureType: e.target.value })}>
              <ReadableOption value="photo" />
              <ReadableOption value="document_scan" />
              <ReadableOption value="signature" />
              <ReadableOption value="video" />
              <ReadableOption value="audio" />
              <ReadableOption value="file_upload" />
              <ReadableOption value="generated_pdf" />
            </select>
          </Field>
          <Field label="Required"><select className="recordarr-select" value={String(captureRequest.required)} onChange={(e) => setCaptureRequest({ ...captureRequest, required: e.target.value === 'true' })}><option value="true">Yes</option><option value="false">No</option></select></Field>
          <Field label="Title"><input className="recordarr-input" value={captureRequest.title} onChange={(e) => setCaptureRequest({ ...captureRequest, title: e.target.value })} /></Field>
          <Field label="Upload session"><UploadSessionReferencePicker value={captureRequest.uploadSessionRef} onChange={(uploadSessionRef) => setCaptureRequest({ ...captureRequest, uploadSessionRef })} options={uploadSessionOptions} /></Field>
          <Field label="Evidence requirement ref"><input className="recordarr-input" value={captureRequest.evidenceRequirementRef} onChange={(e) => setCaptureRequest({ ...captureRequest, evidenceRequirementRef: e.target.value })} /></Field>
          <Field label="Instructions" wide><textarea className="recordarr-textarea" value={captureRequest.instructions} onChange={(e) => setCaptureRequest({ ...captureRequest, instructions: e.target.value })} /></Field>
        </div>
        <div className="mt-4 flex flex-wrap gap-3">
          <button type="button" className="recordarr-button" onClick={() => captureRequestMutation.mutate()} disabled={captureRequestMutation.isPending}>
            {captureRequestMutation.isPending ? 'Creating...' : 'Create request'}
          </button>
          {captureRequestMutation.isError ? <span className="text-sm text-rose-300">{getErrorMessage(captureRequestMutation.error, 'Create failed')}</span> : null}
        </div>
        <div className="mt-4 grid gap-3 md:grid-cols-2 xl:grid-cols-3">
          {captureRequestsQuery.data?.map((request) => (
            <div key={request.captureRequestId} className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3 text-sm text-slate-300">
              <div className="flex items-center justify-between gap-3">
                <strong className="text-slate-100">{request.title}</strong>
                <span className="recordarr-pill text-[0.7rem]">{request.status}</span>
              </div>
              <p className="mt-1">{request.captureType} · {request.sourceObjectRef}</p>
              <p className="mt-1 text-xs text-slate-400">Required: {request.required ? 'yes' : 'no'} · Session: {request.uploadSessionRef ?? 'none'}</p>
              <p className="mt-2 text-xs leading-5 text-slate-400">{request.instructions}</p>
              <div className="mt-3 flex flex-wrap gap-2">
                <button type="button" className="recordarr-button secondary" onClick={() => completeCaptureRequestMutation.mutate(request.captureRequestId)} disabled={completeCaptureRequestMutation.isPending || request.status !== 'open'}>
                  Complete
                </button>
                <button type="button" className="recordarr-button secondary" onClick={() => skipCaptureRequestMutation.mutate(request.captureRequestId)} disabled={skipCaptureRequestMutation.isPending || request.status !== 'open'}>
                  Skip
                </button>
                <button type="button" className="recordarr-button secondary" onClick={() => cancelCaptureRequestMutation.mutate(request.captureRequestId)} disabled={cancelCaptureRequestMutation.isPending || request.status !== 'open'}>
                  Cancel
                </button>
                <button type="button" className="recordarr-button secondary" onClick={() => expireCaptureRequestMutation.mutate(request.captureRequestId)} disabled={expireCaptureRequestMutation.isPending || request.status !== 'open'}>
                  Expire
                </button>
              </div>
            </div>
          ))}
          {!captureRequestsQuery.data?.length && !captureRequestsQuery.isLoading ? <EmptyState title="No capture requests yet." /> : null}
        </div>
      </Card>
      <div className="recordarr-grid cols-2">
        <Card title="Upload session" icon={<FileUp className="h-4 w-4 text-cyan-300" />}>
          <div className="grid gap-3 md:grid-cols-2">
            <Field label="Source product"><SourceProductPicker value={upload.sourceProduct} onChange={(sourceProduct) => setUpload({ ...upload, sourceProduct, sourceObjectType: '', sourceObjectId: '' })} /></Field>
            <Field label="Source reference">
              <SourceObjectRefPicker
                value={buildSourceObjectRef(upload.sourceProduct, upload.sourceObjectType, upload.sourceObjectId)}
                sourceProduct={upload.sourceProduct}
                onChange={(_, selected) => {
                  if (!selected) return
                  setUpload({
                    ...upload,
                    sourceProduct: selected.sourceProduct,
                    sourceObjectType: selected.sourceObjectType,
                    sourceObjectId: selected.sourceObjectId,
                  })
                }}
              />
            </Field>
            <Field label="Purpose"><input className="recordarr-input" value={upload.uploadPurpose} onChange={(e) => setUpload({ ...upload, uploadPurpose: e.target.value })} /></Field>
            <Field label="Requires document scan"><select className="recordarr-select" value={String(upload.requiresDocumentScan)} onChange={(e) => setUpload({ ...upload, requiresDocumentScan: e.target.value === 'true' })}><option value="true">Yes</option><option value="false">No</option></select></Field>
            <Field label="Requires OCR"><select className="recordarr-select" value={String(upload.requiresOcr)} onChange={(e) => setUpload({ ...upload, requiresOcr: e.target.value === 'true' })}><option value="true">Yes</option><option value="false">No</option></select></Field>
            <Field label="Manual review"><select className="recordarr-select" value={String(upload.requiresManualReview)} onChange={(e) => setUpload({ ...upload, requiresManualReview: e.target.value === 'true' })}><option value="true">Yes</option><option value="false">No</option></select></Field>
          </div>
          <div className="mt-4 flex flex-wrap gap-3">
            <button type="button" className="recordarr-button" onClick={() => uploadMutation.mutate()} disabled={uploadMutation.isPending}>
              {uploadMutation.isPending ? 'Creating...' : 'Create session'}
            </button>
            {uploadMutation.isError ? <span className="text-sm text-rose-300">{getErrorMessage(uploadMutation.error, 'Create failed')}</span> : null}
          </div>
          <div className="mt-4 space-y-3">
            {uploadSessionsQuery.data?.map((session) => (
              <div key={session.uploadSessionId} className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3 text-sm text-slate-300">
                <div className="flex items-center justify-between gap-3">
                  <strong className="text-slate-100">{session.uploadSessionNumber}</strong>
                  <span className="recordarr-pill text-[0.7rem]">{session.status}</span>
                </div>
                <p className="mt-1">{session.uploadPurpose} · {session.sourceProduct}</p>
              </div>
            ))}
            {!uploadSessionsQuery.data?.length && !uploadSessionsQuery.isLoading ? <EmptyState title="No upload sessions yet." /> : null}
          </div>
        </Card>

        <Card title="Scan processing" icon={<ScanSearch className="h-4 w-4 text-cyan-300" />}>
          <div className="grid gap-3 md:grid-cols-2">
            <Field label="Record"><RecordReferencePicker value={scan.recordId} onChange={(recordId) => setScan({ ...scan, recordId })} options={recordOptions} isLoading={recordOptionsLoading} /></Field>
            <Field label="Original file"><input className="recordarr-input" value={scan.originalFileName} onChange={(e) => setScan({ ...scan, originalFileName: e.target.value })} /></Field>
            <Field label="Scan purpose"><input className="recordarr-input" value={scan.scanPurpose} onChange={(e) => setScan({ ...scan, scanPurpose: e.target.value })} /></Field>
            <Field label="Edge coordinates" wide><input className="recordarr-input" value={scan.edgeCoordinates} onChange={(e) => setScan({ ...scan, edgeCoordinates: e.target.value })} /></Field>
            <Field label="Corrected by"><PersonReferencePicker value={scan.correctedByPersonId} onChange={(correctedByPersonId) => setScan({ ...scan, correctedByPersonId })} /></Field>
            <Field label="Selected scan">
              <select className="recordarr-select" value={selectedScanId} onChange={(e) => setSelectedScanId(e.target.value)}>
                {scansQuery.data?.map((entry) => (
                  <option key={entry.scanProcessingId} value={entry.scanProcessingId}>
                    {entry.originalFileName} · {entry.status}
                  </option>
                ))}
              </select>
            </Field>
          </div>
          <div className="mt-4 flex flex-wrap gap-3">
            <button type="button" className="recordarr-button" onClick={() => scanMutation.mutate()} disabled={scanMutation.isPending}>
              {scanMutation.isPending ? 'Scanning...' : 'Create scan'}
            </button>
            <button type="button" className="recordarr-button secondary" onClick={() => correctionMutation.mutate()} disabled={correctionMutation.isPending || !selectedScan}>
              Manual correction
            </button>
          </div>
          <div className="mt-4 space-y-3">
            {scansQuery.data?.map((entry) => (
              <button
                key={entry.scanProcessingId}
                type="button"
                className={[
                  'w-full rounded-xl border p-3 text-left text-sm transition-colors',
                  entry.scanProcessingId === selectedScanId
                    ? 'border-cyan-400/40 bg-cyan-500/10 text-slate-200'
                    : 'border-slate-700/70 bg-slate-900/70 text-slate-300 hover:bg-slate-900/90',
                ].join(' ')}
                onClick={() => setSelectedScanId(entry.scanProcessingId)}
              >
                <div className="flex items-center justify-between gap-3">
                  <strong className="text-slate-100">{entry.originalFileName}</strong>
                  <span className="recordarr-pill text-[0.7rem]">{entry.status}</span>
                </div>
                <p className="mt-1">{entry.scanPurpose} · confidence {entry.confidenceScore.toFixed(2)}</p>
                <p className="mt-1 text-xs text-slate-400">
                  OCR {entry.ocrResultId ?? 'pending'} · extraction {entry.extractionResultId ?? 'pending'} · edge {entry.edgeDetectionResult?.status ?? 'pending'}
                </p>
              </button>
            ))}
            {!scansQuery.data?.length && !scansQuery.isLoading ? <EmptyState title="No scan processing yet." /> : null}
          </div>
        </Card>
      </div>
      <div className="mt-6 grid gap-6 lg:grid-cols-2">
        <Card title="OCR and extraction review" icon={<FileText className="h-4 w-4 text-cyan-300" />}>
          {selectedScan ? (
            <div className="space-y-4">
              <div className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3 text-sm text-slate-300">
                <div className="flex items-center justify-between gap-3">
                  <strong className="text-slate-100">{selectedScan.originalFileName}</strong>
                  <span className="recordarr-pill text-[0.7rem]">{selectedScan.status}</span>
                </div>
                <p className="mt-1">OCR result {selectedScan.ocrResultId ?? 'pending'} · extraction result {selectedScan.extractionResultId ?? 'pending'}</p>
                <p className="mt-1 text-xs text-slate-400">
                  Manual correction: {selectedScan.manualEdgeCoordinates ? `${selectedScan.manualEdgeCoordinates} · ${selectedScan.correctedByPersonId ?? 'unknown'} · ${formatDate(selectedScan.correctedAt)}` : 'not yet corrected'}
                </p>
              </div>
              <div className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3 text-sm text-slate-300">
                <div className="flex items-center justify-between gap-3">
                  <strong className="text-slate-100">OCR result</strong>
                  <span className="recordarr-pill text-[0.7rem]">{ocrQuery.data?.status ?? 'unloaded'}</span>
                </div>
                {ocrQuery.data ? (
                  <div className="mt-3 space-y-2">
                    <p><strong className="text-slate-100">Engine:</strong> {ocrQuery.data.engine}</p>
                    <p><strong className="text-slate-100">Confidence:</strong> {ocrQuery.data.confidenceScore.toFixed(2)}</p>
                    <p><strong className="text-slate-100">Language:</strong> {ocrQuery.data.language}</p>
                    <p><strong className="text-slate-100">Extracted:</strong> {formatDate(ocrQuery.data.extractedAt)}</p>
                    <p className="rounded-lg border border-slate-700/60 bg-slate-950/60 p-3 text-xs leading-6 text-slate-300">{ocrQuery.data.fullText}</p>
                    <div className="rounded-lg border border-slate-700/60 bg-slate-950/60 p-3 text-xs leading-6 text-slate-300">
                      <p className="font-medium text-slate-100">Page results</p>
                      <div className="mt-2 space-y-2">
                        {ocrQuery.data.pageResults.map((page) => (
                          <div key={page.pageResultId} className="rounded-md border border-slate-700/50 bg-slate-900/60 p-2">
                            <p className="text-slate-100">Page {page.pageNumber} · confidence {page.confidenceScore.toFixed(2)}</p>
                            <p className="mt-1">{page.text}</p>
                            <p className="mt-1 text-[0.7rem] text-slate-400">Blocks: {page.blocks.join(' | ') || 'none'}</p>
                          </div>
                        ))}
                      </div>
                      <p className="mt-3 font-medium text-slate-100">Block results</p>
                      <p className="mt-1">{ocrQuery.data.blockResults.join(' | ') || 'none'}</p>
                    </div>
                  </div>
                ) : (
                  <div className="mt-3"><EmptyState title="Select a scan with an OCR result." /></div>
                )}
              </div>
              <div className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3 text-sm text-slate-300">
                <div className="flex items-center justify-between gap-3">
                  <strong className="text-slate-100">Edge detection and enhancement</strong>
                  <span className="recordarr-pill text-[0.7rem]">{selectedScan.edgeDetectionResult?.status ?? 'unloaded'}</span>
                </div>
                {selectedScan.edgeDetectionResult && selectedScan.enhancementSettings ? (
                  <div className="mt-3 space-y-2">
                    <p><strong className="text-slate-100">Confidence:</strong> {selectedScan.edgeDetectionResult.confidenceScore.toFixed(2)}</p>
                    <p><strong className="text-slate-100">Corners:</strong> {selectedScan.edgeDetectionResult.corners ?? 'n/a'}</p>
                    <p><strong className="text-slate-100">Manual correction:</strong> {selectedScan.edgeDetectionResult.requiresManualCorrection ? 'required' : 'not required'}</p>
                    <p><strong className="text-slate-100">Enhancement:</strong> {selectedScan.enhancementSettings.outputFormat} · crop {selectedScan.enhancementSettings.cropApplied ? 'yes' : 'no'} · perspective {selectedScan.enhancementSettings.perspectiveCorrectionApplied ? 'yes' : 'no'}</p>
                  </div>
                ) : (
                  <div className="mt-3"><EmptyState title="Select a scan with edge detection metadata." /></div>
                )}
              </div>
              <div className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3 text-sm text-slate-300">
                <div className="flex items-center justify-between gap-3">
                  <strong className="text-slate-100">Extraction result</strong>
                  <span className="recordarr-pill text-[0.7rem]">{formatDisplayLabel(extractionQuery.data?.status ?? 'unloaded')}</span>
                </div>
                {extractionQuery.data ? (
                  <div className="mt-3 space-y-3">
                    <div className="grid gap-3 md:grid-cols-2">
                      <Field label="Reviewed by"><PersonReferencePicker value={extractionReview.reviewedByPersonId} onChange={(reviewedByPersonId) => setExtractionReview({ ...extractionReview, reviewedByPersonId })} /></Field>
                      <Field label="Review status">
                        <select className="recordarr-select" value={extractionReview.status} onChange={(e) => setExtractionReview({ ...extractionReview, status: e.target.value })}>
                          <ReadableOption value="completed" />
                          <ReadableOption value="manual_review_required" />
                          <ReadableOption value="failed" />
                        </select>
                      </Field>
                      <Field label="Failure reason" wide><input className="recordarr-input" value={extractionReview.failureReason} onChange={(e) => setExtractionReview({ ...extractionReview, failureReason: e.target.value })} /></Field>
                    </div>
                    <button type="button" className="recordarr-button secondary" onClick={() => reviewExtractionMutation.mutate()} disabled={reviewExtractionMutation.isPending}>
                      {reviewExtractionMutation.isPending ? 'Saving...' : 'Save review'}
                    </button>
                    <div className="space-y-2">
                      <p><strong className="text-slate-100">Type:</strong> {extractionQuery.data.extractionType}</p>
                      <p><strong className="text-slate-100">Confidence:</strong> {extractionQuery.data.confidenceScore.toFixed(2)}</p>
                      <p><strong className="text-slate-100">Extracted:</strong> {formatDate(extractionQuery.data.extractedAt)}</p>
                      <p><strong className="text-slate-100">Reviewed by:</strong> {extractionQuery.data.reviewedByPersonId ?? 'n/a'}</p>
                      <div className="flex flex-wrap gap-2">
                        {extractionQuery.data.extractedFields.map((field) => (
                          <span key={field.extractedFieldId} className="recordarr-pill text-[0.7rem]" title={`Page ${field.pageNumber ?? 'n/a'} · ${field.boundingBox ?? 'no bounding box'}`}>
                            {field.label}: {field.value}
                          </span>
                        ))}
                      </div>
                    </div>
                  </div>
                ) : (
                  <div className="mt-3"><EmptyState title="Select a scan with an extraction result." /></div>
                )}
              </div>
            </div>
          ) : (
            <EmptyState title="Select a scan to review OCR and extraction results." />
          )}
        </Card>
      </div>
      <Card title="Evidence mappings" icon={<BadgeCheck className="h-4 w-4 text-cyan-300" />}>
        <div className="grid gap-3 md:grid-cols-2">
          <Field label="Record"><RecordReferencePicker value={mapping.recordId} onChange={(recordId) => setMapping({ ...mapping, recordId })} options={recordOptions} isLoading={recordOptionsLoading} /></Field>
          <Field label="Source product"><SourceProductPicker value={mapping.sourceProduct} onChange={(sourceProduct) => setMapping({ ...mapping, sourceProduct, sourceObjectType: '', sourceObjectId: '' })} /></Field>
          <Field label="Source reference">
            <SourceObjectRefPicker
              value={buildSourceObjectRef(mapping.sourceProduct, mapping.sourceObjectType, mapping.sourceObjectId)}
              sourceProduct={mapping.sourceProduct}
              onChange={(_, selected) => {
                if (!selected) return
                setMapping({
                  ...mapping,
                  sourceProduct: selected.sourceProduct,
                  sourceObjectType: selected.sourceObjectType,
                  sourceObjectId: selected.sourceObjectId,
                })
              }}
            />
          </Field>
          <Field label="Requirement ref"><input className="recordarr-input" value={mapping.complianceRequirementRef} onChange={(e) => setMapping({ ...mapping, complianceRequirementRef: e.target.value })} /></Field>
          <Field label="Evidence type"><input className="recordarr-input" value={mapping.evidenceTypeKey} onChange={(e) => setMapping({ ...mapping, evidenceTypeKey: e.target.value })} /></Field>
          <Field label="Mapping source"><input className="recordarr-input" value={mapping.mappingSource} onChange={(e) => setMapping({ ...mapping, mappingSource: e.target.value })} /></Field>
          <Field label="Confidence"><input className="recordarr-input" type="number" step="0.01" value={String(mapping.confidenceScore)} onChange={(e) => setMapping({ ...mapping, confidenceScore: Number(e.target.value) })} /></Field>
        </div>
        <div className="mt-4 flex flex-wrap gap-3">
          <button type="button" className="recordarr-button" onClick={() => mappingMutation.mutate()} disabled={mappingMutation.isPending}>
            {mappingMutation.isPending ? 'Creating...' : 'Create mapping'}
          </button>
        </div>
        <div className="mt-4 space-y-3">
          {mappingsQuery.data?.map((mappingRow) => (
            <div key={mappingRow.evidenceMappingId} className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3">
              <div className="flex items-center justify-between gap-3">
                <strong className="text-sm text-slate-100">{mappingRow.complianceRequirementRef}</strong>
                <span className="recordarr-pill text-[0.7rem]">{mappingRow.status}</span>
              </div>
              <p className="mt-1 text-sm text-slate-300">{mappingRow.recordId} · {mappingRow.evidenceTypeKey}</p>
              <div className="mt-3 flex flex-wrap gap-2">
                <button type="button" className="recordarr-button secondary" onClick={() => confirmEvidenceMapping(accessToken, mappingRow.evidenceMappingId, { confirmedByPersonId: actorPersonId, notes: null }).then(() => queryClient.invalidateQueries({ queryKey: ['recordarr'] }))}>
                  Confirm
                </button>
                <button type="button" className="recordarr-button secondary" onClick={() => rejectEvidenceMapping(accessToken, mappingRow.evidenceMappingId, { rejectedByPersonId: actorPersonId, rejectionReason: 'needs_followup', notes: null }).then(() => queryClient.invalidateQueries({ queryKey: ['recordarr'] }))}>
                  Reject
                </button>
              </div>
            </div>
          ))}
          {!mappingsQuery.data?.length && !mappingsQuery.isLoading ? <EmptyState title="No evidence mappings yet." /> : null}
        </div>
        <div className="mt-6 border-t border-slate-800 pt-4">
          <div className="flex items-center justify-between gap-3">
            <h3 className="text-sm font-semibold text-slate-100">Evidence coverage</h3>
            <span className="recordarr-pill text-[0.7rem]">{coverageQuery.data?.length ?? 0} rows</span>
          </div>
          <div className="mt-3 space-y-3">
            {coverageQuery.data?.map((coverage) => (
              <div key={coverage.evidenceCoverageId} className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3 text-sm text-slate-300">
                <div className="flex flex-wrap items-center justify-between gap-3">
                  <strong className="text-slate-100">{coverage.complianceCoreRequirementRef}</strong>
                  <span className="recordarr-pill text-[0.7rem]">{coverage.status}</span>
                </div>
                <p className="mt-1">{coverage.sourceObjectRef}</p>
                <p className="mt-1 text-xs text-slate-400">
                  Records: {coverage.recordRefs.join(', ') || 'none'} · Evaluated {formatDate(coverage.evaluatedAt)}
                </p>
                <p className="mt-1 text-xs text-slate-400">
                  Missing: {coverage.missingEvidenceTypes.join(', ') || 'none'} · Invalid: {coverage.invalidRecordRefs.join(', ') || 'none'}
                </p>
              </div>
            ))}
            {!coverageQuery.data?.length && !coverageQuery.isLoading ? <EmptyState title="No evidence coverage yet." /> : null}
          </div>
        </div>
      </Card>
    </div>
  )
}

function DocumentsPage({ accessToken, actorPersonId }: WorkspacePageProps) {
  const queryClient = useQueryClient()
  const docsQuery = useQuery({
    queryKey: ['recordarr', 'documents'],
    queryFn: () => listControlledDocuments(accessToken),
    enabled: Boolean(accessToken),
  })
  const [selectedDocumentId, setSelectedDocumentId] = useState<string>('')
  const [newDocument, setNewDocument] = useState({
    title: '',
    description: '',
    controlledDocumentType: 'procedure',
    ownerPersonId: actorPersonId,
    departmentOrgUnitId: '',
    staffarrSiteId: '',
    acknowledgementRequired: true,
  })
  const [versionForm, setVersionForm] = useState({
    fileName: '',
    createdByPersonId: actorPersonId,
    changeSummary: '',
  })
  const [reviewForm, setReviewForm] = useState({
    versionId: '',
    reviewType: 'periodic_review',
    requestedByPersonId: actorPersonId,
    reviewerPersonId: '',
    dueAt: new Date(Date.now() + 5 * 24 * 60 * 60 * 1000).toISOString(),
  })
  const [selectedReviewId, setSelectedReviewId] = useState<string>('')
  const [completeReviewForm, setCompleteReviewForm] = useState({
    status: 'approved',
    decisionReason: '',
    comments: '',
  })
  const [supersedeForm, setSupersedeForm] = useState({
    supersededByDocumentRef: '',
  })
  const [distributionForm, setDistributionForm] = useState({
    versionId: '',
    distributionType: 'person',
    targetRef: '',
  })
  const [acknowledgementForm, setAcknowledgementForm] = useState({
    versionId: '',
    personId: '',
    attestationText: '',
    dueAt: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString(),
  })

  useEffect(() => {
    if (!selectedDocumentId && docsQuery.data?.[0]) {
      setSelectedDocumentId(docsQuery.data[0].controlledDocumentId)
    }
  }, [docsQuery.data, selectedDocumentId])

  useEffect(() => {
    const currentVersionId = docsQuery.data?.find((doc) => doc.controlledDocumentId === selectedDocumentId)?.currentVersionId
    if (currentVersionId && reviewForm.versionId !== currentVersionId) {
      setReviewForm((current) => ({ ...current, versionId: currentVersionId }))
    }
    if (currentVersionId && distributionForm.versionId !== currentVersionId) {
      setDistributionForm((current) => ({ ...current, versionId: currentVersionId }))
    }
    if (currentVersionId && acknowledgementForm.versionId !== currentVersionId) {
      setAcknowledgementForm((current) => ({ ...current, versionId: currentVersionId }))
    }
  }, [docsQuery.data, reviewForm.versionId, distributionForm.versionId, acknowledgementForm.versionId, selectedDocumentId])

  const versionsQuery = useQuery({
    queryKey: ['recordarr', 'document-versions', selectedDocumentId],
    queryFn: () => listDocumentVersions(accessToken, selectedDocumentId),
    enabled: Boolean(accessToken && selectedDocumentId),
  })
  const reviewsQuery = useQuery({
    queryKey: ['recordarr', 'document-reviews', selectedDocumentId],
    queryFn: () => listDocumentReviews(accessToken, selectedDocumentId),
    enabled: Boolean(accessToken && selectedDocumentId),
  })
  const distributionsQuery = useQuery({
    queryKey: ['recordarr', 'document-distributions', selectedDocumentId],
    queryFn: () => listDocumentDistributions(accessToken, selectedDocumentId),
    enabled: Boolean(accessToken && selectedDocumentId),
  })
  const acknowledgementsQuery = useQuery({
    queryKey: ['recordarr', 'document-acknowledgements', selectedDocumentId],
    queryFn: () => listDocumentAcknowledgements(accessToken, selectedDocumentId),
    enabled: Boolean(accessToken && selectedDocumentId),
  })

  useEffect(() => {
    if (!selectedReviewId && reviewsQuery.data?.[0]) {
      setSelectedReviewId(reviewsQuery.data[0].documentReviewId)
    }
  }, [reviewsQuery.data, selectedReviewId])

  useEffect(() => {
    if (selectedReviewId && !reviewsQuery.data?.some((review) => review.documentReviewId === selectedReviewId)) {
      setSelectedReviewId(reviewsQuery.data?.[0]?.documentReviewId ?? '')
    }
  }, [reviewsQuery.data, selectedReviewId])

  const createDocumentMutation = useMutation({
    mutationFn: () => createControlledDocument(accessToken, newDocument),
    onSuccess: async (document) => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
      setSelectedDocumentId(document.controlledDocumentId)
    },
  })
  const refreshWorkflowsMutation = useMutation({
    mutationFn: () => refreshControlledDocumentWorkflows(accessToken),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const archiveDocumentMutation = useMutation({
    mutationFn: () => archiveControlledDocument(accessToken, selectedDocumentId, { updatedByPersonId: actorPersonId }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const obsoleteDocumentMutation = useMutation({
    mutationFn: () => obsoleteControlledDocument(accessToken, selectedDocumentId, { updatedByPersonId: actorPersonId }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const supersedeDocumentMutation = useMutation({
    mutationFn: () =>
      supersedeControlledDocument(accessToken, selectedDocumentId, {
        supersededByDocumentRef: supersedeForm.supersededByDocumentRef,
        supersededByPersonId: actorPersonId,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const createVersionMutation = useMutation({
    mutationFn: () => createDocumentVersion(accessToken, selectedDocumentId, versionForm),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const promoteVersionMutation = useMutation({
    mutationFn: (versionId: string) =>
      promoteDocumentVersion(accessToken, selectedDocumentId, versionId, {
        approvedByPersonId: actorPersonId,
        effectiveAt: new Date().toISOString(),
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const createReviewMutation = useMutation({
    mutationFn: () =>
      createDocumentReview(accessToken, selectedDocumentId, {
        ...reviewForm,
        versionId: reviewForm.versionId || selectedDocument?.currentVersionId || '',
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const completeReviewMutation = useMutation({
    mutationFn: () =>
      completeDocumentReview(accessToken, selectedDocumentId, selectedReviewId, {
        status: completeReviewForm.status,
        decisionReason: completeReviewForm.decisionReason,
        comments: completeReviewForm.comments,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const createDistributionMutation = useMutation({
    mutationFn: () =>
      createDocumentDistribution(accessToken, selectedDocumentId, {
        ...distributionForm,
        versionId: distributionForm.versionId || selectedDocument?.currentVersionId || '',
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const revokeDistributionMutation = useMutation({
    mutationFn: (distributionId: string) =>
      revokeDocumentDistribution(accessToken, selectedDocumentId, distributionId, {
        revokedByPersonId: actorPersonId,
        revokeReason: 'Distribution no longer needed.',
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const expireDistributionMutation = useMutation({
    mutationFn: (distributionId: string) =>
      expireDocumentDistribution(accessToken, selectedDocumentId, distributionId, {
        expiredByPersonId: actorPersonId,
        expireReason: 'Distribution expired on review cycle.',
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const createAcknowledgementMutation = useMutation({
    mutationFn: () =>
      createDocumentAcknowledgement(accessToken, selectedDocumentId, {
        ...acknowledgementForm,
        versionId: acknowledgementForm.versionId || selectedDocument?.currentVersionId || '',
        dueAt: acknowledgementForm.dueAt || null,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const completeAcknowledgementMutation = useMutation({
    mutationFn: (acknowledgementId: string) =>
      completeDocumentAcknowledgement(accessToken, selectedDocumentId, acknowledgementId, {
        signatureRecordRef: null,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })

  const selectedDocument = docsQuery.data?.find((doc) => doc.controlledDocumentId === selectedDocumentId) ?? null
  const documentOptions = useMemo(
    () => (docsQuery.data ?? []).map(toControlledDocumentOption),
    [docsQuery.data],
  )
  const currentVersion = useMemo(
    () => versionsQuery.data?.find((version) => version.versionId === selectedDocument?.currentVersionId) ?? null,
    [selectedDocument?.currentVersionId, versionsQuery.data],
  )
  const draftVersions = (versionsQuery.data ?? []).filter((version) => version.status === 'draft')
  const reviewVersions = (versionsQuery.data ?? []).filter((version) => version.status === 'review')

  return (
    <div className="recordarr-page">
      <SectionHeader
        eyebrow="Documents"
        title="Controlled document management"
        description="Version controlled procedures, approvals, review cadences, distributions, and acknowledgements."
        action={
          <div className="flex flex-wrap items-center gap-2">
            <button
              type="button"
              className="recordarr-button secondary"
              onClick={() => refreshWorkflowsMutation.mutate()}
              disabled={refreshWorkflowsMutation.isPending}
            >
              {refreshWorkflowsMutation.isPending ? 'Refreshing...' : 'Refresh workflows'}
            </button>
            <span className="recordarr-pill"><Archive className="h-4 w-4" /> {docsQuery.data?.length ?? 0} documents</span>
          </div>
        }
      />
      <div className="recordarr-card">
        <div className="recordarr-card-inner space-y-4">
          <div className="flex items-center gap-2">
            <Archive className="h-4 w-4 text-cyan-300" />
            <h2 className="text-lg font-semibold text-slate-50">Create controlled document</h2>
          </div>
          <div className="grid gap-3 md:grid-cols-2">
            <Field label="Title"><input className="recordarr-input" value={newDocument.title} onChange={(e) => setNewDocument({ ...newDocument, title: e.target.value })} /></Field>
            <Field label="Type"><input className="recordarr-input" value={newDocument.controlledDocumentType} onChange={(e) => setNewDocument({ ...newDocument, controlledDocumentType: e.target.value })} /></Field>
            <Field label="Owner person"><PersonReferencePicker value={newDocument.ownerPersonId} onChange={(ownerPersonId) => setNewDocument({ ...newDocument, ownerPersonId })} /></Field>
            <Field label="Department org unit"><input className="recordarr-input" value={newDocument.departmentOrgUnitId} onChange={(e) => setNewDocument({ ...newDocument, departmentOrgUnitId: e.target.value })} /></Field>
            <Field label="StaffArr site"><StaffSiteReferencePicker value={newDocument.staffarrSiteId} onChange={(staffarrSiteId) => setNewDocument({ ...newDocument, staffarrSiteId })} /></Field>
            <Field label="Acknowledgement required"><select className="recordarr-select" value={String(newDocument.acknowledgementRequired)} onChange={(e) => setNewDocument({ ...newDocument, acknowledgementRequired: e.target.value === 'true' })}><option value="true">Yes</option><option value="false">No</option></select></Field>
            <Field label="Description" wide><textarea className="recordarr-textarea" value={newDocument.description} onChange={(e) => setNewDocument({ ...newDocument, description: e.target.value })} /></Field>
              </div>
              <button type="button" className="recordarr-button" onClick={() => createDocumentMutation.mutate()} disabled={createDocumentMutation.isPending}>
                {createDocumentMutation.isPending ? 'Creating...' : 'Create document'}
              </button>
            </div>
          </div>

      <div className="recordarr-grid cols-2">
        <Card title="Controlled documents" icon={<Archive className="h-4 w-4 text-cyan-300" />}>
          <div className="space-y-3">
            {docsQuery.data?.map((doc) => (
              <button
                key={doc.controlledDocumentId}
                type="button"
                className={[
                  'w-full rounded-xl border px-3 py-3 text-left transition-colors',
                  doc.controlledDocumentId === selectedDocumentId
                    ? 'border-cyan-400/40 bg-cyan-500/10'
                    : 'border-slate-700/70 bg-slate-900/70 hover:bg-slate-900/90',
                ].join(' ')}
                onClick={() => setSelectedDocumentId(doc.controlledDocumentId)}
              >
                <div className="flex items-center justify-between gap-3">
                  <strong className="text-sm text-slate-100">{doc.documentNumber}</strong>
                  <span className="recordarr-pill text-[0.7rem]">{doc.status}</span>
                </div>
                <p className="mt-1 text-sm text-slate-300">{doc.title}</p>
              </button>
            ))}
            {!docsQuery.data?.length && !docsQuery.isLoading ? <EmptyState title="No controlled documents yet." /> : null}
          </div>
        </Card>

        <Card title="Version and review" icon={<FileText className="h-4 w-4 text-cyan-300" />}>
          {selectedDocument ? (
            <div className="space-y-4">
              <div className="grid gap-3 md:grid-cols-2">
                <div className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3 text-sm text-slate-300">
                  <p className="text-xs uppercase tracking-wide text-slate-400">Current effective version</p>
                  <p className="mt-1 text-base font-semibold text-slate-50">
                    {currentVersion?.versionLabel ?? 'n/a'}
                  </p>
                  <p className="mt-1">{currentVersion?.fileName ?? 'No effective version yet.'}</p>
                </div>
                <div className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3 text-sm text-slate-300">
                  <p className="text-xs uppercase tracking-wide text-slate-400">Document status</p>
                  <p className="mt-1 text-base font-semibold text-slate-50">{selectedDocument.status}</p>
                  <p className="mt-1">
                    {selectedDocument.status === 'approved'
                      ? 'Approved and awaiting effective promotion.'
                      : `Next review: ${formatDate(selectedDocument.nextReviewAt)}`}
                  </p>
                </div>
                <div className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3 text-sm text-slate-300">
                  <p className="text-xs uppercase tracking-wide text-slate-400">Related records</p>
                  <div className="mt-2 flex flex-wrap gap-2">
                    {selectedDocument.relatedRecordRefs.length > 0 ? (
                      selectedDocument.relatedRecordRefs.map((recordRef) => (
                        <span key={recordRef} className="recordarr-pill text-[0.7rem]">{recordRef}</span>
                      ))
                    ) : (
                      <span className="text-slate-400">No related records.</span>
                    )}
                  </div>
                </div>
              </div>
              <div className="grid gap-3 md:grid-cols-2">
                <Field label="File name"><input className="recordarr-input" value={versionForm.fileName} onChange={(e) => setVersionForm({ ...versionForm, fileName: e.target.value })} /></Field>
                <Field label="Created by"><PersonReferencePicker value={versionForm.createdByPersonId} onChange={(createdByPersonId) => setVersionForm({ ...versionForm, createdByPersonId })} /></Field>
                <Field label="Change summary" wide><input className="recordarr-input" value={versionForm.changeSummary} onChange={(e) => setVersionForm({ ...versionForm, changeSummary: e.target.value })} /></Field>
              </div>
              <button type="button" className="recordarr-button" onClick={() => createVersionMutation.mutate()} disabled={createVersionMutation.isPending}>
                {createVersionMutation.isPending ? 'Creating...' : 'Create version'}
              </button>

              <div className="grid gap-3 md:grid-cols-2">
                <Field label="Review type"><input className="recordarr-input" value={reviewForm.reviewType} onChange={(e) => setReviewForm({ ...reviewForm, reviewType: e.target.value })} /></Field>
                <Field label="Requested by"><PersonReferencePicker value={reviewForm.requestedByPersonId} onChange={(requestedByPersonId) => setReviewForm({ ...reviewForm, requestedByPersonId })} /></Field>
                <Field label="Reviewer person"><PersonReferencePicker value={reviewForm.reviewerPersonId} onChange={(reviewerPersonId) => setReviewForm({ ...reviewForm, reviewerPersonId })} /></Field>
                <Field label="Due at"><input className="recordarr-input" value={reviewForm.dueAt} onChange={(e) => setReviewForm({ ...reviewForm, dueAt: e.target.value })} /></Field>
              </div>
              <button type="button" className="recordarr-button secondary" onClick={() => createReviewMutation.mutate()} disabled={createReviewMutation.isPending}>
                {createReviewMutation.isPending ? 'Requesting...' : 'Request review'}
              </button>
              <div className="grid gap-3 md:grid-cols-2">
                <Field label="Distribution type"><input className="recordarr-input" value={distributionForm.distributionType} onChange={(e) => setDistributionForm({ ...distributionForm, distributionType: e.target.value })} /></Field>
                <Field label="Target">
                  {distributionForm.distributionType === 'person' ? (
                    <PersonReferencePicker value={distributionForm.targetRef} onChange={(targetRef) => setDistributionForm({ ...distributionForm, targetRef })} />
                  ) : distributionForm.distributionType === 'site' ? (
                    <StaffSiteReferencePicker value={distributionForm.targetRef} onChange={(targetRef) => setDistributionForm({ ...distributionForm, targetRef })} />
                  ) : (
                    <input className="recordarr-input" value={distributionForm.targetRef} onChange={(e) => setDistributionForm({ ...distributionForm, targetRef: e.target.value })} />
                  )}
                </Field>
                <Field label="Acknowledgement person"><PersonReferencePicker value={acknowledgementForm.personId} onChange={(personId) => setAcknowledgementForm({ ...acknowledgementForm, personId })} /></Field>
                <Field label="Acknowledgement due at"><input className="recordarr-input" value={acknowledgementForm.dueAt} onChange={(e) => setAcknowledgementForm({ ...acknowledgementForm, dueAt: e.target.value })} /></Field>
                <Field label="Attestation" wide><textarea className="recordarr-textarea" value={acknowledgementForm.attestationText} onChange={(e) => setAcknowledgementForm({ ...acknowledgementForm, attestationText: e.target.value })} /></Field>
              </div>
              <div className="flex flex-wrap gap-3">
                <button type="button" className="recordarr-button secondary" onClick={() => createDistributionMutation.mutate()} disabled={createDistributionMutation.isPending}>
                  {createDistributionMutation.isPending ? 'Distributing...' : 'Create distribution'}
                </button>
              <button type="button" className="recordarr-button secondary" onClick={() => createAcknowledgementMutation.mutate()} disabled={createAcknowledgementMutation.isPending}>
                  {createAcknowledgementMutation.isPending ? 'Creating...' : 'Create acknowledgement'}
                </button>
              </div>
              <div className="flex flex-wrap gap-3">
                <button
                  type="button"
                  className="recordarr-button secondary"
                  onClick={() => archiveDocumentMutation.mutate()}
                  disabled={archiveDocumentMutation.isPending || !selectedDocument || selectedDocument.status === 'archived'}
                >
                  {archiveDocumentMutation.isPending ? 'Archiving...' : 'Archive document'}
                </button>
                <button
                  type="button"
                  className="recordarr-button secondary"
                  onClick={() => obsoleteDocumentMutation.mutate()}
                  disabled={obsoleteDocumentMutation.isPending || !selectedDocument || selectedDocument.status === 'obsolete'}
                >
                  {obsoleteDocumentMutation.isPending ? 'Updating...' : 'Mark obsolete'}
                </button>
              </div>
              <div className="grid gap-3 md:grid-cols-2">
                <Field label="Superseded by document" wide>
                  <ControlledDocumentReferencePicker
                    value={supersedeForm.supersededByDocumentRef}
                    onChange={(supersededByDocumentRef) => setSupersedeForm({ ...supersedeForm, supersededByDocumentRef })}
                    options={documentOptions}
                  />
                </Field>
              </div>
              <button
                type="button"
                className="recordarr-button secondary"
                onClick={() => supersedeDocumentMutation.mutate()}
                disabled={
                  supersedeDocumentMutation.isPending ||
                  !supersedeForm.supersededByDocumentRef ||
                  supersedeForm.supersededByDocumentRef === selectedDocumentId
                }
              >
                {supersedeDocumentMutation.isPending ? 'Superseding...' : 'Supersede with replacement document'}
              </button>

              <div className="space-y-3">
                <div>
                  <h3 className="text-sm font-semibold text-slate-100">Versions</h3>
                  <div className="mt-2 space-y-2">
                    {(versionsQuery.data ?? []).map((version) => (
                      <div key={version.versionId} className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3 text-sm text-slate-300">
                        <div className="flex items-center justify-between gap-3">
                          <strong className="text-slate-100">{version.versionLabel}</strong>
                          <span className="recordarr-pill text-[0.7rem]">{version.status}</span>
                        </div>
                        <p className="mt-1">{version.fileName}</p>
                        <div className="mt-3 flex flex-wrap gap-2">
                          <button
                            type="button"
                            className="recordarr-button secondary"
                            onClick={() => promoteVersionMutation.mutate(version.versionId)}
                            disabled={promoteVersionMutation.isPending || version.status === 'effective'}
                          >
                            {promoteVersionMutation.isPending ? 'Promoting...' : 'Make effective'}
                          </button>
                        </div>
                      </div>
                    ))}
                    {!versionsQuery.data?.length ? <EmptyState title="No versions yet." /> : null}
                  </div>
                  {draftVersions.length > 0 ? (
                    <p className="mt-2 text-xs text-slate-400">Draft/review versions: {draftVersions.map((version) => version.versionLabel).join(', ')}</p>
                  ) : null}
                  {reviewVersions.length > 0 ? (
                    <p className="mt-1 text-xs text-slate-400">In review: {reviewVersions.map((version) => version.versionLabel).join(', ')}</p>
                  ) : null}
                </div>
                <div>
                  <h3 className="text-sm font-semibold text-slate-100">Reviews</h3>
                  <div className="mt-2 space-y-2">
                    {(reviewsQuery.data ?? []).map((review) => (
                      <button
                        key={review.documentReviewId}
                        type="button"
                        className={[
                          'w-full rounded-xl border p-3 text-left text-sm transition-colors',
                          review.documentReviewId === selectedReviewId
                            ? 'border-cyan-400/40 bg-cyan-500/10'
                            : 'border-slate-700/70 bg-slate-900/70 hover:bg-slate-900/90',
                        ].join(' ')}
                        onClick={() => setSelectedReviewId(review.documentReviewId)}
                      >
                        <div className="flex items-center justify-between gap-3">
                          <strong className="text-slate-100">{review.reviewType}</strong>
                          <span className="recordarr-pill text-[0.7rem]">{review.status}</span>
                        </div>
                        <p className="mt-1 text-slate-300">{review.reviewerPersonId}</p>
                        <p className="mt-1 text-xs text-slate-400">Requested by {review.requestedByPersonId}</p>
                      </button>
                    ))}
                    {!reviewsQuery.data?.length ? <EmptyState title="No reviews yet." /> : null}
                  </div>
                  <div className="mt-4 rounded-2xl border border-slate-700/70 bg-slate-950/50 p-4">
                    <div className="grid gap-3 md:grid-cols-2">
                      <Field label="Selected review">
                        <select
                          className="recordarr-select"
                          value={selectedReviewId}
                          onChange={(e) => setSelectedReviewId(e.target.value)}
                        >
                          <option value="">Choose a review</option>
                          {(reviewsQuery.data ?? []).map((review) => (
                            <option key={review.documentReviewId} value={review.documentReviewId}>
                              {review.reviewType} · {review.status}
                            </option>
                          ))}
                        </select>
                      </Field>
                      <Field label="Decision">
                        <select
                          className="recordarr-select"
                          value={completeReviewForm.status}
                          onChange={(e) => setCompleteReviewForm({ ...completeReviewForm, status: e.target.value })}
                        >
                          <ReadableOption value="approved" />
                          <ReadableOption value="rejected" />
                          <ReadableOption value="changes_requested" />
                        </select>
                      </Field>
                      <Field label="Decision reason" wide>
                        <textarea
                          className="recordarr-textarea"
                          value={completeReviewForm.decisionReason}
                          onChange={(e) => setCompleteReviewForm({ ...completeReviewForm, decisionReason: e.target.value })}
                        />
                      </Field>
                      <Field label="Comments" wide>
                        <textarea
                          className="recordarr-textarea"
                          value={completeReviewForm.comments}
                          onChange={(e) => setCompleteReviewForm({ ...completeReviewForm, comments: e.target.value })}
                        />
                      </Field>
                    </div>
                    <button
                      type="button"
                      className="recordarr-button secondary mt-3"
                      onClick={() => completeReviewMutation.mutate()}
                      disabled={completeReviewMutation.isPending || !selectedReviewId}
                    >
                      {completeReviewMutation.isPending ? 'Completing...' : 'Complete review'}
                    </button>
                  </div>
                </div>
                <div>
                  <h3 className="text-sm font-semibold text-slate-100">Distributions</h3>
                  <div className="mt-2 space-y-2">
                    {(distributionsQuery.data ?? []).map((distribution) => (
                      <div key={distribution.distributionId} className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3 text-sm text-slate-300">
                        <div className="flex items-center justify-between gap-3">
                          <strong className="text-slate-100">{distribution.distributionType}</strong>
                          <span className="recordarr-pill text-[0.7rem]">{distribution.status}</span>
                        </div>
                        <p className="mt-1">{distribution.targetRef}</p>
                        <div className="mt-3 flex flex-wrap gap-2">
                          <button
                            type="button"
                            className="recordarr-button secondary"
                            onClick={() => revokeDistributionMutation.mutate(distribution.distributionId)}
                            disabled={revokeDistributionMutation.isPending || distribution.status === 'revoked'}
                          >
                            Revoke
                          </button>
                          <button
                            type="button"
                            className="recordarr-button secondary"
                            onClick={() => expireDistributionMutation.mutate(distribution.distributionId)}
                            disabled={expireDistributionMutation.isPending || distribution.status === 'expired'}
                          >
                            Expire
                          </button>
                        </div>
                      </div>
                    ))}
                    {!distributionsQuery.data?.length ? <EmptyState title="No distributions yet." /> : null}
                  </div>
                </div>
                <div>
                  <h3 className="text-sm font-semibold text-slate-100">Acknowledgements</h3>
                  <div className="mt-2 space-y-2">
                    {(acknowledgementsQuery.data ?? []).map((acknowledgement) => (
                      <div key={acknowledgement.acknowledgementId} className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3 text-sm text-slate-300">
                        <div className="flex items-center justify-between gap-3">
                          <strong className="text-slate-100">{acknowledgement.personId}</strong>
                          <span className="recordarr-pill text-[0.7rem]">{acknowledgement.status}</span>
                        </div>
                        <p className="mt-1">{acknowledgement.attestationText ?? 'No attestation'}</p>
                        <p className="mt-1 text-xs text-slate-400">Due {formatDate(acknowledgement.dueAt)}</p>
                        <div className="mt-3 flex flex-wrap gap-2">
                          <button
                            type="button"
                            className="recordarr-button secondary"
                            onClick={() => completeAcknowledgementMutation.mutate(acknowledgement.acknowledgementId)}
                            disabled={completeAcknowledgementMutation.isPending}
                          >
                            Complete
                          </button>
                        </div>
                      </div>
                    ))}
                    {!acknowledgementsQuery.data?.length ? <EmptyState title="No acknowledgements yet." /> : null}
                  </div>
                  {acknowledgementsQuery.data?.some((acknowledgement) => acknowledgement.status === 'overdue') ? (
                    <p className="mt-2 text-xs text-amber-300">One or more acknowledgements are overdue.</p>
                  ) : null}
                </div>
                <div>
                  <h3 className="text-sm font-semibold text-slate-100">Audit trail</h3>
                  <div className="mt-2 space-y-2">
                    {(selectedDocument.auditTrail ?? []).slice().reverse().map((entry) => (
                      <div key={entry.auditTrailEntryId} className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3 text-sm text-slate-300">
                        <div className="flex items-center justify-between gap-3">
                          <strong className="text-slate-100">{entry.action}</strong>
                          <span className="recordarr-pill text-[0.7rem]">{formatDate(entry.occurredAt)}</span>
                        </div>
                        <p className="mt-1">{entry.details}</p>
                        <p className="mt-1 text-xs text-slate-400">Actor: {entry.actorPersonId}</p>
                      </div>
                    ))}
                    {!selectedDocument.auditTrail?.length ? <EmptyState title="No audit entries yet." /> : null}
                  </div>
                </div>
              </div>
            </div>
          ) : (
            <EmptyState title="Select a controlled document to inspect versions and reviews." />
          )}
        </Card>
      </div>
    </div>
  )
}

function PackagesPage({ accessToken }: { accessToken: string }) {
  const queryClient = useQueryClient()
  const { options: recordOptions, isLoading: recordOptionsLoading } = useRecordReferenceOptions(accessToken)
  const [selectedPackageId, setSelectedPackageId] = useState('')
  const [form, setForm] = useState({
    title: '',
    packageType: 'delivery',
    sourceProduct: '',
    sourceObjectRef: '',
    recordRef: '',
  })

  const packagesQuery = useQuery({
    queryKey: ['recordarr', 'packages'],
    queryFn: () => listPackages(accessToken),
    enabled: Boolean(accessToken),
  })
  const manifestQuery = useQuery({
    queryKey: ['recordarr', 'package-manifest', selectedPackageId],
    queryFn: () => getPackageManifest(accessToken, selectedPackageId),
    enabled: Boolean(accessToken && selectedPackageId),
  })

  useEffect(() => {
    if (!selectedPackageId && packagesQuery.data?.[0]) {
      setSelectedPackageId(packagesQuery.data[0].packageId)
    }
  }, [packagesQuery.data, selectedPackageId])

  const selectedPackage = (packagesQuery.data ?? []).find((pkg) => pkg.packageId === selectedPackageId) ?? null
  const packageTimeline = useMemo(() => {
    if (!selectedPackage) return []
    return [
      { key: 'created', label: 'Created', value: formatDate(selectedPackage.createdAt) },
      { key: 'completed', label: 'Completed', value: formatDate(selectedPackage.completedAt) },
      { key: 'locked', label: 'Locked', value: formatDate(selectedPackage.lockedAt) },
      { key: 'archived', label: 'Archived', value: formatDate(selectedPackage.archivedAt) },
      { key: 'expires', label: 'Expires', value: formatDate(selectedPackage.expiresAt) },
      { key: 'status', label: 'Status', value: selectedPackage.status },
    ]
  }, [selectedPackage])

  const createMutation = useMutation({
    mutationFn: () => createPackage(accessToken, form),
    onSuccess: async (pkg) => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
      setSelectedPackageId(pkg.packageId)
    },
  })
  const lockMutation = useMutation({
    mutationFn: () => lockPackage(accessToken, selectedPackageId),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const archiveMutation = useMutation({
    mutationFn: () => archivePackage(accessToken, selectedPackageId),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const downloadMutation = useMutation({
    mutationFn: () => downloadPackage(accessToken, selectedPackageId),
    onSuccess: async (content) => {
      const blob = new Blob([content], { type: 'text/plain;charset=utf-8' })
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = `${selectedPackageId || 'recordarr-package'}.txt`
      anchor.click()
      URL.revokeObjectURL(url)
    },
  })

  return (
    <div className="recordarr-page">
      <SectionHeader
        eyebrow="Packages"
        title="Evidence packet builder"
        description="Assemble record packages, lock them, and inspect their manifests before export."
        action={<span className="recordarr-pill"><PackageSearch className="h-4 w-4" /> {packagesQuery.data?.length ?? 0} packages</span>}
      />
      <div className="recordarr-card">
        <div className="recordarr-card-inner space-y-4">
          <div className="grid gap-3 md:grid-cols-2">
            <Field label="Title"><input className="recordarr-input" value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} /></Field>
            <Field label="Package type"><input className="recordarr-input" value={form.packageType} onChange={(e) => setForm({ ...form, packageType: e.target.value })} /></Field>
            <Field label="Source product"><SourceProductPicker value={form.sourceProduct} onChange={(sourceProduct) => setForm({ ...form, sourceProduct, sourceObjectRef: '' })} /></Field>
            <Field label="Source object ref">
              <SourceObjectRefPicker
                value={form.sourceObjectRef}
                sourceProduct={form.sourceProduct}
                onChange={(sourceObjectRef, selected) => setForm({ ...form, sourceProduct: selected?.sourceProduct ?? form.sourceProduct, sourceObjectRef })}
              />
            </Field>
            <Field label="Record"><RecordReferencePicker value={form.recordRef} onChange={(recordRef) => setForm({ ...form, recordRef })} options={recordOptions} isLoading={recordOptionsLoading} /></Field>
          </div>
          <div className="flex flex-wrap gap-3">
            <button type="button" className="recordarr-button" onClick={() => createMutation.mutate()} disabled={createMutation.isPending}>
              {createMutation.isPending ? 'Creating...' : 'Create package'}
            </button>
            <button type="button" className="recordarr-button secondary" onClick={() => lockMutation.mutate()} disabled={lockMutation.isPending || !selectedPackageId}>
              {lockMutation.isPending ? 'Locking...' : 'Lock selected package'}
            </button>
            <button type="button" className="recordarr-button secondary" onClick={() => archiveMutation.mutate()} disabled={archiveMutation.isPending || !selectedPackageId}>
              {archiveMutation.isPending ? 'Archiving...' : 'Archive selected package'}
            </button>
            <button type="button" className="recordarr-button secondary" onClick={() => downloadMutation.mutate()} disabled={downloadMutation.isPending || !selectedPackageId}>
              {downloadMutation.isPending ? 'Preparing...' : 'Download package'}
            </button>
          </div>
        </div>
      </div>

      <div className="recordarr-grid cols-2">
        <Card title="Packages" icon={<PackageSearch className="h-4 w-4 text-cyan-300" />}>
          <div className="space-y-3">
            {packagesQuery.data?.map((pkg) => (
              <button
                key={pkg.packageId}
                type="button"
                className={[
                  'w-full rounded-xl border px-3 py-3 text-left transition-colors',
                  pkg.packageId === selectedPackageId
                    ? 'border-cyan-400/40 bg-cyan-500/10'
                    : 'border-slate-700/70 bg-slate-900/70 hover:bg-slate-900/90',
                ].join(' ')}
                onClick={() => setSelectedPackageId(pkg.packageId)}
              >
                <div className="flex items-center justify-between gap-3">
                  <strong className="text-sm text-slate-100">{pkg.packageNumber}</strong>
                  <span className="recordarr-pill text-[0.7rem]">{pkg.status}</span>
                </div>
                <p className="mt-1 text-sm text-slate-300">{pkg.title}</p>
                <p className="mt-1 text-xs text-slate-400">Records: {pkg.recordRefs.length} · Source objects: {pkg.sourceObjectRefs.length}</p>
              </button>
            ))}
            {!packagesQuery.data?.length && !packagesQuery.isLoading ? <EmptyState title="No packages yet." /> : null}
          </div>
        </Card>
        <Card title="Package detail" icon={<FileText className="h-4 w-4 text-cyan-300" />}>
          {selectedPackage ? (
            <div className="space-y-5 text-sm text-slate-300">
              <div className="rounded-2xl border border-slate-700/70 bg-slate-950/60 p-4">
                <div className="flex flex-wrap items-center justify-between gap-3">
                  <div>
                    <h3 className="text-base font-semibold text-slate-50">{selectedPackage.packageNumber}</h3>
                    <p className="mt-1 text-slate-300">{selectedPackage.title}</p>
                  </div>
                  <span className="recordarr-pill text-[0.7rem]">{selectedPackage.status}</span>
                </div>
                <div className="mt-3 grid gap-2 md:grid-cols-2">
                  <p><strong className="text-slate-100">Source product:</strong> {selectedPackage.sourceProduct}</p>
                  <p><strong className="text-slate-100">Package type:</strong> {selectedPackage.packageType}</p>
                  <p><strong className="text-slate-100">Source scope:</strong> {selectedPackage.sourceObjectRefs.join(', ')}</p>
                  <p><strong className="text-slate-100">Record refs:</strong> {selectedPackage.recordRefs.join(', ')}</p>
                </div>
              </div>

              <div className="grid gap-3 md:grid-cols-2">
                <div>
                  <h3 className="text-sm font-semibold text-slate-100">Record list</h3>
                  <div className="mt-2 space-y-2">
                    {manifestQuery.data?.recordEntries.map((entry) => (
                      <div key={entry.entryId} className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3">
                        <div className="flex items-center justify-between gap-3">
                          <strong className="text-slate-100">{entry.displayName}</strong>
                          <span className="recordarr-pill text-[0.7rem]">{entry.statusSnapshot ?? entry.entryType}</span>
                        </div>
                        <p className="mt-1 text-xs text-slate-400">Checksum: {entry.checksum}</p>
                      </div>
                    )) ?? <EmptyState title="No record entries in manifest." />}
                  </div>
                </div>
                <div>
                  <h3 className="text-sm font-semibold text-slate-100">Requirement/evidence matrix</h3>
                  <div className="mt-2 space-y-2">
                    {manifestQuery.data?.requirementEntries.map((entry) => (
                      <div key={entry.entryId} className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3">
                        <div className="flex items-center justify-between gap-3">
                          <strong className="text-slate-100">{entry.complianceRequirementRef ?? entry.displayName}</strong>
                          <span className="recordarr-pill text-[0.7rem]">{entry.statusSnapshot ?? 'linked'}</span>
                        </div>
                        <p className="mt-1 text-xs text-slate-400">Evidence checksum: {entry.checksum}</p>
                      </div>
                    )) ?? <EmptyState title="No requirement mappings in manifest." />}
                  </div>
                </div>
              </div>

              <div className="grid gap-3 md:grid-cols-2">
                <div>
                  <h3 className="text-sm font-semibold text-slate-100">Source scope</h3>
                  <div className="mt-2 space-y-2">
                    {manifestQuery.data?.sourceObjectEntries.map((entry) => (
                      <div key={entry.entryId} className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3">
                        <div className="flex items-center justify-between gap-3">
                          <strong className="text-slate-100">{entry.displayName}</strong>
                          <span className="recordarr-pill text-[0.7rem]">{entry.entryType}</span>
                        </div>
                        <p className="mt-1 text-xs text-slate-400">{entry.sourceProduct} · {entry.sourceObjectRef}</p>
                      </div>
                    )) ?? <EmptyState title="No source scope entries in manifest." />}
                  </div>
                </div>
                <div>
                  <h3 className="text-sm font-semibold text-slate-100">Generated output</h3>
                  <div className="mt-2 space-y-2 rounded-xl border border-slate-700/70 bg-slate-900/70 p-3">
                    <p><strong className="text-slate-100">Manifest checksum:</strong> {selectedPackage.manifestChecksum ?? 'n/a'}</p>
                    <p><strong className="text-slate-100">PDF record ref:</strong> {selectedPackage.generatedPdfRecordRef ?? 'n/a'}</p>
                    <p><strong className="text-slate-100">ZIP file ref:</strong> {selectedPackage.generatedZipFileRef ?? 'n/a'}</p>
                  </div>
                </div>
              </div>

              <div>
                <h3 className="text-sm font-semibold text-slate-100">Timeline</h3>
                <div className="mt-2 grid gap-2 md:grid-cols-2">
                  {packageTimeline.map((item) => (
                    <div key={item.key} className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3">
                      <p className="text-xs uppercase tracking-wide text-slate-400">{item.label}</p>
                      <p className="mt-1 text-slate-100">{item.value}</p>
                    </div>
                  ))}
                </div>
              </div>

              {manifestQuery.data ? (
                <div>
                  <h3 className="text-sm font-semibold text-slate-100">Manifest</h3>
                  <div className="mt-2 rounded-xl border border-slate-700/70 bg-slate-900/70 p-3">
                    <p><strong className="text-slate-100">Checksum:</strong> {manifestQuery.data.checksum}</p>
                    <p><strong className="text-slate-100">Generated by:</strong> {manifestQuery.data.generatedByPersonId}</p>
                    <p><strong className="text-slate-100">Manifest version:</strong> {manifestQuery.data.manifestVersion}</p>
                  </div>
                </div>
              ) : null}
            </div>
          ) : (
            <EmptyState title="Select a package to inspect its detail." />
          )}
        </Card>
      </div>
    </div>
  )
}

function RetentionPage({ accessToken, actorPersonId }: WorkspacePageProps) {
  const queryClient = useQueryClient()
  const { options: recordOptions, isLoading: recordOptionsLoading } = useRecordReferenceOptions(accessToken)
  const [recordId, setRecordId] = useState('')
  const [selectedDisposalReviewId, setSelectedDisposalReviewId] = useState('')
  const [disposalForm, setDisposalForm] = useState({
    recordId: '',
    retentionStatusRef: '',
    proposedAction: 'archive',
    requestedByPersonId: actorPersonId,
  })
  const [completeDisposalForm, setCompleteDisposalForm] = useState({
    status: 'approved',
    reviewedByPersonId: actorPersonId,
    decisionReason: '',
  })
  const policiesQuery = useQuery({
    queryKey: ['recordarr', 'retention-policies'],
    queryFn: () => listRetentionPolicies(accessToken),
    enabled: Boolean(accessToken),
  })
  const statusQuery = useQuery({
    queryKey: ['recordarr', 'retention-status', recordId],
    queryFn: () => getRetentionStatus(accessToken, recordId),
    enabled: Boolean(accessToken && recordId),
  })
  const holdsQuery = useQuery({
    queryKey: ['recordarr', 'legal-holds'],
    queryFn: () => listLegalHolds(accessToken),
    enabled: Boolean(accessToken),
  })
  const disposalReviewsQuery = useQuery({
    queryKey: ['recordarr', 'disposal-reviews'],
    queryFn: () => listDisposalReviews(accessToken),
    enabled: Boolean(accessToken),
  })
  const activeHoldsForRecord = (holdsQuery.data ?? []).filter((hold) => hold.status === 'active' && hold.recordRefs.includes(recordId))

  useEffect(() => {
    if (!selectedDisposalReviewId && disposalReviewsQuery.data?.[0]) {
      setSelectedDisposalReviewId(disposalReviewsQuery.data[0].disposalReviewId)
    }
  }, [disposalReviewsQuery.data, selectedDisposalReviewId])

  useEffect(() => {
    if (statusQuery.data?.retentionStatusId && disposalForm.retentionStatusRef !== statusQuery.data.retentionStatusId) {
      setDisposalForm((current) => ({ ...current, retentionStatusRef: statusQuery.data!.retentionStatusId }))
    }
  }, [disposalForm.retentionStatusRef, statusQuery.data])

  const createDisposalMutation = useMutation({
    mutationFn: () => createDisposalReview(accessToken, disposalForm),
    onSuccess: async (review) => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
      setSelectedDisposalReviewId(review.disposalReviewId)
    },
  })
  const completeDisposalMutation = useMutation({
    mutationFn: () =>
      selectedDisposalReviewId
        ? completeDisposalReview(accessToken, selectedDisposalReviewId, completeDisposalForm)
        : Promise.reject(new Error('No disposal review selected.')),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const recalculateRetentionMutation = useMutation({
    mutationFn: () => recalculateRetentionStatuses(accessToken),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })

  return (
    <div className="recordarr-page">
      <SectionHeader
        eyebrow="Retention"
        title="Retention rules and expiration"
        description="Review policy definitions, hold overrides, and the active retention status of any record."
        action={<span className="recordarr-pill"><Clock3 className="h-4 w-4" /> {policiesQuery.data?.length ?? 0} policies</span>}
      />
      <div className="recordarr-grid cols-2">
        <Card title="Policies" icon={<Clock3 className="h-4 w-4 text-cyan-300" />}>
          <div className="space-y-3">
            {policiesQuery.data?.map((policy) => (
              <div key={policy.retentionPolicyId} className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3 text-sm text-slate-300">
                <div className="flex items-center justify-between gap-3">
                  <strong className="text-slate-100">{policy.title}</strong>
                  <span className="recordarr-pill text-[0.7rem]">{policy.status}</span>
                </div>
                <p className="mt-1">{policy.description}</p>
                <p className="mt-2 text-xs text-slate-400">{policy.retainFor} {policy.retentionUnit} from {policy.retentionStartTrigger}</p>
              </div>
            ))}
            {!policiesQuery.data?.length && !policiesQuery.isLoading ? <EmptyState title="No retention policies found." /> : null}
          </div>
        </Card>
        <Card title="Record retention status" icon={<LockKeyhole className="h-4 w-4 text-cyan-300" />}>
          <Field label="Record">
            <RecordReferencePicker value={recordId} onChange={setRecordId} options={recordOptions} isLoading={recordOptionsLoading} />
          </Field>
          <div className="mt-4 space-y-3 text-sm text-slate-300">
            {statusQuery.data ? (
              <>
                <p><strong className="text-slate-100">Status:</strong> {statusQuery.data.status}</p>
                <p><strong className="text-slate-100">Expires:</strong> {formatDate(statusQuery.data.retentionExpiresAt)}</p>
                <p><strong className="text-slate-100">Next review:</strong> {formatDate(statusQuery.data.nextReviewAt)}</p>
                <p><strong className="text-slate-100">Last reviewed:</strong> {formatDate(statusQuery.data.lastReviewedAt)}</p>
                <p><strong className="text-slate-100">Legal holds:</strong> {activeHoldsForRecord.length > 0 ? activeHoldsForRecord.map((hold) => hold.holdNumber).join(', ') : 'none'}</p>
              </>
            ) : (
              <EmptyState title="Select a record to inspect its retention status." />
            )}
          </div>
          <button
            type="button"
            className="recordarr-button secondary mt-3"
            onClick={() => recalculateRetentionMutation.mutate()}
            disabled={recalculateRetentionMutation.isPending}
          >
            {recalculateRetentionMutation.isPending ? 'Recalculating...' : 'Refresh retention scheduler'}
          </button>
        </Card>
      </div>
      <div className="recordarr-card mt-6">
        <div className="recordarr-card-inner space-y-4">
          <div className="flex items-center gap-2">
            <BadgeCheck className="h-4 w-4 text-cyan-300" />
            <h2 className="text-lg font-semibold text-slate-50">Disposal review</h2>
          </div>
          <div className="grid gap-3 md:grid-cols-2">
            <Field label="Record"><RecordReferencePicker value={disposalForm.recordId} onChange={(recordId) => setDisposalForm({ ...disposalForm, recordId })} options={recordOptions} isLoading={recordOptionsLoading} /></Field>
            <Field label="Retention status">
              <div className="recordarr-input text-slate-400">
                {disposalForm.retentionStatusRef || 'Select a record to resolve retention status.'}
              </div>
            </Field>
            <Field label="Proposed action"><input className="recordarr-input" value={disposalForm.proposedAction} onChange={(e) => setDisposalForm({ ...disposalForm, proposedAction: e.target.value })} /></Field>
            <Field label="Requested by"><PersonReferencePicker value={disposalForm.requestedByPersonId} onChange={(requestedByPersonId) => setDisposalForm({ ...disposalForm, requestedByPersonId })} /></Field>
          </div>
          {statusQuery.data?.status === 'blocked_by_legal_hold' ? (
            <p className="text-sm text-amber-300">This record is blocked by an active legal hold.</p>
          ) : null}
          <div className="flex flex-wrap gap-3">
            <button type="button" className="recordarr-button" onClick={() => createDisposalMutation.mutate()} disabled={createDisposalMutation.isPending}>
              {createDisposalMutation.isPending ? 'Creating...' : 'Create disposal review'}
            </button>
            <button type="button" className="recordarr-button secondary" onClick={() => completeDisposalMutation.mutate()} disabled={completeDisposalMutation.isPending || !selectedDisposalReviewId}>
              {completeDisposalMutation.isPending ? 'Completing...' : 'Complete selected review'}
            </button>
          </div>
        </div>
      </div>
      <div className="recordarr-grid cols-2 mt-6">
        <Card title="Disposal reviews" icon={<History className="h-4 w-4 text-cyan-300" />}>
          <div className="space-y-3">
            {disposalReviewsQuery.data?.map((review) => (
              <button
                key={review.disposalReviewId}
                type="button"
                className={[
                  'w-full rounded-xl border px-3 py-3 text-left transition-colors',
                  review.disposalReviewId === selectedDisposalReviewId
                    ? 'border-cyan-400/40 bg-cyan-500/10'
                    : 'border-slate-700/70 bg-slate-900/70 hover:bg-slate-900/90',
                ].join(' ')}
                onClick={() => setSelectedDisposalReviewId(review.disposalReviewId)}
              >
                <div className="flex items-center justify-between gap-3">
                  <strong className="text-sm text-slate-100">{review.recordId}</strong>
                  <span className="recordarr-pill text-[0.7rem]">{review.status}</span>
                </div>
                <p className="mt-1 text-sm text-slate-300">{review.proposedAction}</p>
              </button>
            ))}
            {!disposalReviewsQuery.data?.length && !disposalReviewsQuery.isLoading ? <EmptyState title="No disposal reviews yet." /> : null}
          </div>
        </Card>
        <Card title="Complete review" icon={<BadgeCheck className="h-4 w-4 text-cyan-300" />}>
          {selectedDisposalReviewId ? (
            <div className="space-y-3">
              <div className="grid gap-3 md:grid-cols-2">
                <Field label="Status">
                  <select className="recordarr-select" value={completeDisposalForm.status} onChange={(e) => setCompleteDisposalForm({ ...completeDisposalForm, status: e.target.value })}>
                    <ReadableOption value="approved" />
                    <ReadableOption value="rejected" />
                    <ReadableOption value="completed" />
                    <ReadableOption value="canceled" />
                  </select>
                </Field>
                <Field label="Reviewed by"><PersonReferencePicker value={completeDisposalForm.reviewedByPersonId} onChange={(reviewedByPersonId) => setCompleteDisposalForm({ ...completeDisposalForm, reviewedByPersonId })} /></Field>
                <Field label="Decision reason" wide><textarea className="recordarr-textarea" value={completeDisposalForm.decisionReason} onChange={(e) => setCompleteDisposalForm({ ...completeDisposalForm, decisionReason: e.target.value })} /></Field>
              </div>
              <p className="text-sm text-slate-400">Selected review: {selectedDisposalReviewId}</p>
              <button type="button" className="recordarr-button secondary" onClick={() => completeDisposalMutation.mutate()} disabled={completeDisposalMutation.isPending}>
                {completeDisposalMutation.isPending ? 'Completing...' : 'Apply completion'}
              </button>
            </div>
          ) : (
            <EmptyState title="Select a disposal review to complete it." />
          )}
        </Card>
      </div>
    </div>
  )
}

function HoldsPage({ accessToken, actorPersonId }: WorkspacePageProps) {
  const queryClient = useQueryClient()
  const [selectedHoldId, setSelectedHoldId] = useState('')
  const [form, setForm] = useState({
    title: '',
    description: '',
    holdType: 'audit',
    sourceProduct: '',
    sourceObjectType: '',
    sourceObjectId: '',
    createdByPersonId: actorPersonId,
    scopeRules: '',
    recordRefs: '',
  })

  const holdsQuery = useQuery({
    queryKey: ['recordarr', 'legal-holds'],
    queryFn: () => listLegalHolds(accessToken),
    enabled: Boolean(accessToken),
  })

  useEffect(() => {
    if (!selectedHoldId && holdsQuery.data?.[0]) {
      setSelectedHoldId(holdsQuery.data[0].legalHoldId)
    }
  }, [holdsQuery.data, selectedHoldId])

  const createMutation = useMutation({
    mutationFn: () =>
      createLegalHold(accessToken, {
        ...form,
        scopeRules: form.scopeRules.split('\n').map((item) => item.trim()).filter(Boolean),
        recordRefs: form.recordRefs.split('\n').map((item) => item.trim()).filter(Boolean),
      }),
    onSuccess: async (hold) => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
      setSelectedHoldId(hold.legalHoldId)
    },
  })
  const releaseMutation = useMutation({
    mutationFn: () => releaseLegalHold(accessToken, selectedHoldId, { releasedByPersonId: actorPersonId, releaseReason: '' }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const activateMutation = useMutation({
    mutationFn: () => activateLegalHold(accessToken, selectedHoldId),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })

  return (
    <div className="recordarr-page">
      <SectionHeader
        eyebrow="Holds"
        title="Legal hold management"
        description="Pause retention or disposition when audit, regulatory, or investigative review is in progress."
        action={<span className="recordarr-pill"><ShieldCheck className="h-4 w-4" /> {holdsQuery.data?.length ?? 0} holds</span>}
      />
      <div className="recordarr-card">
        <div className="recordarr-card-inner space-y-4">
          <div className="grid gap-3 md:grid-cols-2">
            <Field label="Title"><input className="recordarr-input" value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} /></Field>
            <Field label="Hold type"><input className="recordarr-input" value={form.holdType} onChange={(e) => setForm({ ...form, holdType: e.target.value })} /></Field>
            <Field label="Source product"><SourceProductPicker value={form.sourceProduct} onChange={(sourceProduct) => setForm({ ...form, sourceProduct, sourceObjectType: '', sourceObjectId: '' })} /></Field>
            <Field label="Source reference">
              <SourceObjectRefPicker
                value={buildSourceObjectRef(form.sourceProduct, form.sourceObjectType, form.sourceObjectId)}
                sourceProduct={form.sourceProduct}
                onChange={(_, selected) => {
                  if (!selected) return
                  setForm({
                    ...form,
                    sourceProduct: selected.sourceProduct,
                    sourceObjectType: selected.sourceObjectType,
                    sourceObjectId: selected.sourceObjectId,
                  })
                }}
              />
            </Field>
            <Field label="Created by"><PersonReferencePicker value={form.createdByPersonId} onChange={(createdByPersonId) => setForm({ ...form, createdByPersonId })} /></Field>
            <Field label="Scope rules" wide><textarea className="recordarr-textarea" value={form.scopeRules} onChange={(e) => setForm({ ...form, scopeRules: e.target.value })} /></Field>
            <Field label="Record refs" wide><textarea className="recordarr-textarea" value={form.recordRefs} onChange={(e) => setForm({ ...form, recordRefs: e.target.value })} /></Field>
            <Field label="Description" wide><textarea className="recordarr-textarea" value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} /></Field>
          </div>
          <div className="flex flex-wrap gap-3">
            <button type="button" className="recordarr-button" onClick={() => createMutation.mutate()} disabled={createMutation.isPending}>
              {createMutation.isPending ? 'Creating...' : 'Create hold'}
            </button>
            <button type="button" className="recordarr-button secondary" onClick={() => activateMutation.mutate()} disabled={activateMutation.isPending || !selectedHoldId}>
              {activateMutation.isPending ? 'Activating...' : 'Activate selected'}
            </button>
            <button type="button" className="recordarr-button secondary" onClick={() => releaseMutation.mutate()} disabled={releaseMutation.isPending || !selectedHoldId}>
              {releaseMutation.isPending ? 'Releasing...' : 'Release selected'}
            </button>
          </div>
        </div>
      </div>
      <div className="recordarr-grid cols-2">
        <Card title="Holds" icon={<ShieldCheck className="h-4 w-4 text-cyan-300" />}>
          <div className="space-y-3">
            {holdsQuery.data?.map((hold) => (
              <button
                key={hold.legalHoldId}
                type="button"
                className={[
                  'w-full rounded-xl border px-3 py-3 text-left transition-colors',
                  hold.legalHoldId === selectedHoldId
                    ? 'border-cyan-400/40 bg-cyan-500/10'
                    : 'border-slate-700/70 bg-slate-900/70 hover:bg-slate-900/90',
                ].join(' ')}
                onClick={() => setSelectedHoldId(hold.legalHoldId)}
              >
                <div className="flex items-center justify-between gap-3">
                  <strong className="text-sm text-slate-100">{hold.holdNumber}</strong>
                  <span className="recordarr-pill text-[0.7rem]">{hold.status}</span>
                </div>
                <p className="mt-1 text-sm text-slate-300">{hold.title}</p>
              </button>
            ))}
            {!holdsQuery.data?.length && !holdsQuery.isLoading ? <EmptyState title="No holds yet." /> : null}
          </div>
        </Card>
        <Card title="Details" icon={<FileText className="h-4 w-4 text-cyan-300" />}>
          {holdsQuery.data?.find((hold) => hold.legalHoldId === selectedHoldId) ? (
            (() => {
              const hold = holdsQuery.data!.find((item) => item.legalHoldId === selectedHoldId)!
              return (
                <div className="space-y-3 text-sm text-slate-300">
                  <p><strong className="text-slate-100">Type:</strong> {hold.holdType}</p>
                  <p><strong className="text-slate-100">Source:</strong> {hold.sourceProduct} · {hold.sourceObjectId}</p>
                  <p><strong className="text-slate-100">Created:</strong> {formatDate(hold.createdAt)}</p>
                  <p><strong className="text-slate-100">Released:</strong> {formatDate(hold.releasedAt)}</p>
                  <p><strong className="text-slate-100">Scope:</strong> {hold.scopeRules.join(', ')}</p>
                  <p><strong className="text-slate-100">Records:</strong> {hold.recordRefs.join(', ')}</p>
                </div>
              )
            })()
          ) : (
            <EmptyState title="Select a hold to view details." />
          )}
        </Card>
      </div>
    </div>
  )
}

function AccessPage({ accessToken, actorPersonId }: WorkspacePageProps) {
  const queryClient = useQueryClient()
  const { options: recordOptions, isLoading: recordOptionsLoading } = useRecordReferenceOptions(accessToken)
  const [shareForm, setShareForm] = useState({
    recordId: '',
    recipientName: '',
    recipientEmail: '',
    sharePurpose: '',
    allowedActions: '',
    createdByPersonId: actorPersonId,
  })
  const [grantForm, setGrantForm] = useState({
    recordId: '',
    granteeType: 'role',
    granteeRef: '',
    permission: 'read',
    grantedByPersonId: actorPersonId,
    expiresAt: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString(),
  })
  const [policyForm, setPolicyForm] = useState({
    recordId: '',
    policyType: 'product_scoped',
    status: 'active',
    readRules: '',
    writeRules: '',
    downloadRules: '',
    shareRules: '',
    exportRules: '',
    purgeRules: '',
    createdByPersonId: actorPersonId,
  })
  const [redactionForm, setRedactionForm] = useState({
    sourceRecordId: '',
    redactedRecordId: '',
    redactionReason: 'privacy',
    redactedByPersonId: actorPersonId,
    redactionRules: '',
  })

  const policiesQuery = useQuery({ queryKey: ['recordarr', 'access-policies'], queryFn: () => listAccessPolicies(accessToken), enabled: Boolean(accessToken) })
  const grantsQuery = useQuery({ queryKey: ['recordarr', 'access-grants'], queryFn: () => listAccessGrants(accessToken), enabled: Boolean(accessToken) })
  const sharesQuery = useQuery({ queryKey: ['recordarr', 'external-shares'], queryFn: () => listExternalShares(accessToken), enabled: Boolean(accessToken) })
  const redactionsQuery = useQuery({ queryKey: ['recordarr', 'redactions'], queryFn: () => listRedactions(accessToken), enabled: Boolean(accessToken) })
  const disposalQuery = useQuery({ queryKey: ['recordarr', 'disposal-reviews'], queryFn: () => listDisposalReviews(accessToken), enabled: Boolean(accessToken) })
  const logsQuery = useQuery({ queryKey: ['recordarr', 'access-logs'], queryFn: () => listAccessLogs(accessToken), enabled: Boolean(accessToken) })

  const shareMutation = useMutation({
    mutationFn: () =>
      createExternalShare(accessToken, {
        ...shareForm,
        allowedActions: shareForm.allowedActions.split('\n').map((item) => item.trim()).filter(Boolean),
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const policyMutation = useMutation({
    mutationFn: () =>
      createAccessPolicy(accessToken, {
        ...policyForm,
        readRules: policyForm.readRules.split('\n').map((item) => item.trim()).filter(Boolean),
        writeRules: policyForm.writeRules.split('\n').map((item) => item.trim()).filter(Boolean),
        downloadRules: policyForm.downloadRules.split('\n').map((item) => item.trim()).filter(Boolean),
        shareRules: policyForm.shareRules.split('\n').map((item) => item.trim()).filter(Boolean),
        exportRules: policyForm.exportRules.split('\n').map((item) => item.trim()).filter(Boolean),
        purgeRules: policyForm.purgeRules.split('\n').map((item) => item.trim()).filter(Boolean),
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const togglePolicyStatusMutation = useMutation({
    mutationFn: ({ policy, status }: { policy: RecordArrAccessPolicy; status: string }) =>
      updateAccessPolicy(accessToken, policy.accessPolicyId, {
        recordId: policy.recordId,
        policyType: policy.policyType,
        status,
        readRules: [...policy.readRules],
        writeRules: [...policy.writeRules],
        downloadRules: [...policy.downloadRules],
        shareRules: [...policy.shareRules],
        exportRules: [...policy.exportRules],
        purgeRules: [...policy.purgeRules],
        updatedByPersonId: actorPersonId,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const expireShareMutation = useMutation({
    mutationFn: (externalShareId: string) =>
      expireExternalShare(accessToken, externalShareId, { expiredByPersonId: actorPersonId }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const refreshSharesMutation = useMutation({
    mutationFn: () => refreshExternalShares(accessToken),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const accessShareMutation = useMutation({
    mutationFn: (externalShareId: string) =>
      recordExternalShareAccess(accessToken, externalShareId, {
        accessedByPersonId: actorPersonId,
        accessAction: 'view',
        sourceIp: null,
        userAgent: navigator.userAgent,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const grantMutation = useMutation({
    mutationFn: () =>
      createAccessGrant(accessToken, {
        ...grantForm,
        expiresAt: grantForm.expiresAt || null,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const refreshGrantsMutation = useMutation({
    mutationFn: () => refreshAccessGrants(accessToken),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const redactionMutation = useMutation({
    mutationFn: () =>
      createRedaction(accessToken, {
        ...redactionForm,
        redactionRules: redactionForm.redactionRules.split('\n').map((item) => item.trim()).filter(Boolean),
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const getSharePolicy = (recordId: string) =>
    policiesQuery.data?.find((policy) => policy.recordId === recordId && policy.status === 'active') ?? null

  return (
    <div className="recordarr-page">
      <SectionHeader
        eyebrow="Access"
        title="Access controls and trail"
        description="Inspect access policies, grants, shares, redactions, and usage history for record governance."
        action={
          <div className="flex flex-wrap items-center gap-2">
            <button
              type="button"
              className="recordarr-button secondary"
              onClick={() => refreshSharesMutation.mutate()}
              disabled={refreshSharesMutation.isPending}
            >
              {refreshSharesMutation.isPending ? 'Refreshing...' : 'Refresh shares'}
            </button>
            <button
              type="button"
              className="recordarr-button secondary"
              onClick={() => refreshGrantsMutation.mutate()}
              disabled={refreshGrantsMutation.isPending}
            >
              {refreshGrantsMutation.isPending ? 'Refreshing...' : 'Refresh grants'}
            </button>
            <span className="recordarr-pill"><LockKeyhole className="h-4 w-4" /> Access governed</span>
          </div>
        }
      />
      <div className="recordarr-card">
        <div className="recordarr-card-inner space-y-4">
          <div className="grid gap-3 md:grid-cols-2">
            <Field label="Record"><RecordReferencePicker value={shareForm.recordId} onChange={(recordId) => setShareForm({ ...shareForm, recordId })} options={recordOptions} isLoading={recordOptionsLoading} /></Field>
            <Field label="Recipient name"><input className="recordarr-input" value={shareForm.recipientName} onChange={(e) => setShareForm({ ...shareForm, recipientName: e.target.value })} /></Field>
            <Field label="Recipient email"><input className="recordarr-input" value={shareForm.recipientEmail} onChange={(e) => setShareForm({ ...shareForm, recipientEmail: e.target.value })} /></Field>
            <Field label="Share purpose"><input className="recordarr-input" value={shareForm.sharePurpose} onChange={(e) => setShareForm({ ...shareForm, sharePurpose: e.target.value })} /></Field>
            <Field label="Allowed actions" wide><textarea className="recordarr-textarea" value={shareForm.allowedActions} onChange={(e) => setShareForm({ ...shareForm, allowedActions: e.target.value })} /></Field>
          </div>
          <button type="button" className="recordarr-button" onClick={() => shareMutation.mutate()} disabled={shareMutation.isPending}>
            {shareMutation.isPending ? 'Creating...' : 'Create external share'}
          </button>
        </div>
      </div>

      <div className="recordarr-card mt-6">
        <div className="recordarr-card-inner space-y-4">
          <div className="flex items-center gap-2">
            <BadgeCheck className="h-4 w-4 text-cyan-300" />
            <h2 className="text-lg font-semibold text-slate-50">Access grant management</h2>
          </div>
          <div className="grid gap-3 md:grid-cols-2">
            <Field label="Record"><RecordReferencePicker value={grantForm.recordId} onChange={(recordId) => setGrantForm({ ...grantForm, recordId })} options={recordOptions} isLoading={recordOptionsLoading} /></Field>
            <Field label="Grantee type"><input className="recordarr-input" value={grantForm.granteeType} onChange={(e) => setGrantForm({ ...grantForm, granteeType: e.target.value })} /></Field>
            <Field label="Grantee ref"><input className="recordarr-input" value={grantForm.granteeRef} onChange={(e) => setGrantForm({ ...grantForm, granteeRef: e.target.value })} /></Field>
            <Field label="Permission"><input className="recordarr-input" value={grantForm.permission} onChange={(e) => setGrantForm({ ...grantForm, permission: e.target.value })} /></Field>
            <Field label="Granted by"><PersonReferencePicker value={grantForm.grantedByPersonId} onChange={(grantedByPersonId) => setGrantForm({ ...grantForm, grantedByPersonId })} /></Field>
            <Field label="Expires at"><input className="recordarr-input" value={grantForm.expiresAt} onChange={(e) => setGrantForm({ ...grantForm, expiresAt: e.target.value })} /></Field>
          </div>
          <button type="button" className="recordarr-button" onClick={() => grantMutation.mutate()} disabled={grantMutation.isPending}>
            {grantMutation.isPending ? 'Creating...' : 'Create access grant'}
          </button>
        </div>
      </div>

      <div className="recordarr-card mt-6">
        <div className="recordarr-card-inner space-y-4">
          <div className="flex items-center gap-2">
            <ShieldCheck className="h-4 w-4 text-cyan-300" />
            <h2 className="text-lg font-semibold text-slate-50">Access policy management</h2>
          </div>
          <div className="grid gap-3 md:grid-cols-2">
            <Field label="Record"><RecordReferencePicker value={policyForm.recordId} onChange={(recordId) => setPolicyForm({ ...policyForm, recordId })} options={recordOptions} isLoading={recordOptionsLoading} /></Field>
            <Field label="Policy type"><input className="recordarr-input" value={policyForm.policyType} onChange={(e) => setPolicyForm({ ...policyForm, policyType: e.target.value })} /></Field>
            <Field label="Status"><input className="recordarr-input" value={policyForm.status} onChange={(e) => setPolicyForm({ ...policyForm, status: e.target.value })} /></Field>
            <Field label="Created by"><PersonReferencePicker value={policyForm.createdByPersonId} onChange={(createdByPersonId) => setPolicyForm({ ...policyForm, createdByPersonId })} /></Field>
            <Field label="Read rules" wide><textarea className="recordarr-textarea" value={policyForm.readRules} onChange={(e) => setPolicyForm({ ...policyForm, readRules: e.target.value })} /></Field>
            <Field label="Write rules" wide><textarea className="recordarr-textarea" value={policyForm.writeRules} onChange={(e) => setPolicyForm({ ...policyForm, writeRules: e.target.value })} /></Field>
            <Field label="Download rules" wide><textarea className="recordarr-textarea" value={policyForm.downloadRules} onChange={(e) => setPolicyForm({ ...policyForm, downloadRules: e.target.value })} /></Field>
            <Field label="Share rules" wide><textarea className="recordarr-textarea" value={policyForm.shareRules} onChange={(e) => setPolicyForm({ ...policyForm, shareRules: e.target.value })} /></Field>
            <Field label="Export rules" wide><textarea className="recordarr-textarea" value={policyForm.exportRules} onChange={(e) => setPolicyForm({ ...policyForm, exportRules: e.target.value })} /></Field>
            <Field label="Purge rules" wide><textarea className="recordarr-textarea" value={policyForm.purgeRules} onChange={(e) => setPolicyForm({ ...policyForm, purgeRules: e.target.value })} /></Field>
          </div>
          <button type="button" className="recordarr-button" onClick={() => policyMutation.mutate()} disabled={policyMutation.isPending}>
            {policyMutation.isPending ? 'Creating...' : 'Create access policy'}
          </button>
        </div>
      </div>

      <div className="recordarr-grid cols-2">
        <Card title="Access policies" icon={<LockKeyhole className="h-4 w-4 text-cyan-300" />}>
          {policiesQuery.data?.map((policy) => (
            <div key={policy.accessPolicyId} className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3 text-sm text-slate-300">
              <div className="flex items-center justify-between gap-3">
                <strong className="text-slate-100">{policy.policyType}</strong>
                <span className="recordarr-pill text-[0.7rem]">{policy.status}</span>
              </div>
              <p className="mt-1">{policy.recordId}</p>
              <p className="mt-2 text-xs text-slate-400">Read: {policy.readRules.join(', ')}</p>
              <div className="mt-3 flex flex-wrap gap-2">
                <button
                  type="button"
                  className="recordarr-button secondary"
                  onClick={() => togglePolicyStatusMutation.mutate({ policy, status: 'active' })}
                  disabled={togglePolicyStatusMutation.isPending || policy.status === 'active'}
                >
                  Activate
                </button>
                <button
                  type="button"
                  className="recordarr-button secondary"
                  onClick={() => togglePolicyStatusMutation.mutate({ policy, status: 'inactive' })}
                  disabled={togglePolicyStatusMutation.isPending || policy.status === 'inactive'}
                >
                  Deactivate
                </button>
              </div>
            </div>
          ))}
          {!policiesQuery.data?.length && !policiesQuery.isLoading ? <EmptyState title="No access policies." /> : null}
        </Card>
        <Card title="Access grants" icon={<BadgeCheck className="h-4 w-4 text-cyan-300" />}>
          {grantsQuery.data?.map((grant) => (
            <div key={grant.accessGrantId} className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3 text-sm text-slate-300">
              <div className="flex items-center justify-between gap-3">
                <strong className="text-slate-100">{grant.granteeRef}</strong>
                <span className="recordarr-pill text-[0.7rem]">{grant.permission}</span>
              </div>
              <p className="mt-1">{grant.recordId}</p>
              <p className="mt-1 text-xs text-slate-400">{grant.granteeType} · {grant.status}</p>
              <p className="mt-1 text-xs text-slate-400">Expires {formatDate(grant.expiresAt)}</p>
              <div className="mt-3 flex flex-wrap gap-2">
                <button
                  type="button"
                  className="recordarr-button secondary"
                  onClick={() => revokeAccessGrant(accessToken, grant.accessGrantId, { revokedByPersonId: actorPersonId, revokeReason: '' }).then(() => queryClient.invalidateQueries({ queryKey: ['recordarr'] }))}
                  disabled={grant.status === 'revoked' || grant.status === 'expired'}
                >
                  Revoke
                </button>
              </div>
            </div>
          ))}
          {!grantsQuery.data?.length && !grantsQuery.isLoading ? <EmptyState title="No access grants." /> : null}
        </Card>
      </div>

      <div className="recordarr-card mt-6">
        <div className="recordarr-card-inner space-y-4">
          <div className="flex items-center gap-2">
            <Archive className="h-4 w-4 text-cyan-300" />
            <h2 className="text-lg font-semibold text-slate-50">Redaction management</h2>
          </div>
          <div className="grid gap-3 md:grid-cols-2">
            <Field label="Source record"><RecordReferencePicker value={redactionForm.sourceRecordId} onChange={(sourceRecordId) => setRedactionForm({ ...redactionForm, sourceRecordId })} options={recordOptions} isLoading={recordOptionsLoading} /></Field>
            <Field label="Redacted record"><RecordReferencePicker value={redactionForm.redactedRecordId} onChange={(redactedRecordId) => setRedactionForm({ ...redactionForm, redactedRecordId })} options={recordOptions} isLoading={recordOptionsLoading} /></Field>
            <Field label="Reason"><input className="recordarr-input" value={redactionForm.redactionReason} onChange={(e) => setRedactionForm({ ...redactionForm, redactionReason: e.target.value })} /></Field>
            <Field label="Redacted by"><PersonReferencePicker value={redactionForm.redactedByPersonId} onChange={(redactedByPersonId) => setRedactionForm({ ...redactionForm, redactedByPersonId })} /></Field>
            <Field label="Redaction rules" wide><textarea className="recordarr-textarea" value={redactionForm.redactionRules} onChange={(e) => setRedactionForm({ ...redactionForm, redactionRules: e.target.value })} /></Field>
          </div>
          <button type="button" className="recordarr-button" onClick={() => redactionMutation.mutate()} disabled={redactionMutation.isPending}>
            {redactionMutation.isPending ? 'Creating...' : 'Create redacted copy'}
          </button>
        </div>
      </div>

      <div className="recordarr-grid cols-2">
        <Card title="External shares" icon={<Upload className="h-4 w-4 text-cyan-300" />}>
          {sharesQuery.data?.map((share) => (
            <div key={share.externalShareId} className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3 text-sm text-slate-300">
              <div className="flex items-center justify-between gap-3">
                <strong className="text-slate-100">{share.shareNumber}</strong>
                <span className="recordarr-pill text-[0.7rem]">{share.status}</span>
              </div>
              <p className="mt-1">{share.recipientName} · {share.recipientEmail}</p>
              <p className="mt-1 text-xs text-slate-400">
                Policy: {getSharePolicy(share.recordId)?.policyType ?? 'none'}
              </p>
              <p className="mt-1 text-xs text-slate-400">Expires {formatDate(share.expiresAt)}</p>
              <div className="mt-3 flex flex-wrap gap-2">
                <button type="button" className="recordarr-button secondary" onClick={() => revokeExternalShare(accessToken, share.externalShareId, { revokedByPersonId: actorPersonId }).then(() => queryClient.invalidateQueries({ queryKey: ['recordarr'] }))}>
                  Revoke
                </button>
                <button
                  type="button"
                  className="recordarr-button secondary"
                  onClick={() => accessShareMutation.mutate(share.externalShareId)}
                  disabled={accessShareMutation.isPending || share.status === 'revoked' || share.status === 'expired'}
                >
                  {share.status === 'created' ? 'Activate / log access' : 'Log access'}
                </button>
                <button
                  type="button"
                  className="recordarr-button secondary"
                  onClick={() => expireShareMutation.mutate(share.externalShareId)}
                  disabled={expireShareMutation.isPending || share.status === 'expired'}
                >
                  Expire
                </button>
              </div>
            </div>
          ))}
          {!sharesQuery.data?.length && !sharesQuery.isLoading ? <EmptyState title="No external shares." /> : null}
        </Card>
        <Card title="Redactions and logs" icon={<History className="h-4 w-4 text-cyan-300" />}>
          <div className="space-y-3">
            {redactionsQuery.data?.map((redaction) => (
              <div key={redaction.redactionId} className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3 text-sm text-slate-300">
                <div className="flex items-center justify-between gap-3">
                  <strong className="text-slate-100">{redaction.redactedRecordId}</strong>
                  <span className="recordarr-pill text-[0.7rem]">{redaction.status}</span>
                </div>
                <p className="mt-1">{redaction.redactionReason}</p>
              </div>
            ))}
            {disposalQuery.data?.map((review) => (
              <div key={review.disposalReviewId} className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3 text-sm text-slate-300">
                <div className="flex items-center justify-between gap-3">
                  <strong className="text-slate-100">{review.recordId}</strong>
                  <span className="recordarr-pill text-[0.7rem]">{review.status}</span>
                </div>
                <p className="mt-1">{review.proposedAction}</p>
              </div>
            ))}
            {logsQuery.data?.slice(0, 4).map((log) => (
              <div key={log.accessLogId} className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3 text-sm text-slate-300">
                <div className="flex items-center justify-between gap-3">
                  <strong className="text-slate-100">{log.action}</strong>
                  <span className="recordarr-pill text-[0.7rem]">{log.result}</span>
                </div>
                <p className="mt-1">{formatDate(log.occurredAt)}</p>
              </div>
            ))}
            {!redactionsQuery.data?.length && !disposalQuery.data?.length && !logsQuery.data?.length ? <EmptyState title="No redactions or logs yet." /> : null}
          </div>
        </Card>
      </div>
    </div>
  )
}

function SettingsPage({
  accessToken,
  session,
}: {
  accessToken: string
  session: StoredRecordArrSession | null
}) {
  return (
    <div className="recordarr-page">
      <SectionHeader
        eyebrow="Settings"
        title="Workspace settings"
        description="Launch routes, API wiring, and current identity details for the RecordArr workspace."
        action={<span className="recordarr-pill"><Settings className="h-4 w-4" /> Local preview</span>}
      />
      <div className="recordarr-card">
        <div className="recordarr-card-inner space-y-3 text-sm text-slate-300">
          <p><strong className="text-slate-100">API base:</strong> <span className="recordarr-kbd">{apiBase || '/api proxy'}</span></p>
          <p><strong className="text-slate-100">Preview port:</strong> <span className="recordarr-kbd">5184</span></p>
          <p><strong className="text-slate-100">API port:</strong> <span className="recordarr-kbd">5110</span></p>
          <p><strong className="text-slate-100">Suite home:</strong> <span className="recordarr-kbd">{suiteHomeUrl}</span></p>
          <p><strong className="text-slate-100">Access token present:</strong> <span className="recordarr-kbd">{accessToken ? 'yes' : 'no'}</span></p>
          <p><strong className="text-slate-100">Current user:</strong> {session?.displayName ?? 'signed out'}</p>
        </div>
      </div>
    </div>
  )
}

export function App() {
  const location = useLocation()
  const { session, sessionQuery, launchCatalogQuery, bootstrapError, workspaceSession, switcherEntitlements, launch } = useRecordArrWorkspace()
  const [bootstrapRedirected, setBootstrapRedirected] = useState(false)

  useEffect(() => {
    if (bootstrapError && !bootstrapRedirected) {
      clearSession()
      setBootstrapRedirected(true)
    }
  }, [bootstrapError, bootstrapRedirected])

  const routerBasename = import.meta.env.VITE_ROUTER_BASENAME?.replace(/\/+$/, '') ?? ''
  const normalizedPathname = (() => {
    const pathname = location.pathname.replace(/\/+$/, '') || '/'
    if (routerBasename && pathname.startsWith(routerBasename)) {
      const stripped = pathname.slice(routerBasename.length)
      return stripped || '/'
    }
    return pathname
  })()

  if (normalizedPathname === '/launch' || normalizedPathname === '/handoff') {
    return <LaunchPage />
  }

  const accessToken = session?.accessToken ?? ''
  const actorPersonId = session?.personId ?? ''

  if (!session && !sessionQuery.isLoading && !launchCatalogQuery.isLoading && !bootstrapError) {
    return (
      <main className="flex min-h-screen items-center justify-center p-6">
        <div className="max-w-md rounded-xl border border-slate-700 bg-slate-900/80 p-8 text-center shadow-lg">
          <h1 className="text-lg font-semibold text-slate-50">Sign in required</h1>
          <p className="mt-3 text-sm text-slate-300">Launch RecordArr from the STL Compliance suite to open your workspace.</p>
        </div>
      </main>
    )
  }

  return (
    <ProductWorkspaceFrame
      productName="RecordArr"
      productKey="recordarr"
      workspaceSubtitle="Records, evidence, retention, and controlled documents"
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
        <Route index element={<DashboardPage accessToken={accessToken} />} />
        <Route path="/records" element={<RecordsPage accessToken={accessToken} actorPersonId={actorPersonId} />} />
        <Route path="/records/:recordId" element={<RecordDetailPage accessToken={accessToken} actorPersonId={actorPersonId} />} />
        <Route path="/capture" element={<CapturePage accessToken={accessToken} actorPersonId={actorPersonId} />} />
        <Route path="/upload-sessions" element={<CapturePage accessToken={accessToken} actorPersonId={actorPersonId} />} />
        <Route path="/uploads" element={<CapturePage accessToken={accessToken} actorPersonId={actorPersonId} />} />
        <Route path="/scan-processing" element={<CapturePage accessToken={accessToken} actorPersonId={actorPersonId} />} />
        <Route path="/ocr-review" element={<CapturePage accessToken={accessToken} actorPersonId={actorPersonId} />} />
        <Route path="/evidence-mappings" element={<CapturePage accessToken={accessToken} actorPersonId={actorPersonId} />} />
        <Route path="/documents" element={<DocumentsPage accessToken={accessToken} actorPersonId={actorPersonId} />} />
        <Route path="/controlled-documents" element={<DocumentsPage accessToken={accessToken} actorPersonId={actorPersonId} />} />
        <Route path="/document-reviews" element={<DocumentsPage accessToken={accessToken} actorPersonId={actorPersonId} />} />
        <Route path="/distributions" element={<DocumentsPage accessToken={accessToken} actorPersonId={actorPersonId} />} />
        <Route path="/acknowledgements" element={<DocumentsPage accessToken={accessToken} actorPersonId={actorPersonId} />} />
        <Route path="/packages" element={<PackagesPage accessToken={accessToken} />} />
        <Route path="/record-packages" element={<PackagesPage accessToken={accessToken} />} />
        <Route path="/retention" element={<RetentionPage accessToken={accessToken} actorPersonId={actorPersonId} />} />
        <Route path="/disposal-reviews" element={<RetentionPage accessToken={accessToken} actorPersonId={actorPersonId} />} />
        <Route path="/holds" element={<HoldsPage accessToken={accessToken} actorPersonId={actorPersonId} />} />
        <Route path="/legal-holds" element={<HoldsPage accessToken={accessToken} actorPersonId={actorPersonId} />} />
        <Route path="/access" element={<AccessPage accessToken={accessToken} actorPersonId={actorPersonId} />} />
        <Route path="/external-shares" element={<AccessPage accessToken={accessToken} actorPersonId={actorPersonId} />} />
        <Route path="/redactions" element={<AccessPage accessToken={accessToken} actorPersonId={actorPersonId} />} />
        <Route path="/access-logs" element={<AccessPage accessToken={accessToken} actorPersonId={actorPersonId} />} />
        <Route path="/settings" element={<SettingsPage accessToken={accessToken} session={session} />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </ProductWorkspaceFrame>
  )
}
