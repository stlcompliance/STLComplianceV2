import { useMutation, useQuery } from '@tanstack/react-query'
import {
  AlertTriangle,
  ArrowRightLeft,
  CheckCircle2,
  ExternalLink,
  FileText,
  PackageCheck,
  Route,
} from 'lucide-react'
import { useEffect, useState } from 'react'
import { Navigate, useParams } from 'react-router-dom'
import { DetailBadge, DetailEmptyState, ProfileDetailsLayout, type DetailRailSectionConfig } from '@stl/shared-ui'
import type { RegisterSupplierOrderDocumentRequest, UpdateSupplierOrderStatusRequest } from '../../api/types'
import {
  createSupplierOrderBrokerDecision,
  createSupplierOrderMagicLink,
  getSupplierOrder,
  getSupplierOrderMetadata,
  getSupplierOrders,
  getSupplierOrderHistory,
  registerSupplierOrderDocument,
  sendSupplierOrderToSupplier,
  splitSupplierOrder,
  submitSupplierOrderStatus,
} from '../../api/supplierOrderClient'
import { listRoutArrTripsBySupplierOrder } from '../../api/routarrReferenceClient'
import { useSupplyArrPageAccess } from './useSupplyArrPageAccess'
import {
  formatSupplierIdentityLabel,
  formatSupplierServiceTypes,
  humanizeSupplierUnitKind,
} from '../../utils/supplierPresentation'
import {
  formatSupplierOrderDateTime,
  humanizeSupplierOrderValue,
  quantitySummary,
  supplierOrderStatusTone,
} from './supplierOrderUi'

