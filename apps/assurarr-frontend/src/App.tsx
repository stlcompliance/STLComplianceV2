import { useEffect, useMemo, useState, type ReactNode } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  AlertTriangle,
  BarChart3,
  BookCheck,
  CalendarClock,
  CheckCircle2,
  ClipboardList,
  FolderKanban,
  Gauge,
  Send,
  Plus,
  ShieldAlert,
  ShieldCheck,
  Sparkles,
  ListTodo,
  History,
} from 'lucide-react'
import type { LucideIcon } from 'lucide-react'
import { Link, Navigate, Route, Routes, useLocation, useParams } from 'react-router-dom'
import { ProductAppShell, type ProductNavItem } from '@stl/shared-ui'
import {
  assurarrApi,
} from './api'

const asNavIcon = (icon: LucideIcon): ProductNavItem['icon'] => icon as unknown as ProductNavItem['icon']

const navItems: ProductNavItem[] = [
  { label: 'Dashboard', to: '/', icon: asNavIcon(Gauge) },
  { label: 'Nonconformances', to: '/nonconformances', icon: asNavIcon(AlertTriangle) },
  { label: 'Holds', to: '/holds', icon: asNavIcon(ShieldCheck) },
  { label: 'CAPA', to: '/capa', icon: asNavIcon(ListTodo) },
  { label: 'Audits', to: '/audits', icon: asNavIcon(ClipboardList) },
  { label: 'Findings', to: '/findings', icon: asNavIcon(Sparkles) },
  { label: 'Reviews', to: '/reviews', icon: asNavIcon(BookCheck) },
  { label: 'Releases', to: '/releases', icon: asNavIcon(CheckCircle2) },
  { label: 'Containment', to: '/containment', icon: asNavIcon(ShieldCheck) },
  { label: 'Dispositions', to: '/dispositions', icon: asNavIcon(ClipboardList) },
  { label: 'Supplier quality', to: '/supplier-quality', icon: asNavIcon(ShieldAlert) },
  { label: 'SCARs', to: '/scars', icon: asNavIcon(Send) },
  { label: 'Complaints', to: '/complaints', icon: asNavIcon(ClipboardList) },
  { label: 'Status', to: '/status', icon: asNavIcon(BookCheck), sectionBreakBefore: true },
  { label: 'Scorecards', to: '/scorecards', icon: asNavIcon(BarChart3) },
  { label: 'History', to: '/history', icon: asNavIcon(History) },
  { label: 'Settings', to: '/settings', icon: asNavIcon(FolderKanban), sectionBreakBefore: true },
]

const statusOptions: Record<string, readonly string[]> = {
  nonconformance: ['open', 'containment', 'investigation', 'disposition_pending', 'corrective_action', 'verification', 'release_pending', 'closed', 'canceled'],
  hold: ['draft', 'active', 'release_pending', 'released', 'rejected', 'canceled', 'expired'],
  capa: ['draft', 'open', 'root_cause', 'action_plan', 'implementation', 'verification', 'effective', 'ineffective', 'closed', 'canceled'],
  audit: ['draft', 'planned', 'in_progress', 'findings_review', 'corrective_action', 'verification', 'closed', 'canceled'],
  finding: ['open', 'accepted', 'disputed', 'nonconformance_created', 'corrective_action', 'verified', 'closed', 'canceled'],
  review: ['pending', 'in_review', 'approved', 'rejected', 'changes_requested', 'canceled'],
  release: ['requested', 'pending_review', 'approved', 'rejected', 'executed', 'canceled'],
  containment: ['open', 'assigned', 'in_progress', 'completed', 'verified', 'canceled'],
  disposition: ['proposed', 'pending_approval', 'approved', 'executed', 'rejected', 'canceled'],
  supplierQuality: ['open', 'supplier_notified', 'response_pending', 'under_review', 'corrective_action', 'resolved', 'closed', 'canceled'],
  scar: ['draft', 'sent', 'acknowledged', 'supplier_response_pending', 'response_received', 'under_review', 'accepted', 'rejected', 'closed', 'canceled'],
  customerComplaint: ['received', 'triage', 'investigating', 'containment', 'response_pending', 'corrective_action', 'resolved', 'closed', 'canceled'],
}

function AppShell({ children }: { children: ReactNode }) {
  return (
    <ProductAppShell
      productName="AssurArr"
      productKey="assurarr"
      workspaceSubtitle="Quality assurance, holds, and CAPA"
      entitlements={['assurarr']}
      navItems={navItems}
    >
      {children}
    </ProductAppShell>
  )
}

function useDashboard() {
  return useQuery({
    queryKey: ['assurarr', 'dashboard'],
    queryFn: assurarrApi.getDashboard,
    staleTime: 30_000,
  })
}

function useRecords<T>(queryKey: readonly unknown[], queryFn: () => Promise<T>) {
  return useQuery({
    queryKey,
    queryFn,
    staleTime: 15_000,
  })
}

function joinRefs(value: string): string[] {
  return value
    .split(/[,\n;]/)
    .map((item) => item.trim())
    .filter(Boolean)
}

function toneClass(tone: string): string {
  switch (tone) {
    case 'danger':
      return 'text-rose-200'
    case 'warning':
      return 'text-amber-200'
    case 'info':
    case 'accent':
      return 'text-cyan-200'
    default:
      return 'text-slate-200'
  }
}

function PageHeader({
  title,
  description,
  action,
}: {
  title: string
  description: string
  action?: ReactNode
}) {
  return (
    <div className="flex flex-col gap-3 rounded-2xl border border-slate-700/70 bg-slate-950/75 p-5 shadow-xl shadow-slate-950/20 lg:flex-row lg:items-end lg:justify-between">
      <div className="space-y-2">
        <p className="text-xs font-semibold uppercase tracking-[0.2em] text-cyan-300">AssurArr</p>
        <h1 className="text-2xl font-semibold text-slate-50">{title}</h1>
        <p className="max-w-3xl text-sm text-slate-300">{description}</p>
      </div>
      {action}
    </div>
  )
}

function DashboardPage() {
  const query = useDashboard()

  return (
    <div className="assurarr-page">
      <PageHeader
        title="Quality control center"
        description="Track nonconformances, holds, CAPA, audits, and the current quality posture that other products consume."
        action={<span className="assurarr-pill"><CalendarClock className="h-4 w-4" /> Updated {query.data ? new Date(query.data.generatedAt).toLocaleString() : 'recently'}</span>}
      />
      {query.isLoading ? <LoadingCard label="Loading dashboard" /> : null}
      {query.data ? (
        <>
          <div className="assurarr-grid cols-3">
            {query.data.cards.map((card) => (
              <div key={card.key} className="assurarr-card">
                <div className="assurarr-card-inner">
                  <div className="flex items-center justify-between gap-4">
                    <div>
                      <p className="assurarr-label">{card.title}</p>
                      <p className="mt-1 text-sm text-slate-300">{card.description}</p>
                    </div>
                    <span className={`text-3xl font-semibold ${toneClass(card.tone)}`}>{card.count}</span>
                  </div>
                </div>
              </div>
            ))}
          </div>

          <div className="assurarr-grid cols-2">
            <div className="assurarr-card">
              <div className="assurarr-card-inner space-y-3">
                <div className="flex items-center gap-2">
                  <ShieldAlert className="h-5 w-5 text-cyan-300" />
                  <h2 className="text-lg font-semibold text-slate-50">Recent events</h2>
                </div>
                <div className="space-y-3">
                  {query.data.recentEvents.length === 0 ? <EmptyState title="No quality events yet" /> : null}
                  {query.data.recentEvents.map((event) => (
                    <div key={event.id} className="rounded-xl border border-slate-700/70 bg-slate-900/80 p-3">
                      <div className="flex items-center justify-between gap-3">
                        <strong className="text-sm text-slate-100">{event.eventType}</strong>
                        <time className="text-xs text-slate-400">{new Date(event.occurredAt).toLocaleString()}</time>
                      </div>
                      <p className="mt-1 text-sm text-slate-300">
                        {event.subjectType} {event.subjectId}
                        {event.details ? ` - ${event.details}` : ''}
                      </p>
                    </div>
                  ))}
                </div>
              </div>
            </div>

            <div className="assurarr-card">
              <div className="assurarr-card-inner space-y-3">
                <div className="flex items-center gap-2">
                  <CheckCircle2 className="h-5 w-5 text-cyan-300" />
                  <h2 className="text-lg font-semibold text-slate-50">Operational posture</h2>
                </div>
                <p className="text-sm text-slate-300">
                  AssurArr is publishing current quality state for downstream products while keeping the underlying issue records editable here.
                </p>
                <ul className="space-y-2 text-sm text-slate-300">
                  <li>Nonconformance holds can block release decisions.</li>
                  <li>CAPA and audit findings are visible in the same workspace.</li>
                  <li>Quality status snapshots are ready for product consumption.</li>
                </ul>
              </div>
            </div>
          </div>
        </>
      ) : null}
    </div>
  )
}

function LoadingCard({ label }: { label: string }) {
  return (
    <div className="assurarr-card">
      <div className="assurarr-card-inner text-sm text-slate-300">{label}</div>
    </div>
  )
}

function EmptyState({ title }: { title: string }) {
  return <div className="rounded-xl border border-dashed border-slate-700/80 p-4 text-sm text-slate-400">{title}</div>
}

function RecordForm({
  title,
  entityLabel,
  onCreate,
}: {
  title: string
  entityLabel: string
  onCreate: (body: {
    title: string
    description: string
    severity: string
    sourceProduct: string
    sourceObjectRef: string
    affectedObjectRefs: string[]
    ownerPersonId: string
    extra?: Record<string, string>
  }) => Promise<unknown>
}) {
  const [form, setForm] = useState({
    title: '',
    description: '',
    severity: 'moderate',
    sourceProduct: 'loadarr',
    sourceObjectRef: '',
    affectedObjectRefs: '',
    ownerPersonId: '',
  })
  const [extra, setExtra] = useState<Record<string, string>>({})
  const queryClient = useQueryClient()
  const mutation = useMutation({
    mutationFn: async () =>
      onCreate({
        ...form,
        affectedObjectRefs: joinRefs(form.affectedObjectRefs),
        extra,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['assurarr'] })
      setForm({
        title: '',
        description: '',
        severity: 'moderate',
        sourceProduct: 'loadarr',
        sourceObjectRef: '',
        affectedObjectRefs: '',
        ownerPersonId: '',
      })
      setExtra({})
    },
  })

  return (
    <div className="assurarr-card">
      <div className="assurarr-card-inner space-y-4">
        <div className="flex items-center gap-2">
          <Plus className="h-4 w-4 text-cyan-300" />
          <h3 className="text-base font-semibold text-slate-50">{title}</h3>
        </div>
        <div className="grid gap-3 md:grid-cols-2">
          <Field label={`${entityLabel} title`}>
            <input className="assurarr-input" value={form.title} onChange={(event) => setForm({ ...form, title: event.target.value })} placeholder={`New ${entityLabel.toLowerCase()} title`} />
          </Field>
          <Field label="Severity">
            <select className="assurarr-select" value={form.severity} onChange={(event) => setForm({ ...form, severity: event.target.value })}>
              <option value="low">Low</option>
              <option value="moderate">Moderate</option>
              <option value="high">High</option>
              <option value="critical">Critical</option>
              <option value="none">None</option>
            </select>
          </Field>
          <Field label="Source product">
            <input className="assurarr-input" value={form.sourceProduct} onChange={(event) => setForm({ ...form, sourceProduct: event.target.value })} />
          </Field>
          <Field label="Source object ref">
            <input className="assurarr-input" value={form.sourceObjectRef} onChange={(event) => setForm({ ...form, sourceObjectRef: event.target.value })} placeholder="loadarr:receiving:RR-24018" />
          </Field>
          <Field label="Affected object refs" wide>
            <textarea className="assurarr-textarea" value={form.affectedObjectRefs} onChange={(event) => setForm({ ...form, affectedObjectRefs: event.target.value })} placeholder="One reference per line or comma-separated" />
          </Field>
          <Field label="Description" wide>
            <textarea className="assurarr-textarea" value={form.description} onChange={(event) => setForm({ ...form, description: event.target.value })} placeholder="Describe the issue, decision, or quality observation" />
          </Field>
          <Field label="Owner person id">
            <input className="assurarr-input" value={form.ownerPersonId} onChange={(event) => setForm({ ...form, ownerPersonId: event.target.value })} placeholder="Optional UUID" />
          </Field>
        </div>
        <div className="flex flex-wrap items-center gap-3">
          <button className="assurarr-button" type="button" onClick={() => mutation.mutate()} disabled={mutation.isPending}>
            {mutation.isPending ? 'Saving...' : `Create ${entityLabel}`}
          </button>
          {mutation.isError ? <span className="text-sm text-rose-300">{String(mutation.error)}</span> : null}
        </div>
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
      <div className="assurarr-label mb-2">{label}</div>
      {children}
    </label>
  )
}

function EntityTable<T extends { id: string; number: string; title: string; status: string; severity: string; sourceProduct: string | null; sourceObjectRef: string | null; affectedObjectRefs: string[]; createdAt: string; updatedAt: string }>(
  {
    items,
    emptyLabel,
    onStatusChange,
    statusChoices,
    detailBasePath,
  }: {
    items: T[]
    emptyLabel: string
    onStatusChange?: (id: string, status: string) => Promise<unknown>
    statusChoices?: readonly string[]
    detailBasePath?: string
  },
) {
  const [selectedStatus, setSelectedStatus] = useState<Record<string, string>>({})
  const queryClient = useQueryClient()
  const mutation = useMutation({
    mutationFn: async ({ id, status }: { id: string; status: string }) => {
      if (!onStatusChange) {
        return null
      }
      return onStatusChange(id, status)
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['assurarr'] })
    },
  })

  if (items.length === 0) {
    return <EmptyState title={emptyLabel} />
  }

  return (
    <div className="overflow-hidden rounded-2xl border border-slate-700/70 bg-slate-950/75">
      <table className="assurarr-table">
        <thead>
          <tr>
            <th>Record</th>
            <th>Status</th>
            <th>Severity</th>
            <th>Source</th>
            <th>Refs</th>
            <th>Updated</th>
            <th />
          </tr>
        </thead>
        <tbody>
          {items.map((item) => (
            <tr key={item.id}>
              <td>
                {detailBasePath ? (
                  <Link to={`${detailBasePath}/${item.id}`} className="font-semibold text-cyan-300 hover:text-cyan-200">
                    {item.number}
                  </Link>
                ) : (
                  <div className="font-semibold text-slate-50">{item.number}</div>
                )}
                <div className="text-sm text-slate-300">
                  {detailBasePath ? (
                    <Link to={`${detailBasePath}/${item.id}`} className="hover:text-slate-50">
                      {item.title}
                    </Link>
                  ) : (
                    item.title
                  )}
                </div>
              </td>
              <td>{item.status}</td>
              <td>{item.severity}</td>
              <td className="text-sm text-slate-300">
                <div>{item.sourceProduct ?? 'manual'}</div>
                <div className="text-xs text-slate-400">{item.sourceObjectRef ?? 'n/a'}</div>
              </td>
              <td className="text-sm text-slate-300">
                {item.affectedObjectRefs.length > 0 ? item.affectedObjectRefs.join(', ') : 'none'}
              </td>
              <td className="text-sm text-slate-300">{new Date(item.updatedAt).toLocaleString()}</td>
              <td>
                {statusChoices && onStatusChange ? (
                  <div className="flex items-center gap-2">
                    <select
                      className="assurarr-select py-2 text-sm"
                      value={selectedStatus[item.id] ?? item.status}
                      onChange={(event) =>
                        setSelectedStatus({
                          ...selectedStatus,
                          [item.id]: event.target.value,
                        })
                      }
                    >
                      {statusChoices.map((choice) => (
                        <option key={choice} value={choice}>
                          {choice}
                        </option>
                      ))}
                    </select>
                    <button
                      className="assurarr-button secondary whitespace-nowrap"
                      type="button"
                      onClick={() =>
                        mutation.mutate({
                          id: item.id,
                          status: selectedStatus[item.id] ?? item.status,
                        })
                      }
                      disabled={mutation.isPending}
                    >
                      Update
                    </button>
                  </div>
                ) : (
                  <span className="assurarr-pill">Read only</span>
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function NonconformancePage() {
  const query = useRecords(['assurarr', 'nonconformances'], assurarrApi.listNonconformances)
  const createMutation = useMutation({
    mutationFn: async (body: {
      title: string
      description: string
      severity: string
      sourceProduct: string
      sourceObjectRef: string
      affectedObjectRefs: string[]
      ownerPersonId: string
      extra?: Record<string, string>
    }) =>
      assurarrApi.createNonconformance({
        title: body.title,
        description: body.description,
        severity: body.severity,
        sourceProduct: body.sourceProduct,
        sourceObjectRef: body.sourceObjectRef,
        affectedObjectRefs: body.affectedObjectRefs,
        ownerPersonId: body.ownerPersonId || undefined,
        nonconformanceType: body.extra?.nonconformanceType ?? 'receiving',
        category: body.extra?.category ?? 'failed_inspection',
        recurrenceFlag: false,
      }),
  })

  return (
    <div className="assurarr-page">
      <PageHeader
        title="Nonconformances"
        description="Open, investigate, and close quality failures before they spread."
        action={<span className="assurarr-pill"><Plus className="h-4 w-4" /> {query.data?.length ?? 0} records</span>}
      />
      <RecordForm
        title="Create nonconformance"
        entityLabel="Nonconformance"
        onCreate={async (body) => {
          await createMutation.mutateAsync({
            ...body,
            extra: { nonconformanceType: 'receiving', category: 'failed_inspection' },
          })
        }}
      />
      {query.data ? (
        <EntityTable
          items={query.data}
          emptyLabel="No nonconformances yet."
          onStatusChange={(id, status) => assurarrApi.updateNonconformanceStatus(id, status)}
          statusChoices={statusOptions.nonconformance}
          detailBasePath="/nonconformances"
        />
      ) : (
        <LoadingCard label="Loading nonconformances" />
      )}
    </div>
  )
}

function NonconformanceDetailPage() {
  const { id = '' } = useParams()
  const queryClient = useQueryClient()
  const query = useQuery({
    queryKey: ['assurarr', 'nonconformance', id],
    queryFn: () => assurarrApi.getNonconformance(id),
    enabled: Boolean(id),
  })
  const holds = useRecords(['assurarr', 'holds'], assurarrApi.listHolds)
  const capas = useRecords(['assurarr', 'capas'], assurarrApi.listCapas)
  const containmentActions = useRecords(['assurarr', 'containment'], assurarrApi.listContainmentActions)
  const dispositions = useRecords(['assurarr', 'dispositions'], assurarrApi.listDispositions)
  const findings = useRecords(['assurarr', 'findings'], assurarrApi.listFindings)
  const rootCauseAnalyses = useRecords(['assurarr', 'root-cause-analyses', id], () => assurarrApi.listRootCauseAnalyses(id))
  const dashboard = useDashboard()
  const nonconformance = query.data
  const [rootCauseForm, setRootCauseForm] = useState({
    title: '',
    description: '',
    status: 'in_progress',
    method: 'manual',
    primaryCauseCategory: 'unknown',
    sourceProduct: '',
    sourceObjectRef: '',
    affectedObjectRefs: '',
    rootCauseSummary: '',
    contributingFactors: '',
    recordRefs: '',
    evidenceRecordRefs: '',
    analyzedByPersonId: '',
    completedAt: '',
  })
  const createRootCauseMutation = useMutation({
    mutationFn: async () => {
      if (!nonconformance) throw new Error('Nonconformance not loaded yet.')
      return assurarrApi.createRootCauseAnalysis({
        title: rootCauseForm.title || `Root cause for ${nonconformance.number}`,
        description: rootCauseForm.description || `Root cause analysis for ${nonconformance.number}.`,
        nonconformanceId: nonconformance.id,
        status: rootCauseForm.status,
        method: rootCauseForm.method,
        primaryCauseCategory: rootCauseForm.primaryCauseCategory,
        sourceProduct: rootCauseForm.sourceProduct || nonconformance.sourceProduct || undefined,
        sourceObjectRef: rootCauseForm.sourceObjectRef || nonconformance.sourceObjectRef || undefined,
        affectedObjectRefs: joinRefs(rootCauseForm.affectedObjectRefs || nonconformance.affectedObjectRefs.join('\n')),
        ownerPersonId: nonconformance.ownerPersonId || undefined,
        recordRefs: joinRefs(rootCauseForm.recordRefs),
        rootCauseSummary: rootCauseForm.rootCauseSummary || undefined,
        contributingFactors: joinRefs(rootCauseForm.contributingFactors),
        evidenceRecordRefs: joinRefs(rootCauseForm.evidenceRecordRefs),
        analyzedByPersonId: rootCauseForm.analyzedByPersonId || undefined,
        completedAt: rootCauseForm.completedAt ? new Date(rootCauseForm.completedAt).toISOString() : undefined,
      })
    },
    onSuccess: async () => {
      setRootCauseForm((form) => ({ ...form, title: '', description: '', rootCauseSummary: '', contributingFactors: '', recordRefs: '', evidenceRecordRefs: '', analyzedByPersonId: '', completedAt: '' }))
      await queryClient.invalidateQueries({ queryKey: ['assurarr'] })
    },
  })
  if (!nonconformance) {
    return <LoadingCard label="Loading nonconformance detail" />
  }
  const relatedHolds = holds.data?.filter((hold) => hold.sourceObjectRef === nonconformance?.number || hold.sourceObjectRef === nonconformance?.sourceObjectRef || hold.sourceProduct === nonconformance?.sourceProduct || nonconformance?.number === hold.sourceObjectRef)
  const relatedCapas = capas.data?.filter((capa) => capa.relatedNonconformanceRefs.includes(nonconformance?.number ?? '') || capa.sourceObjectRef === nonconformance?.number || capa.sourceObjectRef === nonconformance?.sourceObjectRef)
  const relatedContainments = containmentActions.data?.filter((action) => action.nonconformanceRef === nonconformance?.number || action.nonconformanceRef === nonconformance?.sourceObjectRef)
  const relatedDispositions = dispositions.data?.filter((item) => item.nonconformanceRef === nonconformance?.number || item.nonconformanceRef === nonconformance?.sourceObjectRef)
  const relatedFindings = findings.data?.filter((finding) => finding.nonconformanceRef === nonconformance?.number || finding.nonconformanceRef === nonconformance?.sourceObjectRef)
  const relatedRootCauses = rootCauseAnalyses.data ?? []
  const timeline = dashboard.data?.recentEvents.filter((event) => event.subjectType === 'nonconformance' && event.subjectId === nonconformance?.id) ?? []

  return (
    <div className="assurarr-page">
      <PageHeader
        title={nonconformance ? `${nonconformance.number} · ${nonconformance.title}` : 'Nonconformance detail'}
        description="Source context, holds, containment, disposition, CAPA, evidence, and timeline for the quality case."
      />
      <div className="space-y-4">
        <div className="assurarr-grid cols-2">
          <div className="assurarr-card">
            <div className="assurarr-card-inner space-y-3">
              <p className="assurarr-label">Overview</p>
              <div className="flex flex-wrap gap-2 text-sm">
                <span className="assurarr-pill">{nonconformance.status}</span>
                <span className="assurarr-pill">{nonconformance.severity}</span>
                <span className="assurarr-pill">{nonconformance.nonconformanceType}</span>
                <span className="assurarr-pill">{nonconformance.category}</span>
              </div>
              <p className="text-sm text-slate-300">{nonconformance.description}</p>
              <div className="grid gap-2 text-sm text-slate-300 md:grid-cols-2">
                <div><span className="text-slate-500">Source product:</span> {nonconformance.sourceProduct ?? 'manual'}</div>
                <div><span className="text-slate-500">Source object:</span> {nonconformance.sourceObjectRef ?? 'n/a'}</div>
                <div><span className="text-slate-500">Owner:</span> {nonconformance.ownerPersonId ?? 'unassigned'}</div>
                <div><span className="text-slate-500">Due:</span> {nonconformance.dueAt ? new Date(nonconformance.dueAt).toLocaleString() : 'n/a'}</div>
                <div><span className="text-slate-500">Root cause:</span> {nonconformance.rootCauseRef ?? 'not started'}</div>
              </div>
            </div>
          </div>
          <div className="assurarr-card">
            <div className="assurarr-card-inner space-y-3">
              <p className="assurarr-label">Impacts</p>
              <div className="grid gap-2 text-sm text-slate-300 md:grid-cols-2">
                <div><span className="text-slate-500">Customer:</span> {nonconformance.customerImpact ?? 'none'}</div>
                <div><span className="text-slate-500">Supplier:</span> {nonconformance.supplierImpact ?? 'none'}</div>
                <div><span className="text-slate-500">Safety:</span> {nonconformance.safetyImpact ?? 'none'}</div>
                <div><span className="text-slate-500">Compliance:</span> {nonconformance.complianceImpact ?? 'none'}</div>
              </div>
              <div className="flex flex-wrap gap-2 text-xs text-slate-200">
                {nonconformance.affectedObjectRefs.length > 0 ? nonconformance.affectedObjectRefs.map((ref) => <span key={ref} className="assurarr-pill">{ref}</span>) : <span className="assurarr-pill">No affected objects</span>}
              </div>
              <div className="text-sm text-slate-300">
                <div><span className="text-slate-500">Record refs:</span> {nonconformance.recordRefs.length ? nonconformance.recordRefs.join(', ') : 'none'}</div>
                <div><span className="text-slate-500">Blockers:</span> {nonconformance.blockerRefs.length ? nonconformance.blockerRefs.join(', ') : 'none'}</div>
                <div><span className="text-slate-500">Recurrence:</span> {nonconformance.recurrenceFlag ? 'Yes' : 'No'}</div>
                <div><span className="text-slate-500">Repeat of:</span> {nonconformance.repeatOfNonconformanceRef ?? 'n/a'}</div>
              </div>
            </div>
          </div>
        </div>

        <SectionCard title="Related holds" items={relatedHolds?.map((hold) => `${hold.number} · ${hold.title} · ${hold.status}`) ?? []} emptyLabel="No holds linked to this nonconformance." />
        <SectionCard title="Related CAPA" items={relatedCapas?.map((capa) => `${capa.number} · ${capa.title} · ${capa.status}`) ?? []} emptyLabel="No CAPA linked to this nonconformance." />
        <SectionCard title="Containment actions" items={relatedContainments?.map((item) => `${item.number} · ${item.title} · ${item.status}`) ?? []} emptyLabel="No containment actions linked to this nonconformance." />
        <SectionCard title="Dispositions" items={relatedDispositions?.map((item) => `${item.number} · ${item.title} · ${item.status}`) ?? []} emptyLabel="No dispositions linked to this nonconformance." />
        <SectionCard title="Findings" items={relatedFindings?.map((item) => `${item.number} · ${item.title} · ${item.status}`) ?? []} emptyLabel="No findings linked to this nonconformance." />
        <div className="assurarr-card">
          <div className="assurarr-card-inner space-y-4">
            <div>
              <p className="assurarr-label">Root cause analyses</p>
              <h3 className="text-base font-semibold text-slate-50">Track investigation method, cause category, and completion evidence</h3>
            </div>
            <div className="grid gap-3 md:grid-cols-2">
              <Field label="Title">
                <input className="assurarr-input" value={rootCauseForm.title} onChange={(event) => setRootCauseForm({ ...rootCauseForm, title: event.target.value })} />
              </Field>
              <Field label="Status">
                <select className="assurarr-select" value={rootCauseForm.status} onChange={(event) => setRootCauseForm({ ...rootCauseForm, status: event.target.value })}>
                  <option value="not_started">Not started</option>
                  <option value="in_progress">In progress</option>
                  <option value="completed">Completed</option>
                  <option value="inconclusive">Inconclusive</option>
                </select>
              </Field>
              <Field label="Method">
                <select className="assurarr-select" value={rootCauseForm.method} onChange={(event) => setRootCauseForm({ ...rootCauseForm, method: event.target.value })}>
                  <option value="manual">Manual</option>
                  <option value="five_whys">Five whys</option>
                  <option value="fishbone">Fishbone</option>
                  <option value="fault_tree">Fault tree</option>
                  <option value="unknown">Unknown</option>
                </select>
              </Field>
              <Field label="Cause category">
                <select className="assurarr-select" value={rootCauseForm.primaryCauseCategory} onChange={(event) => setRootCauseForm({ ...rootCauseForm, primaryCauseCategory: event.target.value })}>
                  <option value="people">People</option>
                  <option value="process">Process</option>
                  <option value="equipment">Equipment</option>
                  <option value="material">Material</option>
                  <option value="environment">Environment</option>
                  <option value="supplier">Supplier</option>
                  <option value="customer">Customer</option>
                  <option value="documentation">Documentation</option>
                  <option value="training">Training</option>
                  <option value="system">System</option>
                  <option value="unknown">Unknown</option>
                </select>
              </Field>
              <Field label="Source product">
                <input className="assurarr-input" value={rootCauseForm.sourceProduct} onChange={(event) => setRootCauseForm({ ...rootCauseForm, sourceProduct: event.target.value })} placeholder={nonconformance.sourceProduct ?? 'assurarr'} />
              </Field>
              <Field label="Source object ref">
                <input className="assurarr-input" value={rootCauseForm.sourceObjectRef} onChange={(event) => setRootCauseForm({ ...rootCauseForm, sourceObjectRef: event.target.value })} placeholder={nonconformance.sourceObjectRef ?? nonconformance.number} />
              </Field>
              <Field label="Affected objects">
                <textarea className="assurarr-textarea" value={rootCauseForm.affectedObjectRefs} onChange={(event) => setRootCauseForm({ ...rootCauseForm, affectedObjectRefs: event.target.value })} placeholder="One ref per line or comma-separated" />
              </Field>
              <Field label="Root cause summary">
                <textarea className="assurarr-textarea" value={rootCauseForm.rootCauseSummary} onChange={(event) => setRootCauseForm({ ...rootCauseForm, rootCauseSummary: event.target.value })} />
              </Field>
              <Field label="Contributing factors">
                <textarea className="assurarr-textarea" value={rootCauseForm.contributingFactors} onChange={(event) => setRootCauseForm({ ...rootCauseForm, contributingFactors: event.target.value })} placeholder="One factor per line or comma-separated" />
              </Field>
              <Field label="Record refs">
                <textarea className="assurarr-textarea" value={rootCauseForm.recordRefs} onChange={(event) => setRootCauseForm({ ...rootCauseForm, recordRefs: event.target.value })} placeholder="One ref per line or comma-separated" />
              </Field>
              <Field label="Evidence refs">
                <textarea className="assurarr-textarea" value={rootCauseForm.evidenceRecordRefs} onChange={(event) => setRootCauseForm({ ...rootCauseForm, evidenceRecordRefs: event.target.value })} placeholder="One ref per line or comma-separated" />
              </Field>
              <Field label="Analyzed by person id">
                <input className="assurarr-input" value={rootCauseForm.analyzedByPersonId} onChange={(event) => setRootCauseForm({ ...rootCauseForm, analyzedByPersonId: event.target.value })} placeholder="Optional UUID" />
              </Field>
              <Field label="Completed at">
                <input className="assurarr-input" type="datetime-local" value={rootCauseForm.completedAt} onChange={(event) => setRootCauseForm({ ...rootCauseForm, completedAt: event.target.value })} />
              </Field>
            </div>
            <button className="assurarr-button" type="button" onClick={() => createRootCauseMutation.mutate()} disabled={createRootCauseMutation.isPending}>
              {createRootCauseMutation.isPending ? 'Saving...' : 'Create root cause analysis'}
            </button>
            <div className="space-y-2">
              {relatedRootCauses.length > 0 ? (
                <ul className="space-y-2 text-sm text-slate-300">
                  {relatedRootCauses.map((rootCause) => (
                    <li key={rootCause.id} className="rounded-xl border border-slate-700/70 bg-slate-900/70 px-3 py-2">
                      <div className="flex flex-wrap items-center gap-2">
                        <span className="assurarr-pill">{rootCause.number}</span>
                        <span className="assurarr-pill">{rootCause.status}</span>
                        <span className="assurarr-pill">{rootCause.method}</span>
                        <span className="assurarr-pill">{rootCause.primaryCauseCategory}</span>
                      </div>
                      <p className="mt-2 text-slate-200">{rootCause.title}</p>
                      <p className="text-slate-400">{rootCause.rootCauseSummary ?? rootCause.description}</p>
                    </li>
                  ))}
                </ul>
              ) : (
                <EmptyState title="No root cause analyses yet." />
              )}
            </div>
          </div>
        </div>
        <SectionCard title="Timeline" items={timeline.map((event) => `${event.eventType} · ${new Date(event.occurredAt).toLocaleString()}`)} emptyLabel="No timeline events recorded yet." />
      </div>
    </div>
  )
}

function SectionCard({ title, items, emptyLabel }: { title: string; items: string[]; emptyLabel: string }) {
  return (
    <div className="assurarr-card">
      <div className="assurarr-card-inner space-y-3">
        <p className="assurarr-label">{title}</p>
        {items.length > 0 ? (
          <ul className="space-y-2 text-sm text-slate-300">
            {items.map((item) => (
              <li key={item} className="rounded-xl border border-slate-700/70 bg-slate-900/70 px-3 py-2">
                {item}
              </li>
            ))}
          </ul>
        ) : (
          <EmptyState title={emptyLabel} />
        )}
      </div>
    </div>
  )
}

function HoldPage() {
  const query = useRecords(['assurarr', 'holds'], assurarrApi.listHolds)
  const queryClient = useQueryClient()
  const [selectedHoldId, setSelectedHoldId] = useState('')
  const activeHoldId = selectedHoldId || query.data?.[0]?.id || ''
  const activeHold = query.data?.find((hold) => hold.id === activeHoldId) ?? null
  const [releaseForm, setReleaseForm] = useState({
    title: '',
    description: '',
    severity: 'moderate',
    releaseType: 'full',
    conditions: '',
    evidenceRecordRefs: '',
    notes: '',
  })
  const requestReleaseMutation = useMutation({
    mutationFn: async () => {
      if (!activeHold) throw new Error('Select a hold first.')
      return assurarrApi.requestHoldRelease(activeHold.id, {
        title: releaseForm.title || `Release ${activeHold.number}`,
        description: releaseForm.description || `Request release of ${activeHold.number}.`,
        severity: releaseForm.severity,
        sourceProduct: activeHold.sourceProduct || undefined,
        sourceObjectRef: activeHold.sourceObjectRef || undefined,
        affectedObjectRefs: activeHold.affectedObjectRefs,
        ownerPersonId: activeHold.ownerPersonId || undefined,
        holdRef: activeHold.number,
        releaseType: releaseForm.releaseType,
        requestedAt: new Date().toISOString(),
        evidenceRecordRefs: releaseForm.evidenceRecordRefs,
        notes: releaseForm.notes,
      })
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['assurarr'] })
    },
  })
  const approveReleaseMutation = useMutation({
    mutationFn: async () => {
      if (!activeHold) throw new Error('Select a hold first.')
      return assurarrApi.approveHoldRelease(activeHold.id, releaseForm.notes || 'Release approved.')
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['assurarr'] })
    },
  })
  const rejectReleaseMutation = useMutation({
    mutationFn: async () => {
      if (!activeHold) throw new Error('Select a hold first.')
      return assurarrApi.rejectHoldRelease(activeHold.id, releaseForm.notes || 'Release rejected.')
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['assurarr'] })
    },
  })
  return (
    <div className="assurarr-page">
      <PageHeader title="Quality holds" description="Place active restrictions on affected objects until release requirements are met." />
      <div className="assurarr-grid cols-2">
        <RecordForm
          title="Create quality hold"
          entityLabel="Hold"
          onCreate={async (body) =>
            assurarrApi.createHold({
              ...body,
              ownerPersonId: body.ownerPersonId || undefined,
              holdType: 'inventory',
              holdScope: 'full',
              holdReason: 'Needs quality review',
            })
          }
        />
        <div className="assurarr-card">
          <div className="assurarr-card-inner space-y-4">
            <div>
              <p className="assurarr-label">Manage release</p>
              <h3 className="text-base font-semibold text-slate-50">Request, approve, or reject a hold release</h3>
            </div>
            <div className="grid gap-3 md:grid-cols-2">
              <Field label="Hold">
                <select className="assurarr-select" value={activeHoldId} onChange={(event) => setSelectedHoldId(event.target.value)} disabled={!query.data?.length}>
                  {query.data?.map((hold) => (
                    <option key={hold.id} value={hold.id}>
                      {hold.number} · {hold.title}
                    </option>
                  ))}
                </select>
              </Field>
              <Field label="Release type">
                <select className="assurarr-select" value={releaseForm.releaseType} onChange={(event) => setReleaseForm({ ...releaseForm, releaseType: event.target.value })}>
                  <option value="full">Full</option>
                  <option value="partial">Partial</option>
                  <option value="conditional">Conditional</option>
                  <option value="use_as_is">Use as is</option>
                  <option value="release_after_rework">Release after rework</option>
                  <option value="release_after_sort">Release after sort</option>
                </select>
              </Field>
              <Field label="Title">
                <input className="assurarr-input" value={releaseForm.title} onChange={(event) => setReleaseForm({ ...releaseForm, title: event.target.value })} placeholder="Optional release request title" />
              </Field>
              <Field label="Severity">
                <select className="assurarr-select" value={releaseForm.severity} onChange={(event) => setReleaseForm({ ...releaseForm, severity: event.target.value })}>
                  <option value="none">None</option>
                  <option value="low">Low</option>
                  <option value="moderate">Moderate</option>
                  <option value="high">High</option>
                  <option value="critical">Critical</option>
                </select>
              </Field>
            </div>
            <Field label="Description" wide>
              <textarea className="assurarr-textarea" value={releaseForm.description} onChange={(event) => setReleaseForm({ ...releaseForm, description: event.target.value })} placeholder="Describe why release is requested." />
            </Field>
            <Field label="Evidence refs" wide>
              <textarea className="assurarr-textarea" value={releaseForm.evidenceRecordRefs} onChange={(event) => setReleaseForm({ ...releaseForm, evidenceRecordRefs: event.target.value })} placeholder="One ref per line or comma-separated" />
            </Field>
            <Field label="Notes" wide>
              <textarea className="assurarr-textarea" value={releaseForm.notes} onChange={(event) => setReleaseForm({ ...releaseForm, notes: event.target.value })} placeholder="Optional notes for approvers." />
            </Field>
            <div className="flex flex-wrap gap-3">
              <button className="assurarr-button" type="button" onClick={() => requestReleaseMutation.mutate()} disabled={requestReleaseMutation.isPending || !activeHoldId}>
                {requestReleaseMutation.isPending ? 'Requesting...' : 'Request release'}
              </button>
              <button className="assurarr-button secondary" type="button" onClick={() => approveReleaseMutation.mutate()} disabled={approveReleaseMutation.isPending || !activeHoldId}>
                {approveReleaseMutation.isPending ? 'Approving...' : 'Release hold'}
              </button>
              <button className="assurarr-button secondary" type="button" onClick={() => rejectReleaseMutation.mutate()} disabled={rejectReleaseMutation.isPending || !activeHoldId}>
                {rejectReleaseMutation.isPending ? 'Rejecting...' : 'Reject release'}
              </button>
            </div>
            {activeHold ? (
              <div className="rounded-2xl border border-cyan-500/20 bg-cyan-500/10 p-3 text-sm text-cyan-100">
                <div className="font-semibold">{activeHold.number} · {activeHold.title}</div>
                <div className="mt-1 text-cyan-50/80">
                  Status {activeHold.status}. Release requirements {activeHold.releaseRequirements.length ? activeHold.releaseRequirements.length : 'not set'}.
                </div>
              </div>
            ) : null}
          </div>
        </div>
      </div>
      {query.data ? (
        <EntityTable
          items={query.data}
          emptyLabel="No quality holds yet."
          onStatusChange={(id, status) => assurarrApi.updateHoldStatus(id, status)}
          statusChoices={statusOptions.hold}
          detailBasePath="/holds"
        />
      ) : (
        <LoadingCard label="Loading holds" />
      )}
    </div>
  )
}

