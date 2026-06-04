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
  PackageSearch,
  ScanSearch,
  Settings,
  ShieldCheck,
  Upload,
} from 'lucide-react'
import { Navigate, Route, Routes, useLocation, useNavigate, useParams } from 'react-router-dom'
import {
  ApiErrorCallout,
  ProductWorkspaceFrame,
  buildProductLaunchUrlMap,
  formatProductLaunchError,
  getErrorMessage,
  resolveProductWorkspaceBootstrapError,
  resolveSuiteHomeUrl,
  useProductWorkspaceLaunch,
  type ProductNavItem,
} from '@stl/shared-ui'
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
  createExternalShare,
  createRedaction,
  createLegalHold,
  createPackage,
  createRecord,
  createScan,
  createUploadSession,
  downloadPackage,
  getDashboard,
  getExtractionResult,
  getOcrResult,
  getPackageManifest,
  getRecord,
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
  listRecords,
  listRetentionPolicies,
  listScans,
  listUploadSessions,
  listRedactions,
  rejectEvidenceMapping,
  releaseLegalHold,
  reviewExtractionResult,
  revokeExternalShare,
  purgeRecord,
  promoteDocumentVersion,
  obsoleteControlledDocument,
  expireDocumentDistribution,
  supersedeControlledDocument,
  completeDocumentAcknowledgement,
  completeDocumentReview,
  completeDisposalReview,
  revokeAccessGrant,
  revokeDocumentDistribution,
  lockPackage,
  updateRecord,
  type RecordArrControlledDocument,
  type RecordArrLegalHold,
  type RecordArrPackage,
  type RecordArrRecord,
} from './api/client'
import { clearSession, loadSession, type StoredRecordArrSession } from './auth/sessionStorage'

const suiteHomeUrl = resolveSuiteHomeUrl(import.meta.env.VITE_SUITE_URL)
const productLaunchUrls = buildProductLaunchUrlMap(import.meta.env)
const apiBase = import.meta.env.VITE_RECORDARR_API_BASE ?? ''

const navItems: ProductNavItem[] = [
  { label: 'Dashboard', to: '/', icon: LayoutDashboard as ProductNavItem['icon'] },
  { label: 'Records', to: '/records', icon: FileText as ProductNavItem['icon'] },
  { label: 'Capture', to: '/capture', icon: FileUp as ProductNavItem['icon'] },
  { label: 'Documents', to: '/documents', icon: Archive as ProductNavItem['icon'] },
  { label: 'Packages', to: '/packages', icon: PackageSearch as ProductNavItem['icon'] },
  { label: 'Retention', to: '/retention', icon: Clock3 as ProductNavItem['icon'] },
  { label: 'Holds', to: '/holds', icon: ShieldCheck as ProductNavItem['icon'], sectionBreakBefore: true },
  { label: 'Access', to: '/access', icon: LockKeyhole as ProductNavItem['icon'] },
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

function useWorkspaceSessionBootstrap() {
  const session = loadSession()
  const sessionQuery = useQuery({
    queryKey: ['recordarr', 'session', session?.accessToken],
    queryFn: () => getSessionBootstrap(session!.accessToken),
    enabled: Boolean(session?.accessToken),
    retry: false,
  })

  useEffect(() => {
    if (sessionQuery.isError && resolveProductWorkspaceBootstrapError(sessionQuery.error)) {
      clearSession()
    }
  }, [sessionQuery.error, sessionQuery.isError])

  const bootstrapError = sessionQuery.isError
    ? resolveProductWorkspaceBootstrapError(sessionQuery.error)
    : null

  return {
    session,
    sessionQuery,
    bootstrapError,
  }
}

function useRecordArrWorkspace() {
  const queryClient = useQueryClient()
  const { session, sessionQuery, bootstrapError } = useWorkspaceSessionBootstrap()
  const workspaceSession =
    session && sessionQuery.data && !bootstrapError
      ? {
          userDisplayName: session.displayName,
          tenantDisplayName: session.tenantDisplayName,
          tenantSlug: session.tenantSlug,
        }
      : null

  const switcherEntitlements = sessionQuery.data?.entitlements ?? []

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
    bootstrapError,
    workspaceSession,
    switcherEntitlements,
    queryClient,
    launch,
  }
}