export function SupplierOrderDetailPage() {
  const { supplierOrderId } = useParams<{ supplierOrderId: string }>()
  const { session, meQuery, canReadSupplierOrders, canUpdateSupplierOrders } = useSupplyArrPageAccess()
  const [latestMagicLink, setLatestMagicLink] = useState<{
    url: string
    expiresAt?: string | null
    description?: string
  } | null>(null)
  const [statusDraft, setStatusDraft] = useState<UpdateSupplierOrderStatusRequest>({
    newStatus: 'in_progress',
    quantityReady: null,
    estimatedReadyAt: null,
    confirmedReadyAt: null,
    pickupWindowStart: null,
    pickupWindowEnd: null,
    note: null,
    exceptionReason: null,
    readyForPickupConfirmed: false,
  })
  const [documentDraft, setDocumentDraft] = useState<RegisterSupplierOrderDocumentRequest>({
    documentType: 'photo',
    fileName: '',
    contentType: '',
    storageProvider: 'shared_blob',
    storageKey: '',
    sizeBytes: null,
  })
  const [decisionType, setDecisionType] = useState('wait_full')
  const [decisionQuantity, setDecisionQuantity] = useState('')
  const [decisionTripId, setDecisionTripId] = useState('')
  const [decisionNote, setDecisionNote] = useState('')
  const [splitReason, setSplitReason] = useState('Split remaining quantity')
  const [splitTripId, setSplitTripId] = useState('')
  const [splitExpectedReadyAt, setSplitExpectedReadyAt] = useState('')
  const [splitPickupWindowStart, setSplitPickupWindowStart] = useState('')
  const [splitPickupWindowEnd, setSplitPickupWindowEnd] = useState('')

  if (!session) {
    return <p className="text-sm text-[var(--color-text-muted)]">Loading supplier order…</p>
  }

  if (!supplierOrderId) {
    return <Navigate to="/purchasing/supplier-orders" replace />
  }

  if (meQuery.isLoading) {
    return <p className="text-sm text-[var(--color-text-muted)]">Loading supplier-order access…</p>
  }

  if (!canReadSupplierOrders) {
    return (
      <section className="rounded-3xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-8">
        <h1 className="text-2xl font-bold text-[var(--color-text-primary)]">Supplier-order detail</h1>
        <p className="mt-3 text-sm text-[var(--color-text-secondary)]">
          You do not have permission to view supplier orders.
        </p>
      </section>
    )
  }

  const supplierOrderQuery = useQuery({
    queryKey: ['supplyarr-supplier-order', session.accessToken, supplierOrderId],
    queryFn: () => getSupplierOrder(session.accessToken, supplierOrderId),
  })

  const metadataQuery = useQuery({
    queryKey: ['supplyarr-supplier-order-metadata', session.accessToken],
    queryFn: () => getSupplierOrderMetadata(session.accessToken),
  })

  const historyQuery = useQuery({
    queryKey: ['supplyarr-supplier-order-history', session.accessToken, supplierOrderId],
    queryFn: () => getSupplierOrderHistory(session.accessToken, supplierOrderId),
    enabled: supplierOrderQuery.isSuccess,
  })

  const relatedTripsQuery = useQuery({
    queryKey: ['supplyarr-related-routarr-trips', session.accessToken, supplierOrderId],
    queryFn: () => listRoutArrTripsBySupplierOrder(session.accessToken, supplierOrderId),
    enabled: supplierOrderQuery.isSuccess,
  })

  const lineageQuery = useQuery({
    queryKey: ['supplyarr-supplier-order-lineage', session.accessToken, supplierOrderQuery.data?.supplierId],
    queryFn: () =>
      getSupplierOrders(session.accessToken, {
        supplierId: supplierOrderQuery.data?.supplierId,
      }),
    enabled: Boolean(supplierOrderQuery.data?.supplierId),
  })

  const refreshAll = async () => {
    await supplierOrderQuery.refetch()
    await historyQuery.refetch()
    await relatedTripsQuery.refetch()
    await lineageQuery.refetch()
  }

  const sendMutation = useMutation({
    mutationFn: () => sendSupplierOrderToSupplier(session.accessToken, supplierOrderId),
    onSuccess: async (result) => {
      setLatestMagicLink({
        url: result.magicLinkUrl,
        expiresAt: result.expiresAt,
        description: 'Supplier magic link issued from send flow.',
      })
      await refreshAll()
    },
  })

  const magicLinkMutation = useMutation({
    mutationFn: () => createSupplierOrderMagicLink(session.accessToken, supplierOrderId),
    onSuccess: async (result) => {
      setLatestMagicLink({
        url: result.url,
        expiresAt: result.expiresAt,
        description: 'Supplier magic link regenerated.',
      })
      await refreshAll()
    },
  })

  const statusMutation = useMutation({
    mutationFn: () => submitSupplierOrderStatus(session.accessToken, supplierOrderId, statusDraft),
    onSuccess: async () => {
      await refreshAll()
    },
  })

  const documentMutation = useMutation({
    mutationFn: () => registerSupplierOrderDocument(session.accessToken, supplierOrderId, documentDraft),
    onSuccess: async () => {
      setDocumentDraft({
        documentType: 'photo',
        fileName: '',
        contentType: '',
        storageProvider: 'shared_blob',
        storageKey: '',
        sizeBytes: null,
      })
      await refreshAll()
    },
  })

  const decisionMutation = useMutation({
    mutationFn: () =>
      createSupplierOrderBrokerDecision(session.accessToken, supplierOrderId, {
        decisionType,
        authorizedQuantity: decisionQuantity ? Number(decisionQuantity) : null,
        selectedTripId: decisionTripId || null,
        note: decisionNote || null,
      }),
    onSuccess: async () => {
      setDecisionNote('')
      setDecisionQuantity('')
      await refreshAll()
    },
  })

  const splitMutation = useMutation({
    mutationFn: () =>
      splitSupplierOrder(session.accessToken, supplierOrderId, {
        selectedTripId: splitTripId || null,
        splitReason: splitReason || null,
        remainingExpectedReadyAt: splitExpectedReadyAt || null,
        remainingPickupWindowStart: splitPickupWindowStart || null,
        remainingPickupWindowEnd: splitPickupWindowEnd || null,
      }),
    onSuccess: async (result) => {
      setLatestMagicLink({
        url: result.remainingSupplierOrderUrl,
        expiresAt: null,
        description: 'New remaining-child supplier link issued after split.',
      })
      await refreshAll()
    },
  })

  useEffect(() => {
    if (!supplierOrderQuery.data) {
      return
    }

    setStatusDraft((current) => ({
      ...current,
      quantityReady: supplierOrderQuery.data.quantityReady,
      estimatedReadyAt: supplierOrderQuery.data.expectedReadyAt,
      pickupWindowStart: supplierOrderQuery.data.pickupWindowStart,
      pickupWindowEnd: supplierOrderQuery.data.pickupWindowEnd,
    }))
  }, [supplierOrderQuery.data])

  if (supplierOrderQuery.isLoading) {
    return <p className="text-sm text-[var(--color-text-muted)]">Loading supplier order…</p>
  }

  if (supplierOrderQuery.isError || !supplierOrderQuery.data) {
    return (
      <section className="rounded-3xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-8">
        <h1 className="text-2xl font-bold text-[var(--color-text-primary)]">Supplier-order detail unavailable</h1>
        <p className="mt-3 text-sm text-[var(--tone-danger-text)]">
          Unable to load this supplier order right now.
        </p>
      </section>
    )
  }

  const order = supplierOrderQuery.data
  const metadata = metadataQuery.data
  const supplierIdentityLabel = formatSupplierIdentityLabel({
    supplierDisplayName: order.supplierNameSnapshot,
    parentSupplierDisplayName: order.parentSupplierDisplayName,
    supplierUnitKind: order.supplierUnitKind,
  })
  const supplierParentLabel = order.parentSupplierDisplayName ?? 'This supplier record is the parent identity.'
  const supplierUnitKindLabel = humanizeSupplierUnitKind(order.supplierUnitKind)
  const supplierServiceSummary = formatSupplierServiceTypes(order.supplierServiceTypes)
  const splitChildren =
    (lineageQuery.data ?? []).filter((item) => item.parentSupplierOrderId === order.supplierOrderId) ?? []
  const activeSupplierBlockTrips =
    (relatedTripsQuery.data ?? []).filter((trip) =>
        (trip.dispatchBlocks ?? []).some(
        (block) => block.blockType === 'supplier_readiness' && block.status === 'active',
      ),
    ) ?? []
  const readyTripCount = (relatedTripsQuery.data ?? []).length - activeSupplierBlockTrips.length
  const decisionSummary =
    order.status === 'completed_ready_for_dispatch'
      ? 'Supplier released this order for dispatch.'
      : order.status === 'partially_ready'
        ? 'Broker decision required for partial readiness.'
        : order.status === 'unable_to_fulfill'
          ? 'Supplier reported that this order cannot be fulfilled.'
          : 'Waiting on supplier readiness confirmation.'

  const decisionDetail =
    order.status === 'completed_ready_for_dispatch'
      ? 'The supplier-ready confirmation is in. Dispatch can proceed once the trip is unblocked and manually released.'
      : order.status === 'partially_ready'
        ? 'The supplier reported a partial quantity. Decide whether to wait, authorize a partial dispatch, or split the remaining quantity.'
        : order.status === 'unable_to_fulfill'
          ? 'Keep the trip blocked and follow the supplier exception path until the broker resolves the order.'
          : 'The supplier workflow will publish readiness once the supplier confirms pickup readiness.'

  const railSections: DetailRailSectionConfig[] = [
    {
      title: 'Related trips',
      icon: <Route className="h-5 w-5" />,
      content:
        relatedTripsQuery.data && relatedTripsQuery.data.length > 0 ? (
          <div className="space-y-3">
            {relatedTripsQuery.data.map((trip) => {
              const activeBlock = (trip.dispatchBlocks ?? []).find(
                (block) => block.blockType === 'supplier_readiness' && block.status === 'active',
              )
              return (
                <div key={trip.tripId} className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] p-4">
                  <div className="flex items-start justify-between gap-3">
                    <div>
                      <p className="font-medium text-[var(--color-text-primary)]">{trip.title}</p>
                      <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                        {trip.tripNumber} · {humanizeSupplierOrderValue(trip.dispatchStatus)}
                      </p>
                    </div>
                    <DetailBadge
                      label={activeBlock ? 'Dispatch blocked' : 'Dispatch ready'}
                      tone={activeBlock ? 'bad' : 'good'}
                    />
                  </div>
                  <p className="mt-3 text-xs text-[var(--color-text-secondary)]">
                    Supplier status snapshot: {humanizeSupplierOrderValue(trip.supplierReadinessStatusSnapshot)}
                  </p>
                  <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                    {quantitySummary(
                      trip.supplierOrderedQuantitySnapshot ?? 0,
                      trip.supplierQuantityReadySnapshot ?? 0,
                      Math.max(
                        0,
                        (trip.supplierOrderedQuantitySnapshot ?? 0) - (trip.supplierQuantityReadySnapshot ?? 0),
                      ),
                      order.quantityUom,
                    )}
                  </p>
                  <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                    Ready confirmation: {formatSupplierOrderDateTime(trip.supplierConfirmedReadyAtSnapshot)}
                  </p>
                  {activeBlock ? (
                    <p className="mt-2 text-xs text-[var(--color-warning-text)]">
                      Block reason: {humanizeSupplierOrderValue(activeBlock.blockReason)}
                    </p>
                  ) : null}
                  {trip.dispatchOverrideReason ? (
                    <p className="mt-2 text-xs text-[var(--tone-danger-text)]">
                      Override reason: {trip.dispatchOverrideReason}
                    </p>
                  ) : null}
                </div>
              )
            })}
          </div>
        ) : relatedTripsQuery.isLoading ? (
          <p className="text-sm text-[var(--color-text-muted)]">Loading related trips…</p>
        ) : (
          <DetailEmptyState text="No trips reference this supplier order yet." />
        ),
    },
    {
      title: 'Documents',
      icon: <FileText className="h-5 w-5" />,
      content: (
        <div className="space-y-4">
          {order.documents.length > 0 ? (
            <ul className="space-y-3">
              {order.documents.map((document) => (
                <li key={document.documentId} className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] p-4">
                  <p className="font-medium text-[var(--color-text-primary)]">{document.fileName}</p>
                  <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                    {humanizeSupplierOrderValue(document.documentType)} · {document.contentType}
                  </p>
                  <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                    Record #{document.recordArrRecordNumberSnapshot} · uploaded {formatSupplierOrderDateTime(document.uploadedAt)}
                  </p>
                </li>
              ))}
            </ul>
          ) : (
            <DetailEmptyState text="No supplier-order documents have been registered yet." />
          )}

          {canUpdateSupplierOrders ? (
            <div className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-4">
              <h3 className="font-medium text-[var(--color-text-primary)]">Register supplier-order document</h3>
              <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                This records the document metadata. Storage details are available in support information.
              </p>
              <div className="mt-3 grid gap-3">
                <label className="text-sm text-[var(--color-text-secondary)]">
                  File
                  <input
                    type="file"
                    className="mt-1 block w-full text-sm text-[var(--color-text-secondary)]"
                    onChange={(event) => {
                      const file = event.target.files?.[0]
                      if (!file) {
                        return
                      }

                      setDocumentDraft((current) => ({
                        ...current,
                        fileName: file.name,
                        contentType: file.type || 'application/octet-stream',
                        sizeBytes: file.size,
                        storageKey: current.storageKey || `supplier-order/${file.name}`,
                      }))
                    }}
                  />
                </label>
                <div className="grid gap-3 md:grid-cols-2">
                  <label className="text-sm text-[var(--color-text-secondary)]">
                    Document type
                    <select
                      className="mt-1 block w-full rounded-xl border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-[var(--color-text-primary)]"
                      value={documentDraft.documentType}
                      onChange={(event) => setDocumentDraft({ ...documentDraft, documentType: event.target.value })}
                    >
                      {(metadata?.documentTypeOptions ?? []).map((documentType) => (
                        <option key={documentType.value} value={documentType.value}>
                          {documentType.label}
                        </option>
                      ))}
                    </select>
                  </label>
                  <label className="text-sm text-[var(--color-text-secondary)]">
                    Content type
                    <input
                      className="mt-1 block w-full rounded-xl border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-[var(--color-text-primary)]"
                      value={documentDraft.contentType}
                      onChange={(event) => setDocumentDraft({ ...documentDraft, contentType: event.target.value })}
                    />
                  </label>
                </div>
                <label className="text-sm text-[var(--color-text-secondary)]">
                  File name
                  <input
                    className="mt-1 block w-full rounded-xl border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-[var(--color-text-primary)]"
                    value={documentDraft.fileName}
                    onChange={(event) => setDocumentDraft({ ...documentDraft, fileName: event.target.value })}
                  />
                </label>
                <div className="grid gap-3 md:grid-cols-2">
                  <label className="text-sm text-[var(--color-text-secondary)]">
                    Storage provider
                    <input
                      className="mt-1 block w-full rounded-xl border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-[var(--color-text-primary)]"
                      value={documentDraft.storageProvider ?? ''}
                      onChange={(event) => setDocumentDraft({ ...documentDraft, storageProvider: event.target.value })}
                    />
                  </label>
                  <label className="text-sm text-[var(--color-text-secondary)]">
                    Storage key
                    <input
                      className="mt-1 block w-full rounded-xl border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-[var(--color-text-primary)]"
                      value={documentDraft.storageKey ?? ''}
                      onChange={(event) => setDocumentDraft({ ...documentDraft, storageKey: event.target.value })}
                    />
                  </label>
                </div>
                <button
                  type="button"
                  className="rounded-xl bg-[var(--color-accent)] px-4 py-2 text-sm font-semibold text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
                  disabled={
                    documentMutation.isPending ||
                    !documentDraft.fileName.trim() ||
                    !documentDraft.contentType.trim()
                  }
                  onClick={() => documentMutation.mutate()}
                >
                  {documentMutation.isPending ? 'Registering…' : 'Register document'}
                </button>
                {documentMutation.error instanceof Error ? (
                <p className="text-xs text-[var(--tone-danger-text)]">{documentMutation.error.message}</p>
                ) : null}
              </div>
            </div>
          ) : null}
        </div>
      ),
    },
    {
      title: 'Status history',
      icon: <ArrowRightLeft className="h-5 w-5" />,
      content:
        historyQuery.data && historyQuery.data.length > 0 ? (
          <ol className="space-y-3">
            {historyQuery.data.map((entry) => (
              <li key={entry.statusUpdateId} className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] p-4">
                <div className="flex items-start justify-between gap-3">
                  <div>
                    <p className="font-medium text-[var(--color-text-primary)]">
                      {humanizeSupplierOrderValue(entry.previousStatus ?? 'new')} to {humanizeSupplierOrderValue(entry.newStatus)}
                    </p>
                    <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                      {humanizeSupplierOrderValue(entry.source)} · {formatSupplierOrderDateTime(entry.createdAt)}
                    </p>
                  </div>
                  <DetailBadge label={humanizeSupplierOrderValue(entry.newStatus)} tone={supplierOrderStatusTone(entry.newStatus)} />
                </div>
                <p className="mt-2 text-xs text-[var(--color-text-secondary)]">
                  {quantitySummary(
                    entry.orderedQuantitySnapshot,
                    entry.quantityReady,
                    entry.quantityRemaining,
                    order.quantityUom,
                  )}
                </p>
                {entry.note ? <p className="mt-2 text-xs text-[var(--color-text-secondary)]">{entry.note}</p> : null}
                {entry.exceptionReason ? (
                  <p className="mt-2 text-xs text-[var(--tone-danger-text)]">{entry.exceptionReason}</p>
                ) : null}
              </li>
            ))}
          </ol>
        ) : (
          <DetailEmptyState text="No immutable supplier-order status updates have been recorded yet." />
        ),
    },
  ]

  return (
    <ProfileDetailsLayout
      testId="supplyarr-supplier-order-detail"
      backLabel="Supplier orders"
      backTo="/purchasing/supplier-orders"
      breadcrumbs={['Supplier orders', order.itemDescription]}
      icon={<PackageCheck className="h-9 w-9" />}
      title={order.itemDescription}
      subtitle={
        <span className="flex flex-wrap items-center gap-2">
          <span>{supplierIdentityLabel}</span>
          <span className="text-[var(--color-text-muted)]">/</span>
          <span>{order.supplierOrderId}</span>
        </span>
      }
      badges={[
        { label: 'Supplier order', tone: 'info' },
        { label: supplierUnitKindLabel, tone: order.parentSupplierId ? 'neutral' : 'info' },
        { label: humanizeSupplierOrderValue(order.status), tone: supplierOrderStatusTone(order.status) },
        { label: order.brokerOrderNumberSnapshot ?? 'Order ref pending', tone: order.brokerOrderNumberSnapshot ? 'neutral' : 'warn' },
      ]}
      actions={
        canUpdateSupplierOrders ? (
          <>
            <button
              type="button"
              className="inline-flex rounded-xl bg-[var(--color-accent)] px-4 py-3 text-sm font-semibold text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
              disabled={sendMutation.isPending}
              onClick={() => sendMutation.mutate()}
            >
              {sendMutation.isPending ? 'Sending…' : 'Send to supplier'}
            </button>
            <button
              type="button"
              className="inline-flex rounded-xl border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-4 py-3 text-sm font-semibold text-[var(--color-text-primary)] hover:bg-[var(--color-bg-control-hover)] disabled:opacity-50"
              disabled={magicLinkMutation.isPending}
              onClick={() => magicLinkMutation.mutate()}
            >
              {magicLinkMutation.isPending ? 'Generating…' : 'Generate magic link'}
            </button>
          </>
        ) : null
      }
      metrics={[
        {
          label: 'Status',
          value: humanizeSupplierOrderValue(order.status),
          hint: `Updated ${formatSupplierOrderDateTime(order.updatedAt)}`,
          icon: <PackageCheck className="h-5 w-5" />,
          tone: supplierOrderStatusTone(order.status),
        },
        {
          label: 'Quantity',
          value: `${order.quantityReady}/${order.orderedQuantity}`,
          hint: `${order.quantityRemaining} remaining ${order.quantityUom}`,
          icon: <CheckCircle2 className="h-5 w-5" />,
          tone: order.status === 'completed_ready_for_dispatch' ? 'good' : order.status === 'partially_ready' ? 'warn' : 'neutral',
        },
        {
          label: 'Linked trips',
          value: String(relatedTripsQuery.data?.length ?? 0),
          hint: `${activeSupplierBlockTrips.length} blocked · ${readyTripCount} ready`,
          icon: <Route className="h-5 w-5" />,
          tone: activeSupplierBlockTrips.length > 0 ? 'warn' : 'good',
        },
        {
          label: 'Documents',
          value: String(order.documents.length),
          hint: `${order.statusHistory.length} immutable status events`,
          icon: <FileText className="h-5 w-5" />,
          tone: order.documents.length > 0 ? 'good' : 'neutral',
        },
      ]}
      tabs={['Overview', 'Readiness', 'Related records', 'Documents', 'History']}
      snapshotTitle="Supplier-order snapshot"
      snapshotSubtitle="Supplier hierarchy, service coverage, pickup snapshots, and readiness details."
      snapshotFields={[
        { label: 'Supplier order ID', value: order.supplierOrderId, source: 'Order details' },
        { label: 'Supplier identity or sub-unit', value: supplierIdentityLabel, source: 'SupplyArr supplier record' },
        { label: 'Parent supplier', value: supplierParentLabel, source: 'SupplyArr supplier hierarchy' },
        { label: 'Hierarchy role', value: supplierUnitKindLabel, source: 'SupplyArr supplier hierarchy' },
        { label: 'Services provided', value: supplierServiceSummary, source: 'SupplyArr supplier coverage' },
        { label: 'Broker order snapshot', value: order.brokerOrderNumberSnapshot ?? 'Not recorded', source: 'Order snapshot' },
        { label: 'Pickup location snapshot', value: order.pickupLocationNameSnapshot ?? 'Not recorded', source: 'Pickup details' },
        { label: 'Pickup address snapshot', value: order.pickupAddressSnapshot, source: 'Pickup details' },
        { label: 'Destination summary snapshot', value: order.deliveryLocationNameSnapshot ?? 'Hidden or not recorded', source: 'Destination details' },
        { label: 'Destination address snapshot', value: order.deliveryAddressSnapshot ?? 'Hidden or not recorded', source: 'Destination details' },
        { label: 'Expected ready', value: formatSupplierOrderDateTime(order.expectedReadyAt), source: 'Readiness details' },
        { label: 'Confirmed ready', value: formatSupplierOrderDateTime(order.confirmedReadyAt), source: 'Readiness details' },
        { label: 'Pickup window start', value: formatSupplierOrderDateTime(order.pickupWindowStart), source: 'Pickup schedule' },
        { label: 'Pickup window end', value: formatSupplierOrderDateTime(order.pickupWindowEnd), source: 'Pickup schedule' },
        { label: 'Created by', value: order.createdByPersonId ?? 'Not recorded', source: 'Audit details' },
      ]}
      mainContent={
        <div className="space-y-5">
          {latestMagicLink ? (
            <section className="rounded-2xl border border-[var(--color-accent-border)] bg-[var(--color-accent-soft)] p-4">
              <div className="flex items-start justify-between gap-3">
                <div>
                  <h3 className="text-sm font-semibold text-[var(--color-text-primary)]">Latest supplier access link</h3>
                  {latestMagicLink.description ? (
                    <p className="mt-1 text-xs text-[var(--color-text-secondary)]">{latestMagicLink.description}</p>
                  ) : null}
                  <p className="mt-1 break-all text-xs text-[var(--color-text-primary)]">{latestMagicLink.url}</p>
                  {latestMagicLink.expiresAt ? (
                    <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                      Expires {formatSupplierOrderDateTime(latestMagicLink.expiresAt)}
                    </p>
                  ) : (
                    <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                      Expiry is tracked on the newly issued remaining-child order link.
                    </p>
                  )}
                </div>
                <ExternalLink className="h-4 w-4 text-[var(--color-accent)]" />
              </div>
            </section>
          ) : null}

          <section className="rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5">
            <h3 className="text-lg font-bold text-[var(--color-text-primary)]">Broker actions</h3>
          <p className="mt-2 text-sm text-[var(--color-text-secondary)]">
              Record status decisions separately from transportation execution. The supplier portal keeps supplier responses and readiness updates in one place.
            </p>
            {canUpdateSupplierOrders ? (
              <div className="mt-4 grid gap-5 lg:grid-cols-2">
                <div className="rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] p-4">
                  <h4 className="font-semibold text-[var(--color-text-primary)]">Internal status update</h4>
                  <div className="mt-3 grid gap-3">
                    <label className="text-sm text-[var(--color-text-secondary)]">
                      New status
                      <select
                        className="mt-1 block w-full rounded-xl border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-[var(--color-text-primary)]"
                        value={statusDraft.newStatus}
                        onChange={(event) => setStatusDraft({ ...statusDraft, newStatus: event.target.value })}
                      >
                        {(metadata?.internalStatusOptions ?? []).map((status) => (
                          <option key={status.value} value={status.value}>
                            {status.label}
                          </option>
                        ))}
                      </select>
                    </label>
                    <div className="grid gap-3 md:grid-cols-2">
                      <label className="text-sm text-[var(--color-text-secondary)]">
                        Quantity ready
                        <input
                          className="mt-1 block w-full rounded-xl border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-[var(--color-text-primary)]"
                          inputMode="decimal"
                          value={statusDraft.quantityReady?.toString() ?? ''}
                          onChange={(event) =>
                            setStatusDraft({
                              ...statusDraft,
                              quantityReady: event.target.value ? Number(event.target.value) : null,
                            })
                          }
                        />
                      </label>
                      <label className="text-sm text-[var(--color-text-secondary)]">
                        Estimated ready at
                        <input
                          type="datetime-local"
                          className="mt-1 block w-full rounded-xl border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-[var(--color-text-primary)]"
                          value={toDatetimeLocalValue(statusDraft.estimatedReadyAt)}
                          onChange={(event) =>
                            setStatusDraft({ ...statusDraft, estimatedReadyAt: fromDatetimeLocalValue(event.target.value) })
                          }
                        />
                      </label>
                    </div>
                    <div className="grid gap-3 md:grid-cols-2">
                      <label className="text-sm text-[var(--color-text-secondary)]">
                        Pickup window start
                        <input
                          type="datetime-local"
                          className="mt-1 block w-full rounded-xl border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-[var(--color-text-primary)]"
                          value={toDatetimeLocalValue(statusDraft.pickupWindowStart)}
                          onChange={(event) =>
                            setStatusDraft({ ...statusDraft, pickupWindowStart: fromDatetimeLocalValue(event.target.value) })
                          }
                        />
                      </label>
                      <label className="text-sm text-[var(--color-text-secondary)]">
                        Pickup window end
                        <input
                          type="datetime-local"
                          className="mt-1 block w-full rounded-xl border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-[var(--color-text-primary)]"
                          value={toDatetimeLocalValue(statusDraft.pickupWindowEnd)}
                          onChange={(event) =>
                            setStatusDraft({ ...statusDraft, pickupWindowEnd: fromDatetimeLocalValue(event.target.value) })
                          }
                        />
                      </label>
                    </div>
                    <label className="text-sm text-[var(--color-text-secondary)]">
                      Note
                      <textarea
                        className="mt-1 block w-full rounded-xl border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-[var(--color-text-primary)]"
                        rows={3}
                        value={statusDraft.note ?? ''}
                        onChange={(event) => setStatusDraft({ ...statusDraft, note: event.target.value || null })}
                      />
                    </label>
                    <label className="text-sm text-[var(--color-text-secondary)]">
                      Exception reason
                      <textarea
                        className="mt-1 block w-full rounded-xl border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-[var(--color-text-primary)]"
                        rows={2}
                        value={statusDraft.exceptionReason ?? ''}
                        onChange={(event) =>
                          setStatusDraft({ ...statusDraft, exceptionReason: event.target.value || null })
                        }
                      />
                    </label>
                    <label className="flex items-start gap-3 rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-3 text-sm text-[var(--color-text-secondary)]">
                      <input
                        type="checkbox"
                        className="mt-1"
                        checked={statusDraft.readyForPickupConfirmed}
                        onChange={(event) =>
                          setStatusDraft({ ...statusDraft, readyForPickupConfirmed: event.target.checked })
                        }
                      />
                      <span>I confirm this order is complete, staged, and ready for pickup.</span>
                    </label>
                    <button
                      type="button"
                      className="rounded-xl bg-[var(--color-accent)] px-4 py-2 text-sm font-semibold text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
                      disabled={statusMutation.isPending}
                      onClick={() => statusMutation.mutate()}
                    >
                      {statusMutation.isPending ? 'Saving…' : 'Save internal status update'}
                    </button>
                    {statusMutation.error instanceof Error ? (
                      <p className="text-xs text-[var(--tone-danger-text)]">{statusMutation.error.message}</p>
                    ) : null}
                  </div>
                </div>

                <div className="space-y-5">
                  <div className="rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] p-4">
                    <h4 className="font-semibold text-[var(--color-text-primary)]">Partial readiness decision</h4>
                    <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                      Keep the supplier order blocked, authorize a partial dispatch, or prepare the split flow.
                    </p>
                    <div className="mt-3 grid gap-3">
                      <label className="text-sm text-[var(--color-text-secondary)]">
                        Decision
                        <select
                          className="mt-1 block w-full rounded-xl border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-[var(--color-text-primary)]"
                          value={decisionType}
                          onChange={(event) => setDecisionType(event.target.value)}
                        >
                          {(metadata?.brokerDecisionTypeOptions ?? []).map((decisionTypeOption) => (
                            <option key={decisionTypeOption.value} value={decisionTypeOption.value}>
                              {decisionTypeOption.label}
                            </option>
                          ))}
                        </select>
                      </label>
                      <div className="grid gap-3 md:grid-cols-2">
                        <label className="text-sm text-[var(--color-text-secondary)]">
                          Authorized quantity
                          <input
                            className="mt-1 block w-full rounded-xl border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-[var(--color-text-primary)]"
                            inputMode="decimal"
                            value={decisionQuantity}
                            onChange={(event) => setDecisionQuantity(event.target.value)}
                          />
                        </label>
                        <label className="text-sm text-[var(--color-text-secondary)]">
                          Selected trip ID
                          <input
                            className="mt-1 block w-full rounded-xl border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-[var(--color-text-primary)]"
                            value={decisionTripId}
                            onChange={(event) => setDecisionTripId(event.target.value)}
                          />
                        </label>
                      </div>
                      <label className="text-sm text-[var(--color-text-secondary)]">
                        Note
                        <textarea
                          className="mt-1 block w-full rounded-xl border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-[var(--color-text-primary)]"
                          rows={3}
                          value={decisionNote}
                          onChange={(event) => setDecisionNote(event.target.value)}
                        />
                      </label>
                      <button
                        type="button"
                        className="rounded-xl bg-[var(--color-accent)] px-4 py-2 text-sm font-semibold text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
                        disabled={decisionMutation.isPending}
                        onClick={() => decisionMutation.mutate()}
                      >
                        {decisionMutation.isPending ? 'Saving…' : 'Record broker decision'}
                      </button>
                      {decisionMutation.error instanceof Error ? (
                      <p className="text-xs text-[var(--tone-danger-text)]">{decisionMutation.error.message}</p>
                      ) : null}
                    </div>
                  </div>

                  <div className="rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] p-4">
                    <h4 className="font-semibold text-[var(--color-text-primary)]">Split remaining quantity</h4>
                    <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                      Create ready and remaining child orders, revoke the old token, and rotate remaining-child access.
                    </p>
                    <div className="mt-3 grid gap-3">
                      <label className="text-sm text-[var(--color-text-secondary)]">
                        Split reason
                        <input
                          className="mt-1 block w-full rounded-xl border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-[var(--color-text-primary)]"
                          value={splitReason}
                          onChange={(event) => setSplitReason(event.target.value)}
                        />
                      </label>
                      <label className="text-sm text-[var(--color-text-secondary)]">
                        Selected ready-trip ID
                        <input
                          className="mt-1 block w-full rounded-xl border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-[var(--color-text-primary)]"
                          value={splitTripId}
                          onChange={(event) => setSplitTripId(event.target.value)}
                        />
                      </label>
                      <div className="grid gap-3 md:grid-cols-2">
                        <label className="text-sm text-[var(--color-text-secondary)]">
                          Remaining expected ready at
                          <input
                            type="datetime-local"
                            className="mt-1 block w-full rounded-xl border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-[var(--color-text-primary)]"
                            value={splitExpectedReadyAt}
                            onChange={(event) => setSplitExpectedReadyAt(event.target.value)}
                          />
                        </label>
                        <label className="text-sm text-[var(--color-text-secondary)]">
                          Remaining pickup window start
                          <input
                            type="datetime-local"
                            className="mt-1 block w-full rounded-xl border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-[var(--color-text-primary)]"
                            value={splitPickupWindowStart}
                            onChange={(event) => setSplitPickupWindowStart(event.target.value)}
                          />
                        </label>
                      </div>
                      <label className="text-sm text-[var(--color-text-secondary)]">
                        Remaining pickup window end
                        <input
                          type="datetime-local"
                          className="mt-1 block w-full rounded-xl border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-[var(--color-text-primary)]"
                          value={splitPickupWindowEnd}
                          onChange={(event) => setSplitPickupWindowEnd(event.target.value)}
                        />
                      </label>
                      <button
                        type="button"
                        className="rounded-xl bg-[var(--color-warning)] px-4 py-2 text-sm font-semibold text-[var(--color-text-primary)] hover:bg-[var(--color-warning-hover)] disabled:opacity-50"
                        disabled={splitMutation.isPending}
                        onClick={() => splitMutation.mutate()}
                      >
                        {splitMutation.isPending ? 'Splitting…' : 'Split remaining quantity'}
                      </button>
                      {splitMutation.error instanceof Error ? (
                      <p className="text-xs text-[var(--tone-danger-text)]">{splitMutation.error.message}</p>
                      ) : null}
                    </div>
                  </div>
                </div>
              </div>
            ) : (
              <p className="mt-4 text-sm text-[var(--color-text-muted)]">You can view this record, but update actions are restricted.</p>
            )}
          </section>

          <section className="rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5">
            <h3 className="text-lg font-bold text-[var(--color-text-primary)]">Split lineage</h3>
            <div className="mt-4 grid gap-4 md:grid-cols-2">
              <div className="rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] p-4">
                <p className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Parent supplier order</p>
                <p className="mt-2 text-sm font-medium text-[var(--color-text-primary)]">{order.parentSupplierOrderId ?? 'This is the parent order.'}</p>
                {order.splitReason ? (
                  <p className="mt-2 text-xs text-[var(--color-text-muted)]">Split reason: {order.splitReason}</p>
                ) : null}
              </div>
              <div className="rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] p-4">
                <p className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Child supplier orders</p>
                {splitChildren.length > 0 ? (
                  <ul className="mt-2 space-y-2">
                    {splitChildren.map((child) => (
                      <li key={child.supplierOrderId} className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-3">
                        <p className="text-sm font-medium text-[var(--color-text-primary)]">{child.itemDescription}</p>
                        <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                          {child.supplierOrderId} · {humanizeSupplierOrderValue(child.status)}
                        </p>
                      </li>
                    ))}
                  </ul>
                ) : (
                  <p className="mt-2 text-sm text-[var(--color-text-muted)]">No child supplier orders have been created from this parent yet.</p>
                )}
              </div>
            </div>
          </section>
        </div>
      }
      decisionTitle="Readiness decision"
      decisionBadge={{
        label:
          order.status === 'completed_ready_for_dispatch'
            ? 'Dispatch releasable'
            : order.status === 'partially_ready'
              ? 'Broker decision required'
              : order.status === 'unable_to_fulfill'
                ? 'Blocked by supplier'
                : 'Waiting on supplier',
        tone:
          order.status === 'completed_ready_for_dispatch'
            ? 'good'
            : order.status === 'unable_to_fulfill'
              ? 'bad'
              : 'warn',
      }}
      decisionIcon={
        order.status === 'completed_ready_for_dispatch' ? (
          <CheckCircle2 className="h-5 w-5 text-[var(--color-success-text)]" />
        ) : (
          <AlertTriangle className="h-5 w-5 text-[var(--color-warning-text)]" />
        )
      }
      decisionSummary={decisionSummary}
      decisionDetail={decisionDetail}
      allowedChecks={[
        order.status === 'completed_ready_for_dispatch',
        readyTripCount > 0,
        order.documents.length > 0,
      ].filter(Boolean).length}
      blockedChecks={[
        order.status !== 'completed_ready_for_dispatch',
        activeSupplierBlockTrips.length > 0,
        order.status === 'unable_to_fulfill',
      ].filter(Boolean).length}
      railSections={railSections}
    />
  )
}

function fromDatetimeLocalValue(value: string): string | null {
  if (!value) {
    return null
  }

  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return null
  }

  return date.toISOString()
}

function toDatetimeLocalValue(value: string | null | undefined): string {
  if (!value) {
    return ''
  }

  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return ''
  }

  const year = date.getFullYear()
  const month = `${date.getMonth() + 1}`.padStart(2, '0')
  const day = `${date.getDate()}`.padStart(2, '0')
  const hours = `${date.getHours()}`.padStart(2, '0')
  const minutes = `${date.getMinutes()}`.padStart(2, '0')
  return `${year}-${month}-${day}T${hours}:${minutes}`
}