function HoldDetailPage() {
  const { id = '' } = useParams()
  const query = useQuery({
    queryKey: ['assurarr', 'hold', id],
    queryFn: () => assurarrApi.getHold(id),
    enabled: Boolean(id),
  })
  const releases = useRecords(['assurarr', 'releases'], assurarrApi.listQualityReleases)
  const dashboard = useDashboard()

  if (query.isLoading) {
    return <LoadingCard label="Loading hold detail" />
  }

  if (query.isError || !query.data) {
    return (
      <div className="assurarr-page">
        <PageHeader title="Hold detail" description="Could not load the requested quality hold." />
        <EmptyState title="Quality hold not found." />
      </div>
    )
  }

  const hold = query.data
  const relatedReleases = releases.data?.filter((release) => release.holdRef === hold.number) ?? []
  const timeline = dashboard.data?.recentEvents.filter((event) => event.subjectType === 'hold' && event.subjectId === hold.id) ?? []

  return (
    <div className="assurarr-page">
      <PageHeader
        title={`${hold.number} · ${hold.title}`}
        description="Hold detail, release requirements, release evidence, and the downstream records blocked by this quality decision."
      />
      <div className="space-y-4">
        <div className="assurarr-grid cols-2">
          <div className="assurarr-card">
            <div className="assurarr-card-inner space-y-3">
              <p className="assurarr-label">Overview</p>
              <div className="flex flex-wrap gap-2 text-sm">
                <span className="assurarr-pill">{hold.status}</span>
                <span className="assurarr-pill">{hold.severity}</span>
                <span className="assurarr-pill">{hold.holdType}</span>
                <span className="assurarr-pill">{hold.holdScope}</span>
              </div>
              <p className="text-sm text-slate-300">{hold.description}</p>
              <div className="grid gap-2 text-sm text-slate-300 md:grid-cols-2">
                <div><span className="text-slate-500">Source product:</span> {hold.sourceProduct ?? 'manual'}</div>
                <div><span className="text-slate-500">Source object:</span> {hold.sourceObjectRef ?? 'n/a'}</div>
                <div><span className="text-slate-500">Owner:</span> {hold.ownerPersonId ?? 'unassigned'}</div>
                <div><span className="text-slate-500">Placed:</span> {hold.placedAt ? new Date(hold.placedAt).toLocaleString() : 'n/a'}</div>
                <div><span className="text-slate-500">Released:</span> {hold.releasedAt ? new Date(hold.releasedAt).toLocaleString() : 'n/a'}</div>
                <div><span className="text-slate-500">Rejected:</span> {hold.rejectedAt ? new Date(hold.rejectedAt).toLocaleString() : 'n/a'}</div>
              </div>
            </div>
          </div>
          <div className="assurarr-card">
            <div className="assurarr-card-inner space-y-3">
              <p className="assurarr-label">Blocking context</p>
              <div className="flex flex-wrap gap-2 text-xs text-slate-200">
                {hold.affectedObjectRefs.length > 0 ? hold.affectedObjectRefs.map((ref) => <span key={ref} className="assurarr-pill">{ref}</span>) : <span className="assurarr-pill">No affected objects</span>}
              </div>
              <div className="text-sm text-slate-300">
                <div><span className="text-slate-500">Quantity held:</span> {hold.quantityHeld ?? 'n/a'} {hold.unitOfMeasure ?? ''}</div>
                <div><span className="text-slate-500">Lot:</span> {hold.lotNumber ?? 'n/a'}</div>
                <div><span className="text-slate-500">Serial:</span> {hold.serialNumber ?? 'n/a'}</div>
                <div><span className="text-slate-500">Hold reason:</span> {hold.holdReason ?? 'n/a'}</div>
                <div><span className="text-slate-500">Release reason:</span> {hold.releaseReason ?? 'n/a'}</div>
                <div><span className="text-slate-500">Rejection reason:</span> {hold.rejectionReason ?? 'n/a'}</div>
              </div>
            </div>
          </div>
        </div>

        <SectionCard
          title="Release requirements"
          items={hold.releaseRequirements.map((item) => item)}
          emptyLabel="No release requirements have been captured yet."
        />
        <SectionCard
          title="Release approvals"
          items={hold.releaseApprovalRefs.map((item) => item)}
          emptyLabel="No release approvals have been captured yet."
        />
        <SectionCard
          title="Evidence"
          items={hold.recordRefs.map((item) => item)}
          emptyLabel="No evidence records are linked to this hold."
        />
        <SectionCard
          title="Related quality releases"
          items={relatedReleases.map((release) => `${release.number} · ${release.status} · ${release.releaseType}${release.evidenceRecordRefs.length ? ` · evidence ${release.evidenceRecordRefs.length}` : ''}`)}
          emptyLabel="No quality release requests are linked to this hold."
        />
        <SectionCard
          title="Timeline"
          items={timeline.map((event) => `${event.eventType} · ${new Date(event.occurredAt).toLocaleString()}`)}
          emptyLabel="No timeline events recorded yet."
        />
      </div>
    </div>
  )
}

function CapaPage() {
  const query = useRecords(['assurarr', 'capas'], assurarrApi.listCapas)
  const queryClient = useQueryClient()
  const [selectedCapaId, setSelectedCapaId] = useState('')
  const activeCapaId = selectedCapaId || query.data?.[0]?.id || ''
  const [selectedActionId, setSelectedActionId] = useState('')
  const [blockerForm, setBlockerForm] = useState({
    blockerType: 'waiting_supplier',
    sourceProduct: 'supplyarr',
    sourceObjectRef: '',
    title: '',
    description: '',
  })
  const [actionForm, setActionForm] = useState({
    title: '',
    description: '',
    actionType: 'update_work_instruction',
    assignedPersonId: '',
    assignedTeamRef: '',
    sourceProductActionRef: '',
    targetProduct: 'manual',
    targetObjectRef: '',
    dueAt: '',
    verificationRequired: true,
    evidenceRecordRefs: '',
    blockerRefs: '',
    notes: '',
  })
  const [verificationForm, setVerificationForm] = useState({
    title: '',
    description: '',
    verificationType: 'audit',
    successCriteria: '',
    sampleSize: '5',
    observationPeriodDays: '14',
    requiredEvidenceTypes: '',
    responsiblePersonId: '',
    plannedVerificationAt: '',
  })
  const actionQuery = useQuery({
    queryKey: ['assurarr', 'capa-actions', activeCapaId],
    queryFn: () => assurarrApi.listCapaActions(activeCapaId),
    enabled: Boolean(activeCapaId),
    staleTime: 15_000,
  })
  const activeActionId = selectedActionId || actionQuery.data?.[0]?.id || ''
  useEffect(() => {
    setSelectedActionId('')
  }, [activeCapaId])
  const blockerQuery = useQuery({
    queryKey: ['assurarr', 'capa-action-blockers', activeCapaId, activeActionId],
    queryFn: () => assurarrApi.listCapaActionBlockers(activeCapaId, activeActionId),
    enabled: Boolean(activeCapaId && activeActionId),
    staleTime: 15_000,
  })
  const verificationQuery = useQuery({
    queryKey: ['assurarr', 'verification-plans', activeCapaId],
    queryFn: () => assurarrApi.listVerificationPlans(activeCapaId),
    enabled: Boolean(activeCapaId),
    staleTime: 15_000,
  })
  const createActionMutation = useMutation({
    mutationFn: async () =>
      assurarrApi.createCapaAction(activeCapaId, {
        title: actionForm.title,
        description: actionForm.description,
        actionType: actionForm.actionType,
        assignedPersonId: actionForm.assignedPersonId || undefined,
        assignedTeamRef: actionForm.assignedTeamRef || undefined,
        sourceProductActionRef: actionForm.sourceProductActionRef || undefined,
        targetProduct: actionForm.targetProduct,
        targetObjectRef: actionForm.targetObjectRef || undefined,
        dueAt: actionForm.dueAt || undefined,
        verificationRequired: actionForm.verificationRequired,
        evidenceRecordRefs: joinRefs(actionForm.evidenceRecordRefs),
        blockerRefs: joinRefs(actionForm.blockerRefs),
        notes: actionForm.notes || undefined,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['assurarr'] })
      setActionForm({
        title: '',
        description: '',
        actionType: 'update_work_instruction',
        assignedPersonId: '',
        assignedTeamRef: '',
        sourceProductActionRef: '',
        targetProduct: 'manual',
        targetObjectRef: '',
        dueAt: '',
        verificationRequired: true,
        evidenceRecordRefs: '',
        blockerRefs: '',
        notes: '',
      })
    },
  })
  const createBlockerMutation = useMutation({
    mutationFn: async () =>
      assurarrApi.createCapaActionBlocker(activeCapaId, activeActionId, {
        blockerType: blockerForm.blockerType,
        sourceProduct: blockerForm.sourceProduct,
        sourceObjectRef: blockerForm.sourceObjectRef || undefined,
        title: blockerForm.title,
        description: blockerForm.description,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['assurarr'] })
      setBlockerForm({
        blockerType: 'waiting_supplier',
        sourceProduct: 'supplyarr',
        sourceObjectRef: '',
        title: '',
        description: '',
      })
    },
  })
  const resolveBlockerMutation = useMutation({
    mutationFn: async ({ blockerId, status }: { blockerId: string; status: string }) =>
      assurarrApi.updateCapaActionBlockerStatus(activeCapaId, activeActionId, blockerId, status, undefined, new Date().toISOString()),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['assurarr'] })
    },
  })
  const createVerificationMutation = useMutation({
    mutationFn: async () =>
      assurarrApi.createVerificationPlan(activeCapaId, {
        title: verificationForm.title,
        description: verificationForm.description,
        verificationType: verificationForm.verificationType,
        successCriteria: verificationForm.successCriteria,
        sampleSize: verificationForm.sampleSize ? Number.parseInt(verificationForm.sampleSize, 10) : undefined,
        observationPeriodDays: verificationForm.observationPeriodDays ? Number.parseInt(verificationForm.observationPeriodDays, 10) : undefined,
        requiredEvidenceTypes: joinRefs(verificationForm.requiredEvidenceTypes),
        responsiblePersonId: verificationForm.responsiblePersonId || undefined,
        plannedVerificationAt: verificationForm.plannedVerificationAt || undefined,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['assurarr'] })
      setVerificationForm({
        title: '',
        description: '',
        verificationType: 'audit',
        successCriteria: '',
        sampleSize: '5',
        observationPeriodDays: '14',
        requiredEvidenceTypes: '',
        responsiblePersonId: '',
        plannedVerificationAt: '',
      })
    },
  })
  return (
    <div className="assurarr-page">
      <PageHeader title="CAPA" description="Track root cause, action plans, implementation, verification, and closeout." />
      <RecordForm
        title="Create CAPA"
        entityLabel="CAPA"
        onCreate={async (body) =>
          assurarrApi.createCapa({
            ...body,
            ownerPersonId: body.ownerPersonId || undefined,
            capaType: 'corrective_and_preventive',
            sourceType: 'manual',
            rootCauseSummary: 'Awaiting analysis',
          })
        }
      />
      <div className="assurarr-grid cols-2">
        <div className="assurarr-card">
          <div className="assurarr-card-inner space-y-4">
            <div className="flex items-center gap-2">
              <ListTodo className="h-4 w-4 text-cyan-300" />
              <h3 className="text-base font-semibold text-slate-50">CAPA action plan</h3>
            </div>
            <div className="grid gap-3 md:grid-cols-2">
              <Field label="CAPA">
                <select className="assurarr-select" value={activeCapaId} onChange={(event) => setSelectedCapaId(event.target.value)}>
                  {query.data?.map((capa) => (
                    <option key={capa.id} value={capa.id}>
                      {capa.number} - {capa.title}
                    </option>
                  ))}
                </select>
              </Field>
              <Field label="Action type">
                <select className="assurarr-select" value={actionForm.actionType} onChange={(event) => setActionForm({ ...actionForm, actionType: event.target.value })}>
                  <option value="update_procedure">Update procedure</option>
                  <option value="retrain_personnel">Retrain personnel</option>
                  <option value="repair_asset">Repair asset</option>
                  <option value="change_supplier">Change supplier</option>
                  <option value="update_inspection">Update inspection</option>
                  <option value="update_pm">Update PM</option>
                  <option value="update_work_instruction">Update work instruction</option>
                  <option value="update_document">Update document</option>
                  <option value="quarantine_inventory">Quarantine inventory</option>
                  <option value="rework_inventory">Rework inventory</option>
                  <option value="customer_notification">Customer notification</option>
                  <option value="supplier_corrective_action">Supplier corrective action</option>
                  <option value="system_change">System change</option>
                  <option value="process_change">Process change</option>
                  <option value="audit_followup">Audit followup</option>
                  <option value="other">Other</option>
                </select>
              </Field>
              <Field label="Target product">
                <select className="assurarr-select" value={actionForm.targetProduct} onChange={(event) => setActionForm({ ...actionForm, targetProduct: event.target.value })}>
                  <option value="manual">Manual</option>
                  <option value="staffarr">StaffArr</option>
                  <option value="trainarr">TrainArr</option>
                  <option value="maintainarr">MaintainArr</option>
                  <option value="loadarr">LoadArr</option>
                  <option value="supplyarr">SupplyArr</option>
                  <option value="routarr">RoutArr</option>
                  <option value="customarr">CustomArr</option>
                  <option value="ordarr">OrdArr</option>
                  <option value="recordarr">RecordArr</option>
                  <option value="compliancecore">Compliance Core</option>
                </select>
              </Field>
              <Field label="Due at">
                <input className="assurarr-input" type="datetime-local" value={actionForm.dueAt} onChange={(event) => setActionForm({ ...actionForm, dueAt: event.target.value })} />
              </Field>
              <Field label="Title" wide>
                <input className="assurarr-input" value={actionForm.title} onChange={(event) => setActionForm({ ...actionForm, title: event.target.value })} />
              </Field>
              <Field label="Description" wide>
                <textarea className="assurarr-textarea" value={actionForm.description} onChange={(event) => setActionForm({ ...actionForm, description: event.target.value })} />
              </Field>
              <Field label="Target object ref">
                <input className="assurarr-input" value={actionForm.targetObjectRef} onChange={(event) => setActionForm({ ...actionForm, targetObjectRef: event.target.value })} />
              </Field>
              <Field label="Assigned team ref">
                <input className="assurarr-input" value={actionForm.assignedTeamRef} onChange={(event) => setActionForm({ ...actionForm, assignedTeamRef: event.target.value })} />
              </Field>
              <Field label="Evidence refs" wide>
                <textarea className="assurarr-textarea" value={actionForm.evidenceRecordRefs} onChange={(event) => setActionForm({ ...actionForm, evidenceRecordRefs: event.target.value })} />
              </Field>
              <Field label="Blocker refs" wide>
                <textarea className="assurarr-textarea" value={actionForm.blockerRefs} onChange={(event) => setActionForm({ ...actionForm, blockerRefs: event.target.value })} />
              </Field>
              <Field label="Notes" wide>
                <textarea className="assurarr-textarea" value={actionForm.notes} onChange={(event) => setActionForm({ ...actionForm, notes: event.target.value })} />
              </Field>
            </div>
            <button className="assurarr-button" type="button" onClick={() => createActionMutation.mutate()} disabled={createActionMutation.isPending || !activeCapaId}>
              {createActionMutation.isPending ? 'Saving...' : 'Create CAPA action'}
            </button>
            <div className="grid gap-3 md:grid-cols-2">
              <Field label="Active action">
                <select className="assurarr-select" value={activeActionId} onChange={(event) => setSelectedActionId(event.target.value)} disabled={!actionQuery.data?.length}>
                  {actionQuery.data?.map((action) => (
                    <option key={action.id} value={action.id}>
                      {action.number} - {action.title}
                    </option>
                  ))}
                </select>
              </Field>
            </div>
            {actionQuery.data?.length ? (
              <div className="space-y-3">
                {actionQuery.data.map((action) => (
                  <button
                    key={action.id}
                    type="button"
                    className={`w-full rounded-xl border p-3 text-left transition ${action.id === activeActionId ? 'border-cyan-500/70 bg-slate-900/90' : 'border-slate-700/70 bg-slate-900/80'}`}
                    onClick={() => setSelectedActionId(action.id)}
                  >
                    <div className="flex items-center justify-between gap-3">
                      <strong className="text-sm text-slate-50">{action.number}</strong>
                      <span className="assurarr-pill">{action.status}</span>
                    </div>
                    <p className="mt-1 text-sm text-slate-300">{action.title}</p>
                    <p className="mt-1 text-xs text-slate-400">{action.actionType} - {action.targetProduct}</p>
                    <p className="mt-1 text-xs text-slate-400">
                      Blockers: {action.blockerRefs.length > 0 ? action.blockerRefs.join(', ') : 'none'}
                    </p>
                  </button>
                ))}
              </div>
            ) : (
              <EmptyState title={activeCapaId ? 'No CAPA actions yet.' : 'Select a CAPA first.'} />
            )}
            <div className="rounded-2xl border border-slate-700/70 bg-slate-900/80 p-4">
              <div className="flex items-center gap-2">
                <ShieldAlert className="h-4 w-4 text-cyan-300" />
                <h4 className="text-sm font-semibold text-slate-50">Blockers for selected action</h4>
              </div>
            <div className="mt-3 space-y-3">
                <div className="grid gap-3 md:grid-cols-2">
                  <Field label="Blocker type">
                    <select className="assurarr-select" value={blockerForm.blockerType} onChange={(event) => setBlockerForm({ ...blockerForm, blockerType: event.target.value })}>
                      <option value="missing_approval">Missing approval</option>
                      <option value="missing_evidence">Missing evidence</option>
                      <option value="waiting_training">Waiting training</option>
                      <option value="waiting_maintenance">Waiting maintenance</option>
                      <option value="waiting_supplier">Waiting supplier</option>
                      <option value="waiting_customer">Waiting customer</option>
                      <option value="waiting_inventory">Waiting inventory</option>
                      <option value="waiting_document">Waiting document</option>
                      <option value="system">System</option>
                      <option value="other">Other</option>
                    </select>
                  </Field>
                  <Field label="Source object ref">
                    <input className="assurarr-input" value={blockerForm.sourceObjectRef} onChange={(event) => setBlockerForm({ ...blockerForm, sourceObjectRef: event.target.value })} />
                  </Field>
                  <Field label="Source product">
                    <input className="assurarr-input" value={blockerForm.sourceProduct} onChange={(event) => setBlockerForm({ ...blockerForm, sourceProduct: event.target.value })} />
                  </Field>
                  <Field label="Title" wide>
                    <input className="assurarr-input" value={blockerForm.title} onChange={(event) => setBlockerForm({ ...blockerForm, title: event.target.value })} />
                  </Field>
                  <Field label="Description" wide>
                    <textarea className="assurarr-textarea" value={blockerForm.description} onChange={(event) => setBlockerForm({ ...blockerForm, description: event.target.value })} />
                  </Field>
                </div>
                <button className="assurarr-button" type="button" onClick={() => createBlockerMutation.mutate()} disabled={createBlockerMutation.isPending || !activeActionId}>
                  {createBlockerMutation.isPending ? 'Saving...' : 'Add blocker'}
                </button>
                {blockerQuery.data?.length ? (
                  <div className="space-y-2">
                    {blockerQuery.data.map((blocker) => (
                      <div key={blocker.id} className="rounded-xl border border-slate-700/70 bg-slate-950/60 p-3">
                        <div className="flex items-center justify-between gap-3">
                          <strong className="text-sm text-slate-50">{blocker.number}</strong>
                          <span className="assurarr-pill">{blocker.status}</span>
                        </div>
                        <p className="mt-1 text-sm text-slate-300">{blocker.title}</p>
                        <p className="mt-1 text-xs text-slate-400">{blocker.blockerType}{blocker.sourceProduct ? ` - ${blocker.sourceProduct}` : ''}</p>
                        <div className="mt-3 flex flex-wrap gap-2">
                          <button className="assurarr-button secondary" type="button" onClick={() => resolveBlockerMutation.mutate({ blockerId: blocker.id, status: 'resolved' })} disabled={resolveBlockerMutation.isPending || blocker.status !== 'active'}>
                            Mark resolved
                          </button>
                          <button className="assurarr-button secondary" type="button" onClick={() => resolveBlockerMutation.mutate({ blockerId: blocker.id, status: 'overridden' })} disabled={resolveBlockerMutation.isPending || blocker.status !== 'active'}>
                            Override
                          </button>
                        </div>
                      </div>
                    ))}
                  </div>
                ) : (
                  <EmptyState title={activeActionId ? 'No blockers for the selected action.' : 'Select an action first.'} />
                )}
              </div>
            </div>
          </div>
        </div>

        <div className="assurarr-card">
          <div className="assurarr-card-inner space-y-4">
            <div className="flex items-center gap-2">
              <ShieldCheck className="h-4 w-4 text-cyan-300" />
              <h3 className="text-base font-semibold text-slate-50">Verification plan</h3>
            </div>
            <div className="grid gap-3 md:grid-cols-2">
              <Field label="Verification type">
                <select className="assurarr-select" value={verificationForm.verificationType} onChange={(event) => setVerificationForm({ ...verificationForm, verificationType: event.target.value })}>
                  <option value="observation">Observation</option>
                  <option value="audit">Audit</option>
                  <option value="inspection">Inspection</option>
                  <option value="trend_review">Trend review</option>
                  <option value="sample_review">Sample review</option>
                  <option value="customer_confirmation">Customer confirmation</option>
                  <option value="supplier_confirmation">Supplier confirmation</option>
                  <option value="document_review">Document review</option>
                  <option value="training_completion_review">Training completion review</option>
                </select>
              </Field>
              <Field label="Planned at">
                <input className="assurarr-input" type="datetime-local" value={verificationForm.plannedVerificationAt} onChange={(event) => setVerificationForm({ ...verificationForm, plannedVerificationAt: event.target.value })} />
              </Field>
              <Field label="Title" wide>
                <input className="assurarr-input" value={verificationForm.title} onChange={(event) => setVerificationForm({ ...verificationForm, title: event.target.value })} />
              </Field>
              <Field label="Success criteria" wide>
                <textarea className="assurarr-textarea" value={verificationForm.successCriteria} onChange={(event) => setVerificationForm({ ...verificationForm, successCriteria: event.target.value })} />
              </Field>
              <Field label="Description" wide>
                <textarea className="assurarr-textarea" value={verificationForm.description} onChange={(event) => setVerificationForm({ ...verificationForm, description: event.target.value })} />
              </Field>
              <Field label="Sample size">
                <input className="assurarr-input" value={verificationForm.sampleSize} onChange={(event) => setVerificationForm({ ...verificationForm, sampleSize: event.target.value })} />
              </Field>
              <Field label="Observation days">
                <input className="assurarr-input" value={verificationForm.observationPeriodDays} onChange={(event) => setVerificationForm({ ...verificationForm, observationPeriodDays: event.target.value })} />
              </Field>
              <Field label="Required evidence types" wide>
                <textarea className="assurarr-textarea" value={verificationForm.requiredEvidenceTypes} onChange={(event) => setVerificationForm({ ...verificationForm, requiredEvidenceTypes: event.target.value })} />
              </Field>
            </div>
            <button className="assurarr-button" type="button" onClick={() => createVerificationMutation.mutate()} disabled={createVerificationMutation.isPending || !activeCapaId}>
              {createVerificationMutation.isPending ? 'Saving...' : 'Create verification plan'}
            </button>
            {verificationQuery.data?.length ? (
              <div className="space-y-3">
                {verificationQuery.data.map((plan) => (
                  <div key={plan.id} className="rounded-xl border border-slate-700/70 bg-slate-900/80 p-3">
                    <div className="flex items-center justify-between gap-3">
                      <strong className="text-sm text-slate-50">{plan.number}</strong>
                      <span className="assurarr-pill">{plan.status}</span>
                    </div>
                    <p className="mt-1 text-sm text-slate-300">{plan.title}</p>
                    <p className="mt-1 text-xs text-slate-400">{plan.verificationType}</p>
                  </div>
                ))}
              </div>
            ) : (
              <EmptyState title={activeCapaId ? 'No verification plans yet.' : 'Select a CAPA first.'} />
            )}
          </div>
        </div>
      </div>
      {query.data ? (
        <EntityTable
          items={query.data}
          emptyLabel="No CAPA records yet."
          onStatusChange={(id, status) => assurarrApi.updateCapaStatus(id, status)}
          statusChoices={statusOptions.capa}
          detailBasePath="/capa"
        />
      ) : (
        <LoadingCard label="Loading CAPA" />
      )}
    </div>
  )
}

