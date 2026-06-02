import { useQuery } from '@tanstack/react-query'
import {
  AlertTriangle,
  ArrowLeft,
  Boxes,
  Building2,
  CalendarClock,
  CheckCircle2,
  ChevronDown,
  CircleCheck,
  DollarSign,
  FileText,
  History,
  Mail,
  MapPin,
  MoreHorizontal,
  PackagePlus,
  Pencil,
  Phone,
  Plus,
  ShieldCheck,
  Star,
  Truck,
  Users,
  XCircle,
} from 'lucide-react'
import { Link, useLocation } from 'react-router-dom'
import { useMemo, type ReactNode } from 'react'
import {
  getCompliancePartyDetail,
  getPartyVendorRestrictionEnforcement,
  getSupplierOnboardingByParty,
  getVendorReportDetail,
  getVendorSupplyReadiness,
  listAuditHistory,
  listPartyComplianceDocuments,
  listPartySupplierIncidents,
  listPartyVendorRestrictions,
} from '../../api/client'
import type {
  CreatePartyContactRequest,
  ExternalPartyResponse,
  PartyComplianceDocumentResponse,
  UpdateExternalPartyRequest,
} from '../../api/types'
import { PartyRegistryPanel } from '../../components/PartyRegistryPanel'
import type { SupplyArrWorkspaceState } from '../useSupplyArrWorkspaceState'

type Props = { state: SupplyArrWorkspaceState }
type PartiesViewMode = 'drawer' | 'details' | 'create'
type Tone = 'good' | 'warn' | 'bad' | 'info' | 'neutral'

const detailTabs = [
  'Overview',
  'Contacts',
  'Documents',
  'Items & Services',
  'Purchase Orders',
  'Quotes',
  'Performance',
  'History',
]

function partyRegistryHandlers(
  s: SupplyArrWorkspaceState,
  route: 'vendors' | 'suppliers' | 'dealers',
) {
  return {
    onUpdateParty: (partyId: string, request: UpdateExternalPartyRequest) =>
      s.updatePartyMutation.mutate({ route, partyId, request }),
    onUpdateApprovalStatus: (partyId: string, approvalStatus: string) =>
      s.updatePartyApprovalMutation.mutate({ route, partyId, approvalStatus }),
    onUpdateStatus: (partyId: string, status: string) =>
      s.updatePartyStatusMutation.mutate({ route, partyId, status }),
    onAddContact: (partyId: string, request: CreatePartyContactRequest) =>
      s.addPartyContactMutation.mutate({ route, partyId, request }),
    isUpdating: s.updatePartyMutation.isPending,
    isUpdatingApproval: s.updatePartyApprovalMutation.isPending,
    isUpdatingStatus: s.updatePartyStatusMutation.isPending,
    isAddingContact: s.addPartyContactMutation.isPending,
  }
}

function humanize(value: string | null | undefined): string {
  if (!value) return 'Not recorded'
  return value.replace(/[_-]+/g, ' ').replace(/\b\w/g, (char) => char.toUpperCase())
}

function formatDate(value: string | null | undefined): string {
  if (!value) return 'Not recorded'
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return 'Not recorded'
  return date.toLocaleDateString(undefined, { month: 'short', day: '2-digit', year: 'numeric' })
}

function daysUntil(value: string | null | undefined): number | null {
  if (!value) return null
  const timestamp = Date.parse(value)
  if (!Number.isFinite(timestamp)) return null
  return Math.ceil((timestamp - Date.now()) / 86_400_000)
}

function formatCurrency(value: number): string {
  if (!Number.isFinite(value) || value <= 0) return 'Not tracked'
  return new Intl.NumberFormat(undefined, {
    style: 'currency',
    currency: 'USD',
    maximumFractionDigits: value >= 1000 ? 0 : 2,
  }).format(value)
}

function badgeClass(tone: Tone): string {
  if (tone === 'good') return 'border-emerald-400/30 bg-emerald-500/15 text-emerald-200'
  if (tone === 'warn') return 'border-amber-400/30 bg-amber-500/15 text-amber-200'
  if (tone === 'bad') return 'border-red-400/30 bg-red-500/15 text-red-200'
  if (tone === 'info') return 'border-sky-400/30 bg-sky-500/15 text-sky-200'
  return 'border-slate-500/30 bg-slate-500/10 text-slate-300'
}

function statusTone(value: string | null | undefined): Tone {
  const normalized = value?.toLowerCase() ?? ''
  if (['approved', 'active', 'current', 'satisfied', 'ready'].includes(normalized)) return 'good'
  if (['pending', 'review', 'submitted', 'expiring_soon'].includes(normalized)) return 'warn'
  if (['restricted', 'inactive', 'blocked', 'rejected', 'expired', 'not_ready'].includes(normalized)) return 'bad'
  return 'neutral'
}

function Badge({ label, tone = 'neutral' }: { label: string; tone?: Tone }) {
  return (
    <span className={`inline-flex items-center rounded-full border px-3 py-1 text-xs font-semibold ${badgeClass(tone)}`}>
      {label}
    </span>
  )
}

