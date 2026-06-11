import {
  DetailBadge as Badge,
  DetailEmptyState as EmptyPanel,
  ProfileDetailsLayout,
  type DetailTone,
} from '@stl/shared-ui'
import { useQuery } from '@tanstack/react-query'
import {
  AlertTriangle,
  Boxes,
  Building2,
  CalendarClock,
  CheckCircle2,
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
} from 'lucide-react'
import { Link, useLocation } from 'react-router-dom'
import { useMemo } from 'react'
import {
  getPartyVendorRestrictionEnforcement,
  getSupplierOnboardingByParty,
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
type Tone = DetailTone

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

function statusTone(value: string | null | undefined): Tone {
  const normalized = value?.toLowerCase() ?? ''
  if (['approved', 'active', 'current', 'satisfied', 'ready'].includes(normalized)) return 'good'
  if (['pending', 'review', 'submitted', 'expiring_soon'].includes(normalized)) return 'warn'
  if (['restricted', 'inactive', 'blocked', 'rejected', 'expired', 'not_ready'].includes(normalized)) return 'bad'
  return 'neutral'
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
  const documentsQuery = useQuery({
    queryKey: ['supplyarr-party-profile-documents', s.accessToken, selectedPartyId],
    queryFn: () => listPartyComplianceDocuments(s.accessToken, selectedPartyId),
    enabled: detailQueriesEnabled && s.canReadParties,
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
  const contractRecords = (s.contractsQuery?.data ?? []).filter(
    (contract) => contract.vendorPartyId === selectedParty.partyId,
  )
  const activeContract =
    contractRecords.find((contract) => contract.status === 'active') ??
    contractRecords.find((contract) => contract.status === 'expiring_soon') ??
    contractRecords[0] ??
    null
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
  const linkedPartCount = partyPartLinks.length
  const preferredPartLinkCount = partyPartLinks.filter(({ link }) => link.isPreferred).length
  const rawDocuments = documentsQuery.data ?? []
  const documents = rawDocuments.map((document) => ({
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
    linkedPartCount > 0,
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
  const recentPurchaseOrders = [...purchaseOrders]
    .sort((left, right) => Date.parse(right.createdAt) - Date.parse(left.createdAt))
    .slice(0, 3)
    .map((order) => ({
      id: order.purchaseOrderId,
      key: order.orderKey,
      title: order.title,
      status: order.status,
      detail: order.lines.length > 0
        ? `${order.lines.length} line${order.lines.length === 1 ? '' : 's'} - ${order.lines.reduce((total, line) => total + line.quantityReceived, 0)}/${order.lines.reduce((total, line) => total + line.quantityOrdered, 0)} received`
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
    <ProfileDetailsLayout
      testId="supplyarr-party-profile"
      backLabel="Suppliers"
      backTo="/parties/drawer"
      breadcrumbs={[humanize(selectedParty.partyType), selectedParty.displayName]}
      icon={<Building2 className="h-9 w-9" />}
      title={selectedParty.displayName}
      subtitle={(
        <span className="flex flex-wrap items-center gap-2">
          <MapPin className="h-4 w-4 text-slate-400" />
          <span>{selectedParty.legalName || 'Legal name not recorded'}</span>
          <span className="text-slate-600">-</span>
          <span>{humanize(selectedParty.partyType)}</span>
        </span>
      )}
      badges={[
        { label: selectedParty.partyKey.toUpperCase(), tone: 'info' },
        { label: humanize(selectedParty.approvalStatus), tone: statusTone(selectedParty.approvalStatus) },
        ...(documentWatchLabel ? [{ label: documentWatchLabel, tone: 'warn' as Tone }] : []),
      ]}
      actions={(
        <>
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
        </>
      )}
      metrics={[
        {
          label: 'Approval state',
          value: humanize(selectedParty.approvalStatus),
          hint: documents.some((document) => document.isAttention) ? 'Document watch active' : 'No document blockers',
          icon: <ShieldCheck className="h-5 w-5" />,
          tone: statusTone(selectedParty.approvalStatus),
        },
        {
          label: 'Open orders',
          value: openOrders.length,
          hint: `${waitingShipmentCount} waiting shipment - ${purchaseRequests.length} request${purchaseRequests.length === 1 ? '' : 's'}`,
          icon: <Boxes className="h-5 w-5" />,
          tone: 'neutral',
        },
        {
          label: 'YTD spend',
          value: formatCurrency(ytdSpend),
          hint: linkedPartCount > 0 ? `${linkedPartCount} linked item${linkedPartCount === 1 ? '' : 's'}` : 'No linked catalog spend',
          icon: <DollarSign className="h-5 w-5" />,
          tone: 'good',
        },
        {
          label: 'Fill rate',
          value: fillRate == null ? 'Not tracked' : `${fillRate}%`,
          hint: orderedQuantity > 0 ? 'Based on received quantities' : 'No received quantity data',
          icon: <CircleCheck className="h-5 w-5" />,
          tone: 'good',
        },
      ]}
      tabs={detailTabs}
      snapshotTitle="Supplier snapshot"
      snapshotSubtitle="Supplier identity, approval state, ownership, terms, documents, and cross-product usage."
      snapshotFields={[
        { label: 'Supplier ID', value: selectedParty.partyId, source: 'SupplyArr source of truth' },
        { label: 'Legal name', value: selectedParty.legalName || selectedParty.displayName, source: 'Supplier profile' },
        { label: 'Supplier category', value: humanize(selectedParty.partyType), source: 'Selectable catalog' },
        { label: 'Primary site', value: 'Not recorded', source: 'StaffArr site reference' },
        { label: 'Supplier owner', value: 'Not assigned', source: 'StaffArr personId' },
        { label: 'Payment terms', value: activeContract?.paymentTerms || 'Not recorded', source: 'Approved terms' },
        { label: 'Freight terms', value: activeContract?.freightTerms || 'Not recorded', source: 'Approved terms' },
        { label: 'Warranty', value: activeContract?.warrantyTerms || 'Not recorded', source: 'Approved terms' },
        { label: 'Minimum spend', value: activeContract?.minimumSpend == null ? 'Not recorded' : formatCurrency(activeContract.minimumSpend), source: 'Approved terms' },
        { label: 'Contract records', value: contractRecords.length > 0 ? contractRecords.length : 'None', source: 'Contract registry' },
        { label: 'Tax status', value: selectedParty.taxIdentifier ? 'Tax identifier on file' : 'Not recorded', source: 'Document evidence' },
        { label: 'Insurance', value: documents.some((document) => document.title.toLowerCase().includes('insurance')) ? 'Document on file' : 'Not recorded', source: 'Document watch' },
        { label: 'Risk tier', value: isBlocked ? 'High' : hasWatchItems ? 'Medium' : 'Low', source: 'Supplier review' },
        { label: 'Phone', value: primaryContact?.phone || 'Not recorded', source: 'Primary contact' },
        { label: 'Email', value: primaryContact?.email || 'Not recorded', source: 'Primary contact' },
        { label: 'Website', value: 'Not recorded', source: 'Supplier profile' },
      ]}
      mainContent={(
        <div className="grid gap-4 lg:grid-cols-3">
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

          <section className="rounded-2xl border border-slate-800 bg-slate-950/60 p-5">
            <div className="mb-4 flex items-start justify-between gap-3">
              <div>
                <h3 className="text-lg font-bold text-white">Contracts & purchasing terms</h3>
                <p className="mt-1 text-xs text-slate-400">Current vendor agreements and commercial terms.</p>
              </div>
              <Badge label={`${contractRecords.length} record${contractRecords.length === 1 ? '' : 's'}`} tone={contractRecords.length > 0 ? 'info' : 'neutral'} />
            </div>
            <div className="space-y-3">
              {contractRecords.length > 0 ? contractRecords.slice(0, 3).map((contract) => (
                <div key={contract.contractId} className="rounded-xl border border-slate-800 bg-slate-950/80 p-4">
                  <div className="flex items-start justify-between gap-3">
                    <div>
                      <h4 className="font-semibold text-white">{contract.contractKey}</h4>
                      <p className="mt-2 text-sm text-sky-100/80">{contract.title}</p>
                      <p className="mt-1 text-xs text-slate-500">
                        {humanize(contract.contractType)} · {formatDate(contract.effectiveAt)} to {contract.expiresAt ? formatDate(contract.expiresAt) : 'open-ended'}
                      </p>
                    </div>
                    <div className="flex flex-col items-end gap-2">
                      <Badge label={humanize(contract.status)} tone={statusTone(contract.status)} />
                      <Badge label={humanize(contract.approvalStatus)} tone={statusTone(contract.approvalStatus)} />
                    </div>
                  </div>
                  <div className="mt-4 grid gap-3 sm:grid-cols-2">
                    <div>
                      <p className="text-xs uppercase tracking-wide text-slate-500">Payment terms</p>
                      <p className="mt-1 text-sm text-white">{contract.paymentTerms || 'Not recorded'}</p>
                    </div>
                    <div>
                      <p className="text-xs uppercase tracking-wide text-slate-500">Freight terms</p>
                      <p className="mt-1 text-sm text-white">{contract.freightTerms || 'Not recorded'}</p>
                    </div>
                    <div>
                      <p className="text-xs uppercase tracking-wide text-slate-500">Warranty</p>
                      <p className="mt-1 text-sm text-white">{contract.warrantyTerms || 'Not recorded'}</p>
                    </div>
                    <div>
                      <p className="text-xs uppercase tracking-wide text-slate-500">Minimum spend</p>
                      <p className="mt-1 text-sm text-white">{contract.minimumSpend == null ? 'Not recorded' : formatCurrency(contract.minimumSpend)}</p>
                    </div>
                  </div>
                  {contract.serviceLevelAgreement ? (
                    <p className="mt-4 rounded-xl border border-slate-800 bg-slate-900 px-3 py-2 text-xs text-slate-300">
                      SLA: {contract.serviceLevelAgreement}
                    </p>
                  ) : null}
                </div>
              )) : <EmptyPanel text="No contract records on file." />}
            </div>
          </section>
        </div>
      )}
      decisionTitle="Supplier decision"
      decisionBadge={{ label: decisionLabel, tone: decisionTone }}
      decisionIcon={decisionTone === 'good' ? (
        <CheckCircle2 className="h-5 w-5 text-emerald-300" />
      ) : (
        <AlertTriangle className={`h-5 w-5 ${decisionTone === 'bad' ? 'text-red-300' : 'text-amber-300'}`} />
      )}
      decisionSummary={decisionTitle}
      decisionDetail={decisionDetail}
      allowedChecks={allowedChecks}
      blockedChecks={blockedChecks}
      railSections={[
        {
          title: 'Primary contacts',
          icon: <Users className="h-5 w-5" />,
          content: (
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
          ),
        },
        {
          title: 'Performance',
          icon: <Star className="h-5 w-5" />,
          content: (
            <>
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
                  <p className="font-bold text-white">{preferredPartLinkCount}</p>
                </div>
              </div>
            </>
          ),
        },
        {
          title: 'Documents',
          icon: <FileText className="h-5 w-5" />,
          content: (
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
          ),
        },
        {
          title: 'Upcoming requirements',
          icon: <CalendarClock className="h-5 w-5" />,
          content: (
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
          ),
        },
        {
          title: 'Recent activity',
          icon: <History className="h-5 w-5" />,
          content: (
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
          ),
        },
      ]}
    />
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