function CapaDetailPage() {
  const { id = '' } = useParams()
  const queryClient = useQueryClient()
  const query = useQuery({
    queryKey: ['assurarr', 'capa', id],
    queryFn: () => assurarrApi.getCapa(id),
    enabled: Boolean(id),
  })
  const actionQuery = useRecords(['assurarr', 'capa-actions', id], () => assurarrApi.listCapaActions(id))
  const verificationQuery = useRecords(['assurarr', 'verification-plans', id], () => assurarrApi.listVerificationPlans(id))
  const effectivenessQuery = useRecords(['assurarr', 'effectiveness-verifications', id], () => assurarrApi.listEffectivenessVerifications(id))
  const nonconformanceQuery = useRecords(['assurarr', 'nonconformances'], assurarrApi.listNonconformances)
  const findingQuery = useRecords(['assurarr', 'findings'], assurarrApi.listFindings)
  const dashboard = useDashboard()
  const [selectedActionId, setSelectedActionId] = useState('')
  const [effectivenessForm, setEffectivenessForm] = useState({
    verificationPlanId: '',
    status: 'scheduled',
    performedByPersonId: '',
    performedAt: '',
    resultSummary: '',
    evidenceRecordRefs: '',
    metricResults: '',
    recurrenceFound: false,
    followUpRequired: false,
    reopenedCapaRef: '',
  })

  useEffect(() => {
    setSelectedActionId('')
  }, [id])

  const activeActionId = selectedActionId || actionQuery.data?.[0]?.id || ''
  const blockerQuery = useQuery({
    queryKey: ['assurarr', 'capa-action-blockers', id, activeActionId],
    queryFn: () => assurarrApi.listCapaActionBlockers(id, activeActionId),
    enabled: Boolean(id && activeActionId),
    staleTime: 15_000,
  })
  const createEffectivenessMutation = useMutation({
    mutationFn: async () =>
      assurarrApi.createEffectivenessVerification(id, {
        verificationPlanId: effectivenessForm.verificationPlanId || undefined,
        status: effectivenessForm.status,
        performedByPersonId: effectivenessForm.performedByPersonId || undefined,
        performedAt: effectivenessForm.performedAt || undefined,
        resultSummary: effectivenessForm.resultSummary || undefined,
        evidenceRecordRefs: joinRefs(effectivenessForm.evidenceRecordRefs),
        metricResults: joinRefs(effectivenessForm.metricResults),
        recurrenceFound: effectivenessForm.recurrenceFound,
        followUpRequired: effectivenessForm.followUpRequired,
        reopenedCapaRef: effectivenessForm.reopenedCapaRef || undefined,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['assurarr'] })
      setEffectivenessForm({
        verificationPlanId: '',
        status: 'scheduled',
        performedByPersonId: '',
        performedAt: '',
        resultSummary: '',
        evidenceRecordRefs: '',
        metricResults: '',
        recurrenceFound: false,
        followUpRequired: false,
        reopenedCapaRef: '',
      })
    },
  })
  const updateEffectivenessMutation = useMutation({
    mutationFn: async ({ verificationId, status }: { verificationId: string; status: string }) =>
      assurarrApi.updateEffectivenessVerificationStatus(id, verificationId, status, {
        resultSummary: status === 'effective' ? 'Verified effective.' : status === 'ineffective' ? 'Verification found an issue.' : undefined,
        recurrenceFound: status === 'ineffective',
        followUpRequired: status === 'ineffective',
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['assurarr'] })
    },
  })

  if (query.isLoading) {
    return <LoadingCard label="Loading CAPA detail" />
  }

  if (query.isError || !query.data) {
    return (
      <div className="assurarr-page">
        <PageHeader title="CAPA detail" description="Could not load the requested corrective action plan." />
        <EmptyState title="CAPA not found." />
      </div>
    )
  }

  const capa = query.data
  const relatedNonconformances = nonconformanceQuery.data?.filter((item) => capa.relatedNonconformanceRefs.includes(item.number)) ?? []
  const relatedFindings = findingQuery.data?.filter((item) => capa.relatedAuditFindingRefs.includes(item.number) || item.capaRef === capa.number) ?? []
  const currentEffectivenessVerification = effectivenessQuery.data?.[0] ?? null
  const timeline = dashboard.data?.recentEvents.filter((event) => event.subjectType === 'capa' && event.subjectId === capa.id) ?? []
  const currentAction = actionQuery.data?.find((action) => action.id === activeActionId) ?? null

  return (
    <div className="assurarr-page">
      <PageHeader
        title={`${capa.number} · ${capa.title}`}
        description="Root cause, action plan, assigned actions, blockers, verification plans, and the evidence trail for the CAPA."
      />
      <div className="space-y-4">
        <div className="assurarr-grid cols-2">
          <div className="assurarr-card">
            <div className="assurarr-card-inner space-y-3">
              <p className="assurarr-label">Overview</p>
              <div className="flex flex-wrap gap-2 text-sm">
                <span className="assurarr-pill">{capa.status}</span>
                <span className="assurarr-pill">{capa.severity}</span>
                <span className="assurarr-pill">{capa.capaType}</span>
                <span className="assurarr-pill">{capa.sourceType}</span>
              </div>
              <p className="text-sm text-slate-300">{capa.description}</p>
              <div className="grid gap-2 text-sm text-slate-300 md:grid-cols-2">
                <div><span className="text-slate-500">Source product:</span> {capa.sourceProduct ?? 'manual'}</div>
                <div><span className="text-slate-500">Source object:</span> {capa.sourceObjectRef ?? 'n/a'}</div>
                <div><span className="text-slate-500">Owner:</span> {capa.ownerPersonId ?? 'unassigned'}</div>
                <div><span className="text-slate-500">Sponsor:</span> {capa.sponsorPersonId ?? 'n/a'}</div>
                <div><span className="text-slate-500">Due:</span> {capa.dueAt ? new Date(capa.dueAt).toLocaleString() : 'n/a'}</div>
                <div><span className="text-slate-500">Closed:</span> {capa.closedAt ? new Date(capa.closedAt).toLocaleString() : 'n/a'}</div>
              </div>
            </div>
          </div>
          <div className="assurarr-card">
            <div className="assurarr-card-inner space-y-3">
              <p className="assurarr-label">Root cause and sources</p>
              <div className="text-sm text-slate-300">
                <div><span className="text-slate-500">Root cause:</span> {capa.rootCauseSummary ?? 'Awaiting analysis'}</div>
                <div><span className="text-slate-500">Source refs:</span> {capa.recordRefs.length ? capa.recordRefs.join(', ') : 'none'}</div>
                <div><span className="text-slate-500">Related nonconformances:</span> {capa.relatedNonconformanceRefs.length ? capa.relatedNonconformanceRefs.join(', ') : 'none'}</div>
                <div><span className="text-slate-500">Related findings:</span> {capa.relatedAuditFindingRefs.length ? capa.relatedAuditFindingRefs.join(', ') : 'none'}</div>
                <div><span className="text-slate-500">Effectiveness refs:</span> {capa.effectivenessVerificationRefs.length ? capa.effectivenessVerificationRefs.join(', ') : 'none'}</div>
              </div>
            </div>
          </div>
        </div>

        <div className="assurarr-grid cols-2">
          <div className="assurarr-card">
            <div className="assurarr-card-inner space-y-4">
              <p className="assurarr-label">Assigned actions</p>
              {actionQuery.data?.length ? (
                <div className="space-y-2">
                  {actionQuery.data.map((action) => (
                    <button
                      key={action.id}
                      type="button"
                      className={`w-full rounded-xl border p-3 text-left transition ${action.id === activeActionId ? 'border-cyan-500/70 bg-slate-900/90' : 'border-slate-700/70 bg-slate-900/80'}`}
                      onClick={() => setSelectedActionId(action.id)}
                    >
                      <div className="flex items-center justify-between gap-3">
                        <strong className="text-sm text-slate-50">{action.number}</strong>
                        <span className="assurarr-pill">{action.status}</span>
                      </div>
                      <p className="mt-1 text-sm text-slate-300">{action.title}</p>
                      <p className="mt-1 text-xs text-slate-400">
                        {action.actionType} - {action.targetProduct}
                      </p>
                      <p className="mt-1 text-xs text-slate-400">
                        Blockers: {action.blockerRefs.length > 0 ? action.blockerRefs.join(', ') : 'none'}
                      </p>
                    </button>
                  ))}
                </div>
              ) : (
                <EmptyState title="No CAPA actions yet." />
              )}
            </div>
          </div>
          <div className="assurarr-card">
            <div className="assurarr-card-inner space-y-4">
              <p className="assurarr-label">Blockers and verification</p>
              <div className="grid gap-3">
                <div className="rounded-xl border border-slate-700/70 bg-slate-950/60 p-3">
                  <div className="flex items-center justify-between gap-3">
                    <strong className="text-sm text-slate-50">Selected action</strong>
                    <span className="assurarr-pill">{currentAction?.status ?? 'none'}</span>
                  </div>
                  <p className="mt-1 text-sm text-slate-300">{currentAction ? currentAction.title : 'Select an action to see its blockers.'}</p>
                </div>
                {blockerQuery.data?.length ? (
                  <div className="space-y-2">
                    {blockerQuery.data.map((blocker) => (
                      <div key={blocker.id} className="rounded-xl border border-slate-700/70 bg-slate-950/60 p-3">
                        <div className="flex items-center justify-between gap-3">
                          <strong className="text-sm text-slate-50">{blocker.number}</strong>
                          <span className="assurarr-pill">{blocker.status}</span>
                        </div>
                        <p className="mt-1 text-sm text-slate-300">{blocker.title}</p>
                        <p className="mt-1 text-xs text-slate-400">{blocker.blockerType}{blocker.sourceProduct ? ` - ${blocker.sourceProduct}` : ''}</p>
                      </div>
                    ))}
                  </div>
                ) : (
                  <EmptyState title={activeActionId ? 'No blockers for the selected action.' : 'Select an action first.'} />
                )}
                {verificationQuery.data?.length ? (
                  <div className="space-y-2">
                    {verificationQuery.data.map((plan) => (
                      <div key={plan.id} className="rounded-xl border border-slate-700/70 bg-slate-950/60 p-3">
                        <div className="flex items-center justify-between gap-3">
                          <strong className="text-sm text-slate-50">{plan.number}</strong>
                          <span className="assurarr-pill">{plan.status}</span>
                        </div>
                        <p className="mt-1 text-sm text-slate-300">{plan.title}</p>
                        <p className="mt-1 text-xs text-slate-400">{plan.verificationType}</p>
                        <p className="mt-1 text-xs text-slate-400">{plan.successCriteria}</p>
                      </div>
                    ))}
                  </div>
                ) : (
                  <EmptyState title="No verification plans yet." />
                )}
                <div className="rounded-xl border border-slate-700/70 bg-slate-950/60 p-3">
                  <p className="assurarr-label">Effectiveness verification</p>
                  <div className="grid gap-3 md:grid-cols-2">
                    <Field label="Plan">
                      <select className="assurarr-select" value={effectivenessForm.verificationPlanId} onChange={(event) => setEffectivenessForm({ ...effectivenessForm, verificationPlanId: event.target.value })}>
                        <option value="">No plan selected</option>
                        {verificationQuery.data?.map((plan) => (
                          <option key={plan.id} value={plan.id}>
                            {plan.number} · {plan.title}
                          </option>
                        ))}
                      </select>
                    </Field>
                    <Field label="Status">
                      <select className="assurarr-select" value={effectivenessForm.status} onChange={(event) => setEffectivenessForm({ ...effectivenessForm, status: event.target.value })}>
                        <option value="scheduled">Scheduled</option>
                        <option value="in_progress">In progress</option>
                        <option value="effective">Effective</option>
                        <option value="ineffective">Ineffective</option>
                        <option value="inconclusive">Inconclusive</option>
                        <option value="canceled">Canceled</option>
                      </select>
                    </Field>
                    <Field label="Performed by">
                      <input className="assurarr-input" value={effectivenessForm.performedByPersonId} onChange={(event) => setEffectivenessForm({ ...effectivenessForm, performedByPersonId: event.target.value })} placeholder="Optional UUID" />
                    </Field>
                    <Field label="Performed at">
                      <input className="assurarr-input" type="datetime-local" value={effectivenessForm.performedAt} onChange={(event) => setEffectivenessForm({ ...effectivenessForm, performedAt: event.target.value })} />
                    </Field>
                    <Field label="Result summary" wide>
                      <textarea className="assurarr-textarea" value={effectivenessForm.resultSummary} onChange={(event) => setEffectivenessForm({ ...effectivenessForm, resultSummary: event.target.value })} />
                    </Field>
                    <Field label="Evidence refs" wide>
                      <textarea className="assurarr-textarea" value={effectivenessForm.evidenceRecordRefs} onChange={(event) => setEffectivenessForm({ ...effectivenessForm, evidenceRecordRefs: event.target.value })} />
                    </Field>
                    <Field label="Metric results" wide>
                      <textarea className="assurarr-textarea" value={effectivenessForm.metricResults} onChange={(event) => setEffectivenessForm({ ...effectivenessForm, metricResults: event.target.value })} />
                    </Field>
                    <Field label="Reopened CAPA ref">
                      <input className="assurarr-input" value={effectivenessForm.reopenedCapaRef} onChange={(event) => setEffectivenessForm({ ...effectivenessForm, reopenedCapaRef: event.target.value })} placeholder="CAPA-000001" />
                    </Field>
                    <Field label="Follow-up required">
                      <select className="assurarr-select" value={effectivenessForm.followUpRequired ? 'yes' : 'no'} onChange={(event) => setEffectivenessForm({ ...effectivenessForm, followUpRequired: event.target.value === 'yes' })}>
                        <option value="no">No</option>
                        <option value="yes">Yes</option>
                      </select>
                    </Field>
                    <Field label="Recurrence found">
                      <select className="assurarr-select" value={effectivenessForm.recurrenceFound ? 'yes' : 'no'} onChange={(event) => setEffectivenessForm({ ...effectivenessForm, recurrenceFound: event.target.value === 'yes' })}>
                        <option value="no">No</option>
                        <option value="yes">Yes</option>
                      </select>
                    </Field>
                  </div>
                  <button className="assurarr-button mt-3" type="button" onClick={() => createEffectivenessMutation.mutate()} disabled={createEffectivenessMutation.isPending}>
                    {createEffectivenessMutation.isPending ? 'Saving...' : 'Create effectiveness verification'}
                  </button>
                </div>
                {effectivenessQuery.data?.length ? (
                  <div className="space-y-2">
                    {effectivenessQuery.data.map((verification) => (
                      <div key={verification.id} className="rounded-xl border border-slate-700/70 bg-slate-950/60 p-3">
                        <div className="flex items-center justify-between gap-3">
                          <strong className="text-sm text-slate-50">{verification.number}</strong>
                          <span className="assurarr-pill">{verification.status}</span>
                        </div>
                        <p className="mt-1 text-sm text-slate-300">{verification.resultSummary ?? 'No result summary yet.'}</p>
                        <p className="mt-1 text-xs text-slate-400">Metric results: {verification.metricResults.length ? verification.metricResults.join(', ') : 'none'}</p>
                        <div className="mt-3 flex flex-wrap gap-2">
                          <button className="assurarr-button secondary" type="button" onClick={() => updateEffectivenessMutation.mutate({ verificationId: verification.id, status: 'effective' })} disabled={updateEffectivenessMutation.isPending}>
                            Mark effective
                          </button>
                          <button className="assurarr-button secondary" type="button" onClick={() => updateEffectivenessMutation.mutate({ verificationId: verification.id, status: 'ineffective' })} disabled={updateEffectivenessMutation.isPending}>
                            Mark ineffective
                          </button>
                          <button className="assurarr-button secondary" type="button" onClick={() => updateEffectivenessMutation.mutate({ verificationId: verification.id, status: 'inconclusive' })} disabled={updateEffectivenessMutation.isPending}>
                            Mark inconclusive
                          </button>
                        </div>
                      </div>
                    ))}
                  </div>
                ) : (
                  <EmptyState title="No effectiveness verifications yet." />
                )}
                <div className="rounded-xl border border-slate-700/70 bg-slate-950/60 p-3">
                  <div className="flex items-center justify-between gap-3">
                    <strong className="text-sm text-slate-50">Current effectiveness status</strong>
                    <span className="assurarr-pill">{currentEffectivenessVerification?.status ?? 'none'}</span>
                  </div>
                  <p className="mt-1 text-sm text-slate-300">
                    {currentEffectivenessVerification
                      ? currentEffectivenessVerification.resultSummary ?? 'Verification result not summarized yet.'
                      : 'No effectiveness verification has been created for this CAPA yet.'}
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>

        <SectionCard
          title="Effectiveness evidence"
          items={capa.recordRefs.map((item) => item)}
          emptyLabel="No evidence records are linked to this CAPA."
        />
        <SectionCard
          title="Related nonconformances"
          items={relatedNonconformances.map((item) => `${item.number} · ${item.title} · ${item.status}`)}
          emptyLabel="No linked nonconformances were found."
        />
        <SectionCard
          title="Related findings"
          items={relatedFindings.map((item) => `${item.number} · ${item.title} · ${item.status}`)}
          emptyLabel="No linked findings were found."
        />
        <SectionCard
          title="Timeline"
          items={timeline.map((event) => `${event.eventType} · ${new Date(event.occurredAt).toLocaleString()}`)}
          emptyLabel="No timeline events recorded yet."
        />
      </div>
    </div>
  )
}