function MetricCard({
  label,
  value,
  hint,
  icon,
  tone = 'neutral',
}: {
  label: string
  value: string | number
  hint: string
  icon: ReactNode
  tone?: Tone
}) {
  const iconClass = {
    good: 'bg-emerald-500/15 text-emerald-300',
    warn: 'bg-amber-500/15 text-amber-300',
    bad: 'bg-red-500/15 text-red-300',
    info: 'bg-sky-500/15 text-sky-300',
    neutral: 'bg-slate-700/60 text-slate-300',
  }[tone]

  return (
    <section className="min-h-32 rounded-2xl border border-slate-800 bg-slate-950/70 p-4">
      <div className="flex items-start justify-between gap-3">
        <div>
          <p className="text-sm text-sky-200/80">{label}</p>
          <p className="mt-3 text-3xl font-bold tracking-normal text-white">{value}</p>
        </div>
        <div className={`rounded-xl p-3 ${iconClass}`}>{icon}</div>
      </div>
      <p className="mt-2 text-xs text-slate-400">{hint}</p>
    </section>
  )
}

function SnapshotField({ label, value, source }: { label: string; value: string; source: string }) {
  return (
    <div className="min-h-[4.5rem] rounded-xl border border-slate-800 bg-slate-950/60 p-3">
      <div className="flex items-start justify-between gap-2">
        <dt className="text-xs font-semibold uppercase tracking-normal text-sky-200/55">{label}</dt>
        <span className="shrink-0 text-[10px] text-slate-500">{source}</span>
      </div>
      <dd className="mt-3 break-words text-sm font-semibold text-white">{value}</dd>
    </div>
  )
}

function RailSection({ title, icon, children }: { title: string; icon: ReactNode; children: ReactNode }) {
  return (
    <section className="rounded-2xl border border-slate-800 bg-slate-950/70 p-5">
      <div className="mb-4 flex items-center justify-between gap-3">
        <h2 className="text-lg font-bold text-white">{title}</h2>
        <div className="text-sky-300">{icon}</div>
      </div>
      {children}
    </section>
  )
}

function EmptyPanel({ text }: { text: string }) {
  return (
    <div className="rounded-xl border border-slate-800 bg-slate-950/40 p-4 text-sm text-slate-400">
      {text}
    </div>
  )
}

function normalizeDocumentStatus(document: PartyComplianceDocumentResponse): string {
  if (document.reviewStatus === 'approved' && document.expiresAt) {
    const remaining = daysUntil(document.expiresAt)
    if (remaining != null && remaining < 0) return 'Expired'
    if (remaining != null && remaining <= 60) return 'Review'
  }
  return humanize(document.reviewStatus)
}

function PartiesRegistryWorkspace({ state: s, mode }: { state: SupplyArrWorkspaceState; mode: PartiesViewMode }) {
  const vendorHandlers = partyRegistryHandlers(s, 'vendors')
  const supplierHandlers = partyRegistryHandlers(s, 'suppliers')
  const dealerHandlers = partyRegistryHandlers(s, 'dealers')

  return (
    <div className="grid gap-6 lg:grid-cols-2" data-testid="supplyarr-party-registry-workspace">
      <PartyRegistryPanel
        mode={mode}
        title="Vendors"
        partyType="vendors"
        parties={s.vendors}
        canManage={s.canManage}
        isLoading={s.vendorsQuery.isLoading}
        partyKey={s.vendorKey}
        displayName={s.vendorName}
        legalName={s.vendorLegalName}
        taxIdentifier={s.vendorTaxId}
        notes={s.vendorNotes}
        onPartyKeyChange={s.setVendorKey}
        onDisplayNameChange={s.setVendorName}
        onLegalNameChange={s.setVendorLegalName}
        onTaxIdentifierChange={s.setVendorTaxId}
        onNotesChange={s.setVendorNotes}
        onCreate={() => s.createVendorMutation.mutate()}
        isCreating={s.createVendorMutation.isPending}
        {...vendorHandlers}
      />
      <PartyRegistryPanel
        mode={mode}
        title="Suppliers"
        partyType="suppliers"
        parties={s.suppliersQuery.data ?? []}
        canManage={s.canManage}
        isLoading={s.suppliersQuery.isLoading}
        partyKey={s.supplierKey}
        displayName={s.supplierName}
        legalName={s.supplierLegalName}
        taxIdentifier={s.supplierTaxId}
        notes={s.supplierNotes}
        onPartyKeyChange={s.setSupplierKey}
        onDisplayNameChange={s.setSupplierName}
        onLegalNameChange={s.setSupplierLegalName}
        onTaxIdentifierChange={s.setSupplierTaxId}
        onNotesChange={s.setSupplierNotes}
        onCreate={() => s.createSupplierMutation.mutate()}
        isCreating={s.createSupplierMutation.isPending}
        {...supplierHandlers}
      />
      <PartyRegistryPanel
        mode={mode}
        title="Dealers"
        partyType="dealers"
        parties={s.dealersQuery.data ?? []}
        canManage={s.canManage}
        isLoading={s.dealersQuery.isLoading}
        partyKey={s.dealerKey}
        displayName={s.dealerName}
        legalName={s.dealerLegalName}
        taxIdentifier={s.dealerTaxId}
        notes={s.dealerNotes}
        onPartyKeyChange={s.setDealerKey}
        onDisplayNameChange={s.setDealerName}
        onLegalNameChange={s.setDealerLegalName}
        onTaxIdentifierChange={s.setDealerTaxId}
        onNotesChange={s.setDealerNotes}
        onCreate={() => s.createDealerMutation.mutate()}
        isCreating={s.createDealerMutation.isPending}
        {...dealerHandlers}
      />
    </div>
  )
}