function DashboardPage({ accessToken }: { accessToken: string }) {
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

function RecordsPage({ accessToken }: { accessToken: string }) {
  const queryClient = useQueryClient()
  const navigate = useNavigate()
  const [search, setSearch] = useState('')
  const [form, setForm] = useState({
    title: 'Inbound BOL for delivery load',
    description: 'Captured from RoutArr proof-of-delivery handoff.',
    recordType: 'document',
    documentType: 'bol',
    sourceProduct: 'routarr',
    sourceObjectType: 'trip',
    sourceObjectId: 'trip-7781',
    sourceObjectDisplayName: 'RT-7781',
    ownerPersonId: 'person-route-lead',
    uploadedByPersonId: 'person-route-lead',
    currentFileName: 'bol-7781.pdf',
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
            <Field label="Record type"><input className="recordarr-input" value={form.recordType} onChange={(e) => setForm({ ...form, recordType: e.target.value })} /></Field>
            <Field label="Document type"><input className="recordarr-input" value={form.documentType} onChange={(e) => setForm({ ...form, documentType: e.target.value })} /></Field>
            <Field label="Source product"><input className="recordarr-input" value={form.sourceProduct} onChange={(e) => setForm({ ...form, sourceProduct: e.target.value })} /></Field>
            <Field label="Source object type"><input className="recordarr-input" value={form.sourceObjectType} onChange={(e) => setForm({ ...form, sourceObjectType: e.target.value })} /></Field>
            <Field label="Source object id"><input className="recordarr-input" value={form.sourceObjectId} onChange={(e) => setForm({ ...form, sourceObjectId: e.target.value })} /></Field>
            <Field label="Source display name"><input className="recordarr-input" value={form.sourceObjectDisplayName} onChange={(e) => setForm({ ...form, sourceObjectDisplayName: e.target.value })} /></Field>
            <Field label="Owner person id"><input className="recordarr-input" value={form.ownerPersonId} onChange={(e) => setForm({ ...form, ownerPersonId: e.target.value })} /></Field>
            <Field label="Uploaded by person id"><input className="recordarr-input" value={form.uploadedByPersonId} onChange={(e) => setForm({ ...form, uploadedByPersonId: e.target.value })} /></Field>
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

function RecordDetailPage({ accessToken }: { accessToken: string }) {
  const queryClient = useQueryClient()
  const params = useParams()
  const recordId = params.recordId ?? ''
  const [status, setStatus] = useState('review')

  const recordQuery = useQuery({
    queryKey: ['recordarr', 'records', recordId],
    queryFn: () => getRecord(accessToken, recordId),
    enabled: Boolean(accessToken && recordId),
  })
  const retentionQuery = useQuery({
    queryKey: ['recordarr', 'retention-status', recordId],
    queryFn: () => getRetentionStatus(accessToken, recordId),
    enabled: Boolean(accessToken && recordId),
  })
  const logsQuery = useQuery({
    queryKey: ['recordarr', 'access-logs'],
    queryFn: () => listAccessLogs(accessToken),
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
    mutationFn: () => updateRecord(accessToken, recordId, { status }),
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

  const record = recordQuery.data
  const relevantLogs = (logsQuery.data ?? []).filter((entry) => entry.recordId === recordId)
  const relatedScans = (scansQuery.data ?? []).filter((scan) => scan.recordId === recordId)
  const relatedMappings = (mappingsQuery.data ?? []).filter((mapping) => mapping.recordId === recordId)
  const relatedPackages = (packagesQuery.data ?? []).filter((pkg) => pkg.recordRefs.includes(recordId))
  const relatedUploads = (uploadsQuery.data ?? []).filter((upload) => upload.uploadedRecordRefs.includes(recordId))
  const relatedDocuments = (documentsQuery.data ?? []).filter((document) => document.recordId === recordId)
  const activeHold = (holdsQuery.data ?? []).find((hold) => hold.status === 'active' && hold.recordRefs.includes(recordId)) ?? null
  const timeline = useMemo(() => {
    if (!record) return []
    return [
      { key: 'uploaded', label: 'Uploaded', value: formatDate(record.uploadedAt) },
      { key: 'effective', label: 'Effective', value: formatDate(record.effectiveAt) },
      { key: 'expires', label: 'Expires', value: formatDate(record.expiresAt) },
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
                <p><strong className="text-slate-100">Version:</strong> v{record.versionNumber}</p>
                <div className="flex flex-wrap gap-2 pt-1">
                  {record.tags.map((tag) => (
                    <span key={tag} className="recordarr-pill text-[0.7rem]">{tag}</span>
                  ))}
                </div>
              </div>
            </Card>
            <Card title="Lifecycle control" icon={<BadgeCheck className="h-4 w-4 text-cyan-300" />}>
              <div className="grid gap-3 md:grid-cols-2">
                <Field label="Status">
                  <select className="recordarr-select" value={status} onChange={(e) => setStatus(e.target.value)}>
                    <option value="processing">processing</option>
                    <option value="review">review</option>
                    <option value="active">active</option>
                    <option value="approved">approved</option>
                    <option value="archived">archived</option>
                    <option value="expired">expired</option>
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
                  <p className="text-xs uppercase tracking-[0.16em] text-slate-500">{entry.label}</p>
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

function CapturePage({ accessToken }: { accessToken: string }) {
  const queryClient = useQueryClient()
  const [upload, setUpload] = useState({
    sourceProduct: 'routarr',
    sourceObjectType: 'trip',
    sourceObjectId: 'trip-7781',
    uploadPurpose: 'proof_of_delivery',
    requiresDocumentScan: true,
    requiresOcr: true,
    requiresManualReview: true,
  })
  const [selectedScanId, setSelectedScanId] = useState('')
  const [scan, setScan] = useState({
    recordId: 'rec-bol-001',
    originalFileName: 'bol-7781.jpg',
    scanPurpose: 'bol',
    edgeCoordinates: '10,10,540,20,540,720,10,720',
  })
  const [mapping, setMapping] = useState({
    recordId: 'rec-bol-001',
    sourceProduct: 'routarr',
    sourceObjectType: 'trip',
    sourceObjectId: 'trip-7781',
    complianceRequirementRef: 'evidence_requirement.trip.pod',
    evidenceTypeKey: 'proof_of_delivery',
    mappingSource: 'manual_review',
    confidenceScore: 0.94,
  })
  const [extractionReview, setExtractionReview] = useState({
    reviewedByPersonId: 'person-doc-controller',
    status: 'completed',
    failureReason: '',
  })

  const uploadSessionsQuery = useQuery({
    queryKey: ['recordarr', 'upload-sessions'],
    queryFn: () => listUploadSessions(accessToken),
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
  const scanMutation = useMutation({
    mutationFn: () => createScan(accessToken, scan),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const correctionMutation = useMutation({
    mutationFn: () => applyManualCorrection(accessToken, selectedScan?.scanProcessingId ?? '', { edgeCoordinates: scan.edgeCoordinates }),
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
      <div className="recordarr-grid cols-2">
        <Card title="Upload session" icon={<FileUp className="h-4 w-4 text-cyan-300" />}>
          <div className="grid gap-3 md:grid-cols-2">
            <Field label="Source product"><input className="recordarr-input" value={upload.sourceProduct} onChange={(e) => setUpload({ ...upload, sourceProduct: e.target.value })} /></Field>
            <Field label="Object type"><input className="recordarr-input" value={upload.sourceObjectType} onChange={(e) => setUpload({ ...upload, sourceObjectType: e.target.value })} /></Field>
            <Field label="Object id"><input className="recordarr-input" value={upload.sourceObjectId} onChange={(e) => setUpload({ ...upload, sourceObjectId: e.target.value })} /></Field>
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
            <Field label="Record id"><input className="recordarr-input" value={scan.recordId} onChange={(e) => setScan({ ...scan, recordId: e.target.value })} /></Field>
            <Field label="Original file"><input className="recordarr-input" value={scan.originalFileName} onChange={(e) => setScan({ ...scan, originalFileName: e.target.value })} /></Field>
            <Field label="Scan purpose"><input className="recordarr-input" value={scan.scanPurpose} onChange={(e) => setScan({ ...scan, scanPurpose: e.target.value })} /></Field>
            <Field label="Edge coordinates" wide><input className="recordarr-input" value={scan.edgeCoordinates} onChange={(e) => setScan({ ...scan, edgeCoordinates: e.target.value })} /></Field>
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
                <p className="mt-1 text-xs text-slate-400">OCR {entry.ocrResultId ?? 'pending'} · extraction {entry.extractionResultId ?? 'pending'}</p>
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
                  </div>
                ) : (
                  <div className="mt-3"><EmptyState title="Select a scan with an OCR result." /></div>
                )}
              </div>
              <div className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3 text-sm text-slate-300">
                <div className="flex items-center justify-between gap-3">
                  <strong className="text-slate-100">Extraction result</strong>
                  <span className="recordarr-pill text-[0.7rem]">{extractionQuery.data?.status ?? 'unloaded'}</span>
                </div>
                {extractionQuery.data ? (
                  <div className="mt-3 space-y-3">
                    <div className="grid gap-3 md:grid-cols-2">
                      <Field label="Reviewed by"><input className="recordarr-input" value={extractionReview.reviewedByPersonId} onChange={(e) => setExtractionReview({ ...extractionReview, reviewedByPersonId: e.target.value })} /></Field>
                      <Field label="Review status">
                        <select className="recordarr-select" value={extractionReview.status} onChange={(e) => setExtractionReview({ ...extractionReview, status: e.target.value })}>
                          <option value="completed">completed</option>
                          <option value="manual_review_required">manual_review_required</option>
                          <option value="failed">failed</option>
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
                          <span key={field.extractedFieldId} className="recordarr-pill text-[0.7rem]">
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
          <Field label="Record id"><input className="recordarr-input" value={mapping.recordId} onChange={(e) => setMapping({ ...mapping, recordId: e.target.value })} /></Field>
          <Field label="Source product"><input className="recordarr-input" value={mapping.sourceProduct} onChange={(e) => setMapping({ ...mapping, sourceProduct: e.target.value })} /></Field>
          <Field label="Source object type"><input className="recordarr-input" value={mapping.sourceObjectType} onChange={(e) => setMapping({ ...mapping, sourceObjectType: e.target.value })} /></Field>
          <Field label="Source object id"><input className="recordarr-input" value={mapping.sourceObjectId} onChange={(e) => setMapping({ ...mapping, sourceObjectId: e.target.value })} /></Field>
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
                <button type="button" className="recordarr-button secondary" onClick={() => confirmEvidenceMapping(accessToken, mappingRow.evidenceMappingId, { confirmedByPersonId: 'person-doc-controller', notes: 'Auto-confirmed in demo workspace.' }).then(() => queryClient.invalidateQueries({ queryKey: ['recordarr'] }))}>
                  Confirm
                </button>
                <button type="button" className="recordarr-button secondary" onClick={() => rejectEvidenceMapping(accessToken, mappingRow.evidenceMappingId, { rejectedByPersonId: 'person-doc-controller', rejectionReason: 'needs_followup', notes: 'Review required.' }).then(() => queryClient.invalidateQueries({ queryKey: ['recordarr'] }))}>
                  Reject
                </button>
              </div>
            </div>
          ))}
          {!mappingsQuery.data?.length && !mappingsQuery.isLoading ? <EmptyState title="No evidence mappings yet." /> : null}
        </div>
      </Card>
    </div>
  )
}

function DocumentsPage({ accessToken }: { accessToken: string }) {
  const queryClient = useQueryClient()
  const docsQuery = useQuery({
    queryKey: ['recordarr', 'documents'],
    queryFn: () => listControlledDocuments(accessToken),
    enabled: Boolean(accessToken),
  })
  const [selectedDocumentId, setSelectedDocumentId] = useState<string>('')
  const [newDocument, setNewDocument] = useState({
    title: 'Warehouse safe handling procedure',
    description: 'Controlled SOP for receiving and staging hazardous stock.',
    controlledDocumentType: 'procedure',
    ownerPersonId: 'person-doc-controller',
    departmentOrgUnitId: 'org-receiving',
    staffarrSiteId: 'site-north-yard',
    acknowledgementRequired: true,
  })
  const [versionForm, setVersionForm] = useState({
    fileName: 'hazmat-receiving-v3.pdf',
    createdByPersonId: 'person-doc-controller',
    changeSummary: 'Added evidence capture and review steps',
  })
  const [reviewForm, setReviewForm] = useState({
    versionId: '',
    reviewType: 'periodic_review',
    requestedByPersonId: 'person-doc-controller',
    reviewerPersonId: 'person-quality-reviewer',
    dueAt: new Date(Date.now() + 5 * 24 * 60 * 60 * 1000).toISOString(),
  })
  const [selectedReviewId, setSelectedReviewId] = useState<string>('')
  const [completeReviewForm, setCompleteReviewForm] = useState({
    status: 'approved',
    decisionReason: 'Review completed and approved for use.',
    comments: 'Approved in demo workspace.',
  })
  const [supersedeForm, setSupersedeForm] = useState({
    supersededByDocumentRef: '',
  })
  const [distributionForm, setDistributionForm] = useState({
    versionId: '',
    distributionType: 'person',
    targetRef: 'person-quality-reviewer',
  })
  const [acknowledgementForm, setAcknowledgementForm] = useState({
    versionId: '',
    personId: 'person-quality-reviewer',
    attestationText: 'I acknowledge receipt and review of this controlled document.',
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
  const archiveDocumentMutation = useMutation({
    mutationFn: () => archiveControlledDocument(accessToken, selectedDocumentId, { updatedByPersonId: 'person-doc-controller' }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const obsoleteDocumentMutation = useMutation({
    mutationFn: () => obsoleteControlledDocument(accessToken, selectedDocumentId, { updatedByPersonId: 'person-doc-controller' }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const supersedeDocumentMutation = useMutation({
    mutationFn: () =>
      supersedeControlledDocument(accessToken, selectedDocumentId, {
        supersededByDocumentRef: supersedeForm.supersededByDocumentRef,
        supersededByPersonId: 'person-doc-controller',
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
        approvedByPersonId: 'person-doc-controller',
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
        revokedByPersonId: 'person-doc-controller',
        revokeReason: 'Distribution no longer needed.',
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })
  const expireDistributionMutation = useMutation({
    mutationFn: (distributionId: string) =>
      expireDocumentDistribution(accessToken, selectedDocumentId, distributionId, {
        expiredByPersonId: 'person-doc-controller',
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
        signatureRecordRef: 'sig-rec-doc-controller',
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['recordarr'] })
    },
  })

  const selectedDocument = docsQuery.data?.find((doc) => doc.controlledDocumentId === selectedDocumentId) ?? null

  return (
    <div className="recordarr-page">
      <SectionHeader
        eyebrow="Documents"
        title="Controlled document management"
        description="Version controlled procedures, approvals, review cadences, distributions, and acknowledgements."
        action={<span className="recordarr-pill"><Archive className="h-4 w-4" /> {docsQuery.data?.length ?? 0} documents</span>}
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
            <Field label="Owner person id"><input className="recordarr-input" value={newDocument.ownerPersonId} onChange={(e) => setNewDocument({ ...newDocument, ownerPersonId: e.target.value })} /></Field>
            <Field label="Department org unit"><input className="recordarr-input" value={newDocument.departmentOrgUnitId} onChange={(e) => setNewDocument({ ...newDocument, departmentOrgUnitId: e.target.value })} /></Field>
            <Field label="StaffArr site id"><input className="recordarr-input" value={newDocument.staffarrSiteId} onChange={(e) => setNewDocument({ ...newDocument, staffarrSiteId: e.target.value })} /></Field>
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
                <Field label="File name"><input className="recordarr-input" value={versionForm.fileName} onChange={(e) => setVersionForm({ ...versionForm, fileName: e.target.value })} /></Field>
                <Field label="Created by"><input className="recordarr-input" value={versionForm.createdByPersonId} onChange={(e) => setVersionForm({ ...versionForm, createdByPersonId: e.target.value })} /></Field>
                <Field label="Change summary" wide><input className="recordarr-input" value={versionForm.changeSummary} onChange={(e) => setVersionForm({ ...versionForm, changeSummary: e.target.value })} /></Field>
              </div>
              <button type="button" className="recordarr-button" onClick={() => createVersionMutation.mutate()} disabled={createVersionMutation.isPending}>
                {createVersionMutation.isPending ? 'Creating...' : 'Create version'}
              </button>

              <div className="grid gap-3 md:grid-cols-2">
                <Field label="Review type"><input className="recordarr-input" value={reviewForm.reviewType} onChange={(e) => setReviewForm({ ...reviewForm, reviewType: e.target.value })} /></Field>
                <Field label="Requested by"><input className="recordarr-input" value={reviewForm.requestedByPersonId} onChange={(e) => setReviewForm({ ...reviewForm, requestedByPersonId: e.target.value })} /></Field>
                <Field label="Reviewer person"><input className="recordarr-input" value={reviewForm.reviewerPersonId} onChange={(e) => setReviewForm({ ...reviewForm, reviewerPersonId: e.target.value })} /></Field>
                <Field label="Due at"><input className="recordarr-input" value={reviewForm.dueAt} onChange={(e) => setReviewForm({ ...reviewForm, dueAt: e.target.value })} /></Field>
              </div>
              <button type="button" className="recordarr-button secondary" onClick={() => createReviewMutation.mutate()} disabled={createReviewMutation.isPending}>
                {createReviewMutation.isPending ? 'Requesting...' : 'Request review'}
              </button>
              <div className="grid gap-3 md:grid-cols-2">
                <Field label="Distribution type"><input className="recordarr-input" value={distributionForm.distributionType} onChange={(e) => setDistributionForm({ ...distributionForm, distributionType: e.target.value })} /></Field>
                <Field label="Target ref"><input className="recordarr-input" value={distributionForm.targetRef} onChange={(e) => setDistributionForm({ ...distributionForm, targetRef: e.target.value })} /></Field>
                <Field label="Acknowledgement person"><input className="recordarr-input" value={acknowledgementForm.personId} onChange={(e) => setAcknowledgementForm({ ...acknowledgementForm, personId: e.target.value })} /></Field>
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
                <Field label="Superseded by document id" wide>
                  <input
                    className="recordarr-input"
                    value={supersedeForm.supersededByDocumentRef}
                    onChange={(e) => setSupersedeForm({ ...supersedeForm, supersededByDocumentRef: e.target.value })}
                    placeholder="doc-..."
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
                            {promoteVersionMutation.isPending ? 'Promoting...' : 'Approve and make effective'}
                          </button>
                        </div>
                      </div>
                    ))}
                    {!versionsQuery.data?.length ? <EmptyState title="No versions yet." /> : null}
                  </div>
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
                          <option value="approved">approved</option>
                          <option value="rejected">rejected</option>
                          <option value="changes_requested">changes_requested</option>
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
  const [selectedPackageId, setSelectedPackageId] = useState('')
  const [form, setForm] = useState({
    title: 'RoutArr closeout packet',
    packageType: 'delivery',
    sourceProduct: 'routarr',
    sourceObjectRef: 'trip-7781',
    recordRef: 'rec-bol-001',
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
            <Field label="Source product"><input className="recordarr-input" value={form.sourceProduct} onChange={(e) => setForm({ ...form, sourceProduct: e.target.value })} /></Field>
            <Field label="Source object ref"><input className="recordarr-input" value={form.sourceObjectRef} onChange={(e) => setForm({ ...form, sourceObjectRef: e.target.value })} /></Field>
            <Field label="Record ref"><input className="recordarr-input" value={form.recordRef} onChange={(e) => setForm({ ...form, recordRef: e.target.value })} /></Field>
          </div>
          <div className="flex flex-wrap gap-3">
            <button type="button" className="recordarr-button" onClick={() => createMutation.mutate()} disabled={createMutation.isPending}>
              {createMutation.isPending ? 'Creating...' : 'Create package'}
            </button>
            <button type="button" className="recordarr-button secondary" onClick={() => lockMutation.mutate()} disabled={lockMutation.isPending || !selectedPackageId}>
              {lockMutation.isPending ? 'Locking...' : 'Lock selected package'}
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
              </button>
            ))}
            {!packagesQuery.data?.length && !packagesQuery.isLoading ? <EmptyState title="No packages yet." /> : null}
          </div>
        </Card>
        <Card title="Manifest" icon={<FileText className="h-4 w-4 text-cyan-300" />}>
          {manifestQuery.data ? (
            <div className="space-y-3 text-sm text-slate-300">
              <p><strong className="text-slate-100">Checksum:</strong> {manifestQuery.data.checksum}</p>
              <p><strong className="text-slate-100">Generated by:</strong> {manifestQuery.data.generatedByPersonId}</p>
              <div className="space-y-2">
                {manifestQuery.data.recordEntries.map((entry) => (
                  <div key={entry.entryId} className="rounded-xl border border-slate-700/70 bg-slate-900/70 p-3">
                    <div className="flex items-center justify-between gap-3">
                      <strong className="text-slate-100">{entry.displayName}</strong>
                      <span className="recordarr-pill text-[0.7rem]">{entry.entryType}</span>
                    </div>
                    <p className="mt-1">{entry.checksum}</p>
                  </div>
                ))}
              </div>
            </div>
          ) : (
            <EmptyState title="Select a package to inspect its manifest." />
          )}
        </Card>
      </div>
    </div>
  )
}

function RetentionPage({ accessToken }: { accessToken: string }) {
  const queryClient = useQueryClient()
  const [recordId, setRecordId] = useState('rec-bol-001')
  const [selectedDisposalReviewId, setSelectedDisposalReviewId] = useState('')
  const [disposalForm, setDisposalForm] = useState({
    recordId: 'rec-bol-001',
    retentionStatusRef: 'rstat-001',
    proposedAction: 'archive',
    requestedByPersonId: 'person-audit-admin',
  })
  const [completeDisposalForm, setCompleteDisposalForm] = useState({
    status: 'approved',
    reviewedByPersonId: 'person-audit-admin',
    decisionReason: 'Retention review completed.',
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
  const disposalReviewsQuery = useQuery({
    queryKey: ['recordarr', 'disposal-reviews'],
    queryFn: () => listDisposalReviews(accessToken),
    enabled: Boolean(accessToken),
  })

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
          <Field label="Record id">
            <input className="recordarr-input" value={recordId} onChange={(e) => setRecordId(e.target.value)} />
          </Field>
          <div className="mt-4 space-y-3 text-sm text-slate-300">
            {statusQuery.data ? (
              <>
                <p><strong className="text-slate-100">Status:</strong> {statusQuery.data.status}</p>
                <p><strong className="text-slate-100">Expires:</strong> {formatDate(statusQuery.data.retentionExpiresAt)}</p>
                <p><strong className="text-slate-100">Next review:</strong> {formatDate(statusQuery.data.nextReviewAt)}</p>
                <p><strong className="text-slate-100">Last reviewed:</strong> {formatDate(statusQuery.data.lastReviewedAt)}</p>
              </>
            ) : (
              <EmptyState title="Enter a record id to inspect its retention status." />
            )}
          </div>
        </Card>
      </div>
      <div className="recordarr-card mt-6">
        <div className="recordarr-card-inner space-y-4">
          <div className="flex items-center gap-2">
            <BadgeCheck className="h-4 w-4 text-cyan-300" />
            <h2 className="text-lg font-semibold text-slate-50">Disposal review</h2>
          </div>
          <div className="grid gap-3 md:grid-cols-2">
            <Field label="Record id"><input className="recordarr-input" value={disposalForm.recordId} onChange={(e) => setDisposalForm({ ...disposalForm, recordId: e.target.value })} /></Field>
            <Field label="Retention status ref"><input className="recordarr-input" value={disposalForm.retentionStatusRef} onChange={(e) => setDisposalForm({ ...disposalForm, retentionStatusRef: e.target.value })} /></Field>
            <Field label="Proposed action"><input className="recordarr-input" value={disposalForm.proposedAction} onChange={(e) => setDisposalForm({ ...disposalForm, proposedAction: e.target.value })} /></Field>
            <Field label="Requested by"><input className="recordarr-input" value={disposalForm.requestedByPersonId} onChange={(e) => setDisposalForm({ ...disposalForm, requestedByPersonId: e.target.value })} /></Field>
          </div>
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
                    <option value="approved">approved</option>
                    <option value="rejected">rejected</option>
                    <option value="completed">completed</option>
                    <option value="canceled">canceled</option>
                  </select>
                </Field>
                <Field label="Reviewed by"><input className="recordarr-input" value={completeDisposalForm.reviewedByPersonId} onChange={(e) => setCompleteDisposalForm({ ...completeDisposalForm, reviewedByPersonId: e.target.value })} /></Field>
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

function HoldsPage({ accessToken }: { accessToken: string }) {
  const queryClient = useQueryClient()
  const [selectedHoldId, setSelectedHoldId] = useState('')
  const [form, setForm] = useState({
    title: 'Open audit hold',
    description: 'Holds controlled documents while audit evidence is reviewed.',
    holdType: 'audit',
    sourceProduct: 'compliancecore',
    sourceObjectType: 'audit',
    sourceObjectId: 'audit-901',
    createdByPersonId: 'person-audit-admin',
    scopeRules: 'record_type:document\ndocument_type:procedure',
    recordRefs: 'rec-sop-001\nrec-bol-001',
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
    mutationFn: () => releaseLegalHold(accessToken, selectedHoldId, { releasedByPersonId: 'person-audit-admin', releaseReason: 'Audit evidence reviewed.' }),
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
            <Field label="Source product"><input className="recordarr-input" value={form.sourceProduct} onChange={(e) => setForm({ ...form, sourceProduct: e.target.value })} /></Field>
            <Field label="Source object type"><input className="recordarr-input" value={form.sourceObjectType} onChange={(e) => setForm({ ...form, sourceObjectType: e.target.value })} /></Field>
            <Field label="Source object id"><input className="recordarr-input" value={form.sourceObjectId} onChange={(e) => setForm({ ...form, sourceObjectId: e.target.value })} /></Field>
            <Field label="Created by"><input className="recordarr-input" value={form.createdByPersonId} onChange={(e) => setForm({ ...form, createdByPersonId: e.target.value })} /></Field>
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

function AccessPage({ accessToken }: { accessToken: string }) {
  const queryClient = useQueryClient()
  const [shareForm, setShareForm] = useState({
    recordId: 'rec-bol-001',
    recipientName: 'Avery Auditor',
    recipientEmail: 'avery.auditor@example.com',
    sharePurpose: 'auditor_access',
    allowedActions: 'view\ndownload',
    createdByPersonId: 'person-doc-controller',
  })
  const [grantForm, setGrantForm] = useState({
    recordId: 'rec-bol-001',
    granteeType: 'role',
    granteeRef: 'evidence-manager',
    permission: 'read',
    grantedByPersonId: 'person-doc-controller',
    expiresAt: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString(),
  })
  const [redactionForm, setRedactionForm] = useState({
    sourceRecordId: 'rec-bol-001',
    redactedRecordId: 'rec-bol-001-redacted',
    redactionReason: 'privacy',
    redactedByPersonId: 'person-doc-controller',
    redactionRules: 'mask:signature\nmask:phone',
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

  return (
    <div className="recordarr-page">
      <SectionHeader
        eyebrow="Access"
        title="Access controls and trail"
        description="Inspect access policies, grants, shares, redactions, and usage history for record governance."
        action={<span className="recordarr-pill"><LockKeyhole className="h-4 w-4" /> Access governed</span>}
      />
      <div className="recordarr-card">
        <div className="recordarr-card-inner space-y-4">
          <div className="grid gap-3 md:grid-cols-2">
            <Field label="Record id"><input className="recordarr-input" value={shareForm.recordId} onChange={(e) => setShareForm({ ...shareForm, recordId: e.target.value })} /></Field>
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
            <Field label="Record id"><input className="recordarr-input" value={grantForm.recordId} onChange={(e) => setGrantForm({ ...grantForm, recordId: e.target.value })} /></Field>
            <Field label="Grantee type"><input className="recordarr-input" value={grantForm.granteeType} onChange={(e) => setGrantForm({ ...grantForm, granteeType: e.target.value })} /></Field>
            <Field label="Grantee ref"><input className="recordarr-input" value={grantForm.granteeRef} onChange={(e) => setGrantForm({ ...grantForm, granteeRef: e.target.value })} /></Field>
            <Field label="Permission"><input className="recordarr-input" value={grantForm.permission} onChange={(e) => setGrantForm({ ...grantForm, permission: e.target.value })} /></Field>
            <Field label="Granted by"><input className="recordarr-input" value={grantForm.grantedByPersonId} onChange={(e) => setGrantForm({ ...grantForm, grantedByPersonId: e.target.value })} /></Field>
            <Field label="Expires at"><input className="recordarr-input" value={grantForm.expiresAt} onChange={(e) => setGrantForm({ ...grantForm, expiresAt: e.target.value })} /></Field>
          </div>
          <button type="button" className="recordarr-button" onClick={() => grantMutation.mutate()} disabled={grantMutation.isPending}>
            {grantMutation.isPending ? 'Creating...' : 'Create access grant'}
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
              <div className="mt-3 flex flex-wrap gap-2">
                <button
                  type="button"
                  className="recordarr-button secondary"
                  onClick={() => revokeAccessGrant(accessToken, grant.accessGrantId, { revokedByPersonId: 'person-doc-controller', revokeReason: 'Access no longer required.' }).then(() => queryClient.invalidateQueries({ queryKey: ['recordarr'] }))}
                  disabled={grant.status === 'revoked'}
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
            <Field label="Source record id"><input className="recordarr-input" value={redactionForm.sourceRecordId} onChange={(e) => setRedactionForm({ ...redactionForm, sourceRecordId: e.target.value })} /></Field>
            <Field label="Redacted record id"><input className="recordarr-input" value={redactionForm.redactedRecordId} onChange={(e) => setRedactionForm({ ...redactionForm, redactedRecordId: e.target.value })} /></Field>
            <Field label="Reason"><input className="recordarr-input" value={redactionForm.redactionReason} onChange={(e) => setRedactionForm({ ...redactionForm, redactionReason: e.target.value })} /></Field>
            <Field label="Redacted by"><input className="recordarr-input" value={redactionForm.redactedByPersonId} onChange={(e) => setRedactionForm({ ...redactionForm, redactedByPersonId: e.target.value })} /></Field>
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
              <div className="mt-3 flex flex-wrap gap-2">
                <button type="button" className="recordarr-button secondary" onClick={() => revokeExternalShare(accessToken, share.externalShareId, { revokedByPersonId: 'person-doc-controller' }).then(() => queryClient.invalidateQueries({ queryKey: ['recordarr'] }))}>
                  Revoke
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
  const { session, sessionQuery, bootstrapError, workspaceSession, switcherEntitlements, launch } = useRecordArrWorkspace()
  const [bootstrapRedirected, setBootstrapRedirected] = useState(false)

  useEffect(() => {
    if (bootstrapError && !bootstrapRedirected) {
      clearSession()
      setBootstrapRedirected(true)
    }
  }, [bootstrapError, bootstrapRedirected])

  const currentTitle = useMemo(() => {
    if (location.pathname.startsWith('/records/')) return 'Record detail'
    if (location.pathname.startsWith('/records')) return 'Records'
    if (location.pathname.startsWith('/capture')) return 'Capture'
    if (location.pathname.startsWith('/documents')) return 'Documents'
    if (location.pathname.startsWith('/packages')) return 'Packages'
    if (location.pathname.startsWith('/retention')) return 'Retention'
    if (location.pathname.startsWith('/holds')) return 'Holds'
    if (location.pathname.startsWith('/access')) return 'Access'
    if (location.pathname.startsWith('/settings')) return 'Settings'
    return 'Dashboard'
  }, [location.pathname])

  const accessToken = session?.accessToken ?? ''

  if (!session && !sessionQuery.isLoading && !bootstrapError) {
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
      workspaceSession={workspaceSession}
      isBootstrapping={sessionQuery.isLoading}
      bootstrapError={bootstrapError}
    >
      <Routes>
        <Route index element={<DashboardPage accessToken={accessToken} />} />
        <Route path="/records" element={<RecordsPage accessToken={accessToken} />} />
        <Route path="/records/:recordId" element={<RecordDetailPage accessToken={accessToken} />} />
        <Route path="/capture" element={<CapturePage accessToken={accessToken} />} />
        <Route path="/documents" element={<DocumentsPage accessToken={accessToken} />} />
        <Route path="/packages" element={<PackagesPage accessToken={accessToken} />} />
        <Route path="/retention" element={<RetentionPage accessToken={accessToken} />} />
        <Route path="/holds" element={<HoldsPage accessToken={accessToken} />} />
        <Route path="/access" element={<AccessPage accessToken={accessToken} />} />
        <Route path="/settings" element={<SettingsPage accessToken={accessToken} session={session} />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
      <p className="mt-6 text-sm text-slate-400">Current view: {currentTitle}</p>
    </ProductWorkspaceFrame>
  )
}