function AuditPage() {
  const query = useRecords(['assurarr', 'audits'], assurarrApi.listAudits)
  const queryClient = useQueryClient()
  const [selectedAuditId, setSelectedAuditId] = useState('')
  const activeAuditId = selectedAuditId || query.data?.[0]?.id || ''
  const [selectedChecklistId, setSelectedChecklistId] = useState('')
  const [checklistForm, setChecklistForm] = useState({
    title: '',
    description: '',
    status: 'draft',
  })
  const [itemForm, setItemForm] = useState({
    sequence: '1',
    prompt: '',
    helpText: '',
    requirementRef: '',
    responseType: 'pass_fail',
    required: true,
    responseValue: '',
    result: 'pass',
    findingCreated: false,
    findingRef: '',
    evidenceRecordRefs: '',
    answeredByPersonId: '',
    answeredAt: '',
  })
  const checklistQuery = useQuery({
    queryKey: ['assurarr', 'audit-checklists', activeAuditId],
    queryFn: () => assurarrApi.listAuditChecklists(activeAuditId),
    enabled: Boolean(activeAuditId),
    staleTime: 15_000,
  })
  const activeChecklistId = selectedChecklistId || checklistQuery.data?.[0]?.id || ''
  const itemQuery = useQuery({
    queryKey: ['assurarr', 'audit-checklist-items', activeAuditId, activeChecklistId],
    queryFn: () => assurarrApi.listAuditChecklistItems(activeAuditId, activeChecklistId),
    enabled: Boolean(activeAuditId && activeChecklistId),
    staleTime: 15_000,
  })
  const createChecklistMutation = useMutation({
    mutationFn: async () =>
      assurarrApi.createAuditChecklist(activeAuditId, {
        title: checklistForm.title,
        description: checklistForm.description,
        status: checklistForm.status,
      }),
    onSuccess: async (created) => {
      await queryClient.invalidateQueries({ queryKey: ['assurarr'] })
      setSelectedChecklistId(created.id)
      setChecklistForm({ title: '', description: '', status: 'draft' })
    },
  })
  const createItemMutation = useMutation({
    mutationFn: async () =>
      assurarrApi.createAuditChecklistItem(activeAuditId, activeChecklistId, {
        sequence: Number.parseInt(itemForm.sequence, 10) || 1,
        prompt: itemForm.prompt,
        helpText: itemForm.helpText || undefined,
        requirementRef: itemForm.requirementRef || undefined,
        responseType: itemForm.responseType,
        required: itemForm.required,
        responseValue: itemForm.responseValue || undefined,
        result: itemForm.result || undefined,
        findingCreated: itemForm.findingCreated,
        findingRef: itemForm.findingRef || undefined,
        evidenceRecordRefs: joinRefs(itemForm.evidenceRecordRefs),
        answeredByPersonId: itemForm.answeredByPersonId || undefined,
        answeredAt: itemForm.answeredAt || undefined,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['assurarr'] })
      setItemForm({
        sequence: '1',
        prompt: '',
        helpText: '',
        requirementRef: '',
        responseType: 'pass_fail',
        required: true,
        responseValue: '',
        result: 'pass',
        findingCreated: false,
        findingRef: '',
        evidenceRecordRefs: '',
        answeredByPersonId: '',
        answeredAt: '',
      })
    },
  })

  return (
    <div className="assurarr-page">
      <PageHeader title="Audits" description="Plan, execute, and close internal or supplier audits with findings tied back to quality action." />
      <RecordForm
        title="Create audit"
        entityLabel="Audit"
        onCreate={async (body) =>
          assurarrApi.createAudit({
            ...body,
            ownerPersonId: body.ownerPersonId || undefined,
            auditType: 'internal',
            auditScope: 'Receiving process review',
            auditorPersonIds: [],
            checklistRefs: [],
          })
        }
      />
      <div className="assurarr-grid cols-2">
        <div className="assurarr-card">
          <div className="assurarr-card-inner space-y-4">
            <div className="flex items-center gap-2">
              <ClipboardList className="h-4 w-4 text-cyan-300" />
              <h3 className="text-base font-semibold text-slate-50">Prepare audit checklist</h3>
            </div>
            <div className="grid gap-3 md:grid-cols-2">
              <Field label="Audit">
                <select className="assurarr-select" value={activeAuditId} onChange={(event) => setSelectedAuditId(event.target.value)}>
                  {query.data?.map((audit) => (
                    <option key={audit.id} value={audit.id}>
                      {audit.number} - {audit.title}
                    </option>
                  ))}
                </select>
              </Field>
              <Field label="Checklist status">
                <select className="assurarr-select" value={checklistForm.status} onChange={(event) => setChecklistForm({ ...checklistForm, status: event.target.value })}>
                  <option value="draft">Draft</option>
                  <option value="active">Active</option>
                  <option value="completed">Completed</option>
                  <option value="canceled">Canceled</option>
                </select>
              </Field>
              <Field label="Checklist title" wide>
                <input className="assurarr-input" value={checklistForm.title} onChange={(event) => setChecklistForm({ ...checklistForm, title: event.target.value })} />
              </Field>
              <Field label="Description" wide>
                <textarea className="assurarr-textarea" value={checklistForm.description} onChange={(event) => setChecklistForm({ ...checklistForm, description: event.target.value })} />
              </Field>
            </div>
            <button className="assurarr-button" type="button" onClick={() => createChecklistMutation.mutate()} disabled={createChecklistMutation.isPending || !activeAuditId}>
              {createChecklistMutation.isPending ? 'Saving...' : 'Create checklist'}
            </button>
            {checklistQuery.data?.length ? (
              <div className="space-y-2">
                {checklistQuery.data.map((checklist) => (
                  <button
                    key={checklist.id}
                    type="button"
                    className={`w-full rounded-xl border px-4 py-3 text-left transition ${
                      checklist.id === activeChecklistId ? 'border-cyan-400/70 bg-cyan-400/10' : 'border-slate-700/70 bg-slate-900/80'
                    }`}
                    onClick={() => setSelectedChecklistId(checklist.id)}
                  >
                    <div className="flex items-center justify-between gap-3">
                      <strong className="text-sm text-slate-50">{checklist.number}</strong>
                      <span className="assurarr-pill">{checklist.status}</span>
                    </div>
                    <p className="mt-1 text-sm text-slate-300">{checklist.title}</p>
                    <p className="mt-1 text-xs text-slate-400">{checklist.itemRefs.length} item refs</p>
                  </button>
                ))}
              </div>
            ) : (
              <EmptyState title={activeAuditId ? 'No checklists yet.' : 'Select an audit first.'} />
            )}
          </div>
        </div>

        <div className="assurarr-card">
          <div className="assurarr-card-inner space-y-4">
            <div className="flex items-center gap-2">
              <Sparkles className="h-4 w-4 text-cyan-300" />
              <h3 className="text-base font-semibold text-slate-50">Add checklist item response</h3>
            </div>
            <div className="grid gap-3 md:grid-cols-2">
              <Field label="Checklist">
                <select className="assurarr-select" value={activeChecklistId} onChange={(event) => setSelectedChecklistId(event.target.value)} disabled={!checklistQuery.data?.length}>
                  {checklistQuery.data?.map((checklist) => (
                    <option key={checklist.id} value={checklist.id}>
                      {checklist.number} - {checklist.title}
                    </option>
                  ))}
                </select>
              </Field>
              <Field label="Sequence">
                <input className="assurarr-input" value={itemForm.sequence} onChange={(event) => setItemForm({ ...itemForm, sequence: event.target.value })} />
              </Field>
              <Field label="Response type">
                <select className="assurarr-select" value={itemForm.responseType} onChange={(event) => setItemForm({ ...itemForm, responseType: event.target.value })}>
                  <option value="pass_fail">Pass / fail</option>
                  <option value="yes_no">Yes / no</option>
                  <option value="numeric">Numeric</option>
                  <option value="text">Text</option>
                  <option value="select">Select</option>
                  <option value="multi_select">Multi select</option>
                  <option value="photo">Photo</option>
                  <option value="document">Document</option>
                </select>
              </Field>
              <Field label="Required">
                <select className="assurarr-select" value={String(itemForm.required)} onChange={(event) => setItemForm({ ...itemForm, required: event.target.value === 'true' })}>
                  <option value="true">Required</option>
                  <option value="false">Optional</option>
                </select>
              </Field>
              <Field label="Prompt" wide>
                <input className="assurarr-input" value={itemForm.prompt} onChange={(event) => setItemForm({ ...itemForm, prompt: event.target.value })} />
              </Field>
              <Field label="Help text" wide>
                <textarea className="assurarr-textarea" value={itemForm.helpText} onChange={(event) => setItemForm({ ...itemForm, helpText: event.target.value })} />
              </Field>
              <Field label="Requirement ref">
                <input className="assurarr-input" value={itemForm.requirementRef} onChange={(event) => setItemForm({ ...itemForm, requirementRef: event.target.value })} />
              </Field>
              <Field label="Response value">
                <input className="assurarr-input" value={itemForm.responseValue} onChange={(event) => setItemForm({ ...itemForm, responseValue: event.target.value })} />
              </Field>
              <Field label="Result">
                <select className="assurarr-select" value={itemForm.result} onChange={(event) => setItemForm({ ...itemForm, result: event.target.value })}>
                  <option value="pass">Pass</option>
                  <option value="fail">Fail</option>
                  <option value="observation">Observation</option>
                  <option value="not_applicable">Not applicable</option>
                </select>
              </Field>
              <Field label="Evidence refs" wide>
                <textarea className="assurarr-textarea" value={itemForm.evidenceRecordRefs} onChange={(event) => setItemForm({ ...itemForm, evidenceRecordRefs: event.target.value })} />
              </Field>
            </div>
            <button className="assurarr-button" type="button" onClick={() => createItemMutation.mutate()} disabled={createItemMutation.isPending || !activeChecklistId}>
              {createItemMutation.isPending ? 'Saving...' : 'Create checklist item'}
            </button>
            {itemQuery.data?.length ? (
              <div className="space-y-3">
                {itemQuery.data.map((item) => (
                  <div key={item.id} className="rounded-xl border border-slate-700/70 bg-slate-900/80 p-3">
                    <div className="flex items-center justify-between gap-3">
                      <strong className="text-sm text-slate-50">
                        {item.sequence}. {item.prompt}
                      </strong>
                      <span className="assurarr-pill">{item.result ?? item.responseValue ?? 'unanswered'}</span>
                    </div>
                    <p className="mt-1 text-xs text-slate-400">
                      {item.responseType} {item.required ? 'required' : 'optional'}
                    </p>
                    {item.helpText ? <p className="mt-2 text-sm text-slate-300">{item.helpText}</p> : null}
                    {item.evidenceRecordRefs.length ? <p className="mt-2 text-xs text-slate-400">{item.evidenceRecordRefs.join(', ')}</p> : null}
                  </div>
                ))}
              </div>
            ) : (
              <EmptyState title={activeChecklistId ? 'No items on this checklist yet.' : 'Select a checklist first.'} />
            )}
          </div>
        </div>
      </div>
      {query.data ? (
        <EntityTable
          items={query.data}
          emptyLabel="No audits yet."
          onStatusChange={(id, status) => assurarrApi.updateAuditStatus(id, status)}
          statusChoices={statusOptions.audit}
          detailBasePath="/audits"
        />
      ) : (
        <LoadingCard label="Loading audits" />
      )}
    </div>
  )
}

function AuditDetailPage() {
  const { id = '' } = useParams()
  const query = useQuery({
    queryKey: ['assurarr', 'audit', id],
    queryFn: () => assurarrApi.getAudit(id),
    enabled: Boolean(id),
  })
  const checklistQuery = useRecords(['assurarr', 'audit-checklists', id], () => assurarrApi.listAuditChecklists(id))
  const findingsQuery = useRecords(['assurarr', 'findings'], assurarrApi.listFindings)
  const dashboard = useDashboard()
  const [selectedChecklistId, setSelectedChecklistId] = useState('')

  useEffect(() => {
    setSelectedChecklistId('')
  }, [id])

  if (query.isLoading) {
    return <LoadingCard label="Loading audit detail" />
  }

  if (query.isError || !query.data) {
    return (
      <div className="assurarr-page">
        <PageHeader title="Audit detail" description="Could not load the requested audit." />
        <EmptyState title="Audit not found." />
      </div>
    )
  }

  const audit = query.data
  const activeChecklistId = selectedChecklistId || checklistQuery.data?.[0]?.id || ''
  const selectedChecklist = checklistQuery.data?.find((checklist) => checklist.id === activeChecklistId) ?? null
  const itemQuery = useQuery({
    queryKey: ['assurarr', 'audit-checklist-items', id, activeChecklistId],
    queryFn: () => assurarrApi.listAuditChecklistItems(id, activeChecklistId),
    enabled: Boolean(id && activeChecklistId),
    staleTime: 15_000,
  })
  const relatedFindings = findingsQuery.data?.filter((finding) => finding.auditRef === audit.number || audit.findingRefs.includes(finding.number)) ?? []
  const timeline = dashboard.data?.recentEvents.filter((event) => event.subjectType === 'audit' && event.subjectId === audit.id) ?? []

  return (
    <div className="assurarr-page">
      <PageHeader
        title={`${audit.number} · ${audit.title}`}
        description="Audit scope, checklist structure, findings, and the linked evidence trail."
      />
      <div className="space-y-4">
        <div className="assurarr-grid cols-2">
          <div className="assurarr-card">
            <div className="assurarr-card-inner space-y-3">
              <p className="assurarr-label">Overview</p>
              <div className="flex flex-wrap gap-2 text-sm">
                <span className="assurarr-pill">{audit.status}</span>
                <span className="assurarr-pill">{audit.severity}</span>
                <span className="assurarr-pill">{audit.auditType}</span>
                <span className="assurarr-pill">{audit.auditScope ?? 'no scope'}</span>
              </div>
              <p className="text-sm text-slate-300">{audit.description}</p>
              <div className="grid gap-2 text-sm text-slate-300 md:grid-cols-2">
                <div><span className="text-slate-500">Source product:</span> {audit.sourceProduct ?? 'manual'}</div>
                <div><span className="text-slate-500">Source object:</span> {audit.sourceObjectRef ?? 'n/a'}</div>
                <div><span className="text-slate-500">Lead auditor:</span> {audit.leadAuditorPersonId ?? 'n/a'}</div>
                <div><span className="text-slate-500">Owner:</span> {audit.ownerPersonId ?? 'unassigned'}</div>
                <div><span className="text-slate-500">Planned start:</span> {audit.plannedStartAt ? new Date(audit.plannedStartAt).toLocaleString() : 'n/a'}</div>
                <div><span className="text-slate-500">Planned end:</span> {audit.plannedEndAt ? new Date(audit.plannedEndAt).toLocaleString() : 'n/a'}</div>
              </div>
            </div>
          </div>
          <div className="assurarr-card">
            <div className="assurarr-card-inner space-y-3">
              <p className="assurarr-label">Coverage</p>
              <div className="text-sm text-slate-300">
                <div><span className="text-slate-500">Auditors:</span> {audit.auditorPersonIds.length ? audit.auditorPersonIds.join(', ') : 'none'}</div>
                <div><span className="text-slate-500">Sites:</span> {audit.staffArrSiteId ?? 'n/a'}</div>
                <div><span className="text-slate-500">Locations:</span> {audit.staffArrLocationId ?? 'n/a'}</div>
                <div><span className="text-slate-500">Supplier:</span> {audit.supplierRef ?? 'n/a'}</div>
                <div><span className="text-slate-500">Customer:</span> {audit.customerRef ?? 'n/a'}</div>
                <div><span className="text-slate-500">Record refs:</span> {audit.recordRefs.length ? audit.recordRefs.join(', ') : 'none'}</div>
              </div>
            </div>
          </div>
        </div>

        <div className="assurarr-grid cols-2">
          <div className="assurarr-card">
            <div className="assurarr-card-inner space-y-4">
              <p className="assurarr-label">Checklists</p>
              {checklistQuery.data?.length ? (
                <div className="space-y-2">
                  {checklistQuery.data.map((checklist) => (
                    <button
                      key={checklist.id}
                      type="button"
                      className={`w-full rounded-xl border px-4 py-3 text-left transition ${checklist.id === activeChecklistId ? 'border-cyan-400/70 bg-cyan-400/10' : 'border-slate-700/70 bg-slate-900/80'}`}
                      onClick={() => setSelectedChecklistId(checklist.id)}
                    >
                      <div className="flex items-center justify-between gap-3">
                        <strong className="text-sm text-slate-50">{checklist.number}</strong>
                        <span className="assurarr-pill">{checklist.status}</span>
                      </div>
                      <p className="mt-1 text-sm text-slate-300">{checklist.title}</p>
                      <p className="mt-1 text-xs text-slate-400">{checklist.itemRefs.length} item refs</p>
                    </button>
                  ))}
                </div>
              ) : (
                <EmptyState title="No checklists yet." />
              )}
            </div>
          </div>
          <div className="assurarr-card">
            <div className="assurarr-card-inner space-y-4">
              <p className="assurarr-label">Selected checklist items</p>
              {selectedChecklist ? (
                <div className="rounded-xl border border-slate-700/70 bg-slate-950/60 p-3">
                  <div className="flex items-center justify-between gap-3">
                    <strong className="text-sm text-slate-50">{selectedChecklist.number}</strong>
                    <span className="assurarr-pill">{selectedChecklist.status}</span>
                  </div>
                  <p className="mt-1 text-sm text-slate-300">{selectedChecklist.title}</p>
                  <p className="mt-1 text-xs text-slate-400">{selectedChecklist.description}</p>
                </div>
              ) : null}
              {itemQuery.data?.length ? (
                <div className="space-y-3">
                  {itemQuery.data.map((item) => (
                    <div key={item.id} className="rounded-xl border border-slate-700/70 bg-slate-900/80 p-3">
                      <div className="flex items-center justify-between gap-3">
                        <strong className="text-sm text-slate-50">{item.sequence}. {item.prompt}</strong>
                        <span className="assurarr-pill">{item.result ?? item.responseValue ?? 'unanswered'}</span>
                      </div>
                      <p className="mt-1 text-xs text-slate-400">{item.responseType} {item.required ? 'required' : 'optional'}</p>
                      {item.helpText ? <p className="mt-2 text-sm text-slate-300">{item.helpText}</p> : null}
                      {item.evidenceRecordRefs.length ? <p className="mt-2 text-xs text-slate-400">{item.evidenceRecordRefs.join(', ')}</p> : null}
                    </div>
                  ))}
                </div>
              ) : (
                <EmptyState title={activeChecklistId ? 'No items on this checklist yet.' : 'Select a checklist first.'} />
              )}
            </div>
          </div>
        </div>

        <SectionCard
          title="Related findings"
          items={relatedFindings.map((finding) => `${finding.number} · ${finding.title} · ${finding.status}`)}
          emptyLabel="No linked findings were found."
        />
        <SectionCard
          title="Timeline"
          items={timeline.map((event) => `${event.eventType} · ${new Date(event.occurredAt).toLocaleString()}`)}
          emptyLabel="No timeline events recorded yet."
        />
      </div>
    </div>
  )
}

function FindingsPage() {
  const query = useRecords(['assurarr', 'findings'], assurarrApi.listFindings)
  return (
    <div className="assurarr-page">
      <PageHeader title="Findings" description="Audit observations and nonconformances that may drive CAPA or release review." />
      <RecordForm
        title="Create finding"
        entityLabel="Finding"
        onCreate={async (body) =>
          assurarrApi.createFinding({
            ...body,
            ownerPersonId: body.ownerPersonId || undefined,
            findingType: 'major_nonconformance',
          })
        }
      />
      {query.data ? (
        <EntityTable
          items={query.data}
          emptyLabel="No findings yet."
          onStatusChange={(id, status) => assurarrApi.updateFindingStatus(id, status)}
          statusChoices={statusOptions.finding}
          detailBasePath="/findings"
        />
      ) : (
        <LoadingCard label="Loading findings" />
      )}
    </div>
  )
}

function FindingDetailPage() {
  const { id = '' } = useParams()
  const query = useQuery({
    queryKey: ['assurarr', 'finding', id],
    queryFn: () => assurarrApi.getFinding(id),
    enabled: Boolean(id),
  })
  const audits = useRecords(['assurarr', 'audits'], assurarrApi.listAudits)
  const capas = useRecords(['assurarr', 'capas'], assurarrApi.listCapas)
  const nonconformances = useRecords(['assurarr', 'nonconformances'], assurarrApi.listNonconformances)
  const dashboard = useDashboard()

  if (query.isLoading) {
    return <LoadingCard label="Loading finding detail" />
  }

  if (query.isError || !query.data) {
    return (
      <div className="assurarr-page">
        <PageHeader title="Finding detail" description="Could not load the requested audit finding." />
        <EmptyState title="Finding not found." />
      </div>
    )
  }

  const finding = query.data
  const relatedAudit = audits.data?.find((audit) => audit.number === finding.auditRef) ?? null
  const relatedNonconformance = nonconformances.data?.find((item) => item.number === finding.nonconformanceRef) ?? null
  const relatedCapa = capas.data?.find((item) => item.number === finding.capaRef) ?? null
  const timeline = dashboard.data?.recentEvents.filter((event) => event.subjectType === 'finding' && event.subjectId === finding.id) ?? []

  return (
    <div className="assurarr-page">
      <PageHeader
        title={`${finding.number} · ${finding.title}`}
        description="Audit finding details, linked quality records, evidence, and follow-up action references."
      />
      <div className="space-y-4">
        <div className="assurarr-grid cols-2">
          <div className="assurarr-card">
            <div className="assurarr-card-inner space-y-3">
              <p className="assurarr-label">Overview</p>
              <div className="flex flex-wrap gap-2 text-sm">
                <span className="assurarr-pill">{finding.status}</span>
                <span className="assurarr-pill">{finding.severity}</span>
                <span className="assurarr-pill">{finding.findingType}</span>
              </div>
              <p className="text-sm text-slate-300">{finding.description}</p>
              <div className="grid gap-2 text-sm text-slate-300 md:grid-cols-2">
                <div><span className="text-slate-500">Source product:</span> {finding.sourceProduct ?? 'manual'}</div>
                <div><span className="text-slate-500">Source object:</span> {finding.sourceObjectRef ?? 'n/a'}</div>
                <div><span className="text-slate-500">Owner:</span> {finding.ownerPersonId ?? 'unassigned'}</div>
                <div><span className="text-slate-500">Due:</span> {finding.dueAt ? new Date(finding.dueAt).toLocaleString() : 'n/a'}</div>
                <div><span className="text-slate-500">Closed:</span> {finding.closedAt ? new Date(finding.closedAt).toLocaleString() : 'n/a'}</div>
                <div><span className="text-slate-500">Record refs:</span> {finding.recordRefs.length ? finding.recordRefs.join(', ') : 'none'}</div>
              </div>
            </div>
          </div>
          <div className="assurarr-card">
            <div className="assurarr-card-inner space-y-3">
              <p className="assurarr-label">Linked quality records</p>
              <div className="text-sm text-slate-300">
                <div><span className="text-slate-500">Audit:</span> {relatedAudit ? `${relatedAudit.number} · ${relatedAudit.title}` : finding.auditRef ?? 'n/a'}</div>
                <div><span className="text-slate-500">Nonconformance:</span> {relatedNonconformance ? `${relatedNonconformance.number} · ${relatedNonconformance.title}` : finding.nonconformanceRef ?? 'n/a'}</div>
                <div><span className="text-slate-500">CAPA:</span> {relatedCapa ? `${relatedCapa.number} · ${relatedCapa.title}` : finding.capaRef ?? 'n/a'}</div>
              </div>
            </div>
          </div>
        </div>

        <SectionCard
          title="Affected objects"
          items={finding.affectedObjectRefs.map((ref) => ref)}
          emptyLabel="No affected objects recorded."
        />
        <SectionCard
          title="Evidence"
          items={finding.recordRefs.map((ref) => ref)}
          emptyLabel="No evidence records linked to this finding."
        />
        <SectionCard
          title="Timeline"
          items={timeline.map((event) => `${event.eventType} · ${new Date(event.occurredAt).toLocaleString()}`)}
          emptyLabel="No timeline events recorded yet."
        />
      </div>
    </div>
  )
}

function ReviewPage() {
  const query = useRecords(['assurarr', 'reviews'], assurarrApi.listQualityReviews)
  const [form, setForm] = useState({
    title: '',
    description: '',
    severity: 'moderate',
    reviewType: 'hold_release',
    sourceProduct: 'assurarr',
    sourceObjectRef: '',
    affectedObjectRefs: '',
    ownerPersonId: '',
    sourceReviewRef: '',
    reviewerPersonId: '',
    requestedAt: '',
    dueAt: '',
    decisionReason: '',
    requiredEvidenceRefs: '',
    submittedEvidenceRefs: '',
    notes: '',
  })
  const queryClient = useQueryClient()
  const mutation = useMutation({
    mutationFn: async () =>
      assurarrApi.createQualityReview({
        title: form.title,
        description: form.description,
        severity: form.severity,
        reviewType: form.reviewType,
        sourceProduct: form.sourceProduct,
        sourceObjectRef: form.sourceObjectRef,
        affectedObjectRefs: joinRefs(form.affectedObjectRefs),
        ownerPersonId: form.ownerPersonId || undefined,
        sourceReviewRef: form.sourceReviewRef || undefined,
        reviewerPersonId: form.reviewerPersonId || undefined,
        requestedAt: form.requestedAt || undefined,
        dueAt: form.dueAt || undefined,
        decisionReason: form.decisionReason || undefined,
        requiredEvidenceRefs: joinRefs(form.requiredEvidenceRefs),
        submittedEvidenceRefs: joinRefs(form.submittedEvidenceRefs),
        notes: form.notes || undefined,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['assurarr'] })
      setForm({
        title: '',
        description: '',
        severity: 'moderate',
        reviewType: 'hold_release',
        sourceProduct: 'assurarr',
        sourceObjectRef: '',
        affectedObjectRefs: '',
        ownerPersonId: '',
        sourceReviewRef: '',
        reviewerPersonId: '',
        requestedAt: '',
        dueAt: '',
        decisionReason: '',
        requiredEvidenceRefs: '',
        submittedEvidenceRefs: '',
        notes: '',
      })
    },
  })

  return (
    <div className="assurarr-page">
      <PageHeader
        title="Quality reviews"
        description="Review gates for evidence, disposition, release, and closure decisions."
        action={<span className="assurarr-pill"><Plus className="h-4 w-4" /> {query.data?.length ?? 0} records</span>}
      />
      <div className="assurarr-card">
        <div className="assurarr-card-inner space-y-4">
          <div className="flex items-center gap-2">
            <Plus className="h-4 w-4 text-cyan-300" />
            <h3 className="text-base font-semibold text-slate-50">Create quality review</h3>
          </div>
          <div className="grid gap-3 md:grid-cols-2">
            <Field label="Review title">
              <input className="assurarr-input" value={form.title} onChange={(event) => setForm({ ...form, title: event.target.value })} />
            </Field>
            <Field label="Review type">
              <select className="assurarr-select" value={form.reviewType} onChange={(event) => setForm({ ...form, reviewType: event.target.value })}>
                <option value="nonconformance_review">Nonconformance review</option>
                <option value="hold_release">Hold release</option>
                <option value="disposition_review">Disposition review</option>
                <option value="capa_review">CAPA review</option>
                <option value="audit_finding_review">Audit finding review</option>
                <option value="supplier_response_review">Supplier response review</option>
                <option value="customer_response_review">Customer response review</option>
                <option value="document_quality_review">Document quality review</option>
              </select>
            </Field>
            <Field label="Severity">
              <select className="assurarr-select" value={form.severity} onChange={(event) => setForm({ ...form, severity: event.target.value })}>
                <option value="low">Low</option>
                <option value="moderate">Moderate</option>
                <option value="high">High</option>
                <option value="critical">Critical</option>
                <option value="none">None</option>
              </select>
            </Field>
            <Field label="Source object ref">
              <input className="assurarr-input" value={form.sourceObjectRef} onChange={(event) => setForm({ ...form, sourceObjectRef: event.target.value })} placeholder="HOLD-000001" />
            </Field>
            <Field label="Affected object refs" wide>
              <textarea className="assurarr-textarea" value={form.affectedObjectRefs} onChange={(event) => setForm({ ...form, affectedObjectRefs: event.target.value })} />
            </Field>
            <Field label="Required evidence refs" wide>
              <textarea className="assurarr-textarea" value={form.requiredEvidenceRefs} onChange={(event) => setForm({ ...form, requiredEvidenceRefs: event.target.value })} />
            </Field>
            <Field label="Submitted evidence refs" wide>
              <textarea className="assurarr-textarea" value={form.submittedEvidenceRefs} onChange={(event) => setForm({ ...form, submittedEvidenceRefs: event.target.value })} />
            </Field>
            <Field label="Description" wide>
              <textarea className="assurarr-textarea" value={form.description} onChange={(event) => setForm({ ...form, description: event.target.value })} />
            </Field>
            <Field label="Notes" wide>
              <textarea className="assurarr-textarea" value={form.notes} onChange={(event) => setForm({ ...form, notes: event.target.value })} />
            </Field>
            <Field label="Reviewer person id">
              <input className="assurarr-input" value={form.reviewerPersonId} onChange={(event) => setForm({ ...form, reviewerPersonId: event.target.value })} />
            </Field>
            <Field label="Requested at">
              <input className="assurarr-input" type="datetime-local" value={form.requestedAt} onChange={(event) => setForm({ ...form, requestedAt: event.target.value })} />
            </Field>
            <Field label="Due at">
              <input className="assurarr-input" type="datetime-local" value={form.dueAt} onChange={(event) => setForm({ ...form, dueAt: event.target.value })} />
            </Field>
            <Field label="Decision reason" wide>
              <input className="assurarr-input" value={form.decisionReason} onChange={(event) => setForm({ ...form, decisionReason: event.target.value })} />
            </Field>
          </div>
          <button className="assurarr-button" type="button" onClick={() => mutation.mutate()} disabled={mutation.isPending}>
            {mutation.isPending ? 'Saving...' : 'Create review'}
          </button>
        </div>
      </div>
      {query.data ? (
        <EntityTable
          items={query.data}
          emptyLabel="No quality reviews yet."
          onStatusChange={(id, status) => assurarrApi.updateQualityReviewStatus(id, status)}
          statusChoices={statusOptions.review}
          detailBasePath="/reviews"
        />
      ) : (
        <LoadingCard label="Loading quality reviews" />
      )}
    </div>
  )
}