function PartiesProfile({ state: s, parties }: { state: SupplyArrWorkspaceState; parties: ExternalPartyResponse[] }) {
  const location = useLocation()
  const requestedPartyId = useMemo(() => new URLSearchParams(location.search).get('partyId') ?? '', [location.search])
  const selectedParty = useMemo(() => {
    if (requestedPartyId) {
      const requested = parties.find((party) => party.partyId === requestedPartyId)
      if (requested) return requested
    }

    return (
      parties.find((party) => party.approvalStatus === 'approved' && party.status === 'active') ??
      parties.find((party) => party.status === 'active') ??
      parties[0] ??
      null
    )
  }, [parties, requestedPartyId])

  const selectedPartyId = selectedParty?.partyId ?? ''
  const hasSelectedParty = Boolean(selectedParty && selectedPartyId)
  const detailQueriesEnabled = Boolean(s.accessToken && hasSelectedParty)

  const readinessQuery = useQuery({
    queryKey: ['supplyarr-party-profile-readiness', s.accessToken, selectedPartyId],
    queryFn: () => getVendorSupplyReadiness(s.accessToken, selectedPartyId),
    enabled: detailQueriesEnabled && s.canReadSupplyReadiness,
  })
  const vendorReportQuery = useQuery({
    queryKey: ['supplyarr-party-profile-vendor-report', s.accessToken, selectedPartyId],
    queryFn: () => getVendorReportDetail(s.accessToken, selectedPartyId),
    enabled: detailQueriesEnabled && s.canReadVendorReports,
  })
  const complianceDetailQuery = useQuery({
    queryKey: ['supplyarr-party-profile-compliance', s.accessToken, selectedPartyId],
    queryFn: () => getCompliancePartyDetail(s.accessToken, selectedPartyId),
    enabled: detailQueriesEnabled && s.canReadComplianceReports,
  })
  const documentsQuery = useQuery({
    queryKey: ['supplyarr-party-profile-documents', s.accessToken, selectedPartyId],
    queryFn: () => listPartyComplianceDocuments(s.accessToken, selectedPartyId),
    enabled: detailQueriesEnabled && (s.canManage || s.canReadComplianceReports),
  })
  const onboardingQuery = useQuery({
    queryKey: ['supplyarr-party-profile-onboarding', s.accessToken, selectedPartyId],
    queryFn: () => getSupplierOnboardingByParty(s.accessToken, selectedPartyId),
    enabled: detailQueriesEnabled && (s.canManage || s.canApprovePr),
    retry: false,
  })
  const restrictionsQuery = useQuery({
    queryKey: ['supplyarr-party-profile-restrictions', s.accessToken, selectedPartyId],
    queryFn: () => listPartyVendorRestrictions(s.accessToken, selectedPartyId),
    enabled: detailQueriesEnabled && s.canManage,
  })
  const enforcementQuery = useQuery({
    queryKey: ['supplyarr-party-profile-enforcement', s.accessToken, selectedPartyId],
    queryFn: () => getPartyVendorRestrictionEnforcement(s.accessToken, selectedPartyId),
    enabled: detailQueriesEnabled && s.canManage,
  })
  const incidentsQuery = useQuery({
    queryKey: ['supplyarr-party-profile-incidents', s.accessToken, selectedPartyId],
    queryFn: () => listPartySupplierIncidents(s.accessToken, selectedPartyId),
    enabled: detailQueriesEnabled && s.canManage,
  })
  const auditQuery = useQuery({
    queryKey: ['supplyarr-party-profile-audit', s.accessToken, selectedPartyId],
    queryFn: () => listAuditHistory(s.accessToken, { targetId: selectedPartyId, limit: 5 }),
    enabled: detailQueriesEnabled && s.canReadAuditHistory,
  })

  if (!selectedParty) {
    return (
      <div className="rounded-3xl border border-slate-800 bg-slate-950/70 p-8 text-center">
        <Building2 className="mx-auto h-10 w-10 text-sky-300" />
        <h1 className="mt-4 text-2xl font-bold text-white">No supplier profile selected</h1>
        <p className="mt-2 text-sm text-slate-400">Create or load a vendor, supplier, or dealer to view its profile.</p>
        <Link
          to="/parties/create"
          className="mt-5 inline-flex items-center gap-2 rounded-xl bg-sky-500 px-4 py-2 text-sm font-semibold text-slate-950 hover:bg-sky-400"
        >
          <Plus className="h-4 w-4" />
          Add party
        </Link>
      </div>
    )
  }

  const primaryContact = selectedParty.contacts.find((contact) => contact.isPrimary) ?? selectedParty.contacts[0] ?? null
  const parts = s.partsQuery?.data ?? []
  const partyPartLinks = parts.flatMap((part) =>
    part.vendorLinks
      .filter((link) => link.partyId === selectedParty.partyId)
      .map((link) => ({ part, link })),
  )
  const purchaseOrders = (s.purchaseOrdersQuery?.data ?? []).filter((order) => order.vendorPartyId === selectedParty.partyId)
  const purchaseRequests = (s.purchaseRequestsQuery?.data ?? []).filter(
    (request) => request.vendorPartyId === selectedParty.partyId,
  )
  const openOrders = purchaseOrders.filter((order) => !['cancelled', 'received', 'closed'].includes(order.status))
  const waitingShipmentCount = purchaseOrders.filter((order) =>
    order.lines.some((line) => line.quantityRemaining > 0),
  ).length
  const orderedQuantity = purchaseOrders.reduce(
    (total, order) => total + order.lines.reduce((lineTotal, line) => lineTotal + line.quantityOrdered, 0),
    0,
  )
  const receivedQuantity = purchaseOrders.reduce(
    (total, order) => total + order.lines.reduce((lineTotal, line) => lineTotal + line.quantityReceived, 0),
    0,
  )
  const fillRate = orderedQuantity > 0 ? Math.round((receivedQuantity / orderedQuantity) * 100) : null
  const leadTimes = partyPartLinks
    .map(({ link }) => link.catalogLeadTimeDays)
    .filter((value): value is number => typeof value === 'number')
  const averageLeadTime = leadTimes.length
    ? leadTimes.reduce((total, value) => total + value, 0) / leadTimes.length
    : null
  const ytdSpend = purchaseOrders.reduce((total, order) => {
    const createdYear = new Date(order.createdAt).getFullYear()
    if (createdYear !== new Date().getFullYear()) return total
    return total + order.lines.reduce((lineTotal, line) => {
      const pricedLink = partyPartLinks.find(({ part, link }) => part.partId === line.partId && link.catalogUnitPrice != null)
      return lineTotal + line.quantityOrdered * (pricedLink?.link.catalogUnitPrice ?? 0)
    }, 0)
  }, 0)
  const vendorSummary = vendorReportQuery.data?.summary
  const reportOpenOrders = vendorSummary?.openPurchaseOrderCount ?? openOrders.length
  const reportPartLinks = vendorSummary?.partVendorLinkCount ?? partyPartLinks.length
  const reportPreferredLinks = vendorSummary?.preferredPartLinkCount ?? partyPartLinks.filter(({ link }) => link.isPreferred).length
  const complianceDocuments = complianceDetailQuery.data?.documents
  const rawDocuments = documentsQuery.data ?? []
  const documents = complianceDocuments
    ? complianceDocuments.map((document) => ({
        id: document.documentId,
        title: document.title,
        status: document.effectiveStatus === 'effective' ? humanize(document.reviewStatus) : humanize(document.effectiveStatus),
        tone: document.isExpired ? 'bad' as Tone : document.isExpiringSoon ? 'warn' as Tone : statusTone(document.reviewStatus),
        subtitle: document.expiresAt ? `Expires ${formatDate(document.expiresAt)}` : `Updated ${formatDate(document.updatedAt)}`,
        expiresAt: document.expiresAt,
        isAttention: document.isExpired || document.isExpiringSoon || document.reviewStatus !== 'approved',
      }))
    : rawDocuments.map((document) => ({
        id: document.documentId,
        title: document.title,
        status: normalizeDocumentStatus(document),
        tone: statusTone(normalizeDocumentStatus(document)),
        subtitle: document.expiresAt ? `Expires ${formatDate(document.expiresAt)}` : `Uploaded ${formatDate(document.createdAt)}`,
        expiresAt: document.expiresAt,
        isAttention: normalizeDocumentStatus(document) !== 'Approved',
      }))
  const activeRestrictions = (restrictionsQuery.data ?? []).filter((restriction) => restriction.status === 'active')
  const openIncidents = (incidentsQuery.data ?? []).filter(
    (incident) => !['resolved', 'closed', 'cancelled'].includes(incident.status),
  )
  const onboardingRequirements = onboardingQuery.data?.documentRequirements ?? []
  const unsatisfiedOnboarding = onboardingRequirements.filter((requirement) => requirement.isRequired && !requirement.isSatisfied)
  const readiness = readinessQuery.data
  const enforcement = enforcementQuery.data
  const isBlocked =
    selectedParty.status !== 'active' ||
    selectedParty.approvalStatus === 'restricted' ||
    readiness?.readinessStatus === 'not_ready' ||
    Boolean(enforcement?.isBlocked) ||
    activeRestrictions.length > 0
  const hasWatchItems =
    selectedParty.approvalStatus !== 'approved' ||
    documents.some((document) => document.isAttention) ||
    unsatisfiedOnboarding.length > 0 ||
    openIncidents.length > 0
  const decisionTone: Tone = isBlocked ? 'bad' : hasWatchItems ? 'warn' : 'good'
  const decisionLabel = isBlocked ? 'Blocked' : hasWatchItems ? 'Approved with watch' : 'Approved'
  const decisionTitle = isBlocked
    ? 'Purchasing blocked'
    : hasWatchItems
      ? 'Purchasing allowed, review watch items'
      : 'Purchasing allowed'
  const decisionDetail = isBlocked
    ? enforcement?.blockReason ?? activeRestrictions[0]?.reason ?? 'Active restriction or readiness blocker prevents normal purchasing.'
    : hasWatchItems
      ? 'Normal purchasing may continue while open documents, onboarding, or incident checks are monitored.'
      : 'Supplier checks allow normal purchasing and receiving activity.'
  const blockedChecks = [
    selectedParty.status !== 'active',
    selectedParty.approvalStatus === 'restricted',
    Boolean(enforcement?.isBlocked),
    activeRestrictions.length > 0,
    readiness?.readinessStatus === 'not_ready',
  ].filter(Boolean).length
  const allowedChecks = [
    selectedParty.status === 'active',
    selectedParty.approvalStatus === 'approved',
    !enforcement?.isBlocked,
    activeRestrictions.length === 0,
    readiness?.readinessStatus !== 'not_ready',
    Boolean(primaryContact),
    reportPartLinks > 0,
    documents.every((document) => document.tone !== 'bad'),
    openIncidents.length === 0,
  ].filter(Boolean).length
  const documentWatchLabel = documents.some((document) => document.isAttention) ? 'Document watch' : null
  const upcomingRequirements = [
    ...documents
      .filter((document) => document.isAttention)
      .slice(0, 2)
      .map((document) => ({
        title: `${document.title} review`,
        detail: document.subtitle,
        badge: document.expiresAt && daysUntil(document.expiresAt) != null
          ? `Due in ${Math.max(daysUntil(document.expiresAt) ?? 0, 0)} days`
          : document.status,
        tone: document.tone,
      })),
    ...unsatisfiedOnboarding.slice(0, 2).map((requirement) => ({
      title: requirement.label,
      detail: 'Required supplier onboarding document',
      badge: 'Required',
      tone: 'warn' as Tone,
    })),
    ...openIncidents.slice(0, 1).map((incident) => ({
      title: incident.title,
      detail: humanize(incident.severity),
      badge: humanize(incident.status),
      tone: 'bad' as Tone,
    })),
  ].slice(0, 3)
  const recentPurchaseOrders = vendorReportQuery.data?.recentPurchaseOrders.map((order) => ({
    id: order.purchaseOrderId,
    key: order.orderKey,
    title: order.title,
    status: order.status,
    detail: `${order.lineCount} line${order.lineCount === 1 ? '' : 's'} - ${order.quantityReceived}/${order.quantityOrdered} received`,
  })) ?? purchaseOrders.slice(0, 3).map((order) => ({
    id: order.purchaseOrderId,
    key: order.orderKey,
    title: order.title,
    status: order.status,
    detail: order.lines[0]
      ? `${order.lines[0].partDisplayName} - ${order.lines[0].quantityOrdered} ${order.lines[0].unitOfMeasure}`
      : 'No lines',
  }))
  const recentActivity = auditQuery.data?.items.map((item) => ({
    id: item.id,
    category: humanize(item.action.split('.').slice(-1)[0]),
    title: humanize(item.action),
    detail: `${formatDate(item.occurredAt)} - ${item.result}`,
  })) ?? [
    ...recentPurchaseOrders.slice(0, 2).map((order) => ({
      id: order.id,
      category: 'Purchase order',
      title: `${order.key} ${humanize(order.status)}`,
      detail: order.detail,
    })),
    ...documents.slice(0, 2).map((document) => ({
      id: document.id,
      category: 'Document',
      title: `${document.title} ${document.status}`,
      detail: document.subtitle,
    })),
  ]

  return (
    <div className="space-y-6" data-testid="supplyarr-party-profile">
      <section className="rounded-3xl border border-slate-800 bg-slate-950/80 p-6 shadow-2xl shadow-sky-950/20">
        <div className="flex flex-wrap items-start justify-between gap-5">
          <div className="min-w-0">
            <nav className="mb-5 flex flex-wrap items-center gap-3 text-sm text-sky-200/80">
              <Link
                to="/parties/drawer"
                className="inline-flex items-center gap-2 rounded-xl border border-slate-800 bg-slate-950/60 px-3 py-2 hover:border-sky-700"
              >
                <ArrowLeft className="h-4 w-4" />
                Suppliers
              </Link>
              <span>/</span>
              <span>{humanize(selectedParty.partyType)}</span>
              <span>/</span>
              <span className="font-semibold text-white">{selectedParty.displayName}</span>
            </nav>
            <div className="flex items-start gap-4">
              <div className="flex h-18 w-18 shrink-0 items-center justify-center rounded-2xl border border-sky-700/50 bg-sky-500/15 text-sky-300">
                <Building2 className="h-9 w-9" />
              </div>
              <div className="min-w-0">
                <div className="mb-3 flex flex-wrap gap-2">
                  <Badge label={selectedParty.partyKey.toUpperCase()} tone="info" />
                  <Badge label={humanize(selectedParty.approvalStatus)} tone={statusTone(selectedParty.approvalStatus)} />
                  {documentWatchLabel ? <Badge label={documentWatchLabel} tone="warn" /> : null}
                </div>
                <h1 className="break-words text-3xl font-bold tracking-normal text-white">{selectedParty.displayName}</h1>
                <div className="mt-2 flex flex-wrap items-center gap-2 text-sm text-sky-100/75">
                  <MapPin className="h-4 w-4 text-slate-400" />
                  <span>{selectedParty.legalName || 'Legal name not recorded'}</span>
                  <span className="text-slate-600">-</span>
                  <span>{humanize(selectedParty.partyType)}</span>
                </div>
              </div>
            </div>
          </div>
          <div className="flex flex-wrap gap-2">
            <Link
              to="/purchasing/procurement"
              className="inline-flex items-center gap-2 rounded-xl bg-sky-500 px-4 py-3 text-sm font-semibold text-slate-950 hover:bg-sky-400"
            >
              <PackagePlus className="h-4 w-4" />
              Create PO
            </Link>
            <Link
              to="/purchasing/procurement"
              className="inline-flex items-center gap-2 rounded-xl border border-slate-800 bg-slate-900 px-4 py-3 text-sm font-semibold text-white hover:border-sky-700"
            >
              <Plus className="h-4 w-4" />
              Request quote
            </Link>
            <Link
              to={`/parties/drawer?partyId=${selectedParty.partyId}`}
              className="inline-flex items-center gap-2 rounded-xl border border-slate-800 bg-slate-900 px-4 py-3 text-sm font-semibold text-white hover:border-sky-700"
            >
              <Pencil className="h-4 w-4" />
              Edit supplier
            </Link>
            <button
              type="button"
              className="inline-flex h-12 w-12 items-center justify-center rounded-xl border border-slate-800 bg-slate-900 text-slate-200 hover:border-sky-700"
              aria-label="More supplier actions"
            >
              <MoreHorizontal className="h-5 w-5" />
            </button>
          </div>
        </div>
      </section>

      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        <MetricCard
          label="Approval state"
          value={humanize(selectedParty.approvalStatus)}
          hint={documents.some((document) => document.isAttention) ? 'Document watch active' : 'No document blockers'}
          icon={<ShieldCheck className="h-5 w-5" />}
          tone={statusTone(selectedParty.approvalStatus)}
        />
        <MetricCard
          label="Open orders"
          value={reportOpenOrders}
          hint={`${waitingShipmentCount} waiting shipment - ${purchaseRequests.length} request${purchaseRequests.length === 1 ? '' : 's'}`}
          icon={<Boxes className="h-5 w-5" />}
          tone="neutral"
        />
        <MetricCard
          label="YTD spend"
          value={formatCurrency(ytdSpend)}
          hint={reportPartLinks > 0 ? `${reportPartLinks} linked item${reportPartLinks === 1 ? '' : 's'}` : 'No linked catalog spend'}
          icon={<DollarSign className="h-5 w-5" />}
          tone="good"
        />
        <MetricCard
          label="Fill rate"
          value={fillRate == null ? 'Not tracked' : `${fillRate}%`}
          hint={orderedQuantity > 0 ? 'Based on received quantities' : 'No received quantity data'}
          icon={<CircleCheck className="h-5 w-5" />}
          tone="good"
        />
      </div>

      <div className="grid gap-6 xl:grid-cols-[minmax(0,1fr)_28rem]">
        <section className="overflow-hidden rounded-2xl border border-slate-800 bg-slate-950/70">
          <div className="flex overflow-x-auto border-b border-slate-800">
            {detailTabs.map((tab, index) => (
              <button
                key={tab}
                type="button"
                role="tab"
                aria-selected={index === 0}
                className={`shrink-0 px-5 py-4 text-sm font-semibold ${
                  index === 0 ? 'bg-slate-900 text-sky-300' : 'text-sky-200/75 hover:bg-slate-900/50'
                }`}
              >
                {tab}
              </button>
            ))}
          </div>

          <div className="space-y-6 p-6">
            <div className="flex flex-wrap items-start justify-between gap-3">
              <div>
                <h2 className="text-xl font-bold text-white">Supplier snapshot</h2>
                <p className="mt-1 text-sm text-sky-100/70">
                  Supplier identity, approval state, ownership, terms, documents, and cross-product usage.
                </p>
              </div>
              <button
                type="button"
                className="inline-flex items-center gap-2 rounded-xl border border-slate-800 bg-slate-900 px-4 py-2 text-sm font-semibold text-sky-100"
              >
                Field sources
                <ChevronDown className="h-4 w-4" />
              </button>
            </div>

            <dl className="grid gap-3 md:grid-cols-2 2xl:grid-cols-3">
              <SnapshotField label="Supplier ID" value={selectedParty.partyId} source="SupplyArr source of truth" />
              <SnapshotField label="Legal name" value={selectedParty.legalName || selectedParty.displayName} source="Supplier profile" />
              <SnapshotField label="Supplier category" value={humanize(selectedParty.partyType)} source="Selectable catalog" />
              <SnapshotField label="Primary site" value="Not recorded" source="StaffArr site reference" />
              <SnapshotField label="Supplier owner" value="Not assigned" source="StaffArr personId" />
              <SnapshotField label="Payment terms" value="Not recorded" source="Approved terms" />
              <SnapshotField label="Tax status" value={selectedParty.taxIdentifier ? 'Tax identifier on file' : 'Not recorded'} source="Document evidence" />
              <SnapshotField label="Insurance" value={documents.some((document) => document.title.toLowerCase().includes('insurance')) ? 'Document on file' : 'Not recorded'} source="Document watch" />
              <SnapshotField label="Risk tier" value={isBlocked ? 'High' : hasWatchItems ? 'Medium' : 'Low'} source="Supplier review" />
              <SnapshotField label="Phone" value={primaryContact?.phone || 'Not recorded'} source="Primary contact" />
              <SnapshotField label="Email" value={primaryContact?.email || 'Not recorded'} source="Primary contact" />
              <SnapshotField label="Website" value="Not recorded" source="Supplier profile" />
            </dl>

            <div className="grid gap-4 lg:grid-cols-2">
              <section className="rounded-2xl border border-slate-800 bg-slate-950/60 p-5">
                <div className="mb-4 flex items-center justify-between gap-3">
                  <h3 className="text-lg font-bold text-white">Supplied items</h3>
                  <Badge label="MaintainArr-linked" tone="info" />
                </div>
                <div className="space-y-3">
                  {partyPartLinks.length > 0 ? partyPartLinks.slice(0, 3).map(({ part, link }) => (
                    <div key={link.linkId} className="rounded-xl border border-slate-800 bg-slate-950/80 p-4">
                      <div className="flex items-start justify-between gap-3">
                        <div>
                          <h4 className="font-semibold text-white">{part.displayName}</h4>
                          <p className="mt-2 text-sm text-sky-100/80">
                            {humanize(part.categoryKey)} - {link.catalogLeadTimeDays ?? 'untracked'} days lead time
                          </p>
                          <p className="mt-1 text-xs text-slate-500">Vendor part {link.vendorPartNumber || 'not recorded'}</p>
                        </div>
                        <Badge label={link.isPreferred ? 'Preferred' : 'Approved'} tone={link.isPreferred ? 'good' : 'neutral'} />
                      </div>
                    </div>
                  )) : <EmptyPanel text="No linked items yet." />}
                </div>
              </section>

              <section className="rounded-2xl border border-slate-800 bg-slate-950/60 p-5">
                <div className="mb-4 flex items-center justify-between gap-3">
                  <h3 className="text-lg font-bold text-white">Recent purchase orders</h3>
                  <Link to="/purchasing/procurement" className="text-sm font-semibold text-sky-300 hover:text-sky-200">View all</Link>
                </div>
                <div className="space-y-3">
                  {recentPurchaseOrders.length > 0 ? recentPurchaseOrders.slice(0, 3).map((order) => (
                    <div key={order.id} className="rounded-xl border border-slate-800 bg-slate-950/80 p-4">
                      <div className="flex items-start justify-between gap-3">
                        <div>
                          <h4 className="font-semibold text-white">{order.key}</h4>
                          <p className="mt-2 text-sm text-sky-100/80">{order.title}</p>
                          <p className="mt-1 text-xs text-slate-500">{order.detail}</p>
                        </div>
                        <Badge label={humanize(order.status)} tone={statusTone(order.status)} />
                      </div>
                    </div>
                  )) : <EmptyPanel text="No purchase orders yet." />}
                </div>
              </section>
            </div>
          </div>
        </section>

        <aside className="space-y-6">
          <section className="rounded-2xl border border-slate-800 bg-slate-950/70 p-5">
            <div className="mb-4 flex items-center justify-between gap-3">
              <h2 className="text-lg font-bold text-white">Supplier decision</h2>
              <Badge label={decisionLabel} tone={decisionTone} />
            </div>
            <div className={`rounded-2xl border p-5 ${
              decisionTone === 'bad'
                ? 'border-red-500/30 bg-red-950/20'
                : decisionTone === 'warn'
                  ? 'border-amber-500/30 bg-amber-950/20'
                  : 'border-emerald-500/30 bg-emerald-950/20'
            }`}>
              <div className="flex gap-3">
                {decisionTone === 'good' ? (
                  <CheckCircle2 className="mt-1 h-5 w-5 shrink-0 text-emerald-300" />
                ) : (
                  <AlertTriangle className={`mt-1 h-5 w-5 shrink-0 ${decisionTone === 'bad' ? 'text-red-300' : 'text-amber-300'}`} />
                )}
                <div>
                  <h3 className="font-bold text-white">{decisionTitle}</h3>
                  <p className="mt-2 text-sm leading-6 text-sky-100/80">{decisionDetail}</p>
                </div>
              </div>
            </div>
            <div className="mt-4 grid grid-cols-2 gap-3">
              <div className="rounded-xl border border-slate-800 bg-slate-900 p-4">
                <CheckCircle2 className="h-5 w-5 text-emerald-300" />
                <p className="mt-3 text-xs text-slate-400">Allowed checks</p>
                <p className="text-xl font-bold text-white">{allowedChecks}</p>
              </div>
              <div className="rounded-xl border border-slate-800 bg-slate-900 p-4">
                <XCircle className="h-5 w-5 text-red-300" />
                <p className="mt-3 text-xs text-slate-400">Blocked checks</p>
                <p className="text-xl font-bold text-white">{blockedChecks}</p>
              </div>
            </div>
          </section>

          <RailSection title="Primary contacts" icon={<Users className="h-5 w-5" />}>
            <div className="space-y-3">
              {selectedParty.contacts.length > 0 ? selectedParty.contacts.slice(0, 3).map((contact) => (
                <div key={contact.contactId} className="rounded-xl border border-slate-800 bg-slate-900 p-4">
                  <div className="flex items-start justify-between gap-3">
                    <div>
                      <h3 className="font-semibold text-white">{contact.contactName}</h3>
                      <p className="mt-2 text-sm text-sky-100/75">{contact.roleLabel || 'Contact'}</p>
                    </div>
                    {contact.isPrimary ? <Badge label="Primary" tone="info" /> : null}
                  </div>
                  {contact.email ? (
                    <p className="mt-3 flex items-center gap-2 text-xs text-slate-400">
                      <Mail className="h-4 w-4" />
                      {contact.email}
                    </p>
                  ) : null}
                  {contact.phone ? (
                    <p className="mt-2 flex items-center gap-2 text-xs text-slate-400">
                      <Phone className="h-4 w-4" />
                      {contact.phone}
                    </p>
                  ) : null}
                </div>
              )) : <EmptyPanel text="No contacts on file." />}
            </div>
          </RailSection>

          <RailSection title="Performance" icon={<Star className="h-5 w-5" />}>
            <div className="rounded-xl border border-slate-800 bg-slate-900 p-4">
              <p className="text-sm font-semibold text-white">Fill rate</p>
              <div className="mt-3 h-2 rounded-full bg-slate-800">
                <div
                  className="h-2 rounded-full bg-sky-400"
                  style={{ width: `${Math.min(Math.max(fillRate ?? 0, 0), 100)}%` }}
                />
              </div>
              <p className="mt-2 text-xs text-slate-400">
                {fillRate == null ? 'No received quantity data' : `${fillRate}% based on current purchase orders`}
              </p>
            </div>
            <div className="mt-3 grid grid-cols-2 gap-3">
              <div className="rounded-xl border border-slate-800 bg-slate-900 p-4">
                <Truck className="h-5 w-5 text-sky-300" />
                <p className="mt-3 text-xs text-slate-400">Avg lead time</p>
                <p className="font-bold text-white">{averageLeadTime == null ? 'Not tracked' : `${averageLeadTime.toFixed(1)} days`}</p>
              </div>
              <div className="rounded-xl border border-slate-800 bg-slate-900 p-4">
                <DollarSign className="h-5 w-5 text-sky-300" />
                <p className="mt-3 text-xs text-slate-400">Preferred items</p>
                <p className="font-bold text-white">{reportPreferredLinks}</p>
              </div>
            </div>
          </RailSection>

          <RailSection title="Documents" icon={<FileText className="h-5 w-5" />}>
            <div className="overflow-hidden rounded-xl border border-slate-800 bg-slate-950/60">
              {documents.length > 0 ? documents.slice(0, 4).map((document, index) => (
                <div
                  key={document.id}
                  className={`flex items-center justify-between gap-3 p-4 ${index > 0 ? 'border-t border-slate-800' : ''}`}
                >
                  <div>
                    <h3 className="font-semibold text-white">{document.title}</h3>
                    <p className="mt-1 text-xs text-slate-400">{document.subtitle}</p>
                  </div>
                  <Badge label={document.status} tone={document.tone} />
                </div>
              )) : <div className="p-4"><EmptyPanel text="No compliance documents on file." /></div>}
            </div>
          </RailSection>

          <RailSection title="Upcoming requirements" icon={<CalendarClock className="h-5 w-5" />}>
            <div className="space-y-3">
              {upcomingRequirements.length > 0 ? upcomingRequirements.map((requirement) => (
                <div key={`${requirement.title}-${requirement.badge}`} className="rounded-xl border border-slate-800 bg-slate-900 p-4">
                  <div className="flex items-start justify-between gap-3">
                    <div>
                      <h3 className="font-semibold text-white">{requirement.title}</h3>
                      <p className="mt-2 text-xs text-slate-400">{requirement.detail}</p>
                    </div>
                    <Badge label={requirement.badge} tone={requirement.tone} />
                  </div>
                </div>
              )) : <EmptyPanel text="No upcoming supplier requirements." />}
            </div>
          </RailSection>

          <RailSection title="Recent activity" icon={<History className="h-5 w-5" />}>
            <div className="space-y-4">
              {recentActivity.length > 0 ? recentActivity.slice(0, 5).map((activity) => (
                <div key={activity.id} className="relative pl-7">
                  <span className="absolute left-0 top-1.5 h-3 w-3 rounded-full bg-sky-400 shadow-lg shadow-sky-500/40" />
                  <p className="text-xs font-bold uppercase tracking-normal text-sky-300">{activity.category}</p>
                  <h3 className="mt-1 font-semibold text-white">{activity.title}</h3>
                  <p className="mt-1 text-xs text-slate-400">{activity.detail}</p>
                </div>
              )) : <EmptyPanel text="No recent supplier activity." />}
            </div>
          </RailSection>
        </aside>
      </div>
    </div>
  )
}

export function PartiesSection({ state: s }: Props) {
  const location = useLocation()
  const mode: PartiesViewMode = location.pathname.startsWith('/parties/create')
    ? 'create'
    : location.pathname.startsWith('/parties/details')
      ? 'details'
      : 'drawer'
  const parties = [
    ...(s.suppliersQuery?.data ?? []),
    ...(s.vendors ?? []),
    ...(s.dealersQuery?.data ?? []),
  ]

  if (mode === 'details') {
    return <PartiesProfile state={s} parties={parties} />
  }

  return <PartiesRegistryWorkspace state={s} mode={mode} />
}
