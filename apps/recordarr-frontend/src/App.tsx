import { useEffect, useMemo, useRef, useState, type ChangeEvent, type KeyboardEvent as ReactKeyboardEvent, type PointerEvent as ReactPointerEvent, type ReactNode } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  Archive,
  BadgeCheck,
  ChevronDown,
  Camera,
  CheckCircle2,
  CircleDashed,
  Crop,
  Clock3,
  Filter,
  FileText,
  FileUp,
  History,
  LayoutDashboard,
  LockKeyhole,
  MessageSquare,
  PackageSearch,
  ScanSearch,
  Search,
  Settings,
  Sparkles,
  ShieldCheck,
  SlidersHorizontal,
  Upload,
} from 'lucide-react'
import { Navigate, Route, Routes, useLocation, useNavigate, useParams } from 'react-router-dom'
import {
  ApiErrorCallout,
  AsyncSearchPicker,
  ControlledSelect,
  StaticSearchPicker,
  ProductWorkspaceFrame,
  buildSourceObjectRef,
  buildProductLaunchUrlMap,
  formatDisplayLabel,
  formatProductLaunchError,
  getLaunchCatalog,
  getErrorMessage,
  ReferenceProviderClient,
  ReferenceSearchPicker,
  SourceReferenceSearchPicker,
  resolveProductWorkspaceBootstrapError,
  resolveSuiteHomeUrl,
  SUITE_SOURCE_PRODUCT_OPTIONS,
  useRegisterPrintableSurface,
  useProductWorkspaceLaunch,
  type PickerOption,
  type ProductNavItem,
  type SourceReferenceOption,
} from '@stl/shared-ui'
import { LaunchPage } from './LaunchPage'
import { RecordPrintPreview, RecordPrintToolbarActions } from './components/RecordPrint'
import { buildRecordSnapshotSummary, buildRecordTechnicalDetails } from './lib/recordSnapshot'
import {
  activateLegalHold,
  archiveRecord,
  applyManualCorrection,
  createControlledDocument,
  createAccessGrant,
  createDocumentAcknowledgement,
  createDocumentDistribution,
  createDocumentReview,
  createDocumentVersion,
  archiveControlledDocument,
  createDisposalReview,
  createPhotoEvidence,
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
  listVocabularyTerms,
  releaseLegalHold,
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
  updateAccessPolicy,
  revokeAccessGrant,
  revokeDocumentDistribution,
  lockPackage,
  updateRecord,
  createRecordMetadata,
  createRecordLink,
  createRecordComment,
  type RecordArrAccessPolicy,
  type RecordArrFile,
  type RecordArrControlledDocument,
  type RecordArrLegalHold,
  type RecordArrPackage,
  type RecordArrReminder,
  type RecordArrRecord,
  type RecordArrScanProcessing,
  type VocabularyTerm,
  listRecordComments,
  updateRecordComment,
} from './api/client'
import { clearSession, loadSession, type StoredRecordArrSession } from './auth/sessionStorage'

const suiteHomeUrl = resolveSuiteHomeUrl(import.meta.env.VITE_SUITE_URL)
const productLaunchUrls = buildProductLaunchUrlMap(import.meta.env)
const apiBase = import.meta.env.VITE_RECORDARR_API_BASE ?? ''
const staffArrApiBase = import.meta.env.VITE_STAFFARR_API_BASE ?? ''
const complianceCoreApiBase = import.meta.env.VITE_COMPLIANCECORE_API_BASE ?? ''
const staffReferenceClient = new ReferenceProviderClient({
  baseUrl: staffArrApiBase,
  getHeaders: () => ({
    Authorization: `Bearer ${loadSession()?.accessToken ?? ''}`,
  }),
})
const supplyReferenceClient = new ReferenceProviderClient({
  baseUrl: import.meta.env.VITE_SUPPLYARR_API_BASE ?? apiBase,
  getHeaders: () => ({
    Authorization: `Bearer ${loadSession()?.accessToken ?? ''}`,
  }),
})
const customReferenceClient = new ReferenceProviderClient({
  baseUrl: import.meta.env.VITE_CUSTOMARR_API_BASE ?? apiBase,
  getHeaders: () => ({
    Authorization: `Bearer ${loadSession()?.accessToken ?? ''}`,
  }),
})
const maintainReferenceClient = new ReferenceProviderClient({
  baseUrl: import.meta.env.VITE_MAINTAINARR_API_BASE ?? apiBase,
  getHeaders: () => ({
    Authorization: `Bearer ${loadSession()?.accessToken ?? ''}`,
  }),
})

type StaffRoleOption = {
  roleId: string
  name: string
  roleType: string
  isArchived: boolean
}

type GeometryPoint = {
  x: number
  y: number
}

type CaptureFormState = {
  title: string
  description: string
  documentClass: string
  documentType: string
  documentSubtype: string
  classification: string
  sourceProduct: string
  sourceObjectType: string
  sourceObjectId: string
  sourceObjectDisplayName: string
  ownerPersonId: string
}

type CaptureFileSource = 'camera' | 'upload'

function stripFileName(name: string) {
  return name.replace(/\.[^.]+$/, '').replace(/[-_]+/g, ' ').trim() || name
}

function createCaptureForm(actorPersonId: string): CaptureFormState {
  return {
    title: '',
    description: '',
    documentClass: '',
    documentType: '',
    documentSubtype: '',
    classification: 'internal',
    sourceProduct: '',
    sourceObjectType: '',
    sourceObjectId: '',
    sourceObjectDisplayName: '',
    ownerPersonId: actorPersonId,
  }
}

type RecordColumnKey =
  | 'title'
  | 'class'
  | 'type'
  | 'subtype'
  | 'product'
  | 'relatedRecord'
  | 'party'
  | 'documentDate'
  | 'filedDate'
  | 'status'
  | 'retention'
  | 'owner'
  | 'ocr'

type RecordFilters = {
  status: string
  classification: string
  recordType: string
  sourceProduct: string
}

const recordColumnDefinitions: { key: RecordColumnKey; label: string }[] = [
  { key: 'title', label: 'Title' },
  { key: 'class', label: 'Class' },
  { key: 'type', label: 'Type' },
  { key: 'subtype', label: 'Subtype' },
  { key: 'product', label: 'Product' },
  { key: 'relatedRecord', label: 'Related record' },
  { key: 'party', label: 'Party' },
  { key: 'documentDate', label: 'Document date' },
  { key: 'filedDate', label: 'Filed date' },
  { key: 'status', label: 'Status' },
  { key: 'retention', label: 'Retention' },
  { key: 'owner', label: 'Owner' },
  { key: 'ocr', label: 'OCR' },
]

const recordColumnDefaults: RecordColumnKey[] = ['title', 'class', 'type', 'relatedRecord', 'status']

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

function isPrintPreviewLocation(search: string) {
  const params = new URLSearchParams(search)
  return params.get('print') === '1' || params.get('printPreview') === '1'
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
  htmlFor,
}: {
  label: string
  children: ReactNode
  wide?: boolean
  htmlFor?: string
}) {
  return (
    <div className={wide ? 'md:col-span-2' : ''}>
      <label className="recordarr-label mb-2 block" htmlFor={htmlFor}>
        {label}
      </label>
      {children}
    </div>
  )
}

function ReadableOption({ value }: { value: string }) {
  return <option value={value}>{formatDisplayLabel(value)}</option>
}

type StaffPersonSummaryResponse = {
  personId: string
  displayName: string
  employmentStatus: string
  jobTitle: string | null
}

function toStaffPersonOption(person: StaffPersonSummaryResponse): PickerOption {
  return {
    value: person.personId,
    label: person.jobTitle ? `${person.displayName} - ${person.jobTitle}` : person.displayName,
    inactive: person.employmentStatus !== 'active',
  }
}

async function fetchStaffPeople(accessToken: string, query: string): Promise<PickerOption[]> {
  if (!staffArrApiBase || !accessToken) {
    return []
  }

  const search = new URLSearchParams()
  if (query.trim()) {
    search.set('query', query.trim())
  }
  search.set('limit', '25')

  const response = await fetch(`${staffArrApiBase}/api/people?${search.toString()}`, {
    headers: {
      Authorization: `Bearer ${accessToken}`,
    },
  })

  if (!response.ok) {
    throw new Error('Failed to load StaffArr people')
  }

  const people = (await response.json()) as StaffPersonSummaryResponse[]
  return people.map(toStaffPersonOption)
}