function ReviewDetailPage() {
  const { id = '' } = useParams()
  const query = useQuery({
    queryKey: ['assurarr', 'review', id],
    queryFn: () => assurarrApi.getQualityReview(id),
    enabled: Boolean(id),
  })
  const dashboard = useDashboard()

  if (query.isLoading) {
    return <LoadingCard label="Loading review detail" />
  }

  if (query.isError || !query.data) {
    return (
      <div className="assurarr-page">
        <PageHeader title="Quality review detail" description="Could not load the requested quality review." />
        <EmptyState title="Quality review not found." />
      </div>
    )
  }

  const review = query.data
  const timeline = dashboard.data?.recentEvents.filter((event) => event.subjectType === 'review' && event.subjectId === review.id) ?? []

  return (
    <div className="assurarr-page">
      <PageHeader
        title={`${review.number} · ${review.title}`}
        description="Evidence review, decision state, and closure context for the quality gate."
      />
      <div className="space-y-4">
        <div className="assurarr-grid cols-2">
          <div className="assurarr-card">
            <div className="assurarr-card-inner space-y-3">
              <p className="assurarr-label">Overview</p>
              <div className="flex flex-wrap gap-2 text-sm">
                <span className="assurarr-pill">{review.status}</span>
                <span className="assurarr-pill">{review.severity}</span>
                <span className="assurarr-pill">{review.reviewType}</span>
              </div>
              <p className="text-sm text-slate-300">Review gate for evidence, closure, and release decisions.</p>
              <div className="grid gap-2 text-sm text-slate-300 md:grid-cols-2">
                <div><span className="text-slate-500">Source product:</span> {review.sourceProduct ?? 'manual'}</div>
                <div><span className="text-slate-500">Source object:</span> {review.sourceObjectRef ?? 'n/a'}</div>
                <div><span className="text-slate-500">Reviewer:</span> {review.reviewerPersonId ?? 'unassigned'}</div>
                <div><span className="text-slate-500">Requested:</span> {review.requestedAt ? new Date(review.requestedAt).toLocaleString() : 'n/a'}</div>
                <div><span className="text-slate-500">Due:</span> {review.dueAt ? new Date(review.dueAt).toLocaleString() : 'n/a'}</div>
                <div><span className="text-slate-500">Decision:</span> {review.decisionAt ? new Date(review.decisionAt).toLocaleString() : 'n/a'}</div>
              </div>
            </div>
          </div>
          <div className="assurarr-card">
            <div className="assurarr-card-inner space-y-3">
              <p className="assurarr-label">Evidence</p>
              <div className="text-sm text-slate-300">
                <div><span className="text-slate-500">Required refs:</span> {review.requiredEvidenceRefs.length ? review.requiredEvidenceRefs.join(', ') : 'none'}</div>
                <div><span className="text-slate-500">Submitted refs:</span> {review.submittedEvidenceRefs.length ? review.submittedEvidenceRefs.join(', ') : 'none'}</div>
                <div><span className="text-slate-500">Source review ref:</span> {review.sourceReviewRef ?? 'n/a'}</div>
                <div><span className="text-slate-500">Decision reason:</span> {review.decisionReason ?? 'n/a'}</div>
                <div><span className="text-slate-500">Notes:</span> {review.notes ?? 'n/a'}</div>
              </div>
            </div>
          </div>
        </div>

        <SectionCard
          title="Affected objects"
          items={review.affectedObjectRefs.map((ref) => ref)}
          emptyLabel="No affected objects recorded."
        />
        <SectionCard
          title="Timeline"
          items={timeline.map((event) => `${event.eventType} · ${new Date(event.occurredAt).toLocaleString()}`)}
          emptyLabel="No timeline events recorded yet."
        />
      </div>
    </div>
  )
}

function ReleasePage() {
  const query = useRecords(['assurarr', 'releases'], assurarrApi.listQualityReleases)
  const [form, setForm] = useState({
    title: '',
    description: '',
    severity: 'none',
    sourceProduct: 'assurarr',
    sourceObjectRef: '',
    affectedObjectRefs: '',
    ownerPersonId: '',
    holdRef: '',
    releaseType: 'full',
    requestedByPersonId: '',
    requestedAt: '',
    conditions: '',
    expirationAt: '',
    evidenceRecordRefs: '',
    notes: '',
  })
  const queryClient = useQueryClient()
  const mutation = useMutation({
    mutationFn: async () =>
      assurarrApi.createQualityRelease({
        title: form.title,
        description: form.description,
        severity: form.severity,
        sourceProduct: form.sourceProduct,
        sourceObjectRef: form.sourceObjectRef,
        affectedObjectRefs: joinRefs(form.affectedObjectRefs),
        ownerPersonId: form.ownerPersonId || undefined,
        holdRef: form.holdRef,
        releaseType: form.releaseType,
        requestedByPersonId: form.requestedByPersonId || undefined,
        requestedAt: form.requestedAt || undefined,
        conditions: form.conditions || undefined,
        expirationAt: form.expirationAt || undefined,
        evidenceRecordRefs: joinRefs(form.evidenceRecordRefs),
        notes: form.notes || undefined,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['assurarr'] })
      setForm({
        title: '',
        description: '',
        severity: 'none',
        sourceProduct: 'assurarr',
        sourceObjectRef: '',
        affectedObjectRefs: '',
        ownerPersonId: '',
        holdRef: '',
        releaseType: 'full',
        requestedByPersonId: '',
        requestedAt: '',
        conditions: '',
        expirationAt: '',
        evidenceRecordRefs: '',
        notes: '',
      })
    },
  })

  return (
    <div className="assurarr-page">
      <PageHeader
        title="Quality releases"
        description="Explicit release decisions that unblock held objects after review and evidence checks."
        action={<span className="assurarr-pill"><CheckCircle2 className="h-4 w-4" /> {query.data?.length ?? 0} records</span>}
      />
      <div className="assurarr-card">
        <div className="assurarr-card-inner space-y-4">
          <div className="flex items-center gap-2">
            <Plus className="h-4 w-4 text-cyan-300" />
            <h3 className="text-base font-semibold text-slate-50">Create quality release</h3>
          </div>
          <div className="grid gap-3 md:grid-cols-2">
            <Field label="Release title">
              <input className="assurarr-input" value={form.title} onChange={(event) => setForm({ ...form, title: event.target.value })} />
            </Field>
            <Field label="Release type">
              <select className="assurarr-select" value={form.releaseType} onChange={(event) => setForm({ ...form, releaseType: event.target.value })}>
                <option value="full">Full</option>
                <option value="partial">Partial</option>
                <option value="conditional">Conditional</option>
                <option value="use_as_is">Use as is</option>
                <option value="release_after_rework">Release after rework</option>
                <option value="release_after_sort">Release after sort</option>
              </select>
            </Field>
            <Field label="Severity">
              <select className="assurarr-select" value={form.severity} onChange={(event) => setForm({ ...form, severity: event.target.value })}>
                <option value="none">None</option>
                <option value="low">Low</option>
                <option value="moderate">Moderate</option>
                <option value="high">High</option>
                <option value="critical">Critical</option>
              </select>
            </Field>
            <Field label="Hold ref">
              <input className="assurarr-input" value={form.holdRef} onChange={(event) => setForm({ ...form, holdRef: event.target.value })} placeholder="HOLD-000001" />
            </Field>
            <Field label="Source object ref">
              <input className="assurarr-input" value={form.sourceObjectRef} onChange={(event) => setForm({ ...form, sourceObjectRef: event.target.value })} placeholder="loadarr:inventory:LOT-991" />
            </Field>
            <Field label="Affected object refs" wide>
              <textarea className="assurarr-textarea" value={form.affectedObjectRefs} onChange={(event) => setForm({ ...form, affectedObjectRefs: event.target.value })} />
            </Field>
            <Field label="Evidence record refs" wide>
              <textarea className="assurarr-textarea" value={form.evidenceRecordRefs} onChange={(event) => setForm({ ...form, evidenceRecordRefs: event.target.value })} />
            </Field>
            <Field label="Description" wide>
              <textarea className="assurarr-textarea" value={form.description} onChange={(event) => setForm({ ...form, description: event.target.value })} />
            </Field>
            <Field label="Notes" wide>
              <textarea className="assurarr-textarea" value={form.notes} onChange={(event) => setForm({ ...form, notes: event.target.value })} />
            </Field>
            <Field label="Requested by person id">
              <input className="assurarr-input" value={form.requestedByPersonId} onChange={(event) => setForm({ ...form, requestedByPersonId: event.target.value })} />
            </Field>
            <Field label="Requested at">
              <input className="assurarr-input" type="datetime-local" value={form.requestedAt} onChange={(event) => setForm({ ...form, requestedAt: event.target.value })} />
            </Field>
            <Field label="Expiration at">
              <input className="assurarr-input" type="datetime-local" value={form.expirationAt} onChange={(event) => setForm({ ...form, expirationAt: event.target.value })} />
            </Field>
            <Field label="Conditions" wide>
              <input className="assurarr-input" value={form.conditions} onChange={(event) => setForm({ ...form, conditions: event.target.value })} />
            </Field>
          </div>
          <button className="assurarr-button" type="button" onClick={() => mutation.mutate()} disabled={mutation.isPending}>
            {mutation.isPending ? 'Saving...' : 'Create release'}
          </button>
        </div>
      </div>
      {query.data ? (
        <EntityTable
          items={query.data}
          emptyLabel="No quality releases yet."
          onStatusChange={(id, status) => assurarrApi.updateQualityReleaseStatus(id, status)}
          statusChoices={statusOptions.release}
          detailBasePath="/releases"
        />
      ) : (
        <LoadingCard label="Loading quality releases" />
      )}
    </div>
  )
}

function ReleaseDetailPage() {
  const { id = '' } = useParams()
  const query = useQuery({
    queryKey: ['assurarr', 'release', id],
    queryFn: () => assurarrApi.getQualityRelease(id),
    enabled: Boolean(id),
  })
  const holds = useRecords(['assurarr', 'holds'], assurarrApi.listHolds)
  const dashboard = useDashboard()

  if (query.isLoading) {
    return <LoadingCard label="Loading release detail" />
  }

  if (query.isError || !query.data) {
    return (
      <div className="assurarr-page">
        <PageHeader title="Quality release detail" description="Could not load the requested quality release." />
        <EmptyState title="Quality release not found." />
      </div>
    )
  }

  const release = query.data
  const relatedHold = holds.data?.find((hold) => hold.number === release.holdRef) ?? null
  const timeline = dashboard.data?.recentEvents.filter((event) => event.subjectType === 'release' && event.subjectId === release.id) ?? []

  return (
    <div className="assurarr-page">
      <PageHeader
        title={`${release.number} · ${release.title}`}
        description="Release decision, evidence, and the hold context that this record unblocks."
      />
      <div className="space-y-4">
        <div className="assurarr-grid cols-2">
          <div className="assurarr-card">
            <div className="assurarr-card-inner space-y-3">
              <p className="assurarr-label">Overview</p>
              <div className="flex flex-wrap gap-2 text-sm">
                <span className="assurarr-pill">{release.status}</span>
                <span className="assurarr-pill">{release.severity}</span>
                <span className="assurarr-pill">{release.releaseType}</span>
              </div>
              <p className="text-sm text-slate-300">Release decision for the linked held object and its downstream workflow.</p>
              <div className="grid gap-2 text-sm text-slate-300 md:grid-cols-2">
                <div><span className="text-slate-500">Hold ref:</span> {release.holdRef}</div>
                <div><span className="text-slate-500">Source product:</span> {release.sourceProduct ?? 'manual'}</div>
                <div><span className="text-slate-500">Source object:</span> {release.sourceObjectRef ?? 'n/a'}</div>
                <div><span className="text-slate-500">Owner:</span> {release.ownerPersonId ?? 'unassigned'}</div>
                <div><span className="text-slate-500">Requested:</span> {release.requestedAt ? new Date(release.requestedAt).toLocaleString() : 'n/a'}</div>
                <div><span className="text-slate-500">Approved:</span> {release.approvedAt ? new Date(release.approvedAt).toLocaleString() : 'n/a'}</div>
                <div><span className="text-slate-500">Executed:</span> {release.executedAt ? new Date(release.executedAt).toLocaleString() : 'n/a'}</div>
                <div><span className="text-slate-500">Expiration:</span> {release.expirationAt ? new Date(release.expirationAt).toLocaleString() : 'n/a'}</div>
              </div>
            </div>
          </div>
          <div className="assurarr-card">
            <div className="assurarr-card-inner space-y-3">
              <p className="assurarr-label">Evidence and conditions</p>
              <div className="text-sm text-slate-300">
                <div><span className="text-slate-500">Conditions:</span> {release.conditions ?? 'none'}</div>
                <div><span className="text-slate-500">Requested by:</span> {release.requestedByPersonId ?? 'n/a'}</div>
                <div><span className="text-slate-500">Approved by:</span> {release.approvedByPersonId ?? 'n/a'}</div>
                <div><span className="text-slate-500">Evidence refs:</span> {release.evidenceRecordRefs.length ? release.evidenceRecordRefs.join(', ') : 'none'}</div>
                <div><span className="text-slate-500">Notes:</span> {release.notes ?? 'n/a'}</div>
              </div>
            </div>
          </div>
        </div>

        <SectionCard
          title="Affected objects"
          items={release.affectedObjectRefs.map((ref) => ref)}
          emptyLabel="No affected objects recorded."
        />
        <SectionCard
          title="Related hold"
          items={relatedHold ? [`${relatedHold.number} · ${relatedHold.title} · ${relatedHold.status}`] : []}
          emptyLabel="No matching hold was found for this release."
        />
        <SectionCard
          title="Timeline"
          items={timeline.map((event) => `${event.eventType} · ${new Date(event.occurredAt).toLocaleString()}`)}
          emptyLabel="No timeline events recorded yet."
        />
      </div>
    </div>
  )
}

function ContainmentPage() {
  const query = useRecords(['assurarr', 'containment'], assurarrApi.listContainmentActions)
  const [form, setForm] = useState({
    title: '',
    description: '',
    severity: 'moderate',
    actionType: 'quarantine',
    sourceProduct: 'loadarr',
    sourceObjectRef: '',
    affectedObjectRefs: '',
    nonconformanceRef: '',
    assignedPersonId: '',
    assignedTeamRef: '',
    sourceProductActionRef: '',
    dueAt: '',
    evidenceRecordRefs: '',
    notes: '',
  })
  const queryClient = useQueryClient()
  const mutation = useMutation({
    mutationFn: async () =>
      assurarrApi.createContainmentAction({
        title: form.title,
        description: form.description,
        severity: form.severity,
        actionType: form.actionType,
        sourceProduct: form.sourceProduct,
        sourceObjectRef: form.sourceObjectRef,
        affectedObjectRefs: joinRefs(form.affectedObjectRefs),
        nonconformanceRef: form.nonconformanceRef || undefined,
        assignedPersonId: form.assignedPersonId || undefined,
        assignedTeamRef: form.assignedTeamRef || undefined,
        sourceProductActionRef: form.sourceProductActionRef || undefined,
        dueAt: form.dueAt || undefined,
        evidenceRecordRefs: joinRefs(form.evidenceRecordRefs),
        notes: form.notes || undefined,
        verificationRequired: true,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['assurarr'] })
      setForm({
        title: '',
        description: '',
        severity: 'moderate',
        actionType: 'quarantine',
        sourceProduct: 'loadarr',
        sourceObjectRef: '',
        affectedObjectRefs: '',
        nonconformanceRef: '',
        assignedPersonId: '',
        assignedTeamRef: '',
        sourceProductActionRef: '',
        dueAt: '',
        evidenceRecordRefs: '',
        notes: '',
      })
    },
  })

  return (
    <div className="assurarr-page">
      <PageHeader
        title="Containment actions"
        description="Immediate actions that stop spread, isolate issues, and protect downstream work."
        action={<span className="assurarr-pill"><ShieldCheck className="h-4 w-4" /> {query.data?.length ?? 0} records</span>}
      />
      <div className="assurarr-card">
        <div className="assurarr-card-inner space-y-4">
          <div className="flex items-center gap-2">
            <Plus className="h-4 w-4 text-cyan-300" />
            <h3 className="text-base font-semibold text-slate-50">Create containment action</h3>
          </div>
          <div className="grid gap-3 md:grid-cols-2">
            <Field label="Title">
              <input className="assurarr-input" value={form.title} onChange={(event) => setForm({ ...form, title: event.target.value })} />
            </Field>
            <Field label="Action type">
              <select className="assurarr-select" value={form.actionType} onChange={(event) => setForm({ ...form, actionType: event.target.value })}>
                <option value="isolate">Isolate</option>
                <option value="quarantine">Quarantine</option>
                <option value="stop_ship">Stop ship</option>
                <option value="stop_use">Stop use</option>
                <option value="notify_customer">Notify customer</option>
                <option value="notify_supplier">Notify supplier</option>
                <option value="inspect_all">Inspect all</option>
                <option value="sort">Sort</option>
                <option value="retrain">Retrain</option>
                <option value="repair">Repair</option>
                <option value="rework">Rework</option>
                <option value="block_order">Block order</option>
                <option value="block_supplier">Block supplier</option>
                <option value="block_asset">Block asset</option>
                <option value="hold_inventory">Hold inventory</option>
                <option value="other">Other</option>
              </select>
            </Field>
            <Field label="Severity">
              <select className="assurarr-select" value={form.severity} onChange={(event) => setForm({ ...form, severity: event.target.value })}>
                <option value="low">Low</option>
                <option value="moderate">Moderate</option>
                <option value="high">High</option>
                <option value="critical">Critical</option>
              </select>
            </Field>
            <Field label="Nonconformance ref">
              <input className="assurarr-input" value={form.nonconformanceRef} onChange={(event) => setForm({ ...form, nonconformanceRef: event.target.value })} />
            </Field>
            <Field label="Source object ref">
              <input className="assurarr-input" value={form.sourceObjectRef} onChange={(event) => setForm({ ...form, sourceObjectRef: event.target.value })} />
            </Field>
            <Field label="Affected object refs" wide>
              <textarea className="assurarr-textarea" value={form.affectedObjectRefs} onChange={(event) => setForm({ ...form, affectedObjectRefs: event.target.value })} />
            </Field>
            <Field label="Evidence record refs" wide>
              <textarea className="assurarr-textarea" value={form.evidenceRecordRefs} onChange={(event) => setForm({ ...form, evidenceRecordRefs: event.target.value })} />
            </Field>
            <Field label="Description" wide>
              <textarea className="assurarr-textarea" value={form.description} onChange={(event) => setForm({ ...form, description: event.target.value })} />
            </Field>
            <Field label="Assigned team ref">
              <input className="assurarr-input" value={form.assignedTeamRef} onChange={(event) => setForm({ ...form, assignedTeamRef: event.target.value })} />
            </Field>
            <Field label="Assigned person id">
              <input className="assurarr-input" value={form.assignedPersonId} onChange={(event) => setForm({ ...form, assignedPersonId: event.target.value })} />
            </Field>
            <Field label="Source action ref">
              <input className="assurarr-input" value={form.sourceProductActionRef} onChange={(event) => setForm({ ...form, sourceProductActionRef: event.target.value })} />
            </Field>
            <Field label="Due at">
              <input className="assurarr-input" type="datetime-local" value={form.dueAt} onChange={(event) => setForm({ ...form, dueAt: event.target.value })} />
            </Field>
            <Field label="Notes" wide>
              <textarea className="assurarr-textarea" value={form.notes} onChange={(event) => setForm({ ...form, notes: event.target.value })} />
            </Field>
          </div>
          <button className="assurarr-button" type="button" onClick={() => mutation.mutate()} disabled={mutation.isPending}>
            {mutation.isPending ? 'Saving...' : 'Create containment action'}
          </button>
        </div>
      </div>
      {query.data ? (
        <EntityTable
          items={query.data}
          emptyLabel="No containment actions yet."
          onStatusChange={(id, status) => assurarrApi.updateContainmentActionStatus(id, status)}
          statusChoices={statusOptions.containment}
          detailBasePath="/containment"
        />
      ) : (
        <LoadingCard label="Loading containment actions" />
      )}
    </div>
  )
}

function DispositionPage() {
  const query = useRecords(['assurarr', 'dispositions'], assurarrApi.listDispositions)
  const [form, setForm] = useState({
    title: '',
    description: '',
    severity: 'moderate',
    dispositionType: 'conditional_release',
    sourceProduct: 'assurarr',
    sourceObjectRef: '',
    affectedObjectRefs: '',
    nonconformanceRef: '',
    decisionByPersonId: '',
    decisionAt: '',
    approvedByPersonId: '',
    approvedAt: '',
    rationale: '',
    requiredActions: '',
    executionProduct: '',
    executionObjectRef: '',
    evidenceRecordRefs: '',
    notes: '',
  })
  const queryClient = useQueryClient()
  const mutation = useMutation({
    mutationFn: async () =>
      assurarrApi.createDisposition({
        title: form.title,
        description: form.description,
        severity: form.severity,
        dispositionType: form.dispositionType,
        sourceProduct: form.sourceProduct,
        sourceObjectRef: form.sourceObjectRef,
        affectedObjectRefs: joinRefs(form.affectedObjectRefs),
        nonconformanceRef: form.nonconformanceRef || undefined,
        decisionByPersonId: form.decisionByPersonId || undefined,
        decisionAt: form.decisionAt || undefined,
        approvedByPersonId: form.approvedByPersonId || undefined,
        approvedAt: form.approvedAt || undefined,
        rationale: form.rationale || undefined,
        requiredActions: joinRefs(form.requiredActions),
        executionProduct: form.executionProduct || undefined,
        executionObjectRef: form.executionObjectRef || undefined,
        evidenceRecordRefs: joinRefs(form.evidenceRecordRefs),
        notes: form.notes || undefined,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['assurarr'] })
      setForm({
        title: '',
        description: '',
        severity: 'moderate',
        dispositionType: 'conditional_release',
        sourceProduct: 'assurarr',
        sourceObjectRef: '',
        affectedObjectRefs: '',
        nonconformanceRef: '',
        decisionByPersonId: '',
        decisionAt: '',
        approvedByPersonId: '',
        approvedAt: '',
        rationale: '',
        requiredActions: '',
        executionProduct: '',
        executionObjectRef: '',
        evidenceRecordRefs: '',
        notes: '',
      })
    },
  })

  return (
    <div className="assurarr-page">
      <PageHeader
        title="Dispositions"
        description="Decisions for what happens to the affected object after investigation and review."
        action={<span className="assurarr-pill"><ClipboardList className="h-4 w-4" /> {query.data?.length ?? 0} records</span>}
      />
      <div className="assurarr-card">
        <div className="assurarr-card-inner space-y-4">
          <div className="flex items-center gap-2">
            <Plus className="h-4 w-4 text-cyan-300" />
            <h3 className="text-base font-semibold text-slate-50">Create disposition</h3>
          </div>
          <div className="grid gap-3 md:grid-cols-2">
            <Field label="Title">
              <input className="assurarr-input" value={form.title} onChange={(event) => setForm({ ...form, title: event.target.value })} />
            </Field>
            <Field label="Disposition type">
              <select className="assurarr-select" value={form.dispositionType} onChange={(event) => setForm({ ...form, dispositionType: event.target.value })}>
                <option value="use_as_is">Use as is</option>
                <option value="rework">Rework</option>
                <option value="repair">Repair</option>
                <option value="return_to_supplier">Return to supplier</option>
                <option value="scrap">Scrap</option>
                <option value="sort">Sort</option>
                <option value="regrade">Regrade</option>
                <option value="reject">Reject</option>
                <option value="replace">Replace</option>
                <option value="conditional_release">Conditional release</option>
                <option value="release_no_action">Release no action</option>
              </select>
            </Field>
            <Field label="Severity">
              <select className="assurarr-select" value={form.severity} onChange={(event) => setForm({ ...form, severity: event.target.value })}>
                <option value="low">Low</option>
                <option value="moderate">Moderate</option>
                <option value="high">High</option>
                <option value="critical">Critical</option>
              </select>
            </Field>
            <Field label="Nonconformance ref">
              <input className="assurarr-input" value={form.nonconformanceRef} onChange={(event) => setForm({ ...form, nonconformanceRef: event.target.value })} />
            </Field>
            <Field label="Source object ref">
              <input className="assurarr-input" value={form.sourceObjectRef} onChange={(event) => setForm({ ...form, sourceObjectRef: event.target.value })} />
            </Field>
            <Field label="Affected object refs" wide>
              <textarea className="assurarr-textarea" value={form.affectedObjectRefs} onChange={(event) => setForm({ ...form, affectedObjectRefs: event.target.value })} />
            </Field>
            <Field label="Required actions" wide>
              <textarea className="assurarr-textarea" value={form.requiredActions} onChange={(event) => setForm({ ...form, requiredActions: event.target.value })} />
            </Field>
            <Field label="Evidence record refs" wide>
              <textarea className="assurarr-textarea" value={form.evidenceRecordRefs} onChange={(event) => setForm({ ...form, evidenceRecordRefs: event.target.value })} />
            </Field>
            <Field label="Description" wide>
              <textarea className="assurarr-textarea" value={form.description} onChange={(event) => setForm({ ...form, description: event.target.value })} />
            </Field>
            <Field label="Rationale" wide>
              <textarea className="assurarr-textarea" value={form.rationale} onChange={(event) => setForm({ ...form, rationale: event.target.value })} />
            </Field>
            <Field label="Execution product">
              <input className="assurarr-input" value={form.executionProduct} onChange={(event) => setForm({ ...form, executionProduct: event.target.value })} />
            </Field>
            <Field label="Execution object ref">
              <input className="assurarr-input" value={form.executionObjectRef} onChange={(event) => setForm({ ...form, executionObjectRef: event.target.value })} />
            </Field>
            <Field label="Decision by person id">
              <input className="assurarr-input" value={form.decisionByPersonId} onChange={(event) => setForm({ ...form, decisionByPersonId: event.target.value })} />
            </Field>
            <Field label="Decision at">
              <input className="assurarr-input" type="datetime-local" value={form.decisionAt} onChange={(event) => setForm({ ...form, decisionAt: event.target.value })} />
            </Field>
            <Field label="Approved by person id">
              <input className="assurarr-input" value={form.approvedByPersonId} onChange={(event) => setForm({ ...form, approvedByPersonId: event.target.value })} />
            </Field>
            <Field label="Approved at">
              <input className="assurarr-input" type="datetime-local" value={form.approvedAt} onChange={(event) => setForm({ ...form, approvedAt: event.target.value })} />
            </Field>
            <Field label="Notes" wide>
              <textarea className="assurarr-textarea" value={form.notes} onChange={(event) => setForm({ ...form, notes: event.target.value })} />
            </Field>
          </div>
          <button className="assurarr-button" type="button" onClick={() => mutation.mutate()} disabled={mutation.isPending}>
            {mutation.isPending ? 'Saving...' : 'Create disposition'}
          </button>
        </div>
      </div>
      {query.data ? (
        <EntityTable
          items={query.data}
          emptyLabel="No dispositions yet."
          onStatusChange={(id, status) => assurarrApi.updateDispositionStatus(id, status)}
          statusChoices={statusOptions.disposition}
          detailBasePath="/dispositions"
        />
      ) : (
        <LoadingCard label="Loading dispositions" />
      )}
    </div>
  )
}

function ContainmentDetailPage() {
  const { id = '' } = useParams()
  const query = useQuery({
    queryKey: ['assurarr', 'containment-action', id],
    queryFn: () => assurarrApi.getContainmentAction(id),
    enabled: Boolean(id),
  })
  const dashboard = useDashboard()

  if (!query.data) {
    return <LoadingCard label="Loading containment detail" />
  }

  const action = query.data
  const timeline = dashboard.data?.recentEvents.filter((event) => event.subjectType === 'containment' && event.subjectId === action.id) ?? []

  return (
    <div className="assurarr-page">
      <PageHeader
        title={`${action.number} · ${action.title}`}
        description="Containment decisions, affected objects, and the execution trail for the action."
      />
      <div className="space-y-4">
        <div className="assurarr-grid cols-2">
          <div className="assurarr-card">
            <div className="assurarr-card-inner space-y-3">
              <p className="assurarr-label">Overview</p>
              <div className="flex flex-wrap gap-2 text-sm">
                <span className="assurarr-pill">{action.status}</span>
                <span className="assurarr-pill">{action.severity}</span>
                <span className="assurarr-pill">{action.actionType}</span>
                {action.verificationRequired ? <span className="assurarr-pill">verification required</span> : null}
              </div>
              <p className="text-sm text-slate-300">{action.notes ?? action.closureSummary ?? 'No notes recorded.'}</p>
              <div className="grid gap-2 text-sm text-slate-300 md:grid-cols-2">
                <div><span className="text-slate-500">Source product:</span> {action.sourceProduct ?? 'manual'}</div>
                <div><span className="text-slate-500">Source object:</span> {action.sourceObjectRef ?? 'n/a'}</div>
                <div><span className="text-slate-500">Nonconformance ref:</span> {action.nonconformanceRef ?? 'n/a'}</div>
                <div><span className="text-slate-500">Assigned person:</span> {action.assignedPersonId ?? 'n/a'}</div>
                <div><span className="text-slate-500">Assigned team:</span> {action.assignedTeamRef ?? 'n/a'}</div>
                <div><span className="text-slate-500">Due:</span> {action.dueAt ? new Date(action.dueAt).toLocaleString() : 'n/a'}</div>
              </div>
            </div>
          </div>
          <div className="assurarr-card">
            <div className="assurarr-card-inner space-y-3">
              <p className="assurarr-label">Execution</p>
              <div className="grid gap-2 text-sm text-slate-300">
                <div><span className="text-slate-500">Started:</span> {action.startedAt ? new Date(action.startedAt).toLocaleString() : 'n/a'}</div>
                <div><span className="text-slate-500">Completed:</span> {action.completedAt ? new Date(action.completedAt).toLocaleString() : 'n/a'}</div>
                <div><span className="text-slate-500">Verified:</span> {action.verificationRequired ? (action.verifiedAt ? new Date(action.verifiedAt).toLocaleString() : 'pending') : 'not required'}</div>
                <div><span className="text-slate-500">Closed:</span> {action.closedAt ? new Date(action.closedAt).toLocaleString() : 'n/a'}</div>
              </div>
              <p className="text-sm text-slate-300">{action.notes ?? 'No execution notes recorded.'}</p>
            </div>
          </div>
        </div>

        <SectionCard title="Affected objects" items={action.affectedObjectRefs} emptyLabel="No affected objects recorded." />
        <SectionCard title="Evidence" items={action.evidenceRecordRefs} emptyLabel="No evidence records recorded." />
        <SectionCard title="Timeline" items={timeline.map((event) => `${event.eventType} · ${new Date(event.occurredAt).toLocaleString()}`)} emptyLabel="No timeline events recorded yet." />
      </div>
    </div>
  )
}

function DispositionDetailPage() {
  const { id = '' } = useParams()
  const query = useQuery({
    queryKey: ['assurarr', 'disposition', id],
    queryFn: () => assurarrApi.getDisposition(id),
    enabled: Boolean(id),
  })
  const dashboard = useDashboard()

  if (!query.data) {
    return <LoadingCard label="Loading disposition detail" />
  }

  const disposition = query.data
  const timeline = dashboard.data?.recentEvents.filter((event) => event.subjectType === 'disposition' && event.subjectId === disposition.id) ?? []

  return (
    <div className="assurarr-page">
      <PageHeader
        title={`${disposition.number} · ${disposition.title}`}
        description="Disposition decision, execution context, evidence, and timeline."
      />
      <div className="space-y-4">
        <div className="assurarr-grid cols-2">
          <div className="assurarr-card">
            <div className="assurarr-card-inner space-y-3">
              <p className="assurarr-label">Overview</p>
              <div className="flex flex-wrap gap-2 text-sm">
                <span className="assurarr-pill">{disposition.status}</span>
                <span className="assurarr-pill">{disposition.severity}</span>
                <span className="assurarr-pill">{disposition.dispositionType}</span>
                {disposition.executionProduct ? <span className="assurarr-pill">{disposition.executionProduct}</span> : null}
              </div>
              <p className="text-sm text-slate-300">{disposition.rationale ?? disposition.notes ?? 'No rationale recorded.'}</p>
              <div className="grid gap-2 text-sm text-slate-300 md:grid-cols-2">
                <div><span className="text-slate-500">Source product:</span> {disposition.sourceProduct ?? 'manual'}</div>
                <div><span className="text-slate-500">Source object:</span> {disposition.sourceObjectRef ?? 'n/a'}</div>
                <div><span className="text-slate-500">Nonconformance ref:</span> {disposition.nonconformanceRef ?? 'n/a'}</div>
                <div><span className="text-slate-500">Decision by:</span> {disposition.decisionByPersonId ?? 'n/a'}</div>
                <div><span className="text-slate-500">Decision at:</span> {disposition.decisionAt ? new Date(disposition.decisionAt).toLocaleString() : 'n/a'}</div>
                <div><span className="text-slate-500">Approved at:</span> {disposition.approvedAt ? new Date(disposition.approvedAt).toLocaleString() : 'n/a'}</div>
              </div>
            </div>
          </div>
          <div className="assurarr-card">
            <div className="assurarr-card-inner space-y-3">
              <p className="assurarr-label">Execution</p>
              <div className="grid gap-2 text-sm text-slate-300">
                <div><span className="text-slate-500">Execution object:</span> {disposition.executionObjectRef ?? 'n/a'}</div>
                <div><span className="text-slate-500">Closed:</span> {disposition.closedAt ? new Date(disposition.closedAt).toLocaleString() : 'n/a'}</div>
                <div><span className="text-slate-500">Closed by:</span> {disposition.closedByPersonId ?? 'n/a'}</div>
              </div>
              <p className="text-sm text-slate-300">{disposition.closureSummary ?? 'No closure summary recorded.'}</p>
            </div>
          </div>
        </div>

        <SectionCard title="Affected objects" items={disposition.affectedObjectRefs} emptyLabel="No affected objects recorded." />
        <SectionCard title="Required actions" items={disposition.requiredActions} emptyLabel="No required actions recorded." />
        <SectionCard title="Evidence" items={disposition.evidenceRecordRefs} emptyLabel="No evidence records recorded." />
        <SectionCard title="Timeline" items={timeline.map((event) => `${event.eventType} · ${new Date(event.occurredAt).toLocaleString()}`)} emptyLabel="No timeline events recorded yet." />
      </div>
    </div>
  )
}

function SupplierQualityPage() {
  const query = useRecords(['assurarr', 'supplier-quality'], assurarrApi.listSupplierQualityIssues)
  const [form, setForm] = useState({
    title: '',
    description: '',
    severity: 'moderate',
    issueType: 'damaged_received',
    sourceProduct: 'loadarr',
    sourceObjectRef: '',
    affectedReceiptRefs: '',
    affectedPurchaseOrderRefs: '',
    affectedItemRefs: '',
    supplierRef: '',
    nonconformanceRef: '',
    scarRef: '',
    holdRefs: '',
    recordRefs: '',
    ownerPersonId: '',
    openedAt: '',
  })
  const queryClient = useQueryClient()
  const mutation = useMutation({
    mutationFn: async () =>
      assurarrApi.createSupplierQualityIssue({
        title: form.title,
        description: form.description,
        severity: form.severity,
        issueType: form.issueType,
        sourceProduct: form.sourceProduct,
        sourceObjectRef: form.sourceObjectRef,
        affectedObjectRefs: joinRefs(form.affectedReceiptRefs)
          .concat(joinRefs(form.affectedPurchaseOrderRefs), joinRefs(form.affectedItemRefs)),
        affectedReceiptRefs: joinRefs(form.affectedReceiptRefs),
        affectedPurchaseOrderRefs: joinRefs(form.affectedPurchaseOrderRefs),
        affectedItemRefs: joinRefs(form.affectedItemRefs),
        supplierRef: form.supplierRef || undefined,
        nonconformanceRef: form.nonconformanceRef || undefined,
        scarRef: form.scarRef || undefined,
        holdRefs: joinRefs(form.holdRefs),
        recordRefs: joinRefs(form.recordRefs),
        ownerPersonId: form.ownerPersonId || undefined,
        openedAt: form.openedAt || undefined,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['assurarr'] })
      setForm({
        title: '',
        description: '',
        severity: 'moderate',
        issueType: 'damaged_received',
        sourceProduct: 'loadarr',
        sourceObjectRef: '',
        affectedReceiptRefs: '',
        affectedPurchaseOrderRefs: '',
        affectedItemRefs: '',
        supplierRef: '',
        nonconformanceRef: '',
        scarRef: '',
        holdRefs: '',
        recordRefs: '',
        ownerPersonId: '',
        openedAt: '',
      })
    },
  })

  return (
    <div className="assurarr-page">
      <PageHeader
        title="Supplier quality"
        description="Track supplier-responsible quality issues, SCARs, and held receipts."
        action={<span className="assurarr-pill"><ShieldAlert className="h-4 w-4" /> {query.data?.length ?? 0} records</span>}
      />
      <div className="assurarr-card">
        <div className="assurarr-card-inner space-y-4">
          <div className="flex items-center gap-2">
            <Plus className="h-4 w-4 text-cyan-300" />
            <h3 className="text-base font-semibold text-slate-50">Create supplier quality issue</h3>
          </div>
          <div className="grid gap-3 md:grid-cols-2">
            <Field label="Title">
              <input className="assurarr-input" value={form.title} onChange={(event) => setForm({ ...form, title: event.target.value })} />
            </Field>
            <Field label="Issue type">
              <select className="assurarr-select" value={form.issueType} onChange={(event) => setForm({ ...form, issueType: event.target.value })}>
                <option value="damaged_received">Damaged received</option>
                <option value="wrong_item">Wrong item</option>
                <option value="late_with_quality_impact">Late with quality impact</option>
                <option value="missing_document">Missing document</option>
                <option value="invalid_document">Invalid document</option>
                <option value="failed_specification">Failed specification</option>
                <option value="recurring_defect">Recurring defect</option>
                <option value="packaging_failure">Packaging failure</option>
                <option value="labeling_failure">Labeling failure</option>
                <option value="other">Other</option>
              </select>
            </Field>
            <Field label="Severity">
              <select className="assurarr-select" value={form.severity} onChange={(event) => setForm({ ...form, severity: event.target.value })}>
                <option value="low">Low</option>
                <option value="moderate">Moderate</option>
                <option value="high">High</option>
                <option value="critical">Critical</option>
              </select>
            </Field>
            <Field label="Supplier ref">
              <input className="assurarr-input" value={form.supplierRef} onChange={(event) => setForm({ ...form, supplierRef: event.target.value })} placeholder="supplyarr:supplier:acme" />
            </Field>
            <Field label="Source object ref">
              <input className="assurarr-input" value={form.sourceObjectRef} onChange={(event) => setForm({ ...form, sourceObjectRef: event.target.value })} />
            </Field>
            <Field label="Affected receipt refs" wide>
              <textarea className="assurarr-textarea" value={form.affectedReceiptRefs} onChange={(event) => setForm({ ...form, affectedReceiptRefs: event.target.value })} />
            </Field>
            <Field label="Affected PO refs" wide>
              <textarea className="assurarr-textarea" value={form.affectedPurchaseOrderRefs} onChange={(event) => setForm({ ...form, affectedPurchaseOrderRefs: event.target.value })} />
            </Field>
            <Field label="Affected item refs" wide>
              <textarea className="assurarr-textarea" value={form.affectedItemRefs} onChange={(event) => setForm({ ...form, affectedItemRefs: event.target.value })} />
            </Field>
            <Field label="Hold refs" wide>
              <textarea className="assurarr-textarea" value={form.holdRefs} onChange={(event) => setForm({ ...form, holdRefs: event.target.value })} />
            </Field>
            <Field label="Record refs" wide>
              <textarea className="assurarr-textarea" value={form.recordRefs} onChange={(event) => setForm({ ...form, recordRefs: event.target.value })} />
            </Field>
            <Field label="Description" wide>
              <textarea className="assurarr-textarea" value={form.description} onChange={(event) => setForm({ ...form, description: event.target.value })} />
            </Field>
            <Field label="Nonconformance ref">
              <input className="assurarr-input" value={form.nonconformanceRef} onChange={(event) => setForm({ ...form, nonconformanceRef: event.target.value })} />
            </Field>
            <Field label="SCAR ref">
              <input className="assurarr-input" value={form.scarRef} onChange={(event) => setForm({ ...form, scarRef: event.target.value })} placeholder="SCAR-000001" />
            </Field>
            <Field label="Owner person id">
              <input className="assurarr-input" value={form.ownerPersonId} onChange={(event) => setForm({ ...form, ownerPersonId: event.target.value })} />
            </Field>
            <Field label="Opened at">
              <input className="assurarr-input" type="datetime-local" value={form.openedAt} onChange={(event) => setForm({ ...form, openedAt: event.target.value })} />
            </Field>
          </div>
          <button className="assurarr-button" type="button" onClick={() => mutation.mutate()} disabled={mutation.isPending}>
            {mutation.isPending ? 'Saving...' : 'Create supplier quality issue'}
          </button>
        </div>
      </div>
      {query.data ? (
        <EntityTable
          items={query.data}
          emptyLabel="No supplier quality issues yet."
          onStatusChange={(id, status) => assurarrApi.updateSupplierQualityIssueStatus(id, status)}
          statusChoices={statusOptions.supplierQuality}
          detailBasePath="/supplier-quality"
        />
      ) : (
        <LoadingCard label="Loading supplier quality issues" />
      )}
    </div>
  )
}