async function fetchStaffPersonById(accessToken: string, personId: string): Promise<PickerOption | null> {
  if (!staffArrApiBase || !accessToken || !personId) {
    return null
  }

  const response = await fetch(`${staffArrApiBase}/api/people/${encodeURIComponent(personId)}`, {
    headers: {
      Authorization: `Bearer ${accessToken}`,
    },
  })

  if (!response.ok) {
    return null
  }

  const person = (await response.json()) as StaffPersonSummaryResponse
  return toStaffPersonOption(person)
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

function useVocabularyTermOptions(accessToken: string, vocabularyTypeKey: string) {
  const vocabularyQuery = useQuery({
    queryKey: ['recordarr', 'compliancecore-vocabulary', vocabularyTypeKey, accessToken],
    queryFn: () => listVocabularyTerms(accessToken, vocabularyTypeKey),
    enabled: Boolean(accessToken && complianceCoreApiBase && vocabularyTypeKey),
    retry: false,
  })

  const options = useMemo(
    () =>
      (vocabularyQuery.data ?? []).map((term: VocabularyTerm) => ({
        value: term.termKey,
        label: `${term.label} · ${term.termKey}`,
        inactive: !term.isActive,
      })),
    [vocabularyQuery.data],
  )

  return { options, isLoading: vocabularyQuery.isLoading }
}

function useStaffRoleOptions(accessToken: string) {
  const rolesQuery = useQuery({
    queryKey: ['recordarr', 'staffarr-roles', accessToken],
    queryFn: async () => {
      if (!staffArrApiBase || !accessToken) return []
      const response = await fetch(`${staffArrApiBase}/api/v1/roles`, {
        headers: {
          Authorization: `Bearer ${accessToken}`,
        },
      })
      if (!response.ok) {
        throw new Error('Failed to load StaffArr roles')
      }
      return (await response.json()) as StaffRoleOption[]
    },
    enabled: Boolean(accessToken),
    retry: false,
  })

  const options = useMemo(
    () =>
      (rolesQuery.data ?? []).map((role) => ({
        value: role.roleId,
        label: `${role.name} · ${role.roleType}`,
        inactive: role.isArchived,
      })),
    [rolesQuery.data],
  )

  return { options, isLoading: rolesQuery.isLoading }
}

function GranteeRefPicker({
  granteeType,
  value,
  onChange,
}: {
  granteeType: string
  value: string
  onChange: (value: string) => void
}) {
  const session = loadSession()
  const accessToken = session?.accessToken ?? ''
  const roleOptions = useStaffRoleOptions(accessToken)

  if (granteeType === 'person') {
    return <PersonReferencePicker value={value} onChange={onChange} placeholder="Search StaffArr people" />
  }

  if (granteeType === 'org_unit') {
    return (
      <ReferenceSearchPicker
        client={staffReferenceClient}
        referenceType="org_unit"
        value={value}
        onChange={onChange}
        placeholder="Search StaffArr org units"
      />
    )
  }

  return (
    <StaticSearchPicker
      value={value}
      onChange={onChange}
      options={roleOptions.options}
      placeholder={roleOptions.isLoading ? 'Loading StaffArr roles…' : 'Search StaffArr roles'}
      disabled={roleOptions.isLoading}
    />
  )
}

function PersonReferencePicker({
  id,
  value,
  onChange,
  placeholder = 'Search StaffArr people',
  disabled = false,
}: {
  id?: string
  value: string
  onChange: (value: string) => void
  placeholder?: string
  disabled?: boolean
}) {
  const session = loadSession()
  const accessToken = session?.accessToken ?? ''
  const selectedPersonQuery = useQuery({
    queryKey: ['recordarr', 'staffarr-person', value],
    queryFn: () => fetchStaffPersonById(accessToken, value),
    enabled: Boolean(accessToken && value),
    retry: false,
  })

  return (
    <AsyncSearchPicker
      value={value}
      onChange={onChange}
      queryKey={['recordarr', 'staffarr-people', accessToken]}
      queryFn={(query) => fetchStaffPeople(accessToken, query)}
      selectedOption={selectedPersonQuery.data ?? (value ? { value, label: value, inactive: true } : undefined)}
      placeholder={placeholder}
      id={id}
      disabled={disabled || !accessToken || !staffArrApiBase}
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
  disabled = false,
}: {
  id?: string
  value: string
  onChange: (value: string) => void
  disabled?: boolean
}) {
  return (
    <ControlledSelect
      id={id}
      value={value}
      onChange={onChange}
      options={SUITE_SOURCE_PRODUCT_OPTIONS}
      className="recordarr-select"
      emptyLabel="Select source product"
      disabled={disabled}
    />
  )
}

function SourceObjectRefPicker({
  id,
  value,
  sourceProduct,
  onChange,
  disabled = false,
}: {
  id?: string
  value: string
  sourceProduct?: string | null
  onChange: (value: string, selected?: SourceReferenceOption | null) => void
  disabled?: boolean
}) {
  return (
    <SourceReferenceSearchPicker
      id={id}
      clientsByProduct={{
        staffarr: staffReferenceClient,
        supplyarr: supplyReferenceClient,
        customarr: customReferenceClient,
        maintainarr: maintainReferenceClient,
      }}
      sourceProduct={sourceProduct}
      value={value}
      onChange={onChange}
      placeholder="Search source records"
      disabled={disabled}
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

type WorkspacePageProps = {
  accessToken: string
  actorPersonId: string
  actorDisplayName?: string
  tenantDisplayName?: string
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

function readFileAsBase64(file: File): Promise<string> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader()
    reader.onload = () => {
      const result = typeof reader.result === 'string' ? reader.result : ''
      resolve(result.includes(',') ? result.split(',', 2)[1] ?? '' : result)
    }
    reader.onerror = () => reject(new Error('Failed to read file.'))
    reader.readAsDataURL(file)
  })
}

function formatBytes(bytes: number): string {
  if (!Number.isFinite(bytes) || bytes < 0) {
    return '0 B'
  }

  const units = ['B', 'KB', 'MB', 'GB', 'TB']
  let value = bytes
  let unitIndex = 0

  while (value >= 1024 && unitIndex < units.length - 1) {
    value /= 1024
    unitIndex += 1
  }

  const rounded = unitIndex === 0 ? value : Number(value.toFixed(value >= 10 ? 0 : 1))
  return `${rounded} ${units[unitIndex]}`
}

function isImageFile(file: File): boolean {
  return file.type.startsWith('image/') || /\.(avif|gif|heic|heif|jpe?g|png|webp|bmp|tiff?)$/i.test(file.name)
}

async function loadImageDimensions(file: File): Promise<{ width: number; height: number }> {
  const source = URL.createObjectURL(file)
  try {
    const image = await new Promise<HTMLImageElement>((resolve, reject) => {
      const nextImage = new Image()
      nextImage.onload = () => resolve(nextImage)
      nextImage.onerror = () => reject(new Error('Failed to load preview image.'))
      nextImage.src = source
    })

    if (!image.naturalWidth || !image.naturalHeight) {
      throw new Error('Preview image dimensions are unavailable.')
    }

    return { width: image.naturalWidth, height: image.naturalHeight }
  } finally {
    URL.revokeObjectURL(source)
  }
}

function defaultGeometryPoints(width: number, height: number): GeometryPoint[] {
  return [
    { x: width * 0.08, y: height * 0.08 },
    { x: width * 0.92, y: height * 0.1 },
    { x: width * 0.9, y: height * 0.9 },
    { x: width * 0.1, y: height * 0.92 },
  ]
}

function clampGeometryPoint(point: GeometryPoint, width: number, height: number): GeometryPoint {
  return {
    x: Math.min(width, Math.max(0, point.x)),
    y: Math.min(height, Math.max(0, point.y)),
  }
}

function formatGeometryPoints(points: GeometryPoint[]) {
  return points
    .map((point) => `${Math.round(point.x)},${Math.round(point.y)}`)
    .join(',')
}

function RecordsPage({ accessToken, actorPersonId: _actorPersonId, actorDisplayName: _actorDisplayName }: WorkspacePageProps) {
  const navigate = useNavigate()
  const [search, setSearch] = useState('')
  const [showFilters, setShowFilters] = useState(false)
  const [filters, setFilters] = useState<RecordFilters>({
    status: 'all',
    classification: 'all',
    recordType: 'all',
    sourceProduct: 'all',
  })
  const [visibleColumnKeys, setVisibleColumnKeys] = useState<RecordColumnKey[]>(recordColumnDefaults)

  const recordsQuery = useQuery({
    queryKey: ['recordarr', 'records', search.trim()],
    queryFn: () => listRecords(accessToken, search),
    enabled: Boolean(accessToken),
  })

  const sortedRecords = useMemo(
    () =>
      [...(recordsQuery.data ?? [])].sort(
        (left, right) => new Date(right.uploadedAt).getTime() - new Date(left.uploadedAt).getTime(),
      ),
    [recordsQuery.data],
  )

  const filterOptions = useMemo(
    () => ({
      statuses: [...new Set(sortedRecords.map((record) => record.status))].filter(Boolean),
      classifications: [...new Set(sortedRecords.map((record) => record.classification))].filter(Boolean),
      recordTypes: [...new Set(sortedRecords.map((record) => record.recordType))].filter(Boolean),
      sourceProducts: [...new Set(sortedRecords.map((record) => record.sourceProduct))].filter(Boolean),
    }),
    [sortedRecords],
  )

  const filteredRecords = useMemo(
    () =>
      sortedRecords.filter((record) => {
        if (filters.status !== 'all' && record.status !== filters.status) return false
        if (filters.classification !== 'all' && record.classification !== filters.classification) return false
        if (filters.recordType !== 'all' && record.recordType !== filters.recordType) return false
        if (filters.sourceProduct !== 'all' && record.sourceProduct !== filters.sourceProduct) return false
        return true
      }),
    [filters, sortedRecords],
  )

  const visibleColumns = useMemo(
    () => recordColumnDefinitions.filter((column) => visibleColumnKeys.includes(column.key)),
    [visibleColumnKeys],
  )

  const selectedColumnCount = visibleColumnKeys.length
  const canAddColumn = selectedColumnCount < 5

  return (
    <div className="recordarr-page">
      <SectionHeader
        eyebrow="Records"
        title="Documents"
        description="Search existing documents and narrow results with drawer filters."
        action={
          <div className="flex flex-wrap items-center gap-2">
            <button type="button" className="recordarr-button secondary" onClick={() => setShowFilters((current) => !current)}>
              <Filter className="h-4 w-4" />
              Filters
              <ChevronDown className="h-4 w-4" />
            </button>
            <button type="button" className="recordarr-button" onClick={() => navigate('/capture')}>
              <FileUp className="h-4 w-4" />
              Capture document
            </button>
            <span className="recordarr-pill">
              <FileText className="h-4 w-4" /> {filteredRecords.length} documents
            </span>
          </div>
        }
      />
      <div className="recordarr-card">
        <div className="recordarr-card-inner space-y-3">
          <div className="flex items-center gap-2">
            <Search className="h-4 w-4 text-cyan-300" />
            <h2 className="text-lg font-semibold text-slate-50">Search</h2>
          </div>
          <Field label="Search" wide>
            <input
              className="recordarr-input"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Search title, OCR text, vendor, PO number, person, asset, related record..."
            />
          </Field>
          <div className="flex flex-wrap items-center gap-3">
            <button type="button" className="recordarr-button secondary" onClick={() => setSearch('')} disabled={!search}>
              Clear search
            </button>
            <span className="text-sm text-[var(--color-text-secondary)]">
              Search matches record number, title, description, source, file name, and tags.
            </span>
          </div>
        </div>
      </div>

      <div className="recordarr-card">
        <div className="recordarr-card-inner space-y-4">
          <div className="flex items-center gap-2">
            <SlidersHorizontal className="h-4 w-4 text-cyan-300" />
            <div>
              <h2 className="text-lg font-semibold text-slate-50">Visible columns</h2>
              <p className="text-sm text-[var(--color-text-secondary)]">Choose up to 5 columns for the document table.</p>
            </div>
          </div>
          <div className="flex flex-wrap gap-x-4 gap-y-3">
            {recordColumnDefinitions.map((column) => {
              const checked = visibleColumnKeys.includes(column.key)
              const disabled = !checked && !canAddColumn

              return (
                <label
                  key={column.key}
                  className={[
                    'flex items-center gap-2 rounded-full border px-3 py-2 text-sm transition-colors',
                    checked
                      ? 'border-cyan-400/40 bg-cyan-500/10 text-slate-50'
                      : 'border-slate-700/70 bg-slate-900/60 text-[var(--color-text-secondary)]',
                    disabled ? 'cursor-not-allowed opacity-50' : 'cursor-pointer hover:bg-slate-900/80',
                  ].join(' ')}
                >
                  <input
                    type="checkbox"
                    className="h-4 w-4 accent-cyan-400"
                    checked={checked}
                    disabled={disabled}
                    onChange={() => {
                      setVisibleColumnKeys((current) => {
                        if (current.includes(column.key)) {
                          return current.filter((key) => key !== column.key)
                        }
                        if (current.length >= 5) return current
                        return [...current, column.key]
                      })
                    }}
                  />
                  {column.label}
                </label>
              )
            })}
          </div>
          <p className="text-sm text-[var(--color-text-secondary)]">{selectedColumnCount}/5 columns visible.</p>
        </div>
      </div>

      {showFilters ? (
        <div className="recordarr-card">
          <div className="recordarr-card-inner space-y-4">
            <div className="flex items-center justify-between gap-3">
              <div>
                <h2 className="text-lg font-semibold text-slate-50">Filters</h2>
                <p className="text-sm text-[var(--color-text-secondary)]">Narrow the visible document set without leaving the page.</p>
              </div>
              <button
                type="button"
                className="recordarr-button secondary"
                onClick={() =>
                  setFilters({
                    status: 'all',
                    classification: 'all',
                    recordType: 'all',
                    sourceProduct: 'all',
                  })
                }
              >
                Clear filters
              </button>
            </div>
            <div className="grid gap-3 md:grid-cols-2 xl:grid-cols-4">
              <Field label="Status">
                <select className="recordarr-select" value={filters.status} onChange={(e) => setFilters((current) => ({ ...current, status: e.target.value }))}>
                  <option value="all">All</option>
                  {filterOptions.statuses.map((status) => (
                    <option key={status} value={status}>
                      {formatDisplayLabel(status)}
                    </option>
                  ))}
                </select>
              </Field>
              <Field label="Classification">
                <select className="recordarr-select" value={filters.classification} onChange={(e) => setFilters((current) => ({ ...current, classification: e.target.value }))}>
                  <option value="all">All</option>
                  {filterOptions.classifications.map((classification) => (
                    <option key={classification} value={classification}>
                      {formatDisplayLabel(classification)}
                    </option>
                  ))}
                </select>
              </Field>
              <Field label="Type">
                <select className="recordarr-select" value={filters.recordType} onChange={(e) => setFilters((current) => ({ ...current, recordType: e.target.value }))}>
                  <option value="all">All</option>
                  {filterOptions.recordTypes.map((recordType) => (
                    <option key={recordType} value={recordType}>
                      {formatDisplayLabel(recordType)}
                    </option>
                  ))}
                </select>
              </Field>
              <Field label="Product">
                <select className="recordarr-select" value={filters.sourceProduct} onChange={(e) => setFilters((current) => ({ ...current, sourceProduct: e.target.value }))}>
                  <option value="all">All</option>
                  {filterOptions.sourceProducts.map((sourceProduct) => (
                    <option key={sourceProduct} value={sourceProduct}>
                      {formatDisplayLabel(sourceProduct)}
                    </option>
                  ))}
                </select>
              </Field>
            </div>
          </div>
        </div>
      ) : null}

      <RecordsTable records={filteredRecords} loading={recordsQuery.isLoading} columns={visibleColumns} onSelect={(recordId) => navigate(`/records/${recordId}`)} />
    </div>
  )
}

function RecordsTable({
  records,
  loading,
  columns,
  onSelect,
}: {
  records: RecordArrRecord[]
  loading: boolean
  columns: { key: RecordColumnKey; label: string }[]
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
        {records.length === 0 && !loading ? <EmptyState title="No matching documents." /> : null}
        {records.length > 0 ? (
          <div className="overflow-x-auto rounded-2xl border border-slate-700/70">
            <table className="recordarr-table">
              <thead>
                <tr>
                  {columns.map((column) => (
                    <th key={column.key}>{column.label}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {records.map((record) => (
                  <tr key={record.recordId}>
                    {columns.map((column) => {
                      if (column.key === 'title') {
                        return (
                          <td key={column.key}>
                            <button type="button" className="text-left" onClick={() => onSelect(record.recordId)}>
                              <div className="font-semibold text-slate-50 hover:text-cyan-300">{record.title}</div>
                              <div className="text-xs text-slate-400">{record.recordNumber}</div>
                            </button>
                          </td>
                        )
                      }

                      if (column.key === 'class') {
                        return <td key={column.key} className="text-sm text-slate-300">{record.documentClass}</td>
                      }

                      if (column.key === 'type') {
                        return <td key={column.key} className="text-sm text-slate-300">{record.documentType}</td>
                      }

                      if (column.key === 'subtype') {
                        return <td key={column.key} className="text-sm text-slate-300">{record.documentSubtype}</td>
                      }

                      if (column.key === 'product') {
                        return <td key={column.key} className="text-sm text-slate-300">{record.sourceProduct}</td>
                      }

                      if (column.key === 'relatedRecord') {
                        return (
                          <td key={column.key} className="text-sm text-cyan-300">
                            <button type="button" className="text-left hover:underline" onClick={() => onSelect(record.recordId)}>
                              {record.sourceObjectDisplayName || 'Unlinked'}
                            </button>
                          </td>
                        )
                      }

                      if (column.key === 'party') {
                        return <td key={column.key} className="text-sm text-slate-300">{record.sourceObjectDisplayName || '—'}</td>
                      }

                      if (column.key === 'documentDate') {
                        return <td key={column.key} className="text-sm text-slate-300">{formatDate(record.effectiveAt ?? record.uploadedAt)}</td>
                      }

                      if (column.key === 'filedDate') {
                        return <td key={column.key} className="text-sm text-slate-300">{formatDate(record.uploadedAt)}</td>
                      }

                      if (column.key === 'status') {
                        return (
                          <td key={column.key}>
                            <span className="recordarr-pill text-[0.7rem]">{formatDisplayLabel(record.status)}</span>
                          </td>
                        )
                      }

                      if (column.key === 'retention') {
                        return <td key={column.key} className="text-sm text-slate-300">{record.expiresAt ? formatDate(record.expiresAt) : 'No expiry'}</td>
                      }

                      if (column.key === 'owner') {
                        return <td key={column.key} className="text-sm text-slate-300">{record.ownerPersonId ? 'Assigned' : 'Not recorded'}</td>
                      }

                      if (column.key === 'ocr') {
                        return <td key={column.key} className="text-sm text-slate-300">{record.ocrResultRefs.length > 0 ? `${record.ocrResultRefs.length} result(s)` : 'Pending'}</td>
                      }

                      return <td key={column.key} />
                    })}
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

function RecordDetailPage({ accessToken, actorPersonId, actorDisplayName, tenantDisplayName }: WorkspacePageProps) {
  const queryClient = useQueryClient()
  const location = useLocation()
  const params = useParams()
  const recordId = params.recordId ?? ''
  const isPrintPreview = isPrintPreviewLocation(location.search)
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
  const ownerPersonQuery = useQuery({
    queryKey: ['recordarr', 'staffarr-person', recordQuery.data?.ownerPersonId ?? 'none'],
    queryFn: () => fetchStaffPersonById(accessToken, recordQuery.data?.ownerPersonId ?? ''),
    enabled: Boolean(accessToken && recordQuery.data?.ownerPersonId),
    retry: false,
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
  const redactionsQuery = useQuery({
    queryKey: ['recordarr', 'redactions'],
    queryFn: () => listRedactions(accessToken),
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
    mutationFn: () => archiveRecord(accessToken, recordId, { actorPersonId }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const purgeMutation = useMutation({
    mutationFn: () => purgeRecord(accessToken, recordId, { actorPersonId }),
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
  const relevantLogs = (logsQuery.data ?? []).filter((entry) => entry.recordId === recordId)
  const relatedScans = (scansQuery.data ?? []).filter((scan) => scan.recordId === recordId)
  const relatedMappings = (mappingsQuery.data ?? []).filter((mapping) => mapping.recordId === recordId)
  const relatedPackages = (packagesQuery.data ?? []).filter((pkg) => pkg.recordRefs.includes(recordId))
  const relatedUploads = (uploadsQuery.data ?? []).filter((upload) => upload.uploadedRecordRefs.includes(recordId))
  const relatedDocuments = (documentsQuery.data ?? []).filter((document) => document.recordId === recordId)
  const relatedRedactions = (redactionsQuery.data ?? []).filter((redaction) =>
    redaction.sourceRecordId === recordId || redaction.redactedRecordId === recordId,
  )
  const recordComments = commentsQuery.data ?? []
  const recordFiles: RecordArrFile[] = filesQuery.data ?? []
  const selectedComment = recordComments.find((comment) => comment.commentId === editingCommentId) ?? null
  const currentFile = record ? recordFiles.find((file) => file.fileId === record.currentFileRef) ?? null : null
  const ownerDisplayName = ownerPersonQuery.data?.label ?? (record?.ownerPersonId ? 'Assigned' : 'Not recorded')
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

  const printableSurface = useMemo(() => {
    if (!record) {
      return false
    }

    return {
      title: record.title,
      sourceDisplayRef: record.recordNumber,
      sourceEntityType: 'record',
      sourceEntityId: record.recordId,
      templateKey: 'recordarr.document.cover_sheet',
      documentStatus: 'working_copy' as const,
        previewLayout: 'custom' as const,
      metadata: {
        actorDisplayName,
        tenantDisplayName,
      },
      downloadPdf: {
        label: 'Download Copy PDF',
        request: {
          sourceEntityType: 'record',
          sourceEntityId: record.recordId,
          sourceDisplayRef: record.recordNumber,
          templateKey: 'recordarr.document.copy',
          documentStatus: 'copy' as const,
        },
      },
      downloadPacket: {
        label: 'Download Packet',
        request: {
          sourceEntityType: 'record',
          sourceEntityId: record.recordId,
          sourceDisplayRef: record.recordNumber,
          templateKey: 'recordarr.record.packet',
          documentStatus: 'copy' as const,
        },
      },
      archiveOfficial: {
        request: {
          sourceEntityType: 'record',
          sourceEntityId: record.recordId,
          sourceDisplayRef: record.recordNumber,
          templateKey: 'recordarr.document.original',
          documentStatus: 'official' as const,
        },
      },
      reprint: {
        sourceEntityType: 'record',
        sourceEntityId: record.recordId,
        sourceDisplayRef: record.recordNumber,
        templateKey: 'recordarr.document.copy',
        documentStatus: 'copy' as const,
        requireReason: true,
        dialogTitle: 'Reason required for record reprint',
        confirmLabel: 'Record and download copy',
        followUpAction: 'download_pdf' as const,
      },
      toolbarActions: accessToken ? (
        <RecordPrintToolbarActions
          accessToken={accessToken}
          record={record}
          actorDisplayName={actorDisplayName}
          tenantDisplayName={tenantDisplayName}
          redactions={relatedRedactions}
        />
      ) : null,
    }
  }, [accessToken, actorDisplayName, record, relatedRedactions, tenantDisplayName])

  useRegisterPrintableSurface(printableSurface)

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
      {record && isPrintPreview ? (
        <RecordPrintPreview
          record={record}
          files={recordFiles}
          retentionStatus={retentionQuery.data ?? null}
          accessLogs={relevantLogs}
          packages={relatedPackages}
          redactions={relatedRedactions}
          actorPersonId={actorPersonId}
          actorDisplayName={actorDisplayName}
          tenantDisplayName={tenantDisplayName}
        />
      ) : null}
      {record && !isPrintPreview ? (
        <>
          <div className="recordarr-grid cols-3">
            <MetricCard title="Classification" value={record.classification} hint={`${record.recordType} / ${record.documentClass} / ${record.documentType} / ${record.documentSubtype}`} />
            <MetricCard title="Effective" value={formatDate(record.effectiveAt)} hint="Lifecycle start" />
            <MetricCard title="Expires" value={formatDate(record.expiresAt)} hint="Retention clock" />
          </div>
          <div className="recordarr-grid cols-2">
            <Card title="Record snapshot" icon={<FileText className="h-4 w-4 text-cyan-300" />}>
              <div className="space-y-4 text-sm text-slate-300">
                <dl className="grid gap-3 md:grid-cols-2">
                  {buildRecordSnapshotSummary(record, ownerDisplayName).map((entry) => (
                    <div key={entry.label} className="rounded-xl border border-slate-800/80 bg-slate-950/50 p-3">
                      <dt className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">{entry.label}</dt>
                      <dd className="mt-1 break-words text-sm text-slate-100">{entry.value}</dd>
                    </div>
                  ))}
                </dl>
                <div className="flex flex-wrap gap-2 pt-1">
                  {record.tags.map((tag) => (
                    <span key={tag} className="recordarr-pill text-[0.7rem]">{tag}</span>
                  ))}
                </div>
                <details className="rounded-xl border border-slate-800/80 bg-slate-950/50 p-3">
                  <summary className="cursor-pointer text-xs font-semibold uppercase tracking-wide text-slate-100">
                    Advanced technical details
                  </summary>
                  <dl className="mt-3 grid gap-3 md:grid-cols-2">
                    {buildRecordTechnicalDetails(record).map((entry) => (
                      <div key={entry.label} className="rounded-lg border border-slate-800 bg-slate-950/80 p-3">
                        <dt className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">{entry.label}</dt>
                        <dd className="mt-1 break-all text-sm text-slate-100">{entry.value}</dd>
                      </div>
                    ))}
                  </dl>
                </details>
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
                  <details className="rounded-xl border border-slate-700/70 bg-slate-950/60 p-3 text-sm text-slate-300">
                    <summary className="cursor-pointer text-xs font-semibold uppercase tracking-wide text-slate-100">
                      Advanced technical details
                    </summary>
                    <p className="mt-3 text-xs text-slate-400">
                      This preview shows the raw downloaded text for troubleshooting. It may include the stored
                      payload rather than a human-friendly summary.
                    </p>
                    <pre className="mt-3 max-h-48 overflow-auto rounded-lg border border-slate-700/70 bg-slate-950/80 p-3 text-xs text-slate-300 whitespace-pre-wrap">
                      {selectedFileDownload}
                    </pre>
                  </details>
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

/* Legacy capture page retained for reference while the triage-first flow is rebuilt.
function CapturePage({ accessToken, actorPersonId }: WorkspacePageProps) {
  const queryClient = useQueryClient()
  const { options: documentClassOptions, isLoading: documentClassOptionsLoading } = useVocabularyTermOptions(accessToken, 'document_class')
  const { options: documentTypeOptions, isLoading: documentTypeOptionsLoading } = useVocabularyTermOptions(accessToken, 'document_type')
  const { options: documentSubtypeOptions, isLoading: documentSubtypeOptionsLoading } = useVocabularyTermOptions(accessToken, 'document_subtype')
  const [captureSource, setCaptureSource] = useState<'camera' | 'upload' | 'scanner'>('camera')
  const [previewScale, setPreviewScale] = useState(1)
  const [previewRotation, setPreviewRotation] = useState(0)
  const [draftSavedAt, setDraftSavedAt] = useState<string | null>(null)
  const [draftUploadSessionId, setDraftUploadSessionId] = useState<string>('')
  const [draftCaptureRequestId, setDraftCaptureRequestId] = useState<string>('')
  const [createdRecordId, setCreatedRecordId] = useState<string>('')
  const [capturePages, setCapturePages] = useState<CapturePageItem[]>([])
  const [captureForm, setCaptureForm] = useState({
    title: '',
    description: '',
    recordType: 'document',
    documentClass: '',
    documentType: '',
    documentSubtype: '',
    classification: 'internal',
    sourceProduct: '',
    sourceObjectType: '',
    sourceObjectId: '',
    sourceObjectDisplayName: '',
    ownerPersonId: actorPersonId,
    currentFileName: '',
    currentMimeType: 'application/pdf',
    fileContentBase64: '',
  })
  const [selectedScanId, setSelectedScanId] = useState('')
  const [scan, setScan] = useState({
    recordId: '',
    originalFileName: '',
    scanPurpose: '',
    edgeCoordinates: '',
    correctedByPersonId: actorPersonId,
  })
  const videoRef = useRef<HTMLVideoElement | null>(null)
  const cameraStreamRef = useRef<MediaStream | null>(null)
  const [cameraStatus, setCameraStatus] = useState<'idle' | 'requesting' | 'ready' | 'error'>('idle')
  const [cameraError, setCameraError] = useState<string | null>(null)
  const [cameraFacingMode, setCameraFacingMode] = useState<'environment' | 'user'>('environment')
  const uploadSessionsQuery = useQuery({
    queryKey: ['recordarr', 'upload-sessions'],
    queryFn: () => listUploadSessions(accessToken),
    enabled: Boolean(accessToken),
  })
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

  useEffect(() => {
    if (!selectedScanId && scansQuery.data?.[0]) {
      setSelectedScanId(scansQuery.data[0].scanProcessingId)
    }
  }, [scansQuery.data, selectedScanId])

  const stopCamera = useCallback(() => {
    stopMediaStream(cameraStreamRef.current)
    cameraStreamRef.current = null
    if (videoRef.current) {
      videoRef.current.srcObject = null
    }
    setCameraStatus((current) => (current === 'idle' ? current : 'idle'))
  }, [])

  const startCamera = useCallback(async () => {
    if (!navigator.mediaDevices?.getUserMedia) {
      setCameraError('Camera access is not available in this browser.')
      setCameraStatus('error')
      return
    }

    stopCamera()
    setCameraError(null)
    setCameraStatus('requesting')

    try {
      const stream = await navigator.mediaDevices.getUserMedia({
        video: {
          facingMode: { ideal: cameraFacingMode },
        },
        audio: false,
      })

      cameraStreamRef.current = stream
      const video = videoRef.current
      if (video) {
        video.srcObject = stream
        await video.play().catch(() => undefined)
      }
      setCameraStatus('ready')
    } catch (error) {
      stopCamera()
      setCameraError(getErrorMessage(error))
      setCameraStatus('error')
    }
  }, [cameraFacingMode, stopCamera])

  useEffect(() => {
    if (captureSource === 'camera') {
      void startCamera()
      return () => stopCamera()
    }

    stopCamera()
    return undefined
  }, [captureSource, startCamera, stopCamera])

  const buildCapturePayload = async () => {
    if (capturePages.length === 0) {
      throw new Error('Add at least one page before filing the record.')
    }

    if (!captureForm.title.trim()) {
      throw new Error('A title is required before filing the record.')
    }

    if (
      !captureForm.sourceProduct.trim() ||
      !captureForm.sourceObjectType.trim() ||
      !captureForm.sourceObjectId.trim() ||
      !captureForm.sourceObjectDisplayName.trim()
    ) {
      throw new Error('Select a source product and source reference before filing the record.')
    }

    const primaryPage = capturePages[0]
    const combinedIsPdf =
      capturePages.length > 1 ||
      primaryPage.file.type === 'application/pdf' ||
      primaryPage.file.name.toLowerCase().endsWith('.pdf')
    const currentFileName = combinedIsPdf
      ? `${slugifyFileName(captureForm.title || primaryPage.file.name)}-packet.pdf`
      : primaryPage.file.name
    const currentMimeType = combinedIsPdf ? 'application/pdf' : primaryPage.file.type || 'application/octet-stream'
    const fileContentBase64 = capturePages.length > 1
      ? await buildMultipagePdfBase64(capturePages)
      : await readFileAsBase64(primaryPage.file)

    return {
      currentFileName,
      currentMimeType,
      fileContentBase64,
      captureType: captureForm.recordType === 'photo' ? 'photo' : 'document_scan',
      scanPurpose: captureForm.title || captureForm.description || currentFileName,
      sourceObjectRef: captureForm.sourceProduct && captureForm.sourceObjectType && captureForm.sourceObjectId
        ? buildSourceObjectRef(captureForm.sourceProduct, captureForm.sourceObjectType, captureForm.sourceObjectId)
        : '',
    }
  }
  const saveDraftMutation = useMutation({
    mutationFn: async () => {
      const payload = await buildCapturePayload()
      const uploadSession = await createUploadSession(accessToken, {
        sourceProduct: captureForm.sourceProduct,
        sourceObjectType: captureForm.sourceObjectType,
        sourceObjectId: captureForm.sourceObjectId,
        uploadPurpose: captureForm.description || captureForm.title || payload.currentFileName,
        requiresDocumentScan: true,
        requiresOcr: true,
        requiresManualReview: true,
      })
      const captureRequest = await createCaptureRequest(accessToken, {
        sourceProduct: captureForm.sourceProduct,
        sourceObjectRef: payload.sourceObjectRef,
        captureType: payload.captureType,
        title: captureForm.title || payload.currentFileName || 'Capture request',
        instructions: captureForm.description || captureForm.title || 'Capture record',
        required: true,
        uploadSessionRef: uploadSession.uploadSessionId,
        evidenceRequirementRef: null,
      })
      return { uploadSession, captureRequest }
    },
    onSuccess: async ({ uploadSession, captureRequest }) => {
      setDraftSavedAt(uploadSession.createdAt)
      setDraftUploadSessionId(uploadSession.uploadSessionId)
      setDraftCaptureRequestId(captureRequest.captureRequestId)
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const createRecordMutation = useMutation({
    mutationFn: async () => {
      const payload = await buildCapturePayload()
      let uploadSessionId = draftUploadSessionId
      let draftCreatedAt = draftSavedAt
      if (!uploadSessionId) {
        const session = await createUploadSession(accessToken, {
          sourceProduct: captureForm.sourceProduct,
          sourceObjectType: captureForm.sourceObjectType,
          sourceObjectId: captureForm.sourceObjectId,
          uploadPurpose: captureForm.description || captureForm.title || payload.currentFileName,
          requiresDocumentScan: true,
          requiresOcr: true,
          requiresManualReview: true,
        })
        uploadSessionId = session.uploadSessionId
        draftCreatedAt = session.createdAt
      }
      let captureRequestId = draftCaptureRequestId
      if (!captureRequestId) {
        const request = await createCaptureRequest(accessToken, {
          sourceProduct: captureForm.sourceProduct,
          sourceObjectRef: payload.sourceObjectRef,
          captureType: payload.captureType,
          title: captureForm.title || payload.currentFileName || 'Capture request',
          instructions: captureForm.description || captureForm.title || 'Capture record',
          required: true,
          uploadSessionRef: uploadSessionId,
          evidenceRequirementRef: null,
        })
        captureRequestId = request.captureRequestId
      }

      const record = await createRecord(accessToken, {
        ...captureForm,
        currentFileName: payload.currentFileName,
        currentMimeType: payload.currentMimeType,
        fileContentBase64: payload.fileContentBase64,
        uploadedByPersonId: actorPersonId,
      })

      await completeUploadSession(accessToken, uploadSessionId, record.recordId)
      await completeCaptureRequest(accessToken, captureRequestId)

      const nextScan = await createScan(accessToken, {
        recordId: record.recordId,
        originalFileName: record.currentFileName,
        scanPurpose: payload.scanPurpose,
      })

      return { record, nextScan, uploadSessionId, captureRequestId, draftCreatedAt }
    },
    onSuccess: async ({ record, nextScan, uploadSessionId, captureRequestId, draftCreatedAt }) => {
      setCreatedRecordId(record.recordId)
      setDraftSavedAt(draftCreatedAt ?? new Date().toISOString())
      setDraftUploadSessionId(uploadSessionId)
      setDraftCaptureRequestId(captureRequestId)
      setCaptureForm((current) => ({
        ...current,
        currentFileName: record.currentFileName,
        currentMimeType: record.currentMimeType,
      }))
      setScan((current) => ({
        ...current,
        recordId: record.recordId,
        originalFileName: record.currentFileName,
        scanPurpose: captureForm.title || current.scanPurpose,
      }))
      setSelectedScanId(nextScan.scanProcessingId)
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const captureCameraFrameMutation = useMutation({
    mutationFn: async () => {
      if (cameraStatus !== 'ready') {
        throw new Error('Start the camera before capturing a page.')
      }
      const video = videoRef.current
      if (!video) {
        throw new Error('Camera preview is not ready.')
      }
      return captureVideoFrame(video, captureForm.title || captureForm.sourceObjectDisplayName || 'camera-capture')
    },
    onSuccess: async (file) => {
      setCapturePages((current) => [
        ...current,
        {
          pageId: `page-${crypto.randomUUID()}`,
          file,
        },
      ])
      setPreviewScale(1)
      setPreviewRotation(0)
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const scanMutation = useMutation({
    mutationFn: (recordId?: string) =>
      createScan(accessToken, {
        recordId: recordId ?? createdRecordId ?? scan.recordId,
        originalFileName: captureForm.currentFileName || scan.originalFileName || 'scan.pdf',
        scanPurpose: captureForm.title || scan.scanPurpose || 'Scan capture',
      }),
    onSuccess: async (nextScan) => {
      setSelectedScanId(nextScan.scanProcessingId)
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
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
  const previewSteps = useMemo(() => {
    return [
      {
        label: 'Save draft',
        detail: draftSavedAt ? `Saved ${formatDate(draftSavedAt)}` : 'Waiting to be saved',
        complete: Boolean(draftSavedAt),
      },
      {
        label: 'Capture request',
        detail: draftCaptureRequestId || captureRequestsQuery.data?.[0]?.captureRequestId ? 'Draft request created' : 'Waiting for a request',
        complete: Boolean(draftCaptureRequestId || captureRequestsQuery.data?.[0]),
      },
      {
        label: 'Create record',
        detail: createdRecordId ? `Record ${createdRecordId}` : 'Waiting for record creation',
        complete: Boolean(createdRecordId),
      },
      {
        label: 'Queue OCR',
        detail: selectedScan?.scanProcessingId ? `Scan ${selectedScan.scanProcessingId.slice(-8).toUpperCase()}` : 'Waiting for a queued scan',
        complete: Boolean(selectedScan?.scanProcessingId),
      },
      {
        label: 'Extract text (OCR)',
        detail: ocrQuery.data ? `${ocrQuery.data.engine} · ${ocrQuery.data.confidenceScore.toFixed(2)}` : 'Waiting for OCR',
        complete: Boolean(ocrQuery.data),
      },
      {
        label: 'Extract metadata',
        detail: extractionQuery.data ? `${formatDisplayLabel(extractionQuery.data.status)} · ${extractionQuery.data.extractedFields.length} fields` : 'Waiting for extraction',
        complete: Boolean(extractionQuery.data),
      },
    ]
  }, [createdRecordId, draftSavedAt, extractionQuery.data, ocrQuery.data, selectedScan])
  const capturePrimaryFile = capturePages[0]?.file ?? null
  const capturePrimaryFileName = capturePages.length > 1
    ? `${slugifyFileName(captureForm.title || capturePages[0]?.file.name || 'capture')}-packet.pdf`
    : capturePrimaryFile?.name || captureForm.currentFileName || ''
  const capturePrimaryMimeType = capturePages.length > 1
    ? 'application/pdf'
    : capturePrimaryFile?.type || captureForm.currentMimeType || 'application/octet-stream'
  const previewScore = selectedScan ? `${Math.round(selectedScan.confidenceScore * 100)}%` : `${capturePages.length} page(s)`
  const previewTitle = capturePrimaryFileName || selectedScan?.originalFileName || 'No file selected'
  const previewSubtitle = captureForm.title || captureForm.description || 'Choose one or more pages, then file the record.'
  const previewBody = [
    capturePages.length ? `Pages: ${capturePages.length}` : 'Pages: add one or more pages',
    captureForm.documentClass ? `Class: ${captureForm.documentClass}` : 'Class: select a document class',
    captureForm.documentType ? `Type: ${captureForm.documentType}` : 'Type: select a document type',
    captureForm.documentSubtype ? `Subtype: ${captureForm.documentSubtype}` : 'Subtype: select a document subtype',
    captureForm.sourceProduct ? `Source: ${captureForm.sourceProduct} / ${captureForm.sourceObjectDisplayName || 'pending reference'}` : 'Source: select a product and reference',
    capturePrimaryMimeType ? `File type: ${capturePrimaryMimeType}` : 'File type: attach a source file',
  ].join('\n')
  const previewComplete = previewSteps.every((step) => step.complete)
  const captureCanCreate =
    Boolean(captureForm.title.trim()) &&
    Boolean(captureForm.documentClass.trim()) &&
    Boolean(captureForm.documentType.trim()) &&
    Boolean(captureForm.documentSubtype.trim()) &&
    Boolean(captureForm.sourceProduct.trim()) &&
    Boolean(captureForm.sourceObjectType.trim()) &&
    Boolean(captureForm.sourceObjectId.trim()) &&
    Boolean(captureForm.sourceObjectDisplayName.trim()) &&
    capturePages.length > 0
  const captureCanDraft =
    Boolean(captureForm.title.trim()) &&
    Boolean(captureForm.sourceProduct.trim()) &&
    Boolean(captureForm.sourceObjectType.trim()) &&
    Boolean(captureForm.sourceObjectId.trim()) &&
    Boolean(captureForm.sourceObjectDisplayName.trim()) &&
    capturePages.length > 0
  const captureCanScan = Boolean(createdRecordId || scan.recordId)
  const resetCapture = () => {
    setCaptureSource('camera')
    setPreviewScale(1)
    setPreviewRotation(0)
    setDraftSavedAt(null)
    setDraftUploadSessionId('')
    setDraftCaptureRequestId('')
    setCreatedRecordId('')
    setCapturePages([])
    setCaptureForm({
      title: '',
      description: '',
      recordType: 'document',
      documentClass: '',
      documentType: '',
      documentSubtype: '',
      classification: 'internal',
      sourceProduct: '',
      sourceObjectType: '',
      sourceObjectId: '',
      sourceObjectDisplayName: '',
      ownerPersonId: actorPersonId,
      currentFileName: '',
      currentMimeType: 'application/pdf',
      fileContentBase64: '',
    })
    setSelectedScanId('')
    setScan({
      recordId: '',
      originalFileName: '',
      scanPurpose: '',
      edgeCoordinates: '',
      correctedByPersonId: actorPersonId,
    })
  }
  const captureInputAccept = captureSource === 'camera' ? 'image/*' : 'image/*,.pdf'
  const addCapturePages = async (files: File[]) => {
    if (files.length === 0) return
    setCapturePages((current) => [
      ...current,
      ...files.map((file) => ({
        pageId: `page-${crypto.randomUUID()}`,
        file,
      })),
    ])
    setCaptureSource('upload')
  }
  const removeCapturePage = (pageId: string) => {
    setCapturePages((current) => current.filter((page) => page.pageId !== pageId))
  }
  const moveCapturePage = (pageId: string, direction: -1 | 1) => {
    setCapturePages((current) => {
      const index = current.findIndex((page) => page.pageId === pageId)
      const nextIndex = index + direction
      if (index < 0 || nextIndex < 0 || nextIndex >= current.length) {
        return current
      }

      const next = [...current]
      const [entry] = next.splice(index, 1)
      next.splice(nextIndex, 0, entry)
      return next
    })
  }

  return (
    <div className="recordarr-page recordarr-capture-page">
      <div className="recordarr-capture-shell">
        <div className="recordarr-capture-breadcrumbs">
          <span>Capture</span>
          <span>›</span>
          <strong>Scan Capture</strong>
        </div>
        <div className="recordarr-capture-header">
          <div className="space-y-2">
            <h1 className="recordarr-capture-title">Scan Capture</h1>
            <p className="recordarr-capture-subtitle">Capture a file, create the record, and queue OCR from one screen.</p>
          </div>
          <div className="recordarr-capture-kpis">
            <span className="recordarr-capture-pill"><Upload className="h-4 w-4" /> {uploadSessionsQuery.data?.length ?? 0} drafts</span>
            <span className="recordarr-capture-pill"><ScanSearch className="h-4 w-4" /> {scansQuery.data?.length ?? 0} scans</span>
            <span className="recordarr-capture-pill"><BadgeCheck className="h-4 w-4" /> {captureRequestsQuery.data?.length ?? 0} requests</span>
          </div>
        </div>
        <div className="recordarr-capture-source-group">
          <button
            type="button"
            className={captureSource === 'camera' ? 'recordarr-capture-source-active' : 'recordarr-capture-source'}
            onClick={() => setCaptureSource('camera')}
          >
            <Camera className="h-4 w-4" />
            Camera
          </button>
          <button
            type="button"
            className={captureSource === 'upload' ? 'recordarr-capture-source-active' : 'recordarr-capture-source'}
            onClick={() => setCaptureSource('upload')}
          >
            <Upload className="h-4 w-4" />
            Upload File
          </button>
          <button
            type="button"
            className={captureSource === 'scanner' ? 'recordarr-capture-source-active' : 'recordarr-capture-source'}
            onClick={() => setCaptureSource('scanner')}
          >
            <ScanSearch className="h-4 w-4" />
            Scanner
          </button>
        </div>
        <div className="recordarr-capture-workspace">
          <section className="recordarr-capture-stage">
            <div className="recordarr-capture-stage-layout">
              <div className="recordarr-capture-rail">
                <button type="button" className="recordarr-capture-rail-button" onClick={() => setPreviewRotation((current) => current - 90)}>
                  <RotateCcw className="h-4 w-4" />
                  <span>Rotate Left</span>
                </button>
                <button type="button" className="recordarr-capture-rail-button" onClick={() => setPreviewRotation((current) => current + 90)}>
                  <RotateCw className="h-4 w-4" />
                  <span>Rotate Right</span>
                </button>
                <button type="button" className="recordarr-capture-rail-button" onClick={() => setPreviewScale((current) => Math.min(1.35, Number((current + 0.1).toFixed(2))))}>
                  <ZoomIn className="h-4 w-4" />
                  <span>Zoom In</span>
                </button>
                <button type="button" className="recordarr-capture-rail-button" onClick={() => setPreviewScale((current) => Math.max(0.75, Number((current - 0.1).toFixed(2))))}>
                  <ZoomOut className="h-4 w-4" />
                  <span>Zoom Out</span>
                </button>
                <button type="button" className="recordarr-capture-rail-button" onClick={() => {
                  setPreviewScale(1)
                  setPreviewRotation(0)
                }}>
                  <Crop className="h-4 w-4" />
                  <span>Fit</span>
                </button>
              </div>
              <div className="recordarr-capture-canvas-wrap">
                <div className="recordarr-capture-canvas">
                  <div className="recordarr-capture-canvas-topline">
                    <span className="recordarr-capture-status-dot" />
                    <span>
                      {captureSource === 'camera'
                        ? cameraStatus === 'requesting'
                            ? 'Requesting camera permission'
                            : cameraStatus === 'error'
                              ? 'Camera unavailable'
                              : capturePages.length > 0
                                ? `${capturePages.length} page(s) captured`
                                : 'Camera live'
                        : capturePages.length > 0
                          ? `${capturePages.length} page(s) loaded`
                          : 'Awaiting pages'}
                    </span>
                    <span className="recordarr-capture-canvas-muted">{captureSource === 'camera' ? 'Camera' : captureSource === 'upload' ? 'Upload file' : 'Scanner'}</span>
                  </div>
                  <div className="recordarr-capture-paper-shell">
                    {captureSource === 'camera' ? (
                      <div className="recordarr-capture-camera-shell">
                        <div className="recordarr-capture-camera-view">
                          <video ref={videoRef} className="recordarr-capture-camera-video" autoPlay playsInline muted />
                          <div className="recordarr-capture-camera-overlay">
                            <div className="recordarr-capture-camera-status">
                              <span className="recordarr-capture-camera-status-badge">
                                <Camera className="h-4 w-4" />
                                {cameraStatus === 'requesting'
                                  ? 'Requesting camera permission'
                                  : cameraStatus === 'ready'
                                    ? 'Camera live'
                                    : cameraStatus === 'error'
                                      ? 'Camera blocked'
                                      : 'Camera idle'}
                              </span>
                              <p className="recordarr-capture-camera-status-title">
                                {capturePages.length > 0 ? 'Capture the next page' : 'Enable camera to capture the first page'}
                              </p>
                              <p className="recordarr-capture-camera-status-copy">
                                {cameraStatus === 'ready'
                                  ? 'Hold the document in frame and capture a clean page into this packet.'
                                  : cameraStatus === 'requesting'
                                    ? 'Waiting for the browser permission prompt...'
                                    : cameraError ?? 'The page will ask for camera access so you can capture directly into the record packet.'}
                              </p>
                              {cameraError ? <p className="recordarr-capture-camera-status-error">{cameraError}</p> : null}
                            </div>
                            <div className="recordarr-capture-camera-actions">
                              <button
                                type="button"
                                className="recordarr-capture-stage-button recordarr-capture-camera-action recordarr-capture-camera-action-primary"
                                onClick={() => captureCameraFrameMutation.mutate()}
                                disabled={captureCameraFrameMutation.isPending || cameraStatus !== 'ready'}
                              >
                                <Camera className="h-4 w-4" />
                                {captureCameraFrameMutation.isPending ? 'Capturing...' : 'Capture frame'}
                              </button>
                              <button
                                type="button"
                                className="recordarr-capture-stage-button recordarr-capture-camera-action"
                                onClick={() => void startCamera()}
                                disabled={cameraStatus === 'requesting'}
                              >
                                <RotateCw className="h-4 w-4" />
                                Retry camera
                              </button>
                              <button
                                type="button"
                                className="recordarr-capture-stage-button recordarr-capture-camera-action"
                                onClick={() => setCameraFacingMode((current) => (current === 'environment' ? 'user' : 'environment'))}
                              >
                                <Sparkles className="h-4 w-4" />
                                {cameraFacingMode === 'environment' ? 'Front camera' : 'Rear camera'}
                              </button>
                              <button
                                type="button"
                                className="recordarr-capture-stage-button recordarr-capture-camera-action"
                                onClick={() => setCaptureSource('upload')}
                              >
                                <Upload className="h-4 w-4" />
                                Use upload
                              </button>
                            </div>
                          </div>
                        </div>
                        <div className="recordarr-capture-tags">
                          <span className="recordarr-capture-tag">
                            <Camera className="h-3.5 w-3.5" />
                            {capturePages.length} page(s) captured
                          </span>
                          <span className="recordarr-capture-tag">
                            <ScanSearch className="h-3.5 w-3.5" />
                            {selectedScan?.status ?? 'OCR pending'}
                          </span>
                        </div>
                      </div>
                    ) : (
                      <div className="recordarr-capture-paper" style={{ transform: `rotate(${previewRotation}deg) scale(${previewScale})` }}>
                        <div className="recordarr-capture-paper-corners">
                          <span />
                          <span />
                          <span />
                          <span />
                        </div>
                        <div className="recordarr-capture-paper-header">
                          <div>
                            <p className="recordarr-capture-paper-eyebrow">{captureForm.title || 'Capture intake'}</p>
                            <p className="recordarr-capture-paper-subtle">{captureForm.description || 'Add a description to guide OCR and filing.'}</p>
                            <p className="recordarr-capture-paper-subtle">{captureForm.sourceObjectDisplayName || 'Pick a source reference'}</p>
                          </div>
                          <div className="text-right">
                            <p className="recordarr-capture-paper-eyebrow">{captureForm.recordType.replaceAll('_', ' ')}</p>
                            <p className="recordarr-capture-paper-subtle">{captureForm.classification}</p>
                          </div>
                        </div>
                        <div className="recordarr-capture-paper-grid">
                          <div className="space-y-2">
                            <p className="recordarr-capture-paper-caption">Source file</p>
                            <p className="recordarr-capture-paper-value">{previewTitle}</p>
                            <p className="recordarr-capture-paper-subtle">{previewSubtitle}</p>
                          </div>
                          <div className="space-y-2 text-right">
                            <p className="recordarr-capture-paper-caption">Readiness</p>
                            <p className="recordarr-capture-paper-value">{previewScore}</p>
                            <p className="recordarr-capture-paper-subtle">{captureCanCreate ? 'Ready to create' : 'Finish the required fields'}</p>
                          </div>
                        </div>
                        <div className="recordarr-capture-paper-body">
                          <p className="whitespace-pre-line">{previewBody}</p>
                        </div>
                        <div className="recordarr-capture-paper-footer">
                          <span>{captureSource === 'upload' ? 'Uploaded file' : 'Scanner intake'}</span>
                          <span>{selectedScan?.status ?? 'draft'}</span>
                        </div>
                      </div>
                    )}
                  </div>
                  <div className="recordarr-capture-stage-badges">
                    <span className="recordarr-capture-chip recordarr-capture-chip-success">{captureCanCreate ? 'Ready to file' : 'Draft in progress'}</span>
                    <span className="recordarr-capture-chip recordarr-capture-chip-success">{selectedScan ? 'OCR queued' : 'OCR pending'}</span>
                  </div>
                </div>
              </div>
            </div>
            <div className="recordarr-capture-stage-actions">
              <button
                type="button"
                className="recordarr-capture-stage-button"
                onClick={() => {
                  if (captureSource === 'camera' && capturePages.length > 0) {
                    setCapturePages((current) => current.slice(0, -1))
                    return
                  }
                  setScan({
                    recordId: '',
                    originalFileName: '',
                    scanPurpose: '',
                    edgeCoordinates: '',
                    correctedByPersonId: actorPersonId,
                  })
                  setSelectedScanId('')
                  setPreviewScale(1)
                  setPreviewRotation(0)
                }}
              >
                <Camera className="h-4 w-4" />
                {captureSource === 'camera' && capturePages.length > 0 ? 'Retake last frame' : 'Retake'}
              </button>
              <button type="button" className="recordarr-capture-stage-button" onClick={() => setScan((current) => ({ ...current, edgeCoordinates: '' }))}>
                <Crop className="h-4 w-4" />
                Re-crop
              </button>
              <button type="button" className="recordarr-capture-stage-button" onClick={() => setPreviewScale((current) => Math.min(1.5, Number((current + 0.05).toFixed(2))))}>
                <Sparkles className="h-4 w-4" />
                Enhance
              </button>
              <button
                type="button"
                className="recordarr-capture-stage-button recordarr-capture-stage-primary"
                onClick={() => scanMutation.mutate(createdRecordId || scan.recordId || undefined)}
                disabled={scanMutation.isPending || !captureCanScan}
              >
                <ScanSearch className="h-4 w-4" />
                {scanMutation.isPending ? 'Running OCR...' : 'Queue OCR'}
              </button>
            </div>
            <div className="recordarr-capture-tip">
              <Sparkles className="h-4 w-4" />
              <p>Tip: save the draft session first, then file the record. OCR will queue as soon as the record exists.</p>
            </div>
          </section>
          <aside className="space-y-4">
            <section className="recordarr-capture-panel">
              <div className="recordarr-capture-panel-header">
                <div>
                  <h2>Processing</h2>
                  <p>{previewComplete ? 'All capture steps are complete' : 'Capture is still in progress'}</p>
                </div>
                <span className="recordarr-capture-inline-status">{previewComplete ? 'Complete' : 'In progress'}</span>
              </div>
              <div className="mt-4 space-y-3">
                {previewSteps.map((step) => (
                  <div key={step.label} className="recordarr-capture-step">
                    <div className="recordarr-capture-step-left">
                      {step.complete ? <CheckCircle2 className="h-4 w-4 text-emerald-500" /> : <CircleDashed className="h-4 w-4 text-slate-300" />}
                      <span>{step.label}</span>
                    </div>
                    <span className={step.complete ? 'recordarr-capture-step-done' : 'recordarr-capture-step-waiting'}>{step.complete ? 'Completed' : 'Pending'}</span>
                    <p className="recordarr-capture-step-detail">{step.detail}</p>
                  </div>
                ))}
              </div>
            </section>
            <section className="recordarr-capture-panel">
              <div className="recordarr-capture-panel-header">
                <div>
                  <h2>Capture details</h2>
                  <p>Fill the fields once and file the record with OCR-ready metadata.</p>
                </div>
                <div className="text-right">
                  <span className="recordarr-capture-inline-status">{captureCanCreate ? 'Ready' : 'Draft'}</span>
                  <p className="mt-1 text-xs text-[var(--color-text-secondary)]">{capturePages.length > 0 ? `${capturePages.length} page(s) attached` : 'No pages attached'}</p>
                </div>
              </div>
              <div className="mt-4 space-y-3">
                <label className="recordarr-capture-field">
                  <span>Document Type</span>
                  <select
                    className="recordarr-capture-input"
                    value={captureForm.recordType}
                    onChange={(e) => setCaptureForm({ ...captureForm, recordType: e.target.value })}
                  >
                    <ReadableOption value="document" />
                    <ReadableOption value="photo" />
                    <ReadableOption value="signature" />
                    <ReadableOption value="video" />
                    <ReadableOption value="audio" />
                    <ReadableOption value="form_submission" />
                    <ReadableOption value="generated_pdf" />
                    <ReadableOption value="certificate" />
                    <ReadableOption value="inspection_record" />
                    <ReadableOption value="training_record" />
                    <ReadableOption value="maintenance_record" />
                    <ReadableOption value="receiving_record" />
                    <ReadableOption value="delivery_record" />
                    <ReadableOption value="quality_record" />
                    <ReadableOption value="audit_evidence" />
                    <ReadableOption value="evidence_package" />
                    <ReadableOption value="report_output" />
                    <ReadableOption value="other" />
                  </select>
                </label>
                <label className="recordarr-capture-field">
                  <span>Title</span>
                  <input className="recordarr-capture-input" value={captureForm.title} onChange={(e) => setCaptureForm({ ...captureForm, title: e.target.value })} placeholder="PO-2024-0156 - Global Supplies, Inc." />
                </label>
                <label className="recordarr-capture-field">
                  <span>Description</span>
                  <textarea className="recordarr-capture-textarea" value={captureForm.description} onChange={(e) => setCaptureForm({ ...captureForm, description: e.target.value })} placeholder="Explain what the file is and where it came from." />
                </label>
                <label className="recordarr-capture-field">
                  <span>Classification</span>
                  <select className="recordarr-capture-input" value={captureForm.classification} onChange={(e) => setCaptureForm({ ...captureForm, classification: e.target.value })}>
                    <ReadableOption value="public" />
                    <ReadableOption value="internal" />
                    <ReadableOption value="confidential" />
                    <ReadableOption value="restricted" />
                    <ReadableOption value="legal_hold" />
                  </select>
                </label>
                <label className="recordarr-capture-field">
                  <span>Document class</span>
                  <StaticSearchPicker
                    value={captureForm.documentClass}
                    onChange={(documentClass) => setCaptureForm({ ...captureForm, documentClass })}
                    options={documentClassOptions}
                    placeholder={documentClassOptionsLoading ? 'Loading document classes…' : 'Search document classes'}
                    disabled={documentClassOptionsLoading}
                  />
                </label>
                <label className="recordarr-capture-field">
                  <span>Document type</span>
                  <StaticSearchPicker
                    value={captureForm.documentType}
                    onChange={(documentType) => setCaptureForm({ ...captureForm, documentType })}
                    options={documentTypeOptions}
                    placeholder={documentTypeOptionsLoading ? 'Loading document types…' : 'Search document types'}
                    disabled={documentTypeOptionsLoading}
                  />
                </label>
                <label className="recordarr-capture-field">
                  <span>Document subtype</span>
                  <StaticSearchPicker
                    value={captureForm.documentSubtype}
                    onChange={(documentSubtype) => setCaptureForm({ ...captureForm, documentSubtype })}
                    options={documentSubtypeOptions}
                    placeholder={documentSubtypeOptionsLoading ? 'Loading document subtypes…' : 'Search document subtypes'}
                    disabled={documentSubtypeOptionsLoading}
                  />
                </label>
                <label className="recordarr-capture-field">
                  <span>Source product</span>
                  <SourceProductPicker
                    value={captureForm.sourceProduct}
                    onChange={(sourceProduct) => setCaptureForm({ ...captureForm, sourceProduct, sourceObjectType: '', sourceObjectId: '', sourceObjectDisplayName: '' })}
                  />
                </label>
                <label className="recordarr-capture-field">
                  <span>Source object</span>
                  <SourceObjectRefPicker
                    value={buildSourceObjectRef(captureForm.sourceProduct, captureForm.sourceObjectType, captureForm.sourceObjectId)}
                    sourceProduct={captureForm.sourceProduct}
                    onChange={(_sourceObjectRef, selected) => {
                      if (!selected) return
                      setCaptureForm({
                        ...captureForm,
                        sourceProduct: selected.sourceProduct,
                        sourceObjectType: selected.sourceObjectType,
                        sourceObjectId: selected.sourceObjectId,
                        sourceObjectDisplayName: selected.sourceObjectDisplayName,
                      })
                    }}
                  />
                </label>
                <label className="recordarr-capture-field">
                  <span>Owner person</span>
                  <PersonReferencePicker value={captureForm.ownerPersonId} onChange={(ownerPersonId) => setCaptureForm({ ...captureForm, ownerPersonId })} />
                </label>
                <label className="recordarr-capture-field">
                  <span>Pages</span>
                  <div className="space-y-3 rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-3">
                    <input
                      className="recordarr-input file:mr-4 file:rounded-lg file:border-0 file:bg-[var(--color-accent)] file:px-3 file:py-2 file:font-semibold file:text-white hover:file:bg-[var(--color-accent-hover)]"
                      type="file"
                      accept={captureInputAccept}
                      capture={captureSource === 'camera' ? 'environment' : undefined}
                      multiple
                      onChange={async (e) => {
                        const files = Array.from(e.target.files ?? [])
                        await addCapturePages(files)
                        e.currentTarget.value = ''
                      }}
                    />
                    <p className="text-xs text-[var(--color-text-secondary)]">
                      Add one page or many. Images and PDFs are combined into a single packet when you file the record.
                    </p>
                    <div className="space-y-2">
                      {capturePages.length > 0 ? (
                        capturePages.map((page, index) => (
                          <div key={page.pageId} className="flex items-center gap-2 rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] px-3 py-2 text-sm">
                            <span className="inline-flex h-7 w-7 shrink-0 items-center justify-center rounded-full bg-[var(--color-accent-soft)] text-xs font-semibold text-[var(--color-accent)]">
                              {index + 1}
                            </span>
                            <div className="min-w-0 flex-1">
                              <p className="truncate font-semibold text-[var(--color-text-primary)]">{page.file.name}</p>
                              <p className="text-xs text-[var(--color-text-secondary)]">{page.file.type || 'application/octet-stream'}</p>
                            </div>
                            <button type="button" className="recordarr-capture-stage-button h-8 min-h-8 px-2 py-0 text-xs" onClick={() => moveCapturePage(page.pageId, -1)} disabled={index === 0}>
                              ↑
                            </button>
                            <button type="button" className="recordarr-capture-stage-button h-8 min-h-8 px-2 py-0 text-xs" onClick={() => moveCapturePage(page.pageId, 1)} disabled={index === capturePages.length - 1}>
                              ↓
                            </button>
                            <button type="button" className="recordarr-capture-stage-button h-8 min-h-8 px-2 py-0 text-xs" onClick={() => removeCapturePage(page.pageId)}>
                              Remove
                            </button>
                          </div>
                        ))
                      ) : (
                        <div className="rounded-xl border border-dashed border-[var(--color-border-subtle)] px-3 py-4 text-sm text-[var(--color-text-secondary)]">
                          No pages yet. Add the first page to start the packet.
                        </div>
                      )}
                    </div>
                    <div className="flex items-center justify-between gap-3 text-xs text-[var(--color-text-secondary)]">
                      <span>{capturePages.length} page(s)</span>
                      <button type="button" className="text-[var(--color-accent)] hover:underline" onClick={() => setCapturePages([])} disabled={capturePages.length === 0}>
                        Clear pages
                      </button>
                    </div>
                  </div>
                </label>
                <label className="recordarr-capture-field">
                  <span>Current file name</span>
                  <input
                    className="recordarr-capture-input"
                    value={capturePrimaryFileName}
                    readOnly
                    aria-readonly="true"
                  />
                </label>
                <label className="recordarr-capture-field">
                  <span>Mime type</span>
                  <input
                    className="recordarr-capture-input"
                    value={capturePrimaryMimeType}
                    readOnly
                    aria-readonly="true"
                  />
                </label>
              </div>
            </section>
            <section className="recordarr-capture-panel">
              <div className="recordarr-capture-panel-header">
                <div>
                  <h2>Automation</h2>
                  <p>Backend-powered session, request, record, and OCR steps.</p>
                </div>
                <span className="recordarr-capture-inline-status">Live</span>
              </div>
              <div className="mt-4 space-y-3 text-sm text-[var(--color-text-secondary)]">
                <p>{draftSavedAt ? `Draft session saved ${formatDate(draftSavedAt)}` : 'No backend draft session saved yet.'}</p>
                <p>{draftUploadSessionId ? `Upload session ${draftUploadSessionId}` : 'No upload session linked yet.'}</p>
                <p>{draftCaptureRequestId ? `Capture request ${draftCaptureRequestId}` : 'No capture request linked yet.'}</p>
                <p>{createdRecordId ? `Record ${createdRecordId}${selectedScan?.scanProcessingId ? ` · scan ${selectedScan.scanProcessingId}` : ''}` : 'No record has been created yet.'}</p>
                <p>{capturePages.length > 0 ? `${capturePages.length} page(s) ready to file.` : 'No pages added yet.'}</p>
              </div>
            </section>
          </aside>
        </div>
        <div className="recordarr-capture-footer">
          <button type="button" className="recordarr-capture-secondary-footer-button" onClick={resetCapture}>
            Reset form
          </button>
          <div className="flex items-center gap-3">
            {draftSavedAt ? <span className="text-sm text-[var(--color-accent)]">Draft saved {formatDate(draftSavedAt)}</span> : null}
            <button type="button" className="recordarr-capture-secondary-footer-button" onClick={() => saveDraftMutation.mutate()} disabled={saveDraftMutation.isPending || !captureCanDraft}>
              Save as Draft
            </button>
            <button type="button" className="recordarr-capture-primary-footer-button" onClick={() => createRecordMutation.mutate()} disabled={createRecordMutation.isPending || !captureCanCreate}>
              <FileText className="h-4 w-4" />
              File Record
            </button>
          </div>
        </div>
      </div>
    </div>
  )
}

*/
function CapturePage({ accessToken, actorPersonId, actorDisplayName }: WorkspacePageProps) {
  const queryClient = useQueryClient()
  const { options: documentClassOptions, isLoading: documentClassOptionsLoading } = useVocabularyTermOptions(accessToken, 'document_class')
  const { options: documentTypeOptions, isLoading: documentTypeOptionsLoading } = useVocabularyTermOptions(accessToken, 'document_type')
  const { options: documentSubtypeOptions, isLoading: documentSubtypeOptionsLoading } = useVocabularyTermOptions(accessToken, 'document_subtype')
  const uploadInputRef = useRef<HTMLInputElement | null>(null)
  const cameraInputRef = useRef<HTMLInputElement | null>(null)
  const previewSurfaceRef = useRef<HTMLDivElement | null>(null)
  const activeCornerRef = useRef<number | null>(null)
  const loadSequenceRef = useRef(0)
  const [selectedFile, setSelectedFile] = useState<File | null>(null)
  const [captureSource, setCaptureSource] = useState<CaptureFileSource | null>(null)
  const [previewUrl, setPreviewUrl] = useState('')
  const [imageSize, setImageSize] = useState<{ width: number; height: number } | null>(null)
  const [geometryPoints, setGeometryPoints] = useState<GeometryPoint[]>([])
  const [geometryConfirmed, setGeometryConfirmed] = useState(false)
  const [processingState, setProcessingState] = useState<'idle' | 'processing' | 'ready' | 'error'>('idle')
  const [processingMessage, setProcessingMessage] = useState('Upload or use the camera to begin triage.')
  const [captureForm, setCaptureForm] = useState<CaptureFormState>(() => createCaptureForm(actorPersonId))
  const [createdRecord, setCreatedRecord] = useState<RecordArrRecord | null>(null)
  const [createdScan, setCreatedScan] = useState<RecordArrScanProcessing | null>(null)
  const [syncWarning, setSyncWarning] = useState<string | null>(null)
  const [syncWarningStage, setSyncWarningStage] = useState<'scan' | 'correction' | null>(null)

  useEffect(() => {
    setCaptureForm((current) => ({
      ...current,
      ownerPersonId: actorPersonId,
    }))
  }, [actorPersonId])

  useEffect(() => {
    if (!selectedFile) {
      setPreviewUrl('')
      return undefined
    }

    const nextPreviewUrl = URL.createObjectURL(selectedFile)
    setPreviewUrl(nextPreviewUrl)
    return () => URL.revokeObjectURL(nextPreviewUrl)
  }, [selectedFile])

  const resetCapture = () => {
    loadSequenceRef.current += 1
    activeCornerRef.current = null
    setSelectedFile(null)
    setCaptureSource(null)
    setPreviewUrl('')
    setImageSize(null)
    setGeometryPoints([])
    setGeometryConfirmed(false)
    setProcessingState('idle')
    setProcessingMessage('Upload or use the camera to begin triage.')
    setCaptureForm(createCaptureForm(actorPersonId))
    setCreatedRecord(null)
    setCreatedScan(null)
    setSyncWarning(null)
    setSyncWarningStage(null)
    if (uploadInputRef.current) {
      uploadInputRef.current.value = ''
    }
    if (cameraInputRef.current) {
      cameraInputRef.current.value = ''
    }
  }

  const beginFileCapture = async (file: File, source: CaptureFileSource) => {
    const nextSequence = loadSequenceRef.current + 1
    loadSequenceRef.current = nextSequence
    activeCornerRef.current = null
    setSelectedFile(file)
    setCaptureSource(source)
    setImageSize(null)
    setGeometryPoints([])
    setGeometryConfirmed(false)
    setProcessingState('processing')
    setProcessingMessage(source === 'camera' ? 'Reading the camera capture…' : 'Reading the uploaded file…')
    setCaptureForm({
      ...createCaptureForm(actorPersonId),
      title: stripFileName(file.name),
      description: `${source === 'camera' ? 'Camera capture' : 'Uploaded file'} · ${formatBytes(file.size)}`,
      sourceProduct: 'recordarr',
      sourceObjectType: 'capture',
      sourceObjectId: `capture-${crypto.randomUUID()}`,
      sourceObjectDisplayName: stripFileName(file.name),
      ownerPersonId: actorPersonId,
    })
    setCreatedRecord(null)
    setCreatedScan(null)
    setSyncWarning(null)
    setSyncWarningStage(null)

    if (!isImageFile(file)) {
      setProcessingState('ready')
      setProcessingMessage('File loaded. Geometry correction is skipped for non-image uploads.')
      setGeometryConfirmed(true)
      return
    }

    try {
      const dimensions = await loadImageDimensions(file)
      if (loadSequenceRef.current !== nextSequence) {
        return
      }

      setImageSize(dimensions)
      setGeometryPoints(defaultGeometryPoints(dimensions.width, dimensions.height))
      setProcessingState('ready')
      setProcessingMessage('Auto-detected document edges. Drag the corners to refine the crop before filing.')
    } catch (error) {
      if (loadSequenceRef.current !== nextSequence) {
        return
      }

      setImageSize(null)
      setGeometryPoints([])
      setGeometryConfirmed(false)
      setProcessingState('error')
      setProcessingMessage(getErrorMessage(error))
    }
  }

  const openNativeFileChooser = (element: HTMLInputElement | null) => {
    if (!element) {
      return
    }

    element.value = ''
    element.click()
  }

  const handleFileInputChange = (source: CaptureFileSource) => async (event: ChangeEvent<HTMLInputElement>) => {
    const nextFile = event.currentTarget.files?.[0] ?? null
    event.currentTarget.value = ''
    if (!nextFile) {
      return
    }

    await beginFileCapture(nextFile, source)
  }

  const updateGeometryPointFromClient = (index: number, clientX: number, clientY: number) => {
    if (!imageSize || !previewSurfaceRef.current) {
      return
    }

    const bounds = previewSurfaceRef.current.getBoundingClientRect()
    if (!bounds.width || !bounds.height) {
      return
    }

    const nextPoint = clampGeometryPoint(
      {
        x: ((clientX - bounds.left) / bounds.width) * imageSize.width,
        y: ((clientY - bounds.top) / bounds.height) * imageSize.height,
      },
      imageSize.width,
      imageSize.height,
    )

    setGeometryPoints((current) => current.map((point, pointIndex) => (pointIndex === index ? nextPoint : point)))
  }

  const nudgeGeometryPoint = (index: number, deltaX: number, deltaY: number) => {
    if (!imageSize) {
      return
    }

    setGeometryPoints((current) =>
      current.map((point, pointIndex) =>
        pointIndex === index
          ? clampGeometryPoint(
              { x: point.x + deltaX, y: point.y + deltaY },
              imageSize.width,
              imageSize.height,
            )
          : point,
      ),
    )
  }

  const finishGeometryEdit = () => {
    activeCornerRef.current = null
  }

  const applyCornerPointerDown = (index: number) => (event: ReactPointerEvent<HTMLButtonElement>) => {
    if (!imageSize) {
      return
    }

    activeCornerRef.current = index
    event.currentTarget.setPointerCapture(event.pointerId)
    updateGeometryPointFromClient(index, event.clientX, event.clientY)
  }

  const applyCornerPointerMove = (index: number) => (event: ReactPointerEvent<HTMLButtonElement>) => {
    if (activeCornerRef.current !== index) {
      return
    }

    updateGeometryPointFromClient(index, event.clientX, event.clientY)
  }

  const applyCornerPointerUp = (index: number) => () => {
    if (activeCornerRef.current === index) {
      activeCornerRef.current = null
    }
  }

  const applyCornerKeyDown = (index: number) => (event: ReactKeyboardEvent<HTMLButtonElement>) => {
    const step = event.shiftKey ? 12 : 3
    switch (event.key) {
      case 'ArrowUp':
        event.preventDefault()
        nudgeGeometryPoint(index, 0, -step)
        break
      case 'ArrowDown':
        event.preventDefault()
        nudgeGeometryPoint(index, 0, step)
        break
      case 'ArrowLeft':
        event.preventDefault()
        nudgeGeometryPoint(index, -step, 0)
        break
      case 'ArrowRight':
        event.preventDefault()
        nudgeGeometryPoint(index, step, 0)
        break
      default:
        break
    }
  }

  const resetGeometry = () => {
    if (!imageSize) {
      return
    }

    setGeometryPoints(defaultGeometryPoints(imageSize.width, imageSize.height))
    setGeometryConfirmed(false)
    setProcessingMessage('Geometry reset to the auto-detected frame.')
  }

  const confirmGeometry = () => {
    if (!selectedFile) {
      return
    }

    setGeometryConfirmed(true)
    setProcessingMessage('Geometry confirmed. Finish the document details, then submit.')
  }

  const detailsReady = Boolean(selectedFile && (geometryConfirmed || !imageSize) && processingState !== 'error')
  const sourceRef = buildSourceObjectRef(captureForm.sourceProduct, captureForm.sourceObjectType, captureForm.sourceObjectId)
  const uploadSummary = selectedFile
    ? `${selectedFile.name} · ${formatBytes(selectedFile.size)}`
    : 'No file selected'
  const captureSourceLabel = captureSource === 'camera' ? 'Camera' : captureSource === 'upload' ? 'Upload' : 'Awaiting file'
  const geometrySummary = processingState === 'error'
    ? 'Preview error'
    : imageSize
      ? geometryConfirmed
        ? 'Geometry locked'
        : 'Geometry ready'
      : selectedFile
        ? isImageFile(selectedFile)
          ? processingState === 'processing'
            ? 'Analyzing image'
            : 'Geometry pending'
          : 'No crop needed'
        : 'Waiting for file'
  const processingBadge = processingState === 'error'
    ? 'Preview error'
    : processingState === 'processing'
      ? 'Analyzing file'
      : selectedFile
        ? geometryConfirmed || !imageSize
          ? 'Ready to file'
          : 'Geometry ready'
        : 'Awaiting file'

  const submitCapture = useMutation({
    mutationFn: async () => {
      if (!selectedFile) {
        throw new Error('Choose a file before submitting.')
      }
      if (!detailsReady) {
        throw new Error('Confirm the crop before submitting.')
      }
      if (!captureForm.title.trim()) {
        throw new Error('Add a title before submitting.')
      }
      if (!captureForm.description.trim()) {
        throw new Error('Add a description before submitting.')
      }
      if (!captureForm.documentClass.trim() || !captureForm.documentType.trim() || !captureForm.documentSubtype.trim()) {
        throw new Error('Choose a document class, type, and subtype before submitting.')
      }

      const fileContentBase64 = await readFileAsBase64(selectedFile)
      const record = await createRecord(accessToken, {
        title: captureForm.title.trim(),
        description: captureForm.description.trim(),
        recordType: 'document',
        documentClass: captureForm.documentClass.trim(),
        documentType: captureForm.documentType.trim(),
        documentSubtype: captureForm.documentSubtype.trim(),
        classification: captureForm.classification.trim(),
        sourceProduct: captureForm.sourceProduct.trim(),
        sourceObjectType: captureForm.sourceObjectType.trim(),
        sourceObjectId: captureForm.sourceObjectId.trim(),
        sourceObjectDisplayName: captureForm.sourceObjectDisplayName.trim(),
        ownerPersonId: captureForm.ownerPersonId.trim(),
        uploadedByPersonId: actorPersonId,
        currentFileName: selectedFile.name,
        currentMimeType: selectedFile.type || 'application/octet-stream',
        fileContentBase64,
      })

      let queuedScan: RecordArrScanProcessing | null = null
      let warning: string | null = null
      let warningStage: 'scan' | 'correction' | null = null

      try {
        queuedScan = await createScan(accessToken, {
          recordId: record.recordId,
          originalFileName: selectedFile.name,
          scanPurpose: captureForm.title.trim() || stripFileName(selectedFile.name),
        })

        if (imageSize && geometryPoints.length === 4) {
          try {
            queuedScan = await applyManualCorrection(accessToken, queuedScan.scanProcessingId, {
              edgeCoordinates: formatGeometryPoints(geometryPoints),
              correctedByPersonId: actorPersonId,
            })
          } catch (error) {
            warning = getErrorMessage(error)
            warningStage = 'correction'
          }
        }
      } catch (error) {
        warning = getErrorMessage(error)
        warningStage = 'scan'
      }

      return { record, scan: queuedScan, warning, warningStage }
    },
    onSuccess: async ({ record, scan, warning, warningStage }) => {
      setCreatedRecord(record)
      setCreatedScan(scan)
      setSyncWarning(warning)
      setSyncWarningStage(warningStage as 'scan' | 'correction' | null)
      setProcessingMessage(warning ? warning : `Record ${record.recordNumber} filed and queued for backend processing.`)
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
    onError: (error) => {
      setProcessingMessage(getErrorMessage(error))
      setSyncWarning(getErrorMessage(error))
      setSyncWarningStage(null)
    },
  })

  const retryProcessingMutation = useMutation({
    mutationFn: async () => {
      if (!createdRecord) {
        throw new Error('Submit the record before retrying backend processing.')
      }

      try {
        let nextScan = createdScan
        let warning: string | null = null
        let warningStage: 'scan' | 'correction' | null = null

        if (!nextScan || syncWarningStage === 'scan') {
          nextScan = await createScan(accessToken, {
            recordId: createdRecord.recordId,
            originalFileName: selectedFile?.name || createdRecord.currentFileName || 'capture',
            scanPurpose: captureForm.title.trim() || stripFileName(selectedFile?.name || createdRecord.currentFileName || 'capture'),
          })
        }

        if (imageSize && geometryPoints.length === 4) {
          try {
            nextScan = await applyManualCorrection(accessToken, nextScan.scanProcessingId, {
              edgeCoordinates: formatGeometryPoints(geometryPoints),
              correctedByPersonId: actorPersonId,
            })
          } catch (error) {
            warning = getErrorMessage(error)
            warningStage = 'correction'
          }
        }

        return { nextScan, warning, warningStage }
      } catch (error) {
        return {
          nextScan: createdScan,
          warning: getErrorMessage(error),
          warningStage: createdScan ? 'correction' : 'scan',
        }
      }
    },
    onSuccess: async ({ nextScan, warning, warningStage }) => {
      if (nextScan) {
        setCreatedScan(nextScan)
      }
      setSyncWarning(warning)
      setSyncWarningStage(warningStage as 'scan' | 'correction' | null)
      setProcessingMessage(warning ? warning : 'Backend scan updated successfully.')
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
    onError: (error) => {
      setProcessingMessage(getErrorMessage(error))
      setSyncWarning(getErrorMessage(error))
      setSyncWarningStage(null)
    },
  })

  const canSubmit =
    Boolean(selectedFile) &&
    detailsReady &&
    Boolean(captureForm.title.trim()) &&
    Boolean(captureForm.description.trim()) &&
    Boolean(captureForm.documentClass.trim()) &&
    Boolean(captureForm.documentType.trim()) &&
    Boolean(captureForm.documentSubtype.trim()) &&
    Boolean(captureForm.classification.trim()) &&
    !submitCapture.isPending &&
    !retryProcessingMutation.isPending

  const scansQuery = useQuery({
    queryKey: ['recordarr', 'capture-scans', createdRecord?.recordId ?? 'none'],
    queryFn: () => listScans(accessToken),
    enabled: Boolean(accessToken && createdRecord?.recordId),
    refetchInterval: createdRecord ? 3000 : false,
  })

  const liveScan = useMemo(() => {
    if (!createdRecord) {
      return null
    }

    const allScans = scansQuery.data ?? []
    if (createdScan) {
      return allScans.find((scan) => scan.scanProcessingId === createdScan.scanProcessingId) ?? createdScan
    }

    return allScans.find((scan) => scan.recordId === createdRecord.recordId) ?? null
  }, [createdRecord, createdScan, scansQuery.data])

  const ocrQuery = useQuery({
    queryKey: ['recordarr', 'capture-ocr', liveScan?.ocrResultId],
    queryFn: () => getOcrResult(accessToken, liveScan!.ocrResultId!),
    enabled: Boolean(accessToken && liveScan?.ocrResultId),
  })

  const extractionQuery = useQuery({
    queryKey: ['recordarr', 'capture-extraction', liveScan?.extractionResultId],
    queryFn: () => getExtractionResult(accessToken, liveScan!.extractionResultId!),
    enabled: Boolean(accessToken && liveScan?.extractionResultId),
  })

  const processingSteps = useMemo(
    () => [
      {
        label: 'Upload received',
        detail: selectedFile ? uploadSummary : 'No file yet',
        complete: Boolean(selectedFile),
      },
      {
        label: 'Crop triaged',
        detail:
          processingState === 'error'
            ? 'Preview error'
            : imageSize
              ? geometrySummary
              : selectedFile
                ? 'No crop needed'
                : 'Waiting for file',
        complete: Boolean(selectedFile && processingState !== 'error' && (!imageSize || geometryConfirmed)),
      },
      {
        label: 'Details complete',
        detail: detailsReady ? 'Document fields unlocked' : 'Confirm the geometry first',
        complete: detailsReady,
      },
      {
        label: 'Record filed',
        detail: createdRecord ? `${createdRecord.recordNumber} · ${createdRecord.recordId}` : 'Waiting to submit',
        complete: Boolean(createdRecord),
      },
      {
        label: 'Scan queued',
        detail: liveScan ? `${liveScan.scanProcessingId} · ${formatDisplayLabel(liveScan.status)}` : createdRecord ? 'Waiting for scan queue' : 'Waiting to submit',
        complete: Boolean(liveScan),
      },
      {
        label: 'OCR available',
        detail: ocrQuery.data ? `${ocrQuery.data.engine} · ${Math.round(ocrQuery.data.confidenceScore * 100)}%` : 'Waiting for OCR',
        complete: Boolean(ocrQuery.data),
      },
      {
        label: 'Metadata extracted',
        detail: extractionQuery.data
          ? `${formatDisplayLabel(extractionQuery.data.status)} · ${extractionQuery.data.extractedFields.length} field(s)`
          : 'Waiting for extraction',
        complete: Boolean(extractionQuery.data),
      },
    ],
    [
      createdRecord,
      createdScan,
      detailsReady,
      extractionQuery.data,
      geometryConfirmed,
      geometrySummary,
      imageSize,
      liveScan,
      ocrQuery.data,
      selectedFile,
      uploadSummary,
    ],
  )

  const activeScanStatus = liveScan?.status ? formatDisplayLabel(liveScan.status) : 'Not queued'
  const submitLabel = createdRecord
    ? retryProcessingMutation.isPending
      ? 'Retrying…'
      : syncWarningStage === 'scan'
        ? 'Queue scan again'
        : 'Resync crop'
    : submitCapture.isPending
      ? 'Submitting…'
      : 'Submit document'

  const fileActionButtons = (
    <div className="recordarr-capture-button-row">
      <button type="button" className="recordarr-capture-button primary" onClick={() => openNativeFileChooser(cameraInputRef.current)}>
        <Camera className="h-4 w-4" />
        Use camera
      </button>
      <button type="button" className="recordarr-capture-button" onClick={() => openNativeFileChooser(uploadInputRef.current)}>
        <Upload className="h-4 w-4" />
        Upload file
      </button>
      {selectedFile ? (
        <button type="button" className="recordarr-capture-button secondary" onClick={resetCapture}>
          Reset
        </button>
      ) : null}
    </div>
  )

  return (
    <div className="recordarr-page recordarr-capture-page">
      <SectionHeader
        eyebrow="Capture"
        title="Scan Capture"
        description="Upload or use the device camera, correct geometry locally, then file the record once the details are ready."
        action={
          <div className="recordarr-capture-badges">
            <span className="recordarr-capture-chip">{selectedFile ? 'File ready' : 'Awaiting upload'}</span>
            <span className="recordarr-capture-chip">{geometrySummary}</span>
            <span className={`recordarr-capture-chip ${createdRecord ? 'recordarr-capture-chip-success' : ''}`}>
              {createdRecord ? 'Filed' : 'No draft record'}
            </span>
          </div>
        }
      />

      <div className="recordarr-capture-shell">
        <div className="recordarr-capture-header">
          <div className="max-w-3xl">
            <p className="recordarr-capture-kicker">Upload first, triage second, submit last.</p>
            <h2 className="recordarr-capture-title">Geometry correction lives before the canonical record exists.</h2>
            <p className="recordarr-capture-subtitle">
              The file stays local while you crop and tune the corners. Only the final submit creates the canonical record and queues the backend scan.
            </p>
          </div>
          <div className="recordarr-capture-badges">
            <span className="recordarr-capture-chip">{captureSourceLabel}</span>
            <span className="recordarr-capture-chip">{processingBadge}</span>
          </div>
        </div>

        <div className="recordarr-capture-layout">
          <div className="recordarr-capture-stack">
            <section className="recordarr-card">
              <div className="recordarr-card-inner recordarr-capture-stack">
                <div className="recordarr-capture-panel-header">
                  <div>
                    <p className="recordarr-capture-panel-eyebrow">1 · Upload</p>
                    <h3 className="recordarr-capture-panel-title">Bring in a file or use the camera</h3>
                    <p className="recordarr-capture-panel-copy">
                      The native file picker can open the camera on supported devices. No in-page capture, no draft session, and no record until you submit.
                    </p>
                  </div>
                  <span className={`recordarr-capture-chip ${selectedFile ? 'recordarr-capture-chip-success' : ''}`}>
                    {selectedFile ? 'Ready for triage' : 'No file selected'}
                  </span>
                </div>

                {selectedFile ? (
                  <div className="recordarr-capture-summary">
                    <div className="recordarr-capture-summary-grid">
                      <div className="recordarr-capture-summary-item">
                        <p className="recordarr-capture-summary-label">File</p>
                        <p className="recordarr-capture-summary-value">{uploadSummary}</p>
                      </div>
                      <div className="recordarr-capture-summary-item">
                        <p className="recordarr-capture-summary-label">Source</p>
                        <p className="recordarr-capture-summary-value">{captureSourceLabel}</p>
                      </div>
                      <div className="recordarr-capture-summary-item">
                        <p className="recordarr-capture-summary-label">Preview</p>
                        <p className="recordarr-capture-summary-value">
                          {imageSize
                            ? `${imageSize.width} × ${imageSize.height}`
                            : selectedFile && isImageFile(selectedFile)
                              ? 'Analyzing image…'
                              : 'No geometry preview needed'}
                        </p>
                      </div>
                      <div className="recordarr-capture-summary-item">
                        <p className="recordarr-capture-summary-label">Owner</p>
                        <p className="recordarr-capture-summary-value">{actorDisplayName || actorPersonId}</p>
                      </div>
                    </div>
                    <div className="recordarr-capture-note">
                      <p className="m-0">
                        {processingMessage}
                      </p>
                    </div>
                  </div>
                ) : (
                  <div className="recordarr-capture-dropzone">
                    <div className="space-y-2">
                      <p className="recordarr-capture-dropzone-title">Capture from the device, not the page.</p>
                      <p className="recordarr-capture-dropzone-copy">
                        Choose a file or ask the browser for the camera. The file is analyzed immediately after selection so you can correct geometry before filing anything.
                      </p>
                    </div>
                    {fileActionButtons}
                  </div>
                )}

                {selectedFile ? fileActionButtons : null}
              </div>
            </section>

            <section className="recordarr-card">
              <div className="recordarr-card-inner recordarr-capture-stack">
                <div className="recordarr-capture-panel-header">
                  <div>
                    <p className="recordarr-capture-panel-eyebrow">2 · Triage</p>
                    <h3 className="recordarr-capture-panel-title">Correct geometry and crop preview</h3>
                    <p className="recordarr-capture-panel-copy">
                      Drag any corner handle to fix the crop. When you lock it, the document details section opens.
                    </p>
                  </div>
                  <span className={`recordarr-capture-chip ${detailsReady ? 'recordarr-capture-chip-success' : ''}`}>
                    {geometrySummary}
                  </span>
                </div>

                {selectedFile && imageSize && previewUrl ? (
                  <>
                    <div className="recordarr-capture-preview-frame">
                      <div
                        ref={previewSurfaceRef}
                        className="recordarr-capture-preview-aspect"
                        style={{ aspectRatio: `${imageSize.width} / ${imageSize.height}` }}
                      >
                        <img
                          src={previewUrl}
                          alt={`Preview of ${selectedFile.name}`}
                          className="recordarr-capture-preview-image"
                          draggable={false}
                        />
                        <svg
                          className="recordarr-capture-preview-overlay"
                          viewBox={`0 0 ${imageSize.width} ${imageSize.height}`}
                          preserveAspectRatio="none"
                          aria-hidden
                        >
                          <polygon
                            points={geometryPoints.map((point) => `${point.x},${point.y}`).join(' ')}
                            fill="var(--color-accent-soft)"
                            stroke="var(--color-accent)"
                            strokeWidth={Math.max(4, Math.round(Math.min(imageSize.width, imageSize.height) * 0.004))}
                            strokeLinejoin="round"
                          />
                        </svg>
                        {geometryPoints.map((point, index) => (
                          <button
                            key={`${index}-${Math.round(point.x)}-${Math.round(point.y)}`}
                            type="button"
                            aria-label={`Adjust corner ${index + 1}`}
                            className="recordarr-capture-handle"
                            style={{
                              left: `${(point.x / imageSize.width) * 100}%`,
                              top: `${(point.y / imageSize.height) * 100}%`,
                            }}
                            onPointerDown={applyCornerPointerDown(index)}
                            onPointerMove={applyCornerPointerMove(index)}
                            onPointerUp={applyCornerPointerUp(index)}
                            onPointerCancel={applyCornerPointerUp(index)}
                            onBlur={finishGeometryEdit}
                            onKeyDown={applyCornerKeyDown(index)}
                          />
                        ))}
                      </div>
                    </div>

                    <div className="recordarr-capture-button-row">
                      <button
                        type="button"
                        className="recordarr-capture-button"
                        onClick={resetGeometry}
                        disabled={!imageSize}
                      >
                        <Crop className="h-4 w-4" />
                        Reset crop
                      </button>
                      <button
                        type="button"
                        className="recordarr-capture-button primary"
                        onClick={
                          geometryConfirmed
                            ? () => {
                                setGeometryConfirmed(false)
                                setProcessingMessage('Geometry unlocked. Adjust the crop and confirm again.')
                              }
                            : confirmGeometry
                        }
                        disabled={!selectedFile || processingState === 'error'}
                      >
                        <CheckCircle2 className="h-4 w-4" />
                        {geometryConfirmed ? 'Edit geometry' : 'Confirm geometry'}
                      </button>
                    </div>
                  </>
                ) : selectedFile ? (
                  <div className="recordarr-capture-note">
                    <p className="m-0">
                      {processingState === 'error'
                        ? 'The image preview could not be generated. Replace the file and try again.'
                        : processingState === 'processing' && isImageFile(selectedFile)
                          ? 'Analyzing the image and auto-detecting edges now.'
                          : 'This file does not need crop correction. Continue to document details.'}
                    </p>
                  </div>
                ) : (
                  <div className="recordarr-capture-note">
                    <p className="m-0">Upload a file first. Geometry correction becomes available as soon as the preview loads.</p>
                  </div>
                )}
              </div>
            </section>

            <section className="recordarr-card">
              <div className="recordarr-card-inner recordarr-capture-stack">
                <div className="recordarr-capture-panel-header">
                  <div>
                    <p className="recordarr-capture-panel-eyebrow">3 · Details</p>
                    <h3 className="recordarr-capture-panel-title">Document details</h3>
                    <p className="recordarr-capture-panel-copy">
                      {detailsReady
                        ? 'Finish the metadata fields now that the crop is locked.'
                        : 'Lock the crop first to unlock the details form.'}
                    </p>
                  </div>
                  <span className={`recordarr-capture-chip ${detailsReady ? 'recordarr-capture-chip-success' : ''}`}>
                    {detailsReady ? 'Unlocked' : 'Locked'}
                  </span>
                </div>

                {!detailsReady ? (
                  <div className="recordarr-capture-note">
                    <p className="m-0">Confirm the geometry above and this form will open up.</p>
                  </div>
                ) : null}

                <div className={`grid gap-3 md:grid-cols-2 ${detailsReady ? '' : 'pointer-events-none opacity-60'}`}>
                  <Field label="Title" htmlFor="capture-title">
                    <input
                      id="capture-title"
                      className="recordarr-input"
                      value={captureForm.title}
                      onChange={(event) => setCaptureForm({ ...captureForm, title: event.target.value })}
                      placeholder="PO-2024-0156 - Global Supplies, Inc."
                      disabled={!detailsReady || submitCapture.isPending || retryProcessingMutation.isPending}
                    />
                  </Field>
                  <Field label="Classification" htmlFor="capture-classification">
                    <select
                      id="capture-classification"
                      className="recordarr-select"
                      value={captureForm.classification}
                      onChange={(event) => setCaptureForm({ ...captureForm, classification: event.target.value })}
                      disabled={!detailsReady || submitCapture.isPending || retryProcessingMutation.isPending}
                    >
                      <ReadableOption value="public" />
                      <ReadableOption value="internal" />
                      <ReadableOption value="confidential" />
                      <ReadableOption value="restricted" />
                      <ReadableOption value="legal_hold" />
                    </select>
                  </Field>
                  <Field label="Document class" htmlFor="capture-document-class">
                    <select
                      id="capture-document-class"
                      className="recordarr-select"
                      value={captureForm.documentClass}
                      onChange={(event) => setCaptureForm({ ...captureForm, documentClass: event.target.value })}
                      disabled={!detailsReady || documentClassOptionsLoading || submitCapture.isPending || retryProcessingMutation.isPending}
                    >
                      <option value="">{documentClassOptionsLoading ? 'Loading document classes…' : 'Select document class'}</option>
                      {documentClassOptions.map((option) => (
                        <option key={option.value} value={option.value} disabled={option.inactive && option.value !== captureForm.documentClass}>
                          {option.label}
                        </option>
                      ))}
                    </select>
                  </Field>
                  <Field label="Document type" htmlFor="capture-document-type">
                    <select
                      id="capture-document-type"
                      className="recordarr-select"
                      value={captureForm.documentType}
                      onChange={(event) => setCaptureForm({ ...captureForm, documentType: event.target.value })}
                      disabled={!detailsReady || documentTypeOptionsLoading || submitCapture.isPending || retryProcessingMutation.isPending}
                    >
                      <option value="">{documentTypeOptionsLoading ? 'Loading document types…' : 'Select document type'}</option>
                      {documentTypeOptions.map((option) => (
                        <option key={option.value} value={option.value} disabled={option.inactive && option.value !== captureForm.documentType}>
                          {option.label}
                        </option>
                      ))}
                    </select>
                  </Field>
                  <Field label="Document subtype" htmlFor="capture-document-subtype">
                    <select
                      id="capture-document-subtype"
                      className="recordarr-select"
                      value={captureForm.documentSubtype}
                      onChange={(event) => setCaptureForm({ ...captureForm, documentSubtype: event.target.value })}
                      disabled={!detailsReady || documentSubtypeOptionsLoading || submitCapture.isPending || retryProcessingMutation.isPending}
                    >
                      <option value="">{documentSubtypeOptionsLoading ? 'Loading document subtypes…' : 'Select document subtype'}</option>
                      {documentSubtypeOptions.map((option) => (
                        <option key={option.value} value={option.value} disabled={option.inactive && option.value !== captureForm.documentSubtype}>
                          {option.label}
                        </option>
                      ))}
                    </select>
                  </Field>
                  <Field label="Description" htmlFor="capture-description" wide>
                    <textarea
                      id="capture-description"
                      className="recordarr-textarea"
                      value={captureForm.description}
                      onChange={(event) => setCaptureForm({ ...captureForm, description: event.target.value })}
                      placeholder="Explain what the file is and where it came from."
                      rows={4}
                      disabled={!detailsReady || submitCapture.isPending || retryProcessingMutation.isPending}
                    />
                  </Field>
                </div>

                <div className="recordarr-capture-summary-grid">
                  <div className="recordarr-capture-summary-item">
                    <p className="recordarr-capture-summary-label">Source lineage</p>
                    <p className="recordarr-capture-summary-value">{selectedFile ? 'RecordArr capture' : 'Auto-fills after file selection'}</p>
                    {sourceRef ? <p className="recordarr-capture-check-copy">{sourceRef}</p> : null}
                  </div>
                  <div className="recordarr-capture-summary-item">
                    <p className="recordarr-capture-summary-label">Owner</p>
                    <p className="recordarr-capture-summary-value">{actorDisplayName || actorPersonId}</p>
                  </div>
                </div>
              </div>
            </section>
          </div>

          <aside className="recordarr-capture-rail">
            <section className="recordarr-card">
              <div className="recordarr-card-inner recordarr-capture-stack">
                <div className="recordarr-capture-panel-header">
                  <div>
                    <p className="recordarr-capture-panel-eyebrow">{createdRecord ? '4 · Processing' : '4 · Submit'}</p>
                    <h3 className="recordarr-capture-panel-title">{createdRecord ? 'Backend processing' : 'File the record'}</h3>
                    <p className="recordarr-capture-panel-copy">
                      {createdRecord
                        ? 'The canonical record exists now. OCR and metadata extraction continue in the backend.'
                        : 'Once the details are ready, this is the only step that creates the canonical record.'}
                    </p>
                  </div>
                  <span className={`recordarr-capture-chip ${createdRecord ? 'recordarr-capture-chip-success' : ''}`}>
                    {createdRecord ? 'Filed' : 'Local only'}
                  </span>
                </div>

                {syncWarning ? (
                  <div className="recordarr-capture-alert">
                    <strong>
                      {syncWarningStage === 'scan'
                        ? 'Scan queue issue'
                        : syncWarningStage === 'correction'
                          ? 'Crop sync issue'
                          : 'Submit issue'}
                    </strong>
                    <p>{syncWarning}</p>
                  </div>
                ) : null}

                <div className="recordarr-capture-checklist">
                  {processingSteps.map((step) => (
                    <div key={step.label} className="recordarr-capture-check">
                      <div className="recordarr-capture-check-left">
                        {step.complete ? <CheckCircle2 className="h-4 w-4 text-emerald-500" /> : <CircleDashed className="h-4 w-4 text-slate-400" />}
                        <div className="min-w-0">
                          <p className="recordarr-capture-check-title">{step.label}</p>
                          <p className="recordarr-capture-check-copy">{step.detail}</p>
                        </div>
                      </div>
                      <span className={`recordarr-capture-chip ${step.complete ? 'recordarr-capture-chip-success' : ''}`}>
                        {step.complete ? 'Done' : 'Pending'}
                      </span>
                    </div>
                  ))}
                </div>

                <div className="recordarr-capture-note">
                  <p className="m-0">
                    {createdRecord
                      ? `Record ${createdRecord.recordNumber} is filed. Scan status: ${activeScanStatus}.`
                      : 'No backend record exists yet. Upload and geometry triage stay local until you submit.'}
                  </p>
                </div>

                <div className="recordarr-capture-button-row">
                  {!createdRecord ? (
                    <button
                      type="button"
                      className="recordarr-capture-button primary"
                      onClick={() => submitCapture.mutate()}
                      disabled={!canSubmit}
                    >
                      <FileText className="h-4 w-4" />
                      {submitLabel}
                    </button>
                  ) : (
                    <button
                      type="button"
                      className="recordarr-capture-button primary"
                      onClick={() => retryProcessingMutation.mutate()}
                      disabled={retryProcessingMutation.isPending || !createdRecord}
                    >
                      <Sparkles className="h-4 w-4" />
                      {submitLabel}
                    </button>
                  )}
                  <button type="button" className="recordarr-capture-button" onClick={resetCapture}>
                    Reset all
                  </button>
                </div>
              </div>
            </section>

            {createdRecord ? (
              <section className="recordarr-card">
                <div className="recordarr-card-inner recordarr-capture-stack">
                  <div className="recordarr-capture-panel-header">
                    <div>
                      <p className="recordarr-capture-panel-eyebrow">Live backend output</p>
                      <h3 className="recordarr-capture-panel-title">OCR and metadata</h3>
                      <p className="recordarr-capture-panel-copy">These values update as the scan engine finishes work on the newly filed record.</p>
                    </div>
                    <span className="recordarr-capture-chip">{createdScan?.scanProcessingId ? 'Scan live' : 'Waiting for scan'}</span>
                  </div>

                  <div className="recordarr-capture-summary-grid">
                    <div className="recordarr-capture-summary-item">
                      <p className="recordarr-capture-summary-label">Record</p>
                      <p className="recordarr-capture-summary-value">{createdRecord.recordNumber}</p>
                    </div>
                    <div className="recordarr-capture-summary-item">
                      <p className="recordarr-capture-summary-label">Scan</p>
                      <p className="recordarr-capture-summary-value">{createdScan?.scanProcessingId || 'Queued'}</p>
                    </div>
                    <div className="recordarr-capture-summary-item">
                      <p className="recordarr-capture-summary-label">OCR</p>
                      <p className="recordarr-capture-summary-value">{ocrQuery.data ? `${ocrQuery.data.engine} · ${Math.round(ocrQuery.data.confidenceScore * 100)}%` : 'Waiting'}</p>
                    </div>
                    <div className="recordarr-capture-summary-item">
                      <p className="recordarr-capture-summary-label">Metadata</p>
                      <p className="recordarr-capture-summary-value">{extractionQuery.data ? `${formatDisplayLabel(extractionQuery.data.status)} · ${extractionQuery.data.extractedFields.length}` : 'Waiting'}</p>
                    </div>
                  </div>
                </div>
              </section>
            ) : null}
          </aside>
        </div>
      </div>

      <input
        ref={cameraInputRef}
        type="file"
        accept="image/*,application/pdf"
        capture="environment"
        className="hidden"
        onChange={handleFileInputChange('camera')}
      />
      <input
        ref={uploadInputRef}
        type="file"
        accept="image/*,application/pdf"
        className="hidden"
        onChange={handleFileInputChange('upload')}
      />
    </div>
  )
}

function DocumentsPage({ accessToken, actorPersonId }: WorkspacePageProps) {
  const queryClient = useQueryClient()
  const roleOptions = useStaffRoleOptions(accessToken)
  const { options: documentClassOptions, isLoading: documentClassOptionsLoading } = useVocabularyTermOptions(accessToken, 'document_class')
  const { options: documentTypeOptions, isLoading: documentTypeOptionsLoading } = useVocabularyTermOptions(accessToken, 'document_type')
  const { options: documentSubtypeOptions, isLoading: documentSubtypeOptionsLoading } = useVocabularyTermOptions(accessToken, 'document_subtype')
  const docsQuery = useQuery({
    queryKey: ['recordarr', 'documents'],
    queryFn: () => listControlledDocuments(accessToken),
    enabled: Boolean(accessToken),
  })
  const [selectedDocumentId, setSelectedDocumentId] = useState<string>('')
  const [newDocument, setNewDocument] = useState({
    title: '',
    description: '',
    documentClass: '',
    documentType: '',
    documentSubtype: '',
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
  const createDocumentDisabled =
    !newDocument.title.trim() ||
    !newDocument.documentClass ||
    !newDocument.documentType ||
    !newDocument.documentSubtype ||
    !newDocument.ownerPersonId ||
    !newDocument.departmentOrgUnitId ||
    !newDocument.staffarrSiteId
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
            <Field label="Document class">
              <StaticSearchPicker
                value={newDocument.documentClass}
                onChange={(documentClass) => setNewDocument({ ...newDocument, documentClass })}
                options={documentClassOptions}
                placeholder={documentClassOptionsLoading ? 'Loading document classes…' : 'Search document classes'}
                disabled={documentClassOptionsLoading}
              />
            </Field>
            <Field label="Document type">
              <StaticSearchPicker
                value={newDocument.documentType}
                onChange={(documentType) => setNewDocument({ ...newDocument, documentType })}
                options={documentTypeOptions}
                placeholder={documentTypeOptionsLoading ? 'Loading document types…' : 'Search document types'}
                disabled={documentTypeOptionsLoading}
              />
            </Field>
            <Field label="Document subtype">
              <StaticSearchPicker
                value={newDocument.documentSubtype}
                onChange={(documentSubtype) => setNewDocument({ ...newDocument, documentSubtype })}
                options={documentSubtypeOptions}
                placeholder={documentSubtypeOptionsLoading ? 'Loading document subtypes…' : 'Search document subtypes'}
                disabled={documentSubtypeOptionsLoading}
              />
            </Field>
            <Field label="Owner person"><PersonReferencePicker value={newDocument.ownerPersonId} onChange={(ownerPersonId) => setNewDocument({ ...newDocument, ownerPersonId })} /></Field>
            <Field label="Department org unit">
              <ReferenceSearchPicker
                client={staffReferenceClient}
                referenceType="org_unit"
                value={newDocument.departmentOrgUnitId}
                onChange={(departmentOrgUnitId) => setNewDocument({ ...newDocument, departmentOrgUnitId })}
                placeholder="Search StaffArr org units"
              />
            </Field>
            <Field label="StaffArr site"><StaffSiteReferencePicker value={newDocument.staffarrSiteId} onChange={(staffarrSiteId) => setNewDocument({ ...newDocument, staffarrSiteId })} /></Field>
            <Field label="Acknowledgement required"><select className="recordarr-select" value={String(newDocument.acknowledgementRequired)} onChange={(e) => setNewDocument({ ...newDocument, acknowledgementRequired: e.target.value === 'true' })}><option value="true">Yes</option><option value="false">No</option></select></Field>
            <Field label="Description" wide><textarea className="recordarr-textarea" value={newDocument.description} onChange={(e) => setNewDocument({ ...newDocument, description: e.target.value })} /></Field>
              </div>
              <button type="button" className="recordarr-button" onClick={() => createDocumentMutation.mutate()} disabled={createDocumentMutation.isPending || createDocumentDisabled}>
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
                <p className="mt-1 text-xs text-slate-400">{doc.documentClass} / {doc.documentType} / {doc.documentSubtype}</p>
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
                <Field label="Distribution type">
                  <ControlledSelect
                    value={distributionForm.distributionType}
                    onChange={(distributionType) => setDistributionForm({ ...distributionForm, distributionType, targetRef: '' })}
                    options={[
                      { value: 'person', label: 'Person' },
                      { value: 'role', label: 'Role' },
                      { value: 'department', label: 'Department' },
                      { value: 'site', label: 'Site' },
                      { value: 'team', label: 'Team' },
                      { value: 'product', label: 'Product' },
                      { value: 'external_link', label: 'External link' },
                    ]}
                    className="recordarr-select"
                  />
                </Field>
                <Field label="Target">
                  {distributionForm.distributionType === 'person' ? (
                    <PersonReferencePicker value={distributionForm.targetRef} onChange={(targetRef) => setDistributionForm({ ...distributionForm, targetRef })} />
                  ) : distributionForm.distributionType === 'role' ? (
                    <StaticSearchPicker
                      value={distributionForm.targetRef}
                      onChange={(targetRef) => setDistributionForm({ ...distributionForm, targetRef })}
                      options={roleOptions.options}
                      placeholder={roleOptions.isLoading ? 'Loading StaffArr roles…' : 'Search StaffArr roles'}
                      disabled={roleOptions.isLoading}
                    />
                  ) : distributionForm.distributionType === 'department' || distributionForm.distributionType === 'team' ? (
                    <ReferenceSearchPicker
                      client={staffReferenceClient}
                      referenceType="org_unit"
                      value={distributionForm.targetRef}
                      onChange={(targetRef) => setDistributionForm({ ...distributionForm, targetRef })}
                      placeholder="Search StaffArr org units"
                    />
                  ) : distributionForm.distributionType === 'site' ? (
                    <StaffSiteReferencePicker value={distributionForm.targetRef} onChange={(targetRef) => setDistributionForm({ ...distributionForm, targetRef })} />
                  ) : distributionForm.distributionType === 'product' ? (
                    <ControlledSelect
                      value={distributionForm.targetRef}
                      onChange={(targetRef) => setDistributionForm({ ...distributionForm, targetRef })}
                      options={SUITE_SOURCE_PRODUCT_OPTIONS}
                      className="recordarr-select"
                      emptyLabel="Select product"
                    />
                  ) : distributionForm.distributionType === 'external_link' ? (
                    <input
                      className="recordarr-input"
                      type="url"
                      placeholder="https://..."
                      value={distributionForm.targetRef}
                      onChange={(e) => setDistributionForm({ ...distributionForm, targetRef: e.target.value })}
                    />
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
                onChange={(_sourceObjectRef, selected) => {
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
                  <p><strong className="text-slate-100">Source:</strong> {hold.sourceProduct} · {hold.sourceObjectType}</p>
                  <p><strong className="text-slate-100">Created:</strong> {formatDate(hold.createdAt)}</p>
                  <p><strong className="text-slate-100">Released:</strong> {formatDate(hold.releasedAt)}</p>
                  <p><strong className="text-slate-100">Scope:</strong> {hold.scopeRules.join(', ')}</p>
                  <p><strong className="text-slate-100">Records:</strong> {hold.recordRefs.length} linked record(s)</p>
                  <details className="rounded-lg border border-slate-800/80 bg-slate-950/40 p-3 text-xs text-slate-300">
                    <summary className="cursor-pointer text-[var(--color-text-muted)]">Advanced technical details</summary>
                    <div className="mt-2 space-y-2 break-all">
                      <p><strong className="text-slate-100">Source object ID:</strong> {hold.sourceObjectId}</p>
                      <p><strong className="text-slate-100">Record refs:</strong> {hold.recordRefs.join(', ') || 'none'}</p>
                    </div>
                  </details>
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
            <Field label="Grantee type">
              <ControlledSelect
                value={grantForm.granteeType}
                onChange={(granteeType) => setGrantForm({ ...grantForm, granteeType, granteeRef: '' })}
                options={[
                  { value: 'role', label: 'Role' },
                  { value: 'person', label: 'Person' },
                  { value: 'org_unit', label: 'Org unit' },
                ]}
              />
            </Field>
            <Field label="Grantee ref">
              <GranteeRefPicker
                granteeType={grantForm.granteeType}
                value={grantForm.granteeRef}
                onChange={(granteeRef) => setGrantForm({ ...grantForm, granteeRef })}
              />
            </Field>
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
  const { session, sessionQuery, launchCatalogQuery, bootstrapError, workspaceSession, launch } = useRecordArrWorkspace()
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

  if (normalizedPathname === '/handoff') {
    return <Navigate replace to={{ pathname: '/launch', search: location.search }} />
  }

  if (normalizedPathname === '/launch') {
    return <LaunchPage />
  }

  const accessToken = session?.accessToken ?? ''
  const actorPersonId = session?.personId ?? ''
  const actorDisplayName = session?.displayName ?? ''
  const tenantDisplayName = session?.tenantDisplayName ?? ''

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
        <Route path="/records" element={<RecordsPage accessToken={accessToken} actorPersonId={actorPersonId} actorDisplayName={actorDisplayName} />} />
        <Route
          path="/records/:recordId"
          element={
            <RecordDetailPage
              accessToken={accessToken}
              actorPersonId={actorPersonId}
              actorDisplayName={actorDisplayName}
              tenantDisplayName={tenantDisplayName}
            />
          }
        />
        <Route path="/capture" element={<CapturePage accessToken={accessToken} actorPersonId={actorPersonId} actorDisplayName={actorDisplayName} tenantDisplayName={tenantDisplayName} />} />
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