function SupplierQualityDetailPage() {
  const { id = '' } = useParams()
  const query = useQuery({
    queryKey: ['assurarr', 'supplier-quality-issue', id],
    queryFn: () => assurarrApi.getSupplierQualityIssue(id),
    enabled: Boolean(id),
  })
  const nonconformances = useRecords(['assurarr', 'nonconformances'], assurarrApi.listNonconformances)
  const scars = useRecords(['assurarr', 'scars'], assurarrApi.listScars)
  const holds = useRecords(['assurarr', 'holds'], assurarrApi.listHolds)
  const dashboard = useDashboard()

  if (query.isLoading) {
    return <LoadingCard label="Loading supplier quality detail" />
  }

  if (query.isError || !query.data) {
    return (
      <div className="assurarr-page">
        <PageHeader title="Supplier quality detail" description="Could not load the requested supplier quality issue." />
        <EmptyState title="Supplier quality issue not found." />
      </div>
    )
  }

  const issue = query.data
  const relatedNonconformance = nonconformances.data?.find((item) => item.number === issue.nonconformanceRef) ?? null
  const relatedScar = scars.data?.find((item) => item.number === issue.scarRef) ?? null
  const relatedHolds = holds.data?.filter((hold) => issue.holdRefs.includes(hold.number) || issue.holdRefs.includes(hold.id)) ?? []
  const timeline = dashboard.data?.recentEvents.filter(
    (event) => event.subjectType === 'supplier_quality_issue' && event.subjectId === issue.id,
  ) ?? []

  return (
    <div className="assurarr-page">
      <PageHeader
        title={`${issue.number} · ${issue.title}`}
        description="Supplier quality issue detail, linked records, and the activity trail for the supplier-facing workflow."
      />
      <div className="space-y-4">
        <div className="assurarr-grid cols-2">
          <div className="assurarr-card">
            <div className="assurarr-card-inner space-y-3">
              <p className="assurarr-label">Overview</p>
              <div className="flex flex-wrap gap-2 text-sm">
                <span className="assurarr-pill">{issue.status}</span>
                <span className="assurarr-pill">{issue.severity}</span>
                <span className="assurarr-pill">{issue.issueType}</span>
              </div>
              <p className="text-sm text-slate-300">Supplier quality workflow record for supplier-facing issue tracking and closure.</p>
              <div className="grid gap-2 text-sm text-slate-300 md:grid-cols-2">
                <div><span className="text-slate-500">Source product:</span> {issue.sourceProduct ?? 'manual'}</div>
                <div><span className="text-slate-500">Source object:</span> {issue.sourceObjectRef ?? 'n/a'}</div>
                <div><span className="text-slate-500">Supplier:</span> {issue.supplierRef ?? 'unassigned'}</div>
                <div><span className="text-slate-500">Owner:</span> {issue.ownerPersonId ?? 'unassigned'}</div>
                <div><span className="text-slate-500">Opened:</span> {issue.openedAt ? new Date(issue.openedAt).toLocaleString() : 'n/a'}</div>
                <div><span className="text-slate-500">Closed:</span> {issue.closedAt ? new Date(issue.closedAt).toLocaleString() : 'n/a'}</div>
              </div>
              <div className="grid gap-2 text-sm text-slate-300">
                <div><span className="text-slate-500">Closure summary:</span> {issue.closureSummary ?? 'n/a'}</div>
                <div><span className="text-slate-500">Closed by:</span> {issue.closedByPersonId ?? 'n/a'}</div>
              </div>
            </div>
          </div>
          <div className="assurarr-card">
            <div className="assurarr-card-inner space-y-3">
              <p className="assurarr-label">Linked records</p>
              <div className="space-y-2 text-sm text-slate-300">
                <div>
                  <span className="text-slate-500">Nonconformance:</span>{' '}
                  {relatedNonconformance ? (
                    <Link to={`/nonconformances/${relatedNonconformance.id}`} className="text-cyan-300 hover:text-cyan-200">
                      {relatedNonconformance.number}
                    </Link>
                  ) : (
                    issue.nonconformanceRef ?? 'none'
                  )}
                </div>
                <div>
                  <span className="text-slate-500">SCAR:</span>{' '}
                  {relatedScar ? (
                    <Link to={`/scars/${relatedScar.id}`} className="text-cyan-300 hover:text-cyan-200">
                      {relatedScar.number}
                    </Link>
                  ) : (
                    issue.scarRef ?? 'none'
                  )}
                </div>
                <div>
                  <span className="text-slate-500">Holds:</span>{' '}
                  {relatedHolds.length > 0 ? (
                    <span className="flex flex-wrap gap-2 pt-1">
                      {relatedHolds.map((hold) => (
                        <Link key={hold.id} to={`/holds/${hold.id}`} className="assurarr-pill">
                          {hold.number}
                        </Link>
                      ))}
                    </span>
                  ) : (
                    'none'
                  )}
                </div>
                <div>
                  <span className="text-slate-500">Record refs:</span>{' '}
                  {issue.recordRefs.length > 0 ? issue.recordRefs.join(', ') : 'none'}
                </div>
                <div>
                  <span className="text-slate-500">Affected refs:</span>{' '}
                  {issue.affectedObjectRefs.length > 0 ? issue.affectedObjectRefs.join(', ') : 'none'}
                </div>
              </div>
            </div>
          </div>
        </div>
        <div className="assurarr-card">
          <div className="assurarr-card-inner space-y-3">
            <p className="assurarr-label">Timeline</p>
            <div className="space-y-2">
              {timeline.length === 0 ? <EmptyState title="No timeline yet." /> : null}
              {timeline.map((event) => (
                <div key={event.id} className="rounded-xl border border-slate-700/70 bg-slate-900/80 p-3">
                  <div className="flex items-center justify-between gap-3">
                    <strong className="text-sm text-slate-100">{event.eventType}</strong>
                    <time className="text-xs text-slate-400">{new Date(event.occurredAt).toLocaleString()}</time>
                  </div>
                  <p className="mt-1 text-sm text-slate-300">{event.details ?? event.subjectType}</p>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}

function ScarPage() {
  const query = useRecords(['assurarr', 'scars'], assurarrApi.listScars)
  const [form, setForm] = useState({
    title: '',
    description: '',
    severity: 'high',
    sourceProduct: 'assurarr',
    sourceObjectRef: '',
    affectedObjectRefs: '',
    supplierRef: '',
    sourceNonconformanceRef: '',
    sourceCapaRef: '',
    requestedByPersonId: '',
    requestedAt: '',
    supplierDueAt: '',
    supplierResponseRecordRefs: '',
    reviewPersonId: '',
    reviewedAt: '',
    reviewDecision: '',
    followUpCapaRef: '',
    recordRefs: '',
    ownerPersonId: '',
  })
  const queryClient = useQueryClient()
  const mutation = useMutation({
    mutationFn: async () =>
      assurarrApi.createScar({
        title: form.title,
        description: form.description,
        severity: form.severity,
        sourceProduct: form.sourceProduct,
        sourceObjectRef: form.sourceObjectRef,
        affectedObjectRefs: joinRefs(form.affectedObjectRefs),
        supplierRef: form.supplierRef || undefined,
        sourceNonconformanceRef: form.sourceNonconformanceRef || undefined,
        sourceCapaRef: form.sourceCapaRef || undefined,
        requestedByPersonId: form.requestedByPersonId || undefined,
        requestedAt: form.requestedAt || undefined,
        supplierDueAt: form.supplierDueAt || undefined,
        supplierResponseRecordRefs: joinRefs(form.supplierResponseRecordRefs),
        reviewPersonId: form.reviewPersonId || undefined,
        reviewedAt: form.reviewedAt || undefined,
        reviewDecision: form.reviewDecision || undefined,
        followUpCapaRef: form.followUpCapaRef || undefined,
        recordRefs: joinRefs(form.recordRefs),
        ownerPersonId: form.ownerPersonId || undefined,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['assurarr'] })
      setForm({
        title: '',
        description: '',
        severity: 'high',
        sourceProduct: 'assurarr',
        sourceObjectRef: '',
        affectedObjectRefs: '',
        supplierRef: '',
        sourceNonconformanceRef: '',
        sourceCapaRef: '',
        requestedByPersonId: '',
        requestedAt: '',
        supplierDueAt: '',
        supplierResponseRecordRefs: '',
        reviewPersonId: '',
        reviewedAt: '',
        reviewDecision: '',
        followUpCapaRef: '',
        recordRefs: '',
        ownerPersonId: '',
      })
    },
  })

  return (
    <div className="assurarr-page">
      <PageHeader
        title="SCARs"
        description="Send supplier corrective action requests, review responses, and close the loop with evidence."
        action={<span className="assurarr-pill"><Send className="h-4 w-4" /> {query.data?.length ?? 0} records</span>}
      />
      <div className="assurarr-card">
        <div className="assurarr-card-inner space-y-4">
          <div className="flex items-center gap-2">
            <Plus className="h-4 w-4 text-cyan-300" />
            <h3 className="text-base font-semibold text-slate-50">Create SCAR</h3>
          </div>
          <div className="grid gap-3 md:grid-cols-2">
            <Field label="Title">
              <input className="assurarr-input" value={form.title} onChange={(event) => setForm({ ...form, title: event.target.value })} />
            </Field>
            <Field label="Severity">
              <select className="assurarr-select" value={form.severity} onChange={(event) => setForm({ ...form, severity: event.target.value })}>
                <option value="low">Low</option>
                <option value="moderate">Moderate</option>
                <option value="high">High</option>
                <option value="critical">Critical</option>
              </select>
            </Field>
            <Field label="Supplier ref">
              <input className="assurarr-input" value={form.supplierRef} onChange={(event) => setForm({ ...form, supplierRef: event.target.value })} placeholder="supplyarr:supplier:acme" />
            </Field>
            <Field label="Source product">
              <input className="assurarr-input" value={form.sourceProduct} onChange={(event) => setForm({ ...form, sourceProduct: event.target.value })} />
            </Field>
            <Field label="Source object ref">
              <input className="assurarr-input" value={form.sourceObjectRef} onChange={(event) => setForm({ ...form, sourceObjectRef: event.target.value })} placeholder="SQA-000001" />
            </Field>
            <Field label="Source nonconformance ref">
              <input className="assurarr-input" value={form.sourceNonconformanceRef} onChange={(event) => setForm({ ...form, sourceNonconformanceRef: event.target.value })} placeholder="NCR-000001" />
            </Field>
            <Field label="Source CAPA ref">
              <input className="assurarr-input" value={form.sourceCapaRef} onChange={(event) => setForm({ ...form, sourceCapaRef: event.target.value })} placeholder="CAPA-000001" />
            </Field>
            <Field label="Affected object refs" wide>
              <textarea className="assurarr-textarea" value={form.affectedObjectRefs} onChange={(event) => setForm({ ...form, affectedObjectRefs: event.target.value })} placeholder="One reference per line or comma-separated" />
            </Field>
            <Field label="Supplier response record refs" wide>
              <textarea className="assurarr-textarea" value={form.supplierResponseRecordRefs} onChange={(event) => setForm({ ...form, supplierResponseRecordRefs: event.target.value })} />
            </Field>
            <Field label="Record refs" wide>
              <textarea className="assurarr-textarea" value={form.recordRefs} onChange={(event) => setForm({ ...form, recordRefs: event.target.value })} />
            </Field>
            <Field label="Description" wide>
              <textarea className="assurarr-textarea" value={form.description} onChange={(event) => setForm({ ...form, description: event.target.value })} />
            </Field>
            <Field label="Requested by person id">
              <input className="assurarr-input" value={form.requestedByPersonId} onChange={(event) => setForm({ ...form, requestedByPersonId: event.target.value })} />
            </Field>
            <Field label="Requested at">
              <input className="assurarr-input" type="datetime-local" value={form.requestedAt} onChange={(event) => setForm({ ...form, requestedAt: event.target.value })} />
            </Field>
            <Field label="Supplier due at">
              <input className="assurarr-input" type="datetime-local" value={form.supplierDueAt} onChange={(event) => setForm({ ...form, supplierDueAt: event.target.value })} />
            </Field>
            <Field label="Review person id">
              <input className="assurarr-input" value={form.reviewPersonId} onChange={(event) => setForm({ ...form, reviewPersonId: event.target.value })} />
            </Field>
            <Field label="Reviewed at">
              <input className="assurarr-input" type="datetime-local" value={form.reviewedAt} onChange={(event) => setForm({ ...form, reviewedAt: event.target.value })} />
            </Field>
            <Field label="Review decision">
              <input className="assurarr-input" value={form.reviewDecision} onChange={(event) => setForm({ ...form, reviewDecision: event.target.value })} />
            </Field>
            <Field label="Follow-up CAPA ref">
              <input className="assurarr-input" value={form.followUpCapaRef} onChange={(event) => setForm({ ...form, followUpCapaRef: event.target.value })} placeholder="CAPA-000001" />
            </Field>
            <Field label="Owner person id">
              <input className="assurarr-input" value={form.ownerPersonId} onChange={(event) => setForm({ ...form, ownerPersonId: event.target.value })} />
            </Field>
          </div>
          <button className="assurarr-button" type="button" onClick={() => mutation.mutate()} disabled={mutation.isPending}>
            {mutation.isPending ? 'Saving...' : 'Create SCAR'}
          </button>
        </div>
      </div>
      {query.data ? (
        <EntityTable
          items={query.data}
          emptyLabel="No SCARs yet."
          onStatusChange={(id, status) => assurarrApi.updateScarStatus(id, status)}
          statusChoices={statusOptions.scar}
          detailBasePath="/scars"
        />
      ) : (
        <LoadingCard label="Loading SCARs" />
      )}
    </div>
  )
}

function ScarDetailPage() {
  const { id = '' } = useParams()
  const query = useQuery({
    queryKey: ['assurarr', 'scar', id],
    queryFn: () => assurarrApi.getScar(id),
    enabled: Boolean(id),
  })
  const nonconformances = useRecords(['assurarr', 'nonconformances'], assurarrApi.listNonconformances)
  const capas = useRecords(['assurarr', 'capas'], assurarrApi.listCapas)
  const dashboard = useDashboard()

  if (query.isLoading) {
    return <LoadingCard label="Loading SCAR detail" />
  }

  if (query.isError || !query.data) {
    return (
      <div className="assurarr-page">
        <PageHeader title="SCAR detail" description="Could not load the requested supplier corrective action request." />
        <EmptyState title="SCAR not found." />
      </div>
    )
  }

  const scar = query.data
  const relatedNonconformance = nonconformances.data?.find((item) => item.number === scar.sourceNonconformanceRef) ?? null
  const relatedCapa = capas.data?.find((item) => item.number === scar.sourceCapaRef || item.number === scar.followUpCapaRef) ?? null
  const timeline = dashboard.data?.recentEvents.filter((event) => event.subjectType === 'scar' && event.subjectId === scar.id) ?? []

  return (
    <div className="assurarr-page">
      <PageHeader
        title={`${scar.number} · ${scar.title}`}
        description="Supplier corrective action request detail, response evidence, and the closure trail for the supplier workflow."
      />
      <div className="space-y-4">
        <div className="assurarr-grid cols-2">
          <div className="assurarr-card">
            <div className="assurarr-card-inner space-y-3">
              <p className="assurarr-label">Overview</p>
              <div className="flex flex-wrap gap-2 text-sm">
                <span className="assurarr-pill">{scar.status}</span>
                <span className="assurarr-pill">{scar.severity}</span>
              </div>
              <p className="text-sm text-slate-300">Supplier corrective action request for supplier response and follow-up closure.</p>
              <div className="grid gap-2 text-sm text-slate-300 md:grid-cols-2">
                <div><span className="text-slate-500">Source product:</span> {scar.sourceProduct ?? 'manual'}</div>
                <div><span className="text-slate-500">Source object:</span> {scar.sourceObjectRef ?? 'n/a'}</div>
                <div><span className="text-slate-500">Supplier:</span> {scar.supplierRef ?? 'unassigned'}</div>
                <div><span className="text-slate-500">Owner:</span> {scar.ownerPersonId ?? 'unassigned'}</div>
                <div><span className="text-slate-500">Requested:</span> {scar.requestedAt ? new Date(scar.requestedAt).toLocaleString() : 'n/a'}</div>
                <div><span className="text-slate-500">Supplier due:</span> {scar.supplierDueAt ? new Date(scar.supplierDueAt).toLocaleString() : 'n/a'}</div>
              </div>
              <div className="grid gap-2 text-sm text-slate-300">
                <div><span className="text-slate-500">Review decision:</span> {scar.reviewDecision ?? 'n/a'}</div>
                <div><span className="text-slate-500">Reviewed:</span> {scar.reviewedAt ? new Date(scar.reviewedAt).toLocaleString() : 'n/a'}</div>
                <div><span className="text-slate-500">Closed:</span> {scar.closedAt ? new Date(scar.closedAt).toLocaleString() : 'n/a'}</div>
                <div><span className="text-slate-500">Closure summary:</span> {scar.closureSummary ?? 'n/a'}</div>
              </div>
            </div>
          </div>
          <div className="assurarr-card">
            <div className="assurarr-card-inner space-y-3">
              <p className="assurarr-label">Linked records</p>
              <div className="space-y-2 text-sm text-slate-300">
                <div>
                  <span className="text-slate-500">Nonconformance:</span>{' '}
                  {relatedNonconformance ? (
                    <Link to={`/nonconformances/${relatedNonconformance.id}`} className="text-cyan-300 hover:text-cyan-200">
                      {relatedNonconformance.number}
                    </Link>
                  ) : (
                    scar.sourceNonconformanceRef ?? 'none'
                  )}
                </div>
                <div>
                  <span className="text-slate-500">CAPA:</span>{' '}
                  {relatedCapa ? (
                    <Link to={`/capa/${relatedCapa.id}`} className="text-cyan-300 hover:text-cyan-200">
                      {relatedCapa.number}
                    </Link>
                  ) : (
                    scar.sourceCapaRef ?? scar.followUpCapaRef ?? 'none'
                  )}
                </div>
                <div>
                  <span className="text-slate-500">Affected refs:</span>{' '}
                  {scar.affectedObjectRefs.length > 0 ? scar.affectedObjectRefs.join(', ') : 'none'}
                </div>
                <div>
                  <span className="text-slate-500">Response records:</span>{' '}
                  {scar.supplierResponseRecordRefs.length > 0 ? scar.supplierResponseRecordRefs.join(', ') : 'none'}
                </div>
                <div>
                  <span className="text-slate-500">Record refs:</span>{' '}
                  {scar.recordRefs.length > 0 ? scar.recordRefs.join(', ') : 'none'}
                </div>
              </div>
              <div className="grid gap-2 text-sm text-slate-300">
                <div><span className="text-slate-500">Requested by:</span> {scar.requestedByPersonId ?? 'n/a'}</div>
                <div><span className="text-slate-500">Approved follow-up:</span> {scar.followUpCapaRef ?? 'n/a'}</div>
                <div><span className="text-slate-500">Reviewed by:</span> {scar.reviewPersonId ?? 'n/a'}</div>
              </div>
            </div>
          </div>
        </div>
        <div className="assurarr-card">
          <div className="assurarr-card-inner space-y-3">
            <p className="assurarr-label">Timeline</p>
            <div className="space-y-2">
              {timeline.length === 0 ? <EmptyState title="No timeline yet." /> : null}
              {timeline.map((event) => (
                <div key={event.id} className="rounded-xl border border-slate-700/70 bg-slate-900/80 p-3">
                  <div className="flex items-center justify-between gap-3">
                    <strong className="text-sm text-slate-100">{event.eventType}</strong>
                    <time className="text-xs text-slate-400">{new Date(event.occurredAt).toLocaleString()}</time>
                  </div>
                  <p className="mt-1 text-sm text-slate-300">{event.details ?? event.subjectType}</p>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}

function CustomerComplaintPage() {
  const query = useRecords(['assurarr', 'complaints'], assurarrApi.listCustomerComplaintQualityCases)
  const [form, setForm] = useState({
    title: '',
    description: '',
    severity: 'moderate',
    complaintType: 'product_quality',
    sourceProduct: 'routarr',
    sourceObjectRef: '',
    affectedOrderRefs: '',
    affectedShipmentRefs: '',
    affectedItemRefs: '',
    affectedAssetRefs: '',
    customerRef: '',
    customerContactSnapshot: '',
    customerLocationRef: '',
    nonconformanceRef: '',
    holdRefs: '',
    capaRefs: '',
    customerResponseRecordRefs: '',
    recordRefs: '',
    ownerPersonId: '',
    receivedAt: '',
    receivedByPersonId: '',
    customerResponseDueAt: '',
  })
  const queryClient = useQueryClient()
  const mutation = useMutation({
    mutationFn: async () =>
      assurarrApi.createCustomerComplaintQualityCase({
        title: form.title,
        description: form.description,
        severity: form.severity,
        complaintType: form.complaintType,
        sourceProduct: form.sourceProduct,
        sourceObjectRef: form.sourceObjectRef,
        affectedObjectRefs: joinRefs(form.affectedOrderRefs)
          .concat(joinRefs(form.affectedShipmentRefs), joinRefs(form.affectedItemRefs), joinRefs(form.affectedAssetRefs)),
        affectedOrderRefs: joinRefs(form.affectedOrderRefs),
        affectedShipmentRefs: joinRefs(form.affectedShipmentRefs),
        affectedItemRefs: joinRefs(form.affectedItemRefs),
        affectedAssetRefs: joinRefs(form.affectedAssetRefs),
        customerRef: form.customerRef || undefined,
        customerContactSnapshot: form.customerContactSnapshot || undefined,
        customerLocationRef: form.customerLocationRef || undefined,
        nonconformanceRef: form.nonconformanceRef || undefined,
        holdRefs: joinRefs(form.holdRefs),
        capaRefs: joinRefs(form.capaRefs),
        customerResponseRecordRefs: joinRefs(form.customerResponseRecordRefs),
        recordRefs: joinRefs(form.recordRefs),
        ownerPersonId: form.ownerPersonId || undefined,
        receivedAt: form.receivedAt || undefined,
        receivedByPersonId: form.receivedByPersonId || undefined,
        customerResponseDueAt: form.customerResponseDueAt || undefined,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['assurarr'] })
      setForm({
        title: '',
        description: '',
        severity: 'moderate',
        complaintType: 'product_quality',
        sourceProduct: 'routarr',
        sourceObjectRef: '',
        affectedOrderRefs: '',
        affectedShipmentRefs: '',
        affectedItemRefs: '',
        affectedAssetRefs: '',
        customerRef: '',
        customerContactSnapshot: '',
        customerLocationRef: '',
        nonconformanceRef: '',
        holdRefs: '',
        capaRefs: '',
        customerResponseRecordRefs: '',
        recordRefs: '',
        ownerPersonId: '',
        receivedAt: '',
        receivedByPersonId: '',
        customerResponseDueAt: '',
      })
    },
  })

  return (
    <div className="assurarr-page">
      <PageHeader
        title="Customer complaints"
        description="Track complaint quality cases, customer impact, and the evidence needed for closure."
        action={<span className="assurarr-pill"><ClipboardList className="h-4 w-4" /> {query.data?.length ?? 0} records</span>}
      />
      <div className="assurarr-card">
        <div className="assurarr-card-inner space-y-4">
          <div className="flex items-center gap-2">
            <Plus className="h-4 w-4 text-cyan-300" />
            <h3 className="text-base font-semibold text-slate-50">Create complaint quality case</h3>
          </div>
          <div className="grid gap-3 md:grid-cols-2">
            <Field label="Title">
              <input className="assurarr-input" value={form.title} onChange={(event) => setForm({ ...form, title: event.target.value })} />
            </Field>
            <Field label="Complaint type">
              <select className="assurarr-select" value={form.complaintType} onChange={(event) => setForm({ ...form, complaintType: event.target.value })}>
                <option value="product_quality">Product quality</option>
                <option value="service_quality">Service quality</option>
                <option value="delivery_quality">Delivery quality</option>
                <option value="documentation">Documentation</option>
                <option value="damaged_goods">Damaged goods</option>
                <option value="wrong_item">Wrong item</option>
                <option value="late_delivery_quality_impact">Late delivery quality impact</option>
                <option value="failed_requirement">Failed requirement</option>
                <option value="repeat_issue">Repeat issue</option>
                <option value="other">Other</option>
              </select>
            </Field>
            <Field label="Severity">
              <select className="assurarr-select" value={form.severity} onChange={(event) => setForm({ ...form, severity: event.target.value })}>
                <option value="low">Low</option>
                <option value="moderate">Moderate</option>
                <option value="high">High</option>
                <option value="critical">Critical</option>
              </select>
            </Field>
            <Field label="Customer ref">
              <input className="assurarr-input" value={form.customerRef} onChange={(event) => setForm({ ...form, customerRef: event.target.value })} placeholder="customarr:customer:contoso" />
            </Field>
            <Field label="Source object ref">
              <input className="assurarr-input" value={form.sourceObjectRef} onChange={(event) => setForm({ ...form, sourceObjectRef: event.target.value })} />
            </Field>
            <Field label="Affected order refs" wide>
              <textarea className="assurarr-textarea" value={form.affectedOrderRefs} onChange={(event) => setForm({ ...form, affectedOrderRefs: event.target.value })} />
            </Field>
            <Field label="Affected shipment refs" wide>
              <textarea className="assurarr-textarea" value={form.affectedShipmentRefs} onChange={(event) => setForm({ ...form, affectedShipmentRefs: event.target.value })} />
            </Field>
            <Field label="Affected item refs" wide>
              <textarea className="assurarr-textarea" value={form.affectedItemRefs} onChange={(event) => setForm({ ...form, affectedItemRefs: event.target.value })} />
            </Field>
            <Field label="Affected asset refs" wide>
              <textarea className="assurarr-textarea" value={form.affectedAssetRefs} onChange={(event) => setForm({ ...form, affectedAssetRefs: event.target.value })} />
            </Field>
            <Field label="Hold refs" wide>
              <textarea className="assurarr-textarea" value={form.holdRefs} onChange={(event) => setForm({ ...form, holdRefs: event.target.value })} />
            </Field>
            <Field label="CAPA refs" wide>
              <textarea className="assurarr-textarea" value={form.capaRefs} onChange={(event) => setForm({ ...form, capaRefs: event.target.value })} />
            </Field>
            <Field label="Customer response refs" wide>
              <textarea className="assurarr-textarea" value={form.customerResponseRecordRefs} onChange={(event) => setForm({ ...form, customerResponseRecordRefs: event.target.value })} />
            </Field>
            <Field label="Record refs" wide>
              <textarea className="assurarr-textarea" value={form.recordRefs} onChange={(event) => setForm({ ...form, recordRefs: event.target.value })} />
            </Field>
            <Field label="Description" wide>
              <textarea className="assurarr-textarea" value={form.description} onChange={(event) => setForm({ ...form, description: event.target.value })} />
            </Field>
            <Field label="Customer contact snapshot" wide>
              <input className="assurarr-input" value={form.customerContactSnapshot} onChange={(event) => setForm({ ...form, customerContactSnapshot: event.target.value })} />
            </Field>
            <Field label="Customer location ref">
              <input className="assurarr-input" value={form.customerLocationRef} onChange={(event) => setForm({ ...form, customerLocationRef: event.target.value })} />
            </Field>
            <Field label="Nonconformance ref">
              <input className="assurarr-input" value={form.nonconformanceRef} onChange={(event) => setForm({ ...form, nonconformanceRef: event.target.value })} />
            </Field>
            <Field label="Owner person id">
              <input className="assurarr-input" value={form.ownerPersonId} onChange={(event) => setForm({ ...form, ownerPersonId: event.target.value })} />
            </Field>
            <Field label="Received at">
              <input className="assurarr-input" type="datetime-local" value={form.receivedAt} onChange={(event) => setForm({ ...form, receivedAt: event.target.value })} />
            </Field>
            <Field label="Received by person id">
              <input className="assurarr-input" value={form.receivedByPersonId} onChange={(event) => setForm({ ...form, receivedByPersonId: event.target.value })} />
            </Field>
            <Field label="Customer response due at">
              <input className="assurarr-input" type="datetime-local" value={form.customerResponseDueAt} onChange={(event) => setForm({ ...form, customerResponseDueAt: event.target.value })} />
            </Field>
          </div>
          <button className="assurarr-button" type="button" onClick={() => mutation.mutate()} disabled={mutation.isPending}>
            {mutation.isPending ? 'Saving...' : 'Create complaint case'}
          </button>
        </div>
      </div>
      {query.data ? (
        <EntityTable
          items={query.data}
          emptyLabel="No customer complaint quality cases yet."
          onStatusChange={(id, status) => assurarrApi.updateCustomerComplaintQualityCaseStatus(id, status)}
          statusChoices={statusOptions.customerComplaint}
          detailBasePath="/complaints"
        />
      ) : (
        <LoadingCard label="Loading complaint cases" />
      )}
    </div>
  )
}

function CustomerComplaintDetailPage() {
  const { id = '' } = useParams()
  const query = useQuery({
    queryKey: ['assurarr', 'customer-complaint', id],
    queryFn: () => assurarrApi.getCustomerComplaintQualityCase(id),
    enabled: Boolean(id),
  })
  const nonconformances = useRecords(['assurarr', 'nonconformances'], assurarrApi.listNonconformances)
  const holds = useRecords(['assurarr', 'holds'], assurarrApi.listHolds)
  const capas = useRecords(['assurarr', 'capas'], assurarrApi.listCapas)
  const dashboard = useDashboard()

  if (query.isLoading) {
    return <LoadingCard label="Loading complaint detail" />
  }

  if (query.isError || !query.data) {
    return (
      <div className="assurarr-page">
        <PageHeader title="Customer complaint detail" description="Could not load the requested customer complaint quality case." />
        <EmptyState title="Customer complaint quality case not found." />
      </div>
    )
  }

  const complaint = query.data
  const relatedNonconformance = nonconformances.data?.find((item) => item.number === complaint.nonconformanceRef) ?? null
  const relatedHolds = holds.data?.filter((hold) => complaint.holdRefs.includes(hold.number) || complaint.holdRefs.includes(hold.id)) ?? []
  const relatedCapas = capas.data?.filter((capa) => complaint.capaRefs.includes(capa.number) || complaint.capaRefs.includes(capa.id)) ?? []
  const timeline = dashboard.data?.recentEvents.filter(
    (event) => event.subjectType === 'customer_complaint' && event.subjectId === complaint.id,
  ) ?? []

  return (
    <div className="assurarr-page">
      <PageHeader
        title={`${complaint.number} · ${complaint.title}`}
        description="Complaint detail, linked quality records, and the activity trail for the customer-facing workflow."
      />
      <div className="space-y-4">
        <div className="assurarr-grid cols-2">
          <div className="assurarr-card">
            <div className="assurarr-card-inner space-y-3">
              <p className="assurarr-label">Overview</p>
              <div className="flex flex-wrap gap-2 text-sm">
                <span className="assurarr-pill">{complaint.status}</span>
                <span className="assurarr-pill">{complaint.severity}</span>
                <span className="assurarr-pill">{complaint.complaintType}</span>
              </div>
              <p className="text-sm text-slate-300">Customer complaint workflow record for customer impact tracking and closure.</p>
              <div className="grid gap-2 text-sm text-slate-300 md:grid-cols-2">
                <div><span className="text-slate-500">Source product:</span> {complaint.sourceProduct ?? 'manual'}</div>
                <div><span className="text-slate-500">Source object:</span> {complaint.sourceObjectRef ?? 'n/a'}</div>
                <div><span className="text-slate-500">Customer:</span> {complaint.customerRef ?? 'unassigned'}</div>
                <div><span className="text-slate-500">Owner:</span> {complaint.ownerPersonId ?? 'unassigned'}</div>
                <div><span className="text-slate-500">Received:</span> {complaint.receivedAt ? new Date(complaint.receivedAt).toLocaleString() : 'n/a'}</div>
                <div><span className="text-slate-500">Response due:</span> {complaint.customerResponseDueAt ? new Date(complaint.customerResponseDueAt).toLocaleString() : 'n/a'}</div>
              </div>
              <div className="grid gap-2 text-sm text-slate-300">
                <div><span className="text-slate-500">Received by:</span> {complaint.receivedByPersonId ?? 'n/a'}</div>
                <div><span className="text-slate-500">Closed:</span> {complaint.closedAt ? new Date(complaint.closedAt).toLocaleString() : 'n/a'}</div>
                <div><span className="text-slate-500">Closure summary:</span> {complaint.closureSummary ?? 'n/a'}</div>
              </div>
            </div>
          </div>
          <div className="assurarr-card">
            <div className="assurarr-card-inner space-y-3">
              <p className="assurarr-label">Linked records</p>
              <div className="space-y-2 text-sm text-slate-300">
                <div>
                  <span className="text-slate-500">Nonconformance:</span>{' '}
                  {relatedNonconformance ? (
                    <Link to={`/nonconformances/${relatedNonconformance.id}`} className="text-cyan-300 hover:text-cyan-200">
                      {relatedNonconformance.number}
                    </Link>
                  ) : (
                    complaint.nonconformanceRef ?? 'none'
                  )}
                </div>
                <div>
                  <span className="text-slate-500">Holds:</span>{' '}
                  {relatedHolds.length > 0 ? (
                    <span className="flex flex-wrap gap-2 pt-1">
                      {relatedHolds.map((hold) => (
                        <Link key={hold.id} to={`/holds/${hold.id}`} className="assurarr-pill">
                          {hold.number}
                        </Link>
                      ))}
                    </span>
                  ) : (
                    'none'
                  )}
                </div>
                <div>
                  <span className="text-slate-500">CAPA refs:</span>{' '}
                  {relatedCapas.length > 0 ? (
                    <span className="flex flex-wrap gap-2 pt-1">
                      {relatedCapas.map((capa) => (
                        <Link key={capa.id} to={`/capa/${capa.id}`} className="assurarr-pill">
                          {capa.number}
                        </Link>
                      ))}
                    </span>
                  ) : (
                    'none'
                  )}
                </div>
                <div>
                  <span className="text-slate-500">Customer response refs:</span>{' '}
                  {complaint.customerResponseRecordRefs.length > 0 ? complaint.customerResponseRecordRefs.join(', ') : 'none'}
                </div>
                <div>
                  <span className="text-slate-500">Record refs:</span>{' '}
                  {complaint.recordRefs.length > 0 ? complaint.recordRefs.join(', ') : 'none'}
                </div>
                <div>
                  <span className="text-slate-500">Affected refs:</span>{' '}
                  {complaint.affectedObjectRefs.length > 0 ? complaint.affectedObjectRefs.join(', ') : 'none'}
                </div>
              </div>
              <div className="space-y-1 text-sm text-slate-300">
                <div><span className="text-slate-500">Customer contact:</span> {complaint.customerContactSnapshot ?? 'n/a'}</div>
                <div><span className="text-slate-500">Customer location:</span> {complaint.customerLocationRef ?? 'n/a'}</div>
              </div>
            </div>
          </div>
        </div>
        <div className="assurarr-card">
          <div className="assurarr-card-inner space-y-3">
            <p className="assurarr-label">Timeline</p>
            <div className="space-y-2">
              {timeline.length === 0 ? <EmptyState title="No timeline yet." /> : null}
              {timeline.map((event) => (
                <div key={event.id} className="rounded-xl border border-slate-700/70 bg-slate-900/80 p-3">
                  <div className="flex items-center justify-between gap-3">
                    <strong className="text-sm text-slate-100">{event.eventType}</strong>
                    <time className="text-xs text-slate-400">{new Date(event.occurredAt).toLocaleString()}</time>
                  </div>
                  <p className="mt-1 text-sm text-slate-300">{event.details ?? event.subjectType}</p>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}

function StatusPage() {
  const query = useRecords(['assurarr', 'status-snapshots'], assurarrApi.listSnapshots)
  const [lookup, setLookup] = useState({ targetProduct: 'loadarr', targetObjectId: 'inventory:LOT-991' })
  const riskProfilesQuery = useRecords(['assurarr', 'risk-profiles'], assurarrApi.listRiskProfiles)
  const [riskForm, setRiskForm] = useState({
    targetType: 'site',
    targetRef: 'loadarr:site:north-yard',
    riskLevel: 'high',
    riskFactors: 'open nonconformance trend\nrecent hold activity\ncritical defect recurrence',
    openIssueCount: '4',
    repeatIssueCount: '2',
    criticalIssueCount: '1',
    lastIncidentAt: '',
    mitigationActions: 'continue receiving containment review\nverify hold release evidence\nmonitor repeat defects',
    reviewedAt: '',
    reviewedByPersonId: '',
  })
  const lookupQuery = useQuery({
    queryKey: ['assurarr', 'quality-status', lookup.targetProduct, lookup.targetObjectId],
    queryFn: () => assurarrApi.getQualityStatus(lookup.targetProduct, lookup.targetObjectId),
    enabled: false,
  })
  const lookupMutation = useMutation({
    mutationFn: async () => lookupQuery.refetch(),
  })
  const createRiskMutation = useMutation({
    mutationFn: async () =>
      assurarrApi.createRiskProfile({
        targetType: riskForm.targetType,
        targetRef: riskForm.targetRef,
        riskLevel: riskForm.riskLevel,
        riskFactors: joinRefs(riskForm.riskFactors),
        openIssueCount: Number(riskForm.openIssueCount || 0),
        repeatIssueCount: Number(riskForm.repeatIssueCount || 0),
        criticalIssueCount: Number(riskForm.criticalIssueCount || 0),
        lastIncidentAt: riskForm.lastIncidentAt || undefined,
        mitigationActions: joinRefs(riskForm.mitigationActions),
        reviewedAt: riskForm.reviewedAt || undefined,
        reviewedByPersonId: riskForm.reviewedByPersonId || undefined,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['assurarr'] })
    },
  })
  const queryClient = useQueryClient()
  return (
    <div className="assurarr-page">
      <PageHeader title="Quality status" description="Publish current quality posture to downstream products that need to block, warn, or permit work." />
      <RecordForm
        title="Create quality status snapshot"
        entityLabel="Status"
        onCreate={async (body) =>
          assurarrApi.createSnapshot({
            ...body,
            ownerPersonId: body.ownerPersonId || undefined,
            targetProduct: body.sourceProduct || 'loadarr',
            targetObjectRef: body.sourceObjectRef || 'loadarr:inventory:example',
            qualityStatus: 'under_review',
            activeHoldRefs: [],
            openNonconformanceRefs: [],
            openCapaRefs: [],
            openFindingRefs: [],
          })
        }
      />
      {query.data ? (
        <EntityTable
          items={query.data}
          emptyLabel="No status snapshots yet."
        />
      ) : (
        <LoadingCard label="Loading quality status" />
      )}
      <div className="assurarr-card">
        <div className="assurarr-card-inner space-y-4">
          <div className="flex items-center gap-2">
            <BookCheck className="h-4 w-4 text-cyan-300" />
            <h3 className="text-base font-semibold text-slate-50">Look up current quality status</h3>
          </div>
          <div className="grid gap-3 md:grid-cols-2">
            <Field label="Target product">
              <input className="assurarr-input" value={lookup.targetProduct} onChange={(event) => setLookup({ ...lookup, targetProduct: event.target.value })} />
            </Field>
            <Field label="Target object id">
              <input className="assurarr-input" value={lookup.targetObjectId} onChange={(event) => setLookup({ ...lookup, targetObjectId: event.target.value })} />
            </Field>
          </div>
          <button className="assurarr-button" type="button" onClick={() => lookupMutation.mutate()} disabled={lookupMutation.isPending}>
            {lookupMutation.isPending ? 'Looking up...' : 'Fetch quality status'}
          </button>
          {lookupQuery.data ? (
            <div className="rounded-2xl border border-slate-700/70 bg-slate-900/80 p-4 text-sm text-slate-200">
              <div className="flex flex-wrap items-center gap-2">
                <span className="assurarr-pill">{lookupQuery.data.targetProduct}</span>
                <span className="assurarr-pill">{lookupQuery.data.targetObjectRef}</span>
                <span className="assurarr-pill">{lookupQuery.data.qualityStatus}</span>
              </div>
              <p className="mt-3 text-slate-300">{lookupQuery.data.description}</p>
            </div>
          ) : null}
        </div>
      </div>
      <div className="assurarr-card mt-6">
        <div className="assurarr-card-inner space-y-4">
          <div className="flex items-center gap-2">
            <Gauge className="h-4 w-4 text-cyan-300" />
            <h3 className="text-base font-semibold text-slate-50">Risk profiles</h3>
            <span className="assurarr-pill">{riskProfilesQuery.data?.length ?? 0} records</span>
          </div>
          <div className="grid gap-3 md:grid-cols-2">
            <Field label="Target type">
              <select className="assurarr-select" value={riskForm.targetType} onChange={(event) => setRiskForm({ ...riskForm, targetType: event.target.value })}>
                <option value="supplier">Supplier</option>
                <option value="customer">Customer</option>
                <option value="process">Process</option>
                <option value="site">Site</option>
                <option value="asset">Asset</option>
                <option value="inventory_item">Inventory item</option>
                <option value="order">Order</option>
                <option value="route">Route</option>
              </select>
            </Field>
            <Field label="Target ref">
              <input className="assurarr-input" value={riskForm.targetRef} onChange={(event) => setRiskForm({ ...riskForm, targetRef: event.target.value })} />
            </Field>
            <Field label="Risk level">
              <select className="assurarr-select" value={riskForm.riskLevel} onChange={(event) => setRiskForm({ ...riskForm, riskLevel: event.target.value })}>
                <option value="low">Low</option>
                <option value="moderate">Moderate</option>
                <option value="high">High</option>
                <option value="critical">Critical</option>
                <option value="unknown">Unknown</option>
              </select>
            </Field>
            <Field label="Open issue count">
              <input className="assurarr-input" value={riskForm.openIssueCount} onChange={(event) => setRiskForm({ ...riskForm, openIssueCount: event.target.value })} />
            </Field>
            <Field label="Repeat issue count">
              <input className="assurarr-input" value={riskForm.repeatIssueCount} onChange={(event) => setRiskForm({ ...riskForm, repeatIssueCount: event.target.value })} />
            </Field>
            <Field label="Critical issue count">
              <input className="assurarr-input" value={riskForm.criticalIssueCount} onChange={(event) => setRiskForm({ ...riskForm, criticalIssueCount: event.target.value })} />
            </Field>
            <Field label="Risk factors">
              <textarea className="assurarr-textarea" value={riskForm.riskFactors} onChange={(event) => setRiskForm({ ...riskForm, riskFactors: event.target.value })} />
            </Field>
            <Field label="Mitigation actions">
              <textarea className="assurarr-textarea" value={riskForm.mitigationActions} onChange={(event) => setRiskForm({ ...riskForm, mitigationActions: event.target.value })} />
            </Field>
            <Field label="Last incident at">
              <input className="assurarr-input" type="datetime-local" value={riskForm.lastIncidentAt} onChange={(event) => setRiskForm({ ...riskForm, lastIncidentAt: event.target.value })} />
            </Field>
            <Field label="Reviewed at">
              <input className="assurarr-input" type="datetime-local" value={riskForm.reviewedAt} onChange={(event) => setRiskForm({ ...riskForm, reviewedAt: event.target.value })} />
            </Field>
            <Field label="Reviewed by person id">
              <input className="assurarr-input" value={riskForm.reviewedByPersonId} onChange={(event) => setRiskForm({ ...riskForm, reviewedByPersonId: event.target.value })} />
            </Field>
          </div>
          <button className="assurarr-button" type="button" onClick={() => createRiskMutation.mutate()} disabled={createRiskMutation.isPending}>
            {createRiskMutation.isPending ? 'Saving...' : 'Create risk profile'}
          </button>
          <div className="grid gap-3 md:grid-cols-2">
            {riskProfilesQuery.data?.length ? (
              riskProfilesQuery.data.map((profile) => (
                <div key={profile.id} className="rounded-2xl border border-slate-700/70 bg-slate-900/80 p-4 text-sm text-slate-200">
                  <div className="flex flex-wrap items-center gap-2">
                    <span className="assurarr-pill">{profile.targetType}</span>
                    <span className="assurarr-pill">{profile.targetRef}</span>
                    <span className="assurarr-pill">{profile.riskLevel}</span>
                  </div>
                  <div className="mt-3 grid gap-2 text-xs text-slate-400 md:grid-cols-2">
                    <div>Open issues: {profile.openIssueCount}</div>
                    <div>Repeat issues: {profile.repeatIssueCount}</div>
                    <div>Critical issues: {profile.criticalIssueCount}</div>
                    <div>Reviewed: {profile.reviewedAt ? new Date(profile.reviewedAt).toLocaleString() : 'n/a'}</div>
                  </div>
                  <p className="mt-2 text-xs text-cyan-300">
                    <Link to={`/risk-profiles/${profile.id}`} className="hover:text-cyan-200">
                      Open detail
                    </Link>
                  </p>
                  <p className="mt-3 text-xs text-slate-400">Risk factors: {profile.riskFactors.length ? profile.riskFactors.join(', ') : 'none'}</p>
                  <p className="mt-1 text-xs text-slate-400">Mitigation: {profile.mitigationActions.length ? profile.mitigationActions.join(', ') : 'none'}</p>
                </div>
              ))
            ) : (
              <EmptyState title="No risk profiles yet." />
            )}
          </div>
        </div>
      </div>
    </div>
  )
}

function RiskProfileDetailPage() {
  const { id = '' } = useParams<{ id: string }>()
  const query = useRecords(['assurarr', 'risk-profile', id], () => assurarrApi.getRiskProfile(id))

  if (query.isLoading) {
    return <LoadingCard label="Loading risk profile detail" />
  }

  if (!query.data) {
    return (
      <div className="assurarr-page">
        <PageHeader title="Risk profile detail" description="Could not load the requested risk profile." />
      </div>
    )
  }

  const profile = query.data

  return (
    <div className="assurarr-page">
      <PageHeader
        title={profile.targetRef}
        description={`Risk profile for ${profile.targetType}`}
        action={<span className="assurarr-pill">{profile.riskLevel}</span>}
      />
      <div className="assurarr-grid cols-2">
        <div className="assurarr-card">
          <div className="assurarr-card-inner space-y-3">
            <p className="assurarr-label">Overview</p>
            <div className="space-y-2 text-sm text-slate-300">
              <div><span className="text-slate-500">Target type:</span> {profile.targetType}</div>
              <div><span className="text-slate-500">Target ref:</span> {profile.targetRef}</div>
              <div><span className="text-slate-500">Open issues:</span> {profile.openIssueCount}</div>
              <div><span className="text-slate-500">Repeat issues:</span> {profile.repeatIssueCount}</div>
              <div><span className="text-slate-500">Critical issues:</span> {profile.criticalIssueCount}</div>
              <div><span className="text-slate-500">Reviewed:</span> {profile.reviewedAt ? new Date(profile.reviewedAt).toLocaleString() : 'n/a'}</div>
              <div><span className="text-slate-500">Reviewed by:</span> {profile.reviewedByPersonId ?? 'n/a'}</div>
              <div><span className="text-slate-500">Last incident:</span> {profile.lastIncidentAt ? new Date(profile.lastIncidentAt).toLocaleString() : 'n/a'}</div>
            </div>
          </div>
        </div>
        <div className="assurarr-card">
          <div className="assurarr-card-inner space-y-3">
            <p className="assurarr-label">Risk factors and mitigation</p>
            <div className="space-y-4 text-sm text-slate-300">
              <div>
                <div className="text-slate-500">Risk factors</div>
                <div>{profile.riskFactors.length ? profile.riskFactors.join(', ') : 'none'}</div>
              </div>
              <div>
                <div className="text-slate-500">Mitigation actions</div>
                <div>{profile.mitigationActions.length ? profile.mitigationActions.join(', ') : 'none'}</div>
              </div>
            </div>
          </div>
        </div>
      </div>
      <div className="assurarr-card mt-6">
        <div className="assurarr-card-inner space-y-3">
          <p className="assurarr-label">Timeline</p>
          <div className="space-y-2">
            <div className="rounded-xl border border-slate-700/70 bg-slate-900/80 p-3 text-sm text-slate-300">
              Quality risk profiles are published through the status workspace and reflected in the dashboard count for elevated risk.
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}

function ScorecardPage() {
  const query = useRecords(['assurarr', 'scorecards'], assurarrApi.listScorecards)
  const now = useMemo(() => new Date(), [])
  return (
    <div className="assurarr-page">
      <PageHeader title="Scorecards" description="Trend quality by site, supplier, process, or customer target." />
      <RecordForm
        title="Create scorecard"
        entityLabel="Scorecard"
        onCreate={async (body) =>
          assurarrApi.createScorecard({
            ...body,
            ownerPersonId: body.ownerPersonId || undefined,
            targetType: 'site',
            targetRef: body.sourceObjectRef || 'loadarr:site:north-yard',
            periodStart: new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000).toISOString(),
            periodEnd: now.toISOString(),
            qualityStatus: 'warning',
            trend: 'worsening',
          })
        }
      />
      {query.data ? (
        <EntityTable
          items={query.data}
          emptyLabel="No scorecards yet."
          detailBasePath="/scorecards"
        />
      ) : (
        <LoadingCard label="Loading scorecards" />
      )}
    </div>
  )
}

function ScorecardDetailPage() {
  const { id = '' } = useParams<{ id: string }>()
  const queryClient = useQueryClient()
  const scorecardQuery = useRecords(['assurarr', 'scorecard', id], () => assurarrApi.getScorecard(id))
  const metricsQuery = useRecords(['assurarr', 'scorecard-metrics', id], () => assurarrApi.listQualityMetrics(id))
  const [metricForm, setMetricForm] = useState({
    metricKey: 'open-nonconformance-count',
    title: 'Open nonconformance count',
    description: 'Count of open quality issues that are not closed or canceled.',
    category: 'nonconformance',
    value: '0',
    numerator: '0',
    denominator: '0',
    unit: 'count',
    targetValue: '0',
    warningThreshold: '2',
    criticalThreshold: '5',
    status: 'warning',
    sourceProductRefs: 'assurarr, loadarr',
  })
  const [reviewForm, setReviewForm] = useState({
    reviewedByPersonId: '',
    reviewedAt: '',
  })
  const createMetricMutation = useMutation({
    mutationFn: async () =>
      assurarrApi.createQualityMetric(id, {
        title: metricForm.title,
        description: metricForm.description,
        metricKey: metricForm.metricKey,
        category: metricForm.category,
        value: metricForm.value ? Number(metricForm.value) : null,
        numerator: metricForm.numerator ? Number(metricForm.numerator) : null,
        denominator: metricForm.denominator ? Number(metricForm.denominator) : null,
        unit: metricForm.unit,
        targetValue: metricForm.targetValue ? Number(metricForm.targetValue) : null,
        warningThreshold: metricForm.warningThreshold ? Number(metricForm.warningThreshold) : null,
        criticalThreshold: metricForm.criticalThreshold ? Number(metricForm.criticalThreshold) : null,
        status: metricForm.status,
        sourceProductRefs: joinRefs(metricForm.sourceProductRefs),
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['assurarr'] })
      setMetricForm({
        metricKey: '',
        title: '',
        description: '',
        category: 'nonconformance',
        value: '',
        numerator: '',
        denominator: '',
        unit: '',
        targetValue: '',
        warningThreshold: '',
        criticalThreshold: '',
        status: 'unknown',
        sourceProductRefs: '',
      })
    },
  })
  const reviewMutation = useMutation({
    mutationFn: async () =>
      assurarrApi.reviewScorecard(id, {
        reviewedByPersonId: reviewForm.reviewedByPersonId || undefined,
        reviewedAt: reviewForm.reviewedAt || undefined,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['assurarr'] })
    },
  })

  if (scorecardQuery.isLoading || metricsQuery.isLoading) {
    return <LoadingCard label="Loading scorecard detail" />
  }

  if (!scorecardQuery.data) {
    return (
      <div className="assurarr-page">
        <PageHeader title="Scorecard detail" description="Could not load the requested scorecard." />
      </div>
    )
  }

  const scorecard = scorecardQuery.data
  const metrics = metricsQuery.data ?? []

  return (
    <div className="assurarr-page">
      <PageHeader
        title={scorecard.number}
        description={scorecard.title}
        action={<span className="assurarr-pill">{scorecard.qualityStatus} · {scorecard.trend}</span>}
      />
      <div className="assurarr-grid cols-2">
        <div className="assurarr-card">
          <div className="assurarr-card-inner space-y-3">
            <p className="assurarr-label">Overview</p>
            <div className="space-y-2 text-sm text-slate-300">
              <div><span className="text-slate-500">Target:</span> {scorecard.targetType} · {scorecard.targetRef}</div>
              <div><span className="text-slate-500">Period:</span> {new Date(scorecard.periodStart).toLocaleDateString()} to {new Date(scorecard.periodEnd).toLocaleDateString()}</div>
              <div><span className="text-slate-500">Score:</span> {scorecard.overallScore ?? 'n/a'}</div>
              <div><span className="text-slate-500">Generated:</span> {new Date(scorecard.generatedAt).toLocaleString()}</div>
              <div><span className="text-slate-500">Metric refs:</span> {scorecard.metricRefs.length ? scorecard.metricRefs.join(', ') : 'none'}</div>
              <div><span className="text-slate-500">Reviewed:</span> {scorecard.reviewedAt ? new Date(scorecard.reviewedAt).toLocaleString() : 'n/a'}</div>
              <div><span className="text-slate-500">Reviewed by:</span> {scorecard.reviewedByPersonId ?? 'n/a'}</div>
            </div>
          </div>
        </div>
        <div className="assurarr-card">
          <div className="assurarr-card-inner space-y-4">
            <div className="flex items-center justify-between gap-3">
              <p className="assurarr-label">Create metric</p>
              <span className="assurarr-pill">{metrics.length} metrics</span>
            </div>
            <div className="assurarr-grid cols-2">
              <input className="assurarr-input" value={metricForm.metricKey} onChange={(event) => setMetricForm({ ...metricForm, metricKey: event.target.value })} placeholder="open-nonconformance-count" />
              <input className="assurarr-input" value={metricForm.title} onChange={(event) => setMetricForm({ ...metricForm, title: event.target.value })} placeholder="Open nonconformance count" />
              <textarea className="assurarr-textarea" value={metricForm.description} onChange={(event) => setMetricForm({ ...metricForm, description: event.target.value })} placeholder="Explain the metric and what it measures." />
              <select className="assurarr-select" value={metricForm.category} onChange={(event) => setMetricForm({ ...metricForm, category: event.target.value })}>
                <option value="nonconformance">Nonconformance</option>
                <option value="hold">Hold</option>
                <option value="capa">CAPA</option>
                <option value="audit">Audit</option>
                <option value="supplier">Supplier</option>
                <option value="customer">Customer</option>
                <option value="delivery">Delivery</option>
                <option value="inventory">Inventory</option>
                <option value="maintenance">Maintenance</option>
                <option value="documentation">Documentation</option>
              </select>
              <input className="assurarr-input" value={metricForm.value} onChange={(event) => setMetricForm({ ...metricForm, value: event.target.value })} placeholder="0" />
              <input className="assurarr-input" value={metricForm.unit} onChange={(event) => setMetricForm({ ...metricForm, unit: event.target.value })} placeholder="count" />
              <input className="assurarr-input" value={metricForm.numerator} onChange={(event) => setMetricForm({ ...metricForm, numerator: event.target.value })} placeholder="0" />
              <input className="assurarr-input" value={metricForm.denominator} onChange={(event) => setMetricForm({ ...metricForm, denominator: event.target.value })} placeholder="0" />
              <input className="assurarr-input" value={metricForm.targetValue} onChange={(event) => setMetricForm({ ...metricForm, targetValue: event.target.value })} placeholder="0" />
              <input className="assurarr-input" value={metricForm.warningThreshold} onChange={(event) => setMetricForm({ ...metricForm, warningThreshold: event.target.value })} placeholder="2" />
              <input className="assurarr-input" value={metricForm.criticalThreshold} onChange={(event) => setMetricForm({ ...metricForm, criticalThreshold: event.target.value })} placeholder="5" />
              <select className="assurarr-select" value={metricForm.status} onChange={(event) => setMetricForm({ ...metricForm, status: event.target.value })}>
                <option value="good">Good</option>
                <option value="warning">Warning</option>
                <option value="critical">Critical</option>
                <option value="unknown">Unknown</option>
              </select>
              <textarea className="assurarr-textarea" value={metricForm.sourceProductRefs} onChange={(event) => setMetricForm({ ...metricForm, sourceProductRefs: event.target.value })} placeholder="One product ref per line or comma-separated" />
            </div>
            <button className="assurarr-button" type="button" onClick={() => createMetricMutation.mutate()} disabled={createMetricMutation.isPending}>
              {createMetricMutation.isPending ? 'Saving...' : 'Create metric'}
            </button>
          </div>
        </div>
        <div className="assurarr-card">
          <div className="assurarr-card-inner space-y-4">
            <p className="assurarr-label">Review</p>
            <div className="grid gap-3 md:grid-cols-2">
              <Field label="Reviewed by person id">
                <input className="assurarr-input" value={reviewForm.reviewedByPersonId} onChange={(event) => setReviewForm({ ...reviewForm, reviewedByPersonId: event.target.value })} placeholder="Optional UUID" />
              </Field>
              <Field label="Reviewed at">
                <input className="assurarr-input" type="datetime-local" value={reviewForm.reviewedAt} onChange={(event) => setReviewForm({ ...reviewForm, reviewedAt: event.target.value })} />
              </Field>
            </div>
            <button className="assurarr-button secondary" type="button" onClick={() => reviewMutation.mutate()} disabled={reviewMutation.isPending}>
              {reviewMutation.isPending ? 'Saving...' : 'Mark reviewed'}
            </button>
          </div>
        </div>
      </div>
      <div className="assurarr-card mt-6">
        <div className="assurarr-card-inner space-y-3">
          <p className="assurarr-label">Metrics</p>
          {metrics.length ? (
            <div className="grid gap-3 md:grid-cols-2">
              {metrics.map((metric) => (
                <div key={metric.id} className="rounded-2xl border border-slate-700/70 bg-slate-950/70 p-4">
                  <div className="flex items-center justify-between gap-3">
                    <strong className="text-sm text-slate-50">{metric.metricKey}</strong>
                    <span className="assurarr-pill">{metric.status}</span>
                  </div>
                  <p className="mt-1 text-sm text-slate-300">{metric.title}</p>
                  <p className="mt-1 text-xs text-slate-400">{metric.description}</p>
                  <div className="mt-3 grid gap-2 text-xs text-slate-400 md:grid-cols-2">
                    <div>Category: {metric.category}</div>
                    <div>Value: {metric.value ?? 'n/a'} {metric.unit ?? ''}</div>
                    <div>Numerator: {metric.numerator ?? 'n/a'}</div>
                    <div>Denominator: {metric.denominator ?? 'n/a'}</div>
                    <div>Target: {metric.targetValue ?? 'n/a'}</div>
                    <div>Thresholds: {metric.warningThreshold ?? 'n/a'} / {metric.criticalThreshold ?? 'n/a'}</div>
                  </div>
                  <p className="mt-3 text-xs text-slate-400">
                    Source refs: {metric.sourceProductRefs.length ? metric.sourceProductRefs.join(', ') : 'none'}
                  </p>
                </div>
              ))}
            </div>
          ) : (
            <EmptyState title="No metrics yet." />
          )}
        </div>
      </div>
    </div>
  )
}

function HistoryPage() {
  const query = useDashboard()

  return (
    <div className="assurarr-page">
      <PageHeader title="History" description="History is recorded as part of the quality dashboard event stream." />
      <div className="assurarr-card">
        <div className="assurarr-card-inner space-y-3">
          {query.data?.recentEvents.length ? (
            query.data.recentEvents.map((event) => (
              <div key={event.id} className="rounded-xl border border-slate-700/70 bg-slate-900/80 p-3">
                <div className="flex items-center justify-between gap-3">
                  <strong className="text-sm text-slate-100">{event.eventType}</strong>
                  <time className="text-xs text-slate-400">{new Date(event.occurredAt).toLocaleString()}</time>
                </div>
                <p className="mt-1 text-sm text-slate-300">
                  {event.subjectType} {event.subjectId}
                  {event.details ? ` - ${event.details}` : ''}
                </p>
              </div>
            ))
          ) : (
            <EmptyState title="No history yet." />
          )}
        </div>
      </div>
    </div>
  )
}

function SettingsPage() {
  const roleGroups = [
    {
      name: 'Quality Viewer',
      permissions: ['Read nonconformances', 'Read holds', 'Read CAPAs', 'Read audits', 'Read scorecards'],
    },
    {
      name: 'Quality Technician',
      permissions: ['Create nonconformances', 'Complete containment actions', 'Upload evidence', 'Execute assigned audit checklist items'],
    },
    {
      name: 'Quality Reviewer',
      permissions: ['Triage nonconformances', 'Review evidence', 'Propose dispositions', 'Request hold releases'],
    },
    {
      name: 'Quality Manager',
      permissions: ['Place and release holds', 'Approve dispositions', 'Open and close CAPAs', 'Approve verification', 'Close nonconformances'],
    },
    {
      name: 'Supplier Quality Manager',
      permissions: ['Manage supplier quality issues', 'Send and review SCARs', 'Update supplier quality status'],
    },
    {
      name: 'Customer Quality Manager',
      permissions: ['Manage customer complaint quality cases', 'Prepare response records', 'Coordinate customer-facing closure with CustomArr'],
    },
    {
      name: 'Quality Auditor',
      permissions: ['Create and execute audits', 'Create findings', 'Verify finding closure'],
    },
    {
      name: 'AssurArr Admin',
      permissions: ['Manage settings', 'Manage templates', 'Manage catalogs', 'Manage role configuration'],
    },
  ]

  const uiSurfaces = [
    'dashboard',
    'nonconformances',
    'nonconformance detail',
    'holds',
    'hold detail',
    'containment actions',
    'dispositions',
    'CAPA',
    'CAPA detail',
    'audits',
    'audit detail',
    'findings',
    'supplier quality',
    'customer complaints',
    'quality releases',
    'scorecards',
    'settings',
  ]

  const integrationBoundaries = [
    'RecordArr owns supporting evidence files and document retention.',
    'StaffArr owns reviewer and approver identity data.',
    'CustomArr owns customer-facing closure workflows where referenced.',
    'AssurArr owns the quality workflow state, history, and dashboard summaries.',
  ]

  return (
    <div className="assurarr-page">
      <PageHeader title="Settings" description="Admin reference for roles, product surfaces, and AssurArr integration boundaries." />
      <div className="grid gap-4 lg:grid-cols-2">
        <div className="assurarr-card">
          <div className="assurarr-card-header">
            <h2 className="text-lg font-semibold text-slate-50">Role configuration</h2>
            <p className="text-sm text-slate-400">Doc-backed role examples for the AssurArr admin and quality workflows.</p>
          </div>
          <div className="assurarr-card-inner space-y-4">
            {roleGroups.map((role) => (
              <div key={role.name} className="rounded-2xl border border-slate-700/80 bg-slate-950/40 p-4">
                <h3 className="text-sm font-semibold uppercase tracking-[0.2em] text-cyan-300">{role.name}</h3>
                <ul className="mt-3 space-y-1 text-sm text-slate-300">
                  {role.permissions.map((permission) => (
                    <li key={permission}>• {permission}</li>
                  ))}
                </ul>
              </div>
            ))}
          </div>
        </div>

        <div className="assurarr-card">
          <div className="assurarr-card-header">
            <h2 className="text-lg font-semibold text-slate-50">Product surfaces</h2>
            <p className="text-sm text-slate-400">The UI routes explicitly called out in the AssurArr feature set.</p>
          </div>
          <div className="assurarr-card-inner">
            <div className="flex flex-wrap gap-2">
              {uiSurfaces.map((surface) => (
                <span key={surface} className="rounded-full border border-slate-700 bg-slate-950/50 px-3 py-1 text-xs uppercase tracking-[0.18em] text-slate-300">
                  {surface}
                </span>
              ))}
            </div>
          </div>
        </div>

        <div className="assurarr-card">
          <div className="assurarr-card-header">
            <h2 className="text-lg font-semibold text-slate-50">Integration boundaries</h2>
            <p className="text-sm text-slate-400">Ownership reminders pulled from the constitution and product docs.</p>
          </div>
          <div className="assurarr-card-inner space-y-3 text-sm text-slate-300">
            {integrationBoundaries.map((item) => (
              <p key={item}>{item}</p>
            ))}
          </div>
        </div>

        <div className="assurarr-card">
          <div className="assurarr-card-header">
            <h2 className="text-lg font-semibold text-slate-50">Runtime wiring</h2>
            <p className="text-sm text-slate-400">Local environment and API endpoint reference for AssurArr.</p>
          </div>
          <div className="assurarr-card-inner space-y-2 text-sm text-slate-300">
            <p>Frontend base URL: <span className="assurarr-kbd">{import.meta.env.VITE_ASSURARR_API_BASE ?? '/api proxy'}</span></p>
            <p>Local preview port: <span className="assurarr-kbd">5183</span></p>
            <p>API port: <span className="assurarr-kbd">5109</span></p>
          </div>
        </div>
      </div>
    </div>
  )
}

export function App() {
  const location = useLocation()

  const title = useMemo(() => {
    const path = location.pathname
    if (path.startsWith('/nonconformances')) return 'Nonconformances'
    if (path.startsWith('/holds')) return 'Holds'
    if (path.startsWith('/capa')) return 'CAPA'
    if (path.startsWith('/audits')) return 'Audits'
    if (path.startsWith('/findings')) return 'Findings'
    if (path.startsWith('/reviews')) return 'Reviews'
    if (path.startsWith('/releases')) return 'Releases'
    if (path.startsWith('/containment')) return 'Containment'
    if (path.startsWith('/dispositions')) return 'Dispositions'
    if (path.startsWith('/supplier-quality')) return 'Supplier quality'
    if (path.startsWith('/scars')) return 'SCARs'
    if (path.startsWith('/complaints')) return 'Complaints'
    if (path.startsWith('/status')) return 'Status'
    if (path.startsWith('/risk-profiles')) return 'Risk profiles'
    if (path.startsWith('/scorecards')) return 'Scorecards'
    if (path.startsWith('/history')) return 'History'
    return 'Dashboard'
  }, [location.pathname])

  return (
    <AppShell>
      <Routes>
        <Route index element={<DashboardPage />} />
        <Route path="/nonconformances" element={<NonconformancePage />} />
        <Route path="/nonconformances/:id" element={<NonconformanceDetailPage />} />
        <Route path="/holds" element={<HoldPage />} />
        <Route path="/holds/:id" element={<HoldDetailPage />} />
        <Route path="/capa" element={<CapaPage />} />
        <Route path="/capa/:id" element={<CapaDetailPage />} />
        <Route path="/audits" element={<AuditPage />} />
        <Route path="/audits/:id" element={<AuditDetailPage />} />
        <Route path="/findings" element={<FindingsPage />} />
        <Route path="/findings/:id" element={<FindingDetailPage />} />
        <Route path="/reviews" element={<ReviewPage />} />
        <Route path="/reviews/:id" element={<ReviewDetailPage />} />
        <Route path="/releases" element={<ReleasePage />} />
        <Route path="/releases/:id" element={<ReleaseDetailPage />} />
        <Route path="/containment" element={<ContainmentPage />} />
        <Route path="/containment/:id" element={<ContainmentDetailPage />} />
        <Route path="/dispositions" element={<DispositionPage />} />
        <Route path="/dispositions/:id" element={<DispositionDetailPage />} />
        <Route path="/supplier-quality" element={<SupplierQualityPage />} />
        <Route path="/supplier-quality/:id" element={<SupplierQualityDetailPage />} />
        <Route path="/scars" element={<ScarPage />} />
        <Route path="/scars/:id" element={<ScarDetailPage />} />
        <Route path="/complaints" element={<CustomerComplaintPage />} />
        <Route path="/complaints/:id" element={<CustomerComplaintDetailPage />} />
        <Route path="/status" element={<StatusPage />} />
        <Route path="/risk-profiles/:id" element={<RiskProfileDetailPage />} />
        <Route path="/scorecards" element={<ScorecardPage />} />
        <Route path="/scorecards/:id" element={<ScorecardDetailPage />} />
        <Route path="/history" element={<HistoryPage />} />
        <Route path="/settings" element={<SettingsPage />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
      <p className="mt-6 text-sm text-slate-400">Current view: {title}</p>
    </AppShell>
  )
}
